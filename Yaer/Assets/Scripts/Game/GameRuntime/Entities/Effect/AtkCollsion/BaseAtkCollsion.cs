using Game.GameMgr.Component;
using Game.GameMgr;
using Game.GameRuntime.Entities.Base.BaseSceneObj;
using Game.GameRuntime.Entities.Component.Battle;
using Game.GameRuntime.Entities.Component.Battle.Attack;
using Game.GameRuntime.Entities.Component.Battle.Damage;
using Game.GameRuntime.Entities.Component.PhysicsDetect;
using Game.GameRuntime.Entities.Monster;
using Game.GameRuntime.Entities.Player;
using GameFramework.UnityRuntime.Entity;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Game.Static.Enum;

public enum AtkCollsionType
{
    None, // 未知类型
    Player, // 玩家
    Enemy, // 敌人
    Neutral, // 中立生物，同时攻击玩家和敌人
}

public enum AtkDirCheckType
{
    Entity, // 按所附加的实体的位置判断
    Collider, // 按照碰撞体本身
}

// 控制攻击碰撞区域逻辑的脚本
public class BaseAtkCollsion : MonoBehaviour
{
    public EntityLogic entityLogic; // 当前碰撞体所属的实体节点,玩家或者怪物Logic等
    public new Collider2D collider2D; // 碰撞体组件

    public AtkCollsionType atkCollsionType; // 碰撞体的类型
    public int damage = 0; // 本次伤害数值
    public int damage_2 = 0;
    public int damage_3 = 0;
    public int damage_4 = 0;
    public bool damageIsRight; // 是否是来自右边的伤害
    public AttackType atkType = AttackType.NormalType; // 攻击类型
    bool isPlayerDashAtk = false; // 是否是玩家的冲刺攻击
    string atkSkillName; // 本次攻击的技能名称
    // ================start下面两个变量是配合KnockBackComponent组件使用的
    public float breakWidth = 0; // 击飞目标的距离
    public float breakHight = 0; // 击飞目标的高度
    public float breakTime = 0; // 击飞目标的持续时间
    // ======================end

    DamageData damageData;// 伤害详细数据

