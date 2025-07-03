using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

namespace DefenceGame.Core
{
    /// <summary>
    /// 게임의 자원(골드, 점수 등)을 관리하는 매니저
    /// 자원 획득, 소비, 비용 계산 등을 담당
    /// </summary>
    public class ResourceManager : MonoBehaviour
    {
        #region Resource Data Structures
        [System.Serializable]
        public class TowerCost
        {
            [Header("Tower Info")]
            public DefenceGame.Tower.TowerType towerType;
            public int baseCost = 100;
            public int upgradeCost = 50;
            
            [Header("Cost Scaling")]
            public float costMultiplier = 1.2f;
            public int maxLevel = 3;
        }
        
        [System.Serializable]
        public class ResourceData
        {
            [Header("Current Resources")]
            public int gold = 100;
            public int score = 0;
            public int lives = 100;
            
            [Header("Resource Limits")]
            public int maxGold = 99999;
            public int maxScore = 999999;
            public int maxLives = 100;
        }
        #endregion

        #region Inspector Fields
        [Header("Resource Configuration")]
        [SerializeField] private ResourceData resourceData = new ResourceData();
        [SerializeField] private List<TowerCost> towerCosts = new List<TowerCost>();
        
        [Header("Resource Rewards")]
        [SerializeField] private int goldPerKill = 10;
        [SerializeField] private int goldPerWave = 50;
        [SerializeField] private int scorePerKill = 5;
        [SerializeField] private int scorePerWave = 100;
        
        [Header("Resource Display")]
        [SerializeField] private bool enableResourceAnimation = true;
        [SerializeField] private float resourceChangeAnimationDuration = 0.5f;
        #endregion

        #region Events
        [Header("Events")]
        public UnityEvent<int> OnGoldChanged;           // newGold
        public UnityEvent<int> OnScoreChanged;          // newScore
        public UnityEvent<int> OnLivesChanged;          // newLives
        public UnityEvent<int, int> OnResourceChanged;  // oldValue, newValue
        public UnityEvent<string> OnResourceInsufficient; // resourceType
        public UnityEvent<int> OnGoldEarned;            // earnedAmount
        public UnityEvent<int> OnScoreEarned;           // earnedAmount
        #endregion

        #region Properties
        public int Gold 
        { 
            get => resourceData.gold; 
            private set
            {
                int oldGold = resourceData.gold;
                resourceData.gold = Mathf.Clamp(value, 0, resourceData.maxGold);
                
                if (oldGold != resourceData.gold)
                {
                    OnGoldChanged?.Invoke(resourceData.gold);
                    OnResourceChanged?.Invoke(oldGold, resourceData.gold);
                }
            }
        }
        
        public int Score 
        { 
            get => resourceData.score; 
            private set
            {
                int oldScore = resourceData.score;
                resourceData.score = Mathf.Clamp(value, 0, resourceData.maxScore);
                
                if (oldScore != resourceData.score)
                {
                    OnScoreChanged?.Invoke(resourceData.score);
                    OnResourceChanged?.Invoke(oldScore, resourceData.score);
                }
            }
        }
        
        public int Lives 
        { 
            get => resourceData.lives; 
            private set
            {
                int oldLives = resourceData.lives;
                resourceData.lives = Mathf.Clamp(value, 0, resourceData.maxLives);
                
                if (oldLives != resourceData.lives)
                {
                    OnLivesChanged?.Invoke(resourceData.lives);
                    OnResourceChanged?.Invoke(oldLives, resourceData.lives);
                }
            }
        }
        
        public int MaxGold => resourceData.maxGold;
        public int MaxScore => resourceData.maxScore;
        public int MaxLives => resourceData.maxLives;
        public int GoldPerKill => goldPerKill;
        public int GoldPerWave => goldPerWave;
        public int ScorePerKill => scorePerKill;
        public int ScorePerWave => scorePerWave;
        #endregion

        #region Private Fields
        private bool isInitialized = false;
        #endregion

        #region Initialization
        private void Awake()
        {
            InitializeResourceManager();
        }
        
        private void InitializeResourceManager()
        {
            if (isInitialized) return;
            
            Debug.Log("ResourceManager 초기화 시작");
            
            // 기본 타워 비용 설정
            if (towerCosts.Count == 0)
            {
                CreateDefaultTowerCosts();
            }
            
            // 초기 자원 설정
            ResetResources();
            
            isInitialized = true;
            
            Debug.Log("ResourceManager 초기화 완료");
        }
        
