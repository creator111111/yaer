using Game.GameMgr.Component.Archive.ArchiveDataClass.Player;
using Game.GameRuntime.Entities.Component.Anima;
using Game.GameRuntime.Entities.Component.Battle;
using Game.GameRuntime.Entities.Component.Battle.Attack;
using Game.GameRuntime.Entities.Component.Battle.Damage;
using Game.GameRuntime.Entities.Component.CldController;
using Game.GameRuntime.Entities.Component.Health;
using Game.GameRuntime.Entities.Component.Move;
using Game.GameRuntime.Entities.Component.Path;
using Game.GameRuntime.Entities.Component.Physics;
using Game.GameRuntime.Entities.Monster.Slime.Anima;
using Game.GameRuntime.Entities.Monster.Slime.Anima.State;
using Game.GameRuntime.Entities.Player;
using Game.GameRuntime.GameSceneManager.Base;
using Game.Static.Utility;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

// 史莱姆怪物的基础动画枚举
public enum SlimeAniState
{
    Sleep,
    Idle,
    Move,
    Attack,
    Wound,
    Dead,
    JumpAttack,
    BornSubAttack,
    JumpAtkSubState,
}

namespace Game.GameRuntime.Entities.Monster.Slime
{
    public class Slime : BaseMonster, ISlime
    {
        
        // 组件
        [SerializeField] private SpriteRenderer sr;
        [SerializeField] private Transform attack1Pos;
        [SerializeField] private Transform jumpAttackPos;
        
        [SerializeField] private Rigidbody2D bodyRg;
        [SerializeField] private SlimeAnimationEvent animationEvent;
        [SerializeField] private KnockBackComponent knockBackComponent;
        public PathfindingComponent pathfindingCpn;

        // trigger相关
        [Header("FindTarget")] [SerializeField]
        private float findRange;

        [SerializeField] private Transform findArea;

        [Header("AttackArea")] [SerializeField]
        private float attackRange;

        [SerializeField] private Transform attackArea;

        [Header("Born")] [SerializeField] private float triggerBornRange; // 触发出生半径

        [SerializeField] private float bornDownY; // 落地y
        [SerializeField] private Transform triggerBornPos;

        [Header("JumpAttack")] [SerializeField]
        private float attack1Range;

        [SerializeField] private float jumpAttackRange;
        public float jumpHeight;

        [Header("StateEffect")] [SerializeField]
        private Transform weakEffectTsf;

        [SerializeField] private Transform angryEffectTsf;

        // 动画
        [SerializeField] private float moveSpeed;
        [SerializeField] private float fallSpeed;

        //怪物数值
        [SerializeField] private float atkBackDistance;
        [SerializeField] private float attackCd;
        public bool isJumpAttacking;
        public bool isFallDownAtk; // 是否处于下落攻击中
        private bool attacking;

        // 动画状态枚举对应的动画名称
        private Dictionary<SlimeAniState, string> stateNameData = new Dictionary<SlimeAniState, string>()
        {
            { SlimeAniState.Sleep, "Sleep"}, { SlimeAniState.Idle, "Idle"},{ SlimeAniState.Move, "Move"},
            { SlimeAniState.Attack, "Attack"},{ SlimeAniState.Wound, "Wound"},{ SlimeAniState.Dead, "Dead"},
            { SlimeAniState.JumpAtkSubState, "JumpAtkSubState"},{ SlimeAniState.BornSubAttack, "BornSubAttack"},
        };
        public SlimeAniState baseAniState;// 初始动画状态

        public BaseEntityLogic atkTargetLogic; // 攻击目标对象

        // 状态
        private int dir;
        private bool isBorn;
        private int jumpAttackValue;

        public Rigidbody2D BodyRg => bodyRg;
        public Collider2D FootCld => footCld;
        public float AttackRange => attackRange;
        public float BornDownY => bornDownY;
        public float FallSpeed => fallSpeed;
        public float flyMoveSpeedX; // 飞行过程中的速度
        private float HpRate => curHp / (float)maxHp;

