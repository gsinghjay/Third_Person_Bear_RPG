// Assets/Scripts/Items/FoodDropManager.cs
using UnityEngine;
using System.Collections.Generic;

namespace Items
{
    public class FoodDropManager : MonoBehaviour
    {
        private static FoodDropManager instance;
        public static FoodDropManager Instance => instance;

        [System.Serializable]
        public class FoodDrop
        {
            public GameObject prefab;
            public float dropChance;
        }

        [Header("Drop Settings")]
        [SerializeField] private List<FoodDrop> possibleDrops;
        [SerializeField] private float dropHeight = 1f;
        [SerializeField] private float dropForwardOffset = 2f;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void SpawnFoodDrop(Vector3 position, Quaternion rotation)
        {
            foreach (var drop in possibleDrops)
            {
                if (Random.value <= drop.dropChance)
                {
                    // Calculate drop position
                    Vector3 dropPosition = position + Vector3.up * dropHeight;
                    dropPosition += rotation * Vector3.forward * dropForwardOffset;

                    // Ensure the drop position is above the ground
                    if (Physics.Raycast(dropPosition + Vector3.up * 2f, Vector3.down, out RaycastHit hit))
                    {
                        dropPosition.y = hit.point.y + dropHeight;
                    }

                    Instantiate(drop.prefab, dropPosition, Quaternion.identity);
                    break; // Only spawn one food item
                }
            }
        }
    }
}