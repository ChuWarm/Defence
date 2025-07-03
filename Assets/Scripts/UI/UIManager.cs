using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

namespace DefenceGame.Core
{
    /// <summary>
    /// 게임의 모든 UI 요소를 관리하는 매니저
    /// 게임 UI, 타워 UI, 메뉴 UI 등을 통합 관리
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        #region UI Panel References
        [Header("UI Panels")]
        [SerializeField] private GameObject gameUIPanel;
        [SerializeField] private GameObject towerUIPanel;
        [SerializeField] private GameObject menuUIPanel;
        [SerializeField] private GameObject pauseUIPanel;
        [SerializeField] private GameObject gameOverUIPanel;
        [SerializeField] private GameObject victoryUIPanel;
        [SerializeField] private GameObject loadingUIPanel;
        #endregion

        #region Game UI Elements
        [Header("Game UI Elements")]
        [SerializeField] private TextMeshProUGUI goldText;
        [SerializeField] private TextMeshProUGUI waveText;
        [SerializeField] private TextMeshProUGUI healthText;
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI preparationTimeText;
        [SerializeField] private Slider healthSlider;
        [SerializeField] private Button pauseButton;
        [SerializeField] private Button towerButton;
        #endregion

        #region Tower UI Elements
        [Header("Tower UI Elements")]
        [SerializeField] private Button basicTowerButton;
        [SerializeField] private Button sniperTowerButton;
        [SerializeField] private Button areaTowerButton;
        [SerializeField] private Button slowTowerButton;
        [SerializeField] private TextMeshProUGUI basicTowerCostText;
        [SerializeField] private TextMeshProUGUI sniperTowerCostText;
        [SerializeField] private TextMeshProUGUI areaTowerCostText;
        [SerializeField] private TextMeshProUGUI slowTowerCostText;
        [SerializeField] private GameObject towerInfoPanel;
        [SerializeField] private TextMeshProUGUI selectedTowerNameText;
        [SerializeField] private TextMeshProUGUI selectedTowerLevelText;
        [SerializeField] private Button upgradeTowerButton;
        [SerializeField] private Button sellTowerButton;
        [SerializeField] private TextMeshProUGUI upgradeCostText;
        [SerializeField] private TextMeshProUGUI sellValueText;
        #endregion

        #region Menu UI Elements
        [Header("Menu UI Elements")]
        [SerializeField] private Button startGameButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button returnToMenuButton;
        [SerializeField] private Button restartButton;
        #endregion

        #region Events
        [Header("UI Events")]
        public UnityEvent OnStartGameClicked;
        public UnityEvent OnPauseGameClicked;
        public UnityEvent OnResumeGameClicked;
        public UnityEvent OnReturnToMenuClicked;
        public UnityEvent OnRestartGameClicked;
        public UnityEvent OnQuitGameClicked;
        public UnityEvent<DefenceGame.Tower.TowerType> OnTowerButtonClicked;
        public UnityEvent OnUpgradeTowerClicked;
        public UnityEvent OnSellTowerClicked;
        #endregion

        #region Private Fields
        private DefenceGame.Tower.Tower selectedTower;
        private bool isInitialized = false;
        private Canvas mainCanvas;
        private CanvasScaler canvasScaler;
        #endregion

        #region Properties
        public bool IsGameUIVisible => gameUIPanel != null && gameUIPanel.activeSelf;
        public bool IsTowerUIVisible => towerUIPanel != null && towerUIPanel.activeSelf;
        public bool IsMenuUIVisible => menuUIPanel != null && menuUIPanel.activeSelf;
        public bool IsPauseUIVisible => pauseUIPanel != null && pauseUIPanel.activeSelf;
        #endregion

        #region Initialization
        private void Awake()
        {
            InitializeUIManager();
        }
        
