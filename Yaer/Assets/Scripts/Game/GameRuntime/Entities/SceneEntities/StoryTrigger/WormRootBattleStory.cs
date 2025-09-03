using Game.GameRuntime.Entities.Monster.WoodWorm;
using Game.GameRuntime.Entities.Monster.WoodWormRoot;
using Game.GameRuntime.GameSceneManager.Component.Story;
using System.Collections.Generic;
using UnityEngine;

// 虫巢战斗事件
public class WormRootBattleStory : BaseSceneStoryObj
{
    public List<WoodWormRootLogic> allWormRootList = new List<WoodWormRootLogic>();// 所有虫巢对象
    public List<WoodWormLogic> allWoodWormLogics = new List<WoodWormLogic>(); // 事件中所有的虫子对象
    public WoodWormLogic firstSpcWoodWorm; // 首个特殊蠕虫对象

    public float timeCount = 0; // 计时器
    public float targetTime = 60; // X秒后没结束事件则触发对话
    public bool hasTriggerSpcTalk = false; // 是否触发了特殊对话
    public bool hasWoodEscpae = false; // 是否有虫子逃跑了
    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
    }

    public override void InitStoryMgr()
    {
        base.InitStoryMgr();
        storyMgr = WoodWormRootBattleMgr.getInstance(); // 初始设置管理器
    }

    public override void HideSceneObj()
    {
        base.HideSceneObj();
        foreach (var wormRoot in allWormRootList)
        {
            wormRoot.gameObject.SetActive(false);// 隐藏虫巢
        }
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();
        if (!hasStartStory) { return; }
        if (hasEndStory) { return; }
        if (hasTriggerSpcTalk) { return; }
        timeCount += Time.deltaTime;
        if (timeCount >= targetTime)
        {
            // 触发特殊对话
            hasTriggerSpcTalk = true;
            sceneMgr.GetModule<StoryComponentGSM>().TriggerStory("VerdantCorridorNotDestoryNestForLongTime");
        }
    }

    public override void BattleStoryStartOrEnd(bool isStart)
    {
        base.BattleStoryStartOrEnd(isStart);
        // 是否激活碰撞体限制玩家行动
        if (leftColliderNode != null) { leftColliderNode.SetActive(isStart); }
        if (rightColliderNode != null) {  rightColliderNode.SetActive(isStart); }
        if (isStart)
        {
            hasStartStory = true;
            // 开始虫巢战斗事件
            // 1:设置一个虫子往玩家身边爬
            if (firstSpcWoodWorm != null)
            {
                firstSpcWoodWorm.moveToPlayer();
            }
            foreach(var wormRoot in allWormRootList)
            {
                wormRoot.SetObjActive(true);
                wormRoot.canCreateWoodWorm = true;// 激活所有巢穴
            }
        }
        else
        {
            hasEndStory = true;
            // 结束虫巢战斗事件,播放结束剧情动画
            // 检测是否所有虫子都被消灭
            if (checkAllWormHasDead())
            {
                hasWoodEscpae = false;
                // 没有虫子了
                WoodWormRootBattleMgr.getInstance().hasKillAllWorm = true;
            }
            else
            {
                hasWoodEscpae = true;
                WoodWormRootBattleMgr.getInstance().hasKillAllWorm = false;
                // 剩余虫子播放逃跑动画
                allWormEscape();
            }
            // 播放剧情动画，通过节点树自带的方法检测当前走哪条分支
            sceneMgr.GetModule<StoryComponentGSM>().TriggerStory("VerdantCorridorAfterDestoryNest");
        }
    }

    public bool GetHasWoodEscape()
    {
        return hasWoodEscpae;
    }

    bool checkAllWormHasDead()
    {
        var deadCount = 0;
        foreach (var woodWorm in allWoodWormLogics)
        {
            if (woodWorm.IsDead) { deadCount++; }
        }
        return deadCount >= allWoodWormLogics.Count;
    }

    private void allWormEscape()
    {
        foreach(var woodWormLogic in allWoodWormLogics)
        {
            if (woodWormLogic.IsDead) { continue; }
            // 设置虫子延时逃跑
            var delayTime = GameTools.getRandomIntNum(5, 10);
            float realTime = delayTime / 10.0f;
            GameActionMgr.runDelayTimeAction(realTime, () =>
            {
                woodWormLogic.ChangeToEscapeState();
            }, woodWormLogic.gameObject);
        }
    }

    public void addNewWoodWorm(WoodWormLogic woodWormLogic)
    {
        allWoodWormLogics.Add(woodWormLogic);
    }

}
