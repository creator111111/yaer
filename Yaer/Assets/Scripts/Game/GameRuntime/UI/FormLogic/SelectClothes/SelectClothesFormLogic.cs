using System;
using System.Collections.Generic;
using Game.GameMgr;
using Game.GameMgr.Component;
using Game.GameMgr.Component.Archive.ArchiveDataClass.Player;
using Game.GameMgr.Component.Archive.ArchiveDataClass.Scene;
using Game.GameMgr.Component.UI;
using Game.GameRuntime.GameSceneManager.Component;
using Game.GameRuntime.GameSceneManager.Component.Story;
using Game.GameRuntime.UI.FormLogic.Base;
using Game.GameRuntime.UI.FormLogic.SystemTips;
using Game.Static.Enum.Goods;
using Game.Static.Name.Clothes;
using Game.Static.Name.Res;
using Game.Static.Name.Settings;
using Game.Static.Path;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

namespace Game.GameRuntime.UI.FormLogic.SelectClothes
{
    public class SelectClothesFormLogic : BaseUIFormLogic
    {
        [Header("SelectClothesPanel")] [SerializeField]
        private Button btnClothes;

        [SerializeField] private Button btnBra;
        [SerializeField] private Button btnUnderwear;
        [SerializeField] private Button btnTrousers;
        [SerializeField] private Button btnShoes;
        [SerializeField] private Button btnHeadWear;
        [SerializeField] private Button btnWeapon;
        [SerializeField] private Button btnBack;
        [SerializeField] private ScrollRect scrollRect;
        public SoundToggleComponent soundSfxCpn;

        [Header("控件预设体")] [SerializeField] private GameObject prefabs;

        [SerializeField] private Animator animator;
        
        private string selectBoneName;
        public int removeHeadWearTimes;
        
        private SelectClothesFormProxy proxy;
        private StoryComponentGSM storyComponentGSM;
        
        private List<Toggle> togglesList = new List<Toggle>();

        public Button returnBtn;
        public GameObject mainTabArea; // 主页签区域
        public GameObject subTabArea; // 子页签区域

        public GameObject imgClothes;
        public GameObject imgBra;
        public GameObject imgUnderwear;
        public GameObject imgTrousers;
        public GameObject imgShoes;
        public GameObject imgHeadWear;
        public GameObject imgWeapon;
        
        SpriteAtlas spriteAtlas;
        SpriteAtlas spriteAtlas_en;
        SpriteAtlas spriteAtlas_jp;
        SpriteAtlas returnBtnAtlas;

        protected internal override void OnInit(object userData)
        {
            base.OnInit(userData);

            btnClothes.onClick.AddListener(() => OnClickClothesBtn(BoneName.Clothes));
            btnBra.onClick.AddListener(() => OnClickClothesBtn(BoneName.Bra));
            btnUnderwear.onClick.AddListener(() => OnClickClothesBtn(BoneName.Underwear));
            btnTrousers.onClick.AddListener(() => OnClickClothesBtn(BoneName.Trousers));
            btnShoes.onClick.AddListener(() => OnClickClothesBtn(BoneName.Shoes));
            btnHeadWear.onClick.AddListener(() => OnClickClothesBtn(BoneName.Headwear));
            btnWeapon.onClick.AddListener(() => OnClickClothesBtn(BoneName.Weapon));
            btnBack.onClick.AddListener(Exit);
            returnBtn.onClick.AddListener(() =>
            {
                // 隐藏子页签，显示主页签区域
                ShowTabArea(true);
            });

            proxy = GetProxy<SelectClothesFormProxy>();
            proxy.onUpdateClothesNamesForBones = UpdatedClothesList;

            LoadAtlas(4);
        }

