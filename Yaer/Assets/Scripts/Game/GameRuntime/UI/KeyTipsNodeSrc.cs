using Game.GameMgr;
using Game.GameMgr.Component.Archive.ArchiveDataClass.Player;
using Game.GameRuntime.Entities.Monster;
using Game.GameRuntime.Entities.Player;
using Game.Static.Enum;
using Game.Static.Enum.Goods;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

// 按键提示脚本
public class KeyTipsNodeSrc : MonoBehaviour
{
    public GameObject textKey;
    public GameObject mouseLeft;
    public GameObject mouseRight;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // 显示按键提示
    // isTrigger：是否处于触发按键状态，触发时按键提示会变色
    public void ShowStoryTriggerEffect(bool isTrigger =false, ControlInputType inputType = ControlInputType.Interact)
    {
        var touchKey = GameManager.GetKeyStrByInputType(inputType);
        var isMouseKeyType = touchKey == "Mouse0" || touchKey == "Mouse1";
        textKey.SetActive(!isMouseKeyType);
        mouseLeft.SetActive(isMouseKeyType);
        mouseRight.SetActive(isMouseKeyType);
        if (isMouseKeyType)
        {
            if (touchKey == "Mouse0") { ShowMouseLeftType(isTrigger); }
            else if (touchKey == "Mouse1") { ShowMouseLeftType(isTrigger, false); }
        }
        else
        {
            var strArg = isTrigger ? "<color=red>{0}</color>" : "<color=white>{0}</color>";
            var keyText = string.Format(strArg, touchKey);
            GameTools.setTMPUGUIText(textKey, keyText);
        }
    }

    private void ShowMouseLeftType(bool isTrigger = false, bool isLeft = true)
    {
        mouseLeft.SetActive(isLeft);
        mouseRight.SetActive(!isLeft);
        var touchImg = UIUtils.findChild(mouseLeft, "touchImg");
        if (touchImg != null) { touchImg.SetActive(isTrigger); }
        var touchImg_2 = UIUtils.findChild(mouseRight, "touchImg");
        if (touchImg_2 != null) { touchImg_2.SetActive(isTrigger); }
    }
    
}
