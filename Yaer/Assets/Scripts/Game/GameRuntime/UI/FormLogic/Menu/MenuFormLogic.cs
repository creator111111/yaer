using Game.GameMgr;
using Game.GameMgr.Component;
using Game.GameMgr.Component.UI;
using Game.GameRuntime.Entities.Player;
using Game.GameRuntime.GameSceneManager.Base;
using Game.GameRuntime.UI.Component;
using Game.GameRuntime.UI.Control;
using Game.GameRuntime.UI.FormLogic.Archive.LoadGamePanel;
using Game.GameRuntime.UI.FormLogic.Base;
using Game.GameRuntime.UI.FormLogic.Menu.MainItemPage;
using Game.GameRuntime.UI.FormLogic.SystemTips;
using Game.Static.Path;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

namespace Game.GameRuntime.UI.FormLogic.Menu
{
    public class MenuFormLogic : BaseUIFormLogic
    {
        [SerializeField] private UIListener btnItem;
        [SerializeField] private UIListener btnSave;
        [SerializeField] private UIListener btnLoad;
        [SerializeField] private UIListener btnBack;
        [SerializeField] private UIListener btnExit;

        [SerializeField] private MenuFormMainItemPage mainItemPage;
        [SerializeField] private DetailFormLogic detailForm;
        private MenuFormProxy proxy;

        public GameObject imgItemNor;
        public GameObject imgItemClick;
        public GameObject imgSaveNor;
        public GameObject imgSaveClick;
        public GameObject imgLoadNor;
        public GameObject imgLoadClick;
        public GameObject imgBackNor;
        public GameObject imgBackClick;
        public GameObject imgExitNor;
        public GameObject imgExitClick;
        SpriteAtlas spriteAtlas;
        SpriteAtlas spriteAtlas_en;
        SpriteAtlas spriteAtlas_jp;

        protected internal override void OnInit(object userData)
        {
            base.OnInit(userData);

            btnItem.OnPressed += OnClickBtnItem;
            btnSave.OnPressed += OnClickBtnSave;
            btnLoad.OnPressed += OnClickBtnLoad;
            btnBack.OnPressed += OnClickBtnBack;
            btnExit.OnPressed += OnClickBtnExit;

            proxy = GetProxy<MenuFormProxy>();
            mainItemPage.OnInit(proxy, this);

            LoadAtlas(3);
        }

        protected override void LoadAtlas(int targetAtlasCount)
        {
            base.LoadAtlas(targetAtlasCount);
            var path = "Assets/GameRes/Atlas/MenuPanel/btn.spriteatlas";
            GameManager.GetGMComponent<ResComponentGM>().LoadAsset<SpriteAtlas>(path, atlas =>
            {
                if (atlas == null) {  return; }
                if (spriteAtlas != null) { return; }
                spriteAtlas = atlas;
                loadAtlasCallFunc();
            });
            path = "Assets/GameRes/Atlas/MenuPanel/btn_en.spriteatlas";
            GameManager.GetGMComponent<ResComponentGM>().LoadAsset<SpriteAtlas>(path, atlas =>
            {
                if (atlas == null) { return; }
                if (spriteAtlas_en != null) { return; }
                spriteAtlas_en = atlas;
                loadAtlasCallFunc();
            });
            path = "Assets/GameRes/Atlas/MenuPanel/btn_jp.spriteatlas";
            GameManager.GetGMComponent<ResComponentGM>().LoadAsset<SpriteAtlas>(path, atlas =>
            {
                if (atlas == null) { return; }
                if (spriteAtlas_jp != null) { return; }
                spriteAtlas_jp = atlas;
                loadAtlasCallFunc();
            });
            imgItemNor.SetActive(false);
            imgItemClick.SetActive(false);
            imgSaveNor.SetActive(false);
            imgSaveClick.SetActive(false);
            imgLoadNor.SetActive(false);
            imgLoadClick.SetActive(false);
            imgBackNor.SetActive(false);
            imgBackClick.SetActive(false);
            imgExitNor.SetActive(false);
            imgExitClick.SetActive(false);
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
            imgItemNor.SetActive(true);
            imgSaveNor.SetActive(true);
            imgLoadNor.SetActive(true);
            imgBackNor.SetActive(true);
            imgExitNor.SetActive(true);
            GameTools.loadTextureByAtlas(imgItemNor, mySpriteAtlas, "贵重物品");
            GameTools.loadTextureByAtlas(imgItemClick, mySpriteAtlas, "贵重物品点");
            GameTools.loadTextureByAtlas(imgSaveNor, mySpriteAtlas, "保存");
            GameTools.loadTextureByAtlas(imgSaveClick, mySpriteAtlas, "保存点");
            GameTools.loadTextureByAtlas(imgLoadNor, mySpriteAtlas, "读取");
            GameTools.loadTextureByAtlas(imgLoadClick, mySpriteAtlas, "读取点");
            GameTools.loadTextureByAtlas(imgBackNor, mySpriteAtlas, "返回");
            GameTools.loadTextureByAtlas(imgBackClick, mySpriteAtlas, "返回点");
            GameTools.loadTextureByAtlas(imgExitNor, mySpriteAtlas, "退出旅途");
            GameTools.loadTextureByAtlas(imgExitClick, mySpriteAtlas, "退出旅途点");
        }

