using System;
using Cinemachine;
using Game.GameMgr;
using Game.GameMgr.Component;
using Game.GameMgr.Component.Archive.ArchiveDataClass;
using Game.GameMgr.Component.ChangeScene;
using Game.GameMgr.Component.Cursor;
using Game.GameMgr.Component.UI;
using Game.GameRuntime.GameSceneManager.Base;
using Game.GameRuntime.GameSceneManager.Component;
using Game.GameRuntime.GameSceneManager.Component.CameraGSM;
using Game.GameRuntime.GameSceneManager.Component.Story;
using Game.GameRuntime.UI.FormLogic.Black;
using Game.GameRuntime.UI.FormLogic.Shop;
using Game.GameRuntime.UI.FormLogic.Story.Dialogue;
using Game.Static.Enum.Map;
using Game.Static.Name.Res;
using Game.Static.Path;
using UnityEngine;

namespace Game.GameRuntime.GameSceneManager.Scene.Village_Shop
{
    /// <summary>
    /// 肯姆尼村商店 <see cref="SceneName.Village_Shop"/> 纯 UI 场景管理器。
    /// 无玩家、无走路；店内 ESC = 离店回村（0829），不再开 MenuPanel。
    /// </summary>
    /// <remarks>
    /// 相机策略：场景挂齐 GSM 相机链避免未绑定 Error；进场后锁死并强制对准「商店界面合层」。
    /// 原因：无 Follow 时 FramingTransposer 可能把机位带偏，只 CancelFollow 仍看不见海报。
    /// 替代方案：把合层整页搬进 Overlay Canvas（0704 长线）——本阶段不采用。
    /// ESC：本 GSM 订阅 <c>InputComponentGM.onEscPressed</c> + <c>SetAllowOpenMenu(false)</c>，
    /// 勿改全局 <c>InputComponentGSM</c> 默认「ESC→菜单」语义（村内仍开菜单）。
    /// </remarks>
    public class Village_ShopSceneManager : BaseGameSceneManager
    {
        /// <summary>世界空间美术合层根节点名（与 Hierarchy 一致）。</summary>
        private const string ShopCompositeRootName = "商店界面合层";

        /// <summary>首次进店对白 Prefab 名（与 <c>Village_ShopStart.prefab</c>、存档键一致；存档只播一次）。</summary>
        private const string ShopStartStoryName = "Village_ShopStart";

        /// <summary>
        /// 非首次进店短招呼 Prefab 名（与 <c>Village_ShopRepeat.prefab</c> 对齐）。
        /// 原因：0830 R1——0827「二进宫静默」作废；Start used 后每次进店都播，禁止用 CheckStoryUsed 闸 Repeat。
        /// 勿与 Start / Yes / No / Head 混用。
        /// </summary>
        private const string ShopRepeatStoryName = "Village_ShopRepeat";

        /// <summary>
        /// 点头特殊对白 Prefab 名（与磁盘文件名对齐；可重复触发，不做存档旗标）。
        /// 原因：0829 方案 A——产品真源为 <c>Village_ShopHead.prefab</c>；旧名
        /// <c>Village_ShopKeeper_HeadClick</c> 无对应资源，TriggerStory 会加载失败。
        /// </summary>
        public const string ShopkeeperHeadClickStoryName = "Village_ShopHead";

        /// <summary>
        /// 点胸特殊对白 Prefab 名（店内 C1～C5；C6+ 树屋下期；可重复触发，不做存档旗标）。
        /// 原因：0830 方案 A——产品真源为 <c>Village_ShopChest.prefab</c>；旧名
        /// <c>Village_ShopKeeper_ChestClick</c> 无对应资源，TriggerStory 会加载失败（对齐 0829 Head）。
        /// 0601「建议 prefab 名 ChestClick」作废。
        /// </summary>
        public const string ShopkeeperChestClickStoryName = "Village_ShopChest";

        /// <summary>
        /// 购买成功对白 Prefab 名（与磁盘 <c>Village_ShopYes.prefab</c> 对齐）。
        /// 原因：0829 方案 A'——成败走特殊对白同管线，禁止 ShopFormLogic 直开 TriggerStory。
        /// </summary>
        public const string PurchaseSuccessStoryName = "Village_ShopYes";

        /// <summary>
        /// 金币不足对白 Prefab 名（与磁盘 <c>Village_ShopNo.prefab</c> 对齐）。
        /// 仅「TrySpend 失败」播；数量 0 / 堆叠超 / 出售未实现不播（文案是「没钱」）。
        /// </summary>
        public const string PurchaseFailInsufficientGoldStoryName = "Village_ShopNo";

        /// <summary>场景买卖 UI 根节点名（Hierarchy <c>UI_Shop</c>）；序列化引用优先，Find 兜底。</summary>
        private const string ShopUiRootName = "UI_Shop";
        /// <summary>合层下热区父节点名：<c> MerchantPainting/Trigger</c>。</summary>
        private const string ShopkeeperHotspotRootName = "Trigger";

        [SerializeField] private GameObject shopUiRoot;

        /// <summary>
        /// 头/胸热区根（可选序列化；空则运行时在合层下 Find「Trigger」）。
        /// 用于 <see cref="SetShopkeeperHotspotsEnabled"/> 总开关。
        /// </summary>
        [SerializeField] private Transform shopkeeperHotspotRoot;

        /// <summary>合层背景中心附近的默认机位（与执行说明一致）；找不到合层时兜底。</summary>
        private static readonly Vector3 FallbackCameraWorldPos = new Vector3(0.65f, -0.14f, -10f);

        /// <summary>对齐 HomeScene2 的正交尺寸，刚好框住 19.2×10.8 背景。</summary>
        private const float ShopOrthoSize = 5.4f;

        /// <summary>黑幕内 Trigger 后等待对话壳就绪再淡出（对齐 KenMuNi）。</summary>
        private const float ShopStartStoryReadyHoldSeconds = 0.15f;

