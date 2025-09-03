using Game.GameMgr;
using Game.GameMgr.Component.Archive.ArchiveDataClass.BaseDataClass;
using Game.GameMgr.Component.UI;
using Game.GameRuntime.GameSceneManager.Base;
using Game.Static.Path;
using NodeCanvas.Framework;
using System;
using UnityEngine;
// 场景中的故事事件管理器
public class BaseSceneStoryMgr : BaseArchiveData
{
    public bool hasPassEvent = false; // 是否通过事件,通过后则不会再触发事件
    public BaseSceneStoryObj sceneStoryObj = null; // 场景事件对象，需要通过一个添加到场景的脚本来处理相关事件
    public bool hasInCurStory = false; // 是否处于当前场景事件中
    public static BaseSceneStoryMgr instance;
    public static BaseSceneStoryMgr getInstance()
    {
        if (instance == null)
        {
            instance = new BaseSceneStoryMgr();
        }
        return instance;
    }

    public virtual void ParseStoryAcitonArgs(string args)
    {

    }
    

    public virtual void OnSceneStoryTrigger(bool isStart)
    {
        
        if (isStart)
        {
            // 触发场景事件时自动关闭部分界面
            var uiForm = GameManager.GetGMComponent<UIComponentGM>().GetUIForm(UIPrefabPath.GetUIPrefabPath("MenuPanel"));
            if (uiForm != null) { GameManager.GetGMComponent<UIComponentGM>().CloseUIForm(uiForm); }
            uiForm = GameManager.GetGMComponent<UIComponentGM>().GetUIForm(UIPrefabPath.GetUIPrefabPath("SaveGamePanel"));
            if (uiForm != null) { GameManager.GetGMComponent<UIComponentGM>().CloseUIForm(uiForm); }
        }
        hasInCurStory = isStart;
        if (sceneStoryObj != null) { sceneStoryObj.BattleStoryStartOrEnd(isStart); }
    }

    // 检测当前战斗是否结束
    public virtual void CheckEventHasEnd()
    {
        
    }

    // 初始化数据
    public virtual void InitBattleData(bool singleUseInArchive, bool enabled)
    {
        if (!singleUseInArchive) { return; }// 如果不是每个存档只触发一次的则不用处理
        hasPassEvent = !enabled;// 事件不能被触发则视为已经通过了事件
        if (hasPassEvent && sceneStoryObj != null)
        {
            sceneStoryObj.HideSceneObj();
        }
    }
    // 退出当前事件
    public virtual void ExitCurStory()
    {
        hasInCurStory = false;
        sceneStoryObj.sceneMgr.canShowSaveGame = true;
        sceneStoryObj = null;
    }

    // =============================存档和读档Start
    public override void ParseInternal(MasterGameData masterData)
    {
    }

    public override void SerializeInternal(MasterGameData masterData)
    {
    }
    //===============================存档和读档End
}
