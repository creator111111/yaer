using Game.Static.Name.Settings;
using UnityEngine;

namespace Game.GameRuntime.Entities.Component.Physics
{
    /// <summary>
    /// 村庄 Walk 障碍层 <see cref="LayerName.VillageWalkObstacle"/> 的 Physics 2D 矩阵策略（<b>方案 1</b>，执行文档 0514）。
    /// <para>
    /// <b>当前策略</b>：障碍层与<strong>所有</strong>层（含 <see cref="LayerName.PlayerFoot"/>）均 <see cref="Physics2D.IgnoreLayerCollision"/>，
    /// 即<strong>不参与刚体接触求解</strong>；「挡不挡」由 <see cref="Game.GameRuntime.Entities.Player.Components.TownPlayerLocomotion"/> 等对障碍层做
    /// <c>Cast / Overlap</c> 的脚本逻辑负责。障碍 Collider 语义上应为 <c>isTrigger = true</c>，与矩阵双保险。
    /// </para>
    /// <para><b>替代方案</b>：在 Project Settings → Physics 2D 中手工维护矩阵并关闭本脚本；或仅在非村庄场景恢复旧「脚↔障碍硬碰」策略（须在离村时另行调用，见 OPEN_QUESTIONS）。</para>
    /// </summary>
    public static class VillageWalkObstacleCollisionBootstrap
    {
        /// <summary>每局开始前执行，打包与 Editor 进 Play 后矩阵一致。</summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void ApplyOnLoad()
        {
            ApplyPolicy();
        }

        /// <summary>
        /// 方案 1：障碍层与 0..31 层全部设为忽略碰撞，避免脚与障碍物理解算与脚本写回 <c>position</c> 抢控制权。
        /// </summary>
        public static void ApplyPolicy()
        {
            int obstacle = LayerMask.NameToLayer(LayerName.VillageWalkObstacle);
            if (obstacle < 0)
            {
                Debug.LogWarning(
                    $"[VillageWalkObstacle] 未找到 Layer「{LayerName.VillageWalkObstacle}」，跳过 2D 矩阵设置。请检查 TagManager。");
                return;
            }

            for (int i = 0; i < 32; i++)
            {
                Physics2D.IgnoreLayerCollision(obstacle, i, true);
            }
        }
    }
}
