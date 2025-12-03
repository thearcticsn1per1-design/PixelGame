using System;
using System.Collections.Generic;
using UnityEngine;

namespace PixelGame
{
    /// <summary>
    /// Manages all player statistics including base stats, modifiers, and calculations
    /// </summary>
    public class PlayerStats : MonoBehaviour
    {
        #region Base Stats

        [Header("Health")]
        public float baseMaxHealth = 100f;
        public float currentHealth;
        public float baseHealthRegen = 0f;

        [Header("Damage")]
        public float baseDamage = 10f;
        public float basePhysicalDamage = 0f;
        public float baseMagicDamage = 0f;
        public float baseCritChance = 0.05f; // 5%
        public float baseCritMultiplier = 2f; // 200%

        [Header("Speed")]
        public float baseMoveSpeed = 5f;
        public float baseAttackSpeed = 1f;

        [Header("Defense")]
        public float baseArmor = 0f;
        public float baseDodgeChance = 0f;
        public float baseDamageReduction = 0f;

        [Header("Resources")]
        public float baseMaxMana = 100f;
        public float currentMana;
        public float baseManaRegen = 1f;

        [Header("Level")]
        public int currentLevel = 1;
        public int currentXP = 0;
        public int xpToNextLevel = 100;

        #endregion

        #region Stat Modifiers

        // Flat modifiers (added directly)
        private Dictionary<StatType, float> flatModifiers = new Dictionary<StatType, float>();

        // Multiplicative modifiers (percentage increases, stacks additively)
        private Dictionary<StatType, float> multiplierModifiers = new Dictionary<StatType, float>();

        // Cached calculated stats (updated when modifiers change)
        private Dictionary<StatType, float> cachedStats = new Dictionary<StatType, float>();
        private bool statsDirty = true;

        #endregion

        #region Events

        public event Action<float, float> OnHealthChanged;
        public event Action<float, float> OnManaChanged;
        public event Action<int> OnLevelChanged;
        public event Action<int, int> OnXPChanged;

        #endregion

        /// <summary>
        /// Initialize stats with base values
        /// </summary>
        public void Initialize()
        {
            currentHealth = GetStat(StatType.MaxHealth);
            currentMana = GetStat(StatType.MaxMana);
            MarkStatsDirty();
        }

        #region Stat Calculation

        /// <summary>
        /// Get final calculated value for a stat
        /// </summary>
        public float GetStat(StatType statType)
        {
            if (statsDirty)
            {
                RecalculateAllStats();
            }

            if (cachedStats.TryGetValue(statType, out float value))
            {
                return value;
            }

            return GetBaseStat(statType);
        }

        /// <summary>
        /// Get base stat value before modifiers
        /// </summary>
        private float GetBaseStat(StatType statType)
        {
            switch (statType)
            {
                case StatType.MaxHealth: return baseMaxHealth;
                case StatType.HealthRegen: return baseHealthRegen;
                case StatType.Damage: return baseDamage;
                case StatType.PhysicalDamage: return basePhysicalDamage;
                case StatType.MagicDamage: return baseMagicDamage;
                case StatType.CritChance: return baseCritChance;
                case StatType.CritMultiplier: return baseCritMultiplier;
                case StatType.MoveSpeed: return baseMoveSpeed;
                case StatType.AttackSpeed: return baseAttackSpeed;
                case StatType.Armor: return baseArmor;
                case StatType.DodgeChance: return baseDodgeChance;
                case StatType.DamageReduction: return baseDamageReduction;
                case StatType.MaxMana: return baseMaxMana;
                case StatType.ManaRegen: return baseManaRegen;
                default: return 0f;
            }
        }

        /// <summary>
        /// Recalculate all stats with modifiers
        /// </summary>
        private void RecalculateAllStats()
        {
            cachedStats.Clear();

            foreach (StatType statType in System.Enum.GetValues(typeof(StatType)))
            {
                float baseValue = GetBaseStat(statType);
                float flatMod = GetFlatModifier(statType);
                float multMod = GetMultiplierModifier(statType);

                // Formula: (Base + Flat) * (1 + Multiplier)
                float finalValue = (baseValue + flatMod) * (1f + multMod);

                // Clamp certain stats
                finalValue = ClampStat(statType, finalValue);

                cachedStats[statType] = finalValue;
            }

            statsDirty = false;
        }

