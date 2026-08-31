using Game.GameMgr.Component.Archive.ArchiveDataClass;
using Game.GameRuntime.Entities.Base.BaseSceneObj;
using Game.GameRuntime.Entities.Component.Interactive;
using Game.GameRuntime.Entities.Player;
using Game.GameRuntime.GameSceneManager.Component.Story;
using System;
using UnityEngine;

namespace Game.GameRuntime.Entities.SceneEntities
{
    /// <summary>
    /// 简单剧情触发器：挂在场景实体上，场景初始化后按配置触发对话剧情。
    /// <para>
    /// 支持三种方式：<see cref="TriggerType.Click"/>（点击）、<see cref="TriggerType.Enter"/>（进入范围）、
    /// <see cref="TriggerType.Stay"/>（在范围内停留达到 <see cref="StayTimeToTriggerStory"/> 秒）。
    /// 实际播放由 <see cref="StoryComponentGSM.TriggerStory"/> 发起。
    /// </para>
    /// <para>
    /// <see cref="StoryPrefabName"/> 需与工程内对话预制体名称一致（常见路径如 <c>GameRes/Prefabs/Dialogue/</c>）；
    /// 与 <see cref="StoryComponentGSM"/> 及
    /// <see cref="Game.GameRuntime.UI.FormLogic.Story.Dialogue.NormalDialogueFormNewLogic"/> 等 UI 流程配合。
    /// </para>
    /// <para>
    /// 若开启 <see cref="SingleUseInArchive"/>，同一存档内该剧情仅完成一次；与 <see cref="StoryTriggerCountData"/> 及剧情结束时的存档逻辑同步。
    /// </para>
    /// </summary>
    public class SimpleStoryTrigger : BaseSceneEntityLogic
    {
        /// <summary>剧情触发方式。</summary>
        public enum TriggerType
        {
            /// <summary>点击带 <see cref="InteractiveComponent"/> 的物体时触发。</summary>
            Click,

            /// <summary>进入交互触发范围时触发。</summary>
            Enter,

            /// <summary>
            /// 在范围内累计停留，达到 <see cref="StayTimeToTriggerStory"/> 秒后触发；
            /// 在 <see cref="OnUpdate"/> 中用 <paramref name="realElapseSeconds"/> 累加（也可用协程/定时器实现）。
            /// </summary>
            Stay
        }

        /// <summary>对话剧情预制体名称，须与资源中 prefab 名一致。</summary>
        [SerializeField]
        private string StoryPrefabName;

        /// <summary>可选：对话选项 UI 在世界空间中的锚点。</summary>
        [SerializeField]
        public Transform OptionPos;

        /// <summary>
        /// 为 true 时同一存档内仅一次；若已使用则禁用本组件，仍会调用 <see cref="InitSomeEventState"/> 同步关卡/战斗状态。
        /// </summary>
        [SerializeField]
        private bool SingleUseInArchive = false;

        /// <summary>当前触发模式，决定在 <see cref="OnInit"/> 中订阅哪些交互事件。</summary>
        [SerializeField]
        private TriggerType triggerType = TriggerType.Click;

        /// <summary>仅 <see cref="TriggerType.Stay"/> 时有效：需累计停留的秒数。</summary>
        [SerializeField]
        private float StayTimeToTriggerStory;

        /// <summary>Stay 模式：玩家是否在交互范围内。</summary>
        private bool PlayerEnter = false;

        /// <summary>Stay 模式：已累计停留时间（秒）；离开范围会清零。</summary>
        private float CurrentStayTime;

        /// <summary>存档中的剧情触发/使用记录。</summary>
        private StoryTriggerCountData storyTriggerCountData;

        /// <summary>当前场景剧情模块。</summary>
        private StoryComponentGSM storyComponentGSM;

        /// <summary>本实体交互组件。</summary>
        private InteractiveComponent interactiveComponent;

        /// <summary>自初始化起成功完成剧情的次数（内存计数，不写进存档）。</summary>
        public int TriggerCountFromInit { get; private set; }

        /// <summary>存档中与本 <see cref="StoryPrefabName"/> 相关的触发次数。</summary>
        public int TriggerCount => storyTriggerCountData.GetStoryTriggerCount(StoryPrefabName);

