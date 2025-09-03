using System.Collections.Generic;
using DG.Tweening;
using Game.GameMgr.Component;
using Game.GameMgr;
using Game.GameMgr.Component.Archive.ArchiveDataClass.Player;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

namespace Game.GameRuntime.UI.FormLogic.Story.GoOut
{
    public class GoOutMapStoryMapFormLogic : MonoBehaviour
    {
        [SerializeField] private Transform road;
        [SerializeField] private Transform places;

        [Header("Sign动画信息")] [SerializeField] private RectTransform signRect; // UI 元素的 RectTransform
        [SerializeField] private float offSet = 15;                           // 偏移量
        [SerializeField] private float duration = 1f;                         // 动画持续时间
        [Header("前景显示时间")] public float time;
        private Ease easeType = Ease.OutQuad; // 缓动类型
        private Dictionary<string, Button> placesButtonDic = new Dictionary<string, Button>();

        private Dictionary<string, Image> roadImageDic = new Dictionary<string, Image>();
        private PlayerMapData playerMapData;
        private Tween tween;

        public GameObject imgPlaceName;

        SpriteAtlas spriteAtlas;
        public void OnInit(object userData)
        {
            // 获取所有路线Image
            var roads = road.GetComponentsInChildren<Image>();
            foreach (var item in roads) roadImageDic.Add(item.name, item);

            // 获取所有地点Button
            var placeToggles = places.GetComponentsInChildren<Button>();
            foreach (var item in placeToggles) placesButtonDic.Add(item.name, item);

            // 获取玩家地图数据
            playerMapData = userData as PlayerMapData;

            ShowUnlockPlace();
            ShowUnlockRoad();
            signRect.gameObject.SetActive(false);

            LoadAtlas();
        }

        protected void LoadAtlas()
        {
            var path = "Assets/GameRes/Atlas/MapPanel/areaName.spriteatlas";
            GameManager.GetGMComponent<ResComponentGM>().LoadAsset<SpriteAtlas>(path, atlas =>
            {
                spriteAtlas = atlas;
                UpdateUI();
            });

        }
        public void UpdateUI()
        {
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

        public void OnClose(bool isShutdown, object userData)
        {
            tween.Kill();
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
            tween.Kill();
            tween = signRect.DOAnchorPos(end, duration).SetEase(easeType).SetLoops(-1, LoopType.Yoyo);
            signRect.gameObject.SetActive(true);
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
    }
}