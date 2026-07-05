#if UNITY_EDITOR
using Game.GameRuntime.UI.FormLogic.Shop;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Game.GameRuntime.UI.FormLogic.Shop.Editor
{
    /// <summary>
    /// DB-5 场景施工：Bar_ListScroll → Buy/Sell 双 Scroll、清空手摆 Shop_Bar、绑定 ShopFormLogic。
    /// 列表行数由 MainItemDatabase 过滤，不再绑定 ShopCatalogConfig。
    /// 菜单：Tools / Shop / Setup Database Driven Lists
    /// </summary>
    public static class ShopCatalogSetupEditor
    {
        private const string MenuPath = "Tools/Shop/Setup Database Driven Lists";
        private const string VillageShopScenePath = "Assets/GameRes/Scenes/Village_Shop.unity";
        private const string ShopBarPrefabPath = "Assets/GameRes/Prefabs/UI/Shop/Shop_Bar.prefab";

        private const string BarNodeName = "Bar";
        private const string BarListScrollBuyName = "Bar_ListScroll_Buy";
        private const string BarListScrollSellName = "Bar_ListScroll_Sell";
        private const string BarListScrollLegacyName = "Bar_ListScroll";
        private const string ContentName = "Content";
        /// <summary>Unity 标准 ScrollView：Content 在 Viewport 下。</summary>
        private const string ViewportContentPath = "Viewport/Content";

        [MenuItem(MenuPath)]
        private static void SetupFromMenu()
        {
            if (!EnsureVillageShopSceneOpen())
            {
                return;
            }

            RunSetup(showDialog: true);
        }

        public static void ExecuteBatchSetup()
        {
            var scene = EditorSceneManager.OpenScene(VillageShopScenePath, OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                Debug.LogError("[ShopDatabaseSetup] 无法打开场景: " + VillageShopScenePath);
                EditorApplication.Exit(1);
                return;
            }

            RunSetup(showDialog: false);
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
                    "Shop Database 驱动列表",
                    "将打开 Village_Shop 场景并修改 Bar 区（双 Scroll + 清空 Content），是否继续？",
                    "继续",
                    "取消"))
            {
                return false;
            }

            EditorSceneManager.OpenScene(VillageShopScenePath);
            return true;
        }

        private static void RunSetup(bool showDialog)
        {
            var uiShop = GameObject.Find("UI_Shop");
            if (uiShop == null)
            {
                Report(showDialog, "未找到 UI_Shop 节点。");
                return;
            }

            var bar = uiShop.transform.Find(BarNodeName);
            if (bar == null)
            {
                Report(showDialog, "未找到 UI_Shop/Bar 节点。");
                return;
            }

            var buyScroll = EnsureBuyScrollRenamed(bar);
            if (buyScroll == null)
            {
                Report(showDialog, "Bar 下缺少 Bar_ListScroll / Bar_ListScroll_Buy。");
                return;
            }

            var sellScroll = EnsureSellScrollDuplicate(bar, buyScroll);
            ClearShopBarChildren(FindScrollContent(buyScroll));
            ClearShopBarChildren(FindScrollContent(sellScroll));

            var shopBarPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ShopBarPrefabPath);
            var formLogic = uiShop.GetComponent<ShopFormLogic>();

            if (formLogic == null)
            {
                Report(showDialog, "UI_Shop 上缺少 ShopFormLogic 组件。");
                return;
            }

            var serialized = new SerializedObject(formLogic);
            serialized.FindProperty("shopBarPrefab").objectReferenceValue = shopBarPrefab;
            serialized.FindProperty("buyContent").objectReferenceValue = FindScrollContent(buyScroll);
            serialized.FindProperty("sellContent").objectReferenceValue = FindScrollContent(sellScroll);
            serialized.FindProperty("barListScrollBuy").objectReferenceValue = buyScroll.gameObject;
            serialized.FindProperty("barListScrollSell").objectReferenceValue = sellScroll.gameObject;

            var btnSell = uiShop.transform.Find("SELL")?.GetComponent<Button>();
            if (btnSell != null)
            {
                serialized.FindProperty("btnSell").objectReferenceValue = btnSell;
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();

            sellScroll.gameObject.SetActive(false);
            buyScroll.gameObject.SetActive(true);

            ShopScrollShellHelper.ApplyInteractionFixes(buyScroll);
            ShopScrollShellHelper.ApplyInteractionFixes(sellScroll);

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();

            Report(
                showDialog,
                "施工完成。\n" +
                $"- {BarListScrollBuyName} + {BarListScrollSellName}（Sell 默认隐藏）\n" +
                "- Buy/Sell Content 已清空（运行时由 MainItemDatabase 过滤生成行）\n" +
                "- ShopFormLogic 已绑定 prefab / Content / Scroll");
        }

        private static Transform EnsureBuyScrollRenamed(Transform bar)
        {
            var buy = bar.Find(BarListScrollBuyName);
            if (buy != null)
            {
                return buy;
            }

            var legacy = bar.Find(BarListScrollLegacyName);
            if (legacy != null)
            {
                legacy.name = BarListScrollBuyName;
                return legacy;
            }

            return null;
        }

        private static Transform EnsureSellScrollDuplicate(Transform bar, Transform buyScroll)
        {
            var existing = bar.Find(BarListScrollSellName);
            if (existing != null)
            {
                return existing;
            }

            var duplicate = Object.Instantiate(buyScroll.gameObject, bar);
            duplicate.name = BarListScrollSellName;
            duplicate.transform.SetSiblingIndex(buyScroll.GetSiblingIndex() + 1);

            var sellRect = duplicate.transform as RectTransform;
            var buyRect = buyScroll as RectTransform;
            if (sellRect != null && buyRect != null)
            {
                sellRect.anchorMin = buyRect.anchorMin;
                sellRect.anchorMax = buyRect.anchorMax;
                sellRect.pivot = buyRect.pivot;
                sellRect.anchoredPosition = buyRect.anchoredPosition;
                sellRect.sizeDelta = buyRect.sizeDelta;
            }

            return duplicate.transform;
        }

        /// <summary>查找 ScrollRect 的 Content：优先 Viewport/Content，兼容 ScrollRect.content 引用。</summary>
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

        private static void ClearShopBarChildren(Transform content)
        {
            if (content == null)
            {
                return;
            }

            for (var i = content.childCount - 1; i >= 0; i--)
            {
                Object.DestroyImmediate(content.GetChild(i).gameObject);
            }
        }

        private static void Report(bool showDialog, string message)
        {
            Debug.Log("[ShopDatabaseSetup] " + message);
            if (showDialog)
            {
                EditorUtility.DisplayDialog("Shop Database 驱动列表", message, "确定");
            }
        }
    }
}
#endif