        /// <summary>初始化：读存档、取模块、按 <see cref="triggerType"/> 绑定事件。</summary>
        /// <param name="userData">基类参数，此处未使用。</param>
        protected internal override void OnInit(object userData)
        {
            base.OnInit(userData);
            TriggerCountFromInit = 0;
            storyTriggerCountData = SceneManager.GetArchiveData<StoryTriggerCountData>();
            storyComponentGSM = SceneManager.GetModule<StoryComponentGSM>();
            interactiveComponent = componentSystem.GetComponent<InteractiveComponent>();

            // 一次性剧情：已用过则禁用，但仍 InitSomeEventState（读档后与场景物体/战斗状态一致）
            if (SingleUseInArchive)
            {
                if (storyTriggerCountData.CheckStoryUsed(StoryPrefabName))
                {
                    // 0722 验收：存档已记过章末剧情名时，右缘再走进来也不会开章末面板（最常见「没演出」原因）
                    if (IsChapterEndDebugTarget())
                    {
                        Debug.LogWarning(
                            $"[ChapterEnd] SimpleStoryTrigger 已用过，组件禁用。story={StoryPrefabName} " +
                            $"go={gameObject.name} path={BuildDebugPath(transform)} " +
                            $"triggerCount={storyTriggerCountData.GetStoryTriggerCount(StoryPrefabName)}");
                    }
                    this.enabled = false;
                    InitSomeEventState(StoryPrefabName);
                    return;
                }
                else
                {
                    InitSomeEventState(StoryPrefabName);
                }
            }

            if (triggerType == TriggerType.Click)
            {
                interactiveComponent.onClickInteractiveEvent += OnClickTriggerStory;
            }
            else if (triggerType == TriggerType.Enter)
            {
                interactiveComponent.onEnterInteractiveEvent += OnEnterTriggerStory;
                if (IsChapterEndDebugTarget())
                {
                    Debug.Log(
                        $"[ChapterEnd] SimpleStoryTrigger 已订阅 Enter。story={StoryPrefabName} " +
                        $"go={gameObject.name} singleUse={SingleUseInArchive}");
                }
            }
            else
            {
                // Stay：进入/离开维护 PlayerEnter；时间在 OnUpdate 中累加
                interactiveComponent.onEnterInteractiveEvent += OnPlayerEnter;
                interactiveComponent.onExitInteractiveEvent += OnPlayerExit;
            }
        }

        /// <summary>Enter 模式：玩家进入范围且未死亡时触发剧情。</summary>
        private void OnEnterTriggerStory(InteractiveComponent component)
        {
            if (component.Entity?.Logic is PlayerLogic playerLogic)
            {
                if (!playerLogic.isDead)
                {
                    if (IsChapterEndDebugTarget())
                    {
                        Debug.Log(
                            $"[ChapterEnd] Enter 命中，准备 TriggerStory。story={StoryPrefabName} go={gameObject.name}");
                    }
                    TriggerStory();
                }
                else if (IsChapterEndDebugTarget())
                {
                    Debug.LogWarning(
                        $"[ChapterEnd] Enter 命中但玩家已死亡，跳过。story={StoryPrefabName}");
                }
            }
        }

        /// <summary>Click 模式：点击回调中触发剧情。</summary>
        private void OnClickTriggerStory(InteractiveComponent component)
        {
            TriggerStory();
        }

        /// <summary>
        /// 解析本次应播放的对话 prefab 名；子类可覆写以实现按存档/任务状态切对话（如埃吉尔交付线）。
        /// </summary>
        protected virtual string ResolveStoryPrefabName() => StoryPrefabName;

        /// <summary>
        /// 若 <see cref="StoryComponentGSM.TriggerStory"/> 返回 true，则订阅 <see cref="StoryComponentGSM.onStoryEnd"/>，
        /// 结束时在 <see cref="OnStoryFinished"/> 中取消订阅。
        /// </summary>
        /// <remarks>若已有剧情在运行，可能返回 false，此时不会订阅 onStoryEnd。</remarks>
        protected virtual void TriggerStory()
        {
            TryStartBoundStory();
        }

        /// <summary>
        /// 子类黑幕编排用：在全黑后再调用。成功启动则订阅 onStoryEnd。
        /// </summary>
        /// <returns>是否已成功启动对话。</returns>
        protected bool TryStartBoundStory()
        {
            var storyPrefab = ResolveStoryPrefabName();
            if (string.IsNullOrEmpty(storyPrefab))
            {
                if (IsChapterEndDebugTarget())
                {
                    Debug.LogWarning($"[ChapterEnd] TriggerStory 失败：剧情名为空。go={gameObject.name}");
                }
                return false;
            }

            if (SingleUseInArchive)
            {
                if (storyTriggerCountData.CheckStoryUsed(storyPrefab))
                {
                    if (IsChapterEndDebugTarget())
                    {
                        Debug.LogWarning(
                            $"[ChapterEnd] TriggerStory 跳过：存档已使用。story={storyPrefab}");
                    }
                    return false;
                }
            }

            if (storyComponentGSM.TriggerStory(storyPrefab))
            {
                if (IsChapterEndDebugTarget())
                {
                    Debug.Log($"[ChapterEnd] TriggerStory 成功启动。story={storyPrefab}");
                }
                storyComponentGSM.onStoryEnd += OnStoryFinished;
                return true;
            }

            if (IsChapterEndDebugTarget())
            {
                // 常见原因：已有其它对话在跑（HasRunningStory）
                Debug.LogWarning(
                    $"[ChapterEnd] TriggerStory 被拒绝（可能已有剧情在播）。story={storyPrefab} " +
                    $"hasRunning={storyComponentGSM.HasRunningStory} " +
                    $"running={storyComponentGSM.CurrentRunningStoryName}");
            }

            return false;
        }

