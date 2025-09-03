using System;
using Game.GameMgr.Component.UI;
using Game.GameRuntime.GameSceneManager.Base;
using Game.GameRuntime.UI.FormLogic.Story.Base;
using Game.GameRuntime.UI.FormLogic.Story.Dialogue;
using Game.Static.Path;
using UnityEngine;

namespace Game.GameMgr.Component.Story
{
    public abstract class BaseStory : MonoBehaviour, IStory
    {
        [HideInInspector] public string assetPath;
        [SerializeField] public string storyName;
        [SerializeField] protected string fileDataPath;
        [SerializeField] protected string uiAssetPath;

        private bool isEnd;
        private bool isActive;
        public bool IsEnd => isEnd;
        public bool IsActive => isActive;
        public string StoryName => storyName;

        protected IGameSceneManager sceneManager;
        
        /// <summary>
        /// 剧情 结束事件，需要重复订阅
        /// </summary>
        public event Action<BaseStory> onStoryEnd;

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(fileDataPath))
            {
                fileDataPath = $"Assets/GameRes/Config/DialogueConfig/{name}.json";
            }

            if (string.IsNullOrEmpty(storyName))
            {
                storyName = name;
            }
        }

        public virtual void OnInit(object userData)
        {
        }

        public virtual void OnEnter(object userData)
        {
            isActive = true;
            isEnd = false;
            gameObject.SetActive(true);
        }

        public virtual void OnUpdate()
        {
        }

        public virtual void OnExit()
        {
            isActive = false;
            isEnd = true;
            gameObject.SetActive(false);
            onStoryEnd?.Invoke(this);
            onStoryEnd = null;
        }

        public virtual void OnShutDown()
        {
            onStoryEnd = null;
            isActive = false;
            isEnd = true;
            gameObject.SetActive(false);
        }

        public void SetSceneManager(IGameSceneManager manager) => sceneManager = manager;

        protected void TriggerLoopDialogueStory(string fileName)
        {
        }
    }
}