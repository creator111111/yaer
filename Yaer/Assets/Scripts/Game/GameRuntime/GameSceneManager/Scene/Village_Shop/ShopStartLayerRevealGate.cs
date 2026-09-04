namespace Game.GameRuntime.GameSceneManager.Scene.Village_Shop
{
    /// <summary>
    /// 商店首次进店分层闸门：换场黑幕淡出完成（店背景可见）后置位。
    /// Prefab 前奏 <see cref="Game.GameRuntime.Story.Node.WaitShopStartBgRevealActionTask"/> 等待此标志后再空拍。
    /// <para>默认 true：非 DeferCover 路径不 Reset，避免 DialogDebug 永久等待。</para>
    /// </summary>
    public static class ShopStartLayerRevealGate
    {
        /// <summary>黑幕已淡完、店背景可见。默认 true 以免非旁路对话卡住。</summary>
        public static bool IsBgFullyVisible { get; private set; } = true;

        /// <summary>首次进店 DeferCover Trigger 前调用：锁闸，等 CloseFormFade 完成再开。</summary>
        public static void ResetForDeferredCover()
        {
            IsBgFullyVisible = false;
        }

        /// <summary>换场黑幕 HideFade 完成回调里调用。</summary>
        public static void SignalBgFullyVisible()
        {
            IsBgFullyVisible = true;
        }
    }
}
