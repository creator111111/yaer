using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.GameRuntime.Entities.Monster.BossMogut
{
    public class BossMogutBrokenLegParalysisDefeatState : BaseBossMogutBattleState
    {
        public override void Enter()
        {
            base.Enter();
            bossMogutLogic.IsDefeating = true;
            bossMogutLogic.isProtect = true;
        }

        public override void Update()
        {
            base.Update();
            if (IsFinished)
            {
                ChangeState<BossMogutBrokenLegParalysisIdleState>();
            }
        }

        public override void Exit()
        {
            base.Exit();
            bossMogutLogic.IsDefeating = false;
            bossMogutLogic.isProtect = false;
        }
    }
}
