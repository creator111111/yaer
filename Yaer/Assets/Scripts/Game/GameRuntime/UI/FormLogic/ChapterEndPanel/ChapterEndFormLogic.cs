using DG.Tweening;
using Game.GameMgr;
using Game.GameMgr.Component;
using Game.GameMgr.Component.Archive.ArchiveDataClass.Player;
using Game.GameMgr.Component.UI;
using Game.GameRuntime.Entities.Player;
using Game.GameRuntime.Entities.Player.Components;
using Game.GameRuntime.GameSceneManager.Base;
using Game.GameRuntime.UI.FormLogic.Base;
using Game.GameRuntime.UI.FormLogic.Black;
using Game.GameRuntime.UI.FormLogic.Map;
using Game.Static.Path;
using Game.Static.Path.Sound;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

namespace Game.GameRuntime.UI.FormLogic.ChapterEndPanel
{
    // 章节结束界面
    public class ChapterEndFormLogic : BaseUIFormLogic
    {
        public GameObject maskBg;
        public GameObject imgTitle;
        public GameObject imgTextTalk_1;
        public GameObject imgTextTalk_2;
        public GameObject textStartNode;
        public GameObject textEndNode;
        public GameObject imgBigTitle;

        int curChapterId = 0; // 当前章节ID
        int curChapterTextNum = 0; // 当前章节结束文本数量
        bool hasEnd;
        // 章节结束时的文本数量字典
        Dictionary<int, int> chapterEndTextNumData = new Dictionary<int, int>() {
            { 0, 5 }// 序章有5句文本
        };
        // 章节结束后需要自动选中的地图名称数据
        Dictionary<int, string> chapterEndAutoSelectMapName = new Dictionary<int, string>() {
            { 0, "ButtonJingLingVillage"},
        };

        int curShowTextCount = 1; // 当前显示是第X句文本，从1开始
        MapFormLogic mapLogic;
        SpriteAtlas spriteAtlas;
        SpriteAtlas spriteBigTitleAtlas;
        PlayerLogic playerLogic;
        BaseGameSceneManager sceneManager;
        protected internal override void OnInit(object userData)
        {
            base.OnInit(userData);
            initUI();
            var playerCommonData = GameManager.GetGameSceneManager().GetArchiveData<PlayerCommonData>();
            curChapterId = playerCommonData.CurChapter;
            curChapterTextNum = chapterEndTextNumData.ContainsKey(curChapterId) ? chapterEndTextNumData[curChapterId] : 0;
            LoadAtlas(2);
        }

        protected override void LoadAtlas(int targetAtlasCount)
        {
            base.LoadAtlas(targetAtlasCount);
            var path = "Assets/GameRes/Atlas/ChapterEnd/Chapter{0}.spriteatlas";
            var realPath = string.Format(path, curChapterId);
            GameManager.GetGMComponent<ResComponentGM>().LoadAsset<SpriteAtlas>(realPath, atlas =>
            {
                if (atlas == null) { return; }
                if (spriteAtlas != null) { return; }
                spriteAtlas = atlas;
                loadAtlasCallFunc();
            });
            realPath = "Assets/GameRes/Atlas/ChapterEnd/ChapterEndTitle.spriteatlas";
            GameManager.GetGMComponent<ResComponentGM>().LoadAsset<SpriteAtlas>(realPath, atlas =>
            {
                if (atlas == null) { return; }
                if (spriteBigTitleAtlas != null) { return; }
                spriteBigTitleAtlas = atlas;
                loadAtlasCallFunc();
            });
        }


        // 刷新游戏UI
        public override void UpdateUI()
        {
            base.UpdateUI();
            hasEnd = false;
            // 根据语言加载不同的UI图片
            // =======结束标题
            var resTag = GameManager.GetCurLanguageResTag();
            var realImgName = "titleTips" + resTag;
            GameTools.loadTextureByAtlas(imgTitle, spriteAtlas, realImgName);
            // 大标题
            realImgName = "chapterEnd" + resTag;
            GameTools.loadTextureByAtlas(imgBigTitle, spriteBigTitleAtlas, realImgName);

            StartChapterEndAni();
        }

