using Game.GameMgr;
using Game.GameMgr.Component;
using Game.GameMgr.Component.Archive.ArchiveDataClass.Quest;
using Game.GameMgr.Component.UI;
using Game.GameRuntime.Entities.Player;
using Game.GameRuntime.GameSceneManager.Base;
using Game.GameRuntime.UI.Component;
using Game.GameRuntime.UI.Control;
using Game.GameRuntime.UI.FormLogic.Archive.LoadGamePanel;
using Game.GameRuntime.UI.FormLogic.Base;
using Game.GameRuntime.UI.FormLogic.Cartoon;

//using Game.GameRuntime.UI.FormLogic.Menu.MainItemPage;
using Game.GameRuntime.UI.FormLogic.SystemTips;
using Game.Static.Path;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

namespace Game.GameRuntime.UI.FormLogic.Menu
{
    public class MenuFormLogic : BaseUIFormLogic
    {
        [SerializeField] private UIListener btnItem;
        [SerializeField] private UIListener btnSave;
        [SerializeField] private UIListener btnLoad;
        [SerializeField] private UIListener btnBack;
        [SerializeField] private UIListener btnExit;

        //[SerializeField] private MenuFormMainItemPage mainItemPage;
        //[SerializeField] private DetailFormLogic detailForm;
        private MenuFormProxy proxy;
        private bool isExitTipsOpening;
        private string systemTipsPanelPath;

        public GameObject imgItemNor;
        public GameObject imgItemClick;
        public GameObject imgItemSelect;
        public GameObject imgSaveNor;
        public GameObject imgSaveClick;
        public GameObject imgSaveSelect;
        public GameObject imgLoadNor;
        public GameObject imgLoadClick;
        public GameObject imgLoadSelect;
        public GameObject imgBackNor;
        public GameObject imgBackClick;
        public GameObject imgBackSelect;
        public GameObject imgExitNor;
        public GameObject imgExitClick;
        public GameObject imgExitSelect;
        SpriteAtlas spriteAtlas;
        SpriteAtlas spriteAtlas_en;
        SpriteAtlas spriteAtlas_jp;

        /// <summary>
        /// 菜单金币图片数字（与商店 Total2 同款 <see cref="UiSpriteNumberDisplay"/>）。
        /// 可空：运行时在 ButtonMoney/Money_Digits 下 Find / Ensure。
        /// </summary>
        [SerializeField] private UiSpriteNumberDisplay moneyDigits;

        private const string ButtonMoneyNodeName = "ButtonMoney";
        private const string MoneyDigitsHostNodeName = "Money_Digits";
        /// <summary>静态占位「0.png」节点名；接 DigitStrip 后须隐藏，避免双「0」。</summary>
        private const string MoneyStaticZeroNodeName = "Money";

        /// <summary>
        /// 菜单 Money 显示防御上限，真源对齐 <see cref="PlayerGoldData.MaxGold"/>。
        /// 0829 改口：存档/逻辑已硬顶 999999；此处 Min 仅防 Digit 池异常扩位。
        /// 禁止 PadLeft 凑满 6 位；禁止改 UiSpriteNumberDisplay.EnsurePoolSize 中枢。
        /// </summary>
        public const int MenuMoneyMaxDisplayValue = PlayerGoldData.MaxGold;

        protected internal override void OnInit(object userData)
        {
            base.OnInit(userData);
            systemTipsPanelPath = UIPrefabPath.GetUIPrefabPath("SystemTipsPanel");

            btnItem.OnPressed += OnClickBtnItem;
            btnSave.OnPressed += OnClickBtnSave;
            btnLoad.OnPressed += OnClickBtnLoad;
            btnBack.OnPressed += OnClickBtnBack;
            btnExit.OnPressed += OnClickBtnExit;

            proxy = GetProxy<MenuFormProxy>();
            //mainItemPage.OnInit(proxy, this);

            LoadAtlas(3);
        }

        protected internal override void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(elapseSeconds, realElapseSeconds);

            if (!isExitTipsOpening)
            {
                return;
            }

