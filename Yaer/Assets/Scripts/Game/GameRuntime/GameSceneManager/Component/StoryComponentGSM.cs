using System;
using System.Collections.Generic;
using System.Linq;
using Game.GameMgr;
using Game.GameMgr.Component;
using Game.GameMgr.Component.Archive.ArchiveDataClass;
using Game.GameMgr.Component.Story;
using Game.GameMgr.Component.UI;
using Game.GameRuntime.GameSceneManager.Base;
using Game.GameRuntime.UI.FormLogic;
using Game.GameMgr.Manager.Settings;
using Game.GameMgr.Manager.Settings.Helper;
using Game.GameRuntime.UI.FormLogic.Story.Dialogue;
using Game.Static.Path;
using GameFramework.UnityRuntime.Utility;
using UnityEngine;

namespace Game.GameRuntime.GameSceneManager.Component.Story
{
    /// <summary>
    /// 处理剧情，并复用剧情对象
    /// </summary>
    public class StoryComponentGSM : BaseComponentGSM
    {
        [SerializeField] private List<BaseStory> sceneStories = new List<BaseStory>(); // 场景中的剧情

        private StoryComponentGM storyComponentGM;
        private List<BaseStory> activeStories = new List<BaseStory>();

        public event Action onStoryTriggered;
        public event Action onStoryEnd;

        private StoryTriggerCountData storyTriggerCountData;
        public bool HasRunningStory {  get; private set; }
        public string CurrentRunningStoryName {  get; private set; }

        private void OnValidate()
        {
            sceneStories = gameObject.GetComponentsInChildren<BaseStory>(true).ToList();
        }

        // 在组件初始化时创建对象池
        public override void OnInit(IGameSceneManager manager)
        {
            base.OnInit(manager);

            storyComponentGM = GameManager.GetGMComponent<StoryComponentGM>();

            // 默认隐藏
            foreach (var story in sceneStories)
            {
                story.gameObject.SetActive(false);
            }

            HasRunningStory = false;
            storyTriggerCountData = SceneManager.GetArchiveData<StoryTriggerCountData>();
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            foreach (var story in activeStories)
            {
                if (story.IsActive && !story.IsEnd)
                {
                    story.OnUpdate();
                }
            }
        }

        public override void OnShutdown()
        {
            base.OnShutdown();

            ShutDownStory();
        }

        public void ShutDownStory()
        {
            // 创建一个临时列表来存储需要删除的剧情
            List<BaseStory> storiesToRemove = new List<BaseStory>();

            // 遍历 activeStories，将每个剧情添加到临时列表
            foreach (var story in activeStories)
            {
                storiesToRemove.Add(story);
            }

            // 遍历临时列表，逐个关闭剧情，并从 activeStories 中移除
            foreach (var story in storiesToRemove)
            {
                story.OnShutDown();
                activeStories.Remove(story);

                if (story is BaseSceneStory)
                {
                    story.gameObject.SetActive(false);
                }
                else
                {
                    storyComponentGM.Unspawn(story);
                }
            }
        }

        public bool TriggerStory(string storyPrefabName, bool ignoreCurrentRunningStory = false)
        {
            if (ignoreCurrentRunningStory || !HasRunningStory)
            {
                if (HasRunningStory)
                {
                    OnStoryEnd();
                }
                HasRunningStory = true;
                CurrentRunningStoryName = storyPrefabName;
                var ResMgr = GameManager.GetGMComponent<ResComponentGM>();
                ResMgr.LoadAsset<GameObject>(DialoguePath.GetPath(storyPrefabName), OnStoryPrefabLoad);
                return true;
            }
            return false;
        }

