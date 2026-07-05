using System.Collections.Generic;
using System.Reflection;
using Game.GameRuntime.Entities.Player;
using Game.GameRuntime.Entities.Player.Components;
using Game.Static.Enum;
using Game.Static.Name.Settings;
using UnityEngine;

namespace Game.GameRuntime.Entities.Component.Physics
{
    /// <summary>
    /// 村庄 DepthZone：在玩家侧集中管理「由触发体积覆盖的 Sorting Layer」。
    /// <para><b>职责</b>：缓存玩家子层级下全部 <see cref="SpriteRenderer"/>（策划约定：身体/头发/武器等统一切层）；
    /// 维护多个 <see cref="VillagePlayerDepthZone"/> 的叠层规则（优先级 + 稳定次序）；仅在 <see cref="PlayerLocomotionMode.Village2_5D"/> 下应用策划层，否则强制回到 <see cref="SortingLayerName.Player"/>。</para>
    /// <para><b>sortingOrder</b>：默认仍由 <see cref="TownPlayerLocomotion.ApplyDepthSortingFromWorldPosition"/> 按世界 Y 每帧写入。
    /// 若当前胜出的 <see cref="VillagePlayerDepthZone"/> 勾选了「锁定 Order」，则本组件在 <c>LateUpdate</c> 末尾再写一次固定 Order，
    /// 从而在<strong>不改移动脚本</strong>的前提下覆盖 Y 驱动（执行说明 §4：双写 Order 的合成策略）。</para>
    /// <para><b>挂载方式</b>：可由场景在玩家根手动挂接；若缺失，<see cref="VillagePlayerDepthZone"/> 会在首次合法触发时在 <see cref="PlayerLogic"/> 所在物体上 <c>AddComponent</c> 一次（避免改移动脚本与 prefab 手工合并冲突）。</para>
    /// </summary>
    [DisallowMultipleComponent]
    public class VillagePlayerDepthZoneListener : MonoBehaviour
    {
        [Header("调试")]
        [Tooltip("为 true 时仅在最终写入的 Sorting Layer 变化时打印 [VillageDepthZone]（执行说明 DZ-06）。")]
        [SerializeField]
        private bool debugLogOnLayerChange;

        /// <summary>可选：只切列出的 Renderer；留空则 Awake/初始化时收集玩家子层级全部 SpriteRenderer。</summary>
        [Tooltip("非空时仅对这些 SpriteRenderer 改 sortingLayerName；为空则 GetComponentsInChildren（含未激活）。")]
        [SerializeField]
        private List<SpriteRenderer> explicitSpriteRenderers = new List<SpriteRenderer>();

        [Header("Order 锁定目标（可选）")]
        [Tooltip(
            "当 DepthZone 要求「锁定 Order」时，优先只覆写此 Renderer（应与 TownPlayerLocomotion 上绑定的 spriteForDepthSort 为同一引用，避免头发/武器等多部位被压成同一 Order）。\n" +
            "留空则尝试反射读取 TownPlayerLocomotion 的私有字段；仍失败则退化为对 explicit / 全量子物体 SpriteRenderer 全部写入同一 Order。")]
        [SerializeField]
        private SpriteRenderer sortOrderOverridePrimaryRenderer;

        private PlayerLogic _playerLogic;
        private PlayerInputComponent _input;
        private SpriteRenderer[] _cachedRenderers = System.Array.Empty<SpriteRenderer>();

        /// <summary>Key = Zone 的 GetInstanceID()，避免同预制体多实例歧义。</summary>
        private readonly Dictionary<int, ZoneContribution> _zones = new Dictionary<int, ZoneContribution>();

        private string _lastAppliedLayer = string.Empty;
        private static int _playerFootLayerId = int.MinValue;

        /// <summary>反射解析到的 TownPlayerLocomotion.spriteForDepthSort；解析失败保持 null。</summary>
        private SpriteRenderer _cachedTownDepthSortSprite;

        /// <summary>与 <see cref="TownPlayerLocomotion"/> 中字段名一致，供反射读取；若重命名须同步改此常量。</summary>
        private const string TownDepthSortFieldName = "spriteForDepthSort";

        private struct ZoneContribution
        {
            public int Priority;
            public string LayerName;
            public bool LockSortingOrder;
            public int SortingOrder;
        }

        /// <summary>多区规则下当前帧应采用的 Layer + 可选 Order 锁定。</summary>
        private struct ResolvedZoneStyle
        {
            public string LayerName;
            public bool LockSortingOrder;
            public int SortingOrder;
        }

