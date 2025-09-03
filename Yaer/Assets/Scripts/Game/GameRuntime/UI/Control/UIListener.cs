using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.GameRuntime.UI.Control
{
    public class UIListener : MonoBehaviour,
        IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        public enum ButtonEvent
        {
            Normal,
            Highlighted,
            Pressed
        }

        [SerializeField] private Selectable control;
        [SerializeField] private ButtonEvent currentEvent = ButtonEvent.Normal;
        public ButtonEvent CurrentEvent => currentEvent;
        public Selectable Control => control;

        // 分别定义三个状态对应的事件（无参数，状态变化可以通过当前状态属性获取）
        public event Action<UIListener> OnNormal;
        public event Action<UIListener> OnHighlighted;
        public event Action<UIListener> OnPressed;

        private void OnValidate()
        {
            if (control == null)
            {
                control = GetComponent<Selectable>();
            }
        }

        private void OnEnable()
        {
            ResetNormalState();
        }

        public void ResetNormalState()
        {
            SetState(ButtonEvent.Normal);
        }
        
        /// <summary>
        /// 统一设置状态并触发对应事件
        /// </summary>
        /// <param name="newEvent">新状态</param>
        private void SetState(ButtonEvent newEvent)
        {
            if (currentEvent != newEvent && control.interactable)
            {
                currentEvent = newEvent;
                switch (currentEvent)
                {
                    case ButtonEvent.Normal:
                        OnNormal?.Invoke(this);
                        break;
                    case ButtonEvent.Highlighted:
                        OnHighlighted?.Invoke(this);
                        break;
                    case ButtonEvent.Pressed:
                        OnPressed?.Invoke(this);
                        break;
                }
            }
        }

        public void SetInteractive(bool isInteractive)
        {
            control.interactable = isInteractive;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (currentEvent != ButtonEvent.Pressed)
            {
                SetState(ButtonEvent.Highlighted);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (currentEvent != ButtonEvent.Pressed)
            {
                SetState(ButtonEvent.Normal);
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            SetState(ButtonEvent.Pressed);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            // 判断释放时是否在按钮区域内
            if (RectTransformUtility.RectangleContainsScreenPoint(
                    transform as RectTransform, eventData.position, eventData.pressEventCamera))
            {
                SetState(ButtonEvent.Highlighted);
            }
            else
            {
                SetState(ButtonEvent.Normal);
            }
        }
    }
}