using System.Collections.Generic;
using Game.GameRuntime.Entities.Player;
using UnityEngine;

namespace Game.GameRuntime.Entities.Component.Physics
{
    /// <summary>
    /// 树屋「双 Trigger + 宽 PlayerFoot」门控（执行文档 §1.3 / §3.2）：对 <b>DepthZone&amp;Colliders</b> 根物体保持 <b>单一 SetActive 写口</b>。
    /// <para>
    /// <b>与旧版「离散两次 Enter 顺序」的差异</b>：宽脚点易同时压住 Trigger-0 与 Trigger-1，同帧 Enter 顺序不可靠；
    /// 本实现以 <b>曾出现「同时 01」</b>（<c>in0 &amp;&amp; in1</c>）为前提，在 <b>从双接触变为恰好单侧残留</b>（Exit 后 <c>xor</c>）时，
    /// 按 <b>先碰</b>（<see cref="FootSession.FirstTouched"/>）与 <b>收尾侧</b>（仅剩 0 或 1）查 §1.3.3 决策表并最多调用一次 <c>SetActive</c>。
    /// </para>
    /// <para>
    /// <b>为何在 Exit 的「双→单」时结算</b>：宽脚点在双区内不会稳定派发「先离谁」的 Enter 序列；在 <c>wasBoth &amp;&amp; xor</c> 时
    /// 「最后是 0/1」已由物理接触唯一确定，与策划表「收尾」列一致，且避免在 <c>Update</c> 里轮询 <c>OverlapCollider</c>（违背 MASTER「禁堆 Update 业务」）。
    /// </para>
    /// <para>
    /// <b>与 <see cref="VillagePlayerDepthZone"/> 的边界</b>：本类只门控场景父节点显隐；不改玩家 Sorting Layer。父物体关闭时子 Zone 仍走既有 <c>OnDisable</c> 注销。
    /// </para>
    /// <para>
    /// <b>策划挂接</b>：协调器应挂在 <b>始终 Active</b> 的父节点（如「树屋」空节点），子物体 Trigger 挂 <see cref="VillageTreehouseDepthGateTriggerForward"/> 并转发 Enter/Exit；
    /// <see cref="depthZoneAndCollidersRoot"/> 指向要开关的包（可与本组件不同物体）。可选填 <see cref="tieBreakTrigger0"/> / <see cref="tieBreakTrigger1"/> 作 §7 同帧双进几何 tie-break。
    /// </para>
    /// </summary>
    [DisallowMultipleComponent]
    public class VillageTreehouseDepthZoneGate : MonoBehaviour
    {
        /// <summary>未观测到单侧先入时的占位；结算前若仍为 None 且配置了 tie-break 碰撞体则改为几何判定。</summary>
        private enum FirstTouchId
        {
            None = -1,
            Trigger0 = 0,
            Trigger1 = 1,
        }

        /// <summary>每名合法玩家一根脚点会话（多玩家村庄时可扩展；单机时字典体量极小）。</summary>
        private sealed class FootSession
        {
            public bool In0;
            public bool In1;
            public FirstTouchId FirstTouched = FirstTouchId.None;
            public bool HadBoth;

            /// <summary>同一 <see cref="Time.fixedTime"/> 内收到的 Enter 次数，用于 §7 同帧双进时覆盖 first 的 tie-break。</summary>
            public float BatchFixedTime = -1f;

            public int BatchEnterCount;
        }

        [Header("目标（单一写口）")]
        [Tooltip("挂 DepthZone 与阻挡 Collider 的父物体；仅本组件对其 SetActive。")]
        [SerializeField]
        private GameObject depthZoneAndCollidersRoot;

        [Header("初始状态")]
        [Tooltip("场景加载后目标是否激活；在 Awake 写入一次 depthZoneAndCollidersRoot。")]
        [SerializeField]
        private bool initialTargetActive;

        [Header("§7 同帧双进（可选）")]
        [Tooltip("与 Trigger-0 体积一致的 Collider2D 引用（可与转发子物体上碰撞体重叠）；用于脚点同帧压两区时几何 tie-break。")]
        [SerializeField]
        private Collider2D tieBreakTrigger0;

        [Tooltip("与 Trigger-1 体积一致的 Collider2D 引用。")]
        [SerializeField]
        private Collider2D tieBreakTrigger1;

        [Header("调试")]
        [Tooltip("为 true 时在 ApplyTable 选择结果或 SetActive 实际变化时打印 [VillageTreehouseDepthGate]。")]
        [SerializeField]
        private bool logStateTransitions;

        /// <summary>按 <see cref="PlayerLogic"/> 分会话，避免 NPC / 第二角色误写同一布尔。</summary>
        private readonly Dictionary<PlayerLogic, FootSession> _sessions = new Dictionary<PlayerLogic, FootSession>();

