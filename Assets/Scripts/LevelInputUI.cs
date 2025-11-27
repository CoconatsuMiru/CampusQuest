using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LevelUpUI : MonoBehaviour
{
    public static LevelUpUI Instance;

    [Header("UI Elements")]
    public GameObject panelRoot;

    public Button continueButton;

    private void Awake()
    {
        Instance = this;

        // Hide on start
        panelRoot.SetActive(false);

        // Automatically bind the continue button
        continueButton.onClick.RemoveAllListeners();
        continueButton.onClick.AddListener(ClosePanel);
    }

    public void ShowLevelUp(int skillPointsEarned)
    {
        panelRoot.SetActive(true);
    }

    private void ClosePanel()
    {
        panelRoot.SetActive(false);
    }
}
