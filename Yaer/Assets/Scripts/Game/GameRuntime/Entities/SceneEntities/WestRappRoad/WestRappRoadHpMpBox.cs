using Game.GameMgr;
using Game.GameMgr.Component;
using Game.GameMgr.Component.Archive.ArchiveDataClass.Player;
using Game.GameMgr.Component.Archive.ArchiveDataClass.Scene;
using Game.GameRuntime.Entities.Base.BaseSceneObj;
using Game.GameRuntime.Entities.Component.Interactive;
using Game.GameRuntime.Entities.Player;
using Game.GameRuntime.GameSceneManager.Component;
using Game.GameRuntime.GameSceneManager.Component.Story;
using Game.Static.Enum.Goods;
using UnityEngine;

namespace Game.GameRuntime.Entities.SceneEntities.WestRappRoad
{
    /// <summary>
    /// WestRappRoad 的药水箱子：结构对齐 HomeScene2Box。
    /// </summary>
    public class WestRappRoadHpMpBox : BaseSceneEntityLogic
    {
        public Animator animator;
        public SoundToggleComponent soundSfxCpn;

        [SerializeField] private bool useStoryOnOpen = false;
        [SerializeField] private string storyName = "";
        [SerializeField] private int hpBallCount = 2;
        [SerializeField] private int mpBallCount = 2;
        [SerializeField] private bool enableDebugLog = true;

        private bool opened; // 已经打开过标识

        protected internal override void OnInit(object userData)
        {
            base.OnInit(userData);

            animator = GetComponent<Animator>();
            componentSystem.GetComponent<InteractiveComponent>().onClickInteractiveEvent += OpenBox;

            if (enableDebugLog)
            {
                Debug.Log(
                    "[WestRappRoadHpMpBox][OnInit-BeforeRead] about to read archive by type: "
                    + nameof(WestRappRoadData),
                    gameObject
                );
            }

            var westData = SceneManager.GetArchiveData<WestRappRoadData>();
            if (enableDebugLog)
            {
                Debug.Log(
                    "[WestRappRoadHpMpBox][OnInit-AfterRead] "
                    + $"archiveType={westData.GetType().FullName}, "
                    + $"archiveInstanceHash={westData.GetHashCode()}, "
                    + $"hpMpBoxOpened={westData.hpMpBoxOpened}",
                    gameObject
                );
            }

            if (westData.hpMpBoxOpened)
            {
                animator.SetBool("Open", true);
                opened = true;
            }

            // 设置宝箱是否可与玩家交互
            componentSystem.GetComponent<InteractiveComponent>().entityControll.canTouchWithPlayer = !opened;

            DebugArchiveAndInteractiveState("OnInit");
        }

        public override void OnShutDown()
        {
            base.OnShutDown();
            componentSystem.GetComponent<InteractiveComponent>().onClickInteractiveEvent -= OpenBox;
        }

        /// <summary>
        /// 打开箱子（供剧情事件调用）
        /// </summary>
        public void OnWestRappRoadHpMpBox_OpenBox()
        {
            SceneManager.GetArchiveData<WestRappRoadData>().hpMpBoxOpened = true;
            animator.SetBool("Open", true);
            soundSfxCpn?.PlaySound();
        }

        /// <summary>
        /// 获得药水（供剧情事件调用）
        /// </summary>
        public void OnWestRappRoadHpMpBox_GetHpMp()
        {
            var bag = SceneManager.GetArchiveData<PlayerBagData>();
            bag.AddMainItem(EMainItemName.HpBall, hpBallCount);
            bag.AddMainItem(EMainItemName.MpBall, mpBallCount);

            var tips = SceneManager.GetModule<TipsComponentGSM>();
            tips.OpenTipsForm("GetHpBall");
            tips.OpenTipsForm("GetMpBall");
        }

        private void OpenBox(InteractiveComponent component)
        {
            if (enableDebugLog)
            {
                Debug.Log("[WestRappRoadHpMpBox] OpenBox() invoked.", gameObject);
            }
            DebugArchiveAndInteractiveState("OpenBox-Before");

            if (opened) return;

            opened = true;
            componentSystem.GetComponent<InteractiveComponent>().entityControll.canTouchWithPlayer = !opened;

            // 设置按键提示消失
            var playerLogic = GameManager.GetGMComponent<EntityComponentGM>().GetEntityLogic<PlayerLogic>();
            if (playerLogic)
            {
                if (playerLogic.keyTipsNode && playerLogic.keyTipsNode.activeSelf)
                {
                    playerLogic.showKeyTipsNode(false);
                }
            }

            // 默认直接开箱发奖励，避免 story prefab 缺失导致箱子被锁死。
            var shouldTriggerStory = useStoryOnOpen && !string.IsNullOrEmpty(storyName);
            if (shouldTriggerStory)
            {
                SceneManager.GetModule<StoryComponentGSM>().TriggerStory(storyName);
            }
            else
            {
                OnWestRappRoadHpMpBox_OpenBox();
                OnWestRappRoadHpMpBox_GetHpMp();
            }

            DebugArchiveAndInteractiveState("OpenBox-After");
        }

        private void DebugArchiveAndInteractiveState(string phase)
        {
            if (!enableDebugLog) return;

            var westData = SceneManager.GetArchiveData<WestRappRoadData>();
            var homeData = SceneManager.GetArchiveData<HomeScene2Data>();
            var interactive = componentSystem.GetComponent<InteractiveComponent>();
            var canTouch = interactive != null && interactive.entityControll != null && interactive.entityControll.canTouchWithPlayer;

            Debug.Log(
                $"[WestRappRoadHpMpBox][{phase}] " +
                $"opened={opened}, " +
                $"west.hpMpBoxOpened={westData.hpMpBoxOpened}, " +
                $"home.boxOpened={homeData.boxOpened}, " +
                $"canTouchWithPlayer={canTouch}, " +
                $"storyName={storyName}",
                gameObject
            );

            // 防误用告警：West 场景箱子不应依赖 Home 的开箱状态。
            // 当 West 记录未开、但 Home 已开且当前交互被关闭时，极可能是读取了错误的数据源或迁移时混入了 Home 逻辑。
            if (!westData.hpMpBoxOpened && homeData.boxOpened && !canTouch)
            {
                Debug.LogError(
                    "[WestRappRoadHpMpBox] 检测到可疑状态：west.hpMpBoxOpened=false 但 home.boxOpened=true 且 canTouchWithPlayer=false。"
                    + " 请检查是否误用了 HomeScene2Data / HomeScene2Box 的逻辑分支。",
                    gameObject
                );
            }
        }
    }
}
