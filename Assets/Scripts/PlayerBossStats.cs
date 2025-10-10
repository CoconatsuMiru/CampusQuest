using UnityEngine;
using System.Collections.Generic;
using Firebase;
using Firebase.Database;
using Firebase.Auth;
using Firebase.Extensions; // recommended to use ContinueWithOnMainThread
using System;

public class PlayerBossStats : MonoBehaviour
{
    public static PlayerBossStats Instance { get; private set; }
    public bool isLoaded { get; private set; } = false;

    // user identity
    public string userId;
    public string username;
    public string email;

    // core stats (names match your DB keys)
    public int stat_03_level = 1;
    public int stat_04_exp = 0;
    public int stat_05_music = 1;
    public int stat_06_art = 1;
    public int stat_07_science = 1;
    public int stat_08_math = 1;
    public int stat_09_english = 1;
    public int stat_10_history = 1;

    // skill point pool
    public int skillPoints = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    private void Start()
    {
        // attempt to set userId automatically if using FirebaseAuth
        if (string.IsNullOrEmpty(userId))
        {
            if (FirebaseAuth.DefaultInstance != null && FirebaseAuth.DefaultInstance.CurrentUser != null)
            {
                userId = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
            }
        }

        if (!string.IsNullOrEmpty(userId))
        {
            LoadFromFirebase(userId);
        }
        else
        {
            Debug.LogWarning("PlayerBossStats: userId not set and no authenticated user. Using default local stats.");
            isLoaded = true; // if you want to wait for login, set false instead
        }
    }

    public void LoadFromFirebase(string uid)
    {
        userId = uid;
        isLoaded = false;

        FirebaseDatabase.DefaultInstance
            .GetReference($"users/{userId}")
            .GetValueAsync()
            .ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("PlayerBossStats: Failed to load user data: " + task.Exception);
                isLoaded = true;
                return;
            }

            DataSnapshot snapshot = task.Result;
            if (snapshot == null || !snapshot.Exists)
            {
                Debug.Log("PlayerBossStats: No existing data for user - defaults will be used.");
                isLoaded = true;
                return;
            }

            username = snapshot.Child("stat_01_username").Value?.ToString() ?? username;
            email = snapshot.Child("stat_02_email").Value?.ToString() ?? email;
            stat_03_level = ParseInt(snapshot.Child("stat_03_level").Value, stat_03_level);
            stat_04_exp = ParseInt(snapshot.Child("stat_04_exp").Value, stat_04_exp);
            stat_05_music = ParseInt(snapshot.Child("stat_05_music").Value, stat_05_music);
            stat_06_art = ParseInt(snapshot.Child("stat_06_art").Value, stat_06_art);
            stat_07_science = ParseInt(snapshot.Child("stat_07_science").Value, stat_07_science);
            stat_08_math = ParseInt(snapshot.Child("stat_08_math").Value, stat_08_math);
            stat_09_english = ParseInt(snapshot.Child("stat_09_english").Value, stat_09_english);
            stat_10_history = ParseInt(snapshot.Child("stat_10_history").Value, stat_10_history);
            skillPoints = ParseInt(snapshot.Child("skillPoints").Value, skillPoints);

            Debug.Log("PlayerBossStats: loaded data for user " + userId);
            isLoaded = true;
        });
    }

    private int ParseInt(object value, int fallback)
    {
        if (value == null) return fallback;
        int outVal;
        if (int.TryParse(value.ToString(), out outVal)) return outVal;
        return fallback;
    }

    // returns stat used to compute damage (simple mapping)
    public int GetStatBySubject(string subject)
    {
        switch (subject.ToLower())
        {
            case "music": return stat_05_music;
            case "art": return stat_06_art;
            case "science": return stat_07_science;
            case "math": return stat_08_math;
            case "english": return stat_09_english;
            case "history": return stat_10_history;
            default: return 1;
        }
    }

    // Add EXP, handle level-ups, award skill points, push to Firebase
    public void AddExp(int amount)
    {
        if (!isLoaded)
        {
            Debug.LogWarning("PlayerBossStats.AddExp called before data was loaded. Ignoring.");
            return;
        }

        if (amount <= 0) return;

        stat_04_exp += amount;
        Debug.Log($"PlayerBossStats: +{amount} EXP (now {stat_04_exp})");

        bool anyLevelUp = false;

        // Level up rule: to go from level L -> L+1 you need (L * 50) EXP.
        while (true)
        {
            int needed = stat_03_level * 50;
            if (stat_04_exp >= needed)
            {
                stat_04_exp -= needed;
                stat_03_level += 1;
                skillPoints += 5; // +5 skill points per level up
                anyLevelUp = true;
                Debug.Log($"PlayerBossStats: Leveled up! New level = {stat_03_level}. SkillPoints = {skillPoints}");
            }
            else
            {
                break;
            }
        }

        // push updated values to Firebase
        UpdateUserDataInFirebase();

        // Optionally trigger an in-game UI/notification for level-up:
        if (anyLevelUp)
        {
            // Example: send event / call UI code to display LevelUp popup
        }
    }

    // Spend skill points to increase a subject stat (1 point = +1 stat)
    public bool SpendSkillPoints(string subject, int points)
    {
        if (points <= 0) return false;
        if (skillPoints < points)
        {
            Debug.LogWarning("PlayerBossStats: Not enough skill points.");
            return false;
        }

        switch (subject.ToLower())
        {
            case "music": stat_05_music += points; break;
            case "art": stat_06_art += points; break;
            case "science": stat_07_science += points; break;
            case "math": stat_08_math += points; break;
            case "english": stat_09_english += points; break;
            case "history": stat_10_history += points; break;
            default:
                Debug.LogWarning("PlayerBossStats: Unknown subject: " + subject);
                return false;
        }

        skillPoints -= points;
        UpdateUserDataInFirebase();
        Debug.Log($"PlayerBossStats: Spent {points} points on {subject}. Remaining skillPoints: {skillPoints}");
        return true;
    }

    // push relevant fields to Firebase
    public void UpdateUserDataInFirebase()
    {
        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogWarning("PlayerBossStats: userId empty, cannot update Firebase.");
            return;
        }

        var updates = new Dictionary<string, object>()
        {
            { "stat_03_level", stat_03_level },
            { "stat_04_exp", stat_04_exp },
            { "stat_05_music", stat_05_music },
            { "stat_06_art", stat_06_art },
            { "stat_07_science", stat_07_science },
            { "stat_08_math", stat_08_math },
            { "stat_09_english", stat_09_english },
            { "stat_10_history", stat_10_history },
            { "skillPoints", skillPoints }
        };

        FirebaseDatabase.DefaultInstance
            .GetReference($"users/{userId}")
            .UpdateChildrenAsync(updates)
            .ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("PlayerBossStats: Failed to update Firebase: " + task.Exception);
            }
            else
            {
                Debug.Log("PlayerBossStats: Successfully updated Firebase with new stats.");
            }
        });
    }
}
