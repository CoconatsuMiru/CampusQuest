using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class PlayerBossStats : MonoBehaviour
{
    public static PlayerBossStats Instance { get; private set; }
    public bool isLoaded { get; private set; } = false;

    // NEW: Queue notification for next scene
    public bool hasPendingLevelUpNotification = false;

    // User identity
    public string username;
    public string email;

    // Core stats
    public int level = 1;
    public int exp = 0;
    public int expNeededToLevelUp = 50;
    public int skillPoints = 0;

    // Subject stats
    public int stat_music = 1;
    public int stat_art = 1;
    public int stat_science = 1;
    public int stat_math = 1;
    public int stat_english = 1;
    public int stat_history = 1;

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
        LoadFromLocal();

        if (GameManager.Instance != null)
            GameManager.Instance.RefreshUI();
    }

    public void LoadFromLocal()
    {
        if (LocalAuthManager.Instance == null || LocalAuthManager.Instance.currentUser == null)
        {
            Debug.LogWarning("⚠ PlayerBossStats: No logged-in user found. Using default stats.");
            isLoaded = true;
            UpdateExpNeeded();
            return;
        }

        var user = LocalAuthManager.Instance.currentUser;
        username = user.username;
        email = user.email;

        level = user.level;
        exp = user.exp;

        if (user.subjects != null)
        {
            stat_music = GetValue(user.subjects, "music", stat_music);
            stat_art = GetValue(user.subjects, "art", stat_art);
            stat_science = GetValue(user.subjects, "science", stat_science);
            stat_math = GetValue(user.subjects, "math", stat_math);
            stat_english = GetValue(user.subjects, "english", stat_english);
            stat_history = GetValue(user.subjects, "history", stat_history);

            if (user.subjects.ContainsKey("skillPoints"))
                skillPoints = user.subjects["skillPoints"];
        }

        UpdateExpNeeded();
        Debug.Log("✅ PlayerBossStats: Loaded stats for " + username);
        isLoaded = true;
    }

    private int GetValue(Dictionary<string, int> dict, string key, int fallback)
    {
        if (dict != null && dict.ContainsKey(key))
            return dict[key];
        return fallback;
    }

    private void UpdateExpNeeded()
    {
        expNeededToLevelUp = level * 50;
    }

    public void AddExp(int amount)
    {
        if (!isLoaded)
        {
            Debug.LogWarning("⚠ PlayerBossStats.AddExp called before data was loaded.");
            return;
        }

        if (amount <= 0) return;

        exp += amount;
        Debug.Log($"PlayerBossStats: +{amount} EXP (now {exp}/{expNeededToLevelUp})");

        bool leveledUp = false;

        while (exp >= expNeededToLevelUp)
        {
            exp -= expNeededToLevelUp;
            level += 1;
            skillPoints += 5;
            leveledUp = true;

            UpdateExpNeeded();
            Debug.Log($"🎉 Level up! New level = {level}, Next level requires {expNeededToLevelUp} EXP.");
        }

        UpdateLocalData();

        if (GameManager.Instance != null)
            GameManager.Instance.RefreshUI();

        // Store notification for next scene
        if (leveledUp)
        {
            hasPendingLevelUpNotification = true;
            Debug.Log("🏆 Level-up notification queued!");
        }
    }

    public bool SpendSkillPoints(string subject, int points)
    {
        if (points <= 0) return false;
        if (skillPoints < points)
        {
            Debug.LogWarning("⚠ Not enough skill points.");
            return false;
        }

        switch (subject.ToLower())
        {
            case "music": stat_music += points; break;
            case "art": stat_art += points; break;
            case "science": stat_science += points; break;
            case "math": stat_math += points; break;
            case "english": stat_english += points; break;
            case "history": stat_history += points; break;
            default:
                Debug.LogWarning("⚠ Unknown subject: " + subject);
                return false;
        }

        skillPoints -= points;
        UpdateLocalData();

        if (GameManager.Instance != null)
            GameManager.Instance.RefreshUI();

        Debug.Log($"✅ {points} skill points spent on {subject}. Remaining: {skillPoints}");
        return true;
    }

    private void UpdateLocalData()
    {
        if (LocalAuthManager.Instance == null || LocalAuthManager.Instance.currentUser == null)
        {
            Debug.LogWarning("⚠ Cannot update local data: no user logged in.");
            return;
        }

        var user = LocalAuthManager.Instance.currentUser;

        user.level = level;
        user.exp = exp;

        if (user.subjects == null)
            user.subjects = new Dictionary<string, int>();

        user.subjects["music"] = stat_music;
        user.subjects["art"] = stat_art;
        user.subjects["science"] = stat_science;
        user.subjects["math"] = stat_math;
        user.subjects["english"] = stat_english;
        user.subjects["history"] = stat_history;
        user.subjects["skillPoints"] = skillPoints;

        LocalAuthManager.Instance.UpdateUserData();
        Debug.Log("💾 PlayerBossStats: Local data updated.");
    }

    public int GetStatBySubject(string subject)
    {
        switch (subject.ToLower())
        {
            case "music": return stat_music;
            case "art": return stat_art;
            case "science": return stat_science;
            case "math": return stat_math;
            case "english": return stat_english;
            case "history": return stat_history;
            default: return 1;
        }
    }
}
