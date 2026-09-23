using System;
using DG.Tweening;
using Game.GameMgr;
using Game.GameMgr.Component.UI;
using Game.GameRuntime.GameSceneManager.Component;
using Game.GameRuntime.UI.FormLogic.Black;
using Game.Static.Name.Res;
using Game.Static.Name.Settings;
using Game.Static.Path;
using UnityEngine;

namespace Game.GameRuntime.Entities.SceneEntities.Village_KenMuNi
{
    /// <summary>
    /// 村长家门口初次对话：靠近 <c>Npc_Chief</c> → 系统 BlackPanel 全黑 →
    /// 启用女二侧面涂层 → TriggerStory → 壳就绪 HideFade；对白结束关侧面，
    /// 再经日常黑幕 <see cref="LoadSceneComponentGSM.LoadScene"/> 进
    /// <see cref="SceneName.Village_Chief_House"/>。
    /// <para>
    /// 0902 产品改口：日常进屋用 BlackPanel；LoadingPanel 仅留给时间跳转。
    /// 推翻 0831「进屋=蛋糕读条」。侧面 = 世界 SR（SceneObject）；与 UI 正脸分离。
    /// </para>
    /// <para>
    /// 替代方案：再开 <c>LoadSceneWithLoadingPanel</c>（产品否，勿回潮）；
    /// 图末 <c>LoadSceneTaskAction</c>；对白结束常驻侧面——关 <see cref="hideSideOnStoryEnd"/>。
    /// </para>
    /// </summary>
    public class ChiefNearDoorStoryTrigger : SimpleStoryTrigger
    {
        /// <summary>Story 名钉死；场景序列化也应写同一字符串。</summary>
        public const string DoorStoryPrefabName = "Village_村长家门口初次对话";

        /// <summary>场景侧面涂层物体名（未绑引用时按名查找）。</summary>
        public const string SidePortraitObjectName = "GushaSidePortrait";

        [Header("黑幕（系统 BlackPanel）")]
        [Tooltip("壳就绪后极短 hold 再淡出；0～0.15 推荐。")]
        [SerializeField]
        private float shellReadyHoldSeconds = 0.1f;

        [Tooltip("壳未起来仍 HideFade，防永久卡黑。")]
        [SerializeField]
        private float storyCoverTimeoutSeconds = 8f;

        [Header("女二侧面涂层（世界 SR）")]
        [Tooltip("Objects/GushaSidePortrait；默认关，全黑后启用。留空则按名查找。")]
        [SerializeField]
        private GameObject gushaSidePortrait;

        [Tooltip("启用时强制 sortingLayer=SceneObject（验收：玩家在涂层下）。")]
        [SerializeField]
        private bool forceSceneObjectSorting = true;

        [Tooltip("SceneObject 层内 order；0～10 可调。")]
        [SerializeField]
        private int sidePortraitSortingOrder = 0;

        [Tooltip("对白结束关闭侧面（不二次黑幕）。产品若要常驻可关。")]
        [SerializeField]
        private bool hideSideOnStoryEnd = true;

        [Header("对白结束 → 进屋（黑幕）")]
        [Tooltip("门口初次对话结束后自动黑幕进村长家；与手动 House_Chief 并存。勿再勾 Loading。")]
        [SerializeField]
        private bool loadChiefHouseOnStoryEnd = true;

        [Header("对白结束 → 藏村长（0922）")]
        [Tooltip("合层装饰贴画「村长」；留空则按名在场景树深度查找。")]
        [SerializeField]
        private GameObject compositeChiefPortrait;

        /// <summary>合层装饰 SR 物体名（与场景 Unicode 名一致）。</summary>
        public const string CompositeChiefPortraitName = "村长";

        /// <summary>合层门贴画名；藏村长后应露门。磁盘默认可已亮，全黑内再确保 Active。</summary>
        public const string CompositeChiefDoorName = "村长家门";

        /// <summary>
        /// 0922 穿帮修复：对白结束布置（藏人/露门/关侧面）须在黑幕全黑后执行。
        /// true 时 <see cref="OnDoorStoryEndHideSide"/> 不再亮屏关侧面（改由 stayAction 做）。
        /// </summary>
        private bool _stageDoorEndUnderBlack;

