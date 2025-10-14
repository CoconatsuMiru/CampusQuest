using UnityEngine;
using UnityEngine.UI;

public class ExpSliderController : MonoBehaviour
{
    public Slider expSlider; // drag your UI slider here
    public PlayerStats playerStats; // drag your PlayerStats script (attached to the player)

    void Start()
    {
        // Initialize slider max and value
        expSlider.maxValue = playerStats.xpToNextLevel;
        expSlider.value = playerStats.currentXP;
    }

    void Update()
    {
        // Continuously update slider to reflect XP
        expSlider.maxValue = playerStats.xpToNextLevel;
        expSlider.value = playerStats.currentXP;
    }
}