using UnityEngine;
using System.Collections.Generic;

public class PoissonCoinSpawner : MonoBehaviour
{
    [System.Serializable]
    public class WeightedPrefab
    {
        public GameObject prefab;
        [Range(0f, 1f)]
        public float spawnChance; // Example: 0.5 = 50%
    }

    [Header("Poisson Sampling Settings")]
    [Tooltip("Minimum distance between spawned objects.")]
    public float minDistance = 1.5f;
    [Tooltip("Maximum radius within which objects can spawn.")]
    public float spawnRadius = 6f;
    [Tooltip("Clear area radius around the spawner’s center (no spawns).")]
    public float clearRadius = 1.0f;
    [Tooltip("Number of tries before rejecting a new Poisson sample.")]
    public int numSamplesBeforeRejection = 30;
    [Tooltip("Maximum number of objects to spawn per cycle.")]
    public int maxSpawnCount = 5;
    [Tooltip("How often to respawn coins (seconds).")]
    public float spawnInterval = 5f;

    [Header("Prefabs (Weighted RNG)")]
    public List<WeightedPrefab> weightedPrefabs;

    private List<GameObject> spawnedObjects = new List<GameObject>();
    private float timer = 0f;

    void Start()
    {
        SpawnCoins();
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer = 0f;
            SpawnCoins();
        }
    }

    void SpawnCoins()
    {
        // Cleanup previous objects
        foreach (GameObject obj in spawnedObjects)
        {
            if (obj != null) Destroy(obj);
        }
        spawnedObjects.Clear();

        // Generate new spawn points using Poisson Disk Sampling
        List<Vector2> points = GeneratePoissonPoints(minDistance, spawnRadius, numSamplesBeforeRejection);
        int spawnCount = 0;

        foreach (Vector2 offset in points)
        {
            if (offset.magnitude < clearRadius) continue;

            Vector3 spawnPos = transform.position + new Vector3(offset.x, 0f, offset.y);

            GameObject prefab = GetWeightedRandomPrefab();
            if (prefab == null) continue;

            GameObject obj = Instantiate(prefab, spawnPos, prefab.transform.rotation);
            spawnedObjects.Add(obj);

            spawnCount++;
            if (spawnCount >= maxSpawnCount) break;
        }

#if UNITY_ANDROID || UNITY_IOS
        if (spawnCount > 0)
        {
            Handheld.Vibrate();
        }
#endif
    }

    GameObject GetWeightedRandomPrefab()
    {
        if (weightedPrefabs == null || weightedPrefabs.Count == 0) return null;

        float total = 0f;
        foreach (var wp in weightedPrefabs)
            total += wp.spawnChance;

        float randomPoint = Random.value * total;

        foreach (var wp in weightedPrefabs)
        {
            if (randomPoint < wp.spawnChance)
                return wp.prefab;
            randomPoint -= wp.spawnChance;
        }

        return weightedPrefabs[weightedPrefabs.Count - 1].prefab; // fallback
    }

    List<Vector2> GeneratePoissonPoints(float radius, float circleRadius, int rejectionLimit)
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

            if (points.Count >= maxSpawnCount * 2) break;
        }

        return points;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, clearRadius);
    }
}