        private void Awake()
        {
            if (depthZoneAndCollidersRoot == null)
            {
                Debug.LogError($"[VillageTreehouseDepthGate] 「{name}」未指定 depthZoneAndCollidersRoot。", this);
                return;
            }

            ApplyTargetActive(initialTargetActive, forceLog: false);
        }

        private void OnDisable()
        {
            // 协调器被关时清空会话，避免 PlayerLogic 销毁后字典悬挂（不触发业务判定，仅释放缓存）。
            _sessions.Clear();
        }

        /// <summary>由 <see cref="VillageTreehouseDepthGateTriggerForward"/> 转发；<paramref name="triggerIndex"/> ∈ {0,1}。</summary>
        public void NotifyTriggerEnter(int triggerIndex, Collider2D other)
        {
            if (depthZoneAndCollidersRoot == null || !IsValidPlayerFoot(other, out var player))
            {
                return;
            }

            if (triggerIndex != 0 && triggerIndex != 1)
            {
                return;
            }

            if (!_sessions.TryGetValue(player, out var session))
            {
                session = new FootSession();
                _sessions[player] = session;
            }

            // 写入 in 之前快照，用于「结算后仍压在一侧、再进另一侧」时补全先碰（避免 FirstTouched 一直为 None）。
            bool prevIn0 = session.In0;
            bool prevIn1 = session.In1;

            // 同帧 Enter 批计数（§7：若需几何 tie-break，可在本 Fixed 步内识别「双区同拍建立」）。
            float ft = Time.fixedTime;
            if (!Mathf.Approximately(session.BatchFixedTime, ft))
            {
                session.BatchFixedTime = ft;
                session.BatchEnterCount = 0;
            }

            session.BatchEnterCount++;

            bool wasLonelyOutside = !prevIn0 && !prevIn1;
            if (wasLonelyOutside)
            {
                session.FirstTouched = triggerIndex == 0 ? FirstTouchId.Trigger0 : FirstTouchId.Trigger1;
            }

            if (triggerIndex == 0)
            {
                session.In0 = true;
            }
            else
            {
                session.In1 = true;
            }

            if (session.In0 && session.In1)
            {
                session.HadBoth = true;

                // 边 B 后已清空 first，但脚点仍留在一侧：再 Enter 另一侧时「先碰」应为已压区一侧（§3.2 重置语义）。
                if (session.FirstTouched == FirstTouchId.None)
                {
                    if (prevIn0 && !prevIn1)
                    {
                        session.FirstTouched = FirstTouchId.Trigger0;
                    }
                    else if (!prevIn0 && prevIn1)
                    {
                        session.FirstTouched = FirstTouchId.Trigger1;
                    }
                }

                // 同帧内第二次 Enter 才形成「同时 01」且第一次 Enter 已写入 first：顺序仍可能违背策划箭头；
                // 若配置了 tie-break 碰撞体，则用脚点中心到两区边界的距离重算「先碰」（推荐几何，文档 §7）。
                if (session.BatchEnterCount >= 2 && tieBreakTrigger0 != null && tieBreakTrigger1 != null)
                {
                    session.FirstTouched = ResolveFirstTouchByGeometry(other);
                }
            }
        }

        /// <summary>由转发脚本在 <c>OnTriggerExit2D</c> 调用；在 <b>wasBoth &amp;&amp; xor</b> 时查 §1.3.3 表并结算。</summary>
        public void NotifyTriggerExit(int triggerIndex, Collider2D other)
        {
            if (depthZoneAndCollidersRoot == null || !IsValidPlayerFoot(other, out var player))
            {
                return;
            }

            if (triggerIndex != 0 && triggerIndex != 1)
            {
                return;
            }

            if (!_sessions.TryGetValue(player, out var session))
            {
                return;
            }

            bool wasBoth = session.In0 && session.In1;

            if (triggerIndex == 0)
            {
                session.In0 = false;
            }
            else
            {
                session.In1 = false;
            }

            bool xorSingle = session.In0 ^ session.In1;

            // 边 B：从双接触变为恰好一侧仍在区内 —— 查表并重置「先碰 / 是否经历 01」累积（§3.2 建议）。
            if (wasBoth && xorSingle)
            {
                int last = session.In0 ? 0 : 1;
                ApplyTable(session, last, other);
                session.FirstTouched = FirstTouchId.None;
                session.HadBoth = false;
            }
            else if (wasBoth && !session.In0 && !session.In1)
            {
                // §7 双出：表未定义 —— 采用「不变」且不强行改 SetActive；清空决策累积以免脏状态。
                if (logStateTransitions)
                {
                    Debug.Log($"[VillageTreehouseDepthGate] 「{name}」双接触同时离开两侧 → 不变（不翻转 SetActive）。", this);
                }

                session.FirstTouched = FirstTouchId.None;
                session.HadBoth = false;
            }

            if (!session.In0 && !session.In1)
            {
                _sessions.Remove(player);
            }
        }

