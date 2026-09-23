using Game.GameMgr;
using Game.GameMgr.Component.UI;
using Game.Static.Path;
using UnityEngine;

namespace Game.GameRuntime.UI.FormLogic.Menu
{
    /// <summary>
    /// 挂在 MenuPanel 的 <b>Center</b> 节点上：当 <see cref="ItemShowFormLogic"/>（贵重物品/道具展示）打开时隐藏 Center，
    /// 关闭 ItemShowPanel 后再显示。使用 <see cref="CanvasGroup"/> 控制显隐，避免 SetActive(false) 导致本脚本无法收到关闭事件。
    /// </summary>
    /// <remarks>
    /// 0922 背包点地图：ItemMap 若先关 Menu 再关 ItemShow，本脚本 OnDisable 已退订，会错过 OnPanelClosed，
    /// 池化重开后 Center 仍 alpha=0 → 只剩外侧 ButtonMoney（金币），按钮全「没了」。
    /// OnEnable 必须按 ItemShow 是否仍在栈上重算显隐。
    /// </remarks>
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
            // 池化重开 / 关菜单早于关道具：按现网是否仍开着 ItemShow 纠正 Center
            RefreshCenterVisibilityFromItemShowState();
        }

        private void OnDisable()
        {
            ItemShowFormLogic.OnPanelOpened -= OnItemShowOpened;
            ItemShowFormLogic.OnPanelClosed -= OnItemShowClosed;
        }

        /// <summary>
        /// ItemShow 仍开着 → 藏 Center；否则强制显示（清掉残留 alpha=0）。
        /// </summary>
        private void RefreshCenterVisibilityFromItemShowState()
        {
            if (IsItemShowPanelOpen())
            {
                ApplyHidden();
            }
            else
            {
                ApplyVisible();
            }
        }

        private static bool IsItemShowPanelOpen()
        {
            var uiGm = GameManager.GetGMComponent<UIComponentGM>();
            if (uiGm == null)
            {
                return false;
            }

            string path = UIPrefabPath.GetUIPrefabPath("ItemShowPanel");
            return uiGm.GetUIForm(path) != null;
        }

        private void OnItemShowOpened()
        {
            ApplyHidden();
        }

        private void OnItemShowClosed()
        {
            ApplyVisible();
        }

        private void ApplyHidden()
        {
            if (_canvasGroup == null)
            {
                return;
            }

            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }

        private void ApplyVisible()
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
