using Game.GameMgr.Component;
using Game.GameRuntime.Entities.Component.Battle.Damage;
using GameFramework.UnityRuntime.Entity;
using System.Collections.Generic;
using UnityEngine;

// 主要用来同时控制玩家和怪物Logic的通用行为
public class BaseEntityLogic : EntityLogic
{
    public Dictionary<string, GameObject> atkCollAreaNodeDict = new Dictionary<string, GameObject>();// 所有类型的攻击碰撞体节点
    public GameObject beHurtEffectNode = null; // 受伤效果节点
    [HideInInspector]
    public GameObject atkCollNodeParent = null; // 攻击碰撞体绑定的父节点,null则直接绑定到整个EntityLogic所在的GameObject上
    public GameObject showdowArea; // 实体的影子，可以为NUll表示没有影子
    public AtkCollsionType curAtkCollsionType = AtkCollsionType.None; // 攻击碰撞体的类型，不同类型碰撞体对不同对象生效
    [HideInInspector]
    public bool isProtect { get; set; } = false; // 是否处于无敌状态
    [HideInInspector]
    public bool isDead; // 是否死亡
    [HideInInspector]
    public bool isNoBreakState = false; // 是否处于霸体状态
    // 角色受伤
    public virtual void HasHurt(DamageData damageData)
    {

    }

    // 实体处于空中时
    public virtual void OnUnIsGround()
    {
        if (showdowArea != null && showdowArea.activeSelf)
        {
            showdowArea.SetActive(false);// 不接触地面时影子需要隐藏
        }
    }

    // 实体处于地面时
    public virtual void OnGroundedEvent()
    {
        if (showdowArea != null && !showdowArea.activeSelf)
        {
            showdowArea.SetActive(true);// 接触地面时影子需要显示出来
        }
    }

    public virtual void PlayAudio(SoundToggleComponent sfxCpn, bool isPlay)
    {
        if (isPlay)
        {
            sfxCpn.PlaySound();
        }
        else
        {
            sfxCpn.StopSound();
        }
    }
}