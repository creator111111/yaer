using Game.GameMgr;
using Game.GameMgr.Component;
using Game.GameMgr.Component.Archive.ArchiveDataClass.BaseDataClass;
using Game.GameMgr.Component.Archive.ArchiveDataClass.Player;
using Game.GameMgr.Component.UI;
using Game.GameRuntime.Component;
using Game.GameRuntime.Entities.Monster.BossMogut;
using Game.Static.Path.Sound;
using System;
using UnityEngine;
// 精灵村前-BOSS战斗事件管理器
public class WestRappRoadBossBattleMgr : BaseSceneStoryMgr
{
    public bool hasSaveMonster; // 是否救了哥布林
    public new static WestRappRoadBossBattleMgr instance;
    public new static WestRappRoadBossBattleMgr getInstance()
    {
        if (instance == null)
        {
            instance = new WestRappRoadBossBattleMgr();
        }
        return instance;
    }
    
    // 获取场景故事对象
    public WestRappRoadBattleStory GetBossBattleStory() { return sceneStoryObj as WestRappRoadBattleStory; }

    public override void OnSceneStoryTrigger(bool isStart)
    {
        base.OnSceneStoryTrigger(isStart);
        if (isStart)
        {
            sceneStoryObj.sceneMgr.canShowSaveGame = false;
            // 播放BOSS战音乐
            GameManager.GetGMComponent<SoundComponentGM>().PlaySound(SoundType.BGM, "BOSS战死斗3_l1.mp3", true, 2f, 5);
            // 显示BOSS血条
            var bossLogic = GetBossBattleStory().bossMonster.GetComponent<BossMogutLogic>();
            UIUtils.OpenPanel("BossHpBarPanel", EUIGroup.Middle, bossLogic);
        }
    }

    public override void CheckEventHasEnd()
    {
        base.CheckEventHasEnd();
        var monsterLogic = GetBossBattleStory().bossMonster.GetComponent<BossMogutLogic>();
        if (monsterLogic.IsDead)
        {
            // 怪物死亡后视为事件结束
            hasPassEvent = true;
            hasInCurStory = false;
            sceneStoryObj.sceneMgr.canShowSaveGame = true;
            GetBossBattleStory().BattleStoryStartOrEnd(false);
            // 播放地图音乐
            GameManager.GetGMComponent<SoundComponentGM>().PlaySound(SoundType.BGM, "龙城郊，东郊苍翠走廊.ogg", true, 2, 5);
            // 关闭血条
            var bossHpBarPanel = UIUtils.GetPanel<BossHpBarFormLogic>("BossHpBarPanel");
            if (bossHpBarPanel != null)
            {
                bossHpBarPanel.SetAutoCloseOnHpInZero(true); // 设置血条自动消失
            }
        }
    }

    public void SetHasSaveMonster(bool hasSaveMonster)
    {
        this.hasSaveMonster = hasSaveMonster;
        // 成就判断
        if (hasSaveMonster)
        {
            AchievementDataMgr.getInstance().RecordAchievementProgress(AchievementType.SaveGoblin, 1);
        }
    }

    public void StartCameraImpluse(Vector3 vector3 = new Vector3())
    {
        if (GetBossBattleStory() != null)
        {
            GetBossBattleStory().StartCameraImpluse(vector3);
        }
    }

    public void CheckEventArgs(string value)
    {
        switch (value)
        {
            case "Start":
                getInstance().OnSceneStoryTrigger(true);
                break;
            case "GoblinUpHead":
                GetBossBattleStory().ShowGoblinUpHead();
                break;
            case "CloseBGM":
                GameManager.GetGMComponent<SoundComponentGM>().StopBGM(2);
                break;
            default:
                break;
        }
    }
    // =============================存档和读档Start
    public override void ParseInternal(MasterGameData masterData)
    {
        getInstance().hasSaveMonster = masterData.GetValue<bool>("WestRappRoadHasSaveMonster", false);
    }

    public override void SerializeInternal(MasterGameData masterData)
    {
        masterData.SetValue("WestRappRoadHasSaveMonster", getInstance().hasSaveMonster);
    }

    //===============================存档和读档End
}
