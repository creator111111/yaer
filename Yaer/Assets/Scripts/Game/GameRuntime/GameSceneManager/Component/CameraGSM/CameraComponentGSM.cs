using System;
using Cinemachine;
using Game.GameRuntime.GameSceneManager.Base;
using GameFramework.UnityRuntime.Utility;
using UnityEngine;

namespace Game.GameRuntime.GameSceneManager.Component.CameraGSM
{
    public class CameraComponentGSM : BaseComponentGSM
    {
        [SerializeField] private CameraComponent cameraComponent;

        private bool isLock;
        public bool IsLock => isLock;
        public Camera MainCamera => cameraComponent.MainCamera;
        public CameraComponent CameraComponent => cameraComponent;

        public override void OnInit(IGameSceneManager manager)
        {
            base.OnInit(manager);
            if (cameraComponent == null)
            {
                Log.Error("CameraComponentGSM未挂载CameraComponent组件");
                return;
            }

            cameraComponent.Init();
        }

        public void SetFollow(Transform target, Action onComplete = null, bool forceSnapToTarget = true)
        {
            if (target is null)
            {
                Log.Error("target为空");
                return;
            }

            if (isLock)
            {
                Log.Debug("CameraComponentGSM被锁定");
                return;
            }

            cameraComponent.SetFollow(target, onComplete, forceSnapToTarget);
        }

        public void CancelFollow()
        {
            // 商店等未绑 CameraComponent 的场景：避免进场空引用直接炸
            if (cameraComponent == null)
            {
                Log.Error("CameraComponentGSM未挂载CameraComponent组件，CancelFollow跳过");
                return;
            }

            cameraComponent.CancelFollow();
        }

        /// <summary>
        /// 加锁不能SetFollow
        /// </summary>
        /// <param name="value"></param>
        public void SetLock(bool value) => isLock = value;

        public void ChangeVirtualCameraShowSize(float targetSize)
        {
            if (cameraComponent != null) cameraComponent.ChangeVirtualCameraShowSize(targetSize);
        }
        public float GetVirtualCameraShowSize()
        {
            if (cameraComponent != null) { return cameraComponent.GetVirtualCameraShowSize(); }
            else { return 7.9f; } // 摄像机正交尺寸默认7.9f;
        }
        public void ResetVirtualCameraShowSize()
        {
            if (cameraComponent != null) { cameraComponent.ResetVirtualCameraShowSize(); }
        }

        public void ChangeCameraBoundingArea(Collider2D collider)
        {
            if (cameraComponent != null) { cameraComponent.ChangeCameraBoundingArea(collider); }
        }

        /// <summary>
        /// 透传 <see cref="CameraComponent.SetFramingTransposerDepthFollow"/>，供村庄分区 Trigger 调用。
        /// 不受 <see cref="isLock"/> 限制（只改 Framing 参数，不改 Follow 目标）。
        /// </summary>
        public void SetFramingTransposerDepthFollow(
            bool followDepthY,
            float yDamping = 0.7f,
            float deadZoneHeightWhenOff = 1f,
            float deadZoneHeightWhenOn = 0.5f,
            float screenYWhenOn = 0.25f)
        {
            if (cameraComponent == null)
            {
                return;
            }

            cameraComponent.SetFramingTransposerDepthFollow(
                followDepthY, yDamping, deadZoneHeightWhenOff, deadZoneHeightWhenOn, screenYWhenOn);
        }

        /// <summary>进入 CameraDepthFollowZone_Part3 时套用 Part3 Profile，离开时恢复右街默认。</summary>
        public void SetKenMuNiPart3CameraMode(
            bool part3Active,
            CinemachineFramingProfile part3Profile,
            CinemachineFramingProfile streetProfile)
        {
            if (cameraComponent == null)
            {
                return;
            }

            cameraComponent.SetKenMuNiPart3CameraMode(part3Active, part3Profile, streetProfile);
        }
    }
}