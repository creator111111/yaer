using System.Linq;
using Game.GameRuntime.Entities.Component.Map;
using GameFramework.Editor.UI;
using UnityEditor;
using UnityEngine;

namespace EditorC.Tool.Map
{
    public class MapEditorWindow : EditorWindow
    {
        private bool isClear;
        private int initPointCount;

        private static MapEditorWindow window;
        private MapLimit script;

        public static void Open(MapLimit script)
        {
            window = GetWindow<MapEditorWindow>("MapEditorWindow");
            window.Show();

            window.script = script;
        }

        public static void FocusWindow()
        {
            window.Focus();
        }

        private void OnGUI()
        {
            EditorGUILayout.ObjectField("编辑中的地图脚本", script, typeof(MonoScript), false);
            
            EditorUI.DrawBoltLabel("点击Scene窗口进行绘制", Color.green);

            if (GUILayout.Button("清空"))
            {
                isClear = true;
                initPointCount = 0;
                script.EdgeCld.points = new Vector2[2] { new Vector2(0, 0), new Vector2(0, 0) };
            }

            if (GUILayout.Button("撤回"))
            {
                var points = script.EdgeCld.points;
                if (points.Length > 2)
                {
                    script.EdgeCld.points = points.Take(points.Length - 1).ToArray();
                }
            }

            if (GUILayout.Button("生成空气墙"))
            {
                GenerateAirWall();
            }
        }

        private void Update()
        {
            // 阻止焦点丢失
            Selection.activeGameObject = script.gameObject;
        }

        private void OnEnable()
        {
            // 注册 SceneView 事件回调
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private void OnDisable()
        {
            // 取消注册 SceneView 事件回调
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            Event e = Event.current;

            if (e.type == EventType.MouseDown && e.button == 0)
            {
                // 获取点击的世界坐标
                Vector3 worldPosition = HandleUtility.GUIPointToWorldRay(e.mousePosition).origin;
                worldPosition.z = 0; // 确保是 2D 平面

                AddPointToEdgeCollider(worldPosition);
                e.Use();
                EditorUtility.SetDirty(script.EdgeCld);
            }

            DrawColliderPoints();
        }

        // 向 EdgeCollider 添加一个点
        private void AddPointToEdgeCollider(Vector3 worldPosition)
        {
            Vector2 localPosition = script.EdgeCld.transform.InverseTransformPoint(worldPosition); // 转为局部坐标

            // 清空后点击先生成前两个点
            if (isClear)
            {
                Vector2[] points = script.EdgeCld.points;
                if (initPointCount == 0)
                {
                    points[0] = localPosition;
                    initPointCount++;
                    script.EdgeCld.points = points;
                }
                else if (initPointCount == 1)
                {
                    points[1] = localPosition;
                    initPointCount++;
                    script.EdgeCld.points = points;
                    isClear = false;
                    initPointCount = 0;
                }
            }
            else
            {
                Vector2[] points = script.EdgeCld.points;
                // 添加新点
                ArrayUtility.Add(ref points, localPosition);
                script.EdgeCld.points = points;
            }
        }

        // 在 Scene 窗口中绘制 EdgeCollider 的节点
        private void DrawColliderPoints()
        {
            Handles.color = Color.green;

            Vector2[] points = script.EdgeCld.points;
            for (int i = 0; i < points.Length; i++)
            {
                // 转换为世界坐标
                Vector3 worldPoint = script.EdgeCld.transform.TransformPoint(points[i]);
                Handles.DrawSolidDisc(worldPoint, Vector3.forward, 0.1f);

                // 显示索引
                Handles.Label(worldPoint, $"P{i}");
            }
        }

        private void GenerateAirWall()
        {
            // 清空
            script.PolygonCld.points = new Vector2[3];

            Vector2[] points = script.EdgeCld.points;
            int count = 0;

            for (int i = script.StartIndex; i < script.EndIndex; i++)
            {
                Vector2[] wallPoints = new Vector2[4];

                wallPoints[0] = points[i];
                wallPoints[1] = points[i + 1];
                wallPoints[2] = new Vector2(points[i + 1].x, script.TargetHeight);
                wallPoints[3] = new Vector2(points[i].x, script.TargetHeight);
                
                script.PolygonCld.pathCount = count + 1;
                script.PolygonCld.SetPath(count, wallPoints);
                count++;
            }
        }
    }
}