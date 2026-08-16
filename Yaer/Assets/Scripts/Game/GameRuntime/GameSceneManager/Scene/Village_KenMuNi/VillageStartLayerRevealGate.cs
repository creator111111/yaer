namespace Game.GameRuntime.GameSceneManager.Scene.Village_KenMuNi
{
    /// <summary>
    /// 村开场分层闸门：黑幕淡出完成（玩家已能看见对话 Prefab 全屏 BG）后置位。
    /// Prefab 前奏 <c>WaitVillageStartBgRevealActionTask</c> 等待此标志后再空拍出立绘。
    /// <para>默认 true：DialogDebug / 非进村旁路不 Reset，开场只跑 Hold，不永久等待。</para>
    /// </summary>
    public static class VillageStartLayerRevealGate
    {
        /// <summary>黑幕已淡完、BG 可见。默认 true 以免非旁路对话卡住。</summary>
        public static bool IsBgFullyVisible { get; private set; } = true;

        /// <summary>进村旁路 Trigger 前调用：锁闸，等 CloseFormFade 完成再开。</summary>
        public static void ResetForDeferredCover()
        {
            IsBgFullyVisible = false;
        }

        /// <summary>黑幕 HideFade 完成回调里调用。</summary>
        public static void SignalBgFullyVisible()
        {
            IsBgFullyVisible = true;
        }
    }
}
