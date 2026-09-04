using System;
using System.Collections.Generic;
using Game.GameMgr;
using Game.GameMgr.Component;
using Game.Static.Enum.Goods;
using Game.Static.Name.Settings;
using Game.Static.Path;
using UnityEngine;
using UnityEngine.U2D;

namespace Game.DataTable.MainItem
{
    /// <summary>
    /// 道具固有属性统一入口：背包 / 菜单 / 商店均通过 GetDef(itemId) 读取 MainItemDatabase。
    /// 加载方式：Editor 固定路径同步 LoadAsset；运行时经 ResComponentGM 异步预加载。
    /// </summary>
    public static class MainItemDefProvider
    {
        public const string MainItemDatabaseAssetPath = "Assets/GameRes/Config/MainItem/MainItemDatabase.asset";
        private const string IconFolderPath = "Assets/ArtRes/UI/Item/Icon/";
        /// <summary>商店三语店招名图 PNG 兜底目录（与 Icon/ 并列）。</summary>
        private const string ShopNameFolderPath = "Assets/ArtRes/UI/Item/ShopName/";

        private static MainItemDatabase _database;
        private static Dictionary<EMainItemName, MainItemDef> _defById;
        private static Dictionary<string, MainItemDef> _defByName;
        private static Dictionary<EMainItemName, MainItemDefEntry> _entryById;
        private static SpriteAtlas _iconAtlas;
        private static bool _databaseLoadRequested;
        private static bool _iconAtlasLoadRequested;

        /// <summary>商店购买页候选缓存（按 Database entries 顺序）。</summary>
        private static readonly List<MainItemDef> ShopBuyCandidatesBuffer = new List<MainItemDef>();

        /// <summary>Database 异步加载或图集就绪后重建缓存时触发；商店可重刷 Icon。</summary>
        public static event Action DefinitionsRebuilt;

        /// <summary>商店出售页候选缓存（按 Database entries 顺序）。</summary>
        private static readonly List<MainItemDef> ShopSellCandidatesBuffer = new List<MainItemDef>();

        /// <summary>同步加载 Database 并构建字典；Editor / Village_Shop 测试场景可立即 GetDef。</summary>
        public static void EnsureLoaded()
        {
            if (_defById != null)
            {
                return;
            }

            TryLoadDatabaseSync();
            if (_database == null)
            {
                TryLoadDatabaseAsync();
                return;
            }

            RebuildCache();
        }

        public static MainItemDef GetDef(EMainItemName itemId)
        {
            EnsureLoaded();
            if (_defById != null && _defById.TryGetValue(itemId, out var def))
            {
                return def;
            }

            return null;
        }

        public static MainItemDef GetDef(string itemName)
        {
            if (string.IsNullOrEmpty(itemName))
            {
                return null;
            }

            if (Enum.TryParse(itemName, out EMainItemName itemId))
            {
                return GetDef(itemId);
            }

            EnsureLoaded();
            if (_defByName != null && _defByName.TryGetValue(itemName, out var def))
            {
                return def;
            }

            return null;
        }

        public static bool TryGetBuyPrice(EMainItemName itemId, out int price)
        {
            var def = GetDef(itemId);
            if (def != null && def.BuyPrice >= 0)
            {
                price = def.BuyPrice;
                return true;
            }

            price = -1;
            return false;
        }

        public static bool TryGetSellPrice(EMainItemName itemId, out int price)
        {
            var def = GetDef(itemId);
            if (def != null && def.SellPrice >= 0)
            {
                price = def.SellPrice;
                return true;
            }

            price = -1;
            return false;
        }

        /// <summary>
        /// Icon 解析优先级：Entry.icon → MainItem_Icon 图集 → Editor ArtRes PNG → null。
        /// </summary>
        public static Sprite ResolveIcon(EMainItemName itemId)
        {
            EnsureLoaded();

            if (_entryById != null && _entryById.TryGetValue(itemId, out var entry))
            {
                return ResolveIconInternal(entry, itemId);
            }

            return ResolveIconInternal(null, itemId);
        }

