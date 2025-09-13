using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Collider))]
public class PoissonForWaypoints : MonoBehaviour
{
    [Header("Boss / Zone Settings")]
    public string bossID;                 // must match BossFightManager bossID
    public bool requiresBossDefeat = true;
    private bool zoneUnlocked = false;

    [Header("Reward Settings")]
    public float minDistance = 1.5f;
    public float spawnRadius = 6f;
    public float playerClearRadius = 1.0f;
    public int numSamplesBeforeRejection = 30;
    public int maxRewardCount = 5;        // how many reward prefabs to spawn
    public GameObject[] rewardPrefabs;    // chests, loot, coins, etc.

    [Header("Player Spawner Control")]
    public MonoBehaviour playerSpawnerScript; // drag your player spawner script here

    private List<GameObject> spawnedRewards = new List<GameObject>();
    private bool playerInsideZone = false;

    void Start()
    {
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    void Update()
    {
        if (!playerInsideZone) return;

        // zone must be unlocked by defeating boss
        if (requiresBossDefeat && !zoneUnlocked)
            return;
    }

    /// <summary>
    /// Called once when the boss is defeated.
    /// </summary>
    public void UnlockZone()
    {
        if (!zoneUnlocked)
        {
            zoneUnlocked = true;
            Debug.Log($"[Waypoint {gameObject.name}] Zone unlocked by defeating {bossID}!");

            if (playerInsideZone)
            {
                SpawnRewards();
            }
        }
    }

    private void SpawnRewards()
    {
        Debug.Log($"[Waypoint {gameObject.name}] Spawning REWARDS!");

        foreach (GameObject obj in spawnedRewards)
        {
            if (obj != null) Destroy(obj);
        }
        spawnedRewards.Clear();

        List<Vector2> points = GeneratePoissonPoints(minDistance, spawnRadius, numSamplesBeforeRejection);
        int spawnCount = 0;

        foreach (Vector2 offset in points)
        {
            if (offset.magnitude < playerClearRadius) continue;

            Vector3 spawnPos = transform.position + new Vector3(offset.x, 0f, offset.y);

            GameObject prefab = rewardPrefabs[Random.Range(0, rewardPrefabs.Length)];
            GameObject obj = Instantiate(prefab, spawnPos, prefab.transform.rotation);
            spawnedRewards.Add(obj);

            spawnCount++;
            if (spawnCount >= maxRewardCount) break;
        }

        Debug.Log($"[Waypoint {gameObject.name}] Spawned {spawnCount} rewards.");
        Handheld.Vibrate();
    }

    private List<Vector2> GeneratePoissonPoints(float radius, float circleRadius, int rejectionLimit)
    {
        List<Vector2> points = new List<Vector2>();
        List<Vector2> spawnPoints = new List<Vector2>();

        spawnPoints.Add(Vector2.zero);

        float cellSize = radius / Mathf.Sqrt(2);
        int gridSize = Mathf.CeilToInt((circleRadius * 2) / cellSize);
        int[,] grid = new int[gridSize, gridSize];

        while (spawnPoints.Count > 0)
        {
            int spawnIndex = Random.Range(0, spawnPoints.Count);
            Vector2 center = spawnPoints[spawnIndex];
            bool accepted = false;

            for (int i = 0; i < rejectionLimit; i++)
            {
                float angle = Random.value * Mathf.PI * 2;
                Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                Vector2 candidate = center + dir * Random.Range(radius, 2 * radius);

                if (candidate.magnitude > circleRadius) continue;

                int cellX = (int)((candidate.x + circleRadius) / cellSize);
                int cellY = (int)((candidate.y + circleRadius) / cellSize);

                bool valid = true;
                for (int x = Mathf.Max(0, cellX - 2); x <= Mathf.Min(gridSize - 1, cellX + 2); x++)
                {
                    for (int y = Mathf.Max(0, cellY - 2); y <= Mathf.Min(gridSize - 1, cellY + 2); y++)
                    {
                        int pointIndex = grid[x, y] - 1;
                        if (pointIndex != -1 && pointIndex < points.Count)
                        {
                            float sqrDist = (candidate - points[pointIndex]).sqrMagnitude;
                            if (sqrDist < radius * radius)
                            {
                                valid = false;
                                break;
                            }
                        }
                    }
                    if (!valid) break;
                }

                if (valid)
                {
                    points.Add(candidate);
                    spawnPoints.Add(candidate);
                    grid[cellX, cellY] = points.Count;
                    accepted = true;
                    break;
                }
            }

            if (!accepted)
            {
                spawnPoints.RemoveAt(spawnIndex);
            }

            if (points.Count >= maxRewardCount * 2) break;
        }

        return points;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, playerClearRadius);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInsideZone = true;
            Debug.Log($"[Waypoint {gameObject.name}] Player entered zone.");

            // disable player spawner while inside
            if (playerSpawnerScript != null)
            {
                playerSpawnerScript.enabled = false;
                Debug.Log($"[Waypoint {gameObject.name}] Disabled Player's spawner script.");
            }

            // spawn rewards if already unlocked
            if (zoneUnlocked)
            {
                SpawnRewards();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInsideZone = false;
            Debug.Log($"[Waypoint {gameObject.name}] Player exited zone.");

            foreach (GameObject obj in spawnedRewards)
            {
                if (obj != null) Destroy(obj);
            }
            spawnedRewards.Clear();

            if (playerSpawnerScript != null)
            {
                playerSpawnerScript.enabled = true;
                Debug.Log($"[Waypoint {gameObject.name}] Re-enabled Player's spawner script.");
            }
        }
    }
}
