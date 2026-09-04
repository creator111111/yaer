namespace Game.GameRuntime.UI.FormLogic.Shop
{
    /// <summary>
    /// 商店老板娘脸部表情 ID（对应磁盘 <c>表情1.png</c>～<c>表情5.png</c>）。
    /// </summary>
    /// <remarks>
    /// 与对话系统 <see cref="Game.Static.Enum.Dialogue.DialogueFaceType"/> 独立；
    /// 台本语义名（Smile/Angry 等）下期由 <c>ShopkeeperFaceMapper</c> 映射，本期仅用 Face1～5。
    /// </remarks>
    public enum ShopkeeperFaceType
    {
        /// <summary>默认脸 · 源图 <c>表情1.png</c></summary>
        Face1 = 0,

        /// <summary>源图 <c>表情2.png</c></summary>
        Face2 = 1,

        /// <summary>源图 <c>表情3.png</c></summary>
        Face3 = 2,

        /// <summary>源图 <c>表情4.png</c></summary>
        Face4 = 3,

        /// <summary>源图 <c>表情5.png</c></summary>
        Face5 = 4,
    }
}
