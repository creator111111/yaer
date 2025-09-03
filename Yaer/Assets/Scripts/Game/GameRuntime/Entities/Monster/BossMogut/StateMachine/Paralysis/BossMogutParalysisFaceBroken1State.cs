using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.GameRuntime.Entities.Monster.BossMogut
{
    public class BossMogutParalysisFaceBroken1State : BaseBossMogutBattleState
    {
        public override void Enter()
        {
            base.Enter();
            bossMogutLogic.isProtect = true;
            bossMogutLogic.PlayBossCallAudio();// 倒地时播放音效
        }
        public override void Update()
        {
            base.Update();
            FinishedChangeState<BossMogutParalysisFaceBroken2State>();
        }
    }
}

