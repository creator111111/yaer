#if UNITY_EDITOR
using System;
using Game.DataTable.MainItem;
using Game.GameMgr;
using Game.GameMgr.Component.Archive;
using Game.GameMgr.Component.Archive.ArchiveDataClass.Player;
using Game.Static.Enum.Goods;
using UnityEditor;
using UnityEngine;

namespace Game.DataTable.MainItem.Editor
{
    /// <summary>
    /// 运行时调试：向当前存档背包一键塞入全部 EMainItemName（含六新消耗品）。
    /// 菜单：Tools / MainItem / 一键添加全部主道具
    /// 原因：测试面板未露出该入口；验收「贵重物品可见」时用 Tools 更稳。
    /// 替代方案：仍可走 AA_TestPanel actionDict 同名键；本菜单不依赖测试面板 UI。
    /// </summary>
    public static class MainItemAddAllBagMenu
    {
        private const string MenuPath = "Tools/MainItem/一键添加全部主道具";

        [MenuItem(MenuPath)]
        private static void AddAllMainItems()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("[MainItem] 请先 Play 进入游戏场景（有存档）后再执行一键添加。");
                return;
            }

            if (GameManager.Instance == null)
            {
                Debug.LogError("[MainItem] GameManager 不可用，请从 InitScene 正规进游戏。");
                return;
            }

            var archive = GameManager.GetGMComponent<ArchiveComponentGM>();
            if (archive == null)
            {
                Debug.LogError("[MainItem] 存档组件不可用。");
                return;
            }

            var bag = archive.GetData<PlayerBagData>();
            if (bag == null)
            {
                Debug.LogError("[MainItem] PlayerBagData 为空，请确认已加载存档。");
                return;
            }

            // 保证 Database 已加载，避免入包时 Icon/itemType 为空
            MainItemDefProvider.EnsureLoaded();

            var qty = PlayerBagData.MaxStackPerItem;
            var added = 0;
            foreach (EMainItemName itemName in Enum.GetValues(typeof(EMainItemName)))
            {
                bag.AddMainItem(itemName, qty);
                added++;
                Debug.Log($"[MainItem] 已添加 {itemName} x{qty}");
            }

            Debug.Log(
                $"[MainItem] 一键添加全部主道具完成：共 {added} 种，每种尝试 +{qty}（堆叠上限 {PlayerBagData.MaxStackPerItem}）。" +
                "请 ESC → 贵重物品验收图标与数量。");
        }

        /// <summary>仅在 Play 时启用菜单项，避免误点。</summary>
        [MenuItem(MenuPath, true)]
        private static bool ValidateAddAllMainItems()
        {
            return Application.isPlaying;
        }
    }
}
#endif