        /// <summary>壳未就绪时防永久卡黑的超时（秒）。</summary>
        private const float ShopStartCoverTimeoutSeconds = 3f;

        /// <summary>对白结束演出黑幕：淡入时长（秒），明显慢于换场默认 1.0s。</summary>
        private const float ShopEndBlackShowDuration = 2.0f;

        /// <summary>对白结束演出黑幕：淡出时长（秒）。</summary>
        private const float ShopEndBlackHideDuration = 2.0f;

        /// <summary>结束黑幕全黑停留（秒），增强「盖住再揭」体感。</summary>
        private const float ShopEndBlackHoldSeconds = 0.4f;

        /// <summary>是否已订阅 <see cref="StoryComponentGSM.onStoryEnd"/>，用于离场时安全退订。</summary>
        private bool shopStartStoryEndSubscribed;

        /// <summary>特殊对白（点头/点胸）是否已订阅 onStoryEnd。</summary>
        private bool specialStoryEndSubscribed;

        /// <summary>是否已订阅 ESC→离店（OnEnter 订、OnDestroy 退）。</summary>
        private bool shopEscExitSubscribed;

        /// <summary>离店换场防抖：ESC 与离开按钮同源，避免连按双开 LoadScene。</summary>
        private bool isExitingShop;

        /// <summary>防止 onStoryTriggered 与超时双触发 CloseFormFade。</summary>
        private bool shopStartCoverCloseIssued;

        /// <summary>LoadScene 旁路交来的 CloseFormFade + OnBlackFadeEnd。</summary>
        private Action deferredCloseBlackAndNotify;

        public override void OnInit()
        {
            base.OnInit();

            // 逻辑自报场景名：供 LastSceneName 匹配、全局查询。
            nowSceneName = SceneName.Village_Shop;

            // 存档「当前地点」仍显示肯姆尼；商店不单独占 PlaceName。
            GetModule<PlayerHandlerComponentGSM>().SetNowPlace(PlaceName.KenMuNi);

            // 纯 UI 不跑 InitPlayer，基类不会自动关 FightingPanel；进店显式关掉血条 HUD。
            // 时机必须在 OnInit（黑幕仍全黑）：OnEnterScene 要等黑幕渐出结束才触发，关晚了会闪一下。
            // 原因：上一场景（如村里）打开的 FightingPanel 会跨场景残留。
            // 替代方案：①仍在 OnEnterScene 关（会闪）；②改基类 canCreatePlayer==false 也调 OpenFightingPanel——影响面大，不采用。
            CloseFightingPanelIfOpen();

            // 进店招呼前总藏买卖 UI + 关热区（首次 Start / 非首次 Repeat 均防闪 Bar）。
            // 原因：0830 起非首次也播 Repeat；若仅首次藏 UI，二进宫黑幕淡出后会先露 Bar（0827 F2 同类）。
            // 0827「二进宫不藏、直接 Idle」作废。
            HideShopUiRoot();
            SetShopkeeperHotspotsEnabled(false);
        }

        public override void OnEnterScene()
        {
            base.OnEnterScene();

            // 再写一次，避免切场顺序覆盖地点键。
            GetModule<PlayerHandlerComponentGSM>().SetNowPlace(PlaceName.KenMuNi);

            // 0829 改口：店内 ESC = 离店回村，不再开 MenuPanel（0713「店内可 ESC 开菜单」作废）。
            // 必须 SetAllowOpenMenu(false)，否则 InputComponentGSM.OnEscPressed 仍会 OpenUIForm(MenuPanel)。
            // 另订 InputComponentGM.onEscPressed → ExitShopToVillage（与离开按钮同源）。
            // 仅本场景订阅；村内无此订阅，仍走 InputComponentGSM → 菜单。
            var input = GetModule<InputComponentGSM>();
            if (input != null)
            {
                input.SetAllowOpenMenu(false);
                Debug.Log("[ShopEscExit] SetAllowOpenMenu(false) 店内禁 ESC 开菜单");
            }
            else
            {
                Debug.LogError("[ShopEscExit] InputComponentGSM 缺失，无法关菜单门卫。", this);
            }

            SubscribeShopEscExit();

            // 无玩家：取消跟拍并锁死，再强制对准合层（Brain 关掉，避免 CM 每帧改机位）。
            // 进店主路径在 TryDeferBlackFadeForCover 黑幕内对焦；此处兜底 blackFade=false。
            LockShopCameraPipeline();
            if (!ShouldPlayShopStartStory())
            {
                FocusMainCameraOnShopComposite();
                // 非首次 Idle/Repeat 前兜底脸，防 Debug F 键 / 上次对白残留非默认身脸。
                ResetShopkeeperPortraitDefault();
            }

            // 验收：确认从 Door_Shop 进来时 LastSceneName 为 Village_KenMuNi1
            var last = GameManager.GetGMComponent<ChangeSceneComponentGM>().LastSceneName;
            Debug.Log($"[VillageShopDebug] enter Village_Shop lastScene={last}");

            // 主路径已在换场黑幕 DeferCover 内 Trigger；此处仅兜底 Start / Repeat。
            TryTriggerShopStartStoryOnce();
            TryTriggerShopRepeatGreetingIfNeeded();

            // 非首次且无 RunningStory：才显 UI + 开热区（Defer 已起 Repeat 时勿抢开）。
            // OnInit 已总藏 UI，故无对白时必须 Show，否则二进宫失败路径会黑屏只见合层。
            if (!ShouldPlayShopStartStory())
            {
                var storyGsm = GetModule<StoryComponentGSM>();
                if (storyGsm == null || !storyGsm.HasRunningStory)
                {
                    ShowShopUiRoot();
                    SetShopkeeperHotspotsEnabled(true);
                }
            }
        }

