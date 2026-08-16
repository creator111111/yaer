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

            // 重要修改说明（0807 方案 D）：
            // 旧路径 CloseFormShowFade = 全黑后立刻关 Form，黑幕随 Bottom Form 销毁，
            // 对话壳在 Middle，无法做「System 拍1 只见 BG」。
            // 现改为只 ShowFade 全黑并回调；关 Form / System BlackPanel 由 NewGameSceneManager 接管。
            // 替代方案：仍 CloseFormShowFade 再另开 BlackPanel——异步开窗易闪一帧裸景，故不采用。
            componentSystemUI.GetComponent<BlackFadeComponent>().ShowFade(() =>
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
