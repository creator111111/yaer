using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace EditorC.Tool.Map
{
    public class CameraAreaEditor: EditorWindow
    {
        private GameObject cameraAreaObj;
        private Transform mapLeft;
        private Transform mapRight;
        private float mapHeight;
        
        [MenuItem("GameObject/Editor/CameraAreaEditor", false, 1)]
        private static void OpenWindow()
        {
            EditorWindow window = GetWindow<CameraAreaEditor>("CameraAreaEditor");
            window.Show();
        }

        private void OnGUI()
        {
            cameraAreaObj = EditorGUILayout.ObjectField("CameraAreaObj", cameraAreaObj, typeof(GameObject), true) as GameObject;
            mapLeft = EditorGUILayout.ObjectField("MapLeft", mapLeft, typeof(Transform), true) as Transform;
            mapRight = EditorGUILayout.ObjectField("MapRight", mapRight, typeof(Transform), true) as Transform;
            mapHeight = EditorGUILayout.FloatField("MapHeight", mapHeight);
            
            if (GUILayout.Button("生成"))
            {
                var cld = cameraAreaObj.GetComponent<PolygonCollider2D>();
                if (cld == null)
                {
                    cld = cameraAreaObj.AddComponent<PolygonCollider2D>();
                }
                
                cld.SetPath(0, new Vector2[]
                {
                    new Vector2() {x = mapLeft.position.x, y = mapHeight/2},
                    new Vector2() {x = mapRight.position.x, y = mapHeight/2},
                    new Vector2() {x = mapRight.position.x, y = -mapHeight/2},
                    new Vector2() {x = mapLeft.position.x, y = -mapHeight/2},
                });
                
                // 标记当前场景为已修改状态
                EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            }
        }
    }
}