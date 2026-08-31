using Game.GameMgr;
using Game.GameRuntime.Entities.Player;
using Game.GameRuntime.GameSceneManager.Base;
using UnityEngine;

namespace Game.GameRuntime.GameSceneManager.Component.CameraGSM
{
    /// <summary>
    /// 挂到 <c>Map/CameraDepthFollowZone_Part3</c>：
    /// <b>玩家位置</b>进入 Zone → Priority 切到 <c>VCam_Part3</c>；离开 → 回 <c>VCam_Street</c>。
    /// </summary>
    /// <remarks>
    /// <para>
    /// 产品再改口（0831）：保留双 VCam + Priority/Blend；废弃 A1「街道路白框完全 ⊆ 绿框」——
    /// 实测人已在高台时镜头框常有一边在盒外，导致长期切不上 Part3。
    /// </para>
    /// <para>
    /// 为何用玩家坐标而非再改 Framing：切机只动 Priority，Body 写死在 Inspector，
    /// 不会复活「改 ScreenY → 白框变 → 狂切」反馈环。
    /// </para>
    /// <para>
    /// 替代方案 P2：PlayerFoot Trigger Enter/Exit——亦可，但依赖 Layer/刚体；
    /// P1（Contains 玩家根坐标）对读档落点更友好，故采用。
    /// </para>
    /// </remarks>
    public class VillageCameraDepthFollowZone : MonoBehaviour
    {
        [Header("文档对照（双机后不再写入 Body；切机只走 Priority）")]
        [SerializeField]
        private CinemachineFramingProfile part3Profile = CinemachineFramingProfile.KenMuNiPart3DepthFollow;

        [SerializeField]
        private CinemachineFramingProfile streetProfile = CinemachineFramingProfile.KenMuNiStreetDefault;

        [Header("滞回（世界单位）")]
        [SerializeField]
        [Tooltip("进入用内缩盒、离开用外扩盒，减轻边界来回抖 Priority。建议 0.2～0.5。")]
        private float hysteresisWorldUnits = 0.35f;

        [Header("切换冷却")]
        [SerializeField]
        [Tooltip("对齐 Brain Blend（CustomBlends≈0.4s）。勿过长，否则进区后体感「半天不切」。")]
        private float modeSwitchCooldownSeconds = 0.4f;

        [Header("调试")]
        [SerializeField]
        private bool logStateTransitions;

        private Collider2D _zoneCollider;
        private bool _part3Active;
        private bool _warnedMissingPlayer;
        private float _nextAllowedSwitchUnscaledTime;

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
            if (_part3Active)
            {
                TrySetPart3CameraMode(false);
            }
        }

        /// <summary>
        /// LateUpdate：与 Brain 同相之后仍可用；读档落点在区内时不依赖 Trigger 是否补发 Enter。
        /// 重要原因：不再订阅 CameraUpdated 算白框，避免与「玩家进区」产品口混淆。
        /// </summary>
        private void LateUpdate()
        {
            if (_zoneCollider == null)
            {
                return;
            }

            bool wantPart3 = IsPlayerInsideZone();
            if (wantPart3 == _part3Active)
            {
                // 稳态：禁止每帧改 Priority / ApplyFraming
                return;
            }

            if (Time.unscaledTime < _nextAllowedSwitchUnscaledTime)
            {
                return;
            }

            if (!TrySetPart3CameraMode(wantPart3))
            {
                return;
            }

            _nextAllowedSwitchUnscaledTime = Time.unscaledTime + Mathf.Max(0f, modeSwitchCooldownSeconds);

            if (logStateTransitions)
            {
                Debug.Log(
                    $"[VillageCameraDepthFollowZone] part3Live={wantPart3} (玩家进区判定)",
                    this);
            }
        }

        /// <summary>
        /// P1：玩家根坐标是否在 Zone 内（带滞回）。
        /// 已激活：外扩盒（更松才离开）；未激活：内缩盒（更严才进入）。
        /// </summary>
        /// <remarks>
        /// 旁路说明：旧 A1 <c>IsStreetFrustumFullyInsideZone</c> 已废弃为主路径（产品否）。
        /// </remarks>
        private bool IsPlayerInsideZone()
        {
            var sceneMgr = GameManager.GetGameSceneManager() as BaseGameSceneManager;
            var entity = sceneMgr?.GetPlayerEntity();
            if (entity == null || !(entity.Logic is PlayerLogic player))
            {
                if (!_warnedMissingPlayer)
                {
                    _warnedMissingPlayer = true;
                    Debug.LogWarning(
                        $"[{nameof(VillageCameraDepthFollowZone)}] 无玩家实体，跳过 Part3 判定。",
                        this);
                }

                return false;
            }

            Vector3 playerPos = player.transform.position;
            Bounds zone = _zoneCollider.bounds;
            float h = Mathf.Max(0f, hysteresisWorldUnits);
            Bounds testZone = _part3Active ? InflateBounds(zone, +h) : InflateBounds(zone, -h);

            // 只判 XY：BoxCollider2D.bounds 的 Z 很薄，玩家 Z 略偏时 3D Contains 会假阴性
            return playerPos.x >= testZone.min.x && playerPos.x <= testZone.max.x
                   && playerPos.y >= testZone.min.y && playerPos.y <= testZone.max.y;
        }

        /// <summary>按世界单位扩展 extents（负值=内缩）。</summary>
        private static Bounds InflateBounds(Bounds source, float amount)
        {
            Vector3 extents = source.extents;
            extents.x = Mathf.Max(0.01f, extents.x + amount);
            extents.y = Mathf.Max(0.01f, extents.y + amount);
            return new Bounds(source.center, extents * 2f);
        }

        /// <summary>
        /// 切换 Part3/Street Live（Priority）。成功后才改 <see cref="_part3Active"/>。
        /// </summary>
        private bool TrySetPart3CameraMode(bool part3Active)
        {
            var cameraGsm = GameManager.GetGameSceneManager()?.GetModule<CameraComponentGSM>();
            if (cameraGsm == null)
            {
                return false;
            }

            if (part3Active)
            {
                var sceneMgr = GameManager.GetGameSceneManager() as BaseGameSceneManager;
                var entity = sceneMgr?.GetPlayerEntity();
                if (entity?.Logic is PlayerLogic player && cameraGsm.IsLock)
                {
                    cameraGsm.SetLock(false);
                    // SetFollow 须双写 Street+Part3（CameraComponent 内已实现）
                    cameraGsm.SetFollow(player.transform, null, false);
                }
            }

            // 双机：内部切 Priority；禁止恢复单机 ApplyFraming 主路径
            cameraGsm.SetKenMuNiPart3CameraMode(part3Active, part3Profile, streetProfile);
            _part3Active = part3Active;
            return true;
        }
    }
}
