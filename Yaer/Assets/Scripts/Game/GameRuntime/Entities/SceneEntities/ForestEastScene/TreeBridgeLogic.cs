using DG.Tweening;
using Game.GameMgr.Component;
using Game.GameMgr.Component.Archive.ArchiveDataClass.Scene;
using Game.GameRuntime.Entities.Base.BaseSceneObj;
using Game.GameRuntime.Entities.Component.Anima;
using Game.GameRuntime.Entities.Component.Interactive;
using Game.GameRuntime.Entities.Monster.WoodWorm;
using Game.GameRuntime.Entities.Monster.WormEgg;
using Game.GameRuntime.Entities.SceneEntities.HomeScene2;
using System.Collections.Generic;
using UnityEngine;



#if UNITY_EDITOR
#endif

namespace Game.GameRuntime.Entities.SceneEntities.ForestEastScene
{
    public class TreeBridgeLogic : BaseSceneEntityLogic
    {
        [SerializeField]
        private SpriteRenderer OuterSprite;
        [SerializeField]
        private float OuterSpriteFadeTime;

        private Animator animator;
        [SerializeField]
        private List<GameObject> AttachedGameObject;
        public List<GameObject> hideObjsInEnterTreeBridge; // 进入树洞后需要隐藏的对象

        public GameObject enterNodeLeft;
        public GameObject enterNodeRight;
        public GameObject outNodeLeft;
        public GameObject outNodeRight;
        public GameObject newCameraBoundingArea;
        public GameObject oldCameraBoundingArea;

        public GameObject storyTriggerEnterNodeLeft;
        public GameObject storyTriggerEnterNodeRight;
        public GameObject storyTriggerOutNodeLeft;
        public GameObject storyTriggerOutNodeRight;

        public GameObject spcWormEgg; // 特殊的蠕虫蛋
        public GameObject eggStoryTrigger; // 蠕虫蛋故事触发器

        public GameObject removeAfterCollider; // 倒树移除后的碰撞体
        public GameObject passTreeBridgeSecretTrigger; // 通过树洞之后的彩蛋触发区域
        public SoundToggleComponent soundSfxCpn;
        public AnimationEventComponent aniEventCpn;

        public BaseSoundEntity waterSoundEntity; // 场景中的瀑布声

        public List<WoodWormLogic> storyWoodWormLogicList = new List<WoodWormLogic>(); // 一开始朝人物移动的虫子

        protected internal override void OnInit(object userData)
        {
            base.OnInit(userData);
            
            if (!CheckFall())
            {
                ForestEastTreeBridgeStoryMgr.getInstance().storyLogic = this;
                animator = GetComponent<Animator>();
                componentSystem.GetComponent<InteractiveComponent>().onEnterInteractiveEvent += (x) => OuterSpriteFade(0);
                componentSystem.GetComponent<InteractiveComponent>().onExitInteractiveEvent += (x) => OuterSpriteFade(1);
                aniEventCpn.RegisterEvent("AfterFallDown", AfterFallDown);
            }
        }

        private void Update()
        {
            if (spcWormEgg != null && spcWormEgg.GetComponent<WormEggLogic>().IsDead
                && !eggStoryTrigger.activeSelf)
            {
                // 特殊虫蛋死亡后，出现剧情
                eggStoryTrigger.SetActive(true);
            }
        }

        private void OuterSpriteFade(float endvalue)
        {
            OuterSprite.DOKill();
            OuterSprite.DOFade(endvalue, OuterSpriteFadeTime);
        }

        public void Fall()
        {
            animator.SetTrigger("Fall");
            foreach (GameObject go in AttachedGameObject)
            {
                go.SetActive(false);
            }
        }

        public bool CheckFall()
        {
            bool TreeBridgeFall = SceneManager.GetArchiveData<ForestEastSceneData>().TreeBridgeFall;
            ForestEastTreeBridgeStoryMgr.getInstance().hasPassEvent = TreeBridgeFall;
            removeAfterCollider.SetActive(TreeBridgeFall);
            passTreeBridgeSecretTrigger.SetActive(TreeBridgeFall);
            if (TreeBridgeFall)
            {
                Destroy(this.gameObject);
                foreach (GameObject go in AttachedGameObject)
                {
                    Destroy(go);
                }
                return true;
            }
            return false;
        }

        public void PlayTreeBridgeMoveSfx()
        {
            var moveSfxName = "木头嘎吱嘎吱声 .mp3";
            soundSfxCpn.ChangeSoundRes(moveSfxName);
            soundSfxCpn.PlaySound();
        }

        void AfterFallDown(string arg)
        {
            // 播放树掉进水中的音效
            var moveSfxName = "树掉进水里的声音.mp3";
            soundSfxCpn.ChangeSoundRes(moveSfxName);
            GameActionMgr.runDelayTimeAction(3f, () =>
            {
                soundSfxCpn.PlaySound();
            });

        }
    }
}

