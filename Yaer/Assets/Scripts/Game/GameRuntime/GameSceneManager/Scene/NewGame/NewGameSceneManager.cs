using System;
using Game.GameMgr;
using Game.GameMgr.Component;
using Game.GameMgr.Component.UI;
using Game.GameRuntime.GameSceneManager.Base;
using Game.GameRuntime.GameSceneManager.Component;
using Game.GameRuntime.GameSceneManager.Component.Story;
using Game.GameRuntime.GameSceneManager.Scene.Village_KenMuNi;
using Game.GameRuntime.UI.FormLogic.Black;
using Game.GameRuntime.UI.FormLogic.Cartoon;
using Game.GameRuntime.UI.FormLogic.Story.Dialogue;
using Game.Static.Name.Res;
using Game.Static.Path;
using Game.Static.Path.Sound;
using UnityEngine;

namespace Game.GameRuntime.GameSceneManager.Scene.NewGame
{
    /// <summary>
    /// 序章场景：漫画结束后按 KenMuNi 标准做分层亮屏
    /// （System BlackPanel 拍1 HideFade + Gate + Prefab 串行 0.5）。
    /// 详见：执行文档 0807 NewGameStory 开场间隔对齐 KenMuNi（方案 D）。
    /// </summary>
    public class NewGameSceneManager : BaseGameSceneManager
    {
        const string NewGameStoryName = "NewGameStory";

        /// <summary>等壳 Open + Prefab 实例化出全屏 BG 的极短 hold（对齐村 VillageStartBgReadyHoldSeconds）。</summary>
        const float StoryBgReadyHoldSeconds = 0.15f;

        /// <summary>壳/Prefab 失败时强制开闸的超时（秒）。</summary>
        const float StoryCoverTimeoutSeconds = 3f;

        private NewGameCartoonFormLogic cartoonFormLogic;

        /// <summary>拍1 用的 System 黑幕；漫画 Form 黑幕在 Bottom，关后挡不住 Middle 对话壳。</summary>
        BlackFormLogic layeredRevealBlackForm;

        /// <summary>防止 onStoryTriggered 与超时双开 HideFade。</summary>
        bool newGameCoverCloseIssued;

        public override void OnInit()
        {
            base.OnInit();
            nowSceneName = SceneName.NewGameScene;
        }

        public override void OnEnterScene()
        {
            base.OnEnterScene();
            GetModule<InputComponentGSM>().SetAllowOpenMenu(false);
            // 漫画面板
            GameManager.GetGMComponent<UIComponentGM>().OpenUIForm(UIPrefabPath.GetUIPrefabPath("NewGameCartoonPanel"), EUIGroup.Bottom, new OpenFormArgs()
            {
                callBack = logic =>
                {
                    if (logic is NewGameCartoonFormLogic cartoonFormLogic)
                    {
                        this.cartoonFormLogic = cartoonFormLogic;

                        // 漫画 Form 仅 ShowFade 全黑后回调；关 Form / 拍1 由本旁路接管（方案 D）
                        cartoonFormLogic.GetProxy<NewGameCartoonFormProxy>().onFinishEvent = OnCartoonFinishedFullyBlack;
                    }
                }
            });
        }

        /// <summary>
        /// 漫画已全黑：接手 System BlackPanel → 关漫画 → Gate Reset → Trigger → Prepare → HideFade=拍1 → Signal。
        /// BGM 仍在此步播放（产品保留）。
        /// 替代方案：关漫画=露景再分层（易露景漏缝，已否决）。
        /// </summary>
        void OnCartoonFinishedFullyBlack()
        {
            newGameCoverCloseIssued = false;

            // 开始播放剧情音乐（与分层前一致，不绑在首句）
            GameManager.GetGMComponent<SoundComponentGM>().PlaySound(SoundType.BGM, "龙宫内BGM.ogg", true);

            // 漫画黑幕在 Bottom；对话壳在 Middle。须先挂 System BlackPanel，再关漫画，否则会闪露。
            GameManager.GetGMComponent<UIComponentGM>().OpenUIForm(
                UIPrefabPath.GetUIPrefabPath("BlackPanel"),
                EUIGroup.System,
                new OpenFormArgs()
                {
                    userData = new ShowBlackFormArgs()
                    {
                        showType = BlackFadeType.RawShow,
                        onShowEnd = OnSystemBlackReadyForLayeredReveal
                    }
                });
        }

