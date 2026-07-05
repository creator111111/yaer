using System.Collections.Generic;
using Game.GameRuntime.Entities.Player;
using Game.GameRuntime.Entities.Player.Components;
using Game.Static.Enum;
using Game.Static.Name.Settings;
using UnityEngine;

namespace Game.GameRuntime.Entities.Component.Physics
{
    /// <summary>
    /// 村庄探索（<see cref="PlayerLocomotionMode.Village2_5D"/>）下，按玩家与锚点的<strong>世界 Y</strong> 比较结果，
    /// 在 <see cref="SortingLayerName.Default"/> 与 <see cref="SortingLayerName.SceneObject"/> 之间切换目标 <see cref="SpriteRenderer"/>，
    /// 实现 DNF 式「玩家在物体后则被挡、在前则盖住」的遮挡观感。
    /// <para><b>与 <see cref="DepthComponent"/> 的关系</b>：<see cref="DepthComponent"/> 每帧改 <c>sortingOrder</c> 且不改 Sorting Layer；
    /// 若同物体启用两者会「双写」打架。推荐本物体<b>不挂</b> <see cref="DepthComponent"/> 或将其禁用，由本脚本统一管理（执行说明 §4.3）。</para>
    /// <para><b>替代方案</b>：若必须保留 <see cref="DepthComponent"/>，可扩展其「外部托管」开关（共用组件，回归面更大）。</para>
    /// </summary>
    [DisallowMultipleComponent]
    public class VillageSceneObjectDepthSort : MonoBehaviour
    {
        [Header("渲染目标")]
        [Tooltip("参与切换 Sorting Layer / Order 的 SpriteRenderer；留空则在 Awake 时取本物体上单个 SpriteRenderer（无则报错日志）。")]
        [SerializeField]
        private List<SpriteRenderer> targetSpriteRenderers = new List<SpriteRenderer>();

        [Header("纵深锚点（世界 Y）")]
        [Tooltip("用于与玩家比较前后关系的锚点；留空则用本物体 Transform.position.y。\n回退顺序：anchorOverride → 本 transform →（若存在）DepthComponent 的 FootCld.bounds.center.y（与执行说明 OC-04 一致）。")]
        [SerializeField]
        private Transform anchorOverride;

        [Header("玩家引用（禁止每帧 Find）")]
        [Tooltip("优先使用：直接拖入场景中的 Player 根物体上的 PlayerLogic。留空则在 OnEnable 内用 Tag「Player」查找一次并缓存。")]
        [SerializeField]
        private PlayerLogic playerLogicOverride;

        [Header("比较方向")]
        [Tooltip("勾选后反转「玩家在前/后」与 Default/SceneObject 的对应关系；若实机与美术约定相反，优先改此项而非改核心移动。")]
        [SerializeField]
        private bool invertPlayerVersusAnchorComparison;

        [Header("Sorting Order（每层可单独配，避免与全局 Y 排序打架）")]
        [Tooltip("切到 Default（玩家相对更靠前、物体应让开）时写入的 sortingOrder。")]
        [SerializeField]
        private int sortingOrderWhenDefaultLayer = 0;

        [Tooltip("切到 SceneObject（物体应挡住玩家）时写入的 sortingOrder。")]
        [SerializeField]
        private int sortingOrderWhenSceneObjectLayer = 0;

        [Header("性能 / 调试")]
        [Tooltip("每 N 帧才刷新一次（1=每帧）。物体极多时可改为 2～3 降低 CPU，可能带来半帧级排序滞后。")]
        [SerializeField]
        [Min(1)]
        private int updateEveryNthFrame = 1;

        [Tooltip("开启后仅在层状态变化时打印 [VillageOcclusion]，用于验收 OC-01～03。")]
        [SerializeField]
        private bool debugLogOnLayerChange;

