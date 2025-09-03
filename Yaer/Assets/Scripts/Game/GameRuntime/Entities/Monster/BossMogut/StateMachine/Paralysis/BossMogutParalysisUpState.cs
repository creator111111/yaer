using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.GameRuntime.Entities.Monster.BossMogut
{
    public class BossMogutParalysisUpState : BaseBossMogutBattleState
    {
        public override void Enter()
        {
            base.Enter();
            bossMogutLogic.isProtect = true;// 起身无敌
            bossMogutLogic.PlayBossCallAudio();// 倒地时播放音效
        }

        public override void Update()
        {
            base.Update();
            if (IsFinished)
            {
                ExitCurrentStateMachine().ChangeState<BossMogutMoveState>();
            }
        }

        public override void Exit()
        {
            base.Exit();
            bossMogutLogic.isProtect = false;
        }
    }
}