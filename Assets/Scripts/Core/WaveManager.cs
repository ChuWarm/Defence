using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

namespace DefenceGame.Core
{
    /// <summary>
    /// 웨이브 시스템을 관리하는 매니저
    /// 적 스폰, 웨이브 진행, 난이도 조절 등을 담당
    /// </summary>
    public class WaveManager : MonoBehaviour
    {
        #region Wave Data Structures
        [System.Serializable]
        public class EnemySpawnData
        {
            [Header("Enemy Info")]
            public GameObject enemyPrefab;
            public int count = 1;
            public float spawnDelay = 1f;
            
            [Header("Wave Info")]
            public int waveNumber;
            public float healthMultiplier = 1f;
            public float speedMultiplier = 1f;
        }
        
        [System.Serializable]
        public class WaveData
        {
            [Header("Wave Settings")]
            public int waveNumber;
            public string waveName = "Wave";
            public List<EnemySpawnData> enemies = new List<EnemySpawnData>();
            
            [Header("Timing")]
            public float spawnInterval = 2f;
            public float waveDelay = 5f;
            public float preparationTime = 10f;
            
            [Header("Rewards")]
            public int goldReward = 50;
            public int scoreReward = 100;
        }
        #endregion

        #region Inspector Fields
        [Header("Wave Configuration")]
        [SerializeField] private List<WaveData> waveDataList = new List<WaveData>();
        [SerializeField] private int currentWaveIndex = 0;
        [SerializeField] private bool isWaveActive = false;
        [SerializeField] private bool isPreparationTime = false;
        
        [Header("Spawn Settings")]
        [SerializeField] private Transform[] spawnPoints;
        [SerializeField] private Transform[] waypoints;
        [SerializeField] private LayerMask enemyLayer;
        
        [Header("Wave Progression")]
        [SerializeField] private float preparationTimeRemaining = 0f;
        [SerializeField] private int enemiesRemainingInWave = 0;
        [SerializeField] private int totalEnemiesSpawned = 0;
        [SerializeField] private int totalEnemiesKilled = 0;
        #endregion

        #region Events
        [Header("Events")]
        public UnityEvent<int> OnWaveStarted;           // waveNumber
        public UnityEvent<int> OnWaveCompleted;         // waveNumber
        public UnityEvent OnAllWavesCompleted;
        public UnityEvent<float> OnPreparationTimeChanged; // remainingTime
        public UnityEvent<int> OnEnemySpawned;          // enemyCount
        public UnityEvent<int> OnEnemyKilled;           // enemyCount
        public UnityEvent<int> OnEnemiesRemainingChanged; // remainingCount
        #endregion

        #region Properties
        public int CurrentWaveIndex => currentWaveIndex;
        public int TotalWaves => waveDataList.Count;
        public bool IsWaveActive => isWaveActive;
        public bool IsPreparationTime => isPreparationTime;
        public float PreparationTimeRemaining => preparationTimeRemaining;
        public int EnemiesRemainingInWave => enemiesRemainingInWave;
        public int TotalEnemiesSpawned => totalEnemiesSpawned;
        public int TotalEnemiesKilled => totalEnemiesKilled;
        public WaveData CurrentWaveData => currentWaveIndex < waveDataList.Count ? waveDataList[currentWaveIndex] : null;
        #endregion

        #region Private Fields
        private Coroutine currentWaveCoroutine;
        private List<GameObject> activeEnemies = new List<GameObject>();
        private bool isInitialized = false;
        #endregion

        #region Initialization
        private void Awake()
        {
            InitializeWaveManager();
        }
        
