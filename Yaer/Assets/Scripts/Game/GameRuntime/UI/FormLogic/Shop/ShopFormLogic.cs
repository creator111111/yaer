using System.Collections.Generic;
using Game.DataTable.MainItem;
using Game.GameMgr;
using Game.GameMgr.Component.Archive;
using Game.GameMgr.Component.Archive.ArchiveDataClass.Player;
using Game.GameMgr.Component.Archive.ArchiveDataClass.Quest;
using Game.GameRuntime.GameSceneManager.Component;
using Game.GameRuntime.GameSceneManager.Scene.Village_Shop;
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
    /// 阶段五：点「决定」购买真实扣款入包；0920：出售真实出包加币（买卖分叉，不混结算）。
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

        /// <summary>
        /// 单笔出售成交行（qty&gt;0）。与 BuyLine 字段相同，故意拆开：
        /// 避免 CollectBuy / CollectSell 混用同一列表，误把卖行塞进购买结算。
        /// </summary>
        private struct SellLine
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
        /// 商店×背包联合验收旁路：为 true 时跳过 TrySpendPlayerGold，直接 AddMainItem 并播 ShopYes。
        /// 正式默认 false（须与 ShopPanel.prefab、Village_Shop 场景 UI_Shop 三处一致为关）。
        /// 联调可手开；开着时验不出「钱不够 → ShopNo」，勿宣称失败对白已验收。
        /// 禁止方案 C（扣款失败仍入包）——旁路是显式跳过，不是失败后白嫖。
        /// 替代（P1 可选）：仅 Editor / 开发菜单暴露此开关，正式 Prefab/场景永不序列化为 true。
        /// </summary>
        [Header("联合验收 · 货币旁路（正式默认关；联调可手开）")]
        [SerializeField] private bool bypassGoldCheckForBagJoint = false;

        /// <summary>
        /// 0923 复验热修：<c>[ShopBuyMax]</c> 诊断。
        /// true=每次 Resolve 都打；false=仅签名变化时打（减刷屏，仍能钉 G2）。
        /// </summary>
        [Header("诊断 · 购买 max 拆解（过滤 [ShopBuyMax]）")]
        [SerializeField] private bool shopBuyMaxVerbose = false;

        [Header("离店 · 回 Village_KenMuNi1（纯 UI 商店无走路出门）")]
        [SerializeField] private Button btnExit;

        private readonly List<ShopBarRowView> _buyRowViews = new List<ShopBarRowView>();
        private readonly List<ShopBarRowView> _sellRowViews = new List<ShopBarRowView>();
        private readonly List<ShopBuyRowQuantityInput> _wiredQuantityInputs = new List<ShopBuyRowQuantityInput>();
        private bool _isBuyTabActive = true;

        /// <summary>
        /// 购买行联合钳制中：改一行后回扫其它行时置位，避免 OnQuantityValueChanged 递归。
        /// </summary>
        private bool _isReclampingBuyCart;

        /// <summary>上次 [ShopBuyMax] 签名（按 item），用于非 Verbose 时去重。</summary>
        private readonly Dictionary<string, string> _lastShopBuyMaxSignatureByItem =
            new Dictionary<string, string>();

        /// <summary>本店打开期间已提示过「堆叠已满」的道具，避免 Warning 刷屏。</summary>
        private readonly HashSet<string> _stackFullWarnedItemIds = new HashSet<string>();

        /// <summary>
        /// 仅 Commit / 决定路径允许打 [ShopBuyMax]（编辑 ValueChanged 禁刷）。
        /// shopBuyMaxVerbose=true 时仍走 LogBuyMaxBreakdown 的 Verbose 分支。
        /// </summary>
        private bool _shopBuyMaxLogEnabled;


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
        /// 0829：店内禁止 ESC 开菜单（改口 vs 0713）；存档/菜单须回村再 ESC。
        /// </summary>
        protected internal override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            EnsureShopRuntimeBound();
            SwitchToBuyTab();
            // 必须 false：否则会盖掉 Village_ShopSceneManager.OnEnter 的 SetAllowOpenMenu(false)，
            // InputComponentGSM 又会 ESC→MenuPanel，与 ESC 离店双开。
            // OnClose 仍 AllowOpenMenu(true)，利回村后菜单（OPEN Q4）。
            AllowOpenMenu(false);
            // SN-8：每次开店再刷一次名图（池化复开 / 进店前已改语言）。
            RefreshAllShopNamesForLanguage();
            // 0923 复验：每店一次堆叠满提示；max 签名缓存也清，便于验收贴新日志。
            _stackFullWarnedItemIds.Clear();
            _lastShopBuyMaxSignatureByItem.Clear();
            Debug.Log("[ShopEscExit] ShopPanel OnOpen AllowOpenMenu(false) 店内 ESC=离店非菜单");
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

        /// <summary>购买 Tab：Σ(每行 QuantityForTotal × ResolveBuyUnitPrice)。</summary>
        public int GetCurrentBuyTotal()
        {
            return SumBuyRowTotals(_buyRowViews);
        }

        /// <summary>出售 Tab：Σ(每行 QuantityForTotal × ShopBarRowView.Price)。</summary>
        public int GetCurrentSellTotal()
        {
            return SumSellRowTotals(_sellRowViews);
        }

        /// <summary>按当前 Tab 刷新 Total2 文案。</summary>
        public void RefreshTotal2()
        {
            var total = _isBuyTabActive ? GetCurrentBuyTotal() : GetCurrentSellTotal();
            SetTotal2Number(total);
        }

        /// <summary>
        /// 点「决定」：购买 Tab 入包并落盘；出售 Tab 出包加币并双落盘。
        /// 合计公式与 Total2 一致：Σ(QuantityForTotal × Price)，仅 qty&gt;0 行。
        /// 购买整单失败：数量为 0 / 堆叠将超；（旁路关闭时）金币不足 → 不入包。
        /// 出售整单失败：数量为 0 / 任一行背包不够 → 不扣包、不加币。
        /// 购买顺序：堆叠预检 →（可选）扣款 → AddMainItem → SavePlayerBag → 成败对白。
        /// 出售顺序：背包预检 → TryRemoveMainItem×N → AddGold + SaveGold → SaveBag → 清零（本期不播对白）。
        /// 货币旁路见 <see cref="bypassGoldCheckForBagJoint"/>：只影响购买，出售不走旁路。
        /// 对白：购买成功 → Village_ShopYes；仅金币不足 → Village_ShopNo。出售成败对白见 OPEN，本期不接。
        /// </summary>
        public void OnConfirmClick()
        {
            // 出售与购买分叉：禁止把卖行塞进购买结算（会按买价扣钱并往包里加素材）。
            if (!_isBuyTabActive)
            {
                OnConfirmSellClick();
                return;
            }

            // 决定前强制 Commit 所有买行（含仍聚焦），再扫数量结算。
            CommitAllWiredQuantityInputs();

            var lines = CollectBuyLinesWithQuantity();
            var total = 0;
            for (var i = 0; i < lines.Count; i++)
            {
                total += lines[i].Quantity * lines[i].UnitPrice;
            }

            // 与 Total2 同口径：全 0 则拒绝，不碰存档、不播 No。
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

            // 任一行购买后将超 MaxStackPerItem → 整单失败（先于扣款 / 旁路入包）；不播 No（文案不符）。
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
                    // 仅金币不足播 No：先 Log 再 Trigger（Hide UI 由 GSM 负责）。
                    TryNotifyPurchaseDialogue(purchaseSucceeded: false);
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

            // 入包成功（含旁路未扣款）再播 Yes；禁止先对白后扣款。
            TryNotifyPurchaseDialogue(purchaseSucceeded: true);
        }

        /// <summary>
        /// 出售 Tab「决定」：整单出包加币并双落盘。
        /// 原因：此前仅 Log「出售结算未接入」就 return，属设计债占位，非回归。
        /// 对称购买：先整单预检 held≥qty，再逐行 TryRemove，再 AddGold+SaveGold+SaveBag。
        /// 禁止：先加钱不扣包（读档白嫖）；任一行不够仍扣半单；擅自播 ShopYes/ShopNo（OPEN 未拍板）。
        /// 替代方案：改 TryAddPlayerGold 门面 —— 现网无此 API，任务发奖已用 AddGold+SavePlayerGold，直接对齐即可。
        /// </summary>
        private void OnConfirmSellClick()
        {
            // 与买同步：决定前先 Commit 卖行编辑中数量。
            CommitAllWiredQuantityInputs();

            var lines = CollectSellLinesWithQuantity();
            var total = 0;
            for (var i = 0; i < lines.Count; i++)
            {
                total += lines[i].Quantity * lines[i].UnitPrice;
            }

            // 与购买一致：全 0 拒绝，不碰存档。
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

            var questMgr = QuestManager.getInstance();
            var goldData = questMgr != null ? questMgr.GetPlayerGoldData() : null;
            if (goldData == null)
            {
                ShopDebugLogger.LogArchiveUnavailable("游戏币存档不可用（请从 InitScene 正规进游戏）");
                return;
            }

            // 整单预检：任一行不够 → 一行都不扣、不加币。
            if (!TryValidateSellBagCounts(bag, lines))
            {
                return;
            }

            // 预检通过后再扣；理想情况不应失败。若仍失败则中止，不再加币（避免「包没扣完却给钱」）。
            for (var i = 0; i < lines.Count; i++)
            {
                var line = lines[i];
                if (!bag.TryRemoveMainItem(line.ItemId, line.Quantity))
                {
                    ShopDebugLogger.LogSellRemoveFailed(line.ItemId.ToString(), line.Quantity);
                    return;
                }
            }

            // 先加币并落盘，再存包：读档后钱与素材应对得上。
            goldData.AddGold(total);
            questMgr.SavePlayerGold();
            SavePlayerBag();

            var logIds = new List<EMainItemName>(lines.Count);
            var logQtys = new List<int>(lines.Count);
            for (var i = 0; i < lines.Count; i++)
            {
                logIds.Add(lines[i].ItemId);
                logQtys.Add(lines[i].Quantity);
            }

            ShopDebugLogger.LogSellFromBag(logIds, logQtys, total);

            // 清零数量，避免连点重复卖。本期不播出售对白（OPEN Q1）。
            ResetAllSellQuantityInputs();
            RefreshTotal2();
        }

        /// <summary>
        /// 通知 GSM 播购买成败短对白（Yes/No），走特殊对白同管线。
        /// </summary>
        /// <remarks>
        /// 原因：UI 不直开 TriggerStory，避免漏藏买卖界面 / 叠第二段对白。
        /// </remarks>
        private static void TryNotifyPurchaseDialogue(bool purchaseSucceeded)
        {
            var shopGsm = GameManager.GetGameSceneManager() as Village_ShopSceneManager;
            if (shopGsm == null)
            {
                Debug.LogWarning("[ShopPurchase] Village_ShopSceneManager 不可用，跳过成败对白");
                return;
            }

            shopGsm.TryTriggerPurchaseResult(purchaseSucceeded);
        }

        /// <summary>
        /// 离开纯 UI 商店：转交 <see cref="Village_ShopSceneManager.ExitShopToVillage"/>，
        /// 与 ESC 同源（黑幕全黑藏 UI_Shop → LoadScene 回村 → EnterFrom_Shop）。
        /// </summary>
        /// <remarks>
        /// 禁止本方法内再传 <see cref="CloseForm"/> 给 LoadScene：
        /// 场景 UI_Shop 无 GF UIForm，CloseForm 会抛异常并卡死黑幕（见 GSM 注释）。
        /// </remarks>
        public void OnExitClick()
        {
            var shopGsm = GameManager.GetGameSceneManager() as Village_ShopSceneManager;
            if (shopGsm != null)
            {
                Debug.Log("[ShopEscExit] 离开按钮 → ExitShopToVillage");
                shopGsm.ExitShopToVillage();
                return;
            }

            // 兜底：非店 GSM（不应发生）；同样勿用 CloseForm。
            var gsm = GameManager.GetGameSceneManager();
            if (gsm == null)
            {
                Debug.LogError("[ShopFormLogic] GetGameSceneManager 为空，无法离店回村。", this);
                return;
            }

            Debug.LogWarning(
                "[ShopEscExit] 当前非 Village_ShopSceneManager，走兜底 LoadScene（Hide 自身）。",
                this);
            gsm.GetModule<LoadSceneComponentGSM>().LoadScene(
                SceneName.Village_KenMuNi1,
                () => gameObject.SetActive(false));
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
                    UnitPrice = ResolveBuyUnitPrice(rowView)
                });
            }

            return lines;
        }

        /// <summary>
        /// 收集出售行中 qty&gt;0 的成交行（与 GetCurrentSellTotal 扫行规则一致）。
        /// 故意独立于 CollectBuyLinesWithQuantity：购买代码路径保持独立，不把卖行塞进买列表。
        /// </summary>
        private List<SellLine> CollectSellLinesWithQuantity()
        {
            var lines = new List<SellLine>();
            foreach (var rowView in _sellRowViews)
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

                lines.Add(new SellLine
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
        /// 预校验每行 held ≥ qty；任一行不够则整单失败、不改存档。
        /// 原因：禁止「卖到没有」半单扣包；须先整单预检再逐行 TryRemove。
        /// </summary>
        private static bool TryValidateSellBagCounts(PlayerBagData bag, List<SellLine> lines)
        {
            for (var i = 0; i < lines.Count; i++)
            {
                var line = lines[i];
                var held = bag.GetMainItemCount(line.ItemId);
                if (held < line.Quantity)
                {
                    ShopDebugLogger.LogInsufficientBag(line.ItemId.ToString(), line.Quantity, held);
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

        /// <summary>
        /// 购买合计：单价与联合 max / 决定扣款同走 <see cref="ResolveBuyUnitPrice"/>
        /// （Bake 价 0 时回退表价，避免口径分裂导致 max/Total2/扣款不一致）。
        /// </summary>
        private static int SumBuyRowTotals(IReadOnlyList<ShopBarRowView> rows)
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
                sum += quantity * ResolveBuyUnitPrice(rowView);
            }

            return sum;
        }

        /// <summary>出售合计：单价用行上 Bake 卖价。</summary>
        private static int SumSellRowTotals(IReadOnlyList<ShopBarRowView> rows)
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

        /// <summary>Buy + Sell 所有行数量变化时刷新 Total2；并注入买卖数量上限回调。</summary>
        private void WireAllRowQuantityRefresh()
        {
            UnwireAllRowQuantityRefresh();
            WireRowListQuantityRefresh(_buyRowViews, isBuyRow: true);
            WireRowListQuantityRefresh(_sellRowViews, isBuyRow: false);
        }

        /// <summary>
        /// 绑数量变更 → Total2；并按行注入 max 回调。
        /// 买：联合总价（本行 max 扣掉其它行已填金额后再÷单价）；卖：持有。
        /// </summary>
        private void WireRowListQuantityRefresh(IReadOnlyList<ShopBarRowView> rows, bool isBuyRow)
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

                // 捕获本行引用：闭包在失焦/输入时再读最新金币与持有，勿缓存成打开店时的快照。
                var capturedRow = rowView;
                var capturedInput = input;
                // T3：Begin 前先 Commit 其它聚焦行。
                capturedInput.SetBeforeBeginEdit(() => CommitFocusedQuantityInputsExcept(capturedInput));

                if (isBuyRow)
                {
                    input.SetMaxQuantityResolver(() => ResolveBuyMaxQuantity(capturedRow));
                    // 买：正式提交后联合回扫整车再刷合计
                    input.OnQuantityValueChanged += OnBuyQuantityChangedForJointCart;
                    // 清空 / 编辑预览：只刷 Total2，不 Reclamp
                    input.OnQuantityClearedForEdit += RefreshTotal2;
                    input.OnQuantityEditPreview += RefreshTotal2;
                    // Commit：打 [ShopBuyMax] + max=0 可辨反馈
                    input.OnQuantityCommitted += (before, after) =>
                        OnBuyQuantityCommitted(capturedRow, before, after);
                }
                else
                {
                    input.SetMaxQuantityResolver(() => ResolveSellMaxQuantity(capturedRow));
                    input.OnQuantityValueChanged += RefreshTotal2;
                    input.OnQuantityClearedForEdit += RefreshTotal2;
                    input.OnQuantityEditPreview += RefreshTotal2;
                    input.OnQuantityCommitted += (before, after) =>
                        OnSellQuantityCommitted(capturedRow, before, after);
                }

                _wiredQuantityInputs.Add(input);
            }
        }

        /// <summary>
        /// 购买数量变更：按联合总价回扫所有买行再刷 Total2。
        /// 原因：各行独立 floor(gold/price) 会让多行合计超钱包；须 gold−其它行金额 再算本行。
        /// </summary>
        private void OnBuyQuantityChangedForJointCart()
        {
            if (!_isReclampingBuyCart)
            {
                ReclampAllBuyRowsForJointGold();
            }

            RefreshTotal2();
        }

        /// <summary>
        /// 静默回扫购买列表：每行用「金币−其它行合计」再钳一次。
        /// T2：跳过正在编辑的行（编辑锁定期不写回）。
        /// </summary>
        private void ReclampAllBuyRowsForJointGold()
        {
            _isReclampingBuyCart = true;
            try
            {
                for (var i = 0; i < _buyRowViews.Count; i++)
                {
                    var row = _buyRowViews[i];
                    if (row == null)
                    {
                        continue;
                    }

                    var input = row.GetComponent<ShopBuyRowQuantityInput>();
                    if (input == null || input.IsQuantityFocused)
                    {
                        continue;
                    }

                    input.ApplyBusinessMaxClampSilent();
                }
            }
            finally
            {
                _isReclampingBuyCart = false;
            }
        }

        /// <summary>决定前 / 切行：Commit 全部已接线数量框。</summary>
        private void CommitAllWiredQuantityInputs()
        {
            for (var i = 0; i < _wiredQuantityInputs.Count; i++)
            {
                _wiredQuantityInputs[i]?.CommitQuantity();
            }
        }

        /// <summary>Begin 前：Commit 其它聚焦行（本行随后清空）。</summary>
        private void CommitFocusedQuantityInputsExcept(ShopBuyRowQuantityInput except)
        {
            for (var i = 0; i < _wiredQuantityInputs.Count; i++)
            {
                var input = _wiredQuantityInputs[i];
                if (input == null || input == except)
                {
                    continue;
                }

                if (input.IsQuantityFocused)
                {
                    input.CommitQuantity();
                }
            }
        }

        /// <summary>
        /// 购买 Commit：每次必打 <c>[ShopBuyCommit]</c> 对账行（输入/价/金/空位/钳后）；
        /// 另保留 [ShopBuyMax]；超限时再打分支 Warning。
        /// </summary>
        private void OnBuyQuantityCommitted(ShopBarRowView row, int beforeParsed, int afterClamped)
        {
            if (row == null)
            {
                return;
            }

            var breakdown = ComputeBuyMaxBreakdown(row);
            var maxStack = PlayerBagData.MaxStackPerItem;
            var reason = ResolveBuyCommitZeroReason(breakdown, beforeParsed, afterClamped);

            // 产品对账：每次提交都打，不过滤、不去重。
            ShopDebugLogger.LogBuyCommitTrace(
                breakdown.ItemKey,
                beforeParsed,
                breakdown.Price,
                breakdown.Gold,
                breakdown.OtherCost,
                breakdown.Held,
                maxStack,
                breakdown.StackRoom,
                breakdown.AffordByGold,
                breakdown.Max,
                afterClamped,
                reason);

            _shopBuyMaxLogEnabled = true;
            try
            {
                LogBuyMaxBreakdown(breakdown);
            }
            finally
            {
                _shopBuyMaxLogEnabled = false;
            }

            // 键入正数被提交钳到更小（含 0）：必须有反馈，禁止静默。
            if (beforeParsed <= afterClamped)
            {
                return;
            }

            if (breakdown.StackRoom <= 0)
            {
                ShopDebugLogger.LogBuyCommitClampedStackFull(
                    breakdown.ItemKey, breakdown.Held, maxStack,
                    beforeParsed, afterClamped);
            }
            else if (breakdown.AffordByGold <= 0)
            {
                ShopDebugLogger.LogBuyCommitClampedInsufficientGold(
                    breakdown.ItemKey, breakdown.Gold, breakdown.Price,
                    beforeParsed, afterClamped);
            }
            else
            {
                ShopDebugLogger.LogBuyCommitClampedToMax(
                    breakdown.ItemKey, beforeParsed, afterClamped, breakdown.Max);
            }
        }

        /// <summary>
        /// 人话原因：有金仍变 0 时优先写「背包已满」；钱不够才写「金币不足」。
        /// </summary>
        private static string ResolveBuyCommitZeroReason(
            BuyMaxBreakdown b, int inputQty, int afterQty)
        {
            if (inputQty == afterQty)
            {
                return afterQty == 0 && inputQty == 0 ? "未输入(空/0)" : "未调整(在上限内)";
            }

            if (b.StackRoom <= 0)
            {
                return $"背包已满(持有{b.Held}/顶{PlayerBagData.MaxStackPerItem})→空位0→上限0【不是没钱】";
            }

            if (b.Price <= 0)
            {
                return "单价≤0→上限0";
            }

            if (b.AffordByGold <= 0)
            {
                return $"金币不够买1件(金{b.Gold}-其它行{b.OtherCost})/价{b.Price}";
            }

            if (afterQty < inputQty)
            {
                return $"超过可买上限max={b.Max}(钱够{b.AffordByGold}件∩空位{b.StackRoom})";
            }

            return "其它";
        }

        /// <summary>出售 Commit：超持有钳回时打可辨 Warning（无 [ShopBuyMax]）。</summary>
        private void OnSellQuantityCommitted(ShopBarRowView row, int beforeParsed, int afterClamped)
        {
            if (row == null || beforeParsed <= afterClamped)
            {
                return;
            }

            var held = ResolveSellMaxQuantity(row);
            ShopDebugLogger.LogSellCommitClampedToHeld(
                row.ItemId.ToString(), beforeParsed, afterClamped, held);
        }

        /// <summary>
        /// 购买行最大可填数量（联合总价）。公式不变；诊断日志仅 Commit 时打。
        /// </summary>
        /// <remarks>
        /// 0923 公式审计：编辑锁定期不调本方法写回；提交瞬间 B1=stackRoom∩afford。
        /// </remarks>
        private int ResolveBuyMaxQuantity(ShopBarRowView row)
        {
            return ComputeBuyMaxBreakdown(row).Max;
        }

        /// <summary>
        /// 购买上限拆解（供 Commit 日志 / Tips 分支）。
        /// <para>
        /// 0923 逐项查错（F1）：本档 held=MaxStack → stackRoom=0 → max=0 是<strong>设计空位门</strong>
        /// （读数正确），不是 gold/price/otherCost 算错。禁止关 stackRoom；满堆叠提交归 0 + Console「已满」为预期。
        /// 验收须先 held&lt;MaxStack，再验 max≥1。
        /// </para>
        /// </summary>
        private struct BuyMaxBreakdown
        {
            public string ItemKey;
            public int Held;
            public int StackRoom;
            public int Price;
            public int Gold;
            public int OtherCost;
            public int AffordByGold;
            public int Max;
            public bool BypassGold;
        }

        /// <summary>
        /// 计算购买 max（公式保留）：
        /// <c>min((gold−otherCost)/price, max(0, MaxStack−held))</c>；旁路则仅 stackRoom。
        /// </summary>
        private BuyMaxBreakdown ComputeBuyMaxBreakdown(ShopBarRowView row)
        {
            var result = new BuyMaxBreakdown
            {
                ItemKey = row != null ? row.ItemId.ToString() : string.Empty,
                Held = 0,
                StackRoom = 0,
                Price = 0,
                Gold = 0,
                OtherCost = 0,
                AffordByGold = 0,
                Max = 0,
                BypassGold = bypassGoldCheckForBagJoint
            };

            if (row == null)
            {
                return result;
            }

            var bag = ResolvePlayerBagData();
            result.Held = bag != null ? bag.GetMainItemCount(row.ItemId) : 0;
            result.StackRoom = Mathf.Max(0, PlayerBagData.MaxStackPerItem - result.Held);

            if (bypassGoldCheckForBagJoint)
            {
                result.Max = result.StackRoom;
                return result;
            }

            result.Price = ResolveBuyUnitPrice(row);
            if (result.Price <= 0)
            {
                result.Max = 0;
                return result;
            }

            var goldData = QuestManager.getInstance()?.GetPlayerGoldData();
            result.Gold = goldData != null ? goldData.gold : 0;
            result.OtherCost = SumBuyRowsCostExcluding(row);
            var remainingGold = result.Gold - result.OtherCost;
            if (remainingGold < 0)
            {
                remainingGold = 0;
            }

            result.AffordByGold = remainingGold / result.Price;
            result.Max = Mathf.Min(result.AffordByGold, result.StackRoom);
            return result;
        }

        private void LogBuyMaxBreakdown(BuyMaxBreakdown b)
        {
            if (!shopBuyMaxVerbose && !_shopBuyMaxLogEnabled)
            {
                return;
            }

            string fields;
            if (b.BypassGold)
            {
                // 旁路无金币门：afford 不适用，显式标 -，避免与正式路径混淆。
                fields =
                    $"gold=(bypass) price=- held={b.Held} stackRoom={b.StackRoom} otherCost=- afford=- max={b.Max} bypassGoldCheck=1";
            }
            else if (b.Price <= 0)
            {
                fields =
                    $"gold=? price={b.Price} held={b.Held} stackRoom={b.StackRoom} otherCost=- afford=0 max=0 bypassGoldCheck=0";
            }
            else
            {
                // 0923 F1：显式 afford，便于一眼看出 min(afford,stackRoom)=max（满堆叠时 afford 仍可>0）。
                fields =
                    $"gold={b.Gold} price={b.Price} held={b.Held} stackRoom={b.StackRoom} otherCost={b.OtherCost} afford={b.AffordByGold} max={b.Max} bypassGoldCheck=0";
            }

            LogShopBuyMaxLine(b.ItemKey, fields);

            var affordForWarn = b.BypassGold ? (b.StackRoom == 0 ? 1 : 0) : b.AffordByGold;
            MaybeWarnBuyBlockedByFullStack(
                b.ItemKey,
                b.Held,
                b.StackRoom,
                b.BypassGold ? -1 : b.Gold,
                b.BypassGold ? -1 : b.Price,
                affordForWarn);
        }

        /// <summary>
        /// 写 <c>[ShopBuyMax]</c>。Verbose 每次打；否则仅签名变化时打（含首次 max=0）。
        /// </summary>
        private void LogShopBuyMaxLine(string itemKey, string fields)
        {
            var signature = $"{itemKey}|{fields}";
            if (!shopBuyMaxVerbose
                && _lastShopBuyMaxSignatureByItem.TryGetValue(itemKey, out var previous)
                && previous == signature)
            {
                return;
            }

            _lastShopBuyMaxSignatureByItem[itemKey] = signature;
            Debug.Log($"[ShopBuyMax] item={itemKey} {fields}", this);
        }

        /// <summary>
        /// 堆叠已满且本有购买力时 Warning 一次（每店每道具），避免被误判成读金/UI 坏。
        /// Tips/对白键另票；本期 Console 可辨即可。
        /// </summary>
        private void MaybeWarnBuyBlockedByFullStack(
            string itemKey,
            int held,
            int stackRoom,
            int gold,
            int price,
            int affordByGold)
        {
            if (stackRoom > 0 || affordByGold <= 0)
            {
                return;
            }

            if (!_stackFullWarnedItemIds.Add(itemKey))
            {
                return;
            }

            ShopDebugLogger.LogBuyBlockedByFullStack(
                itemKey,
                held,
                PlayerBagData.MaxStackPerItem,
                gold,
                price);
        }

        /// <summary>
        /// 购买单价：优先行上 Bake 价；Bake 为 0 时回退道具表买价。
        /// 原因：买价 ≤0 会让上限恒为 0，购买框任何输入都被钳没；贩卖上限不看单价所以不受影响。
        /// </summary>
        private static int ResolveBuyUnitPrice(ShopBarRowView row)
        {
            if (row == null)
            {
                return 0;
            }

            if (row.Price > 0)
            {
                return row.Price;
            }

            if (MainItemDefProvider.TryGetBuyPrice(row.ItemId, out var tablePrice) && tablePrice > 0)
            {
                return tablePrice;
            }

            return 0;
        }

        /// <summary>
        /// Σ(其它购买行 QuantityForTotal × ResolveBuyUnitPrice)；不含 <paramref name="excludeRow"/>。
        /// 与 Total2 / 决定扣款 / ResolveBuyMaxQuantity 同单价口径。
        /// </summary>
        private int SumBuyRowsCostExcluding(ShopBarRowView excludeRow)
        {
            var sum = 0;
            for (var i = 0; i < _buyRowViews.Count; i++)
            {
                var rowView = _buyRowViews[i];
                if (rowView == null || rowView == excludeRow)
                {
                    continue;
                }

                var input = rowView.GetComponent<ShopBuyRowQuantityInput>();
                var quantity = input != null ? input.QuantityForTotal : 0;
                if (quantity <= 0)
                {
                    continue;
                }

                sum += quantity * ResolveBuyUnitPrice(rowView);
            }

            return sum;
        }

        /// <summary>出售行最大可填数量 = 当前背包持有（按道具独立，无共享钱包）。</summary>
        private static int ResolveSellMaxQuantity(ShopBarRowView row)
        {
            if (row == null)
            {
                return 0;
            }

            var bag = ResolvePlayerBagData();
            return bag != null ? bag.GetMainItemCount(row.ItemId) : 0;
        }

        private void UnwireAllRowQuantityRefresh()
        {
            foreach (var input in _wiredQuantityInputs)
            {
                input?.ClearFormCallbacks();
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