        /// <summary>
        /// 进店换场黑幕内 Trigger 招呼对白，就绪后一次 CloseFormFade（防闪 Bar）。
        /// 首次 → <see cref="ShopStartStoryName"/>（分层闸门 + 结束慢黑幕）；
        /// 非首次 → <see cref="ShopRepeatStoryName"/>（无分层；结束对齐特殊对白直接显 UI）。
        /// </summary>
        /// <remarks>
        /// 0830 R1：同一管线分支故事名。0827「非首次 return false → 静默 Idle」作废。
        /// 替代（未采用）：仅 OnEnterScene 补 Trigger（易闪 Bar）；ShopFormLogic Awake 播（与 GSM 打架）。
        /// </remarks>
        public override bool TryDeferBlackFadeForCover(Action closeBlackAndNotify)
        {
            if (ShouldPlayShopStartStory())
            {
                return TryDeferCoverForShopStart(closeBlackAndNotify);
            }

            return TryDeferCoverForShopRepeat(closeBlackAndNotify);
        }

        /// <summary>首次进店：黑幕内 Trigger Start + 分层闸门。</summary>
        private bool TryDeferCoverForShopStart(Action closeBlackAndNotify)
        {
            var storyGsm = GetModule<StoryComponentGSM>();
            if (storyGsm == null)
            {
                Debug.LogWarning("[ShopStart] StoryComponentGSM 缺失，放弃延迟淡出");
                return false;
            }

            if (storyGsm.HasRunningStory)
            {
                Debug.LogWarning("[ShopStart] 已有剧情在跑，放弃延迟淡出");
                return false;
            }

            shopStartCoverCloseIssued = false;
            deferredCloseBlackAndNotify = closeBlackAndNotify;

            // 锁闸：图内 Wait 须等换场黑幕淡完才开始雅/古立绘淡入（0827 P1）。
            ShopStartLayerRevealGate.ResetForDeferredCover();

            // 黑幕下准备合层/相机（用户不可见），避免 OnEnterScene 后再对焦闪帧。
            HideShopUiRoot();
            SetShopkeeperHotspotsEnabled(false);
            LockShopCameraPipeline();
            FocusMainCameraOnShopComposite();

            storyGsm.onStoryTriggered += OnShopStartStoryTriggeredForCover;

            bool started = storyGsm.TriggerStory(ShopStartStoryName);
            if (!started)
            {
                CleanupShopStartCoverSubscriptions(storyGsm);
                deferredCloseBlackAndNotify = null;
                ShopStartLayerRevealGate.SignalBgFullyVisible();
                ShowShopUiRoot();
                SetShopkeeperHotspotsEnabled(true);
                Debug.LogWarning("[ShopStart] TriggerStory 未启动，回退默认淡出");
                return false;
            }

            storyGsm.onStoryEnd += OnShopStartStoryEnd;
            shopStartStoryEndSubscribed = true;
            Debug.Log("[ShopStart] 黑幕阶段 TriggerStory " + ShopStartStoryName + "，等待对话壳就绪后淡出");
            WaitForInvoke(ShopStartCoverTimeoutSeconds, OnShopStartCoverTimeout);
            return true;
        }

        /// <summary>
        /// 非首次进店：黑幕内 Trigger Repeat（短招呼）；不做分层 Prepare；结束走特殊对白语义。
        /// </summary>
        private bool TryDeferCoverForShopRepeat(Action closeBlackAndNotify)
        {
            var storyGsm = GetModule<StoryComponentGSM>();
            if (storyGsm == null)
            {
                Debug.LogWarning("[ShopRepeat] StoryComponentGSM 缺失，放弃延迟淡出");
                return false;
            }

            if (storyGsm.HasRunningStory)
            {
                Debug.LogWarning("[ShopRepeat] 已有剧情在跑，放弃延迟淡出");
                return false;
            }

            // 复用进店 Cover 防抖标志（与 Start 互斥，同帧只走一支）。
            shopStartCoverCloseIssued = false;
            deferredCloseBlackAndNotify = closeBlackAndNotify;

            HideShopUiRoot();
            SetShopkeeperHotspotsEnabled(false);
            LockShopCameraPipeline();
            FocusMainCameraOnShopComposite();

            storyGsm.onStoryTriggered += OnShopRepeatStoryTriggeredForCover;

            bool started = storyGsm.TriggerStory(ShopRepeatStoryName);
            if (!started)
            {
                CleanupShopRepeatCoverSubscriptions(storyGsm);
                deferredCloseBlackAndNotify = null;
                ShowShopUiRoot();
                SetShopkeeperHotspotsEnabled(true);
                Debug.LogWarning("[ShopRepeat] TriggerStory 未启动，回退默认淡出");
                return false;
            }

            // 结束 = Special：ResetDefault + Show UI + 热区；禁止 Start 慢黑幕。
            storyGsm.onStoryEnd += OnShopkeeperSpecialStoryEnd;
            specialStoryEndSubscribed = true;
            Debug.Log("[ShopRepeat] 黑幕阶段 TriggerStory " + ShopRepeatStoryName + "，等待对话壳就绪后淡出");
            WaitForInvoke(ShopStartCoverTimeoutSeconds, OnShopRepeatCoverTimeout);
            return true;
        }

        private void OnShopStartStoryTriggeredForCover()
        {
            var storyGsm = GetModule<StoryComponentGSM>();
            if (storyGsm != null)
            {
                storyGsm.onStoryTriggered -= OnShopStartStoryTriggeredForCover;
            }

            WaitForInvoke(ShopStartStoryReadyHoldSeconds, FinalizeShopStartCoverAndCloseBlack);
        }

        private void OnShopStartCoverTimeout()
        {
            if (shopStartCoverCloseIssued)
            {
                return;
            }

            Debug.LogWarning("[ShopStart] 对话壳 Ready 超时，强制淡出黑幕");
            FinalizeShopStartCoverAndCloseBlack();
        }

