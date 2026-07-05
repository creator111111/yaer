using System.Collections.Generic;
using Game.DataTable.MainItem;
using Game.Static.Enum.Goods;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.GameRuntime.UI.FormLogic.Shop
{
    /// <summary>
    /// 商店 UI 逻辑（DB-0～DB-4：MainItemDatabase 过滤驱动 Shop_Bar + 双 Scroll Tab + TxtTotal）。
    /// 购买页 = CostItem 且 buyPrice&gt;=0；出售页 = MaterialItem 且 sellPrice&gt;=0。
    /// </summary>
    public class ShopFormLogic : MonoBehaviour
    {
        private const string BarNodeName = "Bar";
        private const string BarListScrollBuyName = "Bar_ListScroll_Buy";
        private const string BarListScrollSellName = "Bar_ListScroll_Sell";
        private const string BarListScrollLegacyName = "Bar_ListScroll";
        private const string ViewportContentPath = "Viewport/Content";
        private const string TxtTotalName = "TxtTotal";
        private const string BtnConfirmName = "BtnConfirm";
        private const string BtnSellNodeName = "SELL";
        private const string DefaultShopBarPrefabPath = "Assets/GameRes/Prefabs/UI/Shop/Shop_Bar.prefab";

        [Header("列表 · Database 驱动")]
        [SerializeField] private GameObject shopBarPrefab;
        [SerializeField] private Transform buyContent;
        [SerializeField] private Transform sellContent;

        [Header("Tab · 双 Scroll")]
        [SerializeField] private Button btnBuy;
        [SerializeField] private Button btnSell;
        [SerializeField] private GameObject barListScrollBuy;
        [SerializeField] private GameObject barListScrollSell;

        [Header("阶段三 · 底部合计（生命珠数量 × 单价）")]
        [SerializeField] private Text txtTotal;
        [SerializeField] private TextMeshProUGUI txtTotalTmp;

        [Header("阶段四 · 决定按钮（假购买 Debug Log）")]
        [SerializeField] private Button btnConfirm;

        private readonly List<ShopBarRowView> _buyRowViews = new List<ShopBarRowView>();
        private ShopBuyRowQuantityInput _hpBallQuantityInput;

        private void Awake()
        {
            MainItemDefProvider.DefinitionsRebuilt += OnDefinitionsRebuilt;
            ResolveShopReferences();
            EnsureDualScrollShell();
            ApplyScrollInteractionFixes();
            MainItemDefProvider.EnsureLoaded();
            RefreshBuyList();
            RefreshSellList();
            ResolveTotalTextReference();
            WireBuyTabButton();
            WireSellTabButton();
            ResolveConfirmButtonReference();
            WireConfirmButton();
        }

        private void Start()
        {
            // Database 异步加载完成时，Awake 可能 0 行；Start 再刷一次。
            if (_buyRowViews.Count == 0)
            {
                RefreshBuyList();
                RefreshSellList();
            }

            SwitchToBuyTab();
        }

        private void OnDestroy()
        {
            MainItemDefProvider.DefinitionsRebuilt -= OnDefinitionsRebuilt;
            UnwireHpBallTotalRefresh();

            if (btnConfirm != null)
            {
                btnConfirm.onClick.RemoveListener(OnConfirmClick);
            }

            if (btnSell != null)
            {
                btnSell.onClick.RemoveListener(SwitchToSellTab);
            }
        }

        public void SwitchToBuyTab()
        {
            SetScrollActive(barListScrollBuy, true);
            SetScrollActive(barListScrollSell, false);
            ResetAllBuyQuantityInputs();
            RefreshHpBallBuyTotal();
        }

        public void SwitchToSellTab()
        {
            SetScrollActive(barListScrollBuy, false);
            SetScrollActive(barListScrollSell, true);
            Debug.Log($"{ShopDebugLogger.LogPrefix} 切换到出售页");
        }

        /// <summary>清空 Buy Content，按 GetShopBuyCandidates() Instantiate + Bind。</summary>
        public void RefreshBuyList()
        {
            EnsureShopBarPrefabResolved();
            if (!ValidateListRefreshInputs(buyContent, "RefreshBuyList"))
            {
                return;
            }

            ClearContentChildren(buyContent);
            _buyRowViews.Clear();

            foreach (var def in MainItemDefProvider.GetShopBuyCandidates())
            {
                if (def == null)
                {
                    continue;
                }

                var rowGo = InstantiateShopBarRow(buyContent);
                if (rowGo == null)
                {
                    continue;
                }

                rowGo.name = $"Shop_Bar_{def.ItemId}";

                var rowView = EnsureRowView(rowGo);
                rowView.Bind(def, isBuyRow: true);
                EnsureRowQuantityComponent(rowGo);
                _buyRowViews.Add(rowView);
            }

            CacheHpBallQuantityInput();
            WireHpBallTotalRefresh();
        }

        /// <summary>清空 Sell Content，按 GetShopSellCandidates() Instantiate + Bind。</summary>
        public void RefreshSellList()
        {
            EnsureShopBarPrefabResolved();
            if (!ValidateListRefreshInputs(sellContent, "RefreshSellList"))
            {
                return;
            }

            ClearContentChildren(sellContent);

            foreach (var def in MainItemDefProvider.GetShopSellCandidates())
            {
                if (def == null)
                {
                    continue;
                }

                var rowGo = InstantiateShopBarRow(sellContent);
                if (rowGo == null)
                {
                    continue;
                }
                rowGo.name = $"Shop_Bar_{def.ItemId}";

                var rowView = EnsureRowView(rowGo);
                rowView.Bind(def, isBuyRow: false);
            }
        }

        public int GetBuyQuantity(EMainItemName itemName)
        {
            foreach (var rowView in _buyRowViews)
            {
                if (rowView == null || rowView.ItemId != itemName)
                {
                    continue;
                }

                var input = rowView.GetComponent<ShopBuyRowQuantityInput>();
                return input != null ? input.Quantity : ShopQuantityInputHelper.DefaultQuantity;
            }

            return 0;
        }

        /// <summary>总价 = HpBall 行数量 × MainItemDatabase 中 HpBall.buyPrice。</summary>
        public int GetCurrentHpBallBuyTotal()
        {
            var hpRow = FindBuyRow(EMainItemName.HpBall);
            var unitPrice = hpRow != null ? hpRow.Price : 0;
            var quantity = _hpBallQuantityInput != null ? _hpBallQuantityInput.QuantityForTotal : 0;
            return quantity * unitPrice;
        }

        public void RefreshHpBallBuyTotal()
        {
            SetTotalText(GetCurrentHpBallBuyTotal().ToString());
        }

        /// <summary>
        /// 阶段四：假购买 Log；单价来自 MainItemDatabase，阶段五接真扣款。
        /// </summary>
        public void OnConfirmClick()
        {
            var quantity = _hpBallQuantityInput != null ? _hpBallQuantityInput.QuantityForTotal : 0;
            if (quantity <= 0)
            {
                ShopDebugLogger.LogZeroQuantityWarning();
                return;
            }

            var total = GetCurrentHpBallBuyTotal();
            if (total <= 0)
            {
                ShopDebugLogger.LogZeroQuantityWarning();
                return;
            }

            ShopDebugLogger.LogHpBallPurchaseSuccess(total);
        }

        private void ResolveShopReferences()
        {
            EnsureShopBarPrefabResolved();

            var bar = transform.Find(BarNodeName);
            if (bar == null)
            {
                return;
            }

            // 每次 Play 从 Hierarchy 重绑，避免 Inspector 误拖 Shop_Bar 等到 Scroll 字段导致 Instantiate InvalidCast。
            var buyScroll = bar.Find(BarListScrollBuyName) ?? bar.Find(BarListScrollLegacyName);
            if (buyScroll != null)
            {
                barListScrollBuy = buyScroll.gameObject;
                if (barListScrollBuy.name == BarListScrollLegacyName)
                {
                    barListScrollBuy.name = BarListScrollBuyName;
                }
            }
            else if (IsUnityObjectAlive(barListScrollBuy) && barListScrollBuy.GetComponent<ScrollRect>() == null)
            {
                barListScrollBuy = null;
            }

            var sellScroll = bar.Find(BarListScrollSellName);
            if (sellScroll != null)
            {
                barListScrollSell = sellScroll.gameObject;
            }
            else if (IsUnityObjectAlive(barListScrollSell) && barListScrollSell.GetComponent<ScrollRect>() == null)
            {
                barListScrollSell = null;
            }

            buyContent = ResolveScrollContent(buyScroll ?? barListScrollBuy?.transform);
            sellContent = ResolveScrollContent(sellScroll ?? barListScrollSell?.transform);

            if (btnSell == null)
            {
                btnSell = FindDeepChild(transform, BtnSellNodeName)?.GetComponent<Button>();
            }

            if (!IsShopBarPrefabUsable(shopBarPrefab))
            {
                Debug.LogWarning(
                    $"[ShopFormLogic] shopBarPrefab 无效；Editor 应从 {DefaultShopBarPrefabPath} 加载。",
                    this);
            }

            if (barListScrollSell == null)
            {
                Debug.LogWarning(
                    "[ShopFormLogic] 未找到 Bar_ListScroll_Sell；Play 时将尝试 Duplicate Buy Scroll。",
                    this);
            }
        }

        /// <summary>
        /// Play 时同步 shopBarPrefab：Editor 固定路径 LoadAsset，不读可能 Missing 的旧序列化引用。
        /// 替代方案：Resources.Load("Shop_Bar") — 需把 prefab 挪到 Resources 目录。
        /// </summary>
        private void EnsureShopBarPrefabResolved()
        {
#if UNITY_EDITOR
            var loaded = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(DefaultShopBarPrefabPath);
            if (loaded != null)
            {
                shopBarPrefab = loaded;
                return;
            }

            Debug.LogError($"[ShopFormLogic] 未找到 Shop_Bar 预制体：{DefaultShopBarPrefabPath}", this);
#else
            if (IsShopBarPrefabUsable(shopBarPrefab))
            {
                return;
            }

            shopBarPrefab = null;
            Debug.LogWarning(
                "[ShopFormLogic] shopBarPrefab 丢失；请在 Inspector 绑定 Shop_Bar.prefab 或接入 Res 加载。",
                this);
#endif
        }

        /// <summary>
        /// Fix-L1 运行时兜底：场景未跑 Setup 菜单时，Duplicate Buy → Sell 并清空 Sell Content。
        /// 替代方案：在 Editor 跑 Tools/Shop/Setup Database Driven Lists 持久化到场景。
        /// </summary>
        private void EnsureDualScrollShell()
        {
            if (barListScrollSell != null)
            {
                return;
            }

            if (barListScrollBuy == null || barListScrollBuy.GetComponent<ScrollRect>() == null)
            {
                Debug.LogWarning("[ShopFormLogic] 无法复制 Sell Scroll：Buy Scroll 未就绪。", this);
                return;
            }

            var bar = transform.Find(BarNodeName);
            if (bar == null)
            {
                return;
            }

            // 显式 GameObject.Instantiate，避免 MonoBehaviour.Instantiate<T> 对非 GameObject 原生对象 cast 失败。
            var duplicate = CloneScrollShell(barListScrollBuy, bar);
            if (duplicate == null)
            {
                Debug.LogWarning(
                    "[ShopFormLogic] Sell Scroll 复制失败；请运行 Tools/Shop/Setup Database Driven Lists。",
                    this);
                return;
            }

            duplicate.name = BarListScrollSellName;
            duplicate.transform.SetSiblingIndex(barListScrollBuy.transform.GetSiblingIndex() + 1);

            var sellRect = duplicate.transform as RectTransform;
            var buyRect = barListScrollBuy.transform as RectTransform;
            if (sellRect != null && buyRect != null)
            {
                sellRect.anchorMin = buyRect.anchorMin;
                sellRect.anchorMax = buyRect.anchorMax;
                sellRect.pivot = buyRect.pivot;
                sellRect.anchoredPosition = buyRect.anchoredPosition;
                sellRect.sizeDelta = buyRect.sizeDelta;
            }

            barListScrollSell = duplicate;
            sellContent = ResolveScrollContent(duplicate.transform);
            ClearContentChildren(sellContent);

            duplicate.SetActive(false);
            barListScrollBuy.SetActive(true);

            if (btnSell == null)
            {
                btnSell = FindDeepChild(transform, BtnSellNodeName)?.GetComponent<Button>();
                WireSellTabButton();
            }
        }

        /// <summary>Fix-S1/S2：Buy/Sell Scroll 统一修正 Viewport 与滚轮灵敏度。</summary>
        private void ApplyScrollInteractionFixes()
        {
            if (barListScrollBuy != null)
            {
                ShopScrollShellHelper.ApplyInteractionFixes(barListScrollBuy.transform);
            }

            if (barListScrollSell != null)
            {
                ShopScrollShellHelper.ApplyInteractionFixes(barListScrollSell.transform);
            }
        }

        /// <summary>Fix-I3：图集异步 Load 完成、RebuildCache 后重刷 Icon。</summary>
        private void OnDefinitionsRebuilt()
        {
            RefreshBuyList();
            RefreshSellList();
        }

        private bool ValidateListRefreshInputs(Transform content, string caller)
        {
            if (content == null || !IsShopBarPrefabUsable(shopBarPrefab))
            {
                Debug.LogWarning($"[ShopFormLogic] {caller} 跳过：Content / shopBarPrefab 未就绪。", this);
                return false;
            }

            return true;
        }

        /// <summary>Scroll Content：优先 Viewport/Content，回退 ScrollRect.content（与 Editor 工具一致）。</summary>
        private static Transform ResolveScrollContent(Transform scrollRoot)
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

            return scrollRoot.Find("Content");
        }

        /// <summary>复制 Buy Scroll 为 Sell；失败返回 null 而不抛 InvalidCastException。</summary>
        private static GameObject CloneScrollShell(GameObject buyScrollGo, Transform parent)
        {
            if (buyScrollGo == null || parent == null || buyScrollGo.GetComponent<ScrollRect>() == null)
            {
                return null;
            }

            var clone = Object.Instantiate(buyScrollGo, parent, false);
            return clone;
        }

        /// <summary>实例化 Shop_Bar 行；使用非泛型 Instantiate 避免 prefab 引用类型不匹配。</summary>
        private GameObject InstantiateShopBarRow(Transform parent)
        {
            EnsureShopBarPrefabResolved();
            if (parent == null || !IsShopBarPrefabUsable(shopBarPrefab))
            {
                return null;
            }

            return Object.Instantiate(shopBarPrefab, parent, false);
        }

        /// <summary>Unity 假 null：含 MissingReference 的序列化引用。</summary>
        private static bool IsUnityObjectAlive(Object obj)
        {
            return obj != null;
        }

        /// <summary>shopBarPrefab 须为 Shop_Bar 行预制体；访问前须已 EnsureShopBarPrefabResolved。</summary>
        private static bool IsShopBarPrefabUsable(GameObject prefab)
        {
            try
            {
                if (prefab == null)
                {
                    return false;
                }

                if (prefab.GetComponent<ScrollRect>() != null)
                {
                    return false;
                }

                return prefab.GetComponent<ShopBarRowView>() != null || prefab.name.StartsWith("Shop_Bar");
            }
            catch (MissingReferenceException)
            {
                return false;
            }
        }

        private static void ClearContentChildren(Transform content)
        {
            for (var i = content.childCount - 1; i >= 0; i--)
            {
                var child = content.GetChild(i);
                if (Application.isPlaying)
                {
                    Destroy(child.gameObject);
                }
                else
                {
                    DestroyImmediate(child.gameObject);
                }
            }
        }

        private static ShopBarRowView EnsureRowView(GameObject rowGo)
        {
            var rowView = rowGo.GetComponent<ShopBarRowView>();
            if (rowView == null)
            {
                rowView = rowGo.AddComponent<ShopBarRowView>();
            }

            return rowView;
        }

        private static ShopBuyRowQuantityInput EnsureRowQuantityComponent(GameObject row)
        {
            if (row == null)
            {
                return null;
            }

            var input = row.GetComponent<ShopBuyRowQuantityInput>();
            if (input == null)
            {
                input = row.AddComponent<ShopBuyRowQuantityInput>();
            }

            return input;
        }

        private ShopBarRowView FindBuyRow(EMainItemName itemId)
        {
            foreach (var rowView in _buyRowViews)
            {
                if (rowView != null && rowView.ItemId == itemId)
                {
                    return rowView;
                }
            }

            return null;
        }

        private void CacheHpBallQuantityInput()
        {
            var hpRow = FindBuyRow(EMainItemName.HpBall);
            _hpBallQuantityInput = hpRow != null
                ? hpRow.GetComponent<ShopBuyRowQuantityInput>()
                : null;
        }

        private void ResolveTotalTextReference()
        {
            if (txtTotal != null || txtTotalTmp != null)
            {
                return;
            }

            var totalNode = FindDeepChild(transform, TxtTotalName);
            if (totalNode == null)
            {
                Debug.LogWarning("[ShopFormLogic] 未找到 TxtTotal；请在 Canvas 下放置合计文本。", this);
                return;
            }

            txtTotalTmp = totalNode.GetComponent<TextMeshProUGUI>();
            txtTotal = totalNode.GetComponent<Text>();
        }

        private void WireHpBallTotalRefresh()
        {
            UnwireHpBallTotalRefresh();

            if (_hpBallQuantityInput == null)
            {
                return;
            }

            _hpBallQuantityInput.OnQuantityValueChanged += RefreshHpBallBuyTotal;
        }

        private void UnwireHpBallTotalRefresh()
        {
            if (_hpBallQuantityInput != null)
            {
                _hpBallQuantityInput.OnQuantityValueChanged -= RefreshHpBallBuyTotal;
            }
        }

        private void SetTotalText(string text)
        {
            if (txtTotalTmp != null)
            {
                txtTotalTmp.text = text;
            }

            if (txtTotal != null)
            {
                txtTotal.text = text;
            }
        }

        private void WireBuyTabButton()
        {
            if (btnBuy == null)
            {
                return;
            }

            btnBuy.onClick.RemoveListener(SwitchToBuyTab);
            btnBuy.onClick.AddListener(SwitchToBuyTab);
        }

        private void WireSellTabButton()
        {
            if (btnSell == null)
            {
                return;
            }

            btnSell.onClick.RemoveListener(SwitchToSellTab);
            btnSell.onClick.AddListener(SwitchToSellTab);
        }

        private void ResolveConfirmButtonReference()
        {
            if (btnConfirm != null)
            {
                return;
            }

            var confirmNode = FindDeepChild(transform, BtnConfirmName);
            if (confirmNode != null)
            {
                btnConfirm = confirmNode.GetComponent<Button>();
            }

            if (btnConfirm == null)
            {
                Debug.LogWarning("[ShopFormLogic] 未找到 BtnConfirm；请在底部添加「决定」按钮。", this);
            }
        }

        private void WireConfirmButton()
        {
            if (btnConfirm == null)
            {
                return;
            }

            btnConfirm.onClick.RemoveListener(OnConfirmClick);
            btnConfirm.onClick.AddListener(OnConfirmClick);
        }

        private void ResetAllBuyQuantityInputs()
        {
            foreach (var rowView in _buyRowViews)
            {
                if (rowView == null)
                {
                    continue;
                }

                var input = rowView.GetComponent<ShopBuyRowQuantityInput>();
                input?.EnsureListening();
                input?.ResetToDefault();
            }
        }

        private static void SetScrollActive(GameObject scrollRoot, bool active)
        {
            if (scrollRoot != null)
            {
                scrollRoot.SetActive(active);
            }
        }

        public bool UsesScrollListLayout()
        {
            var bar = transform.Find(BarNodeName);
            if (bar == null)
            {
                return false;
            }

            return bar.Find(BarListScrollBuyName) != null || bar.Find(BarListScrollLegacyName) != null;
        }

        private static Transform FindDeepChild(Transform root, string childName)
        {
            if (root == null)
            {
                return null;
            }

            if (root.name == childName)
            {
                return root;
            }

            for (var i = 0; i < root.childCount; i++)
            {
                var found = FindDeepChild(root.GetChild(i), childName);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }
    }
}
