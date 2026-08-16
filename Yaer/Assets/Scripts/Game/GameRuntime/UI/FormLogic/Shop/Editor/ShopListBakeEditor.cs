#if UNITY_EDITOR
using System.Collections.Generic;
using System.Text;
using Game.DataTable.MainItem;
using Game.GameRuntime.UI.Component;
using Game.Static.Enum.Goods;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.U2D;
using UnityEngine.UI;

namespace Game.GameRuntime.UI.FormLogic.Shop.Editor
{
    /// <summary>
    /// EB-0～EB-5：一键从 MainItemDatabase 烘焙 Buy/Sell 双 Scroll 列表。
    /// 0713 后进店真源是场景 UI_Shop（OpenUIForm ShopPanel 已弃用）；
    /// Bake 必烤场景，可选再镜像同步 ShopPanel.prefab，避免只写 Prefab 导致场景 Name 仍 None。
    ///
    /// 菜单：Tools / Shop / Bake Shop Lists From MainItemDatabase
    /// Batchmode：ShopListBakeEditor.ExecuteBatchBake()
    /// </summary>
    public static class ShopListBakeEditor
    {
        private const string MenuPath = "Tools/Shop/Bake Shop Lists From MainItemDatabase";

        private const string VillageShopScenePath = "Assets/GameRes/Scenes/Village_Shop.unity";
        private const string ShopPanelPrefabPath = "Assets/GameRes/Prefabs/UI/ShopPanel.prefab";
        private const string ShopBarPrefabPath = "Assets/GameRes/Prefabs/UI/Shop/Shop_Bar.prefab";
        private const string MainItemDatabasePath = MainItemDefProvider.MainItemDatabaseAssetPath;
        private const string IconFolderPath = "Assets/ArtRes/UI/Item/Icon/";
        private const string ShopNameFolderPath = "Assets/ArtRes/UI/Item/ShopName/";
        private const string IconAtlasPath = "Assets/GameRes/Atlas/MainItem_Icon.spriteatlas";

        private const string BarNodeName = "Bar";
        private const string BarBgName = "Bar_BG";
        private const string BarListScrollBuyName = "Bar_ListScroll_Buy";
        private const string BarListScrollSellName = "Bar_ListScroll_Sell";
        private const string BarListScrollLegacyName = "Bar_ListScroll";
        private const string ContentName = "Content";
        private const string ViewportName = "Viewport";
        private const string ViewportContentPath = "Viewport/Content";
        private const string VerticalScrollbarName = "Scrollbar Vertical";
        private const string Total2NodeName = "Total2";
        private const string Total2DigitsNodeName = "Total2_Digits";
        private const string PriceNodeName = "Price";
        private const string NumberNodeName = "Number";
        /// <summary>道具名节点；本任务已定为 Image（商店名图）。</summary>
        private const string NameNodeName = "Name";

        /// <summary>Shop_Bar.prefab 行高（SizeDelta.y）。</summary>
        private const float RowHeight = 88f;

        /// <summary>VerticalLayoutGroup 行距。</summary>
        private const float RowSpacing = 16f;

        /// <summary>与 ItemShowPanel 对齐的工程 UI 参考分辨率（Fix-T3）。</summary>
        private const float ReferenceResolutionWidth = 1920f;
        private const float ReferenceResolutionHeight = 1080f;

        private static SpriteAtlas _cachedIconAtlas;
        private static Sprite[] _cachedDigitSprites;

        [MenuItem(MenuPath)]
        public static void BakeFromMenu()
        {
            if (!EnsureVillageShopSceneOpen())
            {
                return;
            }

            RunBake(showDialog: true);
        }

        /// <summary>供 Unity -batchmode -executeMethod 调用；无对话框。</summary>
        public static void ExecuteBatchBake()
        {
            var scene = EditorSceneManager.OpenScene(VillageShopScenePath, OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                Debug.LogError("[ShopListBake] 无法打开场景: " + VillageShopScenePath);
                EditorApplication.Exit(1);
                return;
            }

            RunBake(showDialog: false);
            EditorApplication.Exit(0);
        }

        private static bool EnsureVillageShopSceneOpen()
        {
            var active = SceneManager.GetActiveScene();
            if (active.path == VillageShopScenePath)
            {
                return true;
            }

            if (!EditorUtility.DisplayDialog(
                    "Bake Shop Lists",
                    "将打开 Village_Shop，必烤场景 UI_Shop（进店主路径），并可选同步 ShopPanel.prefab 镜像。是否继续？",
                    "继续",
                    "取消"))
            {
                return false;
            }

            EditorSceneManager.OpenScene(VillageShopScenePath);
            return true;
        }

