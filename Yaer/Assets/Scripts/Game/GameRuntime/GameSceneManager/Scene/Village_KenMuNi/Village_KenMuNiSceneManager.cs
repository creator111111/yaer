using System;
using Game.GameMgr;
using Game.GameMgr.Component;
using Game.GameMgr.Component.Archive.ArchiveDataClass;
using Game.GameMgr.Component.Archive.ArchiveDataClass.Scene;
using Game.GameMgr.Component.ChangeScene;
using Game.GameMgr.Component.UI;
using Game.GameRuntime.Entities.Base.BaseSceneObj;
using Game.GameRuntime.Entities.MainNPC;
using Game.GameRuntime.Entities.Player;
using Game.GameRuntime.Entities.Player.Components;
using Game.GameRuntime.GameSceneManager.Base;
using Game.GameRuntime.GameSceneManager.Component;
using Game.GameRuntime.GameSceneManager.Component.CameraGSM;
using Game.GameRuntime.GameSceneManager.Component.Story;
using Game.GameRuntime.UI.FormLogic;
using Game.GameRuntime.UI.FormLogic.Story.Dialogue;
using Game.Static.Enum.Map;
using Game.Static.Name.Res;
using Game.Static.Path;
using Game.Static.Path.Sound;
using GameFramework.UnityRuntime.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.GameRuntime.GameSceneManager.Scene.Village_KenMuNi
{
    /// <summary>
    /// <c>Village_KenMuNi1</c> 专用场景管理器：业务与 <see cref="Game.GameRuntime.GameSceneManager.Scene.Forest.ForestSceneManager"/> 对齐（同套 <see cref="ForestSceneData"/>、
    /// 莱/林恩实体、BGM/SFX、战斗面板门控等），避免进村后剧情与音效逻辑回退。
    /// <para>
    /// 与森林管理器的<strong>必须差异</strong>：<see cref="SceneName.Village_KenMuNi1"/> 作为当前场景名；
    /// <see cref="PlaceName.KenMuNi"/> 作为存档「当前地点」内部键，使读档 UI 显示「肯尼姆」三语字典项（不动存档管线代码）。
    /// </para>
    /// <para>
    /// <strong>替代方案</strong>：若将来要以 Unity 场景名字符串作地点键，需与 <see cref="SceneName"/> 职责分离，避免表维护混淆；当前采用独立常量 <see cref="PlaceName.KenMuNi"/>。
    /// </para>
    /// </summary>
    public class Village_KenMuNiSceneManager : BaseGameSceneManager
    {
        /// <summary>本村复用森林场景存档结构（门口剧情、莱/林恩显隐等）。</summary>
        private ForestSceneData sceneData;

        SoundToggleComponent bgmSoundCpn;
        /// <summary>随机风吹树叶类环境音。</summary>
        SoundToggleComponent soundSfxCpn_2;
        /// <summary>随机鸟叫环境音。</summary>
        SoundToggleComponent soundSfxCpn_3;
        float timeCount_2 = 0;
        float timeCount_3 = 0;
        /// <summary>风吹音效最小间隔基数，与随机区间配合（与森林逻辑一致）。</summary>
        float timeDistance_2 = 10;
        /// <summary>鸟叫音效间隔基数（与森林逻辑一致）。</summary>
        float timeDistance_3 = 20;

        public override void OnInit()
        {
            base.OnInit();
            timeCount_2 = timeDistance_2 - 1;
            timeCount_3 = timeDistance_3 - 1;

            // 逻辑自报场景名：供全局查询、切场景等；不可再写 ForestScene，否则与真实场景不一致。
            nowSceneName = SceneName.Village_KenMuNi1;

            // 写入 PlayerMapData，存档标题链走 PlaceName 字典（见执行说明 §2.3）。
            GetModule<PlayerHandlerComponentGSM>().SetNowPlace(PlaceName.KenMuNi);

            sceneData = GetArchiveData<ForestSceneData>();

            // 村庄场景未必摆放森林同款 NPC（莱/王/林恩）；缺失时跳过显隐，避免 Awake 中断导致 SceneManager 被禁用。
            // 替代方案：从 ForestScene 复制对应实体到 objRoot 并刷新 sceneObjs，则可恢复与森林一致的剧情显隐。
            TrySetSceneEntityActive<ForestSceneKingLogic>(false);
            TrySetSceneEntityActive<ForestSceneLaiLogic>(!sceneData.homeDoorStoryComplete);
            TrySetSceneEntityActive<ForestSceneLinEnLogic>(false);

            var bgmNode = UIUtils.findChild(gameObject, "BGM");
            bgmSoundCpn = bgmNode.GetComponent<SoundToggleComponent>();
            bgmSoundCpn.gameObject.SetActive(sceneData.homeDoorStoryComplete);
            var sfxNode_2 = UIUtils.findChild(gameObject, "SFX_2");
            soundSfxCpn_2 = sfxNode_2.GetComponent<SoundToggleComponent>();
            var sfxNode_3 = UIUtils.findChild(gameObject, "SFX_3");
            soundSfxCpn_3 = sfxNode_3.GetComponent<SoundToggleComponent>();
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            timeCount_2 += Time.deltaTime;
            if (timeCount_2 > timeDistance_2)
            {
                timeCount_2 = 0;
                timeDistance_2 = GameTools.getRandomIntNum(10, 15);
                PlayWindAudio();
            }
            timeCount_3 += Time.deltaTime;
            if (timeCount_3 > timeDistance_3)
            {
                timeCount_3 = 0;
                PlayBirdAudio();
            }
        }

        /// <summary>开场对白 Prefab 名；与 Dialogue 路径 / StoryTriggerCount 键一致。</summary>
        const string VillageStartStoryName = "Village_KenMuNiStart";

        /// <summary>
        /// 1 楼出门 → 门前 → 古雅送树屋（与 CSV / Prefab / StoryTriggerCount 同名）。
        /// G1 只认 <see cref="SceneName.Village_Chief_House_Door"/>，禁止裸判 Village_Chief_House（防 2 楼误播）。
        /// </summary>
        const string LeaveChiefEscortStoryName = "Village_出村长家送树屋";

        /// <summary>
        /// 壳 Open + Prefab 实例化（含全屏 BG）所需极短 hold。
        /// 分层节奏（框→立绘各≈1s）交给 Prefab 亮屏后播放，不再等满前奏。
        /// <para>替代方案：若偶发 BG 未就绪就淡出，可略增本值或在 Finalize 内再补一帧 hold。</para>
        /// </summary>
        const float VillageStartBgReadyHoldSeconds = 0.15f;

        /// <summary>壳/Prefab 失败时防永久卡黑的超时（秒）。</summary>
        const float VillageStartCoverTimeoutSeconds = 3f;

        /// <summary>防止 onStoryTriggered 与超时双触发 CloseFormFade。</summary>
        bool villageStartCoverCloseIssued;

        /// <summary>LoadScene 旁路交来的 CloseFormFade + OnBlackFadeEnd。</summary>
        Action deferredCloseBlackAndNotify;

        public override void OnEnterScene()
        {
            base.OnEnterScene();

            // 场景加载完成后再写一次当前地点：避免与切场景顺序相关的逻辑在 Awake/OnInit 之后又改回其它地图键；
            // 存档标题读取的是 PlayerMapData.GetNowPlace()，此处与 OnInit 双写保证验收稳定。
            GetModule<PlayerHandlerComponentGSM>().SetNowPlace(PlaceName.KenMuNi);

            if (sceneData.homeDoorStoryComplete == false)
            {
                GetModule<CameraComponentGSM>().CancelFollow();
                GetModule<CameraComponentGSM>().SetLock(true);
            }

            // 正常进村：开场已在黑幕阶段 Trigger（TryDeferBlackFadeForCover）。
            // 此处仅兜底（例如 blackFade=false）；HasRunningStory / CheckStoryUsed 防双开。
            TryTriggerVillageStartStoryOnce();

            // G1：1 楼 LeftDoor（enterPosKey=Village_Chief_House_Door）→ 门前自动送树屋戏。
            // 须在开场兜底之后；开场已用 / 无 Running 时才可能启动。楼梯 2 楼键不进此分支。
            TryTriggerLeaveChiefEscortOnce();
        }

        /// <summary>
        /// 同档首次：从村长家 1 楼门回村落门前时 Trigger「出村长家送树屋」。
        /// </summary>
        void TryTriggerLeaveChiefEscortOnce()
        {
            if (!ShouldPlayLeaveChiefEscort())
            {
                return;
            }

            var storyGsm = GetModule<StoryComponentGSM>();
            if (storyGsm == null)
            {
                Debug.LogWarning("[LeaveChiefEscort] StoryComponentGSM 缺失，跳过 " + LeaveChiefEscortStoryName);
                return;
            }

            if (storyGsm.HasRunningStory)
            {
                // 开场等其它戏占场：本档下次再进门前也不会重试（仅 OnEnter）；产品路径上开场应已用完
                Debug.Log("[LeaveChiefEscort] 已有剧情在跑，跳过本次 Trigger");
                return;
            }

            bool started = storyGsm.TriggerStory(LeaveChiefEscortStoryName);
            Debug.Log(started
                ? "[LeaveChiefEscort] OnEnterScene TriggerStory " + LeaveChiefEscortStoryName
                : "[LeaveChiefEscort] TriggerStory 未启动 " + LeaveChiefEscortStoryName);
        }

        /// <summary>
        /// 门闩：LastScene 必须是大门键（E3′），且本戏未用。
        /// 禁止 <c>last == Village_Chief_House</c>（楼梯 2 楼会误播）。
        /// </summary>
        bool ShouldPlayLeaveChiefEscort()
        {
            var last = GameManager.GetGMComponent<ChangeSceneComponentGM>()?.LastSceneName;
            if (last != SceneName.Village_Chief_House_Door)
            {
                return false;
            }

            var counts = GetArchiveData<StoryTriggerCountData>();
            return counts == null || !counts.CheckStoryUsed(LeaveChiefEscortStoryName);
        }

        /// <summary>
        /// 楼梯从村长家上楼：落 2 楼后把生效 WalkArea 切到 <c>VillageWalkArea2</c>（W1）。
        /// 1 楼大门键 <see cref="SceneName.Village_Chief_House_Door"/> 不切，避免套错多边形。
        /// <para>禁止改 WalkArea2 点集/尺寸。</para>
        /// </summary>
        protected override void SetPlayerPos(PlayerLogic playerLogic)
        {
            base.SetPlayerPos(playerLogic);
            TryBindVillageWalkArea2AfterChiefStairsLanding(playerLogic);
        }

        private void TryBindVillageWalkArea2AfterChiefStairsLanding(PlayerLogic playerLogic)
        {
            if (playerLogic == null)
            {
                return;
            }

            var last = GameManager.GetGMComponent<ChangeSceneComponentGM>()?.LastSceneName;
            // 仅楼梯路径：真实场景名 Village_Chief_House（EnterPos→2f）；大门键不绑 2
            if (last != SceneName.Village_Chief_House)
            {
                return;
            }

            var town = playerLogic.componentSystem != null
                ? playerLogic.componentSystem.TryGetComponent<TownPlayerLocomotion>()
                : null;
            if (town == null)
            {
                Debug.LogWarning("[Village2f] 无 TownPlayerLocomotion，无法绑 VillageWalkArea2");
                return;
            }

            UnityEngine.SceneManagement.Scene scene = SceneManager.GetActiveScene();
            Transform named = FindNamedTransformInScene(scene, "VillageWalkArea2");
            if (named == null)
            {
                Debug.LogError("[Village2f] 未找到 VillageWalkArea2（禁止新建/改形状替代）");
                return;
            }

            var poly = named.GetComponent<PolygonCollider2D>();
            if (poly == null)
            {
                poly = named.GetComponentInChildren<PolygonCollider2D>(true);
            }

            if (poly == null)
            {
                Debug.LogError("[Village2f] VillageWalkArea2 无 PolygonCollider2D");
                return;
            }

            town.SetVillageWalkAreaOverride(poly);
            town.FlushAuthoritativeVillageTransformAfterSceneDepthInject();
            Debug.Log("[Village2f] 已 SetVillageWalkAreaOverride(VillageWalkArea2)，落点后不应用 1 楼 WalkArea");
        }

        /// <summary>仅在指定 Scene 根下按名查找（与 PlayerLogic / Town 同源策略）。</summary>
        /// <remarks>
        /// 须写全名：本文件命名空间含 <c>...GameSceneManager.Scene...</c>，裸写 <c>Scene</c> 会被当成命名空间（CS0118）。
        /// </remarks>
        private static Transform FindNamedTransformInScene(
            UnityEngine.SceneManagement.Scene scene, string objectName)
        {
            if (!scene.IsValid() || string.IsNullOrEmpty(objectName))
            {
                return null;
            }

            foreach (GameObject root in scene.GetRootGameObjects())
            {
                Transform found = FindNamedTransformRecursive(root.transform, objectName);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private static Transform FindNamedTransformRecursive(Transform tr, string objectName)
        {
            if (tr.name == objectName)
            {
                return tr;
            }

            for (int i = 0; i < tr.childCount; i++)
            {
                Transform child = FindNamedTransformRecursive(tr.GetChild(i), objectName);
                if (child != null)
                {
                    return child;
                }
            }

            return null;
        }

        /// <summary>
        /// 分层显现（方案 A）：仍全黑时 Trigger → BG 盖满且框/立绘为 0 → 立刻 CloseFormFade；
        /// 三拍（仅 BG → 框 → 立绘）在亮屏下由 Prefab 播放。仅本档未播过 Start 时接管。
        /// </summary>
        public override bool TryDeferBlackFadeForCover(Action closeBlackAndNotify)
        {
            if (!ShouldPlayVillageStartStory())
            {
                return false;
            }

            var storyGsm = GetModule<StoryComponentGSM>();
            if (storyGsm == null)
            {
                Debug.LogWarning("[VillageStart] StoryComponentGSM 缺失，放弃延迟淡出");
                return false;
            }

            if (storyGsm.HasRunningStory)
            {
                Debug.LogWarning("[VillageStart] 已有剧情在跑，放弃延迟淡出");
                return false;
            }

            villageStartCoverCloseIssued = false;
            deferredCloseBlackAndNotify = closeBlackAndNotify;
            storyGsm.onStoryTriggered += OnVillageStartStoryTriggeredForCover;

            // 锁闸：树里 Wait 须等黑幕淡完才开始「只见 BG」空拍
            VillageStartLayerRevealGate.ResetForDeferredCover();

            bool started = storyGsm.TriggerStory(VillageStartStoryName);
            if (!started)
            {
                storyGsm.onStoryTriggered -= OnVillageStartStoryTriggeredForCover;
                deferredCloseBlackAndNotify = null;
                VillageStartLayerRevealGate.SignalBgFullyVisible();
                Debug.LogWarning("[VillageStart] TriggerStory 未启动，回退默认淡出");
                return false;
            }

            Debug.Log("[VillageStart] 黑幕阶段 TriggerStory " + VillageStartStoryName + "，等待 BG 盖满后分层亮屏");
            // 超时兜底：壳未起来也不要永久卡黑（超时回调仍走 Prepare，避免裸村）
            WaitForInvoke(VillageStartCoverTimeoutSeconds, OnVillageStartCoverTimeout);
            return true;
        }

        bool ShouldPlayVillageStartStory()
        {
            var counts = GetArchiveData<StoryTriggerCountData>();
            return counts == null || !counts.CheckStoryUsed(VillageStartStoryName);
        }

        /// <summary>
        /// 同档首次进入本村时 Trigger 开场（OnEnterScene 兜底）。
        /// 主路径已改到 <see cref="TryDeferBlackFadeForCover"/>。
        /// </summary>
        void TryTriggerVillageStartStoryOnce()
        {
            if (!ShouldPlayVillageStartStory())
            {
                return;
            }

            var storyGsm = GetModule<StoryComponentGSM>();
            if (storyGsm == null)
            {
                Debug.LogWarning("[VillageStart] StoryComponentGSM 缺失，跳过 " + VillageStartStoryName);
                return;
            }

            if (storyGsm.HasRunningStory)
            {
                // 黑幕阶段已启动：静默跳过，避免双开告警刷屏
                return;
            }

            bool started = storyGsm.TriggerStory(VillageStartStoryName);
            Debug.Log(started
                ? "[VillageStart] OnEnterScene 兜底 TriggerStory " + VillageStartStoryName
                : "[VillageStart] TriggerStory 未启动 " + VillageStartStoryName);
        }

        void OnVillageStartStoryTriggeredForCover()
        {
            var storyGsm = GetModule<StoryComponentGSM>();
            if (storyGsm != null)
            {
                storyGsm.onStoryTriggered -= OnVillageStartStoryTriggeredForCover;
            }

            // 壳已回调；再等极短 hold 让 Prefab Instantiate + 全屏 BG 就绪，然后亮屏分层
            WaitForInvoke(VillageStartBgReadyHoldSeconds, FinalizeVillageStartCoverAndCloseBlack);
        }

        void OnVillageStartCoverTimeout()
        {
            if (villageStartCoverCloseIssued)
            {
                return;
            }

            Debug.LogWarning("[VillageStart] BG Ready 超时，强制按分层准备后淡出黑幕");
            FinalizeVillageStartCoverAndCloseBlack();
        }

        void FinalizeVillageStartCoverAndCloseBlack()
        {
            if (villageStartCoverCloseIssued)
            {
                return;
            }

            villageStartCoverCloseIssued = true;

            // 淡出前：只保证 BG 盖景、框与立绘为 0（废除旧 Snap 拉满，否则分层必败）
            PrepareVillageStartLayeredReveal();

            var close = deferredCloseBlackAndNotify;
            deferredCloseBlackAndNotify = null;
            if (close != null)
            {
                // 黑幕淡完后开闸，Prefab Wait 才开始 Hold→立绘（与黑幕时长解耦）
                var loadGsm = GetModule<LoadSceneComponentGSM>();
                if (loadGsm != null)
                {
                    void OnBlackFullyGone()
                    {
                        loadGsm.onEndLoadingSceneEvent -= OnBlackFullyGone;
                        VillageStartLayerRevealGate.SignalBgFullyVisible();
                        Debug.Log("[VillageStart] 黑幕淡完，分层闸门开启（可开始 BG 空拍）");
                    }

                    loadGsm.onEndLoadingSceneEvent += OnBlackFullyGone;
                }
                else
                {
                    VillageStartLayerRevealGate.SignalBgFullyVisible();
                }

                Debug.Log("[VillageStart] BG 盖满且框/立绘已藏，CloseFormFade（拍1）");
                close.Invoke();
            }
            else
            {
                VillageStartLayerRevealGate.SignalBgFullyVisible();
            }
        }

        /// <summary>
        /// 分层显现准备（白名单）：只藏字幕条 + DialogueScene 下场景大立绘；BG 保持可见。
        /// <para>
        /// 重要原因：禁止 <c>GetComponentsInChildren</c> + 名字 Contains 广扫整棵 Panel——
        /// 会误伤 <c>Bottom/Mask/...</c> 内同名 Painting（alpha=0），而 Presenter 只 SetActive 无法自愈 → 小头像黑窗。
        /// </para>
        /// <para>
        /// 其它开场复用：照抄本白名单模式，把场景立绘名换成该 Prefab BB 实际节点；
        /// <b>不要</b>再发明一套名字模糊匹配。名单外 CanvasGroup 一律不碰。
        /// </para>
        /// </summary>
        void PrepareVillageStartLayeredReveal()
        {
            var uiPath = UIPrefabPath.GetUIPrefabPath("NormalDialogueNewPanel");
            var uiForm = GameManager.GetGMComponent<UIComponentGM>().GetUIForm(uiPath);
            if (uiForm == null || uiForm.Logic == null)
            {
                return;
            }

            var logicRoot = uiForm.Logic;

            // —— 白名单 1：字幕条（拍2 由 Prefab NormalDialogueUIAlpha 拉回）——
            if (logicRoot is NormalDialogueFormNewLogic dialogueLogic
                && dialogueLogic.dialogueUICanvasGroup != null)
            {
                dialogueLogic.dialogueUICanvasGroup.alpha = 0f;
            }

            // 场景实例挂在 DialogueSceneContainer 下；Mask 小头像在 Bottom 下，故意不进此根
            var sceneRoot = UIUtils.findChild(logicRoot.gameObject, "DialogueSceneContainer", hasDebugLog: false);
            if (sceneRoot == null)
            {
                Debug.LogWarning("[VillageStart][Prepare] 未找到 DialogueSceneContainer，跳过场景立绘白名单");
                return;
            }

            // —— 白名单 2：全屏 BG 兜底 Active（无 CanvasGroup，不改 alpha）——
            var bg = UIUtils.findChild(sceneRoot, "BG", hasDebugLog: false);
            if (bg != null && !bg.activeSelf)
            {
                bg.SetActive(true);
            }

            // —— 白名单 3：本开场场景大立绘（拍3 由 Prefab CanvasGroupAlpha 拉回）——
            // KenMuNiStart BB：GoOutStoryYaerPainting / GushaPainting；仅在 sceneRoot 下查找
            SetScenePaintingCanvasGroupAlpha(sceneRoot, "GoOutStoryYaerPainting", 0f);
            SetScenePaintingCanvasGroupAlpha(sceneRoot, "GushaPainting", 0f);
        }

        /// <summary>
        /// 仅在对话场景根下按精确子物体名藏/显 CanvasGroup；找不到则静默跳过。
        /// 替代方案：挂 BB 引用列表——需改 FormLogic 序列化，本期用路径名即可。
        /// </summary>
        static void SetScenePaintingCanvasGroupAlpha(GameObject sceneRoot, string paintingObjectName, float alpha)
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
            Debug.Log("[VillageStart][Prepare] hide " + GetTransformPath(paintingGo.transform));
        }

        /// <summary>调试用短路径，验收后可随 Log 一并删。</summary>
        static string GetTransformPath(Transform t)
        {
            if (t == null)
            {
                return string.Empty;
            }

            var path = t.name;
            var p = t.parent;
            while (p != null)
            {
                path = p.name + "/" + path;
                p = p.parent;
            }

            return path;
        }

        protected override void OnOpenFightingPanel(UIFormLogic uIFormLogic)
        {
            var FightingFormLogic = uIFormLogic as FightingFormLogic;
            if (sceneData.homeDoorStoryComplete == false)
            {
                FightingFormLogic.UpdateBattleImageVisiable(false);
            }
        }

        public override TerrainType GetCurSceneTerrainType()
        {
            return TerrainType.LandType;
        }

        /// <summary>播放随机鸟叫资源（与森林相同命名约定）。</summary>
        void PlayBirdAudio()
        {
            var baseName = "鸟叫{0}.mp3";
            var randomIndex = GameTools.getRandomIntNum(1, 3);
            var realName = string.Format(baseName, randomIndex);
            soundSfxCpn_3.ChangeSoundRes(realName);
            soundSfxCpn_3.PlaySound();
        }

        /// <summary>播放风声（SFX_2）。</summary>
        void PlayWindAudio()
        {
            soundSfxCpn_2.PlaySound();
        }

        public override void initAllSceneMonster()
        {
        }

        /// <summary>
        /// 按类型设置场景实体显隐；本村未配置该逻辑组件时静默跳过。
        /// </summary>
        void TrySetSceneEntityActive<T>(bool active) where T : BaseSceneEntityLogic
        {
            var logic = GetModule<SceneEntityComponentGSM>().GetSceneEntityLogic<T>();
            if (logic != null)
            {
                logic.SetObjActive(active);
            }
        }
    }
}
