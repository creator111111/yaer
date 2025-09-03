using System.Collections.Generic;
using System.Linq;
using GameFramework.UnityRuntimeExtend.Component;
using UnityEngine;

namespace Game.GameRuntime.Entities.Component.PhysicsDetect.FindTarget
{
    public class FindTargetComponent : BaseGFComponentMono
    {
        [SerializeField] private Transform detectorParentTsf;
        [SerializeField] private List<TargetDetector> detectorList = new List<TargetDetector>();
        private Dictionary<string, TargetDetector> detectorDict = new Dictionary<string, TargetDetector>();

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (detectorParentTsf != null) detectorList = detectorParentTsf.GetComponentsInChildren<TargetDetector>().ToList();
        }
#endif

        protected override void OnInit()
        {
            foreach (TargetDetector detector in detectorList)
            {
                detectorDict.Add(detector.name, detector);
            }
        }

        /// <summary>
        /// 查找目标
        /// </summary>
        /// <param name="targets"></param>
        /// <param name="detectorNames"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public void FindTarget<T>(ref HashSet<T> targets, params string[] detectorNames)
        {
            // 指定检测器
            if (detectorNames.Length > 0)
            {
                foreach (var n in detectorNames)
                {
                    if (detectorDict.TryGetValue(n, out var value))
                    {
                        value.Detect<T>(ref targets);
                    }
                    else
                    {
                        Debug.LogWarning("未找到检测器: " + n);
                    }
                }
            }
            else
            {
                foreach (var detector in detectorList)
                {
                    detector.Detect<T>(ref targets);
                }
            }
        }

        public bool HasTarget<T>(T target, params string[] detectorNames) where T : class
        {
            // 指定检测器
            if (detectorNames.Length > 0)
            {
                foreach (var n in detectorNames)
                {
                    if (detectorDict.TryGetValue(n, out var value))
                    {
                        if (value.Detect<T>(target))
                        {
                            return true;
                        }
                    }
                    else
                    {
                        Debug.LogWarning("未找到检测器: " + n);
                    }
                }
            }
            else
            {
                foreach (var detector in detectorList)
                {
                    if (detector.Detect<T>(target))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}