using Game.GameMgr;
using Game.GameMgr.Component;
using Game.GameMgr.Component.Archive.ArchiveDataClass.Player;
using Game.GameRuntime.UI.Component.BlackFade;
using Game.GameRuntime.UI.FormLogic.Base;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

namespace Game.GameRuntime.UI.FormLogic.Achievement
{
    public class AchievementFormLogic : BaseUIFormLogic
    {
        [SerializeField] private GameObject AchievementTip;
        [SerializeField] private AchievementItem ItemPrefab;
        [SerializeField] private ScrollRect Achievements;
        [SerializeField] private Slider VSlider;
        [SerializeField] private Image AchievementTipImage;
        [SerializeField] private Image AchievementTipMarker;
        [SerializeField] private Image AchievementInfoImage;
        [SerializeField] private Button btnBack;
        public GameObject textDesc;// 成就描述文本
        public GameObject textDesc_cn;// 成就描述文本中文文本

        public GameObject imgQyaer;

        private SpriteAtlas spriteAtlas;
        [HideInInspector] public SpriteAtlas nameAtlas;
        [HideInInspector] public SpriteAtlas nameAtlas_en;
        [HideInInspector] public SpriteAtlas nameAtlas_jp;

       

        private float contentHeight;
        private float scrollHeight;
        private float viewHeight;
        private Transform content;
        private Transform viewport;

        private readonly List<GameObject> Items = new List<GameObject>();

        Dictionary<LanguageEnumType, string> defualtStrTips = new Dictionary<LanguageEnumType, string>() {
            { LanguageEnumType.Chinese, "成就尚未完成" },
            { LanguageEnumType.English, "Achievement not yet completed" },
            { LanguageEnumType.Japanese, "アチーブメントはまだ未達成です" },
        };

        protected internal override void OnInit(object userData)
        {
            base.OnInit(userData);

            viewport = Achievements.transform.Find("Viewport").transform;
            content = viewport.Find("Content");

            VSlider.onValueChanged.AddListener(value => { content.localPosition = new Vector3(0, value * scrollHeight, 0); });

            Achievements.onValueChanged.AddListener(v => { VSlider.SetValueWithoutNotify(1 - v.y); });
            
            btnBack.onClick.AddListener(OnBtnBack);
            LoadAtlas(4);
        }

        protected override void LoadAtlas(int targetAtlasCount)
        {
            base.LoadAtlas(targetAtlasCount);
            var realPath = "Assets/GameRes/Atlas/YaerQImgAtlas.spriteatlas";
            GameManager.GetGMComponent<ResComponentGM>().LoadAsset<SpriteAtlas>(realPath, atlas =>
            {
                if (atlas == null) { return; }
                if (spriteAtlas != null) { return; }
                spriteAtlas = atlas;
                // 加载好需要的图集后刷新界面
                loadAtlasCallFunc();
            });
            realPath = "Assets/GameRes/Atlas/AchievementPanel/name.spriteatlas";
            GameManager.GetGMComponent<ResComponentGM>().LoadAsset<SpriteAtlas>(realPath, atlas =>
            {
                if (atlas == null) { return; }
                if (nameAtlas != null) { return; }
                nameAtlas = atlas;
                loadAtlasCallFunc();
            });
            realPath = "Assets/GameRes/Atlas/AchievementPanel/name_en.spriteatlas";
            GameManager.GetGMComponent<ResComponentGM>().LoadAsset<SpriteAtlas>(realPath, atlas =>
            {
                if (atlas == null) { return; }
                if (nameAtlas_en != null) { return; }
                nameAtlas_en = atlas;
                loadAtlasCallFunc();
            });
            realPath = "Assets/GameRes/Atlas/AchievementPanel/name_jp.spriteatlas";
            GameManager.GetGMComponent<ResComponentGM>().LoadAsset<SpriteAtlas>(realPath, atlas =>
            {
                if (atlas == null) { return; }
                if (nameAtlas_jp != null) { return; }
                nameAtlas_jp = atlas;
                loadAtlasCallFunc();
            });
        }

        protected internal override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            AchievementTip.SetActive(false);
            textDesc_cn.SetActive(false);
            textDesc.SetActive(false);
            imgQyaer.SetActive(false);
        }

        public override void UpdateUI()
        {
            base.UpdateUI();
            if (AchievementDataMgr.table != null)
            {
                CreateAchievementItems();
            }
            else
            {
                // 加载部分配置
                AchievementDataMgr.getInstance().Init();
                AchievementDataMgr.getInstance().onLoadConfigCallFunc += CreateAchievementItems;
            }
            Invoke("CalculateScrollHeight", 0.1f);

            componentSystemUI.GetComponent<BlackFadeComponent>().HideFade();
            var achieveId = AchievementType.KillSlime_1;
            // 默认选中第一个成就
            OnItemSelected(achieveId, true);
        }

