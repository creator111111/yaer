using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace EditorC.Tool.AnimationTool.Pos
{
    public class AnimationVelocityTool : EditorWindow
    {
        [MenuItem("Editor/Anima/Pos/AnimationVelocityTool")]
        private static void OpenWindow()
        {
            EditorWindow window = GetWindow<AnimationVelocityTool>();
            window.Show();
        }

        public RecordingSpeedData data;

        #region UI

        private Vector2 sv;

        #endregion

        private int count;
        private int lastCount;
        private bool isWrite;
        private bool isSmoothCurve;
        private float speed = 1;
        private int frameRate = 100;
        private string argsName;
        private MonoBehaviour target;

        private List<GameObject> framesList = new List<GameObject>();
        private List<float> framesInterval = new List<float>();
        private List<float> lastFramesInterval = new List<float>();
        private List<Vector2> dSpeed = new List<Vector2>();

        private AnimationClip clip;
        private AnimationClip backupClip;
        private AnimationCurve xCurve = new AnimationCurve();
        private AnimationCurve yCurve = new AnimationCurve();

        private void OnEnable()
        {
            if (!data)
            {
                data = AssetDatabase.LoadAssetAtPath<RecordingSpeedData>("Assets/Editor/AnimationTool/RecordingSpeedData.asset");
            }

            // 读取
            Clear();
            foreach (float f in data.framesInterval)
            {
                AddFrame(null, f);
                count++;
            }

            if (data.xCurve != null)
            {
                foreach (Keyframe keyframe in data.xCurve.keys)
                {
                    xCurve.AddKey(keyframe);
                }
            }

            if (data.yCurve != null)
            {
                foreach (Keyframe keyframe in data.yCurve.keys)
                {
                    yCurve.AddKey(keyframe);
                }
            }

            clip = data.clip;
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            data = EditorGUILayout.ObjectField("缓存文件", data, typeof(RecordingSpeedData), false) as RecordingSpeedData;
            if (GUILayout.Button("清空重置"))
            {
                Clear();
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(10);

            count = EditorGUILayout.IntField("帧数", count);

            #region 自动更新数组

            if (count > framesList.Count)
            {
                for (int i = 0; i < count - framesList.Count; i++)
                {
                    AddFrame(null, 1);
                }
            }
            else if (count < framesList.Count)
            {
                for (int i = 0; i < framesList.Count - count; i++)
                {
                    if (framesList.Count != 0)
                    {
                        RemoveFrame();
                    }
                }
            }

            #endregion

            #region 绘制帧和帧间隔控件

            if (framesList.Count > 10)
            {
                sv = EditorGUILayout.BeginScrollView(sv, GUILayout.Width(400), GUILayout.Height(200));
            }

            for (int i = 0; i < framesList.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                framesList[i] = EditorGUILayout.ObjectField("第" + (i + 1) + "帧", framesList[i], typeof(GameObject), true) as GameObject;
                framesInterval[i] = EditorGUILayout.FloatField(framesInterval[i], GUILayout.Width(40));
                EditorGUILayout.EndHorizontal();
            }

            if (framesList.Count > 10)
            {
                EditorGUILayout.EndScrollView();
            }

            #endregion

            #region +、-按键

            EditorGUILayout.BeginHorizontal();

            GUILayout.FlexibleSpace();


            if (GUILayout.Button("+", GUILayout.Width(40)))
            {
                AddFrame(null, 1);
                count++;
            }

            if (GUILayout.Button("-", GUILayout.Width(40)))
            {
                if (framesList.Count != 0)
                {
                    RemoveFrame();
                    count--;
                }
            }

            EditorGUILayout.EndHorizontal();

            #endregion

            if (GUILayout.Button("一键添加选中帧"))
            {
                // copy framesInterval
                lastFramesInterval.Clear();
                foreach (float f in framesInterval)
                {
                    lastFramesInterval.Add(f);
                }

                framesList.Clear();
                framesInterval.Clear();
                count = 0;
                Object[] gos = Selection.objects;
                foreach (var go in gos)
                {
                    framesList.Add(go as GameObject);
                    framesInterval.Add(1);
                    count++;
                }
            }

            if (GUILayout.Button("恢复上次帧间隔时间"))
            {
                framesInterval.Clear();
                foreach (float f in lastFramesInterval)
                {
                    framesInterval.Add(f);
                }
            }

            isSmoothCurve = EditorGUILayout.Toggle("平滑曲线", isSmoothCurve);
            speed = EditorGUILayout.FloatField("速度", speed);
            frameRate = EditorGUILayout.IntField("帧率", frameRate);

            if (GUILayout.Button("生成位移曲线"))
            {
                CaleSpeedCurve();
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("x轴速度曲线", GUILayout.Width(200));
            EditorGUILayout.LabelField("y轴速度曲线", GUILayout.Width(200));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            xCurve = EditorGUILayout.CurveField(xCurve, GUILayout.Width(200), GUILayout.Height(200));
            yCurve = EditorGUILayout.CurveField(yCurve, GUILayout.Width(200), GUILayout.Height(200));

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);
            clip = EditorGUILayout.ObjectField("动画文件", clip, typeof(AnimationClip), false) as AnimationClip;
            argsName = EditorGUILayout.TextField("参数名", argsName);
            target = EditorGUILayout.ObjectField("目标脚本", target, typeof(MonoBehaviour), true) as MonoBehaviour;
            if (GUILayout.Button("写入动画文件"))
            {
                WriteToAnimation();
            }

            if (GUILayout.Button("撤回写入"))
            {
                WithDrawWriteToAnimation();
            }

            AdjustWindowSize();
        }

        private void RemoveFrame()
        {
            framesList.RemoveAt(framesList.Count - 1);
            framesInterval.RemoveAt(framesInterval.Count - 1);
        }

        private void AddFrame(GameObject frame, float interval)
        {
            framesList.Add(frame);
            framesInterval.Add(interval);
        }

        private void Clear()
        {
            xCurve = new AnimationCurve();
            yCurve = new AnimationCurve();
            framesInterval.Clear();
            dSpeed.Clear();
            framesList.Clear();
            count = 0;
            clip = null;
        }

        private void CaleSpeedCurve()
        {
            if (framesList.Count == 0)
            {
                EditorUtility.DisplayDialog("提示", "请先添加帧", "确定");
                return;
            }

            dSpeed = new List<Vector2>();
            dSpeed.Add(new Vector2(0, 0)); // 第一帧速度为0
            // dx, dy
            for (int i = 0; i < framesList.Count - 1; i++)
            {
                Vector2 nowFrame = new Vector2(framesList[i].transform.localPosition.x, framesList[i].transform.localPosition.y);
                Vector2 nextFrame = new Vector2(framesList[i + 1].transform.localPosition.x, framesList[i + 1].transform.localPosition.y);
                Vector2 dFrame = nextFrame - nowFrame;
                Vector2 dSpeedFrame = dFrame / (framesInterval[i] / speed);
                float x = Mathf.Round(dSpeedFrame.x * 100) / 100;
                float y = Mathf.Round(dSpeedFrame.y * 100) / 100;
                dSpeedFrame = new Vector2(x, y);
                dSpeed.Add(dSpeedFrame);
            }

            // 生成曲线
            xCurve = new AnimationCurve();
            yCurve = new AnimationCurve();

            float time = 0;
            for (int i = 0; i < dSpeed.Count; i++)
            {
                Keyframe xKeyframe = new Keyframe(time, dSpeed[i].x);
                Keyframe yKeyframe = new Keyframe(time, dSpeed[i].y);

                if (!isSmoothCurve)
                {
                    xKeyframe.inTangent = Mathf.Infinity;
                    xKeyframe.outTangent = Mathf.Infinity;
                    yKeyframe.inTangent = Mathf.Infinity;
                    yKeyframe.outTangent = Mathf.Infinity;
                }

                xCurve.AddKey(xKeyframe);
                yCurve.AddKey(yKeyframe);
                time += framesInterval[i] / speed;
            }
        }

        private void WriteToAnimation()
        {
            if (!clip)
            {
                EditorUtility.DisplayDialog("提示", "请先选择动画文件", "确定");
                return;
            }

            if (framesList.Count == 0 || framesList[0] is null)
            {
                EditorUtility.DisplayDialog("提示", "请先添加帧", "确定");
                return;
            }

            isWrite = true;

            // 备份
            backupClip = new AnimationClip();
            clip.ClearCurves();
            EditorUtility.CopySerialized(clip, backupClip);
            // 修改采样率
            clip.frameRate = frameRate;

            clip.SetCurve("", target.GetType(), argsName + ".x", xCurve);
            clip.SetCurve("", target.GetType(), argsName + ".y", yCurve);

            // 写入Sprite
            float time = 0;
            ObjectReferenceKeyframe[] objectReferenceKeyframes = new ObjectReferenceKeyframe[framesList.Count];
            for (int i = 0; i < framesList.Count; i++)
            {
                objectReferenceKeyframes[i] = new ObjectReferenceKeyframe();
                objectReferenceKeyframes[i].time = time;
                objectReferenceKeyframes[i].value = framesList[i].GetComponent<SpriteRenderer>().sprite;
                time += framesInterval[i] / speed;
            }

            // 写入速度曲线
            AnimationUtility.SetObjectReferenceCurve(clip, EditorCurveBinding.PPtrCurve("Animation", typeof(SpriteRenderer), "m_Sprite"),
                objectReferenceKeyframes);

            AssetDatabase.Refresh();
        }

        private void WithDrawWriteToAnimation()
        {
            if (!isWrite)
            {
                EditorUtility.DisplayDialog("提示", "请先写入动画文件", "确定");
                return;
            }

            EditorUtility.CopySerialized(backupClip, clip);

            AssetDatabase.Refresh();
        }

        private void AdjustWindowSize()
        {
            // 获取控件的总高度
            float totalHeight = GUILayoutUtility.GetLastRect().yMax;
            if (totalHeight <= 1)
            {
                return;
            }

            // 设置最小和最大窗口大小
            float newHeight = Mathf.Clamp(totalHeight + 10, 450, 800);
            minSize = new Vector2(410, newHeight);
            maxSize = new Vector2(410, newHeight);
        }

        private void OnDisable()
        {
            // 保存数据
            data.xCurve = new AnimationCurve();
            data.yCurve = new AnimationCurve();
            data.framesInterval.Clear();
            foreach (float f in framesInterval)
            {
                data.framesInterval.Add(f);
            }

            if (xCurve != null)
            {
                foreach (Keyframe keyframe in xCurve.keys)
                {
                    data.xCurve.AddKey(keyframe);
                }
            }

            if (yCurve != null)
            {
                foreach (Keyframe keyframe in yCurve.keys)
                {
                    data.yCurve.AddKey(keyframe);
                }
            }

            data.clip = clip;
        }
    }
}