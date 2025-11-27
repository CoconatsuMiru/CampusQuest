using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class AchievementUI : MonoBehaviour
{
    public static AchievementUI Instance;

    [Header("Monster Achievement UI")]
    public TMP_Text monsterProgressText;
    public Button monsterClaimButton;

    [Header("Correct Answers Achievement UI")]
    public TMP_Text correctProgressText;
    public Button correctClaimButton;

    private void Awake()
    {
        // Scene-based Singleton
        Instance = this;
    }

    private void Start()
    {
        monsterClaimButton.onClick.AddListener(() =>
        {
            AchievementManager.Instance.ClaimMonsterReward();
            UpdateUI();
        });

        correctClaimButton.onClick.AddListener(() =>
        {
            AchievementManager.Instance.ClaimCorrectAnswerReward();
            UpdateUI();
        });

        UpdateUI();
    }

    private void OnEnable()
    {
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (AchievementManager.Instance == null) return;

        var a = AchievementManager.Instance;

        monsterProgressText.text = $"{a.monstersDefeated}/{a.monsterGoal}";
        monsterClaimButton.interactable = a.monstersDefeated >= a.monsterGoal;

        correctProgressText.text = $"{a.correctAnswers}/{a.correctAnswerGoal}";
        correctClaimButton.interactable = a.correctAnswers >= a.correctAnswerGoal;
    }
}
