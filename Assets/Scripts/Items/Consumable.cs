using UnityEngine;

namespace PixelGame
{
    /// <summary>
    /// Consumable item that can be used once
    /// </summary>
    [CreateAssetMenu(fileName = "New Consumable", menuName = "PixelGame/Consumable")]
    public class Consumable : ScriptableObject
    {
        [Header("Basic Info")]
        public string consumableName = "Health Potion";
        [TextArea(3, 5)]
        public string description = "Restores health";
        public Sprite icon;
        public ConsumableType type = ConsumableType.Health;

        [Header("Effects")]
        public float healAmount = 50f;
        public float manaAmount = 0f;
        public float duration = 0f; // For buffs
        public bool isInstant = true;

        [Header("Visual/Audio")]
        public GameObject useEffectPrefab;
        public string useSoundName = "UsePotion";

        /// <summary>
        /// Use this consumable
        /// </summary>
        public void Use(PlayerController player)
        {
            if (player == null) return;

            PlayerStats stats = player.GetComponent<PlayerStats>();
            if (stats == null) return;

            switch (type)
            {
                case ConsumableType.Health:
                    stats.ModifyHealth(healAmount);
                    break;

                case ConsumableType.Mana:
                    stats.ModifyMana(manaAmount);
                    break;

                case ConsumableType.HealthAndMana:
                    stats.ModifyHealth(healAmount);
                    stats.ModifyMana(manaAmount);
                    break;

                case ConsumableType.Buff:
                    ApplyBuff(stats);
                    break;

                case ConsumableType.FullRestore:
                    stats.HealToFull();
                    stats.ModifyMana(stats.GetStat(StatType.MaxMana));
                    break;
            }

            // Visual/Audio feedback
            if (useEffectPrefab != null)
            {
                GameObject effect = Instantiate(useEffectPrefab, player.transform.position, Quaternion.identity);
                Destroy(effect, 2f);
            }

            GameEvents.PlaySoundEffect(useSoundName, player.transform.position);
            GameEvents.ConsumableUsed(this);
        }

        protected virtual void ApplyBuff(PlayerStats stats)
        {
            // Override for specific buff effects
        }
    }

    public enum ConsumableType
    {
        Health,
        Mana,
        HealthAndMana,
        Buff,
        FullRestore
    }
}
