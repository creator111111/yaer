using Game.GameMgr;
using Game.GameRuntime.Entities.Monster.Slime;
using Game.GameRuntime.Entities.Monster.Slime.Anima;
using Game.GameRuntime.Entities.Monster.Slime.Anima.State;
using Game.GameRuntime.GameSceneManager.Component.Story;
using GameFramework.CoreExtend.Component;
using System.Collections.Generic;
using UnityEngine;

// 史莱姆吸食羊事件-2
public class SlimeEatSheepStroy2 : BaseSceneStoryObj
{
    public GameObject deadSheepStoryTrigger;
    public GameObject deadSheepBody; // 死羊的尸体
    public GameObject deadSheepGrave; // 死羊的坟墓
    public GameObject slimeEatAni_1; // 史莱姆动画
    public GameObject slimeEatAni_2;
    public List<Slime> slimeLogics;

    public int curDeadMonsterCount { get; set; } // 目标怪物的死亡数量
    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
        
        deadSheepStoryTrigger.SetActive(false);
        foreach(var slime in slimeLogics)
        {
            slime.gameObject.SetActive(false);
            slime.OnDeadEventFunc += storyMgr.CheckEventHasEnd;
        }
        deadSheepBody.SetActive(true);
        deadSheepGrave.SetActive(false);
        curDeadMonsterCount = 0;
        // 检测事件是否已经结束，事件结束后部分场景对象发生变化
        if (SlimeEatSheepStoryMgr2.getInstance().GetHasCreateGrave())
        {
            ShowSheepGrave();
        }else if (SlimeEatSheepStoryMgr2.getInstance().GetHasTriggerSlime())
        {
            TriggerSlime();
        }
    }

    public override void InitStoryMgr()
    {
        base.InitStoryMgr();
        storyMgr = SlimeEatSheepStoryMgr2.getInstance(); // 初始设置管理器
        sceneMgr.GetArchiveData<SlimeEatSheepStoryMgr2>();
    }

    public override void HideSceneObj()
    {
        base.HideSceneObj();
    }

    public void ShowSheepGrave()
    {
        deadSheepBody.SetActive(false);
        deadSheepGrave.SetActive(true);
        slimeEatAni_1.SetActive(false);
        slimeEatAni_2.SetActive(false);
        // 销毁怪物对象
        foreach (var slime in slimeLogics)
        {
            Destroy(slime.gameObject);
        }
        slimeLogics.Clear();
        deadSheepStoryTrigger.SetActive(true);
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
            // 事件开始后激活史莱姆
            TriggerSlime();
        }
        else
        {
            slimeLogics.Clear();
            // 击败怪物后激活和羊的交互触发器
            deadSheepStoryTrigger.SetActive(true);
        }
    }

    public void TriggerSlime()
    {
        foreach (var slime in slimeLogics)
        {
            if (slime == null) continue;
            slime.gameObject.SetActive(true);
            var csAnimator = slime.componentSystem.GetComponent<SlimeCsAnimator>();
            csAnimator.CurrentCsRuntimeController.Exit();
            csAnimator.ChangeState<SlimeIdleState>();
        }
        slimeEatAni_1.SetActive(false);
        slimeEatAni_2.SetActive(false);
    }
}
