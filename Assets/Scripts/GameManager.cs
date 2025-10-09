using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public PlayerStats PlayerStats;

    public int CurrentXP { get; private set; }
    public int PlayerLevel { get; private set; }
    public int XpToNextLevel { get; private set; }

    // 🔔 Event for UI updates
    public event Action OnStatsChanged;

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
        
        OnStatsChanged?.Invoke(); // 🔔 Notify UI right after loading
    }

    public void SavePlayerProgress()
    {
        PlayerPrefs.SetInt("CurrentXP", CurrentXP);
        PlayerPrefs.SetInt("PlayerLevel", PlayerLevel);
        PlayerPrefs.SetInt("XpToNextLevel", XpToNextLevel);
        PlayerPrefs.Save();

        OnStatsChanged?.Invoke(); // 🔔 Notify any subscribers
    }

    // ===== XP & LEVEL UP FUNCTION =====
    public void AddXP(int amount)
    {
        CurrentXP += amount;

        while (CurrentXP >= XpToNextLevel)
        {
            CurrentXP -= XpToNextLevel;
            LevelUp();
        }

        SavePlayerProgress();
    }

    private void LevelUp()
    {
        PlayerLevel++;
        XpToNextLevel += 5;
        Debug.Log($"🎉 LEVEL UP! Now Level {PlayerLevel}");

        OnStatsChanged?.Invoke(); // 🔔 Notify right after leveling up
    }
}