        /// <summary>
        /// 场景 UI_Shop 必烤（进店真源）；若存在 ShopPanel.prefab 再镜像同步一份。
        /// 替代方案（不采用）：只烤 ShopPanel —— 0713 已弃用 OpenUIForm，会导致场景 Name 仍 None。
        /// </summary>
        private static void RunBake(bool showDialog)
        {
            // 1) 必烤场景 UI_Shop（含已禁用实例）
            var uiShopScene = GameObject.Find("UI_Shop") ?? FindSceneObjectIncludingInactive("UI_Shop");
            if (uiShopScene == null)
            {
                Report(
                    showDialog,
                    "未找到场景 UI_Shop。请打开 Village_Shop 并确认场景内存在 UI_Shop（进店主路径）。");
                return;
            }

            Debug.Log("[ShopListBake] 目标：场景 UI_Shop（进店主路径）");
            var sceneResult = RunBakeOnRoot(uiShopScene, skipSceneSave: false);
            if (!sceneResult.Ok)
            {
                Report(showDialog, "场景 UI_Shop 烘焙失败：\n" + sceneResult.Message);
                return;
            }

            var sb = new StringBuilder();
            sb.AppendLine("场景 UI_Shop 已烤（进店主路径）。");
            sb.AppendLine(sceneResult.Message);

            // 2) 可选镜像 ShopPanel.prefab（非进店主路径，防日后误用时双份漂移）
            var shopPanelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ShopPanelPrefabPath);
            if (shopPanelPrefab != null)
            {
                var contents = PrefabUtility.LoadPrefabContents(ShopPanelPrefabPath);
                try
                {
                    Debug.Log("[ShopListBake] 镜像同步：ShopPanel.prefab（非进店主路径）");
                    var prefabResult = RunBakeOnRoot(contents, skipSceneSave: true);
                    if (prefabResult.Ok)
                    {
                        PrefabUtility.SaveAsPrefabAsset(contents, ShopPanelPrefabPath);
                        sb.AppendLine();
                        sb.AppendLine("ShopPanel.prefab 已同步镜像。");
                        sb.AppendLine(prefabResult.Message);
                        Debug.Log("[ShopListBake] 已写回 " + ShopPanelPrefabPath);
                    }
                    else
                    {
                        sb.AppendLine();
                        sb.AppendLine("ShopPanel.prefab 同步失败：" + prefabResult.Message);
                    }
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(contents);
                }
            }
            else
            {
                sb.AppendLine();
                sb.AppendLine("未找到 ShopPanel.prefab，跳过镜像同步。");
            }

            Report(showDialog, sb.ToString().TrimEnd());
        }

        /// <summary>单次烤根结果：成功与否 + 汇总/错误文案（由 RunBake 统一弹窗）。</summary>
        private struct BakeRootResult
        {
            public bool Ok;
            public string Message;
        }

        /// <summary>在指定根（场景 UI_Shop 或 ShopPanel 内容实例）上执行 Bake 主体。</summary>
        private static BakeRootResult RunBakeOnRoot(GameObject uiShop, bool skipSceneSave)
        {
            // IMG：校正 Shop_Bar 预制体 Price/Number DigitStrip
            EnsureShopBarPrefabDigitStructure();

            var digitSprites = GetDigitSprites();

            // FX-3：校正 CanvasScaler，避免 Constant Pixel Size 导致 4K 下 UI 显小
            EnsureUiShopCanvasScaler(uiShop);
            // IMG：Total2 底框下图片合计节点
            EnsureTotal2Digits(uiShop.transform, digitSprites);

            var bar = uiShop.transform.Find(BarNodeName);
            if (bar == null)
            {
                return Fail("未找到 Bar 节点（ShopPanel / UI_Shop 下）。");
            }

            var barBg = EnsureBarBackground(bar);
            if (barBg == null)
            {
                return Fail("Bar 下缺少 BG/Bar_BG。");
            }

            // 1～3：Ensure 双 Scroll 壳
            var buyScroll = EnsureScrollShell(bar, barBg, BarListScrollBuyName, duplicateFrom: null);
            var sellScroll = EnsureScrollShell(bar, barBg, BarListScrollSellName, duplicateFrom: buyScroll);
            if (buyScroll == null || sellScroll == null)
            {
                return Fail("Buy/Sell Scroll 壳创建失败。");
            }

            // 4：滚轮 / Viewport 修正
            ShopScrollShellHelper.ApplyInteractionFixes(buyScroll);
            ShopScrollShellHelper.ApplyInteractionFixes(sellScroll);

            // 5：加载 Database
            var database = AssetDatabase.LoadAssetAtPath<MainItemDatabase>(MainItemDatabasePath);
            if (database == null || database.entries == null)
            {
                return Fail("未找到 MainItemDatabase: " + MainItemDatabasePath);
            }

            var shopBarPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ShopBarPrefabPath);
            if (shopBarPrefab == null)
            {
                return Fail("未找到 Shop_Bar.prefab: " + ShopBarPrefabPath);
            }

