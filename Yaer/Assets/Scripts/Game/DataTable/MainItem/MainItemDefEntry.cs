using System;
using Game.Static.Enum.Goods;
using UnityEngine;

namespace Game.DataTable.MainItem
{
    /// <summary>
    /// 单条道具固有属性（MainItemDatabase 内 List 元素，不是独立 .asset）。
    /// enum 名 == itemId == 存档 string 键；Icon 可在 Inspector 直接拖 Sprite。
    /// </summary>
    [Serializable]
    public class MainItemDefEntry
    {
        [Tooltip("道具 ID，与 EMainItemName / 存档字典键一致")]
        public EMainItemName itemId;

        [Tooltip("列表/背包图标；留空则运行时走 MainItem_Icon 图集或 ArtRes PNG 兜底")]
        public Sprite icon;

        [Tooltip("中文展示名（原 MainItemConfig.cnName）")]
        public string displayName;

        [Tooltip("任务 / 消耗品 / 素材")]
        public BagItemType itemType;

        [Tooltip("固有买价；>=0 可购买，-1 表示未定价/不可买")]
        public int buyPrice = -1;

        [Tooltip("固有卖价；>=0 可出售，-1 表示未定价/不可卖")]
        public int sellPrice = -1;

        [Tooltip("迁移自 JSON 的 numeric id，供菜单/旧逻辑兼容；运行时查找不依赖此字段")]
        public int legacyNumericId;

        [TextArea(3, 12)]
        public string detail;

        [TextArea(2, 8)]
        public string detailEn;

        [TextArea(2, 8)]
        public string detailJp;
    }
}
