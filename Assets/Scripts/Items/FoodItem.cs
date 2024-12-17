// Assets/Scripts/Items/FoodItem.cs
using UnityEngine;
using Items.Interfaces;
using Player.Core;

namespace Items
{
    public class FoodItem : MonoBehaviour, ICollectible
    {
        [Header("Healing Properties")]
        [SerializeField] private float healAmount = 20f;
        [SerializeField] private float rotationSpeed = 50f;
        [SerializeField] private float bobSpeed = 1f;
        [SerializeField] private float bobHeight = 0.5f;
        
        private Vector3 startPosition;
        private float bobTime;

        private void Start()
        {
            startPosition = transform.position;
            bobTime = Random.Range(0f, 2f * Mathf.PI); // Random start phase
        }

        private void Update()
        {
            // Rotate the item
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
            
            // Bob up and down
            bobTime += Time.deltaTime;
            float newY = startPosition.y + Mathf.Sin(bobTime * bobSpeed) * bobHeight;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }

        public void OnCollect(GameObject collector)
        {
            var playerHealth = collector.GetComponent<PlayerHealthComponent>();
            if (playerHealth != null)
            {
                playerHealth.Heal(healAmount);
                Debug.Log($"Player healed for {healAmount} by food item");
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                OnCollect(other.gameObject);
            }
        }
    }
}