using UnityEngine;
using System.Collections.Generic;

namespace PixelGame
{
    /// <summary>
    /// ScriptableObject defining a playable character class
    /// </summary>
    [CreateAssetMenu(fileName = "New Character Class", menuName = "PixelGame/Character Class")]
    public class CharacterClass : ScriptableObject
    {
        [Header("Basic Info")]
        public string className = "Warrior";
        [TextArea(3, 5)]
        public string description = "A strong melee fighter...";
        public Sprite classIcon;
        public Sprite characterSprite;

        [Header("Player Prefab")]
        public GameObject playerPrefab;

        [Header("Starting Stats")]
        public float healthBonus = 0f;
        public float damageBonus = 0f;
        public float moveSpeedBonus = 0f;
        public float armorBonus = 0f;

        [Header("Stat Modifiers")]
        public List<StatModifier> statModifiers = new List<StatModifier>();

        [Header("Starting Equipment")]
        public WeaponData startingWeapon;
        public List<Item> startingItems = new List<Item>();

        [Header("Starting Traits")]
        public List<Trait> startingTraits = new List<Trait>();

        [Header("Passive Abilities")]
        [TextArea(2, 4)]
        public string passiveDescription = "Class passive ability...";
        public bool hasUniquePassive = false;

        [Header("Unlock Requirements")]
        public bool isUnlocked = true;
        public int masteryLevelRequired = 0;
        [TextArea(2, 3)]
        public string unlockCondition = "";

        /// <summary>
        /// Get total stat bonuses from this class
        /// </summary>
        public float GetStatBonus(StatType statType)
        {
            float bonus = 0f;

            switch (statType)
            {
                case StatType.MaxHealth:
                    bonus += healthBonus;
                    break;
                case StatType.Damage:
                    bonus += damageBonus;
                    break;
                case StatType.MoveSpeed:
                    bonus += moveSpeedBonus;
                    break;
                case StatType.Armor:
                    bonus += armorBonus;
                    break;
            }

            // Add modifiers
            foreach (var modifier in statModifiers)
            {
                if (modifier.statType == statType && !modifier.isMultiplicative)
                {
                    bonus += modifier.value;
                }
            }

            return bonus;
        }
    }
}