        /// <summary>正在开黑 / 等壳 / 等超时；防 Enter 连打。</summary>
        private bool _orchestrating;

        private BlackFormLogic _blackForm;
        private bool _hideIssued;
        private bool _sideEndSubscribed;
        private Tweener _timeoutTween;
        private Tweener _holdTween;

        /// <summary>Enter → 先开黑，全黑后再插侧面 + 播剧情。</summary>
        protected override void TriggerStory()
        {
            if (_orchestrating)
            {
                return;
            }

            // 单次已用 / 已有剧情：绝不开黑（报告：同档再走近不黑幕）
            if (!CanStartStoryNow())
            {
                return;
            }

            _orchestrating = true;
            _hideIssued = false;
            OpenSystemBlackFade(OnBlackFullyShown);
        }

        private void OpenSystemBlackFade(Action<BlackFormLogic> onBlackReady)
        {
            var uiPath = UIPrefabPath.GetUIPrefabPath("BlackPanel");
            GameManager.GetGMComponent<UIComponentGM>().OpenUIForm(uiPath, EUIGroup.System, new OpenFormArgs
            {
                userData = new ShowBlackFormArgs
                {
                    showType = BlackFadeType.FadeShow,
                    onShowEnd = onBlackReady
                }
            });
        }

        private void OnBlackFullyShown(BlackFormLogic blackForm)
        {
            _blackForm = blackForm;

            var gsm = StoryGsm;
            if (gsm == null)
            {
                Debug.LogWarning("[ChiefNearDoor] StoryComponentGSM 缺失，直接 HideFade。", this);
                CloseBlackAndReset();
                return;
            }

            // ① 全黑内启用侧面（须在 HideFade 前；禁止亮屏后弹）
            EnableGushaSidePortrait();

            gsm.onStoryTriggered += OnStoryShellReady;
            ScheduleTimeout();

            // ② 同拍播门口三人戏
            if (!TryStartBoundStory())
            {
                // Prefab 缺失 / 拒播：仍须灭黑，不崩；侧面已启用则收回
                Debug.LogWarning(
                    $"[ChiefNearDoor] TriggerStory 未启动（Prefab 可能未就绪）：{DoorStoryPrefabName}",
                    this);
                gsm.onStoryTriggered -= OnStoryShellReady;
                CancelTimeout();
                SetGushaSidePortraitActive(false);
                CloseBlackAndReset();
                return;
            }

            SubscribeSideHideOnStoryEnd();
        }

        /// <summary>黑幕内点亮侧面并钉 SceneObject 层。</summary>
        private void EnableGushaSidePortrait()
        {
            var side = ResolveSidePortrait();
            if (side == null)
            {
                Debug.LogWarning(
                    $"[ChiefNearDoor] 未找到 {SidePortraitObjectName}，跳过侧面涂层。",
                    this);
                return;
            }

            if (forceSceneObjectSorting)
            {
                var sr = side.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.sortingLayerName = SortingLayerName.SceneObject;
                    sr.sortingOrder = sidePortraitSortingOrder;
                }
            }

            side.SetActive(true);
        }

        private void SetGushaSidePortraitActive(bool active)
        {
            var side = ResolveSidePortrait();
            if (side != null)
            {
                side.SetActive(active);
            }
        }

        private GameObject ResolveSidePortrait()
        {
            if (gushaSidePortrait != null)
            {
                return gushaSidePortrait;
            }

            // GameObject.Find 找不到未激活物体；Objects 下 Find 可含 inactive
            var objectsRoot = GameObject.Find("Objects");
            if (objectsRoot != null)
            {
                var t = objectsRoot.transform.Find(SidePortraitObjectName);
                if (t != null)
                {
                    gushaSidePortrait = t.gameObject;
                }
            }

            return gushaSidePortrait;
        }

        private void SubscribeSideHideOnStoryEnd()
        {
            if (!hideSideOnStoryEnd || _sideEndSubscribed)
            {
                return;
            }

            var gsm = StoryGsm;
            if (gsm == null)
            {
                return;
            }

            gsm.onStoryEnd += OnDoorStoryEndHideSide;
            _sideEndSubscribed = true;
        }