        private void FinalizeShopStartCoverAndCloseBlack()
        {
            if (shopStartCoverCloseIssued)
            {
                return;
            }

            shopStartCoverCloseIssued = true;

            // 淡出前强制雅/古大立绘与对话框 alpha=0，亮屏后由图内 Action 拉回（对齐 KenMuNi Prepare）。
            PrepareShopStartLayeredReveal();

            var close = deferredCloseBlackAndNotify;
            deferredCloseBlackAndNotify = null;
            if (close != null)
            {
                var loadGsm = GetModule<LoadSceneComponentGSM>();
                if (loadGsm != null)
                {
                    void OnBlackFullyGone()
                    {
                        loadGsm.onEndLoadingSceneEvent -= OnBlackFullyGone;
                        ShopStartLayerRevealGate.SignalBgFullyVisible();
                        Debug.Log("[ShopStart] 黑幕淡完，分层闸门开启（可开始立绘淡入）");
                    }

                    loadGsm.onEndLoadingSceneEvent += OnBlackFullyGone;
                }
                else
                {
                    ShopStartLayerRevealGate.SignalBgFullyVisible();
                }

                Debug.Log("[ShopStart] 立绘/框已藏，CloseFormFade");
                close.Invoke();
            }
            else
            {
                ShopStartLayerRevealGate.SignalBgFullyVisible();
            }
        }

        /// <summary>Repeat 进店：壳就绪后仅 CloseFormFade，不做分层 Prepare / 闸门。</summary>
        private void OnShopRepeatStoryTriggeredForCover()
        {
            var storyGsm = GetModule<StoryComponentGSM>();
            if (storyGsm != null)
            {
                storyGsm.onStoryTriggered -= OnShopRepeatStoryTriggeredForCover;
            }

            WaitForInvoke(ShopStartStoryReadyHoldSeconds, FinalizeShopRepeatCoverAndCloseBlack);
        }

        private void OnShopRepeatCoverTimeout()
        {
            if (shopStartCoverCloseIssued)
            {
                return;
            }

            Debug.LogWarning("[ShopRepeat] 对话壳 Ready 超时，强制淡出黑幕");
            FinalizeShopRepeatCoverAndCloseBlack();
        }

        /// <summary>
        /// Repeat 黑幕淡出：无雅/古大立绘，跳过 Prepare 与 LayerRevealGate。
        /// </summary>
        private void FinalizeShopRepeatCoverAndCloseBlack()
        {
            if (shopStartCoverCloseIssued)
            {
                return;
            }

            shopStartCoverCloseIssued = true;

            var close = deferredCloseBlackAndNotify;
            deferredCloseBlackAndNotify = null;
            if (close != null)
            {
                Debug.Log("[ShopRepeat] CloseFormFade（无分层闸门）");
                close.Invoke();
            }
        }

        /// <summary>
        /// 分层显现准备（白名单）：藏字幕条 + DialogueScene 下雅/古大立绘；不碰 Mask 小头像。
        /// </summary>
        private void PrepareShopStartLayeredReveal()
        {
            var uiPath = UIPrefabPath.GetUIPrefabPath("NormalDialogueNewPanel");
            var uiForm = GameManager.GetGMComponent<UIComponentGM>().GetUIForm(uiPath);
            if (uiForm == null || uiForm.Logic == null)
            {
                return;
            }

            var logicRoot = uiForm.Logic;

            if (logicRoot is NormalDialogueFormNewLogic dialogueLogic
                && dialogueLogic.dialogueUICanvasGroup != null)
            {
                dialogueLogic.dialogueUICanvasGroup.alpha = 0f;
            }

            var sceneRoot = UIUtils.findChild(logicRoot.gameObject, "DialogueSceneContainer", hasDebugLog: false);
            if (sceneRoot == null)
            {
                Debug.LogWarning("[ShopStart][Prepare] 未找到 DialogueSceneContainer，跳过场景立绘白名单");
                return;
            }

            SetScenePaintingCanvasGroupAlpha(sceneRoot, "GoOutStoryYaerPainting", 0f);
            SetScenePaintingCanvasGroupAlpha(sceneRoot, "GushaPainting", 0f);
        }

        private static void SetScenePaintingCanvasGroupAlpha(GameObject sceneRoot, string paintingObjectName, float alpha)
        {
            var paintingGo = UIUtils.findChild(sceneRoot, paintingObjectName, hasDebugLog: false);
            if (paintingGo == null)
            {
                return;
            }

            var cg = paintingGo.GetComponent<CanvasGroup>();
            if (cg == null)
            {
                return;
            }

            cg.alpha = alpha;
            Debug.Log("[ShopStart][Prepare] hide " + paintingObjectName);
        }

        private void CleanupShopStartCoverSubscriptions(StoryComponentGSM storyGsm)
        {
            if (storyGsm != null)
            {
                storyGsm.onStoryTriggered -= OnShopStartStoryTriggeredForCover;
            }
        }

        private void CleanupShopRepeatCoverSubscriptions(StoryComponentGSM storyGsm)
        {
            if (storyGsm != null)
            {
                storyGsm.onStoryTriggered -= OnShopRepeatStoryTriggeredForCover;
            }
        }

        private void OnDestroy()
        {
            UnsubscribeShopEscExit();
            UnsubscribeShopStartStoryEnd();
            UnsubscribeSpecialStoryEnd();
            var storyGsm = GetModule<StoryComponentGSM>();
            CleanupShopStartCoverSubscriptions(storyGsm);
            CleanupShopRepeatCoverSubscriptions(storyGsm);
        }

        /// <summary>
        /// 订阅全局 ESC：仅本店场景生效，与离开按钮共用 <see cref="ExitShopToVillage"/>。
        /// </summary>
        private void SubscribeShopEscExit()
        {
            if (shopEscExitSubscribed)
            {
                return;
            }

            var inputGm = GameManager.GetGMComponent<InputComponentGM>();
            if (inputGm == null)
            {
                Debug.LogError("[ShopEscExit] InputComponentGM 缺失，无法订阅 ESC 离店。", this);
                return;
            }

            inputGm.onEscPressed += OnShopEscPressed;
            shopEscExitSubscribed = true;
            Debug.Log("[ShopEscExit] subscribed ESC → 离店回村");
        }

