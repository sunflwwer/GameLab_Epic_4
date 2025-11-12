using UnityEngine;

namespace GMTK.PlatformerToolkit {
    /// <summary>
    /// 플레이어가 밟으면 죽는 타일 (가시, 용암 등)
    /// </summary>
    public class DeathTile : MonoBehaviour {
        [Header("설정")]
        [SerializeField] private bool showDebugLogs = true; // 디버그 로그 표시
        
        [Header("정보")]
        [SerializeField] private string infoMessage = "characterHurt가 자동으로 리스폰 처리합니다. 플레이어는 파괴되지 않습니다.";

        private void OnCollisionEnter2D(Collision2D collision) {
            // 플레이어와 충돌했는지 확인
            if (IsPlayer(collision.gameObject)) {
                KillPlayer(collision.gameObject);
            }
        }

        private void OnTriggerEnter2D(Collider2D other) {
            // Trigger 모드일 때도 지원
            if (IsPlayer(other.gameObject)) {
                KillPlayer(other.gameObject);
            }
        }

        private void KillPlayer(GameObject player) {
            // 디버그 로그
            Debug.Log("💀 플레이어 죽음");
            
            if (showDebugLogs) {
                Debug.Log($"⚠️ 플레이어가 데스 타일에 닿았습니다!");
                Debug.Log($"   - 플레이어: {player.name}");
                Debug.Log($"   - 데스 타일: {gameObject.name}");
                Debug.Log($"   - 위치: {player.transform.position}");
            }

            // characterHurt 스크립트로 리스폰 처리 (플레이어는 파괴되지 않음)
            characterHurt hurtScript = player.GetComponent<characterHurt>();
            if (hurtScript != null) {
                // 기존 hurt 시스템 사용 - 사망 애니메이션, 효과, 리스폰 모두 처리됨
                hurtScript.hurtRoutine();
                if (showDebugLogs) {
                    Debug.Log($"   → characterHurt.hurtRoutine() 호출됨 (리스폰 자동 처리)");
                }
            } else {
                Debug.LogWarning($"⚠️ characterHurt 컴포넌트를 찾을 수 없습니다! 플레이어: {player.name}");
            }

            // 추가 효과 (선택사항)
            PlayDeathEffects(player.transform.position);
        }

        private bool IsPlayer(GameObject obj) {
            // 여러 방법으로 플레이어 확인
            
            // 1. characterMovement 컴포넌트로 확인
            if (obj.GetComponent<characterMovement>() != null) {
                return true;
            }
            
            // 2. 태그로 확인
            if (obj.CompareTag("Player")) {
                return true;
            }
            
            // 3. 레이어로 확인
            if (obj.layer == LayerMask.NameToLayer("Player")) {
                return true;
            }
            
            // 4. 이름으로 확인
            if (obj.name.Contains("Player") || obj.name.Contains("Kit")) {
                return true;
            }
            
            return false;
        }

        private void PlayDeathEffects(Vector3 position) {
            // 여기에 파티클이나 사운드 효과 추가 가능
            // 예: Instantiate(deathParticlePrefab, position, Quaternion.identity);
            
            if (showDebugLogs) {
                Debug.Log($"   💥 죽음 이펙트 재생 위치: {position}");
            }
        }

        private void OnDrawGizmos() {
            // Scene View에서 데스 타일 표시 (빨간색)
            Gizmos.color = new Color(1f, 0f, 0f, 0.5f); // 반투명 빨간색
            
            if (TryGetComponent<Collider2D>(out var col)) {
                Gizmos.DrawCube(col.bounds.center, col.bounds.size);
            } else {
                // Collider가 없으면 작은 큐브 표시
                Gizmos.DrawWireCube(transform.position, Vector3.one * 0.5f);
            }
            
            // X 표시 (위험 표시)
            Gizmos.color = Color.red;
            Vector3 center = transform.position;
            float size = 0.3f;
            Gizmos.DrawLine(center + new Vector3(-size, -size, 0), center + new Vector3(size, size, 0));
            Gizmos.DrawLine(center + new Vector3(-size, size, 0), center + new Vector3(size, -size, 0));
        }
    }
}

