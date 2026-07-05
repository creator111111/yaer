using System;
using Cinemachine;
using UnityEngine;

namespace Game.GameRuntime.GameSceneManager.Component.CameraGSM
{
    public class CameraComponent : MonoBehaviour
    {
        [SerializeField] private CinemachineBrain cinemachineBrain;
        [SerializeField] private CinemachineVirtualCamera virtualCamera;

        /// <summary>
        /// <see cref="SetFollow"/> 在 <c>forceSnapToTarget==true</c> 且 smoothTime&gt;0 时，手推 vcam 靠近目标所用 SmoothDamp 时间常数。
        /// 为 0 时当帧直接对齐并挂 Follow（交还 Cinemachine）。
        /// </summary>
        [Tooltip(">0：切跟拍目标时先用手推平滑；0：当帧瞬切。")]
        public float smoothTime = 0.3f;

        /// <summary>在 (target.x, target.y, 当前 vcam z) 上再叠的世界空间偏移。</summary>
        [Tooltip("跟拍对齐终点的世界空间偏移。")]
        public Vector3 followSnapOffset = Vector3.zero;

        [Header("手推收束（防 onComplete 永不触发）")]
        [Tooltip("手推 vcam 靠近目标时，使用不受 timeScale 影响的步进；对话/暂停时 timeScale=0 仍能收束并回调。")]
        [SerializeField] private bool useUnscaledTimeForHandSnap = true;
        [Tooltip("手推超过该秒数(实时)后强制收束并触发 onComplete，避免跟移动中的目标一直达不到阈值。0=不限时。")]
        [SerializeField] private float maxHandSnapRealSeconds = 2.5f;

        private Camera mainCamera;
        private Transform target;
        private Action onComplete;

        /// <summary> 正在做「去瞬移、改为平滑」的跟拍前对齐时，Cinemachine 的 Follow 会暂时为 null。 </summary>
        private bool _isSmoothingSnap;
        private Vector3 _snapDampVelocity;
        private float _handSnapStartUnscaledTime;

        public bool FollowOnComplete => target != null && Vector2.Distance(target.position, virtualCamera.transform.position) < 0.1f;
        public Camera MainCamera => mainCamera;
        public CinemachineVirtualCamera VirtualCamera => virtualCamera;

        public void Init()
        {
            mainCamera = Camera.main;
            if (cinemachineBrain == null) { Debug.LogError("CinemachineBrain未绑定"); }
            if (virtualCamera == null) { Debug.LogError("CinemachineVirtualCamera未绑定"); }
            // 与画面同频，机位每帧更新，跟玩家无「固定步长台阶感」；不与 FixedUpdate 混用手推（曾导致 50Hz/60Hz 交错抖）
            if (cinemachineBrain != null)
            {
                cinemachineBrain.m_UpdateMethod = CinemachineBrain.UpdateMethod.LateUpdate;
                cinemachineBrain.m_BlendUpdateMethod = CinemachineBrain.BrainUpdateMethod.LateUpdate;
            }
            InitImpulseListener();
        }