        protected override void LoadAtlas(int targetAtlasCount)
        {
            base.LoadAtlas(targetAtlasCount);
            var path = "Assets/GameRes/Atlas/SelectClothesPanel/text.spriteatlas";
            GameManager.GetGMComponent<ResComponentGM>().LoadAsset<SpriteAtlas>(path, atlas =>
            {
                if (atlas == null) { return; }
                if (spriteAtlas != null) { return; }
                spriteAtlas = atlas;
                loadAtlasCallFunc();
            });
            path = "Assets/GameRes/Atlas/SelectClothesPanel/text_en.spriteatlas";
            GameManager.GetGMComponent<ResComponentGM>().LoadAsset<SpriteAtlas>(path, atlas =>
            {
                if (atlas == null) { return; }
                if (spriteAtlas_en != null) { return; }
                spriteAtlas_en = atlas;
                loadAtlasCallFunc();
            });
            path = "Assets/GameRes/Atlas/SelectClothesPanel/text_jp.spriteatlas";
            GameManager.GetGMComponent<ResComponentGM>().LoadAsset<SpriteAtlas>(path, atlas =>
            {
                if (atlas == null) { return; }
                if (spriteAtlas_jp != null) { return; }
                spriteAtlas_jp = atlas;
                loadAtlasCallFunc();
            });
            path = "Assets/GameRes/Atlas/CommonBtn/returnBtnAtlas.spriteatlas";
            GameManager.GetGMComponent<ResComponentGM>().LoadAsset<SpriteAtlas>(path, atlas =>
            {
                if (atlas == null) { return; }
                if (returnBtnAtlas != null) { return; }
                returnBtnAtlas = atlas;
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
            // 设置各种图片多语言版本
            GameTools.loadTextureByAtlas(imgClothes, mySpriteAtlas, "衣服");
            GameTools.loadTextureByAtlas(imgBra, mySpriteAtlas, "文胸");
            GameTools.loadTextureByAtlas(imgUnderwear, mySpriteAtlas, "内裤");
            GameTools.loadTextureByAtlas(imgTrousers, mySpriteAtlas, "丝袜");
            GameTools.loadTextureByAtlas(imgShoes, mySpriteAtlas, "鞋");
            GameTools.loadTextureByAtlas(imgHeadWear, mySpriteAtlas, "头饰");
            GameTools.loadTextureByAtlas(imgWeapon, mySpriteAtlas, "武器");


            var curResTag = GameManager.GetCurLanguageResTag();
            if (curLaunageType == LanguageEnumType.Japanese)
            {
                // 日语用英文的资源
                curResTag = LanguageType.GetLanaguageResTag(LanguageEnumType.English);
            }
            var norResName = "returnNor" + curResTag;
            var clickResName = "returnClick" + curResTag;
            var selectResName = "returnSelect" + curResTag;
            var norResSprite = returnBtnAtlas.GetSprite(norResName);
            var clickResSprite = returnBtnAtlas.GetSprite(clickResName);
            var selectResSprite = returnBtnAtlas.GetSprite(selectResName);
            GameTools.loadBtnSprite(returnBtn, norResSprite, selectResSprite, clickResSprite);

        }

        protected internal override void OnOpen(object userData)
        {
            base.OnOpen(userData);

            // 重新刷新proxy
            proxy = GetProxy<SelectClothesFormProxy>();
            storyComponentGSM = GameManager.GetGameSceneManager().GetModule<StoryComponentGSM>();
            if (GameManager.GetGameSceneManager().GetArchiveData<PlayerBagData>().HasMainItem(EMainItemName.AiLinSword.ToString()))
            {
                // 如果拾取了剑则进入换装之后自动装备剑
                ChangeClothes(BoneName.Weapon, ClothesName.Weapon.AiLinSword);
            }
            animator.Rebind();
            ShowTabArea(true);
            //btnClothes.onClick.Invoke();
            AllowOpenMenu(false);
        }

        protected internal override void OnClose(bool isShutdown, object userData)
        {
            base.OnClose(isShutdown, userData);
            AllowOpenMenu(true);
        }

        private void OnClickClothesBtn(string clothesName)
        {
            PlayChangeTapSfx();
            selectBoneName = clothesName;
            proxy.GetAllClothesNamesForBones(clothesName);
            // 进入子页签
            ShowTabArea(false);
        }

        /// <summary>
        ///     点击按钮更新ScrollView的内容
        /// </summary>
        private void UpdatedClothesList(Dictionary<string, string> data)
        {
            // 清空原来内容
            foreach (var tg in togglesList)
            {
                Destroy(tg.gameObject);
            }
            
            togglesList.Clear();
            
            foreach (var p in data)
            {
                // 跳过NoBra
                if (p.Key == ClothesName.Bra.NoBra || p.Key == ClothesName.Underwear.NoUnderwear) continue;

                // 根据衣服数据生成Toggle
                var toggleClothes = Instantiate(prefabs).GetComponent<ToggleClothes>();
                toggleClothes.gameObject.SetActive(true);
                toggleClothes.txClothesName.text = p.Value;

                var toggle = toggleClothes.GetComponent<Toggle>();
                toggle.transform.SetParent(scrollRect.content);
                toggle.transform.localScale = Vector3.one;

                // 设置ToggleGroup
                toggle.group = scrollRect.content.GetComponent<ToggleGroup>();

                // 添加事件
                toggle.onValueChanged.AddListener(isOn =>
                {
                    if (isOn) { 
                        PlayEnterClothesSfx();
                        ChangeClothes(selectBoneName, p.Key);
                    }
                });
                
                toggle.gameObject.SetActive(true);
                
                togglesList.Add(toggle);
            }
        }

        private void ChangeClothes(string boneName, string clothesName)
        {
            if (boneName == BoneName.Headwear) TriggerCrownDialogue(clothesName);

            proxy.ChangingClothes(boneName, clothesName);
        }

        public override void CloseFormOnEsc()
        {
            Exit();
        }

        private void Exit()
        {
            // 检查
            var wearingClothesDic = proxy.GetWearingClothesData();

            // 正确搭配
            if (wearingClothesDic[BoneName.Clothes] == ClothesName.Clothes.Armor
                && wearingClothesDic[BoneName.Trousers] == ClothesName.Trousers.ArmorTrousers
                && wearingClothesDic[BoneName.Shoes] == ClothesName.Shoes.ArmorShoes
               )
            {
                GameManager.GetGMComponent<UIComponentGM>().OpenUIForm(UIPrefabPath.GetUIPrefabPath("SystemTipsPanel"), EUIGroup.Top, new OpenFormArgs() {
                    userData = ESystemTipsType.LeavingBedroom,
                    callBack = logic => {

                        if (logic is SystemTipsFormLogic systemTipsFormLogic) {
                            systemTipsFormLogic.GetProxy<SystemTipsFormProxy>().UpdateTips(ESystemTipsType.LeavingBedroom);
                            systemTipsFormLogic.GetProxy<SystemTipsFormProxy>().onSureEvent = () => {
                                // 保存数据
                                proxy.SaveWearingClothes();
                                // 退出
                                GameManager.GetGameSceneManager().GetModule<LoadSceneComponentGSM>().LoadScene(SceneName.HomeScene2, CloseForm);

                            };
                            systemTipsFormLogic.GetProxy<SystemTipsFormProxy>().onCancelEvent = null;
                        }
                    }
                });
				return;
			}

            // 禁止穿内衣出去
            if (wearingClothesDic[BoneName.Clothes] == ClothesName.Clothes.NoClothes)
            {
                // 提示信息
                storyComponentGSM.TriggerStory("ChangeClothesSceneWearUnderwearExit");
            }
            else
            {
                // 错误搭配提醒
                storyComponentGSM.TriggerStory("ChangeClothesSceneErrorClothingMatching");
            }
        }

        public SelectClothesSceneData sData { get; private set; }
        public bool pEggEqual3
        {
            get => sData.pEggEqual3;
            set => sData.pEggEqual3 = value;
        }
        public bool pEggMore3
        {
            get => sData.pEggMore3;
            set => sData.pEggMore3 = value;
        }

        /// <summary>
        ///     换装对话
        /// </summary>
        private void TriggerCrownDialogue(string headWear)
        {
            var wearingClothesDic = proxy.GetWearingClothesData();

            // 换上皇冠
            if (headWear == ClothesName.HeadWear.Crown &&
                wearingClothesDic[BoneName.Headwear] != ClothesName.HeadWear.Crown)
            {
                storyComponentGSM.TriggerStory("ChangeClothesScenePutOnCrown");
                return;
            }

            // 想要换下皇冠
            if (wearingClothesDic[BoneName.Headwear] == ClothesName.HeadWear.Crown &&
                headWear != ClothesName.HeadWear.Crown)
            {
                // 皇冠被换下
                removeHeadWearTimes++;
                sData = GameManager.GetGameSceneManager().GetArchiveData<SelectClothesSceneData>();
                storyComponentGSM.TriggerStory("ChangeClothesSceneRemoveCrown");
                /*

                if (removeHeadWearTimes == 3 && sData.pEggEqual3 == false)
                {
                    storyComponentGSM.TriggerStory(StoryPrefabPath.GetPath("ChangeClothesSceneRemoveCrown3Times"));

                    sData.pEggEqual3 = true;
                }
                else if (removeHeadWearTimes > 3 && sData.pEggMore3 == false)
                {
                    storyComponentGSM.TriggerStory(StoryPrefabPath.GetPath("ChangeClothesSceneRemoveCrownMore"));
                    removeHeadWearTimes = 0;

                    sData.pEggMore3 = true;
                }
                else
                {
                    
                }*/
            }
        }


        void PlayChangeTapSfx()
        {
            var sfxPath = "翻页的声音.wav";
            soundSfxCpn.ChangeSoundRes(sfxPath);
            soundSfxCpn.PlaySound();
        }

        void PlayEnterClothesSfx()
        {
            var sfxPath = "确认.mp3";
            soundSfxCpn.ChangeSoundRes(sfxPath);
            soundSfxCpn.PlaySound();
        }
        public override void PlayerOpenAudio()
        {

        }

        void ShowTabArea(bool showMainArea = true)
        {
            mainTabArea.SetActive(showMainArea);
            subTabArea.SetActive(!showMainArea);
        }
    }
}