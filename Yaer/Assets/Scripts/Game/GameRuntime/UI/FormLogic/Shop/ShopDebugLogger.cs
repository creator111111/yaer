using UnityEngine;

namespace Game.GameRuntime.UI.FormLogic.Shop
{
    /// <summary>
    /// 阶段四「假购买」专用 Console 输出。
    /// 前缀 <see cref="LogPrefix"/> 固定为 [ShopDebug]，便于验收时在 Console 过滤。
    /// 阶段五接入真实扣款后，可保留 Log 或改为正式 Tips。
    /// </summary>
    public static class ShopDebugLogger
    {
        public const string LogPrefix = "[ShopDebug]";

        /// <summary>数量 &gt; 0 且总价 &gt; 0 时输出成功假购买日志。</summary>
        public static void LogHpBallPurchaseSuccess(int totalGold)
        {
            Debug.Log($"{LogPrefix} 成功购买生命球，扣除金币 {totalGold}");
        }

        /// <summary>数量为 0 或总价为 0 时不打成功 Log，仅警告。</summary>
        public static void LogZeroQuantityWarning()
        {
            Debug.LogWarning($"{LogPrefix} 数量为 0，无法购买");
        }
    }
}
