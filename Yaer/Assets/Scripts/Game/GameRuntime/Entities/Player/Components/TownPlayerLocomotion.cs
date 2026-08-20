using System.Collections;
using System.Collections.Generic;
using Game.GameRuntime.Entities.Component.Move;
using Game.Static.Enum;
using Game.Static.Name.Res;
using Game.Static.Name.Settings;
using GameFramework.UnityRuntimeExtend.Component;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.GameRuntime.Entities.Player.Components
{
    /// <summary>
    /// 村庄专用「类 DNF」纵深移动：在 2D（<see cref="Rigidbody2D"/>）环境下，W/S 映射到<strong>世界 Y</strong>（见《村庄DNF式2.5D移动_迁移方案》v1.3）。
    /// <para>根节点 <b>Z</b> 进村时冻结，纵深只改 <b>Y</b>，与 <see cref="DepthComponent"/> 按 Y 排序一致。</para>
    /// <para>
    /// 产品 2026-08-19：村街走路<strong>不要滑行惯性</strong>，松手立刻停（覆盖 0512 AC-02 的纵深摩擦）。
    /// 按住时仍加速 / 斜向仍 0.707；无意图的轴在 <see cref="OnFixedUpdate"/> 当场清零（方案 A′）。
    /// </para>
    /// <para><b>替代方案</b>：若与 Home 跳跃叠冲突，可在村庄关跳跃或给 MoveComponent 增加「仅村庄关重力」开关，由策划定稿。</para>
    /// </summary>
    public class TownPlayerLocomotion : BaseGFComponentMono, IPlayerComponent
    {
        [Header("纵深手感（可调）")]
        [Tooltip("按住 W/S（Vertical 轴）时，纵深 Y 方向加速度（世界单位/秒²）")]
        [SerializeField]
        private float depthAcceleration = 28f;

        [Tooltip("纵深 Y 方向最大速度（世界单位/秒，已乘系数前的上限）")]
        [SerializeField]
        private float depthMaxSpeed = 5.5f;

        [Tooltip("村庄地面目标走速（世界单位/秒）。纯横、纯纵、斜向的平面欧氏速度都接到这个数。旧 Prefab 序列化为 0 时回退 11.2。不要把 runSpeed 与 depthMaxSpeed 改成同一个数冒充修复（斜向仍会 √2）。")]
        [SerializeField]
        private float villagePlanarMoveSpeed = 11.2f;

        [Tooltip("旧 0512 摩擦衰减量。产品 2026-08-19 后松键改为当场清零，本字段不再参与松键路径；保留以免 Prefab 丢序列化。禁止把它调极大来冒充立刻停。")]
        [SerializeField]
#pragma warning disable 0414 // 仅序列化保留；松键路径已改为当场清零，勿再读取冒充刹车。
        private float depthFriction = 18f;
#pragma warning restore 0414

        [Tooltip("Vertical 轴参与加速度前的系数（如 0.6 可削弱 W/S 纵深过快）")]
        [SerializeField]
        [Range(0.05f, 1.5f)]
        private float verticalInputScale = 0.6f;

        [Header("边界 AC-05（世界 Y）")]
        [Tooltip("可走纵深 Y 下限（世界坐标）")]
        [SerializeField]
        private float depthYMinWorld = -20f;

        [Tooltip("可走纵深 Y 上限（世界坐标）")]
        [SerializeField]
        private float depthYMaxWorld = 8f;

        [Header("第三阶段：Village WalkArea（PolygonCollider2D）")]
        [Tooltip("可选：直接在 Inspector 绑定可走区多边形，优先级高于场景内按名查找。")]
        [SerializeField]
        private PolygonCollider2D villageWalkAreaOverride;

        [Tooltip("点在多边形外时，沿「边界最近点 → 内侧」微推的距离（世界单位），减少数值落在边外导致 OverlapPoint 为假。")]
        [SerializeField]
        private float walkPolygonInsetEpsilon = 0.02f;

        [Tooltip(
            "WalkArea 将参考点拉回多边形内时，允许相对「当前越界点」的最大位移（世界单位）。\n" +
            "凹角挤压时 Unity 的 ClosestPoint 可能落到对侧边界，若不做上限会整帧「传送」到地图另一端；超过此距离则改用向局部边界的渐进逼近。")]
        [SerializeField]
        private float walkAreaMaxCorrectionWorldDistance = 8f;

        [Header("验收 / 排障")]
        [Tooltip("开启后每个 FixedUpdate 打印纵深输入、速度、权威 Y、Clamp 提示（用完请关）")]
        [SerializeField]
        private bool acceptanceDebugLog;

        [Header("Animator（Home Walk，策划 6.1）")]
        [Tooltip("|水平目标速度| + |纵深速度| 低于此值视为静止，避免抖动")]
        [SerializeField]
        private float walkAnimatorDeadZone = 0.12f;

        [Header("排序 AC-06：与 DepthComponent 一致（按 Y）")]
        [Tooltip("若赋值，则在村庄模式下每帧按深度刷新 sortingOrder；留空则跳过")]
        [SerializeField]
        private SpriteRenderer spriteForDepthSort;

        [Tooltip("Y 对 sortingOrder 的贡献系数")]
        [SerializeField]
        private float depthSortingFactorY = 100f;

        [Header("纵深：VillageWalkObstacle 阻挡（PlayerFoot）")]
        [Tooltip("为 true 时，在权威 Y 积分后对「PlayerFoot 探针」与 Layer=VillageWalkObstacle 做 Cast/夹紧，使 W/S 纵深可被区内障碍挡住（与纯 velocity 横移不同，须写回前几何限制）。")]
        [SerializeField]
        private bool enableVillageDepthObstacleClamp = true;

        [Tooltip("用于检测与障碍重叠的 Collider2D；留空则在玩家子层级中查找 Layer=PlayerFoot 的 Collider2D 中「包围盒面积最小」的一个（避免误用宽探针导致提前挡）。")]
        [SerializeField]
        private Collider2D villageDepthFootProbeOverride;

        [Tooltip(
            "停障内缩：从 Cast/Raycast 命中距离上扣除的世界距离，用于减轻贴边抖动。\n" +
            "若过大，会在「Collider 线框尚未视觉上贴上」时就无法继续向纵深移动（执行说明 B5）；建议 0.005～0.015。")]
        [SerializeField]
        private float villageObstacleContactSkin = 0.01f;

        [Tooltip(
            "仅加长 Cast/Ray 扫描距离（不参与「d - skin」扣减）。\n" +
            "与「停障内缩」拆开后，可避免旧版用同一字段既拉长扫描又大幅缩短允许位移 → 体感提前被挡。")]
        [SerializeField]
        private float villageObstacleCastPadding = 0.002f;

        [Tooltip(
            "为 true 时：纵深阻挡的 Cast 段改用「脚底包围盒底边中心」的射线检测，替代整颗 Foot 形状沿 Y 的 Collider.Cast。\n" +
            "宽胶囊沿 Y 扫过斜栅栏/台阶侧棱时，形状 Cast 会先「刷边」命中，Scene 上仍像有缝却已不能走；射线更贴近策划对脚底的直觉。\n" +
            "宽脚底仍可能穿尖角，若出现可关回 false 或缩小 Foot 碰撞体（执行说明 P2）。")]
        [SerializeField]
        private bool villageObstacleUseFootBottomRayForDepthCast = true;

        [Tooltip(
            "为 true 时：本帧末若 PlayerFoot 与障碍层发生穿透，用 Physics2D.Distance（ColliderDistance2D）沿法向微移根刚体。\n" +
            "原因：Unity 2020.3 的 Physics2D 无 ComputePenetration；用 Distance 的 normal/负 distance 做等价分离。\n" +
            "方案 1 下脚与障碍矩阵已 Ignore，本段与纵深 Cast 相同，依赖 ContactFilter（含 useTriggers）做查询分离。")]
        [SerializeField]
        private bool enableVillageObstacleFootPenetrationSeparation = true;

        [Header("横移：方案 1 VillageWalkObstacle（Cast 夹紧 velocity.x）")]
        [Tooltip(
            "为 true 时：村庄 2.5D 下在 MoveComponent 写入 velocity 之后，用 PlayerFoot 沿 ±X 对障碍层 Cast，将本帧水平位移夹紧到碰撞前允许值。\n" +
            "原因：矩阵不再用物理解算挡人，必须脚本补横移阻挡，否则穿障（执行文档 0514 §3.4）。")]
        [SerializeField]
        private bool enableVillageHorizontalObstacleClamp = true;

        [Tooltip("Foot–障碍穿透分离迭代次数（每 FixedUpdate）；过大可能一帧滑出过多。")]
        [SerializeField]
        [Range(1, 6)]
        private int villageObstacleFootSeparationIterations = 3;

        [Tooltip("单次分离向量模长上限（世界单位），防止一帧弹出过远。")]
        [SerializeField]
        private float villageObstacleFootSeparationMaxStep = 0.07f;

        [Tooltip("在 Distance 求得的穿透深度上额外加的小量，减少数值抖振再次穿入。")]
        [SerializeField]
        private float villageObstacleFootSeparationInset = 0.003f;

        [Header("穿模保险（0819 方案 A：last-free + Distance 推出）")]
        [Tooltip(
            "为 true 时：轴 Cast / 日常 Distance 之后若脚底仍与障碍重叠，则加大法向推出；仍重叠则拉回本帧开始前的墙外点。\n" +
            "原因：斜围栏会被「纯 X Cast + 纯 Y 射线」拆步钻进去；进去后旧逻辑还会把 vx 锁死。禁止用「重叠就焊死」冒充保险。")]
        [SerializeField]
        private bool enableVillageObstaclePenetrationInsurance = true;

        [Tooltip("保险 Distance 推出的迭代次数（日常分离仍用上面的 3 次，避免贴边每帧弹）。建议 8～12。")]
        [SerializeField]
        [Range(4, 16)]
        private int villageObstacleInsuranceSeparationIterations = 10;

        [Tooltip("保险单次法向步长上限（世界单位）。日常分离仍用 0.07，避免正常贴边被推飞。")]
        [SerializeField]
        private float villageObstacleInsuranceMaxStep = 0.15f;

        [Tooltip("保险本帧累计推出硬顶（世界单位）。超过则改拉 last-free，防止一帧飞出楼梯。")]
        [SerializeField]
        private float villageObstacleInsuranceMaxTotalPush = 0.5f;

        [Tooltip("开启后纵深被障碍截断时打印 [VillageBlockerDepth]（用完请关）。")]
        [SerializeField]
        private bool villageObstacleDepthDebugLog;

        [Header("横移：Turn 同帧护栏（订阅 Move 事件，解耦 FixedUpdate 帧缝）")]
        [Tooltip(
            "为 true 时：进村后订阅 MoveComponent.onTurnAction；在 Turn 的同一调用栈内（Update）若脚底已嵌/贴障碍或短扫新面向即遇障，则立刻清零水平速度并做一次脚底分离。\n" +
            "原因：Turn 在 Update、Cast 护栏在 FixedUpdate，中间隔物理积分易整段 vx 穿出 Trigger；不修改 Move/Turn 实现，逻辑放在独立静态类 VillageWalkObstacleTurnImmediateBlock。")]
        [SerializeField]
        private bool enableVillageObstacleTurnImmediateHorizontalClear = true;

        [Tooltip("转身后沿新面向短扫的世界长度（应略大于典型 Run/Walk 单 Fixed 步长）。")]
        [SerializeField]
        private float villageObstaclePostTurnProbeDistance = 0.28f;

        [Tooltip("短扫得到的「可通行余量」低于此值（世界单位）则视为贴壳，清空本帧水平速度。")]
        [SerializeField]
        private float villageObstaclePostTurnBlockClearance = 0.05f;

        /// <summary>旧 Prefab 缺字段写成 0 时的目标走速回退（与现网 runSpeed 一致）。</summary>
        private const float VillagePlanarMoveSpeedFallback = 11.2f;

        /// <summary>与现网 Town / Combat 横纵意图死区一致。</summary>
        private const float VillagePlanarInputDeadZone = 0.01f;

        /// <summary>纵深方向速度（沿世界 Y，米/秒）。</summary>
        private float depthVelocity;

        /// <summary>与刚体/重力解耦的权威纵深世界 Y；Clamp 与积分只改此字段。</summary>
        private float _villageWorldY;

        /// <summary>进村时锁定的根世界 Z，纵深模式不改 Z（文档 0 节）。</summary>
        private float _frozenWorldZ;

        /// <summary>玩家根上的 2D 刚体（与 PlayerLogic 同物体）。</summary>
        private Rigidbody2D _playerRootRb2D;

        /// <summary>进村时从场景解析到的 WalkArea（无 Override 时使用）；离村清空，避免跨场景误引用（验收 P-06）。</summary>
        private PolygonCollider2D _villageWalkPolygonFromScene;

        /// <summary>在 <see cref="WaitForFixedUpdate"/> 之后再次写回 Y/Z，减轻物理步与 MoveComponent 对 Transform 的覆盖。</summary>
        private Coroutine _postPhysicsDepthCoroutine;

        /// <summary>缓存的脚底探针；离村时清空以便换 Prefab 后重新解析。</summary>
        private Collider2D _villageDepthFootProbeResolved;

        /// <summary>子节点中未找到 PlayerFoot 层 Collider 时置 true，避免每帧全量遍历。</summary>
        private bool _villageDepthFootProbeSearchFailed;

        /// <summary>Cast / Overlap 复用列表，避免 FixedUpdate 内 GC。</summary>
        private readonly List<RaycastHit2D> _villageObstacleCastHits = new List<RaycastHit2D>(8);

        /// <summary>脚底射线纵深检测复用缓冲（<see cref="Physics2D.Raycast"/> + <see cref="ContactFilter2D"/>）。</summary>
        private readonly RaycastHit2D[] _villageObstacleRaycastHits = new RaycastHit2D[8];

        /// <summary><see cref="Collider2D.OverlapCollider"/> 结果缓冲。</summary>
        private readonly List<Collider2D> _villageObstacleOverlapBuffer = new List<Collider2D>(8);

        /// <summary>已订阅 <see cref="MoveComponent.onTurnAction"/> 的 Move 引用；离村/销毁时退订，避免泄漏与跨场景回调。</summary>
        private PlayerMoveComponent _villageTurnGuardSubscribedMove;

        /// <summary>仅检测 <see cref="LayerName.VillageWalkObstacle"/>；<see cref="ContactFilter2D.useTriggers"/> 为 true，与障碍 <c>isTrigger</c> 定稿一致。</summary>
        private ContactFilter2D _villageObstacleContactFilter;

        /// <summary>上一帧（本帧积分前）脚底不与障碍重叠时的根刚体 XY。已重叠时禁止覆盖。</summary>
        private Vector2 _lastFreeRootPos;

        /// <summary>与 <see cref="_lastFreeRootPos"/> 同时记下的权威纵深 Y。</summary>
        private float _lastFreeAuthY;

        /// <summary>last-free 是否有效。开局就嵌在墙里时为假，此时只 Distance 推、不焊死、不闪回楼梯中线（OPEN T2）。</summary>
        private bool _hasLastFreeVillagePose;

        /// <summary>父实体逻辑，用于取 Animator、Move、输入等。</summary>
        public PlayerLogic PlayerLogic { get; set; }

        /// <summary>供验收脚本只读：纵深 Y 边界与状态。</summary>
        public float DebugDepthYMinWorld => depthYMinWorld;

        public float DebugDepthYMaxWorld => depthYMaxWorld;

        public float DebugDepthVelocity => depthVelocity;

        public float DebugAuthoritativeWorldY => _villageWorldY;

        public float DebugFrozenWorldZ => _frozenWorldZ;

        /// <summary>供验收脚本只读：当前生效的 WalkArea 多边形（Override 或场景解析）。</summary>
        public PolygonCollider2D DebugEffectiveWalkPolygon => ResolveEffectiveWalkPolygon();

        /// <summary>供验收脚本只读：权威 XY 是否在 WalkArea 内（未配置多边形时视为 true）。</summary>
        public bool DebugIsInsideWalkPolygon
        {
            get
            {
                PolygonCollider2D poly = ResolveEffectiveWalkPolygon();
                if (poly == null || !poly.enabled)
                {
                    return true;
                }

                Vector2 sample = ResolveWalkSampleWorldXY();
                return poly.OverlapPoint(sample);
            }
        }

        protected override void OnInit()
        {
            // 默认关闭，避免非村庄场景误跑 FixedUpdate（AC-01）
            enabled = false;
            EnsureVillagePlanarMoveSpeedConfigured();
        }

        public override void OnFixedUpdate()
        {
            base.OnFixedUpdate();
            if (!enabled || PlayerLogic == null || !PlayerLogic.AllowControl)
            {
                return;
            }

            var input = PlayerLogic.componentSystem.GetComponent<PlayerInputComponent>();
            if (input == null || input.LocomotionMode != PlayerLocomotionMode.Village2_5D)
            {
                return;
            }

            if (_playerRootRb2D == null)
            {
                _playerRootRb2D = PlayerLogic.gameObject.GetComponent<Rigidbody2D>();
            }

            // 方案 A（0819 围栏）：必须在积分 / WalkArea / 写根之前记墙外点。
            // 已重叠时不要覆盖——否则保险会把「栏杆里面」当成合法点，人焊死在墙里。
            TryCaptureVillageLastFreePose();

            // Vertical → 纵深 Y；系数削弱输入，避免「一步跨太大」
            float inputAxis = Input.GetAxisRaw("Vertical");
            float inputH = Input.GetAxisRaw("Horizontal");
            float inputDepth = inputAxis * verticalInputScale;
            float dt = Time.fixedDeltaTime;
            float planarSpeed = ResolveVillagePlanarMoveSpeed();

            if (Mathf.Abs(inputDepth) > VillagePlanarInputDeadZone)
            {
                depthVelocity += inputDepth * depthAcceleration * dt;
                // 方案 A（0818）：纯 W 满速接到目标走速，不再被 depthMaxSpeed(5.5) 卡住，否则纯纵永远慢于纯横。
                depthVelocity = Mathf.Clamp(depthVelocity, -planarSpeed, planarSpeed);
            }
            else
            {
                // 方案 A′（0819）：无纵深输入当场清零。必须写在这一段，不能塞进 NONE——
                // 斜着走时只松 W、仍按 D 走 HORIZ_ONLY，若只在 NONE 清纵深，人还会往前后飘。
                // 原因：村里纵深权威 Y 由本组件积分，Combat Idle 的 StopMove 清不掉 depthVelocity。
                // 禁止：把 depthFriction 调极大冒充刹车（按住时手感也会怪）；禁止仍按着 W/S 时清零。
                // 替代（否决）：方案 C 只让 Combat 退 Idle——刚体停了，权威 Y 照样滑。
                depthVelocity = 0f;
            }

            // 合速度只在这一处缩放：必须在积分 Y 之前，且在 Combat 灌速 / 0818 转向之后。
            string planarBranch = ApplyVillagePlanarMoveSpeedNormalization(input, planarSpeed);

            float yBeforeIntegrate = _villageWorldY;
            _villageWorldY += depthVelocity * dt;
            float yBeforeClamp = _villageWorldY;
            // 顺序（策略 A，执行说明 §5）：标量 Y Clamp → 障碍夹紧 → 写回刚体 → WalkArea 多边形 XY 修正 → 若根位被多边形改动则再障碍夹紧（避免改 X 后 Cast 前提与线框错位）。
            _villageWorldY = Mathf.Clamp(_villageWorldY, depthYMinWorld, depthYMaxWorld);
            // Walk 区内障碍：在写回刚体前对权威 Y 做几何夹紧，否则仅 Layer 碰撞无法挡住「脚本直接改 rb.position.y」的纵深（见《村庄WalkArea内部阻挡碰撞体》§3.2）。
            ApplyVillageWalkObstacleDepthClamp(yBeforeIntegrate);
            WriteRootTransformWithAuthoritativeDepthY();
            // 记录 WalkArea 修正前根刚体 XY：多边形可能改 X/Y，若不在同一几何前提下再跑障碍 Cast，会与 Collider 线框错位（执行说明 §5 P0）。
            Vector2 rootRbBeforeWalkPolygon = _playerRootRb2D != null ? _playerRootRb2D.position : new Vector2(PlayerLogic.transform.position.x, PlayerLogic.transform.position.y);
            ApplyVillageWalkPolygonPostCorrection();
            ApplyVillageWalkObstacleClampAfterWalkPolygonIfNeeded(rootRbBeforeWalkPolygon);
            ApplyVillageWalkObstacleFootPenetrationSeparation();
            // 顺序（方案 1）：脚穿透分离后再夹横移速度，避免分离改位形后仍保留「指向障碍内」的 vx（执行文档 §3.4）。
            ApplyVillageWalkObstacleHorizontalVelocityClamp();
            // 保险必须在 WalkArea 之后：多边形 ClosestPoint 可能把人推进区内围栏；恢复后再跑 ClosestPoint 会再次推进去。
            ApplyVillageWalkObstaclePenetrationInsurance();

            if (acceptanceDebugLog)
            {
                bool clampedLow = yBeforeClamp < depthYMinWorld - 1e-4f;
                bool clampedHigh = yBeforeClamp > depthYMaxWorld + 1e-4f;
                string clampHint = clampedHigh && inputAxis > 0.01f
                    ? "CLAMP_AT_YMAX(按W无效时检查 depthYMaxWorld)"
                    : clampedLow && inputAxis < -0.01f
                        ? "CLAMP_AT_YMIN(按S无效时检查 depthYMinWorld)"
                        : "clamp_ok";
                PlayerMoveComponent moveForLog = PlayerLogic.componentSystem.GetComponent<PlayerMoveComponent>();
                Debug.Log(
                    $"[TownLocomotion] dt={dt:F4} axisH={inputH:F3} intentH={input.HasVillageExploreHorizontalMoveIntent()} " +
                    $"axisV={inputAxis:F3} intentV={input.HasVillageExploreVerticalMoveIntent()} branch={planarBranch} " +
                    $"inputDepth={inputDepth:F3} depthVel={depthVelocity:F3} " +
                    $"vx={(moveForLog != null ? moveForLog.moveSpeedX : 0f):F3} " +
                    $"planar={planarSpeed:F2} (legacyDepthMax={depthMaxSpeed:F2}) " +
                    $"authY {yBeforeIntegrate:F4}->{_villageWorldY:F4} (preClamp={yBeforeClamp:F4}) [{clampHint}] " +
                    $"Ybounds=[{depthYMinWorld:F2},{depthYMaxWorld:F2}] frozenZ={_frozenWorldZ:F3} " +
                    $"rb2D=({(_playerRootRb2D != null ? _playerRootRb2D.position.x : 0f):F3},{(_playerRootRb2D != null ? _playerRootRb2D.position.y : 0f):F3}) " +
                    $"rootPos={PlayerLogic.transform.position}",
                    this);
            }
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            // 不在此用 !enabled 短路：村庄模式下仍希望同步 Walk 与排序（与 HasVillageDepthMoveForHomeStateMachine 的门控顺序一致）
            if (PlayerLogic == null || PlayerLogic.animator == null)
            {
                return;
            }

            var input = PlayerLogic.componentSystem.GetComponent<PlayerInputComponent>();
            if (input == null || input.LocomotionMode != PlayerLocomotionMode.Village2_5D)
            {
                return;
            }

            SyncWalkAnimatorParameter();
            // 纵深积分与写回仅在 enabled 时进行；排序依赖权威 Y，与之一致，避免离村或禁用后仍改 sortingOrder
            if (enabled)
            {
                ApplyDepthSortingFromWorldPosition();
            }
        }

        /// <summary>
        /// 由 <see cref="PlayerLogic.SetVillageExplorationMode"/> 调用：开关村庄逻辑并复位速度，避免离村残留惯性。
        /// </summary>
        public void ApplyVillageMode(bool active)
        {
            if (_postPhysicsDepthCoroutine != null)
            {
                StopCoroutine(_postPhysicsDepthCoroutine);
                _postPhysicsDepthCoroutine = null;
            }

            enabled = active;
            depthVelocity = 0f;
            if (active)
            {
                EnsureVillagePlanarMoveSpeedConfigured();
            }
            if (!active)
            {
                UnregisterVillageTurnObstacleGuard();
                _villageWalkPolygonFromScene = null;
                _villageDepthFootProbeResolved = null;
                _villageDepthFootProbeSearchFailed = false;
                _hasLastFreeVillagePose = false;
            }
            else
            {
                // 进村后允许重新解析脚底 Collider（Prefab 热更或子物体延迟激活）。
                _villageDepthFootProbeSearchFailed = false;
            }

            if (active && PlayerLogic != null)
            {
                if (_playerRootRb2D == null)
                {
                    _playerRootRb2D = PlayerLogic.gameObject.GetComponent<Rigidbody2D>();
                }

                TryBindVillageWalkPolygonFromActiveScene();
                _frozenWorldZ = PlayerLogic.transform.position.z;
                _villageWorldY = _playerRootRb2D != null ? _playerRootRb2D.position.y : PlayerLogic.transform.position.y;
                _villageWorldY = Mathf.Clamp(_villageWorldY, depthYMinWorld, depthYMaxWorld);
                WriteRootTransformWithAuthoritativeDepthY();
                Vector2 rootRbBeforeWalkPolygon = _playerRootRb2D != null ? _playerRootRb2D.position : new Vector2(PlayerLogic.transform.position.x, PlayerLogic.transform.position.y);
                ApplyVillageWalkPolygonPostCorrection();
                ApplyVillageWalkObstacleClampAfterWalkPolygonIfNeeded(rootRbBeforeWalkPolygon);
                ApplyVillageWalkObstacleHorizontalVelocityClamp();
                // 进村时若脚在墙外，先记下初值；若已重叠则保险 Distance 推（不闪回楼梯中线）。
                TryCaptureVillageLastFreePose();
                ApplyVillageWalkObstaclePenetrationInsurance();
                RegisterVillageTurnObstacleGuard();
                _postPhysicsDepthCoroutine = StartCoroutine(PostPhysicsResyncDepthCoroutine());
            }
        }

        /// <summary>
        /// 由 <see cref="PlayerLogic.SetVillageExplorationMode"/> 在 <c>TryInjectVillageDepthYBoundsFromSceneMarkers</c> 之后调用：
        /// 标尺只改了权威 Y 标量，需立刻写回刚体并套 WalkArea，避免首帧与后续 FixedUpdate 不一致。
        /// </summary>
        public void FlushAuthoritativeVillageTransformAfterSceneDepthInject()
        {
            if (!enabled || PlayerLogic == null)
            {
                return;
            }

            WriteRootTransformWithAuthoritativeDepthY();
            Vector2 rootRbBeforeWalkPolygon = _playerRootRb2D != null ? _playerRootRb2D.position : new Vector2(PlayerLogic.transform.position.x, PlayerLogic.transform.position.y);
            ApplyVillageWalkPolygonPostCorrection();
            ApplyVillageWalkObstacleClampAfterWalkPolygonIfNeeded(rootRbBeforeWalkPolygon);
            ApplyVillageWalkObstacleHorizontalVelocityClamp();
            TryCaptureVillageLastFreePose();
            ApplyVillageWalkObstaclePenetrationInsurance();
        }

        /// <summary>运行时调整纵深 Y 边界（场景加载器或空物体标尺对齐后注入）。</summary>
        public void SetDepthYBounds(float minY, float maxY)
        {
            if (minY > maxY)
            {
                (minY, maxY) = (maxY, minY);
            }

            depthYMinWorld = minY;
            depthYMaxWorld = maxY;
            _villageWorldY = Mathf.Clamp(_villageWorldY, depthYMinWorld, depthYMaxWorld);
        }

        /// <summary>兼容旧 API 名称；参数语义为<strong>世界 Y</strong>，不是 Z。</summary>
        public void SetZBounds(float minY, float maxY)
        {
            SetDepthYBounds(minY, maxY);
        }

        /// <summary>旧 Prefab 把新字段序列化成 0 时回退，避免目标走速为 0 导致站桩。</summary>
        private void EnsureVillagePlanarMoveSpeedConfigured()
        {
            if (villagePlanarMoveSpeed <= 0f)
            {
                villagePlanarMoveSpeed = VillagePlanarMoveSpeedFallback;
            }
        }

        /// <summary>村庄地面目标走速；≤0 时回退 <see cref="VillagePlanarMoveSpeedFallback"/>。</summary>
        private float ResolveVillagePlanarMoveSpeed()
        {
            return villagePlanarMoveSpeed > 0f ? villagePlanarMoveSpeed : VillagePlanarMoveSpeedFallback;
        }

        /// <summary>
        /// 方案 A（0818）：把横、纵接到同一目标走速。斜向判定必须对齐村里走路意图（队列 / GetKey），
        /// 禁止只认 <c>GetAxisRaw("Horizontal")</c>——本工程轴可能恒为 0，会进不成 DIAGONAL，横向留下 11.2。
        /// <para>
        /// 方案 A′（0819）：无横意图时写横向 0（DEPTH_ONLY / NONE）。FixedUpdate 早于 Combat Update，
        /// 只靠 CombatRun 的 StopMoveInX 会晚 1 物理帧；斜向松 D 仍按 W 会先飘一下。
        /// 0513 不回归：hasH 为真时进不成这两个分支。禁止用含 |depthVelocity| 的 DepthIntent 每帧清 X。
        /// </para>
        /// <para>
        /// 替代：看速度再 ClampMagnitude（方案 B）在纵深加速期横向仍接近满速；
        /// Town 末尾无条件写 0.707 若用 DirectionSign 会和 0818 点 A 冲突。
        /// </para>
        /// </summary>
        /// <returns>DIAGONAL / HORIZ_ONLY / DEPTH_ONLY / NONE，供 acceptanceDebugLog。</returns>
        private string ApplyVillagePlanarMoveSpeedNormalization(PlayerInputComponent input, float planarSpeed)
        {
            bool hasH = input != null && input.HasVillageExploreHorizontalMoveIntent();
            bool hasV = input != null && input.HasVillageExploreVerticalMoveIntent();
            PlayerMoveComponent move = PlayerLogic != null
                ? PlayerLogic.componentSystem.GetComponent<PlayerMoveComponent>()
                : null;

            float sx = input != null ? input.GetVillageExploreHorizontalSign() : 0f;
            float sy = input != null ? input.GetVillageExploreVerticalSign() : 0f;

            // hasH 已真但符号仍 0 的极端帧：才允许用当前 vx，再没有才 DirectionSign（默认朝右，禁止当第一手）。
            if (hasH && Mathf.Abs(sx) <= VillagePlanarInputDeadZone)
            {
                if (move != null && Mathf.Abs(move.moveSpeedX) > VillagePlanarInputDeadZone)
                {
                    sx = Mathf.Sign(move.moveSpeedX);
                }
                else if (move != null)
                {
                    sx = move.DirectionSign;
                }
                else
                {
                    sx = 1f;
                }
            }

            if (hasV && Mathf.Abs(sy) <= VillagePlanarInputDeadZone)
            {
                sy = Mathf.Abs(depthVelocity) > VillagePlanarInputDeadZone ? Mathf.Sign(depthVelocity) : 1f;
            }

            if (hasH && hasV)
            {
                Vector2 n = new Vector2(sx, sy).normalized;
                WriteVillagePlanarHorizontalSpeed(move, n.x * planarSpeed);
                depthVelocity = n.y * planarSpeed;
                return "DIAGONAL";
            }

            if (hasH)
            {
                WriteVillagePlanarHorizontalSpeed(move, sx * planarSpeed);
                return "HORIZ_ONLY";
            }

            if (hasV)
            {
                // 方案 A′：确认无横意图，当帧写横向 0。不要再等 Combat Update 的 StopMoveInX。
                // 0513 现场是「有横却被当成没横」；0818 后 hasH 已对齐队列/GetKey，按着 D 不会进本分支。
                // 替代（否决）：继续「纯纵深不写横向」——斜向松 D 仍按 W 时，vx 会在下一物理帧前残留。
                WriteVillagePlanarHorizontalSpeed(move, 0f);
                depthVelocity = Mathf.Clamp(depthVelocity, -planarSpeed, planarSpeed);
                return "DEPTH_ONLY";
            }

            // 双手空：横向立刻 0。纵深已在 OnFixedUpdate 无 V 输入段清掉，这里不必再写 depthVelocity。
            WriteVillagePlanarHorizontalSpeed(move, 0f);
            return "NONE";
        }

        /// <summary>同步脚本目标速度与刚体 vx。Move 本帧可能已写入未缩放的 runSpeed，WriteRoot 会保留 vx。</summary>
        private void WriteVillagePlanarHorizontalSpeed(PlayerMoveComponent move, float vx)
        {
            if (move != null)
            {
                move.moveSpeedX = vx;
            }

            if (_playerRootRb2D != null)
            {
                Vector2 v = _playerRootRb2D.velocity;
                _playerRootRb2D.velocity = new Vector2(vx, v.y);
            }
        }

        /// <summary>
        /// 将权威纵深 Y 与冻结 Z 写回根 Transform，并与 Rigidbody2D.position 对齐；清零纵向速度分量，避免与 MoveComponent 重力在同一轴叠加（文档 3.5）。
        /// </summary>
        private void WriteRootTransformWithAuthoritativeDepthY()
        {
            if (PlayerLogic == null)
            {
                return;
            }

            if (_playerRootRb2D != null)
            {
                Vector2 rbPos = _playerRootRb2D.position;
                // 纵深只动 Y；X 跟刚体当前模拟位置；Z 保持进村冻结值（不得把 Vertical 误写到 Z）
                Vector2 newRb = new Vector2(rbPos.x, _villageWorldY);
                _playerRootRb2D.position = newRb;
                Vector2 v = _playerRootRb2D.velocity;
                _playerRootRb2D.velocity = new Vector2(v.x, 0f);
                PlayerLogic.transform.position = new Vector3(newRb.x, newRb.y, _frozenWorldZ);
            }
            else
            {
                Vector3 p = PlayerLogic.transform.position;
                p.y = _villageWorldY;
                p.z = _frozenWorldZ;
                PlayerLogic.transform.position = p;
            }

            var move = PlayerLogic.componentSystem.GetComponent<PlayerMoveComponent>();
            if (move != null)
            {
                move.moveSpeedY = 0f;
            }
        }

        private IEnumerator PostPhysicsResyncDepthCoroutine()
        {
            var wait = new WaitForFixedUpdate();
            while (enabled && PlayerLogic != null)
            {
                yield return wait;
                if (!enabled || PlayerLogic == null)
                {
                    continue;
                }

                var input = PlayerLogic.componentSystem.GetComponent<PlayerInputComponent>();
                if (input == null || input.LocomotionMode != PlayerLocomotionMode.Village2_5D)
                {
                    continue;
                }

                WriteRootTransformWithAuthoritativeDepthY();
                // MoveComponent 等可能在同一物理帧改 X：在 WaitForFixedUpdate 后再收一次多边形，避免贴边穿出（执行说明 §8）。
                Vector2 rootRbBeforeWalkPolygon = _playerRootRb2D != null ? _playerRootRb2D.position : new Vector2(PlayerLogic.transform.position.x, PlayerLogic.transform.position.y);
                ApplyVillageWalkPolygonPostCorrection();
                ApplyVillageWalkObstacleClampAfterWalkPolygonIfNeeded(rootRbBeforeWalkPolygon);
                ApplyVillageWalkObstacleFootPenetrationSeparation();
                ApplyVillageWalkObstacleHorizontalVelocityClamp();
                // 物理步 + WalkArea 之后可能再次嵌进围栏；同一套保险必须再跑，否则会出现「FixedUpdate 拉出来、物理后又焊回去」。
                ApplyVillageWalkObstaclePenetrationInsurance();
            }

            _postPhysicsDepthCoroutine = null;
        }

        /// <summary>
        /// 供 Home 子状态机（Idle/Bink/Walk）使用：村庄模式下是否应视为「纵深在移动」。
        /// <para>优先用 <see cref="depthVelocity"/> 与 <see cref="walkAnimatorDeadZone"/> 对齐 Animator；若首帧尚未积分，则用 <c>Vertical</c> 轴门控，避免纯 W/S 卡在 Idle（执行说明 §5.2，不改 <c>HasMoveInput</c>）。</para>
        /// <para><b>替代方案</b>：扩展 <c>PlayerInputComponent.HasMoveInput</c> 含纵轴可能与「禁止改输入系统」冲突，故集中在本组件判定。</para>
        /// <para><b>判定顺序说明</b>：必须先校验 <see cref="PlayerLocomotionMode.Village2_5D"/> 与竖轴，再校验本脚本 <c>enabled</c>。
        /// 若先判 <c>!enabled</c>，在 <see cref="ApplyVillageMode"/> 将 <c>enabled=true</c> 之前、或同一帧内状态机早于本组件逻辑时，
        /// 会出现「已切村庄模式且按住 W/S，但 Idle 仍不进 <see cref="Game.GameRuntime.Entities.Player.Components.CsAnimator.Home.HomeWalkState"/>」的断层（Animator 子状态 Idle 仅响应 <c>IdleSubState</c> 退出，单靠 <c>Walk</c> 参数无法从 Idle 子态直接切到 Walk，必须靠 C# 先退出子状态机）。</para>
        /// </summary>
        public bool HasVillageDepthMoveForHomeStateMachine()
        {
            if (PlayerLogic == null)
            {
                return false;
            }

            var input = PlayerLogic.componentSystem.GetComponent<PlayerInputComponent>();
            if (input == null || input.LocomotionMode != PlayerLocomotionMode.Village2_5D)
            {
                return false;
            }

            // 与 OnFixedUpdate 同源：按住 W/S 即视为有纵深意图，不依赖本帧是否已执行过 FixedUpdate 积分
            if (Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0.01f)
            {
                return true;
            }

            // 0819 A′ 松键后 depthVelocity 已是 0，本分支通常为假。保留死区判定以免障碍夹紧等残留速度漏驱动 Walk；不要删这段去「假装没惯性」。
            if (!enabled)
            {
                return false;
            }

            return Mathf.Abs(depthVelocity) > walkAnimatorDeadZone;
        }

        private void SyncWalkAnimatorParameter()
        {
            var move = PlayerLogic.componentSystem.GetComponent<PlayerMoveComponent>();
            float horizontalSpeed = move != null ? Mathf.Abs(move.moveSpeedX) : 0f;
            // 未启用时 depthVelocity 不再积分，但横移与竖轴仍应能驱动 Walk，避免纯 A/D 或「仅竖轴」时 Animator 与 Home 子状态机脱节
            float depthSpeed = enabled ? Mathf.Abs(depthVelocity) : 0f;
            // 与 HasVillageDepthMoveForHomeStateMachine 一致：首帧 depth 尚未积分时仍推 Walk，避免 Animator 与 HomeWalkState 脱节
            bool walk =
                horizontalSpeed + depthSpeed > walkAnimatorDeadZone
                || Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0.01f;
            // 仅同步 Walk：Run 由战斗子状态机（CombatIdle/CombatRun）独占，若在此写 Run 会与 SetAnimatorEnter/Exit 打架，
            // 出现「C# 已在 Idle、Animator 仍停在 Run」→ BaseStateMachine 等不到 IsName(Idle)，整卡死（先 AD 再 WS 典型复现）。
            SyncWalkMotionBoolIfPresent(PlayerLogic.animator, walk);
        }

        /// <summary>
        /// 仅写入 Home 控制器上的 <c>Walk</c>（若存在）。战斗控制器上的 <c>Run</c> 必须由 <see cref="Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat.State.Ground.CombatRunState"/> 等状态切换驱动。
        /// </summary>
        private static void SyncWalkMotionBoolIfPresent(Animator animator, bool moving)
        {
            if (animator == null || animator.runtimeAnimatorController == null)
            {
                return;
            }

            foreach (AnimatorControllerParameter p in animator.parameters)
            {
                if (p.type != AnimatorControllerParameterType.Bool || p.name != "Walk")
                {
                    continue;
                }

                animator.SetBool(p.name, moving);
            }
        }

        private void ApplyDepthSortingFromWorldPosition()
        {
            if (spriteForDepthSort == null)
            {
                return;
            }

            Vector3 w = spriteForDepthSort.transform.position;
            // 与 DepthComponent「Y 越低越靠前」一致（文档 3.6）
            int order = Mathf.RoundToInt(-(w.y * depthSortingFactorY));
            spriteForDepthSort.sortingOrder = order;
        }

        /// <summary>与 <see cref="WriteRootTransformWithAuthoritativeDepthY"/> 一致：用刚体 XY 作为 WalkArea 判定参考点（执行说明 §5.2）。</summary>
        private Vector2 ResolveWalkSampleWorldXY()
        {
            if (_playerRootRb2D != null)
            {
                return _playerRootRb2D.position;
            }

            if (PlayerLogic == null)
            {
                return Vector2.zero;
            }

            var rb = PlayerLogic.gameObject.GetComponent<Rigidbody2D>();
            return rb != null ? rb.position : (Vector2)PlayerLogic.transform.position;
        }

        /// <summary>Inspector 覆盖优先；否则使用进村时解析的场景多边形（执行说明 §4.1）。</summary>
        private PolygonCollider2D ResolveEffectiveWalkPolygon()
        {
            if (villageWalkAreaOverride != null)
            {
                return villageWalkAreaOverride;
            }

            return _villageWalkPolygonFromScene;
        }

        /// <summary>
        /// 在 <see cref="SceneName.Village_KenMuNi1"/> 下按物体名 <c>VillageWalkArea</c> 查找 <see cref="PolygonCollider2D"/>（可挂在同名物体自身或子级）。
        /// <para><b>替代方案</b>：多块区域时用列表 + 并集判定；首版仅支持单 Polygon（执行说明 §4.1）。</para>
        /// </summary>
        private void TryBindVillageWalkPolygonFromActiveScene()
        {
            _villageWalkPolygonFromScene = null;
            if (villageWalkAreaOverride != null)
            {
                return;
            }

            Scene scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || scene.name != SceneName.Village_KenMuNi1)
            {
                return;
            }

            Transform named = FindNamedTransformInLoadedScene(scene, "VillageWalkArea");
            if (named == null)
            {
                return;
            }

            PolygonCollider2D poly = named.GetComponent<PolygonCollider2D>();
            if (poly == null)
            {
                poly = named.GetComponentInChildren<PolygonCollider2D>(true);
            }

            _villageWalkPolygonFromScene = poly;
        }

        /// <summary>与 <see cref="PlayerLogic"/> 内标尺查找一致：仅在指定 Scene 根层级递归匹配物体名，避免跨场景误命中。</summary>
        private static Transform FindNamedTransformInLoadedScene(Scene scene, string objectName)
        {
            if (!scene.IsValid() || string.IsNullOrEmpty(objectName))
            {
                return null;
            }

            foreach (GameObject root in scene.GetRootGameObjects())
            {
                Transform found = FindNamedTransformRecursive(root.transform, objectName);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private static Transform FindNamedTransformRecursive(Transform tr, string objectName)
        {
            if (tr.name == objectName)
            {
                return tr;
            }

            for (int i = 0; i < tr.childCount; i++)
            {
                Transform child = FindNamedTransformRecursive(tr.GetChild(i), objectName);
                if (child != null)
                {
                    return child;
                }
            }

            return null;
        }

        /// <summary>
        /// 第三阶段核心：若参考点落在多边形外，则拉回可走区内部（执行说明 §5.2）。
        /// <para>
        /// <b>原因说明（凹角「传送」）</b>：旧版在「最近边界点 + ε」失败后，用 <c>Lerp(boundary, poly.bounds.center)</c> 找内侧。
        /// 凹多边形的 AABB 中心常在形外，<c>ClosestPoint(center)</c> 会落到远处边；弦线穿过形外空隙时，沿参数 t 增长的第一个 <c>OverlapPoint</c>
        /// 可能落在地图另一侧口袋，表现为挤压拐角时整帧跳到对面卡死。
        /// </para>
        /// <para>
        /// <b>当前策略</b>：① 最短路径内推 ε；② 从 <c>boundary</c> 发射多根射线，二分找每条射线上最近的形内点，再在候选中取离 <paramref name="worldPoint"/> 最近者并受 <paramref name="maxCorrectionDistance"/> 约束；
        /// ③ 仍无解时用沿「当前点 → ClosestPoint(当前点)」的渐进逼近（每步跟局部边走，不会跨图跳点）。
        /// </para>
        /// <para><b>替代方案</b>：用 Clipper 做真·多边形内缩/最近内点；或缓存上一帧合法位置作吸引子；成本更高，首版不采用。</para>
        /// </summary>
        private Vector2 ClampWorldPointToPolygonInterior(PolygonCollider2D poly, Vector2 worldPoint, float insetEpsilon)
        {
            if (poly == null || !poly.enabled)
            {
                return worldPoint;
            }

            if (poly.OverlapPoint(worldPoint))
            {
                return worldPoint;
            }

            float push = Mathf.Max(insetEpsilon, 0.001f);
            float maxSq = Mathf.Max(0.05f, walkAreaMaxCorrectionWorldDistance);
            maxSq *= maxSq;

            Vector2 boundary = poly.ClosestPoint(worldPoint);

            // 在「确实在形内」的候选里选离越界点最近的，避免采纳对侧口袋；并拒绝单帧超大位移。
            Vector2 best = boundary;
            float bestDsq = float.MaxValue;

            void ConsiderCandidate(Vector2 candidate)
            {
                if (!poly.OverlapPoint(candidate))
                {
                    return;
                }

                float dsq = (candidate - worldPoint).sqrMagnitude;
                if (dsq > maxSq)
                {
                    return;
                }

                if (dsq < bestDsq)
                {
                    bestDsq = dsq;
                    best = candidate;
                }
            }

            // ① 沿「越界点 → 最近边点」方向越过边界的微推（与旧版一致，凹顶点处可能失败）
            Vector2 dirFromOutside = boundary - worldPoint;
            if (dirFromOutside.sqrMagnitude > 1e-12f)
            {
                ConsiderCandidate(boundary + dirFromOutside.normalized * push);
            }

            // ② 从 boundary 沿圆周方向扫射线，二分找每条线上最近的形内点（不依赖 AABB 中心，避免穿空腔拾到远端）
            float maxRay = Mathf.Clamp(Mathf.Max(poly.bounds.extents.x, poly.bounds.extents.y) * 2.5f, push * 4f, walkAreaMaxCorrectionWorldDistance);
            const int RayCount = 24;
            for (int k = 0; k < RayCount; k++)
            {
                float ang = k * (Mathf.PI * 2f / RayCount);
                Vector2 dir = new Vector2(Mathf.Cos(ang), Mathf.Sin(ang));
                if (TrySmallestInteriorPointOnRay(poly, boundary, dir, push, maxRay, out Vector2 hit))
                {
                    ConsiderCandidate(hit);
                }
            }

            if (bestDsq < float.MaxValue)
            {
                return best;
            }

            // ③ 渐进逼近：每步朝「当前点相对多边形的最近点」移动，ClosestPoint 随 q 连续变化，只收敛到局部边，不会一帧飞到对侧
            Vector2 q = worldPoint;
            for (int iter = 0; iter < 36; iter++)
            {
                if (poly.OverlapPoint(q))
                {
                    return q;
                }

                Vector2 shell = poly.ClosestPoint(q);
                float dist = Vector2.Distance(q, shell);
                float step = Mathf.Max(0.02f, dist * 0.4f);
                q = Vector2.MoveTowards(q, shell, step);
                if (dist < 1e-5f)
                {
                    break;
                }
            }

            return poly.OverlapPoint(q) ? q : boundary;
        }

        /// <summary>
        /// 在射线 <c>origin + dir * t</c>（<paramref name="dir"/> 单位向量）上，找满足 <c>t ∈ [minT, maxT]</c> 的最小 <c>t</c> 使 <c>OverlapPoint</c> 为真。
        /// </summary>
        private static bool TrySmallestInteriorPointOnRay(
            PolygonCollider2D poly,
            Vector2 origin,
            Vector2 dir,
            float minT,
            float maxT,
            out Vector2 result)
        {
            result = default;
            if (poly == null || maxT <= minT || dir.sqrMagnitude < 1e-10f)
            {
                return false;
            }

            dir.Normalize();
            Vector2 near = origin + dir * minT;
            if (poly.OverlapPoint(near))
            {
                result = near;
                return true;
            }

            Vector2 far = origin + dir * maxT;
            if (!poly.OverlapPoint(far))
            {
                return false;
            }

            float lo = minT;
            float hi = maxT;
            for (int i = 0; i < 14; i++)
            {
                float mid = (lo + hi) * 0.5f;
                if (poly.OverlapPoint(origin + dir * mid))
                {
                    hi = mid;
                }
                else
                {
                    lo = mid;
                }
            }

            result = origin + dir * hi;
            return poly.OverlapPoint(result);
        }

        /// <summary>在纵深写回之后调用，统一把 Rigidbody2D.position 收进 WalkArea（策略 A：与标量 Y Clamp 叠加时几何以 Polygon 为准）。</summary>
        private void ApplyVillageWalkPolygonPostCorrection()
        {
            PolygonCollider2D poly = ResolveEffectiveWalkPolygon();
            if (poly == null || !poly.enabled || PlayerLogic == null)
            {
                return;
            }

            var input = PlayerLogic.componentSystem.GetComponent<PlayerInputComponent>();
            if (input == null || input.LocomotionMode != PlayerLocomotionMode.Village2_5D)
            {
                return;
            }

            if (_playerRootRb2D == null)
            {
                _playerRootRb2D = PlayerLogic.gameObject.GetComponent<Rigidbody2D>();
            }

            if (_playerRootRb2D == null)
            {
                return;
            }

            Vector2 p = _playerRootRb2D.position;
            Vector2 corrected = ClampWorldPointToPolygonInterior(poly, p, walkPolygonInsetEpsilon);
            if ((corrected - p).sqrMagnitude <= 1e-10f)
            {
                return;
            }

            _villageWorldY = corrected.y;
            _playerRootRb2D.position = corrected;
            Vector2 v = _playerRootRb2D.velocity;
            _playerRootRb2D.velocity = new Vector2(v.x, 0f);
            PlayerLogic.transform.position = new Vector3(corrected.x, corrected.y, _frozenWorldZ);
            var move = PlayerLogic.componentSystem.GetComponent<PlayerMoveComponent>();
            if (move != null)
            {
                move.moveSpeedY = 0f;
            }
        }

        /// <summary>
        /// WalkArea 只保证「在多边形可走区内」，与障碍 Collider 无关：多边形修正会改根的 XY（尤其 X），
        /// 若障碍夹紧只在修正前执行，修正后脚底可能按新 X 与障碍重叠，表现为与 Polygon 线框不一致或嵌入。（执行说明 §0.2 / §5 P0）
        /// <para>做法：将根暂放到「WalkArea 修正后的 X + 修正前的 Y」，满足 <see cref="ApplyVillageWalkObstacleDepthClamp"/> 内 Cast 对「起点 Y = previousAuthoritativeY」的前提，再以修正后的权威 Y 复跑同一套 Cast/二分/挤出。</para>
        /// <para><b>替代方案</b>：把 WalkArea 与障碍合并为单一约束求解；改动面大，不符合本任务「最小增量」。</para>
        /// </summary>
        /// <param name="rootRbWorldBeforePolygon">调用 <see cref="ApplyVillageWalkPolygonPostCorrection"/> 之前根的 <see cref="Rigidbody2D.position"/>。</param>
        private void ApplyVillageWalkObstacleClampAfterWalkPolygonIfNeeded(Vector2 rootRbWorldBeforePolygon)
        {
            if (!enableVillageDepthObstacleClamp || PlayerLogic == null)
            {
                return;
            }

            int obstacleLayer = LayerMask.NameToLayer(LayerName.VillageWalkObstacle);
            if (obstacleLayer < 0)
            {
                return;
            }

            if (_playerRootRb2D == null)
            {
                _playerRootRb2D = PlayerLogic.gameObject.GetComponent<Rigidbody2D>();
            }

            if (_playerRootRb2D == null)
            {
                return;
            }

            Vector2 rootAfter = _playerRootRb2D.position;
            if ((rootAfter - rootRbWorldBeforePolygon).sqrMagnitude < 1e-14f)
            {
                return;
            }

            // 新 X 必须与 WalkArea 修正后一致（Overlap/Cast 用当前根 X）；起点 Y 取修正前，目标权威 Y 取修正后。
            float x1 = rootAfter.x;
            float y0 = rootRbWorldBeforePolygon.y;
            float y1 = rootAfter.y;
            _playerRootRb2D.position = new Vector2(x1, y0);
            PlayerLogic.transform.position = new Vector3(x1, y0, _frozenWorldZ);
            _villageWorldY = y0;
            Physics2D.SyncTransforms();
            _villageWorldY = y1;
            ApplyVillageWalkObstacleDepthClamp(y0);
            WriteRootTransformWithAuthoritativeDepthY();
        }

        /// <summary>
        /// 缓解 <see cref="LayerName.PlayerFoot"/> 与 <see cref="LayerName.VillageWalkObstacle"/> 的<strong>查询几何重叠</strong>（方案 1 下不再依赖物理解算「顶开」）：
        /// 纵深已由 Cast/Overlap 约束；横移由 <see cref="ApplyVillageWalkObstacleHorizontalVelocityClamp"/> 夹紧 <c>velocity.x</c>。
        /// <para>在 WalkArea/纵深障碍逻辑之后，用 <see cref="Physics2D.Distance"/> 返回的 <see cref="ColliderDistance2D"/>（Unity 2020.3 无 <c>Physics2D.ComputePenetration</c>）做短迭代分离；<see cref="ContactFilter2D.useTriggers"/> 须为 true 以命中 Trigger 障碍。</para>
        /// <para><b>替代方案</b>：完全依赖 Cast 不做 Distance 分离；尖角嵌入时可能残留，以 QA 为准切换。</para>
        /// </summary>
        private void ApplyVillageWalkObstacleFootPenetrationSeparation()
        {
            if (!enableVillageDepthObstacleClamp || !enableVillageObstacleFootPenetrationSeparation || PlayerLogic == null)
            {
                return;
            }

            if (_playerRootRb2D == null)
            {
                _playerRootRb2D = PlayerLogic.gameObject.GetComponent<Rigidbody2D>();
            }

            if (_playerRootRb2D == null)
            {
                return;
            }

            int obstacleLayer = LayerMask.NameToLayer(LayerName.VillageWalkObstacle);
            if (obstacleLayer < 0)
            {
                return;
            }

            Collider2D foot = ResolveVillageDepthFootProbe();
            if (foot == null || !foot.enabled)
            {
                return;
            }

            BuildVillageObstacleContactFilter(obstacleLayer);

            for (int iter = 0; iter < villageObstacleFootSeparationIterations; iter++)
            {
                Physics2D.SyncTransforms();
                _villageObstacleOverlapBuffer.Clear();
                int overlapCount = foot.OverlapCollider(_villageObstacleContactFilter, _villageObstacleOverlapBuffer);
                if (overlapCount == 0)
                {
                    return;
                }

                Vector2 accumulated = Vector2.zero;
                for (int i = 0; i < overlapCount; i++)
                {
                    Collider2D obs = _villageObstacleOverlapBuffer[i];
                    if (obs == null || obs == foot)
                    {
                        continue;
                    }

                    ColliderDistance2D cd = Physics2D.Distance(foot, obs);
                    if (!cd.isValid)
                    {
                        continue;
                    }

                    // 已明显分离则本障碍不参与本帧推力（Unity 2020：重叠时 distance 常为负，见 ColliderDistance2D 文档）
                    if (!cd.isOverlapped && cd.distance > 0.0005f)
                    {
                        continue;
                    }

                    float penetration;
                    if (cd.distance < -1e-5f)
                    {
                        penetration = -cd.distance;
                    }
                    else if (cd.isOverlapped || cd.distance <= 0.0005f)
                    {
                        penetration = villageObstacleFootSeparationInset;
                    }
                    else
                    {
                        continue;
                    }

                    Vector2 n = cd.normal;
                    if (n.sqrMagnitude < 1e-10f)
                    {
                        continue;
                    }

                    float push = Mathf.Min(penetration + villageObstacleFootSeparationInset, villageObstacleFootSeparationMaxStep);
                    accumulated += n * push;
                }

                if (accumulated.sqrMagnitude < 1e-14f)
                {
                    return;
                }

                if (accumulated.magnitude > villageObstacleFootSeparationMaxStep)
                {
                    accumulated = accumulated.normalized * villageObstacleFootSeparationMaxStep;
                }

                Vector2 p = _playerRootRb2D.position + accumulated;
                _playerRootRb2D.position = p;
                _villageWorldY = p.y;
                PlayerLogic.transform.position = new Vector3(p.x, p.y, _frozenWorldZ);
                Vector2 v = _playerRootRb2D.velocity;
                _playerRootRb2D.velocity = new Vector2(v.x, 0f);
                var move = PlayerLogic.componentSystem.GetComponent<PlayerMoveComponent>();
                if (move != null)
                {
                    move.moveSpeedY = 0f;
                }

                LogVillageObstacleDepth(
                    $"foot penetration separation iter={iter} Δ=({accumulated.x:F4},{accumulated.y:F4}) pos=({p.x:F3},{p.y:F3})");
            }
        }

        /// <summary>
        /// 若当前脚底不与 <see cref="LayerName.VillageWalkObstacle"/> 重叠，记下根刚体 XY 与权威 Y，供穿模后拉回。
        /// 已重叠时禁止覆盖：否则保险会把栏杆里面当成合法点。
        /// </summary>
        private void TryCaptureVillageLastFreePose()
        {
            if (!enableVillageDepthObstacleClamp || PlayerLogic == null)
            {
                return;
            }

            if (_playerRootRb2D == null)
            {
                _playerRootRb2D = PlayerLogic.gameObject.GetComponent<Rigidbody2D>();
            }

            if (_playerRootRb2D == null)
            {
                return;
            }

            if (IsVillageFootOverlappingWalkObstaclesNow())
            {
                return;
            }

            _lastFreeRootPos = _playerRootRb2D.position;
            _lastFreeAuthY = _villageWorldY;
            _hasLastFreeVillagePose = true;
        }

        /// <summary>当前位姿下脚底是否与村庄 Walk 障碍重叠（不改 Transform，给 last-free / 保险用）。</summary>
        private bool IsVillageFootOverlappingWalkObstaclesNow()
        {
            int obstacleLayer = LayerMask.NameToLayer(LayerName.VillageWalkObstacle);
            if (obstacleLayer < 0)
            {
                return false;
            }

            Collider2D foot = ResolveVillageDepthFootProbe();
            if (foot == null || !foot.enabled)
            {
                return false;
            }

            BuildVillageObstacleContactFilter(obstacleLayer);
            Physics2D.SyncTransforms();
            _villageObstacleOverlapBuffer.Clear();
            return foot.OverlapCollider(_villageObstacleContactFilter, _villageObstacleOverlapBuffer) > 0;
        }

        /// <summary>
        /// 0819 方案 A 保险：轴 Cast 与日常 Distance 之后若仍重叠，先加大法向推出；仍重叠则拉回 last-free。
        /// <para><b>原因</b>：斜围栏没有合成方向扫掠，斜向一步会从「纯 X / 纯 Y」缝里钻进去；进去后纵深只沿 Y 挤、横移曾锁 vx，人焊在栏杆里。</para>
        /// <para><b>必须在 WalkArea 之后</b>：多边形修正可能把人推进区内围栏；拉回后再 ClosestPoint 会再次推进去。</para>
        /// <para><b>替代（否决）</b>：方案 E 重叠就锁死速度——现网卡死主因。方案 B 合成 Cast 防穿好，但人已在 Composite 内部时仍可能扫空，不能替代本保险。</para>
        /// </summary>
        private void ApplyVillageWalkObstaclePenetrationInsurance()
        {
            if (!enableVillageDepthObstacleClamp || !enableVillageObstaclePenetrationInsurance || PlayerLogic == null)
            {
                return;
            }

            var input = PlayerLogic.componentSystem.GetComponent<PlayerInputComponent>();
            if (input == null || input.LocomotionMode != PlayerLocomotionMode.Village2_5D)
            {
                return;
            }

            if (_playerRootRb2D == null)
            {
                _playerRootRb2D = PlayerLogic.gameObject.GetComponent<Rigidbody2D>();
            }

            if (_playerRootRb2D == null)
            {
                return;
            }

            if (!IsVillageFootOverlappingWalkObstaclesNow())
            {
                return;
            }

            Vector2 totalPush;
            if (TryVillageObstacleInsuranceDistancePush(out totalPush))
            {
                depthVelocity = 0f;
                // 推出方向指向墙外；vx 与推出 X 反向 = 还在往墙里顶，清掉。切向（同号或推出几乎纯 Y）保留，贴边仍能滑。
                float vx = _playerRootRb2D.velocity.x;
                if (totalPush.sqrMagnitude > 1e-10f && vx * totalPush.x < -1e-6f)
                {
                    PlayerMoveComponent moveAfterPush = PlayerLogic.componentSystem.GetComponent<PlayerMoveComponent>();
                    WriteVillagePlanarHorizontalSpeed(moveAfterPush, 0f);
                }

                LogVillageObstacleDepth(
                    $"insurance overlap→push Δ=({totalPush.x:F4},{totalPush.y:F4}) pos=({_playerRootRb2D.position.x:F3},{_playerRootRb2D.position.y:F3})");
                return;
            }

            if (_hasLastFreeVillagePose)
            {
                RestoreVillageLastFreePose();
                LogVillageObstacleDepth(
                    $"insurance restore last-free pos=({_lastFreeRootPos.x:F3},{_lastFreeAuthY:F3})");
                return;
            }

            // OPEN T2：没有 last-free（开局就嵌在墙里）不闪回楼梯中线；上面 Distance 已尽力推，下一帧继续，禁止焊死。
            LogVillageObstacleDepth(
                $"insurance overlap no last-free; keep pushing pos=({_playerRootRb2D.position.x:F3},{_playerRootRb2D.position.y:F3})");
        }

        /// <summary>
        /// 保险用 Distance 法向推出。比日常分离更狠，但累计位移有硬顶，避免一帧飞出楼梯。
        /// </summary>
        /// <returns>推出后脚底不再重叠则为 true。</returns>
        private bool TryVillageObstacleInsuranceDistancePush(out Vector2 accumulatedWorld)
        {
            accumulatedWorld = Vector2.zero;
            int obstacleLayer = LayerMask.NameToLayer(LayerName.VillageWalkObstacle);
            if (obstacleLayer < 0)
            {
                return false;
            }

            Collider2D foot = ResolveVillageDepthFootProbe();
            if (foot == null || !foot.enabled)
            {
                return false;
            }

            BuildVillageObstacleContactFilter(obstacleLayer);
            float maxTotal = Mathf.Max(0.05f, villageObstacleInsuranceMaxTotalPush);
            int iters = Mathf.Max(1, villageObstacleInsuranceSeparationIterations);
            float maxStep = Mathf.Max(0.01f, villageObstacleInsuranceMaxStep);

            for (int iter = 0; iter < iters; iter++)
            {
                if (accumulatedWorld.magnitude >= maxTotal - 1e-5f)
                {
                    break;
                }

                Physics2D.SyncTransforms();
                _villageObstacleOverlapBuffer.Clear();
                int overlapCount = foot.OverlapCollider(_villageObstacleContactFilter, _villageObstacleOverlapBuffer);
                if (overlapCount == 0)
                {
                    return true;
                }

                Vector2 step = Vector2.zero;
                for (int i = 0; i < overlapCount; i++)
                {
                    Collider2D obs = _villageObstacleOverlapBuffer[i];
                    if (obs == null || obs == foot)
                    {
                        continue;
                    }

                    ColliderDistance2D cd = Physics2D.Distance(foot, obs);
                    if (!cd.isValid)
                    {
                        continue;
                    }

                    if (!cd.isOverlapped && cd.distance > 0.0005f)
                    {
                        continue;
                    }

                    float penetration;
                    if (cd.distance < -1e-5f)
                    {
                        penetration = -cd.distance;
                    }
                    else if (cd.isOverlapped || cd.distance <= 0.0005f)
                    {
                        penetration = villageObstacleFootSeparationInset;
                    }
                    else
                    {
                        continue;
                    }

                    Vector2 n = cd.normal;
                    if (n.sqrMagnitude < 1e-10f)
                    {
                        continue;
                    }

                    float push = Mathf.Min(penetration + villageObstacleFootSeparationInset, maxStep);
                    step += n * push;
                }

                if (step.sqrMagnitude < 1e-14f)
                {
                    break;
                }

                if (step.magnitude > maxStep)
                {
                    step = step.normalized * maxStep;
                }

                float remain = maxTotal - accumulatedWorld.magnitude;
                if (step.magnitude > remain)
                {
                    step = step.normalized * Mathf.Max(0f, remain);
                }

                if (step.sqrMagnitude < 1e-14f)
                {
                    break;
                }

                Vector2 p = _playerRootRb2D.position + step;
                _playerRootRb2D.position = p;
                _villageWorldY = p.y;
                PlayerLogic.transform.position = new Vector3(p.x, p.y, _frozenWorldZ);
                Vector2 v = _playerRootRb2D.velocity;
                _playerRootRb2D.velocity = new Vector2(v.x, 0f);
                var move = PlayerLogic.componentSystem.GetComponent<PlayerMoveComponent>();
                if (move != null)
                {
                    move.moveSpeedY = 0f;
                }

                accumulatedWorld += step;
            }

            Physics2D.SyncTransforms();
            return !IsVillageFootOverlappingWalkObstaclesNow();
        }

        /// <summary>
        /// 把根刚体 / Transform / 权威 Y 拉回本帧开始前的墙外点，并清掉本帧速度。
        /// 拉回后不要立刻再跑 WalkArea ClosestPoint：可能再次把人推进围栏。
        /// </summary>
        private void RestoreVillageLastFreePose()
        {
            if (!_hasLastFreeVillagePose || PlayerLogic == null)
            {
                return;
            }

            Vector2 restored = new Vector2(_lastFreeRootPos.x, _lastFreeAuthY);
            _villageWorldY = _lastFreeAuthY;
            depthVelocity = 0f;
            if (_playerRootRb2D != null)
            {
                _playerRootRb2D.position = restored;
                _playerRootRb2D.velocity = new Vector2(0f, 0f);
                PlayerLogic.transform.position = new Vector3(restored.x, restored.y, _frozenWorldZ);
            }
            else
            {
                PlayerLogic.transform.position = new Vector3(restored.x, restored.y, _frozenWorldZ);
            }

            PlayerMoveComponent move = PlayerLogic.componentSystem.GetComponent<PlayerMoveComponent>();
            WriteVillagePlanarHorizontalSpeed(move, 0f);
            if (move != null)
            {
                move.moveSpeedY = 0f;
            }

            Physics2D.SyncTransforms();
        }

        /// <summary>
        /// 使用 <see cref="LayerName.PlayerFoot"/> 上的 <see cref="Collider2D"/> 对 <see cref="LayerName.VillageWalkObstacle"/> 做纵深方向限制，
        /// 在 <see cref="WriteRootTransformWithAuthoritativeDepthY"/> 之前修正 <see cref="_villageWorldY"/> 与 <see cref="depthVelocity"/>。
        /// <para><b>原因</b>：权威 Y 由脚本写回，不经过 Y 向速度物理解算，故必须在积分后显式 Cast/夹紧。</para>
        /// <para><b>替代方案</b>：改为纯物理驱动纵深（改 velocity.y）会与 MoveComponent/重力及现有 WalkArea 策略冲突，故不采用。</para>
        /// </summary>
        /// <param name="previousAuthoritativeY">本帧积分前权威世界 Y（与当前刚体 Y 一致）。</param>
        private void ApplyVillageWalkObstacleDepthClamp(float previousAuthoritativeY)
        {
            if (!enableVillageDepthObstacleClamp || PlayerLogic == null)
            {
                return;
            }

            int obstacleLayer = LayerMask.NameToLayer(LayerName.VillageWalkObstacle);
            if (obstacleLayer < 0)
            {
                return;
            }

            Collider2D foot = ResolveVillageDepthFootProbe();
            if (foot == null || !foot.enabled)
            {
                return;
            }

            BuildVillageObstacleContactFilter(obstacleLayer);

            float yTarget = _villageWorldY;
            float dy = yTarget - previousAuthoritativeY;

            // 静止帧：若已嵌入障碍内，尝试沿 Y 挤出，避免传送/WalkArea 修正后卡在 Collider 内。
            if (Mathf.Abs(dy) < 1e-6f)
            {
                if (IsFootOverlappingWalkObstaclesAtAuthoritativeY(yTarget))
                {
                    if (TryDepenetrateFootFromWalkObstacles(ref _villageWorldY, previousAuthoritativeY))
                    {
                        depthVelocity = 0f;
                        LogVillageObstacleDepth($"stationary depenetrate → authY={_villageWorldY:F4}");
                    }
                }

                return;
            }

            // 快速路径：沿运动方向检测障碍；castDist 仅用较小 Padding 加长扫描，停障距离单独用 villageObstacleContactSkin 扣减，避免「提前挡一大块真空」。
            Vector2 castDir = dy > 0f ? Vector2.up : Vector2.down;
            float castDist = Mathf.Abs(dy) + Mathf.Max(0.0001f, villageObstacleCastPadding);
            Physics2D.SyncTransforms();
            float absDy = Mathf.Abs(dy);
            string hitName;
            float allowedAlong = villageObstacleUseFootBottomRayForDepthCast
                ? ComputeVillageObstacleAllowedAlongFromFootBottomRay(foot, castDir, absDy, castDist, obstacleLayer, out hitName)
                : ComputeVillageObstacleAllowedAlongFromFootShapeCast(foot, castDir, absDy, castDist, obstacleLayer, out hitName);

            if (allowedAlong + 1e-4f < absDy)
            {
                _villageWorldY = previousAuthoritativeY + Mathf.Sign(dy) * allowedAlong;
                depthVelocity = 0f;
                string via = villageObstacleUseFootBottomRayForDepthCast ? "ray" : "shapeCast";
                LogVillageObstacleDepth(
                    $"cast block via={via} hit={(hitName ?? "?")} dy={dy:F4} allowed={allowedAlong:F4} → authY={_villageWorldY:F4}");
                return;
            }

            // Cast 未缩短位移，但目标位置仍可能重叠（起始于障碍内部等）：二分找最后自由位置。
            if (!IsFootOverlappingWalkObstaclesAtAuthoritativeY(yTarget))
            {
                return;
            }

            if (!IsFootOverlappingWalkObstaclesAtAuthoritativeY(previousAuthoritativeY))
            {
                _villageWorldY = dy > 0f
                    ? BinarySearchLastFreeWhenMovingUp(previousAuthoritativeY, yTarget)
                    : BinarySearchLastFreeWhenMovingDown(yTarget, previousAuthoritativeY);
                depthVelocity = 0f;
                LogVillageObstacleDepth($"binary clamp prev={previousAuthoritativeY:F4} target={yTarget:F4} → authY={_villageWorldY:F4}");
                return;
            }

            if (TryDepenetrateFootFromWalkObstacles(ref _villageWorldY, previousAuthoritativeY))
            {
                depthVelocity = 0f;
                LogVillageObstacleDepth($"both ends overlap depenetrate → authY={_villageWorldY:F4}");
            }
        }

        private void BuildVillageObstacleContactFilter(int obstacleLayer)
        {
            // 方案 1：障碍 Collider 为 Trigger；Cast/Overlap/Raycast 必须包含 Trigger，否则会「线框在却查不到」假穿障（执行文档 §3.2）。
            _villageObstacleContactFilter.useTriggers = true;
            _villageObstacleContactFilter.useLayerMask = true;
            _villageObstacleContactFilter.SetLayerMask(1 << obstacleLayer);
        }

        /// <summary>
        /// 方案 1 配套：矩阵不再挡横移，在物理积分前用脚底形状沿水平 Cast，将 <c>velocity.x</c> 与 <see cref="PlayerMoveComponent.moveSpeedX"/> 夹紧。
        /// <para><b>原因</b>：Prefab 中本组件在 <c>componentsList</c> 内排在 <see cref="PlayerMoveComponent"/> 之后，本方法在 <see cref="OnFixedUpdate"/> 末尾调用时 Move 已写入速度。</para>
        /// <para><b>穿障</b>：贴障 / 挤压时 Cast 命中距离常为 0，不得丢弃。0819 起「重叠且 Cast 空」不再一律锁 vx——那是斜围栏卡死主因；有 last-free 时交给保险拉回，允许贴边切向滑（OPEN T1）。</para>
        /// <para><b>转身帧缝</b>：Turn 在 Update、本方法在 FixedUpdate，中间易漏拦；由 <see cref="RegisterVillageTurnObstacleGuard"/> 订阅 <see cref="MoveComponent.onTurnAction"/>，经 <see cref="VillageWalkObstacleTurnImmediateBlock"/> 同栈补判（解耦在独立静态类）。</para>
        /// <para><b>替代方案</b>：改 Move 全局 Turn 或 CCD 子步进；影响面大。重叠就整帧禁止移动（方案 E）会焊死在栏杆里，禁止。</para>
        /// </summary>
        private void ApplyVillageWalkObstacleHorizontalVelocityClamp()
        {
            if (!enableVillageDepthObstacleClamp || !enableVillageHorizontalObstacleClamp || PlayerLogic == null)
            {
                return;
            }

            var input = PlayerLogic.componentSystem.GetComponent<PlayerInputComponent>();
            if (input == null || input.LocomotionMode != PlayerLocomotionMode.Village2_5D)
            {
                return;
            }

            if (_playerRootRb2D == null)
            {
                _playerRootRb2D = PlayerLogic.gameObject.GetComponent<Rigidbody2D>();
            }

            if (_playerRootRb2D == null)
            {
                return;
            }

            int obstacleLayer = LayerMask.NameToLayer(LayerName.VillageWalkObstacle);
            if (obstacleLayer < 0)
            {
                return;
            }

            Collider2D foot = ResolveVillageDepthFootProbe();
            if (foot == null || !foot.enabled)
            {
                return;
            }

            BuildVillageObstacleContactFilter(obstacleLayer);

            float vx = _playerRootRb2D.velocity.x;
            float dt = Time.fixedDeltaTime;
            if (Mathf.Abs(vx) < 1e-5f)
            {
                return;
            }

            Vector2 castDir = vx > 0f ? Vector2.right : Vector2.left;
            float absDx = Mathf.Abs(vx * dt);
            float castDist = absDx + Mathf.Max(0.0001f, villageObstacleCastPadding);

            Physics2D.SyncTransforms();
            // 挤压 / 贴 Trigger 时脚底可能已与障碍层重叠：若仅依赖正向 Cast，「命中距离≈0」的旧分支会被丢弃 → allowedAlong 仍为整段位移 → 概率穿障。
            _villageObstacleOverlapBuffer.Clear();
            bool footEmbeddedInObstacle = foot.OverlapCollider(_villageObstacleContactFilter, _villageObstacleOverlapBuffer) > 0;

            _villageObstacleCastHits.Clear();
            int hitCount = foot.Cast(castDir, _villageObstacleContactFilter, _villageObstacleCastHits, castDist);
            float allowedAlong = absDx;
            float stopSkin = Mathf.Max(0f, villageObstacleContactSkin);

            for (int i = 0; i < hitCount; i++)
            {
                RaycastHit2D h = _villageObstacleCastHits[i];
                if (h.collider == null || h.collider.gameObject.layer != obstacleLayer)
                {
                    continue;
                }

                float d = h.distance;
                // 贴壳或起点已在壳上：d≈0 必须视为「本帧不允许再沿 castDir 深入」，禁止 continue 跳过（否则持续顶障会保持全速 vx）。
                if (d < 0f)
                {
                    allowedAlong = Mathf.Min(allowedAlong, 0f);
                    continue;
                }

                allowedAlong = Mathf.Min(allowedAlong, Mathf.Max(0f, d - stopSkin));
            }

            // Cast 未缩短位移、且脚底与障碍重叠、且正向 Cast 零命中：
            // 旧逻辑在此把 vx 锁成 0，本意是防大块 Trigger 内切向穿出；落在斜围栏内部就变成焊死（方案 E，禁止当保险）。
            // 0819：有 last-free 时不要锁——贴边切向滑交给轴 Cast；重叠由末尾保险 Distance / 拉回处理。
            // 仅开局就嵌在墙里（没有 last-free）才清本帧水平位移，随后保险仍会拼命推，禁止焊死不管。
            if (allowedAlong + 1e-4f >= absDx && footEmbeddedInObstacle && hitCount == 0 && !_hasLastFreeVillagePose)
            {
                allowedAlong = 0f;
            }

            if (allowedAlong + 1e-4f >= absDx)
            {
                return;
            }

            float newVx = Mathf.Sign(vx) * (allowedAlong / dt);
            _playerRootRb2D.velocity = new Vector2(newVx, _playerRootRb2D.velocity.y);
            var move = PlayerLogic.componentSystem.GetComponent<PlayerMoveComponent>();
            if (move != null)
            {
                move.moveSpeedX = newVx;
            }

            LogVillageObstacleDepth($"horizontal vx clamp {vx:F3}->{newVx:F3} allowedDx={allowedAlong:F4}");
        }

        /// <summary>
        /// 进村时订阅 <see cref="MoveComponent.onTurnAction"/>；仅村庄、且本组件启用障碍护栏时注册，避免影响战斗或其它场景。
        /// </summary>
        private void RegisterVillageTurnObstacleGuard()
        {
            if (!enableVillageObstacleTurnImmediateHorizontalClear || PlayerLogic == null)
            {
                return;
            }

            if (!enableVillageDepthObstacleClamp || !enableVillageHorizontalObstacleClamp)
            {
                return;
            }

            var move = PlayerLogic.componentSystem.GetComponent<PlayerMoveComponent>();
            if (move == null)
            {
                return;
            }

            if (_villageTurnGuardSubscribedMove == move)
            {
                return;
            }

            UnregisterVillageTurnObstacleGuard();
            move.onTurnAction += OnMoveTurnActionForVillageWalkObstacleImmediateBlock;
            _villageTurnGuardSubscribedMove = move;
        }

        /// <summary>
        /// 离村或销毁时退订，避免 Move 回调指向已卸载实体。
        /// </summary>
        private void UnregisterVillageTurnObstacleGuard()
        {
            if (_villageTurnGuardSubscribedMove == null)
            {
                return;
            }

            _villageTurnGuardSubscribedMove.onTurnAction -= OnMoveTurnActionForVillageWalkObstacleImmediateBlock;
            _villageTurnGuardSubscribedMove = null;
        }

        /// <summary>
        /// Turn 与 SetRunSpeed 同在 Update 栈内执行完毕后再判定；若应阻挡则立刻清水平速度并做一次脚底分离（仍在本组件内，不改 Move 源码）。
        /// </summary>
        private void OnMoveTurnActionForVillageWalkObstacleImmediateBlock(Vector2 newDirV2)
        {
            if (!enabled || PlayerLogic == null || !PlayerLogic.AllowControl)
            {
                return;
            }

            if (!enableVillageObstacleTurnImmediateHorizontalClear
                || !enableVillageDepthObstacleClamp
                || !enableVillageHorizontalObstacleClamp)
            {
                return;
            }

            var input = PlayerLogic.componentSystem.GetComponent<PlayerInputComponent>();
            if (input == null || input.LocomotionMode != PlayerLocomotionMode.Village2_5D)
            {
                return;
            }

            int obstacleLayer = LayerMask.NameToLayer(LayerName.VillageWalkObstacle);
            if (obstacleLayer < 0)
            {
                return;
            }

            if (_playerRootRb2D == null)
            {
                _playerRootRb2D = PlayerLogic.gameObject.GetComponent<Rigidbody2D>();
            }

            if (_playerRootRb2D == null)
            {
                return;
            }

            Collider2D foot = ResolveVillageDepthFootProbe();
            BuildVillageObstacleContactFilter(obstacleLayer);

            if (!VillageWalkObstacleTurnImmediateBlock.TryShouldClearHorizontalAfterTurn(
                    foot,
                    obstacleLayer,
                    _villageObstacleContactFilter,
                    newDirV2,
                    _villageObstacleOverlapBuffer,
                    _villageObstacleCastHits,
                    villageObstaclePostTurnProbeDistance,
                    villageObstacleCastPadding,
                    villageObstacleContactSkin,
                    villageObstaclePostTurnBlockClearance))
            {
                return;
            }

            Vector2 v = _playerRootRb2D.velocity;
            _playerRootRb2D.velocity = new Vector2(0f, v.y);
            var move = PlayerLogic.componentSystem.GetComponent<PlayerMoveComponent>();
            if (move != null)
            {
                move.moveSpeedX = 0f;
            }

            Physics2D.SyncTransforms();
            // 旋转后子级 Foot 可能瞬移进 Trigger，与 FixedUpdate 末尾分离互补。
            ApplyVillageWalkObstacleFootPenetrationSeparation();
            LogVillageObstacleDepth($"turn immediate horizontal clear (DirV2={newDirV2})");
        }

        private void OnDestroy()
        {
            UnregisterVillageTurnObstacleGuard();
        }

        /// <summary>在子物体上解析 PlayerFoot 层 Collider；可用 Inspector 覆盖。</summary>
        private Collider2D ResolveVillageDepthFootProbe()
        {
            if (villageDepthFootProbeOverride != null)
            {
                return villageDepthFootProbeOverride;
            }

            if (_villageDepthFootProbeSearchFailed)
            {
                return null;
            }

            if (_villageDepthFootProbeResolved != null)
            {
                return _villageDepthFootProbeResolved;
            }

            int footLayer = LayerMask.NameToLayer(LayerName.PlayerFoot);
            if (footLayer < 0)
            {
                _villageDepthFootProbeSearchFailed = true;
                return null;
            }

            Collider2D[] cols = PlayerLogic.GetComponentsInChildren<Collider2D>(true);
            // 取「包围盒面积最小」的 PlayerFoot 碰撞体：子层级若存在多个同层探针，首个遍历顺序不稳定且可能误选「过宽」体积导致纵深 Cast 提前命中（与 Scene 直觉不符）。
            Collider2D best = null;
            float bestArea = float.MaxValue;
            for (int i = 0; i < cols.Length; i++)
            {
                Collider2D c = cols[i];
                if (c == null || !c.enabled || c.gameObject.layer != footLayer)
                {
                    continue;
                }

                Bounds b = c.bounds;
                float area = b.size.x * b.size.y;
                if (area < bestArea - 1e-8f)
                {
                    bestArea = area;
                    best = c;
                }
            }

            if (best != null)
            {
                _villageDepthFootProbeResolved = best;
                return best;
            }

            _villageDepthFootProbeSearchFailed = true;
            Debug.LogWarning(
                $"[VillageBlockerDepth] 未在「{PlayerLogic.name}」子层级找到 Layer={LayerName.PlayerFoot} 的 Collider2D，纵深障碍夹紧已跳过。请在 Inspector 指定 villageDepthFootProbeOverride。",
                this);
            return null;
        }

        /// <summary>
        /// 从脚底包围盒底边附近沿纵深方向射线，得到本帧允许沿 castDir 移动的标量上限（世界单位）。
        /// <para><b>原因</b>：整颗 Foot 形状 <see cref="Collider2D.Cast"/> 沿世界 Y 运动时，易在斜栅栏/台阶的侧棱上先产生「刷边」命中，表现为线框外已不能走。</para>
        /// <para><b>替代方案</b>：在 Prefab 上为纵深单独挂更小 CircleCollider2D 并指定 <see cref="villageDepthFootProbeOverride"/>；本射线为不改资源的折中。</para>
        /// </summary>
        private float ComputeVillageObstacleAllowedAlongFromFootBottomRay(
            Collider2D foot,
            Vector2 castDir,
            float absDy,
            float castDist,
            int obstacleLayer,
            out string firstHitColliderName)
        {
            firstHitColliderName = null;
            Bounds b = foot.bounds;
            // 从底边略抬高，减少起点落在共面/壳层外时的伪命中；抬高量随 Foot 高度略调。
            float lift = Mathf.Clamp(b.size.y * 0.08f, 0.005f, 0.06f);
            Vector2 origin = new Vector2(b.center.x, b.min.y + lift);
            int n = Physics2D.Raycast(origin, castDir, _villageObstacleContactFilter, _villageObstacleRaycastHits, castDist);
            float allowedAlong = absDy;
            float stopSkin = Mathf.Max(0f, villageObstacleContactSkin);
            for (int i = 0; i < n; i++)
            {
                RaycastHit2D h = _villageObstacleRaycastHits[i];
                if (h.collider == null)
                {
                    continue;
                }

                if (h.collider.gameObject.layer != obstacleLayer)
                {
                    continue;
                }

                float d = h.distance;
                // 与横移 Cast 同理：d≈0 表示已贴障碍边界，必须参与夹紧，不可跳过。
                if (d < 0f)
                {
                    allowedAlong = Mathf.Min(allowedAlong, 0f);
                    if (firstHitColliderName == null)
                    {
                        firstHitColliderName = h.collider.name;
                    }

                    continue;
                }

                if (firstHitColliderName == null)
                {
                    firstHitColliderName = h.collider.name;
                }

                allowedAlong = Mathf.Min(allowedAlong, Mathf.Max(0f, d - stopSkin));
            }

            return allowedAlong;
        }

        /// <summary>整颗 Foot 形状沿纵深 Cast；与射线模式二选一用于「运动段」阻挡（嵌入段仍用 OverlapCollider）。</summary>
        private float ComputeVillageObstacleAllowedAlongFromFootShapeCast(
            Collider2D foot,
            Vector2 castDir,
            float absDy,
            float castDist,
            int obstacleLayer,
            out string firstHitColliderName)
        {
            firstHitColliderName = null;
            _villageObstacleCastHits.Clear();
            int hitCount = foot.Cast(castDir, _villageObstacleContactFilter, _villageObstacleCastHits, castDist);
            float allowedAlong = absDy;
            float stopSkin = Mathf.Max(0f, villageObstacleContactSkin);
            for (int i = 0; i < hitCount; i++)
            {
                RaycastHit2D h = _villageObstacleCastHits[i];
                if (h.collider == null)
                {
                    continue;
                }

                if (h.collider.gameObject.layer != obstacleLayer)
                {
                    continue;
                }

                float d = h.distance;
                // 起点与障碍壳共面或略嵌入时 d 可为 0：旧逻辑跳过会导致纵深仍按整段 dy 积分 → 与横移组合挤压时易穿出。
                if (d < 0f)
                {
                    allowedAlong = Mathf.Min(allowedAlong, 0f);
                    if (firstHitColliderName == null)
                    {
                        firstHitColliderName = h.collider.name;
                    }

                    continue;
                }

                if (firstHitColliderName == null)
                {
                    firstHitColliderName = h.collider.name;
                }

                allowedAlong = Mathf.Min(allowedAlong, Mathf.Max(0f, d - stopSkin));
            }

            return allowedAlong;
        }

        /// <summary>临时将权威 Y 写回刚体与 Transform，供 Overlap 查询；随后恢复，调用方须在同帧内完成。</summary>
        private bool IsFootOverlappingWalkObstaclesAtAuthoritativeY(float rootWorldY)
        {
            Collider2D foot = ResolveVillageDepthFootProbe();
            if (foot == null || _playerRootRb2D == null || PlayerLogic == null)
            {
                return false;
            }

            int obstacleLayer = LayerMask.NameToLayer(LayerName.VillageWalkObstacle);
            if (obstacleLayer < 0)
            {
                return false;
            }

            BuildVillageObstacleContactFilter(obstacleLayer);

            Vector2 savedRb = _playerRootRb2D.position;
            Vector3 savedRoot = PlayerLogic.transform.position;
            _playerRootRb2D.position = new Vector2(savedRb.x, rootWorldY);
            PlayerLogic.transform.position = new Vector3(savedRb.x, rootWorldY, _frozenWorldZ);
            Physics2D.SyncTransforms();

            _villageObstacleOverlapBuffer.Clear();
            int n = foot.OverlapCollider(_villageObstacleContactFilter, _villageObstacleOverlapBuffer);

            _playerRootRb2D.position = savedRb;
            PlayerLogic.transform.position = savedRoot;
            Physics2D.SyncTransforms();

            return n > 0;
        }

        /// <summary>沿 +Y 运动：<paramref name="yFreeLow"/> 无重叠、<paramref name="yBlockedHigh"/> 有重叠，二分求区间内最大自由权威 Y。</summary>
        private float BinarySearchLastFreeWhenMovingUp(float yFreeLow, float yBlockedHigh)
        {
            float lo = yFreeLow;
            float hi = yBlockedHigh;
            for (int i = 0; i < 22; i++)
            {
                float mid = (lo + hi) * 0.5f;
                if (IsFootOverlappingWalkObstaclesAtAuthoritativeY(mid))
                {
                    hi = mid;
                }
                else
                {
                    lo = mid;
                }
            }

            return lo;
        }

        /// <summary>沿 −Y 运动：<paramref name="yBlockedLow"/> 有重叠、<paramref name="yFreeHigh"/> 无重叠，二分求区间内最大自由权威 Y（即最靠下的安全位置）。</summary>
        private float BinarySearchLastFreeWhenMovingDown(float yBlockedLow, float yFreeHigh)
        {
            float lo = yBlockedLow;
            float hi = yFreeHigh;
            for (int i = 0; i < 22; i++)
            {
                float mid = (lo + hi) * 0.5f;
                if (IsFootOverlappingWalkObstaclesAtAuthoritativeY(mid))
                {
                    lo = mid;
                }
                else
                {
                    hi = mid;
                }
            }

            return hi;
        }

        /// <summary>从嵌入状态沿 Y 小步探测空闲位置；优先保持接近 <paramref name="referenceY"/>。</summary>
        private bool TryDepenetrateFootFromWalkObstacles(ref float authoritativeY, float referenceY)
        {
            const float step = 0.04f;
            const int maxSteps = 24;

            for (int k = 0; k <= maxSteps; k++)
            {
                for (int s = -1; s <= 1; s += 2)
                {
                    if (k == 0 && s < 0)
                    {
                        continue;
                    }

                    float tryY = referenceY + step * k * s;
                    if (!IsFootOverlappingWalkObstaclesAtAuthoritativeY(tryY))
                    {
                        authoritativeY = tryY;
                        return true;
                    }
                }
            }

            return false;
        }

        private void LogVillageObstacleDepth(string message)
        {
            if (!villageObstacleDepthDebugLog)
            {
                return;
            }

            Debug.Log($"[VillageBlockerDepth] {message}", this);
        }
    }
}