        void OnSystemBlackReadyForLayeredReveal(BlackFormLogic blackFormLogic)
        {
            layeredRevealBlackForm = blackFormLogic;

            // System 已盖住：可安全关掉漫画 Form（带走其 Bottom 黑幕）
            CloseCartoonFormIfAny();

            var storyGsm = GetModule<StoryComponentGSM>();
            if (storyGsm == null)
            {
                Debug.LogWarning("[NewGameStory] StoryComponentGSM 缺失，直接 HideFade 兜底");
                HideSystemBlackAndSignal();
                return;
            }

            // 锁闸：Prefab Wait 须等拍1 黑幕淡完才开始「只见 BG」空拍
            VillageStartLayerRevealGate.ResetForDeferredCover();
            storyGsm.onStoryTriggered += OnNewGameStoryTriggeredForCover;

            bool started = storyGsm.TriggerStory(NewGameStoryName);
            if (!started)
            {
                storyGsm.onStoryTriggered -= OnNewGameStoryTriggeredForCover;
                Debug.LogWarning("[NewGameStory] TriggerStory 未启动，回退 HideFade");
                HideSystemBlackAndSignal();
                return;
            }

            Debug.Log("[NewGameStory] 全黑 TriggerStory，等待壳 Ready 后分层亮屏");
            WaitForInvoke(StoryCoverTimeoutSeconds, OnNewGameCoverTimeout);
        }

        void OnNewGameStoryTriggeredForCover()
        {
            var storyGsm = GetModule<StoryComponentGSM>();
            if (storyGsm != null)
            {
                storyGsm.onStoryTriggered -= OnNewGameStoryTriggeredForCover;
            }

            // 壳已回调；再等极短 hold 让 Prefab Instantiate + 全屏 BG 就绪，然后 Prepare + 拍1
            WaitForInvoke(StoryBgReadyHoldSeconds, FinalizeNewGameCoverAndHideBlack);
        }

        void OnNewGameCoverTimeout()
        {
            if (newGameCoverCloseIssued)
            {
                return;
            }

            Debug.LogWarning("[NewGameStory] BG Ready 超时，强制 Prepare 后 HideFade");
            FinalizeNewGameCoverAndHideBlack();
        }

        void FinalizeNewGameCoverAndHideBlack()
        {
            if (newGameCoverCloseIssued)
            {
                return;
            }

            newGameCoverCloseIssued = true;

            PrepareNewGameLayeredReveal();
            Debug.Log("[NewGameStory] Prepare 完成，HideFade（拍1）");
            HideSystemBlackAndSignal();
        }

        /// <summary>
        /// 拍1：System 黑幕淡出，完成回调开闸（对齐村 CloseFormFade + Signal）。
        /// </summary>
        void HideSystemBlackAndSignal()
        {
            var black = layeredRevealBlackForm;
            layeredRevealBlackForm = null;

            if (black == null)
            {
                Debug.LogWarning("[NewGameStory] 无 System BlackPanel，强制开闸");
                VillageStartLayerRevealGate.SignalBgFullyVisible();
                return;
            }

            black.CloseFormFade(() =>
            {
                VillageStartLayerRevealGate.SignalBgFullyVisible();
                Debug.Log("[NewGameStory] 黑幕淡完，分层闸门开启（可开始 BG 空拍）");
            });
        }