        private void OnDoorStoryEndHideSide()
        {
            var gsm = StoryGsm;
            if (gsm != null)
            {
                gsm.onStoryEnd -= OnDoorStoryEndHideSide;
            }

            _sideEndSubscribed = false;

            // 0922：进屋/黑幕布置路径会在全黑后关侧面；此处亮屏关会穿帮
            if (_stageDoorEndUnderBlack || !hideSideOnStoryEnd)
            {
                return;
            }

            SetGushaSidePortraitActive(false);
        }

        /// <summary>
        /// 0902 F1：对白结束 → 日常黑幕进屋。
        /// 0922：<b>禁止</b>在亮屏当帧藏村长/关侧面（穿帮）；布置一律进黑幕全黑后的 stayAction。
        /// </summary>
        protected override void OnStoryFinished()
        {
            base.OnStoryFinished();

            // 防误伤：Story 名须钉死门口初次对话（场景改绑其它剧情时不藏人/不进屋）
            if (ResolveStoryPrefabName() != DoorStoryPrefabName)
            {
                Debug.LogWarning(
                    $"[ChiefNearDoor] 跳过藏人/自动进屋：Story={ResolveStoryPrefabName()} ≠ {DoorStoryPrefabName}",
                    this);
                return;
            }

            _stageDoorEndUnderBlack = true;

            if (!loadChiefHouseOnStoryEnd)
            {
                // 不进屋：仍开一拍黑幕做布置，再淡出回村（避免露景看到人消失）
                StageDoorEndUnderBlackThenReveal();
                return;
            }

            var loadGsm = SceneManager?.GetModule<LoadSceneComponentGSM>();
            if (loadGsm == null)
            {
                Debug.LogError("[ChiefNearDoor] LoadSceneComponentGSM 缺失，无法黑幕进屋；回退亮屏布置。", this);
                ApplyDoorEndStagingUnderBlack();
                _stageDoorEndUnderBlack = false;
                return;
            }

            Debug.Log(
                $"[ChiefNearDoor] 对白结束 → LoadScene({SceneName.Village_Chief_House})；布置延后到黑幕全黑",
                this);
            // stayAction：黑幕 FadeShow 完成时调用（仍在本场景卸载前）→ 藏人/露门/关侧面，玩家看不见
            loadGsm.LoadScene(
                SceneName.Village_Chief_House,
                stayAction: ApplyDoorEndStagingUnderBlack);
        }

        /// <summary>
        /// 黑幕全黑后布置门口：藏 Npc+合层村长、确保门贴画亮、关侧面涂层。
        /// 由 LoadScene stayAction 或「不进屋」黑幕路径调用。
        /// </summary>
        private void ApplyDoorEndStagingUnderBlack()
        {
            HideChiefNearDoorVisuals();
            EnsureChiefDoorVisible();
            if (hideSideOnStoryEnd)
            {
                SetGushaSidePortraitActive(false);
            }

            _stageDoorEndUnderBlack = false;
        }

        /// <summary>
        /// 不自动进屋时：开黑 → 全黑布置 → 淡出。与进剧情黑幕同 API。
        /// </summary>
        private void StageDoorEndUnderBlackThenReveal()
        {
            OpenSystemBlackFade(black =>
            {
                ApplyDoorEndStagingUnderBlack();
                if (black != null)
                {
                    black.CloseFormFade(null);
                }
            });
        }

        /// <summary>
        /// 藏门口交互体 + 合层贴画。Npc_Chief 挂在本 GO 上；合层「村长」无 Logic，只能 SetActive。
        /// </summary>
        public void HideChiefNearDoorVisuals()
        {
            if (gameObject.activeSelf)
            {
                gameObject.SetActive(false);
            }

            var portrait = ResolveCompositeChiefPortrait();
            if (portrait != null && portrait.activeSelf)
            {
                portrait.SetActive(false);
            }
        }

        /// <summary>全黑内确保合层「村长家门」亮着（藏人后露门；已亮则无操作）。</summary>
        private void EnsureChiefDoorVisible()
        {
            var door = FindSceneObjectByName(CompositeChiefDoorName);
            if (door != null && !door.activeSelf)
            {
                door.SetActive(true);
            }
        }

