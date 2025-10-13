using UnityEngine;

public class DamageBoostManager : MonoBehaviour
{
    public static DamageBoostManager Instance { get; private set; }

    [Header("Current Boost State")]
    [Tooltip("Current global multiplier applied to all damage sources.")]
    public float globalDamageMultiplier = 1f;

    private float boostTimer = 0f;

    void Awake()
    {
        // Make sure only one instance exists and persists across scenes
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        // Decrease timer if boost is active
        if (boostTimer > 0f)
        {
            boostTimer -= Time.deltaTime;

            if (boostTimer <= 0f)
            {
                boostTimer = 0f;
                globalDamageMultiplier = 1f;
                Debug.Log("🔥 Global damage boost expired — back to normal (x1).");
            }
        }
    }

    /// <summary>
    /// Apply a temporary global damage multiplier boost.
    /// </summary>
    /// <param name="multiplier">Damage multiplier (e.g. 2 = double damage).</param>
    /// <param name="duration">Duration in seconds.</param>
    public void ApplyGlobalDamageBoost(float multiplier, float duration)
    {
        if (multiplier <= 1f)
            multiplier = 1f; // Prevent lower or invalid values

        globalDamageMultiplier = multiplier;
        boostTimer = duration;

        Debug.Log($"💥 Global damage boost activated! Multiplier: x{multiplier} for {duration} seconds.");
    }

    /// <summary>
    /// Returns whether a boost is currently active.
    /// </summary>
    public bool IsBoostActive()
    {
        return boostTimer > 0f;
    }

    /// <summary>
    /// Returns remaining boost time (in seconds).
    /// </summary>
    public float GetRemainingBoostTime()
    {
        return boostTimer;
    }
}
