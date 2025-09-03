using Game.GameMgr;
using Game.GameMgr.Component.Archive.ArchiveDataClass.Player;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.U2D;
using UnityEngine.UI;

namespace Game.GameRuntime.UI.FormLogic.Achievement
{
    public class AchievementItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private GameObject Tag;
        [SerializeField] private Image AchievementNameDefault;
        [SerializeField] private Image AchievementName;
        public AchievementFormLogic achievementFormLogic;
        public GameObject imgRealName;

        public Action<AchievementType, bool> OnAchievementItemHover;
        public Action<AchievementType, bool> OnAchievementItemSelected;
        // private bool Selected = false;

        private AchievementType achieveId;

        // 设置按钮为点击选中状态
        public void SetBtnClickState()
        {
            var clickNode = UIUtils.findChild(gameObject, "Click");
            EventSystem.current.SetSelectedGameObject(clickNode);
        }
        public void OnPointerEnter(PointerEventData eventData)
        {
            OnAchievementItemHover.Invoke(achieveId, true);
            //Tag.SetActive(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            OnAchievementItemHover.Invoke(achieveId, false);
            //Tag.SetActive(false);
        }

        SpriteAtlas GetCurSpriteAtlas()
        {
            var curLanageType = GameManager.Instance.language;
            Dictionary<LanguageEnumType, SpriteAtlas> languageData = new Dictionary<LanguageEnumType, SpriteAtlas>() {
                { LanguageEnumType.Chinese, achievementFormLogic.nameAtlas}, 
                { LanguageEnumType.English, achievementFormLogic.nameAtlas_en},
                { LanguageEnumType.Japanese, achievementFormLogic.nameAtlas_jp},
            };
            if (!languageData.ContainsKey(curLanageType)) { return achievementFormLogic.nameAtlas_en; }
            return languageData[curLanageType];
        }

        public void SetData(AchievementType achieveId)
        {
            this.achieveId = achieveId;
            // 设置成就名称
            var achieveName = AchievementDataMgr.getInstance().GetAchievementName(achieveId);
            //textName.SetActive(achieveName != "");
            //AchievementNameDefault.gameObject.SetActive(achieveName == "");
            Debug.Log("=======================AchievementName:" + AchievementName);
            AchievementName.gameObject.SetActive(achieveName == "");
            var spriteAtlas = GetCurSpriteAtlas();
            var keyName = string.Format("{0}", (int)achieveId);
            GameTools.loadTextureByAtlas(imgRealName, spriteAtlas, keyName);
            // 设置是否达成成就
            var hasFinsh = AchievementDataMgr.getInstance().CheckAchievementHasComplete(achieveId);
            Tag.SetActive(hasFinsh);
            imgRealName.SetActive(hasFinsh);
            AchievementNameDefault.gameObject.SetActive(!hasFinsh);
        }

        public void SetSelected()
        {
            OnAchievementItemSelected.Invoke(achieveId, false);
        }

        [Serializable]
        public class AchievementItemHoverEvent : UnityEvent<int, bool>
        {
        }

        [Serializable]
        public class AchievementItemSelectedEvent : UnityEvent<int>
        {
        }
    }
}