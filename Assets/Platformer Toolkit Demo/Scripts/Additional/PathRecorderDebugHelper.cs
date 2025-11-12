using UnityEngine;

namespace GMTK.PlatformerToolkit {
    /// <summary>
    /// PathRecorder 디버그 및 테스트용 헬퍼 스크립트
    /// </summary>
    public class PathRecorderDebugHelper : MonoBehaviour {
        [SerializeField] private PathRecorder pathRecorder;
        [SerializeField] private bool showGizmos = true;
        [SerializeField] private float gizmoRadius = 0.3f;
        
        private void Awake() {
            if (pathRecorder == null) {
                pathRecorder = GetComponent<PathRecorder>();
            }
        }

        private void OnGUI() {
            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.Box("PathRecorder 디버그", GUILayout.Width(290));
            
            if (pathRecorder != null) {
                var paths = pathRecorder.GetAllRecordedPaths();
                GUILayout.Label($"기록된 경로 수: {paths.Count}");
                
                int totalPoints = 0;
                foreach (var path in paths) {
                    totalPoints += path.Count;
                }
                GUILayout.Label($"총 점 개수: {totalPoints}");
                
                GUILayout.Space(10);
                
                if (GUILayout.Button("🎬 기록 시작", GUILayout.Height(30))) {
                    pathRecorder.StartRecording();
                }
                
                if (GUILayout.Button("⏹️ 기록 정지", GUILayout.Height(30))) {
                    pathRecorder.StopRecording();
                }
                
                if (GUILayout.Button("↩️ 마지막 라인 삭제", GUILayout.Height(30))) {
                    pathRecorder.UndoLastLine();
                }
                
                if (GUILayout.Button("🗑️ 라인 삭제", GUILayout.Height(30))) {
                    pathRecorder.ClearAllLines();
                }
                
                if (GUILayout.Button("🗑️ 모든 플랫폼 삭제", GUILayout.Height(30))) {
                    pathRecorder.ClearAllPlatforms();
                }
                
                GUILayout.Space(10);
                GUILayout.Label("💡 우클릭: 현재 플랫폼 토글");
                GUILayout.Label("💡 새 경로 시작 시 이전 플랫폼 유지");
            } else {
                GUILayout.Label("PathRecorder를 찾을 수 없습니다!");
            }
            
            GUILayout.EndArea();
        }

        private void OnDrawGizmos() {
            if (!showGizmos || pathRecorder == null) return;
            
            var paths = pathRecorder.GetAllRecordedPaths();
            
            // 각 경로를 Gizmo로 표시
            for (int i = 0; i < paths.Count; i++) {
                var path = paths[i];
                
                // 색상을 다르게
                Gizmos.color = i == paths.Count - 1 ? Color.green : Color.cyan;
                
                // 점들을 구체로 표시
                foreach (var point in path) {
                    Gizmos.DrawSphere(point, gizmoRadius);
                }
                
                // 선으로 연결
                for (int j = 0; j < path.Count - 1; j++) {
                    Gizmos.DrawLine(path[j], path[j + 1]);
                }
            }
        }
    }
}

