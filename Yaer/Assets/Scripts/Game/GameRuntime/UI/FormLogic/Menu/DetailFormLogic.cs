using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.GameRuntime.UI.FormLogic.Menu
{
    public class DetailFormLogic : MonoBehaviour
    {
        [SerializeField] private TMP_Text txInfo;
        [SerializeField] private GameObject textInfo;
        [SerializeField] private Canvas canvas; // 菜单界面的Canvas
        [SerializeField] private Canvas frameCanvas;
        [SerializeField] private Canvas charCanvas;
        [SerializeField] private Canvas frameCanvas2;
        [SerializeField] private Canvas charCanvas2;

        [Header("锚点定位")]
        [Tooltip("描述框左上角对齐到道具格右下角后，再叠加的屏幕空间偏移（世界 XY）。")]
        [SerializeField]
        private Vector2 cornerOffset = Vector2.zero;

        private RectTransform selfRectTransform;

        private void Awake()
        {
            selfRectTransform = GetComponent<RectTransform>();
        }

        /// <summary>
        /// 更新描述文案，并将描述框锚定到道具格右下角：
        /// 使描述框「左上角」与道具格 RectTransform 世界空间右下角重合（再叠加 cornerOffset）。
        /// </summary>
        public void UpdateInfo(string info, RectTransform anchorSlot)
        {
            frameCanvas.sortingOrder = canvas.sortingOrder + 1;
            charCanvas.sortingOrder = canvas.sortingOrder + 2;

            frameCanvas2.sortingOrder = canvas.sortingOrder + 1;
            charCanvas2.sortingOrder = canvas.sortingOrder + 2;
            txInfo.text = info;
            GameTools.setText(textInfo, info);
            textInfo.GetComponent<Text>().color = Color.white;

            SetPos(anchorSlot);
        }

        /// <summary>
        /// 将描述框摆到 anchorSlot 右下角外侧：保持 pivot 相对「描述框左上角」不变，把左上角移到格子右下角。
        /// </summary>
        private void SetPos(RectTransform anchorSlot)
        {
            if (anchorSlot == null || canvas == null || selfRectTransform == null)
            {
                return;
            }

            var corners = new Vector3[4];
            anchorSlot.GetWorldCorners(corners);
            Vector3 slotBottomRightWorld = corners[3];

            selfRectTransform.GetWorldCorners(corners);
            Vector3 tooltipTopLeftWorld = corners[1];
            Vector3 pivotWorld = selfRectTransform.position;

            // 左上角移到格子右下角：newPivot = slotBR + (pivot - tooltipTL)
            Vector3 newPivotWorld = slotBottomRightWorld + (pivotWorld - tooltipTopLeftWorld);
            newPivotWorld.x += cornerOffset.x;
            newPivotWorld.y += cornerOffset.y;
            newPivotWorld.z = pivotWorld.z;

            selfRectTransform.position = newPivotWorld;
        }
    }
}
