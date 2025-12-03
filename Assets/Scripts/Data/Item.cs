using UnityEngine;
using System.Collections.Generic;

namespace PixelGame
{
    /// <summary>
    /// ScriptableObject for equipment items
    /// </summary>
    [CreateAssetMenu(fileName = "New Item", menuName = "PixelGame/Item")]
    public class Item : ScriptableObject
    {
        [Header("Basic Info")]
        public string itemName = "New Item";
        [TextArea(3, 5)]
        public string description = "Item description...";
        public Sprite icon;
        public ItemType itemType = ItemType.Armor;
        public ItemRarity rarity = ItemRarity.Common;

        [Header("Stats")]
        public List<StatModifier> statModifiers = new List<StatModifier>();

        [Header("Effects")]
        public bool hasOnEquipEffect = false;
        public bool hasOnHitEffect = false;
        public bool hasOnKillEffect = false;
        [TextArea(2, 4)]
        public string effectDescription = "";

        [Header("Requirements")]
        public int levelRequirement = 1;
        public bool isUnlocked = true;

        [Header("Economy")]
        public int goldValue = 100;
        public bool canBeSold = true;

        /// <summary>
        /// Apply item effects when equipped
        /// </summary>
        public virtual void OnEquip(PlayerStats stats)
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

            if (hasOnEquipEffect)
            {
                OnEquipEffect(stats);
            }
        }

        /// <summary>
        /// Remove item effects when unequipped
        /// </summary>
        public virtual void OnUnequip(PlayerStats stats)
        {
            if (stats == null) return;

            foreach (var modifier in statModifiers)
            {
                if (modifier.isMultiplicative)
                {
                    stats.RemoveMultiplierModifier(modifier.statType, modifier.value);
                }
                else
                {
                    stats.RemoveFlatModifier(modifier.statType, modifier.value);
                }
            }
        }

        protected virtual void OnEquipEffect(PlayerStats stats) { }
        public virtual void OnHitEffect() { }
        public virtual void OnKillEffect() { }
    }

    public enum ItemType
    {
        Weapon,
        Armor,
        Helmet,
        Boots,
        Ring,
        Amulet,
        Consumable
    }

    public enum ItemRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }
}