        protected internal override void OnOpen(object userData)
        {
            base.OnOpen(userData);

            proxy.OnMenuActive(true);

            // 默认关闭物品栏
            mainItemPage.gameObject.SetActive(false);
            detailForm.gameObject.SetActive(false);

            // 打开菜单界面时暂停游戏
            var sceneMgr = GameManager.GetGameSceneManager() as BaseGameSceneManager;
            
            if (sceneMgr != null) {
                sceneMgr.SetSceneObjIsPause(true);
                sceneMgr.SetSceneObjAniIsPause(true);
                
            }
            // 部分条件下按钮需要隐藏
            btnSave.gameObject.SetActive(sceneMgr.canShowSaveGame);
            btnLoad.gameObject.SetActive(sceneMgr.canShowLoadGame);
            btnItem.gameObject.SetActive(sceneMgr.canShowItemBag);
        }

        protected internal override void OnReveal()
        {
            base.OnReveal();

            btnItem.GetComponent<UIStateMachine>().ChangeTo("Normal");
            btnSave.GetComponent<UIStateMachine>().ChangeTo("Normal");
            btnLoad.GetComponent<UIStateMachine>().ChangeTo("Normal");
            btnBack.GetComponent<UIStateMachine>().ChangeTo("Normal");
            btnExit.GetComponent<UIStateMachine>().ChangeTo("Normal");

            btnItem.ResetNormalState();
            btnSave.ResetNormalState();
            btnLoad.ResetNormalState();
            btnBack.ResetNormalState();
            btnExit.ResetNormalState();
        }

        protected internal override void OnClose(bool isShutdown, object userData)
        {
            base.OnClose(isShutdown, userData);

            proxy.OnMenuActive(false);
            // 关闭菜单界面时恢复游戏
            var sceneMgr = GameManager.GetGameSceneManager() as BaseGameSceneManager;
            if (sceneMgr != null) {
                sceneMgr.SetSceneObjAniIsPause(false);
                var playerEntity = sceneMgr.GetPlayerEntity();
                if (playerEntity != null && playerEntity.Logic is PlayerLogic playerLogic)
                {
                    if (!playerLogic.hasInStoryEventState)
                    {
                        // 人物处于非故事状态时，才能恢复暂停
                        sceneMgr.SetSceneObjIsPause(false);
                    }
                }
            }
        }

        private void OnClickBtnItem(UIListener listener)
        {
            UIUtils.PlayBtnAudio(this);
            if (mainItemPage.IsOpen)
            {
                mainItemPage.OnClose();
            }
            else
            {
                mainItemPage.OnOpen();
            }
        }

        private void OnClickBtnSave(UIListener listener)
        {
            UIUtils.PlayBtnAudio(this);
            GameManager.GetGMComponent<UIComponentGM>()
                .OpenUIForm(UIPrefabPath.GetUIPrefabPath("SaveGamePanel"), UIForm.UIGroup.Name, new OpenFormArgs());
        }

        private void OnClickBtnLoad(UIListener listener)
        {
            UIUtils.PlayBtnAudio(this);
            GameManager.GetGMComponent<UIComponentGM>().OpenUIForm(UIPrefabPath.GetUIPrefabPath("LoadGamePanel"), UIForm.UIGroup.Name,
                new OpenFormArgs()
                {
                    callBack = logic =>
                    {
                        if (logic is LoadGameFormLogic loadGameFormLogic)
                        {
                            void Action(string guid)
                            {
                                if (isActiveAndEnabled)
                                {
                                    CloseForm();
                                }

                                loadGameFormLogic.GetProxy<LoadGameFormProxy>().onLoadGameAction -= Action;
                            }

                            loadGameFormLogic.GetProxy<LoadGameFormProxy>().onLoadGameAction += Action;
                        }
                    }
                });
        }

        private void OnClickBtnBack(UIListener listener)
        {
            UIUtils.PlayBtnAudio(this);
            CloseForm();
        }

        private void OnClickBtnExit(UIListener listener)
        {
            UIUtils.PlayBtnAudio(this);
            // 提示
            GameManager.GetGMComponent<UIComponentGM>().OpenUIForm(UIPrefabPath.GetUIPrefabPath("SystemTipsPanel"), UIForm.UIGroup.Name,
                new OpenFormArgs()
                {
                    userData = ESystemTipsType.Quit,
                    callBack = logic =>
                    {
                        if (logic is SystemTipsFormLogic systemTipsFormLogic)
                        {
                            systemTipsFormLogic.GetProxy<SystemTipsFormProxy>().onSureEvent = () =>
                            {
                                proxy.OnReturnMainMenu();
                            };
                            systemTipsFormLogic.GetProxy<SystemTipsFormProxy>().onCancelEvent = null;
                        }
                    }
                });
        }
    }
}