using System;
using Game.GameMgr;
using Game.GameMgr.Component;
using Game.GameMgr.Component.Archive.ArchiveDataClass;
using Game.GameMgr.Component.ChangeScene;
using Game.GameMgr.Component.UI;
using Game.GameRuntime.Entities.Player;
using Game.GameRuntime.Entities.Player.Components;
using Game.GameRuntime.GameSceneManager.Base;
using Game.GameRuntime.GameSceneManager.Component;
using Game.GameRuntime.GameSceneManager.Component.Story;
using Game.GameRuntime.UI.FormLogic.Black;
using Game.GameRuntime.UI.FormLogic.Story.Dialogue;
using Game.Static.Enum.Map;
using Game.Static.Name.Res;
using Game.Static.Path;
using GameFramework.UnityRuntime.UI;
using NodeCanvas.DialogueTrees;
using UnityEngine;

namespace Game.GameRuntime.GameSceneManager.Scene.Village_House
{
    /// <summary>
    /// 肯姆尼村长家室内（<see cref="SceneName.Village_Chief_House"/>）场景管理器。
    /// 行为对齐 <see cref="Village_HomeScene1SceneManager"/>：室内脚步、KenMuNi 地点、正确 nowSceneName。
    /// </summary>
    /// <remarks>
    /// 原因：场景曾误挂 <c>ForestSceneManager</c>（nowSceneName/EnterPos 指向森林与龙宫），
    /// 从村门进屋会落点错误或黑屏。禁止复用 Forest GSM 将就酋长家。
    /// 替代方案：仅改户外门 NextSceneName——室内 EnterPos 仍不匹配，不采用。
    /// <para>
    /// 0901：进屋后自动播「继续对话」（C1+F1）；续聊结束 BlackPanel 内换「古莎待机」→「古莎动画合层」。
    /// 勿绑晚宴台本；合层古莎不替代玩家 Controllable。
    /// </para>
    /// <para>
    /// 0902：日常进屋改为 BlackPanel（推翻 0831 Loading）。续聊主路径在
    /// <see cref="TryDeferBlackFadeForCover"/> 全黑内 Trigger，防淡出后露景再出壳；
    /// <see cref="OnEnterScene"/> 仅兜底（blackFade=false / Defer 失败）。勿开回 Loading。
    /// </para>
    /// <para>
    /// 0902 续聊显隐：方案 A——场景涂层「雅儿战斗待机」站古莎旁；关玩家子级 SpriteRenderer（禁 HideEntity / 整根关 / 切 Combat）。
    /// 开场 S1 揭黑前 ApplyContinueTalkVisuals(true)；结束并入既有换古莎那一次黑幕；默认仍开古莎动画合层。
    /// </para>
    /// <para>
    /// 0902 开场分层（T1′）：Defer 揭黑须等续聊树 Instantiate 且白名单 alpha=0 备好，让玩家看见 0→1；
    /// 禁止幕下播完再揭；禁止广扫 Painting 名误伤 Mask。PrepareMask 续聊须 false（对齐门口空框）。
    /// </para>
    /// </remarks>
    public class Village_Chief_HouseSceneManager : BaseGameSceneManager
    {
        /// <summary>屋外门口三人戏（进屋前门闩「已用」键）。</summary>
        private const string DoorStoryName = "Village_村长家门口初次对话";

        /// <summary>进屋后自动续聊（与 CSV / Prefab / Generated 同名）。</summary>
        private const string ContinueStoryName = "Village_村长家继续对话";

        /// <summary>
        /// 黑幕换人成功后的存档旗（≠ 续聊已用键：续聊 OnStoryEnd 早于换人完成）。
        /// </summary>
        private const string GushaAnimStandbyFlag = "Village_ChiefHouse_GushaAnimStandby";

        private const string StandbyObjectName = "古莎待机";
        private const string AnimObjectName = "古莎动画合层";
        private const string AnimBackgroundChildName = "背景";