            var tipsForm = GameManager.GetGMComponent<UIComponentGM>().GetUIForm(systemTipsPanelPath);
            if (tipsForm == null)
            {
                isExitTipsOpening = false;
            }
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
            imgItemSelect.SetActive(false);
            imgSaveNor.SetActive(false);
            imgSaveClick.SetActive(false);
            imgSaveSelect.SetActive(false);
            imgLoadNor.SetActive(false);
            imgLoadClick.SetActive(false);
            imgLoadSelect.SetActive(false);
            imgBackNor.SetActive(false);
            imgBackClick.SetActive(false);
            imgBackSelect.SetActive(false);
            imgExitNor.SetActive(false);
            imgExitClick.SetActive(false);
            imgExitSelect.SetActive(false);
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
            GameTools.loadTextureByAtlas(imgItemSelect, mySpriteAtlas, "贵重物品选择");
            GameTools.loadTextureByAtlas(imgSaveNor, mySpriteAtlas, "保存");
            GameTools.loadTextureByAtlas(imgSaveClick, mySpriteAtlas, "保存点");
            GameTools.loadTextureByAtlas(imgSaveSelect, mySpriteAtlas, "保存选择");
            GameTools.loadTextureByAtlas(imgLoadNor, mySpriteAtlas, "读取");
            GameTools.loadTextureByAtlas(imgLoadClick, mySpriteAtlas, "读取点");
            GameTools.loadTextureByAtlas(imgLoadSelect, mySpriteAtlas, "读取选择");
            GameTools.loadTextureByAtlas(imgBackNor, mySpriteAtlas, "返回");
            GameTools.loadTextureByAtlas(imgBackClick, mySpriteAtlas, "返回点");
            GameTools.loadTextureByAtlas(imgBackSelect, mySpriteAtlas, "返回选择");
            GameTools.loadTextureByAtlas(imgExitNor, mySpriteAtlas, "退出旅途");
            GameTools.loadTextureByAtlas(imgExitClick, mySpriteAtlas, "退出旅途点");
            GameTools.loadTextureByAtlas(imgExitSelect, mySpriteAtlas, "退出旅途选择");
        }

        protected internal override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            isExitTipsOpening = false;

            proxy.OnMenuActive(true);

            // 默认关闭物品栏
            //mainItemPage.gameObject.SetActive(false);
            //detailForm.gameObject.SetActive(false);

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

            // 打开菜单时刷新一次日历数字图片，确保与存档日期一致
            var dayNumDisplay = GetComponentInChildren<MenuCalendarDayNumDisplay>(true);
            dayNumDisplay?.RefreshFromArchive();

            // 0829：Money 区读真实金币 → 商店同款图片数字（自然位数，无前导零）
            RefreshMoneyFromArchive();
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

