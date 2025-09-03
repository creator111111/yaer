using System;
using GameFramework.CoreExtend.Base;

namespace GameFramework.CoreExtend.Event
{
    public interface IEventSystem: IGFExtendSystem
    {
        void AddEventListener(string eventName, Action newAction);
        void AddEventListener<T>(string eventName, Action<T> newAction);
        void RemoveEventListener(string eventName, Action oldAction);
        void RemoveEventListener<T>(string eventName, Action<T> oldAction);
        void TriggerEvent(string eventName);
        void TriggerEvent<T>(string eventName, T parameter);
        void RemoveEvent(string eventName);
        void ClearAllEvent();
    }
}