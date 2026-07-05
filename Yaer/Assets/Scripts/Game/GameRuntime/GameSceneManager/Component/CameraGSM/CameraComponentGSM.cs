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
    }
}