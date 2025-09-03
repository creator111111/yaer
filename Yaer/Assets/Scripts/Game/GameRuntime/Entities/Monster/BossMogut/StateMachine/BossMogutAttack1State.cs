using Game.GameRuntime.Entities.Monster.WoodWorm;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Game.GameRuntime.Entities.Monster.BossMogut
{
    public class BossMogutAttack1State : BaseBossMogutBattleState
    {
        public override void Enter()
        {
            base.Enter();
            animationEventComponent.RegisterEvent("CreateMAtkCollsion", CreateMAtkCollsion);
            animationEventComponent.RegisterEvent("RemoveMAtkCollsion", RemoveMAtkCollsion);
            animationEventComponent.RegisterEvent("StopAniFrameWithSec", StopAniFrameWithSec);
        }
        public override void Update()
        {
            base.Update();
            FinishedChangeState<BossMogutMoveState>();
        }

        public override void Exit()
        {
            base.Exit();
            // 攻击状态结束后进入CD
            monsterLogic.EnterAttackCd();
            moveCdTimeCount = 0;
        }
    }
}