        protected int Hp
        {
            get => curHp;
            set
            {
                curHp = value;
                if (HpRate <= 0)
                {
                    curHp = 0;
                    Dead();
                    return;
                }

                SetBuff(HpRate);
            }
        }

        public int Dir
        {
            get => dir;
            set
            {
                dir = value;

                // 转向
                transform.rotation = Quaternion.Euler(0, value == 1 ? 180 : 0, 0);
            }
        }

        protected override void Update()
        {
            base.Update();
            return;
            if (isDead) return;

            base.Update();

            if (attackCdTimer > 0) attackCdTimer -= Time.deltaTime;
        }

        protected override void FixedUpdate()
        {
            if (isDead) return;
            if (SceneManager.GetSceneObjIsPause()) { return; }
            base.FixedUpdate();

            // 禁用速度
            //bodyRg.velocity = Vector2.zero;
        }

        public override void OnShutDown()
        {
            base.OnShutDown();
            
            attack1Pos = null;
            jumpAttackPos = null;
            sr = null;
            bodyCld = null;
            footCld = null;
            bodyRg = null;
            animationEvent = null;
            knockBackComponent = null;
            pathfindingCpn = null;
        }

        public IPlayer Target { get; private set; }

        protected internal override void OnInit(object userData)
        {
            base.OnInit(userData);

            // GroundCld 图层 OnlyMapObj、默认非 Trigger、盒体很大。
            // PlayerFoot 可能与之发生接触，人被托在史莱姆壳上却又不进 GroundLayerMask → JumpFall/DamageFlyFall 死等 IsGrounded。
            // 怪落地靠 GroundChecker + GravityScale=0，GroundCld 本意是「只和地图碰」；改为 Trigger 后不再当玩家踏板。
            // 替代方案：全局 IgnoreLayerCollision(PlayerFoot, OnlyMapObj)（OPEN_QUESTIONS Q2，会影响虫卵/天琬挡板，本期不采用）；
            // 或恢复 PlayerBodyCollider 挤出订阅（Q5，本期不恢复整套）。
            if (groundCld != null)
            {
                groundCld.isTrigger = true;
            }

            // 组件初始化
            // depthCpn.Init(sr, bodyRg, bodyCld, footCld);
            //knockBackComponent.Init(BodyRg);
            // pathfindingCpn.Init(bodyRg, moveSpeed);
            
            componentSystem.GetComponent<HealthComponent>().onHpIsZero += OnDead;
            componentSystem.GetComponent<BattleComponent>().OnApplyFinalDamage += OnApplyFinalDamage;
            componentSystem.GetComponent<BattleComponent>().OnPlayImpactEffects += OnPlayImpactEffects;
            componentSystem.GetComponent<BattleComponent>().OnApplyStatusEffects += OnApplyStatusEffects;
            componentSystem.GetComponent<MoveComponent>().GroundedEvent += OnGroundedEvent;
            componentSystem.GetComponent<MoveComponent>().UnGroundedEvent += OnUnIsGround;
            knockBackComponent = componentSystem.GetComponent<KnockBackComponent>();
            bodyRg = GetComponent<Rigidbody2D>();
            knockBackComponent.Init(bodyRg);
            knockBackComponent.SetSceneMgr(sceneManager as BaseGameSceneManager);
            // 事件初始化
            animationEvent.onAttack = AttackDetect;

            // 数值初始化
            Dir = -1;
            animator.SetBool("Idle", true);
            initBaseData(1);// 初始化基础数据并设置怪物ID
            jumpAttackValue = (int)(atkValue * 1.5);
            baseMoveSpeed = 1f;

            showdowArea.SetActive(false);

            // 初始记录攻击碰撞体
            var skillInfo = UIUtils.findChild(gameObject, "SkillInfos");
            if (skillInfo != null)
            {
                var atkNode_1 = UIUtils.findChild(skillInfo, "CollArea_NorAtk");
                if (atkNode_1 != null) { atkCollAreaNodeDict["NorAtk"] = atkNode_1; }
                var atkNode_2 = UIUtils.findChild(skillInfo, "CollArea_JumpAtk");
                if (atkNode_2 != null) { atkCollAreaNodeDict["JumpAtk"] = atkNode_2; }
                var atkNode_3 = UIUtils.findChild(skillInfo, "CollArea_FallAtk");
                if (atkNode_3 != null) { atkCollAreaNodeDict["FallAtk"] = atkNode_3; }
            }
        }

