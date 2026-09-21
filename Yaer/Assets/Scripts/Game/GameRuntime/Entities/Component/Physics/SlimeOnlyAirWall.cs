using System.Collections.Generic;
using Game.GameRuntime.Entities.Component.Move;
using Game.GameRuntime.Entities.Monster.Slime;
using Game.GameRuntime.Entities.Monster.Slime.Anima;
using Game.Static.Name.Settings;
using UnityEngine;

namespace Game.GameRuntime.Entities.Component.Physics
{
    /// <summary>
    /// 只拦史莱姆的隐形空气墙（方案 C）。
    /// <para>
    /// <b>为何不用物理硬碰</b>：活体史莱姆 Body / Foot / GroundCld 运行时全是 Trigger，
    /// 走路写 <c>rg.velocity</c>，击退/跳攻写 <c>MovePosition</c> 绝对插值。普通实心墙既对不上盒，也挡不住脚本坐标。
    /// </para>
    /// <para>
    /// <b>做法</b>：本墙 <c>BoxCollider2D.isTrigger=true</c>；每帧 Overlap（含 Trigger）按
    /// <see cref="Slime"/> 身份过滤；用上一帧根坐标扫掠防跳攻穿薄墙；权威
    /// <c>MovePosition</c> 夹紧（执行序 1000，盖过 <see cref="KnockBackComponent"/> 默认 0）。
    /// </para>
    /// <para>
    /// <b>替代方案（本期不用）</b>：A 专用 Layer+矩阵（无实心盒、会误伤木虫）；
    /// B IgnoreCollision 配对实心探测盒（要改史莱姆 Prefab，且打不过击退脚本）；
    /// D = B+C 双保险（OPEN Q5，Play 仍穿再开）；E 改全局矩阵 / F 复用 MapLimit —— 侦探否决。
    /// 不要复用 <see cref="Physics2DComponent"/>（旧 Trigger 夹紧写 transform.position、打 Debug）。
    /// </para>
    /// <para>
    /// 禁止：字符串场景名/「底座」分支；<c>Update</c> 堆逻辑；<c>IgnoreCollision</c> / <c>IgnoreLayerCollision</c>；改史莱姆 AI。
    /// </para>
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(BoxCollider2D))]
    [DefaultExecutionOrder(1000)]
    public class SlimeOnlyAirWall : MonoBehaviour
    {
        private const string LogPrefix = "[SlimeAirWall]";

        [Header("过滤（OPEN Q1 / Q2）")]
        [Tooltip("死后 kinematic FreezeAll，默认不夹，避免和尸体 Snap 抢位。")]
        [SerializeField]
        private bool blockDeadSlimes;

        [Tooltip("睡眠仍是 ISlime；默认仍挡，避免击退把睡着的史莱姆打穿。")]
        [SerializeField]
        private bool blockSleepingSlimes = true;

        [Header("形状")]
        [Tooltip("true=体积挤出（可沿墙外切面滑）。false=只拦 blockNormal 指向的那一侧。")]
        [SerializeField]
        private bool bidirectional = true;

        [Tooltip("本地空间法线。仅 unidirectional 时：该方向为禁侧（走进去会被推回）。")]
        [SerializeField]
        private Vector2 blockNormal = Vector2.right;

        [Header("击退（OPEN Q4）")]
        [Tooltip("撞墙停击退曲线。否则 KnockBack 下一拍仍按 startPos+offset 写 MovePosition，贴墙会抖。")]
        [SerializeField]
        private bool stopKnockbackOnHit = true;

        [Tooltip("挤出后离开表面的余量，防止下一帧又判进。")]
        [SerializeField]
        private float skin = 0.05f;

        [Tooltip("在墙包围盒外扩这么多来找「附近」史莱姆，便于记录 lastPos 做扫掠。跳攻/击退单帧位移应小于此值。")]
        [SerializeField]
        private float nearbyPadding = 12f;

        [Header("验收")]
        [Tooltip("靠近/夹紧/击退穿平面时打 Console。验收完可关。")]
        [SerializeField]
        private bool debugLog;

        [SerializeField]
        private Color gizmoColor = new Color(1f, 0.92f, 0.02f, 0.35f);

        private BoxCollider2D _wallBox;
        private ContactFilter2D _filter;

        /// <summary>史莱姆根 instanceId → 上一 Fixed 的刚体坐标。物体走远或销毁时清掉，不靠 IgnoreCollision 注册表。</summary>
        private readonly Dictionary<int, Vector2> _lastRootPos = new Dictionary<int, Vector2>();

        private readonly HashSet<int> _seenIds = new HashSet<int>();
        private readonly List<int> _pruneBuffer = new List<int>(8);
        private readonly List<Slime> _nearbySlimes = new List<Slime>(8);
        private readonly HashSet<Slime> _nearbyUnique = new HashSet<Slime>();

        private Collider2D[] _overlapBuffer = new Collider2D[32];
        private readonly RaycastHit2D[] _castBuffer = new RaycastHit2D[16];

        private void Awake()
        {
            _wallBox = GetComponent<BoxCollider2D>();
            // 必须 Trigger：本案靠 Overlap 身份过滤，禁止在 Default 层挂实心盒误挡玩家。
            _wallBox.isTrigger = true;

            _filter = new ContactFilter2D();
            _filter.NoFilter();
            _filter.useTriggers = true;

            int ignoreRaycast = LayerMask.NameToLayer(LayerName.IgnoreRaycast);
            Debug.Log(
                $"{LogPrefix} Awake name={name} trigger={_wallBox.isTrigger} size={_wallBox.size} offset={_wallBox.offset} " +
                $"layer={LayerMask.LayerToName(gameObject.layer)}({gameObject.layer}) " +
                $"suggest={LayerName.IgnoreRaycast}({ignoreRaycast}) bidirectional={bidirectional} stopKB={stopKnockbackOnHit}",
                this);
        }

        private void OnDisable()
        {
            _lastRootPos.Clear();
        }

        private void FixedUpdate()
        {
            if (_wallBox == null || !_wallBox.enabled)
            {
                return;
            }

            CollectNearbySlimes();
            _seenIds.Clear();

            for (int i = 0; i < _nearbySlimes.Count; i++)
            {
                var slime = _nearbySlimes[i];
                if (slime == null)
                {
                    continue;
                }

                int id = slime.GetInstanceID();
                _seenIds.Add(id);
                ProcessSlime(slime, id);
            }

            PruneFarSlimes();
        }

        /// <summary>
        /// 扩一圈 Overlap，把附近史莱姆收进列表。不靠 Layer 矩阵。
        /// </summary>
        private void CollectNearbySlimes()
        {
            _nearbySlimes.Clear();
            _nearbyUnique.Clear();

            Bounds b = _wallBox.bounds;
            Vector2 size = (Vector2)b.size + Vector2.one * (nearbyPadding * 2f);
            int count = Physics2D.OverlapBox(b.center, size, 0f, _filter, _overlapBuffer);
            if (count >= _overlapBuffer.Length)
            {
                // 走廊同时刷怪多时扩容，避免漏检。
                _overlapBuffer = new Collider2D[_overlapBuffer.Length * 2];
                count = Physics2D.OverlapBox(b.center, size, 0f, _filter, _overlapBuffer);
            }

            for (int i = 0; i < count; i++)
            {
                var col = _overlapBuffer[i];
                if (col == null || col == _wallBox)
                {
                    continue;
                }

                // 身份：父链上的 Slime。木虫/天琬/玩家/掉落（死后脱离）都进不来。
                var slime = col.GetComponentInParent<Slime>();
                if (slime == null)
                {
                    continue;
                }

                if (_nearbyUnique.Add(slime))
                {
                    _nearbySlimes.Add(slime);
                }
            }
        }

        private void ProcessSlime(Slime slime, int id)
        {
            if (!blockDeadSlimes && slime.IsDead)
            {
                _lastRootPos.Remove(id);
                return;
            }

            if (!blockSleepingSlimes && IsSleeping(slime))
            {
                _lastRootPos.Remove(id);
                return;
            }

            var rg = slime.BodyRg;
            if (rg == null)
            {
                return;
            }

            Vector2 current = rg.position;
            bool hadLast = _lastRootPos.TryGetValue(id, out Vector2 lastPos);
            if (!hadLast)
            {
                lastPos = current;
            }

            if (debugLog)
            {
                LogIdentityOnce(slime, current);
            }

            // 先 default：hadLast==false 时 && 短路不调用 TrySweepHit，C# 会报 CS0165。
            RaycastHit2D sweepHit = default;
            bool hitBySweep = hadLast && TrySweepHit(lastPos, current, slime, out sweepHit);
            bool overlapping = IsOverlappingWall(slime, out Collider2D overlapCol);
            bool forbiddenSide = !bidirectional && IsOnForbiddenSide(current);

            if (!hitBySweep && !overlapping && !forbiddenSide)
            {
                _lastRootPos[id] = current;
                return;
            }

            Vector2 outward;
            Vector2 clamped;
            if (overlapping && overlapCol != null)
            {
                clamped = ComputeExtrusion(rg.position, overlapCol, out outward);
            }
            else if (hitBySweep)
            {
                outward = sweepHit.normal;
                if (outward.sqrMagnitude < 0.0001f)
                {
                    outward = GetWorldBlockNormal();
                }

                clamped = (Vector2)sweepHit.centroid + outward * skin;
            }
            else
            {
                // 单向：人已经站在禁侧但体积未叠（薄墙扫掠漏了）→ 投影回允许面。
                clamped = ProjectToAllowedSide(current, out outward);
            }

            if (!bidirectional)
            {
                // 单向时禁止被挤到禁侧；必要时再投影一次。
                if (IsOnForbiddenSide(clamped))
                {
                    clamped = ProjectToAllowedSide(clamped, out outward);
                }
            }

            ApplyClamp(slime, rg, current, clamped, outward, hitBySweep);
            _lastRootPos[id] = rg.position;
        }

        private bool IsOverlappingWall(Slime slime, out Collider2D overlapCol)
        {
            // 只认史莱姆自己的三盒，不扫子树（掉落物 Trigger 挂在活体下时不要当「史莱姆体积」）。
            if (TryDistanceOverlap(slime.bodyCld, out overlapCol))
            {
                return true;
            }

            if (TryDistanceOverlap(slime.footCld, out overlapCol))
            {
                return true;
            }

            return TryDistanceOverlap(slime.groundCld, out overlapCol);
        }

        private bool TryDistanceOverlap(Collider2D slimeCol, out Collider2D overlapCol)
        {
            overlapCol = null;
            if (slimeCol == null || !slimeCol.enabled)
            {
                return false;
            }

            var d = _wallBox.Distance(slimeCol);
            bool distHit = d.isValid && (d.isOverlapped || d.distance < skin);
            // 两个 Trigger 时 Distance 偶发 isValid=false；AABB 仍能判进墙。
            bool aabbHit = _wallBox.bounds.Intersects(slimeCol.bounds);
            if (distHit || aabbHit)
            {
                overlapCol = slimeCol;
                return true;
            }

            return false;
        }

        /// <summary>
        /// 从上一帧根坐标扫到本帧。跳攻单帧可大于薄墙厚度，Overlap 当帧可能空。
        /// </summary>
        private bool TrySweepHit(Vector2 from, Vector2 to, Slime slime, out RaycastHit2D hit)
        {
            hit = default;
            Vector2 delta = to - from;
            float dist = delta.magnitude;
            if (dist < 0.001f)
            {
                return false;
            }

            Vector2 probe = ResolveProbeSize(slime);
            int n = Physics2D.BoxCast(from, probe, 0f, delta / dist, _filter, _castBuffer, dist);
            for (int i = 0; i < n; i++)
            {
                var h = _castBuffer[i];
                if (h.collider == _wallBox)
                {
                    hit = h;
                    return true;
                }
            }

            return false;
        }

        private static Vector2 ResolveProbeSize(Slime slime)
        {
            if (slime.bodyCld != null)
            {
                var s = slime.bodyCld.bounds.size;
                // 根扫掠用略缩小的盒，避免探针比墙还宽导致「未到墙就命中」。
                return new Vector2(Mathf.Max(0.4f, s.x * 0.25f), Mathf.Max(0.4f, s.y * 0.25f));
            }

            return new Vector2(0.6f, 0.6f);
        }

        private Vector2 ComputeExtrusion(Vector2 rootPos, Collider2D slimeCol, out Vector2 outward)
        {
            var d = _wallBox.Distance(slimeCol);
            if (d.isValid && d.normal.sqrMagnitude > 0.0001f)
            {
                outward = d.normal;
                // Distance：重叠时 distance 为负；normal 从本墙指向对方。
                float push = skin - d.distance;
                if (push < 0f)
                {
                    push = skin;
                }

                return rootPos + outward * push;
            }

            // 退路：按身体盒与墙的重叠深度把根往外推。禁止把墙外的根吸回墙面。
            return ExtrudeRootFromWallAabb(rootPos, slimeCol, out outward);
        }

        private Vector2 ExtrudeRootFromWallAabb(Vector2 rootPos, Collider2D slimeCol, out Vector2 outward)
        {
            Bounds wall = _wallBox.bounds;
            Vector3 root3 = new Vector3(rootPos.x, rootPos.y, wall.center.z);
            if (wall.Contains(root3))
            {
                float dxL = rootPos.x - wall.min.x;
                float dxR = wall.max.x - rootPos.x;
                float dyB = rootPos.y - wall.min.y;
                float dyT = wall.max.y - rootPos.y;
                if (Mathf.Min(dxL, dxR) < Mathf.Min(dyB, dyT))
                {
                    bool pushLeft = dxL < dxR;
                    outward = pushLeft ? Vector2.left : Vector2.right;
                    float x = pushLeft ? wall.min.x - skin : wall.max.x + skin;
                    return new Vector2(x, rootPos.y);
                }

                bool pushDown = dyB < dyT;
                outward = pushDown ? Vector2.down : Vector2.up;
                float y = pushDown ? wall.min.y - skin : wall.max.y + skin;
                return new Vector2(rootPos.x, y);
            }

            Bounds body = slimeCol != null
                ? slimeCol.bounds
                : new Bounds(root3, new Vector3(0.6f, 0.6f, 1f));
            if (body.center.x < wall.center.x)
            {
                outward = Vector2.left;
                float depth = Mathf.Max(0f, body.max.x - wall.min.x);
                return new Vector2(rootPos.x - depth - skin, rootPos.y);
            }

            outward = Vector2.right;
            float depthR = Mathf.Max(0f, wall.max.x - body.min.x);
            return new Vector2(rootPos.x + depthR + skin, rootPos.y);
        }

        private Vector2 GetWorldBlockNormal()
        {
            Vector2 local = blockNormal.sqrMagnitude < 0.0001f ? Vector2.right : blockNormal.normalized;
            Vector2 world = transform.TransformDirection(local);
            if (world.sqrMagnitude < 0.0001f)
            {
                return Vector2.right;
            }

            return world.normalized;
        }

        private bool IsOnForbiddenSide(Vector2 pos)
        {
            Vector2 n = GetWorldBlockNormal();
            Vector2 center = _wallBox.bounds.center;
            Vector2 extents = _wallBox.bounds.extents;
            float halfAlong = Vector2.Dot(new Vector2(Mathf.Abs(n.x), Mathf.Abs(n.y)), extents);
            float allowedMax = -halfAlong - skin;
            return Vector2.Dot(pos - center, n) > allowedMax;
        }

        private Vector2 ProjectToAllowedSide(Vector2 pos, out Vector2 outward)
        {
            Vector2 n = GetWorldBlockNormal();
            Vector2 center = _wallBox.bounds.center;
            Vector2 extents = _wallBox.bounds.extents;
            float halfAlong = Vector2.Dot(new Vector2(Mathf.Abs(n.x), Mathf.Abs(n.y)), extents);
            float allowedMax = -halfAlong - skin;
            float d = Vector2.Dot(pos - center, n);
            outward = -n;
            if (d <= allowedMax)
            {
                return pos;
            }

            return pos - n * (d - allowedMax);
        }

        private void ApplyClamp(Slime slime, Rigidbody2D rg, Vector2 before, Vector2 clamped, Vector2 outward, bool fromSweep)
        {
            if ((clamped - before).sqrMagnitude < 0.0000001f)
            {
                return;
            }

            if (outward.sqrMagnitude < 0.0001f)
            {
                outward = Vector2.right;
            }
            else
            {
                outward.Normalize();
            }

            // 切向保留：只清朝墙内的速度。击飞 Y 在竖直墙上是切向，可留在墙外抖。
            ClearInwardVelocity(slime, rg, outward);

            if (stopKnockbackOnHit)
            {
                var kb = slime.componentSystem != null
                    ? slime.componentSystem.TryGetComponent<KnockBackComponent>()
                    : slime.GetComponent<KnockBackComponent>();
                if (kb != null && kb.IsKnockbackInProgress)
                {
                    kb.StopKnockBackEffect();
                    if (debugLog)
                    {
                        Debug.Log(
                            $"{LogPrefix} stopKnockBack slime={slime.name} before={before} after={clamped} sweep={fromSweep}",
                            this);
                    }
                }
            }

            // 执行序 1000：覆盖本拍 KnockBack / JumpAtk 的 MovePosition。
            rg.MovePosition(clamped);

            // Idle 会 FreezePosition；冻结时 MovePosition 可能被刚体约束吃掉，再写一次权威坐标。
            bool posFrozen = rg.isKinematic
                             || (rg.constraints & RigidbodyConstraints2D.FreezePositionX) != 0
                             || (rg.constraints & RigidbodyConstraints2D.FreezePositionY) != 0;
            if (posFrozen)
            {
                rg.position = clamped;
            }

            if (debugLog)
            {
                Debug.Log(
                    $"{LogPrefix} clamp {slime.name} {before} -> {clamped} n={outward} sweep={fromSweep} dead={slime.IsDead}",
                    this);
            }
        }

        private static void ClearInwardVelocity(Slime slime, Rigidbody2D rg, Vector2 outward)
        {
            MoveComponent move = null;
            if (slime.componentSystem != null)
            {
                move = slime.componentSystem.TryGetComponent<MoveComponent>();
            }

            if (move != null)
            {
                float inward = Vector2.Dot(move.Velocity, -outward);
                if (inward > 0f)
                {
                    move.Velocity += outward * inward;
                }
            }

            Vector2 v = rg.velocity;
            float rin = Vector2.Dot(v, -outward);
            if (rin > 0f)
            {
                rg.velocity = v + outward * rin;
            }
        }

        private static bool IsSleeping(Slime slime)
        {
            if (slime.componentSystem == null)
            {
                return false;
            }

            var cs = slime.componentSystem.TryGetComponent<SlimeCsAnimator>();
            if (cs == null)
            {
                return slime.baseAniState == SlimeAniState.Sleep;
            }

            return cs.GetCurrentAnimatorStateInfo().IsName("Sleep");
        }

        private void PruneFarSlimes()
        {
            _pruneBuffer.Clear();
            foreach (var kv in _lastRootPos)
            {
                if (!_seenIds.Contains(kv.Key))
                {
                    _pruneBuffer.Add(kv.Key);
                }
            }

            for (int i = 0; i < _pruneBuffer.Count; i++)
            {
                _lastRootPos.Remove(_pruneBuffer[i]);
            }
        }

        private bool _loggedIdentity;

        private void LogIdentityOnce(Slime slime, Vector2 pos)
        {
            if (_loggedIdentity)
            {
                return;
            }

            _loggedIdentity = true;
            Debug.Log($"{LogPrefix} nearby slime={slime.name} id={slime.GetInstanceID()} rg={pos} dead={slime.IsDead}", this);
        }

        private void OnDrawGizmos()
        {
            var box = _wallBox != null ? _wallBox : GetComponent<BoxCollider2D>();
            if (box == null)
            {
                return;
            }

            Color fill = gizmoColor;
            Color wire = gizmoColor;
            wire.a = Mathf.Clamp01(gizmoColor.a + 0.4f);

            Gizmos.matrix = transform.localToWorldMatrix;
            Vector3 c = box.offset;
            Vector3 s = box.size;
            Gizmos.color = fill;
            Gizmos.DrawCube(c, s);
            Gizmos.color = wire;
            Gizmos.DrawWireCube(c, s);

            if (!bidirectional)
            {
                Vector3 n = ((Vector2)blockNormal).sqrMagnitude < 0.0001f ? Vector3.right : (Vector3)((Vector2)blockNormal).normalized;
                Gizmos.color = Color.red;
                Gizmos.DrawLine(c, c + n * Mathf.Max(s.x, s.y) * 0.5f);
            }

            Gizmos.matrix = Matrix4x4.identity;
        }
    }
}
