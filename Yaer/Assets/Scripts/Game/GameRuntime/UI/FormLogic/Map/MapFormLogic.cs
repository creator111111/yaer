using System.Collections.Generic;
using DG.Tweening;
using Game.GameMgr.Component.Archive;
using Game.GameMgr;
using Game.GameMgr.Component.Archive.ArchiveDataClass.Player;
using Game.GameRuntime.GameSceneManager.Component;
using Game.GameRuntime.UI.FormLogic.Base;
using Game.Static.Name.Res;
using Game.Static.Path;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Game.GameMgr.Component.Archive.ArchiveDataClass.Date;
using Game.GameMgr.Component;
using Game.GameMgr.Component.UI;
using Game.GameRuntime.Entities.Player.Components;
using Game.GameRuntime.Entities.Player;
using UnityEngine.U2D;

namespace Game.GameRuntime.UI.FormLogic.Map
{
    public class MapFormLogic : BaseUIFormLogic
    {
        [SerializeField] private Transform road;
        [SerializeField] private Transform places;
        [SerializeField] private Animator animator;
        [SerializeField] private TextMeshProUGUI date;

        [Header("Sign动画信息")][SerializeField] private RectTransform signRect; // UI 元素的 RectTransform
        [SerializeField] private float offSet = 15;         // 偏移量
        [SerializeField] private float duration = 1f;       // 动画持续时间
        [Header("前景显示时间")] public float time;
        private Ease easeType = Ease.OutQuad; // 缓动类型

        /// <summary>与 Map 上 <c>ButtonJingLingVillage</c> 的 GameObject 名称一致，用于 <c>switch</c> 分支。</summary>
        private const string JingLingVillageButtonName = "ButtonJingLingVillage";

        /// <summary>
        /// 与预制体 <c>ButtonHome</c> 名称一致；语义为「回序章/重开新游戏」，不参与地点解锁字典。
        /// </summary>
        private const string HomeButtonName = "ButtonHome";

        /// <summary>
        /// 防止连点多次触发换场（与《场景切换与对话触发跳转_架构溯源报告》§7.4 验收「不卡死输入」一致）。
        /// 旧版曾用于自建 BlackPanel；现由 <see cref="LoadSceneComponentGSM.LoadScene"/> 内置黑幕接管，本标记仍在点击至 LoadScene 调用前防连点。
        /// </summary>
        private bool jingLingVillageBlackTransitionInProgress;

        /// <summary>
        /// 防止连点 ButtonHome 多次触发 <see cref="ProcedureComponentGM.RestartNewGameFromProgress"/>。
        /// </summary>
        private bool homeRestartInProgress;

        private Dictionary<string, Button> placesButtonDic = new Dictionary<string, Button>();

        private Dictionary<string, Image> roadImageDic = new Dictionary<string, Image>();
        private PlayerMapData playerMapData;
        private Tween tween;

        public GameObject imgPlaceName;
        SpriteAtlas spriteAtlas;
        protected internal override void OnInit(object userData)
        {
            base.OnInit(userData);
            
            // 获取所有路线Image
            var roads = road.GetComponentsInChildren<Image>();
            foreach (var item in roads) roadImageDic.Add(item.name, item);

            // 获取所有地点 Button；ButtonHome 为「重开新游戏」入口，不纳入地点字典（避免 ShowUnOpenTipsPanel）
            var placeToggles = places.GetComponentsInChildren<Button>();
            foreach (var item in placeToggles)
            {
                if (item.name == HomeButtonName)
                {
                    continue;
                }

                placesButtonDic.Add(item.name, item);
            }

            BingAllBtnClickEvent();
            BindButtonHomeClick();

            LoadAtlas(1);
        }

        protected override void LoadAtlas(int targetAtlasCount)
        {
            base.LoadAtlas(targetAtlasCount);
            var path = "Assets/GameRes/Atlas/MapPanel/areaName.spriteatlas";
            GameManager.GetGMComponent<ResComponentGM>().LoadAsset<SpriteAtlas>(path, atlas =>
            {
                if (atlas == null) return;
                if (spriteAtlas != null) { return; }
                spriteAtlas = atlas;
                loadAtlasCallFunc();
            });
           
        }
        public override void UpdateUI()
        {
            base.UpdateUI();

            var baseName = "地理名{0}";
            var languageTag = GameManager.GetCurLanguageResTag();
            var realName = string.Format(baseName, languageTag);
            if (spriteAtlas.GetSprite(realName) == null)
            {
                realName = "地理名_en"; // 不存在则默认使用英文版本
            }
            GameTools.loadTextureByAtlas(imgPlaceName, spriteAtlas, realName);
            var curLanguage = GameManager.Instance.language;
            if (curLanguage == LanguageEnumType.Chinese)
            {
                imgPlaceName.transform.localPosition = new Vector2(0, 53);
            }
            else
            {
                imgPlaceName.transform.localPosition = new Vector2(0, 0);
            }
        }