        /// <summary>续聊战斗形态贴纸（合层预置；Setup 菜单生成）。</summary>
        private const string YaerCombatStandbyObjectName = "雅儿战斗待机";

        /// <summary>
        /// 树 Instantiate 后极短 hold 再揭黑（对齐 NearDoor；勿加长到把 1s 淡入耗在幕下）。
        /// </summary>
        private const float ContinueShellReadyHoldSeconds = 0.05f;

        /// <summary>等树就绪轮询间隔。</summary>
        private const float ContinueTreeReadyPollSeconds = 0.05f;

        /// <summary>壳/树未起来仍淡出，防永久卡黑。</summary>
        private const float ContinueCoverTimeoutSeconds = 3f;

        /// <summary>续聊前奏立绘白名单（与 Prefab BB 精确同名；仅 DialogueSceneContainer 下）。</summary>
        private static readonly string[] ContinuePortraitObjectNames =
        {
            "GoOutStoryYaerPainting",
            "GushaPainting",
            "ChiefPainting",
        };

        [Header("续聊结束 · 古莎待机换动画合层（0901）")]
        [Tooltip("合层内静态「古莎待机」；可空，运行时按名解析")]
        [SerializeField]
        private GameObject gushaStandby;

        [Tooltip("预置的「古莎动画合层」实例（默认关）；可空，运行时按名解析")]
        [SerializeField]
        private GameObject gushaAnimComposite;

        [Header("续聊 · 雅儿战斗待机涂层（0902）")]
        [Tooltip("合层预置「雅儿战斗待机」；可空，运行时按名解析。勿用 UI GoOut 立绘冒充。")]
        [SerializeField]
        private GameObject yaerCombatStandby;

        [Header("进场落点诊断（0901 飞出排查）")]
        [Tooltip("打 [ChiefEnterPos]：lastScene / 选用锚点 / 与 DefaultBorn 距离；验收通过后可关")]
        [SerializeField]
        private bool enableEnterPosDebugLog = true;

        /// <summary>已订 onStoryEnd，等待续聊结束开黑换人。</summary>
        private bool awaitingContinueStoryEnd;

        /// <summary>换人黑幕进行中，防重入。</summary>
        private bool gushaSwapOrchestrating;

        /// <summary>防止 onStoryTriggered 与超时双触发 CloseFormFade。</summary>
        private bool chiefContinueCoverCloseIssued;

        /// <summary>LoadScene 旁路交来的 CloseFormFade + OnBlackFadeEnd。</summary>
        private Action deferredCloseBlackAndNotify;

        /// <summary>T1′：已开始轮询等树 Instantiated。</summary>
        private bool continueTreeReadyPolling;

        /// <summary>T1′：轮询累计秒数（相对超时）。</summary>
        private float continueTreeReadyElapsed;

        public override void OnInit()
        {
            base.OnInit();

            // 逻辑自报场景名：供 LastSceneName、EnterPosConfig；必须与场景文件名一致。
            nowSceneName = SceneName.Village_Chief_House;

            // 存档「当前地点」显示肯姆尼。
            GetModule<PlayerHandlerComponentGSM>().SetNowPlace(PlaceName.KenMuNi);
        }

        public override void OnEnterScene()
        {
            base.OnEnterScene();

            GetModule<PlayerHandlerComponentGSM>().SetNowPlace(PlaceName.KenMuNi);

            var lastScene = GameManager.GetGMComponent<ChangeSceneComponentGM>().LastSceneName;
            Debug.Log($"[VillageChiefHouseDebug] lastScene={lastScene} place={PlaceName.KenMuNi}");

            // 读档/再进：旗已立（或续聊已用但旗未立 Q7）→ 静默正确 Active，不再黑幕
            ApplyGushaVisualFromArchive();

            // Defer 主路径：续聊已在跑且 S1 已写——勿在此清回可视，否则揭黑后立刻露双雅儿反态
            var storyGsm = GetModule<StoryComponentGSM>();
            if (storyGsm != null && storyGsm.HasRunningStory)
            {
                Debug.Log("[ChiefContinue] OnEnterScene：续聊已在跑，跳过静默显隐与兜底 Trigger");
            }
            else
            {
                ApplyContinueTalkVisuals(inTalk: false);
                // 主路径：换场黑幕 DeferCover 内已 Trigger；此处仅兜底（blackFade=false / Defer 失败）。
                TryTriggerChiefContinueOnce();
            }
        }

