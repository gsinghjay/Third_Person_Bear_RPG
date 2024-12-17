using UnityEngine;
using UnityEngine.UI;
using Combat;
using TMPro;

namespace UI
{
    public class PlayerHealthUI : MonoBehaviour
    {
        [SerializeField] private Slider healthSlider;
        [SerializeField] private TextMeshProUGUI healthText;
        private HealthComponent playerHealth;

        private void Start()
        {
            playerHealth = FindObjectOfType<Player.Core.PlayerHealthComponent>();
            
            if (playerHealth != null)
            {
                playerHealth.OnHealthChanged += UpdateHealthUI;
                UpdateHealthUI(playerHealth.CurrentHealth);
            }
            else
            {
                Debug.LogError("PlayerHealthUI: Could not find PlayerHealthComponent!");
            }
        }

        private void UpdateHealthUI(float currentHealth)
        {
            if (healthSlider != null)
            {
                healthSlider.maxValue = playerHealth.MaxHealth;
                healthSlider.value = currentHealth;
            }

            if (healthText != null)
            {
                healthText.text = $"Health: {currentHealth:F0}/{playerHealth.MaxHealth:F0}";
            }
        }

        private void OnDestroy()
        {
            if (playerHealth != null)
            {
                playerHealth.OnHealthChanged -= UpdateHealthUI;
            }
        }
    }
}
