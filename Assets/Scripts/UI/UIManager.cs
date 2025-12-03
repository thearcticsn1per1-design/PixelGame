using UnityEngine;

namespace PixelGame
{
    /// <summary>
    /// Central UI manager coordinating all UI panels
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("UI Panels")]
        [SerializeField] private HUDManager hudManager;
        [SerializeField] private GameObject pauseMenuPanel;
        [SerializeField] private GameObject traitSelectionPanel;
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private GameObject victoryPanel;

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
            // Subscribe to events
            GameEvents.OnGameStateChanged += HandleGameStateChanged;
            GameEvents.OnShowTraitSelection += ShowTraitSelection;

            // Hide all panels initially
            HideAllPanels();
        }

        private void OnDestroy()
        {
            GameEvents.OnGameStateChanged -= HandleGameStateChanged;
            GameEvents.OnShowTraitSelection -= ShowTraitSelection;
        }

        private void HandleGameStateChanged(GameState previous, GameState current)
        {
            // Hide all panels first
            HideAllPanels();

            // Show appropriate panel
            switch (current)
            {
                case GameState.Playing:
                    if (hudManager != null)
                    {
                        hudManager.gameObject.SetActive(true);
                    }
                    break;

                case GameState.Paused:
                    if (pauseMenuPanel != null)
                    {
                        pauseMenuPanel.SetActive(true);
                    }
                    break;

                case GameState.LevelUp:
                    if (traitSelectionPanel != null)
                    {
                        traitSelectionPanel.SetActive(true);
                    }
                    break;

                case GameState.GameOver:
                    if (gameOverPanel != null)
                    {
                        gameOverPanel.SetActive(true);
                    }
                    break;

                case GameState.Victory:
                    if (victoryPanel != null)
                    {
                        victoryPanel.SetActive(true);
                    }
                    break;
            }
        }

        private void ShowTraitSelection(Trait[] traits)
        {
            if (traitSelectionPanel != null)
            {
                traitSelectionPanel.SetActive(true);
                // Populate trait selection UI with traits
            }
        }

        private void HideAllPanels()
        {
            if (hudManager != null) hudManager.gameObject.SetActive(false);
            if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
            if (traitSelectionPanel != null) traitSelectionPanel.SetActive(false);
            if (gameOverPanel != null) gameOverPanel.SetActive(false);
            if (victoryPanel != null) victoryPanel.SetActive(false);
        }

        public HUDManager GetHUD() => hudManager;
    }
}
