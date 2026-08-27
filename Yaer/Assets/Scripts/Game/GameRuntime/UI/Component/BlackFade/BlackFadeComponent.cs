using System;
using System.Collections.Generic;
using Game.GameMgr;
using Game.GameMgr.Component;
using Game.GameMgr.Component.UI;
using Game.GameRuntime.UI.Control;
using GameFramework.UnityRuntime.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Game.GameRuntime.UI.Component.BlackFade
{
    /// <summary>
    /// UI淡入淡出组件
    /// </summary>
    public class BlackFadeComponent : BaseGFComponentUI
    {
        [SerializeField] private BlackMask blackMask;

        private List<Selectable> controls = new List<Selectable>();

        public bool IsBusy => blackMask.Showing || blackMask.Hiding;

        private void OnValidate()
        {
            if (blackMask == null)
            {
                Debug.LogWarning($"blackMask引用丢失=>{transform.root}");
            }
        }

        protected override void OnInit()
        {
            base.OnInit();

            blackMask.OnInit();
        }


        public override void OnUpdate()
        {
            base.OnUpdate();

            blackMask.OnUpdate();
        }

        /// <summary>
        ///  淡入黑幕.
        /// </summary>
        public void ShowFade(Action callBack = null)
        {
            blackMask.ShowFade(callBack);
        }

        /// <summary>
        ///  淡出黑幕
        /// </summary>
        public void HideFade(Action callBack = null)
        {
            blackMask.HideFade(callBack);
        }

        /// <summary>临时覆盖淡入/淡出时长（秒）。</summary>
        public void SetFadeDurations(float showSeconds, float hideSeconds)
        {
            blackMask.SetFadeDurations(showSeconds, hideSeconds);
        }

        /// <summary>恢复 BlackMask Prefab 默认 show/hide 时长。</summary>
        public void RestoreDefaultFadeDurations()
        {
            blackMask.RestoreDefaultFadeDurations();
        }

        public float GetDefaultShowDuration() => blackMask.DefaultShowTime;
        public float GetDefaultHideDuration() => blackMask.DefaultHideTime;
        public float GetCurrentShowDuration() => blackMask.CurrentShowTime;
        public float GetCurrentHideDuration() => blackMask.CurrentHideTime;

        /// <summary>
        /// 设置为直接隐藏黑幕
        /// </summary>
        public void HideRow(Action callBack = null)
        {
            blackMask.HideRow(callBack);
        }

        /// <summary>
        /// 设置为直接显示黑幕
        /// </summary>
        public void ShowRow(Action callBack = null)
        {
            blackMask.ShowRow(callBack);
        }

        public void AddShowingAction(float normalize, Action action) => blackMask.AddShowingAction(normalize, action);
        public void AddHidingAction(float normalize, Action action) => blackMask.AddHidingAction(normalize, action);

        /// <summary>
        /// 添加控件同时控制交互启用
        /// </summary>
        /// <param name="control"></param>
        public void AddControl(params Selectable[] control)
        {
            foreach (var c in control)
            {
                if (!controls.Contains(c))
                {
                    controls.Add(c);
                }
            }
        }

        /// <summary>
        /// 设置交互是否启用
        /// </summary>
        /// <param name="isInteractive"></param>
        public void SetInteractive(bool isInteractive)
        {
            foreach (var control in controls)
            {
                control.interactable = isInteractive;
            }
        }

        /// <summary>
        /// 淡入黑幕后关闭面板
        /// </summary>
        public void CloseFormShowFade(UIForm uiForm, Action callBack = null)
        {
            if (blackMask.Showing)
            {
                return;
            }

            SetInteractive(false);

            blackMask.ShowFade(() =>
            {
                SetInteractive(true);
                callBack?.Invoke();
                GameManager.GetGMComponent<UIComponentGM>().CloseUIForm(uiForm);
            });
        }

        /// <summary>
        /// 淡出黑幕后关闭面板
        /// </summary>
        public void CloseFormHideFade(UIForm uiForm, Action callBack = null)
        {
            if (blackMask.Showing)
            {
                return;
            }

            SetInteractive(false);

            blackMask.HideFade(() =>
            {
                SetInteractive(true);
                callBack?.Invoke();
                GameManager.GetGMComponent<UIComponentGM>().CloseUIForm(uiForm);
            });
        }

        /// <summary>
        /// 直接显示黑幕然后关闭
        /// </summary>
        public void CloseFormShowRow(UIForm uiForm, Action callBack = null)
        {
            if (blackMask.Showing)
            {
                return;
            }

            SetInteractive(false);

            blackMask.ShowRow(() =>
            {
                SetInteractive(true);
                callBack?.Invoke();
                GameManager.GetGMComponent<UIComponentGM>().CloseUIForm(uiForm);
            });
        }

        /// <summary>
        /// 直接隐藏黑幕然后关闭
        /// </summary>
        public void CloseFormHideRow(UIForm uiForm, Action callBack = null)
        {
            if (blackMask.Showing)
            {
                return;
            }

            SetInteractive(false);
            blackMask.HideRow(() =>
            {
                SetInteractive(true);
                callBack?.Invoke();
                GameManager.GetGMComponent<UIComponentGM>().CloseUIForm(uiForm);
            });
        }

        // 重置黑幕的显示状态
        public void ResetHideState()
        {
            blackMask.SetHidingState(false);
        }
    }
}