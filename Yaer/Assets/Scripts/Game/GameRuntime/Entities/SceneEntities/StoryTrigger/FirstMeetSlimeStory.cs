using Game.GameRuntime.Entities.Monster;
using Game.GameRuntime.GameSceneManager.Component.Story;
using Game.Static.Enum;

// 第一次遇到史莱姆事件
public class FirstMeetSlimeStory : BaseSceneStoryObj
{
    // Start is called before the first frame update
    public BaseMonster slimeLogic;
    public override void Start()
    {
        base.Start();
    }

    public override void InitStoryMgr()
    {
        base.InitStoryMgr();
        storyMgr = FirstMeetSlimeGuideStoryMgr.getInstance(); // 初始设置管理器
    }

    public override void HideSceneObj()
    {
        base.HideSceneObj();
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();
        if (!hasStartStory) { return; }
        if (hasEndStory) { return; }
        
    }

    public override void BattleStoryStartOrEnd(bool isStart)
    {
        base.BattleStoryStartOrEnd(isStart);
        if (isStart)
        {
            // 设置人物头上显示按键提示
            //var mgr = storyMgr as FirstMeetSlimeGuideStoryMgr;
            //var inputType = mgr.inShowNorAtkTips ? ControlInputType.NormalAttack : ControlInputType.None;
            //if (inputType == ControlInputType.None)
            //{
            //    inputType = mgr.inShowSmallAtkTips ? ControlInputType.SmashAttack : ControlInputType.None;
            //}
            //ShowPlayerKeyTipsNode(inputType);
        }
        else
        {
            // 击败怪物后对话
            sceneMgr.GetModule<StoryComponentGSM>().TriggerStory("ForestEastSceneFirstKillSlime");
        }
    }

    public void ShowPlayerKeyTipsNode(ControlInputType controlInputType)
    {
        PlayerGuideMgr.getInstance().ShowPlayerKeyTipsNode(controlInputType);
        //var playerEntity = sceneMgr.GetPlayerEntity();
        //if (playerEntity != null && playerEntity.Logic is PlayerLogic playerLogic)
        //{
        //    var entityControll = playerLogic.GetComponent<BaseEntityControll>();
        //    if (entityControll != null)
        //    {
        //        playerLogic.canTouchObj = entityControll.interactiveComponent;
        //        playerLogic.ShowActionKeyTipsNode(true, controlInputType);
        //    }
        //}
    }
}