        /// <summary>
        /// Clamp stat values to valid ranges
        /// </summary>
        private float ClampStat(StatType statType, float value)
        {
            switch (statType)
            {
                case StatType.CritChance:
                case StatType.DodgeChance:
                    return Mathf.Clamp01(value); // 0-100%

                case StatType.MoveSpeed:
                    return Mathf.Max(0.1f, value); // Minimum move speed

                case StatType.AttackSpeed:
                    return Mathf.Clamp(value, 0.1f, 10f); // Min 0.1x, Max 10x

                case StatType.DamageReduction:
                    return Mathf.Clamp(value, 0f, 0.9f); // Max 90% reduction

                default:
                    return Mathf.Max(0, value); // Most stats can't be negative
            }
        }

        #endregion

        #region Modifier Management

        /// <summary>
        /// Add a flat modifier to a stat
        /// </summary>
        public void AddFlatModifier(StatType statType, float value)
        {
            if (!flatModifiers.ContainsKey(statType))
            {
                flatModifiers[statType] = 0f;
            }

            flatModifiers[statType] += value;
            MarkStatsDirty();
        }

        /// <summary>
        /// Add a multiplier modifier to a stat (0.1 = +10%)
        /// </summary>
        public void AddMultiplierModifier(StatType statType, float value)
        {
            if (!multiplierModifiers.ContainsKey(statType))
            {
                multiplierModifiers[statType] = 0f;
            }

            multiplierModifiers[statType] += value;
            MarkStatsDirty();
        }

        /// <summary>
        /// Remove a flat modifier from a stat
        /// </summary>
        public void RemoveFlatModifier(StatType statType, float value)
        {
            if (flatModifiers.ContainsKey(statType))
            {
                flatModifiers[statType] -= value;
                MarkStatsDirty();
            }
        }

        /// <summary>
        /// Remove a multiplier modifier from a stat
        /// </summary>
        public void RemoveMultiplierModifier(StatType statType, float value)
        {
            if (multiplierModifiers.ContainsKey(statType))
            {
                multiplierModifiers[statType] -= value;
                MarkStatsDirty();
            }
        }

        /// <summary>
        /// Clear all modifiers
        /// </summary>
        public void ClearAllModifiers()
        {
            flatModifiers.Clear();
            multiplierModifiers.Clear();
            MarkStatsDirty();
        }

        private float GetFlatModifier(StatType statType)
        {
            return flatModifiers.TryGetValue(statType, out float value) ? value : 0f;
        }

        private float GetMultiplierModifier(StatType statType)
        {
            return multiplierModifiers.TryGetValue(statType, out float value) ? value : 0f;
        }

        private void MarkStatsDirty()
        {
            statsDirty = true;
        }

        #endregion

        #region Health/Mana Management

