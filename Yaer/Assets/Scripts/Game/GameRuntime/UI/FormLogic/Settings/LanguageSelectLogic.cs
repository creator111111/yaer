using Game.GameMgr;
using Game.GameMgr.Component.UI;
using Game.GameMgr.Manager.Settings;
using Game.GameMgr.Manager.Settings.Helper;
using Game.GameRuntime.UI.FormLogic.Settings;
using Game.GameRuntime.UI.FormLogic.Start;
using Game.Static.Path;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 语言选择脚本
public class LanguageSelectLogic : MonoBehaviour
{
    public GameObject closeBtn;
    public Toggle languageBtn;
    public GameObject selectItemArea;
    public SettingFormLogic settingFormLogic;
    public List<LanguageNodeLogic> itemNodeList;
   

    bool hasShowLanguageSelect = true;
    // Start is called before the first frame update
    void Start()
    {
        closeBtn.SetActive(false);
        GameTools.setObjectClickFunc(closeBtn, () =>
        {
            languageBtn.isOn = false;
        }, null, true, 1);
        languageBtn.onValueChanged.AddListener((isSelect) =>
        {
            selectLauangeBtn(isSelect);
        });

        languageBtn.isOn = false; // 默认不显示语言选项
        selectLauangeBtn(false);
    }

    void selectLauangeBtn(bool isSelect=true)
    {
        if (hasShowLanguageSelect == isSelect) { return; }
        hasShowLanguageSelect = isSelect;
        selectItemArea.SetActive(hasShowLanguageSelect);
        closeBtn.SetActive(hasShowLanguageSelect);
        if (isSelect)
        {
            onSelectOneItemBtn(GameManager.Instance.language);
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void onSelectOneItemBtn(LanguageEnumType type)
    {
        GameManager.Instance.language = type;// 设置新的语言
        // 保存到本地
        var oldType = PlayerPrefs.GetInt("LanguageType", -1);
        if ((LanguageEnumType)oldType != type)
        {
            PlayerPrefs.SetInt("LanguageType", (int)type);
            PlayerPrefs.Save();
            settingFormLogic.UpdateUI();
            string uiPrefabPath = UIPrefabPath.GetUIPrefabPath("StartPanel");
            var uiForm = GameManager.GetGMComponent<UIComponentGM>().GetUIForm(uiPrefabPath);
            if (uiForm != null && uiForm.Logic is StartFormLogic startFormLogic) {
                startFormLogic.UpdateUI();
            }
        }
        
        //settingManager.SettingData.gameLanguageType = type;
        //settingManager.SaveSetting(settingManager.SettingData);
        // 切换选项组的状态
        foreach (LanguageNodeLogic node in itemNodeList)
        {
            node.setSelectItem(node.languageType == type);
        }
    }
}