        /// <summary>
        /// §1.3.3 决策表；<paramref name="hadBoth"/> 使用会话内 <see cref="FootSession.HadBoth"/>（双→单时必为 true）。
        /// <para>「保持打开」行：推荐默认 — 仅当目标已为 Active 时保持（不执行关闭）；若目标为 Inactive 则不变（不强制打开）。</para>
        /// </summary>
        private void ApplyTable(FootSession session, int lastSide, Collider2D otherForLog)
        {
            int first = (int)session.FirstTouched;
            if (first != 0 && first != 1)
            {
                if (tieBreakTrigger0 != null && tieBreakTrigger1 != null)
                {
                    first = (int)ResolveFirstTouchByGeometry(otherForLog);
                }
                else
                {
                    Debug.LogWarning(
                        $"[VillageTreehouseDepthGate] 「{name}」结算时 firstTouched 未定义且无 tie-break 碰撞体，默认按 0 处理。请在 Inspector 配置 tie-break 或检查进入顺序。",
                        this);
                    first = 0;
                }
            }

            bool hadBoth = session.HadBoth;
            bool targetActive = depthZoneAndCollidersRoot.activeSelf;

            // 无「同时 01」的路径不应经双→单回调进入；若 hadBoth 为假则防御性不变。
            if (!hadBoth)
            {
                if (logStateTransitions)
                {
                    Debug.Log($"[VillageTreehouseDepthGate] 「{name}」ApplyTable 跳过：hadBoth=false。", this);
                }

                return;
            }

            bool? wantActive = null;
            string note = null;

            if (first == 0 && lastSide == 1)
            {
                wantActive = true;
                note = "先0，经01，收尾1 → 激活";
            }
            else if (first == 0 && lastSide == 0)
            {
                wantActive = false;
                note = "先0，经01，收尾0 → 关闭";
            }
            else if (first == 1 && lastSide == 0)
            {
                wantActive = false;
                note = "先1，经01，收尾0 → 关闭";
            }
            else if (first == 1 && lastSide == 1)
            {
                // 注：保持打开 — Inactive 时不强制 true；Active 时亦不调用 SetActive（均为「不变」）。
                note = targetActive
                    ? "先1，经01，收尾1 → 保持打开（目标已 Active，不调用 SetActive）"
                    : "先1，经01，收尾1 → 不变（目标 Inactive，推荐默认不强制打开）";
            }

            if (logStateTransitions)
            {
                Debug.Log($"[VillageTreehouseDepthGate] 「{name}」ApplyTable：{note}（first={first}, last={lastSide}）", this);
            }

            if (wantActive.HasValue)
            {
                SetTargetActive(wantActive.Value);
            }
        }

        /// <summary>脚点中心到两 Trigger 边界最近点的距离，取更近者为「先碰」索引（文档 §7 推荐几何 tie-break）。</summary>
        private FirstTouchId ResolveFirstTouchByGeometry(Collider2D foot)
        {
            Vector2 p = foot.bounds.center;
            Vector2 c0 = tieBreakTrigger0.ClosestPoint(p);
            Vector2 c1 = tieBreakTrigger1.ClosestPoint(p);
            float d0 = (p - c0).sqrMagnitude;
            float d1 = (p - c1).sqrMagnitude;
            return d0 <= d1 ? FirstTouchId.Trigger0 : FirstTouchId.Trigger1;
        }

        private static bool IsValidPlayerFoot(Collider2D other, out PlayerLogic player)
        {
            player = null;
            if (other == null || !VillagePlayerDepthZoneListener.IsPlayerFootLayer(other.gameObject.layer))
            {
                return false;
            }

            player = other.GetComponentInParent<PlayerLogic>();
            return player != null;
        }

        private void SetTargetActive(bool active)
        {
            if (depthZoneAndCollidersRoot == null)
            {
                return;
            }

            if (depthZoneAndCollidersRoot.activeSelf == active)
            {
                return;
            }

            depthZoneAndCollidersRoot.SetActive(active);
            if (logStateTransitions)
            {
                Debug.Log($"[VillageTreehouseDepthGate] 「{name}」SetActive({active}) → 「{depthZoneAndCollidersRoot.name}」", this);
            }
        }

        private void ApplyTargetActive(bool active, bool forceLog)
        {
            if (depthZoneAndCollidersRoot == null)
            {
                return;
            }

            bool changed = depthZoneAndCollidersRoot.activeSelf != active;
            depthZoneAndCollidersRoot.SetActive(active);
            if (logStateTransitions && (changed || forceLog))
            {
                Debug.Log($"[VillageTreehouseDepthGate] 「{name}」ApplyTargetActive({active}) → 「{depthZoneAndCollidersRoot.name}」", this);
            }
        }
    }
}
