using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace EditorC.Tool.Sprite
{
    public class SpritePivotTool : EditorWindow
    {
        private bool isStart;
        private float normalOffset = 0.001f;
        private float largeOffset = 0.01f;
        private Vector2 pivotOffset = new Vector2(0.001f, 0.001f); // 调整 Pivot 的步长
        private Vector2 pivotOffsetBig = new Vector2(0.01f, 0.01f);
        private Texture2D selectedTexture;

        [MenuItem("Editor/SpritePivotTool")]
        public static void OpenWindow()
        {
            GetWindow<SpritePivotTool>("SpritePivotTool").Show();
        }

        private void OnFocus()
        {
            GetSelectedGameObjectSprite();
        }

        private void OnGUI()
        {
            GUILayout.Label("选择一个 Sprite 并用方向键调整 Pivot 点");

            var newNormalOffset = EditorGUILayout.FloatField("当前小步进偏移量", normalOffset);
            var newLargeOffset = EditorGUILayout.FloatField("当前大步进偏移量", largeOffset);
            if (newNormalOffset != normalOffset)
            {
                normalOffset = newNormalOffset;
                pivotOffset = new Vector2(normalOffset, normalOffset);
            }

            if (newLargeOffset != largeOffset)
            {
                largeOffset = newLargeOffset;
                pivotOffsetBig = new Vector2(largeOffset, largeOffset);
            }

            if (GUILayout.Button("获取选中 Sprite"))
            {
                GetSelectedSprite();
            }

            if (GUILayout.Button("获取GameObject中的Sprite"))
            {
                GetSelectedGameObjectSprite();
            }

            if (GUILayout.Button("保存退出"))
            {
                if (selectedTexture)
                {
                    SaveSpriteChanges();
                    isStart = false;
                    selectedTexture = null;
                }
            }

            if (selectedTexture != null && isStart)
            {
                EditorGUILayout.ObjectField("当前选择的 Texture: ", selectedTexture, typeof(Object), false);
            }

            Repaint();
            HandleInput();
        }

        private void GetSelectedGameObjectSprite()
        {
            Object selectedObject = Selection.activeObject;
            if (selectedObject is GameObject go)
            {
                var sr = go.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    selectedTexture = sr.sprite.texture;
                    isStart = true;
                }
                else
                {
                    Debug.LogError("请选中一个含有 SpriteRenderer 对象");
                }
            }
            else
            {
                Debug.LogError("请选中一个 Texture2D 对象");
            }
        }

        private void GetSelectedSprite()
        {
            Object selectedObject = Selection.activeObject;
            if (selectedObject is Texture2D)
            {
                selectedTexture = selectedObject as Texture2D;
                isStart = true;
            }
            else
            {
                Debug.LogError("请选中一个 Texture2D 对象");
            }
        }

        private void HandleInput()
        {
            if (selectedTexture == null) return;

            if (Event.current.type == EventType.KeyDown)
            {
                if (Event.current.keyCode == KeyCode.UpArrow)
                {
                    if (Event.current.control == false)
                    {
                        ModifyPivot(Vector2.down * pivotOffset.y);
                    }
                    else
                    {
                        ModifyPivot(Vector2.down * pivotOffsetBig.y);
                    }
                }
                else if (Event.current.keyCode == KeyCode.DownArrow)
                {
                    if (Event.current.control == false)
                    {
                        ModifyPivot(Vector2.up * pivotOffset.y);
                    }
                    else
                    {
                        ModifyPivot(Vector2.up * pivotOffsetBig.y);
                    }
                }
                else if (Event.current.keyCode == KeyCode.LeftArrow)
                {
                    if (!Event.current.control)
                    {
                        ModifyPivot(Vector2.right * pivotOffset.x);
                    }
                    else
                    {
                        ModifyPivot(Vector2.right * pivotOffsetBig.x);
                    }
                }
                else if (Event.current.keyCode == KeyCode.RightArrow)
                {
                    if (!Event.current.control)
                    {
                        ModifyPivot(Vector2.left * pivotOffset.x);
                    }
                    else
                    {
                        ModifyPivot(Vector2.left * pivotOffsetBig.x);
                    }
                }

                if (Event.current.keyCode == KeyCode.S)
                {
                    SaveSpriteChanges();
                }
            }
        }

        private void ModifyPivot(Vector2 direction)
        {
            string assetPath = AssetDatabase.GetAssetPath(selectedTexture);
            TextureImporter textureImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;

            if (textureImporter == null)
            {
                Debug.LogError("TextureImporter 未找到");
                return;
            }

            // 获取当前 Pivot
            Vector2 newPivot = textureImporter.spritePivot + direction;
            // newPivot.x = Mathf.Clamp(newPivot.x, 0f, 1f);
            // newPivot.y = Mathf.Clamp(newPivot.y, 0f, 1f);

            // 设置新的 Pivot
            textureImporter.spritePivot = newPivot;

            Debug.Log($"Pivot 调整到: ({newPivot.x}, {newPivot.y})");
        }

        private void SaveSpriteChanges()
        {
            string assetPath = AssetDatabase.GetAssetPath(selectedTexture);
            TextureImporter textureImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;

            if (textureImporter == null)
            {
                Debug.LogError("TextureImporter 未找到");
                return;
            }

            // 保留有效数字
            textureImporter.spritePivot = new Vector2(RoundToSignificantFigures(textureImporter.spritePivot.x, 4), RoundToSignificantFigures(textureImporter.spritePivot.y, 4));

            textureImporter.SaveAndReimport();
            AssetDatabase.Refresh();
        }

        public float RoundToSignificantFigures(float num, int significantFigures)
        {
            if (num == 0)
                return 0;

            // 计算数量级（10的幂次方）
            double scale = Math.Pow(10, Math.Floor(Math.Log10(Math.Abs(num))) + 1 - significantFigures);

            // 调整数值范围并四舍五入
            return (float)(Math.Round(num / scale) * scale);
        }
    }
}