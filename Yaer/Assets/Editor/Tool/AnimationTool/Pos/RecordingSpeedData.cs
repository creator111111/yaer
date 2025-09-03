using System.Collections.Generic;
using UnityEngine;

namespace EditorC.Tool.AnimationTool.Pos
{
    [CreateAssetMenu(fileName = "New RecordingSpeedData", menuName = "ScriptableObjects/RecordingSpeedData")]
    public class RecordingSpeedData : ScriptableObject
    {
        public List<float> framesInterval = new List<float>();
        public AnimationCurve xCurve = new AnimationCurve();
        public AnimationCurve yCurve = new AnimationCurve();
        public AnimationClip clip;
    }
}