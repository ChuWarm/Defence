using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

namespace DefenceGame.Core
{
    /// <summary>
    /// 게임의 전체 상태와 핵심 시스템들을 관리하는 싱글톤 매니저
    /// Unity 6 호환성, 확장성, 런타임 최적화를 고려하여 설계
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        #region Singleton Pattern
        public static GameManager Instance { get; private set; }
        
        private void Awake()
        {
            // 싱글톤 패턴 구현 - 중복 인스턴스 방지
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeGameManager();
            }
            else
            {
                Debug.LogWarning("GameManager 인스턴스가 이미 존재합니다. 중복 인스턴스를 제거합니다.");
                Destroy(gameObject);
            }
        }
        #endregion

        #region Game State Management
        [Header("Game State")]
        [SerializeField] private GameState currentGameState = GameState.Menu;
        
        public enum GameState
        {
            Menu,           // 메인 메뉴
            Loading,        // 로딩 중
            Playing,        // 게임 플레이 중
            Paused,         // 일시정지
            GameOver,       // 게임 오버
            Victory         // 승리
        }
        
        public GameState CurrentState 
        { 
            get => currentGameState; 
            private set
            {
                if (currentGameState != value)
                {
                    GameState previousState = currentGameState;
                    currentGameState = value;
                    OnGameStateChanged?.Invoke(previousState, currentGameState);
                }
            }
        }
        #endregion

        #region Core Systems References
        [Header("Core Systems")]
        [SerializeField] private WaveManager waveManager;
        [SerializeField] private ResourceManager resourceManager;
        [SerializeField] private InputManager inputManager;
        [SerializeField] private UIManager uiManager;
        
        public WaveManager WaveManager => waveManager;
        public ResourceManager ResourceManager => resourceManager;
        public InputManager InputManager => inputManager;
        public UIManager UIManager => uiManager;
        #endregion

        #region Game Data
        [Header("Game Data")]
        [SerializeField] private int playerHealth = 100;
        [SerializeField] private int maxPlayerHealth = 100;
        [SerializeField] private int currentWave = 0;
        [SerializeField] private float gameTime = 0f;
        [SerializeField] private int totalScore = 0;
        
        public int PlayerHealth 
        { 
            get => playerHealth; 
            private set
            {
                if (playerHealth != value)
                {
                    playerHealth = Mathf.Clamp(value, 0, maxPlayerHealth);
                    OnPlayerHealthChanged?.Invoke(playerHealth, maxPlayerHealth);
                    
                    if (playerHealth <= 0)
                    {
                        GameOver();
                    }
                }
            }
        }
        
        public int MaxPlayerHealth => maxPlayerHealth;
        public int CurrentWave => currentWave;
        public float GameTime => gameTime;
        public int TotalScore => totalScore;
        #endregion

        #region Events
        [Header("Events")]
        public UnityEvent<GameState, GameState> OnGameStateChanged;
        public UnityEvent<int, int> OnPlayerHealthChanged; // current, max
        public UnityEvent<int> OnWaveChanged;
        public UnityEvent<int> OnScoreChanged;
        public UnityEvent OnGameOver;
        public UnityEvent OnVictory;
        public UnityEvent OnGameStarted;
        public UnityEvent OnGamePaused;
        public UnityEvent OnGameResumed;
        #endregion

        #region Initialization
        private void InitializeGameManager()
        {
            Debug.Log("GameManager 초기화 시작");
            
            // 코어 시스템들 자동 찾기 및 초기화
            InitializeCoreSystems();
            
            // 게임 상태 초기화
            ResetGameData();
            
            // 이벤트 리스너 등록
            RegisterEventListeners();
            
            Debug.Log("GameManager 초기화 완료");
        }
        
        private void InitializeCoreSystems()
        {
            // 자동으로 씬에서 매니저들 찾기
            if (waveManager == null)
                waveManager = FindObjectOfType<WaveManager>();
                
            if (resourceManager == null)
                resourceManager = FindObjectOfType<ResourceManager>();
                
            if (inputManager == null)
                inputManager = FindObjectOfType<InputManager>();
                
            if (uiManager == null)
                uiManager = FindObjectOfType<UIManager>();
            
            // 필수 시스템 체크
            if (waveManager == null)
                Debug.LogError("WaveManager를 찾을 수 없습니다!");
            if (resourceManager == null)
                Debug.LogError("ResourceManager를 찾을 수 없습니다!");
            if (inputManager == null)
                Debug.LogError("InputManager를 찾을 수 없습니다!");
            if (uiManager == null)
                Debug.LogError("UIManager를 찾을 수 없습니다!");
        }
        
        private void RegisterEventListeners()
        {
            // 웨이브 매니저 이벤트 구독
            if (waveManager != null)
            {
                waveManager.OnWaveStarted += OnWaveStarted;
                waveManager.OnWaveCompleted += OnWaveCompleted;
                waveManager.OnAllWavesCompleted += OnAllWavesCompleted;
            }
            
            // 리소스 매니저 이벤트 구독
            if (resourceManager != null)
            {
                resourceManager.OnGoldChanged += OnGoldChanged;
            }
        }
        #endregion

        #region Game Control Methods
        /// <summary>
        /// 게임 시작
        /// </summary>
        public void StartGame()
        {
            if (CurrentState == GameState.Menu || CurrentState == GameState.GameOver)
            {
                Debug.Log("게임 시작");
                ResetGameData();
                CurrentState = GameState.Playing;
                OnGameStarted?.Invoke();
            }
        }
        
        /// <summary>
        /// 게임 일시정지
        /// </summary>
        public void PauseGame()
        {
            if (CurrentState == GameState.Playing)
            {
                Debug.Log("게임 일시정지");
                CurrentState = GameState.Paused;
                Time.timeScale = 0f;
                OnGamePaused?.Invoke();
            }
        }
        
        /// <summary>
        /// 게임 재개
        /// </summary>
        public void ResumeGame()
        {
            if (CurrentState == GameState.Paused)
            {
                Debug.Log("게임 재개");
                CurrentState = GameState.Playing;
                Time.timeScale = 1f;
                OnGameResumed?.Invoke();
            }
        }
        
        /// <summary>
        /// 게임 오버
        /// </summary>
        public void GameOver()
        {
            if (CurrentState != GameState.GameOver)
            {
                Debug.Log("게임 오버");
                CurrentState = GameState.GameOver;
                Time.timeScale = 0f;
                OnGameOver?.Invoke();
            }
        }
        
        /// <summary>
        /// 승리
        /// </summary>
        public void Victory()
        {
            if (CurrentState == GameState.Playing)
            {
                Debug.Log("승리!");
                CurrentState = GameState.Victory;
                Time.timeScale = 0f;
                OnVictory?.Invoke();
            }
        }
        
        /// <summary>
        /// 메인 메뉴로 돌아가기
        /// </summary>
        public void ReturnToMenu()
        {
            Debug.Log("메인 메뉴로 이동");
            CurrentState = GameState.Menu;
            Time.timeScale = 1f;
            ResetGameData();
        }
        #endregion

        #region Game Data Methods
        /// <summary>
        /// 게임 데이터 초기화
        /// </summary>
        private void ResetGameData()
        {
            playerHealth = maxPlayerHealth;
            currentWave = 0;
            gameTime = 0f;
            totalScore = 0;
            
            // 이벤트 발생
            OnPlayerHealthChanged?.Invoke(playerHealth, maxPlayerHealth);
            OnWaveChanged?.Invoke(currentWave);
            OnScoreChanged?.Invoke(totalScore);
        }
        
        /// <summary>
        /// 플레이어 체력 변경
        /// </summary>
        public void TakeDamage(int damage)
        {
            if (damage > 0 && CurrentState == GameState.Playing)
            {
                PlayerHealth -= damage;
                Debug.Log($"플레이어가 {damage} 데미지를 받았습니다. 남은 체력: {PlayerHealth}");
            }
        }
        
        /// <summary>
        /// 플레이어 체력 회복
        /// </summary>
        public void HealPlayer(int healAmount)
        {
            if (healAmount > 0)
            {
                PlayerHealth += healAmount;
                Debug.Log($"플레이어가 {healAmount} 체력을 회복했습니다. 현재 체력: {PlayerHealth}");
            }
        }
        
        /// <summary>
        /// 점수 추가
        /// </summary>
        public void AddScore(int score)
        {
            if (score > 0)
            {
                totalScore += score;
                OnScoreChanged?.Invoke(totalScore);
                Debug.Log($"점수 {score} 추가. 총 점수: {totalScore}");
            }
        }
        
        /// <summary>
        /// 웨이브 변경
        /// </summary>
        public void SetCurrentWave(int wave)
        {
            if (wave >= 0 && wave != currentWave)
            {
                currentWave = wave;
                OnWaveChanged?.Invoke(currentWave);
                Debug.Log($"현재 웨이브: {currentWave}");
            }
        }
        #endregion

        #region Event Handlers
        private void OnWaveStarted(int waveNumber)
        {
            SetCurrentWave(waveNumber);
        }
        
        private void OnWaveCompleted(int waveNumber)
        {
            // 웨이브 완료 보상
            if (resourceManager != null)
            {
                resourceManager.AddGold(50); // 웨이브 완료 보상
            }
            AddScore(100); // 웨이브 완료 점수
        }
        
        private void OnAllWavesCompleted()
        {
            Victory();
        }
        
        private void OnGoldChanged(int newGold)
        {
            // 골드 변경 시 필요한 로직
            Debug.Log($"골드 변경: {newGold}");
        }
        #endregion

        #region Unity Lifecycle
        private void Update()
        {
            if (CurrentState == GameState.Playing)
            {
                gameTime += Time.deltaTime;
            }
        }
        
        private void OnDestroy()
        {
            // 이벤트 리스너 해제
            if (waveManager != null)
            {
                waveManager.OnWaveStarted -= OnWaveStarted;
                waveManager.OnWaveCompleted -= OnWaveCompleted;
                waveManager.OnAllWavesCompleted -= OnAllWavesCompleted;
            }
            
            if (resourceManager != null)
            {
                resourceManager.OnGoldChanged -= OnGoldChanged;
            }
        }
        #endregion

        #region Debug Methods
        [Header("Debug")]
        [SerializeField] private bool enableDebugLogs = true;
        
        private void LogDebug(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[GameManager] {message}");
            }
        }
        
        #if UNITY_EDITOR
        [ContextMenu("Reset Game Data")]
        private void DebugResetGameData()
        {
            ResetGameData();
            LogDebug("게임 데이터가 리셋되었습니다.");
        }
        
        [ContextMenu("Take 10 Damage")]
        private void DebugTakeDamage()
        {
            TakeDamage(10);
        }
        
        [ContextMenu("Add 100 Gold")]
        private void DebugAddGold()
        {
            if (resourceManager != null)
            {
                resourceManager.AddGold(100);
            }
        }
        #endif
        #endregion
    }
} 