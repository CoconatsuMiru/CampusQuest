using UnityEngine;
using System.Collections.Generic;

public class PlayerBossStats : MonoBehaviour
{
    public static PlayerBossStats Instance { get; private set; }
    public bool isLoaded { get; private set; } = false;

    // User identity
    public string username;
    public string email;

    // Core stats (matching your LocalAuthManager schema)
    public int level = 1;
    public int exp = 0;
    public int stat_music = 1;
    public int stat_art = 1;
    public int stat_science = 1;
    public int stat_math = 1;
    public int stat_english = 1;
    public int stat_history = 1;

    // Skill points
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
        LoadFromLocal();
    }

    public void LoadFromLocal()
    {
        if (LocalAuthManager.Instance == null || LocalAuthManager.Instance.currentUser == null)
        {
            Debug.LogWarning("⚠ PlayerBossStats: No logged-in user found. Using default stats.");
            isLoaded = true;
            return;
        }

        var user = LocalAuthManager.Instance.currentUser;
        username = user.username;
        email = user.email;

        // Load base stats
        level = user.level;
        exp = user.exp;

        // Load subjects
        if (user.subjects != null)
        {
            stat_music = GetValue(user.subjects, "music", stat_music);
            stat_art = GetValue(user.subjects, "art", stat_art);
            stat_science = GetValue(user.subjects, "science", stat_science);
            stat_math = GetValue(user.subjects, "math", stat_math);
            stat_english = GetValue(user.subjects, "english", stat_english);
            stat_history = GetValue(user.subjects, "history", stat_history);
        }

        Debug.Log("✅ PlayerBossStats: Loaded stats for " + username);
        isLoaded = true;
    }

    private int GetValue(Dictionary<string, int> dict, string key, int fallback)
    {
        if (dict != null && dict.ContainsKey(key))
            return dict[key];
        return fallback;
    }

    // Add EXP and handle level-ups
    public void AddExp(int amount)
    {
        if (!isLoaded)
        {
            Debug.LogWarning("⚠ PlayerBossStats.AddExp called before data was loaded.");
            return;
        }

        if (amount <= 0) return;

        exp += amount;
        Debug.Log($"PlayerBossStats: +{amount} EXP (now {exp})");

        bool leveledUp = false;

        // Level up rule: need (level * 50) EXP to level up
        while (exp >= level * 50)
        {
            exp -= level * 50;
            level += 1;
            skillPoints += 5;
            leveledUp = true;
            Debug.Log($"🎉 Level up! New level = {level}, Skill Points = {skillPoints}");
        }

        UpdateLocalData();

        if (leveledUp)
        {
            Debug.Log("🏆 PlayerBossStats: Level up complete!");
        }
    }

    // Spend skill points to upgrade a subject
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

        Debug.Log($"✅ {points} skill points spent on {subject}. Remaining: {skillPoints}");
        return true;
    }

    // Save updates back to LocalAuthManager’s JSON
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

        LocalAuthManager.Instance.UpdateUserData();

        Debug.Log("💾 PlayerBossStats: Local data updated.");
    }

    // Returns a stat value for quiz damage boosts, etc.
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
