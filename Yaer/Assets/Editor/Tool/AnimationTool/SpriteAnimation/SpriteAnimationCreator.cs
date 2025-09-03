using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class SpriteAnimationCreator : EditorWindow
{
    // 用于新添加帧时的默认间隔时间
    private float defaultInterval = 0.1f;
    // 采样率，默认设置为 100
    private float sampleRate = 100f;
    // 目标子对象路径（即 AnimationCurve 的绑定路径）
    private string targetChildPath = "";

    // 封装每帧数据：图片和该帧的间隔时间
    [System.Serializable]
    public class FrameData
    {
        public Texture2D texture;
        public float interval = 0.1f;
    }

    // 存储所有帧数据
    private List<FrameData> frameDatas = new List<FrameData>();

    // 拖拽的目标 AnimationClip（anima）
    private AnimationClip targetClip = null;

    [MenuItem("Editor/Anima/Sprite Animation Creator")]
    public static void ShowWindow()
    {
        GetWindow<SpriteAnimationCreator>("Sprite 动画生成器");
    }

    private void OnGUI()
    {
        GUILayout.Label("创建 Sprite 动画", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // 全局默认间隔输入（用于新添加帧时赋值）
        defaultInterval = EditorGUILayout.FloatField("默认帧间隔 (秒):", defaultInterval);
        // 采样率输入
        sampleRate = EditorGUILayout.FloatField("采样率 (默认100):", sampleRate);
        // 目标子对象路径输入
        targetChildPath = EditorGUILayout.TextField("目标子对象路径:", targetChildPath);

        // 拖拽目标 AnimationClip
        targetClip = (AnimationClip)EditorGUILayout.ObjectField("目标 AnimationClip:", targetClip, typeof(AnimationClip), false);

        EditorGUILayout.Space();

        // 按钮：获取选中的 Texture2D 并添加为帧数据
        if (GUILayout.Button("获取选中的 Texture2D"))
        {
            AddSelectedTextures();
        }
        
        // 添加统一设置间隔按钮
        if (frameDatas.Count > 0 && GUILayout.Button("设置所有帧相同间隔"))
        {
            SetSameInterval();
        }
        
        // 采样率设置按钮
        if (targetClip != null && GUILayout.Button("设置采样率"))
        {
            targetClip.frameRate = sampleRate;
            EditorUtility.SetDirty(targetClip);
            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog("成功", $"采样率已设置为 {sampleRate}!", "确定");
        }

        if (GUILayout.Button("清空Texture2D"))
        {
            frameDatas.Clear();
        }

        EditorGUILayout.LabelField("帧数量:", frameDatas.Count.ToString());
        EditorGUILayout.Space();

        // 显示每一帧的 Texture2D 及其间隔时间
        for (int i = 0; i < frameDatas.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            frameDatas[i].texture = EditorGUILayout.ObjectField(frameDatas[i].texture, typeof(Texture2D), false) as Texture2D;
            frameDatas[i].interval = EditorGUILayout.FloatField(frameDatas[i].interval, GUILayout.Width(40));
            // 移除按钮
            if (GUILayout.Button("X", GUILayout.Width(20)))
            {
                frameDatas.RemoveAt(i);
                i--;
            }
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.Space();

        // 按钮：写入 AnimationClip
        if (frameDatas.Count > 0 && GUILayout.Button("写入 AnimationClip"))
        {
            if (targetClip == null)
            {
                EditorUtility.DisplayDialog("错误", "请先指定一个目标 AnimationClip！", "确定");
            }
            else
            {
                WriteAnimationToClip();
            }
        }
    }
    
    // 将所有帧的间隔设置为默认间隔
    private void SetSameInterval()
    {
        for (int i = 0; i < frameDatas.Count; i++)
        {
            frameDatas[i].interval = defaultInterval;
        }
    }

    // 获取当前在项目视图中选中的 Texture2D 资源，并以默认间隔添加到帧列表中
    private void AddSelectedTextures()
    {
        Object[] selection = Selection.objects;
        foreach (Object obj in selection)
        {
            if (obj is Texture2D t2d)
            {
                FrameData fd = new FrameData();
                fd.texture = t2d;
                fd.interval = defaultInterval;
                frameDatas.Add(fd);
            }
        }
    }

    // 将选中的 Texture2D 转换为 Sprite，并写入目标 AnimationClip
    private void WriteAnimationToClip()
    {
        // 使用目标采样率设置 AnimationClip 的帧率
        targetClip.frameRate = sampleRate;

        // 设置 AnimationClip 的曲线绑定（SpriteRenderer 的 m_Sprite 属性）
        // path 设置为目标子对象路径，可自由调整（留空则为根对象）
        EditorCurveBinding spriteBinding = new EditorCurveBinding
        {
            type = typeof(SpriteRenderer),
            path = targetChildPath,
            propertyName = "m_Sprite"
        };

        // 计算总时长并为每帧创建 keyframe
        List<ObjectReferenceKeyframe> keyFramesList = new List<ObjectReferenceKeyframe>();
        float time = 0f;
        foreach (var frameData in frameDatas)
        {
            // 通过 AssetDatabase 转换 Texture2D 为 Sprite
            if (frameData.texture == null)
            {
                Debug.LogWarning("某帧 Texture2D 为 null，已跳过！");
                continue;
            }

            string assetPath = AssetDatabase.GetAssetPath(frameData.texture);
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
            if (sprite == null)
            {
                Debug.LogWarning($"资源 {frameData.texture.name} 不能转换为 Sprite，请确认该资源的 Texture Type 为 Sprite (2D and UI)！");
                continue;
            }

            ObjectReferenceKeyframe keyFrame = new ObjectReferenceKeyframe
            {
                time = time,
                value = sprite
            };
            keyFramesList.Add(keyFrame);

            time += frameData.interval;
        }

        // 将 keyframe 曲线写入 AnimationClip
        AnimationUtility.SetObjectReferenceCurve(targetClip, spriteBinding, keyFramesList.ToArray());

        EditorUtility.SetDirty(targetClip);
        AssetDatabase.SaveAssets();
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = targetClip;

        EditorUtility.DisplayDialog("成功", "AnimationClip 已更新！", "确定");
    }
}
