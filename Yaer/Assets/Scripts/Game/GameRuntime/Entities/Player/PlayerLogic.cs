using DG.Tweening;
using Game.GameMgr;
using Game.GameMgr.Component;
using Game.GameMgr.Component.Archive.ArchiveDataClass.Player;
using Game.GameMgr.Component.UI;
using Game.GameMgr.Manager.Settings;
using Game.GameMgr.Manager.Settings.Helper;
using Game.GameRuntime.Entities.Component;
using Game.GameRuntime.Entities.Component.Anima;
using Game.GameRuntime.Entities.Component.Battle;
using Game.GameRuntime.Entities.Component.Battle.Damage;
using Game.GameRuntime.Entities.Component.Health;
using Game.GameRuntime.Entities.Component.Interactive;
using Game.GameRuntime.Entities.Component.Physics;
using Game.GameRuntime.Entities.Component.PhysicsDetect;
using Game.GameRuntime.Entities.Player.Components;
using Game.GameRuntime.Entities.Player.Components.CsAnimator;
using Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat;
using Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat.State;
using Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat.State.Damage;
using Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat.State.Damage.DamageFly;
using Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat.State.Dead;
using Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat.State.Dead.FlyDead;
using Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat.State.Ground;
using Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat.State.Squat;
using Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat.State.Squat.Climb;
using Game.GameRuntime.Entities.Player.Components.CsAnimator.Home;
using Game.GameRuntime.GameSceneManager.Base;
using Game.GameRuntime.GameSceneManager.Component;
using Game.GameRuntime.GameSceneManager.Component.CameraGSM;
using Game.GameRuntime.GameSceneManager.Component.Story;
using Game.GameRuntime.UI.FormLogic;
using Game.Static.Enum;
using Game.Static.Name.Res;
using Game.Static.Path;
using GameFramework.UnityRuntimeExtend.Component;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.GameRuntime.Entities.Player
{
    public class PlayerLogic : BaseEntityLogic
    {
        public ComponentSystemMono componentSystem;
        public IGameSceneManager sceneManager;

        private bool isInit;
        private bool isCombatState;
        
        public bool AllowControl { get; private set; }

        public Animator animator { get; private set; }
        public HealthComponent healthComponent { get; private set; }
        public StaminaComponent staminaComponent { get; private set; }

        private PlayerCommonData playerCommonData;

        private SettingManager settingManager;

        private KnockBackComponent knockBackComponent;

        public SoundToggleComponent commonSfxCpn; // 人物身上音效管理组件,不会同时触发的音效可以用这个来播放
        public SoundToggleComponent clothingSfxCpn; // 人物衣服音效管理组件

        public bool ClothesBroken => playerCommonData.ClothesBroken;

        public bool hasInStoryEventState = false; // 是否处于故事对话事件状态中

        /// <summary>
        /// 由编辑器「人物状态调试工具」维护：为 true 时免疫来自战斗与攻击碰撞的伤害（不拦截滑条等对血量的直接改写）。
        /// 与 <see cref="isProtect"/>（剧情无敌等）独立，避免与故事流程互相覆盖。
        /// </summary>
        public bool EditorInvincible { get; set; }

        public GameObject keyTipsNode = null; // 按键提示节点
        public GameObject actionKeyTipsNode = null; // 玩家动作按键提示节点，用于指引玩家执行某个动作
        public InteractiveComponent canTouchObj; // 当前玩家可交互的最近的组件
        bool canLoadKeyTipsNode = true; // 是否可以加载按键提示节点
        public bool canInStateSetPos = true; // 是否能够在不同状态中直接设置坐标，被挤出时为false
        public Collider2D bodyCollider; // 身体碰撞体

        public float flyMoveSpeedX; // 飞行过程中的速度
        //======================一些动作指令的禁用和开启变量
        public bool isEnableSquatUp { get; set; } = true; // 是否允许从蹲下状态到站立
        public bool isEnableJump { get; set; } = true; // 是否允许跳跃
        public bool isEnableNorAtk { get; set; } = true; // 是否允许普通攻击
        public bool isEnableQuickUseItem { get; set; } = true; // 是否允许快捷使用道具
        public bool isEnableMovePassMonster { get; set; } = false; // 是否允许移动时穿过怪物
        #region event
        public Action DashAttackDust1;
        public Action DashAttackDust2;
        /// <summary>
        /// 摔地事件
        /// </summary>
        public Action FallGroundEvent;
        public Action OnFlyHitClsEvent;
        public Action OnEnterSitIdleEvent;
        public Action OnExitSitIdleEvent;

        public Action<float> OnTakeDamage;
        public Action<bool> OnClothesBrokenChanged;
        #endregion

        private int audioIndex = 1;// 音效播放下标

        #region Unity回调

        protected internal override void OnInit(object userData)
        {
            base.OnInit(userData);
            gameObject.name = "Player";

            if (userData is IGameSceneManager data)
            {
                sceneManager = data;
            }

            playerCommonData = sceneManager.GetArchiveData<PlayerCommonData>();

            componentSystem = GetComponent<ComponentSystemMono>();

            componentSystem.onInitBeforeAction = SetPlayerLogicForComponents; // 初始化前执行
            componentSystem.OnInit();
            componentSystem.GetComponent<BattleComponent>().OnApplyFinalDamage += OnApplyFinalDamage;
            componentSystem.GetComponent<BattleComponent>().OnPlayImpactEffects += OnPlayImpactEffects;
            componentSystem.GetComponent<BattleComponent>().OnApplyStatusEffects += OnApplyStatusEffects;
            

            animator = componentSystem.GetComponent<BaseCsAnimator>().GetAnimator();

            healthComponent = componentSystem.GetComponent<HealthComponent>();
            staminaComponent = componentSystem.GetComponent<StaminaComponent>();

            healthComponent.onHpChange += OnHPChanged;
            componentSystem.GetComponent<HealthComponent>().onHpIsZero += OnDead;
            staminaComponent.OnStaminaChanged += OnStaminaChanged;

            sceneManager.GetModule<StoryComponentGSM>().onStoryTriggered += StoryTriggeredHandle;
            sceneManager.GetModule<StoryComponentGSM>().onStoryEnd += StoryEndHandle;
            sceneManager.GetModule<LoadSceneComponentGSM>().onStartLoadingSceneEvent += LoadingSceneHandle;
            sceneManager.GetModule<LoadSceneComponentGSM>().onEndLoadingSceneEvent += LoadingSceneEndHandle;

            knockBackComponent = componentSystem.GetComponent<KnockBackComponent>();
            var rb = gameObject.GetComponent<Rigidbody2D>();
            knockBackComponent.Init(rb);
            knockBackComponent.SetSceneMgr(sceneManager as BaseGameSceneManager);
            // 存档加载场景
            GameManager.GetGMComponent<ProcedureComponentGM>().onCompleteLoadingSceneEvent += LoadingSceneEndHandle;

            // 游戏暂停
            GameManager.GetGMComponent<ProcedureComponentGM>().onPauseGameAction += PauseGameHandle;

            // 游戏恢复
            GameManager.GetGMComponent<ProcedureComponentGM>().onResumeGameAction += ResumeGameHandle;

            // 默认禁止移动
            componentSystem.GetComponent<PlayerInputComponent>().SetAllowMove(false);

            // 更新衣服
            componentSystem.GetComponent<PlayerRuntimeControllerComponent>()
                .GetAnimatorController(UpdateRuntimeController, sceneManager.Config.isFightingScene);

            settingManager = GameManager.GetManager<SettingManager>();
            settingManager.OnShowWoundChange += SetAnimatorShowWound;

            componentSystem.GetComponent<PlayerMoveComponent>().onTurnAction += OnTurnAction;

            // 村庄 KenMuNi1：纵深移动与输入门控（与 DisablePlayerMove 解耦，避免停左右走）
            RefreshVillageExplorationFromActiveScene();

            // 设置身体碰撞组件
            var bodyObj = UIUtils.findChild(gameObject, "Body");
            if (bodyObj != null)
            {
                bodyCollider = bodyObj.GetComponent<Collider2D>();
            }
            var eventCollider = UIUtils.findChild(gameObject, "Event");
            if (eventCollider != null)
            {
                eventCollider.GetComponent<ColliderResponder>().entityLogic = this;
            }
            // 设置影子
            if (showdowArea == null)
            {
                showdowArea = UIUtils.findChild(gameObject, "ShadowAnimator");
            }
            var sfxObj = UIUtils.findChild(gameObject, "commonSfx");
            if (sfxObj != null){ commonSfxCpn = sfxObj.GetComponent<SoundToggleComponent>(); }
            var sfxObj2 = UIUtils.findChild(gameObject, "clothingSfx");
            if (sfxObj2 != null) { clothingSfxCpn = sfxObj2.GetComponent<SoundToggleComponent>(); }

            AllowControl = true;
            isInit = true;
            curAtkCollsionType = AtkCollsionType.Player;
            FallGroundEvent += () => PlayFallDownInGround();
            // 初始记录攻击碰撞体和受伤特效
            var skillInfo = UIUtils.findChild(gameObject, "SkillInfos");
            if (skillInfo != null)
            {
                var atkNode_1 = UIUtils.findChild(skillInfo, "CollArea_NorAtk_1");
                if (atkNode_1 != null) { atkCollAreaNodeDict["NorAtk_1"] = atkNode_1; }
                var atkNode_2 = UIUtils.findChild(skillInfo, "CollArea_NorAtk_2");
                if (atkNode_2 != null) { atkCollAreaNodeDict["NorAtk_2"] = atkNode_2; }
                var atkNode_3 = UIUtils.findChild(skillInfo, "CollArea_NorAtk_3");
                if (atkNode_3 != null) { atkCollAreaNodeDict["NorAtk_3"] = atkNode_3; }
                var atkNode_4 = UIUtils.findChild(skillInfo, "CollArea_DashAtk");
                if (atkNode_4 != null) { atkCollAreaNodeDict["DashAtk"] = atkNode_4; }
                var atkNode_5 = UIUtils.findChild(skillInfo, "CollArea_SmashAtk_1");
                if (atkNode_5 != null) { atkCollAreaNodeDict["SmashAtk_1"] = atkNode_5; }
                var atkNode_6 = UIUtils.findChild(skillInfo, "CollArea_SmashAtk_2");
                if (atkNode_6 != null) { atkCollAreaNodeDict["SmashAtk_2"] = atkNode_6; }
                var atkNode_7 = UIUtils.findChild(skillInfo, "CollArea_SquatAtk");
                if (atkNode_7 != null) { atkCollAreaNodeDict["SquatAtk"] = atkNode_7; }
                var beHurtNode = UIUtils.findChild(skillInfo, "Effect_BeHurt");
                if (beHurtNode != null) { beHurtEffectNode = beHurtNode; }
            }

            actionKeyTipsNode = UIUtils.findChild(gameObject, "ActionKeyTipsNode");
        }


        protected internal override void OnShow(object userData)
        {
            base.OnShow(userData);
            healthComponent.SetData(playerCommonData.CurrentHP, playerCommonData.MaxHP);
            staminaComponent.SetData(playerCommonData.CurrentStamina, playerCommonData.MaxStamina);
        }

        protected internal override void OnHide(bool isShutdown, object userData)
        {
            base.OnHide(isShutdown, userData);
            healthComponent.onHpChange -= OnHPChanged;
            componentSystem.GetComponent<HealthComponent>().onHpIsZero -= OnDead;
            staminaComponent.OnStaminaChanged -= OnStaminaChanged;

            sceneManager.GetModule<StoryComponentGSM>().onStoryTriggered -= StoryTriggeredHandle;
            sceneManager.GetModule<StoryComponentGSM>().onStoryEnd -= StoryEndHandle;
            sceneManager.GetModule<LoadSceneComponentGSM>().onStartLoadingSceneEvent -= LoadingSceneHandle;
            sceneManager.GetModule<LoadSceneComponentGSM>().onEndLoadingSceneEvent -= LoadingSceneEndHandle;
            GameManager.GetGMComponent<ProcedureComponentGM>().onCompleteLoadingSceneEvent -= LoadingSceneEndHandle;
            GameManager.GetGMComponent<ProcedureComponentGM>().onPauseGameAction -= PauseGameHandle;
            GameManager.GetGMComponent<ProcedureComponentGM>().onResumeGameAction -= ResumeGameHandle;

            componentSystem.GetComponent<BattleComponent>().OnApplyFinalDamage -= OnApplyFinalDamage;
            componentSystem.GetComponent<BattleComponent>().OnPlayImpactEffects -= OnPlayImpactEffects;
            componentSystem.GetComponent<BattleComponent>().OnApplyStatusEffects -= OnApplyStatusEffects;

            GetComponent<AnimationEventComponent>().ClearEvent();
            settingManager.OnShowWoundChange -= SetAnimatorShowWound;
        }

        protected internal override void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(elapseSeconds, realElapseSeconds);

            if (!isInit) return;

            checkCanAddKeyTipsInOtherEntity();
            componentSystem.OnUpdate();
        }

        // 检测是否要添加按键提示到其他实体上
        void checkCanAddKeyTipsInOtherEntity()
        {
            var sceneMgr = sceneManager as BaseGameSceneManager;
            var curCloseObj = sceneMgr.GetFirstCanTouchEntiy(null);
            if (curCloseObj == canTouchObj)
            {
                return;// 一直触发相同的组件就不用处理后续逻辑了
            }
            canTouchObj = curCloseObj;
            if (canTouchObj != null && canTouchObj.entityControll != null && canTouchObj.entityControll.canTouchWithPlayer)
            {
                AddKeyTipsNodeToObj();
            }
            else
            {
                if (keyTipsNode != null)
                {
                    //if (FirstMeetSlimeGuideStoryMgr.getInstance().inShowNorAtkTips) { return; }
                    //if (FirstMeetSlimeGuideStoryMgr.getInstance().inShowSmallAtkTips) { return; }
                    //if (PlayerGuideMgr.getInstance().hasAnyKeyTips()) { return; }
                    showKeyTipsNode(false);
                }
            }
        }

        public void AddKeyTipsNodeToObj(ControlInputType inputType=ControlInputType.Interact)
        {
            if (keyTipsNode == null && canLoadKeyTipsNode)
            {
                var resMgr = GameManager.GetGMComponent<ResComponentGM>();
                var prefabPath = "Assets/GameRes/Prefabs/UINode/KeyTipsNode.prefab";
                canLoadKeyTipsNode = false;// 防止重复加载预制体
                resMgr.LoadAsset<GameObject>(prefabPath, (obj) =>
                {
                    keyTipsNode = Instantiate(obj);
                    canLoadKeyTipsNode = true;
                    if (canTouchObj != null && canTouchObj.entityControll != null && canTouchObj.entityControll.canTouchWithPlayer)
                    {
                        canTouchObj.entityControll.AddKeyTipsNode(keyTipsNode, inputType);
                        keyTipsNode.GetComponent<CanvasGroup>().alpha = 0;
                        keyTipsNode.GetComponent<CanvasGroup>().DOKill();
                        GameActionMgr.runFadeAction(keyTipsNode, 1, 0.5f);
                    }
                    else
                    {
                        keyTipsNode.SetActive(false);
                    }
                });
            }
            else if (keyTipsNode != null)
            {
                showKeyTipsNode(true, canTouchObj, inputType);
            }
        }

        public void showKeyTipsNode(bool isShow, InteractiveComponent interative=null, ControlInputType inputType = ControlInputType.Interact)
        {
            if (isShow)
            {
                keyTipsNode.SetActive(true);
                if (interative != null)
                {
                    interative.entityControll.AddKeyTipsNode(keyTipsNode, inputType);
                    keyTipsNode.GetComponent<CanvasGroup>().alpha = 0;
                    keyTipsNode.GetComponent<CanvasGroup>().DOKill();
                    GameActionMgr.runFadeAction(keyTipsNode, 1, 0.5f);
                }
                else
                {
                    keyTipsNode.SetActive(false);
                }
            }
            else
            {
                keyTipsNode.GetComponent<CanvasGroup>().alpha = 1;
                keyTipsNode.GetComponent<CanvasGroup>().DOKill();
                var action = GameActionMgr.runFadeAction(keyTipsNode, 0, 0.5f);
                action.onComplete = () =>
                {
                    keyTipsNode.SetActive(false);
                };
            }
        }

        public void ShowActionKeyTipsNode(bool isShow = true, ControlInputType inputType = ControlInputType.Interact, bool isTrigger=false)
        {
            if (actionKeyTipsNode == null) { return; }
            if (isShow)
            {
                actionKeyTipsNode.SetActive(true);
                actionKeyTipsNode.GetComponent<KeyTipsNodeSrc>().ShowStoryTriggerEffect(isTrigger, inputType);
                actionKeyTipsNode.GetComponent<CanvasGroup>().alpha = 0;
                actionKeyTipsNode.GetComponent<CanvasGroup>().DOKill();
                GameActionMgr.runFadeAction(actionKeyTipsNode, 1, 0.5f);
            }
            else
            {
                actionKeyTipsNode.GetComponent<CanvasGroup>().alpha = 1;
                actionKeyTipsNode.GetComponent<CanvasGroup>().DOKill();
                var action = GameActionMgr.runFadeAction(actionKeyTipsNode, 0, 0.5f);
                action.onComplete = () =>
                {
                    actionKeyTipsNode.SetActive(false);
                };
            }
            
        }
        
        private void FixedUpdate()
        {
            if (!isInit) return;

            componentSystem.OnFixedUpdate();
        }

        #endregion

        #region BattleComponent

        private void OnApplyStatusEffects(DamageData data)
        {
            var csAnimator = componentSystem.GetComponent<PlayerCsAnimator>();
            if (componentSystem.GetComponent<HealthComponent>().IsDead == false &&
                !isProtect &&
                !EditorInvincible)
            {
                TakeDamage(data.baseDamage);
                if (!isDead && isNoBreakState) { return; } // 霸体状态不会被打断动作
                // 播放动画
                var moveComponent = componentSystem.GetComponent<PlayerMoveComponent>();
                moveComponent.StopMove();
                if (data.dirPos.x > 0) { moveComponent.TurnRight(); }
                else { moveComponent.TurnLeft(); }
                
                // 非特殊状态下只要受伤就强制转换为受伤状态
                if (!csAnimator.GetSign("IsBreakUp") || isDead)
                {
                    if (csAnimator.GetSign("IsClimb"))
                    {
                        var controller = csAnimator.CurrentCsRuntimeController as BaseCsRuntimeController;
                        var subMachine = controller.mainStateMachine.Sub;
                        var stateMachine = subMachine.Sub.ExitCurrentStateMachine();
                        if (data.attackType == Component.Battle.Attack.AttackType.BreakType)
                        {
                            // 设置被击飞
                            moveComponent.SetDamageFlyDistance(data.breakWidth);
                            moveComponent.SetDamageFlyHeight(data.breakHight);
                            stateMachine.Exit();
                            var mainStateMachine = controller.ExitCurrentSubStateMachine();
                            if (isDead) { mainStateMachine.ChangeState<FlyDeadSM, FlyDeadUpState>(); }
                            else{ mainStateMachine.ChangeState<DamageFlySM, DamageFlyUpState>();}
                        }
                        else
                        {
                            if (isDead) { 
                                stateMachine.ChangeState<SquatSM, SquatDeadState>();
                            }
                            else { stateMachine.ChangeState<SquatDamageState>(); }
                            
                        }
                    }
                    else if (csAnimator.GetSign(PlayerStateSign.Squat))
                    {
                        csAnimator.CurrentCsRuntimeController.Exit();
                        if (data.attackType == Component.Battle.Attack.AttackType.BreakType)
                        {
                            // 设置被击飞
                            moveComponent.SetDamageFlyDistance(data.breakWidth);
                            moveComponent.SetDamageFlyHeight(data.breakHight);
                            var stateMachine = csAnimator.CurrentCsRuntimeController.ExitCurrentSubStateMachine();
                            if (isDead) { stateMachine.ChangeState<FlyDeadSM, FlyDeadUpState>(); }
                            else { stateMachine.ChangeState<DamageFlySM, DamageFlyUpState>(); }
                        }
                        else
                        {
                            if (isDead){ csAnimator.ChangeState<SquatSM, SquatDeadState>(); }
                            else { csAnimator.ChangeState<SquatSM, SquatDamageState>(); }
                        }
                    }
                    else if (csAnimator.GetSign("IsJumping") || csAnimator.GetSign("IsSitting"))
                    {
                        moveComponent.SetDamageFlyDistance(data.breakWidth);
                        moveComponent.SetDamageFlyHeight(data.breakHight);
                        csAnimator.CurrentCsRuntimeController.Exit();
                        var mainStateMachine = csAnimator.CurrentCsRuntimeController.ExitCurrentSubStateMachine();
                        if (isDead) { mainStateMachine.ChangeState<FlyDeadSM, FlyDeadUpState>(); }
                        else { mainStateMachine.ChangeState<DamageFlySM, DamageFlyUpState>(); }
                        //csAnimator.ChangeState<DamageFlySM, DamageFlyUpState>();
                    }
                    else
                    {
                        var stateMachine = csAnimator.CurrentCsRuntimeController.ExitCurrentSubStateMachine();
                        if (data.attackType == Component.Battle.Attack.AttackType.BreakType)
                        {
                            // 设置被击飞
                            moveComponent.SetDamageFlyDistance(data.breakWidth);
                            moveComponent.SetDamageFlyHeight(data.breakHight);
                            if (isDead) { stateMachine.ChangeState<FlyDeadSM, FlyDeadUpState>(); }
                            else { stateMachine.ChangeState<DamageFlySM, DamageFlyUpState>(); }
                            return;
                        }
                        var randomNum = GameTools.getRandomIntNum(0, 1);
                        if (isDead)
                        {
                            if (randomNum == 0) { csAnimator.ChangeState<Dead1State>(); }
                            else { csAnimator.ChangeState<Dead2State>(); }
                        }
                        else
                        {
                            if (randomNum == 0) { stateMachine.ChangeState<Damage1State>(); }
                            else { stateMachine.ChangeState<Damage2State>(); }
                        }                     
                        if (data.breakHight > 0)
                        {
                            var dirPos = data.dirPos * -1; // 击退方向和伤害来源方向是相反的
                            knockBackComponent.SetKnockBaseData(data.breakHight, data.breakTime);
                            knockBackComponent.ApplyKnockBack(dirPos, data.breakWidth);
                        }
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

        #endregion

        private void SetPlayerLogicForComponents()
        {
            var components = componentSystem.GetAddComponents();
            foreach (var component in components)
            {
                if (component is IPlayerComponent playerComponent)
                {
                    playerComponent.PlayerLogic = this;
                }
            }
        }
        
        public void PauseGameHandle()
        {
            if (componentSystem == null || componentSystem.GetComponent<PlayerInputComponent>() == null) { return; }
            componentSystem.GetComponent<PlayerInputComponent>().SetAllowMove(false);
        }

        public void ResumeGameHandle()
        {
            if (componentSystem == null || componentSystem.GetComponent<PlayerInputComponent>() == null) { return; }
            componentSystem.GetComponent<PlayerInputComponent>().SetAllowMove(true);
        }

        private void LoadingSceneHandle()
        {
            //componentSystem.GetComponent<PlayerInputComponent>().SetAllowMove(false);
            PauseGameHandle();
        }

        private void LoadingSceneEndHandle()
        {
            ResumeGameHandle();
            //sceneManager.SetSceneObjIsPause(false);// 设置游戏对象可以活动
            GameManager.GetGameSceneManager().GetModule<InputComponentGSM>().SetAllowOpenMenu(true);
            // 换场后根据目标场景重绑村庄 2.5D 模式（AC-01 / AC-08）
            RefreshVillageExplorationFromActiveScene();
            // 0901：若进村后从未权威 Teleport，以当前脚提交，避免 defer 永久跳过夹区 / Loading 打回楼梯
            var town = componentSystem != null
                ? componentSystem.TryGetComponent<TownPlayerLocomotion>()
                : null;
            town?.EnsureAuthoritativeVillageSpawnCommitted();
        }

        private void StoryTriggeredHandle()
        {
            hasInStoryEventState = true;
            //componentSystem.GetComponent<PlayerInputComponent>().SetAllowMove(false);
            var csAnimator = componentSystem.GetComponent<PlayerCsAnimator>();
            
            PauseGameHandle();
            // 故事触发时暂停所有场景对象
            sceneManager.SetSceneObjIsPause(true);
            isProtect = true; // 对话期间玩家无敌
           
            if (csAnimator.GetSign("IsDashAtk")) {
                ChangeStateToIdle();
            }
            componentSystem.GetComponent<PlayerMoveComponent>().StopMove();
            canInStateSetPos = false;// 取消位移事件
        }

        private void StoryEndHandle()
        {
            isProtect = false;
            hasInStoryEventState = false;
            //componentSystem.GetComponent<PlayerInputComponent>().SetAllowMove(true);
            ResumeGameHandle();
            sceneManager.SetSceneObjIsPause(false);// 设置游戏对象可以活动
            if (keyTipsNode && keyTipsNode.activeSelf)
            {
                //if (FirstMeetSlimeGuideStoryMgr.getInstance().inShowNorAtkTips) { return; }
                //if (FirstMeetSlimeGuideStoryMgr.getInstance().inShowSmallAtkTips) { return; }
                //if (PlayerGuideMgr.getInstance().hasAnyKeyTips()) { return; }
                keyTipsNode.GetComponent<KeyTipsNodeSrc>().ShowStoryTriggerEffect();
            }
        }

        /// <summary>
        /// 更新世界 X/Y。村模式（Village2_5D）下走权威 Teleport（Transform+Rb+纵深 Y），
        /// 避免只改 Transform 被 WalkArea/WriteRoot 打回（0901 进场吸楼梯 H2）。
        /// 非村模式仍只写 Transform，保留 Z。
        /// </summary>
        public void SetPos(Vector2 pos)
        {
            var town = componentSystem != null
                ? componentSystem.TryGetComponent<TownPlayerLocomotion>()
                : null;
            if (town != null && town.enabled)
            {
                var input = componentSystem.GetComponent<PlayerInputComponent>();
                if (input != null && input.LocomotionMode == PlayerLocomotionMode.Village2_5D)
                {
                    town.TeleportAuthoritativeVillagePos(pos, thenFlush: true);
                    return;
                }
            }

            Vector3 p = transform.position;
            transform.position = new Vector3(pos.x, pos.y, p.z);
        }

        /// <summary>
        /// 在村庄探索白名单场景内按对象名查找纵深标尺，将世界 Y 写入 <see cref="TownPlayerLocomotion"/>（须在 <see cref="TownPlayerLocomotion.ApplyVillageMode"/> 之后调用，以便用场景边界二次 Clamp）。
        /// <para>策划在场景中放置名为 <c>VillageDepthY_Min</c>、<c>VillageDepthY_Max</c> 的空物体即可调可走带；缺失任一物体则保留 Prefab 上序列化默认值（L-02）。</para>
        /// <para>白名单：<see cref="SceneName.IsVillageExplorationScene"/>（KenMuNi1 + Chief_House，0901）。</para>
        /// </summary>
        private static void TryInjectVillageDepthYBoundsFromSceneMarkers(TownPlayerLocomotion town)
        {
            if (town == null)
            {
                return;
            }

            Scene active = SceneManager.GetActiveScene();
            if (!active.IsValid() || !SceneName.IsVillageExplorationScene(active.name))
            {
                return;
            }

            Transform minTr = FindNamedTransformInScene(active, "VillageDepthY_Min");
            Transform maxTr = FindNamedTransformInScene(active, "VillageDepthY_Max");
            if (minTr == null || maxTr == null)
            {
                return;
            }

            float minY = minTr.position.y;
            float maxY = maxTr.position.y;
            town.SetDepthYBounds(minY, maxY);
        }

        /// <summary>仅在指定 <paramref name="scene"/> 的根层级内递归匹配物体名，避免 <c>GameObject.Find</c> 跨场景误命中。</summary>
        private static Transform FindNamedTransformInScene(Scene scene, string objectName)
        {
            if (!scene.IsValid() || string.IsNullOrEmpty(objectName))
            {
                return null;
            }

            foreach (GameObject root in scene.GetRootGameObjects())
            {
                Transform found = FindNamedTransformRecursive(root.transform, objectName);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private static Transform FindNamedTransformRecursive(Transform tr, string objectName)
        {
            if (tr.name == objectName)
            {
                return tr;
            }

            for (int i = 0; i < tr.childCount; i++)
            {
                Transform child = FindNamedTransformRecursive(tr.GetChild(i), objectName);
                if (child != null)
                {
                    return child;
                }
            }

            return null;
        }

        /// <summary>
        /// 村庄探索模式：不关左右走，只关战斗向能力与纵深组件；勿用 <see cref="DisablePlayerMove"/> 代替（策划 4.4）。
        /// DNF 式移动下同步关闭跳跃（输入屏蔽 + <see cref="isEnableJump"/>），与《村庄DNF式2.5D移动_迁移方案》§5 一致。
        /// </summary>
        /// <param name="enable">true 表示当前应处于村庄探索规则（见 <see cref="SceneName.IsVillageExplorationScene"/>）</param>
        public void SetVillageExplorationMode(bool enable)
        {
            var input = componentSystem.GetComponent<PlayerInputComponent>();
            if (input != null)
            {
                input.SetLocomotionMode(enable
                    ? PlayerLocomotionMode.Village2_5D
                    : PlayerLocomotionMode.Default);
            }

            var town = componentSystem.TryGetComponent<TownPlayerLocomotion>();
            town?.ApplyVillageMode(enable);
            // 进村后再注入场景标尺 Y，使 ApplyVillageMode 首帧 Clamp 与策划可走带一致（执行说明 §5.1）
            if (enable)
            {
                TryInjectVillageDepthYBoundsFromSceneMarkers(town);
                // SetDepthYBounds 只改权威标量，须立刻写回刚体并套 WalkArea，避免首帧与标尺/多边形脱节（第三阶段执行说明 §5.1、P-06）。
                town?.FlushAuthoritativeVillageTransformAfterSceneDepthInject();
            }

            // 与战斗状态机门闸对齐：村内 DNF 禁止跳跃与普攻（输入层已挡 Jump；此处防异常切到 Combat 仍起跳）
            isEnableNorAtk = !enable;
            isEnableJump = !enable;
        }

        /// <summary>
        /// 根据当前激活场景名同步村庄模式（单一事实来源：场景名白名单）。
        /// 原因（0901）：仅 KenMuNi1 时进屋永远 Default；现扩至 <see cref="SceneName.Village_Chief_House"/>。
        /// </summary>
        public void RefreshVillageExplorationFromActiveScene()
        {
            bool village = SceneName.IsVillageExplorationScene(SceneManager.GetActiveScene().name);
            SetVillageExplorationMode(village);
        }

        /// <summary>
        ///     根据衣服数据修改状态机
        /// </summary>
        private void UpdateRuntimeController(RuntimeAnimatorController controllerAsset)
        {
            if (controllerAsset == null)
            {
                Debug.LogError("没有找到RuntimeAnimatorController资源");
                return;
            }

            // 必须先按磁盘资源名分流 Home/Combat。方案 B 会 Clone 一份 Override，
            // Clone 默认名字不含 "Home"；若先换片再 Contains("Home") 会误走 Combat，IsName 卡死。
            if (controllerAsset.name.Contains("Home"))
            {
                RuntimeAnimatorController homeController =
                    VillageHomeDayLightAnimApplier.ApplyIfVillageHome(controllerAsset);
                componentSystem.GetComponent<BaseCsAnimator>()
                    .ChangeRuntimeController<PlayerHomeCsRuntimeController>(homeController);
            }
            else
            {
                componentSystem.GetComponent<BaseCsAnimator>().ChangeRuntimeController<PlayerCombatCsRuntimeController>(controllerAsset);
            }
            SetAnimatorHP(healthComponent.hp);
            SetAnimatorMP(staminaComponent.Stamina);
            SetAnimatorClothesBroken(this.ClothesBroken);
            SetAnimatorShowWound(settingManager.LoadSetting<SettingsConfigData>().showWound);
        }

        public void SetCameraFollow(Transform target)
        {
            sceneManager.GetModule<CameraComponentGSM>().SetFollow(target);
        }

        public void TakeDamage(float damage)
        {
            // 编辑器无敌：不走受伤音效与 UI，避免与「未掉血」不一致
            if (EditorInvincible)
            {
                return;
            }

            PlayBeHurtAudio();
            componentSystem.GetComponent<HealthComponent>().TakeDamage(damage);
            OnTakeDamage?.Invoke(damage);
            var fightPanel = GetFightPanelLogic();
            if (fightPanel != null)
            {
                fightPanel.PlayerUnderAttack(damage);
            }
        }

        public void PlayBeHurtAudio(bool isPlay = true)
        {
            // 受伤时随机播放音效
            string basePath = "雅尔被击音效/被击{0}.mp3";
            var randomIndex = GameTools.getRandomIntNum(1, 4);
            var realResPath = string.Format(basePath, randomIndex);
            commonSfxCpn.ChangeSoundRes(realResPath);
            PlayAudio(commonSfxCpn, isPlay);
        }

        public void PlayRunAudio(bool isPlay=true)
        {
            if (!isPlay) { 
                PlayAudio(commonSfxCpn, isPlay);
                return;
            }
            var sceneMgr = GameManager.GetGameSceneManager() as BaseGameSceneManager;
            var terrainType = sceneMgr.GetCurSceneTerrainType();
            // 根据当前场景播放不同类型的音效
            string baseFilePath = "主角跑步走路音效/{0}";
            Dictionary<TerrainType, string> goundAudioResDict = new Dictionary<TerrainType, string>()
            {
                { TerrainType.IndoorType, "室内走{0}.mp3" },{ TerrainType.GlassType, "草地跑{0}.mp3" },{ TerrainType.LandType, "土地跑{0}.mp3" },
            };
            var baseName = goundAudioResDict[terrainType];
            //var randomIndex = GameTools.getRandomIntNum(1, 6);
            var audioNum = 9;
            
            var randomIndex = audioIndex;
            audioIndex++;
            audioIndex = audioIndex > audioNum ? 1 : audioIndex;
            var resName = string.Format(baseName, randomIndex);
            var realResPath = string.Format(baseFilePath, resName);
            commonSfxCpn.ChangeSoundRes(realResPath);
            PlayAudio(commonSfxCpn, isPlay); // 播放一次走路音效
        }

        public void PlayNorAtkAudio(bool isPlay = true)
        {
            // 挥剑时播放随机音效
            string basePath = "雅尔攻击音效/雅尔 挥剑 sfx/雅尔 挥剑 sfx {0}.wav";
            var randomIndex = GameTools.getRandomIntNum(5, 8);
            var realResPath = string.Format(basePath, randomIndex);
            commonSfxCpn.ChangeSoundRes(realResPath);
            PlayAudio(commonSfxCpn, isPlay);
        }

        public void PlayDashAtkAudio(bool isPlay = true)
        {
            var realResPath = "冲锋音.mp3";
            commonSfxCpn.ChangeSoundRes(realResPath);
            PlayAudio(commonSfxCpn, isPlay);
        }

        public void PlayFallDownInGround(bool isPlay = true)
        {
            var basePath = "跌倒{0}.mp3";
            var randomIndex = GameTools.getRandomIntNum(1, 3);
            var realResPath = string.Format(basePath, randomIndex);
            commonSfxCpn.ChangeSoundRes(realResPath);
            PlayAudio(commonSfxCpn, isPlay);
        }
        // 跳跃落地音效
        public void PlayJumpDownInGround(bool isPlay = true)
        {
            var realResPath = "跳跃落地声音.mp3";
            commonSfxCpn.ChangeSoundRes(realResPath);
            PlayAudio(commonSfxCpn, isPlay);
        }

        // 播放衣服被风吹动的声音
        public void PlayClothingAudio(bool isPlay = true)
        {
            var realResPath = "衣服布料风声.mp3";
            clothingSfxCpn.ChangeSoundRes(realResPath);
            PlayAudio(clothingSfxCpn, isPlay);
        }
        // 播放衣服碎裂声音
        public void PlayClothingBreakAudio(bool isPlay = true)
        {
            var realResPath = "装备水晶碎裂.mp3";
            clothingSfxCpn.ChangeSoundRes(realResPath);
            PlayAudio(clothingSfxCpn, isPlay);
        }

        private void OnHPChanged(float hp)
        {
            SetAnimatorHP(hp);
            playerCommonData.CurrentHP = hp;
            CheckClothesBroken(hp);
            var fightPanel = GetFightPanelLogic();
            if (fightPanel != null)
            {
                fightPanel.UpdateHp(hp);
            }
        }

        private void OnStaminaChanged(float stamimna)
        {
            SetAnimatorMP(stamimna);
            playerCommonData.CurrentStamina = stamimna;
            var fightPanel = GetFightPanelLogic();
            if (fightPanel != null)
            {
                fightPanel.UpdateMp(stamimna);
            }
        }

        private void SetAnimatorHP(float hp)
        {
            // HP影响的人物状态分为三挡
            var hpRate = hp / healthComponent.maxHp;
            var hpTag = 0;
            if (hpRate <= 0.249) { hpTag = 2; }
            else if (hpRate <= 0.499) { hpTag = 1; }
            animator.SetFloat("HPAmount", hpTag);
        }

        private void SetAnimatorMP(float mp) 
        {
            // MP影响的人物状态分为两挡
            var mpRate = mp / staminaComponent.MaxStamina;
            var mpTag = 0;
            if (mpRate <= 0.399) { mpTag = 1; }
            animator.SetFloat("MPAmount", mpTag);
        }

        private void SetAnimatorClothesBroken(bool broken)
        {
            animator.SetFloat("ClothesBroken", broken ? 1 : 0);
        }

        private void SetAnimatorShowWound(bool showWound)
        {
            animator.SetFloat("ShowWound", showWound ? 1 : 0);
        }

        public void AttackBossMogut()
        {
            componentSystem.GetComponent<PlayerCsAnimator>().ChangeState<AttackBossMogutState>();
            var moveComponent = componentSystem.GetComponent<PlayerMoveComponent>();
            moveComponent.TurnRight();
            moveComponent.SetRunSpeed();
        }

        private void CheckClothesBroken(float hp)
        {
            if (hp / healthComponent.maxHp < 0.5f && !playerCommonData.ClothesBroken)
            {
                playerCommonData.ClothesBroken = true;
                SetAnimatorClothesBroken(true);
                OnClothesBrokenChanged?.Invoke(true);
                var fightPanel = GetFightPanelLogic();
                if (fightPanel != null)
                {
                    fightPanel.OnClothesBrokenChanged(true);
                }
            }
        }

        /// <summary>
        /// 修复衣服
        /// </summary>
        public void FixClothes()
        {
            playerCommonData.ClothesBroken = false;
            SetAnimatorClothesBroken(false);
            OnClothesBrokenChanged?.Invoke(false);
            var fightPanel = GetFightPanelLogic();
            if (fightPanel != null)
            {
                fightPanel.OnClothesBrokenChanged(false);
            }
        }

        // 角色受伤
        public override void HasHurt(DamageData damageData)
        {
            base.HasHurt(damageData);
            componentSystem.GetComponent<BattleComponent>().TakeDamage(damageData);
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

        public virtual void OnTurnAction(Vector2 dir)
        {
            if (keyTipsNode != null && keyTipsNode.transform.parent == transform)
            {
                // 设置在人物身上的提示节点跟随人物的旋转度而变化
                var rotationY = gameObject.transform.rotation.y;
                var newQuatern = dir.x > 0 ? Quaternion.Euler(0, 0, 0) : Quaternion.Euler(0, -rotationY, 0);
                keyTipsNode.transform.rotation = newQuatern;
            }else if (actionKeyTipsNode != null)
            {
                var rotationY = gameObject.transform.rotation.y;
                var newQuatern = dir.x > 0 ? Quaternion.Euler(0, 0, 0) : Quaternion.Euler(0, -rotationY, 0);
                actionKeyTipsNode.transform.rotation = newQuatern;
            }
        }

        public void DisablePlayerMove(bool isDisable=true)
        {
            if (isDisable) componentSystem.GetComponent<PlayerMoveComponent>().StopMove();
            canInStateSetPos = !isDisable;
            componentSystem.GetComponent<PlayerInputComponent>().SetAllowMove(!isDisable);
            isEnableJump = !isDisable;
            isEnableNorAtk = !isDisable;
            isEnableSquatUp = !isDisable;
            isEnableQuickUseItem = !isDisable;
        }

        // 玩家死亡
        private void OnDead()
        {
            isProtect = true;
            isDead = true;
            //bodyCollider.isTrigger = true;// 死亡后设置不可被移动
            // 触发死亡时自动关闭部分界面
            var uiForm = GameManager.GetGMComponent<UIComponentGM>().GetUIForm(UIPrefabPath.GetUIPrefabPath("MenuPanel"));
            if (uiForm != null) { GameManager.GetGMComponent<UIComponentGM>().CloseUIForm(uiForm); }
            uiForm = GameManager.GetGMComponent<UIComponentGM>().GetUIForm(UIPrefabPath.GetUIPrefabPath("SaveGamePanel"));
            if (uiForm != null) { GameManager.GetGMComponent<UIComponentGM>().CloseUIForm(uiForm); }
            uiForm = GameManager.GetGMComponent<UIComponentGM>().GetUIForm(UIPrefabPath.GetUIPrefabPath("LoadGamePanel"));
            if (uiForm != null) { GameManager.GetGMComponent<UIComponentGM>().CloseUIForm(uiForm); }
            uiForm = GameManager.GetGMComponent<UIComponentGM>().GetUIForm(UIPrefabPath.GetUIPrefabPath("SystemTipsPanel"));
            if (uiForm != null) { GameManager.GetGMComponent<UIComponentGM>().CloseUIForm(uiForm); }
        }

        // 改变为蹲下状态
        public void ChangeStateToSquat()
        {
            var csAnimator = componentSystem.GetComponent<PlayerCsAnimator>();
            if (csAnimator.GetSign("IsIdle"))
            {
                // 设置蹲下状态
                csAnimator.CurrentCsRuntimeController.EnterSubStateMachine<SquatSM>().ChangeState<SquatDownState>();

            }
            else if (csAnimator.GetSign("IsRunning"))
            {
                var stateMachine = csAnimator.CurrentCsRuntimeController.ExitCurrentSubStateMachine();
                stateMachine.EnterSubStateMachine<SquatSM>().ChangeState<SquatDownState>();
            }
            else if (csAnimator.GetSign("IsClimb"))
            {
                //componentSystem.GetComponent<PlayerMoveComponent>().StopMove();
                //var controller = csAnimator.CurrentCsRuntimeController as BaseCsRuntimeController;
                //var subMachine = controller.mainStateMachine.Sub;
                //var stateMachine = subMachine.Sub.ExitCurrentStateMachine();
                csAnimator.ChangeState<ClimbUpState>();
            }
            
        }
        // 改变为站立状态
        public void ChangeStateToIdle()
        {
            var csAnimator = componentSystem.GetComponent<PlayerCsAnimator>();
            if (csAnimator.GetSign(PlayerStateSign.Squat))
            {
                if (csAnimator.GetSign("IsClimb"))
                {
                    var controller = csAnimator.CurrentCsRuntimeController as BaseCsRuntimeController;
                    var subMachine = controller.mainStateMachine.Sub;
                    var stateMachine = subMachine.Sub.ExitCurrentStateMachine();
                    stateMachine.ChangeState<SquatUpState>();
                }
                else
                {
                    csAnimator.CurrentCsRuntimeController.Exit();
                    csAnimator.ChangeState<SquatUpState>();
                }
            }
            else if (csAnimator.GetSign("IsDashAtk"))
            {
                csAnimator.CurrentCsRuntimeController.Exit();
                csAnimator.ChangeState<CombatIdleState>();
            }
            RemoveAtkCollison();
        }

        void RemoveAtkCollison()
        {
            foreach (var atkCollAreaNode in atkCollAreaNodeDict.Values)
            {
                var collArea = UIUtils.findChild(atkCollAreaNode, "collArea");
                if (collArea == null) { continue; }
                var baseAtkCollsion = collArea.GetComponent<BaseAtkCollsion>();
                baseAtkCollsion.clearData();
                atkCollAreaNode.SetActive(false);
            }
        }

        FightingFormLogic GetFightPanelLogic()
        {
            string panelPath = UIPrefabPath.GetUIPrefabPath("FightingPanel");
            var uiForm = GameManager.GetGMComponent<UIComponentGM>().GetUIForm(panelPath);
            if (uiForm != null && uiForm.Logic is FightingFormLogic fightFromLogic)
            {
                return fightFromLogic;
            }
            return null;
        }
    }
}