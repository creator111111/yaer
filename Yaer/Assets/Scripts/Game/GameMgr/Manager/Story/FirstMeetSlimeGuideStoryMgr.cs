using Game.GameMgr.Component.Archive.ArchiveDataClass.BaseDataClass;

public class FirstMeetSlimeGuideStoryMgr : BaseSceneStoryMgr
{
    public new static FirstMeetSlimeGuideStoryMgr instance;
    public new static FirstMeetSlimeGuideStoryMgr getInstance()
    {
        if (instance == null)
        {
            instance = new FirstMeetSlimeGuideStoryMgr();
        }
        return instance;
    }
    
    // 获取场景故事对象
    public FirstMeetSlimeStory GetGuideStory() { return sceneStoryObj as FirstMeetSlimeStory; }

    public override void OnSceneStoryTrigger(bool isStart)
    {
        base.OnSceneStoryTrigger(isStart);
        //if (inShowSmallAtkTips)
        //{
        //    base.OnSceneStoryTrigger(isStart);
        //}
        //else if (inShowDashAtkTips)
        //{
        //    var story = GetGuideStory();
        //    story.ShowPlayerKeyTipsNode(Game.Static.Enum.ControlInputType.SmashAttack);
        //}
        
    }

    public override void CheckEventHasEnd()
    {
        base.CheckEventHasEnd();
        var story = GetGuideStory();
        if (story.slimeLogic.IsDead)
        {
            hasInCurStory = false;
            story.BattleStoryStartOrEnd(false);
        }
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
