using System;
using System.Collections.Generic;
using Game.DataTable.MainItem;
using Game.GameMgr;
using Game.GameMgr.Component.Archive;
using Game.GameMgr.Component.Archive.ArchiveDataClass.BaseDataClass;
using Game.GameRuntime.BagPack;
using Game.GameRuntime.UI.FormLogic.Menu;
using Game.Static.Enum.Goods;
using UnityEngine;
using UnityEngine.SceneManagement;

// 背包中道具的类型
public enum BagItemType
{
    TaskItem, // 不能使用,重要道具
    CostItem, // 消耗品
    MaterialItem, // 怪物掉落的素材道具
}

namespace Game.GameMgr.Component.Archive.ArchiveDataClass.Player
{
    [Serializable]
    public class PlayerBagData : BaseArchiveData
    {
        /// <summary> 每种主道具的最大堆叠数量（策划约定与文档一致）。 </summary>
        public const int MaxStackPerItem = 10;

        #region 属性

        public static event Action<PlayerBagData> OnDataChange;

        private static bool hasInitRequested;
        private int lastIndex;

        private Dictionary<string, MenuFormMainItemInfo> mainItemDic = new Dictionary<string, MenuFormMainItemInfo>();
        public string[] quickItem;

        #endregion

        #region 初始化

        public static void Init()
        {
            if (hasInitRequested) return;
            hasInitRequested = true;
            // v2：MainItemDatabase.asset 为唯一数据源；不再 LoadConfig MainItemConfig.json
            MainItemDefProvider.EnsureLoaded();
            // 订阅 Database/图集异步就绪：入包时若 def 为空，就绪后重刷 Icon/detail/itemType，避免贵重物品整格空白。
            // 先 -= 再 +=，防止 Init 被多处调用时重复订阅（Provider 事件为静态，无自动清理）。
            MainItemDefProvider.DefinitionsRebuilt -= OnMainItemDefinitionsRebuilt;
            MainItemDefProvider.DefinitionsRebuilt += OnMainItemDefinitionsRebuilt;
        }

        /// <summary>
        /// MainItemDefProvider 缓存重建后：用 Database 覆盖运行时展示字段并通知 UI（快捷栏/背包页）。
        /// </summary>
        private static void OnMainItemDefinitionsRebuilt()
        {
            if (GameManager.Instance == null)
            {
                return;
            }

            var archive = GameManager.GetGMComponent<ArchiveComponentGM>();
            if (archive == null)
            {
                return;
            }

            var bagData = archive.GetData<PlayerBagData>();
            if (bagData == null)
            {
                return;
            }

            bagData.RefreshMainItemRuntimeData();
            OnDataChange?.Invoke(bagData);
        }

        #endregion

        #region 添加

        /// <summary>
        /// 添加主要物品
        /// </summary>
        /// <param name="itemName">物品类型</param>
        /// <param name="count">数量</param>
        public void AddMainItem(EMainItemName itemName, int count = 1)
        {
            AddMainItem(itemName.ToString(), count);
        }

        public void AddMainItem(string itemName, int count = 1)
        {
            if (count <= 0) { return; }
            if (mainItemDic.ContainsKey(itemName))
            {
                mainItemDic[itemName].num += count;
                mainItemDic[itemName].num = Math.Min(mainItemDic[itemName].num, MaxStackPerItem);
            }
            else
            {
                var def = MainItemDefProvider.GetDef(itemName);
                var addNum = Math.Min(count, MaxStackPerItem);
                mainItemDic.Add(itemName, new MenuFormMainItemInfo
                {
                    index = lastIndex++,
                    name = itemName,
                    icon = def?.Icon,
                    detail = def?.Detail ?? string.Empty,
                    detail_en = def?.DetailEn ?? string.Empty,
                    detail_jp = def?.DetailJp ?? string.Empty,
                    id = def?.LegacyNumericId ?? 0,
                    itemType = def?.ItemType ?? GuessItemType(itemName),
                    num = addNum
                });
            }
            DataChanged(itemName);
        }

