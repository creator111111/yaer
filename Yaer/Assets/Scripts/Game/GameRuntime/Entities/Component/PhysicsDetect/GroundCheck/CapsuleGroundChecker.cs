using UnityEngine;

namespace Game.GameRuntime.Entities.Component.PhysicsDetect
{
    /// <summary>
    /// 战斗/平台「是否落地」检测（供 <see cref="Move.MoveComponent"/> 决定是否施加自定义重力）。
    /// </summary>
    /// <remarks>
    /// <b>解耦约定（相对村庄 DNF）</b>：
    /// <list type="bullet">
    /// <item><see cref="Game.GameRuntime.Entities.Player.Components.TownPlayerLocomotion"/> 负责村内纵深 Y，<b>不得</b>改本类，也勿在本类里引用村庄组件。</item>
    /// <item>本类只回答「脚底下方是否有地面层碰撞」；村内仍依赖 Prefab 的 <see cref="BaseGroundChecker.GroundLayerMask"/> 对准真正的地面层（如 GroundCenter），不要把 Default 上的墙体塞进 Mask。</item>
    /// <item>旧实现用「水平 CapsuleCast + distance 0」做旁侧重叠，会把 Forest/Village 的 LeftWall/RightWall（Default、高盒）误判为落地，导致半空 IsGrounded=true、重力不跑、Velocity.y 卡在跳跃初速。</item>
    /// </list>
    /// 替代方案：直接换挂 <see cref="SimpleRaycastGroundChecker"/>；不采纳「在 TownPlayerLocomotion 里重写 IsGrounded」（会把 DNF 与战斗落地耦死）。
    /// </remarks>
    public class CapsuleGroundChecker : BaseGroundChecker
    {
        [SerializeField]
        private Vector3 GroundCheckOffset;

        [SerializeField]
        private float CapsuleHeight;

        [SerializeField]
        private float CapsuleRadius;

        [SerializeField]
        private CapsuleDirection2D CapsuleDirection;

        /// <summary>
        /// 脚下向下探测距离。过小可能站地抖；过大易提前「贴地」。默认按胶囊半径推导，可在 Inspector 覆盖。
        /// </summary>
        [SerializeField]
        [Tooltip("仅向下探测的最大距离；0 表示用 CapsuleRadius*2 与 0.25 的较大值。")]
        private float groundProbeDownDistance = 0f;

        private Vector3 CapsuleCenter
        {
            get
            {
                return Root.position + Root.rotation * GroundCheckOffset;
            }
        }

        private Vector2 CapsuleSize;

        /// <summary>FixedUpdate 高频调用，复用缓冲避免每帧 new 数组。</summary>
        private readonly RaycastHit2D[] _groundHits = new RaycastHit2D[1];

        private Vector3 Sphere1Center
        {
            get
            {
                if (CapsuleDirection == CapsuleDirection2D.Horizontal)
                {
                    return CapsuleCenter - 0.5f * new Vector3(CapsuleHeight, 0);
                }

                return CapsuleCenter + 0.5f * new Vector3(0, CapsuleHeight);
            }
        }

        private Vector3 Sphere2Center
        {
            get
            {
                if (CapsuleDirection == CapsuleDirection2D.Horizontal)
                {
                    return CapsuleCenter + 0.5f * new Vector3(CapsuleHeight, 0);
                }

                return CapsuleCenter - 0.5f * new Vector3(0, CapsuleHeight);
            }
        }

        private float ResolvedProbeDownDistance
        {
            get
            {
                if (groundProbeDownDistance > 0f)
                {
                    return groundProbeDownDistance;
                }

                return Mathf.Max(CapsuleRadius * 2f, 0.25f);
            }
        }

        private void Awake()
        {
            CapsuleSize = new Vector2(CapsuleHeight, CapsuleRadius);
        }

        public override bool GroundCheck()
        {
            if (Root == null)
            {
                return false;
            }

            // 只向下：与 SimpleRaycastGroundChecker 语义对齐，避免旁侧高墙进入 GroundLayerMask 时被当成地面。
            // useTriggers=false：门/剧情 Trigger 即使误在地面相关层也不应支撑角色。
            float distance = ResolvedProbeDownDistance;
            var filter = new ContactFilter2D
            {
                useTriggers = false,
                useLayerMask = true,
                useDepth = false
            };
            filter.SetLayerMask(GroundLayerMask);

            int count = Physics2D.Raycast(CapsuleCenter, Vector2.down, filter, _groundHits, distance);
            return count > 0 && _groundHits[0].collider != null;
        }

        protected override void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(Sphere1Center, CapsuleRadius);
            Gizmos.DrawSphere(Sphere2Center, CapsuleRadius);
            if (CapsuleDirection == CapsuleDirection2D.Horizontal)
            {
                Gizmos.DrawCube(CapsuleCenter, new Vector3(CapsuleHeight, 2 * CapsuleRadius));
            }
            else
            {
                Gizmos.DrawCube(CapsuleCenter, new Vector3(2 * CapsuleRadius, CapsuleHeight));
            }

            // 实际用于判定的向下射线
            Gizmos.color = Color.yellow;
            Vector3 origin = CapsuleCenter;
            Gizmos.DrawLine(origin, origin + Vector3.down * ResolvedProbeDownDistance);
        }
    }
}
