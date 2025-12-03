using UnityEngine;
using UnityEngine.SceneManagement;

namespace PixelGame
{
    /// <summary>
    /// Central game manager that controls game state and coordinates between systems.
    /// Singleton pattern ensures only one instance exists.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Game State")]
        [SerializeField] private GameState currentState = GameState.MainMenu;

        [Header("Game Settings")]
        [SerializeField] private int startingFloor = 1;
        [SerializeField] private float gameTimeScale = 1f;

        // System References
        public PlayerController Player { get; private set; }
        public FloorManager FloorManager { get; private set; }
        public UIManager UIManager { get; private set; }
        public AudioManager AudioManager { get; private set; }
        public SaveManager SaveManager { get; private set; }

        // Game Statistics
        public int CurrentRun { get; private set; }
        public float RunTime { get; private set; }
        public int EnemiesKilledThisRun { get; private set; }
        public int GoldCollectedThisRun { get; private set; }

        private void Awake()
        {
            // Singleton pattern
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeManagers();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            SubscribeToEvents();
        }

        private void Update()
        {
            if (currentState == GameState.Playing)
            {
                RunTime += Time.deltaTime;
            }
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        private void InitializeManagers()
        {
            FloorManager = GetComponent<FloorManager>();
            AudioManager = GetComponent<AudioManager>();
            SaveManager = GetComponent<SaveManager>();

            if (SaveManager != null)
            {
                SaveManager.LoadGame();
            }
        }

        private void SubscribeToEvents()
        {
            GameEvents.OnEnemyKilled += HandleEnemyKilled;
            GameEvents.OnPlayerDeath += HandlePlayerDeath;
            GameEvents.OnFloorCompleted += HandleFloorCompleted;
        }

        private void UnsubscribeFromEvents()
        {
            GameEvents.OnEnemyKilled -= HandleEnemyKilled;
            GameEvents.OnPlayerDeath -= HandlePlayerDeath;
            GameEvents.OnFloorCompleted -= HandleFloorCompleted;
        }

        #region Game State Management

        public void ChangeState(GameState newState)
        {
            if (currentState == newState) return;

            GameState previousState = currentState;
            currentState = newState;

            OnStateChanged(previousState, newState);
            GameEvents.GameStateChanged(previousState, newState);
        }

        private void OnStateChanged(GameState previous, GameState current)
        {
            switch (current)
            {
                case GameState.MainMenu:
                    Time.timeScale = 1f;
                    break;

                case GameState.Playing:
                    Time.timeScale = gameTimeScale;
                    break;

                case GameState.Paused:
                    Time.timeScale = 0f;
                    break;

                case GameState.GameOver:
                    Time.timeScale = 0f;
                    HandleGameOver();
                    break;

                case GameState.Victory:
                    Time.timeScale = 0f;
                    HandleVictory();
                    break;
            }
        }

        public void PauseGame()
        {
            if (currentState == GameState.Playing)
            {
                ChangeState(GameState.Paused);
            }
        }

        public void ResumeGame()
        {
            if (currentState == GameState.Paused)
            {
                ChangeState(GameState.Playing);
            }
        }

        public void TogglePause()
        {
            if (currentState == GameState.Playing)
                PauseGame();
            else if (currentState == GameState.Paused)
                ResumeGame();
        }

        #endregion

        #region Game Flow

        public void StartNewRun(CharacterClass selectedClass)
        {
            CurrentRun++;
            RunTime = 0f;
            EnemiesKilledThisRun = 0;
            GoldCollectedThisRun = 0;

            // Load game scene if not already loaded
            if (SceneManager.GetActiveScene().name != "GameScene")
            {
                SceneManager.LoadScene("GameScene");
            }

            // Initialize player with selected class
            SpawnPlayer(selectedClass);

            // Generate first floor
            if (FloorManager != null)
            {
                FloorManager.GenerateFloor(startingFloor);
            }

            ChangeState(GameState.Playing);
            GameEvents.RunStarted(selectedClass);
        }

        private void SpawnPlayer(CharacterClass characterClass)
        {
            // Find spawn point or use default position
            Vector3 spawnPosition = Vector3.zero;
            GameObject spawnPoint = GameObject.FindGameObjectWithTag("PlayerSpawn");
            if (spawnPoint != null)
            {
                spawnPosition = spawnPoint.transform.position;
            }

            // Instantiate player prefab
            GameObject playerPrefab = characterClass.playerPrefab;
            GameObject playerObj = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
            Player = playerObj.GetComponent<PlayerController>();

            // Initialize player with class data
            Player.Initialize(characterClass);
        }

        public void RestartRun()
        {
            // Clean up current run
            if (Player != null)
            {
                Destroy(Player.gameObject);
            }

            if (FloorManager != null)
            {
                FloorManager.CleanupFloor();
            }

            // Return to main menu or character select
            SceneManager.LoadScene("MainMenu");
            ChangeState(GameState.MainMenu);
        }

        public void ReturnToMainMenu()
        {
            if (SaveManager != null)
            {
                SaveManager.SaveGame();
            }

            SceneManager.LoadScene("MainMenu");
            ChangeState(GameState.MainMenu);
        }

        public void QuitGame()
        {
            if (SaveManager != null)
            {
                SaveManager.SaveGame();
            }

            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        }

        #endregion

        #region Event Handlers

        private void HandleEnemyKilled(EnemyBase enemy)
        {
            EnemiesKilledThisRun++;
        }

        private void HandlePlayerDeath()
        {
            ChangeState(GameState.GameOver);
        }

        private void HandleFloorCompleted(int floorNumber)
        {
            // Award meta-progression XP
            if (SaveManager != null)
            {
                SaveManager.CurrentSaveData.AddMasteryXP(50 * floorNumber);
            }
        }

        private void HandleGameOver()
        {
            // Calculate run statistics
            RunStatistics stats = new RunStatistics
            {
                runNumber = CurrentRun,
                runTime = RunTime,
                enemiesKilled = EnemiesKilledThisRun,
                goldCollected = GoldCollectedThisRun,
                floorReached = FloorManager != null ? FloorManager.CurrentFloor : 1
            };

            // Save statistics
            if (SaveManager != null)
            {
                SaveManager.CurrentSaveData.RecordRunStatistics(stats);
                SaveManager.SaveGame();
            }

            GameEvents.RunEnded(stats);
        }

        private void HandleVictory()
        {
            // Similar to game over but with victory bonuses
            RunStatistics stats = new RunStatistics
            {
                runNumber = CurrentRun,
                runTime = RunTime,
                enemiesKilled = EnemiesKilledThisRun,
                goldCollected = GoldCollectedThisRun,
                floorReached = FloorManager != null ? FloorManager.CurrentFloor : 1,
                isVictory = true
            };

            if (SaveManager != null)
            {
                SaveManager.CurrentSaveData.RecordRunStatistics(stats);
                SaveManager.CurrentSaveData.AddMasteryXP(500); // Victory bonus
                SaveManager.SaveGame();
            }

            GameEvents.RunEnded(stats);
        }

        #endregion

        #region Helper Methods

        public void AddGold(int amount)
        {
            GoldCollectedThisRun += amount;
            GameEvents.GoldChanged(GoldCollectedThisRun);
        }

        public bool SpendGold(int amount)
        {
            if (GoldCollectedThisRun >= amount)
            {
                GoldCollectedThisRun -= amount;
                GameEvents.GoldChanged(GoldCollectedThisRun);
                return true;
            }
            return false;
        }

        #endregion
    }

    /// <summary>
    /// Possible game states
    /// </summary>
    public enum GameState
    {
        MainMenu,
        CharacterSelect,
        Playing,
        Paused,
        LevelUp,
        Shop,
        GameOver,
        Victory
    }

    /// <summary>
    /// Statistics for a completed run
    /// </summary>
    [System.Serializable]
    public class RunStatistics
    {
        public int runNumber;
        public float runTime;
        public int enemiesKilled;
        public int goldCollected;
        public int floorReached;
        public bool isVictory;
    }
}
