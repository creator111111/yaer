using Game.GameRuntime.Entities.Component.CldController;
using Game.GameRuntime.Entities.Monster.TenWan;
using GameFramework.CoreExtend.Component;
using UnityEngine;

namespace Game.GameRuntime.Entities.Monster.Slime.Anima.State
{
    public class SlimeDeadState : BaseSlimeState
    {
        public override void Enter()
        {
            base.Enter();
            slime.isProtect = false; // 史莱姆尸体设置成能被砍中
            slime.componentSystem.GetComponent<CldControllerComponent>().SetActiveAll(true);
            //moveCpn.StopMove();
            //slime.BodyRg.constraints = RigidbodyConstraints2D.FreezeAll;

        }

        public override void Update()
        {
            base.Update();
            if (IsFinished)
            {
                slime.isProtect = true; // 死亡动画结束后才设置无敌
                slime.componentSystem.GetComponent<CldControllerComponent>().SetActiveAll(false);
                if (FirstMeetSlimeGuideStoryMgr.getInstance().hasInCurStory)
                {
                    FirstMeetSlimeGuideStoryMgr.getInstance().CheckEventHasEnd();
                }
            }
        }
    }
}