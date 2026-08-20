using System;
using System.Collections.Generic;
using System.Linq;
using GameFramework.CoreExtend.Component.interf;

namespace GameFramework.CoreExtend.Component
{
    public class ComponentSystem : IComponentSystem
    {
        private ComponentSystemEventBus eventBus;

        private SortedList<int, List<IGFEComponent>> componentsList;
        public Action<IGFEComponent> OnAddComponent { get; set; }
        public Action<IGFEComponent> OnRemoveComponent { get; set; }

        /// <summary>
        /// 独立使用
        /// </summary>
        public ComponentSystem()
        {
            componentsList = new SortedList<int, List<IGFEComponent>>();
            eventBus = new ComponentSystemEventBus();
        }


        public void InitComponents()
        {
            foreach (var kvp in componentsList)
            {
                foreach (var component in kvp.Value)
                {
                    // 防护：序列化/半套施工可能混入 null；跳过并继续，避免整场景 OnInit 中断黑屏
                    // 替代方案：直接 throw——能尽早暴露配置问题，但会再次导致进屋黑屏，故默认跳过
                    if (component == null)
                    {
                        continue;
                    }

                    component.Init(this);
                }
            }
        }

        public void CheckComponents()
        {
            foreach (var kvp in componentsList)
            {
                foreach (var component in kvp.Value)
                {
                    component.Check();
                }
            }
        }

        public bool HasComponent<T>() where T : class, IGFEComponent
        {
            foreach (var kvp in componentsList)
            {
                var component = kvp.Value.Find(x => x is T);
                if (component != null) return true;
            }

            return false;
        }

        public bool HasComponent(IGFEComponent component)
        {
            foreach (var kvp in componentsList)
            {
                if (kvp.Value.Contains(component)) return true;
            }

            return false;
        }

        public T TryGetComponent<T>() where T : class, IGFEComponent
        {
            foreach (var kvp in componentsList)
            {
                var component = kvp.Value.Find(x => x is T);
                if (component != null) return component as T;
            }   

            return null;
        }

        // --------------------------------------------------------------------------------
        // 添加组件

        public void AddComponent(Type type, int priority = 0)
        {
            // new 
            var component = (IGFEComponent)Activator.CreateInstance(type);
            if (component != null)
            {
                AddComponent(component, priority);
            }
            else
            {
                throw new GameFrameworkException("type类型错误没有继承IGFComponent");
            }
        }

        public void AddComponent<T>(int priority = 0) where T : IGFEComponent => AddComponent(typeof(T));

        public void AddComponent(IGFEComponent component, int priority = 0)
        {
            if (componentsList.Values.Any(list => list.Contains(component)))
            {
                return;
            }

            if (!componentsList.ContainsKey(priority))
            {
                componentsList[priority] = new List<IGFEComponent>();
            }

            componentsList[priority].Add(component);
            OnAddComponent?.Invoke(component);
        }

        // 修改组件优先级
        public void SetComponentPriority(IGFEComponent component, int newPriority)
        {
            // 先移除旧的
            RemoveComponent(component);

            // 重新添加到新的优先级
            AddComponent(component, newPriority);
        }

        // 更新组件
        public void UpdateComponents()
        {
            foreach (var kvp in componentsList)
            {
                foreach (var component in kvp.Value)
                {
                    component.OnUpdate();
                }
            }
        }


        public T GetComponent<T>() where T : class, IGFEComponent
        {
            foreach (var kvp in componentsList)
            {
                var component = kvp.Value.Find(x => x is T);
                if (component != null) return component as T;
            }
            
            throw new GameFrameworkException("没有找到组件" + typeof(T).Name);
        }

        public List<T> GetComponents<T>() where T : class, IGFEComponent
        {
            List<T> results = new List<T>();

            foreach (var kvp in componentsList)
            {
                foreach (var component in kvp.Value)
                {
                    if (component is T tComponent)
                    {
                        results.Add(tComponent);
                    }
                }
            }

            return results;
        }

        public void RemoveComponent(Type type)
        {
            foreach (var kvp in componentsList)
            {
                var component = kvp.Value.Find(x => x.GetType() == type);
                if (component != null)
                {
                    component.Dispose();
                    kvp.Value.Remove(component);
                    OnRemoveComponent?.Invoke(component);
                    break;
                }
            }
        }

        public void RemoveComponent<T>() where T : class, IGFEComponent => RemoveComponent(typeof(T));
        public void RemoveComponent(IGFEComponent component) => RemoveComponent(component.GetType());

        public void Dispose()
        {
            foreach (var kvp in componentsList)
            {
                foreach (var component in kvp.Value)
                {
                    component.Dispose();
                }
            }

            componentsList.Clear();
        }


        // 事件系统
        public void Subscribe<T>(string eventName, Action<T> listener) => eventBus.Subscribe(eventName, listener);
        public void Unsubscribe<T>(string eventName, Action<T> listener) => eventBus.Unsubscribe(eventName, listener);
        public void Publish<T>(string eventName, T param) => eventBus.Publish(eventName, param);
    }
}