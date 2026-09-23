using System;
using System.Collections.Generic;
using Game.GameRuntime.Entities.Component.Anima;
using GameFramework.UnityRuntime.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace Game.GameRuntime.UI.Control
{
    public class BlackMask : MonoBehaviour
    {
        [SerializeField] private float showTime = 1;
        [SerializeField] private float hideTime = 1;
        [SerializeField] private Animator animator;

        private float defaultShowTime;
        private float defaultHideTime;
        private Image imgMask;
        private bool showing;
        private bool hiding;
        private Action onShowingAction;
        private AnimatorStateInfo stateInfo;
        private Dictionary<float, Action> showingActions = new Dictionary<float, Action>();
        private Dictionary<float, Action> hidingActions = new Dictionary<float, Action>();
        
        private Action onShowEnd;
        private Action onHideEnd;

        public bool Showing => showing;
        public bool Hiding => hiding;

        public float DefaultShowTime => defaultShowTime;
        public float DefaultHideTime => defaultHideTime;
        public float CurrentShowTime => showTime;
        public float CurrentHideTime => hideTime;

        public void SetHidingState(bool state)
        {
            hiding = state;
            showing = state;
        }

        /// <summary>
        /// 0923：强制清忙态（关残留 BlackPanel 前调用），避免 Showing/Hiding 脏导致下一次淡入淡出空 return。
        /// </summary>
        public void ForceClearBusyFlags()
        {
            showing = false;
            hiding = false;
            onShowEnd = null;
            onHideEnd = null;
            if (imgMask != null)
            {
                imgMask.raycastTarget = false;
            }
        }

        private void OnValidate()
        {
            if (animator == null)
            {
                animator = GetComponent<Animator>();
            }

            if (imgMask == null)
            {
                imgMask = GetComponent<Image>();
            }
        }

        public void OnInit()
        {
            defaultShowTime = showTime;
            defaultHideTime = hideTime;

            imgMask = GetComponent<Image>();
            animator = GetComponent<Animator>();

            GetComponent<AnimationEventComponent>().RegisterEvent("HideEnd", s =>
            {
                hiding = false;
                imgMask.raycastTarget = false;
                onHideEnd?.Invoke();
            });
            GetComponent<AnimationEventComponent>().RegisterEvent("ShowEnd", s =>
            {
                showing = false;
                imgMask.raycastTarget = false;
                onShowEnd?.Invoke();
            });
        }

        public void OnUpdate()
        {
            stateInfo = animator.GetCurrentAnimatorStateInfo(0);

            // Execute showing actions for every animation loop
            if (stateInfo.IsName("Show"))
            {
                TriggerActions(ref showingActions, stateInfo.normalizedTime);
            }

            // Execute hiding actions for every animation loop
            if (stateInfo.IsName("Hide"))
            {
                TriggerActions(ref hidingActions, stateInfo.normalizedTime);
            }
        }

        // Method to trigger actions for showing or hiding
        private void TriggerActions(ref Dictionary<float, Action> actions, float normalizedTime)
        {
            foreach (var action in actions)
            {
                // If the normalizedTime exceeds or matches the action's normalizedTime, execute the action
                if (normalizedTime >= action.Key)
                {
                    action.Value?.Invoke(); // Execute the action
                }
            }
        }

        /// <summary>临时覆盖淡入/淡出时长（秒）；<see cref="RestoreDefaultFadeDurations"/> 恢复 Prefab 默认。</summary>
        public void SetFadeDurations(float showSeconds, float hideSeconds)
        {
            if (showSeconds > 0f)
            {
                showTime = showSeconds;
            }

            if (hideSeconds > 0f)
            {
                hideTime = hideSeconds;
            }
        }

        /// <summary>恢复 OnInit 时缓存的默认 show/hide 时长。</summary>
        public void RestoreDefaultFadeDurations()
        {
            showTime = defaultShowTime;
            hideTime = defaultHideTime;
        }

        /// <summary>
        /// 开始淡入黑幕。
        /// 0923：忙态时排队，禁止空 return（否则 onShowEnd 永不到 → 换场卡死）。
        /// </summary>
        public void ShowFade(Action endCallBack = null)
        {
            if (hiding)
            {
                Log.Warning("BlackPanel ShowFade while hiding — defer until HideEnd");
                ChainAfterHide(() => ShowFade(endCallBack));
                return;
            }

            if (showing)
            {
                Log.Warning("BlackPanel ShowFade while already showing — chain onShowEnd");
                ChainAfterShow(endCallBack);
                return;
            }

            showing = true;
            imgMask.raycastTarget = true;
            animator.speed = 1 / showTime;
            onShowEnd = endCallBack;
            animator.SetTrigger("Show");
        }

        /// <summary>
        /// 直接显示黑幕
        /// </summary>
        public void ShowRow(Action endCallBack = null)
        {
            imgMask.raycastTarget = true;
            animator.speed = 1 / showTime;
            onShowEnd = endCallBack;
            animator.SetTrigger("ShowRow");
        }

        /// <summary>
        /// 开始淡出黑幕。
        /// 0923：Showing/Hiding 时排队或链式回调，禁止空 return（CloseFormHideFade 静默失败 → 永久黑）。
        /// </summary>
        public void HideFade(Action endCallBack = null)
        {
            if (hiding)
            {
                Log.Warning("BlackPanel HideFade while already hiding — chain onHideEnd");
                ChainAfterHide(endCallBack);
                return;
            }

            if (showing)
            {
                Log.Warning("BlackPanel HideFade while showing — defer until ShowEnd");
                ChainAfterShow(() => HideFade(endCallBack));
                return;
            }

            hiding = true;
            imgMask.raycastTarget = true;
            animator.speed = 1 / hideTime;
            onHideEnd = endCallBack;
            animator.SetTrigger("Hide");
        }

        public void HideRow(Action endCallBack = null)
        {
            imgMask.raycastTarget = true;
            animator.speed = 1 / hideTime;
            onHideEnd = endCallBack;
            animator.SetTrigger("HideRow");
        }

        /// <summary>在当前淡入结束后追加回调（保留已有 onShowEnd）。</summary>
        public void EnqueueAfterShow(Action action)
        {
            ChainAfterShow(action);
        }

        private void ChainAfterShow(Action action)
        {
            if (action == null)
            {
                return;
            }

            var prev = onShowEnd;
            onShowEnd = () =>
            {
                prev?.Invoke();
                action.Invoke();
            };
        }

        private void ChainAfterHide(Action action)
        {
            if (action == null)
            {
                return;
            }

            var prev = onHideEnd;
            onHideEnd = () =>
            {
                prev?.Invoke();
                action.Invoke();
            };
        }

        public void AddShowingAction(float normalize, Action action)
        {
            showingActions.Add(normalize, action);
        }

        public void AddHidingAction(float normalize, Action action)
        {
            hidingActions.Add(normalize, action);
        }
    }
}
