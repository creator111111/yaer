#if UNITY_EDITOR
using UnityEditor;

namespace Game.GameRuntime.Entities.Component.Physics.Editor
{
    /// <summary>
    /// 将「方案 1：VillageWalkObstacle 与所有层（含 PlayerFoot）均不碰撞」写入当前工程的 Physics 2D 设置（与运行时 <see cref="VillageWalkObstacleCollisionBootstrap"/> 一致）。
    /// </summary>
    public static class VillageWalkObstacleCollisionMatrixMenu
    {
        private const string MenuPath = "Yaer/Physics2D/应用村庄障碍层（方案1：障碍不与任何层碰撞）";

        [MenuItem(MenuPath)]
        private static void ApplyToProjectSettings()
        {
            VillageWalkObstacleCollisionBootstrap.ApplyPolicy();
            EditorUtility.DisplayDialog(
                "Physics2D",
                "已更新（方案 1）：Layer「VillageWalkObstacle」与所有层在 2D 中均不碰撞；阻挡由脚本 Cast/Overlap 实现。\n请保存工程以便提交 ProjectSettings。",
                "确定");
        }
    }
}
#endif