        // 刷新当前道具的数据，和配置表同步
        // 因为道具数据可以回随着时间修改，而部分道具数据存入存档之后还是旧数据，就需要同步数据
        public void RefreshMainItemDataInTest()
        {
            MainItemDefProvider.EnsureLoaded();
            if (MainItemDefProvider.GetDef(EMainItemName.HpBall) == null)
            {
                Init();
                return;
            }

            var itemNames = new List<string>(mainItemDic.Keys);
            lastIndex = 0;
            var costItem = 0;
            quickItem = new string[6];
            foreach (var itemName in itemNames)
            {
                var count = Math.Min(mainItemDic[itemName].num, MaxStackPerItem);
                var def = MainItemDefProvider.GetDef(itemName);
                var itemType = def?.ItemType ?? GuessItemType(itemName);
                var newItemData = new MenuFormMainItemInfo
                {
                    index = lastIndex++,
                    name = itemName,
                    icon = def?.Icon,
                    detail = def?.Detail ?? string.Empty,
                    detail_en = def?.DetailEn ?? string.Empty,
                    detail_jp = def?.DetailJp ?? string.Empty,
                    id = def?.LegacyNumericId ?? 0,
                    itemType = itemType,
                    num = count
                };
                if (costItem < 6 && itemType == BagItemType.CostItem)
                {
                    quickItem[costItem] = itemName;
                    costItem++;
                }

                mainItemDic[itemName] = newItemData;
                DataChanged(itemName);
            }
        }

        #endregion

        #region 移除

        public bool TryRemoveMainItem(EMainItemName itemName, int count)
        {
            return TryRemoveMainItem(itemName.ToString(), count);
        }

        public bool TryRemoveMainItem(string itemName, int count)
        {
            var isCostItem = IsCanUse(itemName);
            if (mainItemDic.ContainsKey(itemName))
            {
                if (GetMainItemCount(itemName) < count)
                {
                    return false;
                }

                if ((mainItemDic[itemName].num -= count) == 0)
                {
                    if (isCostItem)
                    {
                        // 消耗品被移除了，用不在快捷列表中的第一个道具来填补当前空位
                        var costItems = GetFirstCostItemUnExitQuickItems(1);
                        var targetName = costItems.Count > 0 ? costItems[0] : null;
                        if (targetName != null)
                        {
                            // 后续道具补齐往前补齐
                            for (int i = mainItemDic[targetName].index + 1; i < lastIndex; i++)
                            {
                                GetItemByIndex(i).index--;
                            }
                            mainItemDic[targetName].index = mainItemDic[itemName].index;
                            mainItemDic.Remove(itemName);
                        }
                        else
                        {
                            RefreshBagDataOnRemoveItem(itemName);
                        }
                    }
                    else
                    {
                        RefreshBagDataOnRemoveItem(itemName);
                    }
                    lastIndex--;
                }
                DataChanged(itemName);
                return true;
            }
            return false;
        }

        void RefreshBagDataOnRemoveItem(string itemName)
        {
            for (int i = mainItemDic[itemName].index + 1; i < lastIndex; i++)
            {
                GetItemByIndex(i).index--;
            }
            mainItemDic.Remove(itemName);
        }

        #endregion

        #region 获取

        /// <summary>
        /// 按照索引顺序获取所有主要物品
        /// </summary>
        /// <returns>索引排序的所有主要物品</returns>
        public List<MenuFormMainItemInfo> GetAllMainItem()
        {
            List<MenuFormMainItemInfo> res = new List<MenuFormMainItemInfo>(mainItemDic.Count);

            for (int i = 0; i < mainItemDic.Count; i++)
            {
                res.Add(null);
            }

            foreach (var pair in mainItemDic)
            {
                res[pair.Value.index] = pair.Value;
            }

            return res;
        }

        /// <summary>
        /// 根据索引获取物品
        /// </summary>
        /// <param name="index">索引</param>
        /// <returns>物品</returns>
        public MenuFormMainItemInfo GetItemByIndex(int index)
        {
            if (index >= mainItemDic.Count) return null;

            foreach (var pair in mainItemDic)
            {
                if (pair.Value.index == index) return pair.Value;
            }

            return null;
        }

        /// <summary>
        /// 根据物品枚举获取物品数量
        /// </summary>
        /// <param name="itemName">物品枚举</param>
        /// <returns>数量</returns>
        public int GetMainItemCount(EMainItemName itemName)
        {
            return GetMainItemCount(itemName.ToString());
        }

        /// <summary>
        /// 根据物品名称获取物品数量
        /// </summary>
        /// <param name="itemName">物品名称</param>
        /// <returns>数量</returns>
        public int GetMainItemCount(string itemName)
        {
            if (mainItemDic.TryGetValue(itemName, out var res)) return res.num;
            return 0;
        }

        /// <summary>
        /// 根据物品枚举获取物品
        /// </summary>
        /// <param name="itemName">物品枚举</param>
        /// <returns>物品</returns>
        public MenuFormMainItemInfo GetMainItem(EMainItemName itemName)
        {
            return GetMainItem(itemName.ToString());
        }

        /// <summary>
        /// 根据物品名称获取物品
        /// </summary>
        /// <param name="itemName">物品名称</param>
        /// <returns>物品</returns>
        public MenuFormMainItemInfo GetMainItem(string itemName)
        {
            if (mainItemDic.TryGetValue(itemName, out var res)) return res;
            return null;
        }