        public override void OnDead()
        {
            base.OnDead();
            // 死亡后取消重力影响并设置为不阻挡类型
            //componentSystem.GetComponent<MoveComponent>().canGravity = false;
            bodyCld.isTrigger = true;
            footCld.isTrigger = true;
            
            var csAnimator = componentSystem.GetComponent<SlimeCsAnimator>();
            if (isJumpAttacking && isFallDownAtk)
            {
                isJumpAttacking = false;
                isFallDownAtk = false;
                var stateMachine = csAnimator.CurrentCsRuntimeController.ExitCurrentSubStateMachine();
                stateMachine.ChangeState<SlimeDeadState>();
            }
            else
            {
                csAnimator.ChangeState<SlimeDeadState>();
            }
            componentSystem.GetComponent<CldControllerComponent>().SetActiveAll(false);

            // 记录成就数据
            AchievementDataMgr.getInstance().RecordAchievementProgress(AchievementType.KillSlime_1, 1);
            AchievementDataMgr.getInstance().RecordAchievementProgress(AchievementType.KillSlime_2, 1);
        }

        private void OnApplyStatusEffects(DamageData data)
        {
            if (isDead == false)
            {
                componentSystem.GetComponent<HealthComponent>().TakeDamage(data.baseDamage);
                if (isDead) { return; } // 受伤之后死亡则不需要走下面的逻辑
                // 播放动画
                var csAnimator = componentSystem.GetComponent<SlimeCsAnimator>();
                // 非置空状态才能设置受伤状态
                if (!isJumpAttacking && !isFallDownAtk)
                {
                    var stateMachine = csAnimator.CurrentCsRuntimeController.ExitCurrentSubStateMachine();
                    stateMachine.ChangeState<SlimeWoundState>();
                    if (data.breakHight > 0 && data.attackType == AttackType.NormalType)
                    {
                        var dirPos = data.dirPos * -1; // 击退方向和伤害来源方向是相反的
                        knockBackComponent.SetKnockBaseData(data.breakHight, data.breakTime);
                        knockBackComponent.ApplyKnockBack(dirPos, data.breakWidth);
                    }
                }
            }
        }

        private void OnPlayImpactEffects(DamageData data)
        {
        }

        private void OnApplyFinalDamage(DamageData data)
        {
            
        }


        // --------------------------------------------------------------------------------
        // unity生命周期
        protected override void FindCpn()
        {
            base.FindCpn();

            attack1Pos = transform.Find("AttackPos/Attack1");
            jumpAttackPos = transform.Find("AttackPos/JumpAttack");
            sr = transform.Find("Animation").GetComponent<SpriteRenderer>();
            bodyCld = transform.Find("Cld/Body").GetComponent<Collider2D>();
            footCld = transform.Find("Cld/Foot").GetComponent<Collider2D>();
            bodyRg = GetComponent<Rigidbody2D>();
            animationEvent = GetComponent<SlimeAnimationEvent>();
           
            pathfindingCpn = GetComponent<PathfindingComponent>();
        }


        public void SetVelocity(Vector3 v)
        {
            bodyRg.MovePosition(transform.position + v);
        }


        /// <summary>
        ///     面向目标
        /// </summary>
        public void LookAtTarget()
        {
            if (Target == null) return;
            var v = Target.GameObject.transform.position.x - transform.position.x;

            // 超过阈值才转向
            if (Mathf.Abs(v) > 0.25f) Dir = v > 0f ? 1 : -1;
        }

        private Vector2 GetAttackDir()
        {
            if (Target.GameObject.transform.position.x > transform.position.x) return Vector2.right;

            return Vector2.left;
        }