        /// <summary>
        /// 由 Zone 在触发回调中调用：保证 Listener 已绑定玩家并完成 Renderer 缓存。
        /// </summary>
        /// <param name="playerLogic">从脚点 Collider 向上解析到的玩家逻辑根。</param>
        public void EnsureInitialized(PlayerLogic playerLogic)
        {
            if (playerLogic == null)
            {
                return;
            }

            if (_playerLogic == playerLogic && _cachedRenderers != null && _cachedRenderers.Length > 0)
            {
                ResolveInputIfNeeded();
                // TownPlayerLocomotion 可能在首帧之后才挂上；重复进入时刷新反射缓存，避免 Order 锁定退化为「全 Renderer 写同一 Order」。
                RefreshTownDepthSortSpriteCache();
                return;
            }

            _playerLogic = playerLogic;
            ResolveInputIfNeeded();
            RebuildRendererCache();
            RefreshTownDepthSortSpriteCache();
        }

        private void ResolveInputIfNeeded()
        {
            if (_playerLogic == null)
            {
                return;
            }

            _input = _playerLogic.componentSystem != null
                ? _playerLogic.componentSystem.GetComponent<PlayerInputComponent>()
                : null;
        }

        private void RebuildRendererCache()
        {
            if (_playerLogic == null)
            {
                _cachedRenderers = System.Array.Empty<SpriteRenderer>();
                return;
            }

            if (explicitSpriteRenderers != null && explicitSpriteRenderers.Count > 0)
            {
                var list = new List<SpriteRenderer>();
                for (int i = 0; i < explicitSpriteRenderers.Count; i++)
                {
                    var r = explicitSpriteRenderers[i];
                    if (r != null)
                    {
                        list.Add(r);
                    }
                }

                _cachedRenderers = list.ToArray();
                return;
            }

            // 与任务卡澄清一致：子层级全部 SpriteRenderer 统一改层（含未激活，避免换装后引用遗漏）。
            _cachedRenderers = _playerLogic.GetComponentsInChildren<SpriteRenderer>(true);
        }

        /// <summary>
        /// 在不修改 <see cref="TownPlayerLocomotion"/> 源码的前提下，尝试拿到其用于 Y 排序的 SpriteRenderer，供 Order 锁定时单点覆写。
        /// </summary>
        private void RefreshTownDepthSortSpriteCache()
        {
            _cachedTownDepthSortSprite = TryGetTownPlayerLocomotionDepthSpriteViaReflection(_playerLogic);
        }

