using UnityEngine;
using System.Collections.Generic;
using Enemies.Interfaces;
using Enemies.Types;

namespace Enemies.Core
{
    public class BearSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject normalBearPrefab;
        [SerializeField] private GameObject fireBearPrefab;
        [SerializeField] private GameObject iceBearPrefab;
        [SerializeField] private Terrain terrain;

        // New serialized fields for arena center game objects
        [SerializeField] private GameObject fireArenaCenter;
        [SerializeField] private GameObject iceArenaCenter;
        [SerializeField] private GameObject bossArenaCenter;

        // Update arenaSettings to use game objects
        private (Vector2 position, float radius)[] arenaSettings;

        private void Awake()
        {
            // Initialize arenaSettings using the positions of the game objects
            arenaSettings = new[]
            {
                (new Vector2(fireArenaCenter.transform.position.x / terrain.terrainData.size.x, 
                             fireArenaCenter.transform.position.z / terrain.terrainData.size.z), 35f),
                (new Vector2(iceArenaCenter.transform.position.x / terrain.terrainData.size.x, 
                             iceArenaCenter.transform.position.z / terrain.terrainData.size.z), 40f),
                (new Vector2(bossArenaCenter.transform.position.x / terrain.terrainData.size.x, 
                             bossArenaCenter.transform.position.z / terrain.terrainData.size.z), 50f)
            };
        }

        private Vector2[] GenerateArenaSpawnPoints(Vector2 arenaCenter, float radius, int bearCount, int arenaIndex)
        {
            List<Vector2> points = new List<Vector2>();
            float spawnRadius = radius * 0.6f; // Match TerrainGenerator's 60% spawn radius
            
            Debug.Log($"Arena {arenaIndex} - Center: {arenaCenter}, Radius: {radius}, SpawnRadius: {spawnRadius}");
            
            for (int i = 0; i < bearCount; i++)
            {
                float angle = (360f / bearCount) * i;
                
                // Calculate normalized coordinates with proper scaling
                float normalizedRadius = spawnRadius / terrain.terrainData.size.x;
                float x = arenaCenter.x + (Mathf.Cos(angle * Mathf.Deg2Rad) * normalizedRadius);
                float y = arenaCenter.y + (Mathf.Sin(angle * Mathf.Deg2Rad) * normalizedRadius);
                
                points.Add(new Vector2(x, y));
                Debug.Log($"Arena {arenaIndex} - Spawn point {i}: Normalized({x}, {y})");
            }
            
            return points.ToArray();
        }

        private Vector3[] AdjustSpawnPointsToTerrain(Vector2[] points)
        {
            Vector3[] adjustedPoints = new Vector3[points.Length];
            
            for (int i = 0; i < points.Length; i++)
            {
                // Ensure points stay within terrain bounds
                float x = Mathf.Clamp01(points[i].x);
                float z = Mathf.Clamp01(points[i].y);
                
                float worldX = x * terrain.terrainData.size.x;
                float worldZ = z * terrain.terrainData.size.z;
                float height = terrain.SampleHeight(new Vector3(worldX, 0, worldZ));
                
                adjustedPoints[i] = new Vector3(worldX, height, worldZ);
                Debug.Log($"Arena Point {i}: World({worldX}, {height}, {worldZ})");
            }
            
            return adjustedPoints;
        }

        void Start()
        {
            if (terrain == null)
            {
                terrain = FindObjectOfType<Terrain>();
                if (terrain == null)
                {
                    Debug.LogError("No terrain found in scene!");
                    return;
                }
            }

            // Add these debug logs
            foreach (var (pos, rad) in arenaSettings)
            {
                Debug.Log($"Arena Settings - Position: {pos}, Radius: {rad}");
            }

            Debug.Log($"Terrain size: {terrain.terrainData.size}");
            
            // Match TerrainGenerator bear counts exactly
            SpawnBearsInArena(0, 2, 2, 0); // Fire Arena: 2 normal, 2 fire
            SpawnBearsInArena(1, 1, 0, 3); // Ice Arena: 1 normal, 3 ice
            SpawnBearsInArena(2, 2, 2, 2); // Boss Arena: 2 of each
        }

