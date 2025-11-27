using UnityEngine;
using UnityEngine.UI;

public class AchievementUIManager : MonoBehaviour
{
    [Header("Achievement UI")]
    public Button achievementButton;
    public GameObject achievementPanel;

    private void Start()
    {
        if (achievementPanel != null)
            achievementPanel.SetActive(false);

        if (achievementButton != null)
            achievementButton.onClick.AddListener(ToggleAchievementPanel);
    }

    private void OnEnable()
    {
        if (AchievementUI.Instance != null)
            AchievementUI.Instance.UpdateUI();
    }

    private void ToggleAchievementPanel()
    {
        if (achievementPanel == null) return;

        bool isActive = achievementPanel.activeSelf;
        achievementPanel.SetActive(!isActive);

        if (!isActive)
            AchievementUI.Instance?.UpdateUI();

        Debug.Log("🏆 Achievement Panel toggled: " + (!isActive));
    }
}