        /// <summary>
        /// 0902 F1′：应播续聊时仍全黑 Trigger，壳就绪后再 CloseFormFade。
        /// 原因：默认契约是淡出后才 OnEnterScene，改黑幕后若仍等 OnEnter 再 Trigger 会露室内再出对话框。
        /// 替代方案：接受短闪（F1″）或开回 Loading（产品否）。
        /// </summary>
        public override bool TryDeferBlackFadeForCover(Action closeBlackAndNotify)
        {
            if (!ShouldPlayChiefContinue())
            {
                return false;
            }

            var storyGsm = GetModule<StoryComponentGSM>();
            if (storyGsm == null)
            {
                Debug.LogWarning("[ChiefContinue] StoryComponentGSM 缺失，放弃延迟淡出");
                return false;
            }

            if (storyGsm.HasRunningStory)
            {
                Debug.LogWarning("[ChiefContinue] 已有剧情在跑，放弃延迟淡出");
                return false;
            }

            chiefContinueCoverCloseIssued = false;
            continueTreeReadyPolling = false;
            continueTreeReadyElapsed = 0f;
            deferredCloseBlackAndNotify = closeBlackAndNotify;
            storyGsm.onStoryTriggered += OnChiefContinueStoryTriggeredForCover;

            bool started = storyGsm.TriggerStory(ContinueStoryName);
            if (!started)
            {
                storyGsm.onStoryTriggered -= OnChiefContinueStoryTriggeredForCover;
                deferredCloseBlackAndNotify = null;
                Debug.LogWarning("[ChiefContinue] TriggerStory 未启动，回退默认淡出");
                return false;
            }

            // 续聊结束换人：须在 Defer 成功时就订约（勿等 OnEnterScene 再订）
            SubscribeContinueStoryEnd(storyGsm);

            Debug.Log("[ChiefContinue] 黑幕阶段 TriggerStory " + ContinueStoryName + "，等待树 Instantiate+Prepare 后再淡出");
            WaitForInvoke(ContinueCoverTimeoutSeconds, OnChiefContinueCoverTimeout);
            return true;
        }

        private void OnChiefContinueStoryTriggeredForCover()
        {
            var storyGsm = GetModule<StoryComponentGSM>();
            if (storyGsm != null)
            {
                storyGsm.onStoryTriggered -= OnChiefContinueStoryTriggeredForCover;
            }

            // T1′：onStoryTriggered 早于 StartDialogue→Yield→Instantiate；勿固定 0.1s 就揭黑（空房硬切）。
            // 轮询到树就绪 → Prepare alpha=0 → 再 Finalize CloseFormFade，让玩家看见图内 0→1。
            continueTreeReadyPolling = true;
            continueTreeReadyElapsed = 0f;
            PollContinueTreeReadyThenFinalize();
        }