        //-----------------------------------------------------------------------------------
        private void SetBuff(float hpRate)
        {
            // if (hpRate > 0.5f && hpRate <= 0.75f)
            // {
            //     // 只添加一次
            //     if (SceneManager.GetSubManager<IBuffManager>().GetBuff<SlimeWeakBuff>(this) != null) return;
            //
            //     var buff = SceneManager.GetSubManager<IBuffManager>().AddBuff<SlimeWeakBuff>(this);
            //     var effect = buff?.CreateEffect(weakEffectTsf);
            //     if (effect)
            //     {
            //         effect.transform.localScale = Vector3.one * 1.5f;
            //         effect.FollowSrSortLayer(sr, true);
            //         effect.Play(5);
            //     }
            // }
            // else if (hpRate > 0.25f && hpRate <= 0.5f)
            // {
            //     // 只添加一次
            //     if (SceneManager.GetSubManager<IBuffManager>().GetBuff<SlimeAngryBuff>(this) != null) return;
            //
            //     // 改变buff
            //     SceneManager.GetSubManager<IBuffManager>().RemoveBuff(this, nameof(SlimeWeakBuff));
            //     var buff = SceneManager.GetSubManager<IBuffManager>().AddBuff<SlimeAngryBuff>(this);
            //
            //     var effect = buff?.CreateEffect(angryEffectTsf);
            //     if (effect)
            //     {
            //         effect.transform.localScale = Vector3.one * 2;
            //         effect.FollowSrSortLayer(sr, true);
            //         effect.Play(5);
            //     }
            // }
            // else if (hpRate > 0 && hpRate <= 0.25f)
            // {
            //     // 只添加一次
            //     if (SceneManager.GetSubManager<IBuffManager>().GetBuff<SlimeTiredBuff>(this) != null) return;
            //
            //     // 改变buff
            //     SceneManager.GetSubManager<IBuffManager>().RemoveBuff(this, nameof(SlimeAngryBuff));
            //
            //     var buff = SceneManager.GetSubManager<IBuffManager>().AddBuff<SlimeTiredBuff>(this);
            //     buff.CreateEffect(sr, GetTsf("BuffEffectPos/Tired"));
            // }
        }

#if UNITY_EDITOR
        [Header("Gizmos")] [SerializeField] private bool findGizmos;

        [SerializeField] private bool bornGizmos;
        [SerializeField] private bool fallGizmos;
        [SerializeField] private bool attackGizmos;
        [SerializeField] private bool jumpAttackGizmos;

        private void OnDrawGizmos()
        {
            if (findGizmos && findArea)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(findArea.position, findRange);
            }

            if (bornGizmos && triggerBornPos)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(transform.position, triggerBornRange);
            }

            if (attackGizmos && attackArea)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(attackArea.position, attackRange);
            }

            if (attackGizmos && attack1Pos)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(attack1Pos.position, attack1Range);
            }

            if (jumpAttackGizmos && jumpAttackPos)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(jumpAttackPos.position, jumpAttackRange);
            }

            if (fallGizmos)
            {
                // 落地y
                Gizmos.color = Color.green;

                var position = transform.position;
                Gizmos.DrawLine(new Vector3(position.x - 2, position.y + bornDownY), new Vector3(position.x + 2, position.y + bornDownY));
            }

            // 跳跃攻击高度
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.position + new Vector3(0, jumpHeight));
        }
