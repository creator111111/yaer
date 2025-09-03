using Game.GameMgr;
using Game.GameMgr.Component.PureMVC;
using Game.GameMgr.Component.UI;
using Game.GameRuntime.GameSceneManager.Base;
using Game.GameRuntime.UI.FormLogic.Tips;
using Game.Static.Path;

namespace Game.GameRuntime.GameSceneManager.Component
{
    /// <summary>
    /// 专门处理提示面板的组件
    /// </summary>
    public class TipsComponentGSM : BaseComponentGSM
    {
        private UIComponentGM uiComponentGM;

        private TipsFormLogic tipsFormLogic;

        public override void OnInit(IGameSceneManager manager)
        {
            base.OnInit(manager);

            uiComponentGM = GameManager.GetGMComponent<UIComponentGM>();
        }

        public void OpenTipsForm(string info, ETipsType tipsType = ETipsType.Item)
        {
            var proxy = GameManager.GetGMComponent<MVCComponentGM>().GetProxy<TipsFormProxy>();
            if (proxy.GetTipsSprite(info) == null)
            {
                return;
            }
            // 已经打开提示面板
            if (tipsFormLogic != null && tipsFormLogic.isActiveAndEnabled)
            {
                tipsFormLogic.AddTipsInfo(info);
            }
            else
            {
                uiComponentGM.OpenUIForm(UIPrefabPath.GetUIPrefabPath("TipsPanel"), EUIGroup.Middle, new OpenFormArgs()
                {
                    userData = new TipsFormArgs()
                    {
                        info = info,
                        type = tipsType
                    },
                    callBack = formLogic => tipsFormLogic = formLogic as TipsFormLogic
                });
            }
        }

        public void OpenTipsFormDaysLater(int days)
        {
            OpenTipsForm($"DaysLater_{days}", ETipsType.Info);
        }

        public void OpenTipsArriveScene(string sceneName)
        {
            OpenTipsForm($"Arrive_{sceneName}", ETipsType.Info);
        }
    }
}