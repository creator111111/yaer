using System.Collections.Generic;
using DG.Tweening;
using Game.GameMgr.Component.Archive;
using Game.GameMgr;
using Game.GameMgr.Component.Archive.ArchiveDataClass.Player;
using Game.GameRuntime.UI.FormLogic.Base;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Game.GameMgr.Component.Archive.ArchiveDataClass.Date;
using Game.GameMgr.Component;
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

            // 获取所有地点Button
            var placeToggles = places.GetComponentsInChildren<Button>();
            foreach (var item in placeToggles) placesButtonDic.Add(item.name, item);

            BingAllBtnClickEvent();

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
                default:
                    GameManager.ShowUnOpenTipsPanel();
                    break;
            }
        }
    }
}