        /// <summary>离场退订，避免切回村后仍响应 ESC 离店。</summary>
        private void UnsubscribeShopEscExit()
        {
            if (!shopEscExitSubscribed)
            {
                return;
            }

            var inputGm = GameManager.GetGMComponent<InputComponentGM>();
            if (inputGm != null)
            {
                inputGm.onEscPressed -= OnShopEscPressed;
            }

            shopEscExitSubscribed = false;
        }

        /// <summary>
        /// 店内 ESC：禁菜单已由 cantOpenMenu 挡住 InputComponentGSM；此处只负责离店。
        /// 对白中默认不离店（OPEN Q1）；禁止在 Update 里 GetKeyDown(Escape)。
        /// </summary>
        private void OnShopEscPressed()
        {
            if (isExitingShop)
            {
                return;
            }

            var storyGsm = GetModule<StoryComponentGSM>();
            if (storyGsm != null && storyGsm.HasRunningStory)
            {
                Debug.Log("[ShopEscExit] HasRunningStory，忽略 ESC 离店");
                return;
            }

            Debug.Log("[ShopEscExit] ESC → ExitShopToVillage");
            ExitShopToVillage();
        }

        /// <summary>
        /// 离店回村统一入口：ESC 与离开按钮同源。
        /// LastSceneName 变为 Village_Shop → 村里 EnterPos 命中 EnterFrom_Shop（免动）。
        /// </summary>
        /// <remarks>
        /// stayAction 必须用 <see cref="HideShopUiRoot"/>，禁止 <c>ShopFormLogic.CloseForm</c>。
        /// 原因：正规进店是场景常驻 UI_Shop，无 GF <c>UIForm</c> 组件；
        /// <c>CloseForm</c> → <c>CloseUIForm(null)</c> 抛 <c>UI form is invalid</c>，
        /// 黑幕 onShowEnd 里异常会阻断后续 <c>LoadScene</c> → 永久黑屏（0829 验收复现）。
        /// 替代方案：给 UI_Shop 补 UIForm 并走 OpenUIForm——与 0713 双轨冲突，不采用。
        /// </remarks>
        public void ExitShopToVillage()
        {
            if (isExitingShop)
            {
                Debug.Log("[ShopEscExit] 已在离店，忽略重复请求");
                return;
            }

            isExitingShop = true;

            var loadGsm = GetModule<LoadSceneComponentGSM>();
            if (loadGsm == null)
            {
                isExitingShop = false;
                Debug.LogError("[ShopEscExit] LoadSceneComponentGSM 缺失，无法回村。", this);
                return;
            }

            // 黑幕全黑时只藏场景 UI；真正卸载随换场 OnShutDown / UnloadScene。
            Action stayAction = () =>
            {
                HideShopUiRoot();
                Debug.Log("[ShopEscExit] stayAction HideShopUiRoot（非 CloseForm）");
            };

            Debug.Log("[ShopEscExit] LoadScene Village_KenMuNi1 (Hide UI_Shop on black stay)");
            loadGsm.LoadScene(SceneName.Village_KenMuNi1, stayAction);
        }

        /// <summary>
        /// 购买成败短对白：映射 Yes/No 后走 <see cref="TryTriggerShopkeeperSpecial"/>。
        /// 购买成败 = 特殊对白同管线（Hide UI / 热区 / HasRunningStory / 结束 ResetDefault）。
        /// </summary>
        /// <param name="purchaseSucceeded">
        /// true → <see cref="PurchaseSuccessStoryName"/>（入包成功，含旁路未扣款仍入包）；
        /// false → <see cref="PurchaseFailInsufficientGoldStoryName"/>（仅金币不足）。
        /// </param>
        /// <returns>是否成功启动剧情。</returns>
        /// <remarks>
        /// 时序：调用方须先完成结算或失败判定，再调本方法；禁止先播对白再扣款。
        /// 替代方案：ShopFormLogic 直接 TriggerStory——易漏藏 UI / 叠对白，已否决（方案 B）。
        /// </remarks>
        public bool TryTriggerPurchaseResult(bool purchaseSucceeded)
        {
            var storyName = purchaseSucceeded
                ? PurchaseSuccessStoryName
                : PurchaseFailInsufficientGoldStoryName;
            return TryTriggerShopkeeperSpecial(storyName);
        }

        /// <summary>
        /// Idle 下点击 Head/Chest：藏买卖 UI、关热区、Trigger 独立特殊对白。
        /// 不走首次进店存档旗标；不播结束黑幕（与 ShopStart 解耦）。
        /// 购买成败亦经此门（见 <see cref="TryTriggerPurchaseResult"/>）。
        /// </summary>
        /// <param name="storyName">
        /// <see cref="ShopkeeperHeadClickStoryName"/> / <see cref="ShopkeeperChestClickStoryName"/> /
        /// <see cref="PurchaseSuccessStoryName"/> / <see cref="PurchaseFailInsufficientGoldStoryName"/>。
        /// </param>
        /// <returns>是否成功启动剧情。</returns>
        public bool TryTriggerShopkeeperSpecial(string storyName)
        {
            if (string.IsNullOrEmpty(storyName))
            {
                Debug.LogWarning("[ShopSpecial] storyName 为空", this);
                return false;
            }

            var storyGsm = GetModule<StoryComponentGSM>();
            if (storyGsm == null)
            {
                Debug.LogWarning("[ShopSpecial] StoryComponentGSM 缺失", this);
                return false;
            }

            // 首次进店或任意对白进行中：拒绝叠开（HasRunningStory + 热区 OFF 双保险）。
            if (storyGsm.HasRunningStory)
            {
                Debug.Log("[ShopSpecial] HasRunningStory，忽略 " + storyName);
                return false;
            }

            if (ShouldPlayShopStartStory())
            {
                // 首对话尚未播完 / 仍待 Trigger：禁止特殊线抢跑。
                Debug.Log("[ShopSpecial] 仍处首次进店窗口，忽略 " + storyName);
                return false;
            }

            HideShopUiRoot();
            SetShopkeeperHotspotsEnabled(false);

            bool started = storyGsm.TriggerStory(storyName);
            if (!started)
            {
                ShowShopUiRoot();
                SetShopkeeperHotspotsEnabled(true);
                Debug.LogWarning("[ShopSpecial] TriggerStory 未启动 " + storyName, this);
                return false;
            }

            storyGsm.onStoryEnd += OnShopkeeperSpecialStoryEnd;
            specialStoryEndSubscribed = true;
            Debug.Log("[ShopSpecial] TriggerStory " + storyName);
            return true;
        }

