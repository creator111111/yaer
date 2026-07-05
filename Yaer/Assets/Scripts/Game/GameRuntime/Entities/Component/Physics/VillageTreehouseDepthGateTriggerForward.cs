using UnityEngine;

namespace Game.GameRuntime.Entities.Component.Physics
{
    /// <summary>
    /// 树屋门控子 Trigger 转发：向 <see cref="VillageTreehouseDepthZoneGate"/> 上报 <b>Enter 与 Exit</b>（执行文档 §3.1 / §3.3 方案 A），
    /// 由门控在 Exit 的「双接触→单侧收尾」边查 §1.3.3 表，避免仅用 Enter 顺序在宽脚点下同帧不确定。
    /// </summary>
    [DisallowMultipleComponent]
    public class VillageTreehouseDepthGateTriggerForward : MonoBehaviour
    {
        [Tooltip("接收 Enter/Exit 的协调器；建议挂在场景内 Active 父节点上，勿挂在默认 Inactive 的门体上。")]
        [SerializeField]
        private VillageTreehouseDepthZoneGate gate;

        [Tooltip("0 = Trigger-0，1 = Trigger-1（与策划表 0/1 映射一致）。")]
        [SerializeField]
        private int triggerIndex;

#if UNITY_EDITOR
        /// <summary>将序号限制在 0～1，避免与门控语义不一致。</summary>
        private void OnValidate()
        {
            triggerIndex = Mathf.Clamp(triggerIndex, 0, 1);
        }
#endif

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (gate == null)
            {
                return;
            }

            gate.NotifyTriggerEnter(triggerIndex, other);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (gate == null)
            {
                return;
            }

            gate.NotifyTriggerExit(triggerIndex, other);
        }
    }
}
