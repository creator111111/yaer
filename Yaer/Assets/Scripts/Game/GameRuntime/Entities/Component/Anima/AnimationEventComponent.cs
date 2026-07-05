using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.GameRuntime.Entities.Component.Anima
{
    public class AnimationEventComponent : MonoBehaviour
    {
        private Dictionary<string, Action<string>> eventDic = new Dictionary<string, Action<string>>();

        public void AnimaEventTrigger(string args)
        {
            // 解析string
            // 参数规则 事件名:参数1,参数2, 参数3...
            var array = args.Split(':');
            var eventName = array[0];

            if (eventDic.ContainsKey(eventName) == false)
            {
                Debug.LogWarning("未注册动画事件: " + eventName, gameObject);
                return;
            }

            var arg = array.Length > 1 ? array[1] : "";
            eventDic[eventName]?.Invoke(arg);
        }

        public void RegisterEvent(string eventName, Action<string> action) => eventDic[eventName] = action;

        /// <summary>
        /// 调试用：NodeCanvas 的「等待 Animation 事件」在 OnExecute 时 Register，便于对比「发事件时是否已有人监听」。
        /// </summary>
        public bool IsEventRegistered(string eventName) =>
            !string.IsNullOrEmpty(eventName) && eventDic != null && eventDic.ContainsKey(eventName);

        // 去除某个注册的动画事件
        public void UnRegisterEvent(string eventName)
        {
            eventDic.Remove(eventName);
        }
        public void ClearEvent() => eventDic.Clear();
    }
}