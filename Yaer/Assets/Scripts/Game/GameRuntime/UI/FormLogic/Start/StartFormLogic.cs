using Game.GameMgr;
using Game.GameMgr.Component;
using Game.GameMgr.Component.UI;
using Game.GameRuntime.UI.Component.BlackFade;
using Game.GameRuntime.UI.FormLogic.Archive.LoadGamePanel;
using Game.GameRuntime.UI.FormLogic.Base;
using Game.Static.Path;
using Game.Static.Path.Sound;
using GameFramework.UnityRuntime.Sound;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

namespace Game.GameRuntime.UI.FormLogic.Start
{
    public class StartFormLogic : BaseUIFormLogic
    {
        [SerializeField] private Button btnNewGame;
        [SerializeField] private Button btnLoadGame;
        [SerializeField] private Button btnAchievement;
        [SerializeField] private Button btnSettings;
        [SerializeField] private Button btnQuit;

        [SerializeField] private Animator showTitleAnimator;
        [SerializeField] private Animator showBtnsAnimator;
        [SerializeField] private Transform center;

        SpriteAtlas spriteAtlas;
        SpriteAtlas spriteAtlas_en;
        SpriteAtlas spriteAtlas_jp;

        protected internal override void OnInit(object userData)
        {
            base.OnInit(userData);
            GetProxy<StartFormProxy>();

            btnNewGame.onClick.AddListener(OnClickNewGame);
            btnLoadGame.onClick.AddListener(OnClickLoadGame);
            btnAchievement.onClick.AddListener(OnClickAchievement);
            btnSettings.onClick.AddListener(OnClickSettings);
            btnQuit.onClick.AddListener(OnClickExit);

            componentSystemUI.GetComponent<BlackFadeComponent>().AddHidingAction(0.5f, () =>
            {
                center.gameObject.SetActive(true);
                showTitleAnimator.enabled = true;
                showBtnsAnimator.enabled = true;
            });

            componentSystemUI.GetComponent<BlackFadeComponent>().AddControl(btnNewGame, btnLoadGame, btnAchievement, btnSettings, btnQuit);
            LoadAtlas(3);
        }

        protected override void LoadAtlas(int targetAtlasCount)
        {
            base.LoadAtlas(targetAtlasCount);
            var path = "Assets/GameRes/Atlas/StartPanel/btn.spriteatlas";
            GameManager.GetGMComponent<ResComponentGM>().LoadAsset<SpriteAtlas>(path, atlas =>
            {
                if (atlas == null) { return; }
                if (spriteAtlas != null) { return; }
                spriteAtlas = atlas;
                loadAtlasCallFunc();
            });
            path = "Assets/GameRes/Atlas/StartPanel/btn_en.spriteatlas";
            GameManager.GetGMComponent<ResComponentGM>().LoadAsset<SpriteAtlas>(path, atlas =>
            {
                if (atlas == null) { return; }
                if (spriteAtlas_en != null) { return; }
                spriteAtlas_en = atlas;
                loadAtlasCallFunc();
            });
            path = "Assets/GameRes/Atlas/StartPanel/btn_jp.spriteatlas";
            GameManager.GetGMComponent<ResComponentGM>().LoadAsset<SpriteAtlas>(path, atlas =>
            {
                if (atlas == null) { return; }
                if (spriteAtlas_jp != null) { return; }
                spriteAtlas_jp = atlas;
                loadAtlasCallFunc();
            });
        }

        protected internal override void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(elapseSeconds, realElapseSeconds);
            
        }

