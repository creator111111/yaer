using System;
using System.Collections.Generic;
using Game.GameMgr;
using Game.GameRuntime.UI.Component.BlackFade;
using Game.GameRuntime.UI.FormLogic.Base;
using UnityEngine;

namespace Game.GameRuntime.UI.FormLogic.Init
{
    public class InitFormLogic : BaseUIFormLogic
    {
        [SerializeField]
        private List<GameObject> DisplaySeq;

        private int DisplayProgress;
        /// <summary>编辑器 ESC 跳过时置 true，阻断黑幕链式回调里对 DisplayNextGO / HideCurrentDisplay 的继续执行。</summary>
        private bool abortDisplaySequence;
        /// <summary>保证只通知 ProcedurePreload 一次（正常播完与 ESC 跳过、或动画晚到回调共用）。</summary>
        private bool sequenceEndReported;

        public bool FinishDisplay => DisplaySeq == null || DisplaySeq.Count==0 ? true : DisplayProgress >= DisplaySeq.Count - 1;
        private GameObject CurrentDisplayGO
        {
            get
            {
                if (DisplaySeq == null || DisplayProgress >= DisplaySeq.Count || DisplayProgress < 0) return null;
                return DisplaySeq[DisplayProgress];
            }
        }

        private BlackFadeComponent blackFade;

        protected internal override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            blackFade = componentSystemUI.GetComponent<BlackFadeComponent>();
            DisplayProgress = -1;
            foreach (var item in DisplaySeq)
            {
                item.SetActive(false);
            }
            DisplayNextGO();

            // 检测玩家选择的游戏语言
            var languageType = PlayerPrefs.GetInt("LanguageType", -1);
            var gameLanguage = LanguageEnumType.Chinese;
            if (languageType == -1)
            {
                // 没有设置语言则使用系统语言
                SystemLanguage lang = Application.systemLanguage;
                switch(lang)
                {
                    case SystemLanguage.Chinese:
                        gameLanguage = LanguageEnumType.Chinese;
                        break;
                    case SystemLanguage.English:
                        gameLanguage = LanguageEnumType.English;
                        break;
                    case SystemLanguage.Japanese:
                        gameLanguage = LanguageEnumType.Japanese;
                        break;
                    default:
                        break;
                }
                // 第一次查找语言后自动保存为当前语言
                PlayerPrefs.SetInt("LanguageType", (int)gameLanguage);
                PlayerPrefs.Save();
            }
            else
            {
                // 设置为玩家保存的语言
                gameLanguage = (LanguageEnumType)languageType;
            }
            GameManager.Instance.language = gameLanguage;
        }

        protected internal override void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(elapseSeconds, realElapseSeconds);
#if UNITY_EDITOR
            // 仅在编辑器运行模式下：ESC 跳过 Init 轮播，正式包不包含此逻辑以减小分支与误触风险
            TryEditorSkipInitSequence();
#endif
        }

#if UNITY_EDITOR
        /// <summary>
        /// 编辑器专用：按 ESC 立即结束展示链，等价于正常播完后的「可进入 StartScene」信号。
        /// 替代方案：走 PureMVC 通知 HIDE_INIT_PANEL 并在 Mediator 里调相同结束逻辑；当前工程未接该通知，故在 Form 内直接完成。
        /// </summary>
        private void TryEditorSkipInitSequence()
        {
            if (!Input.GetKeyDown(KeyCode.Escape))
            {
                return;
            }

            if (sequenceEndReported)
            {
                return;
            }

            abortDisplaySequence = true;
            if (DisplaySeq != null)
            {
                foreach (var item in DisplaySeq)
                {
                    if (item != null)
                    {
                        item.SetActive(false);
                    }
                }
            }

            // 避免黑幕状态机与后续 FadeCloseForm 冲突：仅重置内部 busy 标记；链式回调由 abortDisplaySequence 短路
            if (blackFade != null)
            {
                blackFade.ResetHideState();
            }

            CompleteInitSequence();
            Debug.Log("[InitFormLogic] 编辑器：已按 ESC 跳过 InitPanel 展示，将进入 StartScene 流程。");
        }
#endif

        /// <summary>
        /// 通知 ProcedurePreload：Init 展示阶段结束（仅应调用一次）。
        /// </summary>
        private void CompleteInitSequence()
        {
            if (sequenceEndReported)
            {
                return;
            }

            sequenceEndReported = true;
            GetProxy<InitFormProxy>().OnHideEnd();
        }

        /// <summary>
        /// 隐藏上一个物体，显示下一个物体，淡出黑幕
        /// </summary>
        private void DisplayNextGO()
        {
            if (abortDisplaySequence)
            {
                return;
            }

            if (CurrentDisplayGO != null)
            {
                CurrentDisplayGO.SetActive(false);
            }
            DisplayProgress++;
            if (CurrentDisplayGO != null)
            {
                CurrentDisplayGO.SetActive(true);
                if (!FinishDisplay)
                {
                    blackFade.HideFade(HideCurrentDisplay);
                }
                else
                {
                    // 最后一页淡出结束后通知 Preload
                    blackFade.HideFade(CompleteInitSequence);
                }
            }
        }

        /// <summary>
        /// 淡入黑幕
        /// </summary>
        private void HideCurrentDisplay()
        {
            if (abortDisplaySequence)
            {
                return;
            }

            blackFade.ShowFade(DisplayNextGO);
        }

        public void FadeCloseForm(Action action)
        {
            blackFade.CloseFormShowFade(UIForm, action);
        }

        public override void PlayerOpenAudio()
        {

        }
    }
}