            var buyContent = FindScrollContent(buyScroll);
            var sellContent = FindScrollContent(sellScroll);
            if (buyContent == null || sellContent == null)
            {
                return Fail("Scroll 下缺少 Viewport/Content。");
            }

            var missingIcons = new List<string>();
            var missingShopNames = new List<string>();

            // 6～7：Bake 两侧 Content
            var buyEntries = FilterEntries(database.entries, isBuyRow: true);
            var sellEntries = FilterEntries(database.entries, isBuyRow: false);
            var buyCount = BakeContent(buyContent, buyEntries, shopBarPrefab, isBuyRow: true, missingIcons, missingShopNames, digitSprites);
            var sellCount = BakeContent(sellContent, sellEntries, shopBarPrefab, isBuyRow: false, missingIcons, missingShopNames, digitSprites);

            // 8：绑定 ShopFormLogic
            BindShopFormLogic(uiShop, buyContent, sellContent, buyScroll, sellScroll);

            // 9：Sell 默认隐藏
            sellScroll.gameObject.SetActive(false);
            buyScroll.gameObject.SetActive(true);

            // 10：强制刷新 Layout
            LayoutRebuilder.ForceRebuildLayoutImmediate(buyContent as RectTransform);
            LayoutRebuilder.ForceRebuildLayoutImmediate(sellContent as RectTransform);

            // 11：仅烤场景 UI_Shop 时写场景；烤 Prefab 由 RunBake 外层 SaveAsPrefabAsset。
            if (!skipSceneSave)
            {
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
                EditorSceneManager.SaveOpenScenes();
            }

