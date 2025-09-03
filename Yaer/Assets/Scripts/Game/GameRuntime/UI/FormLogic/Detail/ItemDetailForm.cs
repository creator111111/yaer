using Game.GameMgr;
using Game.GameMgr.Component;
using Game.GameRuntime.UI.FormLogic.Base;
using UnityEngine;
using UnityEngine.UI;

namespace Game.GameRuntime.UI.FormLogic.Detail
{
    public class ItemDetailForm: BaseUIFormLogic
    {
        public Image imgBg;
        public Image imgFg;
        public Image imgChar; // 物品描述详细信息图片
        public RectTransform selfRectTransform;
        private RectTransform canvasRectTransform;
        private string nowItemName;
        private Sprite nowSprite;


        protected internal override void OnInit(object userData)
        {
            base.OnInit(userData);
            // canvasRectTransform = GameManager.GetComponent<UIComponentGM>().Canvas.GetComponent<RectTransform>();
        }

        public void UpdateInfo(Sprite sprite)
        {
            // imgChar.sprite = sprite;
            // imgChar.SetNativeSize();
            // // 根据文本图片自适应大小
            // var y = imgChar.rectTransform.sizeDelta.y;
            // // Debug.Log(y);
            // imgBg.rectTransform.sizeDelta = new Vector2(imgBg.rectTransform.sizeDelta.x, y + 20);
            // imgFg.rectTransform.sizeDelta = new Vector2(imgFg.rectTransform.sizeDelta.x, y + 40);
            //
            // // 获取鼠标在屏幕上的位置
            // Vector2 mousePosition = Input.mousePosition;
            // // Debug.Log(mousePosition);
            // // 将鼠标坐标转换为UI坐标
            // var isInside = RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRectTransform, mousePosition,
            //     GameManager.Instance.UISystem.Canvas.worldCamera, out var uiPosition);
            // // Debug.Log(uiPosition);
            // // 检查鼠标是否在Canvas内
            // if (isInside)
            //     // 设置UI对象的anchoredPosition为转换后的UI坐标
            //     selfRectTransform.anchoredPosition = uiPosition;
        }

        public override void PlayerOpenAudio()
        {

        }
    }
}