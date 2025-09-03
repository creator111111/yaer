using DG.Tweening;
using Game.GameMgr.Component;
using Game.GameMgr;
using Game.GameRuntime.UI.FormLogic.Base;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using Game.Static.Name.Settings;
using Game.GameRuntime.GameSceneManager.Base;
using Game.GameRuntime.Entities.Player;

namespace Game.GameRuntime.UI.FormLogic.ChapterEndPanel
{
    // 章节结束界面
    public class ControlTipsFormLogic : BaseUIFormLogic
    {
        public GameObject root;
        public GameObject imgTipsBg;
        public GameObject touchArea;
        SpriteAtlas spriteAtlas;
        SpriteAtlas spriteAtlas_en;
        SpriteAtlas spriteAtlas_jp;
        List<int> curTipsImgTagList = new List<int>(); // 当前需要提示图片的下标列表
        int curTipsIndex = 0;
        bool canNextTipsImg; // 是否能进行一张图片提示
        GameObject oldImgTipsBg;
        protected internal override void OnInit(object userData)
        {
            base.OnInit(userData);
            if (userData is List<int> tipsTagList)
            {
                curTipsImgTagList = tipsTagList;
            }
            LoadAtlas(3);

            GameTools.setObjectClickFunc(touchArea, () =>
            {
                if (!canNextTipsImg)
                {
                    DOTween.Kill(imgTipsBg, true);
                }
                else
                {
                    // 进行下一个图片UI提示
                    ToNextImgTips();
                }
            });
        }

        protected override void LoadAtlas(int targetAtlasCount)
        {
            base.LoadAtlas(targetAtlasCount);
            var resCpnGM = GameManager.GetGMComponent<ResComponentGM>();
            var path = "Assets/GameRes/Atlas/ControlTipsPanel/imgTips.spriteatlas";
            resCpnGM.LoadAsset<SpriteAtlas>(path, (atlas) => {
                if (atlas == null) { return; }
                if (spriteAtlas != null) { return; }
                spriteAtlas = atlas;
                loadAtlasCallFunc();
            });
            path = "Assets/GameRes/Atlas/ControlTipsPanel/imgTips_en.spriteatlas";
            resCpnGM.LoadAsset<SpriteAtlas>(path, (atlas) => {
                if (atlas == null) { return; }
                if (spriteAtlas_en != null) { return; }
                spriteAtlas_en = atlas;
                loadAtlasCallFunc();
            });
            path = "Assets/GameRes/Atlas/ControlTipsPanel/imgTips_jp.spriteatlas";
            resCpnGM.LoadAsset<SpriteAtlas>(path, (atlas) => {
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
            var baseKeyName = "游戏说明{0}";
            var curTipsTag = curTipsImgTagList.Count > curTipsIndex ? curTipsImgTagList[curTipsIndex] : 0;
            var realKeyName = string.Format(baseKeyName, curTipsTag);
            if (mySpriteAtlas.GetSprite(realKeyName) != null)
            {
                imgTipsBg.SetActive(true);
                GameTools.loadTextureByAtlas(imgTipsBg, mySpriteAtlas, realKeyName);
                curTipsIndex++;
            }
            else
            {
                imgTipsBg.SetActive(false);
            }
        }

        protected internal override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            curTipsIndex = 0;
            canNextTipsImg = false;
            if (userData is List<int> tipsTagList)
            {
                curTipsImgTagList = tipsTagList;
            }
            var sceneMgr = GameManager.GetGameSceneManager() as BaseGameSceneManager;
            // 暂停游戏
            if (sceneMgr != null)
            {
                sceneMgr.SetSceneObjIsPause(true);
                var playerEntity = sceneMgr.GetPlayerEntity();
                if (playerEntity != null && playerEntity.Logic is PlayerLogic playerLogic)
                {
                    playerLogic.PauseGameHandle();
                }
            }
            
            ShowImgTips();
        }

        void ShowImgTips(float fadeTime = 1.5f)
        {
            if (fadeTime <= 0)
            {
                canNextTipsImg = true;
                return;
            }
            imgTipsBg.GetComponent<CanvasGroup>().alpha = 0;
            canNextTipsImg = false;
            var fadeAct_1 = GameActionMgr.runFadeAction(imgTipsBg, 1f, fadeTime);
            fadeAct_1.onComplete = () =>
            {
                canNextTipsImg = true;// 背景图片完全出现之后才能进行下一步
            };
        }

        void ToNextImgTips()
        {
            if (!canNextTipsImg) { return; }
            // 是否还有下一张图片提示
            if (curTipsImgTagList.Count <= curTipsIndex)
            {
                var fadeAct_1 = GameActionMgr.runFadeAction(imgTipsBg, 0f, 1f);
                fadeAct_1.onComplete = () =>
                {
                    // 淡出消失后自动关闭界面
                    CloseForm();
                };
            }
            else
            {
                // 显示下一张图片
                if (oldImgTipsBg != null) { Destroy(oldImgTipsBg); }
                oldImgTipsBg = imgTipsBg;
                imgTipsBg = Instantiate(oldImgTipsBg); // 复制一张图片
                imgTipsBg.transform.SetParent(oldImgTipsBg.transform.parent, false);
                oldImgTipsBg.transform.SetAsLastSibling();
                UpdateUI(); // 先更换图片
                oldImgTipsBg.GetComponent <CanvasGroup>().alpha = 1f;
                var fadeAct_1 = GameActionMgr.runFadeAction(oldImgTipsBg, 0f, 1f);
                fadeAct_1.onComplete = () =>
                {
                    // 淡出消失后直接显示当前图片
                    ShowImgTips(0);
                };
            }
        }

        protected internal override void OnClose(bool isShutdown, object userData)
        {
            base.OnClose(isShutdown, userData);
            var sceneMgr = GameManager.GetGameSceneManager() as BaseGameSceneManager;
            // 恢复暂停
            if (sceneMgr != null)
            {
                sceneMgr.SetSceneObjIsPause(false);
                var playerEntity = sceneMgr.GetPlayerEntity();
                if (playerEntity != null && playerEntity.Logic is PlayerLogic playerLogic)
                {
                    playerLogic.ResumeGameHandle();
                }
            }
        }
        public override void PlayerOpenAudio()
        {

        }
    }
}