using System;
using System.Collections.Generic;
using GameFramework.CoreExtend.Base;

namespace GameFramework.CoreExtend.Component.interf
{
    public interface IComponentSystem : IGFExtendSystem
    {
        Action<IGFEComponent> OnAddComponent { get; set; }
        Action<IGFEComponent> OnRemoveComponent { get; set; }

        void InitComponents();
        void AddComponent(IGFEComponent component, int priority = 0);
        void AddComponent<T>(int priority = 0) where T : IGFEComponent;
        void AddComponent(Type type, int priority = 0);
        
        void RemoveComponent(Type type);
        void RemoveComponent(IGFEComponent component);
        void RemoveComponent<T>() where T : class, IGFEComponent;
        
        T GetComponent<T>() where T : class, IGFEComponent;
        List<T> GetComponents<T>() where T : class, IGFEComponent;
        void UpdateComponents();
        void SetComponentPriority(IGFEComponent component, int newPriority);
        void Subscribe<T>(string eventName, Action<T> listener);
        void Unsubscribe<T>(string eventName, Action<T> listener);
        void Publish<T>(string eventName, T param);
        void CheckComponents();
        bool HasComponent<T>() where T : class, IGFEComponent;
        bool HasComponent(IGFEComponent component);
        T TryGetComponent<T>() where T : class, IGFEComponent;
    }
}