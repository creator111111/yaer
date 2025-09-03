using System;
using Cinemachine;
using UnityEngine;

namespace Game.GameRuntime.GameSceneManager.Component.CameraGSM
{
    public class CameraComponent : MonoBehaviour
    {
        [SerializeField] private CinemachineBrain cinemachineBrain;
        [SerializeField] private CinemachineVirtualCamera virtualCamera;

        private Camera mainCamera;
        private Transform target;
        private Action onComplete;
        public bool FollowOnComplete => Vector2.Distance(target.position, virtualCamera.transform.position) < 0.1f;
        public Camera MainCamera => mainCamera;
        public CinemachineVirtualCamera VirtualCamera => virtualCamera;

        public void Init()
        {
            mainCamera = Camera.main;
            if (cinemachineBrain == null) Debug.LogError("CinemachineBrain未绑定");
            if (virtualCamera == null)
            {
                Debug.LogError("CinemachineVirtualCamera未绑定");
            }
            InitImpulseListener();
        }

        private void Update()
        {
            if (onComplete != null && FollowOnComplete)
            {
                onComplete?.Invoke();
                onComplete = null;
            }
        }

        private void InitImpulseListener()
        {
            if (!virtualCamera.TryGetComponent<CinemachineImpulseListener>(out var impulseListener))
            {
                impulseListener = virtualCamera.gameObject.AddComponent<CinemachineImpulseListener>();
            }
            virtualCamera.AddExtension(impulseListener);
        }

        public void SetFollow(Transform target, Action onComplete = null)
        {
            this.target = target;
            this.onComplete = onComplete;
            //
            virtualCamera.Follow = target;
            virtualCamera.ForceCameraPosition(new Vector3(target.position.x, target.position.y, virtualCamera.transform.position.z), virtualCamera.transform.rotation);
        }
        
        public void CancelFollow()
        {
            virtualCamera.Follow = null;
        }

        // 修改摄像机的正交尺寸
        public void ChangeVirtualCameraShowSize(float targetSize)
        {
            virtualCamera.GetComponent<CinemachineVirtualCamera>().m_Lens.OrthographicSize = targetSize;
        }

        // 获取当前摄像机的正交尺寸
        public float GetVirtualCameraShowSize()
        {
            return virtualCamera.GetComponent<CinemachineVirtualCamera>().m_Lens.OrthographicSize;
        }

        // 重置摄像机的正交尺寸
        public void ResetVirtualCameraShowSize()
        {
            //virtualCamera.GetComponent<CinemachineConfiner>().m_ConfineScreenEdges = true;// 限制在屏幕边缘
            virtualCamera.GetComponent<CinemachineVirtualCamera>().m_Lens.OrthographicSize = 7.9f;
        }

        // 修改相机的边界碰撞区域
        public void ChangeCameraBoundingArea(Collider2D newColliderArea)
        {
            virtualCamera.GetComponent<CinemachineConfiner>().m_BoundingShape2D = newColliderArea;
        }
    }
}