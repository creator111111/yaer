namespace Game.Static.Name.Settings
{
    public class LayerName
    {
        /// <summary>玩家脚底/可走检测等碰撞体所在 Layer。村庄方案 1 下与 <see cref="VillageWalkObstacle"/> 在 2D 矩阵中<strong>不</strong>发生物理解算；阻挡由脚本查询障碍 Collider 几何实现。</summary>
        public const string PlayerFoot = "PlayerFoot";

        /// <summary>村庄 WalkArea 内障碍碰撞体所在 Layer；Collider 建议 <c>isTrigger=true</c> 作语义标注。2D 矩阵由 <see cref="Game.GameRuntime.Entities.Component.Physics.VillageWalkObstacleCollisionBootstrap"/> 设为与所有层 Ignore（方案 1）。</summary>
        public const string VillageWalkObstacle = "VillageWalkObstacle";

        public const string SceneObject = "SceneObject";
        public const string SceneObjectOther = "SceneObject_Other";
        public const string SceneObjectDepth = "SceneObject_Depth";
        public const string SceneObjectPlayer = "SceneObject_Player";
        public const string SceneObjectPhy1 = "SceneObject_Phy1";
        public const string SceneObjectPhy2 = "SceneObject_Phy2";
        public const string SceneObjectPhy3 = "SceneObject_Phy3";
        public const string SceneObjectPhy4 = "SceneObject_Phy4";
        public const string SceneObjectPhy5 = "SceneObject_Phy5";
        public const string SceneObjectPhy6 = "SceneObject_Phy6";
    }
}