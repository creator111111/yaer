using Game.GameMgr.Component;
using Game.GameMgr;
using Game.GameRuntime.Entities.Component;
using Game.GameRuntime.Entities.Component.Anima;
using Game.GameRuntime.Entities.Component.Anima.interf;
using UnityEngine;
using Game.GameMgr.Component.UI;
using Game.Static.Path;
using Game.GameRuntime.GameSceneManager.Base;
using Game.GameRuntime.GameSceneManager.Component;
using Game.GameRuntime.Entities.Component.Move;
using Game.GameRuntime.Entities.Component.Interactive;

namespace Game.GameRuntime.Entities.Player.Components.CsAnimator.Base
{
    public class BasePlayerState : BaseState
    {
        protected bool IsTest = false;

        protected PlayerLogic playerLogic;
        protected PlayerMoveComponent moveComponent;
        protected PlayerInputComponent inputComponent;
        protected PlayerCsAnimator csAnimator;
        protected AnimationEventComponent animationEventComponent;
        protected PlayerStaminaComponent staminaComponent;

        bool hasOpenDeadPanel = false;
        protected int monsterCenterLayer = 17; // 怪物图层
        protected int playerLayer = 11; // 玩家图层

        int curAnimatorSpeed = 1;
        public override void Init(IStateMachine stateMachine, string argsName, string stateName)
        {
            base.Init(stateMachine, argsName, stateName);

            playerLogic = stateMachine.GetEntityLogic<PlayerLogic>();
            moveComponent = playerLogic.componentSystem.GetComponent<PlayerMoveComponent>();
            inputComponent = playerLogic.componentSystem.GetComponent<PlayerInputComponent>();
            csAnimator = playerLogic.componentSystem.GetComponent<PlayerCsAnimator>();
            animationEventComponent = playerLogic.GetComponent<AnimationEventComponent>();
            staminaComponent = playerLogic.componentSystem.GetComponent<PlayerStaminaComponent>();
        }

        public override void Update()
        {
            base.Update();
            CheckAnimationPauseAndPlay();
        }

        protected virtual void CheckAnimationPauseAndPlay()
        {
            var animator = playerLogic.GetComponent<Animator>();
            if (animator == null) { return; }
            var mgr = playerLogic.sceneManager as BaseGameSceneManager;
            if (mgr.GetSceneObjAniIsPause())
            {
                if (curAnimatorSpeed != 0)
                {
                    curAnimatorSpeed = 0;
                    if (playerLogic.componentSystem.TryGetComponent<MoveComponent>() != null)
                    {
                        playerLogic.componentSystem.GetComponent<MoveComponent>().StopMove();
                    }
                    animator.speed = curAnimatorSpeed;// 暂停动画
                }
            }
            else
            {
                if (curAnimatorSpeed != 1)
                {
                    curAnimatorSpeed = 1;
                    animator.speed = curAnimatorSpeed;// 恢复动画
                }
            }
        }

        protected virtual void FootAlign(string msg)
        {
            if (!playerLogic.canInStateSetPos) { return; }
            var currentPos = moveComponent.GetPos();
            Vector2 delta = new Vector2(float.Parse(msg), 0);
            if (moveComponent.Direction == Component.Move.EDirectionType.Left) delta = -delta;
            //playerLogic.transform.DOMove(currentPos + delta, 0.02f);
            moveComponent.TFMovePosition(currentPos + delta);
        }

        protected override void ChangeState<T>()
        {
            base.ChangeState<T>();
            // 转换状态时需要清除当前状态的攻击碰撞体
            RemoveAtkCollsion("defalutName");
        }