        /// <summary>
        /// 等 DialogueSceneContainer 下出现 DialogueTreeController，再 Prepare 白名单 alpha=0 后揭黑。
        /// </summary>
        private void PollContinueTreeReadyThenFinalize()
        {
            if (chiefContinueCoverCloseIssued || !continueTreeReadyPolling)
            {
                return;
            }

            if (TryPrepareContinueLayeredReveal())
            {
                continueTreeReadyPolling = false;
                WaitForInvoke(ContinueShellReadyHoldSeconds, FinalizeChiefContinueCoverAndCloseBlack);
                return;
            }

            continueTreeReadyElapsed += ContinueTreeReadyPollSeconds;
            if (continueTreeReadyElapsed >= ContinueCoverTimeoutSeconds)
            {
                Debug.LogWarning("[ChiefContinue] 等树就绪超时，强制 Prepare（可能仍空）后淡出");
                continueTreeReadyPolling = false;
                TryPrepareContinueLayeredReveal();
                FinalizeChiefContinueCoverAndCloseBlack();
                return;
            }

            WaitForInvoke(ContinueTreeReadyPollSeconds, PollContinueTreeReadyThenFinalize);
        }

        private void OnChiefContinueCoverTimeout()
        {
            if (chiefContinueCoverCloseIssued)
            {
                return;
            }

            Debug.LogWarning("[ChiefContinue] 壳就绪总超时，强制淡出黑幕");
            continueTreeReadyPolling = false;
            TryPrepareContinueLayeredReveal();
            FinalizeChiefContinueCoverAndCloseBlack();
        }

        /// <summary>
        /// 全黑内分层准备（白名单）：字幕条 + DialogueScene 下三立绘 CanvasGroup alpha=0。
        /// <para>
        /// 对齐 KenMuNi Prepare：禁止广扫整棵 Panel / 名字 Contains，以免误伤 Bottom/Mask 小头像。
        /// 返回 true = 已找到 Instantiated 对话树并写完 alpha（可揭黑）。
        /// </para>
        /// </summary>
        private bool TryPrepareContinueLayeredReveal()
        {
            var uiPath = UIPrefabPath.GetUIPrefabPath("NormalDialogueNewPanel");
            var uiForm = GameManager.GetGMComponent<UIComponentGM>().GetUIForm(uiPath);
            if (uiForm == null || !(uiForm.Logic is NormalDialogueFormNewLogic dialogueLogic))
            {
                return false;
            }

            var sceneRoot = UIUtils.findChild(dialogueLogic.gameObject, "DialogueSceneContainer", hasDebugLog: false);
            if (sceneRoot == null)
            {
                return false;
            }

            // Instantiated 标志：容器下有 DialogueTreeController（StartDialogue Yield 之后）
            var tree = sceneRoot.GetComponentInChildren<DialogueTreeController>(true);
            if (tree == null)
            {
                return false;
            }

            // 白名单 1：字幕条（UIAlpha 任务会再从 StartAlpha 淡入）
            if (dialogueLogic.dialogueUICanvasGroup != null)
            {
                dialogueLogic.dialogueUICanvasGroup.alpha = 0f;
            }

            // 白名单 2：场景大立绘（精确名；勿扫 Mask）
            for (var i = 0; i < ContinuePortraitObjectNames.Length; i++)
            {
                SetScenePaintingCanvasGroupAlpha(sceneRoot, ContinuePortraitObjectNames[i], 0f);
            }

            Debug.Log("[ChiefContinue][Prepare] 树已 Instantiated，白名单 alpha=0，可揭黑看淡入");
            return true;
        }

        /// <summary>仅在 DialogueSceneContainer 下按精确子物体名改 CanvasGroup；找不到则静默跳过。</summary>
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
        }

        private void FinalizeChiefContinueCoverAndCloseBlack()
        {
            if (chiefContinueCoverCloseIssued)
            {
                return;
            }

            chiefContinueCoverCloseIssued = true;
            continueTreeReadyPolling = false;

            var storyGsm = GetModule<StoryComponentGSM>();
            if (storyGsm != null)
            {
                storyGsm.onStoryTriggered -= OnChiefContinueStoryTriggeredForCover;
            }

            var close = deferredCloseBlackAndNotify;
            deferredCloseBlackAndNotify = null;

            // S1：揭黑前藏室内 Home 皮 + 亮战斗待机（双雅儿风险：必须先关玩家 SR）
            ApplyContinueTalkVisuals(inTalk: true);

            if (close != null)
            {
                Debug.Log("[ChiefContinue] Prepare+S1 完成，CloseFormFade（玩家可见立绘/框 0→1）");
                close.Invoke();
            }
        }

