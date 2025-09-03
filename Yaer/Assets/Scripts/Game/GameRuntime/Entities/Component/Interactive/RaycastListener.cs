using System;
using Game.GameRuntime.Entities.Base;
using Game.GameRuntime.Entities.Player;
using SingularityGroup.HotReload;
using UnityEngine;
using GFEntity = GameFramework.UnityRuntime.Entity.Entity;

namespace Game.GameRuntime.Entities.Component.Interactive
{
    public class RaycastListener : MonoBehaviour
    {
        [SerializeField] private Collider2D listenerCollider;
        private InteractiveComponent interactiveComponent;

        public InteractiveComponent InteractiveComponent => interactiveComponent;


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
            if (playerEntity.Logic is PlayerLogic playerLogic)
            {
                var component = playerLogic.componentSystem.GetComponent<InteractiveComponent>();

                if (component != null && AreCollidersOverlapping(component.InteractiveCollider))
                {
                    if (onClickEvent == null)
                    {
                        Debug.LogWarning("RaycastListener没有绑定点击事件", gameObject);
                        return;
                    }
                    onClickEvent.Invoke();
                }
            }
        }

        /// <summary>
        /// 判断碰撞盒是否有重叠
        /// </summary>
        private bool AreCollidersOverlapping(Collider2D other)
        {
            return listenerCollider.bounds.Intersects(other.bounds);
        }
    }
}