        private void CreateDefaultTowerCosts()
        {
            // 기본 타워 비용 설정
            towerCosts.Add(new TowerCost
            {
                towerType = DefenceGame.Tower.TowerType.Basic,
                baseCost = 100,
                upgradeCost = 50,
                costMultiplier = 1.2f,
                maxLevel = 3
            });
            
            towerCosts.Add(new TowerCost
            {
                towerType = DefenceGame.Tower.TowerType.Sniper,
                baseCost = 200,
                upgradeCost = 100,
                costMultiplier = 1.3f,
                maxLevel = 3
            });
            
            towerCosts.Add(new TowerCost
            {
                towerType = DefenceGame.Tower.TowerType.Area,
                baseCost = 300,
                upgradeCost = 150,
                costMultiplier = 1.4f,
                maxLevel = 3
            });
            
            towerCosts.Add(new TowerCost
            {
                towerType = DefenceGame.Tower.TowerType.Slow,
                baseCost = 250,
                upgradeCost = 125,
                costMultiplier = 1.25f,
                maxLevel = 3
            });
        }
        #endregion

        #region Resource Management Methods
        /// <summary>
        /// 자원 초기화
        /// </summary>
        public void ResetResources()
        {
            Gold = 100; // 시작 골드
            Score = 0;
            Lives = 100;
            
            Debug.Log("자원이 초기화되었습니다.");
        }
        
        /// <summary>
        /// 골드 추가
        /// </summary>
        public void AddGold(int amount)
        {
            if (amount > 0)
            {
                int oldGold = Gold;
                Gold += amount;
                OnGoldEarned?.Invoke(amount);
                
                Debug.Log($"골드 {amount} 획득. 총 골드: {Gold}");
            }
        }
        
        /// <summary>
        /// 골드 소비
        /// </summary>
        public bool SpendGold(int amount)
        {
            if (amount <= 0)
            {
                Debug.LogWarning("소비할 골드가 0 이하입니다.");
                return false;
            }
            
            if (CanAfford(amount))
            {
                Gold -= amount;
                Debug.Log($"골드 {amount} 소비. 남은 골드: {Gold}");
                return true;
            }
            else
            {
                OnResourceInsufficient?.Invoke("Gold");
                Debug.LogWarning($"골드가 부족합니다. 필요: {amount}, 보유: {Gold}");
                return false;
            }
        }
        
        /// <summary>
        /// 점수 추가
        /// </summary>
        public void AddScore(int amount)
        {
            if (amount > 0)
            {
                int oldScore = Score;
                Score += amount;
                OnScoreEarned?.Invoke(amount);
                
                Debug.Log($"점수 {amount} 획득. 총 점수: {Score}");
            }
        }
        
        /// <summary>
        /// 체력 변경
        /// </summary>
        public void ChangeLives(int amount)
        {
            if (amount != 0)
            {
                Lives += amount;
                
                string action = amount > 0 ? "회복" : "감소";
                Debug.Log($"체력 {Mathf.Abs(amount)} {action}. 현재 체력: {Lives}");
            }
        }
        
        /// <summary>
        /// 자원 충분 여부 확인
        /// </summary>
        public bool CanAfford(int cost)
        {
            return Gold >= cost;
        }
        
        /// <summary>
        /// 적 처치 보상
        /// </summary>
        public void RewardForKill()
        {
            AddGold(goldPerKill);
            AddScore(scorePerKill);
        }
        
        /// <summary>
        /// 웨이브 완료 보상
        /// </summary>
        public void RewardForWave()
        {
            AddGold(goldPerWave);
            AddScore(scorePerWave);
        }
        #endregion

        #region Tower Cost Management
        /// <summary>
        /// 타워 구매 비용 반환
        /// </summary>
        public int GetTowerCost(DefenceGame.Tower.TowerType towerType)
        {
            TowerCost cost = GetTowerCostData(towerType);
            return cost?.baseCost ?? 0;
        }
        
        /// <summary>
        /// 타워 업그레이드 비용 반환
        /// </summary>
        public int GetTowerUpgradeCost(DefenceGame.Tower.TowerType towerType, int currentLevel)
        {
            TowerCost cost = GetTowerCostData(towerType);
            if (cost == null) return 0;
            
            if (currentLevel >= cost.maxLevel)
            {
                return -1; // 최대 레벨
            }
            
            return Mathf.RoundToInt(cost.upgradeCost * Mathf.Pow(cost.costMultiplier, currentLevel - 1));
        }
        
