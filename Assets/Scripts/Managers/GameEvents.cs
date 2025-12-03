using System;
using UnityEngine;

namespace PixelGame
{
    /// <summary>
    /// Central event system for game-wide communication.
    /// Uses C# events for loose coupling between systems.
    /// </summary>
    public static class GameEvents
    {
        #region Player Events

        /// <summary>
        /// Invoked when player health changes
        /// Parameters: currentHealth, maxHealth
        /// </summary>
        public static event Action<float, float> OnPlayerHealthChanged;

        /// <summary>
        /// Invoked when player dies
        /// </summary>
        public static event Action OnPlayerDeath;

        /// <summary>
        /// Invoked when player gains XP
        /// Parameters: xpGained, currentXP, xpToNextLevel
        /// </summary>
        public static event Action<int, int, int> OnPlayerXPGained;

        /// <summary>
        /// Invoked when player levels up
        /// Parameters: newLevel
        /// </summary>
        public static event Action<int> OnPlayerLevelUp;

        /// <summary>
        /// Invoked when player takes damage
        /// Parameters: damage, damageSource
        /// </summary>
        public static event Action<float, GameObject> OnPlayerDamaged;

        #endregion

        #region Combat Events

        /// <summary>
        /// Invoked when any entity takes damage
        /// Parameters: target, damage, isCritical
        /// </summary>
        public static event Action<GameObject, float, bool> OnDamageDealt;

        /// <summary>
        /// Invoked when an enemy is killed
        /// Parameters: enemy
        /// </summary>
        public static event Action<EnemyBase> OnEnemyKilled;

        /// <summary>
        /// Invoked when a boss is defeated
        /// Parameters: boss
        /// </summary>
        public static event Action<BossBase> OnBossDefeated;

        /// <summary>
        /// Invoked when a projectile is spawned
        /// Parameters: projectile, isPlayerProjectile
        /// </summary>
        public static event Action<Projectile, bool> OnProjectileSpawned;

        #endregion

        #region Room/Floor Events

        /// <summary>
        /// Invoked when player enters a room
        /// Parameters: room
        /// </summary>
        public static event Action<Room> OnRoomEntered;

        /// <summary>
        /// Invoked when a room is cleared of all enemies
        /// </summary>
        public static event Action OnRoomCleared;

        /// <summary>
        /// Invoked when player exits a room
        /// Parameters: room
        /// </summary>
        public static event Action<Room> OnRoomExited;

        /// <summary>
        /// Invoked when a floor is completed
        /// Parameters: floorNumber
        /// </summary>
        public static event Action<int> OnFloorCompleted;

        /// <summary>
        /// Invoked when a new floor begins
        /// Parameters: floorNumber
        /// </summary>
        public static event Action<int> OnFloorStarted;

        #endregion

        #region Item Events

        /// <summary>
        /// Invoked when player collects an item
        /// Parameters: item
        /// </summary>
        public static event Action<Item> OnItemCollected;

        /// <summary>
        /// Invoked when player equips an item
        /// Parameters: item, slot
        /// </summary>
        public static event Action<Item, ItemType> OnItemEquipped;

        /// <summary>
        /// Invoked when player unequips an item
        /// Parameters: item, slot
        /// </summary>
        public static event Action<Item, ItemType> OnItemUnequipped;

        /// <summary>
        /// Invoked when player uses a consumable
        /// Parameters: consumable
        /// </summary>
        public static event Action<Consumable> OnConsumableUsed;

        #endregion

        #region Progression Events

        /// <summary>
        /// Invoked when player selects a trait
        /// Parameters: trait
        /// </summary>
        public static event Action<Trait> OnTraitSelected;

        /// <summary>
        /// Invoked when trait selection screen should be shown
        /// Parameters: availableTraits
        /// </summary>
        public static event Action<Trait[]> OnShowTraitSelection;

        /// <summary>
        /// Invoked when mastery level increases
        /// Parameters: newMasteryLevel
        /// </summary>
        public static event Action<int> OnMasteryLevelUp;

        /// <summary>
        /// Invoked when new content is unlocked
        /// Parameters: unlockType, unlockId
        /// </summary>
        public static event Action<UnlockType, string> OnContentUnlocked;

        #endregion

        #region Game State Events

        /// <summary>
        /// Invoked when game state changes
        /// Parameters: previousState, newState
        /// </summary>
        public static event Action<GameState, GameState> OnGameStateChanged;

        /// <summary>
        /// Invoked when a new run starts
        /// Parameters: selectedClass
        /// </summary>
        public static event Action<CharacterClass> OnRunStarted;

        /// <summary>
        /// Invoked when a run ends
        /// Parameters: runStatistics
        /// </summary>
        public static event Action<RunStatistics> OnRunEnded;

        #endregion

        #region Economy Events

        /// <summary>
        /// Invoked when gold amount changes
        /// Parameters: newAmount
        /// </summary>
        public static event Action<int> OnGoldChanged;

        /// <summary>
        /// Invoked when player purchases something
        /// Parameters: itemPurchased, cost
        /// </summary>
        public static event Action<string, int> OnPurchaseMade;

        #endregion

        #region Audio Events

        /// <summary>
        /// Invoked when sound effect should be played
        /// Parameters: soundName, position
        /// </summary>
        public static event Action<string, Vector3> OnPlaySoundEffect;

