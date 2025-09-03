using Game.GameMgr;
using Game.GameMgr.Component.UI;
using Game.GameRuntime.Entities.Component.Anima;
using Game.GameRuntime.Entities.Component.Anima.interf;
using Game.GameRuntime.Entities.Component.PhysicsDetect;
using Game.GameRuntime.Entities.Player;
using Game.GameRuntime.Entities.Player.Components;
using Game.GameRuntime.Entities.Player.Components.CsAnimator;
using Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat.State.Squat;
using Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat.State.Squat.Climb;
using Game.GameRuntime.GameSceneManager.Component.CameraGSM;
using Game.GameRuntime.UI.FormLogic.Black;
using Game.Static.Path;
using GameFramework.CoreExtend.Component;
using UnityEngine;
using static Game.GameRuntime.Entities.Player.Components.PlayerInputComponent;

namespace Game.GameRuntime.Entities.SceneEntities
{
    // 树洞进入或出去触发脚本
    public class ForestEastTreeEnterTrigger : MonoBehaviour
    {
        
        public bool isEnterTree;
        public bool isFromLeftEnter;
        public bool isFromLeftOut;

        public bool hasFindPlayer; // 当前是否触碰玩家
        PlayerLogic playerLogic;
        public bool hasInTiggerStory; // 是否处于触发进入或者出去状态中
        //public InteractiveComponent interactiveComponent;
        //protected internal override void OnInit(object userData)
        //{
        //    base.OnInit(userData);
        //    //interactiveComponent = componentSystem.GetComponent<InteractiveComponent>();
        //    interactiveComponent.onEnterInteractiveEvent += SetAutoMove;
        //}
        private void OnCollisionEnter2D(Collision2D collision)
        {
            var enetityLogic = collision.collider.GetComponent<ColliderResponder>()?.GetEntityLogic() as PlayerLogic;
            if (enetityLogic == null) { return; }
            if (enetityLogic.isDead) { return; }
            if (hasFindPlayer) { return; }
            hasFindPlayer = true;
            playerLogic = enetityLogic;
            playerLogic.DisablePlayerMove();// 禁用玩家行为
            GameManager.GetGameSceneManager().SetSceneObjIsPause(true); // 暂停
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            var enetityLogic = collision.GetComponent<ColliderResponder>()?.GetEntityLogic() as PlayerLogic;
            if (enetityLogic == null) { return; }
            if (enetityLogic.isDead) { return; }
            if (hasFindPlayer) { return; }
            hasFindPlayer = true;
            playerLogic = enetityLogic;
            playerLogic.DisablePlayerMove();// 禁用玩家行为
            GameManager.GetGameSceneManager().SetSceneObjIsPause(true); // 暂停
        }

        protected void Update()
        {
            if (hasInTiggerStory) { return; }
            if (!hasFindPlayer) { return; }// 玩家进入范围后开始检测
            if (playerLogic.isDead) { return; }
            var csAnimator = playerLogic.componentSystem.GetComponent<PlayerCsAnimator>();
            // 当人物处于站立在地上的状态时，才进行后续逻辑
            if (!csAnimator.GetSign("IsIdle") && !csAnimator.GetSign("IsRunning") &&
                !csAnimator.GetSign(PlayerStateSign.Squat)) 
            {
                return; 
            }
            hasInTiggerStory = true;
            ForestEastTreeBridgeStoryMgr.getInstance().playerIsInTreeBridge = isEnterTree;
            if (!isEnterTree)
            {
                ForestEastTreeBridgeStoryMgr.getInstance().StopCameraAction();
            }
            
            // 改变人物状态
            ChangePlayerState();
        }