        /// <summary>
        /// 商店货架名图解析：当前语槽 → Editor PNG 兜底 → 回退链（英 → 中）→ null。
        /// 缺图禁止回退写 displayName 文字。日志前缀 [ShopNameSprite]。
        /// 替代方案（不采用）：单图 + TMP 翻语 —— 违背「名字全是图片」。
        /// </summary>
        public static Sprite ResolveShopNameSprite(EMainItemName itemId, LanguageEnumType language)
        {
            EnsureLoaded();

            MainItemDefEntry entry = null;
            if (_entryById != null)
            {
                _entryById.TryGetValue(itemId, out entry);
            }

            // 1) 当前语；2) English；3) Chinese；每步含 Editor PNG 兜底。
            var cascade = BuildLanguageFallbackCascade(language);
            Sprite resolved = null;
            var resolvedLanguage = language;
            for (var i = 0; i < cascade.Length; i++)
            {
                var candidateLanguage = cascade[i];
                resolved = ResolveShopNameSpriteForLanguage(entry, itemId, candidateLanguage);
                if (resolved != null)
                {
                    resolvedLanguage = candidateLanguage;
                    break;
                }
            }

            if (resolved == null)
            {
                Debug.LogWarning(
                    $"[ShopNameSprite] 三语皆空：{itemId}；Name.Image 将留空，不会回退 displayName。");
                return null;
            }

            if (resolvedLanguage != language)
            {
                Debug.LogWarning(
                    $"[ShopNameSprite] {itemId} 缺 {language} 名图，回退使用 {resolvedLanguage}。");
            }

            return resolved;
        }

        /// <summary>无参：用 GameManager 当前语言解析（Play 进店 / Bind）。</summary>
        public static Sprite ResolveShopNameSprite(EMainItemName itemId)
        {
            var language = LanguageEnumType.Chinese;
            if (GameManager.Instance != null)
            {
                language = GameManager.Instance.language;
            }

            return ResolveShopNameSprite(itemId, language);
        }

        /// <summary>兼容 PlayerBagData.GetItemRow，内部转 MainItemDef。</summary>
        public static MainItemDataTableRow ToDataTableRow(string itemName)
        {
            var def = GetDef(itemName);
            return def?.ToDataTableRow();
        }

        /// <summary>
        /// 商店购买页：CostItem 且 buyPrice&gt;=0；顺序 = Database entries 数组顺序。
        /// </summary>
        public static IReadOnlyList<MainItemDef> GetShopBuyCandidates()
        {
            EnsureLoaded();
            ShopBuyCandidatesBuffer.Clear();

            if (_database?.entries == null)
            {
                return ShopBuyCandidatesBuffer;
            }

            foreach (var entry in _database.entries)
            {
                if (entry == null)
                {
                    continue;
                }

                if (entry.itemType != BagItemType.CostItem || entry.buyPrice < 0)
                {
                    continue;
                }

                var def = GetDef(entry.itemId);
                if (def != null)
                {
                    ShopBuyCandidatesBuffer.Add(def);
                }
            }

            return ShopBuyCandidatesBuffer;
        }

        /// <summary>
        /// 商店出售页：MaterialItem 且 sellPrice&gt;=0；顺序 = Database entries 数组顺序。
        /// </summary>
        public static IReadOnlyList<MainItemDef> GetShopSellCandidates()
        {
            EnsureLoaded();
            ShopSellCandidatesBuffer.Clear();

            if (_database?.entries == null)
            {
                return ShopSellCandidatesBuffer;
            }

            foreach (var entry in _database.entries)
            {
                if (entry == null)
                {
                    continue;
                }

                if (entry.itemType != BagItemType.MaterialItem || entry.sellPrice < 0)
                {
                    continue;
                }

                var def = GetDef(entry.itemId);
                if (def != null)
                {
                    ShopSellCandidatesBuffer.Add(def);
                }
            }

            return ShopSellCandidatesBuffer;
        }

        /// <summary>Database 异步加载完成后刷新 Provider 缓存（供 PlayerBagData 重刷 Icon）。</summary>
        public static void RebuildCacheIfLoaded()
        {
            if (_database != null)
            {
                RebuildCache();
            }
        }

        private static void TryLoadDatabaseSync()
        {
            if (_database != null)
            {
                return;
            }

#if UNITY_EDITOR
            _database = UnityEditor.AssetDatabase.LoadAssetAtPath<MainItemDatabase>(MainItemDatabaseAssetPath);
#endif
        }

        private static void TryLoadDatabaseAsync()
        {
            if (_databaseLoadRequested || _database != null)
            {
                return;
            }

            _databaseLoadRequested = true;

            try
            {
                if (GameManager.Instance == null)
                {
                    _databaseLoadRequested = false;
                    Debug.LogWarning("[MainItemDefProvider] Database 未加载：GameManager 不可用。");
                    return;
                }

                GameManager.GetGMComponent<ResComponentGM>().LoadAsset<MainItemDatabase>(
                    MainItemDatabaseAssetPath,
                    asset =>
                    {
                        _database = asset;
                        RebuildCache();
                    });
            }
            catch (Exception ex)
            {
                _databaseLoadRequested = false;
                Debug.LogWarning($"[MainItemDefProvider] Database 异步加载失败：{ex.Message}");
            }
        }

