using Game.GameMgr.Component;
using Game.GameMgr;
using Game.GameMgr.Component.Archive.ArchiveDataClass.BaseDataClass;
using Game.GameRuntime.Entities.Player;
using Game.Static.Enum;
using System;
using UnityEngine.Rendering;

//   玩家指引管理器
public class PlayerGuideMgr
{
    public bool inShowJumpTips { get; set; } // 是否在提示跳跃
    public bool inShowSquatTips { get; set; } // 是否在提示蹲下
    public bool inShowNorAtkTips { get; set; } // 是否提示普通攻击
    public bool inShowSmashAtkTips { get; set; } // 是否提示重击
    public bool inShowDashAtkTips { get; set; } // 是否提示冲刺
    public bool inShowSitTips { get; set; } // 是否提示坐下


    public readonly string JUMP_GUIDE = "Jump";
    public readonly string SQUAT_GUIDE = "Squat";
    public readonly string NORATK_GUIDE = "NorAtk";
    public readonly string SMASHATK_GUIDE = "SmashAtk";
    public readonly string DASHATK_GUIDE = "DashAtk";
    public readonly string SIT_GUIDE = "Sit";

    public static PlayerGuideMgr instance;
    public static PlayerGuideMgr getInstance()
    {
        if (instance == null)
        {
            instance = new PlayerGuideMgr();
        }
        return instance;
    }

    public void PraseActName(string value)
    {
        inShowJumpTips = JUMP_GUIDE == value;
        inShowSquatTips = SQUAT_GUIDE == value;
        inShowNorAtkTips = NORATK_GUIDE == value;
        inShowSitTips = SIT_GUIDE == value;
        inShowSmashAtkTips = SMASHATK_GUIDE == value;
        inShowDashAtkTips = DASHATK_GUIDE == value;
        if (inShowJumpTips)
        {
            // 提示跳跃
            ShowPlayerKeyTipsNode(ControlInputType.Jump);
        }
        else if (inShowSquatTips)
        {
            ShowPlayerKeyTipsNode(ControlInputType.Squat);
        }
        else if (inShowNorAtkTips)
        {
            ShowPlayerKeyTipsNode(ControlInputType.NormalAttack);
        }
        else if (inShowSmashAtkTips)
        {
            ShowPlayerKeyTipsNode(ControlInputType.SmashAttack);
        }
        else if (inShowDashAtkTips)
        {
            ShowPlayerKeyTipsNode(ControlInputType.DashAttack);
        }
        else if (inShowSitTips)
        {
            ShowPlayerKeyTipsNode(ControlInputType.SitDown);
        }
    }

    // 去掉某个按键提示
    public void RemoveKeyTips(string value)
    {
        inShowJumpTips = JUMP_GUIDE == value ? false : inShowJumpTips;
        inShowSquatTips = SQUAT_GUIDE == value ? false : inShowJumpTips;
        inShowNorAtkTips = NORATK_GUIDE == value ? false : inShowNorAtkTips;
        inShowSitTips = SIT_GUIDE == value ? false : inShowSitTips;
        inShowDashAtkTips = DASHATK_GUIDE == value ? false : inShowDashAtkTips;
        inShowSmashAtkTips = SMASHATK_GUIDE == value ? false : inShowSmashAtkTips;

        var playerLogic = GameManager.GetGMComponent<EntityComponentGM>().GetEntityLogic<PlayerLogic>();
        if (playerLogic) { playerLogic.ShowActionKeyTipsNode(false); }
    }

    public void RemoveAllKeyTips()
    {
        inShowJumpTips = false;
        inShowSquatTips = false;
        inShowNorAtkTips = false;
        inShowSmashAtkTips = false;
        inShowDashAtkTips = false;
        inShowSitTips = false;
        var playerLogic = GameManager.GetGMComponent<EntityComponentGM>().GetEntityLogic<PlayerLogic>();
        if (playerLogic && !playerLogic.isDead) { playerLogic.ShowActionKeyTipsNode(false); }
    }

    // 是否处于任意指引状态中
    public bool hasAnyKeyTips()
    {
        return inShowJumpTips || inShowSquatTips || inShowNorAtkTips || inShowSitTips
            || inShowDashAtkTips || inShowSmashAtkTips;
    }

    public void ShowPlayerKeyTipsNode(ControlInputType controlInputType)
    {
        var playerLogic = GameManager.GetGMComponent<EntityComponentGM>().GetEntityLogic<PlayerLogic>();
        if (playerLogic && !playerLogic.isDead)
        {
            var entityControll = playerLogic.GetComponent<BaseEntityControll>();
            if (entityControll != null)
            {
                playerLogic.canTouchObj = entityControll.interactiveComponent;
                playerLogic.ShowActionKeyTipsNode(true, controlInputType);
            }
        }
    }
}
