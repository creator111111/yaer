using Game.GameMgr.Component.Archive.ArchiveDataClass.Player;
using Game.GameMgr.Component.UI;
using Game.GameMgr;
using Game.GameRuntime.UI.FormLogic.Menu;
using Game.Static.Path;

namespace Game.GameRuntime.BagPack
{
    public class ItemMap : ItemBase
    {
        public override void OnClick(object data)
        {
            MenuFormLogic menuFormLogic = data as MenuFormLogic;
            GameManager.GetGMComponent<UIComponentGM>().CloseUIForm(menuFormLogic.UIForm);
            // 打开地图前先关闭道具界面
            UIUtils.ClosePanel("ItemShowPanel");
            string uiPrefabPath = UIPrefabPath.GetUIPrefabPath("MapPanel");

            var uiForm = GameManager.GetGMComponent<UIComponentGM>().GetUIForm(uiPrefabPath);
            if (uiForm == null)
            {
                GameManager.GetGMComponent<UIComponentGM>()
                .OpenUIForm(uiPrefabPath, EUIGroup.Middle, new OpenFormArgs()
                {
                    userData = GameManager.GetGameSceneManager().GetArchiveData<PlayerMapData>()
                });
            }
        }
    }
}