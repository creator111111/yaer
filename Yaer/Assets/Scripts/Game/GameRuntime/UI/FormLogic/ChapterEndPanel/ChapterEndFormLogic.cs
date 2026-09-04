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
using Game.Static.Enum.Map;
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

        /// <summary>
        /// 预制体中章末标题与字幕区域的父节点（带 CanvasGroup，可做整块渐隐渐现）；未在 Inspector 赋值时在 OnInit 内按名称查找。
        /// 与文档第七节「root 渐隐渐现与 ESC 跳过」一致。
        /// </summary>
        public GameObject root;

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

        /// <summary>
        /// 是否处于「章节结束滚动字幕」播放阶段（此阶段 ESC 表示跳过，而非关界面）。
        /// 与开发说明一致：与 SetAllowEscapeClose / CloseFormOnEsc 语义区隔。
        /// </summary>
        bool isChapterEndTalkRolling;

        /// <summary>
        /// 大标题（imgBigTitle）渐隐渐现阶段：第七节要求与字幕同一 ESC 语义，一次跳过「剩余全部章末演出」的起点之一。
        /// </summary>
        bool chapterEndBigTitleRolling;

        /// <summary>
        /// <see cref="OnTextShowFinsh"/> 内 imgTitle 渐隐 + maskBg 渐隐链路未完全结束（含地图高亮后等效收尾前）。
        /// 此阶段按 ESC 须 Kill root/imgTitle/maskBg 上 Tween 并收束到与播完一致的终态。
        /// </summary>
        bool chapterEndRootOutroRolling;

        /// <summary>
        /// 防止「大标题 Tween 被 Kill 后 onComplete 仍触发」与「ESC 跳过」双通道重复进入 <see cref="OnOpenMapToNextTalk"/>。
        /// </summary>
        bool chapterEndMapFlowEntered;

        /// <summary>
        /// 序章交错双行字幕时，第二行启动前的延迟 Tween；跳过或关闭时必须 Kill，避免延迟回调在跳过后仍启动第二行。
        /// </summary>
        Tween chapterEndTalkDelayTween;

        /// <summary>
        /// ESC 跳过字幕时 MapPanel 尚未 Open 回调（mapLogic 仍空）：已收束字幕视觉，等地图就绪后再走 <see cref="OnTextShowFinsh"/>。
        /// 原因：OpenMap 与 StartShowTalkText 并行，字幕滚动可能早于 mapLogic 赋值。
        /// </summary>
        bool pendingSkipRollingWhenMapReady;

        /// <summary>
        /// 字幕自然结束时 mapLogic 仍空：等 Open 回调后再 <see cref="MapFormLogic.SelectPlaceLight"/>。
        /// </summary>
        bool pendingSelectPlaceLightWhenMapReady;

        SpriteAtlas spriteBigTitleAtlas;
        PlayerLogic playerLogic;
        BaseGameSceneManager sceneManager;
        protected internal override void OnInit(object userData)
        {
            base.OnInit(userData);
            // 第七节：root 为章末演出容器，与预制体 Hierarchy 中节点名一致；未拖引用时自动查找，避免漏配。
            if (root == null)
            {
                var rootTransform = transform.Find("root");
                if (rootTransform != null)
                {
                    root = rootTransform.gameObject;
                }
            }

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
            chapterEndMapFlowEntered = false;
            chapterEndBigTitleRolling = false;
            chapterEndRootOutroRolling = false;
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
                // 重要：玩家实体未就绪时禁止空引用，否则 ChapterEndPanel 打开即炸，表现为「无大标题」
                if (playerLogic == null)
                {
                    Debug.LogWarning("[ChapterEnd] DisablePlayerLogic：玩家尚未就绪，跳过禁输入（面板仍继续）");
                    return;
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
            Debug.Log($"[ChapterEnd] StartChapterEndAni 大标题开始，chapter={curChapterId} textNum={curChapterTextNum}");
            chapterEndBigTitleRolling = true;
            imgBigTitle.SetActive(true);
            imgBigTitle.GetComponent<CanvasGroup>().alpha = 0f;// 设置透明
            List<Tween> tweens = new List<Tween>() {
                GameActionMgr.runFadeAction(imgBigTitle, 1f, 3),
                GameActionMgr.runFadeAction(imgBigTitle, 0f, 3),
            };
            var SeqAct = GameActionMgr.runSequenceAction(imgBigTitle, tweens);
            SeqAct.onComplete = () =>
            {
                chapterEndBigTitleRolling = false;
                // 标题消失后打开地图界面
                OnOpenMapToNextTalk();
            };
            
        }

        void OnOpenMapToNextTalk()
        {
            // 仅允许进入一次：正常播完回调与 ESC 跳过 Kill 后手动调用共用，避免重复打开黑幕/地图。
            if (chapterEndMapFlowEntered)
            {
                return;
            }

            chapterEndMapFlowEntered = true;
            chapterEndBigTitleRolling = false;
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

            // MP-1：开图前写入存档解锁，MapFormLogic.OnOpen → ShowUnlockPlace 才能正式点亮关卡（禁止只靠 SelectPlaceLight）。
            UnlockChapterEndMapPlace();

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

                    // MP-3：ESC 跳过字幕时若 map 尚未就绪，此处补跑收尾 / 高亮。
                    FlushPendingMapReadyActions();
                }
            });
        }

        /// <summary>
        /// 序章结束解锁本章地图关卡点。ButtonJingLingVillage → PlaceName.JingLingVillage（肯姆尼）。
        /// 本期不做 UnlockRoad；点选后进村由 MapFormLogic 保留 LoadScene。
        /// </summary>
        void UnlockChapterEndMapPlace()
        {
            if (!chapterEndAutoSelectMapName.TryGetValue(curChapterId, out var buttonName) ||
                string.IsNullOrEmpty(buttonName))
            {
                Debug.LogWarning($"[MapSelect] 本章无自动选中关卡配置，跳过 UnlockPlace。chapter={curChapterId}");
                return;
            }

            // 约定：地图按钮名 = "Button" + Place 解锁键
            const string buttonPrefix = "Button";
            var placeKey = buttonName.StartsWith(buttonPrefix)
                ? buttonName.Substring(buttonPrefix.Length)
                : buttonName;
            if (string.IsNullOrEmpty(placeKey))
            {
                Debug.LogWarning($"[MapSelect] 无法从按钮名解析解锁键：{buttonName}");
                return;
            }

            var playerData = GameManager.GetGMComponent<PlayerDataComponentGM>();
            if (playerData == null)
            {
                Debug.LogWarning("[MapSelect] PlayerDataComponentGM 不可用，无法 UnlockPlace。");
                return;
            }

            var newlyAdded = playerData.UnlockPlace(placeKey);
            // 序章定稿解锁键应对齐 PlaceName.JingLingVillage（肯姆尼）
            if (curChapterId == 0 && placeKey != PlaceName.JingLingVillage)
            {
                Debug.LogWarning(
                    $"[MapSelect] 序章解锁键与 PlaceName.JingLingVillage 不一致：got={placeKey}，expected={PlaceName.JingLingVillage}");
            }

            Debug.Log(
                $"[MapSelect] 序章/章末解锁关卡 place={placeKey}（按钮={buttonName}），newlyAdded={newlyAdded}，chapter={curChapterId}");
        }

        /// <summary>
        /// MapPanel Open 回调到达后：消化「跳过字幕排队」与「高亮排队」。
        /// </summary>
        void FlushPendingMapReadyActions()
        {
            if (mapLogic == null)
            {
                return;
            }

            if (pendingSkipRollingWhenMapReady)
            {
                pendingSkipRollingWhenMapReady = false;
                pendingSelectPlaceLightWhenMapReady = false;
                if (!hasEnd)
                {
                    Debug.Log($"[ChapterEnd] FlushPending SkipRolling → OnTextShowFinsh，chapter={curChapterId}");
                    OnTextShowFinsh();
                }
                else if (chapterEndAutoSelectMapName.TryGetValue(curChapterId, out var targetMapName))
                {
                    mapLogic.SelectPlaceLight(targetMapName);
                }

                return;
            }

            if (pendingSelectPlaceLightWhenMapReady)
            {
                pendingSelectPlaceLightWhenMapReady = false;
                if (chapterEndAutoSelectMapName.TryGetValue(curChapterId, out var targetMapName))
                {
                    mapLogic.SelectPlaceLight(targetMapName);
                    Debug.Log($"[MapSelect] 延迟 SelectPlaceLight {targetMapName}，chapter={curChapterId}");
                }
            }
        }

        void StartShowTalkText()
        {
            curShowTextCount = 1;
            // 本章无结束句或句数为 0：不进入「可 ESC 跳过」的滚动阶段，避免误触与空引用
            if (curChapterTextNum <= 0 || curShowTextCount > curChapterTextNum)
            {
                return;
            }

            isChapterEndTalkRolling = true;
            var moveTime = 8f;
            ImgTextRunMoveUpAndFadeAciton(imgTextTalk_1, moveTime);
            // 记录延迟 Tween，便于 Skip 时 Kill（不传 link 时延迟与界面生命周期解绑，易产生幽灵回调）
            chapterEndTalkDelayTween = GameActionMgr.runDelayTimeAction(moveTime / 2, () =>
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
            // 无论自然播完还是 ESC 跳过，进入统一收尾后不再视为「滚动中」，避免 OnUpdate 重复跳过
            isChapterEndTalkRolling = false;
            chapterEndTalkDelayTween = null;
            hasEnd = true;
            // 同时显示当前章节名称标题
            imgTitle.SetActive(true);
            imgTitle.GetComponent<CanvasGroup>().alpha = 0f;// 设置透明
            // root 子树（imgTitle）+ maskBg 的渐隐链路由 ESC 跳过时需 Kill 并收束终态，见第七节
            chapterEndRootOutroRolling = true;
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
                    chapterEndRootOutroRolling = false;
                };
                // 显示完所有的文本后点亮地图（Unlock 已写入；此处仅高亮提示）
                var targetMapName = chapterEndAutoSelectMapName[curChapterId];
                if (mapLogic != null)
                {
                    mapLogic.SelectPlaceLight(targetMapName);
                }
                else
                {
                    pendingSelectPlaceLightWhenMapReady = true;
                    Debug.LogWarning(
                        $"[ChapterEnd] OnTextShowFinsh：mapLogic 仍空，排队 SelectPlaceLight={targetMapName}，chapter={curChapterId}");
                }
            };
        }

        void setImgTextSprite(GameObject imgText, int textIndex)
        {
            var resTag = GameManager.GetCurLanguageResTag();
            var baseImgName = string.Format("text_{0}", textIndex);
            var realImgName = baseImgName + resTag;
            GameTools.loadTextureByAtlas(imgText, spriteAtlas, realImgName);
        }

        /// <summary>
        /// 按 ESC 一次跳过当前及剩余全部滚动字幕，收敛到与正常播完相同的 <see cref="OnTextShowFinsh"/>。
        /// 替代方案：改为「只跳当前句」需在计数与双行交错上单独设计衔接，改动面更大；当前采用文档推荐的一次跳过全部。
        /// </summary>
        void SkipRollingSubtitles()
        {
            if (!isChapterEndTalkRolling || hasEnd || curChapterTextNum <= 0)
            {
                return;
            }

            // MP-3：OpenMap 与字幕并行，mapLogic 可能尚未赋值 —— 禁止静默 return，改为排队等 Open 回调。
            if (mapLogic == null)
            {
                pendingSkipRollingWhenMapReady = true;
                KillChapterEndTweenTargets(true);
                curShowTextCount = curChapterTextNum + 1;
                SnapChapterEndTalkToSkippedVisualState();
                // 停止「滚动中」标记，避免每帧重复 ESC；地图就绪后 FlushPending 再进 OnTextShowFinsh。
                isChapterEndTalkRolling = false;
                Debug.LogWarning(
                    $"[ChapterEnd] SkipRollingSubtitles deferred (mapLogic null)，chapter={curChapterId}");
                return;
            }

            // 第七节：字幕跳过须同时 Kill root/imgTitle/maskBg 等上可能存在的渐隐链，避免「字幕没了 root 动效卡一半」
            KillChapterEndTweenTargets(true);

            // 与「已播完所有句」等价，使后续若仍有残留逻辑读到计数时状态一致
            curShowTextCount = curChapterTextNum + 1;
            SnapChapterEndTalkToSkippedVisualState();

            Debug.Log($"[ChapterEnd] SkipRollingSubtitles chapter={curChapterId}, curShowTextCount={curShowTextCount} (equiv. all lines done)");

            // 复用现成收尾：地图 SelectPlaceLight、maskBg 渐隐等，与不按 ESC 播完保持一致
            OnTextShowFinsh();
        }

        /// <summary>
        /// 跳过后立即收束字幕节点表现（隐藏），避免停在半透明度或中途位置。
        /// </summary>
        void SnapChapterEndTalkToSkippedVisualState()
        {
            if (imgTextTalk_1 != null)
            {
                var cg1 = imgTextTalk_1.GetComponent<CanvasGroup>();
                if (cg1 != null)
                {
                    cg1.alpha = 0f;
                }

                imgTextTalk_1.SetActive(false);
            }

            if (imgTextTalk_2 != null)
            {
                var cg2 = imgTextTalk_2.GetComponent<CanvasGroup>();
                if (cg2 != null)
                {
                    cg2.alpha = 0f;
                }

                imgTextTalk_2.SetActive(false);
            }
        }

        /// <summary>
        /// 统一 Kill 文档第七节所列「root、maskBg、imgTitle、imgBigTitle、字幕行」上的 DOTween；complete=false 避免幽灵 onComplete。
        /// </summary>
        /// <param name="killSubtitleDelay">是否同时终止 <see cref="chapterEndTalkDelayTween"/>（双行交错延迟）。</param>
        void KillChapterEndTweenTargets(bool killSubtitleDelay)
        {
            if (killSubtitleDelay)
            {
                chapterEndTalkDelayTween?.Kill(false);
                chapterEndTalkDelayTween = null;
            }

            KillTweenOnGameObjectSafe(root);
            KillTweenOnGameObjectSafe(maskBg);
            KillTweenOnGameObjectSafe(imgTitle);
            KillTweenOnGameObjectSafe(imgBigTitle);
            KillTweenOnGameObjectSafe(imgTextTalk_1);
            KillTweenOnGameObjectSafe(imgTextTalk_2);
        }

        /// <summary>
        /// SetLink 绑定在 GameObject 上的 Tween 使用 DOTween.Kill(go) 清理；go 为空时忽略。
        /// </summary>
        static void KillTweenOnGameObjectSafe(GameObject go)
        {
            if (go == null)
            {
                return;
            }

            DOTween.Kill(go, false);
        }

        /// <summary>
        /// ESC 在大标题（imgBigTitle）渐隐渐现阶段：Kill 动效并直接进入与播完大标题后相同的 <see cref="OnOpenMapToNextTalk"/>。
        /// 替代方案：若策划要求黑幕也必须播完，可在此处改为仅快进 Tween 时长而非改流程入口。
        /// </summary>
        void SkipFromBigTitlePhase()
        {
            if (!chapterEndBigTitleRolling || hasEnd)
            {
                return;
            }

            chapterEndBigTitleRolling = false;
            if (imgBigTitle != null)
            {
                DOTween.Kill(imgBigTitle, false);
                var cg = imgBigTitle.GetComponent<CanvasGroup>();
                if (cg != null)
                {
                    cg.alpha = 0f;
                }

                imgBigTitle.SetActive(false);
            }

            Debug.Log($"[ChapterEnd] SkipFromBigTitlePhase chapter={curChapterId}");
            OnOpenMapToNextTalk();
        }

        /// <summary>
        /// ESC 在 <see cref="OnTextShowFinsh"/> 内 imgTitle/maskBg（及未来 root 整块）渐隐阶段：收束到与播完一致的终态，避免半透挡点击。
        /// </summary>
        void SkipRootOutroPhase()
        {
            if (!chapterEndRootOutroRolling || mapLogic == null)
            {
                return;
            }

            // 延迟若仍存在（极端时序）一并清理
            KillChapterEndTweenTargets(true);
            SnapChapterEndTalkToSkippedVisualState();
            if (imgTitle != null)
            {
                var titleCg = imgTitle.GetComponent<CanvasGroup>();
                if (titleCg != null)
                {
                    titleCg.alpha = 0f;
                }
            }

            if (maskBg != null)
            {
                var maskCg = maskBg.GetComponent<CanvasGroup>();
                if (maskCg != null)
                {
                    maskCg.alpha = 0f;
                }
            }

            // 预制体 root 带 CanvasGroup：当前逻辑未对 root 做渐隐；若后续策划加 root 整体淡入淡出，Kill 后此处应改为「该段结束态」表（与策划对表）
            if (root != null)
            {
                var rootCg = root.GetComponent<CanvasGroup>();
                if (rootCg != null)
                {
                    rootCg.alpha = 1f;
                }
            }

            chapterEndRootOutroRolling = false;
            var targetMapName = chapterEndAutoSelectMapName[curChapterId];
            mapLogic.SelectPlaceLight(targetMapName);
            AllowOpenMenu(true);
            Debug.Log($"[ChapterEnd] SkipRootOutroPhase chapter={curChapterId}");
        }

        /// <summary>
        /// 关闭界面时清理章末相关 Tween 与阶段标记，避免关闭后回调访问已销毁引用。
        /// </summary>
        void KillChapterEndTalkTweensOnClose()
        {
            KillChapterEndTweenTargets(true);
            isChapterEndTalkRolling = false;
            chapterEndBigTitleRolling = false;
            chapterEndRootOutroRolling = false;
            pendingSkipRollingWhenMapReady = false;
            pendingSelectPlaceLightWhenMapReady = false;
        }

        protected internal override void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(elapseSeconds, realElapseSeconds);
            if (!Input.GetKeyDown(KeyCode.Escape))
            {
                return;
            }

            // 不在此路由走 InputComponentGM.onEscPressed / CloseFormOnEsc，避免误关 ChapterEnd 或地图（地图已 SetAllowEscapeClose(false)）
            // 第七节：同一 ESC 语义覆盖「大标题 → 地图+字幕 → root/imgTitle 收尾」中仍剩余的演出
            if (chapterEndBigTitleRolling)
            {
                SkipFromBigTitlePhase();
            }
            else if (isChapterEndTalkRolling && !hasEnd)
            {
                SkipRollingSubtitles();
            }
            else if (chapterEndRootOutroRolling)
            {
                SkipRootOutroPhase();
            }
        }

        protected internal override void OnClose(bool isShutdown, object userData)
        {
            KillChapterEndTalkTweensOnClose();
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