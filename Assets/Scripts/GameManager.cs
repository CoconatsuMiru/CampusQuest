using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_Text usernameText;
    public TMP_Text levelText;
    public TMP_Text expText;
    public TMP_Text skillPointsText;

    void Start()
    {
        LoadUserData();
    }

    private void LoadUserData()
    {
        if (LocalAuthManager.Instance == null || LocalAuthManager.Instance.currentUser == null)
        {
            Debug.LogError("❌ No logged-in user found! Cannot load data.");
            return;
        }

        var user = LocalAuthManager.Instance.currentUser;

        string username = user.username;
        int level = user.level;
        int exp = user.exp;

        // Skill points (if you're tracking them in PlayerBossStats)
        int skillPoints = PlayerBossStats.Instance != null ? PlayerBossStats.Instance.skillPoints : 0;

        // Calculate EXP requirement
        int expToNext = level * 50;

        // 🖥️ Update UI safely
        if (usernameText != null) usernameText.text = username;
        if (levelText != null) levelText.text = $"Level: {level}";
        if (expText != null) expText.text = $"Exp: {exp}/{expToNext}";
        if (skillPointsText != null) skillPointsText.text = $"Skill Points: {skillPoints}";

        Debug.Log($"✅ Loaded user data from JSON — {username} (Level {level}, Exp {exp}/{expToNext})");
    }
}
