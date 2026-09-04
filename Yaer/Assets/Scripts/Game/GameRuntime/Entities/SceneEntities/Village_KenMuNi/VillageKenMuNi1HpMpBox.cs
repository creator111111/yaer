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

namespace Game.GameRuntime.Entities.SceneEntities.Village_KenMuNi
{
    /// <summary>
    /// 巨树 2 楼 WalkArea2 宝箱：生命球×3 + 体力球×3 + GetHpBall / GetMpBall Tips。
    /// 逻辑拷贝 <c>WestRappRoadHpMpBox</c>；存档读本场景 <see cref="VillageKenMuNi1Data"/>，禁止挂西境脚本。
    /// </summary>
    /// <remarks>
    /// 原因（0901）：West 脚本读 WestRappRoadData，会与村档串档且 Debug 误比 HomeScene2。
    /// 替代方案：对话图发奖——缺 Prefab 会锁箱；本期 <c>useStoryOnOpen=false</c>。
    /// Tips 顺序对齐 West：先 Hp 后 Mp（HomeScene1Xiaer 相反，不跟）。
    /// </remarks>
    public class VillageKenMuNi1HpMpBox : BaseSceneEntityLogic
    {
        public Animator animator;
        public SoundToggleComponent soundSfxCpn;

        /// <summary>是否走 Story Prefab 开箱；默认 false（对齐 West 防锁死）。</summary>
        [SerializeField] private bool useStoryOnOpen = false;

        [SerializeField] private string storyName = "";

        /// <summary>默认 3（产品 ×3）；场景可改。</summary>
        [SerializeField] private int hpBallCount = 3;

        [SerializeField] private int mpBallCount = 3;

        [SerializeField] private bool enableDebugLog = false;

        /// <summary>本局已开（内存）；与存档旗同步。</summary>
        private bool opened;

        protected internal override void OnInit(object userData)
        {
            base.OnInit(userData);

            animator = GetComponent<Animator>();
            componentSystem.GetComponent<InteractiveComponent>().onClickInteractiveEvent += OpenBox;

            var data = SceneManager.GetArchiveData<VillageKenMuNi1Data>();
            if (enableDebugLog)
            {
                Debug.Log(
                    "[VillageKenMuNi1HpMpBox][OnInit] tree2fHpMpBoxOpened="
                    + data.tree2fHpMpBoxOpened,
                    gameObject);
            }

            if (data.tree2fHpMpBoxOpened)
            {
                animator.SetBool("Open", true);
                opened = true;
            }

            componentSystem.GetComponent<InteractiveComponent>().entityControll.canTouchWithPlayer =
                !opened;
        }

        public override void OnShutDown()
        {
            base.OnShutDown();
            componentSystem.GetComponent<InteractiveComponent>().onClickInteractiveEvent -= OpenBox;
        }

        /// <summary>开箱态：存档 + 动画 + SFX（供调试 / 可选 Story 回调）。</summary>
        public void OnOpenBox()
        {
            SceneManager.GetArchiveData<VillageKenMuNi1Data>().tree2fHpMpBoxOpened = true;
            animator.SetBool("Open", true);
            soundSfxCpn?.PlaySound();
        }

        /// <summary>入包 Hp/Mp + Tips 队列（先 Hp 后 Mp）。</summary>
        public void OnGetHpMp()
        {
            var bag = SceneManager.GetArchiveData<PlayerBagData>();
            bag.AddMainItem(EMainItemName.HpBall, hpBallCount);
            bag.AddMainItem(EMainItemName.MpBall, mpBallCount);

            var tips = SceneManager.GetModule<TipsComponentGSM>();
            // 一次入包 count 个球，各弹一次对应图集 Key（图上可不写 ×3）
            tips.OpenTipsForm("GetHpBall");
            tips.OpenTipsForm("GetMpBall");
        }

        private void OpenBox(InteractiveComponent component)
        {
            if (enableDebugLog)
            {
                Debug.Log("[VillageKenMuNi1HpMpBox] OpenBox() invoked.", gameObject);
            }

            if (opened)
            {
                return;
            }

            opened = true;
            componentSystem.GetComponent<InteractiveComponent>().entityControll.canTouchWithPlayer =
                !opened;

            // 收掉按键提示（对齐 West / Home2）
            var playerLogic = GameManager.GetGMComponent<EntityComponentGM>()
                .GetEntityLogic<PlayerLogic>();
            if (playerLogic && playerLogic.keyTipsNode && playerLogic.keyTipsNode.activeSelf)
            {
                playerLogic.showKeyTipsNode(false);
            }

            var shouldTriggerStory = useStoryOnOpen && !string.IsNullOrEmpty(storyName);
            if (shouldTriggerStory)
            {
                SceneManager.GetModule<StoryComponentGSM>().TriggerStory(storyName);
            }
            else
            {
                OnOpenBox();
                OnGetHpMp();
            }
        }
    }
}
