using System;
using Game.GameRuntime.UI.Component;
using Game.GameRuntime.UI.Component.BlackFade;
using Game.GameRuntime.UI.FormLogic.Base;
using GameFramework.UnityRuntime.Utility;

namespace Game.GameRuntime.UI.FormLogic.Black
{
    public class BlackFormLogic : BaseUIFormLogic
    {
        protected internal override void OnInit(object userData)
        {
            base.OnInit(userData);

            GetProxy<BlackFormProxy>();
        }

        protected internal override void OnOpen(object userData)
        {
            base.OnOpen(userData);

            if (userData is ShowBlackFormArgs args)
            {
                if (args.showType == BlackFadeType.FadeShow)
                {
                    componentSystemUI.GetComponent<BlackFadeComponent>().ShowFade(()=> args.onShowEnd?.Invoke(this));
                }
                else if (args.showType == BlackFadeType.RawShow)
                {
                    componentSystemUI.GetComponent<BlackFadeComponent>().ShowRow(()=> args.onShowEnd?.Invoke(this));
                }
            }
            else
            {
                Log.Error("userData is not ShowBlackPanelArgs");
            }
        }

        public void CloseFormFade(Action action = null)
        {
            componentSystemUI.GetComponent<BlackFadeComponent>().CloseFormHideFade(UIForm, action);
        }

        public override void PlayerOpenAudio()
        {

        }
    }
}