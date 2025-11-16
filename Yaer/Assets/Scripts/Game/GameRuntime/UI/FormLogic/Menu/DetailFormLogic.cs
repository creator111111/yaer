using System.Globalization;
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
        private RectTransform selfRectTransform;
        private RectTransform canvasRectTransform;
        private string nowItemName;

        private void Awake()
        {
            canvasRectTransform = canvas.GetComponent<RectTransform>();
            selfRectTransform = GetComponent<RectTransform>();
        }

        public void UpdateInfo(string info)
        {
            // 设置canvas
            frameCanvas.sortingOrder = canvas.sortingOrder + 1;
            charCanvas.sortingOrder = canvas.sortingOrder + 2;

            frameCanvas2.sortingOrder = canvas.sortingOrder + 1;
            charCanvas2.sortingOrder = canvas.sortingOrder + 2;
            txInfo.text = info;
            //这里设置的是Text，而非TMP_Text
            //GameTools.setText(textInfo, info);
            txInfo.color = Color.white;

            SetPos();
        }

        /// <summary>
        /// 设置显示位置
        /// </summary>
        private void SetPos()
        {
            // 获取鼠标在屏幕上的位置
            Vector2 mousePosition = Input.mousePosition;
            // Debug.Log(mousePosition);
            // 将鼠标坐标转换为UI坐标
            var isInside = RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRectTransform, mousePosition, canvas.worldCamera, out var uiPosition);
            // 检查鼠标是否在Canvas内
            if (isInside)
                // 设置UI对象的anchoredPosition为转换后的UI坐标
                selfRectTransform.anchoredPosition = uiPosition;
        }
    }
}