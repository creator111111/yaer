using Game.GameRuntime.Component;
using Game.GameRuntime.Entities.Monster;
using Game.GameRuntime.Entities.Monster.WoodWorm;
using Game.GameRuntime.GameSceneManager.Component;
using Game.GameRuntime.GameSceneManager.Component.Story;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// 精灵村前BOSS战斗事件
public class WestRappRoadBattleStory : BaseSceneStoryObj
{
    public GameObject bossMonster; // BOSS怪物
    public GameObject gushaSitObj; // 和BOSS事件相关的对象，事件结束后需要移除
    public GameObject gobinObj; // 和BOSS事件相关的对象，
    public GameObject bloodObj;

    public GameObject npcStandObj; // 站立的NPC
    public GameObject impluseTrigger;
    // 因为事件中人物可能乱跑，所以在事件中保存后的人物读档后强制移动到指定坐标
    public GameObject eventSavePosNode; // 事件中保存时使用的人物的坐标点，

    //=================哥布林图片相关
    public GameObject goblin1;
    public GameObject goblin2;
    public GameObject goblin1_upHead;
    public GameObject goblin2_upHead;

    public List<WoodWormLogic> allWoodWormLogics = new List<WoodWormLogic>(); // 事件中所有的虫子对象

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
    }

    public override void InitStoryMgr()
    {
        base.InitStoryMgr();
        storyMgr = WestRappRoadBossBattleMgr.getInstance(); // 初始设置管理器
        sceneMgr.GetArchiveData<WestRappRoadBossBattleMgr>();
    }

    public override void HideSceneObj()
    {
        base.HideSceneObj();
        HideBossObj();
    }

    // 隐藏BOSS战相关对象
    public void HideBossObj()
    {
        // 
        bossMonster.SetActive(false);
        npcStandObj.SetActive(false);
        gushaSitObj.SetActive(false);
        gobinObj.SetActive(false);
        var hasSaveGobin = WestRappRoadBossBattleMgr.getInstance().hasSaveMonster;
        bloodObj.SetActive(!hasSaveGobin);
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
            // 显示BOSS名称,开始BOSS对战
            sceneMgr.GetModule<TipsComponentGSM>().OpenTipsForm("Boss1", Game.GameRuntime.UI.FormLogic.Tips.ETipsType.Boss);
        }
        else
        {
            // 击败BOSS后对话
            sceneMgr.GetModule<StoryComponentGSM>().TriggerStory("WestRappRoadAfterKillBoss");
            // 设置召唤的虫子逃跑
            foreach (var woodWormLogic in allWoodWormLogics)
            {
                if (woodWormLogic.IsDead) continue;
                // 设置虫子延时逃跑
                var delayTime = GameTools.getRandomIntNum(5, 10);
                float realTime = delayTime / 10.0f;
                GameActionMgr.runDelayTimeAction(realTime, () =>
                {
                    woodWormLogic.ChangeToEscapeState();
                }, woodWormLogic.gameObject);
            }
        }
    }

    public void ShowGoblinUpHead()
    {
        goblin1.SetActive(false);
        goblin2.SetActive(false);
        goblin1_upHead.SetActive(true);
        goblin2_upHead.SetActive(true);
    }

    // 开始屏幕震动
    public void StartCameraImpluse(Vector3 vector3)
    {
        if (impluseTrigger != null)
        {
            impluseTrigger.GetComponent<CameraImpluseTrigger>().CameraImpulse(vector3);
        }
    }
}
