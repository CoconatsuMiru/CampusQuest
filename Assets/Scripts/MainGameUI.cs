using UnityEngine;
using TMPro;

public class MainGameUI : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text levelText;
    public TMP_Text xpText;

    void Start()
    {
        UpdateXPUI();
    }

    public void UpdateXPUI()
    {
        if (GameManager.Instance != null)
        {
            levelText.text = $"Level: {GameManager.Instance.PlayerLevel}";
            xpText.text = $"XP: {GameManager.Instance.CurrentXP}/{GameManager.Instance.XpToNextLevel}";
        }
    }
}
