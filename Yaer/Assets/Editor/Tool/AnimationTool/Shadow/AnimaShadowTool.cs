using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace EditorC.Tool.AnimationTool.Shadow
{
    public class AnimaShadowTool : EditorWindow
    {
        private AnimationClip[] selectedClips;
        private SpriteRenderer sourceObject;
        private SpriteRenderer targetObject;

        [MenuItem("Editor/Anima/Shadow/Anima Shadow Tool")]
        public static void ShowWindow()
        {
            GetWindow<AnimaShadowTool>("Anima Shadow Tool");
        }

        private void OnGUI()
        {
            GUILayout.Label("Select Animation Clip", EditorStyles.boldLabel);
            if (GUILayout.Button("选择动画文件"))
            {
                Object[] selection = Selection.objects;
                selectedClips = new AnimationClip[selection.Length];
                for (int i = 0; i < selection.Length; i++)
                {
                    var obj = selection[i];
                    if (obj is AnimationClip ac)
                    {
                        string assetPath = AssetDatabase.GetAssetPath(ac);
                        selectedClips[i] = AssetDatabase.LoadAssetAtPath<AnimationClip>(assetPath);
                    }
                }
            }

            // 选择动画文件
            //selectedClip = (AnimationClip)EditorGUILayout.ObjectField("Animation Clip", selectedClip, typeof(AnimationClip), false);

            // 选择源节点
            sourceObject = EditorGUILayout.ObjectField("Source Object", sourceObject, typeof(SpriteRenderer), true) as SpriteRenderer;

            // 选择目标节点
            targetObject = EditorGUILayout.ObjectField("Target Object", targetObject, typeof(SpriteRenderer), true) as SpriteRenderer;

            if (selectedClips != null && sourceObject != null && targetObject != null)
            {
                if (GUILayout.Button("Copy Sprite Keyframes"))
                {
                    foreach (var selectedClip in selectedClips)
                    {
                        CopySpriteKeyframes(selectedClip, sourceObject, targetObject);
                    }
                    AssetDatabase.Refresh();
                }
            }
        }

        private string GetPath(Transform source)
        {
            string root = source.root.name;
            string fullPath = source.name;
            
            while (source.parent != null)
            {
                source = source.parent;
                fullPath = source.name + "/" + fullPath;
            }
            
            return fullPath.Replace(root + "/", "");
        }

        private void CopySpriteKeyframes(AnimationClip clip, SpriteRenderer source, SpriteRenderer target)
        {
            // 获取源节点的 SpriteRenderer
            if (source == null)
            {
                Debug.LogError("Source GameObject does not have a SpriteRenderer.");
                return;
            }

            // 获取目标节点的 SpriteRenderer
            if (target == null)
            {
                Debug.LogError("Target GameObject does not have a SpriteRenderer.");
                return;
            }

            string sourcePath = GetPath(source.transform);
            string targetPath = GetPath(target.transform);

            var bindings = AnimationUtility.GetObjectReferenceCurveBindings(clip);
            foreach (var binding in bindings)
            {
                if (binding.path == sourcePath)
                {
                    var keyframes = AnimationUtility.GetObjectReferenceCurve(clip, binding);
                    // 创建新的目标曲线，设置绑定
                    EditorCurveBinding targetBinding = new EditorCurveBinding
                    {
                        path = targetPath,
                        type = typeof(SpriteRenderer),
                        propertyName = "m_Sprite"
                    };

                    // 创建一个 List 用来存放目标的 ObjectReferenceKeyframe
                    List<ObjectReferenceKeyframe> targetKeyframes = new List<ObjectReferenceKeyframe>();

                    // 将源曲线中的每个关键帧复制到目标 ObjectReferenceKeyframe 中
                    foreach (var keyframe in keyframes)
                    {
                        if (keyframe.value is UnityEngine.Sprite sprite)
                        {
                            // 创建 ObjectReferenceKeyframe，并将 sprite 赋值给 value
                            ObjectReferenceKeyframe targetKeyframe = new ObjectReferenceKeyframe
                            {
                                time = keyframe.time,
                                value = sprite // 将 Sprite 作为 keyframe 的值
                            };

                            targetKeyframes.Add(targetKeyframe);
                        }
                    }

                    // 将新的曲线写入到目标动画剪辑
                    AnimationUtility.SetObjectReferenceCurve(clip, targetBinding, targetKeyframes.ToArray());
                }
            }
        }
    }
}