        /// <summary>
        /// 开关 Head/Chest 热区：Collider、<see cref="ShopkeeperBodyHotspot"/>、以及同物体上的
        /// <see cref="CursorChangeTrigger"/>。
        /// 对白期 / 首次进店期强制关，防漏点与光标卡态。
        /// </summary>
        /// <remarks>
        /// 重要：必须同步 disable <see cref="CursorChangeTrigger"/>。
        /// 原因：只关 Collider 时 Trigger 的 Update 仍跑，OverlapPoint 可能仍判「在头上」，
        /// 对白中会卡在 Catch/Chat；Trigger.OnDisable 才会走 Exit 出队。
        /// 替代方案：方案 B（IPointerEnter/Exit 写在 Hotspot 里）——本期不采用，与村内样例对齐方案 A。
        /// </remarks>
        public void SetShopkeeperHotspotsEnabled(bool enabled)
        {
            var root = ResolveShopkeeperHotspotRoot();
            if (root == null)
            {
                return;
            }

            var colliders = root.GetComponentsInChildren<Collider2D>(true);
            for (var i = 0; i < colliders.Length; i++)
            {
                if (colliders[i] != null)
                {
                    colliders[i].enabled = enabled;
                }
            }

            var hotspots = root.GetComponentsInChildren<ShopkeeperBodyHotspot>(true);
            for (var i = 0; i < hotspots.Length; i++)
            {
                if (hotspots[i] != null)
                {
                    hotspots[i].enabled = enabled;
                }
            }

            // 0829：Head 悬停 Catch 等光标入口与点击热区同开同关。
            var cursorTriggers = root.GetComponentsInChildren<CursorChangeTrigger>(true);
            for (var i = 0; i < cursorTriggers.Length; i++)
            {
                if (cursorTriggers[i] != null)
                {
                    cursorTriggers[i].enabled = enabled;
                }
            }

            Debug.Log(
                $"[ShopSpecial] SetShopkeeperHotspotsEnabled({enabled}) " +
                $"colliders={colliders.Length} hotspots={hotspots.Length} cursorTriggers={cursorTriggers.Length}");
        }

        /// <summary>
        /// 特殊对白 / 非首次进店 Repeat 结束：直接显买卖 UI 并开热区（无 Start 结束慢黑幕）。
        /// </summary>
        private void OnShopkeeperSpecialStoryEnd()
        {
            UnsubscribeSpecialStoryEnd();
            // 点头末 Face5+Red / 点胸末 Face2+Red / Repeat 合层脸 → Idle 前回默认（0828 / 0830）。
            ResetShopkeeperPortraitDefault();
            ShowShopUiRoot();
            SetShopkeeperHotspotsEnabled(true);
            Debug.Log("[ShopSpecial] onStoryEnd → ResetDefault + Show UI_Shop + 热区 ON（含 ShopRepeat）");
        }

        /// <summary>
        /// 合层商人立绘回 Normal+Face1（经 <see cref="ShopkeeperFaceRegistry"/>）。
        /// 挂在对白结束与进店兜底；不改点头胸 Trigger / CSV。
        /// </summary>
        private static void ResetShopkeeperPortraitDefault()
        {
            ShopkeeperFaceRegistry.ResetDefault();
            Debug.Log("[ShopPortrait] ResetDefault → Normal + Face1");
        }

        private void UnsubscribeSpecialStoryEnd()
        {
            if (!specialStoryEndSubscribed)
            {
                return;
            }

            var storyGsm = GetModule<StoryComponentGSM>();
            if (storyGsm != null)
            {
                storyGsm.onStoryEnd -= OnShopkeeperSpecialStoryEnd;
            }

            specialStoryEndSubscribed = false;
        }

        private Transform ResolveShopkeeperHotspotRoot()
        {
            if (shopkeeperHotspotRoot != null)
            {
                return shopkeeperHotspotRoot;
            }

            var composite = GameObject.Find(ShopCompositeRootName);
            if (composite == null)
            {
                return null;
            }

            // 场景实例名为「 MerchantPainting」（前导空格，与 Hierarchy 一致）。
            Transform painting = null;
            for (var i = 0; i < composite.transform.childCount; i++)
            {
                var child = composite.transform.GetChild(i);
                if (child != null && child.name.IndexOf("MerchantPainting", System.StringComparison.Ordinal) >= 0)
                {
                    painting = child;
                    break;
                }
            }

            if (painting == null)
            {
                return null;
            }

            shopkeeperHotspotRoot = painting.Find(ShopkeeperHotspotRootName);
            if (shopkeeperHotspotRoot == null)
            {
                Debug.LogWarning(
                    "[ShopSpecial] 未找到 MerchantPainting/Trigger。请运行 Tools/Dialogue/Setup Shopkeeper Hotspots。",
                    this);
            }

            return shopkeeperHotspotRoot;
        }

        /// <summary>
        /// 本档是否尚未播过首次进店对白（<see cref="StoryTriggerCountData.CheckStoryUsed"/>）。
        /// 仅闸 <see cref="ShopStartStoryName"/>；非首次改播 Repeat，勿用本方法判断「进店是否说话」。
        /// </summary>
        private bool ShouldPlayShopStartStory()
        {
            var counts = GetArchiveData<StoryTriggerCountData>();
            return counts == null || !counts.CheckStoryUsed(ShopStartStoryName);
        }

