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
        /// 购买每次 Commit 必打一行（过滤 <c>[ShopBuyCommit]</c>）：
        /// 输入数量、单价、金币、持有、空位、可买力、上限、提交后数量、归零原因。
        /// 用于对拍「有 800 金为何变 0」——多数是 stackRoom=0，不是钱不够。
        /// </summary>
        public static void LogBuyCommitTrace(
            string itemId,
            int inputQty,
            int price,
            int gold,
            int otherCost,
            int held,
            int maxStack,
            int stackRoom,
            int afford,
            int maxQty,
            int afterQty,
            string reason)
        {
            Debug.Log(
                $"[ShopBuyCommit] item={itemId} " +
                $"输入数量={inputQty} 单价={price} 金币={gold} 其它行占金={otherCost} " +
                $"持有={held}/{maxStack} 空位stackRoom={stackRoom} 钱够买afford={afford} 上限max={maxQty} " +
                $"提交后数量={afterQty} 原因={reason}");
        }

        /// <summary>
        /// 输入/上限侧：有金但堆叠已满 → max=0。与「金币不足」区分，避免误判 UI/读金坏了。
        /// </summary>
        public static void LogBuyBlockedByFullStack(
            string itemId,
            int held,
            int maxStack,
            int gold,
            int price)
        {
            if (gold < 0)
            {
                Debug.LogWarning(
                    $"{LogPrefix} 堆叠已满无法再买：{itemId} 持有 {held}/{maxStack}（stackRoom=0）；输入钳 0 属业务预期");
                return;
            }

            Debug.LogWarning(
                $"{LogPrefix} 堆叠已满无法再买：{itemId} 持有 {held}/{maxStack}（stackRoom=0）；" +
                $"当前金币 {gold}、单价 {price} 仍够买，但空位为 0 → 输入变 0 属业务预期，非读金失败");
        }

        /// <summary>Commit：键入被钳因堆叠已满（Tips 键未齐时 Console 可辨）。</summary>
        public static void LogBuyCommitClampedStackFull(
            string itemId, int held, int maxStack, int beforeQty, int afterQty)
        {
            Debug.LogWarning(
                $"{LogPrefix} 提交后数量调整：{itemId} 该道具已满（持有 {held}/{maxStack}），" +
                $"输入 {beforeQty} → {afterQty}");
        }

        /// <summary>Commit：键入被钳因金币不足。</summary>
        public static void LogBuyCommitClampedInsufficientGold(
            string itemId, int gold, int price, int beforeQty, int afterQty)
        {
            Debug.LogWarning(
                $"{LogPrefix} 提交后数量调整：{itemId} 金币不足（持有金 {gold}、单价 {price}），" +
                $"输入 {beforeQty} → {afterQty}");
        }

        /// <summary>Commit：键入超过联合可买上限（有空位且有金，多行占金等）。</summary>
        public static void LogBuyCommitClampedToMax(
            string itemId, int beforeQty, int afterQty, int maxQty)
        {
            Debug.LogWarning(
                $"{LogPrefix} 提交后数量调整：{itemId} 超过可买上限 {maxQty}，输入 {beforeQty} → {afterQty}");
        }

        /// <summary>出售 Commit：超过持有钳回。</summary>
        public static void LogSellCommitClampedToHeld(
            string itemId, int beforeQty, int afterQty, int held)
        {
            Debug.LogWarning(
                $"{LogPrefix} 提交后数量调整：{itemId} 超过持有 {held}，输入 {beforeQty} → {afterQty}");
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
