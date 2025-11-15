using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SkillPointsManagerLocal : MonoBehaviour
{
    public static SkillPointsManagerLocal Instance;

    [Header("UI References")]
    public TMP_Text levelText;           // e.g., "Level 5"
    public TMP_Text skillPointsText;     // e.g., "Skill Points: 12"

    [Header("Upgrade Buttons")]
    public Button addMathButton;
    public Button addScienceButton;
    public Button addEnglishButton;
    public Button addArtButton;
    public Button addMusicButton;

    [Header("History (Separate Fields)")]
    public Button addHistoryButton;
    public TMP_Text historyValueText;    // Separate TMP just for history

    [Header("Subject Values (Other Subjects)")]
    public TMP_Text mathValueText;
    public TMP_Text scienceValueText;
    public TMP_Text englishValueText;
    public TMP_Text artValueText;
    public TMP_Text musicValueText;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        SetupButtons();
        RefreshUI();
    }

    private void SetupButtons()
    {
        if (addMathButton) addMathButton.onClick.AddListener(() => UpgradeSubject("math"));
        if (addScienceButton) addScienceButton.onClick.AddListener(() => UpgradeSubject("science"));
        if (addEnglishButton) addEnglishButton.onClick.AddListener(() => UpgradeSubject("english"));
        if (addArtButton) addArtButton.onClick.AddListener(() => UpgradeSubject("art"));
        if (addMusicButton) addMusicButton.onClick.AddListener(() => UpgradeSubject("music"));
        if (addHistoryButton) addHistoryButton.onClick.AddListener(() => UpgradeSubject("history"));
    }

    public void RefreshUI()
    {
        if (PlayerBossStats.Instance == null || !PlayerBossStats.Instance.isLoaded)
        {
            Debug.LogWarning("⚠ SkillPointsManagerLocal: Player stats not loaded yet.");
            return;
        }

        var stats = PlayerBossStats.Instance;

        // Level and Skill Points
        if (levelText != null)
            levelText.text = $"Level {stats.level}";
        if (skillPointsText != null)
            skillPointsText.text = $"Skill Points: {stats.skillPoints}";

        // Subject Values
        if (mathValueText != null) mathValueText.text = stats.stat_math.ToString();
        if (scienceValueText != null) scienceValueText.text = stats.stat_science.ToString();
        if (englishValueText != null) englishValueText.text = stats.stat_english.ToString();
        if (artValueText != null) artValueText.text = stats.stat_art.ToString();
        if (musicValueText != null) musicValueText.text = stats.stat_music.ToString();

        // Separate History Field
        if (historyValueText != null) historyValueText.text = stats.stat_history.ToString();
    }

    private void UpgradeSubject(string subject)
    {
        if (PlayerBossStats.Instance == null || !PlayerBossStats.Instance.isLoaded)
        {
            Debug.LogWarning("⚠ Cannot upgrade subject: Player stats not ready.");
            return;
        }

        bool success = PlayerBossStats.Instance.SpendSkillPoints(subject, 1);

        if (success)
        {
            Debug.Log($"✅ Upgraded {subject} by +1");
            RefreshUI();
        }
        else
        {
            Debug.LogWarning($"❌ Failed to upgrade {subject}: Not enough points or invalid subject.");
        }
    }
}
