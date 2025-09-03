using UnityEngine;

namespace Game.GameRuntime.Entities.Component.Move
{
    [CreateAssetMenu(fileName = "MoveCurveData", menuName = "Movement/MoveCurveData", order = 1)]
    public class MoveCurveData : ScriptableObject
    {
        public string curveName;
        // 用于控制移动进度的曲线，通常 x 轴为 normalizedTime（0~1），y 轴为 lerp 插值值
        public AnimationCurve movementCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        // 移动总时长（秒）
        public float duration = 1f;

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(curveName))
            {
                curveName = name;
            }
        }
    }
}