#endif

        //-----------------------------------------------------------------------------------


        #region 查找和攻击

        /// <summary>
        ///     判断攻击动作
        /// </summary>
        /// <returns>攻击类型</returns>
        public int AttackAction()
        {
            // if (attackCdTimer <= 0)
            //     // 判断是否相同纵深
            //     if (Target.IsInSameDepth(this))
            //         // 攻击
            //         if (Mathf.Abs(Target.GameObject.transform.position.x - transform.position.x) < AttackRange &&
            //             attackCdTimer <= 0)
            //         {
            //             var r = Random.Range(0, 2);
            //             return r;
            //         }

            return -1;
        }

        public bool JumpAttackDetect()
        {
            // 碰撞到玩家
            if (bodyCld.IsTouchingLayers(1 << LayerMask.NameToLayer("Player")) && Target.IsInSameDepth(Y))
            {
                WoundedTarget();
                return true;
            }

            return false;
        }

        /// <summary>
        ///     攻击1范围检测
        /// </summary>
        private void AttackDetect()
        {
            if (Physics2DUtility.CircleDetectSingle(attack1Pos.position, attack1Range, "Player")) WoundedTarget();
        }

        private void WoundedTarget()
        {
            Target.Wound(atkValue, GetAttackDir(), atkBackDistance);
        }


        public override Vector2 Wound(int value, Vector2 dir, float backDistance)
        {
            if (isDead) return default;

            Hp -= value;
            if (Hp <= 0) return default;

            // 跳跃攻击霸体
            if (!isJumpAttacking)
            {
                componentSystem.GetComponent<BaseCsAnimator>().ChangeState<SlimeWoundState>();

                // 击退
                knockBackComponent.ApplyKnockBack(dir, backDistance);
            }

            return GetWoundPosTsf(dir);
        }

        private void Dead()
        {
            Target = null;
            IsDead = true;
            footCld.isTrigger = true;
            componentSystem.GetComponent<BaseCsAnimator>().ChangeState<SlimeDeadState>();
        }

        //public IPlayer FinePlayer()
        //{
        //    if (IsDead) return null;

        //    if (findArea is null) return null;

        //    // 查找范围是否有目标
        //    if (Target == null)
        //    {
        //        var playerClds = Physics2DUtility.CircleDetectMulti(findArea.position, findRange, "Player");
        //        if (playerClds != null)
        //        {
        //            foreach (var cld in playerClds)
        //            {
        //                if (cld is null) break;
        //                Target = GameObjectUtility.GetParentComponent<IPlayer>(cld.transform, tag: "Player");
        //                if (Target != null) break;
        //            }

        //            // 检测到玩家
        //            if (Target != null)
        //            {
        //                componentSystem.GetComponent<BaseCsAnimator>().ChangeState<SlimeMoveState>();
        //                return Target;
        //            }
        //        }
        //    }
        //    else
        //    {
        //        if (Target.GameObject)
        //        {
        //            // 已经有目标判断是否超出索敌范围
        //            if (Vector2.Distance(Target.GameObject.transform.position, transform.position) > findRange)
        //            {
        //                Target = null;
        //                componentSystem.GetComponent<BaseCsAnimator>().ChangeState<SlimeIdleState>();
        //                return null;
        //            }

        //            return Target;
        //        }
        //    }

        //    return null;
        //}

        public bool IsTriggerBorn()
        {
            if (isBorn) return false;

            if (triggerBornPos is null) return false;

            var player = Physics2DUtility.CircleDetectSingle(triggerBornPos.position, triggerBornRange, "Player");
            if (player is null == false)
            {
                isBorn = true;
                return true;
            }

            return false;
        }

        #endregion

        // 获取当前某个动画状态的名称
        public string getAniNameByState(SlimeAniState state)
        {
            if (stateNameData.ContainsKey(state))
            {
                return stateNameData[state];
            }
            else
            {
                return "";
            }
        }

        // 当角色触碰地面时
        public override void OnGroundedEvent()
        {
            base.OnGroundedEvent();
            if (knockBackComponent != null)
            {
                knockBackComponent.StopKnockBackEffect();
            }
        }

        public override void OnMonsterStateChange()
        {
            base.OnMonsterStateChange();
        }

        public override void PlayDeadSfx(bool isPlay = true)
        {
            base.PlayDeadSfx(isPlay);
            var baseName = "史莱姆死亡{0}.mp3";
            var randomIndex = GameTools.getRandomIntNum(1, 2);
            var realResName = string.Format(baseName, randomIndex);
            commonSfxCpn.ChangeSoundRes(realResName);
            PlayAudio(commonSfxCpn, isPlay);
        }
    }
}