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
using Game.Static.Enum.Map;
using Game.Static.Name.Res;
using Game.Static.Path;
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
    /// 勿绑晚宴台本；勿用 Loading 当这次换人表现；合层古莎不替代玩家 Controllable。
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

        [Header("续聊结束 · 古莎待机换动画合层（0901）")]
        [Tooltip("合层内静态「古莎待机」；可空，运行时按名解析")]
        [SerializeField]
        private GameObject gushaStandby;

        [Tooltip("预置的「古莎动画合层」实例（默认关）；可空，运行时按名解析")]
        [SerializeField]
        private GameObject gushaAnimComposite;

        [Header("进场落点诊断（0901 飞出排查）")]
        [Tooltip("打 [ChiefEnterPos]：lastScene / 选用锚点 / 与 DefaultBorn 距离；验收通过后可关")]
        [SerializeField]
        private bool enableEnterPosDebugLog = true;

        /// <summary>已订 onStoryEnd，等待续聊结束开黑换人。</summary>
        private bool awaitingContinueStoryEnd;

        /// <summary>换人黑幕进行中，防重入。</summary>
        private bool gushaSwapOrchestrating;

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

            // C1：趁 LoadingPanel 仍盖住时立刻 Trigger，减少露景。
            TryTriggerChiefContinueOnce();
        }

        public override void OnShutDown()
        {
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

            // F3 双保险：再 Flush 一次（SetPos 村模式已 Teleport+Flush；此处确保 Loading 前 Rb≡Transform）
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
                return;
            }

            bool started = storyGsm.TriggerStory(ContinueStoryName);
            Debug.Log(started
                ? "[ChiefContinue] OnEnterScene TriggerStory " + ContinueStoryName
                : "[ChiefContinue] TriggerStory 未启动（Prefab 可能缺失）" + ContinueStoryName);

            if (started)
            {
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
                ApplyGushaVisual(showAnim: true);
                MarkGushaAnimStandbyFlag();
                Debug.Log("[ChiefGushaSwap] 全黑内已切换：待机关 → 动画合层开，并记档旗。");
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