        private void InitializeUIManager()
        {
            if (isInitialized) return;
            
            Debug.Log("UIManager 초기화 시작");
            
            // Canvas 컴포넌트 찾기
            mainCanvas = GetComponent<Canvas>();
            canvasScaler = GetComponent<CanvasScaler>();
            
            // UI 요소들 자동 찾기
            FindUIElements();
            
            // 이벤트 리스너 등록
            RegisterEventListeners();
            
            // 초기 UI 상태 설정
            SetInitialUIState();
            
            isInitialized = true;
            
            Debug.Log("UIManager 초기화 완료");
        }
        
        private void FindUIElements()
        {
            // UI 패널들 찾기
            if (gameUIPanel == null)
                gameUIPanel = transform.Find("GameUIPanel")?.gameObject;
            if (towerUIPanel == null)
                towerUIPanel = transform.Find("TowerUIPanel")?.gameObject;
            if (menuUIPanel == null)
                menuUIPanel = transform.Find("MenuUIPanel")?.gameObject;
            if (pauseUIPanel == null)
                pauseUIPanel = transform.Find("PauseUIPanel")?.gameObject;
            if (gameOverUIPanel == null)
                gameOverUIPanel = transform.Find("GameOverUIPanel")?.gameObject;
            if (victoryUIPanel == null)
                victoryUIPanel = transform.Find("VictoryUIPanel")?.gameObject;
            if (loadingUIPanel == null)
                loadingUIPanel = transform.Find("LoadingUIPanel")?.gameObject;
            
            // 게임 UI 요소들 찾기
            if (goldText == null)
                goldText = gameUIPanel?.transform.Find("GoldText")?.GetComponent<TextMeshProUGUI>();
            if (waveText == null)
                waveText = gameUIPanel?.transform.Find("WaveText")?.GetComponent<TextMeshProUGUI>();
            if (healthText == null)
                healthText = gameUIPanel?.transform.Find("HealthText")?.GetComponent<TextMeshProUGUI>();
            if (scoreText == null)
                scoreText = gameUIPanel?.transform.Find("ScoreText")?.GetComponent<TextMeshProUGUI>();
            if (preparationTimeText == null)
                preparationTimeText = gameUIPanel?.transform.Find("PreparationTimeText")?.GetComponent<TextMeshProUGUI>();
            if (healthSlider == null)
                healthSlider = gameUIPanel?.transform.Find("HealthSlider")?.GetComponent<Slider>();
            if (pauseButton == null)
                pauseButton = gameUIPanel?.transform.Find("PauseButton")?.GetComponent<Button>();
            if (towerButton == null)
                towerButton = gameUIPanel?.transform.Find("TowerButton")?.GetComponent<Button>();
            
            // 타워 UI 요소들 찾기
            if (basicTowerButton == null)
                basicTowerButton = towerUIPanel?.transform.Find("BasicTowerButton")?.GetComponent<Button>();
            if (sniperTowerButton == null)
                sniperTowerButton = towerUIPanel?.transform.Find("SniperTowerButton")?.GetComponent<Button>();
            if (areaTowerButton == null)
                areaTowerButton = towerUIPanel?.transform.Find("AreaTowerButton")?.GetComponent<Button>();
            if (slowTowerButton == null)
                slowTowerButton = towerUIPanel?.transform.Find("SlowTowerButton")?.GetComponent<Button>();
            
            // 메뉴 UI 요소들 찾기
            if (startGameButton == null)
                startGameButton = menuUIPanel?.transform.Find("StartGameButton")?.GetComponent<Button>();
            if (settingsButton == null)
                settingsButton = menuUIPanel?.transform.Find("SettingsButton")?.GetComponent<Button>();
            if (quitButton == null)
                quitButton = menuUIPanel?.transform.Find("QuitButton")?.GetComponent<Button>();
            if (resumeButton == null)
                resumeButton = pauseUIPanel?.transform.Find("ResumeButton")?.GetComponent<Button>();
            if (returnToMenuButton == null)
                returnToMenuButton = pauseUIPanel?.transform.Find("ReturnToMenuButton")?.GetComponent<Button>();
            if (restartButton == null)
                restartButton = gameOverUIPanel?.transform.Find("RestartButton")?.GetComponent<Button>();
        }
        
