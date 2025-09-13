using UnityEngine;
using System.Collections.Generic;

public class PoissonAroundPlayer : MonoBehaviour
{
    [Header("Poisson Sampling Settings")]
    public float minDistance = 1.5f;
    public float spawnRadius = 6f;
    public float playerClearRadius = 1.0f;
    public int numSamplesBeforeRejection = 30;
    public int maxSpawnCount = 5;
    public float spawnInterval = 5f;

    [Header("Prefabs")]
    public GameObject[] spawnPrefabs;

    [Header("Player")]
    public Transform player;

    private List<GameObject> spawnedObjects = new List<GameObject>();
    private float timer = 0f;

    void Start()
    {
        SpawnAroundPlayer();
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer = 0f;
            SpawnAroundPlayer();
        }
    }

    void SpawnAroundPlayer()
    {
        // Cleanup old objects
        foreach (GameObject obj in spawnedObjects)
        {
            if (obj != null) Destroy(obj);
        }
        spawnedObjects.Clear();

        // Generate new points around the player
        List<Vector2> points = GeneratePoissonPoints(minDistance, spawnRadius, numSamplesBeforeRejection);
        int spawnCount = 0;

        foreach (Vector2 offset in points)
        {
            if (offset.magnitude < playerClearRadius) continue;

            // Use XZ plane for 3D top-down
            Vector3 spawnPos = player.position + new Vector3(offset.x, 0f, offset.y);

            GameObject prefab = spawnPrefabs[Random.Range(0, spawnPrefabs.Length)];
            GameObject obj = Instantiate(prefab, spawnPos, prefab.transform.rotation);
            spawnedObjects.Add(obj);

            spawnCount++;
            if (spawnCount >= maxSpawnCount) break;
        }

        // Vibrate once after spawning objects if any were spawned
        if (spawnCount > 0)
        {
            Handheld.Vibrate();
        }
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
        if (player != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(player.position, spawnRadius);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(player.position, playerClearRadius);
        }
    }
}
