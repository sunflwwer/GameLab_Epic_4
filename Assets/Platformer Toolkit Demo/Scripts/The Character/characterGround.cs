using UnityEngine;



namespace GMTK.PlatformerToolkit {
    //This script is used by both movement and jump to detect when the character is touching the ground

    public class characterGround : MonoBehaviour {
        private bool onGround;

        [Header("Collider Settings")]
        [SerializeField][Tooltip("Length of the ground-checking collider")] private float groundLength = 0.95f;
        [SerializeField][Tooltip("Distance between the ground-checking colliders")] private Vector3 colliderOffset;
        [SerializeField][Tooltip("Radius for circle cast ground detection")] private float groundCheckRadius = 0.2f;

        [Header("Layer Masks")]
        [SerializeField][Tooltip("Which layers are read as the ground")] private LayerMask groundLayer;


        private void Update() {
            //Determine if the player is stood on objects on the ground layer
            //Using multiple detection methods for better accuracy
            
            // 기본 Raycast (좌우)
            bool raycastLeft = Physics2D.Raycast(transform.position + colliderOffset, Vector2.down, groundLength, groundLayer);
            bool raycastRight = Physics2D.Raycast(transform.position - colliderOffset, Vector2.down, groundLength, groundLayer);
            
            // 중앙 Raycast (끼었을 때 감지용)
            bool raycastCenter = Physics2D.Raycast(transform.position, Vector2.down, groundLength, groundLayer);
            
            // CircleCast (더 넓은 범위 감지 - 끼었을 때 특히 유용)
            RaycastHit2D circleCast = Physics2D.CircleCast(transform.position, groundCheckRadius, Vector2.down, groundLength, groundLayer);
            
            // 하나라도 감지되면 Ground로 판정
            onGround = raycastLeft || raycastRight || raycastCenter || circleCast.collider != null;
        }

        private void OnDrawGizmos() {
            //Draw the ground colliders on screen for debug purposes
            if (onGround) { Gizmos.color = Color.green; } else { Gizmos.color = Color.red; }
            
            // 좌우 Raycast
            Gizmos.DrawLine(transform.position + colliderOffset, transform.position + colliderOffset + Vector3.down * groundLength);
            Gizmos.DrawLine(transform.position - colliderOffset, transform.position - colliderOffset + Vector3.down * groundLength);
            
            // 중앙 Raycast (끼었을 때 감지용)
            Gizmos.DrawLine(transform.position, transform.position + Vector3.down * groundLength);
            
            // CircleCast 시각화 (시작 지점과 끝 지점)
            Gizmos.DrawWireSphere(transform.position, groundCheckRadius);
            Gizmos.DrawWireSphere(transform.position + Vector3.down * groundLength, groundCheckRadius);
        }

        //Send ground detection to other scripts
        public bool GetOnGround() { return onGround; }
    }
}