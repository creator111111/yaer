using Game.GameMgr;
using Game.GameRuntime.Entities.Player;
using Game.GameRuntime.Entities.Player.Components;
using Game.GameRuntime.GameSceneManager.Component.CameraGSM;
using Game.Static.Name.Res;
using NodeCanvas.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.GameRuntime.Story.Node
{
    public class PlayerAutoMoveActionTask : ActionTask
    {
        public BBParameter<PlayerInputComponent.AutoInputMove> AutoInputMove;
        public BBParameter<Transform> Destination;

        /// <summary>
        /// 0922 序章链 P1：为 true 时自动走期间锁镜（CancelFollow+Lock），结束再跟拍定格。
        /// Prefab 可显式勾选；未勾选时东郊 FirstEnter 的 AutoMoveDestination 仍自动启用。
        /// </summary>
        public bool lockCameraWhileMoving;

        private PlayerLogic player;
        private PlayerInputComponent playerInputComponent;
        private bool _didLockCamera;

        protected override string OnInit()
        {
            player = GameObject.FindObjectOfType<PlayerLogic>();
            playerInputComponent = player.componentSystem.GetComponent<PlayerInputComponent>();
            return base.OnInit();
        }

        protected override void OnExecute()
        {
            _didLockCamera = false;
            if (ShouldLockCameraWhileMoving())
            {
                TryLockCameraForAutoMove();
            }

            playerInputComponent.AutoMoveState = AutoInputMove.value;
        }

        protected override void OnUpdate()
        {
            if (Destination == null || Destination.value == null || player == null)
            {
                EndAutoMoveAndUnlock();
                EndAction();
                return;
            }

            float deltaX = (player.transform.position - Destination.value.position).x;
            if (Mathf.Abs(deltaX) < 0.1f)
            {
                EndAutoMoveAndUnlock();
                EndAction();
            }
        }

        protected override void OnStop()
        {
            // 被中断时也要解锁，避免其它路径残留 Lock
            if (_didLockCamera)
            {
                EndAutoMoveAndUnlock();
            }
        }

        /// <summary>
        /// Prefab 勾选，或东郊 FirstEnter 锚点名命中时锁镜。
        /// 原因：East smoothTime=0 揭幕已定格，但 AutoMove 横移时 Framing XDamping 仍软跟 → 对白中「闪/滑」。
        /// </summary>
        private bool ShouldLockCameraWhileMoving()
        {
            if (lockCameraWhileMoving)
            {
                return true;
            }

            if (Destination == null || Destination.value == null)
            {
                return false;
            }

            return SceneManager.GetActiveScene().name == SceneName.ForestEastScene
                && Destination.value.name == "AutoMoveDestination";
        }

        private void TryLockCameraForAutoMove()
        {
            var cameraGsm = ResolveCameraGsm();
            if (cameraGsm == null)
            {
                return;
            }

            cameraGsm.CancelFollow();
            cameraGsm.SetLock(true);
            _didLockCamera = true;
        }

        private void EndAutoMoveAndUnlock()
        {
            if (playerInputComponent != null)
            {
                playerInputComponent.AutoMoveState = PlayerInputComponent.AutoInputMove.None;
            }

            if (!_didLockCamera)
            {
                return;
            }

            _didLockCamera = false;
            var cameraGsm = ResolveCameraGsm();
            if (cameraGsm == null || player == null)
            {
                return;
            }

            cameraGsm.SetLock(false);
            // 跟拍贴回玩家；East smoothTime=0 → 当帧对齐，不二次手推
            cameraGsm.SetFollow(player.transform, null, forceSnapToTarget: true);
        }

        private static CameraComponentGSM ResolveCameraGsm()
        {
            var sceneMgr = GameManager.GetGameSceneManager();
            return sceneMgr != null ? sceneMgr.GetModule<CameraComponentGSM>() : null;
        }
    }
}
