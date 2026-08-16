using System.Collections.Generic;
using Game.GameMgr;
using Game.GameMgr.Component.Archive;
using Game.GameMgr.Component.Archive.ArchiveDataClass.Player;
using Game.GameMgr.Component.Archive.ArchiveDataClass.Quest;
using Game.GameRuntime.GameSceneManager.Component;
using Game.GameRuntime.UI.Component;
using Game.GameRuntime.UI.FormLogic.Base;
using Game.Static.Enum.Goods;
using Game.Static.Name.Res;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.GameRuntime.UI.FormLogic.Shop
{
    /// <summary>
    /// 商店 UI 逻辑（EB 烘焙 + ST Total2 + IMG 图片数字）：
    /// Total2 按 Tab 显示 Σ(Number×单价) 的图片数字；Number 为隐形输入 + DigitStrip。
    /// 阶段五：点「决定」按购买合计真实扣款入包（与 Total2 同公式）。
    /// 双轨入口：场景常驻 UI_Shop（正规进店，走 Awake）与将来 OpenUIForm(ShopPanel)（走 OnInit）
    /// 共用 <see cref="EnsureShopRuntimeBound"/>，避免只信 OnInit 导致 Total2/贩卖全断。
    /// </summary>
    public class ShopFormLogic : BaseUIFormLogic
    {
        /// <summary>单笔购买成交行（qty&gt;0），供预校验与入包循环共用，避免扫两遍时数量不一致。</summary>
        private struct BuyLine
        {
            public EMainItemName ItemId;
            public int Quantity;
            public int UnitPrice;
        }

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
        /// <summary>兼容场景里尚未改名的「Confirm」节点。</summary>
        private const string BtnConfirmLegacyName = "Confirm";
        private const string BtnExitName = "BtnExit";
        private const string BtnCloseName = "BtnClose";
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

        [Header("阶段五 · 决定按钮（真实扣款 + 入包）")]
        [SerializeField] private Button btnConfirm;

        /// <summary>
        /// 商店×背包联合验收旁路：为 true 时跳过 TrySpendPlayerGold，直接 AddMainItem。
        /// 本阶段开发默认 true，避免「没钱」卡死入包验收；货币闭环仍见金币文档，提测货币前改 false。
        /// 禁止方案 C（扣款失败仍入包）——旁路是显式跳过，不是失败后白嫖。
        /// </summary>
        [Header("联合验收 · 货币旁路（提测金币前请关）")]
        [SerializeField] private bool bypassGoldCheckForBagJoint = true;

        [Header("离店 · 回 Village_KenMuNi1（纯 UI 商店无走路出门）")]
        [SerializeField] private Button btnExit;

        private readonly List<ShopBarRowView> _buyRowViews = new List<ShopBarRowView>();
        private readonly List<ShopBarRowView> _sellRowViews = new List<ShopBarRowView>();
        private readonly List<ShopBuyRowQuantityInput> _wiredQuantityInputs = new List<ShopBuyRowQuantityInput>();
        private bool _isBuyTabActive = true;

        /// <summary>
        /// 是否已完成运行时绑定。幂等守卫：Awake（场景 UI_Shop）与 OnInit（GF Prefab）都会调 Ensure。
        /// </summary>
        private bool _shopRuntimeBound;

        /// <summary>
        /// 保证 canvas 引用后再走基类 Awake（接 UICamera），并完成商店运行时绑定。
        /// 原因：正规进店用场景常驻 UI_Shop，不走 OpenUIForm → OnInit 不跑；必须在 Awake 接线。
        /// </summary>
        protected override void Awake()
        {
            if (canvas == null)
            {
                canvas = GetComponent<Canvas>();
            }

            if (componentSystemUI == null)
            {
                componentSystemUI = GetComponent<ComponentSystemUI>()
                                   ?? gameObject.AddComponent<ComponentSystemUI>();
            }

            base.Awake();
            EnsureShopRuntimeBound();
        }

        /// <summary>
        /// Form 首次创建（OpenUIForm 路径）：幂等接线，与 Awake 共用 Ensure。
        /// </summary>
        protected internal override void OnInit(object userData)
        {
            base.OnInit(userData);
            EnsureShopRuntimeBound();
        }

        /// <summary>
        /// 每次打开：防御性再 Ensure（池化复开），刷到购买 Tab。
        /// 店内允许 ESC 开菜单（贵重物品验收可在店内完成）。
        /// </summary>
        protected internal override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            EnsureShopRuntimeBound();
            SwitchToBuyTab();
            // 显式放行：避免其它 UI 关过后菜单仍锁死；场景侧 Village_ShopSceneManager 也会 SetAllowOpenMenu(true)。
            AllowOpenMenu(true);
            // SN-8：每次开店再刷一次名图（池化复开 / 进店前已改语言）。
            RefreshAllShopNamesForLanguage();
            Debug.Log("[VillageShopDebug] ShopPanel OnOpen AllowOpenMenu(true) 店内可 ESC 开菜单");
        }

        /// <summary>
        /// 解析引用 + 收集行 + 绑数量刷新 / Tab / 确认 / 离店。
        /// 必须幂等：OnInit（GF）与 Awake/OnOpen（场景 UI_Shop）都可能调用。
        /// 原因：正规进店不走 OpenUIForm，仅 OnInit 接线会导致 Total2/贩卖全失效。
        /// 替代方案：强制 OpenUIForm(ShopPanel) 并禁用场景 UI_Shop —— 与现行双轨冲突，本期不采用。
        /// </summary>
        private void EnsureShopRuntimeBound()
        {
            if (_shopRuntimeBound)
            {
                return;
            }

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
            ResolveExitButtonReference();
            WireExitButton();

            _shopRuntimeBound = true;

            // FIX-2：正规进店 Console 必须能看到；wiredInputs>0 且 sellBtn=ok 才算接线成功。
            var sellBtnState = btnSell != null ? "ok" : "null";
            Debug.Log(
                $"[ShopFormLogic] runtime bound buyRows={_buyRowViews.Count} sellRows={_sellRowViews.Count} " +
                $"wiredInputs={_wiredQuantityInputs.Count} sellBtn={sellBtnState}",
                this);

            // SN-8：进店按当前语言贴三语名图（Bake 仅为中文预览）。
            RefreshAllShopNamesForLanguage();
        }

        /// <summary>
        /// 多语言 UI 刷新钩子：设置改语言后若 Form 触发 UpdateUI，重刷货架名图。
        /// 替代方案：仅重进店才刷 —— 底线可用，但店内切语会错；故尽量挂此钩子。
        /// </summary>
        public override void UpdateUI()
        {
            base.UpdateUI();
            RefreshAllShopNamesForLanguage();
        }

        /// <summary>
        /// 设置面板盖住商店再露出时重刷名图（尽量不关店即换语）。
        /// </summary>
        protected internal override void OnReveal()
        {
            base.OnReveal();
            if (_shopRuntimeBound)
            {
                RefreshAllShopNamesForLanguage();
            }
        }

        /// <summary>遍历买/卖可见行，按当前语言幂等重贴 Name 名图。</summary>
        public void RefreshAllShopNamesForLanguage()
        {
            RefreshRowListShopNames(_buyRowViews);
            RefreshRowListShopNames(_sellRowViews);
        }

        private static void RefreshRowListShopNames(IReadOnlyList<ShopBarRowView> rows)
        {
            if (rows == null)
            {
                return;
            }

            for (var i = 0; i < rows.Count; i++)
            {
                rows[i]?.RefreshShopNameForLanguage();
            }
        }

        /// <summary>
        /// 关闭时保持 ESC 可开，供回村后菜单可用。
        /// </summary>
        protected internal override void OnClose(bool isShutdown, object userData)
        {
            base.OnClose(isShutdown, userData);
            AllowOpenMenu(true);
            Debug.Log("[VillageShopDebug] ShopPanel OnClose AllowOpenMenu(true)");
        }

        private void OnDestroy()
        {
            // 离场时勿把全局菜单永久锁死（下一场景会自己 AllowResponse / SetAllowOpenMenu）。
            UnwireAllRowQuantityRefresh();

            if (btnConfirm != null)
            {
                btnConfirm.onClick.RemoveListener(OnConfirmClick);
            }

            if (btnExit != null)
            {
                btnExit.onClick.RemoveListener(OnExitClick);
            }

            if (btnSell != null)
            {
                btnSell.onClick.RemoveListener(SwitchToSellTab);
            }

            if (btnBuy != null)
            {
                btnBuy.onClick.RemoveListener(SwitchToBuyTab);
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
        /// 点「决定」：购买 Tab 入包并落盘；出售 Tab 本阶段未接入。
        /// 合计公式与 Total2 一致：Σ(QuantityForTotal × Price)，仅 qty&gt;0 行。
        /// 整单失败：数量为 0 / 堆叠将超；（旁路关闭时）金币不足 → 不入包。
        /// 顺序：堆叠预检 →（可选）扣款 → AddMainItem → SavePlayerBag。
        /// 货币旁路见 <see cref="bypassGoldCheckForBagJoint"/>：联合验收不看金币，正式验货币前关掉。
        /// </summary>
        public void OnConfirmClick()
        {
            // 出售为 P1：工期紧时仅提示，不改存档。
            if (!_isBuyTabActive)
            {
                ShopDebugLogger.LogSellNotImplemented();
                return;
            }

            var lines = CollectBuyLinesWithQuantity();
            var total = 0;
            for (var i = 0; i < lines.Count; i++)
            {
                total += lines[i].Quantity * lines[i].UnitPrice;
            }

            // 与 Total2 同口径：全 0 则拒绝，不碰存档。
            if (lines.Count == 0 || total <= 0)
            {
                ShopDebugLogger.LogZeroQuantityWarning();
                return;
            }

            var bag = ResolvePlayerBagData();
            if (bag == null)
            {
                ShopDebugLogger.LogArchiveUnavailable("背包存档不可用（请从 InitScene 正规进游戏）");
                return;
            }

            // 任一行购买后将超 MaxStackPerItem → 整单失败（先于扣款 / 旁路入包）。
            if (!TryValidateBuyStackLimits(bag, lines))
            {
                return;
            }

            // 联合验收旁路：跳过扣款；关闭旁路后仍走原 TrySpendPlayerGold（失败则整单不入包）。
            if (!bypassGoldCheckForBagJoint)
            {
                var questMgr = QuestManager.getInstance();
                var goldData = questMgr.GetPlayerGoldData();
                if (goldData == null)
                {
                    ShopDebugLogger.LogArchiveUnavailable("游戏币存档不可用（请从 InitScene 正规进游戏）");
                    return;
                }

                if (!questMgr.TrySpendPlayerGold(total))
                {
                    ShopDebugLogger.LogInsufficientGold(total, goldData.gold);
                    return;
                }
            }

            for (var i = 0; i < lines.Count; i++)
            {
                bag.AddMainItem(lines[i].ItemId, lines[i].Quantity);
            }

            SavePlayerBag();

            // 入包明细 Log：SB-V2 Console 对账用（SmallHpPotion×2, …）
            var logIds = new List<EMainItemName>(lines.Count);
            var logQtys = new List<int>(lines.Count);
            for (var i = 0; i < lines.Count; i++)
            {
                logIds.Add(lines[i].ItemId);
                logQtys.Add(lines[i].Quantity);
            }

            ShopDebugLogger.LogPurchaseIntoBag(logIds, logQtys, total, bypassGoldCheckForBagJoint);

            // 成功后数量清零并刷 Total2，避免连点重复入包。
            ResetAllBuyQuantityInputs();
            RefreshTotal2();
        }

        /// <summary>
        /// 离开纯 UI 商店：黑幕全黑时再 CloseForm，然后换场回村。
        /// LastSceneName 将变为 Village_Shop，供村里 EnterPosConfig 匹配门外落点。
        /// 原因：先关 Panel 再开黑幕会闪一下空场景；用 stayAction=CloseForm 与进店对称。
        /// 替代方案：仅靠 ESC 菜单「返回」关菜单 —— 不能代替离店，故必须有明确离开入口。
        /// </summary>
        public void OnExitClick()
        {
            var gsm = GameManager.GetGameSceneManager();
            if (gsm == null)
            {
                Debug.LogError("[ShopFormLogic] GetGameSceneManager 为空，无法离店回村。", this);
                return;
            }

            Debug.Log("[VillageShopDebug] exit shop → LoadScene Village_KenMuNi1 (CloseForm on black stay)");
            // CloseForm 作为 stayAction：黑幕 FadeShow 结束后、切场前关闭 ShopPanel，避免面板残留到村里。
            gsm.GetModule<LoadSceneComponentGSM>().LoadScene(SceneName.Village_KenMuNi1, CloseForm);
        }

        /// <summary>收集购买行中 qty&gt;0 的成交行（与 GetCurrentBuyTotal 扫行规则一致）。</summary>
        private List<BuyLine> CollectBuyLinesWithQuantity()
        {
            var lines = new List<BuyLine>();
            foreach (var rowView in _buyRowViews)
            {
                if (rowView == null)
                {
                    continue;
                }

                var input = rowView.GetComponent<ShopBuyRowQuantityInput>();
                var quantity = input != null ? input.QuantityForTotal : 0;
                if (quantity <= 0)
                {
                    continue;
                }

                lines.Add(new BuyLine
                {
                    ItemId = rowView.ItemId,
                    Quantity = quantity,
                    UnitPrice = rowView.Price
                });
            }

            return lines;
        }

        /// <summary>
        /// 预校验每行 held+qty ≤ MaxStackPerItem；失败打 Log 并返回 false。
        /// 原因：AddMainItem 内部会钳到 10，若不预检会出现「钱已扣、道具少到账」。
        /// </summary>
        private static bool TryValidateBuyStackLimits(PlayerBagData bag, List<BuyLine> lines)
        {
            for (var i = 0; i < lines.Count; i++)
            {
                var line = lines[i];
                var held = bag.GetMainItemCount(line.ItemId);
                if (held + line.Quantity > PlayerBagData.MaxStackPerItem)
                {
                    ShopDebugLogger.LogStackOverflow(
                        line.ItemId.ToString(),
                        held,
                        line.Quantity,
                        PlayerBagData.MaxStackPerItem);
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 取背包：优先场景 Archive（与战斗/掉落一致），无 SceneManager 时走 GM 组件（与 QuestManager 金币 fallback 同思路）。
        /// </summary>
        private static PlayerBagData ResolvePlayerBagData()
        {
            var sceneMgr = GameManager.GetGameSceneManager();
            if (sceneMgr != null)
            {
                return sceneMgr.GetArchiveData<PlayerBagData>();
            }

            var archive = GameManager.GetGMComponent<ArchiveComponentGM>();
            return archive != null ? archive.GetData<PlayerBagData>() : null;
        }

        /// <summary>背包落盘：与金币 SaveSpcData 同机制，保证买完读档一致。</summary>
        private static void SavePlayerBag()
        {
            var archive = GameManager.GetGMComponent<ArchiveComponentGM>();
            if (archive != null)
            {
                archive.SaveSpcData<PlayerBagData>();
            }
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

            // 优先 BtnConfirm；兼容场景里仍叫 Confirm 的节点。
            var confirmNode = FindDeepChild(transform, BtnConfirmName)
                              ?? FindDeepChild(transform, BtnConfirmLegacyName);
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

        private void ResolveExitButtonReference()
        {
            if (btnExit != null)
            {
                return;
            }

            // 与美术约定优先 BtnExit；BtnClose 作为备选节点名（OPEN Q-B）。
            var exitNode = FindDeepChild(transform, BtnExitName)
                           ?? FindDeepChild(transform, BtnCloseName);
            if (exitNode != null)
            {
                btnExit = exitNode.GetComponent<Button>();
            }

            if (btnExit == null)
            {
                Debug.LogWarning("[ShopFormLogic] 未找到 BtnExit / BtnClose；离店按钮未接线。", this);
            }
        }

        private void WireExitButton()
        {
            if (btnExit == null)
            {
                return;
            }

            btnExit.onClick.RemoveListener(OnExitClick);
            btnExit.onClick.AddListener(OnExitClick);
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