        #endregion

        #region 检查

        public bool HasMainItem(string itemName)
        {
            return mainItemDic.ContainsKey(itemName);
        }

        public bool HasMainItem(EMainItemName itemName)
        {
            return HasMainItem(itemName.ToString());
        }

        public bool IsCanUse(string name)
        {
            var item = GetMainItem(name);
            if (item != null)
            {
                return item.itemType == BagItemType.CostItem;
            }
            var def = MainItemDefProvider.GetDef(name);
            if (def != null)
            {
                return def.ItemType == BagItemType.CostItem;
            }
            return GuessItemType(name) == BagItemType.CostItem;
        }


        #endregion

        #region 改变

        /// <summary>
        /// 交换两个物品的索引
        /// </summary>
        /// <param name="i1">索引1</param>
        /// <param name="i2">索引2</param>
        public void SwapItemsIndex(int i1, int i2)
        {
            if (i1 == i2) return;
            var item1 = GetItemByIndex(i1);
            var item2 = GetItemByIndex(i2);
            if (item1 == null || item2 == null) return;
            mainItemDic[item1.name].index = i2;
            mainItemDic[item2.name].index = i1;
        }

        /// <summary>
        /// 交换两个物品的索引
        /// </summary>
        /// <param name="i1">索引1</param>
        /// <param name="i2">索引2</param>
        public void SwapItemsIndex(MenuFormMainItemInfo i1, MenuFormMainItemInfo i2)
        {
            if (i1 == i2 || i1 == null || i2 == null) return;
            (i2.index, i1.index) = (i1.index, i2.index);
            RefreshCostItemOnIndexChane(i1, i2);
            OnDataChange?.Invoke(this);
        }

        #endregion

        #region 存档

        public override void ParseInternal(MasterGameData masterData)
        {
            Init();
            var bytes = masterData.GetValue<byte[]>("PlayerBagData_mainItemDic");
            if (bytes != default)
            {
                mainItemDic = ES3.Deserialize<Dictionary<string, MenuFormMainItemInfo>>(bytes);
            }
            else
            {
                mainItemDic = new Dictionary<string, MenuFormMainItemInfo>();
            }
            RefreshMainItemRuntimeData();
            ClampAllItemStacks();
            lastIndex = masterData.GetValue("PlayerBagData_lastIndex", 0);
            bytes = masterData.GetValue<byte[]>("PlayerBagData_quickItem");
            if (bytes != default)
            {
                quickItem = ES3.Deserialize<string[]>(bytes);
            }
            else
            {
                quickItem = new string[6];
            }
            EnsureQuickItemLength();
            RefreshCostItem();
            OnDataChange?.Invoke(this);
        }

        public override void SerializeInternal(MasterGameData masterData)
        {
            masterData.SetValue("PlayerBagData_lastIndex", lastIndex);
            masterData.SetValue("PlayerBagData_quickItem", ES3.Serialize(quickItem));
            masterData.SetValue("PlayerBagData_mainItemDic", ES3.Serialize(mainItemDic));
        }

        #endregion

        #region 事件

        /// <summary>
        /// 触发数据改变事件
        /// </summary>
        public void DataChanged(string name)
        {

            EnsureQuickItemLength();
            if (!IsCanUse(name)) return;

            int index = -1;
            int nullIndex = -1;

            for (int i = 0; i < quickItem.Length; i++)
            {
                if (quickItem[i] == name)
                {
                    index = i;
                }
                else if (nullIndex == -1 && string.IsNullOrEmpty(quickItem[i]))
                {
                    nullIndex = i;
                }
            }

            if (HasMainItem(name))
            {
                if (index == -1)
                {
                    if (nullIndex != -1)
                    {
                        quickItem[nullIndex] = name;
                    }
                }
            }
            else
            {
                if (index != -1)
                {
                    quickItem[index] = null;
                }
            }
            RefreshCostItem();

            OnDataChange?.Invoke(this);
        }

        // 交换道具位置之后刷新快捷道具栏显示顺序
        void RefreshCostItemOnIndexChane(MenuFormMainItemInfo i1, MenuFormMainItemInfo i2)
        {
            if (i1 == null || i2 == null) { return; }
            if (i1.itemType != BagItemType.CostItem && i2.itemType != BagItemType.CostItem) { return; }
            // 更新当前6个消耗类道具到快捷道具栏
            quickItem = new string[6];
            var index = 0;
            var mainItemList = GetAllMainItem();
            foreach (var item in mainItemList)
            {
                var itemType = item.itemType;
                var itemName = item.name;
                if (index >= quickItem.Length) { break; }// 找到足够的道具则跳出
                if (itemType == BagItemType.CostItem)
                {
                    quickItem[index] = itemName;
                    index++;
                }
                
            }
        }

