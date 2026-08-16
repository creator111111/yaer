#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using Game.Static.Enum.Goods;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace Game.DataTable.MainItem.Editor
{
    /// <summary>
    /// IA-1：从 MainItemConfig.json 一次性导入 MainItemDatabase.asset（文案/itemType/Icon）。
    /// buyPrice / sellPrice 导入后请在 Database Inspector 维护；HpBall 默认 buyPrice=200。
    /// 菜单：Tools / MainItem / Import Database From JSON
    /// </summary>
    public static class MainItemDatabaseEditor
    {
        private const string MenuPath = "Tools/MainItem/Import Database From JSON";
        private const string JsonPath = "Assets/GameRes/Config/MainItemConfig/MainItemConfig.json";
        private const string DatabasePath = MainItemDefProvider.MainItemDatabaseAssetPath;
        private const string IconFolderPath = "Assets/ArtRes/UI/Item/Icon/";

        [MenuItem(MenuPath)]
        private static void ImportFromMenu()
        {
            ImportDatabase(showDialog: true);
        }

        /// <summary>供 Unity -batchmode -executeMethod 调用。</summary>
        public static void ExecuteBatchImport()
        {
            ImportDatabase(showDialog: false);
            EditorApplication.Exit(0);
        }

        private static void ImportDatabase(bool showDialog)
        {
            var jsonAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(JsonPath);
            if (jsonAsset == null)
            {
                Report(showDialog, "未找到 JSON：" + JsonPath);
                return;
            }

            List<Dictionary<string, string>> rows;
            try
            {
                rows = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(jsonAsset.text);
            }
            catch (Exception ex)
            {
                Report(showDialog, "JSON 解析失败：" + ex.Message);
                return;
            }

            var buyPriceByItemId = BuildDefaultBuyPrices();
            var sellPriceByItemId = BuildDefaultSellPrices();
            var database = AssetDatabase.LoadAssetAtPath<MainItemDatabase>(DatabasePath);
            if (database == null)
            {
                EnsureFolderExists("Assets/GameRes/Config/MainItem");
                database = ScriptableObject.CreateInstance<MainItemDatabase>();
                AssetDatabase.CreateAsset(database, DatabasePath);
            }

            database.entries.Clear();

            foreach (var row in rows)
            {
                if (!row.TryGetValue("name", out var name) || string.IsNullOrEmpty(name))
                {
                    continue;
                }

                if (!Enum.TryParse(name, out EMainItemName itemId))
                {
                    Debug.LogWarning("[MainItemDatabaseEditor] 跳过未知 name（enum 未登记）：" + name);
                    continue;
                }

                var entry = new MainItemDefEntry
                {
                    itemId = itemId,
                    displayName = row.TryGetValue("cnName", out var cnName) ? cnName : name,
                    itemType = ParseBagItemType(row),
                    buyPrice = ResolveBuyPrice(itemId, buyPriceByItemId),
                    sellPrice = ResolveSellPrice(itemId, sellPriceByItemId),
                    legacyNumericId = ParseInt(row, "id"),
                    detail = row.TryGetValue("detail", out var detail) ? detail : string.Empty,
                    detailEn = row.TryGetValue("detail_en", out var detailEn) ? detailEn : string.Empty,
                    detailJp = row.TryGetValue("detail_jp", out var detailJp) ? detailJp : string.Empty,
                    icon = AssetDatabase.LoadAssetAtPath<Sprite>(IconFolderPath + name + ".png"),
                    // SN：导入时顺带挂上三语店招名图（目录有则拖进槽；无则留空由策划补）。
                    shopNameSprite = AssetDatabase.LoadAssetAtPath<Sprite>(
                        "Assets/ArtRes/UI/Item/ShopName/" + name + ".png"),
                    shopNameSpriteEn = AssetDatabase.LoadAssetAtPath<Sprite>(
                        "Assets/ArtRes/UI/Item/ShopName/" + name + "_en.png"),
                    shopNameSpriteJp = AssetDatabase.LoadAssetAtPath<Sprite>(
                        "Assets/ArtRes/UI/Item/ShopName/" + name + "_jp.png")
                };

                database.entries.Add(entry);
            }

            EditorUtility.SetDirty(database);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Report(
                showDialog,
                $"导入完成：{database.entries.Count} 条 → {DatabasePath}\n" +
                "价格请在 MainItemDatabase Inspector 校对；商店行数由 itemType + 价格过滤。");
        }

        /// <summary>JSON 首次导入时的买价默认值（不含 ShopCatalog）。</summary>
        private static Dictionary<EMainItemName, int> BuildDefaultBuyPrices()
        {
            return new Dictionary<EMainItemName, int>
            {
                { EMainItemName.HpBall, 200 },
            };
        }

        /// <summary>素材默认卖价（与策划 §3.2 一致）。</summary>
        private static Dictionary<EMainItemName, int> BuildDefaultSellPrices()
        {
            return new Dictionary<EMainItemName, int>
            {
                { EMainItemName.InsectBeak, 5 },
                { EMainItemName.TenWangFruit, 5 },
                { EMainItemName.SlimeCore, 5 },
            };
        }

        private static int ResolveBuyPrice(EMainItemName itemId, Dictionary<EMainItemName, int> fromCatalog)
        {
            if (fromCatalog.TryGetValue(itemId, out var price) && price > 0)
            {
                return price;
            }

            // 文档 §4.5：HpBall 固定 200（Shop 已有）；其余默认 -1
            if (itemId == EMainItemName.HpBall)
            {
                return 200;
            }

            return -1;
        }

        private static int ResolveSellPrice(EMainItemName itemId, Dictionary<EMainItemName, int> defaults)
        {
            if (defaults.TryGetValue(itemId, out var price))
            {
                return price;
            }

            return -1;
        }

        private static BagItemType ParseBagItemType(Dictionary<string, string> row)
        {
            var itemType = ParseInt(row, "itemType");
            switch (itemType)
            {
                case 1:
                    return BagItemType.CostItem;
                case 2:
                    return BagItemType.MaterialItem;
                default:
                    return BagItemType.TaskItem;
            }
        }

        private static int ParseInt(Dictionary<string, string> row, string key)
        {
            if (row.TryGetValue(key, out var raw) && int.TryParse(raw, out var value))
            {
                return value;
            }

            return 0;
        }

        private static void EnsureFolderExists(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            const string root = "Assets/GameRes/Config";
            if (!AssetDatabase.IsValidFolder(root))
            {
                return;
            }

            AssetDatabase.CreateFolder(root, "MainItem");
        }

        private static void Report(bool showDialog, string message)
        {
            Debug.Log("[MainItemDatabaseEditor] " + message);
            if (showDialog)
            {
                EditorUtility.DisplayDialog("MainItem Database Import", message, "确定");
            }
        }
    }
}
#endif
