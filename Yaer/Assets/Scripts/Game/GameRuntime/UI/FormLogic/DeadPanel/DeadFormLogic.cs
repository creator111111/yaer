using DG.Tweening;
using Game.GameMgr;
using Game.GameMgr.Component;
using Game.GameMgr.Component.Archive;
using Game.GameMgr.Component.Archive.ArchiveDataClass.Player;
using Game.GameRuntime.GameSceneManager.Component;
using Game.GameRuntime.UI.Component.BlackFade;
using Game.GameRuntime.UI.FormLogic.Base;
using Game.Static.Name.Clothes;
using Game.Static.Path.Sound;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

namespace Game.GameRuntime.UI.FormLogic
{
    public class DeadFormLogic : BaseUIFormLogic
    {
        [SerializeField]
        private GameObject bg_noHeadWear;
        [SerializeField]
        private GameObject bg_crown;
        [SerializeField]
        private GameObject bg_armorHead;
        [SerializeField]
        private GameObject TalksGO;
        [SerializeField]
        private RectTransform TalkContentRtf;
        [SerializeField]
        private GameObject LoadSave;
        [SerializeField]
        private Button YesBtn;
        [SerializeField]
        private Button NoBtn;
        [SerializeField]
        private Button YesBtn_en;
        [SerializeField]
        private Button NoBtn_en;
        [SerializeField]
        private float TalkShowDuration;
        [SerializeField]
        private float TalkShowDeltaTime;
        [SerializeField]
        private float GrayScaleDuration;

        private Button skipBtn;

        private List<Image> talks;
        private Sequence talkSeq;

        private float GrayScale;
        private Material GrayScaleMat;

        private DeadPanelProxy proxy;
        private ArchiveComponentGM archiveComponentGM;

        public GameObject imgBg;
        public GameObject talksArea;
        public GameObject imgTitle;
        public GameObject tipsArea;
        public GameObject blackMask;
        public GameObject maskPanel;
        public GameObject titleSkipArea;


        public GameObject imgLoadTips;
        SpriteAtlas spriteAtlas;
        SpriteAtlas spriteAtlas_en;
        SpriteAtlas spriteAtlas_jp;

        protected internal override void OnInit(object userData)
        {
            base.OnInit(userData);
            skipBtn = transform.Find("Bg").GetComponent<Button>();
            skipBtn.onClick.AddListener(SkipTalkAnimation);
            talks = new List<Image>(TalkContentRtf.childCount);
            for (int i = 0; i< TalkContentRtf.childCount; i++)
            {
                talks.Add(TalkContentRtf.GetChild(i).GetComponent<Image>());
            }
            YesBtn.onClick.AddListener(LoadLastSave);
            NoBtn.onClick.AddListener(ReturnToMainMenu);
            YesBtn_en.onClick.AddListener(LoadLastSave);
            NoBtn_en.onClick.AddListener(ReturnToMainMenu);
            proxy = GetProxy<DeadPanelProxy>();
            archiveComponentGM = GameManager.GetGMComponent<ArchiveComponentGM>();

            BuildTalkAnimation();

            GrayScaleMat = bg_noHeadWear.GetComponent<Image>().material;
            LoadAtlas(3);
        }
        protected override void LoadAtlas(int targetAtlasCount)
        {
            base.LoadAtlas(targetAtlasCount);
            var path = "Assets/GameRes/Atlas/DeadPanel/content.spriteatlas";
            GameManager.GetGMComponent<ResComponentGM>().LoadAsset<SpriteAtlas>(path, atlas =>
            {
                if (atlas == null) { return; }
                if (spriteAtlas != null) { return; }
                spriteAtlas = atlas;
                loadAtlasCallFunc();
            });
            path = "Assets/GameRes/Atlas/DeadPanel/content_en.spriteatlas";
            GameManager.GetGMComponent<ResComponentGM>().LoadAsset<SpriteAtlas>(path, atlas =>
            {
                if (atlas == null) { return; }
                if (spriteAtlas_en != null) { return; }
                spriteAtlas_en = atlas;
                loadAtlasCallFunc();
            });
            path = "Assets/GameRes/Atlas/DeadPanel/content_jp.spriteatlas";
            GameManager.GetGMComponent<ResComponentGM>().LoadAsset<SpriteAtlas>(path, atlas =>
            {
                if (atlas == null) { return; }
                if (spriteAtlas_jp != null) { return; }
                spriteAtlas_jp = atlas;
                loadAtlasCallFunc();
            });
        }

