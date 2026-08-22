using Game.GameMgr;
using Game.GameRuntime.Entities.Player;
using Game.GameRuntime.GameSceneManager.Base;
using UnityEngine;

namespace Game.GameRuntime.GameSceneManager.Component.CameraGSM
{
    /// <summary>
    /// 挂到 <c>Map/CameraDepthFollowZone_Part3</c>：玩家进入 BoxCollider 后，将 Virtual Camera 的 Framing Transposer
    /// 切换为 <see cref="part3Profile"/>；离开后恢复 <see cref="streetProfile"/>。
    /// </summary>
    /// <remarks>
    /// 参数记录在 Inspector 的 Profile 字段（与你在 Cinemachine 上调的一致），运行时由
    /// <see cref="CameraComponent.ApplyFramingTransposerProfile"/> 写入。
    /// 替代方案：直接改场景里 VCam YAML——无法按 Zone 分区，右街会一起变。
    /// </remarks>
    public class VillageCameraDepthFollowZone : MonoBehaviour
    {
        [Header("进入 Zone 后套用的 Framing Transposer 参数")]
        [SerializeField]
        private CinemachineFramingProfile part3Profile = CinemachineFramingProfile.KenMuNiPart3DepthFollow;

        [Header("离开 Zone 后恢复的右街默认参数")]
        [SerializeField]
        private CinemachineFramingProfile streetProfile = CinemachineFramingProfile.KenMuNiStreetDefault;

        [SerializeField]
        [Tooltip("脚底碰撞体名字，与 Player.prefab 中 PlayerFoot 一致")]
        private string playerFootObjectName = "PlayerFoot";

        [Header("调试")]
        [SerializeField]
        private bool logStateTransitions;

        private Collider2D _zoneCollider;
        private int _overlapCount;
        private bool _part3Active;

        private void Awake()
        {
            _zoneCollider = GetComponent<Collider2D>();
            if (_zoneCollider == null)
            {
                Debug.LogError($"[{nameof(VillageCameraDepthFollowZone)}] 「{name}」需要 Collider2D。", this);
            }
        }

        private void OnDisable()
        {
            _overlapCount = 0;
            if (_part3Active)
            {
                SetPart3CameraMode(false);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            TryChangeOverlap(other, +1);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            TryChangeOverlap(other, -1);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision?.collider != null)
            {
                TryChangeOverlap(collision.collider, +1);
            }
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            if (collision?.collider != null)
            {
                TryChangeOverlap(collision.collider, -1);
            }
        }

        /// <summary>LateUpdate 用 bounds 补检：读档落点在区内时 OnTriggerEnter 可能不触发。</summary>
        private void LateUpdate()
        {
            if (_zoneCollider == null)
            {
                return;
            }

            var sceneMgr = GameManager.GetGameSceneManager() as BaseGameSceneManager;
            var entity = sceneMgr?.GetPlayerEntity();
            if (entity == null || !(entity.Logic is PlayerLogic player))
            {
                return;
            }

            bool inside = _zoneCollider.bounds.Contains(player.transform.position);
            if (inside == _part3Active)
            {
                return;
            }

            SetPart3CameraMode(inside);

            if (logStateTransitions)
            {
                Debug.Log($"[VillageCameraDepthFollowZone] part3={inside} pos={player.transform.position}", this);
            }
        }

        private void TryChangeOverlap(Collider2D other, int delta)
        {
            if (other == null || other.gameObject.name != playerFootObjectName)
            {
                return;
            }

            int prev = _overlapCount;
            _overlapCount = Mathf.Max(0, _overlapCount + delta);

            if (prev == 0 && _overlapCount > 0)
            {
                SetPart3CameraMode(true);
            }
            else if (prev > 0 && _overlapCount == 0)
            {
                SetPart3CameraMode(false);
            }
        }

        private void SetPart3CameraMode(bool part3Active)
        {
            _part3Active = part3Active;

            var cameraGsm = GameManager.GetGameSceneManager()?.GetModule<CameraComponentGSM>();
            if (cameraGsm == null)
            {
                return;
            }

            if (part3Active)
            {
                var sceneMgr = GameManager.GetGameSceneManager() as BaseGameSceneManager;
                var entity = sceneMgr?.GetPlayerEntity();
                if (entity?.Logic is PlayerLogic player && cameraGsm.IsLock)
                {
                    cameraGsm.SetLock(false);
                    cameraGsm.SetFollow(player.transform, null, false);
                }
            }

            cameraGsm.SetKenMuNiPart3CameraMode(part3Active, part3Profile, streetProfile);
        }
    }
}
