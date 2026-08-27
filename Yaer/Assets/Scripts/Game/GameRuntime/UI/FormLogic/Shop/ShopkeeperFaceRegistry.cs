namespace Game.GameRuntime.UI.FormLogic.Shop
{
    /// <summary>
    /// 轻量注册表：供下期对白桥接（<c>ShopkeeperDialogueFaceBridge</c>）查找当前场景的切脸器。
    /// </summary>
    /// <remarks>
    /// 本期仅注册/注销；不驱动业务。多实例时以最后一次 <see cref="Register"/> 为准（商店场景通常唯一）。
    /// </remarks>
    public static class ShopkeeperFaceRegistry
    {
        /// <summary>当前活跃的老板娘切脸器；进店合层 Awake 时注册。</summary>
        public static ShopkeeperFaceController Instance { get; private set; }

        /// <summary>注册切脸器（合层根 Awake 调用）。</summary>
        public static void Register(ShopkeeperFaceController controller)
        {
            Instance = controller;
        }

        /// <summary>注销切脸器（OnDestroy 调用，避免跨场景残留）。</summary>
        public static void Unregister(ShopkeeperFaceController controller)
        {
            if (Instance == controller)
            {
                Instance = null;
            }
        }
    }
}
