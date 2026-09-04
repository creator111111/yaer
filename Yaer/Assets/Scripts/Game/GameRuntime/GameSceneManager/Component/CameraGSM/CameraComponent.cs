using System;
using Cinemachine;
using UnityEngine;

namespace Game.GameRuntime.GameSceneManager.Component.CameraGSM
{
    /// <summary>Cinemachine Framing Transposer 一组参数（旧单机 Apply 路径 / 文档对照保留）。</summary>
    [Serializable]
    public struct CinemachineFramingProfile
    {
        public float screenX;
        public float screenY;
        public float deadZoneWidth;
        public float deadZoneHeight;
        public float xDamping;
        public float yDamping;
        public float softZoneWidth;
        public float softZoneHeight;
        public float biasX;
        public float biasY;

        public static CinemachineFramingProfile KenMuNiStreetDefault => new CinemachineFramingProfile
        {
            screenX = 0.5f,
            screenY = 0.5f,
            deadZoneWidth = 0f,
            deadZoneHeight = 1f,
            xDamping = 0.7f,
            yDamping = 0f,
            softZoneWidth = 0.25f,
            softZoneHeight = 1f,
            biasX = 0f,
            biasY = 0f,
        };

        /// <summary>Part3 高台表（应固化在 VCam_Part3 Inspector；静态仅作文档/旧 API 兜底）。</summary>
        public static CinemachineFramingProfile KenMuNiPart3DepthFollow => new CinemachineFramingProfile
        {
            screenX = 0.5f,
            screenY = 0.88f,
            deadZoneWidth = 0f,
            deadZoneHeight = 0f,
            xDamping = 0.7f,
            yDamping = 0f,
            softZoneWidth = 0.25f,
            softZoneHeight = 0.351f,
            biasX = 0.5f,
            biasY = 0.5f,
        };
    }

    /// <summary>
    /// 场景相机组件。KenMuNi1 Part3 起支持双 VirtualCamera：Street（主）+ Part3，
    /// 进区只切 Priority，Body 写死在 Inspector。
    /// </summary>
    public class CameraComponent : MonoBehaviour
    {
        [SerializeField] private CinemachineBrain cinemachineBrain;
        [Tooltip("街道路 / 主 VCam（Shop 等仍吃此引用）。")]
        [SerializeField] private CinemachineVirtualCamera virtualCamera;
        [Tooltip("Part3 高台 VCam；为空则退回旧「单机 Apply Profile」。")]
        [SerializeField] private CinemachineVirtualCamera virtualCameraPart3;

        [Header("双机 Priority（仅 Part3 机切换时用）")]
        [SerializeField] private int streetPriority = 10;
        [SerializeField] private int part3PriorityWhenActive = 20;
        [SerializeField] private int part3PriorityWhenStandby = 0;

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

        public bool FollowOnComplete =>
            target != null && virtualCamera != null
            && Vector2.Distance(target.position, virtualCamera.transform.position) < 0.1f;

        public Camera MainCamera => mainCamera;
        /// <summary>主/街道路 VCam（兼容旧调用面）。</summary>
        public CinemachineVirtualCamera VirtualCamera => virtualCamera;
        /// <summary>Part3 高台 VCam；未配置时为 null。</summary>
        public CinemachineVirtualCamera VirtualCameraPart3 => virtualCameraPart3;
        /// <summary>是否已挂双机（Zone 走 Priority，不再 Apply Body）。</summary>
        public bool HasPart3VirtualCamera => virtualCameraPart3 != null;

        public void Init()
        {
            mainCamera = Camera.main;
            if (cinemachineBrain == null) { Debug.LogError("CinemachineBrain未绑定"); }
            if (virtualCamera == null) { Debug.LogError("CinemachineVirtualCamera未绑定"); }

            if (cinemachineBrain != null)
            {
                cinemachineBrain.m_UpdateMethod = CinemachineBrain.UpdateMethod.LateUpdate;
                cinemachineBrain.m_BlendUpdateMethod = CinemachineBrain.BrainUpdateMethod.LateUpdate;
            }

            // 街道路 Standby 也要更新：Zone A1 用 Street.State 算框，Live 为 Part3 时仍须新鲜
            EnsureStandbyUpdateAlways(virtualCamera);
            EnsureStandbyUpdateAlways(virtualCameraPart3);

            if (virtualCamera != null)
            {
                virtualCamera.Priority = streetPriority;
            }

            if (virtualCameraPart3 != null)
            {
                virtualCameraPart3.Priority = part3PriorityWhenStandby;
            }

            InitImpulseListener(virtualCamera);
            InitImpulseListener(virtualCameraPart3);
        }

        private static void EnsureStandbyUpdateAlways(CinemachineVirtualCamera vcam)
        {
            if (vcam == null)
            {
                return;
            }

            vcam.m_StandbyUpdate = CinemachineVirtualCameraBase.StandbyUpdateMode.Always;
        }