        /// <summary>
        /// 开黑幕前预检：空名 / 单次已用 / 已有剧情在跑 → false（勿先开黑）。
        /// </summary>
        protected bool CanStartStoryNow()
        {
            var storyPrefab = ResolveStoryPrefabName();
            if (string.IsNullOrEmpty(storyPrefab))
            {
                return false;
            }

            if (SingleUseInArchive && storyTriggerCountData.CheckStoryUsed(storyPrefab))
            {
                return false;
            }

            if (storyComponentGSM != null && storyComponentGSM.HasRunningStory)
            {
                return false;
            }

            return true;
        }

        /// <summary>供子类订阅壳就绪 / 超时兜底。</summary>
        protected StoryComponentGSM StoryGsm => storyComponentGSM;

        /// <summary>
        /// 剧情结束：计数并移除 onStoryEnd 订阅。
        /// 子类可 <c>override</c> 后 <c>base</c>，再挂切场等收尾（如门口对白 → Loading 进屋）。
        /// </summary>
        protected virtual void OnStoryFinished()
        {
            TriggerCountFromInit++;
            storyComponentGSM.onStoryEnd -= OnStoryFinished;
        }

        /// <summary>Stay：玩家进入范围，开始计时。</summary>
        private void OnPlayerEnter(InteractiveComponent component)
        {
            if (component.Entity?.Logic is PlayerLogic)
            {
                CurrentStayTime = 0;
                PlayerEnter = true;
            }
        }

        /// <summary>Stay：玩家离开范围，停止计时。</summary>
        private void OnPlayerExit(InteractiveComponent component)
        {
            if (component.Entity?.Logic is PlayerLogic)
            {
                CurrentStayTime = 0;
                PlayerEnter = false;
            }
        }

        /// <summary>
        /// Stay：每帧在范围内且无进行中剧情时累加停留时间；达到阈值则 <see cref="TriggerStory"/> 并重置状态。
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间。</param>
        /// <param name="realElapseSeconds">真实时间增量，用于停留累计。</param>
        protected internal override void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            if (PlayerEnter)
            {
                // 有剧情播放时不累计停留时间
                if (!storyComponentGSM.HasRunningStory)
                {
                    CurrentStayTime += realElapseSeconds;
                    if (CurrentStayTime >= StayTimeToTriggerStory)
                    {
                        TriggerStory();
                        CurrentStayTime = 0;
                        PlayerEnter = false;
                    }
                }
            }
        }

        /// <summary>
        /// 按对话名把一次性剧情状态同步到对应场景 Mgr（<c>InitBattleData</c>）；未列出的名称不处理。
        /// </summary>
        /// <param name="storyPrefabName">与 <see cref="StoryPrefabName"/> 相同的资源名。</param>
        private void InitSomeEventState(string storyPrefabName)
        {
            switch (storyPrefabName)
            {
                case "VerdantCorridorBeforeDestoryNest":
                    WoodWormRootBattleMgr.getInstance().InitBattleData(SingleUseInArchive, enabled);
                    break;
                case "WestRappRoadGoblinAndGusha":
                    WestRappRoadBossBattleMgr.getInstance().InitBattleData(SingleUseInArchive, enabled);
                    break;
                case "ForestEastSceneSlimeEatSheep":
                    SlimeEatSheepStoryMgr.getInstance().InitBattleData(SingleUseInArchive, enabled);
                    break;
                default:
                    break;
            }
        }

        /// <summary>验收日志用：输出物体层级路径，便于确认是否是 ChapterEndTrigger。</summary>
        static string BuildDebugPath(Transform t)
        {
            var names = new System.Collections.Generic.List<string>();
            while (t != null)
            {
                names.Add(t.name);
                t = t.parent;
            }
            names.Reverse();
            return string.Join("/", names);
        }

        /// <summary>仅章末相关触发器打 [ChapterEnd] 日志，避免污染其它 Enter 剧情 Console。</summary>
        bool IsChapterEndDebugTarget()
        {
            if (gameObject.name == "ChapterEndTrigger")
            {
                return true;
            }
            return !string.IsNullOrEmpty(StoryPrefabName) &&
                   StoryPrefabName.StartsWith("ChapterEndStory");
        }
    }
}