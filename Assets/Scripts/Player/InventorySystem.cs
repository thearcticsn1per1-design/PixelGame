using UnityEngine;
using System.Collections.Generic;

namespace PixelGame
{
    /// <summary>
    /// Manages player inventory and equipment
    /// </summary>
    public class InventorySystem : MonoBehaviour
    {
        [Header("Equipment Slots")]
        [SerializeField] private Dictionary<ItemType, Item> equippedItems = new Dictionary<ItemType, Item>();

        [Header("Consumables")]
        [SerializeField] private List<Consumable> consumables = new List<Consumable>();
        [SerializeField] private int maxConsumableSlots = 5;

        private PlayerStats stats;

        private void Awake()
        {
            stats = GetComponent<PlayerStats>();
        }

        #region Equipment Management

        /// <summary>
        /// Equip an item to its designated slot
        /// </summary>
        public bool EquipItem(Item item)
        {
            if (item == null) return false;

            // Unequip current item in slot
            if (equippedItems.ContainsKey(item.itemType))
            {
                UnequipItem(item.itemType);
            }

            // Equip new item
            equippedItems[item.itemType] = item;
            item.OnEquip(stats);

            GameEvents.ItemEquipped(item, item.itemType);

            return true;
        }

        /// <summary>
        /// Unequip item from a slot
        /// </summary>
        public bool UnequipItem(ItemType slot)
        {
            if (!equippedItems.ContainsKey(slot)) return false;

            Item item = equippedItems[slot];
            item.OnUnequip(stats);

            equippedItems.Remove(slot);

            GameEvents.ItemUnequipped(item, slot);

            return true;
        }

        /// <summary>
        /// Get equipped item in a slot
        /// </summary>
        public Item GetEquippedItem(ItemType slot)
        {
            return equippedItems.TryGetValue(slot, out Item item) ? item : null;
        }

        /// <summary>
        /// Check if slot has an item
        /// </summary>
        public bool HasItemInSlot(ItemType slot)
        {
            return equippedItems.ContainsKey(slot);
        }

        #endregion

        #region Consumables Management

        /// <summary>
        /// Add consumable to inventory
        /// </summary>
        public bool AddConsumable(Consumable consumable)
        {
            if (consumable == null) return false;

            if (consumables.Count >= maxConsumableSlots)
            {
                Debug.Log("Consumable inventory is full!");
                return false;
            }

            consumables.Add(consumable);
            return true;
        }

        /// <summary>
        /// Use consumable at index
        /// </summary>
        public bool UseConsumable(int index)
        {
            if (index < 0 || index >= consumables.Count) return false;

            Consumable consumable = consumables[index];
            PlayerController player = GetComponent<PlayerController>();

            if (consumable != null && player != null)
            {
                consumable.Use(player);
                consumables.RemoveAt(index);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Get consumable at index
        /// </summary>
        public Consumable GetConsumable(int index)
        {
            if (index >= 0 && index < consumables.Count)
            {
                return consumables[index];
            }
            return null;
        }

        /// <summary>
        /// Get count of consumables
        /// </summary>
        public int GetConsumableCount()
        {
            return consumables.Count;
        }

        #endregion

        #region Utility

        /// <summary>
        /// Get all equipped items
        /// </summary>
        public Dictionary<ItemType, Item> GetAllEquippedItems()
        {
            return new Dictionary<ItemType, Item>(equippedItems);
        }

        /// <summary>
        /// Clear all equipment
        /// </summary>
        public void ClearAllEquipment()
        {
            List<ItemType> slots = new List<ItemType>(equippedItems.Keys);

            foreach (var slot in slots)
            {
                UnequipItem(slot);
            }
        }

        #endregion
    }
}
