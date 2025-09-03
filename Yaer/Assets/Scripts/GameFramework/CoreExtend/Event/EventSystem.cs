using System;
using System.Collections.Generic;

namespace GameFramework.CoreExtend.Event
{
    public class EventSystem : IEventSystem
    {
        // 接口父类装子类, 方便带泛型事件
        private readonly Dictionary<string, IEventInfo> eventDic = new Dictionary<string, IEventInfo>();

        /// <summary>
        ///     添加事件监听
        /// </summary>
        /// <param name="eventName">事件名字</param>
        /// <param name="newAction">新的事件</param>
        public void AddEventListener(string eventName, Action newAction)
        {
            // 查找有无对应事件, 无则创建
            if (eventDic.ContainsKey(eventName))
                ((EventInfo)eventDic[eventName]).action += newAction;
            else
                eventDic.Add(eventName, new EventInfo(newAction));
        }

        /// <summary>
        ///     带泛型添加事件监听
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="newAction"></param>
        /// <typeparam name="T"></typeparam>
        public void AddEventListener<T>(string eventName, Action<T> newAction)
        {
            // 查找有无对应事件, 无则创建
            if (eventDic.ContainsKey(eventName))
                ((EventInfo<T>)eventDic[eventName]).action += newAction;
            else
                eventDic.Add(eventName, new EventInfo<T>(newAction));
        }

        /// <summary>
        ///     清空指定事件
        /// </summary>
        /// <param name="eventName">事件名</param>
        /// <param name="oldAction">目标事件</param>
        public void RemoveEventListener(string eventName, Action oldAction)
        {
            if (eventDic.ContainsKey(eventName)) ((EventInfo)eventDic[eventName]).action -= oldAction;
        }

        public void RemoveEventListener<T>(string eventName, Action<T> oldAction)
        {
            if (eventDic.ContainsKey(eventName)) ((EventInfo<T>)eventDic[eventName]).action -= oldAction;
        }

        // 触发事件执行
        public void TriggerEvent(string eventName)
        {
            if (eventDic.ContainsKey(eventName))
                ((EventInfo)eventDic[eventName]).action?.Invoke();
            else
                throw new Exception("无此事件:" + eventName);
        }

        public void TriggerEvent<T>(string eventName, T parameter)
        {
            if (eventDic.ContainsKey(eventName))

                // 带参数触发
                ((EventInfo<T>)eventDic[eventName]).action?.Invoke(parameter);
            else
                throw new Exception("无此事件:" + eventName);
        }

        /// <summary>
        ///     删除该名称下所有事件
        /// </summary>
        /// <param name="eventName"></param>
        public void RemoveEvent(string eventName)
        {
            eventDic.Remove(eventName);
        }

        // 清空所有事件
        public void ClearAllEvent()
        {
            eventDic.Clear();
        }
    }
}