using UnityEngine;
using System.IO;
using System.Collections.Generic;

namespace PixelGame
{
    /// <summary>
    /// Manages game saving and loading
    /// </summary>
    public class SaveManager : MonoBehaviour
    {
        [Header("Save Settings")]
        [SerializeField] private string saveFileName = "savegame.json";
        [SerializeField] private bool useEncryption = false;

        public SaveData CurrentSaveData { get; private set; }

        private string SaveFilePath => Path.Combine(Application.persistentDataPath, saveFileName);

        private void Awake()
        {
            CurrentSaveData = new SaveData();
        }

        #region Save/Load

        /// <summary>
        /// Save game data to file
        /// </summary>
        public void SaveGame()
        {
            try
            {
                string json = JsonUtility.ToJson(CurrentSaveData, true);

                if (useEncryption)
                {
                    json = EncryptData(json);
                }

                File.WriteAllText(SaveFilePath, json);

                Debug.Log($"Game saved to: {SaveFilePath}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to save game: {e.Message}");
            }
        }

        /// <summary>
        /// Load game data from file
        /// </summary>
        public void LoadGame()
        {
            try
            {
                if (File.Exists(SaveFilePath))
                {
                    string json = File.ReadAllText(SaveFilePath);

                    if (useEncryption)
                    {
                        json = DecryptData(json);
                    }

                    CurrentSaveData = JsonUtility.FromJson<SaveData>(json);

                    Debug.Log("Game loaded successfully!");
                }
                else
                {
                    Debug.Log("No save file found. Creating new save.");
                    CurrentSaveData = new SaveData();
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to load game: {e.Message}");
                CurrentSaveData = new SaveData();
            }
        }

        /// <summary>
        /// Delete save file
        /// </summary>
        public void DeleteSave()
        {
            if (File.Exists(SaveFilePath))
            {
                File.Delete(SaveFilePath);
                Debug.Log("Save file deleted.");
            }

            CurrentSaveData = new SaveData();
        }

        /// <summary>
        /// Check if save file exists
        /// </summary>
        public bool SaveFileExists()
        {
            return File.Exists(SaveFilePath);
        }

        #endregion

        #region Encryption (Basic)

        private string EncryptData(string data)
        {
            // Basic XOR encryption (not secure, just obfuscation)
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(data);
            byte key = 0x5A; // Simple key

            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = (byte)(bytes[i] ^ key);
            }

            return System.Convert.ToBase64String(bytes);
        }

        private string DecryptData(string data)
        {
            byte[] bytes = System.Convert.FromBase64String(data);
            byte key = 0x5A;

            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = (byte)(bytes[i] ^ key);
            }

            return System.Text.Encoding.UTF8.GetString(bytes);
        }

        #endregion
    }

    /// <summary>
    /// Save data structure containing all persistent game data
    /// </summary>
    [System.Serializable]
    public class SaveData
    {
        [Header("Meta Progression")]
        public int masteryLevel = 1;
        public int masteryXP = 0;
        public int masteryXPToNextLevel = 100;

        [Header("Unlocks")]
        public List<string> unlockedClasses = new List<string>();
        public List<string> unlockedWeapons = new List<string>();
        public List<string> unlockedItems = new List<string>();
        public List<string> unlockedTraits = new List<string>();
        public List<string> unlockedPerks = new List<string>();

        [Header("Statistics")]
        public int totalRunsCompleted = 0;
        public int totalRunsAttempted = 0;
        public int totalEnemiesKilled = 0;
        public int totalBossesDefeated = 0;
        public int totalGoldCollected = 0;
        public float totalPlayTime = 0f;
        public int highestFloorReached = 1;

        [Header("Settings")]
        public float masterVolume = 1f;
        public float musicVolume = 0.8f;
        public float sfxVolume = 1f;

        /// <summary>
        /// Add mastery XP and level up if threshold reached
        /// </summary>
        public void AddMasteryXP(int amount)
        {
            masteryXP += amount;

            while (masteryXP >= masteryXPToNextLevel)
            {
                masteryLevel++;
                masteryXP -= masteryXPToNextLevel;
                masteryXPToNextLevel = CalculateMasteryXPForNextLevel();

                GameEvents.MasteryLevelUp(masteryLevel);
            }
        }

        private int CalculateMasteryXPForNextLevel()
        {
            return Mathf.RoundToInt(100f * Mathf.Pow(1.1f, masteryLevel - 1));
        }

        /// <summary>
        /// Record run statistics
        /// </summary>
        public void RecordRunStatistics(RunStatistics stats)
        {
            totalRunsAttempted++;

            if (stats.isVictory)
            {
                totalRunsCompleted++;
            }

            totalEnemiesKilled += stats.enemiesKilled;
            totalGoldCollected += stats.goldCollected;

            if (stats.floorReached > highestFloorReached)
            {
                highestFloorReached = stats.floorReached;
            }

            // Award mastery XP based on performance
            int xpReward = stats.enemiesKilled * 2 + stats.floorReached * 10;
            if (stats.isVictory)
            {
                xpReward += 100;
            }

            AddMasteryXP(xpReward);
        }

        /// <summary>
        /// Unlock content
        /// </summary>
        public bool UnlockContent(UnlockType type, string id)
        {
            List<string> targetList = null;

            switch (type)
            {
                case UnlockType.CharacterClass:
                    targetList = unlockedClasses;
                    break;
                case UnlockType.Weapon:
                    targetList = unlockedWeapons;
                    break;
                case UnlockType.Item:
                    targetList = unlockedItems;
                    break;
                case UnlockType.Trait:
                    targetList = unlockedTraits;
                    break;
                case UnlockType.Perk:
                    targetList = unlockedPerks;
                    break;
            }

            if (targetList != null && !targetList.Contains(id))
            {
                targetList.Add(id);
                GameEvents.ContentUnlocked(type, id);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Check if content is unlocked
        /// </summary>
        public bool IsUnlocked(UnlockType type, string id)
        {
            switch (type)
            {
                case UnlockType.CharacterClass:
                    return unlockedClasses.Contains(id);
                case UnlockType.Weapon:
                    return unlockedWeapons.Contains(id);
                case UnlockType.Item:
                    return unlockedItems.Contains(id);
                case UnlockType.Trait:
                    return unlockedTraits.Contains(id);
                case UnlockType.Perk:
                    return unlockedPerks.Contains(id);
                default:
                    return false;
            }
        }
    }
}