        /// <summary>
        /// 手推 SmoothDamp 与 onComplete（非手推路径）均放在 LateUpdate，与 Brain、渲染同相位。
        /// 方案 E2：手推期间 Street / Part3 两台 Transform 对齐。
        /// </summary>
        private void LateUpdate()
        {
            if (virtualCamera == null || target == null) { return; }

            if (_isSmoothingSnap)
            {
                var z = virtualCamera.transform.position.z;
                var goal = new Vector3(target.position.x, target.position.y, z) + followSnapOffset;
                var dt = useUnscaledTimeForHandSnap ? Time.unscaledDeltaTime : Time.deltaTime;

                virtualCamera.transform.position = Vector3.SmoothDamp(
                    virtualCamera.transform.position,
                    goal,
                    ref _snapDampVelocity,
                    Mathf.Max(0.0001f, smoothTime),
                    Mathf.Infinity,
                    dt
                );
                // E2：Part3 同步到同一世界位，避免手推结束后 Live 切过去时跳变
                SyncPart3TransformToStreet();

                var toGoal2 = (virtualCamera.transform.position - goal).sqrMagnitude;
                var reached = toGoal2 < 0.01f;
                var timeout = maxHandSnapRealSeconds > 0.001f
                    && Time.unscaledTime - _handSnapStartUnscaledTime >= maxHandSnapRealSeconds;
                if (reached || timeout)
                {
                    if (timeout && !reached)
                    {
                        virtualCamera.transform.position = goal;
                        SyncPart3TransformToStreet();
                    }

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

        private void SyncPart3TransformToStreet()
        {
            if (virtualCameraPart3 == null || virtualCamera == null)
            {
                return;
            }

            virtualCameraPart3.transform.position = virtualCamera.transform.position;
            virtualCameraPart3.transform.rotation = virtualCamera.transform.rotation;
        }

        private void ApplyFollowWithCinemachineStateAligned(Transform followTarget, float updateDeltaTime)
        {
            if (virtualCamera == null || followTarget == null) { return; }

            ApplyFollowToVcam(virtualCamera, followTarget, updateDeltaTime, forceStateAlign: true);
            // Part3：绑 Follow 并 Invalidate，不强制再推一遍（避免与 Street 抢最终位）
            if (virtualCameraPart3 != null)
            {
                virtualCameraPart3.Follow = followTarget;
                virtualCameraPart3.PreviousStateIsValid = false;
            }
        }

        private static void ApplyFollowToVcam(
            CinemachineVirtualCamera vcam,
            Transform followTarget,
            float updateDeltaTime,
            bool forceStateAlign)
        {
            if (vcam == null || followTarget == null)
            {
                return;
            }

            vcam.Follow = followTarget;
            if (!forceStateAlign)
            {
                return;
            }

            vcam.PreviousStateIsValid = false;
            vcam.UpdateCameraState(Vector3.up, updateDeltaTime);
            var s = vcam.State;
            vcam.ForceCameraPosition(s.FinalPosition, s.FinalOrientation);
        }

        private void InitImpulseListener(CinemachineVirtualCamera vcam)
        {
            if (vcam == null)
            {
                return;
            }

            if (!vcam.TryGetComponent<CinemachineImpulseListener>(out var impulseListener))
            {
                impulseListener = vcam.gameObject.AddComponent<CinemachineImpulseListener>();
            }

            vcam.AddExtension(impulseListener);
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
                    // 手推期间两台都清 Follow，避免 CM 与手推抢位
                    SetFollowOnBoth(null);
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
                SetFollowOnBoth(newTarget);
            }
        }

        private void SetFollowOnBoth(Transform followTarget)
        {
            if (virtualCamera != null)
            {
                virtualCamera.Follow = followTarget;
            }

            if (virtualCameraPart3 != null)
            {
                virtualCameraPart3.Follow = followTarget;
            }
        }

        public void CancelFollow()
        {
            _isSmoothingSnap = false;
            _handSnapStartUnscaledTime = 0f;
            _snapDampVelocity = Vector3.zero;
            onComplete = null;
            // 开场锁机：必须清掉两台，否则 Part3 Standby 仍跟玩家、抢 Live
            SetFollowOnBoth(null);
        }

        public void ChangeVirtualCameraShowSize(float targetSize)
        {
            SetOrthoSizeOnBoth(targetSize);
        }

        public float GetVirtualCameraShowSize()
        {
            if (virtualCamera == null)
            {
                return 7.9f;
            }

            return virtualCamera.m_Lens.OrthographicSize;
        }

        public void ResetVirtualCameraShowSize()
        {
            SetOrthoSizeOnBoth(7.9f);
        }

        private void SetOrthoSizeOnBoth(float size)
        {
            if (virtualCamera != null)
            {
                var lens = virtualCamera.m_Lens;
                lens.OrthographicSize = size;
                virtualCamera.m_Lens = lens;
            }

            if (virtualCameraPart3 != null)
            {
                var lens = virtualCameraPart3.m_Lens;
                lens.OrthographicSize = size;
                virtualCameraPart3.m_Lens = lens;
            }
        }

        public void ChangeCameraBoundingArea(Collider2D newColliderArea)
        {
            SetConfinerOnBoth(newColliderArea);
        }

        private void SetConfinerOnBoth(Collider2D shape)
        {
            SetConfiner(virtualCamera, shape);
            SetConfiner(virtualCameraPart3, shape);
        }

        private static void SetConfiner(CinemachineVirtualCamera vcam, Collider2D shape)
        {
            if (vcam == null)
            {
                return;
            }

            var confiner = vcam.GetComponent<CinemachineConfiner>();
            if (confiner == null)
            {
                Debug.LogWarning($"{nameof(CameraComponent)} 「{vcam.name}」无 CinemachineConfiner，跳过 BoundingArea。");
                return;
            }

            confiner.m_BoundingShape2D = shape;
        }

        /// <summary>
        /// 将 Framing Transposer 整组替换为指定 Profile（旧单机路径 / 兼容保留）。
        /// Part3 Zone 主路径已改为切 Priority，不应再调用本方法切高台。
        /// </summary>
        public void ApplyFramingTransposerProfile(CinemachineFramingProfile profile)
        {
            ApplyFramingToVcam(virtualCamera, profile);
        }

        private static void ApplyFramingToVcam(CinemachineVirtualCamera vcam, CinemachineFramingProfile profile)
        {
            if (vcam == null)
            {
                return;
            }

            var framingTransposer = vcam.GetCinemachineComponent<CinemachineFramingTransposer>();
            if (framingTransposer == null)
            {
                Debug.LogWarning($"{nameof(CameraComponent)} 「{vcam.name}」未找到 FramingTransposer，跳过 Profile。");
                return;
            }

            framingTransposer.m_ScreenX = profile.screenX;
            framingTransposer.m_ScreenY = profile.screenY;
            framingTransposer.m_DeadZoneWidth = profile.deadZoneWidth;
            framingTransposer.m_DeadZoneHeight = profile.deadZoneHeight;
            framingTransposer.m_XDamping = profile.xDamping;
            framingTransposer.m_YDamping = profile.yDamping;
            framingTransposer.m_SoftZoneWidth = profile.softZoneWidth;
            framingTransposer.m_SoftZoneHeight = profile.softZoneHeight;
            framingTransposer.m_BiasX = profile.biasX;
            framingTransposer.m_BiasY = profile.biasY;
        }

        /// <summary>
        /// 进入 Part3：拉高 Part3 Priority；离开：Part3 回 Standby。
        /// 有双机时<strong>不</strong>改 Framing Body；无 Part3 引用时退回旧 Apply。
        /// </summary>
        public void SetKenMuNiPart3CameraMode(bool part3Active)
        {
            if (virtualCameraPart3 == null)
            {
                ApplyFramingTransposerProfile(
                    part3Active
                        ? CinemachineFramingProfile.KenMuNiPart3DepthFollow
                        : CinemachineFramingProfile.KenMuNiStreetDefault);
                return;
            }

            if (virtualCamera != null)
            {
                virtualCamera.Priority = streetPriority;
            }

            virtualCameraPart3.Priority = part3Active ? part3PriorityWhenActive : part3PriorityWhenStandby;
        }

        /// <summary>兼容旧 Zone 签名；有双机时忽略 Profile，只切 Priority。</summary>
        public void SetKenMuNiPart3CameraMode(
            bool part3Active,
            CinemachineFramingProfile part3Profile,
            CinemachineFramingProfile streetProfile)
        {
            if (virtualCameraPart3 != null)
            {
                SetKenMuNiPart3CameraMode(part3Active);
                return;
            }

            // 无双机兜底：仍走单机 Apply（其它场景）
            ApplyFramingTransposerProfile(part3Active ? part3Profile : streetProfile);
        }

        /// <summary>
        /// 村庄探索：切换 Framing Transposer 纵深（Y）跟拍强度（旧 API，保留兼容）。
        /// </summary>
        public void SetFramingTransposerDepthFollow(
            bool followDepthY,
            float yDamping = 0.7f,
            float deadZoneHeightWhenOff = 1f,
            float deadZoneHeightWhenOn = 0.5f,
            float screenYWhenOn = 0.25f)
        {
            if (followDepthY)
            {
                var profile = CinemachineFramingProfile.KenMuNiPart3DepthFollow;
                profile.yDamping = yDamping;
                profile.deadZoneHeight = deadZoneHeightWhenOn;
                profile.screenY = screenYWhenOn;
                ApplyFramingTransposerProfile(profile);
            }
            else
            {
                ApplyFramingTransposerProfile(CinemachineFramingProfile.KenMuNiStreetDefault);
            }
        }
    }
}
