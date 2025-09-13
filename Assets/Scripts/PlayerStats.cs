using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public int level = 1;
    public int currentXP = 0;
    public int xpToNextLevel = 10;

    // Add XP when a quiz ends
    public void AddXP(int amount)
    {
        currentXP += amount;
        Debug.Log("Gained " + amount + " XP. Total XP: " + currentXP);

        CheckLevelUp();
    }

    private void CheckLevelUp()
    {
        while (currentXP >= xpToNextLevel)
        {
            currentXP -= xpToNextLevel;
            level++;
            xpToNextLevel = Mathf.RoundToInt(xpToNextLevel * 1.5f); // simple scaling
            Debug.Log("🎉 Level Up! New Level: " + level);
        }
    }

    // TEMP persistence using PlayerPrefs
    public void SaveStats()
    {
        PlayerPrefs.SetInt("PlayerLevel", level);
        PlayerPrefs.SetInt("PlayerXP", currentXP);
        PlayerPrefs.SetInt("XPToNextLevel", xpToNextLevel);
        PlayerPrefs.Save();
    }

    public void LoadStats()
    {
        level = PlayerPrefs.GetInt("PlayerLevel", 1);
        currentXP = PlayerPrefs.GetInt("PlayerXP", 0);
        xpToNextLevel = PlayerPrefs.GetInt("XPToNextLevel", 10);
    }
}
