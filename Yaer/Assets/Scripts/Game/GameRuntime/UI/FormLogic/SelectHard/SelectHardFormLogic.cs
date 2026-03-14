using Game.GameMgr;
using Game.GameMgr.Component;
using Game.GameMgr.Component.UI;
using Game.GameRuntime.UI.Component;
using Game.GameRuntime.UI.Component.BlackFade;
using Game.GameRuntime.UI.Control;
using Game.GameRuntime.UI.FormLogic.Base;
using Game.Static.Enum;
using Game.Static.Path;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

namespace Game.GameRuntime.UI.FormLogic.SelectHard
{
    public class SelectHardFormLogic : BaseUIFormLogic
    {
        [SerializeField] private UIListener btnEasy;
        [SerializeField] private UIListener btnNormal;
        [SerializeField] private UIListener btnHard;
        [SerializeField] private UIListener btnHardest;

        public GameObject imgBtnEasy;
        public GameObject imgBtnNormal;
        public GameObject imgBtnHard;
        public GameObject imgBtnHardest;
        public GameObject imgBtnEasyPressed;
        public GameObject imgBtnNormalPressed;
        public GameObject imgBtnHardPressed;
        public GameObject imgBtnHardestPressed;
        public GameObject imgTextEasy;
        public GameObject imgTextNormal;
        public GameObject imgTextHard;
        public GameObject imgTextHardest;

        SpriteAtlas spriteAtlas;
        SpriteAtlas spriteAtlas_en;
        SpriteAtlas spriteAtlas_jp;

        protected internal override void OnInit(object userData)
        {
            base.OnInit(userData);

            GetProxy<SelectHardFormProxy>();

            btnEasy.OnPressed += listener => OnClickHandle(listener, EGameHard.Easy);
            btnNormal.OnPressed += listener => OnClickHandle(listener, EGameHard.Normal); 
            btnHard.OnPressed += listener => OnClickHandle(listener, EGameHard.Hard);
            btnHardest.OnPressed += listener => OnClickHandle(listener, EGameHard.Hardest);
            
            btnEasy.OnHighlighted += OnHighlight;
            btnNormal.OnHighlighted += OnHighlight;
            btnHard.OnHighlighted += OnHighlight;
            btnHardest.OnHighlighted += OnHighlight;
            
            btnEasy.OnNormal += OnNormal;
            btnNormal.OnNormal += OnNormal;
            btnHard.OnNormal += OnNormal;
            btnHardest.OnNormal += OnNormal;

            componentSystemUI.GetComponent<BlackFadeComponent>().AddControl(btnEasy.Control, btnNormal.Control, btnHard.Control, btnHardest.Control);

            var buttonGroup = GetComponent<SelectHardButtonGroup>();
            if (buttonGroup != null)
                buttonGroup.Init(btnEasy, btnNormal, btnHard, btnHardest);

            LoadAtlas(3);
        }
        protected override void LoadAtlas(int targetAtlasCount)
        {
            base.LoadAtlas(targetAtlasCount);
            var path = "Assets/GameRes/Atlas/HardPanel/content.spriteatlas";
            GameManager.GetGMComponent<ResComponentGM>().LoadAsset<SpriteAtlas>(path, atlas =>
            {
                if (atlas == null) { return; }
                if (spriteAtlas != null) { return; }
                spriteAtlas = atlas;
                loadAtlasCallFunc();
            });
            path = "Assets/GameRes/Atlas/HardPanel/content_en.spriteatlas";
            GameManager.GetGMComponent<ResComponentGM>().LoadAsset<SpriteAtlas>(path, atlas =>
            {
                if (atlas == null) { return; }
                if (spriteAtlas_en != null) { return; }
                spriteAtlas_en = atlas;
                loadAtlasCallFunc();
            });
            path = "Assets/GameRes/Atlas/HardPanel/content_jp.spriteatlas";
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
            spriteAtlas_jp = spriteAtlas_jp == null ? spriteAtlas_en : spriteAtlas_jp;
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
            // 设置各种按钮的图片
            GameTools.loadTextureByAtlas(imgBtnEasy, mySpriteAtlas, "简单");
            GameTools.loadTextureByAtlas(imgBtnEasyPressed, mySpriteAtlas, "简单选择");
            GameTools.loadTextureByAtlas(imgBtnNormal, mySpriteAtlas, "普通");
            GameTools.loadTextureByAtlas(imgBtnNormalPressed, mySpriteAtlas, "普通选择");
            GameTools.loadTextureByAtlas(imgBtnHard, mySpriteAtlas, "困难");
            GameTools.loadTextureByAtlas(imgBtnHardPressed, mySpriteAtlas, "困难选择");
            GameTools.loadTextureByAtlas(imgBtnHardest, mySpriteAtlas, "残酷");
            GameTools.loadTextureByAtlas(imgBtnHardestPressed, mySpriteAtlas, "残酷选择");
            GameTools.loadTextureByAtlas(imgTextEasy, mySpriteAtlas, "textImgEasy");
            GameTools.loadTextureByAtlas(imgTextNormal, mySpriteAtlas, "textImgNormal");
            GameTools.loadTextureByAtlas(imgTextHard, mySpriteAtlas, "textImgHard");
            GameTools.loadTextureByAtlas(imgTextHardest, mySpriteAtlas, "textImgHardest");
        }

