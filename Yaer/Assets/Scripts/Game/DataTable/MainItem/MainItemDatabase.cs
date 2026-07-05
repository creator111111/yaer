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
            }
        }
#endif
    }
}
