using Game.GameMgr.Component;
using Game.GameMgr.Component.Archive.ArchiveDataClass.Player;
using Game.GameRuntime.Entities.Base;
using Game.GameRuntime.Entities.Component.Anima;
using Game.GameRuntime.Entities.Component.Battle;
using Game.GameRuntime.Entities.Component.Battle.Attack;
using Game.GameRuntime.Entities.Component.Battle.Damage;
using Game.GameRuntime.Entities.Component.Health;
using Game.GameRuntime.Entities.Component.Move;
using Game.GameRuntime.Entities.Component.Physics;
using Game.GameRuntime.Entities.Component.PhysicsDetect;
using Game.GameRuntime.Entities.Monster.BossMogut;
using Game.GameRuntime.Entities.Monster.WoodWorm.Components.Anima;
using Game.GameRuntime.Entities.Monster.WoodWorm.Components.Anima.State;
using Game.GameRuntime.Entities.Monster.WoodWormRoot;
using Game.GameRuntime.Entities.Monster.WormEgg;
using Game.GameRuntime.GameSceneManager.Base;
using GameFramework.UnityRuntimeExtend.Component;
using UnityEngine;

namespace Game.GameRuntime.Entities.Monster.WoodWorm
{
    public class WoodWormLogic : BaseMonster
    {
        public bool eggBorn; // 虫卵孵化
        public bool rootBorn; // 虫巢孵化
        public bool bossMogutBorn; // BOSS产生

        private KnockBackComponent knockBackComponent;

        public WormEggLogic bornEggLogic; // 诞生的虫蛋,可以为null
        public WoodWormRootLogic bornRootLogic; // 诞生的虫巢,可以为null
        public BossMogutLogic bornMogutLogic; // 诞生的来源，可以为null

