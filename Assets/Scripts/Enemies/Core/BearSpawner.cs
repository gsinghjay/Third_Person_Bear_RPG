using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using Enemies.Types;
using Enemies.Interfaces;

namespace Enemies.Core
{
    public class BearSpawner : MonoBehaviour
    {
        public enum ArenaType { Northwest, Northeast, Boss }
        
        [Header("Bear Prefabs")]
        [SerializeField] private GameObject normalBearPrefab;
        [SerializeField] private GameObject fireBearPrefab;
        [SerializeField] private GameObject iceBearPrefab;
        
        [Header("Arena References")]
        [SerializeField] private Terrain terrain;
        [SerializeField] private GameObject fireArenaCenter;
        [SerializeField] private GameObject iceArenaCenter;
        [SerializeField] private GameObject bossArenaCenter;
        
        private Dictionary<ArenaType, ArenaSettings> arenaSettings;
        public bool IsInitialized { get; private set; }

        private void Awake()
        {
            ValidateReferences();
            InitializeArenaSettings();
            IsInitialized = true;
        }

        private void InitializeArenaSettings()
        {
            arenaSettings = new Dictionary<ArenaType, ArenaSettings>
            {
                { ArenaType.Northwest, new ArenaSettings 
                    {
                        questId = "northwest_arena",
                        name = "Northwest Arena",
                        position = new Vector2(
                            fireArenaCenter.transform.position.x,
                            fireArenaCenter.transform.position.z
                        ),
                        radius = 35f,
                        normalBearCount = 4,
                        fireBearCount = 0,
                        iceBearCount = 0
                    }
                },
                { ArenaType.Northeast, new ArenaSettings
                    {
                        questId = "northeast_arena",
                        name = "Northeast Arena",
                        position = new Vector2(
                            iceArenaCenter.transform.position.x,
                            iceArenaCenter.transform.position.z
                        ),
                        radius = 40f,
                        normalBearCount = 2,
                        fireBearCount = 2,
                        iceBearCount = 0
                    }
                },
                { ArenaType.Boss, new ArenaSettings
                    {
                        questId = "boss_arena",
                        name = "Boss Arena",
                        position = new Vector2(
                            bossArenaCenter.transform.position.x,
                            bossArenaCenter.transform.position.z
                        ),
                        radius = 50f,
                        normalBearCount = 2,
                        fireBearCount = 2,
                        iceBearCount = 2
                    }
                }
            };
        }

        private void ValidateReferences()
        {
            if (!terrain) 
            {
                terrain = FindObjectOfType<Terrain>();
                if (!terrain) Debug.LogError("No terrain found!");
            }
            
            if (!fireArenaCenter) Debug.LogError("Fire Arena Center is missing!");
            if (!iceArenaCenter) Debug.LogError("Ice Arena Center is missing!");
            if (!bossArenaCenter) Debug.LogError("Boss Arena Center is missing!");
            
            if (!normalBearPrefab) Debug.LogError("Normal Bear Prefab is missing!");
            if (!fireBearPrefab) Debug.LogError("Fire Bear Prefab is missing!");
            if (!iceBearPrefab) Debug.LogError("Ice Bear Prefab is missing!");
        }

        public void SpawnArena(ArenaType arenaType)
        {
            if (!IsInitialized)
            {
                Debug.LogError("BearSpawner not initialized!");
                return;
            }

            if (!arenaSettings.TryGetValue(arenaType, out ArenaSettings settings))
            {
                Debug.LogError($"Arena settings not found for {arenaType}");
                return;
            }

            Debug.Log($"Spawning {arenaType} arena at position {settings.position}");
            
            Vector2[] spawnPoints = GenerateArenaSpawnPoints(settings.position, settings.radius, 
                settings.normalBearCount + settings.fireBearCount + settings.iceBearCount);
            
            SpawnBears(spawnPoints, settings);
        }

        private Vector2[] GenerateArenaSpawnPoints(Vector2 center, float radius, int count)
        {
            List<Vector2> points = new List<Vector2>();
            float spawnRadius = radius * 0.6f;
            int maxAttempts = 30; // Prevent infinite loops
            
            for (int i = 0; i < count; i++)
            {
                Vector2 validPoint = FindValidSpawnPoint(center, spawnRadius, maxAttempts);
                points.Add(validPoint);
            }
            
            return points.ToArray();
        }

