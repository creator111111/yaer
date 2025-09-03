using Game.GameMgr.Component.PureMVC;
using Game.GameMgr;
using Game.GameRuntime.UI.FormLogic.Menu;
using System;
using Game.GameRuntime.Entities.Player;
using Game.GameMgr.Component;
using Game.GameMgr.Component.Archive.ArchiveDataClass.Player;
using Game.GameMgr.Component.Archive;

namespace Game.GameRuntime.BagPack
{
    public abstract class ItemBase
    {

        #region ÊôÐÔ

        public PlayerLogic PlayerLogic => GameManager.GetGMComponent<EntityComponentGM>().GetEntityLogic<PlayerLogic>();

        public PlayerBagData PlayerBagData => GameManager.GetGMComponent<ArchiveComponentGM>().GetData<PlayerBagData>();

        #endregion

        public static void OnClick(string name, object data)
        {
            Type type = GetItemType(name);
            if (type == null) return;
            var inst = Activator.CreateInstance(type);
            type.GetMethod("OnClick").Invoke(inst, new object[] { data });
        }

        public static Type GetItemType(string name)
        {
            return Type.GetType("Game.GameRuntime.BagPack.Item" + name);
        }

        public void RefreshUI()
        {
            GameManager
                .GetGMComponent<MVCComponentGM>()
                .GetProxy<MenuFormProxy>()
                .UpdateItemPage();
        }

        public abstract void OnClick(object data);

        public virtual void OnUse() { }
    }

}
