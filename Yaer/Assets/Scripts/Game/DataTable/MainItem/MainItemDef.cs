using Game.Static.Enum.Goods;
using UnityEngine;

namespace Game.DataTable.MainItem
{
    /// <summary>
    /// 道具运行时只读视图；由 MainItemDefProvider 从 Entry 构造，不必再建独立 SO。
    /// </summary>
    public sealed class MainItemDef
    {
        public EMainItemName ItemId { get; }
        public string DisplayName { get; }
        public Sprite Icon { get; }
        public int BuyPrice { get; }
        public int SellPrice { get; }
        public BagItemType ItemType { get; }
        public string Detail { get; }
        public string DetailEn { get; }
        public string DetailJp { get; }
        public int LegacyNumericId { get; }

        public MainItemDef(
            EMainItemName itemId,
            string displayName,
            Sprite icon,
            int buyPrice,
            int sellPrice,
            BagItemType itemType,
            string detail,
            string detailEn,
            string detailJp,
            int legacyNumericId)
        {
            ItemId = itemId;
            DisplayName = displayName;
            Icon = icon;
            BuyPrice = buyPrice;
            SellPrice = sellPrice;
            ItemType = itemType;
            Detail = detail ?? string.Empty;
            DetailEn = detailEn ?? string.Empty;
            DetailJp = detailJp ?? string.Empty;
            LegacyNumericId = legacyNumericId;
        }

        /// <summary>兼容旧 GetItemRow / DataTable 调用链，减少全项目替换。</summary>
        public MainItemDataTableRow ToDataTableRow()
        {
            return new MainItemDataTableRow
            {
                id = LegacyNumericId,
                name = ItemId.ToString(),
                cnName = DisplayName,
                detail = Detail,
                detail_en = DetailEn,
                detail_jp = DetailJp,
                itemType = (int)ItemType
            };
        }
    }
}