        /// <summary>
        /// Invoked when music should change
        /// Parameters: musicName
        /// </summary>
        public static event Action<string> OnMusicChanged;

        #endregion

        #region Invoke Methods

        // Player
        public static void PlayerHealthChanged(float currentHealth, float maxHealth)
            => OnPlayerHealthChanged?.Invoke(currentHealth, maxHealth);

        public static void PlayerDeath()
            => OnPlayerDeath?.Invoke();

        public static void PlayerXPGained(int xpGained, int currentXP, int xpToNextLevel)
            => OnPlayerXPGained?.Invoke(xpGained, currentXP, xpToNextLevel);

        public static void PlayerLevelUp(int newLevel)
            => OnPlayerLevelUp?.Invoke(newLevel);

        public static void PlayerDamaged(float damage, GameObject source)
            => OnPlayerDamaged?.Invoke(damage, source);

        // Combat
        public static void DamageDealt(GameObject target, float damage, bool isCritical)
            => OnDamageDealt?.Invoke(target, damage, isCritical);

        public static void EnemyKilled(EnemyBase enemy)
            => OnEnemyKilled?.Invoke(enemy);

        public static void BossDefeated(BossBase boss)
            => OnBossDefeated?.Invoke(boss);

        public static void ProjectileSpawned(Projectile projectile, bool isPlayerProjectile)
            => OnProjectileSpawned?.Invoke(projectile, isPlayerProjectile);

        // Room/Floor
        public static void RoomEntered(Room room)
            => OnRoomEntered?.Invoke(room);

        public static void RoomCleared()
            => OnRoomCleared?.Invoke();

        public static void RoomExited(Room room)
            => OnRoomExited?.Invoke(room);

        public static void FloorCompleted(int floorNumber)
            => OnFloorCompleted?.Invoke(floorNumber);

        public static void FloorStarted(int floorNumber)
            => OnFloorStarted?.Invoke(floorNumber);

        // Items
        public static void ItemCollected(Item item)
            => OnItemCollected?.Invoke(item);

        public static void ItemEquipped(Item item, ItemType slot)
            => OnItemEquipped?.Invoke(item, slot);

        public static void ItemUnequipped(Item item, ItemType slot)
            => OnItemUnequipped?.Invoke(item, slot);

        public static void ConsumableUsed(Consumable consumable)
            => OnConsumableUsed?.Invoke(consumable);

        // Progression
        public static void TraitSelected(Trait trait)
            => OnTraitSelected?.Invoke(trait);

        public static void ShowTraitSelection(Trait[] availableTraits)
            => OnShowTraitSelection?.Invoke(availableTraits);

        public static void MasteryLevelUp(int newLevel)
            => OnMasteryLevelUp?.Invoke(newLevel);

        public static void ContentUnlocked(UnlockType type, string id)
            => OnContentUnlocked?.Invoke(type, id);

        // Game State
        public static void GameStateChanged(GameState previousState, GameState newState)
            => OnGameStateChanged?.Invoke(previousState, newState);

        public static void RunStarted(CharacterClass selectedClass)
            => OnRunStarted?.Invoke(selectedClass);

        public static void RunEnded(RunStatistics stats)
            => OnRunEnded?.Invoke(stats);

        // Economy
        public static void GoldChanged(int newAmount)
            => OnGoldChanged?.Invoke(newAmount);

        public static void PurchaseMade(string itemName, int cost)
            => OnPurchaseMade?.Invoke(itemName, cost);

        // Audio
        public static void PlaySoundEffect(string soundName, Vector3 position = default)
            => OnPlaySoundEffect?.Invoke(soundName, position);

        public static void MusicChanged(string musicName)
            => OnMusicChanged?.Invoke(musicName);

        #endregion

        #region Utility Methods

        /// <summary>
        /// Clear all event subscriptions (useful for cleanup or scene transitions)
        /// </summary>
        public static void ClearAllEvents()
        {
            // Player
            OnPlayerHealthChanged = null;
            OnPlayerDeath = null;
            OnPlayerXPGained = null;
            OnPlayerLevelUp = null;
            OnPlayerDamaged = null;

            // Combat
            OnDamageDealt = null;
            OnEnemyKilled = null;
            OnBossDefeated = null;
            OnProjectileSpawned = null;

            // Room/Floor
            OnRoomEntered = null;
            OnRoomCleared = null;
            OnRoomExited = null;
            OnFloorCompleted = null;
            OnFloorStarted = null;

            // Items
            OnItemCollected = null;
            OnItemEquipped = null;
            OnItemUnequipped = null;
            OnConsumableUsed = null;

            // Progression
            OnTraitSelected = null;
            OnShowTraitSelection = null;
            OnMasteryLevelUp = null;
            OnContentUnlocked = null;

            // Game State
            OnGameStateChanged = null;
            OnRunStarted = null;
            OnRunEnded = null;

            // Economy
            OnGoldChanged = null;
            OnPurchaseMade = null;

            // Audio
            OnPlaySoundEffect = null;
            OnMusicChanged = null;
        }

        #endregion
    }

    /// <summary>
    /// Types of unlockable content
    /// </summary>
    public enum UnlockType
    {
        CharacterClass,
        Weapon,
        Item,
        Trait,
        Floor,
        Boss,
        Perk
    }
}
