using System;
using Game.GameRuntime.Entities.Base;
using Game.GameRuntime.Entities.Player;
using SingularityGroup.HotReload;
using UnityEngine;
using GFEntity = GameFramework.UnityRuntime.Entity.Entity;

namespace Game.GameRuntime.Entities.Component.Interactive
{
    /// <summary>
    /// 场景物体被射线命中后的点击转发器。
    /// 默认要求玩家 InteractiveCollider 与本物体 bounds 相交（NPC 走近才点得动）；
    /// 物品讲解等可关闭 <see cref="requirePlayerOverlap"/>，实现远程点击播对白。
    /// </summary>
    public class RaycastListener : MonoBehaviour
    {
        [SerializeField] private Collider2D listenerCollider;

        /// <summary>
        /// 为 true（默认）：OnClick 须玩家交互盒与本 Collider 相交（Expand 0.2），与 E 键近距一致。
        /// 为 false：射线命中即可派发点击（物品远程交互）；NPC 勿勾取消，避免远处误触整段对白。
        /// 替代方案：新建 RemoteRaycastListener 子类——旧 Prefab 零 diff，但多一种组件要记。
        /// </summary>
        [SerializeField]
        [Tooltip("勾选=须靠近才响应（NPC 默认）；取消勾选=远程点击即可（物品交互）")]
        private bool requirePlayerOverlap = true;

        private InteractiveComponent interactiveComponent;

        public InteractiveComponent InteractiveComponent => interactiveComponent;

        /// <summary>是否要求玩家与点击盒 overlap；供调试与子类读取。</summary>
        public bool RequirePlayerOverlap => requirePlayerOverlap;

        /// <summary>
        /// 点击事件
        /// </summary>
        public event Action onClickEvent;

        private void OnValidate()
        {
            listenerCollider = GetComponent<Collider2D>();
            if (listenerCollider == null)
            {
                Debug.LogError("RaycastListener的collider引用丢失", gameObject);
            }
        }

        public void OnInit(InteractiveComponent component)
        {
            interactiveComponent = component;
        }

        public virtual void OnClick(GFEntity playerEntity)
        {
            if (!(playerEntity.Logic is PlayerLogic playerLogic))
            {
                return;
            }

            var component = playerLogic.componentSystem.GetComponent<InteractiveComponent>();
            if (component == null)
            {
                return;
            }

            // 物品远程：跳过 overlap；NPC 默认仍须靠近（不相交则静默 return，与改前行为一致）
            if (requirePlayerOverlap && !AreCollidersOverlapping(component.InteractiveCollider))
            {
                return;
            }

            if (onClickEvent == null)
            {
                Debug.LogWarning("RaycastListener没有绑定点击事件", gameObject);
                return;
            }

            onClickEvent.Invoke();
        }

        /// <summary>
        /// 判断碰撞盒是否有重叠
        /// </summary>
        private bool AreCollidersOverlapping(Collider2D other)
        {
            if (listenerCollider == null || other == null) return false;
            var a = listenerCollider.bounds;
            var b = other.bounds;
            // 与 InteractiveComponent 保持一致的容差，避免点击判定比 E 判定更严格导致“明明靠近却不触发”。
            a.Expand(new Vector3(0.2f, 0.2f, 0));
            return a.Intersects(b);
        }
    }
}