        /// <summary>
        /// 反射读取序列化私有字段；IL2CPP 下通常仍可用。若字段改名返回 null，策划应在 Listener 上手动指定 sortOrderOverridePrimaryRenderer。
        /// </summary>
        private static SpriteRenderer TryGetTownPlayerLocomotionDepthSpriteViaReflection(PlayerLogic playerLogic)
        {
            if (playerLogic?.componentSystem == null)
            {
                return null;
            }

            var town = playerLogic.componentSystem.TryGetComponent<TownPlayerLocomotion>();
            if (town == null)
            {
                return null;
            }

            try
            {
                FieldInfo fi = typeof(TownPlayerLocomotion).GetField(
                    TownDepthSortFieldName,
                    BindingFlags.Instance | BindingFlags.NonPublic);
                return fi?.GetValue(town) as SpriteRenderer;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 进入 DepthZone 时由 <see cref="VillagePlayerDepthZone"/> 注册。
        /// </summary>
        /// <param name="lockSortingOrder">为 true 时本区胜出期间由 Listener 固定 sortingOrder（见 Zone 上说明）。</param>
        /// <param name="sortingOrder">与 lockSortingOrder 配套的 Order 数值。</param>
        internal void RegisterZone(
            VillagePlayerDepthZone zone,
            int priority,
            string sortingLayerName,
            bool lockSortingOrder,
            int sortingOrder)
        {
            if (zone == null || string.IsNullOrEmpty(sortingLayerName))
            {
                return;
            }

            int id = zone.GetInstanceID();
            _zones[id] = new ZoneContribution
            {
                Priority = priority,
                LayerName = sortingLayerName,
                LockSortingOrder = lockSortingOrder,
                SortingOrder = sortingOrder
            };
            ApplyResolvedLayer();
        }

        /// <summary>
        /// 离开或禁用时注销。
        /// </summary>
        internal void UnregisterZone(VillagePlayerDepthZone zone)
        {
            if (zone == null)
            {
                return;
            }

            _zones.Remove(zone.GetInstanceID());
            ApplyResolvedLayer();
        }

        private void LateUpdate()
        {
            // 非村庄模式须每帧覆盖表现（避免战斗/其它场景误用策划层）；不在 Enter/Exit 里依赖「出村事件」。
            if (_playerLogic != null && _cachedRenderers.Length > 0)
            {
                ApplyResolvedLayer();
                // TownPlayerLocomotion 在 Update 里写 Order；本处在 LateUpdate 末尾再写，满足「区内 Order 由 Zone 固定」且不修改移动脚本。
                ApplySortingOrderOverrideAfterLocomotion();
            }
        }

        private void OnDisable()
        {
            // 被禁用时清空叠层状态，避免再启用时残留无效 ZoneId。
            _zones.Clear();
            ForceLayerOnAllRenderers(SortingLayerName.Player, log: false);
            _lastAppliedLayer = string.Empty;
        }

        private void ApplyResolvedLayer()
        {
            if (_cachedRenderers == null || _cachedRenderers.Length == 0)
            {
                return;
            }

            ResolveInputIfNeeded();
            bool village = _input != null && _input.LocomotionMode == PlayerLocomotionMode.Village2_5D;
            string target = village ? ResolveBestZoneStyle().LayerName : SortingLayerName.Player;

            if (target == _lastAppliedLayer)
            {
                return;
            }

            ForceLayerOnAllRenderers(target, log: debugLogOnLayerChange);
            _lastAppliedLayer = target;
        }

        /// <summary>
        /// 与文档一致：取最高 Priority；同 Priority 时取更大 ZoneInstanceId（稳定次序）。
        /// </summary>
        private ResolvedZoneStyle ResolveBestZoneStyle()
        {
            if (_zones.Count == 0)
            {
                return new ResolvedZoneStyle
                {
                    LayerName = SortingLayerName.Player,
                    LockSortingOrder = false,
                    SortingOrder = 0
                };
            }

            int bestPriority = int.MinValue;
            int bestZoneId = int.MinValue;
            ResolvedZoneStyle best = default;
            best.LayerName = SortingLayerName.Player;
            foreach (var kv in _zones)
            {
                var z = kv.Value;
                if (z.Priority > bestPriority || (z.Priority == bestPriority && kv.Key > bestZoneId))
                {
                    bestPriority = z.Priority;
                    bestZoneId = kv.Key;
                    best.LayerName = z.LayerName;
                    best.LockSortingOrder = z.LockSortingOrder;
                    best.SortingOrder = z.SortingOrder;
                }
            }

            return best;
        }

        /// <summary>
        /// 当胜出 Zone 要求锁定 Order 时，在 Locomotion 的 Y 排序之后强制写回；否则不碰 sortingOrder，保持纯 Y 驱动。
        /// </summary>
        private void ApplySortingOrderOverrideAfterLocomotion()
        {
            ResolveInputIfNeeded();
            if (_input == null || _input.LocomotionMode != PlayerLocomotionMode.Village2_5D)
            {
                return;
            }

            ResolvedZoneStyle style = ResolveBestZoneStyle();
            if (!style.LockSortingOrder)
            {
                return;
            }

            // 优先手动绑定（无反射、无 IL2CPP 字段名风险）；否则用进村时缓存的反射结果。
            SpriteRenderer primary = sortOrderOverridePrimaryRenderer != null
                ? sortOrderOverridePrimaryRenderer
                : _cachedTownDepthSortSprite;
            if (primary != null)
            {
                primary.sortingOrder = style.SortingOrder;
                return;
            }

            // 退化：多部位共用同一 Order，可能与换装子部件的前后关系冲突；验收时若发现头发错位，请改用 sortOrderOverridePrimaryRenderer。
            for (int i = 0; i < _cachedRenderers.Length; i++)
            {
                var r = _cachedRenderers[i];
                if (r != null)
                {
                    r.sortingOrder = style.SortingOrder;
                }
            }
        }

        private void ForceLayerOnAllRenderers(string layerName, bool log)
        {
            for (int i = 0; i < _cachedRenderers.Length; i++)
            {
                var r = _cachedRenderers[i];
                if (r == null)
                {
                    continue;
                }

                if (r.sortingLayerName != layerName)
                {
                    r.sortingLayerName = layerName;
                }
            }

            if (log)
            {
                Debug.Log($"[VillageDepthZone] 玩家 SortingLayer → {layerName}（activeZones={_zones.Count} village={_input != null && _input.LocomotionMode == PlayerLocomotionMode.Village2_5D}）", this);
            }
        }

        /// <summary>
        /// 供 Zone 判断「other 是否为脚点层」时复用，避免重复 NameToLayer。
        /// </summary>
        internal static bool IsPlayerFootLayer(int layer)
        {
            if (_playerFootLayerId == int.MinValue)
            {
                _playerFootLayerId = LayerMask.NameToLayer(LayerName.PlayerFoot);
            }

            return _playerFootLayerId >= 0 && layer == _playerFootLayerId;
        }
    }
}