        public override void OnShutDown()
        {
            // 场景销毁时清掉未完成的 defer 订阅，避免回调打到已毁 GSM
            var storyGsm = GetModule<StoryComponentGSM>();
            if (storyGsm != null)
            {
                storyGsm.onStoryTriggered -= OnChiefContinueStoryTriggeredForCover;
            }

            deferredCloseBlackAndNotify = null;
            continueTreeReadyPolling = false;
            UnsubscribeContinueStoryEnd();
            base.OnShutDown();
        }

        /// <summary>
        /// 从村进屋走 EnterPos→EnterFrom_Village（不用 DefaultBorn）；落点须在 VillageWalkArea 内，否则 Town ClosestPoint 会一帧吸入形内（0901 飞出 H1）。
        /// </summary>
        protected override void SetPlayerPos(PlayerLogic playerLogic)
        {
            var last = GameManager.GetGMComponent<ChangeSceneComponentGM>()?.LastSceneName;
            Transform chosen = null;
            string chosenKind = "DefaultBorn";

            if (EnterPosConfig != null)
            {
                for (int i = 0; i < EnterPosConfig.Count; i++)
                {
                    var ep = EnterPosConfig[i];
                    if (ep != null && ep.lastScene == last && ep.pos != null)
                    {
                        chosen = ep.pos;
                        chosenKind = "EnterPos:" + ep.lastScene;
                        break;
                    }
                }
            }

            if (chosen == null)
            {
                var map = GetModule<MapControlComponentGSM>();
                chosen = map != null ? map.DefaultBornTsf : null;
            }

            base.SetPlayerPos(playerLogic);

            // F3 双保险：再 Flush 一次（SetPos 村模式已 Teleport+Flush；此处确保换场淡出前 Rb≡Transform）
            var town = playerLogic.componentSystem != null
                ? playerLogic.componentSystem.TryGetComponent<TownPlayerLocomotion>()
                : null;
            if (town != null && town.enabled)
            {
                town.FlushAuthoritativeVillageTransformAfterSceneDepthInject();
            }

            if (!enableEnterPosDebugLog || playerLogic == null)
            {
                return;
            }

            var born = GetModule<MapControlComponentGSM>()?.DefaultBornTsf;
            Vector3 foot = playerLogic.transform.position;
            var rb = playerLogic.GetComponent<Rigidbody2D>();
            Vector2 rbPos = rb != null ? rb.position : new Vector2(foot.x, foot.y);
            float rbMismatch = Vector2.Distance(new Vector2(foot.x, foot.y), rbPos);
            Vector3 target = chosen != null ? chosen.position : foot;
            float distBorn = born != null
                ? Vector2.Distance(new Vector2(foot.x, foot.y), new Vector2(born.position.x, born.position.y))
                : -1f;
            float distTarget = Vector2.Distance(
                new Vector2(foot.x, foot.y),
                new Vector2(target.x, target.y));

            // WalkArea Overlap：验收「形内」；无 poly 时跳过
            bool? overlap = null;
            var walk = GameObject.Find("VillageWalkArea");
            var poly = walk != null ? walk.GetComponent<PolygonCollider2D>() : null;
            if (poly != null)
            {
                overlap = poly.OverlapPoint(new Vector2(foot.x, foot.y));
            }

            Debug.Log(
                "[ChiefEnterPos] lastScene=" + last
                + " kind=" + chosenKind
                + " target=" + (chosen != null ? chosen.name + "@" + target : "null")
                + " foot=" + foot
                + " rb=" + rbPos
                + " rbMismatch=" + rbMismatch.ToString("F4")
                + " distToTarget=" + distTarget.ToString("F3")
                + " distToDefaultBorn=" + distBorn.ToString("F3")
                + " OverlapWalkArea=" + (overlap.HasValue ? overlap.Value.ToString() : "n/a"));
        }