        protected internal override void OnOpen(object userData)
        {
            // 每次打开地图时允许再次点击精灵城入口 / Home 重开（新实例或重开时避免上一段换场中的标记残留）
            jingLingVillageBlackTransitionInProgress = false;
            homeRestartInProgress = false;
            AllowOpenMenu(false);
            base.OnOpen(userData);
            // 获取玩家地图数据
            playerMapData = userData as PlayerMapData;

            //animator.Rebind();

            ShowUnlockPlace();
            ShowUnlockRoad();
            signRect.gameObject.SetActive(false);
            date.text = GameManager.GetGMComponent<ArchiveComponentGM>().GetData<DateData>().Date;
            date.gameObject.SetActive(false);
            var entityCpn = GameManager.GetGMComponent<EntityComponentGM>();
            if (entityCpn != null)
            {
                var playerLogic = entityCpn.GetEntityLogic<PlayerLogic>();
                if (playerLogic != null)
                {
                    playerLogic.PauseGameHandle();
                }
            }
        }

        protected internal override void OnClose(bool isShutdown, object userData)
        {
            // 精灵村换场在 stayAction 里会主动关地图；此处复位防连点，避免下次打开 Map 仍不可点
            jingLingVillageBlackTransitionInProgress = false;
            homeRestartInProgress = false;
            AllowOpenMenu(true);
            base.OnClose(isShutdown, userData);

            if (tween != null) { tween.Kill(); }
            var entityCpn = GameManager.GetGMComponent<EntityComponentGM>();
            if (entityCpn != null)
            {
                var playerLogic = entityCpn.GetEntityLogic<PlayerLogic>();
                if (playerLogic != null && playerLogic.commonSfxCpn != null)
                {
                    playerLogic.ResumeGameHandle();
                }
            }
            //GameManager.GetGMComponent<EntityComponentGM>().GetEntityLogic<PlayerLogic>().componentSystem.GetComponent<PlayerInputComponent>().SetAllowMove(true);

            PlayerOpenAudio();
        }

        public void SetSign(string place)
        {
            var rect = placesButtonDic[$"Button{place}"].transform as RectTransform;
            signRect.anchoredPosition = rect.anchoredPosition;
            signRect.localScale = Vector3.one;

            // sign横向偏移半个图标
            signRect.anchoredPosition += new Vector2(rect.sizeDelta.x / 2, 0);
            var end = signRect.anchoredPosition + new Vector2(offSet, 0);

            // 使用 DOMove 方法实现往返动画
            if (tween != null) { tween.Kill(); }
            tween = signRect.DOAnchorPos(end, duration).SetEase(easeType).SetLoops(-1, LoopType.Yoyo);
            signRect.gameObject.SetActive(true);
        }

        // 设置一个地标为高亮状态
        public void SelectPlaceLight(string placeName)
        {
            if (placesButtonDic.TryGetValue(placeName, out var placeBtn))
            {
                placeBtn.Select();
                placeBtn.interactable = true;
            }
        }

        private void ShowUnlockRoad()
        {
            // 全部失活
            foreach (var image in roadImageDic.Values) image.gameObject.SetActive(false);

            // 根据数据激活已经解锁的路线
            foreach (var unlockRoad in playerMapData.GetUnlockRoad())
                if (roadImageDic.ContainsKey($"Image{unlockRoad}"))
                    roadImageDic[$"Image{unlockRoad}"].gameObject.SetActive(true);
        }

        private void ShowUnlockPlace()
        {
            // 全部禁止交互
            foreach (var button in placesButtonDic.Values) button.interactable = false;

            // 根据数据激活交互
            foreach (var place in playerMapData.GetUnlockPlaces())
                if (placesButtonDic.ContainsKey($"Button{place}"))
                    placesButtonDic[$"Button{place}"].interactable = true;
        }

        // 绑定按钮点击事件
        void BingAllBtnClickEvent()
        {
            foreach(var data in placesButtonDic)
            {
                var placeName = data.Key;
                var btn = data.Value;
                btn.onClick.AddListener(()=>OnSelectOnePlace(placeName));
            }
        }

