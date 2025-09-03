using Game.GameMgr;
using Game.GameMgr.Component;
using Game.GameMgr.Component.Archive.ArchiveDataClass.BaseDataClass;
using Game.GameMgr.Component.Archive.ArchiveDataClass.Player;
using Game.GameMgr.Component.UI;
using Game.GameRuntime.Entities.Player;
using Game.GameRuntime.GameSceneManager.Base;
using Game.Static.Path;
using NodeCanvas.Framework;
using System;
using UnityEngine;
// 道具使用效果管理器
public class ItemEffectDataMgr
{
    public PlayerLogic playerLogic;
    public PlayerBagData playerBagData;
    public static ItemEffectDataMgr instance;
    public static ItemEffectDataMgr getInstance()
    {
        if (instance == null)
        {
            instance = new ItemEffectDataMgr();
        }
        return instance;
    }

    public PlayerLogic GetPlayerLogic()
    {
        if (playerLogic == null)
        {
            playerLogic = GameManager.GetGMComponent<EntityComponentGM>().GetEntityLogic<PlayerLogic>();
        }
        return playerLogic;
    }
    public PlayerBagData GetPlayerBagData()
    {
        return GameManager.GetGameSceneManager().GetArchiveData<PlayerBagData>();
        // 如果使用下面的做法，会导致this.playerBagData和GameManager获取到的PlayerBagData是两个不同地址的数据，原因在于
        //if (playerBagData == null)
        //{
        //    playerBagData = GameManager.GetGameSceneManager().GetArchiveData<PlayerBagData>();
        //}
        //return playerBagData;
    }

    // 使用HP恢复道具
    public bool UseHpBall()
    {
        if (GetPlayerLogic().healthComponent.IsMax) { return false; }
        if (GetPlayerBagData().TryRemoveMainItem(Game.Static.Enum.Goods.EMainItemName.HpBall, 1))
        {
            GetPlayerLogic().healthComponent.AddHp(9999);
            return true;
        }else
        {
            return false;
        }
    }

    public bool UseMpBall()
    {
        if (GetPlayerLogic().staminaComponent.IsMax) { return false; }
        if (GetPlayerBagData().TryRemoveMainItem(Game.Static.Enum.Goods.EMainItemName.MpBall, 1))
        {
            GetPlayerLogic().staminaComponent.AddStamina(9999);
            return true;
        }
        else
        {
            return false;
        }
    }

    public void UseItem(string itemName)
    {
        if (GetPlayerLogic().isDead) { return; }
        switch (itemName)
        {
            case "HpBall":
                UseHpBall();
                break;
            case "MpBall":
                UseMpBall();
                break;
            default:
                break;
        }
    }

    void ItemTestEffect(int testId)
    {
        Debug.Log("===============当前使用了测试道具,Id:" + testId);
    }
}