        private void InitializeWaveManager()
        {
            if (isInitialized) return;
            
            Debug.Log("WaveManager 초기화 시작");
            
            // 스폰 포인트와 웨이포인트 자동 찾기
            if (spawnPoints == null || spawnPoints.Length == 0)
            {
                GameObject[] spawnPointObjects = GameObject.FindGameObjectsWithTag("SpawnPoint");
                spawnPoints = new Transform[spawnPointObjects.Length];
                for (int i = 0; i < spawnPointObjects.Length; i++)
                {
                    spawnPoints[i] = spawnPointObjects[i].transform;
                }
            }
            
            if (waypoints == null || waypoints.Length == 0)
            {
                GameObject[] waypointObjects = GameObject.FindGameObjectsWithTag("Waypoint");
                waypoints = new Transform[waypointObjects.Length];
                for (int i = 0; i < waypointObjects.Length; i++)
                {
                    waypoints[i] = waypointObjects[i].transform;
                }
            }
            
            // 기본 웨이브 데이터 생성 (테스트용)
            if (waveDataList.Count == 0)
            {
                CreateDefaultWaveData();
            }
            
            ResetWaveManager();
            isInitialized = true;
            
            Debug.Log("WaveManager 초기화 완료");
        }
        
        private void CreateDefaultWaveData()
        {
            // 기본 웨이브 데이터 생성 (나중에 ScriptableObject로 대체)
            for (int i = 0; i < 5; i++)
            {
                WaveData waveData = new WaveData
                {
                    waveNumber = i + 1,
                    waveName = $"Wave {i + 1}",
                    spawnInterval = 2f - (i * 0.2f), // 점점 빨라짐
                    waveDelay = 5f,
                    preparationTime = 10f,
                    goldReward = 50 + (i * 10),
                    scoreReward = 100 + (i * 50)
                };
                
                // 기본 적 스폰 데이터
                EnemySpawnData enemyData = new EnemySpawnData
                {
                    enemyPrefab = null, // 나중에 설정
                    count = 3 + (i * 2), // 점점 많아짐
                    spawnDelay = 1f,
                    waveNumber = i + 1,
                    healthMultiplier = 1f + (i * 0.2f), // 점점 강해짐
                    speedMultiplier = 1f + (i * 0.1f)
                };
                
                waveData.enemies.Add(enemyData);
                waveDataList.Add(waveData);
            }
        }
        #endregion

        #region Wave Control Methods
        /// <summary>
        /// 웨이브 매니저 리셋
        /// </summary>
        public void ResetWaveManager()
        {
            StopAllCoroutines();
            currentWaveIndex = 0;
            isWaveActive = false;
            isPreparationTime = false;
            preparationTimeRemaining = 0f;
            enemiesRemainingInWave = 0;
            totalEnemiesSpawned = 0;
            totalEnemiesKilled = 0;
            
            // 활성 적들 제거
            ClearActiveEnemies();
        }
        
        /// <summary>
        /// 다음 웨이브 시작
        /// </summary>
        public void StartNextWave()
        {
            if (currentWaveIndex >= waveDataList.Count)
            {
                Debug.Log("모든 웨이브가 완료되었습니다!");
                OnAllWavesCompleted?.Invoke();
                return;
            }
            
            if (isWaveActive)
            {
                Debug.LogWarning("현재 웨이브가 진행 중입니다.");
                return;
            }
            
            StartCoroutine(StartWaveCoroutine(currentWaveIndex));
        }
        
        /// <summary>
        /// 특정 웨이브 시작
        /// </summary>
        public void StartWave(int waveIndex)
        {
            if (waveIndex < 0 || waveIndex >= waveDataList.Count)
            {
                Debug.LogError($"잘못된 웨이브 인덱스: {waveIndex}");
                return;
            }
            
            currentWaveIndex = waveIndex;
            StartNextWave();
        }
        
        /// <summary>
        /// 현재 웨이브 중단
        /// </summary>
        public void StopCurrentWave()
        {
            if (isWaveActive)
            {
                StopAllCoroutines();
                isWaveActive = false;
                isPreparationTime = false;
                Debug.Log("현재 웨이브가 중단되었습니다.");
            }
        }
        #endregion

