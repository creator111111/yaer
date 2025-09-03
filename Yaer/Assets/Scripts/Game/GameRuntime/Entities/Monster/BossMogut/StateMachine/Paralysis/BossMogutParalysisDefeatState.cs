using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.GameRuntime.Entities.Monster.BossMogut
{
    public class BossMogutParalysisDefeatState : BaseBossMogutBattleState
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
                if (bossMogutLogic.CurrentParalysisUpFaceHitTimes >= bossMogutLogic.ParalysisUpFaceHitTimes)
                {
                    ChangeState<BossMogutParalysisUpState>();
                }
                else
                {
                    ChangeState<BossMogutParalysisIdleState>();
                }
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