        void initUI()
        {
            // 初始隐藏需要替换资源的UI
            maskBg.SetActive(false);
            imgTitle.SetActive(false);
            imgBigTitle.SetActive(false);
            imgTextTalk_1.SetActive(false);
            imgTextTalk_2.SetActive(false);

            DisablePlayerLogic();
            
        }

        protected internal override void OnOpen(object userData)
        {
            AllowOpenMenu(false);
            base.OnOpen(userData);
            DisablePlayerLogic();
            if (mapLogic != null)
            {
                mapLogic.SetAllowEscapeClose(false);
            }
        }

        void DisablePlayerLogic()
        {
            if (sceneManager == null)
            {
                sceneManager = GameManager.GetGameSceneManager() as BaseGameSceneManager;
            }
            if (sceneManager != null)
            {
                sceneManager.SetSceneObjIsPause(true);
                if (playerLogic == null)
                {
                    var playerEntity = sceneManager.GetPlayerEntity();
                    if (playerEntity != null && playerEntity.Logic is PlayerLogic logic)
                    {
                        playerLogic = logic;
                    }
                }
                playerLogic.componentSystem.GetComponent<PlayerInputComponent>().canInputContorll = false;
                playerLogic.DisablePlayerMove();// 禁止玩家行动
                sceneManager.canShowItemBag = false;
                sceneManager.canShowSaveGame = false;
                sceneManager.canShowLoadGame = false;
            }
        }


        void StartChapterEndAni()
        {
            
            imgBigTitle.SetActive(true);
            imgBigTitle.GetComponent<CanvasGroup>().alpha = 0f;// 设置透明
            List<Tween> tweens = new List<Tween>() {
                GameActionMgr.runFadeAction(imgBigTitle, 1f, 3),
                GameActionMgr.runFadeAction(imgBigTitle, 0f, 3),
            };
            var SeqAct = GameActionMgr.runSequenceAction(imgBigTitle, tweens);
            SeqAct.onComplete = () =>
            {
                // 标题消失后打开地图界面
                OnOpenMapToNextTalk();
            };
            
        }

        void OnOpenMapToNextTalk()
        {
            imgTextTalk_1.SetActive(true);
            imgTextTalk_2.SetActive(true);
            imgTextTalk_1.GetComponent<CanvasGroup>().alpha = 0f;// 设置透明
            imgTextTalk_2.GetComponent<CanvasGroup>().alpha = 0f;// 设置透明
            // 黑幕转场
            var uiPath = UIPrefabPath.GetUIPrefabPath("BlackPanel");
            GameManager.GetGMComponent<UIComponentGM>().OpenUIForm(uiPath, EUIGroup.System, new OpenFormArgs()
            {
                userData = new ShowBlackFormArgs()
                {
                    showType = BlackFadeType.FadeShow,
                    onShowEnd = blackFormLogic =>
                    {
                        OpenMap();
                        blackFormLogic.CloseFormFade(() =>
                        {
                            // 开始滚动播放文本
                            StartShowTalkText();
                        });
                    }
                }
            });
        }

        void OpenMap()
        {
            // 播放章节结束后的背景音
            GameManager.GetGMComponent<SoundComponentGM>().PlaySound(SoundType.BGM, "Start.ogg", true, 2, 5);
            // 淡入显示黑色背景
            maskBg.SetActive(true);
            maskBg.GetComponent<CanvasGroup>().alpha = 0f;
            GameActionMgr.runFadeAction(maskBg, 1f, 1);
            // 打开地图
            string uiPrefabPath = UIPrefabPath.GetUIPrefabPath("MapPanel");
            GameManager.GetGMComponent<UIComponentGM>()
            .OpenUIForm(uiPrefabPath, EUIGroup.Top, new OpenFormArgs()
            {
                userData = GameManager.GetGameSceneManager().GetArchiveData<PlayerMapData>(),
                callBack = (uiForm) =>
                {
                    mapLogic = uiForm.UIForm.Logic as MapFormLogic;
                    // 调整显示层级
                    canvas.sortingOrder = mapLogic.GetCanvas().sortingOrder + 1;
                    mapLogic.SetAllowEscapeClose(false);

                }
            });
        }

