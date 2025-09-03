using Game.GameMgr.Component.PureMVC.Base;

namespace Game.GameRuntime.UI.FormLogic.Loading
{
    public class LoadingPanelProxy : BaseProxy
    {
        public LoadingPanelProxy(string proxyName = nameof(LoadingPanelProxy), object data = null) : base(proxyName, data)
        {
        }

        // public void StartLoading(IGamePreloadHandler handler, Action loadEndCallBack = null)
        // {
        //     handler.OnComplete += () => LoadingEnd(loadEndCallBack); // 加载完成自动关闭加载面板
        //     Action showEndCallBack = handler.StartLoad; // 显示完成加载面板才开始加载
        //     SendNotification(NotificationName.UI.SHOW_LOADING_PANEL, showEndCallBack);
        // }
        //
        // public void LoadingEnd(Action callBack = null)
        // {
        //     SendNotification(NotificationName.UI.HIDE_LOADING_PANEL, callBack);
        // }
    }
}