        private void SpawnBearsInArena(int arenaIndex, int normalCount, int fireCount, int iceCount)
        {
            if (arenaIndex >= arenaSettings.Length)
            {
                Debug.LogError($"Invalid arena index: {arenaIndex}");
                return;
            }

            var (arenaCenter, arenaRadius) = arenaSettings[arenaIndex];
            Debug.Log($"SpawnBearsInArena {arenaIndex} - Center: {arenaCenter}, Radius: {arenaRadius}");
            
            ArenaSettings arena = new ArenaSettings
            {
                position = arenaCenter,
                normalBearCount = normalCount,
                fireBearCount = fireCount,
                iceBearCount = iceCount
            };

            int totalBears = normalCount + fireCount + iceCount;
            Debug.Log($"SpawnBearsInArena {arenaIndex} - Total Bears: {totalBears}");
            
            Vector2[] spawnPoints = GenerateArenaSpawnPoints(arenaCenter, arenaRadius, totalBears, arenaIndex);
            Debug.Log($"SpawnBearsInArena {arenaIndex} - Generated {spawnPoints.Length} spawn points");
            
            SpawnBears(spawnPoints, arena);
        }

        // Updated visualization
        private void OnDrawGizmos()
        {
            if (terrain == null) return;

            for (int i = 0; i < arenaSettings.Length; i++)
            {
                var (arenaCenter, radius) = arenaSettings[i];
                float spawnRadius = radius * 0.6f;
                
                // Draw arena center
                Gizmos.color = Color.blue;
                Vector3 centerWorld = new Vector3(
                    arenaCenter.x * terrain.terrainData.size.x,
                    terrain.SampleHeight(new Vector3(arenaCenter.x * terrain.terrainData.size.x, 0, arenaCenter.y * terrain.terrainData.size.z)),
                    arenaCenter.y * terrain.terrainData.size.z
                );
                Gizmos.DrawSphere(centerWorld, 3f);
                
                // Draw arena boundary
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(centerWorld, radius);
                
                // Draw spawn radius
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(centerWorld, spawnRadius);

                // Draw spawn points
                Gizmos.color = Color.red;
                int totalBears = (i == 0) ? 4 : (i == 1) ? 4 : 6;
                Vector2[] spawnPoints = GenerateArenaSpawnPoints(arenaCenter, radius, totalBears, i);
                Vector3[] worldPoints = AdjustSpawnPointsToTerrain(spawnPoints);
                
                foreach (Vector3 point in worldPoints)
                {
                    Gizmos.DrawSphere(point, 2f);
                    Gizmos.DrawLine(centerWorld, point);
                }
            }
        }

        public void SpawnBears(Vector2[] spawnPoints, ArenaSettings arena)
        {
            // Validate input
            int totalBearsNeeded = arena.normalBearCount + arena.fireBearCount + arena.iceBearCount;
            if (spawnPoints.Length < totalBearsNeeded)
            {
                Debug.LogError($"Not enough spawn points ({spawnPoints.Length}) for requested bears ({totalBearsNeeded})");
                return;
            }

            int spawnIndex = 0;
            Vector3[] adjustedSpawnPoints = AdjustSpawnPointsToTerrain(spawnPoints);

            // Spawn normal bears
            for (int i = 0; i < arena.normalBearCount && spawnIndex < adjustedSpawnPoints.Length; i++)
            {
                SpawnBear(normalBearPrefab, adjustedSpawnPoints[spawnIndex++]);
            }

            // Spawn fire bears
            for (int i = 0; i < arena.fireBearCount && spawnIndex < adjustedSpawnPoints.Length; i++)
            {
                SpawnBear(fireBearPrefab, adjustedSpawnPoints[spawnIndex++]);
            }

            // Spawn ice bears
            for (int i = 0; i < arena.iceBearCount && spawnIndex < adjustedSpawnPoints.Length; i++)
            {
                SpawnBear(iceBearPrefab, adjustedSpawnPoints[spawnIndex++]);
            }
        }

        private void SpawnBear(GameObject bearPrefab, Vector3 spawnPoint)
        {
            if (bearPrefab == null)
            {
                Debug.LogError("Bear prefab is null!");
                return;
            }

            Debug.Log($"Spawning bear at position: {spawnPoint}");
            GameObject bearObject = Instantiate(bearPrefab, spawnPoint, Quaternion.identity);
            if (bearObject == null)
            {
                Debug.LogError("Failed to instantiate bear prefab!");
                return;
            }

            // Add a temporary visual marker
            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            marker.transform.position = spawnPoint;
            marker.transform.localScale = Vector3.one * 2; // Adjust size as needed
            Destroy(marker, 5f); // Remove after 5 seconds

            IBear bear = bearObject.GetComponent<IBear>();
            
            if (bear == null)
            {
                Debug.LogError($"IBear component not found on prefab: {bearPrefab.name}");
                return;
            }
            
            bear.Initialize(spawnPoint);
        }
    }
} 