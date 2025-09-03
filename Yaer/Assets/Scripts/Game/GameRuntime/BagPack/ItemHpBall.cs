using Game.GameMgr;
using Game.GameMgr.Component;
using Game.GameMgr.Component.Archive;
using Game.GameMgr.Component.Archive.ArchiveDataClass.Player;
using Game.GameMgr.Component.PureMVC;
using Game.GameRuntime.BagPack;
using Game.GameRuntime.Entities.Player;
using Game.GameRuntime.UI.FormLogic.Menu;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.GameRuntime.BagPack
{
    public class ItemHpBall : ItemBase
    {
        public override void OnClick(object data)
        {
            //if (PlayerLogic.healthComponent.IsMax)
            //    return;

            //if (PlayerBagData.TryRemoveMainItem(Static.Enum.Goods.EMainItemName.HpBall, 1))
            //{
            //    PlayerLogic.healthComponent.AddHp(114514);
            //    RefreshUI();
            //}
            if (ItemEffectDataMgr.getInstance().UseHpBall())
            {
                RefreshUI();
            }
        }
    }
}