        // 多语言修改图片UI
        public override void UpdateUI()
        {
            if (GameManager.Instance == null) { return; }
            base.UpdateUI();
            spriteAtlas_jp = spriteAtlas_jp == null ? spriteAtlas_en : spriteAtlas_jp;
            Dictionary<LanguageEnumType, SpriteAtlas> spriteAtlasData= new Dictionary<LanguageEnumType, SpriteAtlas>() {
                { LanguageEnumType.Chinese, spriteAtlas }, {  LanguageEnumType.English, spriteAtlas_en },
                {  LanguageEnumType.Japanese, spriteAtlas_jp },
            };

            var curLaunageType = GameManager.Instance.language;
            SpriteAtlas mySpriteAtlas;
            if (!spriteAtlasData.ContainsKey(curLaunageType)) {
                // 不存在的语言一律使用英文
                mySpriteAtlas = spriteAtlas_en;
            }
            else
            {
                mySpriteAtlas = spriteAtlasData[curLaunageType];
            }
            // 设置各种按钮的图片
            var newGameSprite = mySpriteAtlas.GetSprite("NewGame");
            var newGameSprite_select = mySpriteAtlas.GetSprite("NewGameOnSelect");
            var newGameSprite_click = mySpriteAtlas.GetSprite("NewGameOnClick");
            GameTools.loadBtnSprite(btnNewGame, newGameSprite, newGameSprite_select, newGameSprite_click);
            var loadGameSprite = mySpriteAtlas.GetSprite("LoadGame");
            var loadGameSprite_select = mySpriteAtlas.GetSprite("LoadGameOnSelect");
            var loadGameSprite_click = mySpriteAtlas.GetSprite("LoadGameOnClick");
            GameTools.loadBtnSprite(btnLoadGame, loadGameSprite, loadGameSprite_select, loadGameSprite_click);
            var achievementSprite = mySpriteAtlas.GetSprite("Achivement");
            var achievementSprite_select = mySpriteAtlas.GetSprite("AchivementOnSelect");
            var achievementSprite_click = mySpriteAtlas.GetSprite("AchiveOnClick");
            GameTools.loadBtnSprite(btnAchievement, achievementSprite, achievementSprite_select, achievementSprite_click);
            var settingSprite = mySpriteAtlas.GetSprite("Settings");
            var settingSprite_select = mySpriteAtlas.GetSprite("SettingsOnSelect");
            var settingSprite_click = mySpriteAtlas.GetSprite("SettingsOnClick");
            GameTools.loadBtnSprite(btnSettings, settingSprite, settingSprite_select, settingSprite_click);
            var exitGameSprite = mySpriteAtlas.GetSprite("Exit");
            var exitGameSprite_select = mySpriteAtlas.GetSprite("ExitOnSelect");
            var exitGameSprite_click = mySpriteAtlas.GetSprite("ExitOnClick");
            GameTools.loadBtnSprite(btnQuit, exitGameSprite, exitGameSprite_select, exitGameSprite_click);
        }

        protected internal override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            GameManager.GetGMComponent<SoundComponentGM>().PlaySound(SoundType.BGM, "Start.ogg", true);
        }

        protected internal override void OnReveal()
        {
            base.OnReveal();

            // 先淡出黑幕
            componentSystemUI.GetComponent<BlackFadeComponent>().HideFade();
            center.gameObject.SetActive(false);
        }

        protected internal override void OnClose(bool isShutdown, object userData)
        {
            base.OnClose(isShutdown, userData);
            
            showTitleAnimator.enabled = false;
            showBtnsAnimator.enabled = false;
        }


        private void OnClickNewGame()
        {
            UIUtils.PlayBtnAudio(this);
            componentSystemUI.GetComponent<BlackFadeComponent>().ShowFade(() =>
            {
                GameManager.GetGMComponent<UIComponentGM>().OpenUIForm(UIPrefabPath.SelectHardPanel, UIForm.UIGroup.Name, new OpenFormArgs());
            });
        }

        private void OnClickLoadGame()
        {
            UIUtils.PlayBtnAudio(this);
            componentSystemUI.GetComponent<BlackFadeComponent>().ShowFade(() =>
            {
                GameManager.GetGMComponent<UIComponentGM>().OpenUIForm(UIPrefabPath.LoadGamePanel, UIForm.UIGroup.Name, new OpenFormArgs()
                {
                    callBack = logic =>
                    {
                        if (logic is LoadGameFormLogic loadGameFormLogic)
                        {
                            // 监听是否加载存档关闭菜单
                            void Action(string guid)
                            {
                                CloseForm();
                                loadGameFormLogic.GetProxy<LoadGameFormProxy>().onLoadGameAction -= Action;
                            }
                            loadGameFormLogic.GetProxy<LoadGameFormProxy>().onLoadGameAction += Action;
                        }
                    }
                });
            });
        }

        private void OnClickAchievement()
        {
            UIUtils.PlayBtnAudio(this);
            componentSystemUI.GetComponent<BlackFadeComponent>().ShowFade(() =>
            {
                GameManager.GetGMComponent<UIComponentGM>().OpenUIForm(UIPrefabPath.AchievementPanel, UIForm.UIGroup.Name, new OpenFormArgs());
            });
        }

        private void OnClickSettings()
        {
            UIUtils.PlayBtnAudio(this);
            componentSystemUI.GetComponent<BlackFadeComponent>().ShowFade(() =>
            {
                GameManager.GetGMComponent<UIComponentGM>().OpenUIForm(UIPrefabPath.SettingPanel, UIForm.UIGroup.Name, new OpenFormArgs());
            });
        }

        private void OnClickExit()
        {
            UIUtils.PlayBtnAudio(this);
            Application.Quit(0);
        }

        public override void PlayerOpenAudio()
        {
            
        }
    }
}