        /// <summary>
        /// 门闩 F1：门口戏已记档 ∧ 续聊尚未记档 → 可播。
        /// 无存档数据时不播（避免裸进房误播；续聊依赖「门口已用」）。
        /// </summary>
        private bool ShouldPlayChiefContinue()
        {
            var counts = GetArchiveData<StoryTriggerCountData>();
            if (counts == null)
            {
                return false;
            }

            return counts.CheckStoryUsed(DoorStoryName) && !counts.CheckStoryUsed(ContinueStoryName);
        }

        /// <summary>
        /// 同档首次满足 F1 时自动 Trigger 续聊；成功则订 onStoryEnd → 黑幕换人。
        /// 主路径已在 <see cref="TryDeferBlackFadeForCover"/>；此处兜底。
        /// </summary>
        private void TryTriggerChiefContinueOnce()
        {
            if (!ShouldPlayChiefContinue())
            {
                return;
            }

            var storyGsm = GetModule<StoryComponentGSM>();
            if (storyGsm == null)
            {
                Debug.LogWarning("[ChiefContinue] StoryComponentGSM 缺失，跳过 " + ContinueStoryName);
                return;
            }

            if (storyGsm.HasRunningStory)
            {
                // 黑幕阶段已启动：静默跳过，避免双开告警刷屏
                return;
            }

            bool started = storyGsm.TriggerStory(ContinueStoryName);
            Debug.Log(started
                ? "[ChiefContinue] OnEnterScene 兜底 TriggerStory " + ContinueStoryName
                : "[ChiefContinue] TriggerStory 未启动（Prefab 可能缺失）" + ContinueStoryName);

            if (started)
            {
                // 兜底路径可能已无换场黑幕：仍尽量写 S1；可能闪一帧室内主角
                Debug.LogWarning("[ChiefContinue] 兜底路径 ApplyContinueTalkVisuals（可能已无遮罩）");
                ApplyContinueTalkVisuals(inTalk: true);
                // onStoryEnd 触发时 CurrentRunningStoryName 已清空，故用本旗位确认是续聊结束
                SubscribeContinueStoryEnd(storyGsm);
            }
        }

        private void SubscribeContinueStoryEnd(StoryComponentGSM storyGsm)
        {
            if (awaitingContinueStoryEnd)
            {
                return;
            }

            awaitingContinueStoryEnd = true;
            storyGsm.onStoryEnd += OnChiefContinueStoryEnd;
        }

        private void UnsubscribeContinueStoryEnd()
        {
            if (!awaitingContinueStoryEnd)
            {
                return;
            }

            var storyGsm = GetModule<StoryComponentGSM>();
            if (storyGsm != null)
            {
                storyGsm.onStoryEnd -= OnChiefContinueStoryEnd;
            }

            awaitingContinueStoryEnd = false;
        }

        /// <summary>
        /// 续聊结束 → 系统 BlackPanel 全黑内关待机、开动画合层 → 淡出。
        /// 仅本 GSM 订约后的第一次 onStoryEnd；晚宴等其它对白未订约则不进。
        /// </summary>
        private void OnChiefContinueStoryEnd()
        {
            UnsubscribeContinueStoryEnd();

            if (gushaSwapOrchestrating)
            {
                return;
            }

            // 已换过则静默（防异常双订）；正常路径进房已 Apply
            if (IsGushaAnimStandbyFlagSet())
            {
                ApplyGushaVisual(showAnim: true);
                return;
            }

            gushaSwapOrchestrating = true;
            OpenSystemBlackFade(OnBlackFullyShownForGushaSwap);
        }

