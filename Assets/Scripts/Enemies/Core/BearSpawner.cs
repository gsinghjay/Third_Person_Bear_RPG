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

        public void SpawnBears(Vector2[] spawnPoints, ArenaSettings arena)
        {
            int spawnIndex = 0;

            // Spawn normal bears
            for (int i = 0; i < arena.normalBearCount; i++)
            {
                SpawnBear(normalBearPrefab, spawnPoints[spawnIndex++]);
            }

            // Spawn fire bears
            for (int i = 0; i < arena.fireBearCount; i++)
            {
                SpawnBear(fireBearPrefab, spawnPoints[spawnIndex++]);
            }

            // Spawn ice bears
            for (int i = 0; i < arena.iceBearCount; i++)
            {
                SpawnBear(iceBearPrefab, spawnPoints[spawnIndex++]);
            }
        }

        private void SpawnBear(GameObject bearPrefab, Vector2 spawnPoint)
        {
            Vector3 spawnPosition = new Vector3(
                spawnPoint.x,
                Terrain.activeTerrain.SampleHeight(new Vector3(spawnPoint.x, 0, spawnPoint.y)),
                spawnPoint.y
            );

            GameObject bearObject = Instantiate(bearPrefab, spawnPosition, Quaternion.identity);
            IBear bear = bearObject.GetComponent<IBear>();
            bear.Initialize(spawnPosition);
        }
    }
} 