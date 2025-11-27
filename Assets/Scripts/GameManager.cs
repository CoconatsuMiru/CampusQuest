using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("UI Elements")]
    public TMP_Text usernameText;
    public TMP_Text levelText;

    [Header("EXP Bar")]
    public Slider expSlider;

    [Header("Level Up Notification")]
    public GameObject levelUpNotification; // Drag your TMP Text object here

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        RefreshUI();

        // Check queued notification
        if (PlayerBossStats.Instance != null && PlayerBossStats.Instance.hasPendingLevelUpNotification)
        {
            ShowLevelUpNotification();
            PlayerBossStats.Instance.hasPendingLevelUpNotification = false;
        }
    }

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

        int expToNext = level * 50;

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

    public void ShowLevelUpNotification()
    {
        if (levelUpNotification == null)
        {
            Debug.LogWarning("⚠ Level up notification not assigned in GameManager!");
            return;
        }

        StartCoroutine(ShowNotificationRoutine());
    }

    private System.Collections.IEnumerator ShowNotificationRoutine()
    {
        levelUpNotification.SetActive(true);

        yield return new WaitForSeconds(4f);

        Destroy(levelUpNotification);
    }
}