        public void UpdateView(AchievementDataProxy proxy)
        {
        }

        private void CalculateScrollHeight()
        {
            contentHeight = content.GetComponent<RectTransform>().rect.height;
            viewHeight = Achievements.GetComponent<RectTransform>().rect.height -
                         (viewport.GetComponent<RectTransform>().offsetMin.y -
                          viewport.GetComponent<RectTransform>().offsetMax.y);

            scrollHeight = contentHeight - viewHeight;
        }

        private void CreateAchievementItems()
        {
            Clear();
            var achievementCount = AchievementDataMgr.getInstance().GetAchievementCount();
            ItemPrefab.gameObject.SetActive(true);
            for (var i = AchievementType.KillSlime_1; (int)i <= achievementCount; i++)
            {
                var item = Instantiate(ItemPrefab, content);
                item.SetData(i);
                item.OnAchievementItemHover = OnItemHover;
                item.OnAchievementItemSelected = OnItemSelected;
                item.gameObject.SetActive(true);
                Items.Add(item.gameObject);
                if (i == AchievementType.KillSlime_1)
                {
                    item.SetBtnClickState();
                };
            }
            ItemPrefab.gameObject.SetActive(false);
        }

        public void OnItemHover(AchievementType id, bool isActive)
        {
            AchievementTip.SetActive(isActive);
            var hasFinsh = AchievementDataMgr.getInstance().CheckAchievementHasComplete(id);
            var checkMask = UIUtils.findChild(AchievementTip, "Checkmark");
            if (checkMask != null)
            {
                checkMask.SetActive(hasFinsh);
            }
            // 成就提示
            var achieveTipsDesc = AchievementDataMgr.getInstance().GetAchievementTips(id);
            var textTips = UIUtils.findChild(AchievementTip, "textTips");
            var textTips_cn = UIUtils.findChild(AchievementTip, "textTips_cn");
            GameTools.setText(textTips, achieveTipsDesc);
            GameTools.setText(textTips_cn, achieveTipsDesc);
            var languageType = GameManager.Instance.language;
            textTips_cn.SetActive(languageType == LanguageEnumType.Chinese);
            textTips.SetActive(languageType != LanguageEnumType.Chinese);
            //AchievementTipImage.sprite = 
        }

        public void OnItemSelected(AchievementType id, bool isDefualtSelect=false)
        {
            var hasFinsh = AchievementDataMgr.getInstance().CheckAchievementHasComplete(id);
            // 成就描述
            var achieveTipsDesc = AchievementDataMgr.getInstance().GetAchievementDesc(id);
            // 显示成就对应的人物图片
            var tag = AchievementDataMgr.getInstance().GetAchievementYaerTag(id);
            GameTools.loadTextureByAtlas(imgQyaer, spriteAtlas, tag.ToString());
            var languageType = GameManager.Instance.language;
            var unFinshTips = defualtStrTips.ContainsKey(languageType) ? defualtStrTips[languageType] : "";
            var curTextObj = languageType == LanguageEnumType.Chinese ? textDesc_cn : textDesc;
            textDesc_cn.SetActive(languageType == LanguageEnumType.Chinese);
            textDesc.SetActive(languageType != LanguageEnumType.Chinese);
            if (!hasFinsh)
            {
                GameTools.setText(curTextObj, unFinshTips);
            }
            else
            {
                GameTools.setText(curTextObj, achieveTipsDesc);
            }
            imgQyaer.SetActive(hasFinsh);
            if (!isDefualtSelect)
            {
                // 播放音效
                UIUtils.PlayTapExChangeSfx(this);
            }
        }

        private void Clear()
        {
            for (var i = Items.Count - 1; i >= 0; i--) Destroy(Items[i]);

            Items.Clear();
        }

        private void OnBtnBack()
        {
            componentSystemUI.GetComponent<BlackFadeComponent>().CloseFormShowFade(UIForm);

            UIUtils.PlayBtnAudio(this);
        }

        public override void CloseFormOnEsc()
        {
            OnBtnBack();
        }

        protected internal override void OnClose(bool isShutdown, object userData)
        {
            base.OnClose(isShutdown, userData);
            AchievementDataMgr.getInstance().onLoadConfigCallFunc -= CreateAchievementItems;
        }
    }
}