            // 12：汇总（不弹窗；由 RunBake 统一 Report）
            var summary = BuildSummary(buyCount, sellCount, missingIcons, missingShopNames, savedScene: !skipSceneSave);
            return new BakeRootResult { Ok = true, Message = summary };
        }

        private static BakeRootResult Fail(string message)
        {
            return new BakeRootResult { Ok = false, Message = message };
        }

        /// <summary>含已禁用物体；排除 Prefab 资产本身。</summary>
        private static GameObject FindSceneObjectIncludingInactive(string objectName)
        {
            var transforms = Resources.FindObjectsOfTypeAll<Transform>();
            for (var i = 0; i < transforms.Length; i++)
            {
                var t = transforms[i];
                if (t == null || t.name != objectName || EditorUtility.IsPersistent(t))
                {
                    continue;
                }

                if (t.gameObject.scene.IsValid())
                {
                    return t.gameObject;
                }
            }

            return null;
        }

        /// <summary>
        /// Fix-T3：UI_Shop 根节点对齐工程标准 Scale With Screen Size · 1920×1080 · 全屏 stretch。
        /// 替代方案：仅手工改场景、不写代码——适合一次性测试场；Bake 内校正可防止回退 Constant Pixel Size。
        /// </summary>
        private static void EnsureUiShopCanvasScaler(GameObject uiShop)
        {
            var scaler = uiShop.GetComponent<CanvasScaler>();
            if (scaler == null)
            {
                scaler = uiShop.AddComponent<CanvasScaler>();
            }

            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(ReferenceResolutionWidth, ReferenceResolutionHeight);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0f;

            var rect = uiShop.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.anchoredPosition = Vector2.zero;
                rect.sizeDelta = Vector2.zero;
            }

            EditorUtility.SetDirty(uiShop);
        }

        /// <summary>SC-0：重命名 BG → Bar_BG，关闭底图 Raycast。</summary>
        private static RectTransform EnsureBarBackground(Transform bar)
        {
            Transform barBgTransform = bar.Find(BarBgName);
            if (barBgTransform == null)
            {
                barBgTransform = bar.Find("BG");
                if (barBgTransform != null)
                {
                    barBgTransform.name = BarBgName;
                }
            }

            if (barBgTransform == null)
            {
                return null;
            }

            var image = barBgTransform.GetComponent<Image>();
            if (image != null)
            {
                image.raycastTarget = false;
            }

            return barBgTransform as RectTransform;
        }

        /// <summary>
        /// 创建或校正 Scroll 壳。Sell 无则 Duplicate Buy；Legacy Bar_ListScroll 自动改名为 Buy。
        /// </summary>
        private static Transform EnsureScrollShell(
            Transform bar,
            RectTransform barBg,
            string scrollName,
            Transform duplicateFrom)
        {
            Transform scrollRoot = bar.Find(scrollName);

            // Legacy 单 Scroll 场景：首次 Bake 时改名为 Buy
            if (scrollRoot == null && scrollName == BarListScrollBuyName)
            {
                var legacy = bar.Find(BarListScrollLegacyName);
                if (legacy != null)
                {
                    legacy.name = BarListScrollBuyName;
                    scrollRoot = legacy;
                }
            }

            GameObject scrollGo;

            if (scrollRoot != null)
            {
                scrollGo = scrollRoot.gameObject;
            }
            else if (duplicateFrom != null)
            {
                // Sell：复制 Buy 壳
                var duplicate = Object.Instantiate(duplicateFrom.gameObject, bar);
                duplicate.name = scrollName;
                duplicate.transform.SetSiblingIndex(duplicateFrom.GetSiblingIndex() + 1);

                var sellRect = duplicate.transform as RectTransform;
                var buyRect = duplicateFrom as RectTransform;
                if (sellRect != null && buyRect != null)
                {
                    sellRect.anchorMin = buyRect.anchorMin;
                    sellRect.anchorMax = buyRect.anchorMax;
                    sellRect.pivot = buyRect.pivot;
                    sellRect.anchoredPosition = buyRect.anchoredPosition;
                    sellRect.sizeDelta = buyRect.sizeDelta;
                }

                scrollGo = duplicate;
            }
            else
            {
                // Buy：DefaultControls 创建标准 ScrollRect
                scrollGo = DefaultControls.CreateScrollView(new DefaultControls.Resources());
                scrollGo.name = scrollName;
                scrollGo.transform.SetParent(bar, false);

                var horizontalScrollbar = scrollGo.transform.Find("Scrollbar Horizontal");
                if (horizontalScrollbar != null)
                {
                    Object.DestroyImmediate(horizontalScrollbar.gameObject);
                }

                var verticalScrollbar = scrollGo.transform.Find(VerticalScrollbarName);
                if (verticalScrollbar != null)
                {
                    verticalScrollbar.gameObject.SetActive(false);
                }
            }

            var scrollRect = scrollGo.GetComponent<ScrollRect>();
            if (scrollRect == null)
            {
                scrollRect = scrollGo.AddComponent<ScrollRect>();
            }

            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            scrollRect.scrollSensitivity = ShopScrollShellHelper.DefaultScrollSensitivity;

            AlignScrollWithBarBackground(scrollGo.transform as RectTransform, barBg);
            ConfigureViewport(scrollGo.transform);
            ConfigureContent(FindScrollContent(scrollGo.transform));
            HideVerticalScrollbarInGame(scrollGo.transform);

            return scrollGo.transform;
        }

        /// <summary>
        /// 按 Database entries 顺序过滤：Buy = CostItem &amp; buyPrice&gt;=0；Sell = MaterialItem &amp; sellPrice&gt;=0。
        /// </summary>
        private static List<MainItemDefEntry> FilterEntries(
            IReadOnlyList<MainItemDefEntry> entries,
            bool isBuyRow)
        {
            var result = new List<MainItemDefEntry>();
            if (entries == null)
            {
                return result;
            }

            foreach (var entry in entries)
            {
                if (entry == null)
                {
                    continue;
                }

                if (isBuyRow)
                {
                    if (entry.itemType == BagItemType.CostItem && entry.buyPrice >= 0)
                    {
                        result.Add(entry);
                    }
                }
                else if (entry.itemType == BagItemType.MaterialItem && entry.sellPrice >= 0)
                {
                    result.Add(entry);
                }
            }

            return result;
        }

        /// <summary>
        /// 清空 Content → InstantiatePrefab → 写 Icon/Name(名图)/Price → 序列化 ShopBarRowView baked 字段。
        /// Bake 名图固定中文槽预览；Play 再按当前语言 Resolve。
        /// </summary>
        private static int BakeContent(
            Transform content,
            IReadOnlyList<MainItemDefEntry> entries,
            GameObject shopBarPrefab,
            bool isBuyRow,
            List<string> missingIcons,
            List<string> missingShopNames,
            Sprite[] digitSprites)
        {
            ClearChildren(content);
            var count = 0;

            foreach (var entry in entries)
            {
                var row = PrefabUtility.InstantiatePrefab(shopBarPrefab, content) as GameObject;
                if (row == null)
                {
                    continue;
                }

                row.name = $"Shop_Bar_{entry.itemId}";
                var price = isBuyRow ? entry.buyPrice : entry.sellPrice;

                // ① 直接写 UI（Scene 里立刻可见，不依赖 Play）
                var icon = ResolveIconEditor(entry, missingIcons);
                SetImageSprite(row.transform, "Icon", icon);

                // SN-6：Name 贴中文店招图；禁止再 SetText(displayName)。
                var shopName = ResolveShopNameSpriteEditor(entry, missingShopNames);
                SetImageSprite(row.transform, NameNodeName, shopName, requireImage: true);

                SetSpriteNumber(row.transform, PriceNodeName, price, digitSprites, TextAnchor.MiddleCenter);

                // ② 组件序列化（运行时只读 baked 字段）
                var view = row.GetComponent<ShopBarRowView>() ?? row.AddComponent<ShopBarRowView>();
                view.EditorSetBakedData(entry.itemId, price, isBuyRow);

                // ST + IMG：Buy / Sell 均挂数量输入 + 默认 0 图片
                EnsureShopRowQuantityInput(row, digitSprites);

                ResetRowRectForLayout(row.transform as RectTransform);
                EditorUtility.SetDirty(row);
                count++;
            }

            return count;
        }

        /// <summary>
        /// Editor Icon 解析：entry.icon → ArtRes PNG → MainItem_Icon 图集 → null（与运行时 Provider 对齐，顺序按施工文档）。
        /// </summary>
        private static Sprite ResolveIconEditor(MainItemDefEntry entry, List<string> missingIcons)
        {
            if (entry == null)
            {
                return null;
            }

            if (entry.icon != null)
            {
                return entry.icon;
            }

            var pngPath = IconFolderPath + entry.itemId + ".png";
            var pngSprite = AssetDatabase.LoadAssetAtPath<Sprite>(pngPath);
            if (pngSprite != null)
            {
                return pngSprite;
            }

            var atlas = GetIconAtlas();
            if (atlas != null)
            {
                var atlasSprite = atlas.GetSprite(entry.itemId.ToString());
                if (atlasSprite != null)
                {
                    return atlasSprite;
                }
            }

            missingIcons.Add(entry.itemId.ToString());
            Debug.LogWarning(
                $"[ShopListBake] Icon 未解析：{entry.itemId}；请在 MainItemDatabase 拖 icon 或补 PNG/图集。",
                entry.icon);
            return null;
        }

        /// <summary>
        /// Bake 预览固定中文：entry.shopNameSprite → ShopName/{itemId}.png → null（记 summary）。
        /// </summary>
        private static Sprite ResolveShopNameSpriteEditor(MainItemDefEntry entry, List<string> missingShopNames)
        {
            if (entry == null)
            {
                return null;
            }

            if (entry.shopNameSprite != null)
            {
                return entry.shopNameSprite;
            }

            var pngPath = ShopNameFolderPath + entry.itemId + ".png";
            var pngSprite = AssetDatabase.LoadAssetAtPath<Sprite>(pngPath);
            if (pngSprite != null)
            {
                return pngSprite;
            }

            missingShopNames.Add(entry.itemId.ToString());
            Debug.LogWarning(
                $"[ShopNameSprite] Bake 缺中文名图：{entry.itemId}；请拖 shopNameSprite 或补 {pngPath}",
                entry.shopNameSprite);
            return null;
        }

        private static SpriteAtlas GetIconAtlas()
        {
            if (_cachedIconAtlas == null)
            {
                _cachedIconAtlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(IconAtlasPath);
            }

            return _cachedIconAtlas;
        }

        private static void BindShopFormLogic(
            GameObject uiShop,
            Transform buyContent,
            Transform sellContent,
            Transform buyScroll,
            Transform sellScroll)
        {
            var formLogic = uiShop.GetComponent<ShopFormLogic>();
            if (formLogic == null)
            {
                Debug.LogWarning("[ShopListBake] UI_Shop 上缺少 ShopFormLogic，跳过 Inspector 绑定。");
                return;
            }

            var serialized = new SerializedObject(formLogic);
            serialized.FindProperty("buyContent").objectReferenceValue = buyContent;
            serialized.FindProperty("sellContent").objectReferenceValue = sellContent;
            serialized.FindProperty("barListScrollBuy").objectReferenceValue = buyScroll.gameObject;
            serialized.FindProperty("barListScrollSell").objectReferenceValue = sellScroll.gameObject;

            var btnSell = uiShop.transform.Find("SELL")?.GetComponent<Button>();
            if (btnSell != null)
            {
                serialized.FindProperty("btnSell").objectReferenceValue = btnSell;
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(formLogic);
        }

        /// <summary>IMG-1：Shop_Bar.prefab 的 Price/Number 下补 DigitStrip；SN-5：Name 改为 Image。</summary>
        private static void EnsureShopBarPrefabDigitStructure()
        {
            var prefabRoot = PrefabUtility.LoadPrefabContents(ShopBarPrefabPath);
            if (prefabRoot == null)
            {
                Debug.LogWarning("[ShopListBake] 无法加载 Shop_Bar.prefab 做 DigitStrip 校正。");
                return;
            }

            try
            {
                var sprites = GetDigitSprites();
                EnsureRowPriceAndNumberDigitStrips(prefabRoot.transform, sprites);
                EnsureNameNodeIsImage(prefabRoot.transform);
                PrefabUtility.SaveAsPrefabAsset(prefabRoot, ShopBarPrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }
        }

        /// <summary>
        /// SN-5：Name 节点移除 TMP / Legacy Text，确保挂 Image（Preserve Aspect On，Raycast 关）。
        /// Bake 时顺带校正，避免源 Prefab 漏改导致行实例仍带 TMP。
        /// </summary>
        private static void EnsureNameNodeIsImage(Transform rowRoot)
        {
            var nameNode = rowRoot.Find(NameNodeName);
            if (nameNode == null)
            {
                Debug.LogError("[ShopNameSprite] Shop_Bar 缺少 Name 节点，无法换成 Image。");
                return;
            }

            var tmp = nameNode.GetComponent<TextMeshProUGUI>();
            if (tmp != null)
            {
                Object.DestroyImmediate(tmp);
            }

            var legacy = nameNode.GetComponent<Text>();
            if (legacy != null)
            {
                Object.DestroyImmediate(legacy);
            }

            var image = nameNode.GetComponent<Image>();
            if (image == null)
            {
                image = nameNode.gameObject.AddComponent<Image>();
            }

            image.color = Color.white;
            image.raycastTarget = false;
            image.preserveAspect = true;
            EditorUtility.SetDirty(nameNode.gameObject);
        }

        private static void EnsureRowPriceAndNumberDigitStrips(Transform rowRoot, Sprite[] digitSprites)
        {
            var priceNode = rowRoot.Find(PriceNodeName);
            if (priceNode != null)
            {
                DisableLegacyTextOnNode(priceNode);
                var priceDisplay = UiSpriteNumberDisplay.EnsureOn(
                    priceNode,
                    TextAnchor.MiddleCenter,
                    UiSpriteNumberDisplay.ShopPriceSpacing);
                priceDisplay.AssignSprites(digitSprites);
                priceDisplay.SetSpacing(UiSpriteNumberDisplay.ShopPriceSpacing);
                priceDisplay.EditorBakeSetNumber(0);
            }

            var numberNode = rowRoot.Find(NumberNodeName) ?? rowRoot.Find("TxtStock");
            if (numberNode != null)
            {
                DisableLegacyTextOnNode(numberNode);
                var numberDisplay = UiSpriteNumberDisplay.EnsureOn(
                    numberNode,
                    TextAnchor.MiddleRight,
                    UiSpriteNumberDisplay.ShopNumberSpacing,
                    ShopQuantityInputHelper.MaxQuantityDigits);
                numberDisplay.AssignSprites(digitSprites);
                numberDisplay.SetSpacing(UiSpriteNumberDisplay.ShopNumberSpacing);
                numberDisplay.EditorBakeSetNumber(ShopQuantityInputHelper.DefaultQuantity);
            }
        }

        private static Sprite[] GetDigitSprites()
        {
            if (_cachedDigitSprites == null)
            {
                _cachedDigitSprites = UiSpriteNumberDisplay.LoadDefaultDigitSpritesEditor();
            }

            return _cachedDigitSprites;
        }

        /// <summary>Price 列：Bake 时刷图片数字（替代 SetText）。</summary>
        private static void SetSpriteNumber(
            Transform row,
            string childName,
            int value,
            Sprite[] digitSprites,
            TextAnchor alignment)
        {
            var node = row.Find(childName);
            if (node == null)
            {
                return;
            }

            DisableLegacyTextOnNode(node);
            var stripSpacing = ResolveDigitStripSpacing(alignment);
            var display = UiSpriteNumberDisplay.EnsureOn(node, alignment, stripSpacing);
            display.AssignSprites(digitSprites);
            display.SetSpacing(stripSpacing);
            display.EditorBakeSetNumber(value);
            EditorUtility.SetDirty(display);
        }

        /// <summary>v3：Number 右对齐 -1px；Price 居中 0px。</summary>
        private static float ResolveDigitStripSpacing(TextAnchor alignment)
        {
            return alignment == TextAnchor.MiddleRight
                ? UiSpriteNumberDisplay.ShopNumberSpacing
                : UiSpriteNumberDisplay.ShopPriceSpacing;
        }

        private static void DisableLegacyTextOnNode(Transform node)
        {
            if (node == null)
            {
                return;
            }

            var legacyText = node.GetComponent<Text>();
            if (legacyText != null)
            {
                legacyText.enabled = false;
            }
        }

        /// <summary>IMG：Total2 下图片合计 DigitStrip，初始 0。</summary>
        private static void EnsureTotal2Digits(Transform uiShopRoot, Sprite[] digitSprites)
        {
            var total2 = uiShopRoot.Find(Total2NodeName);
            if (total2 == null)
            {
                Debug.LogWarning("[ShopListBake] 未找到 Total2，跳过 Total2_Digits 创建。");
                return;
            }

            var legacyTxt = total2.Find("TxtTotal2");
            if (legacyTxt != null)
            {
                var legacyText = legacyTxt.GetComponent<Text>();
                if (legacyText != null)
                {
                    legacyText.enabled = false;
                }

                var legacyTmp = legacyTxt.GetComponent<TextMeshProUGUI>();
                if (legacyTmp != null)
                {
                    legacyTmp.enabled = false;
                }
            }

            var existing = total2.Find(Total2DigitsNodeName);
            UiSpriteNumberDisplay display;
            if (existing != null)
            {
                display = existing.GetComponent<UiSpriteNumberDisplay>();
                if (display == null)
                {
                    display = existing.gameObject.AddComponent<UiSpriteNumberDisplay>();
                }
            }
            else
            {
                var stripGo = new GameObject(Total2DigitsNodeName, typeof(RectTransform));
                stripGo.transform.SetParent(total2, false);
                stripGo.layer = total2.gameObject.layer;

                var rect = stripGo.GetComponent<RectTransform>();
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
                rect.pivot = new Vector2(0.5f, 0.5f);

                display = stripGo.AddComponent<UiSpriteNumberDisplay>();
            }

            display.AssignSprites(digitSprites);
            display.ApplyShopTotalLayoutForBake();
            display.EditorBakeSetNumber(0);
            EditorUtility.SetDirty(display.gameObject);
        }

        /// <summary>Buy / Sell 行共用：数量输入 + DigitStrip 默认 0。</summary>
        private static void EnsureShopRowQuantityInput(GameObject row, Sprite[] digitSprites)
        {
            if (row.GetComponent<ShopBuyRowQuantityInput>() == null)
            {
                row.AddComponent<ShopBuyRowQuantityInput>();
            }

            var quantityNode = row.transform.Find("TxtStock") ?? row.transform.Find(NumberNodeName);
            if (quantityNode == null)
            {
                return;
            }

            DisableLegacyTextOnNode(quantityNode);
            ShopQuantityInputHelper.EnsureTmpIntegerInputField(
                quantityNode,
                ShopQuantityInputHelper.DefaultQuantity);

            var display = UiSpriteNumberDisplay.EnsureOn(
                quantityNode,
                TextAnchor.MiddleRight,
                UiSpriteNumberDisplay.ShopNumberSpacing,
                ShopQuantityInputHelper.MaxQuantityDigits);
            display.AssignSprites(digitSprites);
            display.SetSpacing(UiSpriteNumberDisplay.ShopNumberSpacing);
            display.EditorBakeSetNumber(ShopQuantityInputHelper.DefaultQuantity);
        }

        private static void SetImageSprite(Transform row, string childName, Sprite sprite, bool requireImage = false)
        {
            var node = row.Find(childName);
            if (node == null)
            {
                if (requireImage)
                {
                    Debug.LogError(
                        $"[ShopNameSprite] 缺少子节点 {childName}（row={row.name}）。",
                        row);
                }

                return;
            }

            var image = node.GetComponent<Image>();
            if (image == null)
            {
                if (requireImage || childName == NameNodeName)
                {
                    Debug.LogError(
                        $"[ShopNameSprite] {childName} 节点缺少 Image（row={row.name}）。请将 Shop_Bar.Name 换成 Image 后再 Bake。",
                        node);
                }

                return;
            }

            image.sprite = sprite;
            image.enabled = sprite != null;
        }

        /// <summary>
        /// 写子节点文案：优先 TMP，再写 Legacy（合计等非 Name 节点仍可能用到）。
        /// Name 已改为 Image，请走 <see cref="SetImageSprite"/>，禁止再对本节点写 displayName。
        /// </summary>
        private static void SetText(Transform row, string childName, string value)
        {
            var node = row.Find(childName);
            if (node == null)
            {
                return;
            }

            if (childName == NameNodeName)
            {
                Debug.LogError(
                    $"[ShopNameSprite] 禁止对 Name 写文字（row={row.name}）；请用 SetImageSprite + shopNameSprite。",
                    node);
                return;
            }

            SetTextOnNode(node, value);
        }

        private static void SetTextOnNode(Transform node, string value)
        {
            var tmp = node.GetComponent<TextMeshProUGUI>();
            if (tmp != null)
            {
                tmp.text = value ?? string.Empty;
            }

            var legacy = node.GetComponent<Text>();
            if (legacy != null)
            {
                legacy.text = value ?? string.Empty;
            }
        }

        private static Transform FindScrollContent(Transform scrollRoot)
        {
            if (scrollRoot == null)
            {
                return null;
            }

            var content = scrollRoot.Find(ViewportContentPath);
            if (content != null)
            {
                return content;
            }

            var scrollRect = scrollRoot.GetComponent<ScrollRect>();
            if (scrollRect != null && scrollRect.content != null)
            {
                return scrollRect.content;
            }

            return scrollRoot.Find(ContentName);
        }

        private static void AlignScrollWithBarBackground(RectTransform scrollRect, RectTransform barBg)
        {
            if (scrollRect == null || barBg == null)
            {
                return;
            }

            scrollRect.anchorMin = barBg.anchorMin;
            scrollRect.anchorMax = barBg.anchorMax;
            scrollRect.pivot = new Vector2(0.5f, 0.5f);
            scrollRect.anchoredPosition = barBg.anchoredPosition;
            scrollRect.sizeDelta = barBg.sizeDelta;
        }

        /// <summary>
        /// Viewport：挂 RectMask2D 硬裁剪壳，并去掉冗余 Image（Fix-S1）。
        /// Softness 虚化过渡由随后的 ShopScrollShellHelper.ApplyInteractionFixes 统一写入，禁止在此写魔法数。
        /// </summary>
        private static void ConfigureViewport(Transform scrollRoot)
        {
            var viewport = scrollRoot.Find(ViewportName) as RectTransform;
            if (viewport == null)
            {
                return;
            }

            viewport.pivot = new Vector2(0.5f, 1f);
            viewport.anchorMin = new Vector2(0f, 1f);
            viewport.anchorMax = new Vector2(1f, 1f);
            viewport.anchoredPosition = Vector2.zero;
            viewport.sizeDelta = new Vector2(0f, scrollRoot.GetComponent<RectTransform>().sizeDelta.y);

            if (viewport.GetComponent<RectMask2D>() == null)
            {
                var legacyMask = viewport.GetComponent<Mask>();
                if (legacyMask != null)
                {
                    Object.DestroyImmediate(legacyMask);
                }

                viewport.gameObject.AddComponent<RectMask2D>();
            }

            var viewportImage = viewport.GetComponent<Image>();
            if (viewportImage != null)
            {
                Object.DestroyImmediate(viewportImage);
            }
        }

        /// <summary>Content：VerticalLayoutGroup + ContentSizeFitter。</summary>
        private static void ConfigureContent(Transform contentTransform)
        {
            if (contentTransform == null)
            {
                return;
            }

            var content = contentTransform as RectTransform;
            content.pivot = new Vector2(0.5f, 1f);
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.anchoredPosition = Vector2.zero;
            content.sizeDelta = new Vector2(0f, 0f);

            var layout = content.GetComponent<VerticalLayoutGroup>();
            if (layout == null)
            {
                layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
            }

            layout.spacing = RowSpacing;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            var fitter = content.GetComponent<ContentSizeFitter>();
            if (fitter == null)
            {
                fitter = content.gameObject.AddComponent<ContentSizeFitter>();
            }

            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }

        private static void HideVerticalScrollbarInGame(Transform scrollRoot)
        {
            var scrollRect = scrollRoot.GetComponent<ScrollRect>();
            var scrollbarTransform = scrollRoot.Find(VerticalScrollbarName);
            if (scrollbarTransform != null)
            {
                scrollbarTransform.gameObject.SetActive(false);
            }

            if (scrollRect != null)
            {
                scrollRect.verticalScrollbar = null;
            }
        }

        private static void ResetRowRectForLayout(RectTransform rowRect)
        {
            if (rowRect == null)
            {
                return;
            }

            rowRect.localScale = Vector3.one;
            rowRect.localRotation = Quaternion.identity;
            rowRect.anchoredPosition = Vector2.zero;
            rowRect.sizeDelta = new Vector2(rowRect.sizeDelta.x, RowHeight);
        }

        private static void ClearChildren(Transform content)
        {
            for (var i = content.childCount - 1; i >= 0; i--)
            {
                Object.DestroyImmediate(content.GetChild(i).gameObject);
            }
        }

        private static string BuildSummary(
            int buyCount,
            int sellCount,
            List<string> missingIcons,
            List<string> missingShopNames,
            bool savedScene)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Bake 完成。");
            sb.AppendLine($"- Buy 行数：{buyCount}");
            sb.AppendLine($"- Sell 行数：{sellCount}");
            sb.AppendLine("- Sell Scroll 默认隐藏，Buy 显示");
            if (savedScene)
            {
                sb.AppendLine("- 场景已 MarkDirty + Save");
            }
            else
            {
                sb.AppendLine("- Prefab 内容已烤（由外层 SaveAsPrefabAsset）");
            }

            sb.AppendLine("- Name：中文店招名图预览（Play 按当前语言 Resolve）");

            if (missingIcons.Count > 0)
            {
                sb.AppendLine($"- 未解析 Icon 的 itemId：{string.Join(", ", missingIcons)}");
            }

            if (missingShopNames.Count > 0)
            {
                sb.AppendLine($"- 缺中文店招名图 itemId：{string.Join(", ", missingShopNames)}");
            }

            return sb.ToString().TrimEnd();
        }

        private static void Report(bool showDialog, string message)
        {
            Debug.Log("[ShopListBake] " + message);
            if (showDialog)
            {
                EditorUtility.DisplayDialog("Bake Shop Lists", message, "确定");
            }
        }
    }
}
#endif
