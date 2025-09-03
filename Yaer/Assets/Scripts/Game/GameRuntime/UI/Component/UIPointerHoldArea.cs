using Game.GameMgr.Component.UI;
using Game.GameMgr;
using Game.GameRuntime.UI.FormLogic;
using Game.Static.Path;
using UnityEngine;
using UnityEngine.EventSystems;
using System;

namespace Game.GameRuntime.UI.Component
{
    public class UIPointerHoldArea : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public event Action onHoldProgressEnd;

        private PointerHoldFormLogic pointerHoldFormLogic = null;
        private string pointerHoldPanelPath => UIPrefabPath.GetUIPrefabPath("PointerHoldPanel");

        private void OnEnable()
        {
            var pointerHoldUI = GameManager.GetGMComponent<UIComponentGM>().GetUIForm(pointerHoldPanelPath);
            if (pointerHoldUI != null) 
            {
                this.pointerHoldFormLogic = pointerHoldUI.Logic as PointerHoldFormLogic;
            }
            else
            {
                GameManager.GetGMComponent<UIComponentGM>().OpenUIForm(pointerHoldPanelPath, EUIGroup.Top, new OpenFormArgs()
                {
                    callBack = pointerHoldLogic =>
                    {
                        this.pointerHoldFormLogic = pointerHoldLogic as PointerHoldFormLogic;
                    }
                });
            } 
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (pointerHoldFormLogic != null)
            {
                this.pointerHoldFormLogic.AddListener(OnHoldProgressEnd);
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (pointerHoldFormLogic != null)
            {
                this.pointerHoldFormLogic.RemoveListener(OnHoldProgressEnd);
            }
        }

        private void OnHoldProgressEnd()
        {
            this.pointerHoldFormLogic.RemoveListener(OnHoldProgressEnd);
            onHoldProgressEnd?.Invoke();
        }
    }
}

