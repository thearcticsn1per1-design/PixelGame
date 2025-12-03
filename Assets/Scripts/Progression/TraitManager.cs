using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace PixelGame
{
    /// <summary>
    /// Manages trait selection and application
    /// </summary>
    public class TraitManager : MonoBehaviour
    {
        public static TraitManager Instance { get; private set; }

        [Header("Trait Database")]
        [SerializeField] private List<Trait> allTraits = new List<Trait>();

        [Header("Selection Settings")]
        [SerializeField] private int traitsToShow = 3;
        [SerializeField] private bool allowDuplicates = false;

        // Player's selected traits this run
        private List<Trait> selectedTraits = new List<Trait>();
        private PlayerStats playerStats;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            // Subscribe to level up event
            GameEvents.OnPlayerLevelUp += HandlePlayerLevelUp;
        }

        private void OnDestroy()
        {
            GameEvents.OnPlayerLevelUp -= HandlePlayerLevelUp;
        }

        /// <summary>
        /// Initialize with player reference
        /// </summary>
        public void Initialize(PlayerStats stats)
        {
            playerStats = stats;
            selectedTraits.Clear();
        }

        private void HandlePlayerLevelUp(int level)
        {
            ShowTraitSelection(traitsToShow);
        }

        /// <summary>
        /// Show trait selection UI with random traits
        /// </summary>
        public void ShowTraitSelection(int count)
        {
            Trait[] traits = SelectRandomTraits(count);
            GameEvents.ShowTraitSelection(traits);
        }

        /// <summary>
        /// Select random traits based on rarity weights
        /// </summary>
        private Trait[] SelectRandomTraits(int count)
        {
            List<Trait> available = new List<Trait>(allTraits);
            List<Trait> selected = new List<Trait>();

            // Remove already selected traits if duplicates not allowed
            if (!allowDuplicates)
            {
                available = available.Where(t => !selectedTraits.Contains(t)).ToList();
            }

            // Select traits
            for (int i = 0; i < count && available.Count > 0; i++)
            {
                Trait trait = SelectWeightedRandom(available);
                selected.Add(trait);
                available.Remove(trait);
            }

            return selected.ToArray();
        }

        /// <summary>
        /// Select trait with rarity-based weighting
        /// </summary>
        private Trait SelectWeightedRandom(List<Trait> traits)
        {
            if (traits.Count == 0) return null;

            // Calculate total weight
            float totalWeight = 0f;
            foreach (var trait in traits)
            {
                totalWeight += GetRarityWeight(trait.rarity);
            }

            // Random selection
            float randomValue = Random.Range(0f, totalWeight);
            float currentWeight = 0f;

            foreach (var trait in traits)
            {
                currentWeight += GetRarityWeight(trait.rarity);
                if (randomValue <= currentWeight)
                {
                    return trait;
                }
            }

            return traits[traits.Count - 1];
        }

        /// <summary>
        /// Get weight for rarity (higher = more common)
        /// </summary>
        private float GetRarityWeight(TraitRarity rarity)
        {
            switch (rarity)
            {
                case TraitRarity.Common: return 50f;
                case TraitRarity.Uncommon: return 25f;
                case TraitRarity.Rare: return 15f;
                case TraitRarity.Epic: return 7f;
                case TraitRarity.Legendary: return 3f;
                default: return 1f;
            }
        }

        /// <summary>
        /// Player selects a trait
        /// </summary>
        public void SelectTrait(Trait trait)
        {
            if (trait == null || playerStats == null) return;

            // Apply trait effects
            trait.Apply(playerStats);

            // Track selection
            selectedTraits.Add(trait);

            Debug.Log($"Selected trait: {trait.traitName}");
        }

        /// <summary>
        /// Get all traits selected this run
        /// </summary>
        public List<Trait> GetSelectedTraits()
        {
            return new List<Trait>(selectedTraits);
        }

        /// <summary>
        /// Clear all traits (for new run)
        /// </summary>
        public void ClearTraits()
        {
            selectedTraits.Clear();
        }
    }
}