        /// <summary>
        /// 分层显现准备（白名单）：只藏字幕条 + DialogueScene 下 <c>YaerPainting</c>；BG 保持可见。
        /// 并关掉故事根 Animator，避免 Start.anim / Write Defaults 抢写 alpha。
        /// <para>禁止名字广扫整棵 Panel（会误伤 Mask 同名 Painting → 小头像黑窗）。</para>
        /// </summary>
        void PrepareNewGameLayeredReveal()
        {
            var uiPath = UIPrefabPath.GetUIPrefabPath("NormalDialogueNewPanel");
            var uiForm = GameManager.GetGMComponent<UIComponentGM>().GetUIForm(uiPath);
            if (uiForm == null || uiForm.Logic == null)
            {
                Debug.LogWarning("[NewGameStory][Prepare] 对话壳未就绪，跳过白名单");
                return;
            }

            var logicRoot = uiForm.Logic;

            // —— 白名单 1：字幕条（拍3 由 Prefab NormalDialogueUIAlpha 拉回）——
            if (logicRoot is NormalDialogueFormNewLogic dialogueLogic
                && dialogueLogic.dialogueUICanvasGroup != null)
            {
                dialogueLogic.dialogueUICanvasGroup.alpha = 0f;
            }

            var sceneRoot = UIUtils.findChild(logicRoot.gameObject, "DialogueSceneContainer", hasDebugLog: false);
            if (sceneRoot == null)
            {
                Debug.LogWarning("[NewGameStory][Prepare] 未找到 DialogueSceneContainer，跳过场景立绘白名单");
                return;
            }

            // —— 白名单 2：全屏 BG 兜底 Active——
            var bg = UIUtils.findChild(sceneRoot, "BG", hasDebugLog: false);
            if (bg != null && !bg.activeSelf)
            {
                bg.SetActive(true);
            }

            // —— 白名单 3：YaerPainting 藏起 + 关掉故事根 Animator ——
            // 原因：Start 状态若写 alpha / Write Defaults=ON，会每帧抢 alpha，分层空拍失效。
            // 淡入完成后再由 CanvasGroupAlpha 开回 Animator 并落到 YaerShow 末帧（供 KingMove）。
            SetScenePaintingCanvasGroupAlpha(sceneRoot, "YaerPainting", 0f);
            DisableStoryRootAnimator(sceneRoot);
        }

        static void SetScenePaintingCanvasGroupAlpha(GameObject sceneRoot, string paintingObjectName, float alpha)
        {
            var paintingGo = UIUtils.findChild(sceneRoot, paintingObjectName, hasDebugLog: false);
            if (paintingGo == null)
            {
                return;
            }

            var cg = paintingGo.GetComponent<CanvasGroup>();
            if (cg == null)
            {
                return;
            }

            cg.alpha = alpha;
            Debug.Log("[NewGameStory][Prepare] hide " + paintingObjectName);
        }

        /// <summary>关掉 DialogueScene 根上的 Animator，避免与 CanvasGroup Fade 抢写。</summary>
        static void DisableStoryRootAnimator(GameObject sceneRoot)
        {
            if (sceneRoot == null)
            {
                return;
            }

            // Prefab 故事根通常是 DialogueSceneContainer 下第一个带 Animator 的子物体
            var animators = sceneRoot.GetComponentsInChildren<Animator>(true);
            for (var i = 0; i < animators.Length; i++)
            {
                var anim = animators[i];
                if (anim == null)
                {
                    continue;
                }

                // 只关「故事演出」Animator（带 YaerShow 参数的）；避免误伤无关 UI Animator
                foreach (var p in anim.parameters)
                {
                    if (p.name == "YaerShow" || p.name == "KingMove")
                    {
                        anim.enabled = false;
                        Debug.Log("[NewGameStory][Prepare] disable Animator on " + anim.gameObject.name);
                        return;
                    }
                }
            }
        }

        void CloseCartoonFormIfAny()
        {
            if (cartoonFormLogic == null)
            {
                return;
            }

            var form = cartoonFormLogic.UIForm;
            cartoonFormLogic = null;
            if (form != null)
            {
                GameManager.GetGMComponent<UIComponentGM>().CloseUIForm(form);
            }
        }

        public override void OnShutDown()
        {
            base.OnShutDown();

            var storyGsm = GetModule<StoryComponentGSM>();
            if (storyGsm != null)
            {
                storyGsm.onStoryTriggered -= OnNewGameStoryTriggeredForCover;
            }

            CloseCartoonFormIfAny();

            // 场景关掉时若分层未走完，强制开闸并清掉残留 System 黑幕，避免污染下一场景/DialogDebug
            if (layeredRevealBlackForm != null)
            {
                var black = layeredRevealBlackForm;
                layeredRevealBlackForm = null;
                black.CloseForm();
            }

            VillageStartLayerRevealGate.SignalBgFullyVisible();
        }

        public override void initAllSceneMonster()
        {
        }
    }
}