        // 创建攻击碰撞体
        protected virtual void CreateAtkCollsion(string atkTypeName)
        {
            if (playerLogic.atkCollAreaNodeDict.ContainsKey(atkTypeName))
            {
                //var oldAtkNode = monsterLogic.atkCollAreaNodeDict[atkTypeName];
                playerLogic.atkCollAreaNodeDict[atkTypeName].SetActive(true);
                var collArea = UIUtils.findChild(playerLogic.atkCollAreaNodeDict[atkTypeName], "collArea");
                if (collArea == null) { return; }
                var baseAtkCollsion = collArea.GetComponent<BaseAtkCollsion>();
                baseAtkCollsion.initAtkDataByName(playerLogic, playerLogic.curAtkCollsionType, atkTypeName);
                return;
                //Object.Destroy(monsterLogic.atkCollAreaNode);
                //monsterLogic.atkCollAreaNode = null;
            }
            var resMgr = GameManager.GetGMComponent<ResComponentGM>();
            var prefabPath = "Assets/GameRes/Prefabs/Entity/Effect/Player/AtkCollsion/CollArea_{0}.prefab";
            var realPath = string.Format(prefabPath, atkTypeName);
            resMgr.LoadAsset<GameObject>(realPath, (obj) =>
            {
                if (playerLogic.atkCollAreaNodeDict.ContainsKey(atkTypeName)) { return; }
                playerLogic.atkCollAreaNodeDict[atkTypeName] = Object.Instantiate(obj);
                var parentNode = playerLogic.atkCollNodeParent == null ? playerLogic.gameObject : playerLogic.atkCollNodeParent;
                playerLogic.atkCollAreaNodeDict[atkTypeName].transform.SetParent(parentNode.transform, false);
                var collArea = UIUtils.findChild(playerLogic.atkCollAreaNodeDict[atkTypeName], "collArea");
                if (collArea == null) { return; }
                var baseAtkCollsion = collArea.GetComponent<BaseAtkCollsion>();
                baseAtkCollsion.initAtkDataByName(playerLogic, playerLogic.curAtkCollsionType, atkTypeName);
            });
        }

        // 移除攻击碰撞体
        protected void RemoveAtkCollsion(string atkTypeName)
        {
            foreach (var atkCollAreaNode in playerLogic.atkCollAreaNodeDict.Values)
            {
                var collArea = UIUtils.findChild(atkCollAreaNode, "collArea");
                if (collArea == null) { continue; }
                var baseAtkCollsion = collArea.GetComponent<BaseAtkCollsion>();
                baseAtkCollsion.clearData();
                atkCollAreaNode.SetActive(false);
            }
        }

        public virtual void ShowDeadPanel()
        {
            if (!hasOpenDeadPanel)
            {
                hasOpenDeadPanel = true;
                var sceneManager = playerLogic.sceneManager as BaseGameSceneManager;
                sceneManager.SetSceneObjIsPause(true);// 人物死亡后暂停游戏
                // 打开死亡界面
                var delayTimeAct = GameActionMgr.runDelayTimeAction(1f, () =>
                {
                    var UIMgr = GameManager.GetGMComponent<UIComponentGM>();
                    UIMgr.OpenUIForm(UIPrefabPath.GetUIPrefabPath("DeadPanel"), EUIGroup.Top, new OpenFormArgs());
                });
                // 死亡界面出现时不允许打开菜单
                GameManager.GetGameSceneManager().GetModule<InputComponentGSM>().SetAllowOpenMenu(false);
            }
        }

        public virtual void StopMove(string args)
        {
            moveComponent.StopMove();
        }

        // 在动画某一帧播放一个音效
        public virtual void PlayAudioSfx(string resPathName)
        {
            playerLogic.commonSfxCpn.ChangeSoundRes(resPathName);
            playerLogic.PlayAudio(playerLogic.commonSfxCpn, true);
        }
        // 在动画某一帧播放走路音效
        public virtual void PlayMoveSfx(string args)
        {
            playerLogic.PlayRunAudio();
        }

        // 在动画某一帧播放攻击音效
        public virtual void PlayNorAtkAudioInAniFunc(string args)
        {
            playerLogic.PlayNorAtkAudio();
        }

        // 交互
        protected virtual void InteractAciton()
        {
            var sceneMgr = playerLogic.sceneManager as BaseGameSceneManager;
            if (!sceneMgr.isCanTouchWithOther)
            {
                return;
            }
            var playerInteractiveComponent = playerLogic.componentSystem.GetComponent<InteractiveComponent>();
            var closestComponent = sceneMgr.GetFirstCanTouchEntiy(playerInteractiveComponent);
            // 交互最近的对象
            if (closestComponent != null)
            {
                closestComponent.OnInteractive();
            }
        }
    }
}