        private Vector2 FindValidSpawnPoint(Vector2 center, float radius, int maxAttempts)
        {
            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                float angle = Random.Range(0f, 360f);
                float distance = Random.Range(radius * 0.3f, radius);
                
                float x = center.x + (Mathf.Cos(angle * Mathf.Deg2Rad) * distance);
                float z = center.y + (Mathf.Sin(angle * Mathf.Deg2Rad) * distance);
                
                Vector3 worldPoint = new Vector3(x, 1000f, z); // Start high
                
                // Sample terrain height
                float terrainHeight = terrain.SampleHeight(worldPoint);
                worldPoint.y = terrainHeight;

                // Check if point is on NavMesh
                NavMeshHit hit;
                if (NavMesh.SamplePosition(worldPoint, out hit, 5f, NavMesh.AllAreas))
                {
                    return new Vector2(hit.position.x, hit.position.z);
                }
            }
            
            // Fallback to center point if no valid position found
            Debug.LogWarning("Could not find valid spawn point, using center position");
            return center;
        }

        private void SpawnBears(Vector2[] spawnPoints, ArenaSettings arena)
        {
            int spawnIndex = 0;

            // Spawn normal bears
            for (int i = 0; i < arena.normalBearCount; i++)
            {
                SpawnBear(normalBearPrefab, spawnPoints[spawnIndex++], arena.questId);
            }

            // Spawn fire bears
            for (int i = 0; i < arena.fireBearCount; i++)
            {
                SpawnBear(fireBearPrefab, spawnPoints[spawnIndex++], arena.questId);
            }

            // Spawn ice bears
            for (int i = 0; i < arena.iceBearCount; i++)
            {
                SpawnBear(iceBearPrefab, spawnPoints[spawnIndex++], arena.questId);
            }
        }

        private void SpawnBear(GameObject prefab, Vector2 spawnPoint, string questId)
        {
            if (prefab == null)
            {
                Debug.LogError("Bear prefab is null!");
                return;
            }

            // Convert 2D point to 3D and find valid NavMesh position
            Vector3 spawnPosition = new Vector3(spawnPoint.x, 1000f, spawnPoint.y);
            float terrainHeight = terrain.SampleHeight(spawnPosition);
            spawnPosition.y = terrainHeight;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(spawnPosition, out hit, 5f, NavMesh.AllAreas))
            {
                GameObject bearObject = Instantiate(prefab, hit.position, Quaternion.identity);
                
                if (bearObject.TryGetComponent<IBear>(out var bear))
                {
                    bear.QuestId = questId;
                    bear.OnDeath += HandleBearDeath;
                    bear.Initialize(hit.position);
                }
                else
                {
                    Debug.LogError($"Failed to get IBear component from spawned bear: {prefab.name}");
                }
            }
            else
            {
                Debug.LogError($"Could not find valid NavMesh position near {spawnPosition}");
            }
        }

        private void HandleBearDeath(IBear bear)
        {
            // Unsubscribe from the event
            bear.OnDeath -= HandleBearDeath;
            
            // Notify the QuestManager
            QuestManager.Instance.OnBearKilled(bear.QuestId);
        }

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying || terrain == null) return;

            foreach (var kvp in arenaSettings)
            {
                var settings = kvp.Value;
                Vector3 centerWorld = new Vector3(
                    settings.position.x,
                    terrain.SampleHeight(new Vector3(settings.position.x, 0, settings.position.y)),
                    settings.position.y
                );

                // Draw arena center
                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(centerWorld, 3f);
                
                // Draw arena boundary
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(centerWorld, settings.radius);
                
                // Draw spawn radius
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(centerWorld, settings.radius * 0.6f);

                // Draw NavMesh validation
                NavMeshHit hit;
                if (NavMesh.SamplePosition(centerWorld, out hit, 5f, NavMesh.AllAreas))
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawLine(centerWorld, hit.position);
                }
                else
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawWireCube(centerWorld, Vector3.one * 2f);
                }
            }
        }

        public ArenaSettings GetArenaSettings(ArenaType arenaType)
        {
            if (arenaSettings.TryGetValue(arenaType, out ArenaSettings settings))
            {
                return settings;
            }
            return null;
        }

        public bool ValidateNorthwestArena()
        {
            return fireArenaCenter != null && normalBearPrefab != null;
        }

        public bool ValidateNortheastArena()
        {
            return iceArenaCenter != null && normalBearPrefab != null && fireBearPrefab != null;
        }

        public bool ValidateBossArena()
        {
            return bossArenaCenter != null && 
                   normalBearPrefab != null && 
                   fireBearPrefab != null && 
                   iceBearPrefab != null;
        }

        // Make ArenaSettings public so it can be accessed
        public class ArenaSettings
        {
            public string questId;
            public string name;
            public Vector2 position;
            public float radius;
            public int normalBearCount;
            public int fireBearCount;
            public int iceBearCount;
        }
    }
}