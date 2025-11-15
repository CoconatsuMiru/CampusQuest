using UnityEngine;
using System;

public class CooldownManager : MonoBehaviour
{
    public static CooldownManager Instance { get; private set; }

    [Header("Cooldown Settings")]
    [Tooltip("Cooldown duration in minutes.")]
    public float cooldownMinutes = 2f;

    private const string LastQuizTimeKey = "LastQuizTime";

    private DateTime lastQuizTime;
    public bool IsCooldownActive { get; private set; }

    // ------------------------------
    void Awake()
    {
        // Singleton pattern (safe)
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }
    // ------------------------------

    void Start()
    {
        LoadCooldown();
    }

    void Update()
    {
        if (IsCooldownActive)
            CheckCooldown();
    }

    // ------------------------------
    // Load previously saved cooldown
    // ------------------------------
    private void LoadCooldown()
    {
        if (!PlayerPrefs.HasKey(LastQuizTimeKey))
        {
            IsCooldownActive = false;
            return;
        }

        string savedTime = PlayerPrefs.GetString(LastQuizTimeKey);

        if (!DateTime.TryParse(savedTime, out lastQuizTime))
        {
            // corrupted or invalid data
            IsCooldownActive = false;
            PlayerPrefs.DeleteKey(LastQuizTimeKey);
            return;
        }

        double minsPassed = (DateTime.Now - lastQuizTime).TotalMinutes;

        if (minsPassed < cooldownMinutes)
            IsCooldownActive = true;
        else
            EndCooldown();
    }

    // ------------------------------
    // Check the remaining time
    // ------------------------------
    private void CheckCooldown()
    {
        double secondsLeft = (cooldownMinutes * 60) - (DateTime.Now - lastQuizTime).TotalSeconds;

        if (secondsLeft <= 0)
            EndCooldown();
    }

    // ------------------------------
    // Start cooldown timer
    // ------------------------------
    public void StartCooldown()
    {
        lastQuizTime = DateTime.Now;

        PlayerPrefs.SetString(LastQuizTimeKey, lastQuizTime.ToString());
        PlayerPrefs.Save();

        IsCooldownActive = true;
    }

    // ------------------------------
    // End cooldown, reset everything
    // ------------------------------
    private void EndCooldown()
    {
        IsCooldownActive = false;
        PlayerPrefs.DeleteKey(LastQuizTimeKey);
    }

    // ------------------------------
    // Get remaining time in seconds
    // ------------------------------
    public double GetRemainingSeconds()
    {
        if (!IsCooldownActive) return 0;

        return (cooldownMinutes * 60) - (DateTime.Now - lastQuizTime).TotalSeconds;
    }
}
