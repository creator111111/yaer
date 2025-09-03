using System;
using GameFramework.CoreExtend.Component.interf;

namespace GameFramework.CoreExtend.Component
{
    namespace GameFramework.CoreExtend.Systems.Component
    {
        public abstract class BaseGFEComponent : IComparable<BaseGFEComponent>, IGFEComponent
        {
            private int priority;
            
            public int Priority => priority;

            private IComponentSystem system;

            protected BaseGFEComponent(int priority = 0)
            {
                this.priority = priority;
            }

            public void Init(IComponentSystem system)
            {
                if (this.system != null)
                {
                    throw new Exception("组件已经初始化");
                }

                this.system = system;

                OnInit();
            }

            public void Check()
            {
                
            }

            protected abstract void OnInit();
            public abstract void OnUpdate();

            public virtual void Dispose()
            {
            }

            public int CompareTo(BaseGFEComponent other)
            {
                if (other == null) return 1;
                return priority.CompareTo(other.priority);
            }
        }
    }
}