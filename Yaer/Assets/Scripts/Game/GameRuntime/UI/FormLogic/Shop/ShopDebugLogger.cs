using System.Collections.Generic;
using System.Text;
using Game.Static.Enum.Goods;
using UnityEngine;

namespace Game.GameRuntime.UI.FormLogic.Shop
{
    /// <summary>
    /// 商店交易 Console 输出。
    /// 前缀 <see cref="LogPrefix"/> 固定为 [ShopDebug]，便于验收时在 Console 过滤。
    /// 正式 Tips 图集键未齐时，不足/成功均先走本类；有 Tips 键后再叠加 TipsForm。
    /// </summary>
    public static class ShopDebugLogger
    {
        public const string LogPrefix = "[ShopDebug]";

        /// <summary>
        /// 购买入包成功：文案以道具明细为准（SB-V2 对账用）；可附带合计标价 / 是否旁路未扣款。
        /// </summary>
        /// <param name="itemIds">成交行道具 ID</param>
        /// <param name="quantities">与 itemIds 等长的数量</param>
        /// <param name="totalGold">Σ(qty×单价)，旁路时仍打印标价便于对照 Total2</param>
        /// <param name="goldBypassed">true=联合验收旁路未走 TrySpendPlayerGold</param>
        public static void LogPurchaseIntoBag(
            IReadOnlyList<EMainItemName> itemIds,
            IReadOnlyList<int> quantities,
            int totalGold,
            bool goldBypassed)
        {
            var summary = BuildItemSummary(itemIds, quantities);
            if (goldBypassed)
            {
                Debug.Log(
                    $"{LogPrefix} 购买入包成功（货币旁路未扣款）：{summary}；合计标价 {totalGold}");
            }
            else
            {
                Debug.Log(
                    $"{LogPrefix} 购买入包成功：{summary}；扣除金币 {totalGold}");
            }
        }

        /// <summary>兼容旧调用：无明细时仅打扣款成功。</summary>
        public static void LogPurchaseSuccess(int totalGold)
        {
            Debug.Log($"{LogPrefix} 购买成功，扣除金币 {totalGold}");
        }

        /// <summary>兼容旧调用名；内部转 <see cref="LogPurchaseSuccess"/>。</summary>
        public static void LogHpBallPurchaseSuccess(int totalGold)
        {
            LogPurchaseSuccess(totalGold);
        }

        /// <summary>拼「SmallHpPotion×2, Fish×1」便于 Console 过滤对账。</summary>
        private static string BuildItemSummary(
            IReadOnlyList<EMainItemName> itemIds,
            IReadOnlyList<int> quantities)
        {
            if (itemIds == null || itemIds.Count == 0)
            {
                return "(无明细)";
            }

            var sb = new StringBuilder();
            for (var i = 0; i < itemIds.Count; i++)
            {
                if (i > 0)
                {
                    sb.Append(", ");
                }

                var qty = quantities != null && i < quantities.Count ? quantities[i] : 0;
                sb.Append(itemIds[i]);
                sb.Append('×');
                sb.Append(qty);
            }

            return sb.ToString();
        }

        /// <summary>数量为 0 或总价为 0 时不打成功 Log，仅警告（买卖共用）。</summary>
        public static void LogZeroQuantityWarning()
        {
            Debug.LogWarning($"{LogPrefix} 数量为 0，无法交易");
        }

        /// <summary>金币不足：整单失败，不扣款、不入包。</summary>
        public static void LogInsufficientGold(int need, int have)
        {
            Debug.LogWarning($"{LogPrefix} 金币不足，需要 {need}，当前持有 {have}");
        }

        /// <summary>堆叠将超上限：整单失败（预校验，避免扣款后道具被钳制）。</summary>
        public static void LogStackOverflow(string itemId, int held, int buyQty, int maxStack)
        {
            Debug.LogWarning(
                $"{LogPrefix} 背包将超堆叠上限：{itemId} 持有 {held} + 购买 {buyQty} > {maxStack}，整单取消");
        }

        /// <summary>
        /// 出售出包成功：明细 + 加币额，便于 Console 对账。
        /// </summary>
        /// <param name="itemIds">成交行道具 ID</param>
        /// <param name="quantities">与 itemIds 等长的数量</param>
        /// <param name="totalGold">Σ(qty×卖价)，与 Total2 同口径</param>
        public static void LogSellFromBag(
            IReadOnlyList<EMainItemName> itemIds,
            IReadOnlyList<int> quantities,
            int totalGold)
        {
            var summary = BuildItemSummary(itemIds, quantities);
            Debug.Log($"{LogPrefix} 出售出包成功：{summary}；获得金币 {totalGold}");
        }

        /// <summary>背包持有不足：整单失败，不扣包、不加币。</summary>
        public static void LogInsufficientBag(string itemId, int need, int held)
        {
            Debug.LogWarning(
                $"{LogPrefix} 背包不足，无法出售：{itemId} 需要 {need}，当前持有 {held}，整单取消");
        }

        /// <summary>预检通过后 TryRemove 仍失败（理想不应发生）。</summary>
        public static void LogSellRemoveFailed(string itemId, int qty)
        {
            Debug.LogError($"{LogPrefix} 出售扣包失败：{itemId}×{qty}（预检后仍失败，已中止加币）");
        }

        /// <summary>
        /// 历史占位：出售未接入时打这条。接入后正常路径不应再调用。
        /// 保留方法避免旧联调脚本编译失败。
        /// </summary>
        public static void LogSellNotImplemented()
        {
            Debug.Log($"{LogPrefix} 出售结算未接入");
        }

        /// <summary>读档入口不可用（无 Archive / 沙盒直开场景）。</summary>
        public static void LogArchiveUnavailable(string reason)
        {
            Debug.LogWarning($"{LogPrefix} 无法交易：{reason}");
        }
    }
}
