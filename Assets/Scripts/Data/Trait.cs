using UnityEngine;
using System.Collections.Generic;

namespace PixelGame
{
    /// <summary>
    /// ScriptableObject representing a trait that can be selected on level-up
    /// </summary>
    [CreateAssetMenu(fileName = "New Trait", menuName = "PixelGame/Trait")]
    public class Trait : ScriptableObject
    {
        [Header("Basic Info")]
        public string traitName = "New Trait";
        [TextArea(3, 5)]
        public string description = "Trait description...";
        public Sprite icon;
        public TraitRarity rarity = TraitRarity.Common;
        public TraitCategory category = TraitCategory.Offensive;

        [Header("Stat Modifiers")]
        public List<StatModifier> statModifiers = new List<StatModifier>();

        [Header("Special Effects")]
        public bool hasSpecialEffect = false;
        public string specialEffectDescription = "";

        /// <summary>
        /// Apply this trait's effects to player stats
        /// </summary>
        public void Apply(PlayerStats stats)
        {
            if (stats == null) return;

            foreach (var modifier in statModifiers)
            {
                if (modifier.isMultiplicative)
                {
                    stats.AddMultiplierModifier(modifier.statType, modifier.value);
                }
                else
                {
                    stats.AddFlatModifier(modifier.statType, modifier.value);
                }
            }

            // Apply special effects (implement custom logic per trait)
            if (hasSpecialEffect)
            {
                ApplySpecialEffect(stats);
            }

            GameEvents.TraitSelected(this);
        }

        /// <summary>
        /// Override in trait-specific implementations for custom effects
        /// </summary>
        protected virtual void ApplySpecialEffect(PlayerStats stats)
        {
            // Custom effect logic here
            // Examples: on-hit effects, proc chances, unique mechanics
        }

        /// <summary>
        /// Get formatted description with values highlighted
        /// </summary>
        public string GetFormattedDescription()
        {
            string formatted = description;

            foreach (var modifier in statModifiers)
            {
                string valueStr = modifier.isMultiplicative
                    ? $"+{(modifier.value * 100):F0}%"
                    : $"+{modifier.value:F1}";

                formatted = formatted.Replace($"[{modifier.statType}]", $"<color=yellow>{valueStr}</color>");
            }

            return formatted;
        }
    }

    [System.Serializable]
    public class StatModifier
    {
        public StatType statType;
        public float value;
        public bool isMultiplicative = false; // false = flat, true = percentage
    }

    public enum TraitRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }

    public enum TraitCategory
    {
        Offensive,
        Defensive,
        Utility,
        Mobility,
        Resource,
        Special
    }
}
