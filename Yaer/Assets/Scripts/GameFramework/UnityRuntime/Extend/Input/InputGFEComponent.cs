using System;
using System.Collections.Generic;
using GameFramework.CoreExtend.Event;
using GameFramework.UnityRuntime.Base;
using GameFramework.UnityRuntimeExtend.Base;
using UnityEngine;

namespace GameFramework.CoreExtend.Systems.Input
{
    /// <summary>
    ///     按下类型
    /// </summary>
    public enum EDownType
    {
        LongPress, // 长按
        Down, // 按下
        Up // 抬起
    }

    public class InputGFEComponent : GameFrameworkComponent
    {
        
        private readonly Dictionary<string, Action> checkCodeActionDic = new Dictionary<string, Action>();
        private bool isStart;
        private Action keyCodeAction;
        private IEventSystem eventSystem;
    
        public InputGFEComponent()
        {
            // 每帧执行检测
            // MonoSystem.Instance.AddUpdateEvent(CheckKeyCode);
        }
        /// <summary>
        ///     执行触发
        /// </summary>
        /// <param name="keyCode">要检测的按键</param>
        private void TriggerKeyCode(KeyCode keyCode, EDownType type, string thingName)
        {
            // 长按
            if (UnityEngine.Input.GetKey(keyCode) && type == EDownType.LongPress)
                eventSystem.TriggerEvent(keyCode + EDownType.LongPress.ToString() + thingName);
            // 按下
            if (UnityEngine.Input.GetKeyDown(keyCode) && type == EDownType.Down)
                eventSystem.TriggerEvent(keyCode + EDownType.Down.ToString() + thingName);
            // 抬起
            if (UnityEngine.Input.GetKeyUp(keyCode) && type == EDownType.Up)
                eventSystem.TriggerEvent(keyCode + EDownType.Up.ToString() + thingName);
        }
    
        /// <summary>
        ///     检测按键
        /// </summary>
        /// <param name="checkKeyCodeFunc"></param>
        private void Update()
        {
            if (!isStart) return;
    
            keyCodeAction?.Invoke();
        }
    
        public void AddCheckKeyCode(KeyCode keyCode, EDownType type, string thingName)
        {
            keyCodeAction += () => { TriggerKeyCode(keyCode, type, thingName); };
        }
    
        public void AddKeyCodeListener(KeyCode keyCode, EDownType type, string eventName, Action func)
        {
            eventSystem.AddEventListener(keyCode.ToString() + type + eventName, func);
            // 记录已注册的按键事件
            checkCodeActionDic.Add(keyCode.ToString() + type + eventName, func);
        }
    
        public void Start()
        {
            isStart = true;
        }
    
        public void Stop()
        {
            isStart = false;
        }
    
        public void Clear(KeyCode keyCode, EDownType type, string eventName, Action func)
        {
            keyCodeAction -= func;
            eventSystem.RemoveEventListener(keyCode.ToString() + type + eventName, func);
        }
    
        public void ClearAll()
        {
            keyCodeAction = null;
            foreach (var item in checkCodeActionDic) eventSystem.RemoveEventListener(item.Key, item.Value);
            checkCodeActionDic.Clear();
        }
    }
}