        public bool isLockPlayer = false;// 是否锁定玩家
#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            
            if (rootBorn && eggBorn)
            {
                eggBorn = false; 
                rootBorn = false;
                Debug.LogError("不能同时从虫卵和虫巢孵化");
            }
        }
#endif

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
        }

        protected internal override void OnInit(object userData)
        {
            
            base.OnInit(userData);
            componentSystem.GetComponent<MoveComponent>().canGravity = true;
            // 注册动画事件
            GetComponent<AnimationEventComponent>().RegisterEvent("Attack", OnAttack);

            componentSystem.GetComponent<WoodWormCsAnimator>().ChangeState<WoodWormIdleState>();

            componentSystem.GetComponent<HealthComponent>().onHpIsZero += OnDead;

            componentSystem.GetComponent<BattleComponent>().OnApplyFinalDamage += OnApplyFinalDamage;
            componentSystem.GetComponent<BattleComponent>().OnPlayImpactEffects += OnPlayImpactEffects;
            componentSystem.GetComponent<BattleComponent>().OnApplyStatusEffects += OnApplyStatusEffects;
            componentSystem.GetComponent<MoveComponent>().GroundedEvent += OnGroundedEvent;
            componentSystem.GetComponent<MoveComponent>().UnGroundedEvent += OnUnIsGround;
            knockBackComponent = componentSystem.GetComponent<KnockBackComponent>();
            var rb = gameObject.GetComponent<Rigidbody2D>();
            knockBackComponent.Init(rb);
            knockBackComponent.SetSceneMgr(sceneManager);

            initBaseData(3);// 初始化基础数据并设置怪物ID
            baseMoveSpeed = 1.5f;
            initComponentData(userData);
            deadIsToObjectPool = false;
        }

        // 代码创建的怪物需要走下面的初始化逻辑
        public void initComponentData(object userData)
        {
            if (GetComponent<SceneEntity>() != null) { return; } // 场景对象不需要走下面的初始化逻辑
            if (userData is BaseGameSceneManager data)
            {
                sceneManager = data;
            }
            updateMonsterGroundType(groundType);// 刷新碰撞类型
            showdowArea = UIUtils.findChild(gameObject, "ShadowAnimator"); // 设置影子
            curAtkCollsionType = AtkCollsionType.Enemy; // 攻击类型
            componentSystem = GetComponent<ComponentSystemMono>();
            // 设置碰撞体相关
            var bodyCldObj = UIUtils.findChild(gameObject, "Body1");
            if (bodyCldObj != null) { 
                bodyCld = bodyCldObj.GetComponent<Collider2D>();
                bodyCldObj.GetComponent<ColliderResponder>().entityLogic = this;
                // 修改身体组件图层让其始终作为受伤碰撞检测区域
                bodyCld.gameObject.layer = atkCheckLayer;
            }
            var groundCldObj = UIUtils.findChild(gameObject, "GroundCld");
            if (groundCldObj != null)
            {
                groundCld = groundCldObj.GetComponent<Collider2D>();
                groundCldObj.GetComponent<ColliderResponder>().entityLogic = this;
                groundCld.gameObject.layer = onlyMapObjLayer;
            }
            var footObj = UIUtils.findChild(gameObject, "Foot");
            if (footObj != null) {
                footCld = footObj.GetComponent<Collider2D>();
                footObj.GetComponent<ColliderResponder>().entityLogic = this;
                //footCld.isTrigger = false;
            }
            // 设置怪物状态相关
            angryTag = UIUtils.findChild(gameObject, "Effect_MonsterState_Angry");
            weakTag = UIUtils.findChild(gameObject, "Effect_MonsterState_Weak");
            escapeTag = UIUtils.findChild(gameObject, "Effect_MonsterState_Tired");
            buffArea = UIUtils.findChild(gameObject, "BuffEffectPos");
            if (angryTag != null) { angryTag.SetActive(false); }
            if (weakTag != null) { weakTag.SetActive(false); }
            if (escapeTag != null) { escapeTag.SetActive(false); }

            animator = GetComponent<Animator>();

            dropItem = UIUtils.findChild(gameObject, "dropItem");
            animationNode = UIUtils.findChild(gameObject, "Animation");
            groundType = GroundType.Center; // 默认放置在中心位置
            hasDropItem = true; // 默认会掉落道具
            isDead = false;
            isProtect = false;
            // 设置透明度
            var oldColor = animationNode.GetComponent<SpriteRenderer>().color;
            animationNode.GetComponent<SpriteRenderer>().color = new Color(oldColor.r, oldColor.g, oldColor.b, 1f);
            dropItem.GetComponent<SpriteRenderer>().color = new Color(oldColor.r, oldColor.g, oldColor.b, 1f);
            // 设置碰撞体冻结类型
            var bodyRg = GetComponent<Rigidbody2D>();
            if (bodyRg != null)
            {
                bodyRg.constraints = RigidbodyConstraints2D.FreezeRotation;
                bodyRg.isKinematic = false;
            }
            componentSystem.GetComponent<HealthComponent>().hp = maxHp;// 重新设置HP
            onlyMapObjLayer = 7;

            var sfxObj = UIUtils.findChild(gameObject, "commonSfx");
            if (sfxObj != null) { commonSfxCpn = sfxObj.GetComponent<SoundToggleComponent>(); }
            // 攻击碰撞体和受伤特效相关
            var skillInfo = UIUtils.findChild(gameObject, "SkillInfos");
            if (skillInfo != null)
            {
                var atkNode_1 = UIUtils.findChild(skillInfo, "CollArea_NorAtk");
                if (atkNode_1 != null) { atkCollAreaNodeDict["NorAtk"] = atkNode_1; }
                var beHurtNode = UIUtils.findChild(skillInfo, "Effect_NormalAttack");
                if (beHurtNode != null) { beHurtEffectNode = beHurtNode; }
            }

            //componentSystem.GetComponent<WoodWormCsAnimator>().SceneEntity = this;
        }

        private void OnAttack(string obj)
        {
            componentSystem.GetComponent<BattleComponent>().PerformAttack("Attack");
        }

        public override void OnDead()
        {
            base.OnDead();
            isDead = true;
            isProtect = true;
            // 死亡后取消重力影响并设置为不阻挡类型
            //componentSystem.GetComponent<MoveComponent>().canGravity = false;
            bodyCld.isTrigger = true;
            footCld.isTrigger = true;
            componentSystem.GetComponent<WoodWormCsAnimator>().ChangeState<WoodWormDeadState>();
            AchievementDataMgr.getInstance().RecordAchievementProgress(AchievementType.WoodWormKill_1, 1);
            AchievementDataMgr.getInstance().RecordAchievementProgress(AchievementType.WoodWormKill_2, 1);
        }
        
        #region BattleComponent

        private void OnApplyStatusEffects(DamageData data)
        {
            if (isDead == false)
            {
                // 播放动画
                componentSystem.GetComponent<WoodWormCsAnimator>().ChangeState<WoodWormDamageState>();
                componentSystem.GetComponent<HealthComponent>().TakeDamage(data.baseDamage);

                if (data.breakHight > 0 && data.attackType == AttackType.NormalType)
                {
                    var dirPos = data.dirPos * -1; // 击退方向和伤害来源方向是相反的
                    knockBackComponent.SetKnockBaseData(data.breakHight, data.breakTime);
                    knockBackComponent.ApplyKnockBack(dirPos, data.breakWidth);
                }
            }
        }

        // 从虫蛋诞生
        public void OnBounceFromWormEgg(WormEggLogic wormEggLogic)
        {
            eggBorn = true;
            bornEggLogic = wormEggLogic;
            componentSystem.GetComponent<WoodWormCsAnimator>().ChangeState<WoodWormBounceState>();
        }
        // 从虫巢诞生
        public void OnBounceFromWormRoot(WoodWormRootLogic rootLogic)
        {
            gameObject.SetActive(true);
            rootBorn = true;
            bornRootLogic = rootLogic;
            hasDropItem = false; // 从虫巢诞生的怪物不会掉落道具
            deadIsToObjectPool = true;
            canRandomMove = true;
            componentSystem.GetComponent<WoodWormCsAnimator>().ChangeState<WoodWormBornState>();
           
        }

        // 从BOSS体内诞生
        public void OnBounceFromBossMogut(BossMogutLogic mogutLogic)
        {
            gameObject.SetActive(true);
            bossMogutBorn = true;
            bornMogutLogic = mogutLogic;
            hasDropItem = false; // BOSS产生的不会掉道具
            deadIsToObjectPool = true;
            canRandomMove = true;
            var rotation = mogutLogic.transform.rotation;
            transform.rotation = rotation;
            componentSystem.GetComponent<WoodWormCsAnimator>().ChangeState<WoodWormBounceState>();
        }

        private void OnPlayImpactEffects(DamageData data)
        {
        }

        private void OnApplyFinalDamage(DamageData data)
        {
        }

        #endregion

        public override void OnMonsterStateChange()
        {
            // 从虫巢诞生的才会出现逃跑状态
            if (rootBorn)
            {
                base.OnMonsterStateChange();
            }
        }

        // 直接播放逃跑动画
        public void ChangeToEscapeState()
        {
            componentSystem.GetComponent<WoodWormCsAnimator>().ChangeState<WoodWormEscapeState>();
        }

        // 当角色触碰地面时
        public override void OnGroundedEvent()
        {
            base.OnGroundedEvent();
            if (knockBackComponent != null)
            {
                knockBackComponent.StopKnockBackEffect();
            }
            //footCld.gameObject.layer = curMonsterLayer;
        }

        public override void OnUnIsGround()
        {
            //footCld.gameObject.layer = onlyMapObjLayer; // 空中设置只和地图碰撞
        }

        public override void MonsterRealRemove()
        {
            base.MonsterRealRemove();
            if (bornEggLogic != null)
            {
                bornEggLogic.MonsterRealRemove();
            }
            if (bornRootLogic != null)
            {
                var mgr = WoodWormRootBattleMgr.getInstance();
                if (mgr.hasInWormBattleStory)
                {
                    // 在虫巢战斗中死亡的虫子需要特殊处理
                    if (mgr.wormBattleStory.allWoodWormLogics.Contains(this))
                    {
                        mgr.wormBattleStory.allWoodWormLogics.Remove(this); // 移除死亡后的蠕虫
                    }
                }
                bornRootLogic.curBornWormCount--; // 设置虫巢产生的虫子个数减少
                if (bornRootLogic.curBornWormCount <= 0)
                {
                    bornRootLogic.MonsterRealRemove(); // 虫巢所有的虫子死亡后虫巢才设置真正移除处理
                }
            }
            if (bornMogutLogic != null)
            {
                var mgr = WestRappRoadBossBattleMgr.getInstance();
                if (mgr.hasInCurStory)
                {
                    // 在虫巢战斗中死亡的虫子需要特殊处理
                    if (mgr.GetBossBattleStory().allWoodWormLogics.Contains(this))
                    {
                        mgr.GetBossBattleStory().allWoodWormLogics.Remove(this); // 移除死亡后的蠕虫
                    }
                }
                bornMogutLogic.curBornWormCount--;
            }
        }

        // 朝人物方向移动
        public void moveToPlayer()
        {
            isLockPlayer = true; // 设置为锁定玩家

            componentSystem.GetComponent<WoodWormCsAnimator>().ChangeState<WoodWormMoveState>();
        }

        public override void PlayDeadSfx(bool isPlay = true)
        {
            base.PlayDeadSfx(isPlay);
            var realResName = "虫子死亡.mp3";
            commonSfxCpn.ChangeSoundRes(realResName);
            PlayAudio(commonSfxCpn, isPlay);
        }
    }
}