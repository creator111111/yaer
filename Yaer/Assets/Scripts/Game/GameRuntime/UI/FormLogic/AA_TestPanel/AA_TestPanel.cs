using Game.GameMgr;
using Game.GameMgr.Component;
using Game.GameMgr.Component.Archive.ArchiveDataClass.Player;
using Game.GameMgr.Component.UI;
using Game.GameRuntime.BagPack;
using Game.GameRuntime.Entities.Component.Battle.Damage;
using Game.GameRuntime.Entities.Player;
using Game.GameRuntime.Entities.Player.Components;
using Game.GameRuntime.UI.FormLogic.Base;
using Game.Static.Enum;
using Game.Static.Enum.Goods;
using Game.Static.Path;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game.GameRuntime.UI.FormLogic.AA_TestPanel
{
    // 测试指令界面
    public class AA_TestPanel : BaseUIFormLogic
    {
        public GameObject cmdItemNode;
        public GameObject cmdArea;
        public GameObject scrollContent;
        public Button closeBtn;
        public PlayerLogic playerLogic;

        Dictionary<string, Action<string[]>> actionDict = new Dictionary<string, Action<string[]>>() { };
        protected internal override void OnInit(object userData)
        {
            base.OnInit(userData);
            initActionDict();
            updateView();
            closeBtn.onClick.AddListener(()=>{
                CloseForm();
            });
        }
        protected internal override void OnOpen(object userData)
        {
            base.OnOpen(userData);

            playerLogic = GameManager.GetGMComponent<EntityComponentGM>().GetEntityLogic<PlayerLogic>();
            if (playerLogic != null)
            {
                playerLogic.isEnableNorAtk = false;
                playerLogic.isEnableQuickUseItem = false;
            }
            AllowOpenMenu(false);
        }
        protected internal override void OnClose(bool isShutdown, object userData)
        {
            base.OnClose(isShutdown, userData);
            if (playerLogic != null)
            {
                playerLogic.isEnableNorAtk = true;
                playerLogic.isEnableQuickUseItem = true;
            }
            AllowOpenMenu(true);
        }

        void updateView()
        {
            cmdItemNode.SetActive(true);
            foreach (var data in actionDict)
            {
                var actionName = data.Key;
                var actionCallFunc = data.Value;
                var cmdBtnArea = Instantiate(cmdItemNode, scrollContent.transform);
                setCmdData(cmdBtnArea, actionName, actionCallFunc);
            }
            cmdItemNode.SetActive(false);
        }

        private void setCmdData(GameObject cmdBtnArea, string actionName, Action<string[]> actionCallFunc)
        {
            var textName = UIUtils.findChild(cmdBtnArea, "textTitle");
            GameTools.setText(textName, actionName);
            var enterBtn = UIUtils.findChild(cmdBtnArea, "enterBtn");
            if (enterBtn != null)
            {
                GameTools.setObjectClickFunc(enterBtn, () =>
                {
                    var inputText = UIUtils.findChild(cmdBtnArea, "inputText");
                    if (inputText != null)
                    {
                        // 获取文本输入框的内容
                        var argStr = inputText.GetComponent<InputField>().text;
                        var strArray = argStr.Split(',');
                        //if (strArray.Length <= 0)
                        //{
                        //    UIUtils.showTips("参数输入错误!");
                        //    return;
                        //}
                        if (actionCallFunc != null)
                        {
                            actionCallFunc(strArray);
                        }
                        //if (isAutoClosePanel)
                        //{
                        //    enterBtn.transform.DOKill();
                        //    close();
                        //}
                    }
                });
            }
        }

        void initActionDict()
        {
            actionDict["设置成就进度(成就ID,进度)"] = (argStr) =>
            {
                // 参数格式(怪物UID,新被动ID)
                var valueList = getValueListFromArgs(argStr);
                var achieveId = valueList.Count > 0 ? valueList[0] : 0;
                var proValue = valueList.Count > 1 ? valueList[1] : 0;
                if (achieveId <= 0) { return; }
                AchievementDataMgr.getInstance().RecordAchievementProgress((AchievementType)achieveId, proValue);
            };
            actionDict["重置所有成就进度并清空成就完成情况(不用参数直接执行)"] = (argStr) =>
            {
                AchievementDataMgr.getInstance().ResetAllAchievementData();
            };
            actionDict["设置人物为无敌状态:1开启,0关闭"] = (argStr) =>
            {
                var valueList = getValueListFromArgs(argStr);
                var openArg = valueList.Count > 0 ? valueList[0] : -1;
                playerLogic.isProtect = openArg == 1;
                if (openArg == 1) { Debug.Log("==========设置人物无敌状态成功"); }
                else { Debug.Log("==========关闭人物无敌状态"); }
            };
            actionDict["设置玩家死亡(不用参数直接执行)"] = (argStr) =>
            {
                var data = new DamageData();
                data.baseDamage = 999;
                playerLogic.HasHurt(data);
                
            };
            actionDict["设置玩家被击飞(不用参数直接执行)"] = (argStr) =>
            {
                var data = new DamageData();
                data.baseDamage = 0;
                data.breakHight = 5;
                data.breakWidth = 10;
                data.breakTime = 1;
                playerLogic.HasHurt(data);
            };
            actionDict["设置游戏难度(0,1,2,3)"] = (argStr) =>
            {
                var valueList = getValueListFromArgs(argStr);
                var hardType = valueList.Count > 0 ? valueList[0] : -1;
                if (!new List<int>() { 0, 1, 2, 3 }.Contains(hardType))
                {
                    Debug.Log("==========无效的难度类型!!!");
                    return;
                }
                var hardCompont = GameManager.GetGMComponent<HardComponentGM>();
                hardCompont.SetHard((EGameHard)hardType);
                Debug.Log("设置当前游戏难度为:" + ((EGameHard)hardType).ToString());
            };
            actionDict["打开设置界面(不需要参数)"] = (argStr) =>
            {
                string uiPrefabPath = UIPrefabPath.GetUIPrefabPath("SettingPanel");
                var uiForm = GameManager.GetGMComponent<UIComponentGM>().GetUIForm(uiPrefabPath);
                if (uiForm == null)
                {
                    GameManager.GetGMComponent<UIComponentGM>().OpenUIForm(uiPrefabPath, EUIGroup.Top, new OpenFormArgs());
                }

            };
            actionDict["人物移动加速(默认20)"] = (argStr) =>
            {
                var valueList = getValueListFromArgs(argStr);
                var moveSpeed = valueList.Count > 0 ? valueList[0] : 20;
                var moveCpn = playerLogic.componentSystem.GetComponent<PlayerMoveComponent>();
                moveCpn.ChangeMoveSpeed(moveSpeed);
            };
            actionDict["人物无敌加速(默认20)并穿透怪物"] = (argStr) =>
            {
                var valueList = getValueListFromArgs(argStr);
                var moveSpeed = valueList.Count > 0 ? valueList[0] : 20;
                var moveCpn = playerLogic.componentSystem.GetComponent<PlayerMoveComponent>();
                moveCpn.ChangeMoveSpeed(moveSpeed);
                playerLogic.isEnableMovePassMonster = true;
                playerLogic.isProtect = true;
                playerLogic.gameObject.layer = 17;
            };
            actionDict["添加道具(道具ID,数量(默认1))"] = (argStr) =>
            {
                var valueList = getValueListFromArgs(argStr);
                var itemId = valueList.Count > 0 ? valueList[0] : -1;
                var itemNum = valueList.Count > 1 ? valueList[1] : 1;
                if (itemId < 0)
                {
                    Debug.Log("=========================无效的道具ID：" + itemId);
                    return;
                }
                var itemName = (EMainItemName)itemId;
                GameManager.GetGameSceneManager().GetArchiveData<PlayerBagData>().AddMainItem(itemName, itemNum);
                Debug.Log("============添加：" + itemName + " X " + itemNum);
            };
            actionDict["重置本地语言保存情况"] = (argStr) =>
            {

                PlayerPrefs.SetInt("LanguageType", -1);
                PlayerPrefs.Save();
            };
            actionDict["打开死亡界面"] = (argStr) =>
            {
                string uiPrefabPath = UIPrefabPath.GetUIPrefabPath("DeadPanel");
                var uiForm = GameManager.GetGMComponent<UIComponentGM>().GetUIForm(uiPrefabPath);
                if (uiForm == null)
                {
                    GameManager.GetGMComponent<UIComponentGM>().OpenUIForm(uiPrefabPath, EUIGroup.Top, new OpenFormArgs());
                }
            };
            actionDict["设置人物受到伤害(数值(默认10))"] = (argStr) =>
            {
                var valueList = getValueListFromArgs(argStr);
                var damageValue = valueList.Count > 0 ? valueList[0] : 10;
                var damageData = new DamageData();
                damageData.baseDamage = damageValue;
                playerLogic.HasHurt(damageData);
            };
            actionDict["临时修改游戏语言(0:中,1:英，2:日)"] = (argStr) =>
            {
                var valueList = getValueListFromArgs(argStr);
                var type = valueList.Count > 0 ? valueList[0] : 1;
                LanguageEnumType languageEnumType = (LanguageEnumType)type;
                GameManager.Instance.language = languageEnumType;
            };
            actionDict["打开游戏指引界面(多个参数1,2...表示连续显示)"] = (argStr) =>
            {
                var valueList = getValueListFromArgs(argStr);
                string uiPrefabPath = UIPrefabPath.GetUIPrefabPath("ControlTipsPanel");
                var uiForm = GameManager.GetGMComponent<UIComponentGM>().GetUIForm(uiPrefabPath);
                if (uiForm == null)
                {
                    GameManager.GetGMComponent<UIComponentGM>().OpenUIForm(uiPrefabPath, EUIGroup.Top, new OpenFormArgs()
                    {
                        userData = valueList,
                    });
                }
            };
        }

        List<int> getValueListFromArgs(string[] argStr)
        {
            var valueList = new List<int>();
            for (int i = 0; i < argStr.Length; i++)
            {
                if (argStr[i] == "") { continue; }
                var value = int.Parse(argStr[i]);
                valueList.Add(value);
            }
            return valueList;
        }
    }
}