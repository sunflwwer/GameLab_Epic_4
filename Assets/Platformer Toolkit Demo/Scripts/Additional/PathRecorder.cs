using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GMTK.PlatformerToolkit {

    /// <summary>
    /// 플레이어의 이동 경로를 기록하고 라인으로 그리는 스크립트
    /// </summary>
    public class PathRecorder : MonoBehaviour {
        [Header("대상 설정")]
        [SerializeField] private Transform playerTransform; // 추적할 플레이어
        
        [Header("라인 설정")]
        [SerializeField] private GameObject lineRendererPrefab; // LineRenderer가 있는 프리팹
        [SerializeField] private Color lineColor = new Color(0f, 0.5f, 1f, 1f); // 파란색 (기록 중 라인)
        [SerializeField] private float lineWidth = 0.2f; // 0.1에서 0.2로 증가
        [SerializeField] private Material lineMaterial;
        
        [Header("기록 설정")]
        [SerializeField] private float minDistanceBetweenPoints = 0.2f; // 0.1에서 0.2로 증가 (더 자주 기록)
        [SerializeField] private bool recordOnStart; // 시작 시 자동으로 기록 시작 (홀드 방식에서는 보통 false)
        
        [Header("고스트 라인 설정 (우클릭)")]
        [SerializeField] private Color previewLineColor = new Color(1f, 1f, 1f, 0.5f); // 미리보기 색상 (반투명 흰색)
        [SerializeField] private Color ghostLineColor = Color.yellow; // 실제 플랫폼 색상 (노란색)
        [SerializeField] private float ghostLineWidth = 0.3f; // 더 굵게 (충돌 감지 용이)
        [SerializeField] private float ghostOffsetX = 2f; // 플레이어 앞 X 오프셋
        [SerializeField] private bool addPhysicsCollider = true; // 물리 충돌 추가
        [SerializeField] private float colliderWidth = 0.15f; // Collider 두께 (끼임 방지 - 더 얇게)
        [SerializeField] private string ghostLineLayer = "Ground"; // 고스트 라인이 속할 레이어 이름
        
        [Header("디버그")]
        [SerializeField] private bool showDebugInfo;

        private bool isRecording;
        private LineRenderer currentLineRenderer;
        private List<Vector3> currentPathPoints = new List<Vector3>();
        private List<GameObject> recordedLines = new List<GameObject>(); // 기록된 모든 라인들
        
        // 고스트 라인 관련
        private GameObject previewPlatform; // 미리보기 플랫폼 (우클릭 누르는 동안)
        private GameObject currentGhostPlatform; // 현재 경로의 실제 플랫폼 (우클릭 뗐을 때)
        private bool isPreviewVisible; // 미리보기 표시 여부
        private bool isCurrentGhostVisible; // 현재 고스트 표시 여부
        private List<GameObject> permanentPlatforms = new List<GameObject>(); // 이전 경로들의 영구 플랫폼들
        private List<Vector3> lastRecordedPath = new List<Vector3>(); // 마지막으로 기록된 경로
        private bool lastRecordedFacingRight; // 경로 녹화 시 플레이어가 오른쪽을 보고 있었는지
        
        // 클릭 카운트 방식
        private float lastRightClickTime; // 마지막 우클릭 시간
        private const float doubleClickThreshold = 0.3f; // 더블 클릭 인식 시간 (초)
        private int rightClickCount; // 우클릭 카운트 (1 = 싱글, 2 = 더블)
        
        // 더블클릭 되돌리기용 상태
        private GameObject pendingCreatedPlatform; // 방금 만든(확정 직전) 플랫폼
        private float pendingCreatedAt = -999f;
        private bool hasPendingPlatform = false;
        
        private void Awake() {
            // 플레이어가 설정되지 않았으면 찾기 시도
            if (playerTransform == null) {
                // 먼저 자기 자신이 플레이어인지 확인
                if (GetComponent<characterMovement>() != null) {
                    playerTransform = transform;
                    Debug.Log($"✅ PathRecorder: 자기 자신을 플레이어로 설정 ({gameObject.name})");
                }
                // Player 태그로 찾기
                else {
                    GameObject player = GameObject.FindGameObjectWithTag("Player");
                    if (player != null) {
                        playerTransform = player.transform;
                        Debug.Log($"✅ PathRecorder: Player 태그로 플레이어 찾음 ({player.name})");
                    }
                    // characterMovement 컴포넌트로 찾기
                    else {
                        characterMovement movement = FindFirstObjectByType<characterMovement>();
                        if (movement != null) {
                            playerTransform = movement.transform;
                            Debug.Log($"✅ PathRecorder: characterMovement로 플레이어 찾음 ({movement.gameObject.name})");
                        }
                        // 마지막 수단: 자기 자신
                        else {
                            playerTransform = transform;
                            Debug.LogWarning($"⚠️ PathRecorder: 플레이어를 찾지 못해 자기 자신을 추적합니다 ({gameObject.name})");
                        }
                    }
                }
            } else {
                Debug.Log($"✅ PathRecorder: Player Transform 설정됨 ({playerTransform.name})");
            }
        }

        private void Start() {
            Debug.Log($"🎮 PathRecorder Start - recordOnStart: {recordOnStart}, playerTransform: {playerTransform?.name ?? "NULL"}");
            
            if (recordOnStart) {
                StartRecording();
            } else {
                Debug.Log("⏸️ 자동 기록이 꺼져있습니다. 좌클릭으로 시작하세요.");
            }
        }

        private void Update() {
            HandleInput();
            
            // 기록 중이면 플레이어 위치 추적
            if (isRecording && playerTransform != null) {
                RecordPlayerPosition();
            }
            
            // 미리보기 표시 중이면 플레이어 이동에 따라 업데이트
            if (isPreviewVisible && playerTransform != null) {
                UpdatePreview();
            }
        }

        private void HandleInput() {
            // Mouse가 없으면 리턴
            if (Mouse.current == null) return;

            // 좌클릭 홀드 방식 - 누르고 있는 동안만 녹화
            if (Mouse.current.leftButton.wasPressedThisFrame) {
                // 좌클릭 누르기 시작 - 기록 시작
                StartRecording();
            }
            
            if (Mouse.current.leftButton.wasReleasedThisFrame) {
                // 좌클릭 떼기 - 기록 정지
                if (isRecording) {
                    StopRecording();
                }
            }

            // === 우클릭 처리 (기록 중이 아닐 때만) ===
            if (!isRecording)
            {
                if (Mouse.current.rightButton.wasPressedThisFrame)
                {
                    float dt = Time.time - lastRightClickTime;

                    // 더블 클릭: "바로 직전에 생성한 것 + 이전 영구 플랫폼" 모두 삭제
                    if (dt <= doubleClickThreshold && dt > 0f)
                    {
                        // 미리보기 숨김
                        if (previewPlatform != null) previewPlatform.SetActive(false);
                        isPreviewVisible = false;

                        // 1) 방금 만든 임시(팬딩) 플랫폼 삭제
                        if (hasPendingPlatform && pendingCreatedPlatform != null)
                        {
                            Destroy(pendingCreatedPlatform);
                        }
                        pendingCreatedPlatform = null;
                        hasPendingPlatform = false;

                        // 2) 이전에 배치한 영구 플랫폼 삭제(최신 것부터 1개)
                        if (permanentPlatforms.Count > 0)
                        {
                            int lastIndex = permanentPlatforms.Count - 1;
                            var last = permanentPlatforms[lastIndex];
                            if (last != null) Destroy(last);
                            permanentPlatforms.RemoveAt(lastIndex);
                        }

                        // 타이머 초기화
                        rightClickCount = 0;
                        lastRightClickTime = 0f;
                        return;
                    }
                    else
                    {
                        // 첫 클릭 시작: 미리보기 표시
                        rightClickCount = 1;
                        lastRightClickTime = Time.time;
                        if (!isPreviewVisible) ShowPreview();
                    }
                }

                // 누르는 동안 미리보기 갱신
                if (rightClickCount == 1 && Mouse.current.rightButton.isPressed)
                {
                    if (isPreviewVisible) UpdatePreview();
                }

                // 우클릭 릴리즈: 즉시 생성(대기 없음)
                if (rightClickCount == 1 && Mouse.current.rightButton.wasReleasedThisFrame)
                {
                    // 기존에 표시되던 현재 고스트를 영구화하고 새로운 것을 만드는 기존 로직을 사용하되,
                    // 방금 만든 것을 pending으로 기억한다.
                    if (isPreviewVisible)
                    {
                        // 미리보기에서 즉시 생성 (새 고스트 한 개를 만든다)
                        if (previewPlatform != null) previewPlatform.SetActive(false);
                        isPreviewVisible = false;

                        // 현재 고스트가 있으면 먼저 영구화
                        if (currentGhostPlatform != null && isCurrentGhostVisible)
                        {
                            permanentPlatforms.Add(currentGhostPlatform);
                            currentGhostPlatform.name = "PermanentPlatform_" + permanentPlatforms.Count;
                            currentGhostPlatform = null;
                            isCurrentGhostVisible = false;
                        }

                        // 새로운 플랫폼 생성 (플레이어 앞에)
                        // ShowCurrentGhost 내부에서 currentGhostPlatform을 만든다.
                        ShowCurrentGhost();

                        // 방금 만든 것을 pending으로 기록
                        pendingCreatedPlatform = currentGhostPlatform;
                        hasPendingPlatform = pendingCreatedPlatform != null;
                        pendingCreatedAt = Time.time;

                        // 생성된 것은 "현재 고스트" 상태로 켜져있다.
                        // 다음 더블클릭이 오면 이것도 같이 제거된다.
                    }

                    rightClickCount = 0;
                    // 마지막 클릭 시각 업데이트(싱글 뒤 곧바로 더블을 받을 수 있게)
                    lastRightClickTime = Time.time;
                }
            }
            else
            {
                if (Mouse.current.rightButton.wasPressedThisFrame && showDebugInfo)
                    Debug.LogWarning("기록 중에는 플랫폼을 생성할 수 없습니다. 좌클릭을 떼서 기록을 먼저 정지하세요.");
            }

        }
        
        /// <summary>
        /// 플레이어 경로 기록 시작
        /// </summary>
        public void StartRecording() {
            if (isRecording) {
                Debug.LogWarning("이미 기록 중입니다!");
                return;
            }

            // 새 경로 시작 시, 현재 고스트가 표시되어 있으면 영구 플랫폼으로 전환
            if (currentGhostPlatform != null && isCurrentGhostVisible) {
                permanentPlatforms.Add(currentGhostPlatform);
                currentGhostPlatform.name = "PermanentPlatform_" + permanentPlatforms.Count;
                
                if (showDebugInfo) {
                    Debug.Log($"🔒 현재 고스트를 영구 플랫폼으로 전환: {currentGhostPlatform.name}");
                }
                
                currentGhostPlatform = null;
                isCurrentGhostVisible = false;
            }

            isRecording = true;
            currentPathPoints.Clear();

            // 새로운 라인 오브젝트 생성
            GameObject lineObject;
            
            if (lineRendererPrefab != null) {
                lineObject = Instantiate(lineRendererPrefab);
                Debug.Log($"LineRenderer Prefab 사용: {lineRendererPrefab.name}");
            } else {
                lineObject = new GameObject("PlayerPath");
                lineObject.AddComponent<LineRenderer>();
                Debug.Log("새 LineRenderer GameObject 생성");
            }

            currentLineRenderer = lineObject.GetComponent<LineRenderer>();
            
            // 라인 렌더러 설정
            SetupLineRenderer(currentLineRenderer);
            
            recordedLines.Add(lineObject);

            // 시작 위치를 첫 번째 점으로 추가
            if (playerTransform != null) {
                currentPathPoints.Add(playerTransform.position);
                UpdateLineRenderer();
                Debug.Log($"✅ 경로 기록 시작! 플레이어 위치: {playerTransform.position}, LineRenderer: {lineObject.name}");
            } else {
                Debug.LogError("❌ Player Transform이 null입니다!");
            }
        }

        private void SetupLineRenderer(LineRenderer lineRenderer) {
            // 라인 두께 설정
            lineRenderer.startWidth = lineWidth;
            lineRenderer.endWidth = lineWidth;
            
            // 라인 색상 설정
            lineRenderer.startColor = lineColor;
            lineRenderer.endColor = lineColor;
            
            // Material 설정
            if (lineMaterial != null) {
                lineRenderer.material = lineMaterial;
            } else {
                // 기본 머티리얼 생성 - 여러 쉐이더 시도
                Material mat;
                
                // 2D 쉐이더 시도
                if (Shader.Find("Sprites/Default") != null) {
                    mat = new Material(Shader.Find("Sprites/Default"));
                }
                // Unlit 쉐이더 시도 (항상 보임)
                else if (Shader.Find("Unlit/Color") != null) {
                    mat = new Material(Shader.Find("Unlit/Color"));
                }
                // 기본 쉐이더
                else {
                    mat = new Material(Shader.Find("Standard"));
                }
                
                mat.color = lineColor;
                lineRenderer.material = mat;
            }
            
            // 기본 설정
            lineRenderer.positionCount = 0;
            lineRenderer.useWorldSpace = true;
            lineRenderer.alignment = LineAlignment.View; // 카메라를 향하도록
            
            // 2D 게임을 위한 설정
            lineRenderer.sortingLayerName = "Default";
            lineRenderer.sortingOrder = 100; // 매우 앞에 표시
            
            // 추가 렌더링 설정
            lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lineRenderer.receiveShadows = false;
            lineRenderer.allowOcclusionWhenDynamic = false;
            
            if (showDebugInfo) {
                Debug.Log($"LineRenderer 설정 완료 - Width: {lineWidth}, Color: {lineColor}, Material: {lineRenderer.material.name}");
            }
        }

        private void RecordPlayerPosition() {
            if (playerTransform == null) {
                if (showDebugInfo) {
                    Debug.LogError("Player Transform이 null입니다!");
                }
                return;
            }
            
            Vector3 playerPos = playerTransform.position;
            
            // 첫 번째 점이거나, 이전 점과의 거리가 최소 거리 이상일 때만 추가
            if (currentPathPoints.Count == 0 || 
                Vector3.Distance(playerPos, currentPathPoints[currentPathPoints.Count - 1]) >= minDistanceBetweenPoints) {
                
                currentPathPoints.Add(playerPos);
                UpdateLineRenderer();
                
                if (showDebugInfo) {
                    Debug.Log($"📍 위치 기록: {playerPos}, 총 점: {currentPathPoints.Count}, LineRenderer 점: {currentLineRenderer?.positionCount}");
                }
            }
        }

        private void UpdateLineRenderer() {
            if (currentLineRenderer == null) {
                if (showDebugInfo) {
                    Debug.LogError("❌ currentLineRenderer가 null입니다!");
                }
                return;
            }
            
            if (currentPathPoints.Count > 0) {
                currentLineRenderer.positionCount = currentPathPoints.Count;
                currentLineRenderer.SetPositions(currentPathPoints.ToArray());
                
                if (showDebugInfo && currentPathPoints.Count <= 3) { // 처음 몇 개만 로그
                    Debug.Log($"🎨 LineRenderer 업데이트: {currentPathPoints.Count}개 점, enabled: {currentLineRenderer.enabled}");
                }
            }
        }

        /// <summary>
        /// 플레이어 경로 기록 정지
        /// </summary>
        public void StopRecording() {
            isRecording = false;
            
            if (showDebugInfo) {
                Debug.Log($"플레이어 경로 기록 종료. 총 {currentPathPoints.Count}개의 점이 기록됨");
            }

            // 점이 너무 적으면 라인 제거
            if (currentPathPoints.Count < 2) {
                if (currentLineRenderer != null) {
                    recordedLines.Remove(currentLineRenderer.gameObject);
                    Destroy(currentLineRenderer.gameObject);
                }
            } else {
                // 마지막 경로를 저장 (고스트 라인용)
                lastRecordedPath = new List<Vector3>(currentPathPoints);
                
                // 녹화 시의 플레이어 방향 저장
                if (playerTransform != null) {
                    lastRecordedFacingRight = playerTransform.localScale.x > 0;
                }
                
                // 기록이 완료되면 노란색 라인 숨기기/삭제
                if (currentLineRenderer != null) {
                    recordedLines.Remove(currentLineRenderer.gameObject);
                    Destroy(currentLineRenderer.gameObject);
                    
                    if (showDebugInfo) {
                        string direction = lastRecordedFacingRight ? "오른쪽" : "왼쪽";
                        Debug.Log($"💾 마지막 경로 저장됨: {lastRecordedPath.Count}개 점 (녹화 방향: {direction}, 노란색 라인 제거됨)");
                    }
                } else {
                    if (showDebugInfo) {
                        Debug.Log($"💾 마지막 경로 저장됨: {lastRecordedPath.Count}개 점");
                    }
                }
            }

            currentLineRenderer = null;
        }


        /// <summary>
        /// 모든 기록된 라인 삭제
        /// </summary>
        public void ClearAllLines() {
            // 노란색 기록 라인 삭제
            foreach (GameObject line in recordedLines) {
                if (line != null) {
                    Destroy(line);
                }
            }
            recordedLines.Clear();
            
            if (showDebugInfo) {
                Debug.Log("모든 노란색 라인 삭제됨");
            }
        }

        /// <summary>
        /// 모든 플랫폼 삭제 (현재 고스트 + 영구 플랫폼)
        /// </summary>
        public void ClearAllPlatforms() {
            // 현재 고스트 삭제
            if (currentGhostPlatform != null) {
                Destroy(currentGhostPlatform);
                currentGhostPlatform = null;
                isCurrentGhostVisible = false;
            }
            
            // 모든 영구 플랫폼 삭제
            foreach (GameObject platform in permanentPlatforms) {
                if (platform != null) {
                    Destroy(platform);
                }
            }
            permanentPlatforms.Clear();
            
            if (showDebugInfo) {
                Debug.Log("🗑️ 모든 플랫폼 삭제됨");
            }
        }

        /// <summary>
        /// 마지막 플랫폼 삭제 (우클릭 더블 클릭)
        /// 더블 클릭은 항상 이전에 배치한 플랫폼만 삭제 (currentGhost는 무시)
        /// </summary>
        private void DeleteLastPlatform() {
            // 더블 클릭은 배치가 아닌 삭제 명령!
            // currentGhostPlatform은 무시하고 이전 플랫폼만 삭제
            
            // 1. 이전에 배치한 영구 플랫폼 삭제 (최신 것부터)
            if (permanentPlatforms.Count > 0) {
                int lastIndex = permanentPlatforms.Count - 1;
                GameObject lastPlatform = permanentPlatforms[lastIndex];
                
                if (lastPlatform != null) {
                    Debug.Log($"🗑️ 플랫폼 삭제: {lastPlatform.name} (더블 클릭)");
                    Destroy(lastPlatform);
                } else {
                    Debug.LogWarning("⚠️ 플랫폼이 이미 null입니다!");
                }
                
                permanentPlatforms.RemoveAt(lastIndex);
                return;
            }
            
            // 2. currentGhostPlatform이 있으면 삭제 (배치 후 아직 영구화 안 된 경우)
            if (currentGhostPlatform != null) {
                Debug.Log($"🗑️ 배치된 플랫폼 삭제 (더블 클릭) - {currentGhostPlatform.name}");
                Destroy(currentGhostPlatform);
                currentGhostPlatform = null;
                isCurrentGhostVisible = false;
                return;
            }
            
            // 3. 삭제할 플랫폼이 전혀 없음
            Debug.LogWarning("⚠️ 삭제할 플랫폼이 없습니다! (플랫폼을 먼저 배치하세요)");
        }

        /// <summary>
        /// 마지막 라인 삭제
        /// </summary>
        public void UndoLastLine() {
            if (recordedLines.Count > 0) {
                GameObject lastLine = recordedLines[recordedLines.Count - 1];
                recordedLines.RemoveAt(recordedLines.Count - 1);
                if (lastLine != null) {
                    Destroy(lastLine);
                }
                
                if (showDebugInfo) {
                    Debug.Log("마지막 라인 삭제됨");
                }
            }
        }

        /// <summary>
        /// 기록된 경로 점들 가져오기
        /// </summary>
        public List<List<Vector3>> GetAllRecordedPaths() {
            List<List<Vector3>> allPaths = new List<List<Vector3>>();
            
            foreach (GameObject lineObj in recordedLines) {
                if (lineObj != null) {
                    LineRenderer lr = lineObj.GetComponent<LineRenderer>();
                    if (lr != null && lr.positionCount > 0) {
                        List<Vector3> path = new List<Vector3>();
                        Vector3[] positions = new Vector3[lr.positionCount];
                        lr.GetPositions(positions);
                        path.AddRange(positions);
                        allPaths.Add(path);
                    }
                }
            }
            
            return allPaths;
        }

        /// <summary>
        /// 미리보기 표시 (우클릭 누르는 순간)
        /// </summary>
        private void ShowPreview() {
            if (lastRecordedPath == null || lastRecordedPath.Count < 2) {
                if (showDebugInfo) {
                    Debug.LogWarning("⚠️ 표시할 이전 경로가 없습니다!");
                }
                return;
            }

            if (playerTransform == null) {
                Debug.LogError("❌ Player Transform이 null입니다!");
                return;
            }

            // 미리보기 플랫폼 생성
            if (previewPlatform == null) {
                previewPlatform = new GameObject("PreviewPlatform");
                LineRenderer lr = previewPlatform.AddComponent<LineRenderer>();
                SetupPreviewLineRenderer(lr);
            }

            // 경로 계산 및 표시
            List<Vector3> previewPath = CalculateGhostPath();
            
            LineRenderer previewLineRenderer = previewPlatform.GetComponent<LineRenderer>();
            previewLineRenderer.positionCount = previewPath.Count;
            previewLineRenderer.SetPositions(previewPath.ToArray());
            
            previewPlatform.SetActive(true);
            isPreviewVisible = true;

            if (showDebugInfo) {
                Debug.Log($"👁️ 미리보기 표시: {previewPath.Count}개 점");
            }
        }

        /// <summary>
        /// 미리보기 업데이트 (플레이어 이동 시 실시간 갱신)
        /// </summary>
        private void UpdatePreview() {
            if (previewPlatform == null || !previewPlatform.activeSelf) return;
            
            // 경로 재계산
            List<Vector3> previewPath = CalculateGhostPath();
            
            // LineRenderer 업데이트
            LineRenderer previewLineRenderer = previewPlatform.GetComponent<LineRenderer>();
            if (previewLineRenderer != null) {
                previewLineRenderer.positionCount = previewPath.Count;
                previewLineRenderer.SetPositions(previewPath.ToArray());
            }
        }

        /// <summary>
        /// 미리보기에서 실제 플랫폼 생성 (우클릭 뗐을 때)
        /// </summary>
        private void CreatePlatformFromPreview() {
            // 미리보기 숨기기
            if (previewPlatform != null) {
                previewPlatform.SetActive(false);
            }
            isPreviewVisible = false;

            // 기존에 표시된 현재 고스트가 있으면 영구 플랫폼으로 전환
            if (currentGhostPlatform != null && isCurrentGhostVisible) {
                // 현재 고스트를 영구 플랫폼으로 승격
                permanentPlatforms.Add(currentGhostPlatform);
                currentGhostPlatform.name = "PermanentPlatform_" + permanentPlatforms.Count;
                
                if (showDebugInfo) {
                    Debug.Log($"🔒 현재 고스트를 영구 플랫폼으로 전환: {currentGhostPlatform.name}");
                }
                
                currentGhostPlatform = null;
                isCurrentGhostVisible = false;
            }

            // 항상 새로운 플랫폼 생성
            ShowCurrentGhost();
        }

        /// <summary>
        /// 현재 경로의 고스트 라인 토글 (우클릭)
        /// </summary>
        private void ToggleGhostLine() {
            if (isCurrentGhostVisible) {
                HideCurrentGhost();
            } else {
                ShowCurrentGhost();
            }
        }

        /// <summary>
        /// 현재 경로의 고스트 플랫폼 표시 (플레이어 앞에)
        /// </summary>
        private void ShowCurrentGhost() {
            if (lastRecordedPath == null || lastRecordedPath.Count < 2) {
                Debug.LogWarning("⚠️ 표시할 이전 경로가 없습니다!");
                return;
            }

            if (playerTransform == null) {
                Debug.LogError("❌ Player Transform이 null입니다!");
                return;
            }

            // 현재 고스트 플랫폼 오브젝트 생성
            if (currentGhostPlatform == null) {
                currentGhostPlatform = new GameObject("CurrentGhostPlatform");
                LineRenderer lineRenderer = currentGhostPlatform.AddComponent<LineRenderer>();
                SetupGhostLineRenderer(lineRenderer);
                
                // 레이어 설정 (Ground 레이어로 설정하여 플레이어가 밟을 수 있게)
                int layerIndex = LayerMask.NameToLayer(ghostLineLayer);
                if (layerIndex == -1) {
                    // Ground 레이어가 없으면 Default 사용
                    Debug.LogWarning($"⚠️ '{ghostLineLayer}' 레이어를 찾을 수 없습니다. Default 레이어를 사용합니다.");
                    Debug.LogWarning($"💡 Unity 상단 메뉴 > Edit > Project Settings > Tags and Layers에서 '{ghostLineLayer}' 레이어를 추가하세요!");
                    layerIndex = LayerMask.NameToLayer("Default");
                }
                currentGhostPlatform.layer = layerIndex;
                
                if (showDebugInfo) {
                    Debug.Log($"🏷️ 고스트 라인 레이어 설정: {LayerMask.LayerToName(layerIndex)} (Index: {layerIndex})");
                }
            }

            // 경로 계산
            List<Vector3> ghostPath = CalculateGhostPath();

            // 고스트 라인 렌더러 업데이트
            LineRenderer ghostLineRenderer = currentGhostPlatform.GetComponent<LineRenderer>();
            ghostLineRenderer.positionCount = ghostPath.Count;
            ghostLineRenderer.SetPositions(ghostPath.ToArray());
            
            // 물리 충돌 추가
            if (addPhysicsCollider) {
                AddPhysicsCollider(currentGhostPlatform, ghostPath);
            }
            
            currentGhostPlatform.SetActive(true);

            isCurrentGhostVisible = true;

            if (showDebugInfo) {
                string direction = playerTransform.localScale.x > 0 ? "오른쪽" : "왼쪽";
                Debug.Log($"👻 현재 고스트 플랫폼 표시: {ghostPath.Count}개 점, 방향: {direction}, 물리 충돌: {addPhysicsCollider}");
            }
        }

        /// <summary>
        /// 현재 고스트 플랫폼 숨기기
        /// </summary>
        private void HideCurrentGhost() {
            if (currentGhostPlatform != null) {
                currentGhostPlatform.SetActive(false);
            }

            isCurrentGhostVisible = false;

            if (showDebugInfo) {
                Debug.Log("👻 현재 고스트 플랫폼 숨김");
            }
        }

        /// <summary>
        /// 고스트 경로 계산 (공통 로직)
        /// </summary>
        private List<Vector3> CalculateGhostPath() {
            Vector3 playerPos = playerTransform.position;
            bool facingRight = playerTransform.localScale.x > 0;
            
            // 녹화 방향과 배치 방향이 다른지 확인
            bool shouldMirror = (lastRecordedFacingRight != facingRight);
            
            // 경로를 좌우 반전해야 하는 경우
            List<Vector3> pathToUse = new List<Vector3>();
            if (shouldMirror) {
                // 경로의 중심점 계산
                float minX = lastRecordedPath[0].x;
                float maxX = lastRecordedPath[0].x;
                foreach (Vector3 point in lastRecordedPath) {
                    if (point.x < minX) minX = point.x;
                    if (point.x > maxX) maxX = point.x;
                }
                float centerX = (minX + maxX) / 2f;
                
                // 중심점을 기준으로 X축 반전
                foreach (Vector3 point in lastRecordedPath) {
                    float distanceFromCenter = point.x - centerX;
                    Vector3 mirroredPoint = new Vector3(centerX - distanceFromCenter, point.y, point.z);
                    pathToUse.Add(mirroredPoint);
                }
                
                if (showDebugInfo) {
                    Debug.Log($"🔄 경로 좌우 반전 (녹화: {(lastRecordedFacingRight ? "→" : "←")}, 배치: {(facingRight ? "→" : "←")})");
                }
            } else {
                // 반전하지 않음
                pathToUse = new List<Vector3>(lastRecordedPath);
            }
            
            // 플레이어 방향에 따라 기준점 결정
            Vector3 referencePoint;
            
            if (facingRight) {
                // 오른쪽을 보고 있으면: 경로의 가장 왼쪽 점을 찾기
                referencePoint = pathToUse[0];
                foreach (Vector3 point in pathToUse) {
                    if (point.x < referencePoint.x) {
                        referencePoint = point;
                    }
                }
            } else {
                // 왼쪽을 보고 있으면: 경로의 가장 오른쪽 점을 찾기
                referencePoint = pathToUse[0];
                foreach (Vector3 point in pathToUse) {
                    if (point.x > referencePoint.x) {
                        referencePoint = point;
                    }
                }
            }
            
            // 기준점이 플레이어 앞에 위치하도록 오프셋 계산
            Vector3 offset = new Vector3(ghostOffsetX, 0, 0);
            if (!facingRight) {
                offset.x = -offset.x;
            }
            
            Vector3 ghostOffset = playerPos + offset - referencePoint;
            List<Vector3> ghostPath = new List<Vector3>();

            foreach (Vector3 point in pathToUse) {
                ghostPath.Add(point + ghostOffset);
            }
            
            return ghostPath;
        }

        /// <summary>
        /// 미리보기 라인 렌더러 설정
        /// </summary>
        private void SetupPreviewLineRenderer(LineRenderer lineRenderer) {
            // 라인 두께 설정
            lineRenderer.startWidth = ghostLineWidth;
            lineRenderer.endWidth = ghostLineWidth;
            
            // 미리보기 색상 설정 (반투명)
            lineRenderer.startColor = previewLineColor;
            lineRenderer.endColor = previewLineColor;
            
            // Material 설정
            Material mat;
            
            if (Shader.Find("Sprites/Default") != null) {
                mat = new Material(Shader.Find("Sprites/Default"));
            }
            else if (Shader.Find("Unlit/Color") != null) {
                mat = new Material(Shader.Find("Unlit/Color"));
            }
            else {
                mat = new Material(Shader.Find("Standard"));
            }
            
            mat.color = previewLineColor;
            lineRenderer.material = mat;
            
            // 기본 설정
            lineRenderer.positionCount = 0;
            lineRenderer.useWorldSpace = true;
            lineRenderer.alignment = LineAlignment.View;
            
            // 2D 게임을 위한 설정
            lineRenderer.sortingLayerName = "Default";
            lineRenderer.sortingOrder = 98; // 고스트보다 살짝 뒤
            
            // 추가 렌더링 설정
            lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lineRenderer.receiveShadows = false;
            lineRenderer.allowOcclusionWhenDynamic = false;
            
            if (showDebugInfo) {
                Debug.Log($"👁️ Preview LineRenderer 설정 완료 - Color: {previewLineColor}");
            }
        }

        /// <summary>
        /// 고스트 라인 렌더러 설정
        /// </summary>
        private void SetupGhostLineRenderer(LineRenderer lineRenderer) {
            // 라인 두께 설정
            lineRenderer.startWidth = ghostLineWidth;
            lineRenderer.endWidth = ghostLineWidth;
            
            // 라인 색상 설정 (반투명)
            lineRenderer.startColor = ghostLineColor;
            lineRenderer.endColor = ghostLineColor;
            
            // Material 설정
            Material mat;
            
            // 2D 쉐이더 시도
            if (Shader.Find("Sprites/Default") != null) {
                mat = new Material(Shader.Find("Sprites/Default"));
            }
            // Unlit 쉐이더 시도
            else if (Shader.Find("Unlit/Color") != null) {
                mat = new Material(Shader.Find("Unlit/Color"));
            }
            // 기본 쉐이더
            else {
                mat = new Material(Shader.Find("Standard"));
            }
            
            mat.color = ghostLineColor;
            lineRenderer.material = mat;
            
            // 기본 설정
            lineRenderer.positionCount = 0;
            lineRenderer.useWorldSpace = true;
            lineRenderer.alignment = LineAlignment.View;
            
            // 2D 게임을 위한 설정
            lineRenderer.sortingLayerName = "Default";
            lineRenderer.sortingOrder = 99; // 일반 라인보다 살짝 뒤
            
            // 추가 렌더링 설정
            lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lineRenderer.receiveShadows = false;
            lineRenderer.allowOcclusionWhenDynamic = false;
            
            if (showDebugInfo) {
                Debug.Log($"👻 Ghost LineRenderer 설정 완료 - Width: {ghostLineWidth}, Color: {ghostLineColor}");
            }
        }

        /// <summary>
        /// 고스트 라인에 물리 충돌 추가
        /// </summary>
        private void AddPhysicsCollider(GameObject ghostObject, List<Vector3> path) {
            // 기존 Collider 제거
            EdgeCollider2D existingCollider = ghostObject.GetComponent<EdgeCollider2D>();
            if (existingCollider != null) {
                Destroy(existingCollider);
            }

            // EdgeCollider2D 추가
            EdgeCollider2D edgeCollider = ghostObject.AddComponent<EdgeCollider2D>();
            
            // 경로 점들을 Vector2 배열로 변환
            Vector2[] colliderPoints = new Vector2[path.Count];
            for (int i = 0; i < path.Count; i++) {
                // 월드 좌표를 로컬 좌표로 변환
                colliderPoints[i] = new Vector2(path[i].x, path[i].y);
            }
            
            // EdgeCollider에 점들 설정
            edgeCollider.points = colliderPoints;
            
            // Collider 두께 설정
            edgeCollider.edgeRadius = colliderWidth;
            
            // Physics Material 추가 (마찰 제거하여 끼임 방지)
            PhysicsMaterial2D physicsMaterial = new PhysicsMaterial2D("PlatformMaterial");
            physicsMaterial.friction = 0.0f; // 마찰 없음 (끼였을 때 미끄러져 나올 수 있음)
            physicsMaterial.bounciness = 0.0f; // 튕기지 않음
            edgeCollider.sharedMaterial = physicsMaterial;
            
            // EdgeCollider 추가 설정
            edgeCollider.offset = Vector2.zero;
            
            // Rigidbody2D 추가 (Static으로 설정)
            Rigidbody2D rb = ghostObject.GetComponent<Rigidbody2D>();
            if (rb == null) {
                rb = ghostObject.AddComponent<Rigidbody2D>();
            }
            rb.bodyType = RigidbodyType2D.Static; // 움직이지 않는 정적 오브젝트
            rb.simulated = true; // 물리 시뮬레이션 활성화
            
            if (showDebugInfo) {
                Debug.Log($"💥 물리 충돌 추가됨: {colliderPoints.Length}개 점, Collider 두께: {colliderWidth}, 마찰: {physicsMaterial.friction}");
            }
        }

        private void OnDestroy() {
            // 미리보기 플랫폼 정리
            if (previewPlatform != null) {
                Destroy(previewPlatform);
            }
            
            // 현재 고스트 플랫폼 정리
            if (currentGhostPlatform != null) {
                Destroy(currentGhostPlatform);
            }
            
            // 모든 영구 플랫폼 정리
            foreach (GameObject platform in permanentPlatforms) {
                if (platform != null) {
                    Destroy(platform);
                }
            }
        }
    }
}
