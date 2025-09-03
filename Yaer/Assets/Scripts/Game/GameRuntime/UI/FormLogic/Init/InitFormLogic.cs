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

        /// <summary>
        /// 隐藏上一个物体，显示下一个物体，淡出黑幕
        /// </summary>
        private void DisplayNextGO()
        {
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
                    blackFade.HideFade(GetProxy<InitFormProxy>().OnHideEnd);
                }
            }
        }

        /// <summary>
        /// 淡入黑幕
        /// </summary>
        private void HideCurrentDisplay()
        {
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