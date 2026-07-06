using System.Collections.Generic;
using Game.GameRuntime.UI.Component;
using Game.Static.Enum.Goods;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.GameRuntime.UI.FormLogic.Shop
{
    /// <summary>
    /// 商店 UI 逻辑（EB 烘焙 + ST Total2 + IMG 图片数字）：
    /// Total2 按 Tab 显示 Σ(Number×单价) 的图片数字；Number 为隐形输入 + DigitStrip。
    /// </summary>
    public class ShopFormLogic : MonoBehaviour
    {
        private const string BarNodeName = "Bar";
        private const string BarListScrollBuyName = "Bar_ListScroll_Buy";
        private const string BarListScrollSellName = "Bar_ListScroll_Sell";
        private const string BarListScrollLegacyName = "Bar_ListScroll";
        private const string ViewportContentPath = "Viewport/Content";
        private const string Total2NodeName = "Total2";
        private const string Total2DigitsNodeName = "Total2_Digits";
        private const string TxtTotal2LegacyNodeName = "TxtTotal2";
        private const string TxtTotalLegacyName = "TxtTotal";
        private const string BtnConfirmName = "BtnConfirm";
        private const string BtnSellNodeName = "SELL";

        [Header("列表 · Editor Bake 后绑定")]
        [SerializeField] private Transform buyContent;
        [SerializeField] private Transform sellContent;

        [Header("Tab · 双 Scroll")]
        [SerializeField] private Button btnBuy;
        [SerializeField] private Button btnSell;
        [SerializeField] private GameObject barListScrollBuy;
        [SerializeField] private GameObject barListScrollSell;

        [Header("IMG · Total2 图片合计（购买 Σ 买价 / 出售 Σ 卖价）")]
        [SerializeField] private UiSpriteNumberDisplay total2Digits;

        // 兼容旧场景：无 DigitStrip 时回退字体
        private Text _txtTotal2Fallback;
        private TextMeshProUGUI _txtTotal2TmpFallback;

        [Header("阶段四 · 决定按钮（假购买 Debug Log · 仍仅 HpBall）")]
        [SerializeField] private Button btnConfirm;

        private readonly List<ShopBarRowView> _buyRowViews = new List<ShopBarRowView>();
        private readonly List<ShopBarRowView> _sellRowViews = new List<ShopBarRowView>();
        private readonly List<ShopBuyRowQuantityInput> _wiredQuantityInputs = new List<ShopBuyRowQuantityInput>();
        private bool _isBuyTabActive = true;

        private void Awake()
        {
            ResolveShopReferences();
            EnsureDualScrollShell();
            ApplyScrollInteractionFixes();
            CollectBuyRowViews();
            CollectSellRowViews();
            ResolveTotal2DigitsReference();
            WireAllRowQuantityRefresh();
            WireBuyTabButton();
            WireSellTabButton();
            ResolveConfirmButtonReference();
            WireConfirmButton();
        }

        private void Start()
        {
            SwitchToBuyTab();
        }

        private void OnDestroy()
        {
            UnwireAllRowQuantityRefresh();

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
            _isBuyTabActive = true;
            SetScrollActive(barListScrollBuy, true);
            SetScrollActive(barListScrollSell, false);
            ResetAllBuyQuantityInputs();
            RefreshTotal2();
        }

        public void SwitchToSellTab()
        {
            _isBuyTabActive = false;
            SetScrollActive(barListScrollBuy, false);
            SetScrollActive(barListScrollSell, true);
            ResetAllSellQuantityInputs();
            RefreshTotal2();
            Debug.Log($"{ShopDebugLogger.LogPrefix} 切换到出售页");
        }

        /// <summary>从 buyContent 已有子节点收集 ShopBarRowView，不再 Instantiate。</summary>
        private void CollectBuyRowViews()
        {
            CollectRowViews(buyContent, _buyRowViews, "buyContent");
        }

        /// <summary>从 sellContent 已有子节点收集 ShopBarRowView。</summary>
        private void CollectSellRowViews()
        {
            CollectRowViews(sellContent, _sellRowViews, "sellContent");
        }

        private void CollectRowViews(Transform content, List<ShopBarRowView> buffer, string label)
        {
            buffer.Clear();

            if (content == null)
            {
                Debug.LogWarning($"[ShopFormLogic] {label} 未绑定；请运行 Bake 菜单。", this);
                return;
            }

            for (var i = 0; i < content.childCount; i++)
            {
                var rowView = content.GetChild(i).GetComponent<ShopBarRowView>();
                if (rowView != null)
                {
                    buffer.Add(rowView);
                }
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

        /// <summary>购买 Tab：Σ(每行 QuantityForTotal × ShopBarRowView.Price)。</summary>
        public int GetCurrentBuyTotal()
        {
            return SumRowTotals(_buyRowViews);
        }

        /// <summary>出售 Tab：Σ(每行 QuantityForTotal × ShopBarRowView.Price)。</summary>
        public int GetCurrentSellTotal()
        {
            return SumRowTotals(_sellRowViews);
        }

        /// <summary>按当前 Tab 刷新 Total2 文案。</summary>
        public void RefreshTotal2()
        {
            var total = _isBuyTabActive ? GetCurrentBuyTotal() : GetCurrentSellTotal();
            SetTotal2Number(total);
        }

        /// <summary>
        /// 阶段四：假购买 Log（仍仅 HpBall）；Total2 全行合计见 <see cref="GetCurrentBuyTotal"/>。
        /// </summary>
        public void OnConfirmClick()
        {
            var hpRow = FindBuyRow(EMainItemName.HpBall);
            var hpInput = hpRow != null ? hpRow.GetComponent<ShopBuyRowQuantityInput>() : null;
            var quantity = hpInput != null ? hpInput.QuantityForTotal : 0;
            if (quantity <= 0)
            {
                ShopDebugLogger.LogZeroQuantityWarning();
                return;
            }

            var unitPrice = hpRow != null ? hpRow.Price : 0;
            var total = quantity * unitPrice;
            if (total <= 0)
            {
                ShopDebugLogger.LogZeroQuantityWarning();
                return;
            }

            ShopDebugLogger.LogHpBallPurchaseSuccess(total);
        }

        private void ResolveShopReferences()
        {
            var bar = transform.Find(BarNodeName);
            if (bar == null)
            {
                return;
            }

            var buyScroll = bar.Find(BarListScrollBuyName) ?? bar.Find(BarListScrollLegacyName);
            if (buyScroll != null)
            {
                barListScrollBuy = buyScroll.gameObject;
                if (barListScrollBuy.name == BarListScrollLegacyName)
                {
                    barListScrollBuy.name = BarListScrollBuyName;
                }
            }
            else if (barListScrollBuy != null && barListScrollBuy.GetComponent<ScrollRect>() == null)
            {
                barListScrollBuy = null;
            }

            var sellScroll = bar.Find(BarListScrollSellName);
            if (sellScroll != null)
            {
                barListScrollSell = sellScroll.gameObject;
            }
            else if (barListScrollSell != null && barListScrollSell.GetComponent<ScrollRect>() == null)
            {
                barListScrollSell = null;
            }

            buyContent = ResolveScrollContent(buyScroll ?? barListScrollBuy?.transform);
            sellContent = ResolveScrollContent(sellScroll ?? barListScrollSell?.transform);

            if (btnSell == null)
            {
                btnSell = FindDeepChild(transform, BtnSellNodeName)?.GetComponent<Button>();
            }
        }

        private void EnsureDualScrollShell()
        {
            if (barListScrollSell != null)
            {
                return;
            }

            Debug.LogWarning(
                "[ShopFormLogic] Bar_ListScroll_Sell 未就绪；请运行 Tools/Shop/Bake Shop Lists From MainItemDatabase。",
                this);
        }

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

        /// <summary>Σ qty×price；单价来自 Bake 的 ShopBarRowView.Price，数量空串按 0。</summary>
        private static int SumRowTotals(IReadOnlyList<ShopBarRowView> rows)
        {
            var sum = 0;
            foreach (var rowView in rows)
            {
                if (rowView == null)
                {
                    continue;
                }

                var input = rowView.GetComponent<ShopBuyRowQuantityInput>();
                var quantity = input != null ? input.QuantityForTotal : 0;
                sum += quantity * rowView.Price;
            }

            return sum;
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

        /// <summary>
        /// 解析 Total2 图片合计：Total2/Total2_Digits → Total2 自身 DigitStrip → 兼容 TxtTotal2/TxtTotal 字体。
        /// </summary>
        private void ResolveTotal2DigitsReference()
        {
            if (total2Digits != null)
            {
                total2Digits.ApplyShopTotalLayout();
                return;
            }

            var total2 = transform.Find(Total2NodeName);
            if (total2 != null)
            {
                var digitsNode = total2.Find(Total2DigitsNodeName);
                if (digitsNode != null)
                {
                    total2Digits = digitsNode.GetComponent<UiSpriteNumberDisplay>();
                }

                if (total2Digits == null)
                {
                    total2Digits = UiSpriteNumberDisplay.FindUnder(total2);
                }

                if (total2Digits == null)
                {
                    total2Digits = UiSpriteNumberDisplay.EnsureOn(
                        total2,
                        TextAnchor.MiddleCenter,
                        stripSpacing: UiSpriteNumberDisplay.ShopTotalSpacing,
                        capacity: UiSpriteNumberDisplay.ShopTotalPoolCapacity);
                    total2Digits.TryLoadDefaultSpritesIfEmpty();
                    total2Digits.ApplyShopTotalLayout();
                }
            }

            if (total2Digits != null)
            {
                total2Digits.ApplyShopTotalLayout();
                return;
            }

            var legacyTxt2 = total2 != null ? total2.Find(TxtTotal2LegacyNodeName) : null;
            if (legacyTxt2 != null)
            {
                _txtTotal2TmpFallback = legacyTxt2.GetComponent<TextMeshProUGUI>();
                _txtTotal2Fallback = legacyTxt2.GetComponent<Text>();
                return;
            }

            var legacy = FindDeepChild(transform, TxtTotalLegacyName);
            if (legacy != null)
            {
                _txtTotal2TmpFallback = legacy.GetComponent<TextMeshProUGUI>();
                _txtTotal2Fallback = legacy.GetComponent<Text>();
                if (_txtTotal2Fallback != null && !_txtTotal2Fallback.enabled)
                {
                    _txtTotal2Fallback.enabled = true;
                }
            }

            if (total2Digits == null && _txtTotal2Fallback == null && _txtTotal2TmpFallback == null)
            {
                Debug.LogWarning(
                    "[ShopFormLogic] 未找到 Total2_Digits / Total2；请运行 Bake 或补合计图片节点。",
                    this);
            }
        }

        /// <summary>Buy + Sell 所有行数量变化时刷新 Total2。</summary>
        private void WireAllRowQuantityRefresh()
        {
            UnwireAllRowQuantityRefresh();
            WireRowListQuantityRefresh(_buyRowViews);
            WireRowListQuantityRefresh(_sellRowViews);
        }

        private void WireRowListQuantityRefresh(IReadOnlyList<ShopBarRowView> rows)
        {
            foreach (var rowView in rows)
            {
                if (rowView == null)
                {
                    continue;
                }

                var input = rowView.GetComponent<ShopBuyRowQuantityInput>();
                if (input == null)
                {
                    continue;
                }

                input.OnQuantityValueChanged += RefreshTotal2;
                _wiredQuantityInputs.Add(input);
            }
        }

        private void UnwireAllRowQuantityRefresh()
        {
            foreach (var input in _wiredQuantityInputs)
            {
                if (input != null)
                {
                    input.OnQuantityValueChanged -= RefreshTotal2;
                }
            }

            _wiredQuantityInputs.Clear();
        }

        private void SetTotal2Number(int total)
        {
            if (total2Digits != null)
            {
                total2Digits.SetNumber(total);
                return;
            }

            var text = total.ToString();
            if (_txtTotal2TmpFallback != null)
            {
                _txtTotal2TmpFallback.text = text;
            }

            if (_txtTotal2Fallback != null)
            {
                _txtTotal2Fallback.text = text;
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
            ResetRowListQuantityInputs(_buyRowViews);
        }

        private void ResetAllSellQuantityInputs()
        {
            ResetRowListQuantityInputs(_sellRowViews);
        }

        private static void ResetRowListQuantityInputs(IReadOnlyList<ShopBarRowView> rows)
        {
            foreach (var rowView in rows)
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