        void StartShowTalkText()
        {
            
            curShowTextCount = 1;
            if (curShowTextCount > curChapterTextNum) { return; }
            var moveTime = 8f;
            ImgTextRunMoveUpAndFadeAciton(imgTextTalk_1, moveTime);
            GameActionMgr.runDelayTimeAction(moveTime / 2, () =>
            {
                ImgTextRunMoveUpAndFadeAciton(imgTextTalk_2, moveTime);
            });
        }

        void ImgTextRunMoveUpAndFadeAciton(GameObject imgText, float actionTime)
        {
            var startPos = textStartNode.transform.localPosition;
            var endPos = textEndNode.transform.localPosition;
            setImgTextSprite(imgText, curShowTextCount);
            imgText.transform.localPosition = startPos;
            imgText.GetComponent<CanvasGroup>().alpha = 0f;// 设置透明
            GameActionMgr.runMoveToAction(imgText, endPos, actionTime).SetEase(Ease.Linear); // 执行向上移动动作
            // 同时执行淡入淡出动作
            List<Tween> tweens = new List<Tween>() { 
                GameActionMgr.runFadeAction(imgText, 1, actionTime / 2),
                GameActionMgr.runFadeAction(imgText, 0, actionTime / 2),
            };
            var seqAct = GameActionMgr.runSequenceAction(imgText, tweens);
            seqAct.onComplete = () =>
            {
                if (curShowTextCount > curChapterTextNum)
                {
                    // 结束文本显示逻辑
                    imgText.SetActive(false);
                    if (!hasEnd) OnTextShowFinsh();
                    return;
                }
                // 递归执行下一句文本的动作直至所有文本显示完毕
                ImgTextRunMoveUpAndFadeAciton(imgText, actionTime);
            };
            curShowTextCount++; // 显示文本计数增加
        }

        private void OnTextShowFinsh()
        {
            hasEnd = true;
            // 同时显示当前章节名称标题
            imgTitle.SetActive(true);
            imgTitle.GetComponent<CanvasGroup>().alpha = 0f;// 设置透明
            List<Tween> tweens = new List<Tween>() {
                GameActionMgr.runFadeAction(imgTitle, 1f, 3),
                GameActionMgr.runFadeAction(imgTitle, 0f, 3),
            };
            var seqAct = GameActionMgr.runSequenceAction(imgTitle, tweens);
            seqAct.onComplete = () =>
            {
                var fadeAct = GameActionMgr.runFadeAction(maskBg, 0f, 1);
                fadeAct.onComplete = () =>
                {
                    AllowOpenMenu(true);
                };
                // 显示完所有的文本后点亮地图
                var targetMapName = chapterEndAutoSelectMapName[curChapterId];
                mapLogic.SelectPlaceLight(targetMapName);
            };
        }

        void setImgTextSprite(GameObject imgText, int textIndex)
        {
            var resTag = GameManager.GetCurLanguageResTag();
            var baseImgName = string.Format("text_{0}", textIndex);
            var realImgName = baseImgName + resTag;
            GameTools.loadTextureByAtlas(imgText, spriteAtlas, realImgName);
        }

        protected internal override void OnClose(bool isShutdown, object userData)
        {
            AllowOpenMenu(true);
            base.OnClose(isShutdown, userData);
            mapLogic.SetAllowEscapeClose(true);
            if (playerLogic != null) { playerLogic.DisablePlayerMove(false); }
            if (sceneManager != null)
            {
                sceneManager.canShowItemBag = true;
                sceneManager.canShowSaveGame = true;
                sceneManager.canShowLoadGame = true;
            }
        }

        public override void PlayerOpenAudio()
        {

        }
    }
}