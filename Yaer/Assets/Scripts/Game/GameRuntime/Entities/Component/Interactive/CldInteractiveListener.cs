using System;
using UnityEngine;

namespace Game.GameRuntime.Entities.Component.Interactive
{
    /// <summary>
    /// 碰撞盒监听
    /// </summary>
    public class CldInteractiveListener: MonoBehaviour
    {
        [SerializeField] private Collider2D listenerCollider;
        
        private InteractiveComponent interactiveComponent;
        
        public Action<Collider2D> onTriggerEnterEvent;
        public Action<Collider2D> onTriggerExitEvent;
        public Action<Collider2D> onTriggerStayEvent;

        public Action<Collision2D> onCollisionEnterEvent;
        public Action<Collision2D> onCollisionExitEvent;
        public Action<Collision2D> onCollisionStayEvent;

        public InteractiveComponent InteractiveComponent => interactiveComponent;

        private void OnValidate()
        {
            listenerCollider = GetComponent<Collider2D>();
            if (listenerCollider == null)
            {
                Debug.LogError("CldListener的collider引用丢失", gameObject);
            }
        }
        
        public void OnInit(InteractiveComponent component)
        {
            interactiveComponent = component;
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            onTriggerEnterEvent?.Invoke(other);
        }
        
        
        private void OnTriggerExit2D(Collider2D other)
        {
            onTriggerExitEvent?.Invoke(other);
        }
        
        
        private void OnTriggerStay2D(Collider2D other)
        {
            onTriggerStayEvent?.Invoke(other);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            onCollisionEnterEvent?.Invoke(collision);
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            onCollisionStayEvent?.Invoke(collision);
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            onCollisionExitEvent?.Invoke(collision);
        }
    }
}