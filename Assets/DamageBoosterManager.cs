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
        if (boostTimer > 0f)
        {
            boostTimer -= Time.deltaTime;

            if (boostTimer <= 0f)
            {
                boostTimer = 0f;
                globalDamageMultiplier = 1f;
                Debug.Log("💤 Global damage boost expired — back to normal (x1).");
            }
        }
    }

    public void ApplyGlobalDamageBoost(float multiplier, float duration)
    {
        if (multiplier <= 1f)
            multiplier = 1f;

        globalDamageMultiplier = multiplier;
        boostTimer = duration;

        Debug.Log($"💥 Global damage boost activated! Multiplier: x{multiplier} for {duration} seconds.");
    }

    public bool IsBoostActive()
    {
        return boostTimer > 0f;
    }

    public float GetRemainingBoostTime()
    {
        return boostTimer;
    }
}
