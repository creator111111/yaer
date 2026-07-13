#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Game.GameRuntime.UI.FormLogic.Shop.Editor
{
    /// <summary>
    /// 0704 Shop_Bar 列表滚动壳 · SC-0～SC-4 一键施工。
    /// 依据：Assets/Doc/执行文档/0704/Shop_Bar列表滚动_架构溯源与施工执行说明.md
    ///
    /// 菜单：Tools / Shop / Setup Bar List Scroll (SC-0~SC-4)
    ///
    /// 替代方案（不跑本脚本时）：
    /// 1. 在 Unity 中手动 UI → Scroll View，按文档 §4 搭 Hierarchy；
    /// 2. 接数据阶段再改 ShopFormLogic，由 VerticalLayoutGroup 接管行距，跳过 LayoutBuyRowsVertically。
    /// </summary>
    public static class ShopBarListScrollSetupEditor
    {
        private const string MenuPath = "Tools/Shop/Setup Bar List Scroll (SC-0~SC-4)";

        /// <summary>测试场景路径（Village_Shop · UI_Shop/Bar）。</summary>
        private const string VillageShopScenePath = "Assets/GameRes/Scenes/Village_Shop.unity";

        /// <summary>行预制体；本阶段仅作 Content 下空壳占位。</summary>
        private const string ShopBarPrefabPath = "Assets/GameRes/Prefabs/UI/Shop/Shop_Bar.prefab";

        private const string BarNodeName = "Bar";
        private const string BarBgName = "Bar_BG";
        private const string BarListScrollName = "Bar_ListScroll";
        private const string ContentName = "Content";
        private const string ViewportName = "Viewport";
        /// <summary>Unity 标准 ScrollView：Content 在 Viewport 下，非 Scroll 根的直接子节点。</summary>
        private const string ViewportContentPath = "Viewport/Content";
        private const string VerticalScrollbarName = "Scrollbar Vertical";

        /// <summary>Shop_Bar.prefab 行高（SizeDelta.y）。</summary>
        private const float RowHeight = 88f;

        /// <summary>VerticalLayoutGroup 行距；Play 测试 2026-07-05 由 8 调至 16。</summary>
        private const float RowSpacing = 16f;

        /// <summary>可见行数；Viewport 高 = RowHeight×6 + RowSpacing×5。</summary>
        private const int VisibleRowCount = 6;

        /// <summary>SC-2 验收用占位行总数（超过 6 行才需滚动）。</summary>
        private const int TestPlaceholderRowCount = 8;

        /// <summary>Viewport 高度 = 88×6 + 8×5 = 568。</summary>
        private static float ViewportHeight => RowHeight * VisibleRowCount + RowSpacing * (VisibleRowCount - 1);

        [MenuItem(MenuPath)]
        private static void SetupFromMenu()
        {
            if (!EditorUtility.DisplayDialog(
                    "菜单已合并",
                    "「Setup Bar List Scroll」已合并到「Bake Shop Lists From MainItemDatabase」。\n是否立即执行 Bake？",
                    "执行 Bake",
                    "取消"))
            {
                return;
            }

            ShopListBakeEditor.BakeFromMenu();
        }

        /// <summary>已废弃：请使用 ShopListBakeEditor.ExecuteBatchBake。</summary>
        [System.Obsolete("Use ShopListBakeEditor.ExecuteBatchBake instead.")]
        public static void ExecuteBatchSetup()
        {
            ShopListBakeEditor.ExecuteBatchBake();
        }

        private static void RunSetup_Obsolete(bool showDialog)
        {
            var bar = FindBarTransform();
            if (bar == null)
            {
                Report(showDialog, "未找到 UI_Shop/Bar 节点，请确认 Village_Shop 场景。");
                return;
            }

            // SC-0：Bar/BG → Bar_BG，Raycast 关闭以免挡滚动。
            var barBg = EnsureBarBackground(bar);

            // SC-0：创建或更新 Bar_ListScroll（购买/出售共用唯一 ScrollView）。
            var scrollRoot = EnsureBarListScroll(bar, barBg);

            // SC-1 / SC-2：迁入 Shop_Bar 并补足 8 条测试空壳。
            var content = FindScrollContent(scrollRoot);
            if (content == null)
            {
                Report(showDialog, "Bar_ListScroll 下缺少 Viewport/Content，请检查 Scroll View 结构。");
                return;
            }

            var shopBarPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ShopBarPrefabPath);
            if (shopBarPrefab == null)
            {
                Report(showDialog, "未找到 Shop_Bar.prefab: " + ShopBarPrefabPath);
                return;
            }

            MigrateExistingShopBarRows(bar, content);
            EnsureTestPlaceholderRows(content, shopBarPrefab);

            // SC-4：运行时隐藏侧边滚动条，仅保留滚轮滚动。
            HideVerticalScrollbarInGame(scrollRoot);

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();

            Report(
                showDialog,
                $"施工完成。\n" +
                $"- {BarBgName} + {BarListScrollName}/Viewport/Content\n" +
                $"- Content 行距 {RowSpacing}px，Viewport 对齐 Bar_BG 高度\n" +
                $"- 占位行 {TestPlaceholderRowCount} 条（Play 测滚轮/滚动条）");
        }

        private static Transform FindBarTransform()
        {
            var uiShop = GameObject.Find("UI_Shop");
            if (uiShop == null)
            {
                return null;
            }

            return uiShop.transform.Find(BarNodeName);
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
                Debug.LogError("[ShopBarListScroll] Bar 下缺少 BG/Bar_BG。");
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
        /// SC-0：用 DefaultControls 创建标准 ScrollRect 壳；已存在则只校正 Viewport/Content 参数。
        /// </summary>
        private static Transform EnsureBarListScroll(Transform bar, RectTransform barBg)
        {
            var existing = bar.Find(BarListScrollName);
            GameObject scrollGo;

            if (existing != null)
            {
                scrollGo = existing.gameObject;
            }
            else
            {
                scrollGo = DefaultControls.CreateScrollView(new DefaultControls.Resources());
                scrollGo.name = BarListScrollName;
                scrollGo.transform.SetParent(bar, false);

                // 删除水平滚动条；保留 Vertical Scrollbar 供 SC-4 接线。
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
            ShopScrollShellHelper.ApplyInteractionFixes(scrollGo.transform);

            return scrollGo.transform;
        }

        /// <summary>查找 ScrollRect 的 Content：优先 Viewport/Content，兼容旧场景直接挂 Content。</summary>
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

            // 兜底：ScrollRect 组件上已绑定的 content（场景手改 Hierarchy 时仍可用）。
            var scrollRect = scrollRoot.GetComponent<ScrollRect>();
            if (scrollRect != null && scrollRect.content != null)
            {
                return scrollRect.content;
            }

            return scrollRoot.Find(ContentName);
        }

        /// <summary>Bar_ListScroll 与 Bar_BG 同位置同宽；高度用 Viewport 公式（568）。</summary>
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
            // 与 Bar_BG 同高同宽，避免 Viewport 超出底图内缘。
            scrollRect.sizeDelta = barBg.sizeDelta;
        }

        /// <summary>
        /// Viewport：RectMask2D 裁剪；Fix-S1 去掉冗余 Image（raycast 改由 Scroll 根承担）。
        /// Softness 虚化由紧随其后的 ShopScrollShellHelper.ApplyInteractionFixes 统一写入，禁止在此写魔法数。
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
            // 高度跟随 Bar_ListScroll（= Bar_BG），与行距解耦。
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

        /// <summary>Content：VerticalLayoutGroup Spacing=16 + ContentSizeFitter 竖向 Preferred。</summary>
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

        /// <summary>SC-1：把 Bar 下直接挂的 Shop_Bar 迁入 Content（不绑 ShopFormLogic）。</summary>
        private static void MigrateExistingShopBarRows(Transform bar, Transform content)
        {
            for (var i = bar.childCount - 1; i >= 0; i--)
            {
                var child = bar.GetChild(i);
                if (child == content.parent || child.name == BarBgName || child.name == BarListScrollName)
                {
                    continue;
                }

                if (!child.name.StartsWith("Shop_Bar"))
                {
                    continue;
                }

                child.SetParent(content, false);
                ResetRowRectForLayout(child as RectTransform);
            }
        }

        /// <summary>SC-2：补足 TestPlaceholderRowCount 条空壳，用于验证「超过 6 行才滚动」。</summary>
        private static void EnsureTestPlaceholderRows(Transform content, GameObject shopBarPrefab)
        {
            var existingCount = 0;
            for (var i = 0; i < content.childCount; i++)
            {
                if (content.GetChild(i).name.StartsWith("Shop_Bar"))
                {
                    existingCount++;
                }
            }

            for (var i = existingCount; i < TestPlaceholderRowCount; i++)
            {
                var row = (GameObject)PrefabUtility.InstantiatePrefab(shopBarPrefab, content);
                row.name = i == 0 ? "Shop_Bar" : "Shop_Bar (" + i + ")";
                ResetRowRectForLayout(row.transform as RectTransform);
            }

            // 统一刷新 Layout，避免首次 Play 行重叠。
            LayoutRebuilder.ForceRebuildLayoutImmediate(content as RectTransform);
        }

        /// <summary>Layout 组接管尺寸后，清零 anchoredPosition，保留预制体行高。</summary>
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

        /// <summary>SC-4：禁用 Scrollbar Vertical，ScrollRect 不绑引用；游戏里仅滚轮滚动。</summary>
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

        private static void Report(bool showDialog, string message)
        {
            Debug.Log("[ShopBarListScroll] " + message);
            if (showDialog)
            {
                EditorUtility.DisplayDialog("Shop Bar 列表滚动", message, "确定");
            }
        }
    }
}
#endif
