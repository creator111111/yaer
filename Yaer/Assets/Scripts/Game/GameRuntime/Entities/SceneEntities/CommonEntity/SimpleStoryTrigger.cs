using Game.GameMgr.Component.Archive.ArchiveDataClass;
using Game.GameRuntime.Entities.Base.BaseSceneObj;
using Game.GameRuntime.Entities.Component.Interactive;
using Game.GameRuntime.Entities.Player;
using Game.GameRuntime.GameSceneManager.Component.Story;
using System;
using UnityEngine;

namespace Game.GameRuntime.Entities.SceneEntities
{
    public class SimpleStoryTrigger : BaseSceneEntityLogic
    {
        public enum TriggerType
        {
            Click,
            Enter,
            Stay
        }
        [SerializeField]
        private string StoryPrefabName;
        /// <summary>
        /// 对话选择框需要对齐的位置
        /// </summary>
        [SerializeField]
        public Transform OptionPos;
        /// <summary>
        /// 是否一个存档只能触发一次
        /// </summary>
        [SerializeField]
        private bool SingleUseInArchive = false;
        [SerializeField]
        private TriggerType triggerType = TriggerType.Click;
        /// <summary>
        /// 如果剧情为Stay触发，需要等待的时间
        /// </summary>
        [SerializeField]
        private float StayTimeToTriggerStory;

        private bool PlayerEnter = false;
        private float CurrentStayTime;

        private StoryTriggerCountData storyTriggerCountData;
        private StoryComponentGSM storyComponentGSM;
        private InteractiveComponent interactiveComponent;

        public int TriggerCountFromInit { get; private set; }
        public int TriggerCount => storyTriggerCountData.GetStoryTriggerCount(StoryPrefabName);

        protected internal override void OnInit(object userData)
        {
            base.OnInit(userData);
            TriggerCountFromInit = 0;
            storyTriggerCountData = SceneManager.GetArchiveData<StoryTriggerCountData>();
            storyComponentGSM = SceneManager.GetModule<StoryComponentGSM>();
            interactiveComponent = componentSystem.GetComponent<InteractiveComponent>();

            if (SingleUseInArchive)
            {
                if (storyTriggerCountData.CheckStoryUsed(StoryPrefabName))
                {
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
            }
            else
            {
                interactiveComponent.onEnterInteractiveEvent += OnPlayerEnter;
                interactiveComponent.onExitInteractiveEvent += OnPlayerExit;
            }
        }

        private void OnEnterTriggerStory(InteractiveComponent component)
        {
            if (component.Entity?.Logic is PlayerLogic playerLogic)
            {
                // 人物非死亡状态才能触发事件
                if (!playerLogic.isDead)
                {
                    TriggerStory();
                }
            }
        }

        private void OnClickTriggerStory(InteractiveComponent component)
        {
            TriggerStory();
        }

        protected virtual void TriggerStory()
        {
            if (SingleUseInArchive)
            {
                if (storyTriggerCountData.CheckStoryUsed(StoryPrefabName))
                {
                    return;
                }
            }
            if (storyComponentGSM.TriggerStory(StoryPrefabName))
            {
                storyComponentGSM.onStoryEnd += OnStoryFinished;
            }
        }

        protected void OnStoryFinished()
        {
            TriggerCountFromInit++;
            storyComponentGSM.onStoryEnd -= OnStoryFinished;
        }

        private void OnPlayerEnter(InteractiveComponent component)
        {
            if (component.Entity?.Logic is PlayerLogic)
            {
                CurrentStayTime = 0;
                PlayerEnter = true;
            }
        }

        private void OnPlayerExit(InteractiveComponent component)
        {
            if (component.Entity?.Logic is PlayerLogic)
            {
                CurrentStayTime = 0;
                PlayerEnter = false;
            }
        }

        protected internal override void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            if (PlayerEnter)
            {
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

        // 初始化部分事件的数据
        private void InitSomeEventState(string storyPrefabName)
        {
            switch(storyPrefabName)
            {
                case "VerdantCorridorBeforeDestoryNest": // 虫巢事件数据初始化
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
    }
}

