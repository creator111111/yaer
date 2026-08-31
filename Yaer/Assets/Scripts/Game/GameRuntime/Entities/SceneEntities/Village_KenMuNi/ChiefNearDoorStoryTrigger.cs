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
    /// 再经 LoadingPanel 进 <see cref="SceneName.Village_Chief_House"/>。
    /// <para>
    /// 0831：靠近黑幕只服务插层+三人戏；进屋另开 Loading，禁止用 BlackPanel 当进屋主表现。
    /// 侧面 = 世界 SR（SceneObject）；与 UI <c>GushaPainting</c> / Mask 正脸职责分离。
    /// </para>
    /// <para>
    /// 替代方案：图末 <c>LoadSceneTaskAction</c>（现网无 Loading，勿裸用）；
    /// 对白结束常驻侧面——关 <see cref="hideSideOnStoryEnd"/>；
    /// 仅解锁门仍点 E——产品要自动故否。
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

        [Header("对白结束 → 进屋（Loading）")]
        [Tooltip("门口初次对话结束后自动 Loading 进村长家；与手动 House_Chief 并存。")]
        [SerializeField]
        private bool loadChiefHouseOnStoryEnd = true;

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
            SetGushaSidePortraitActive(false);
        }

        /// <summary>
        /// L2：对白结束 → LoadingPanel → <c>Village_Chief_House</c>（blackFade:false）。
        /// 仅当本 Trigger 播的是门口初次对话，且开关开启；勿再 Open BlackPanel。
        /// </summary>
        protected override void OnStoryFinished()
        {
            base.OnStoryFinished();

            if (!loadChiefHouseOnStoryEnd)
            {
                return;
            }

            // 防误伤：Story 名须钉死门口初次对话（场景改绑其它剧情时不进屋）
            if (ResolveStoryPrefabName() != DoorStoryPrefabName)
            {
                Debug.LogWarning(
                    $"[ChiefNearDoor] 跳过自动进屋：Story={ResolveStoryPrefabName()} ≠ {DoorStoryPrefabName}",
                    this);
                return;
            }

            var loadGsm = SceneManager?.GetModule<LoadSceneComponentGSM>();
            if (loadGsm == null)
            {
                Debug.LogError("[ChiefNearDoor] LoadSceneComponentGSM 缺失，无法 Loading 进屋。", this);
                return;
            }

            Debug.Log(
                $"[ChiefNearDoor] 对白结束 → LoadSceneWithLoadingPanel({SceneName.Village_Chief_House})",
                this);
            loadGsm.LoadSceneWithLoadingPanel(SceneName.Village_Chief_House);
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