        private void OnStoryPrefabLoad(GameObject go)
        {
            if (go == null)
            {
                // ResComponentGM 的 Load 失败时走失败回调、通常不会进成功分支；此日志用于确认回调偶发传空
                Log.Error("OnStoryPrefabLoad: 已加载的预制体为 null。CurrentName={0}", CurrentRunningStoryName);
                return;
            }
            string uiPrefabPath = UIPrefabPath.GetUIPrefabPath("NormalDialogueNewPanel");
            // 重要修改说明：
            // 对话开始前统一关闭战斗立绘，避免“对话UI + 战斗立绘”同屏造成视觉混叠。
            // 这里不隐藏整个 FightingPanel，只处理战斗立绘，确保战斗HUD其它元素行为不受影响。
            SetFightingBattleIllustrationVisible(false);
            
            var uiForm = GameManager.GetGMComponent<UIComponentGM>().GetUIForm(uiPrefabPath);
            var sceneMgr = GameManager.GetGameSceneManager() as BaseGameSceneManager;
            if (uiForm == null)
            {
                GameManager.GetGMComponent<UIComponentGM>().OpenUIForm(uiPrefabPath, EUIGroup.Middle, new OpenFormArgs()
                {
                    callBack = logic =>
                    {
                        if (sceneMgr != null) { sceneMgr.curStoryPrefab = go; }// 记录当前添加到场景中的对象
                        if (logic is NormalDialogueFormNewLogic dialogueForm)
                        {
                            onStoryTriggered?.Invoke();
                            // 直接开始对话
                            dialogueForm.StartDialogue(go);
                        }
                    }
                });
            }
            else
            {
                onStoryTriggered?.Invoke();
                (uiForm.Logic as NormalDialogueFormNewLogic).StartDialogue(go);
            }
        }

        public void OnStoryEnd()
        {
            HasRunningStory = false;
            // 对话触发次数增加
            storyTriggerCountData.OnStoryTriggered(CurrentRunningStoryName);
            CurrentRunningStoryName = null;
            // 剧情管理器广播对话结束事件
            onStoryEnd?.Invoke();
            // 对话结束之后设置对话预制体为null
            var sceneMgr = GameManager.GetGameSceneManager() as BaseGameSceneManager;
            if (sceneMgr != null) { sceneMgr.curStoryPrefab = null; }

            // 剧情结束后，根据当前设置重新同步一次战斗立绘的显示状态（兜底）。
            // 与“对话开始时强制关闭”配对，确保不会把用户设置永久覆盖。
            // —— 注意：下面对「显示」的调用传 isStoryEndRestore: true。FightingFormLogic 内会【延迟约 0.3~0.5s】再真正打开战斗立绘，
            // 用于错开本帧 OnStoryEnd 与「紧接着的下一道教学/自言自语对话」的加载时机，避免战斗立绘与对话 UI 同一瞬间抢显示导致闪屏。
            // 【维护警告】若改为单参或 false 取消延迟，易复现“国王演出结束接战斗教学时立绘闪一下”的回归，非需求勿改。
            var settingManager = GameManager.GetManager<SettingManager>();
            if (settingManager != null)
            {
                var configData = settingManager.LoadSetting<SettingsConfigData>();
                if (configData != null)
                {
                    SetFightingBattleIllustrationVisible(configData.showBattleImage, isStoryEndRestore: true);
                }
            }
        }

        /// <summary>
        /// 统一控制战斗立绘显隐，避免在多个流程重复获取 FightingPanel 逻辑并提升可维护性。
        /// </summary>
        /// <param name="isVisible">true 为显示，false 为隐藏。</param>
        /// <param name="isStoryEndRestore">
        /// 仅 <c>true</c> 时，由 <see cref="FightingFormLogic"/> 在「应显示」路径上走协程延迟再显，避免 <c>OnStoryEnd</c> 与紧接教学对话的显示冲突与立绘闪烁；对话开始时传 <c>false</c> 以立刻关战斗立绘并取消未完成的延迟显式。
        /// <para>【维护警告】从 <see cref="OnStoryEnd"/> 恢复立显时必须为 <c>true</c>，勿为省事改为默认单参，否则会失去与 FightingFormLogic 的延迟配合。</para>
        /// </param>
        private void SetFightingBattleIllustrationVisible(bool isVisible, bool isStoryEndRestore = false)
        {
            string fightingPanelPath = UIPrefabPath.GetUIPrefabPath("FightingPanel");
            var uiForm = GameManager.GetGMComponent<UIComponentGM>().GetUIForm(fightingPanelPath);
            if (uiForm != null && uiForm.Logic is FightingFormLogic fightingFormLogic)
            {
                fightingFormLogic.UpdateBattleImageVisiable(isVisible, isStoryEndRestore);
            }
        }
    }
}