using System;
using System.Collections.Generic;
using Game.GameMgr;
using Game.GameMgr.Component.Event;
using Game.GameRuntime.GameSceneManager.Base;
using UnityEngine;

namespace Game.GameRuntime.GameSceneManager.Component
{
    public class EventComponentGSM: BaseComponentGSM
    {
        private EventComponentGM eventComponentGM;
        private List<string> sceneEventList = new List<string>();

        public override void OnInit(IGameSceneManager manager)
        {
            base.OnInit(manager);

            eventComponentGM = GameManager.GetGMComponent<EventComponentGM>();
        }

        public override void OnShutdown()
        {
            base.OnShutdown();
            
            ClearAllSceneEvent();
        }

        public void TriggerEvent(object sender, string eventName)
        {
            eventComponentGM.TriggerEvent(sender, eventName);
        }

        public void TriggerEvent<T>(object sender, string eventName, T arg)
        {
            eventComponentGM.TriggerEvent(sender, eventName, arg);
        }

        public void RegisterSceneEvent(string eventName, Action<object> action)
        {
            if (!sceneEventList.Contains(eventName))
            {
                sceneEventList.Add(eventName);
                eventComponentGM.RegisterEvent(eventName, action);
            }
            else
            {
                Debug.LogError("该事件已经注册过: " + eventName + " 使用AddEventAction");
            }
        }
        
        /// <summary>
        /// 注册一个只触发一次的事件
        /// </summary>
        /// <param name="eventName">事件名</param>
        /// <param name="action">事件</param>
        public void RegisterOnceSceneEvent(string eventName, Action<object> action)
        {
            if (!sceneEventList.Contains(eventName))
            {
                action += sender =>
                {
                    sceneEventList.Remove(eventName);
                    eventComponentGM.RemoveEvent(eventName);
                };
                sceneEventList.Add(eventName);
                eventComponentGM.RegisterEvent(eventName, action);
            }
            else
            {
                Debug.LogError("该事件已经注册过: " + eventName + " 没有被触发！");
            }
        }

        public void AddSceneEventAction<T>(string eventName, Action<object, T> action)
        {
            eventComponentGM.RegisterEvent<T>(eventName, action);
        }

        public void RemoveSceneEvent(string eventName)
        {
            if (sceneEventList.Contains(eventName))
            {
                sceneEventList.Remove(eventName);
                eventComponentGM.RemoveEvent(eventName);
            }
        }

        private void ClearAllSceneEvent()
        {
            foreach (var eventName in sceneEventList)
            {
                eventComponentGM.RemoveEvent(eventName);
            }
            
            sceneEventList.Clear();
        }
    }
}