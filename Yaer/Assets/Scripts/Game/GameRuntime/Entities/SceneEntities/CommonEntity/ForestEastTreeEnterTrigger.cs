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
    /// <summary>
    /// 东郊倒树进/出洞门：碰到后等 Sign（或超时）→ 自动走 → 黑幕传送。
    /// 对白盒 <c>EnterTreeBridge</c> 在传送点上，人还在洞外时默认碰不到。
    /// </summary>
    public class ForestEastTreeEnterTrigger : MonoBehaviour
    {
        public bool isEnterTree;
        public bool isFromLeftEnter;
        public bool isFromLeftOut;

        public bool hasFindPlayer; // 当前是否已锁到玩家
        PlayerLogic playerLogic;
        public bool hasInTiggerStory; // 是否已进入黑幕传送流程

        /// <summary>
        /// Sign 等待超时（秒）。超时仍强制走完进洞黑幕（OPEN Q4），禁止永久 Disable+Pause。
        /// 替代方案：超时解锁并清 hasFindPlayer 让玩家重试——易停在半锁，本期不用。
        /// </summary>
        const float SignWaitTimeoutSeconds = 2f;
        float findPlayerTime;

        /// <summary>
        /// 洞内主角相对传送前脚高的偏移。复验 R1：须为 0。
        /// 原因：Player Body 与 Layer13 <c>GroundUp</c>（顶≈−6.35）相交；offset=1/0.35 嵌深约 1.34/0.69，
        /// 像顶死墙，碎卵脚本再关盒也没用。出洞仍 <c>oldY + offset</c>（0 时进出脚高不变）。
        /// 净空另靠场景 <c>GroundUp</c> 顶 ≤ Body 底（≈脚−0.09）−ε；禁止再降 GroundCenter、禁止改回 1。
        /// 替代：只抄 In/OutPos 完整 XY（OutPos.y=-6.3 与洞外≈-6.6 不一致）。
        /// </summary>
        const float InTreePlayerYOffset = 0f;

        private void OnCollisionEnter2D(Collision2D collision)
        {
            var enetityLogic = collision.collider.GetComponent<ColliderResponder>()?.GetEntityLogic() as PlayerLogic;
            OnPlayerDetected(enetityLogic);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            var enetityLogic = collision.GetComponent<ColliderResponder>()?.GetEntityLogic() as PlayerLogic;
            OnPlayerDetected(enetityLogic);
        }

        /// <summary>
        /// 命中进/出洞门：先停自动爬，再锁操作等黑幕。
        /// 原因：SquatUp 盒宽约 80，洞口仍在区内时 Exit 不发生，StopAutoCrawl 可能永不跑，
        /// 会和本脚本的 Disable/AutoMove 叠锁。停爬后立刻再 Disable；进洞期间 blockAutoCrawl，
        /// 避免爬区 Update 看到已清 cmds 后又 Start。
        /// </summary>
        void OnPlayerDetected(PlayerLogic enetityLogic)
        {
            if (enetityLogic == null) { return; }
            if (enetityLogic.isDead) { return; }
            if (hasFindPlayer) { return; }
            hasFindPlayer = true;
            findPlayerTime = Time.time;
            playerLogic = enetityLogic;
            CanNotSomeActionArea.StopAutoCrawlForPlayer(playerLogic);
            CanNotSomeActionArea.SetBlockAutoCrawl(true);
            playerLogic.DisablePlayerMove(); // 禁止玩家移动
            GameManager.GetGameSceneManager().SetSceneObjIsPause(true); // 暂停场景物
            Debug.Log($"[TreeEnter] find player isEnter={isEnterTree} fromLeftEnter={isFromLeftEnter} fromLeftOut={isFromLeftOut}");
        }

        protected void Update()
        {
            if (hasInTiggerStory) { return; }
            if (!hasFindPlayer) { return; } // 玩家进入范围开始检测
            if (playerLogic == null || playerLogic.isDead) { return; }
            var csAnimator = playerLogic.componentSystem.GetComponent<PlayerCsAnimator>();
            // Idle / Run / Squat 才能进黑幕（Climb 在 SquatSM 下时 Squat Sign 一般为 true）。
            // 卡在 Hurt / 跳 / Dash 等三者全假会永久 Disable+Pause → 超时强制继续（OPEN Q4）。
            bool signReady = csAnimator != null
                && (csAnimator.GetSign("IsIdle")
                    || csAnimator.GetSign("IsRunning")
                    || csAnimator.GetSign(PlayerStateSign.Squat));
            bool timedOut = Time.time - findPlayerTime >= SignWaitTimeoutSeconds;
            if (!signReady && !timedOut)
            {
                return;
            }
            if (!signReady && timedOut)
            {
                Debug.LogWarning($"[TreeEnter] Sign wait timeout {SignWaitTimeoutSeconds}s, force ChangePlayerState isEnter={isEnterTree}");
            }
            hasInTiggerStory = true;
            ForestEastTreeBridgeStoryMgr.getInstance().playerIsInTreeBridge = isEnterTree;
            if (!isEnterTree)
            {
                ForestEastTreeBridgeStoryMgr.getInstance().StopCameraAction();
            }

            // 改变玩家状态
            ChangePlayerState();
        }

        void ChangePlayerState()
        {
            if (isEnterTree)
            {
                playerLogic.ChangeStateToSquat();
            }
            ForestEastTreeBridgeStoryMgr.getInstance().ChangeEnterAndOutNodeActive(isEnterTree);
            // 让玩家向当前朝向自动移动一小段再进黑幕
            var moveCpn = playerLogic.componentSystem.GetComponent<PlayerMoveComponent>();
            var autoInputType = moveCpn.IsTurnRight ? AutoInputMove.Right : AutoInputMove.Left;
            playerLogic.componentSystem.GetComponent<PlayerInputComponent>().AutoMoveState = autoInputType;
            GameActionMgr.runDelayTimeAction(1f, () =>
            {
                // 改变玩家的位置
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
                            CanNotSomeActionArea.SetBlockAutoCrawl(false);
                            Debug.LogError("============TreeBridge:Not Find TargetNode To Set Player Pos!!!");
                            return;
                        }
                        // 只抄落点 X。Y：进洞相对当前脚高再降 1，出洞对称加回（方案 A）。
                        // 禁止抄 target.y：In/OutPos 都是 -6.3 且从未被使用；禁止靠降地板——无重力吸附。
                        var oldPos = playerLogic.gameObject.transform.position;
                        var afterY = isEnterTree
                            ? oldPos.y - InTreePlayerYOffset
                            : oldPos.y + InTreePlayerYOffset;
                        playerLogic.gameObject.transform.position = new Vector2(targetObj.transform.position.x, afterY);
                        Debug.Log($"[TreeBridgePlayerY] enter={isEnterTree} before={oldPos.y} after={afterY} target=({targetObj.transform.position.x},{targetObj.transform.position.y})");
                        // 改变摄像机边界与尺寸
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
                            // 恢复玩家行动
                            playerLogic.DisablePlayerMove(false);
                            playerLogic.isEnableSquatUp = !isEnterTree;
                            hasInTiggerStory = false;
                            hasFindPlayer = false;
                            CanNotSomeActionArea.SetBlockAutoCrawl(false);
                            // 进出洞判定结束后重新激活各故事触发器
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