        private void RegisterEventListeners()
        {
            // 게임 매니저 이벤트 구독
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStateChanged += OnGameStateChanged;
                GameManager.Instance.OnPlayerHealthChanged += OnPlayerHealthChanged;
                GameManager.Instance.OnWaveChanged += OnWaveChanged;
                GameManager.Instance.OnScoreChanged += OnScoreChanged;
                GameManager.Instance.OnGameOver += OnGameOver;
                GameManager.Instance.OnVictory += OnVictory;
                GameManager.Instance.OnGameStarted += OnGameStarted;
                GameManager.Instance.OnGamePaused += OnGamePaused;
                GameManager.Instance.OnGameResumed += OnGameResumed;
            }
            
            // 리소스 매니저 이벤트 구독
            if (GameManager.Instance?.ResourceManager != null)
            {
                GameManager.Instance.ResourceManager.OnGoldChanged += OnGoldChanged;
            }
            
            // 웨이브 매니저 이벤트 구독
            if (GameManager.Instance?.WaveManager != null)
            {
                GameManager.Instance.WaveManager.OnPreparationTimeChanged += OnPreparationTimeChanged;
            }
            
            // 입력 매니저 이벤트 구독
            if (GameManager.Instance?.InputManager != null)
            {
                GameManager.Instance.InputManager.OnTowerSelected += OnTowerSelected;
                GameManager.Instance.InputManager.OnTowerDeselected += OnTowerDeselected;
            }
            
