using UnityEngine;
using UnityEditor;
using System.IO;

namespace GMTK.PlatformerToolkit.Editor {
    /// <summary>
    /// PathRecorder에 필요한 에셋(Prefab, Material)을 자동으로 생성하는 에디터 도구
    /// </summary>
    public class PathRecorderSetupTool : EditorWindow {
        private string materialPath = "Assets/Platformer Toolkit Demo/Resources/PathLineMaterial.mat";
        private string prefabPath = "Assets/Platformer Toolkit Demo/Resources/PathLinePrefab.prefab";
        
        private Color lineColor = Color.white;
        private float lineWidth = 0.1f;

        [MenuItem("Tools/PathRecorder/Setup Assets")]
        public static void ShowWindow() {
            PathRecorderSetupTool window = GetWindow<PathRecorderSetupTool>("PathRecorder Setup");
            window.minSize = new Vector2(400, 300);
            window.Show();
        }

        [MenuItem("Tools/PathRecorder/Quick Setup (Default Settings)")]
        public static void QuickSetup() {
            PathRecorderSetupTool tool = CreateInstance<PathRecorderSetupTool>();
            tool.CreateAllAssets();
            EditorUtility.DisplayDialog("완료", "PathRecorder 에셋이 생성되었습니다!\n\nResources 폴더를 확인하세요.", "확인");
        }

        private void OnGUI() {
            GUILayout.Label("PathRecorder 에셋 생성 도구", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "이 도구는 PathRecorder에 필요한 Material과 Prefab을 자동으로 생성합니다.\n" +
                "생성 위치: Assets/Platformer Toolkit Demo/Resources/",
                MessageType.Info
            );

            EditorGUILayout.Space(10);

            // 설정
            GUILayout.Label("라인 설정", EditorStyles.boldLabel);
            lineColor = EditorGUILayout.ColorField("라인 색상", lineColor);
            lineWidth = EditorGUILayout.Slider("라인 두께", lineWidth, 0.01f, 1f);

            EditorGUILayout.Space(10);

            // 경로 표시
            EditorGUILayout.LabelField("생성될 파일:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Material:", materialPath, EditorStyles.wordWrappedLabel);
            EditorGUILayout.LabelField("Prefab:", prefabPath, EditorStyles.wordWrappedLabel);

            EditorGUILayout.Space(20);

            // 생성 버튼
            if (GUILayout.Button("에셋 생성하기", GUILayout.Height(40))) {
                CreateAllAssets();
            }

            EditorGUILayout.Space(10);

            if (GUILayout.Button("기존 에셋 삭제", GUILayout.Height(30))) {
                DeleteExistingAssets();
            }
        }

        private void CreateAllAssets() {
            // Resources 폴더 확인/생성
            string resourcesFolder = "Assets/Platformer Toolkit Demo/Resources";
            if (!AssetDatabase.IsValidFolder(resourcesFolder)) {
                string[] folders = resourcesFolder.Split('/');
                string currentPath = folders[0];
                for (int i = 1; i < folders.Length; i++) {
                    string newPath = currentPath + "/" + folders[i];
                    if (!AssetDatabase.IsValidFolder(newPath)) {
                        AssetDatabase.CreateFolder(currentPath, folders[i]);
                    }
                    currentPath = newPath;
                }
            }

            // Material 생성
            CreateLineMaterial();

            // Prefab 생성
            CreateLinePrefab();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("✅ PathRecorder 에셋 생성 완료!");
            Debug.Log($"Material: {materialPath}");
            Debug.Log($"Prefab: {prefabPath}");
        }

        private void CreateLineMaterial() {
            // 기존 Material이 있으면 덮어쓰기
            Material existingMat = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            Material mat;

            if (existingMat != null) {
                mat = existingMat;
            } else {
                mat = new Material(Shader.Find("Sprites/Default"));
            }

            mat.color = lineColor;
            mat.SetColor("_Color", lineColor);

            // Rendering 설정
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            mat.SetInt("_ZWrite", 0);

            if (existingMat == null) {
                AssetDatabase.CreateAsset(mat, materialPath);
            } else {
                EditorUtility.SetDirty(mat);
            }

            Debug.Log($"✅ Material 생성/업데이트 완료: {materialPath}");
        }

        private void CreateLinePrefab() {
            // 임시 GameObject 생성
            GameObject tempObj = new GameObject("PathLinePrefab");
            LineRenderer lr = tempObj.AddComponent<LineRenderer>();

            // LineRenderer 설정
            lr.startWidth = lineWidth;
            lr.endWidth = lineWidth;
            lr.startColor = lineColor;
            lr.endColor = lineColor;
            lr.useWorldSpace = true;
            lr.alignment = LineAlignment.TransformZ;

            // Material 적용
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (mat != null) {
                lr.material = mat;
            }

            // Sorting Layer 설정 (2D용)
            lr.sortingOrder = 10;

            // 기본 점 2개 설정
            lr.positionCount = 2;
            lr.SetPosition(0, Vector3.zero);
            lr.SetPosition(1, Vector3.right);

            // Prefab 저장
            GameObject existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (existingPrefab != null) {
                PrefabUtility.SaveAsPrefabAsset(tempObj, prefabPath);
            } else {
                PrefabUtility.SaveAsPrefabAsset(tempObj, prefabPath);
            }

            // 임시 오브젝트 삭제
            DestroyImmediate(tempObj);

            Debug.Log($"✅ Prefab 생성/업데이트 완료: {prefabPath}");
        }

        private void DeleteExistingAssets() {
            if (EditorUtility.DisplayDialog(
                "에셋 삭제",
                "정말로 기존 PathRecorder 에셋을 삭제하시겠습니까?",
                "삭제",
                "취소")) {

                if (File.Exists(materialPath)) {
                    AssetDatabase.DeleteAsset(materialPath);
                    Debug.Log($"Material 삭제됨: {materialPath}");
                }

                if (File.Exists(prefabPath)) {
                    AssetDatabase.DeleteAsset(prefabPath);
                    Debug.Log($"Prefab 삭제됨: {prefabPath}");
                }

                AssetDatabase.Refresh();
                EditorUtility.DisplayDialog("완료", "에셋이 삭제되었습니다.", "확인");
            }
        }
    }
}

