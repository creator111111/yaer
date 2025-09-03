using System.Collections.Generic;
using System.Linq;
using GameFramework.UnityRuntimeExtend.Component;
using UnityEngine;

namespace Game.GameRuntime.Entities.Component.PhysicsDetect.HitPoint
{
    public class HitPointComponent : BaseGFComponentMono
    {
        [SerializeField] private Transform hitPointParentTsf;
        [SerializeField] private List<HitPointInfo> points = new List<HitPointInfo>();

#if UNITY_EDITOR
        [SerializeField] private bool openGizmos = true;

        private void OnValidate()
        {
            points = hitPointParentTsf.GetComponentsInChildren<HitPointInfo>().ToList();
        }

        private void OnDrawGizmos()
        {
            if (openGizmos == false || points.Count == 0)
            {
                return;
            }
            
            foreach (var point in points)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(point.transform.position, 0.1f);
            }
        }
#endif

        protected override void OnInit()
        {
        }

        /// <summary>
        /// 获取最近碰撞点
        /// </summary>
        /// <returns></returns>
        public Transform GetHitPoint(Vector3 pos)
        {
            float d = 0f;
            Transform result = null;
            foreach (var point in points)
            {
                if (point)
                {
                    var dis = Vector3.Distance(pos, point.transform.position);
                    if (d == 0f)
                    {
                        result = point.transform;
                        d = dis;
                    }
                    if (dis < d)
                    {
                        d = dis;
                        result = point.transform;
                    } 
                }
            }

            return result;
        }
    }
}