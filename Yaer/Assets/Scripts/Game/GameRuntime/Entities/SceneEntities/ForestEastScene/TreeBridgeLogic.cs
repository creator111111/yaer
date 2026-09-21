using DG.Tweening;
using Game.GameMgr.Component;
using Game.GameMgr.Component.Archive.ArchiveDataClass.Scene;
using Game.GameRuntime.Entities.Base.BaseSceneObj;
using Game.GameRuntime.Entities.Component.Anima;
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
                // 外壳「外」淡出改绑 ChangeCamera（切镜完成后），不再在 Interactive 靠近时 Fade。
                // 原因：产品要求走近倒树保持不透明；进洞/出洞/读档洞内统一走 ChangeCamera 末尾 Fade。
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

        /// <summary>
        /// 淡出/淡入倒树外壳「外」。由 <c>ChangeCamera</c> 在切镜写完后调用：进洞 endvalue=0，出洞=1。
        /// 替代方案：揭黑幕后再淡（方案 A+B）——读档无黑幕须仍在 ChangeCamera 调一次。
        /// </summary>
        public void OuterSpriteFade(float endvalue)
        {
            if (OuterSprite == null)
            {
                Debug.LogWarning("[TreeBridgeOuter] OuterSprite is null, skip fade");
                return;
            }
            OuterSprite.DOKill();
            OuterSprite.DOFade(endvalue, OuterSpriteFadeTime);
        }

        /// <summary>
        /// Pass 对白图调用：播倒下动画并关掉挂件。
        /// 必须跳过 null：合层换遮罩后 Attached 槽曾变 Missing，对 null SetActive 会 Unassigned，
        /// 掐断 Pass 链（「好险…」等台词播不到）。遮罩应绑 <c>遮罩只影响人物</c>，勿只靠跳过不绑。
        /// </summary>
        public void Fall()
        {
            animator.SetTrigger("Fall");
            int skipped = 0;
            foreach (GameObject go in AttachedGameObject)
            {
                if (go == null)
                {
                    skipped++;
                    continue;
                }
                go.SetActive(false);
            }
            if (skipped > 0)
            {
                Debug.LogWarning($"[TreeBridgeFall] Fall skipped {skipped} null AttachedGameObject entry(ies)");
            }
        }

        /// <summary>
        /// 读档已倒下：销毁倒树与挂件。同样必须跳过 null，否则读档也会在 Destroy(null) 处炸。
        /// </summary>
        public bool CheckFall()
        {
            bool TreeBridgeFall = SceneManager.GetArchiveData<ForestEastSceneData>().TreeBridgeFall;
            ForestEastTreeBridgeStoryMgr.getInstance().hasPassEvent = TreeBridgeFall;
            removeAfterCollider.SetActive(TreeBridgeFall);
            passTreeBridgeSecretTrigger.SetActive(TreeBridgeFall);
            if (TreeBridgeFall)
            {
                Destroy(this.gameObject);
                int skipped = 0;
                foreach (GameObject go in AttachedGameObject)
                {
                    if (go == null)
                    {
                        skipped++;
                        continue;
                    }
                    Destroy(go);
                }
                if (skipped > 0)
                {
                    Debug.LogWarning($"[TreeBridgeFall] CheckFall skipped {skipped} null AttachedGameObject entry(ies)");
                }
                return true;
            }
            return false;
        }

        public void PlayTreeBridgeMoveSfx()
        {
            // Exact SFX file name (space before .mp3). See ForestEastScene music/SFX tech doc §5.
            var moveSfxName = "木头嘎吱嘎吱声 .mp3";
            soundSfxCpn.ChangeSoundRes(moveSfxName);
            soundSfxCpn.PlaySound();
        }

        void AfterFallDown(string arg)
        {
            // Tree fall into water SFX; delayed ~3s per tech doc §5.
            var moveSfxName = "树掉进水里的声音.mp3";
            soundSfxCpn.ChangeSoundRes(moveSfxName);
            GameActionMgr.runDelayTimeAction(3f, () =>
            {
                soundSfxCpn.PlaySound();
            });

        }

    }
}

