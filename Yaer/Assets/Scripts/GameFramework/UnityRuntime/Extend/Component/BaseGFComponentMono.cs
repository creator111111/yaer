using System;
using GameFramework.CoreExtend.Component.interf;
using GameFramework.UnityRuntime.Utility;
using UnityEngine;

namespace GameFramework.UnityRuntimeExtend.Component
{
    public abstract class BaseGFComponentMono : MonoBehaviour, IComparable<BaseGFComponentMono>, IGFEComponent
    {
        [SerializeField] private int priority;

        public int Priority
        {
            get => priority;
            set => priority = value;
        }

        private IComponentSystem componentSystem;

        public void Init(IComponentSystem system)
        {
            if (componentSystem != null)
            {
                throw new Exception( GetType().Name + "组件已经初始化");
            }

            componentSystem = system;

            OnInit();
        }

        public virtual void Check()
        {
            
        }

        protected abstract void OnInit();
        
        public virtual void OnUpdate()
        {
            
        }

        public virtual void OnFixedUpdate()
        {
            
        }

        public virtual void Dispose()
        {
        }

        public int CompareTo(BaseGFComponentMono other)
        {
            if (other == null) return 1;
            return priority.CompareTo(other.priority);
        }

        public new T GetComponent<T>() where T : class, IGFEComponent
        {
            return componentSystem.GetComponent<T>();
        }
    }
}