using System;
using System.Collections.Generic;
using Game.GameMgr.Component.Base;
using GameFramework.Event;
using GameFramework.UnityRuntime.Event;
using UnityEngine;

namespace Game.GameMgr.Component.Event
{
    public class EventAction<T> : EventAction
    {
        public Action<object, T> argsAction;
    }

    public class EventAction
    {
        public Action<object> action;
    }

    public class EventComponentGM : BaseComponentGM
    {
        private EventComponent eventComponent;
        private Dictionary<string, EventAction> eventDic = new Dictionary<string, EventAction>();

        public override void OnInit()
        {
            base.OnInit();

            eventComponent = GameManager.GetGFComponent<EventComponent>();

            eventComponent.Subscribe(SceneEntityEventArgs.EventId, EventHandler);
        }

        private void EventHandler(object sender, GameEventArgs e)
        {
            if (e is SceneEntityEventArgs sceneEntityEventArgs)
            {
                if (eventDic.TryGetValue(sceneEntityEventArgs.eventName, out var eventAction))
                {
                    eventAction.action?.Invoke(sender);
                }
            }
        }
        
        private void EventHandler<T>(object sender, GameEventArgs e)
        {
            if (e is SceneEntityEventArgs<T> sceneEntityEventArgs)
            {
                if (eventDic.TryGetValue(sceneEntityEventArgs.eventName, out var eventAction) && eventAction is EventAction<T> typedEventAction)
                {
                    typedEventAction.argsAction?.Invoke(sender, sceneEntityEventArgs.arg);
                }
            }
        }
        
        public void RegisterEvent(string eventName, Action<object> action)
        {
            if (eventDic.ContainsKey(eventName))
            {
                Debug.LogError("该事件已经注册过: " + eventName);
                return;
            }

            var eventAction = new EventAction();            
            eventAction.action = action;
            eventDic.Add(eventName, eventAction);
        }

        public void RegisterEvent<T>(string eventName, Action<object, T> action)
        {
            if (eventDic.ContainsKey(eventName))
            {
                Debug.LogError("该事件已经注册过: " + eventName);
                return;
            }

            var eventAction = new EventAction<T>();
            eventAction.argsAction = action;
            eventDic.Add(eventName, eventAction);

            if (!eventComponent.Check(SceneEntityEventArgs<T>.EventId, EventHandler<T>))
            {
                // 订阅泛型事件（每个泛型类型都会有不同的 EventId）
                eventComponent.Subscribe(SceneEntityEventArgs<T>.EventId, EventHandler<T>);
            }
        }
        
        public bool HasEvent(string eventName) => eventDic.ContainsKey(eventName);
        
        public void AddEventAction(string eventName, Action<object> action)
        {
            if (eventDic.TryGetValue(eventName, out var value))
            {
                value.action += action;
            }
            else
            {
                Debug.LogError("未找到匹配的事件: " + eventName);
            }
        }
        
        /// <summary>
        /// 追加事件
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="action"></param>
        /// <typeparam name="T"></typeparam>
        public void AddEventAction<T>(string eventName, Action<object, T> action)
        {
            if (eventDic.TryGetValue(eventName, out var value) && value is EventAction<T> typedValue)
            {
                typedValue.argsAction += action;
            }
            else
            {
                Debug.LogError("未找到匹配的事件: " + eventName);
            }
        }
        
        public void RemoveEventAction(string eventName, Action<object> action)
        {
            if (eventDic.TryGetValue(eventName, out var value))
            {
                value.action -= action;
            }
            else
            {
                Debug.LogError("未找到匹配的事件: " + eventName);
            }
        }
        
        public void RemoveEventAction<T>(string eventName, Action<object, T> action)
        {
            if (eventDic.TryGetValue(eventName, out var value) && value is EventAction<T> typedValue)
            {
                typedValue.argsAction -= action;
            }
            else
            {
                Debug.LogError("未找到匹配的事件: " + eventName);
            }
        }
        
        
        public void TriggerEvent(object sender, string eventName)
        {
            var eventArgs = SceneEntityEventArgs.Create();
            eventArgs.eventName = eventName;
            eventComponent.Fire(sender, eventArgs);
        }

        public void TriggerEvent<T>(object sender, string eventName, T arg)
        {
            var eventArgs = SceneEntityEventArgs<T>.Create();
            eventArgs.eventName = eventName;
            eventArgs.arg = arg;
            eventComponent.Fire(sender, eventArgs);
        }
        
        public void RemoveEvent(string eventName)
        {
            if (eventDic.ContainsKey(eventName))
            {
                eventDic.Remove(eventName);
            }
        }
    }
}