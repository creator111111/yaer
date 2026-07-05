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
using Game.GameRuntime.Entities.Player.Components;

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
        protected int monsterCenterLayer = 17; // ???????
        protected int playerLayer = 11; // ??????

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

        /// <summary>
        /// ��ׯ 2.5D�������W/S��ʱ�Ƿ������һ�µ���Ϊ�����ߡ����� Home Idle/Bink/Walk �� <see cref="PlayerInputComponent.HasMoveInput"/> �����жϣ��ڶ��׶� L-01����
        /// </summary>
        protected bool HasVillageExploreDepthMoveIntent()
        {
            TownPlayerLocomotion town = playerLogic.componentSystem.TryGetComponent<TownPlayerLocomotion>();
            return town != null && town.HasVillageDepthMoveForHomeStateMachine();
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
                    animator.speed = curAnimatorSpeed;// ???????
                }
            }
            else
            {
                if (curAnimatorSpeed != 1)
                {
                    curAnimatorSpeed = 1;
                    animator.speed = curAnimatorSpeed;// ???????
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
            // ???????????????????????????
            RemoveAtkCollsion("defalutName");
        }

        // ?????????????
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

        // ????????????
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
                sceneManager.SetSceneObjIsPause(true);// ????????????????
                // ??????????
                var delayTimeAct = GameActionMgr.runDelayTimeAction(1f, () =>
                {
                    var UIMgr = GameManager.GetGMComponent<UIComponentGM>();
                    UIMgr.OpenUIForm(UIPrefabPath.GetUIPrefabPath("DeadPanel"), EUIGroup.Top, new OpenFormArgs());
                });
                // ??????????????????????
                GameManager.GetGameSceneManager().GetModule<InputComponentGSM>().SetAllowOpenMenu(false);
            }
        }

        public virtual void StopMove(string args)
        {
            moveComponent.StopMove();
        }

        // ?????????????????��
        public virtual void PlayAudioSfx(string resPathName)
        {
            playerLogic.commonSfxCpn.ChangeSoundRes(resPathName);
            playerLogic.PlayAudio(playerLogic.commonSfxCpn, true);
        }
        // ??????????????��??��
        public virtual void PlayMoveSfx(string args)
        {
            playerLogic.PlayRunAudio();
        }

        // ?????????????????��
        public virtual void PlayNorAtkAudioInAniFunc(string args)
        {
            playerLogic.PlayNorAtkAudio();
        }

        // ????
        protected virtual void InteractAciton()
        {
            var sceneMgr = playerLogic.sceneManager as BaseGameSceneManager;
            if (!sceneMgr.isCanTouchWithOther)
            {
                return;
            }
            var playerInteractiveComponent = playerLogic.componentSystem.GetComponent<InteractiveComponent>();
            var closestComponent = sceneMgr.GetFirstCanTouchEntiy(playerInteractiveComponent);
            // ????????????
            if (closestComponent != null)
            {
                closestComponent.OnInteractive();
            }
        }
    }
}