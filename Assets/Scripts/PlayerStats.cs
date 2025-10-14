using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public int level = 1;
    public int currentXP = 0;
    public int xpToNextLevel = 10;

    // 👇 Simple event (UI can listen to this)
    public System.Action OnStatsChanged;

    public void AddXP(int amount)
    {
        currentXP += amount;
        if (currentXP >= xpToNextLevel)
        {
            currentXP = 0;
            level++;
        }

        // 🔔 Notify listeners (like the UI)
        OnStatsChanged?.Invoke();
    }

    // 👇 Optional: call this if you change stats in Inspector during Play mode
    private void OnValidate()
    {
        OnStatsChanged?.Invoke();
    }
}
