using UnityEngine;
using UnityEngine.InputSystem;

namespace GMTK.PlatformerToolkit {
    /// <summary>
    /// 일방향 플랫폼 - 아래에서 위로 통과 가능, 위에서는 밟을 수 있음, S키로 떨어지기
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class OneWayPlatform : MonoBehaviour {
        [Header("설정")]
        [SerializeField] private float dropThroughDuration = 0.3f; // S키 눌렀을 때 무시할 시간
        [SerializeField] private LayerMask playerLayer; // 플레이어 레이어
        
        private Collider2D platformCollider;
        private bool canDropThrough = false;
        private float dropThroughTimer = 0f;

        private void Awake() {
            platformCollider = GetComponent<Collider2D>();
            
            if (platformCollider == null) {
                Debug.LogError("❌ Collider2D가 없습니다! BoxCollider2D 또는 TilemapCollider2D를 추가하세요.");
                return;
            }
            
            // PlatformEffector2D가 없으면 추가
            PlatformEffector2D effector = GetComponent<PlatformEffector2D>();
            if (effector == null) {
                effector = gameObject.AddComponent<PlatformEffector2D>();
                Debug.Log($"➕ PlatformEffector2D 추가됨: {gameObject.name}");
            }
            
            // PlatformEffector2D 설정 (강제)
            effector.useOneWay = true; // 일방향 활성화
            effector.surfaceArc = 180f; // 위쪽 180도만 충돌
            effector.sideArc = 0f; // 옆면 충돌 없음
            effector.useColliderMask = false; // 모든 레이어와 상호작용
            effector.rotationalOffset = 0f; // 회전 없음
            
            // Collider가 Effector를 사용하도록 설정 (강제)
            platformCollider.usedByEffector = true;
            
            // CompositeCollider2D인 경우 특별 처리
            if (platformCollider is CompositeCollider2D compositeCollider) {
                compositeCollider.geometryType = CompositeCollider2D.GeometryType.Polygons;
                Debug.Log($"📐 CompositeCollider2D 감지 - Polygons 타입으로 설정");
            }
            
            Debug.Log($"✅ OneWayPlatform 초기화 완료: {gameObject.name}");
            Debug.Log($"   - Collider Type: {platformCollider.GetType().Name}");
            Debug.Log($"   - UsedByEffector: {platformCollider.usedByEffector}");
            Debug.Log($"   - PlatformEffector2D 존재: {effector != null}");
            Debug.Log($"   - UseOneWay: {effector.useOneWay}");
            Debug.Log($"   - SurfaceArc: {effector.surfaceArc}");
            
            if (!platformCollider.usedByEffector) {
                Debug.LogError("⚠️ Collider의 'Used By Effector'가 체크되지 않았습니다! Inspector에서 체크해주세요!");
            }
        }

        private void Update() {
            // S키 + 아래 방향키로 떨어지기
            bool pressingDown = false;
            
            // Input System (New Input System)
            if (Keyboard.current != null) {
                pressingDown = Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed;
            }
            // Legacy Input (Old Input System)
            else {
                pressingDown = Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow);
            }
            
            if (pressingDown && canDropThrough) {
                Debug.Log($"⬇️ S키 감지 - 떨어지기 시작!");
                StartDropThrough();
            }
            
            // 떨어지기 타이머 처리
            if (dropThroughTimer > 0f) {
                dropThroughTimer -= Time.deltaTime;
                
                // 타이머 종료 시 충돌 복구
                if (dropThroughTimer <= 0f) {
                    platformCollider.enabled = true;
                    Debug.Log($"💚 플랫폼 충돌 복구: {gameObject.name}");
                }
            }
        }

        private void OnCollisionEnter2D(Collision2D collision) {
            // 플레이어가 충돌했는지 체크
            if (IsPlayer(collision.gameObject)) {
                Debug.Log($"🔵 플레이어 충돌: {collision.gameObject.name} → {gameObject.name}");
            }
        }

        private void OnCollisionStay2D(Collision2D collision) {
            // 플레이어가 위에 있는지 체크
            if (IsPlayer(collision.gameObject)) {
                // 충돌 접촉점을 기준으로 플레이어가 위에 있는지 확인
                bool playerIsAbove = false;
                
                if (collision.contactCount > 0) {
                    // 첫 번째 접촉점의 노멀 벡터로 판단 (위쪽이면 노멀이 아래를 향함)
                    Vector2 contactNormal = collision.GetContact(0).normal;
                    // 노멀이 위쪽(0, 1)을 향하면 플레이어가 위에 있음
                    playerIsAbove = contactNormal.y < -0.5f; // 아래쪽 노멀 (-1에 가까우면)
                    
                    if (playerIsAbove != canDropThrough) {
                        canDropThrough = playerIsAbove;
                        Debug.Log($"🔄 떨어지기 가능 상태: {canDropThrough} (노멀: {contactNormal}, 접촉점: {collision.contactCount})");
                    }
                } else {
                    // 접촉점이 없으면 위치로 판단
                    float heightDifference = collision.transform.position.y - transform.position.y;
                    playerIsAbove = heightDifference > 0.3f;
                    canDropThrough = playerIsAbove;
                }
            }
        }

        private void OnCollisionExit2D(Collision2D collision) {
            // 플레이어가 떠나면 떨어지기 불가
            if (IsPlayer(collision.gameObject)) {
                canDropThrough = false;
                Debug.Log($"🔴 플레이어 떠남: {collision.gameObject.name}");
            }
        }

        private void StartDropThrough() {
            // 플랫폼 충돌 비활성화
            platformCollider.enabled = false;
            dropThroughTimer = dropThroughDuration;
            canDropThrough = false;
            
            Debug.Log($"⬇️ 플랫폼 통과 시작: {gameObject.name}, {dropThroughDuration}초간");
        }

        private bool IsPlayer(GameObject obj) {
            // characterMovement 컴포넌트로 플레이어 확인
            if (obj.GetComponent<characterMovement>() != null) {
                return true;
            }
            
            // 태그로 확인
            if (obj.CompareTag("Player")) {
                return true;
            }
            
            // 레이어로 확인
            if (playerLayer != 0 && ((1 << obj.layer) & playerLayer) != 0) {
                return true;
            }
            
            return false;
        }

        private void OnDrawGizmos() {
            // Scene View에서 일방향 플랫폼 표시
            Gizmos.color = Color.green;
            
            // 플랫폼 범위 표시
            if (TryGetComponent<Collider2D>(out var col)) {
                Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
            }
            
            // 위쪽 방향 표시 (통과 불가 방향)
            Gizmos.color = Color.red;
            Vector3 center = transform.position;
            Vector3 upArrow = center + Vector3.up * 0.5f;
            Gizmos.DrawLine(center, upArrow);
            Gizmos.DrawLine(upArrow, upArrow + Vector3.left * 0.2f);
            Gizmos.DrawLine(upArrow, upArrow + Vector3.right * 0.2f);
        }
    }
}

