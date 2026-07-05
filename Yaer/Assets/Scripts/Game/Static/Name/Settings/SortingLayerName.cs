namespace Game.Static.Name.Settings
{
    /// <summary>
    /// Sprite 的 Sorting Layer 名称常量（须与 <c>ProjectSettings/TagManager.asset</c> 中 <c>m_SortingLayers</c> 完全一致）。
    /// <para>与 <see cref="LayerName"/> 区分：后者多为 Unity 物理 Layer，本类仅用于 <see cref="UnityEngine.SpriteRenderer.sortingLayerName"/>。</para>
    /// </summary>
    public static class SortingLayerName
    {
        /// <summary>列表靠前，相对 Player / SceneObject 通常更靠后绘制（以项目 TagManager 顺序为准）。</summary>
        public const string Default = "Default";

        /// <summary>玩家角色默认绘制层（与 TagManager → Sprite Sorting Layers 中名称一致）；村庄 DepthZone 离开特殊区后恢复至此层。</summary>
        public const string Player = "Player";

        /// <summary>列表中在 Player 之后，相对 Default 更靠前绘制；村庄遮挡任务中与「物体盖住玩家」状态对应。</summary>
        public const string SceneObject = "SceneObject";
    }
}