    public List<EntityLogic> allHurtEntityLogic; // 当前碰撞体已经碰撞过的实体
    public AtkDirCheckType dirCheckType = AtkDirCheckType.Entity; // 攻击方向判断类型,默认根据实体位置来判断
    // Start is called before the first frame update
    void Start()
    {
        if (collider2D == null)
        {
            collider2D = gameObject.GetComponent<Collider2D>();
        }
        damageData = new DamageData()
        {
            baseDamage = damage,
            attackType = atkType,
            atkSkillName = atkSkillName,
            breakHight = breakHight,
            breakTime = breakTime,
            breakWidth = breakWidth > 0 ? breakWidth : 1,// 没有设置数值则取默认值
        };
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnEnable()
    {
        // 因为静止的碰撞体不会进行碰撞检测，所以每次激活碰撞体都手动检测一次
        // 延迟一帧确保物理系统已更新
        StartCoroutine(ManualTriggerCheck());
        
    }

    IEnumerator ManualTriggerCheck()
    {
        yield return new WaitForFixedUpdate();
        Collider2D[] overlappingColliders = new Collider2D[100];
        ContactFilter2D filter = new ContactFilter2D();
        filter.useTriggers = true;
        filter.layerMask = 1 << gameObject.layer;
        int count = Physics2D.OverlapCollider(
            collider2D,
            filter,
            overlappingColliders
        );
        // 处理检测到的碰撞体
        for (int i = 0; i < count; i++)
        {
            Collider2D other = overlappingColliders[i];
            if (other == null || other == collider2D || other.gameObject.layer != gameObject.layer)
            {
                continue;
            }
            // 触发自定义事件（模拟OnTriggerEnter2D）
            OnTriggerEnter2D(other);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 根据自身类型判断是否能够伤害碰撞目标
        var enetityLogic = collision.GetComponent<ColliderResponder>()?.GetEntityLogic() as BaseEntityLogic;
        if (!enetityLogic) return;
        if (enetityLogic.isProtect) { return; }
        // 编辑器「玩家无敌」：与 isProtect 分离，避免与剧情/受击状态机抢写无敌标记
        if (enetityLogic is PlayerLogic hurtPlayer && hurtPlayer.EditorInvincible)
        {
            return;
        }

        if ((atkCollsionType == AtkCollsionType.Player && enetityLogic is BaseMonster)
            || (atkCollsionType == AtkCollsionType.Enemy && enetityLogic is PlayerLogic)
            || (atkCollsionType == AtkCollsionType.Neutral && (enetityLogic is PlayerLogic || enetityLogic is BaseMonster)))
        {
            if (entityLogic == enetityLogic) {  return; } // 不能打自己
            if (entityLogic == null) { return; }
            // 默认只能对同一个目标造成一次伤害
            if (allHurtEntityLogic.Contains(enetityLogic))
            {
                return;
            }
            // 设置本次伤害的方向

            if (dirCheckType == AtkDirCheckType.Entity)
            {
                // 使用实体本身的坐标来判断
                damageIsRight = entityLogic.transform.position.x >= collision.gameObject.transform.position.x;
            }
            else
            {
                // 使用碰撞体本身位置来判断
                damageIsRight = collider2D.bounds.center.x >= collision.bounds.center.x;
            }
            var dirVector = damageIsRight ? new Vector2(1, 0) : new Vector2(-1, 0);
            damageData.dirPos = dirVector;
            damageData.atkObjName = collision.gameObject.name;
            damageData.atkCollsionType = atkCollsionType;
            enetityLogic.HasHurt(damageData);
            allHurtEntityLogic.Add(enetityLogic);
            // 添加伤害特效
            Vector2 pointOnB = collision.ClosestPoint(collider2D.bounds.center);
            Vector2 pointOnA = collider2D.ClosestPoint(collision.bounds.center);
            Vector2 contactPoint = (pointOnA + pointOnB) * 0.5f;
            //var localPos = enetityLogic.gameObject.transform.InverseTransformPoint(contactPoint);
            if (entityLogic is PlayerLogic)
            {
                UIUtils.addBehurtAnimationEffect(AniEffectType.PlayerNorAtk, enetityLogic, contactPoint);
            }
            else
            {
                // 中立生物攻击造成的特效不同
                var baseEntityLogic = (BaseEntityLogic)entityLogic;
                var effectType = baseEntityLogic.curAtkCollsionType == AtkCollsionType.Neutral ? AniEffectType.PlayerNorAtk : AniEffectType.PlayerBeHurt;
                if (enetityLogic is PlayerLogic) { effectType = AniEffectType.PlayerBeHurt; }// 玩家的受伤效果不会变
                UIUtils.addBehurtAnimationEffect(effectType, enetityLogic, contactPoint);
            }
        }
    }

    public void initAtkDataByName(EntityLogic entityLogic, AtkCollsionType atkType, string atkTypeName)
    {
        if (this.entityLogic != null) { return; } // 不能重复初始化
        this.entityLogic = entityLogic;
        atkCollsionType = atkType;
        // 设置本次攻击伤害相关的数据
        damage = getRealDamage();
        atkSkillName = atkTypeName;// 记录本次攻击的名称
        if (entityLogic is PlayerLogic && atkTypeName == "DashAtk")
        {
            isPlayerDashAtk = true;
        }
    }

    int getRealDamage()
    {
        var hardCompont = GameManager.GetGMComponent<HardComponentGM>();
        var gameHard = hardCompont.Hard;
        Dictionary<EGameHard, int> damageDatas = new Dictionary<EGameHard, int>() {
            { EGameHard.Easy, damage }, { EGameHard.Normal, damage_2 },
            { EGameHard.Hard, damage_3 }, { EGameHard.Hardest, damage_4 },
        };
        if (damageDatas.ContainsKey(gameHard))
        {
            return damageDatas[gameHard];
        }
        else
        {
            return 0;
        }
    }

    public void setDamageValue(int damageValue)
    {
        damage = damageValue;
        damageData.baseDamage = damageValue;
    }

    public void clearData()
    {
        allHurtEntityLogic.Clear();
    }
}