        public override void UpdateUI()
        {
            base.UpdateUI();

            Dictionary<LanguageEnumType, SpriteAtlas> spriteAtlasData = new Dictionary<LanguageEnumType, SpriteAtlas>() {
                { LanguageEnumType.Chinese, spriteAtlas }, {  LanguageEnumType.English, spriteAtlas_en },
                {  LanguageEnumType.Japanese, spriteAtlas_jp },
            };

            var curLaunageType = GameManager.Instance.language;
            SpriteAtlas mySpriteAtlas;
            if (!spriteAtlasData.ContainsKey(curLaunageType))
            {
                // 不存在的语言一律使用英文
                mySpriteAtlas = spriteAtlas_en;
            }
            else
            {
                mySpriteAtlas = spriteAtlasData[curLaunageType];
            }
            bool isChinese = curLaunageType == LanguageEnumType.Chinese;
            YesBtn.gameObject.SetActive(isChinese);
            NoBtn.gameObject.SetActive(isChinese);
            YesBtn_en.gameObject.SetActive(!isChinese);
            NoBtn_en.gameObject.SetActive(!isChinese);

            // 设置各种图片
            GameTools.loadTextureByAtlas(imgTitle, mySpriteAtlas, "gameEndTitle");
            GameTools.loadTextureByAtlas(imgLoadTips, mySpriteAtlas, "要从上一个存档加载吗");
            var index = 1;
            foreach(var obj in talks)
            {
                GameTools.loadTextureByAtlas(obj.gameObject, mySpriteAtlas, index.ToString());
                index++;
            }

        }
        protected internal override void OnOpen(object userData)
        {
            base.OnOpen(userData);

            GameManager.GetGMComponent<SoundComponentGM>().PlaySound(SoundType.BGM, "死亡音乐.mp3", false);
            // 先播放一个进入死亡界面的背景音，然后延时播放死亡界面持续音乐
            GameActionMgr.runDelayTimeAction(10, () =>
            {
                GameManager.GetGMComponent<SoundComponentGM>().PlaySound(SoundType.BGM, "龙宫环境音.mp3", true);
            }, gameObject);
            HideUI();

            GameActionMgr.runFadeAction(maskPanel, 1, 2);
            // 显示标题
            List<Tween> tweens = new List<Tween>() {
                    GameActionMgr.runFadeAction(imgTitle, 1, 4f),
                    GameActionMgr.runFadeAction(imgTitle, 0, 4f, 2f),
                };
            var seqAct = GameActionMgr.runSequenceAction(imgTitle, tweens);
            seqAct.onComplete = () =>
            {
                imgTitle.SetActive(false);
                maskPanel.SetActive(false);
                titleSkipArea.SetActive(false);
                ShowTalkEffect();
            };

            GameTools.setObjectClickFunc(titleSkipArea, () =>
            {
                DOTween.Kill(imgTitle, true);
                DOTween.Kill(seqAct, true);
                // 跳过标题动画后立刻切换音乐
                GameManager.GetGMComponent<SoundComponentGM>().PlaySound(SoundType.BGM, "龙宫环境音.mp3", true);
            });
        }

        void ShowTalkEffect()
        {
            imgBg.SetActive(true);
            talksArea.SetActive(true);
            tipsArea.SetActive(true);
            blackMask.SetActive(true);
            RefreshBG();
            PrepareShowTalk();
            componentSystemUI.GetComponent<BlackFadeComponent>().HideFade(ShowTalk);
        }

        void HideUI()
        {
            imgBg.SetActive(false);
            talksArea.SetActive(false);
            tipsArea.SetActive(false);
            blackMask.SetActive(false);
            imgTitle.SetActive(true);
            maskPanel.SetActive(true);
            titleSkipArea.SetActive(true);
            imgTitle.GetComponent<CanvasGroup>().alpha = 0;
            maskPanel.GetComponent<CanvasGroup>().alpha = 0;
            
        }

        private void BuildTalkAnimation()
        {
            talkSeq = DOTween.Sequence();
            foreach (var talk in talks)
            {
                talkSeq.Append(talk.DOFade(1, TalkShowDuration));
                talkSeq.AppendInterval(TalkShowDeltaTime);
            }
            talkSeq.Append(DOTween.To(() => GrayScale, x => RefreshGrayScaleMat(x), 1, GrayScaleDuration));
            talkSeq.onComplete += ShowLoadSave;
            talkSeq.SetAutoKill(false);
            talkSeq.Pause();
        }

        private void RefreshBG()
        {
            var clothesData = GameManager.GetGameSceneManager().GetArchiveData<PlayerClothesData>();
            string headWear = clothesData.GetClothesName(BoneName.Headwear);
            bg_noHeadWear.SetActive(headWear == ClothesName.HeadWear.NoHeadWear);
            bg_crown.SetActive(headWear == ClothesName.HeadWear.Crown);
            bg_armorHead.SetActive(headWear == ClothesName.HeadWear.ArmorHead);
        }

        private void PrepareShowTalk()
        {
            Color hideColor = new Color(1, 1, 1, 0);
            LoadSave.gameObject.SetActive(false);
            foreach (var talk in talks)
            {
                talk.color = hideColor;
            }
            RefreshGrayScaleMat(0);
        }

        private void ShowTalk()
        {
            talkSeq.Restart();
        }

        private void ShowLoadSave()
        {
            LoadSave.SetActive(true);
        }

        private void SkipTalkAnimation()
        {
            talkSeq.Pause();
            LoadSave.gameObject.SetActive(true);
            foreach (var talk in talks)
            {
                talk.color = Color.white;
            }
            RefreshGrayScaleMat(1);
        }

        private void LoadLastSave()
        {
            var archiveinfo = archiveComponentGM.GetNowArchiveInfo();
            proxy.LoadArchive(archiveinfo.guid);

            UIUtils.PlayBtnAudio(this);
        }

        private void ReturnToMainMenu()
        {
            proxy.OnReturnMainMenu();

            UIUtils.PlayBtnAudio(this);
        }

        private void RefreshGrayScaleMat(float newValue = -1)
        {
            if (newValue >= 0 && newValue <= 1)
            {
                GrayScale = newValue;
            }
            GrayScaleMat.SetFloat("_GrayScale", GrayScale);
        }

        public override void PlayerOpenAudio()
        {

        }
    }
}