        #region Wave Coroutines
        private IEnumerator StartWaveCoroutine(int waveIndex)
        {
            WaveData waveData = waveDataList[waveIndex];
            currentWaveIndex = waveIndex;
            
            Debug.Log($"웨이브 {waveData.waveNumber} 시작: {waveData.waveName}");
            
            // 준비 시간
            if (waveData.preparationTime > 0)
            {
                yield return StartCoroutine(PreparationTimeCoroutine(waveData.preparationTime));
            }
            
            // 웨이브 시작
            isWaveActive = true;
            OnWaveStarted?.Invoke(waveData.waveNumber);
            
            // 적 스폰
            yield return StartCoroutine(SpawnEnemiesCoroutine(waveData));
            
            // 웨이브 완료 대기
            yield return StartCoroutine(WaitForWaveCompletionCoroutine());
            
            // 웨이브 완료
            CompleteWave(waveData);
        }
        
        private IEnumerator PreparationTimeCoroutine(float preparationTime)
        {
            isPreparationTime = true;
            preparationTimeRemaining = preparationTime;
            
            Debug.Log($"준비 시간 시작: {preparationTime}초");
            
            while (preparationTimeRemaining > 0)
            {
                preparationTimeRemaining -= Time.deltaTime;
                OnPreparationTimeChanged?.Invoke(preparationTimeRemaining);
                yield return null;
            }
            
            isPreparationTime = false;
            preparationTimeRemaining = 0f;
            OnPreparationTimeChanged?.Invoke(0f);
            
            Debug.Log("준비 시간 종료");
        }
        
        private IEnumerator SpawnEnemiesCoroutine(WaveData waveData)
        {
            enemiesRemainingInWave = 0;
            
            // 총 적 수 계산
            foreach (var enemyData in waveData.enemies)
            {
                enemiesRemainingInWave += enemyData.count;
            }
            
            OnEnemiesRemainingChanged?.Invoke(enemiesRemainingInWave);
            
            // 각 적 타입별로 스폰
            foreach (var enemyData in waveData.enemies)
            {
                for (int i = 0; i < enemyData.count; i++)
                {
                    SpawnEnemy(enemyData);
                    totalEnemiesSpawned++;
                    OnEnemySpawned?.Invoke(totalEnemiesSpawned);
                    
                    yield return new WaitForSeconds(enemyData.spawnDelay);
                }
            }
        }
        
        private IEnumerator WaitForWaveCompletionCoroutine()
        {
            // 모든 적이 제거될 때까지 대기
            while (enemiesRemainingInWave > 0)
            {
                yield return new WaitForSeconds(0.5f);
            }
        }
        #endregion

        #region Enemy Spawning
        /// <summary>
        /// 적 스폰
        /// </summary>
        private void SpawnEnemy(EnemySpawnData enemyData)
        {
            if (enemyData.enemyPrefab == null)
            {
                Debug.LogWarning("적 프리팹이 설정되지 않았습니다.");
                return;
            }
            
            // 스폰 포인트 선택
            Transform spawnPoint = GetRandomSpawnPoint();
            if (spawnPoint == null)
            {
                Debug.LogError("스폰 포인트를 찾을 수 없습니다!");
                return;
            }
            
            // 적 생성
            GameObject enemy = Instantiate(enemyData.enemyPrefab, spawnPoint.position, spawnPoint.rotation);
            activeEnemies.Add(enemy);
            
            // 적 컴포넌트 설정
            SetupEnemy(enemy, enemyData);
            
            Debug.Log($"적 스폰: {enemy.name} at {spawnPoint.position}");
        }
        
        /// <summary>
        /// 적 설정
        /// </summary>
        private void SetupEnemy(GameObject enemy, EnemySpawnData enemyData)
        {
            // Enemy 컴포넌트 찾기 및 설정
            var enemyComponent = enemy.GetComponent<DefenceGame.Enemy.Enemy>();
            if (enemyComponent != null)
            {
                enemyComponent.SetupEnemy(enemyData.healthMultiplier, enemyData.speedMultiplier);
                enemyComponent.OnEnemyDied += OnEnemyDied;
                enemyComponent.OnEnemyReachedEnd += OnEnemyReachedEnd;
            }
            
            // 웨이포인트 설정
            var enemyMovement = enemy.GetComponent<DefenceGame.Enemy.EnemyMovement>();
            if (enemyMovement != null && waypoints.Length > 0)
            {
                enemyMovement.SetWaypoints(waypoints);
            }
        }
        
