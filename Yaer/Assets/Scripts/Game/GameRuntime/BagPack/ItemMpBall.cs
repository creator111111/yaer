using Game.GameMgr.Component.Archive.ArchiveDataClass.Player;
using Game.GameMgr.Component.Archive;
using Game.GameMgr.Component;
using Game.GameMgr;
using Game.GameRuntime.Entities.Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.GameRuntime.BagPack
{
    public class ItemMpBall : ItemBase
    {
        public override void OnClick(object data)
        {
            //if (PlayerLogic.staminaComponent.IsMax)
            //    return;

            //if (PlayerBagData.TryRemoveMainItem(Static.Enum.Goods.EMainItemName.MpBall, 1))
            //{
            //    PlayerLogic.staminaComponent.AddStamina(114514);
            //    RefreshUI();
            //}
            if (ItemEffectDataMgr.getInstance().UseMpBall())
            {
                RefreshUI();
            }
        }
    }
}