        /// <summary>
        /// 同档首次进入商店时 Trigger 开场对白（OnEnterScene 兜底）。
        /// 主路径已改到 <see cref="TryDeferBlackFadeForCover"/>。
        /// </summary>
        private void TryTriggerShopStartStoryOnce()
        {
            if (!ShouldPlayShopStartStory())
            {
                return;
            }

            var storyGsm = GetModule<StoryComponentGSM>();
            if (storyGsm == null)
            {
                Debug.LogWarning("[ShopStart] StoryComponentGSM 缺失，跳过 " + ShopStartStoryName);
                return;
            }

            if (storyGsm.HasRunningStory)
            {
                // 黑幕阶段已启动：静默跳过，避免双开告警刷屏。
                return;
            }

            HideShopUiRoot();
            SetShopkeeperHotspotsEnabled(false);
            bool started = storyGsm.TriggerStory(ShopStartStoryName);
            if (started)
            {
                storyGsm.onStoryEnd += OnShopStartStoryEnd;
                shopStartStoryEndSubscribed = true;
                Debug.Log("[ShopStart] OnEnterScene 兜底 TriggerStory " + ShopStartStoryName);
            }
            else
            {
                ShowShopUiRoot();
                SetShopkeeperHotspotsEnabled(true);
                Debug.LogWarning("[ShopStart] TriggerStory 未启动 " + ShopStartStoryName);
            }
        }

        /// <summary>
        /// 非首次进店短招呼兜底（Defer 未跑 / blackFade=false）。
        /// </summary>
        /// <remarks>
        /// 禁止 <c>CheckStoryUsed(Village_ShopRepeat)</c> 闸门——每次非首次都播（0830）。
        /// 0827「二进宫静默」作废。结束对齐 <see cref="OnShopkeeperSpecialStoryEnd"/>（无慢黑幕）。
        /// </remarks>
        private void TryTriggerShopRepeatGreetingIfNeeded()
        {
            // 首次窗口只走 Start，勿与 Repeat 同开。
            if (ShouldPlayShopStartStory())
            {
                return;
            }

            var storyGsm = GetModule<StoryComponentGSM>();
            if (storyGsm == null)
            {
                Debug.LogWarning("[ShopRepeat] StoryComponentGSM 缺失，跳过 " + ShopRepeatStoryName);
                return;
            }

            if (storyGsm.HasRunningStory)
            {
                // Defer 已起 Repeat：静默跳过。
                return;
            }

            HideShopUiRoot();
            SetShopkeeperHotspotsEnabled(false);
            bool started = storyGsm.TriggerStory(ShopRepeatStoryName);
            if (started)
            {
                storyGsm.onStoryEnd += OnShopkeeperSpecialStoryEnd;
                specialStoryEndSubscribed = true;
                Debug.Log("[ShopRepeat] OnEnterScene 兜底 TriggerStory " + ShopRepeatStoryName);
            }
            else
            {
                ShowShopUiRoot();
                SetShopkeeperHotspotsEnabled(true);
                Debug.LogWarning("[ShopRepeat] TriggerStory 未启动 " + ShopRepeatStoryName);
            }
        }

        /// <summary>首次对白结束：慢黑幕 → 可选 hold → 显买卖 UI → 慢淡出 → 开热区。</summary>
        private void OnShopStartStoryEnd()
        {
            UnsubscribeShopStartStoryEnd();

            ShowShopBlackFade(
                blackForm =>
                {
                    WaitForInvoke(ShopEndBlackHoldSeconds, () =>
                    {
                        // 全黑 hold 内复位：ShopStart 末句 Face2+Red 不带到 Idle（Q4）。
                        ResetShopkeeperPortraitDefault();
                        ShowShopUiRoot();
                        SetShopkeeperHotspotsEnabled(true);
                        blackForm.CloseFormFade(() =>
                            Debug.Log("[ShopStart] onStoryEnd，黑幕淡出后显示 UI_Shop + 热区 ON"));
                    });
                },
                ShopEndBlackShowDuration,
                ShopEndBlackHideDuration);
        }

        /// <summary>打开系统 BlackPanel 并淡入黑幕，全黑后执行 <paramref name="onBlackReady"/>。</summary>
        private static void ShowShopBlackFade(
            Action<BlackFormLogic> onBlackReady,
            float? showDuration = null,
            float? hideDuration = null)
        {
            var uiPath = UIPrefabPath.GetUIPrefabPath("BlackPanel");
            GameManager.GetGMComponent<UIComponentGM>().OpenUIForm(uiPath, EUIGroup.System, new OpenFormArgs
            {
                userData = new ShowBlackFormArgs
                {
                    showType = BlackFadeType.FadeShow,
                    showDuration = showDuration,
                    hideDuration = hideDuration,
                    onShowEnd = onBlackReady
                }
            });
        }

        private GameObject ResolveShopUiRoot()
        {
            if (shopUiRoot != null)
            {
                return shopUiRoot;
            }

            shopUiRoot = GameObject.Find(ShopUiRootName);
            if (shopUiRoot == null)
            {
                Debug.LogWarning("[ShopStart] 未找到 UI_Shop。", this);
            }

            return shopUiRoot;
        }

        private void HideShopUiRoot()
        {
            var root = ResolveShopUiRoot();
            if (root != null)
            {
                root.SetActive(false);
            }
        }

        private void ShowShopUiRoot()
        {
            var root = ResolveShopUiRoot();
            if (root != null)
            {
                root.SetActive(true);
            }
        }

        private void UnsubscribeShopStartStoryEnd()
        {
            if (!shopStartStoryEndSubscribed)
            {
                return;
            }

            var storyGsm = GetModule<StoryComponentGSM>();
            if (storyGsm != null)
            {
                storyGsm.onStoryEnd -= OnShopStartStoryEnd;
            }

            shopStartStoryEndSubscribed = false;
        }