        // 刷新当前消耗品
        void RefreshCostItem()
        {
            var curNullCount = GetQuickItemNullCount();// 获取当前消耗品中有多少空位置
            List<string> newQuickItemNames = GetFirstCostItemUnExitQuickItems(curNullCount);
            // 逐个添加到快捷列表中
            for (int i = 0; i < quickItem.Length; i++)
            {
                if (newQuickItemNames.Count <= 0) { break; }// 添加完毕后至此新的快捷列表补充完成
                if (quickItem[i] == null)
                {
                    quickItem[i] = newQuickItemNames[0];
                    newQuickItemNames.RemoveAt(0);// 添加后就移除
                }
            }
        }

        // 获取快捷列表中的空位置
        int GetQuickItemNullCount()
        {
            var count = 0;
            for (int i = 0; i < quickItem.Length; i++)
            {
                if (quickItem.GetValue(i) == null)
                {
                    count++;
                }
            }
            return count;
        }

        // 检测当前某个道具是否在快捷列表中
        bool CheckItemHasInQuickItemList(string itemName)
        {
            for (int i = 0; i < quickItem.Length; i++)
            {
                if (itemName == quickItem[i])
                {
                    return true;
                }
            }
            return false;
        }

        // 获取当前按顺序前X个不在快捷列表中的消耗品
        List<string> GetFirstCostItemUnExitQuickItems(int curNullCount)
        {
            List<string> newQuickItemNames = new List<string>(); // 新添加到快捷列表中的数据
            foreach (var item in mainItemDic)
            {
                var itemType = item.Value.itemType;
                var itemName = item.Value.name;
                if (newQuickItemNames.Count >= curNullCount) { break; }// 找到足够的道具则跳出
                if (itemType == BagItemType.CostItem && !CheckItemHasInQuickItemList(itemName))
                {
                    newQuickItemNames.Add(itemName);
                }
            }
            return newQuickItemNames;
        }

        private MainItemDataTableRow GetItemRow(string itemName)
        {
            return MainItemDefProvider.ToDataTableRow(itemName);
        }

        private BagItemType GuessItemType(string itemName)
        {
            if (itemName == EMainItemName.HpBall.ToString() || itemName == EMainItemName.MpBall.ToString())
            {
                return BagItemType.CostItem;
            }
            return BagItemType.TaskItem;
        }

        /// <summary>
        /// 用 MainItemDefProvider 覆盖存档中的 icon/detail/itemType（这些字段不进 ES3）。
        /// Icon 优先取 Def；若仍空则再 ResolveIcon 一次，修正异步窗口期内入包的空图。
        /// </summary>
        private void RefreshMainItemRuntimeData()
        {
            if (mainItemDic == null) { return; }
            MainItemDefProvider.EnsureLoaded();
            foreach (var pair in mainItemDic)
            {
                var item = pair.Value;
                if (item == null) { continue; }
                var def = MainItemDefProvider.GetDef(item.name);
                item.icon = def?.Icon;
                // Def.Icon 仍空时（图集晚到等）再解析一次，配合 DefinitionsRebuilt 闭环
                if (item.icon == null && !string.IsNullOrEmpty(item.name)
                    && Enum.TryParse(item.name, out EMainItemName itemId))
                {
                    item.icon = MainItemDefProvider.ResolveIcon(itemId);
                }
                if (def != null)
                {
                    item.detail = def.Detail;
                    item.detail_en = def.DetailEn;
                    item.detail_jp = def.DetailJp;
                    item.id = def.LegacyNumericId;
                    item.itemType = def.ItemType;
                }
                else
                {
                    item.itemType = GuessItemType(item.name);
                }
            }
        }

        /// <summary> 读档等场景：将旧存档中超过上限的数量钳制到 MaxStackPerItem。 </summary>
        private void ClampAllItemStacks()
        {
            if (mainItemDic == null) { return; }
            foreach (var pair in mainItemDic)
            {
                if (pair.Value == null) { continue; }
                if (pair.Value.num > MaxStackPerItem)
                {
                    pair.Value.num = MaxStackPerItem;
                }
            }
        }

        private void EnsureQuickItemLength()
        {
            if (quickItem == null)
            {
                quickItem = new string[6];
                return;
            }

            if (quickItem.Length == 6) { return; }
            var oldData = quickItem;
            quickItem = new string[6];
            for (int i = 0; i < oldData.Length && i < quickItem.Length; i++)
            {
                quickItem[i] = oldData[i];
            }
        }

        #endregion

    }
}