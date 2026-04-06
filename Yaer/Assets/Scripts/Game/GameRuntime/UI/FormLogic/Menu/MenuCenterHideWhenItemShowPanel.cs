using UnityEngine;

namespace Game.GameRuntime.UI.FormLogic.Menu
{
    /// <summary>
    /// 挂在 MenuPanel 的 <b>Center</b> 节点上：当 <see cref="ItemShowFormLogic"/>（贵重物品/道具展示）打开时隐藏 Center，
    /// 关闭 ItemShowPanel 后再显示。使用 <see cref="CanvasGroup"/> 控制显隐，避免 SetActive(false) 导致本脚本无法收到关闭事件。
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class MenuCenterHideWhenItemShowPanel : MonoBehaviour
    {
        private CanvasGroup _canvasGroup;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        private void OnEnable()
        {
            ItemShowFormLogic.OnPanelOpened += OnItemShowOpened;
            ItemShowFormLogic.OnPanelClosed += OnItemShowClosed;
        }

        private void OnDisable()
        {
            ItemShowFormLogic.OnPanelOpened -= OnItemShowOpened;
            ItemShowFormLogic.OnPanelClosed -= OnItemShowClosed;
        }

        private void OnItemShowOpened()
        {
            if (_canvasGroup == null)
            {
                return;
            }

            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }

        private void OnItemShowClosed()
        {
            if (_canvasGroup == null)
            {
                return;
            }

            _canvasGroup.alpha = 1f;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
        }
    }
}
