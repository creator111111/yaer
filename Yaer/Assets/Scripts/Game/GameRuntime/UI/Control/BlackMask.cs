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

        public void SetHidingState(bool state)
        {
            hiding = state;
            showing = state;
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

        /// <summary>
        /// 开始淡入黑幕
        /// </summary>
        public void ShowFade(Action endCallBack = null)
        {
            if (hiding)
            {
                Log.Warning("BlackPanel is already hiding!");
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
        /// 开始淡出黑幕
        /// </summary>
        public void HideFade(Action endCallBack = null)
        {
            if (hiding)
            {
                Log.Warning("BlackPanel is already showing!");
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