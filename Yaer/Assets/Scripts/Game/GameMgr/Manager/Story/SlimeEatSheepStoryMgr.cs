using Game.GameMgr;
using Game.GameMgr.Component.Archive.ArchiveDataClass.BaseDataClass;

// 史莱姆吸食死羊事件
public class SlimeEatSheepStoryMgr : BaseSceneStoryMgr
{
    bool hasCreateSheepGrave; // 是否为死羊制作坟墓
    bool hasTriggerSlime; // 是否触发史莱姆

    public new static SlimeEatSheepStoryMgr instance;
    public new static SlimeEatSheepStoryMgr getInstance()
    {
        if (instance == null)
        {
            instance = new SlimeEatSheepStoryMgr();
        }
        return instance;
    }

    public bool GetHasCreateGrave()
    {
        return hasCreateSheepGrave;
    }

    public bool GetHasTriggerSlime()
    {
        return hasTriggerSlime;
    }

    public override void InitBattleData(bool singleUseInArchive, bool enabled)
    {
        base.InitBattleData(singleUseInArchive, enabled);
        if (enabled)
        {
            hasCreateSheepGrave = false;
            hasTriggerSlime = false;
        }
    }

    // 获取场景故事对象
    public SlimeEatSheepStroy GetStoryObj() { return sceneStoryObj as SlimeEatSheepStroy; }

    public override void ParseStoryAcitonArgs(string args)
    {
        switch(args)
        {
            case "start":
                OnSceneStoryTrigger(true);
                break;
            case "createGrave":
                SetHasCreateGrave(true);
                GetStoryObj().ShowSheepGrave();
                break;
            default:
                break;
        }
    }

    public override void OnSceneStoryTrigger(bool isStart)
    {
        base.OnSceneStoryTrigger(isStart);
        SetHasTriggerMonster(isStart);
    }

    public override void CheckEventHasEnd()
    {
        base.CheckEventHasEnd();
        var story = GetStoryObj();
        if (story != null)
        {
            story.curDeadMonsterCount++;
            if (story.curDeadMonsterCount >= story.slimeLogics.Count)
            {
                story.BattleStoryStartOrEnd(false);
            }
        }
        
    }

    public void SetHasCreateGrave(bool hasCreateSheepGrave)
    {
        this.hasCreateSheepGrave = hasCreateSheepGrave;
    }
    public void SetHasTriggerMonster(bool hasTriggerSlime)
    {
        this.hasTriggerSlime = hasTriggerSlime;
    }

    // =============================存档和读档Start
    public override void ParseInternal(MasterGameData masterData)
    {
        getInstance().hasCreateSheepGrave = masterData.GetValue("SlimeEatSheepStory_hasCreateGrave", false);
        getInstance().hasTriggerSlime = masterData.GetValue("SlimeEatSheepStory_hasTriggerSlime", false);
    }

    public override void SerializeInternal(MasterGameData masterData)
    {
        masterData.SetValue("SlimeEatSheepStory_hasCreateGrave", getInstance().hasCreateSheepGrave);
        masterData.SetValue("SlimeEatSheepStory_hasTriggerSlime", getInstance().hasTriggerSlime);
    }
    //===============================存档和读档End
}
