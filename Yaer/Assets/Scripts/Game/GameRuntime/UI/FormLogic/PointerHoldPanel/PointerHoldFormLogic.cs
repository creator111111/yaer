using Game.GameRuntime.UI.FormLogic.Base;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Game.GameRuntime.UI.FormLogic
{
    public class PointerHoldFormLogic : BaseUIFormLogic
    {
        [SerializeField]
        private Slider HoldProgressSlider;

        [SerializeField]
        private float SliderShowTime;
        [SerializeField]
        private float MaxProgressTime;

        private float pointerHoldTime;
        private RectTransform HoldProgressSliderRtf;

        private bool hasListener = false;
        public event Action onHoldProgressEnd;

        protected internal override void OnInit(object userData)
        {
            base.OnInit(userData);
            HoldProgressSliderRtf = HoldProgressSlider.transform as RectTransform;
        }

        protected internal override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            HoldProgressSlider.gameObject.SetActive(false);
            pointerHoldTime = 0;
        }

        protected internal override void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(elapseSeconds, realElapseSeconds);
            if (hasListener)
            {
                pointerHoldTime += realElapseSeconds;

                if (pointerHoldTime > SliderShowTime)
                {
                    if (pointerHoldTime < MaxProgressTime)
                    {
                        HoldProgressSlider.gameObject.SetActive(true);
                        HoldProgressSlider.value = pointerHoldTime / MaxProgressTime;
                    }
                    else
                    {
                        OnHoldProgressEnd();
                    }
                }
            }
        }

        private void OnHoldProgressEnd()
        {
            onHoldProgressEnd?.Invoke();
            HoldProgressSlider.gameObject.SetActive(false);
        }

        public void AddListener(Action action)
        {
            hasListener = true;
            onHoldProgressEnd += action;
        }

        public void RemoveListener(Action action)
        {
            hasListener = false;
            onHoldProgressEnd -= action;

            pointerHoldTime = 0;
            HoldProgressSlider.gameObject.SetActive(false);
        }

        public override void PlayerOpenAudio()
        {

        }
    }
}