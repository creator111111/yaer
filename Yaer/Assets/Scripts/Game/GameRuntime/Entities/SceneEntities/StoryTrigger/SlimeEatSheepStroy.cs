using Game.GameMgr;
using Game.GameMgr.Component.Archive.ArchiveDataClass;
using Game.GameRuntime.Entities.Monster.Slime;
using Game.GameRuntime.Entities.Monster.Slime.Anima;
using Game.GameRuntime.Entities.Monster.Slime.Anima.State;
using Game.GameRuntime.GameSceneManager.Component.Story;
using GameFramework.CoreExtend.Component;
using System.Collections.Generic;
using UnityEngine;

// ForestEast slime-eat-sheep. Loop clips are Objects/1 and Objects/2; Part3 sheep is static only.
public class SlimeEatSheepStroy : BaseSceneStoryObj
{
    // Must match scene Trigger StoryPrefabName. Only after this story fired may we hide eat loops.
    public const string EastEatSheepStoryName = "ForestEastSceneSlimeEatSheep";

    public GameObject deadSheepStoryTrigger;
    public GameObject deadSheepBody; // corpse
    public GameObject deadSheepGrave; // grave
    public GameObject slimeEatAni_1; // Objects eat-loop prefab 1
    public GameObject slimeEatAni_2; // Objects eat-loop prefab 2
    public List<Slime> slimeLogics;

    public int curDeadMonsterCount { get; set; }

    public override void Start()
    {
        base.Start();

        deadSheepStoryTrigger.SetActive(false);
        foreach (var slime in slimeLogics)
        {
            //slime.gameObject.SetActive(false);
            slime.OnDeadEventFunc += storyMgr.CheckEventHasEnd;
        }
        deadSheepBody.SetActive(true);
        deadSheepGrave.SetActive(false);
        curDeadMonsterCount = 0;

        // Why: old Mgr2 shared East archive keys; finishing corridor made East Start call TriggerSlime and hide loops.
        // Gate: only restore grave/fight visuals if THIS East story is already in StoryTriggerCount.
        bool eastStoryUsed = false;
        if (sceneMgr != null)
        {
            var counts = sceneMgr.GetArchiveData<StoryTriggerCountData>();
            eastStoryUsed = counts != null && counts.CheckStoryUsed(EastEatSheepStoryName);
        }

        if (eastStoryUsed && SlimeEatSheepStoryMgr.getInstance().GetHasCreateGrave())
        {
            ShowSheepGrave();
        }
        else if (eastStoryUsed && SlimeEatSheepStoryMgr.getInstance().GetHasTriggerSlime())
        {
            TriggerSlime();
        }
        else
        {
            // East eat-sheep not played yet: force loops ON (heals polluted saves).
            // Alt: trust scene Active only -- polluted/runtime-off nodes never recover.
            EnsureSlimeEatAniVisible();
        }
    }

    void EnsureSlimeEatAniVisible()
    {
        if (slimeEatAni_1 != null) slimeEatAni_1.SetActive(true);
        if (slimeEatAni_2 != null) slimeEatAni_2.SetActive(true);
    }

    public override void InitStoryMgr()
    {
        base.InitStoryMgr();
        storyMgr = SlimeEatSheepStoryMgr.getInstance();
        sceneMgr.GetArchiveData<SlimeEatSheepStoryMgr>();
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
        foreach (var slime in slimeLogics)
        {
            Destroy(slime.gameObject);
        }
        slimeLogics.Clear();
        deadSheepStoryTrigger.SetActive(true);
    }

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
            TriggerSlime();
        }
        else
        {
            slimeLogics.Clear();
            deadSheepStoryTrigger.SetActive(true);
            PlayerGuideMgr.getInstance().PraseActName("Sit");
        }
    }

    public void TriggerSlime()
    {
        foreach (var slime in slimeLogics)
        {
            if (slime == null) continue;
            slime.gameObject.SetActive(true);
            if (slime.baseAniState == SlimeAniState.Idle)
            {
                var csAnimator = slime.componentSystem.GetComponent<SlimeCsAnimator>();
                csAnimator.CurrentCsRuntimeController.Exit();
                csAnimator.ChangeState<SlimeIdleState>();
            }
        }
        slimeEatAni_1.SetActive(false);
        slimeEatAni_2.SetActive(false);
    }
}
