using UnityEngine;
using TMPro;

public class MainGameUI : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text levelText;
    public TMP_Text xpText;

    private PlayerStats playerStats;

    void Start()
    {
        playerStats = GameManager.Instance.PlayerStats;

        if (playerStats != null)
        {
            playerStats.OnStatsChanged += UpdateXPUI;
            UpdateXPUI(); // show correct stats immediately
        }
    }

    void OnDestroy()
    {
        if (playerStats != null)
            playerStats.OnStatsChanged -= UpdateXPUI;
    }

    private void UpdateXPUI()
    {
        levelText.text = $"Level: {playerStats.level}";
        xpText.text = $"XP: {playerStats.currentXP}/{playerStats.xpToNextLevel}";
    }
}
