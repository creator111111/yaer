using DG.Tweening;
using Game.GameMgr.Component;
using Game.GameMgr.Component.Archive.ArchiveDataClass.Scene;
using Game.GameRuntime.Entities.Base.BaseSceneObj;
using Game.GameRuntime.Entities.Component.Anima;
using Game.GameRuntime.Entities.Component.Interactive;
using Game.GameRuntime.Entities.Monster.WoodWorm;
using Game.GameRuntime.Entities.Monster.WormEgg;
using Game.GameRuntime.Entities.SceneEntities.HomeScene2;
using System.Collections;
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
        public List<GameObject> hideObjsInEnterTreeBridge; // ?????????????????????

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

        public GameObject spcWormEgg; // ?????????
        public GameObject eggStoryTrigger; // ?????????????

        public GameObject removeAfterCollider; // ???????????????
        public GameObject passTreeBridgeSecretTrigger; // ?????????????????????
        public SoundToggleComponent soundSfxCpn;
        public AnimationEventComponent aniEventCpn;

        public BaseSoundEntity waterSoundEntity; // ?????????????

        public List<WoodWormLogic> storyWoodWormLogicList = new List<WoodWormLogic>(); // ??????????????????

        [Header("???????")]
        [SerializeField]
        [Range(0f, 1f)]
        [Tooltip("???????? BaseSoundEntity ?????????????????????1 ??????0.5 ?????")]
        private float waterSoundVolumeScale = 0.45f;

        /// <summary>?��????????????????????????��?????????</summary>
        private bool m_skipWaterVolumeTweak;

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
            else
            {
                m_skipWaterVolumeTweak = true;
            }
        }

        /// <summary>
        /// ?? <see cref="BaseSoundEntity"/> ?? Start ????? baseVolume ??????????��?????
        /// </summary>
        private IEnumerator Start()
        {
            if (m_skipWaterVolumeTweak || waterSoundEntity == null)
            {
                yield break;
            }

            yield return null;
            waterSoundEntity.ApplyVolumeMultiplier(waterSoundVolumeScale);
        }

        private void Update()
        {
            if (spcWormEgg != null && spcWormEgg.GetComponent<WormEggLogic>().IsDead
                && !eggStoryTrigger.activeSelf)
            {
                // ??????????????????
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
            var moveSfxName = "ľͷ��֨��֨�� .mp3";
            soundSfxCpn.ChangeSoundRes(moveSfxName);
            soundSfxCpn.PlaySound();
        }

        void AfterFallDown(string arg)
        {
            // ����������ˮ�е���Ч
            var moveSfxName = "������ˮ�������.mp3";
            soundSfxCpn.ChangeSoundRes(moveSfxName);
            GameActionMgr.runDelayTimeAction(3f, () =>
            {
                soundSfxCpn.PlaySound();
            });

        }
    }
}

