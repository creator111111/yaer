using Game.GameMgr;
using Game.GameMgr.Component.Archive.ArchiveDataClass.Player;
using Game.GameMgr.Component.PureMVC;
using Game.GameMgr.Component.UI;
using Game.GameRuntime.UI.FormLogic.Menu;
using Game.Static.Path;

namespace Game.GameRuntime.BagPack
{
    /// <summary>
    /// 背包「地图」道具：关菜单/道具页后打开 <c>MapPanel</c>。
    /// </summary>
    /// <remarks>
    /// 0922 背包点地图后 ESC 不开菜单：关菜单须显式清 <c>isOpenMenu</c>；
    /// 地图已在栈上时禁止「只关菜单、不开图」留下 <c>cantOpenMenu</c> 体感「系统没了」。
    /// </remarks>
    public class ItemMap : ItemBase
    {
        public override void OnClick(object data)
        {
            var uiGm = GameManager.GetGMComponent<UIComponentGM>();
            if (uiGm == null)
            {
                return;
            }

            // 关菜单：依赖 OnClose→OnMenuActive(false)；再显式清一次，防 Close 异步/漏回调导致 isOpenMenu 残留
            MenuFormLogic menuFormLogic = data as MenuFormLogic;
            if (menuFormLogic != null && menuFormLogic.UIForm != null)
            {
                uiGm.CloseUIForm(menuFormLogic.UIForm);
            }

            ForceClearMenuActiveFlag();

            // 打开地图前先关闭道具界面
            UIUtils.ClosePanel("ItemShowPanel");

            string uiPrefabPath = UIPrefabPath.GetUIPrefabPath("MapPanel");
            var existingMap = uiGm.GetUIForm(uiPrefabPath);
            var mapArgs = new OpenFormArgs()
            {
                userData = GameManager.GetGameSceneManager()?.GetArchiveData<PlayerMapData>()
            };

            if (existingMap == null)
            {
                uiGm.OpenUIForm(uiPrefabPath, EUIGroup.Middle, mapArgs);
                return;
            }

            // 二次点击：地图仍在栈上。旧逻辑直接 return → 菜单没了、Map 仍锁 cantOpenMenu。
            // 关后再开，走完整 OnOpen（AllowOpenMenu(false)+Pause），保证地图可见可关。
            // 替代：只 Refocus 不重建——若 Form 半关闭态仍不可见，故选关开。
            uiGm.CloseUIForm(uiPrefabPath);
            uiGm.OpenUIForm(uiPrefabPath, EUIGroup.Middle, mapArgs);
        }

        /// <summary>
        /// 强制通知菜单已非 Active，清 <see cref="InputComponentGSM"/> 的 <c>isOpenMenu</c>。
        /// </summary>
        private static void ForceClearMenuActiveFlag()
        {
            var mvc = GameManager.GetGMComponent<MVCComponentGM>();
            var proxy = mvc != null ? mvc.GetProxy<MenuFormProxy>() : null;
            proxy?.OnMenuActive(false);
        }
    }
}
