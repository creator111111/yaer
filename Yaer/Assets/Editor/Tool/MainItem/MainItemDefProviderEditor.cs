#if UNITY_EDITOR
using System.Text;
using Game.DataTable.MainItem;
using UnityEditor;
using UnityEngine;

namespace Game.DataTable.MainItem.Editor
{
    /// <summary>
    /// DB-0 验收：打印商店购买/出售候选行数（预期 Buy=7 Sell=3）。
    /// 仅读 MainItemDatabase，不修改场景、不触发 Shop Scroll 施工菜单。
    /// </summary>
    public static class MainItemDefProviderEditor
    {
        private const string MenuPath = "Tools/MainItem/Debug Print Shop Candidates";
        private const int ExpectedBuyCount = 7;
        private const int ExpectedSellCount = 3;

        [MenuItem(MenuPath)]
        private static void DebugPrintShopCandidates()
        {
            MainItemDefProvider.EnsureLoaded();
            var buy = MainItemDefProvider.GetShopBuyCandidates();
            var sell = MainItemDefProvider.GetShopSellCandidates();

            var log = new StringBuilder();
            log.AppendLine($"[MainItemDefProvider] Shop Buy={buy.Count} Sell={sell.Count}");

            if (buy.Count == 0 && sell.Count == 0)
            {
                log.AppendLine("  提示：候选为空。请先执行 Tools → MainItem → Import Database From JSON。");
            }
            else
            {
                log.Append("  Buy: ");
                AppendCandidateNames(log, buy);
                log.AppendLine();
                log.Append("  Sell: ");
                AppendCandidateNames(log, sell);
            }

            if (buy.Count != ExpectedBuyCount || sell.Count != ExpectedSellCount)
            {
                log.AppendLine(
                    $"  预期 Buy={ExpectedBuyCount} Sell={ExpectedSellCount}；请核对 MainItemDatabase 的 itemType / buyPrice / sellPrice。");
                Debug.LogWarning(log.ToString());
                return;
            }

            Debug.Log(log.ToString());
        }

        private static void AppendCandidateNames(StringBuilder log, System.Collections.Generic.IReadOnlyList<MainItemDef> candidates)
        {
            if (candidates == null || candidates.Count == 0)
            {
                log.Append("(none)");
                return;
            }

            for (var i = 0; i < candidates.Count; i++)
            {
                if (i > 0)
                {
                    log.Append(", ");
                }

                var def = candidates[i];
                log.Append(def != null ? def.ItemId.ToString() : "null");
            }
        }
    }
}
#endif
