using System.Collections.Generic;
using Game.Static.Name.Settings;
using UnityEngine;

namespace Game.GameRuntime.Entities.Player.Components
{
    /// <summary>
    /// 村庄 Walk 障碍（方案 1）在 <b>转身当帧</b> 的纯几何判定：不依赖 FixedUpdate，也不读写 <see cref="MoveComponent"/> 以外状态。
    /// <para>
    /// <b>背景</b>：<see cref="MoveComponent.TurnLeft"/>/<see cref="MoveComponent.TurnRight"/> 在 <b>Update</b> 里触发，
    /// 而 <see cref="TownPlayerLocomotion"/> 的 Cast 夹紧在 <b>FixedUpdate</b> 末尾，中间至少隔一次物理积分，
    /// 易出现「SetRunSpeed + Turn 后整段 vx 已穿出 Trigger」的帧缝。
    /// </para>
    /// <para>
    /// <b>用法</b>：由 <see cref="TownPlayerLocomotion"/> 订阅 <see cref="MoveComponent.onTurnAction"/>，在回调里调用本类的静态方法；
    /// 若返回 true，由订阅方将 <c>velocity.x</c> 与 <c>moveSpeedX</c> 置零并可再跑一次脚底分离（仍留在 Town 内，避免污染通用移动代码）。
    /// </para>
    /// <para><b>替代方案</b>：改 Move 全局 Turn 语义或上 CCD 子步进；影响面大，故采用事件 + 纯查询解耦。</para>
    /// </summary>
    public static class VillageWalkObstacleTurnImmediateBlock
    {
        /// <summary>
        /// 判定转身后是否应立即清空水平速度：脚底已与障碍层重叠，或沿 <paramref name="newFacingWorld"/> 的水平分量短扫即进入「贴壳」距离内。
        /// </summary>
        /// <param name="foot">PlayerFoot 层探针，与村庄纵深/横移护栏同源。</param>
        /// <param name="villageWalkObstacleLayer"><see cref="LayerName.VillageWalkObstacle"/> 解析结果，无效则 false。</param>
        /// <param name="obstacleFilter">须已设 <c>useTriggers=true</c> 且 LayerMask 仅含障碍层。</param>
        /// <param name="newFacingWorld"><see cref="MoveComponent.onTurnAction"/> 传入的 <see cref="MoveComponent.DirV2"/>（世界左右）。</param>
        /// <param name="overlapScratch">复用缓冲，由调用方持有以避免 GC。</param>
        /// <param name="castScratch">Cast 结果缓冲。</param>
        /// <param name="probeDistance">沿新面向短扫的世界长度（略大于典型 |vx|·dt）。</param>
        /// <param name="castPadding">与村庄护栏一致的 Cast 加长量。</param>
        /// <param name="contactSkin">与村庄护栏一致的贴壳扣减。</param>
        /// <param name="blockIfClearanceBelow">沿面向的「可通行余量」低于此值（世界单位）则视为应清空水平速度。</param>
        /// <returns>若应清空水平速度则 true。</returns>
        public static bool TryShouldClearHorizontalAfterTurn(
            Collider2D foot,
            int villageWalkObstacleLayer,
            in ContactFilter2D obstacleFilter,
            Vector2 newFacingWorld,
            List<Collider2D> overlapScratch,
            List<RaycastHit2D> castScratch,
            float probeDistance,
            float castPadding,
            float contactSkin,
            float blockIfClearanceBelow)
        {
            if (foot == null || !foot.enabled || villageWalkObstacleLayer < 0)
            {
                return false;
            }

            Physics2D.SyncTransforms();

            overlapScratch.Clear();
            int overlapCount = foot.OverlapCollider(obstacleFilter, overlapScratch);
            for (int i = 0; i < overlapCount; i++)
            {
                Collider2D c = overlapScratch[i];
                if (c != null && c.gameObject.layer == villageWalkObstacleLayer)
                {
                    return true;
                }
            }

            // 2.5D 横移只看世界 X；DirV2 理论上为 ±(1,0)，此处防御性归一。
            Vector2 castDir = new Vector2(Mathf.Sign(newFacingWorld.x), 0f);
            if (castDir.sqrMagnitude < 1e-8f)
            {
                castDir = newFacingWorld.sqrMagnitude > 1e-8f ? newFacingWorld.normalized : Vector2.right;
                castDir.y = 0f;
                if (castDir.sqrMagnitude < 1e-8f)
                {
                    castDir = Vector2.right;
                }

                castDir.Normalize();
            }

            float skin = Mathf.Max(0f, contactSkin);
            float castDist = Mathf.Max(0.0001f, probeDistance) + Mathf.Max(0.0001f, castPadding);
            castScratch.Clear();
            int hitCount = foot.Cast(castDir, obstacleFilter, castScratch, castDist);
            float bestClearance = float.MaxValue;
            for (int i = 0; i < hitCount; i++)
            {
                RaycastHit2D h = castScratch[i];
                if (h.collider == null || h.collider.gameObject.layer != villageWalkObstacleLayer)
                {
                    continue;
                }

                float d = h.distance;
                if (d < 0f)
                {
                    bestClearance = 0f;
                    break;
                }

                float clearance = Mathf.Max(0f, d - skin);
                if (clearance < bestClearance)
                {
                    bestClearance = clearance;
                }
            }

            if (bestClearance == float.MaxValue)
            {
                return false;
            }

            return bestClearance < Mathf.Max(0.0001f, blockIfClearanceBelow);
        }
    }
}
