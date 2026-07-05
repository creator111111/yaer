using Game.GameRuntime.Entities.Player;
using UnityEngine;

namespace Game.GameRuntime.Component
{
    /// <summary>
    /// 以<strong>玩家根节点</strong>为目标的 offset 世界空间跟随，在 <see cref="FixedUpdate"/> 中更新（与 <see cref="UnityEngine.Rigidbody2D"/> 物理步一致）。
    /// 使用 <see cref="Vector3.SmoothDamp"/> 平滑位置。架构上本脚本应挂在<strong>主摄像机</strong>（或独立机位空物体，不再挂在旧的 CameraTrack 下让 Track 来“带飞”机位）；
    /// 若曾把摄像机放在 Player/CameraTrack 下，会启用解绑，避免 <see cref="detachFromParentInAwake"/> 所述残留旋转/位移动画影响。
    /// </summary>
    /// <remarks>
    /// 与旧架构区别：过去由 CameraTrack 子物体位移带动 Cinemachine/相机；现改为<strong>机位在代码里只跟玩家</strong>+offset，不依赖 CameraTrack 的动画 Transform。
    /// 若项目已用 Cinemachine，本脚本与 VirtualCamera 的 Follow 会冲突，请二选一并统一由一种方式控制 Transform。
    /// </remarks>
    [DisallowMultipleComponent]
    [AddComponentMenu("Game/Player Offset Camera Follow")]
    public class PlayerOffsetCameraFollow : MonoBehaviour
    {
        [Header("跟拍目标（建议：玩家根 Player，不是 CameraTrack）")]
        [Tooltip("要跟随的 Transform，必须指向玩家等角色根。若为空，Awake 时通过 Player Tag 或 PlayerLogic 解析。")]
        [SerializeField] private Transform target;

        [Header("机位：相对目标的世界空间偏移，例如 (0,5,-10)")]
        [SerializeField] private Vector3 offset = new Vector3(0f, 5f, -10f);

        [Header("与旧 CameraTrack 父级解绑（推荐开启）")]
        [Tooltip("Awake 时从父物体下解绑，保持世界位姿不变。避免挂在 Player/CameraTrack 下时，Track 的旋转/动画层仍叠加到本物体上。")]
        [SerializeField] private bool detachFromParentInAwake = true;

        [Header("平滑（位置）")]
        [Tooltip("值越大，SmoothDamp 的 smoothTime 越短。内部用 1/followSpeed 作为 smoothTime。")]
        [SerializeField] private float followSpeed = 4f;

        [Header("朝向（每帧用世界空间覆盖，不继承父级/Track 的 Rotation）")]
        [Tooltip("为 true 时在每个 FixedUpdate 中设置本物体的世界 rotation，使摄像机朝向「玩家身上的瞄点」。")]
        [SerializeField] private bool overrideWorldRotation = true;
        [Tooltip("瞄点 = target.position + 此偏移（如稍抬高到胸口/头）。")]
        [SerializeField] private Vector3 lookAtTargetOffset = new Vector3(0f, 1.5f, 0f);
        [Tooltip("身体扭转时的朝向平滑。越大转向越快。仅当 overrideWorldRotation 为真时有效。")]
        [SerializeField] private float rotationFollowSpeed = 8f;
        [Tooltip("LookRotation 时使用的上方向，一般为世界 up。")]
        [SerializeField] private Vector3 worldUp = new Vector3(0f, 1f, 0f);

        [Header("调试：机位对没对齐")]
        [Tooltip("在 FixedUpdate 中每物理步输出 target 与 newPosition；调好后请关掉，避免 Console 刷屏。")]
        [SerializeField] private bool debugLogFollowInFixedUpdate = true;

        /// <summary>SmoothDamp 所需的 per-axis 速度状态，由 Unity 维护，不要每帧清零。</summary>
        private Vector3 _smoothVelocity;

        private void Awake()
        {
            // 先解绑，再解析玩家；避免仍挂在 CameraTrack 下时父级先影响本物体
            if (detachFromParentInAwake && transform.parent != null)
            {
                // worldPositionStays=true：解绑后世界坐标不变，本脚本随后用世界空间完全接管机位/朝向
                transform.SetParent(null, true);
            }

            TryResolveTarget();
        }

        private void OnEnable()
        {
            // 切场景/还原引用后若 target 仍为空，再试一次
            if (target == null)
            {
                TryResolveTarget();
            }
        }

        private void FixedUpdate()
        {
            if (target == null)
            {
                // 运行中玩家晚于相机生成时，可再试一次
                TryResolveTarget();
                if (target == null) { return; }
            }

            var fd = Time.fixedDeltaTime;
            // 目标世界位置：与玩家同世界轴的固定偏移。不再使用 CameraTrack 的局部变换。
            var desiredPosition = target.position + offset;

            // SmoothDamp 的 delta 必须用 fixedDeltaTime，与 Rigidbody2D 同相位
            var smoothTime = 1f / Mathf.Max(0.0001f, followSpeed);
            transform.position = Vector3.SmoothDamp(
                transform.position,
                desiredPosition,
                ref _smoothVelocity,
                smoothTime,
                Mathf.Infinity,
                fd);

            if (debugLogFollowInFixedUpdate)
            {
                Debug.Log(
                    $"[PlayerOffsetCameraFollow] target 名称: \"{target.name}\" | target.position: {target.position} | " +
                    $"newPosition(=target+offset 期望机位): {desiredPosition} | " +
                    $"本物理步 transform.position(平滑后): {transform.position} | " +
                    $"offset: {offset}",
                    this);
            }

            if (overrideWorldRotation)
            {
                var lookAtPoint = target.position + lookAtTargetOffset;
                var toTarget = lookAtPoint - transform.position;
                if (toTarget.sqrMagnitude > 1e-6f)
                {
                    var desiredRot = Quaternion.LookRotation(toTarget.normalized, worldUp);
                    // 与 position 一致使用 fixedDeltaTime，避免朝向与位移不同步
                    var t = 1f - Mathf.Exp(-rotationFollowSpeed * fd);
                    transform.rotation = Quaternion.Slerp(transform.rotation, desiredRot, t);
                }
            }
        }

        /// <summary>
        /// 1) Tag 为 Player 的物体；2) 场景中的 <see cref="PlayerLogic"/>。优先使用 Inspector 已填的 <see cref="target"/>。
        /// </summary>
        private void TryResolveTarget()
        {
            if (target != null) { return; }

            var byTag = GameObject.FindGameObjectWithTag("Player");
            if (byTag != null)
            {
                target = byTag.transform;
                return;
            }

            var playerLogic = Object.FindObjectOfType<PlayerLogic>();
            if (playerLogic != null)
            {
                target = playerLogic.transform;
            }
        }

        /// <summary> 运行时从外部把目标设回玩家等（供剧情/过场用）。 </summary>
        public void SetTarget(Transform newTarget, bool clearSmoothState = true)
        {
            target = newTarget;
            if (clearSmoothState)
            {
                _smoothVelocity = Vector3.zero;
            }
        }
    }
}