        /// <summary>
        /// 若 FightingPanel（血条 HUD）仍开着则关掉；供 OnInit 在全黑阶段调用，避免渐出后闪一下。
        /// </summary>
        private static void CloseFightingPanelIfOpen()
        {
            var fightingPath = UIPrefabPath.GetUIPrefabPath("FightingPanel");
            var ui = GameManager.GetGMComponent<UIComponentGM>();
            if (ui == null)
            {
                return;
            }

            if (ui.GetUIForm(fightingPath) != null)
            {
                ui.CloseUIForm(fightingPath);
                Debug.Log("[VillageShopDebug] CloseUIForm FightingPanel (OnInit, while black)");
            }
        }

        /// <summary>
        /// 取消跟拍 + 加锁；CameraComponent 未绑定时只打日志，不抛空引用中断进场。
        /// </summary>
        private void LockShopCameraPipeline()
        {
            var cameraGsm = GetModule<CameraComponentGSM>();
            if (cameraGsm == null)
            {
                Debug.LogWarning("[VillageShopDebug] CameraComponentGSM 缺失，跳过 CancelFollow/SetLock。", this);
                return;
            }

            if (cameraGsm.CameraComponent == null)
            {
                Debug.LogWarning(
                    "[VillageShopDebug] CameraComponentGSM.cameraComponent 未绑定：请检查 SceneManager/Camera 引用。",
                    this);
                cameraGsm.SetLock(true);
                return;
            }

            cameraGsm.CancelFollow();
            cameraGsm.SetLock(true);
        }

        /// <summary>
        /// 强制主相机对准合层精灵包围盒中心，并关闭 CinemachineBrain，避免无 Follow 时被带偏。
        /// </summary>
        private void FocusMainCameraOnShopComposite()
        {
            var composite = GameObject.Find(ShopCompositeRootName);
            if (composite != null)
            {
                composite.SetActive(true);
            }
            else
            {
                Debug.LogWarning($"[VillageShopDebug] 未找到「{ShopCompositeRootName}」。", this);
            }

            var cam = ResolveShopMainCamera();
            if (cam == null)
            {
                Debug.LogError("[VillageShopDebug] 找不到主相机，合层无法显示。", this);
                return;
            }

            // 商店无跟拍：关掉 Brain，否则 FramingTransposer 在 Follow=null 时可能把机位拽飞/留在村里坐标。
            var brain = cam.GetComponent<CinemachineBrain>();
            if (brain != null)
            {
                brain.enabled = false;
            }

            cam.enabled = true;
            if (!cam.CompareTag("MainCamera"))
            {
                cam.tag = "MainCamera";
            }

            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Color.black;
            cam.orthographic = true;
            cam.depth = -1;
            // 保证 Default 层（合层 Sprite）在裁剪掩码内；~0 即 Everything。
            cam.cullingMask = ~0;

            // 正交尺寸固定 5.4（对齐 HomeScene2 / 19.2×10.8 背景）。
            // 原因：曾用「全合层包围盒 × 1.08」自动放大，5.4→约 5.83，进店会被刷成 5.8 并出黑边。
            // 替代方案：只按「背景」一张图算 need —— 仍可能因分辨率比例漂移，商店锁死固定值更稳。
            var ortho = ShopOrthoSize;
            var focusXy = FallbackCameraWorldPos;
            var rendererCount = 0;

            if (composite != null)
            {
                // 优先对准名为「背景」的精灵中心；找不到再退回文档兜底坐标。
                var bg = composite.transform.Find("背景");
                if (bg != null)
                {
                    focusXy = bg.position;
                    var bgRenderer = bg.GetComponent<SpriteRenderer>();
                    if (bgRenderer != null)
                    {
                        focusXy = bgRenderer.bounds.center;
                    }
                }

                var renderers = composite.GetComponentsInChildren<SpriteRenderer>(true);
                rendererCount = renderers != null ? renderers.Length : 0;
            }

            var camPos = new Vector3(focusXy.x, focusXy.y, -10f);
            cam.transform.position = camPos;
            cam.orthographicSize = ortho;

            // 若 vcam 仍在，同步机位，避免以后重新启用 Brain 时跳回旧坐标。
            var cameraGsm = GetModule<CameraComponentGSM>();
            var vcam = cameraGsm != null && cameraGsm.CameraComponent != null
                ? cameraGsm.CameraComponent.VirtualCamera
                : null;
            if (vcam != null)
            {
                vcam.transform.position = camPos;
                vcam.m_Lens.OrthographicSize = ortho;
                vcam.Follow = null;
                vcam.PreviousStateIsValid = false;
            }

            Debug.Log(
                $"[VillageShopDebug] focus camPos={camPos} ortho={ortho:F2} " +
                $"camEnabled={cam.enabled} brain={(brain != null && brain.enabled)} " +
                $"composite={(composite != null)} spriteRenderers={rendererCount} " +
                $"vcam={(vcam != null)}",
                this);
        }

        /// <summary>优先 Camera.main；否则找 Tag=MainCamera；再否则场景里第一台非 UI 相机。</summary>
        private static Camera ResolveShopMainCamera()
        {
            if (Camera.main != null)
            {
                return Camera.main;
            }

            var tagged = GameObject.FindWithTag("MainCamera");
            if (tagged != null)
            {
                var taggedCam = tagged.GetComponent<Camera>();
                if (taggedCam != null)
                {
                    return taggedCam;
                }
            }

            var all = UnityEngine.Object.FindObjectsOfType<Camera>();
            for (var i = 0; i < all.Length; i++)
            {
                // UICamera 只渲 UI 层（mask 常为 1<<5），跳过它。
                if (all[i] != null && all[i].cullingMask != (1 << 5))
                {
                    return all[i];
                }
            }

            return all != null && all.Length > 0 ? all[0] : null;
        }

        /// <summary>室内语义：虽无玩家脚步，与民居室内保持一致。</summary>
        public override TerrainType GetCurSceneTerrainType() => TerrainType.IndoorType;

        public override void initAllSceneMonster() { }
    }
}
