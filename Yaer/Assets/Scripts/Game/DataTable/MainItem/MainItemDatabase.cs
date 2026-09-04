using System.Collections.Generic;
using Game.Static.Enum.Goods;
using UnityEngine;

namespace Game.DataTable.MainItem
{
    /// <summary>
    /// 道具总档案柜（唯一数据源）：Icon / Name / Price / detail / itemType 均在 Inspector 维护。
    /// 路径：Assets/GameRes/Config/MainItem/MainItemDatabase.asset
    /// </summary>
    [CreateAssetMenu(fileName = "MainItemDatabase", menuName = "Config/MainItem/MainItemDatabase")]
    public class MainItemDatabase : ScriptableObject
    {
        public List<MainItemDefEntry> entries = new List<MainItemDefEntry>();

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (entries == null || entries.Count == 0)
            {
                return;
            }

            var seen = new HashSet<EMainItemName>();
            foreach (var entry in entries)
            {
                if (entry == null)
                {
                    continue;
                }

                if (seen.Contains(entry.itemId))
                {
                    Debug.LogWarning($"[MainItemDatabase] 重复 itemId：{entry.itemId}", this);
                }
                else
                {
                    seen.Add(entry.itemId);
                }

                if (string.IsNullOrWhiteSpace(entry.displayName))
                {
                    Debug.LogWarning($"[MainItemDatabase] displayName 为空：{entry.itemId}", this);
                }

                if (entry.buyPrice < -1 || entry.sellPrice < -1)
                {
                    Debug.LogWarning($"[MainItemDatabase] 价格不能小于 -1：{entry.itemId}", this);
                }

                // SN-4：仅上架道具（买/卖过滤口径）校验名图；任务道具不强制。
                // 缺中文 Warning；缺英/日降级 Tip，禁止 Error 挡进 Play。
                var isShopListed =
                    (entry.itemType == BagItemType.CostItem && entry.buyPrice >= 0) ||
                    (entry.itemType == BagItemType.MaterialItem && entry.sellPrice >= 0);
                if (!isShopListed)
                {
                    continue;
                }

                if (entry.shopNameSprite == null)
                {
                    Debug.LogWarning(
                        $"[ShopNameSprite] 上架道具缺中文店招名图：{entry.itemId}；请拖 shopNameSprite 或补 ArtRes/UI/Item/ShopName/{entry.itemId}.png",
                        this);
                }

                if (entry.shopNameSpriteEn == null)
                {
                    Debug.Log(
                        $"[ShopNameSprite] Tip：{entry.itemId} 缺英文名图 shopNameSpriteEn（Play 将按英→中回退）",
                        this);
                }

                if (entry.shopNameSpriteJp == null)
                {
                    Debug.Log(
                        $"[ShopNameSprite] Tip：{entry.itemId} 缺日文名图 shopNameSpriteJp（Play 将按英→中回退）",
                        this);
                }
            }
        }
#endif
    }
}