            // 从设置等子界面返回菜单时再刷一次余额（可选；OnOpen 为主路径）
            RefreshMoneyFromArchive();
        }

        /// <summary>
        /// 读 <see cref="QuestManager.GetPlayerGoldData"/> → <c>SetNumber</c>。
        /// goldData 为 null 时显示 0，不 NRE、不隐藏整条数字条。
        /// public：供 Editor 刷金工具在菜单已开时即时刷新。
        /// 原因：与商店购买门面同源，避免另起 displayGold 字段漂移。
        /// 防御 Min：正常 gold 应已 ≤ <see cref="PlayerGoldData.MaxGold"/>；若仍超则 Warning 并顶格（脏档未钳时兜底）。
        /// </summary>
        public void RefreshMoneyFromArchive()
        {
            ResolveMoneyDigitsReference();
            if (moneyDigits == null)
            {
                return;
            }

            var goldData = QuestManager.getInstance().GetPlayerGoldData();
            var gold = goldData != null ? goldData.gold : 0;
            if (gold < 0)
            {
                gold = 0;
            }

            // 防御 Digit 池；数据合法后 Warning 应极少触发。
            var displayGold = gold;
            if (displayGold > MenuMoneyMaxDisplayValue)
            {
                Debug.LogWarning(
                    $"[MenuMoney] gold={gold} 超过 MaxGold={MenuMoneyMaxDisplayValue}（异常），菜单按上限顶格显示。",
                    this);
                displayGold = MenuMoneyMaxDisplayValue;
            }

            moneyDigits.SetNumber(displayGold);
            Debug.Log($"[MenuMoney] SetNumber display={displayGold} (archive gold={gold})");
        }

        /// <summary>
        /// 解析 ButtonMoney/Money_Digits 上的 DigitStrip；缺则 EnsureOn（Editor Play 可补 Sprite）。
        /// Prefab 经 Tools/UI/Setup MenuPanel Money Digits 烘焙后应已有完整引用。
        /// </summary>
        private void ResolveMoneyDigitsReference()
        {
            if (moneyDigits != null)
            {
                moneyDigits.ApplyShopTotalLayout();
                HideStaticMoneyZeroPlaceholder();
                return;
            }

            var buttonMoney = FindDeepChild(transform, ButtonMoneyNodeName);
            if (buttonMoney == null)
            {
                Debug.LogWarning("[MenuMoney] 未找到 ButtonMoney，无法显示金币数字。", this);
                return;
            }

            HideStaticMoneyZeroPlaceholder(buttonMoney);

            var host = buttonMoney.Find(MoneyDigitsHostNodeName) as RectTransform;
            if (host == null)
            {
                // 运行时兜底建宿主：左留币标宽，右贴数字（与 Bake 布局一致）
                var hostGo = new GameObject(MoneyDigitsHostNodeName, typeof(RectTransform));
                hostGo.layer = buttonMoney.gameObject.layer;
                host = hostGo.GetComponent<RectTransform>();
                host.SetParent(buttonMoney, false);
                StretchMoneyDigitsHost(host);
            }

            moneyDigits = UiSpriteNumberDisplay.FindUnder(host);
            if (moneyDigits == null)
            {
                moneyDigits = UiSpriteNumberDisplay.EnsureOn(
                    host,
                    TextAnchor.MiddleRight,
                    stripSpacing: UiSpriteNumberDisplay.ShopTotalSpacing,
                    capacity: UiSpriteNumberDisplay.ShopTotalPoolCapacity);
                moneyDigits.TryLoadDefaultSpritesIfEmpty();
                moneyDigits.ApplyShopTotalLayout();
            }
            else
            {
                moneyDigits.ApplyShopTotalLayout();
            }
        }

        private void HideStaticMoneyZeroPlaceholder()
        {
            var buttonMoney = FindDeepChild(transform, ButtonMoneyNodeName);
            if (buttonMoney != null)
            {
                HideStaticMoneyZeroPlaceholder(buttonMoney);
            }
        }

        private static void HideStaticMoneyZeroPlaceholder(Transform buttonMoney)
        {
            var moneyZero = buttonMoney.Find(MoneyStaticZeroNodeName);
            if (moneyZero != null && moneyZero.gameObject.activeSelf)
            {
                // 禁止静态 0.png 与 DigitStrip 叠出双「0」
                moneyZero.gameObject.SetActive(false);
            }
        }

        private static void StretchMoneyDigitsHost(RectTransform host)
        {
            host.anchorMin = Vector2.zero;
            host.anchorMax = Vector2.one;
            host.pivot = new Vector2(0.5f, 0.5f);
            // 左侧留给 Money (1) 币标约 36px，避免数字压住图标
            host.offsetMin = new Vector2(36f, 0f);
            host.offsetMax = Vector2.zero;
            host.localScale = Vector3.one;
            host.localRotation = Quaternion.identity;
        }

        private static Transform FindDeepChild(Transform root, string childName)
        {
            if (root == null || string.IsNullOrEmpty(childName))
            {
                return null;
            }

            if (root.name == childName)
            {
                return root;
            }

            for (var i = 0; i < root.childCount; i++)
            {
                var found = FindDeepChild(root.GetChild(i), childName);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        protected internal override void OnClose(bool isShutdown, object userData)
        {
            base.OnClose(isShutdown, userData);
            isExitTipsOpening = false;

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
                else
                {
                    // 纯 UI 场景（如 Village_Shop）无玩家：关菜单后必须解暂停，否则交互会一直锁死。
                    // 见 0713/Village_Shop_ESC呼出菜单… §3.4 / 无玩家验收。
                    sceneMgr.SetSceneObjIsPause(false);
                }
            }
        }

        private void OnClickBtnItem(UIListener listener)
        {
            UIUtils.PlayBtnAudio(this);
            UIUtils.OpenPanel("ItemShowPanel", EUIGroup.Top, null,
            (logic) =>
            {
                if (logic is ItemShowFormLogic uiLogic)
                {
                    uiLogic.initData(proxy, this);
                }
            });
            //if (mainItemPage.IsOpen)
            //{
            //    mainItemPage.OnClose();
            //}
            //else
            //{
            //    mainItemPage.OnOpen();
            //}
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
            if (isExitTipsOpening)
            {
                return;
            }

            isExitTipsOpening = true;
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
                            var tipsProxy = systemTipsFormLogic.GetProxy<SystemTipsFormProxy>();
                            tipsProxy.ResetCallbacks();
                            tipsProxy.onSureEvent = () =>
                            {
                                isExitTipsOpening = false;
                                proxy.OnReturnMainMenu();
                            };
                            tipsProxy.onCancelEvent = () =>
                            {
                                isExitTipsOpening = false;
                            };
                        }
                    }
                });
        }
    }
}