using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.GameRuntime.UI.FormLogic.Menu.MainItemPage
{
    public class MenuFormMainItemBtnMask : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        // private Coroutine hoverCoroutine; // 悬停协程
        public MenuFormMainItemBtn button;

        // public float hoverDelay; // 悬停延迟时间
        private bool isHovering; // 是否正在悬停

        // 当鼠标悬停在按钮上时调用
        public void OnPointerEnter(PointerEventData eventData)
        {
            // 启动悬停协程
            isHovering = true;
            StartCoroutine(HoverCoroutine());
        }

        // 当鼠标离开按钮时调用
        public void OnPointerExit(PointerEventData eventData)
        {
            // 可选：添加鼠标离开时的其他逻辑
            isHovering = false;
            button.HideDetail();
        }

        private IEnumerator HoverCoroutine()
        {
            while (isHovering)
            {
                // Continuous display follow mouse
                button.ShowDetail();
                yield return null;
            }
        }
    }
}