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

        /// <summary>
        /// 合层大立绘恢复默认 <see cref="ShopkeeperBodyType.Normal"/> + <see cref="ShopkeeperFaceType.Face1"/>。
        /// </summary>
        /// <remarks>
        /// 原因：对白句会 Apply 成 Red/Face5 等，结束若不复位，Idle 买卖会残留末句身脸（0828 方案 A）。
        /// 替代方案：结束保留末句脸——违背产品 Idle 默认，不采用。
        /// Mask 小表情 Idle 不显示，结束复位非必须（P2）；本 API 只转发给合层 Controller。
        /// </remarks>
        public static void ResetDefault()
        {
            if (Instance != null)
            {
                Instance.ResetDefault();
            }
        }
    }
}
