using Game.GameRuntime.UI.Component.BlackFade;
using Game.GameRuntime.UI.FormLogic.Base;
using UnityEngine;
using Game.GameRuntime.UI.Component;
using Game.GameMgr;
using Game.GameMgr.Component;

namespace Game.GameRuntime.UI.FormLogic.Cartoon
{
    public class NewGameCartoonFormLogic : BaseUIFormLogic
    {
        [SerializeField]
        private UIPointerHoldArea skipHoldArea;
        [SerializeField]
        private CartoonPlayer cartoonPlayer;

        protected internal override void OnInit(object userData)
        {
            base.OnInit(userData);

            GetProxy<NewGameCartoonFormProxy>();

            //GetComponent<AnimationEventComponent>().RegisterEvent("End", s => OnClickBtnSkip());
        }

        protected internal override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            
            // 直接隐藏黑幕
            componentSystemUI.GetComponent<BlackFadeComponent>().HideRow();

            skipHoldArea.onHoldProgressEnd += OnClickBtnSkip;

            cartoonPlayer.PlayCartoon(OnClickBtnSkip);
        }

        private void OnClickBtnSkip()
        {
            cartoonPlayer.StopAllCoroutines();
            skipHoldArea.onHoldProgressEnd -= OnClickBtnSkip;
            componentSystemUI.GetComponent<BlackFadeComponent>().CloseFormShowFade(UIForm, () =>
            {
                GameManager.GetGMComponent<SoundComponentGM>().StopBGM();
                GetProxy<NewGameCartoonFormProxy>().OnFinish();
            });
        }

        public override void PlayerOpenAudio()
        {

        }
    }
}