        /// <summary>
        /// 타워 구매 가능 여부 확인
        /// </summary>
        public bool CanAffordTower(DefenceGame.Tower.TowerType towerType)
        {
            return CanAfford(GetTowerCost(towerType));
        }
        
        /// <summary>
        /// 타워 업그레이드 가능 여부 확인
        /// </summary>
        public bool CanAffordTowerUpgrade(DefenceGame.Tower.TowerType towerType, int currentLevel)
        {
            int upgradeCost = GetTowerUpgradeCost(towerType, currentLevel);
            if (upgradeCost == -1) return false; // 최대 레벨
            
            return CanAfford(upgradeCost);
        }
        
        /// <summary>
        /// 타워 구매
        /// </summary>
        public bool PurchaseTower(DefenceGame.Tower.TowerType towerType)
        {
            int cost = GetTowerCost(towerType);
            return SpendGold(cost);
        }
        
        /// <summary>
        /// 타워 업그레이드
        /// </summary>
        public bool UpgradeTower(DefenceGame.Tower.TowerType towerType, int currentLevel)
        {
            int cost = GetTowerUpgradeCost(towerType, currentLevel);
            if (cost == -1)
            {
                Debug.LogWarning("타워가 최대 레벨입니다.");
                return false;
            }
            
            return SpendGold(cost);
        }
        
        /// <summary>
        /// 타워 비용 데이터 반환
        /// </summary>
        private TowerCost GetTowerCostData(DefenceGame.Tower.TowerType towerType)
        {
            return towerCosts.Find(cost => cost.towerType == towerType);
        }
        #endregion

        #region Resource Configuration
        /// <summary>
        /// 타워 비용 설정
        /// </summary>
        public void SetTowerCost(DefenceGame.Tower.TowerType towerType, int baseCost, int upgradeCost)
        {
            TowerCost cost = GetTowerCostData(towerType);
            if (cost != null)
            {
                cost.baseCost = baseCost;
                cost.upgradeCost = upgradeCost;
            }
            else
            {
                towerCosts.Add(new TowerCost
                {
                    towerType = towerType,
                    baseCost = baseCost,
                    upgradeCost = upgradeCost
                });
            }
        }
        
        /// <summary>
        /// 보상 설정
        /// </summary>
        public void SetRewards(int killGold, int killScore, int waveGold, int waveScore)
        {
            goldPerKill = killGold;
            scorePerKill = killScore;
            goldPerWave = waveGold;
            scorePerWave = waveScore;
        }
        
        /// <summary>
        /// 최대 자원 설정
        /// </summary>
        public void SetMaxResources(int maxGold, int maxScore, int maxLives)
        {
            resourceData.maxGold = maxGold;
            resourceData.maxScore = maxScore;
            resourceData.maxLives = maxLives;
        }
        #endregion

        #region Utility Methods
        /// <summary>
        /// 현재 자원 상태 문자열 반환
        /// </summary>
        public string GetResourceStatus()
        {
            return $"Gold: {Gold}/{MaxGold}, Score: {Score}/{MaxScore}, Lives: {Lives}/{MaxLives}";
        }
        
        /// <summary>
        /// 모든 타워 비용 정보 반환
        /// </summary>
        public List<TowerCost> GetAllTowerCosts()
        {
            return new List<TowerCost>(towerCosts);
        }
        
        /// <summary>
        /// 특정 타입의 타워 비용 정보 반환
        /// </summary>
        public TowerCost GetTowerCostInfo(DefenceGame.Tower.TowerType towerType)
        {
            return GetTowerCostData(towerType);
        }
        #endregion

        #region Debug Methods
        #if UNITY_EDITOR
        [ContextMenu("Add 100 Gold")]
        private void DebugAddGold()
        {
            AddGold(100);
        }
        
        [ContextMenu("Add 1000 Score")]
        private void DebugAddScore()
        {
            AddScore(1000);
        }
        
        [ContextMenu("Reset Resources")]
        private void DebugResetResources()
        {
            ResetResources();
        }
        
        [ContextMenu("Print Resource Status")]
        private void DebugPrintResourceStatus()
        {
            Debug.Log(GetResourceStatus());
        }
        #endif
        #endregion
    }
} 