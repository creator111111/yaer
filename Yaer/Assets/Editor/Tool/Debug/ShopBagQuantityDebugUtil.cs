#if UNITY_EDITOR
using System.Collections.Generic;
using Game.DataTable.MainItem;
using Game.GameMgr;
using Game.GameMgr.Component;
using Game.GameMgr.Component.Archive;
using Game.GameMgr.Component.Archive.ArchiveDataClass.Player;
using Game.GameMgr.Component.PureMVC;
using Game.GameRuntime.UI.FormLogic.Menu;
using Game.Static.Enum.Goods;
using UnityEditor;
using UnityEngine;

namespace EditorC.Tool.DebugTools
{
    /// <summary>
    /// 商店货单背包数量调试共享逻辑：Buy∪Sell 候选、设为 N、清空、全满、Save。
    /// </summary>
    /// <remarks>
    /// 原因：禁止用「一键全部主道具」冒充清店货；列表必须以 Candidates API 动态为准。
    /// 每次改后必须 SaveSpcData（反例：MainItemAddAllBagMenu 未 Save）。
    /// </remarks>
    public static class ShopBagQuantityDebugUtil
    {
        /// <summary>单行展示：商店候选 + 侧别（Buy/Sell/Both）。</summary>
        public sealed class ShopBagRow
        {
            public EMainItemName ItemId;
            public string DisplayName;
            public bool IsBuy;
            public bool IsSell;
        }

        /// <summary>
        /// 动态拉 Buy∪Sell 并集（去重）；顺序：先 Buy 再补 Sell。
        /// </summary>
        public static List<ShopBagRow> BuildShopUnionRows()
        {
            MainItemDefProvider.EnsureLoaded();
            var result = new List<ShopBagRow>();
            var seen = new HashSet<EMainItemName>();

            // 拷贝出 List：Candidates 返回共享 buffer，后续再调另一侧会 Clear 另一 buffer，但本侧仍有效；
            // 仍拷贝，避免日后改成单 buffer 时踩坑。
            var buy = new List<MainItemDef>(MainItemDefProvider.GetShopBuyCandidates());
            var sell = new List<MainItemDef>(MainItemDefProvider.GetShopSellCandidates());

            foreach (var def in buy)
            {
                if (def == null || !seen.Add(def.ItemId))
                {
                    continue;
                }

                result.Add(new ShopBagRow
                {
                    ItemId = def.ItemId,
                    DisplayName = def.DisplayName,
                    IsBuy = true,
                    IsSell = false,
                });
            }

            foreach (var def in sell)
            {
                if (def == null)
                {
                    continue;
                }

                if (seen.Contains(def.ItemId))
                {
                    // 理论上买卖列表不交叉；若交叉则标 Both
                    for (var i = 0; i < result.Count; i++)
                    {
                        if (result[i].ItemId == def.ItemId)
                        {
                            result[i].IsSell = true;
                            break;
                        }
                    }

                    continue;
                }

                seen.Add(def.ItemId);
                result.Add(new ShopBagRow
                {
                    ItemId = def.ItemId,
                    DisplayName = def.DisplayName,
                    IsBuy = false,
                    IsSell = true,
                });
            }

            return result;
        }

        /// <summary>解析 Play 中背包；失败返回 null。</summary>
        public static PlayerBagData TryResolveBag(bool showDialogOnFail = true)
        {
            if (!Application.isPlaying)
            {
                if (showDialogOnFail)
                {
                    EditorUtility.DisplayDialog(
                        "Shop Bag",
                        "请先 Play 进入游戏（建议 InitScene 正规进，有存档）后再执行。",
                        "OK");
                }

                Debug.LogWarning("[DebugShopBag] 未 Play，拒绝改背包。");
                return null;
            }

            if (GameManager.Instance == null)
            {
                Debug.LogError("[DebugShopBag] GameManager 不可用。");
                return null;
            }

            var archive = GameManager.GetGMComponent<ArchiveComponentGM>();
            var bag = archive?.GetData<PlayerBagData>();
            if (bag == null)
            {
                Debug.LogError("[DebugShopBag] PlayerBagData 为空，请确认已加载存档。");
                return null;
            }

            return bag;
        }

        /// <summary>设为 N + Save；返回是否成功（含「本就等于 N」视为成功）。</summary>
        public static bool TrySetCountAndSave(EMainItemName itemId, int targetCount, bool showDialogOnFail = true)
        {
            var bag = TryResolveBag(showDialogOnFail);
            if (bag == null)
            {
                return false;
            }

            var before = bag.GetMainItemCount(itemId);
            bag.SetMainItemCount(itemId, targetCount);
            SaveBag();
            var after = bag.GetMainItemCount(itemId);
            Debug.Log($"[DebugShopBag] Set {itemId}：{before} → {after}（目标 {targetCount}，已 Save）");
            TryRefreshOpenMenuItemPage();
            return true;
        }

        /// <summary>清空商店货单并集内全部持有 → 一次 Save。</summary>
        public static bool TryClearAllShopCandidates(bool showDialogOnFail = true)
        {
            var bag = TryResolveBag(showDialogOnFail);
            if (bag == null)
            {
                return false;
            }

            var rows = BuildShopUnionRows();
            var changed = 0;
            for (var i = 0; i < rows.Count; i++)
            {
                var id = rows[i].ItemId;
                var held = bag.GetMainItemCount(id);
                if (held <= 0)
                {
                    continue;
                }

                if (bag.SetMainItemCount(id, 0))
                {
                    changed++;
                }
            }

            SaveBag();
            Debug.Log($"[DebugShopBag] 清空商店货：处理 {rows.Count} 种，变更 {changed} 种，已 Save。");
            TryRefreshOpenMenuItemPage();
            return true;
        }

        /// <summary>商店货全满（MaxStackPerItem）→ 一次 Save。</summary>
        public static bool TryFillAllShopCandidatesToMax(bool showDialogOnFail = true)
        {
            var bag = TryResolveBag(showDialogOnFail);
            if (bag == null)
            {
                return false;
            }

            MainItemDefProvider.EnsureLoaded();
            var rows = BuildShopUnionRows();
            var max = PlayerBagData.MaxStackPerItem;
            for (var i = 0; i < rows.Count; i++)
            {
                bag.SetMainItemCount(rows[i].ItemId, max);
            }

            SaveBag();
            Debug.Log($"[DebugShopBag] 商店货全满：{rows.Count} 种 ×{max}，已 Save。");
            TryRefreshOpenMenuItemPage();
            return true;
        }

        private static void SaveBag()
        {
            var archive = GameManager.GetGMComponent<ArchiveComponentGM>();
            archive?.SaveSpcData<PlayerBagData>();
        }

        /// <summary>P1：菜单贵重物品页主动刷（不订 OnDataChange；Proxy 非 MonoBehaviour）。</summary>
        private static void TryRefreshOpenMenuItemPage()
        {
            if (GameManager.Instance == null)
            {
                return;
            }

            var mvc = GameManager.GetGMComponent<MVCComponentGM>();
            var proxy = mvc?.GetProxy<MenuFormProxy>();
            if (proxy == null)
            {
                return;
            }

            proxy.UpdateItemPage();
            Debug.Log("[DebugShopBag] 已刷新 MenuFormProxy 贵重物品页。");
        }
    }
}
#endif
