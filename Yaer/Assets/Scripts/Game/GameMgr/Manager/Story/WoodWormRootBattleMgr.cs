using Game.GameMgr;
using Game.GameRuntime.GameSceneManager.Base;
using NodeCanvas.Framework;
using System;
using UnityEngine;
// 翠绿走廊-虫巢战斗事件管理器
public class WoodWormRootBattleMgr : BaseSceneStoryMgr
{
    public bool hasPassWormBattle = false; // 是否通过虫巢战斗事件,通过后则不会再触发事件
    public WormRootBattleStory wormBattleStory = null; // 虫巢战斗事件对象
    public bool hasKillAllWorm = false; // 是否在事件中杀死了所有场上的虫子并消灭虫巢
    public bool hasInWormBattleStory = false; // 是否处于虫巢战斗事件中
    public static new WoodWormRootBattleMgr instance;
    public new static WoodWormRootBattleMgr getInstance()
    {
        if (instance == null)
        {
            instance = new WoodWormRootBattleMgr();
        }
        return instance;
    }
    

    public override void OnSceneStoryTrigger(bool isStart)
    {
        base.OnSceneStoryTrigger(isStart);
        wormBattleStory = sceneStoryObj as WormRootBattleStory;
        //if (wormBattleStory != null)
        //{
        //    wormBattleStory.BattleStoryStartOrEnd(isStart);
        //}
        hasInWormBattleStory = isStart;
        wormBattleStory.sceneMgr.canShowSaveGame = !isStart;
    }

    // 检测当前战斗是否结束
    public override void CheckEventHasEnd()
    {
        if (wormBattleStory == null) { return; }
        var deadCount = 0;
        foreach(var wormRoot in wormBattleStory.allWormRootList)
        {
            if (wormRoot.IsDead)
            {
                deadCount++;
            }
        }
        Debug.Log("=======================deadCount" + deadCount);
        if (deadCount >= wormBattleStory.allWormRootList.Count)
        {
            // 所有虫巢都死亡则事件结束
            wormBattleStory.BattleStoryStartOrEnd(false);
            hasInWormBattleStory = false;
            wormBattleStory.sceneMgr.canShowSaveGame = true;
            hasPassWormBattle = true;
        }
    }

    public override void ExitCurStory()
    {
        base.ExitCurStory();
        hasInWormBattleStory = false;
    }

    //// 初始化数据
    //public void InitBattleData(bool singleUseInArchive, bool enabled)
    //{
    //    if (!singleUseInArchive) { return; }// 如果不是每个存档只触发一次的则不用处理
    //    hasPassWormBattle = !enabled;// 事件不能被触发则视为已经通过了事件
    //    if (hasPassWormBattle && wormBattleStory != null)
    //    {
    //        wormBattleStory.HideAllWoodRoot();
    //    }
    //}
}
