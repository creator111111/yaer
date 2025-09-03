using Game.GameMgr;
using Game.GameMgr.Component;
using Game.GameMgr.Component.PureMVC.Base;
using Game.Static.MVC;
using Game.Static.Path;
using UnityEngine.U2D;

namespace Game.GameRuntime.UI.FormLogic.Detail
{
    public class DetailPanelProxy : BaseProxy
    {
        private SpriteAtlas atlas;

        public DetailPanelProxy(string proxyName = nameof(DetailPanelProxy), object data = null) : base(proxyName, data)
        {
        }

        public void UpdateDetail(string itemName)
        {
            if (atlas == null) 
            {
                GameManager.GetGMComponent<ResComponentGM>().LoadAsset<SpriteAtlas>(SpriteAtlasPath.GetPath("MainItemDetailsChar"), spriteAtlas => 
                {
                    atlas = spriteAtlas;
                    SendNotification(NotificationName.UI.UPDATED_DETAIL_PANEL, atlas.GetSprite(itemName));
                });
            }
            else
            {
                SendNotification(NotificationName.UI.UPDATED_DETAIL_PANEL, atlas.GetSprite(itemName));
            }
        }
    }
}