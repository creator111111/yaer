using System;
using Cinemachine;
using Game.GameMgr;
using Game.GameMgr.Component;
using Game.GameMgr.Component.Archive.ArchiveDataClass;
using Game.GameMgr.Component.ChangeScene;
using Game.GameMgr.Component.UI;
using Game.GameRuntime.GameSceneManager.Base;
using Game.GameRuntime.GameSceneManager.Component;
using Game.GameRuntime.GameSceneManager.Component.CameraGSM;
using Game.GameRuntime.GameSceneManager.Component.Story;
using Game.GameRuntime.UI.FormLogic.Black;
using Game.GameRuntime.UI.FormLogic.Story.Dialogue;
using Game.Static.Enum.Map;
using Game.Static.Name.Res;
using Game.Static.Path;
using UnityEngine;

namespace Game.GameRuntime.GameSceneManager.Scene.Village_Shop
{
    /// <summary>
    /// 肯姆尼村商店 <see cref="SceneName.Village_Shop"/> 纯 UI 场景管理器。
    /// 无玩家、无走路；ESC 菜单靠基类默认挂载的 <c>InputComponentGSM</c>。
    /// </summary>
    /// <remarks>
    /// 相机策略：场景挂齐 GSM 相机链避免未绑定 Error；进场后锁死并强制对准「商店界面合层」。
    /// 原因：无 Follow 时 FramingTransposer 可能把机位带偏，只 CancelFollow 仍看不见海报。
    /// 替代方案：把合层整页搬进 Overlay Canvas（0704 长线）——本阶段不采用。
    /// </remarks>
    public class Village_ShopSceneManager : BaseGameSceneManager
    {
        /// <summary>世界空间美术合层根节点名（与 Hierarchy 一致）。</summary>
        private const string ShopCompositeRootName = "商店界面合层";

        /// <summary>首次进店对白 Prefab 名（与 <c>Village_ShopStart.prefab</c>、存档键一致）。</summary>
        private const string ShopStartStoryName = "Village_ShopStart";

        /// <summary>场景买卖 UI 根节点名（Hierarchy <c>UI_Shop</c>）；序列化引用优先，Find 兜底。</summary>
        private const string ShopUiRootName = "UI_Shop";

        [SerializeField] private GameObject shopUiRoot;

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

            // 首次进店：换场黑幕仍全黑时藏买卖 UI，避免 OnEnterScene 后闪一帧 Bar（0827 F2）。
            if (ShouldPlayShopStartStory())
            {
                HideShopUiRoot();
            }
        }

        public override void OnEnterScene()
        {
            base.OnEnterScene();

            // 再写一次，避免切场顺序覆盖地点键。
            GetModule<PlayerHandlerComponentGSM>().SetNowPlace(PlaceName.KenMuNi);

            // 纯 UI 无玩家：不会走 PlayerLogic.LoadingSceneEndHandle 放行菜单；显式放行 ESC。
            // 若看不到下方「subscribed ESC」日志，说明 InitModules 曾在 Input 前中断（已修 Map 空引用）。
            var input = GetModule<InputComponentGSM>();
            if (input != null)
            {
                input.SetAllowOpenMenu(true);
                Debug.Log("[VillageShopDebug] SetAllowOpenMenu(true)");
            }
            else
            {
                Debug.LogError("[VillageShopDebug] InputComponentGSM 缺失，ESC 无法开菜单。", this);
            }

            // 无玩家：取消跟拍并锁死，再强制对准合层（Brain 关掉，避免 CM 每帧改机位）。
            // 首次进店主路径在 TryDeferBlackFadeForCover 黑幕内对焦；此处兜底二进宫与 blackFade=false。
            LockShopCameraPipeline();
            if (!ShouldPlayShopStartStory())
            {
                FocusMainCameraOnShopComposite();
            }

            // 验收：确认从 Door_Shop 进来时 LastSceneName 为 Village_KenMuNi1
            var last = GameManager.GetGMComponent<ChangeSceneComponentGM>().LastSceneName;
            Debug.Log($"[VillageShopDebug] enter Village_Shop lastScene={last}");

            // 主路径已在换场黑幕 DeferCover 内 Trigger；此处仅兜底。
            TryTriggerShopStartStoryOnce();
        }

        /// <summary>
        /// 首次进店：换场黑幕仍全黑时 TriggerStory，就绪后一次 CloseFormFade（对齐 KenMuNi，消除闪店 R1）。
        /// </summary>
        public override bool TryDeferBlackFadeForCover(Action closeBlackAndNotify)
        {
            if (!ShouldPlayShopStartStory())
            {
                return false;
            }

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
                Debug.LogWarning("[ShopStart] TriggerStory 未启动，回退默认淡出");
                return false;
            }

            storyGsm.onStoryEnd += OnShopStartStoryEnd;
            shopStartStoryEndSubscribed = true;
            Debug.Log("[ShopStart] 黑幕阶段 TriggerStory " + ShopStartStoryName + "，等待对话壳就绪后淡出");
            WaitForInvoke(ShopStartCoverTimeoutSeconds, OnShopStartCoverTimeout);
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

        private void OnDestroy()
        {
            UnsubscribeShopStartStoryEnd();
            var storyGsm = GetModule<StoryComponentGSM>();
            CleanupShopStartCoverSubscriptions(storyGsm);
        }

        /// <summary>
        /// 本档是否尚未播过首次进店对白（<see cref="StoryTriggerCountData.CheckStoryUsed"/>）。
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
                Debug.LogWarning("[ShopStart] TriggerStory 未启动 " + ShopStartStoryName);
            }
        }

        /// <summary>首次对白结束：慢黑幕 → 可选 hold → 显买卖 UI → 慢淡出。</summary>
        private void OnShopStartStoryEnd()
        {
            UnsubscribeShopStartStoryEnd();

            ShowShopBlackFade(
                blackForm =>
                {
                    WaitForInvoke(ShopEndBlackHoldSeconds, () =>
                    {
                        ShowShopUiRoot();
                        blackForm.CloseFormFade(() =>
                            Debug.Log("[ShopStart] onStoryEnd，黑幕淡出后显示 UI_Shop"));
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