        private void OpenSystemBlackFade(Action<BlackFormLogic> onBlackReady)
        {
            // 对齐 ChiefNearDoorStoryTrigger：系统层 FadeShow，换人只在 onShowEnd 全黑内做
            var uiPath = UIPrefabPath.GetUIPrefabPath("BlackPanel");
            GameManager.GetGMComponent<UIComponentGM>().OpenUIForm(uiPath, EUIGroup.System, new OpenFormArgs
            {
                userData = new ShowBlackFormArgs
                {
                    showType = BlackFadeType.FadeShow,
                    onShowEnd = onBlackReady
                }
            });
        }

        private void OnBlackFullyShownForGushaSwap(BlackFormLogic blackForm)
        {
            try
            {
                // 0901：关古莎待机 + 开动画合层（默认仍开，Q1）
                ApplyGushaVisual(showAnim: true);
                // 0902：同一次黑幕关战斗待机 + 恢复室内主角（禁止再 Open 第二次 BlackPanel）
                ApplyContinueTalkVisuals(inTalk: false);
                MarkGushaAnimStandbyFlag();
                Debug.Log("[ChiefGushaSwap] 全黑内已切换：双待机关 → 动画合层开 → 主角恢复，并记档旗。");
            }
            catch (Exception ex)
            {
                Debug.LogError("[ChiefGushaSwap] 换人异常：" + ex.Message);
            }

            if (blackForm == null)
            {
                gushaSwapOrchestrating = false;
                return;
            }

            blackForm.CloseFormFade(() => { gushaSwapOrchestrating = false; });
        }

        /// <summary>
        /// 续聊进行中 / 结束或进房静默的显隐。
        /// <para>
        /// inTalk=true：藏玩家皮 + 亮「雅儿战斗待机」（古莎待机保持由 ApplyGushaVisual 管）。
        /// inTalk=false：关战斗待机 + 恢复玩家可视。
        /// </para>
        /// 方案 A 贴纸；禁止 B 切 Combat / HideEntity / 整根 SetActive(false)。
        /// </summary>
        private void ApplyContinueTalkVisuals(bool inTalk)
        {
            ResolveYaerCombatStandbyIfNeeded();

            // 先关玩家皮再亮贴纸，避免双雅儿一帧
            SetPlayerVisualVisible(!inTalk);

            if (yaerCombatStandby != null)
            {
                yaerCombatStandby.SetActive(inTalk);
            }
            else if (inTalk)
            {
                Debug.LogWarning(
                    "[ChiefContinue] 未找到「"
                    + YaerCombatStandbyObjectName
                    + "」。请跑 Tools/Scene/Setup Chief House 雅儿战斗待机预置。");
            }
        }

        /// <summary>
        /// 只开关子级 SpriteRenderer.enabled，保留根 Active / 碰撞 / 落点 / 输入订阅。
        /// 禁止整根 SetActive(false) 与 Entity HideEntity。
        /// </summary>
        private void SetPlayerVisualVisible(bool visible)
        {
            var player = GameManager.GetGMComponent<EntityComponentGM>()?.GetEntityLogic<PlayerLogic>();
            if (player == null)
            {
                Debug.LogWarning("[ChiefContinue] SetPlayerVisualVisible：无 PlayerLogic。visible=" + visible);
                return;
            }

            var renderers = player.GetComponentsInChildren<SpriteRenderer>(true);
            for (var i = 0; i < renderers.Length; i++)
            {
                renderers[i].enabled = visible;
            }
        }

        private void ResolveYaerCombatStandbyIfNeeded()
        {
            if (yaerCombatStandby == null)
            {
                yaerCombatStandby = FindDeepInactive(YaerCombatStandbyObjectName);
            }
        }

        /// <summary>
        /// 按存档决定待机/动画 Active。
        /// 旗已立 → 动画；续聊已用但旗未立（中断）→ 静默动画（Q7）；否则待机。
        /// </summary>
        private void ApplyGushaVisualFromArchive()
        {
            var counts = GetArchiveData<StoryTriggerCountData>();
            bool flag = counts != null && counts.CheckStoryUsed(GushaAnimStandbyFlag);
            bool continueDone = counts != null && counts.CheckStoryUsed(ContinueStoryName);
            // Q7：续聊已用但换人旗未立 → 静默 Apply，少打扰
            bool showAnim = flag || continueDone;
            ApplyGushaVisual(showAnim);
        }