        /// <summary>
        /// 手推 SmoothDamp 与 onComplete（非手推路径）均放在 LateUpdate，与 Brain、渲染同相位，避免 Fixed 里推机位、Late 里 CM 再算一道的错位抖。
        /// </summary>
        private void LateUpdate()
        {
            if (virtualCamera == null || target == null) { return; }

            if (_isSmoothingSnap)
            {
                var z = virtualCamera.transform.position.z;
                var goal = new Vector3(target.position.x, target.position.y, z) + followSnapOffset;
                // timeScale=0 时若仍用 deltaTime，手推不前进、onComplete 永不触发，剧情回调（如 ForestScene 下一段）会卡死
                var dt = useUnscaledTimeForHandSnap ? Time.unscaledDeltaTime : Time.deltaTime;

                virtualCamera.transform.position = Vector3.SmoothDamp(
                    virtualCamera.transform.position,
                    goal,
                    ref _snapDampVelocity,
                    Mathf.Max(0.0001f, smoothTime),
                    Mathf.Infinity,
                    dt
                );

                var toGoal2 = (virtualCamera.transform.position - goal).sqrMagnitude;
                // 原 0.02^2 过严，跟移动中的玩家时 vcam 可能长期进不了阈值，onComplete 永不触发
                var reached = toGoal2 < 0.01f;
                var timeout = maxHandSnapRealSeconds > 0.001f
                    && Time.unscaledTime - _handSnapStartUnscaledTime >= maxHandSnapRealSeconds;
                if (reached || timeout)
                {
                    if (timeout && !reached) { virtualCamera.transform.position = goal; }

                    _isSmoothingSnap = false;
                    _handSnapStartUnscaledTime = 0f;
                    _snapDampVelocity = Vector3.zero;
                    ApplyFollowWithCinemachineStateAligned(target, dt);
                    onComplete?.Invoke();
                    onComplete = null;
                }
                return;
            }

            if (onComplete != null && FollowOnComplete)
            {
                onComplete.Invoke();
                onComplete = null;
            }
        }

        private void ApplyFollowWithCinemachineStateAligned(Transform followTarget, float updateDeltaTime)
        {
            if (virtualCamera == null || followTarget == null) { return; }
            virtualCamera.Follow = followTarget;
            virtualCamera.PreviousStateIsValid = false;
            virtualCamera.UpdateCameraState(Vector3.up, updateDeltaTime);
            var s = virtualCamera.State;
            virtualCamera.ForceCameraPosition(s.FinalPosition, s.FinalOrientation);
        }

        private void InitImpulseListener()
        {
            if (!virtualCamera.TryGetComponent<CinemachineImpulseListener>(out var impulseListener))
            {
                impulseListener = virtualCamera.gameObject.AddComponent<CinemachineImpulseListener>();
            }
            virtualCamera.AddExtension(impulseListener);
        }

        public void SetFollow(Transform newTarget, Action onComplete = null, bool forceSnapToTarget = true)
        {
            this.target = newTarget;
            this.onComplete = onComplete;

            if (forceSnapToTarget)
            {
                if (smoothTime > 0.0001f)
                {
                    _isSmoothingSnap = true;
                    _snapDampVelocity = Vector3.zero;
                    _handSnapStartUnscaledTime = Time.unscaledTime;
                    virtualCamera.Follow = null;
                }
                else
                {
                    _isSmoothingSnap = false;
                    _handSnapStartUnscaledTime = 0f;
                    _snapDampVelocity = Vector3.zero;
                    ApplyFollowWithCinemachineStateAligned(
                        newTarget,
                        useUnscaledTimeForHandSnap ? Time.unscaledDeltaTime : Time.deltaTime);
                    onComplete?.Invoke();
                    onComplete = null;
                }
            }
            else
            {
                _isSmoothingSnap = false;
                _handSnapStartUnscaledTime = 0f;
                _snapDampVelocity = Vector3.zero;
                virtualCamera.Follow = newTarget;
            }
        }

        public void CancelFollow()
        {
            _isSmoothingSnap = false;
            _handSnapStartUnscaledTime = 0f;
            _snapDampVelocity = Vector3.zero;
            onComplete = null;
            virtualCamera.Follow = null;
        }

        public void ChangeVirtualCameraShowSize(float targetSize)
        {
            virtualCamera.GetComponent<CinemachineVirtualCamera>().m_Lens.OrthographicSize = targetSize;
        }

        public float GetVirtualCameraShowSize()
        {
            return virtualCamera.GetComponent<CinemachineVirtualCamera>().m_Lens.OrthographicSize;
        }

        public void ResetVirtualCameraShowSize()
        {
            virtualCamera.GetComponent<CinemachineVirtualCamera>().m_Lens.OrthographicSize = 7.9f;
        }

        public void ChangeCameraBoundingArea(Collider2D newColliderArea)
        {
            virtualCamera.GetComponent<CinemachineConfiner>().m_BoundingShape2D = newColliderArea;
        }
    }
}
