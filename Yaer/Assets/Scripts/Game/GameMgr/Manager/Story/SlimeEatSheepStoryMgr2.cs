using Game.GameMgr;
using Game.GameMgr.Component.Archive.ArchiveDataClass.BaseDataClass;
using System.Collections.Generic;
using UnityEngine;

// Corridor slime-eat-sheep Mgr2. Archive keys MUST differ from East Mgr1 or corridor progress poisons East eat loops.
public class SlimeEatSheepStoryMgr2 : BaseSceneStoryMgr
{
    bool hasCreateSheepGrave; // grave created
    bool hasTriggerSlime; // fight started

    // Corridor-only keys (do not reuse East SlimeEatSheepStory_*).
    const string KeyCreateGrave = "SlimeEatSheepStory2_hasCreateGrave";
    const string KeyTriggerSlime = "SlimeEatSheepStory2_hasTriggerSlime";
    // Pre-fix shared East keys; migrate only when corridor story already used.
    const string LegacyKeyCreateGrave = "SlimeEatSheepStory_hasCreateGrave";
    const string LegacyKeyTriggerSlime = "SlimeEatSheepStory_hasTriggerSlime";
    const string CorridorEatSheepStoryName = "VerdantCorridorSlimeEatSheep";

    public new static SlimeEatSheepStoryMgr2 instance;
    public new static SlimeEatSheepStoryMgr2 getInstance()
    {
        if (instance == null)
        {
            instance = new SlimeEatSheepStoryMgr2();
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

    public SlimeEatSheepStroy2 GetStoryObj() { return sceneStoryObj as SlimeEatSheepStroy2; }

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

    public override void ParseInternal(MasterGameData masterData)
    {
        // Missing Story2 keys: migrate from legacy ONLY if corridor dialogue already counted.
        // Blind migrate would treat East progress as corridor "already started".
        bool hasStory2Keys = masterData.HasField(KeyCreateGrave) || masterData.HasField(KeyTriggerSlime);
        if (!hasStory2Keys)
        {
            if (IsCorridorEatSheepStoryUsed(masterData))
            {
                getInstance().hasCreateSheepGrave = masterData.GetValue(LegacyKeyCreateGrave, false);
                getInstance().hasTriggerSlime = masterData.GetValue(LegacyKeyTriggerSlime, false);
                Debug.Log($"[SlimeEatSheep] Mgr2 migrate legacy->Story2 grave={getInstance().hasCreateSheepGrave} trigger={getInstance().hasTriggerSlime}");
            }
            else
            {
                getInstance().hasCreateSheepGrave = false;
                getInstance().hasTriggerSlime = false;
            }
        }
        else
        {
            getInstance().hasCreateSheepGrave = masterData.GetValue(KeyCreateGrave, false);
            getInstance().hasTriggerSlime = masterData.GetValue(KeyTriggerSlime, false);
        }
    }

    // Parse StoryTriggerCount JSON from Master (CountData singleton order not guaranteed here).
    static bool IsCorridorEatSheepStoryUsed(MasterGameData masterData)
    {
        string dataStr = masterData.GetValue("StoryTriggerCountData_StoryTriggerCount", "");
        if (string.IsNullOrEmpty(dataStr)) return false;
        try
        {
            var dict = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, int>>(dataStr);
            return dict != null && dict.ContainsKey(CorridorEatSheepStoryName);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[SlimeEatSheep] Mgr2 StoryTriggerCount parse fail: {e.Message}");
            return false;
        }
    }

    public override void SerializeInternal(MasterGameData masterData)
    {
        masterData.SetValue(KeyCreateGrave, getInstance().hasCreateSheepGrave);
        masterData.SetValue(KeyTriggerSlime, getInstance().hasTriggerSlime);
    }
}