        /// <summary>
        /// Modify current health by amount
        /// </summary>
        public void ModifyHealth(float amount)
        {
            float maxHealth = GetStat(StatType.MaxHealth);
            currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        /// <summary>
        /// Set health to maximum
        /// </summary>
        public void HealToFull()
        {
            float maxHealth = GetStat(StatType.MaxHealth);
            currentHealth = maxHealth;
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        /// <summary>
        /// Modify current mana by amount
        /// </summary>
        public void ModifyMana(float amount)
        {
            float maxMana = GetStat(StatType.MaxMana);
            currentMana = Mathf.Clamp(currentMana + amount, 0, maxMana);
            OnManaChanged?.Invoke(currentMana, maxMana);
        }

        /// <summary>
        /// Check if player has enough mana
        /// </summary>
        public bool HasMana(float amount)
        {
            return currentMana >= amount;
        }

        /// <summary>
        /// Try to spend mana
        /// </summary>
        public bool SpendMana(float amount)
        {
            if (HasMana(amount))
            {
                ModifyMana(-amount);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Get health as percentage (0-1)
        /// </summary>
        public float GetHealthPercentage()
        {
            float maxHealth = GetStat(StatType.MaxHealth);
            return maxHealth > 0 ? currentHealth / maxHealth : 0;
        }

        /// <summary>
        /// Get mana as percentage (0-1)
        /// </summary>
        public float GetManaPercentage()
        {
            float maxMana = GetStat(StatType.MaxMana);
            return maxMana > 0 ? currentMana / maxMana : 0;
        }

        #endregion

        #region Level/XP Management

        /// <summary>
        /// Add experience points
        /// </summary>
        public void AddXP(int amount)
        {
            // Apply XP multiplier
            float xpMultiplier = GetStat(StatType.ExperienceGain);
            int modifiedAmount = Mathf.RoundToInt(amount * (1f + xpMultiplier));

            currentXP += modifiedAmount;
            OnXPChanged?.Invoke(currentXP, xpToNextLevel);
            GameEvents.PlayerXPGained(modifiedAmount, currentXP, xpToNextLevel);

            // Check for level up
            while (currentXP >= xpToNextLevel)
            {
                LevelUp();
            }
        }

        /// <summary>
        /// Level up the player
        /// </summary>
        private void LevelUp()
        {
            currentLevel++;
            currentXP -= xpToNextLevel;
            xpToNextLevel = CalculateXPForNextLevel();

            // Small stat increases on level up
            baseMaxHealth += 5;
            baseDamage += 1;

            // Heal to full on level up
            HealToFull();
            ModifyMana(GetStat(StatType.MaxMana)); // Fill mana

            OnLevelChanged?.Invoke(currentLevel);
            GameEvents.PlayerLevelUp(currentLevel);
        }

        /// <summary>
        /// Calculate XP required for next level
        /// </summary>
        private int CalculateXPForNextLevel()
        {
            // Exponential scaling: 100 * (1.15 ^ (level - 1))
            return Mathf.RoundToInt(100f * Mathf.Pow(1.15f, currentLevel - 1));
        }

        #endregion

        #region Regeneration

        /// <summary>
        /// Apply health and mana regeneration (call this in Update)
        /// </summary>
        public void ApplyRegeneration(float deltaTime)
        {
            // Health regen
            float healthRegen = GetStat(StatType.HealthRegen);
            if (healthRegen > 0)
            {
                ModifyHealth(healthRegen * deltaTime);
            }

            // Mana regen
            float manaRegen = GetStat(StatType.ManaRegen);
            if (manaRegen > 0)
            {
                ModifyMana(manaRegen * deltaTime);
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Calculate damage reduction from armor
        /// Formula: Reduction = Armor / (Armor + 100)
        /// </summary>
        public float CalculateDamageReduction()
        {
            float armor = GetStat(StatType.Armor);
            float reduction = armor / (armor + 100f);
            float additionalReduction = GetStat(StatType.DamageReduction);

            return Mathf.Clamp01(reduction + additionalReduction);
        }

        /// <summary>
        /// Check if attack should dodge
        /// </summary>
        public bool RollDodge()
        {
            float dodgeChance = GetStat(StatType.DodgeChance);
            return UnityEngine.Random.value < dodgeChance;
        }

        /// <summary>
        /// Check if attack should crit
        /// </summary>
        public bool RollCritical()
        {
            float critChance = GetStat(StatType.CritChance);
            return UnityEngine.Random.value < critChance;
        }

        /// <summary>
        /// Calculate final damage output
        /// </summary>
        public float CalculateDamageOutput(float baseDamageMultiplier = 1f)
        {
            float damage = GetStat(StatType.Damage) * baseDamageMultiplier;

            if (RollCritical())
            {
                float critMult = GetStat(StatType.CritMultiplier);
                damage *= critMult;
                return damage;
            }

            return damage;
        }

        #endregion
    }
}
