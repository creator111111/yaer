using System;
using Game.GameRuntime.UI.Component;
using Game.GameRuntime.UI.Component.BlackFade;
using Game.GameRuntime.UI.FormLogic.Base;
using GameFramework.UnityRuntime.Utility;

namespace Game.GameRuntime.UI.FormLogic.Black
{
    public class BlackFormLogic : BaseUIFormLogic
    {
        private BlackFadeComponent blackFadeComponent;
        private float? pendingShowDuration;
        private float? pendingHideDuration;

        protected internal override void OnInit(object userData)
        {
            base.OnInit(userData);

            GetProxy<BlackFormProxy>();
            blackFadeComponent = componentSystemUI.GetComponent<BlackFadeComponent>();
        }

        protected internal override void OnOpen(object userData)
        {
            base.OnOpen(userData);

            if (userData is ShowBlackFormArgs args)
            {
                pendingShowDuration = args.showDuration;
                pendingHideDuration = args.hideDuration;

                if (blackFadeComponent != null
                    && (args.showDuration.HasValue || args.hideDuration.HasValue))
                {
                    float show = args.showDuration ?? blackFadeComponent.GetDefaultShowDuration();
                    float hide = args.hideDuration ?? blackFadeComponent.GetDefaultHideDuration();
                    blackFadeComponent.SetFadeDurations(show, hide);
                }

                if (args.showType == BlackFadeType.FadeShow)
                {
                    blackFadeComponent.ShowFade(() => args.onShowEnd?.Invoke(this));
                }
                else if (args.showType == BlackFadeType.RawShow)
                {
                    blackFadeComponent.ShowRow(() => args.onShowEnd?.Invoke(this));
                }
            }
            else
            {
                Log.Error("userData is not ShowBlackPanelArgs");
            }
        }

        public void CloseFormFade(Action action = null)
        {
            if (blackFadeComponent == null)
            {
                action?.Invoke();
                return;
            }

            if (pendingHideDuration.HasValue)
            {
                blackFadeComponent.SetFadeDurations(
                    blackFadeComponent.GetCurrentShowDuration(),
                    pendingHideDuration.Value);
            }

            blackFadeComponent.CloseFormHideFade(UIForm, () =>
            {
                blackFadeComponent.RestoreDefaultFadeDurations();
                pendingShowDuration = null;
                pendingHideDuration = null;
                action?.Invoke();
            });
        }

        public override void PlayerOpenAudio()
        {

        }
    }
}
