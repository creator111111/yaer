using System;
using GameFramework.UnityRuntime.Base;
using GameFramework.UnityRuntimeExtend.Base;

namespace GameFramework.CoreExtend.Event
{
    public class EventSystemComponent : GameFrameworkComponent
    {
        private IEventSystem eventSystem;

        /// <summary>
        ///     添加事件监听
        /// </summary>
        /// <param name="eventName">事件名字</param>
        /// <param name="newAction">新的事件</param>
        public void AddEventListener(string eventName, Action newAction) => eventSystem.AddEventListener(eventName, newAction);

        /// <summary>
        ///     带泛型添加事件监听
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="newAction"></param>
        /// <typeparam name="T"></typeparam>
        public void AddEventListener<T>(string eventName, Action<T> newAction) => eventSystem.AddEventListener(eventName, newAction);

        /// <summary>
        ///     清空指定事件
        /// </summary>
        /// <param name="eventName">事件名</param>
        /// <param name="oldAction">目标事件</param>
        public void RemoveEventListener(string eventName, Action oldAction) => eventSystem.RemoveEventListener(eventName, oldAction);

        public void RemoveEventListener<T>(string eventName, Action<T> oldAction) => eventSystem.RemoveEventListener(eventName, oldAction);

        // 触发事件执行
        public void TriggerEvent(string eventName) => eventSystem.TriggerEvent(eventName);

        public void TriggerEvent<T>(string eventName, T parameter) => eventSystem.TriggerEvent(eventName, parameter);

        /// <summary>
        ///     删除该名称下所有事件
        /// </summary>
        /// <param name="eventName"></param>
        public void RemoveEvent(string eventName) => eventSystem.RemoveEvent(eventName);

        // 清空所有事件
        public void ClearAllEvent() => eventSystem.ClearAllEvent();
    }
}