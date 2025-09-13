using UnityEngine;
using System.Collections.Generic;

public class BossManager : MonoBehaviour
{
    public static BossManager Instance;

    private HashSet<string> defeatedBosses = new HashSet<string>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void MarkBossDefeated(string bossID)
    {
        if (string.IsNullOrEmpty(bossID)) return;
        defeatedBosses.Add(bossID);
        PlayerPrefs.SetInt("boss_" + bossID, 1);
        PlayerPrefs.Save();
        Debug.Log($"[BossManager] Boss '{bossID}' marked defeated.");
    }

    public bool IsBossDefeated(string bossID)
    {
        if (string.IsNullOrEmpty(bossID)) return false;

        if (defeatedBosses.Contains(bossID))
            return true;

        if (PlayerPrefs.GetInt("boss_" + bossID, 0) == 1)
        {
            defeatedBosses.Add(bossID);
            return true;
        }

        return false;
    }
}
