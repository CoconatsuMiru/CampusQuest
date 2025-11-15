using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ExpSliderController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Slider expSlider;   // Drag your EXP slider here
    [SerializeField] private TMP_Text expText;   // Optional: "EXP: current / needed"

    private PlayerBossStats playerStats;

    void Start()
    {
        // Get PlayerBossStats instance
        playerStats = PlayerBossStats.Instance;

        if (playerStats == null)
        {
            Debug.LogError("❌ PlayerBossStats instance not found in scene!");
            return;
        }

        // Initialize slider properties
        expSlider.minValue = 0;
        expSlider.maxValue = playerStats.expNeededToLevelUp;
        expSlider.value = playerStats.exp;

        UpdateExpText();
    }

    void Update()
    {
        if (playerStats == null) return;

        // Sync slider with current EXP and max
        expSlider.maxValue = playerStats.expNeededToLevelUp;
        expSlider.value = playerStats.exp;

        UpdateExpText();
    }

    private void UpdateExpText()
    {
        if (expText != null)
            expText.text = $"{playerStats.exp} / {playerStats.expNeededToLevelUp}";
    }
}
