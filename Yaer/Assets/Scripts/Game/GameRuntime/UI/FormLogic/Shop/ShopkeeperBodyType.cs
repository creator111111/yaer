namespace Game.GameRuntime.UI.FormLogic.Shop
{
    /// <summary>
    /// 商店老板娘身体变体 ID（全身 Sprite，脸层仍叠在上方独立 SR）。
    /// </summary>
    public enum ShopkeeperBodyType
    {
        /// <summary>默认身 · GO <c>Body/Normal</c> · 源图 <c>正常体.png</c></summary>
        Normal = 0,

        /// <summary>脸红 · GO <c>Body/Red</c> · CSV 列写 <c>Red</c></summary>
        Blush = 1,

        /// <summary>阴险 · GO <c>Body/YinXian</c> · CSV 列写 <c>YinXian</c></summary>
        Sinister = 2,
    }
}