            // 버튼 이벤트 등록
            RegisterButtonEvents();
        }
        
        private void RegisterButtonEvents()
        {
            // 게임 UI 버튼들
            if (pauseButton != null)
                pauseButton.onClick.AddListener(OnPauseButtonClicked);
            if (towerButton != null)
                towerButton.onClick.AddListener(OnTowerButtonClicked);
            
            // 타워 UI 버튼들
            if (basicTowerButton != null)
                basicTowerButton.onClick.AddListener(() => OnTowerTypeButtonClicked(DefenceGame.Tower.TowerType.Basic));
            if (sniperTowerButton != null)
                sniperTowerButton.onClick.AddListener(() => OnTowerTypeButtonClicked(DefenceGame.Tower.TowerType.Sniper));
            if (areaTowerButton != null)
                areaTowerButton.onClick.AddListener(() => OnTowerTypeButtonClicked(DefenceGame.Tower.TowerType.Area));
            if (slowTowerButton != null)
                slowTowerButton.onClick.AddListener(() => OnTowerTypeButtonClicked(DefenceGame.Tower.TowerType.Slow));
            if (upgradeTowerButton != null)
                upgradeTowerButton.onClick.AddListener(OnUpgradeTowerButtonClicked);
            if (sellTowerButton != null)
                sellTowerButton.onClick.AddListener(OnSellTowerButtonClicked);
            
            // 메뉴 UI 버튼들
            if (startGameButton != null)
                startGameButton.onClick.AddListener(OnStartGameButtonClicked);
            if (resumeButton != null)
                resumeButton.onClick.AddListener(OnResumeButtonClicked);
            if (returnToMenuButton != null)
                returnToMenuButton.onClick.AddListener(OnReturnToMenuButtonClicked);
            if (restartButton != null)
                restartButton.onClick.AddListener(OnRestartButtonClicked);
            if (quitButton != null)
                quitButton.onClick.AddListener(OnQuitButtonClicked);
        }
        
        private void SetInitialUIState()
        {
            // 모든 UI 패널 비활성화
            SetAllUIPanelsActive(false);
            
            // 메인 메뉴만 활성화
            ShowMenuUI();
        }
        #endregion

        #region UI Panel Management
        /// <summary>
        /// 모든 UI 패널 비활성화
        /// </summary>
        private void SetAllUIPanelsActive(bool active)
        {
            if (gameUIPanel != null) gameUIPanel.SetActive(active);
            if (towerUIPanel != null) towerUIPanel.SetActive(active);
            if (menuUIPanel != null) menuUIPanel.SetActive(active);
            if (pauseUIPanel != null) pauseUIPanel.SetActive(active);
            if (gameOverUIPanel != null) gameOverUIPanel.SetActive(active);
            if (victoryUIPanel != null) victoryUIPanel.SetActive(active);
            if (loadingUIPanel != null) loadingUIPanel.SetActive(active);
        }
        
        /// <summary>
        /// 게임 UI 표시
        /// </summary>
        public void ShowGameUI()
        {
            SetAllUIPanelsActive(false);
            if (gameUIPanel != null) gameUIPanel.SetActive(true);
            UpdateGameUI();
        }
        
        /// <summary>
        /// 타워 UI 표시
        /// </summary>
        public void ShowTowerUI()
        {
            if (towerUIPanel != null) towerUIPanel.SetActive(true);
            UpdateTowerUI();
        }
        
        /// <summary>
        /// 타워 UI 숨기기
        /// </summary>
        public void HideTowerUI()
        {
            if (towerUIPanel != null) towerUIPanel.SetActive(false);
        }
        
        /// <summary>
        /// 메뉴 UI 표시
        /// </summary>
        public void ShowMenuUI()
        {
            SetAllUIPanelsActive(false);
            if (menuUIPanel != null) menuUIPanel.SetActive(true);
        }
        
        /// <summary>
        /// 일시정지 UI 표시
        /// </summary>
        public void ShowPauseUI()
        {
            if (pauseUIPanel != null) pauseUIPanel.SetActive(true);
        }
        
        /// <summary>
        /// 일시정지 UI 숨기기
        /// </summary>
        public void HidePauseUI()
        {
            if (pauseUIPanel != null) pauseUIPanel.SetActive(false);
        }
        
        /// <summary>
        /// 게임 오버 UI 표시
        /// </summary>
        public void ShowGameOverUI()
        {
            SetAllUIPanelsActive(false);
            if (gameOverUIPanel != null) gameOverUIPanel.SetActive(true);
        }
        
        /// <summary>
        /// 승리 UI 표시
        /// </summary>
        public void ShowVictoryUI()
        {
            SetAllUIPanelsActive(false);
            if (victoryUIPanel != null) victoryUIPanel.SetActive(true);
        }
        
        /// <summary>
        /// 로딩 UI 표시
        /// </summary>
        public void ShowLoadingUI()
        {
            SetAllUIPanelsActive(false);
            if (loadingUIPanel != null) loadingUIPanel.SetActive(true);
        }
        #endregion

        #region UI Update Methods
        /// <summary>
        /// 게임 UI 업데이트
        /// </summary>
        public void UpdateGameUI()
        {
            if (GameManager.Instance == null) return;
            
            // 골드 업데이트
            if (goldText != null)
                goldText.text = $"Gold: {GameManager.Instance.ResourceManager?.Gold ?? 0}";
            
            // 웨이브 업데이트
            if (waveText != null)
                waveText.text = $"Wave: {GameManager.Instance.CurrentWave}";
            
            // 체력 업데이트
            if (healthText != null)
                healthText.text = $"Health: {GameManager.Instance.PlayerHealth}/{GameManager.Instance.MaxPlayerHealth}";
            
            if (healthSlider != null)
            {
                healthSlider.maxValue = GameManager.Instance.MaxPlayerHealth;
                healthSlider.value = GameManager.Instance.PlayerHealth;
            }
            
            // 점수 업데이트
            if (scoreText != null)
                scoreText.text = $"Score: {GameManager.Instance.TotalScore}";
        }
        
        /// <summary>
        /// 타워 UI 업데이트
        /// </summary>
        public void UpdateTowerUI()
        {
            if (GameManager.Instance?.ResourceManager == null) return;
            
            var resourceManager = GameManager.Instance.ResourceManager;
            
            // 타워 비용 업데이트
            if (basicTowerCostText != null)
                basicTowerCostText.text = $"${resourceManager.GetTowerCost(DefenceGame.Tower.TowerType.Basic)}";
            if (sniperTowerCostText != null)
                sniperTowerCostText.text = $"${resourceManager.GetTowerCost(DefenceGame.Tower.TowerType.Sniper)}";
            if (areaTowerCostText != null)
                areaTowerCostText.text = $"${resourceManager.GetTowerCost(DefenceGame.Tower.TowerType.Area)}";
            if (slowTowerCostText != null)
                slowTowerCostText.text = $"${resourceManager.GetTowerCost(DefenceGame.Tower.TowerType.Slow)}";
            
            // 타워 버튼 활성화/비활성화
            UpdateTowerButtonStates();
            
            // 선택된 타워 정보 업데이트
            UpdateSelectedTowerInfo();
        }
        
        /// <summary>
        /// 타워 버튼 상태 업데이트
        /// </summary>
        private void UpdateTowerButtonStates()
        {
            if (GameManager.Instance?.ResourceManager == null) return;
            
            var resourceManager = GameManager.Instance.ResourceManager;
            
            if (basicTowerButton != null)
                basicTowerButton.interactable = resourceManager.CanAffordTower(DefenceGame.Tower.TowerType.Basic);
            if (sniperTowerButton != null)
                sniperTowerButton.interactable = resourceManager.CanAffordTower(DefenceGame.Tower.TowerType.Sniper);
            if (areaTowerButton != null)
                areaTowerButton.interactable = resourceManager.CanAffordTower(DefenceGame.Tower.TowerType.Area);
            if (slowTowerButton != null)
                slowTowerButton.interactable = resourceManager.CanAffordTower(DefenceGame.Tower.TowerType.Slow);
        }
        
        /// <summary>
        /// 선택된 타워 정보 업데이트
        /// </summary>
        private void UpdateSelectedTowerInfo()
        {
            if (selectedTower == null)
            {
                if (towerInfoPanel != null)
                    towerInfoPanel.SetActive(false);
                return;
            }
            
            if (towerInfoPanel != null)
                towerInfoPanel.SetActive(true);
            
            if (selectedTowerNameText != null)
                selectedTowerNameText.text = $"Tower: {selectedTower.TowerType}";
            
            if (selectedTowerLevelText != null)
                selectedTowerLevelText.text = $"Level: {selectedTower.Level}";
            
            if (GameManager.Instance?.ResourceManager != null)
            {
                var resourceManager = GameManager.Instance.ResourceManager;
                int upgradeCost = resourceManager.GetTowerUpgradeCost(selectedTower.TowerType, selectedTower.Level);
                
                if (upgradeCostText != null)
                {
                    if (upgradeCost == -1)
                        upgradeCostText.text = "Max Level";
                    else
                        upgradeCostText.text = $"Upgrade: ${upgradeCost}";
                }
                
                if (upgradeTowerButton != null)
                    upgradeTowerButton.interactable = upgradeCost != -1 && resourceManager.CanAfford(upgradeCost);
                
                if (sellValueText != null)
                    sellValueText.text = $"Sell: ${selectedTower.SellValue}";
            }
        }
        #endregion

        #region Event Handlers
        private void OnGameStateChanged(GameManager.GameState previousState, GameManager.GameState newState)
        {
            switch (newState)
            {
                case GameManager.GameState.Menu:
                    ShowMenuUI();
                    break;
                case GameManager.GameState.Playing:
                    ShowGameUI();
                    break;
                case GameManager.GameState.Paused:
                    ShowPauseUI();
                    break;
                case GameManager.GameState.GameOver:
                    ShowGameOverUI();
                    break;
                case GameManager.GameState.Victory:
                    ShowVictoryUI();
                    break;
            }
        }
        
        private void OnPlayerHealthChanged(int currentHealth, int maxHealth)
        {
            UpdateGameUI();
        }
        
        private void OnWaveChanged(int waveNumber)
        {
            UpdateGameUI();
        }
        
        private void OnScoreChanged(int score)
        {
            UpdateGameUI();
        }
        
        private void OnGoldChanged(int gold)
        {
            UpdateGameUI();
            UpdateTowerUI();
        }
        
        private void OnPreparationTimeChanged(float remainingTime)
        {
            if (preparationTimeText != null)
            {
                if (remainingTime > 0)
                {
                    preparationTimeText.text = $"Preparation: {remainingTime:F1}s";
                    preparationTimeText.gameObject.SetActive(true);
                }
                else
                {
                    preparationTimeText.gameObject.SetActive(false);
                }
            }
        }
        
        private void OnTowerSelected(DefenceGame.Tower.Tower tower)
        {
            selectedTower = tower;
            UpdateSelectedTowerInfo();
        }
        
        private void OnTowerDeselected(DefenceGame.Tower.Tower tower)
        {
            selectedTower = null;
            UpdateSelectedTowerInfo();
        }
        
        private void OnGameOver()
        {
            ShowGameOverUI();
        }
        
        private void OnVictory()
        {
            ShowVictoryUI();
        }
        
        private void OnGameStarted()
        {
            ShowGameUI();
        }
        
        private void OnGamePaused()
        {
            ShowPauseUI();
        }
        
        private void OnGameResumed()
        {
            HidePauseUI();
        }
        #endregion

        #region Button Event Handlers
        private void OnStartGameButtonClicked()
        {
            OnStartGameClicked?.Invoke();
            if (GameManager.Instance != null)
                GameManager.Instance.StartGame();
        }
        
        private void OnPauseButtonClicked()
        {
            OnPauseGameClicked?.Invoke();
            if (GameManager.Instance != null)
                GameManager.Instance.PauseGame();
        }
        
        private void OnResumeButtonClicked()
        {
            OnResumeGameClicked?.Invoke();
            if (GameManager.Instance != null)
                GameManager.Instance.ResumeGame();
        }
        
        private void OnReturnToMenuButtonClicked()
        {
            OnReturnToMenuClicked?.Invoke();
            if (GameManager.Instance != null)
                GameManager.Instance.ReturnToMenu();
        }
        
        private void OnRestartButtonClicked()
        {
            OnRestartGameClicked?.Invoke();
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ReturnToMenu();
                GameManager.Instance.StartGame();
            }
        }
        
        private void OnQuitButtonClicked()
        {
            OnQuitGameClicked?.Invoke();
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        }
        
        private void OnTowerButtonClicked()
        {
            ShowTowerUI();
        }
        
        private void OnTowerTypeButtonClicked(DefenceGame.Tower.TowerType towerType)
        {
            OnTowerButtonClicked?.Invoke(towerType);
            if (GameManager.Instance?.InputManager != null)
                GameManager.Instance.InputManager.StartTowerPlacement(towerType);
            HideTowerUI();
        }
        
        private void OnUpgradeTowerButtonClicked()
        {
            OnUpgradeTowerClicked?.Invoke();
            if (selectedTower != null && GameManager.Instance?.ResourceManager != null)
            {
                var resourceManager = GameManager.Instance.ResourceManager;
                if (resourceManager.UpgradeTower(selectedTower.TowerType, selectedTower.Level))
                {
                    selectedTower.Upgrade();
                    UpdateSelectedTowerInfo();
                }
            }
        }
        
        private void OnSellTowerButtonClicked()
        {
            OnSellTowerClicked?.Invoke();
            if (selectedTower != null && GameManager.Instance?.ResourceManager != null)
            {
                var resourceManager = GameManager.Instance.ResourceManager;
                resourceManager.AddGold(selectedTower.SellValue);
                Destroy(selectedTower.gameObject);
                selectedTower = null;
                UpdateSelectedTowerInfo();
            }
        }
        #endregion

        #region Debug Methods
        #if UNITY_EDITOR
        [ContextMenu("Show Game UI")]
        private void DebugShowGameUI()
        {
            ShowGameUI();
        }
        
        [ContextMenu("Show Tower UI")]
        private void DebugShowTowerUI()
        {
            ShowTowerUI();
        }
        
        [ContextMenu("Update All UI")]
        private void DebugUpdateAllUI()
        {
            UpdateGameUI();
            UpdateTowerUI();
        }
        #endif
        #endregion
    }
} 