        void OnSelectOnePlace(string placeName)
        {
            UIUtils.PlayBtnAudio(this);

            switch (placeName)
            {
                case JingLingVillageButtonName:
                    // 策划 §7：点击即换场至 Village_KenMuNi1（不经地图自建黑幕后再 TriggerStory）
                    OnSelectJingLingVillage();
                    break;
                default:
                    GameManager.ShowUnOpenTipsPanel();
                    break;
            }
        }

        /// <summary>
        /// 单独绑定 ButtonHome：走进程层 <see cref="ProcedureComponentGM.RestartNewGameFromProgress"/>，
        /// 不复用 LoadSceneComponentGSM（仅换场无法清档/InitNewGameData）。
        /// </summary>
        private void BindButtonHomeClick()
        {
            var homeBtnTransform = places.Find(HomeButtonName);
            if (homeBtnTransform == null)
            {
                Debug.LogWarning("[MapFormLogic] 未找到 ButtonHome，无法绑定「重开新游戏」。");
                return;
            }

            var homeBtn = homeBtnTransform.GetComponent<Button>();
            if (homeBtn == null)
            {
                return;
            }

            homeBtn.onClick.AddListener(OnClickButtonHome);
        }

        /// <summary>
        /// ButtonHome 点击：黑幕过渡后进入 NewGameScene → NewGameCartoonPanel，链路与主菜单新游戏一致。
        /// 首版无二次确认；若策划要「进度将丢失」弹窗，在此方法内包一层 Confirm UI 即可。
        /// </summary>
        private void OnClickButtonHome()
        {
            UIUtils.PlayBtnAudio(this);

            if (homeRestartInProgress)
            {
                return;
            }

            var procedure = GameManager.GetGMComponent<ProcedureComponentGM>();
            if (procedure == null)
            {
                Debug.LogWarning("[MapFormLogic] 无法重开新游戏：ProcedureComponentGM 不可用。");
                return;
            }

            homeRestartInProgress = true;
            procedure.RestartNewGameFromProgress();
        }

        /// <summary>
        /// 精灵城入口：走 <see cref="LoadSceneComponentGSM.LoadScene"/> → <see cref="Game.GameMgr.Component.ChangeScene.ChangeSceneComponentGM"/>，
        /// 与《场景切换与对话触发跳转_架构溯源报告》§1～2、§7 一致；终点场景为 <see cref="SceneName.Village_KenMuNi1"/>。
        /// 替代方案：若策划改回「先对话再进村」，可恢复 TriggerStory(&quot;Village_KenMuNiStart&quot;) 或在 Village 场景 Procedure 中接对话。
        /// </summary>
        private void OnSelectJingLingVillage()
        {
            if (jingLingVillageBlackTransitionInProgress)
            {
                return;
            }

            var sceneMgr = GameManager.GetGameSceneManager();
            var loadModule = sceneMgr?.GetModule<LoadSceneComponentGSM>();
            if (loadModule == null)
            {
                Debug.LogWarning("[MapFormLogic] 无法跳转精灵村：当前场景无 LoadSceneComponentGSM。");
                return;
            }

            jingLingVillageBlackTransitionInProgress = true;
            // blackFade=true：黑幕由换场组件统一打开。
            // stayAction 在黑幕全显之后、OnExitScene/卸载当前场景之前执行：此时换场已确认进入管线，关闭 MapPanel，
            // 避免地图仍叠在 UI 栈上直至场景卸载（与「确认开始加载/转场」语义一致；若需严格等新场景 Ready 再关，可再订阅 onGameSceneManagerReady）。
            loadModule.LoadScene(
                SceneName.Village_KenMuNi1,
                stayAction: CloseMapPanelAfterJingLingVillageLoadConfirmed,
                blackFade: true);
        }

        /// <summary>
        /// <see cref="LoadSceneComponentGSM.LoadScene"/> 的 stayAction：黑幕已盖住屏幕后关闭世界地图。
        /// 执行顺序见 LoadSceneComponentGSM（stayAction → OnExitScene → LoadScene GF），故此处调用时场景资源可能尚未加载完，但换场已不可逆，满足策划「确认加载流程已走就要关地图」。
        /// </summary>
        private void CloseMapPanelAfterJingLingVillageLoadConfirmed()
        {
            var mapPath = UIPrefabPath.GetUIPrefabPath("MapPanel");
            var uiGm = GameManager.GetGMComponent<UIComponentGM>();
            if (uiGm == null)
            {
                return;
            }

            if (uiGm.GetUIForm(mapPath) == null)
            {
                return;
            }

            uiGm.CloseUIForm(mapPath);
        }
    }
}