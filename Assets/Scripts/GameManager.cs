using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int CurrentXP { get; private set; }
    public int PlayerLevel { get; private set; }
    public int XpToNextLevel { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); 
            LoadPlayerProgress();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadPlayerProgress()
    {
        CurrentXP = PlayerPrefs.GetInt("CurrentXP", 0);
        PlayerLevel = PlayerPrefs.GetInt("PlayerLevel", 1);
        XpToNextLevel = PlayerPrefs.GetInt("XpToNextLevel", 10);
    }

    public void SavePlayerProgress()
    {
        PlayerPrefs.SetInt("CurrentXP", CurrentXP);
        PlayerPrefs.SetInt("PlayerLevel", PlayerLevel);
        PlayerPrefs.SetInt("XpToNextLevel", XpToNextLevel);
        PlayerPrefs.Save();
    }

    // ===== XP & LEVEL UP FUNCTION =====
    public void AddXP(int amount)
    {
        CurrentXP += amount;

        while (CurrentXP >= XpToNextLevel) // keep looping if overflow
        {
            CurrentXP -= XpToNextLevel; // carry over leftover XP
            LevelUp();
        }

        SavePlayerProgress();
    }

    private void LevelUp()
    {
        PlayerLevel++;
        XpToNextLevel += 5; // each level requires more XP (customize this)
        Debug.Log($"🎉 LEVEL UP! Now Level {PlayerLevel}");
    }
}
 