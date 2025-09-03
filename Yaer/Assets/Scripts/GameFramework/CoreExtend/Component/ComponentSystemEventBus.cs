using System;
using System.Collections.Generic;

namespace GameFramework.CoreExtend.Component
{
    // 🔹 事件总线
    public class ComponentSystemEventBus
    {
        private Dictionary<string, Delegate> eventTable = new Dictionary<string, Delegate>();

        public void Subscribe<T>(string eventName, Action<T> listener)
        {
            if (eventTable.TryGetValue(eventName, out var existingDelegate))
            {
                eventTable[eventName] = Delegate.Combine(existingDelegate, listener);
            }
            else
            {
                eventTable[eventName] = listener;
            }
        }

        public void Unsubscribe<T>(string eventName, Action<T> listener)
        {
            if (eventTable.TryGetValue(eventName, out var existingDelegate))
            {
                var newDelegate = Delegate.Remove(existingDelegate, listener);
                if (newDelegate == null)
                {
                    eventTable.Remove(eventName);
                }
                else
                {
                    eventTable[eventName] = newDelegate;
                }
            }
        }

        public void Publish<T>(string eventName, T param)
        {
            if (eventTable.TryGetValue(eventName, out var existingDelegate))
            {
                (existingDelegate as Action<T>)?.Invoke(param);
            }
        }
    }
}