        private static void TryLoadIconAtlas()
        {
            if (_iconAtlas != null || _iconAtlasLoadRequested)
            {
                return;
            }

            _iconAtlasLoadRequested = true;

            try
            {
                if (GameManager.Instance == null)
                {
                    _iconAtlasLoadRequested = false;
                    return;
                }

                GameManager.GetGMComponent<ResComponentGM>().LoadAsset<SpriteAtlas>(
                    SpriteAtlasPath.GetPath("MainItem_Icon"),
                    atlas =>
                    {
                        _iconAtlas = atlas;
                        RebuildCache();
                    });
            }
            catch
            {
                _iconAtlasLoadRequested = false;
            }
        }

        private static void RebuildCache()
        {
            if (_database == null || _database.entries == null)
            {
                Debug.LogWarning("[MainItemDefProvider] MainItemDatabase 为空或未 Import。");
                return;
            }

            _defById = new Dictionary<EMainItemName, MainItemDef>();
            _defByName = new Dictionary<string, MainItemDef>();
            _entryById = new Dictionary<EMainItemName, MainItemDefEntry>();

            foreach (var entry in _database.entries)
            {
                if (entry == null)
                {
                    continue;
                }

                _entryById[entry.itemId] = entry;

                var def = new MainItemDef(
                    entry.itemId,
                    entry.displayName,
                    ResolveIconInternal(entry, entry.itemId),
                    entry.shopNameSprite,
                    entry.shopNameSpriteEn,
                    entry.shopNameSpriteJp,
                    entry.buyPrice,
                    entry.sellPrice,
                    entry.itemType,
                    entry.detail,
                    entry.detailEn,
                    entry.detailJp,
                    entry.legacyNumericId);

                _defById[entry.itemId] = def;
                _defByName[entry.itemId.ToString()] = def;
            }

            DefinitionsRebuilt?.Invoke();
        }

        private static Sprite ResolveIconInternal(MainItemDefEntry entry, EMainItemName itemId)
        {
            if (entry != null && entry.icon != null)
            {
                return entry.icon;
            }

            TryLoadIconAtlas();
            if (_iconAtlas != null)
            {
                var atlasSprite = _iconAtlas.GetSprite(itemId.ToString());
                if (atlasSprite != null)
                {
                    return atlasSprite;
                }
            }

#if UNITY_EDITOR
            var editorSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(
                IconFolderPath + itemId + ".png");
            if (editorSprite != null)
            {
                return editorSprite;
            }
#endif

            return null;
        }

        /// <summary>回退链：当前语 → English → Chinese（去重后保序；与 §5.2 / Tips 习惯对齐）。</summary>
        private static LanguageEnumType[] BuildLanguageFallbackCascade(LanguageEnumType language)
        {
            switch (language)
            {
                case LanguageEnumType.English:
                    return new[] { LanguageEnumType.English, LanguageEnumType.Chinese };
                case LanguageEnumType.Japanese:
                    return new[]
                    {
                        LanguageEnumType.Japanese,
                        LanguageEnumType.English,
                        LanguageEnumType.Chinese
                    };
                default:
                    // 中文优先；若中文槽与 PNG 皆空，仍可试英文（少见，但避免完全空白）。
                    return new[] { LanguageEnumType.Chinese, LanguageEnumType.English };
            }
        }

        /// <summary>单语种：Database 槽 → Editor PNG（{itemId}{_en|_jp}.png）。</summary>
        private static Sprite ResolveShopNameSpriteForLanguage(
            MainItemDefEntry entry,
            EMainItemName itemId,
            LanguageEnumType language)
        {
            var fromEntry = GetEntryShopNameSprite(entry, language);
            if (fromEntry != null)
            {
                return fromEntry;
            }

#if UNITY_EDITOR
            var resTag = LanguageType.GetLanaguageResTag(language);
            var pngPath = ShopNameFolderPath + itemId + resTag + ".png";
            var pngSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(pngPath);
            if (pngSprite != null)
            {
                return pngSprite;
            }
#endif

            return null;
        }

        private static Sprite GetEntryShopNameSprite(MainItemDefEntry entry, LanguageEnumType language)
        {
            if (entry == null)
            {
                return null;
            }

            switch (language)
            {
                case LanguageEnumType.English:
                    return entry.shopNameSpriteEn;
                case LanguageEnumType.Japanese:
                    return entry.shopNameSpriteJp;
                default:
                    return entry.shopNameSprite;
            }
        }
    }
}
