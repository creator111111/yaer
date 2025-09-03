using System;
using System.Collections.Generic;
using Game.GameMgr;
using Game.GameMgr.Component;
using Game.GameRuntime.Entities.Player;
using UnityEngine;
using UnityEngine.UI;

namespace Game.GameRuntime.Entities.Component.Interactive
{
    public class InteractiveComponent : BaseGFComponentEntity
    {
        [SerializeField] private Collider2D interactiveCollider; // 触发交互的碰撞盒区域
        [SerializeField] private List<RaycastListener> raycastListeners = new List<RaycastListener>(); // 用于射线检测的 Collider2D 列表
        [SerializeField] public List<CldInteractiveListener> cldListeners = new List<CldInteractiveListener>(); // 用于碰撞检测的 Collider2D 列表

        public Collider2D InteractiveCollider => interactiveCollider;

        /// <summary>
        /// 点击交互
        /// </summary>
        public event Action<InteractiveComponent> onClickInteractiveEvent;

        /// <summary>
        /// 进入范围交互
        /// </summary>
        public event Action<InteractiveComponent> onEnterInteractiveEvent;

        /// <summary>
        /// 离开范围交互
        /// </summary>
        public event Action<InteractiveComponent> onExitInteractiveEvent;

        /// <summary>
        /// 一直在交互范围内
        /// </summary>
        public event Action<InteractiveComponent> onStayInteractiveEvent;

        private void OnValidate()
        {
            if (interactiveCollider == null) Debug.LogError("interactiveCollider引用丢失", gameObject);
        }

        protected override void OnInit()
        {
            base.OnInit();

            // 监听事件
            foreach (var listener in raycastListeners)
            {
                if (listener is null)
                {
                    continue;
                }

                listener.OnInit(this);
                listener.onClickEvent += () => onClickInteractiveEvent?.Invoke(this);
            }

            foreach (var listener in cldListeners)
            {
                if (listener is null)
                {
                    continue;
                }

                listener.OnInit(this);
                listener.onTriggerEnterEvent = cld =>
                {
                    // 获取碰撞对象的组件
                    var cldInteractiveListener = cld.GetComponent<CldInteractiveListener>();

                    if (cldInteractiveListener)
                    {
                        onEnterInteractiveEvent?.Invoke(cldInteractiveListener.InteractiveComponent);
                    }
                };
                listener.onTriggerExitEvent = cld =>
                {
                    var cldInteractiveListener = cld.GetComponent<CldInteractiveListener>();

                    if (cldInteractiveListener)
                    {
                        onExitInteractiveEvent?.Invoke(cldInteractiveListener.InteractiveComponent);
                    }
                };
                listener.onTriggerStayEvent = cld =>
                {
                    var cldInteractiveListener = cld.GetComponent<CldInteractiveListener>();

                    if (cldInteractiveListener)
                    {
                        onStayInteractiveEvent?.Invoke(cldInteractiveListener.InteractiveComponent);
                    }
                };
            }
        }

        public void OnInteractive()
        {
            onClickInteractiveEvent?.Invoke(this);
        }

        /// <summary>
        /// 判断碰撞盒是否有重叠
        /// </summary>
        public bool AreCollidersOverlapping(Collider2D other)
        {
            return interactiveCollider.bounds.Intersects(other.bounds);
        }

        public bool AreCollidersOverlapping(InteractiveComponent other)
        {
            return interactiveCollider.bounds.Intersects(other.InteractiveCollider.bounds);
        }

        /// <summary>
        /// 交互碰撞盒距离
        /// </summary>
        public float DistanceTo(InteractiveComponent other)
        {
            return Vector2.Distance(interactiveCollider.bounds.center, other.InteractiveCollider.bounds.center);
        }
    }
}