        /// <summary>
        /// 랜덤 스폰 포인트 반환
        /// </summary>
        private Transform GetRandomSpawnPoint()
        {
            if (spawnPoints == null || spawnPoints.Length == 0)
                return null;
                
            return spawnPoints[Random.Range(0, spawnPoints.Length)];
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// 적 사망 처리
        /// </summary>
        private void OnEnemyDied(DefenceGame.Enemy.Enemy enemy)
        {
            activeEnemies.Remove(enemy.gameObject);
            enemiesRemainingInWave--;
            totalEnemiesKilled++;
            
            OnEnemyKilled?.Invoke(totalEnemiesKilled);
            OnEnemiesRemainingChanged?.Invoke(enemiesRemainingInWave);
            
            Debug.Log($"적 사망: {enemy.name}. 남은 적: {enemiesRemainingInWave}");
        }
        
        /// <summary>
        /// 적이 끝에 도달했을 때 처리
        /// </summary>
        private void OnEnemyReachedEnd(DefenceGame.Enemy.Enemy enemy)
        {
            activeEnemies.Remove(enemy.gameObject);
            enemiesRemainingInWave--;
            
            OnEnemiesRemainingChanged?.Invoke(enemiesRemainingInWave);
            
            // 플레이어 체력 감소
            if (GameManager.Instance != null)
            {
                GameManager.Instance.TakeDamage(enemy.DamageToPlayer);
            }
            
            Debug.Log($"적이 끝에 도달: {enemy.name}. 남은 적: {enemiesRemainingInWave}");
        }
        #endregion

        #region Wave Completion
        /// <summary>
        /// 웨이브 완료
        /// </summary>
        private void CompleteWave(WaveData waveData)
        {
            isWaveActive = false;
            currentWaveIndex++;
            
            Debug.Log($"웨이브 {waveData.waveNumber} 완료!");
            
            // 보상 지급
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ResourceManager?.AddGold(waveData.goldReward);
                GameManager.Instance.AddScore(waveData.scoreReward);
            }
            
            OnWaveCompleted?.Invoke(waveData.waveNumber);
            
            // 다음 웨이브 자동 시작 (지연 후)
            if (currentWaveIndex < waveDataList.Count)
            {
                StartCoroutine(DelayedNextWave(waveData.waveDelay));
            }
            else
            {
                OnAllWavesCompleted?.Invoke();
            }
        }
        
        private IEnumerator DelayedNextWave(float delay)
        {
            yield return new WaitForSeconds(delay);
            StartNextWave();
        }
        #endregion

        #region Utility Methods
        /// <summary>
        /// 활성 적들 제거
        /// </summary>
        private void ClearActiveEnemies()
        {
            foreach (var enemy in activeEnemies)
            {
                if (enemy != null)
                {
                    Destroy(enemy);
                }
            }
            activeEnemies.Clear();
        }
        
        /// <summary>
        /// 현재 활성 적 수 반환
        /// </summary>
        public int GetActiveEnemyCount()
        {
            return activeEnemies.Count;
        }
        
        /// <summary>
        /// 웨이브 데이터 추가
        /// </summary>
        public void AddWaveData(WaveData waveData)
        {
            waveDataList.Add(waveData);
        }
        
        /// <summary>
        /// 웨이브 데이터 설정
        /// </summary>
        public void SetWaveData(List<WaveData> newWaveData)
        {
            waveDataList = newWaveData;
        }
        #endregion

        #region Debug Methods
        #if UNITY_EDITOR
        [ContextMenu("Start Next Wave")]
        private void DebugStartNextWave()
        {
            StartNextWave();
        }
        
        [ContextMenu("Stop Current Wave")]
        private void DebugStopCurrentWave()
        {
            StopCurrentWave();
        }
        
        [ContextMenu("Reset Wave Manager")]
        private void DebugResetWaveManager()
        {
            ResetWaveManager();
        }
        #endif
        #endregion
    }
} 