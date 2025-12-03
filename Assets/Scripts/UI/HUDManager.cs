using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace PixelGame
{
    /// <summary>
    /// Manages the in-game HUD display
    /// </summary>
    public class HUDManager : MonoBehaviour
    {
        [Header("Health")]
        [SerializeField] private Slider healthBar;
        [SerializeField] private TextMeshProUGUI healthText;

        [Header("Mana")]
        [SerializeField] private Slider manaBar;
        [SerializeField] private TextMeshProUGUI manaText;

        [Header("XP/Level")]
        [SerializeField] private Slider xpBar;
        [SerializeField] private TextMeshProUGUI levelText;

        [Header("Gold")]
        [SerializeField] private TextMeshProUGUI goldText;

        [Header("Stats Display")]
        [SerializeField] private TextMeshProUGUI damageText;
        [SerializeField] private TextMeshProUGUI attackSpeedText;
        [SerializeField] private TextMeshProUGUI moveSpeedText;

        [Header("Floor Info")]
        [SerializeField] private TextMeshProUGUI floorText;

        private void OnEnable()
        {
            // Subscribe to events
            GameEvents.OnPlayerHealthChanged += UpdateHealth;
            GameEvents.OnPlayerXPGained += UpdateXP;
            GameEvents.OnPlayerLevelUp += UpdateLevel;
            GameEvents.OnGoldChanged += UpdateGold;
            GameEvents.OnFloorStarted += UpdateFloor;
        }

        private void OnDisable()
        {
            // Unsubscribe from events
            GameEvents.OnPlayerHealthChanged -= UpdateHealth;
            GameEvents.OnPlayerXPGained -= UpdateXP;
            GameEvents.OnPlayerLevelUp -= UpdateLevel;
            GameEvents.OnGoldChanged -= UpdateGold;
            GameEvents.OnFloorStarted -= UpdateFloor;
        }

        private void UpdateHealth(float currentHealth, float maxHealth)
        {
            if (healthBar != null)
            {
                healthBar.maxValue = maxHealth;
                healthBar.value = currentHealth;
            }

            if (healthText != null)
            {
                healthText.text = $"{currentHealth:F0} / {maxHealth:F0}";
            }
        }

        private void UpdateXP(int xpGained, int currentXP, int xpToNextLevel)
        {
            if (xpBar != null)
            {
                xpBar.maxValue = xpToNextLevel;
                xpBar.value = currentXP;
            }
        }

        private void UpdateLevel(int level)
        {
            if (levelText != null)
            {
                levelText.text = $"Level {level}";
            }
        }

        private void UpdateGold(int gold)
        {
            if (goldText != null)
            {
                goldText.text = $"Gold: {gold}";
            }
        }

        private void UpdateFloor(int floor)
        {
            if (floorText != null)
            {
                floorText.text = $"Floor {floor}";
            }
        }

        /// <summary>
        /// Update stat displays with player stats
        /// </summary>
        public void UpdateStatDisplays(PlayerStats stats)
        {
            if (stats == null) return;

            if (damageText != null)
            {
                damageText.text = $"DMG: {stats.GetStat(StatType.Damage):F1}";
            }

            if (attackSpeedText != null)
            {
                attackSpeedText.text = $"ATK SPD: {stats.GetStat(StatType.AttackSpeed):F2}x";
            }

            if (moveSpeedText != null)
            {
                moveSpeedText.text = $"MOVE SPD: {stats.GetStat(StatType.MoveSpeed):F1}";
            }
        }
    }
}
