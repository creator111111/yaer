using System.Collections.Generic;
using Game.Static.Utility;
using UnityEngine;

namespace Game.GameRuntime.Entities.Component.PhysicsDetect.FindTarget
{
    public enum DetectRangeType
    {
        Circle,
        Rectangle
    }

    public class TargetDetector : MonoBehaviour
    {
        public string detectorName;
        public DetectRangeType detectRangeType = DetectRangeType.Circle;

        public float radius;
        public float width;
        public float height;

#if UNITY_EDITOR
        public bool openGizmos;

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(detectorName))
            {
                detectorName = gameObject.name;
            }
        }

        private void OnDrawGizmos()
        {
            if (openGizmos == false)
            {
                return;
            }

            switch (detectRangeType)
            {
                case DetectRangeType.Circle:
                    Gizmos.DrawWireSphere(transform.position, radius);
                    break;
                case DetectRangeType.Rectangle:
                    Gizmos.DrawWireCube(transform.position, new Vector3(width, height, 0));
                    break;
            }
        }
#endif

        /// <summary>
        /// 检测
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public void Detect<T>(ref HashSet<T> targets)
        {
            switch (detectRangeType)
            {
                case DetectRangeType.Circle:
                    Collider2D[] clds = new Collider2D[100];
                    Physics2DUtility.CircleDetectMulti(transform.position, radius, ref clds);
                    if (clds == null)
                    {
                        return;
                    }
                    foreach (var cld in clds)
                    {
                        if (cld == null)
                        {
                            break;
                        }

                        if (cld.GetComponent<ColliderResponder>()?.GetEntityLogic() is T target)
                        {
                            targets.Add(target);
                        }
                    }
                    break;
                case DetectRangeType.Rectangle:
                    break;
            }
        }

        /// <summary>
        /// 检测指定目标
        /// </summary>
        /// <param name="target"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool Detect<T>(T target) where T : class
        {
            switch (detectRangeType)
            {
                case DetectRangeType.Circle:
                    Collider2D[] clds = new Collider2D[100];
                    Physics2DUtility.CircleDetectMulti(transform.position, radius, ref clds);
                    foreach (var cld in clds)
                    {
                        if (cld == null)
                        {
                            break;
                        }

                        if (cld.GetComponent<ColliderResponder>()?.GetEntityLogic() as T == target)
                        {
                            return true;
                        }
                    }

                    break;
                case DetectRangeType.Rectangle:
                    break;
            }

            return false;
        }
    }
}