        void ChangePlayerState()
        {
            var csAnimator = playerLogic.componentSystem.GetComponent<PlayerCsAnimator>();
            if (isEnterTree)
            {
                playerLogic.ChangeStateToSquat();
            }
            //else
            //{
            //    ChangePlayerStateOnOutTree();
            //}
            ForestEastTreeBridgeStoryMgr.getInstance().ChangeEnterAndOutNodeActive(isEnterTree);
            // 设置人物走动一段距离后再黑屏
            var moveCpn = playerLogic.componentSystem.GetComponent<PlayerMoveComponent>();
            var autoInputType = moveCpn.IsTurnRight ? AutoInputMove.Right : AutoInputMove.Left;
            playerLogic.componentSystem.GetComponent<PlayerInputComponent>().AutoMoveState = autoInputType;
            GameActionMgr.runDelayTimeAction(1f, () =>
            {
                // 设置玩家的位置
                ChangePlayerPos();
            });
        }

        public void ChangePlayerStateOnOutTree()
        {
            playerLogic.ChangeStateToIdle();
        }

        public void ChangePlayerPos()
        {
            GameManager.GetGameSceneManager().SetSceneObjIsPause(false);

            var uiPath = UIPrefabPath.GetUIPrefabPath("BlackPanel");
            GameManager.GetGMComponent<UIComponentGM>().OpenUIForm(uiPath, EUIGroup.System, new OpenFormArgs() {
                userData = new ShowBlackFormArgs()
                {
                    showType = BlackFadeType.FadeShow,
                    onShowEnd = blackFormLogic =>
                    {
                        playerLogic.componentSystem.GetComponent<PlayerInputComponent>().AutoMoveState = AutoInputMove.None;
                        if (!isEnterTree) { ChangePlayerStateOnOutTree(); }
                        var targetObj = GetTargetGameObj();
                        if (targetObj == null) {
                            playerLogic.DisablePlayerMove(false);
                            Debug.LogError("============TreeBridge:Not Find TargetNode To Set Player Pos!!!");
                            return; 
                        }
                        // 设置人物坐标
                        var oldPos = playerLogic.gameObject.transform.position;
                        playerLogic.gameObject.transform.position = new Vector2(targetObj.transform.position.x, oldPos.y);
                        // 改变摄像机部分属性
                        var sceneMgr = GameManager.GetGameSceneManager();
                        var cameraMgr = sceneMgr.GetModule<CameraComponentGSM>();
                        ForestEastTreeBridgeStoryMgr.getInstance().ChangeCamera(isEnterTree, cameraMgr);
                        if (isEnterTree)
                        {
                            var csAnimator = playerLogic.componentSystem.GetComponent<PlayerCsAnimator>();
                            var controller = csAnimator.CurrentCsRuntimeController as BaseCsRuntimeController;
                            var curState = controller.mainStateMachine.Sub;
                            curState.EnterSubStateMachine<ClimbSM>().ChangeState<ClimbUpState>();
                        }
                        ForestEastTreeBridgeStoryMgr.getInstance().OnEnterOrOutTreeBridge(isEnterTree);
                        blackFormLogic.CloseFormFade(() =>
                        {
                            // 恢复人物行动
                            playerLogic.DisablePlayerMove(false);
                            playerLogic.isEnableSquatUp = !isEnterTree;
                            hasInTiggerStory = false;
                            hasFindPlayer = false;
                            // 人物可行动时重新激活所有故事触发点
                            ForestEastTreeBridgeStoryMgr.getInstance().AwakeAllStoryNodeActive();
                            
                        });
                        
                    }
                }
            });
        }

        GameObject GetTargetGameObj()
        {
            if (isEnterTree)
            {
                if (isFromLeftEnter)
                {
                    return ForestEastTreeBridgeStoryMgr.getInstance().GetEnterStartNode();
                }
                else
                {
                    return ForestEastTreeBridgeStoryMgr.getInstance().GetEnterStartNode(false);
                }
            }
            else
            {
                if (isFromLeftOut)
                {
                    return ForestEastTreeBridgeStoryMgr.getInstance().GetOutStartNode();
                }
                else
                {
                    return ForestEastTreeBridgeStoryMgr.getInstance().GetOutStartNode(false);
                }
            }
        }
    }

}