        /// <summary>玩家用于比较的 Y：选用 <b>Rigidbody2D.position.y</b>（与 <see cref="TownPlayerLocomotion"/> 写回刚体的权威纵深一致）。\n
        /// <b>未采用 spriteForDepthSort.worldY 的原因</b>：精灵 pivot 与「脚底 / 逻辑站位」可能不一致；权威位移仍以刚体为准（与执行说明 §4.2、OPEN_QUESTIONS 可后续改为可选模式）。</summary>
        [Tooltip("为 true 时优先使用 TownPlayerLocomotion.DebugAuthoritativeWorldY（与村庄权威 Y 标量一致）；false 时仅用 Rigidbody2D.position.y。")]
        [SerializeField]
        private bool preferTownLocomotionAuthoritativeY = true;

        // ---------------------------------------------------------------------
        // 运行时缓存：避免 Update 内 Find；离村时恢复进场景时的 Layer/Order
        // ---------------------------------------------------------------------

        private PlayerLogic _cachedPlayerLogic;
        private TownPlayerLocomotion _cachedTownLoco;
        private Rigidbody2D _cachedPlayerRb2D;
        private DepthComponent _cachedDepthComponent;

        private readonly List<(SpriteRenderer r, string layer, int order)> _initialSpriteState = new List<(SpriteRenderer, string, int)>();
        private bool _initialStateCaptured;
        private string _lastAppliedLayerKey = string.Empty;
        private int _frameCounter;

        /// <summary>上一帧是否为村庄模式；用于仅在「出村」边沿恢复初始 Sorting，避免非村庄场景每帧强行 Restore 与其它系统抢写。</summary>
        private bool _wasVillageLastFrame;

        private void Awake()
        {
            _cachedDepthComponent = GetComponent<DepthComponent>();
            if (_cachedDepthComponent != null && _cachedDepthComponent.enabled)
            {
                Debug.LogWarning(
                    $"[VillageOcclusion] 「{name}」同时启用了 DepthComponent 与 VillageSceneObjectDepthSort，可能互相覆盖 sortingOrder。建议禁用 DepthComponent 或移除此脚本其一。",
                    this);
            }

            if (targetSpriteRenderers == null || targetSpriteRenderers.Count == 0)
            {
                var one = GetComponent<SpriteRenderer>();
                if (one != null)
                {
                    targetSpriteRenderers = new List<SpriteRenderer> { one };
                }
                else
                {
                    Debug.LogError($"[VillageOcclusion] 「{name}」未配置 targetSpriteRenderers 且本物体无 SpriteRenderer。", this);
                }
            }
        }

        private void OnEnable()
        {
            ResolvePlayerReferenceIfNeeded();
            CaptureInitialRendererStateIfNeeded();
        }

        private void LateUpdate()
        {
            if (targetSpriteRenderers == null || targetSpriteRenderers.Count == 0)
            {
                return;
            }

            _frameCounter++;
            if (_frameCounter % updateEveryNthFrame != 0)
            {
                return;
            }

            if (_cachedPlayerLogic == null)
            {
                ResolvePlayerReferenceIfNeeded();
                if (_cachedPlayerLogic == null)
                {
                    return;
                }
            }

            var input = _cachedPlayerLogic.componentSystem != null
                ? _cachedPlayerLogic.componentSystem.GetComponent<PlayerInputComponent>()
                : null;

            bool village = input != null && input.LocomotionMode == PlayerLocomotionMode.Village2_5D;
            if (!village)
            {
                if (_wasVillageLastFrame)
                {
                    RestoreInitialSortingState();
                }

                _wasVillageLastFrame = false;
                _lastAppliedLayerKey = string.Empty;
                return;
            }

            _wasVillageLastFrame = true;

            RefreshPlayerComponentCache();
            float playerY = ResolvePlayerComparisonWorldY();
            float anchorY = ResolveAnchorWorldY();

            // 约定：playerY > anchorY → 玩家在锚点「更深处」一侧，物体应挡住玩家 → SceneObject；反之 → Default。
            // 若与美术场景坐标直觉相反，使用 invertPlayerVersusAnchorComparison。
            bool playerDeeperThanAnchor = playerY > anchorY;
            if (invertPlayerVersusAnchorComparison)
            {
                playerDeeperThanAnchor = !playerDeeperThanAnchor;
            }

            string targetLayer = playerDeeperThanAnchor ? SortingLayerName.SceneObject : SortingLayerName.Default;
            int targetOrder = playerDeeperThanAnchor ? sortingOrderWhenSceneObjectLayer : sortingOrderWhenDefaultLayer;

            string stateKey = targetLayer + ":" + targetOrder;
            if (stateKey != _lastAppliedLayerKey)
            {
                ApplyToAllRenderers(targetLayer, targetOrder);
                _lastAppliedLayerKey = stateKey;
                if (debugLogOnLayerChange)
                {
                    Debug.Log(
                        $"[VillageOcclusion] obj={name} playerY={playerY:F3} anchorY={anchorY:F3} → layer={targetLayer} order={targetOrder}",
                        this);
                }
            }
        }

