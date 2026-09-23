using Game.GameMgr;
using Game.GameMgr.Component;
using Game.GameMgr.Component.PureMVC;
using Game.GameMgr.Component.UI;
using Game.GameRuntime.Entities.Component.Interactive;
using Game.GameRuntime.GameSceneManager.Base;
using Game.GameRuntime.UI.FormLogic.Menu;
using Game.Static.Path;
using UnityEngine;

namespace Game.GameRuntime.GameSceneManager.Component
{
    public class InputComponentGSM : BaseComponentGSM
    {
        private bool isOpenMenu;
        private bool cantOpenMenu;
        private bool cantEKey;
        private InteractiveComponent playerInteractiveComponent;
        private InputComponentGM inputComponentGM;

        private MenuFormLogic menuFormLogic;

        public override void OnInit(IGameSceneManager manager)
        {
            base.OnInit(manager);

            inputComponentGM = GameManager.GetGMComponent<InputComponentGM>();

            inputComponentGM.onEscPressed += OnEscPressed;
            inputComponentGM.onEKeyPressed += OnEKeyPressed;

            GameManager.GetGMComponent<MVCComponentGM>().GetProxy<MenuFormProxy>().onMenuActiveEvent += MenuActiveHandle;
            GameManager.GetGMComponent<ProcedureComponentGM>().onStartLoadingSceneEvent += CantResponse;
            GameManager.GetGMComponent<ProcedureComponentGM>().onCompleteLoadingSceneEvent += AllowResponse;
            SceneManager.GetModule<LoadSceneComponentGSM>().onStartLoadingSceneEvent += CantResponse;
            SceneManager.GetModule<LoadSceneComponentGSM>().onEndLoadingSceneEvent += AllowResponse;
            
            // 默认不能交互（等加载结束 AllowResponse / SetAllowOpenMenu 再开）
            CantResponse();

            Debug.Log("[InputComponentGSM] OnInit subscribed ESC");
        }

        private void MenuActiveHandle(bool value)
        {
            isOpenMenu = value;
        }

        private void AllowResponse()
        {
            cantEKey = false;
            cantOpenMenu = false;
        }
        
        private void CantResponse()
        {
            cantEKey = true;
            cantOpenMenu = true;
        }
        

        /// <summary>
        /// 玩家与场景实体产生交互
        /// </summary>
        private void OnEKeyPressed()
        {
            Debug.Log($"[InputComponentGSM] E pressed. cantEKey={cantEKey}");
            if (cantEKey)
            {
                return;
            }

            var closestComponent = SceneManager.GetFirstCanTouchEntiy(playerInteractiveComponent);
            Debug.Log($"[InputComponentGSM] closestComponent={(closestComponent != null ? closestComponent.gameObject.name : "null")}");
            // 交互最近的对象
            if (closestComponent != null)
            {
                closestComponent.OnInteractive();
            }
        }

        private void OnEscPressed()
        {
            // 诊断：纯 UI 场景若 InitModules 中途异常，本回调根本不会挂上；能打到这里说明订阅成功。
            if (isOpenMenu || cantOpenMenu)
            {
                Debug.Log(
                    $"[InputComponentGSM] ESC ignored. isOpenMenu={isOpenMenu} cantOpenMenu={cantOpenMenu}");
                return;
            }

            Debug.Log("[InputComponentGSM] ESC → OpenUIForm MenuPanel");
            GameManager.GetGMComponent<UIComponentGM>().OpenUIForm(UIPrefabPath.GetUIPrefabPath("MenuPanel"), EUIGroup.Top, new OpenFormArgs()
            {
                callBack = logic => menuFormLogic = logic as MenuFormLogic
            });
        }

        /// <summary>
        /// 0922 关地图等路径：强制恢复「可 ESC 开菜单」两旗。
        /// 不代替店内 <c>SetAllowOpenMenu(false)</c>；仅在明确结束禁菜单 UI 后调用。
        /// </summary>
        public void ForceClearMenuEscGate()
        {
            isOpenMenu = false;
            cantOpenMenu = false;
        }

        public override void OnShutdown()
        {
            base.OnShutdown();

            inputComponentGM.onEscPressed -= OnEscPressed;
            inputComponentGM.onEKeyPressed -= OnEKeyPressed;
            
            GameManager.GetGMComponent<MVCComponentGM>().GetProxy<MenuFormProxy>().onMenuActiveEvent -= MenuActiveHandle;
            GameManager.GetGMComponent<ProcedureComponentGM>().onStartLoadingSceneEvent -= CantResponse;
            GameManager.GetGMComponent<ProcedureComponentGM>().onCompleteLoadingSceneEvent -= AllowResponse;
            SceneManager.GetModule<LoadSceneComponentGSM>().onStartLoadingSceneEvent -= CantResponse;
            SceneManager.GetModule<LoadSceneComponentGSM>().onEndLoadingSceneEvent -= AllowResponse;
        }

        public void SetAllowOpenMenu(bool value)
        {   
            cantOpenMenu = !value;
        }
    }
}