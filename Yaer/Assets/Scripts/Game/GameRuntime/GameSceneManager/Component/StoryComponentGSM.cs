using System;
using System.Collections.Generic;
using System.Linq;
using Game.GameMgr;
using Game.GameMgr.Component;
using Game.GameMgr.Component.Archive.ArchiveDataClass;
using Game.GameMgr.Component.Story;
using Game.GameMgr.Component.UI;
using Game.GameRuntime.GameSceneManager.Base;
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
            string uiPrefabPath = UIPrefabPath.GetUIPrefabPath("NormalDialogueNewPanel");
            
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
        }
    }
}