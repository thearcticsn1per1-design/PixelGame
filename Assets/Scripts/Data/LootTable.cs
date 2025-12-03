using UnityEngine;
using System.Collections.Generic;

namespace PixelGame
{
    /// <summary>
    /// ScriptableObject for defining loot drop tables
    /// </summary>
    [CreateAssetMenu(fileName = "New Loot Table", menuName = "PixelGame/Loot Table")]
    public class LootTable : ScriptableObject
    {
        [Header("Loot Settings")]
        [SerializeField] private List<LootEntry> lootEntries = new List<LootEntry>();
        [SerializeField] private int minDrops = 0;
        [SerializeField] private int maxDrops = 1;

        [Header("Gold")]
        [SerializeField] private int minGold = 5;
        [SerializeField] private int maxGold = 15;
        [SerializeField, Range(0f, 1f)] private float goldDropChance = 0.8f;

        [System.Serializable]
        public class LootEntry
        {
            public GameObject dropPrefab;
            [Range(0f, 1f)]
            public float dropChance = 0.5f;
            public int minQuantity = 1;
            public int maxQuantity = 1;
        }

        /// <summary>
        /// Roll loot drops at specified position
        /// </summary>
        public void RollLoot(Vector3 position)
        {
            // Drop gold
            if (Random.value < goldDropChance)
            {
                int goldAmount = Random.Range(minGold, maxGold + 1);
                SpawnGoldDrop(position, goldAmount);
            }

            // Determine number of drops
            int dropCount = Random.Range(minDrops, maxDrops + 1);

            // Roll for each potential drop
            List<LootEntry> availableDrops = new List<LootEntry>(lootEntries);
            int dropsSpawned = 0;

            while (dropsSpawned < dropCount && availableDrops.Count > 0)
            {
                LootEntry entry = availableDrops[Random.Range(0, availableDrops.Count)];

                if (Random.value < entry.dropChance)
                {
                    SpawnLoot(position, entry);
                    dropsSpawned++;
                }

                availableDrops.Remove(entry);
            }
        }

        private void SpawnLoot(Vector3 position, LootEntry entry)
        {
            if (entry.dropPrefab == null) return;

            int quantity = Random.Range(entry.minQuantity, entry.maxQuantity + 1);

            for (int i = 0; i < quantity; i++)
            {
                Vector3 spawnPos = position + (Vector3)Random.insideUnitCircle * 0.5f;
                GameObject drop = Instantiate(entry.dropPrefab, spawnPos, Quaternion.identity);

                // Add a small random force
                Rigidbody2D rb = drop.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    Vector2 force = Random.insideUnitCircle * 2f;
                    rb.AddForce(force, ForceMode2D.Impulse);
                }
            }
        }

        private void SpawnGoldDrop(Vector3 position, int amount)
        {
            // This would spawn a gold pickup prefab
            // For now, directly add to player
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddGold(amount);
            }
        }
    }
}