        private void OnDisable()
        {
            RestoreInitialSortingState();
            _lastAppliedLayerKey = string.Empty;
        }

        // ---------------------------------------------------------------------
        // 内部方法
        // ---------------------------------------------------------------------

        /// <summary>在 OnEnable 时抓取当前 Layer/Order，离村或禁用时还原，避免战斗场景/UI 场景被误改遗留。</summary>
        private void CaptureInitialRendererStateIfNeeded()
        {
            if (_initialStateCaptured)
            {
                return;
            }

            _initialSpriteState.Clear();
            foreach (SpriteRenderer r in targetSpriteRenderers)
            {
                if (r == null)
                {
                    continue;
                }

                _initialSpriteState.Add((r, r.sortingLayerName, r.sortingOrder));
            }

            _initialStateCaptured = true;
        }

        private void RestoreInitialSortingState()
        {
            foreach ((SpriteRenderer r, string layer, int order) t in _initialSpriteState)
            {
                if (t.r == null)
                {
                    continue;
                }

                t.r.sortingLayerName = t.layer;
                t.r.sortingOrder = t.order;
            }
        }

        private void ResolvePlayerReferenceIfNeeded()
        {
            if (playerLogicOverride != null)
            {
                _cachedPlayerLogic = playerLogicOverride;
                return;
            }

            if (_cachedPlayerLogic != null)
            {
                return;
            }

            GameObject tagged = GameObject.FindGameObjectWithTag("Player");
            if (tagged == null)
            {
                return;
            }

            _cachedPlayerLogic = tagged.GetComponent<PlayerLogic>();
        }

        private void RefreshPlayerComponentCache()
        {
            if (_cachedPlayerLogic == null)
            {
                return;
            }

            if (_cachedTownLoco == null)
            {
                _cachedTownLoco = _cachedPlayerLogic.GetComponent<TownPlayerLocomotion>();
            }

            if (_cachedPlayerRb2D == null)
            {
                _cachedPlayerRb2D = _cachedPlayerLogic.GetComponent<Rigidbody2D>();
            }
        }

        /// <summary>与村庄纵深一致：世界 Y；不把 Z 当比较量（验收 OC-01）。</summary>
        private float ResolvePlayerComparisonWorldY()
        {
            // 优先权威 Y：与 TownPlayerLocomotion 内部 _villageWorldY 同源；仅在本组件 enabled 时与刚体写回一致。
            if (preferTownLocomotionAuthoritativeY && _cachedTownLoco != null && _cachedTownLoco.enabled)
            {
                return _cachedTownLoco.DebugAuthoritativeWorldY;
            }

            if (_cachedPlayerRb2D != null)
            {
                return _cachedPlayerRb2D.position.y;
            }

            return _cachedPlayerLogic.transform.position.y;
        }

        private float ResolveAnchorWorldY()
        {
            if (anchorOverride != null)
            {
                return anchorOverride.position.y;
            }

            if (_cachedDepthComponent != null && _cachedDepthComponent.FootCld != null)
            {
                return _cachedDepthComponent.FootCld.bounds.center.y;
            }

            return transform.position.y;
        }

        private void ApplyToAllRenderers(string layerName, int order)
        {
            foreach (SpriteRenderer r in targetSpriteRenderers)
            {
                if (r == null)
                {
                    continue;
                }

                r.sortingLayerName = layerName;
                r.sortingOrder = order;
            }
        }
    }
}
