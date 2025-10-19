using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance; // 👈 Allow other scripts to call RefreshUI()

    [Header("UI Elements")]
    public TMP_Text usernameText;
    public TMP_Text levelText;

    [Header("EXP Bar")]
    public Slider expSlider; // 👈 Assign in Inspector

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        RefreshUI();
    }

    /// <summary>
    /// Refreshes username, level, and EXP slider from the current user data.
    /// </summary>
    public void RefreshUI()
    {
        if (LocalAuthManager.Instance == null || LocalAuthManager.Instance.currentUser == null)
        {
            Debug.LogError("❌ No logged-in user found! Cannot refresh UI.");
            return;
        }

        var user = LocalAuthManager.Instance.currentUser;

        string username = user.username;
        int level = user.level;
        int exp = user.exp;

        // ✅ Calculate EXP needed for next level
        int expToNext = level * 50;

        // 🖥️ Update UI elements
        if (usernameText != null)
            usernameText.text = username;

        if (levelText != null)
            levelText.text = $"Level: {level}";

        if (expSlider != null)
        {
            expSlider.maxValue = expToNext;
            expSlider.value = exp;
        }

        Debug.Log($"🔄 UI refreshed — {username} (Level {level}, EXP {exp}/{expToNext})");
    }
}