        private void ApplyGushaVisual(bool showAnim)
        {
            ResolveGushaRefsIfNeeded();

            if (gushaStandby != null)
            {
                gushaStandby.SetActive(!showAnim);
            }
            else
            {
                Debug.LogWarning("[ChiefGushaSwap] 未找到「古莎待机」。");
            }

            if (gushaAnimComposite != null)
            {
                EnsureAnimBackgroundDisabled(gushaAnimComposite);
                gushaAnimComposite.SetActive(showAnim);
            }
            else
            {
                Debug.LogWarning(
                    "[ChiefGushaSwap] 未找到「古莎动画合层」。请跑 Tools/Scene/Setup Chief House 古莎动画合层预置。");
            }
        }

        private void EnsureAnimBackgroundDisabled(GameObject animRoot)
        {
            // 室内已有房间底；动画 Prefab「背景」盖景 → 实例上保持关
            var t = animRoot.transform.Find(AnimBackgroundChildName);
            if (t != null && t.gameObject.activeSelf)
            {
                t.gameObject.SetActive(false);
            }
        }

        private bool IsGushaAnimStandbyFlagSet()
        {
            var counts = GetArchiveData<StoryTriggerCountData>();
            return counts != null && counts.CheckStoryUsed(GushaAnimStandbyFlag);
        }

        private void MarkGushaAnimStandbyFlag()
        {
            var counts = GetArchiveData<StoryTriggerCountData>();
            if (counts == null)
            {
                Debug.LogWarning("[ChiefGushaSwap] StoryTriggerCountData 为空，无法记换人旗。");
                return;
            }

            if (!counts.CheckStoryUsed(GushaAnimStandbyFlag))
            {
                counts.OnStoryTriggered(GushaAnimStandbyFlag);
            }
        }

        /// <summary>SerializeField 优先；否则在场景内按名深搜（含未激活）。</summary>
        private void ResolveGushaRefsIfNeeded()
        {
            if (gushaStandby == null)
            {
                gushaStandby = FindDeepInactive(StandbyObjectName);
            }

            if (gushaAnimComposite == null)
            {
                gushaAnimComposite = FindDeepInactive(AnimObjectName);
            }

            // 短诊断：缺实例时路径指纹便于对上 H2（场景无「古莎动画合层」）
            if (gushaAnimComposite == null)
            {
                Debug.LogWarning(
                    "[ChiefGushaSwap] Resolve 后动画合层仍为 null（场景 Design/村长家合层 须有预置实例）。"
                    + " standby="
                    + (gushaStandby != null ? GetHierarchyPath(gushaStandby.transform) : "null"));
            }
        }

        private static string GetHierarchyPath(Transform t)
        {
            if (t == null)
            {
                return "null";
            }

            var path = t.name;
            while (t.parent != null)
            {
                t = t.parent;
                path = t.name + "/" + path;
            }

            return path;
        }

        private static GameObject FindDeepInactive(string objectName)
        {
            // Resources.FindObjectsOfTypeAll 含未激活与 Prefab 资产；过滤掉非场景实例
            var all = Resources.FindObjectsOfTypeAll<Transform>();
            for (var i = 0; i < all.Length; i++)
            {
                var t = all[i];
                if (t == null || t.name != objectName)
                {
                    continue;
                }

                if (t.gameObject.scene.IsValid() && t.gameObject.scene.isLoaded)
                {
                    return t.gameObject;
                }
            }

            return null;
        }

        /// <summary>室内脚步资源：室内走{0}.mp3</summary>
        public override TerrainType GetCurSceneTerrainType() => TerrainType.IndoorType;

        public override void initAllSceneMonster() { }
    }
}