        protected internal override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            
            componentSystemUI.GetComponent<BlackFadeComponent>().HideFade();
            btnEasy.ResetNormalState();
            btnNormal.ResetNormalState();
            btnHard.ResetNormalState();
            btnHardest.ResetNormalState();
        }

        private void OnHighlight(UIListener listener)
        {
            listener.GetComponent<Animator>().SetTrigger("Highlighted");
            var listenerName = listener.gameObject.name;
            var isEasySelect = listenerName == "ButtonEasy";
            var isNormalSelect = listenerName == "ButtonNormal";
            var isHardSelect = listenerName == "ButtonHard";
            var isHardestSelect = listenerName == "ButtonHardest";
            imgBtnEasy.SetActive(!isEasySelect);
            imgBtnNormal.SetActive(!isNormalSelect);
            imgBtnHard.SetActive(!isHardSelect);
            imgBtnHardest.SetActive(!isHardestSelect);
            imgBtnEasyPressed.SetActive(isEasySelect);
            imgBtnNormalPressed.SetActive(isNormalSelect);
            imgBtnHardPressed.SetActive(isHardSelect);
            imgBtnHardestPressed.SetActive(isHardestSelect);
        }

        private void OnNormal(UIListener listener)
        {
            listener.GetComponent<Animator>().SetTrigger("ReturnToNormal");
            listener.GetComponent<UIStateMachine>().ChangeTo("Normal");

            var listenerName = listener.gameObject.name;
            var isEasySelect = listenerName == "ButtonEasy";
            var isNormalSelect = listenerName == "ButtonNormal";
            var isHardSelect = listenerName == "ButtonHard";
            var isHardestSelect = listenerName == "ButtonHardest";
            if (isEasySelect) imgBtnEasy.SetActive(isEasySelect);
            if (isNormalSelect) imgBtnNormal.SetActive(isNormalSelect);
            if (isHardSelect) imgBtnHard.SetActive(isHardSelect);
            if (isHardestSelect) imgBtnHardest.SetActive(isHardestSelect);
            imgBtnEasyPressed.SetActive(false);
            imgBtnNormalPressed.SetActive(false);
            imgBtnHardPressed.SetActive(false);
            imgBtnHardestPressed.SetActive(false);
        }

        private void OnClickHandle(UIListener listener, EGameHard hard)
        {
            UIUtils.PlayBtnAudio(this);
            listener.GetComponent<UIStateMachine>().ChangeTo("Pressed");
            componentSystemUI.GetComponent<BlackFadeComponent>().CloseFormShowFade(UIForm, () => GetProxy<SelectHardFormProxy>().SelectHard(hard));
            
/*            // 测试加载界面
            componentSystemUI.GetComponent<BlackFadeComponent>().CloseFormShowFade(UIForm, () =>
            {
                GameManager.GetGMComponent<UIComponentGM>().OpenUIForm(UIPrefabPath.GetUIPrefabPath("LoadingPanel"), EUIGroup.Middle, new OpenFormArgs()
                {
                    userData = new Action(() =>
                    {
                        GetProxy<SelectHardFormProxy>().SelectHard(hard);
                    })
                });
            });*/
        }
    }
}