        /// <summary>解析合层「村长」；优先序列化引用，再深度按名（含未激活父节点下的 Find）。</summary>
        private GameObject ResolveCompositeChiefPortrait()
        {
            if (compositeChiefPortrait != null)
            {
                return compositeChiefPortrait;
            }

            compositeChiefPortrait = FindSceneObjectByName(CompositeChiefPortraitName);
            return compositeChiefPortrait;
        }

        /// <summary>
        /// 本 Trigger 所在场景根下深度按名查找（可找到未激活子物体）。
        /// 优先 gameObject.scene，避免 Awake/换场时 GetActiveScene 指错场。
        /// </summary>
        private GameObject FindSceneObjectByName(string objectName)
        {
            var scene = gameObject.scene;
            if (!scene.IsValid())
            {
                scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            }

            if (!scene.IsValid())
            {
                return null;
            }

            var roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                var found = FindDeepChildByName(roots[i].transform, objectName);
                if (found != null)
                {
                    return found.gameObject;
                }
            }

            return null;
        }

        private static Transform FindDeepChildByName(Transform parent, string objectName)
        {
            if (parent.name == objectName)
            {
                return parent;
            }

            for (int i = 0; i < parent.childCount; i++)
            {
                var found = FindDeepChildByName(parent.GetChild(i), objectName);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private void OnStoryShellReady()
        {
            var gsm = StoryGsm;
            if (gsm != null)
            {
                gsm.onStoryTriggered -= OnStoryShellReady;
            }

            CancelTimeout();

            // 极短 hold 后再淡出，避免壳未 Instantiated 就露景
            CancelHold();
            if (shellReadyHoldSeconds <= 0f)
            {
                HideBlackAfterShellReady();
                return;
            }

            _holdTween = GameActionMgr.runDelayTimeAction(shellReadyHoldSeconds, HideBlackAfterShellReady);
        }

        private void OnCoverTimeout()
        {
            if (_hideIssued)
            {
                return;
            }

            Debug.LogWarning("[ChiefNearDoor] 壳就绪超时，强制 HideFade。", this);
            var gsm = StoryGsm;
            if (gsm != null)
            {
                gsm.onStoryTriggered -= OnStoryShellReady;
            }

            HideBlackAfterShellReady();
        }

        private void HideBlackAfterShellReady()
        {
            if (_hideIssued)
            {
                return;
            }

            _hideIssued = true;
            CancelHold();
            CancelTimeout();

            var black = _blackForm;
            _blackForm = null;

            if (black == null)
            {
                _orchestrating = false;
                return;
            }

            black.CloseFormFade(() =>
            {
                // 对白仍在播；侧面仍亮，等 onStoryEnd 关
                _orchestrating = false;
            });
        }

        private void CloseBlackAndReset()
        {
            _hideIssued = true;
            CancelHold();
            CancelTimeout();

            var black = _blackForm;
            _blackForm = null;
            if (black != null)
            {
                black.CloseFormFade(() => _orchestrating = false);
            }
            else
            {
                _orchestrating = false;
            }
        }

        private void ScheduleTimeout()
        {
            CancelTimeout();
            if (storyCoverTimeoutSeconds <= 0f)
            {
                return;
            }

            _timeoutTween = GameActionMgr.runDelayTimeAction(storyCoverTimeoutSeconds, OnCoverTimeout);
        }

        private void CancelTimeout()
        {
            if (_timeoutTween != null && _timeoutTween.IsActive())
            {
                _timeoutTween.Kill();
            }

            _timeoutTween = null;
        }

        private void CancelHold()
        {
            if (_holdTween != null && _holdTween.IsActive())
            {
                _holdTween.Kill();
            }

            _holdTween = null;
        }

        private void OnDestroy()
        {
            CancelHold();
            CancelTimeout();
            var gsm = StoryGsm;
            if (gsm != null)
            {
                gsm.onStoryTriggered -= OnStoryShellReady;
                if (_sideEndSubscribed)
                {
                    gsm.onStoryEnd -= OnDoorStoryEndHideSide;
                    _sideEndSubscribed = false;
                }
            }
        }
    }
}
