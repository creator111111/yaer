using DG.Tweening;
using Game.GameMgr.Component;
using Game.GameMgr;
using Game.GameMgr.Component.Archive.ArchiveDataClass.Player;
using Game.GameRuntime.UI.FormLogic.Base;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

namespace Game.GameRuntime.UI.FormLogic.AchievementTipsPanel
{
    // 获得成就提示界面
    public class AchieveTipsFormLogic : BaseUIFormLogic
    {
        public GameObject textArea;
        public GameObject imgRealName;
        public List<GameObject> tagObjList;
        [HideInInspector] public SpriteAtlas nameAtlas;
        [HideInInspector] public SpriteAtlas nameAtlas_en;
        [HideInInspector] public SpriteAtlas nameAtlas_jp;

        AchievementType curAchType;
        protected internal override void OnInit(object userData)
        {
            base.OnInit(userData);
            textArea.GetComponent<CanvasGroup>().alpha = 0;
            LoadAtlas(3);
        }

        protected override void LoadAtlas(int targetAtlasCount)
        {
            base.LoadAtlas(targetAtlasCount);
            var realPath = "Assets/GameRes/Atlas/AchievementPanel/name.spriteatlas";
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
            curAchType = (AchievementType)userData;
        }


        public override void UpdateUI()
        {
            base.UpdateUI();
            initData(curAchType);

            // 打开时文字淡入显示
            List<Tween> tweens = new List<Tween>() {
                GameActionMgr.runFadeAction(textArea, 1f, 1f),
                GameActionMgr.runDelayTimeAction(6, () =>
                {
                    FadeOutUI();
                }, gameObject),
            };
            GameActionMgr.runSequenceAction(textArea, tweens);

            foreach (var imgTag in tagObjList)
            {
                imgTag.GetComponent<CanvasGroup>().alpha = 1f;
            }

        }

        private void initData(AchievementType achieveId)
        {
            // 设置成就名称
            //var achieveName = AchievementDataMgr.getInstance().GetAchievementName(achieveId);
            //GameTools.setText(textName, achieveName);
            //GameTools.setText(textName2, achieveName);
            Dictionary<LanguageEnumType, SpriteAtlas> spriteAtlasData = new Dictionary<LanguageEnumType, SpriteAtlas>() {
                { LanguageEnumType.Chinese, nameAtlas }, {  LanguageEnumType.English, nameAtlas_en },
                {  LanguageEnumType.Japanese, nameAtlas_jp },
            };
            var curLaunageType = GameManager.Instance.language;
            SpriteAtlas mySpriteAtlas;
            if (!spriteAtlasData.ContainsKey(curLaunageType))
            {
                // 不存在的语言一律使用英文
                mySpriteAtlas = nameAtlas_en;
            }
            else
            {
                mySpriteAtlas = spriteAtlasData[curLaunageType];
            }
            var keyName = (int)achieveId;
            GameTools.loadTextureByAtlas(imgRealName, mySpriteAtlas, keyName.ToString());
        }

        void FadeOutUI()
        {
            var fadeOutAct = GameActionMgr.runFadeAction(textArea, 0f, 1f);
            fadeOutAct.onComplete = () =>
            {
                CloseForm();
            };
            
            foreach (var imgTag in tagObjList)
            {
                GameActionMgr.runFadeAction(imgTag, 0f, 1f);
            }
        }

        protected internal override void OnClose(bool isShutdown, object userData)
        {
            base.OnClose(isShutdown, userData);
        }


        public override void PlayerOpenAudio()
        {

        }
    }
}