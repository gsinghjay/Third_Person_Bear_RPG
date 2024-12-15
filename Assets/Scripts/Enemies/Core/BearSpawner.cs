using UnityEngine;
using System.Collections.Generic;
using Enemies.Types;
using Enemies.Interfaces;

namespace Enemies.Core
{
    public class BearSpawner : MonoBehaviour
    {
        public enum ArenaType { Northwest, Northeast, Boss }
        
        [SerializeField] private GameObject normalBearPrefab;
        [SerializeField] private GameObject fireBearPrefab;
        [SerializeField] private GameObject iceBearPrefab;
        [SerializeField] private Terrain terrain;
        
        // Arena center GameObjects
        [SerializeField] private GameObject fireArenaCenter;
        [SerializeField] private GameObject iceArenaCenter;
        [SerializeField] private GameObject bossArenaCenter;
        
        private Dictionary<ArenaType, ArenaSettings> arenaSettings;

        private void Awake()
        {
            InitializeArenaSettings();
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

        public void SpawnArena(ArenaType arenaType)
        {
            if (!arenaSettings.TryGetValue(arenaType, out ArenaSettings settings))
            {
                Debug.LogError($"Arena settings not found for {arenaType}");
                return;
            }

            Vector2[] spawnPoints = GenerateArenaSpawnPoints(settings.position, settings.radius, 
                settings.normalBearCount + settings.fireBearCount + settings.iceBearCount);
            
            SpawnBears(spawnPoints, settings);
        }

        private Vector2[] GenerateArenaSpawnPoints(Vector2 center, float radius, int count)
        {
            List<Vector2> points = new List<Vector2>();
            float spawnRadius = radius * 0.6f;
            
            for (int i = 0; i < count; i++)
            {
                float angle = (360f / count) * i;
                float x = center.x + (Mathf.Cos(angle * Mathf.Deg2Rad) * spawnRadius);
                float z = center.y + (Mathf.Sin(angle * Mathf.Deg2Rad) * spawnRadius);
                points.Add(new Vector2(x, z));
            }
            
            return points.ToArray();
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
            Vector3 position = new Vector3(spawnPoint.x, 0, spawnPoint.y);
            GameObject bearObject = Instantiate(prefab, position, Quaternion.identity);
            
            if (bearObject.TryGetComponent<IBear>(out var bear))
            {
                bear.QuestId = questId;
                bear.OnDeath += HandleBearDeath;
                bear.Initialize(position);
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