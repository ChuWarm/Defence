using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace DefenceGame.Core
{
    /// <summary>
    /// 모바일 터치 입력과 UI 상호작용을 관리하는 매니저
    /// 타워 배치, UI 클릭, 제스처 등을 처리
    /// </summary>
    public class InputManager : MonoBehaviour
    {
        #region Input Configuration
        [Header("Input Configuration")]
        [SerializeField] private LayerMask towerPlacementLayer = 1;
        [SerializeField] private LayerMask towerSelectionLayer = 1;
        [SerializeField] private LayerMask uiLayer = 1;
        
        [Header("Touch Settings")]
        [SerializeField] private float touchHoldTime = 0.5f;
        [SerializeField] private float doubleTapTime = 0.3f;
        [SerializeField] private float minSwipeDistance = 50f;
        
        [Header("Tower Placement")]
        [SerializeField] private bool isPlacingTower = false;
        [SerializeField] private DefenceGame.Tower.TowerType selectedTowerType;
        [SerializeField] private GameObject towerPreview;
        [SerializeField] private Material validPlacementMaterial;
        [SerializeField] private Material invalidPlacementMaterial;
        #endregion

        #region Events
        [Header("Events")]
        public UnityEvent<Vector3> OnTowerPlacementRequested;    // worldPosition
        public UnityEvent OnTowerPlacementCancelled;
        public UnityEvent<DefenceGame.Tower.Tower> OnTowerSelected;
        public UnityEvent<DefenceGame.Tower.Tower> OnTowerDeselected;
        public UnityEvent<Vector3> OnMapTapped;                  // worldPosition
        public UnityEvent<Vector2> OnSwipeDetected;              // swipeDirection
        public UnityEvent OnDoubleTap;
        public UnityEvent OnLongPress;
        #endregion

        #region Private Fields
        private Camera mainCamera;
        private bool isInitialized = false;
        private float lastTapTime;
        private Vector2 lastTapPosition;
        private float touchStartTime;
        private Vector2 touchStartPosition;
        private bool isTouchHeld = false;
        private bool isDragging = false;
        #endregion

        #region Properties
        public bool IsPlacingTower => isPlacingTower;
        public DefenceGame.Tower.TowerType SelectedTowerType => selectedTowerType;
        public GameObject TowerPreview => towerPreview;
        #endregion

        #region Initialization
        private void Awake()
        {
            InitializeInputManager();
        }
        
        private void InitializeInputManager()
        {
            if (isInitialized) return;
            
            Debug.Log("InputManager 초기화 시작");
            
            // 메인 카메라 찾기
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                mainCamera = FindObjectOfType<Camera>();
                Debug.LogWarning("메인 카메라를 찾을 수 없습니다. 첫 번째 카메라를 사용합니다.");
            }
            
            isInitialized = true;
            
            Debug.Log("InputManager 초기화 완료");
        }
        #endregion

        #region Unity Update
        private void Update()
        {
            if (!isInitialized) return;
            
            HandleTouchInput();
        }
        #endregion

        #region Touch Input Handling
        /// <summary>
        /// 터치 입력 처리
        /// </summary>
        private void HandleTouchInput()
        {
            // 터치 입력 확인
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                
                switch (touch.phase)
                {
                    case TouchPhase.Began:
                        OnTouchBegan(touch);
                        break;
                    case TouchPhase.Moved:
                        OnTouchMoved(touch);
                        break;
                    case TouchPhase.Ended:
                        OnTouchEnded(touch);
                        break;
                    case TouchPhase.Canceled:
                        OnTouchCanceled(touch);
                        break;
                }
            }
            
            // 마우스 입력 (에디터 테스트용)
            #if UNITY_EDITOR
            HandleMouseInput();
            #endif
        }
        
        /// <summary>
        /// 터치 시작
        /// </summary>
        private void OnTouchBegan(Touch touch)
        {
            touchStartTime = Time.time;
            touchStartPosition = touch.position;
            isTouchHeld = false;
            isDragging = false;
            
            // UI 요소 클릭 확인
            if (IsPointerOverUI(touch.position))
            {
                return; // UI 클릭은 처리하지 않음
            }
            
            // 타워 배치 중인 경우 프리뷰 업데이트
            if (isPlacingTower)
            {
                UpdateTowerPreview(touch.position);
            }
        }
        
        /// <summary>
        /// 터치 이동
        /// </summary>
        private void OnTouchMoved(Touch touch)
        {
            if (isPlacingTower)
            {
                UpdateTowerPreview(touch.position);
            }
            
            // 드래그 감지
            float dragDistance = Vector2.Distance(touchStartPosition, touch.position);
            if (dragDistance > minSwipeDistance && !isDragging)
            {
                isDragging = true;
                Vector2 swipeDirection = (touch.position - touchStartPosition).normalized;
                OnSwipeDetected?.Invoke(swipeDirection);
            }
        }
        
        /// <summary>
        /// 터치 종료
        /// </summary>
        private void OnTouchEnded(Touch touch)
        {
            float touchDuration = Time.time - touchStartTime;
            float tapDistance = Vector2.Distance(touchStartPosition, touch.position);
            
            // UI 요소 클릭 확인
            if (IsPointerOverUI(touch.position))
            {
                return; // UI 클릭은 처리하지 않음
            }
            
            // 타워 배치 처리
            if (isPlacingTower)
            {
                HandleTowerPlacement(touch.position);
                return;
            }
            
            // 일반 탭 처리
            if (tapDistance < minSwipeDistance)
            {
                HandleTap(touch.position, touchDuration);
            }
        }
        
        /// <summary>
        /// 터치 취소
        /// </summary>
        private void OnTouchCanceled(Touch touch)
        {
            isTouchHeld = false;
            isDragging = false;
            
            if (isPlacingTower)
            {
                CancelTowerPlacement();
            }
        }
        
        #if UNITY_EDITOR
        /// <summary>
        /// 마우스 입력 처리 (에디터 테스트용)
        /// </summary>
        private void HandleMouseInput()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vector2 mousePosition = Input.mousePosition;
                
                if (!IsPointerOverUI(mousePosition))
                {
                    if (isPlacingTower)
                    {
                        UpdateTowerPreview(mousePosition);
                    }
                }
            }
            else if (Input.GetMouseButton(0))
            {
                Vector2 mousePosition = Input.mousePosition;
                
                if (isPlacingTower)
                {
                    UpdateTowerPreview(mousePosition);
                }
            }
            else if (Input.GetMouseButtonUp(0))
            {
                Vector2 mousePosition = Input.mousePosition;
                
                if (!IsPointerOverUI(mousePosition))
                {
                    if (isPlacingTower)
                    {
                        HandleTowerPlacement(mousePosition);
                    }
                    else
                    {
                        HandleTap(mousePosition, 0.1f);
                    }
                }
            }
        }
        #endif
        #endregion

        #region Tap Handling
        /// <summary>
        /// 탭 처리
        /// </summary>
        private void HandleTap(Vector2 screenPosition, float duration)
        {
            // 더블 탭 감지
            if (Time.time - lastTapTime < doubleTapTime && 
                Vector2.Distance(screenPosition, lastTapPosition) < minSwipeDistance)
            {
                OnDoubleTap?.Invoke();
                lastTapTime = 0f; // 더블 탭 후 리셋
                return;
            }
            
            // 롱 프레스 감지
            if (duration > touchHoldTime)
            {
                OnLongPress?.Invoke();
                return;
            }
            
            // 일반 탭
            Vector3 worldPosition = ScreenToWorldPosition(screenPosition);
            OnMapTapped?.Invoke(worldPosition);
            
            // 타워 선택 확인
            CheckTowerSelection(worldPosition);
            
            // 탭 정보 저장
            lastTapTime = Time.time;
            lastTapPosition = screenPosition;
        }
        
        /// <summary>
        /// 타워 선택 확인
        /// </summary>
        private void CheckTowerSelection(Vector3 worldPosition)
        {
            Collider2D[] colliders = Physics2D.OverlapPointAll(worldPosition, towerSelectionLayer);
            
            foreach (var collider in colliders)
            {
                var tower = collider.GetComponent<DefenceGame.Tower.Tower>();
                if (tower != null)
                {
                    OnTowerSelected?.Invoke(tower);
                    return;
                }
            }
            
            // 타워가 선택되지 않았으면 선택 해제
            OnTowerDeselected?.Invoke(null);
        }
        #endregion

        #region Tower Placement
        /// <summary>
        /// 타워 배치 시작
        /// </summary>
        public void StartTowerPlacement(DefenceGame.Tower.TowerType towerType)
        {
            if (isPlacingTower)
            {
                CancelTowerPlacement();
            }
            
            selectedTowerType = towerType;
            isPlacingTower = true;
            
            // 타워 프리뷰 생성
            CreateTowerPreview();
            
            Debug.Log($"타워 배치 시작: {towerType}");
        }
        
        /// <summary>
        /// 타워 배치 취소
        /// </summary>
        public void CancelTowerPlacement()
        {
            if (!isPlacingTower) return;
            
            isPlacingTower = false;
            selectedTowerType = DefenceGame.Tower.TowerType.Basic;
            
            // 타워 프리뷰 제거
            DestroyTowerPreview();
            
            OnTowerPlacementCancelled?.Invoke();
            
            Debug.Log("타워 배치 취소");
        }
        
        /// <summary>
        /// 타워 배치 처리
        /// </summary>
        private void HandleTowerPlacement(Vector2 screenPosition)
        {
            Vector3 worldPosition = ScreenToWorldPosition(screenPosition);
            
            // 배치 가능한 위치인지 확인
            if (IsValidPlacementPosition(worldPosition))
            {
                OnTowerPlacementRequested?.Invoke(worldPosition);
                CancelTowerPlacement();
            }
            else
            {
                Debug.LogWarning("유효하지 않은 배치 위치입니다.");
            }
        }
        
        /// <summary>
        /// 타워 프리뷰 생성
        /// </summary>
        private void CreateTowerPreview()
        {
            // 타워 프리팹에서 프리뷰 생성
            GameObject towerPrefab = GetTowerPrefab(selectedTowerType);
            if (towerPrefab != null)
            {
                towerPreview = Instantiate(towerPrefab);
                
                // 프리뷰 설정
                var spriteRenderer = towerPreview.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    Color color = spriteRenderer.color;
                    color.a = 0.5f; // 반투명
                    spriteRenderer.color = color;
                }
                
                // 프리뷰를 비활성화 (위치 업데이트 후 활성화)
                towerPreview.SetActive(false);
            }
        }
        
        /// <summary>
        /// 타워 프리뷰 업데이트
        /// </summary>
        private void UpdateTowerPreview(Vector2 screenPosition)
        {
            if (towerPreview == null) return;
            
            Vector3 worldPosition = ScreenToWorldPosition(screenPosition);
            towerPreview.transform.position = worldPosition;
            
            // 배치 가능 여부에 따라 색상 변경
            bool isValid = IsValidPlacementPosition(worldPosition);
            UpdatePreviewMaterial(isValid);
            
            // 프리뷰 활성화
            if (!towerPreview.activeSelf)
            {
                towerPreview.SetActive(true);
            }
        }
        
        /// <summary>
        /// 타워 프리뷰 제거
        /// </summary>
        private void DestroyTowerPreview()
        {
            if (towerPreview != null)
            {
                Destroy(towerPreview);
                towerPreview = null;
            }
        }
        
        /// <summary>
        /// 프리뷰 머티리얼 업데이트
        /// </summary>
        private void UpdatePreviewMaterial(bool isValid)
        {
            if (towerPreview == null) return;
            
            var spriteRenderer = towerPreview.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.material = isValid ? validPlacementMaterial : invalidPlacementMaterial;
            }
        }
        
        /// <summary>
        /// 유효한 배치 위치인지 확인
        /// </summary>
        private bool IsValidPlacementPosition(Vector3 worldPosition)
        {
            // 다른 타워와 겹치는지 확인
            Collider2D[] colliders = Physics2D.OverlapPointAll(worldPosition, towerPlacementLayer);
            
            foreach (var collider in colliders)
            {
                if (collider.CompareTag("TowerPlacement"))
                {
                    return true;
                }
            }
            
            return false;
        }
        #endregion

        #region Utility Methods
        /// <summary>
        /// 스크린 좌표를 월드 좌표로 변환
        /// </summary>
        private Vector3 ScreenToWorldPosition(Vector2 screenPosition)
        {
            Vector3 worldPosition = mainCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, 10f));
            worldPosition.z = 0f;
            return worldPosition;
        }
        
        /// <summary>
        /// UI 요소 위에 포인터가 있는지 확인
        /// </summary>
        private bool IsPointerOverUI(Vector2 screenPosition)
        {
            if (EventSystem.current == null) return false;
            
            PointerEventData eventData = new PointerEventData(EventSystem.current);
            eventData.position = screenPosition;
            
            var results = new System.Collections.Generic.List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);
            
            return results.Count > 0;
        }
        
        /// <summary>
        /// 타워 프리팹 반환
        /// </summary>
        private GameObject GetTowerPrefab(DefenceGame.Tower.TowerType towerType)
        {
            // 나중에 TowerData에서 프리팹을 가져오도록 수정
            // 현재는 임시로 null 반환
            return null;
        }
        #endregion

        #region Configuration Methods
        /// <summary>
        /// 레이어 마스크 설정
        /// </summary>
        public void SetLayerMasks(LayerMask placementLayer, LayerMask selectionLayer, LayerMask uiLayer)
        {
            towerPlacementLayer = placementLayer;
            towerSelectionLayer = selectionLayer;
            this.uiLayer = uiLayer;
        }
        
        /// <summary>
        /// 터치 설정 변경
        /// </summary>
        public void SetTouchSettings(float holdTime, float doubleTapTime, float minSwipeDistance)
        {
            this.touchHoldTime = holdTime;
            this.doubleTapTime = doubleTapTime;
            this.minSwipeDistance = minSwipeDistance;
        }
        
        /// <summary>
        /// 프리뷰 머티리얼 설정
        /// </summary>
        public void SetPreviewMaterials(Material validMaterial, Material invalidMaterial)
        {
            validPlacementMaterial = validMaterial;
            invalidPlacementMaterial = invalidMaterial;
        }
        #endregion

        #region Debug Methods
        #if UNITY_EDITOR
        [ContextMenu("Start Basic Tower Placement")]
        private void DebugStartBasicTowerPlacement()
        {
            StartTowerPlacement(DefenceGame.Tower.TowerType.Basic);
        }
        
        [ContextMenu("Cancel Tower Placement")]
        private void DebugCancelTowerPlacement()
        {
            CancelTowerPlacement();
        }
        
        [ContextMenu("Print Input Status")]
        private void DebugPrintInputStatus()
        {
            Debug.Log($"IsPlacingTower: {isPlacingTower}, SelectedTowerType: {selectedTowerType}");
        }
        #endif
        #endregion
    }
} 