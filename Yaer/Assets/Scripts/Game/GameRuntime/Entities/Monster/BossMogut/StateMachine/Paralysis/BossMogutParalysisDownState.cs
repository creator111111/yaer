using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.GameRuntime.Entities.Monster.BossMogut
{
    public class BossMogutParalysisDownState : BaseBossMogutBattleState
    {
        public override void Enter()
        {
            base.Enter();
            bossMogutLogic.CurrentParalysisUpFaceHitTimes = 0;
            bossMogutLogic.isProtect = true;
            bossMogutLogic.PlayBossCallAudio();// 倒地时播放音效
        }

        public override void Update()
        {
            base.Update();
            FinishedChangeState<BossMogutParalysisIdleState>();
        }

        public override void Exit()
        {
            base.Exit();
            bossMogutLogic.isProtect = false;
        }
    }
}
