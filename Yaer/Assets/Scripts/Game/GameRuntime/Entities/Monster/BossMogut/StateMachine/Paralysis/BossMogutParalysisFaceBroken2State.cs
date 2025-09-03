using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.GameRuntime.Entities.Monster.BossMogut
{
    public class BossMogutParalysisFaceBroken2State : BaseBossMogutBattleState
    {
        public override void Update()
        {
            base.Update();
            if (bossMogutLogic.IsDead) { return; }
            if (IsFinished)
            {
                // 击倒动画结束后视为BOSS死亡
                bossMogutLogic.OnDead();
            }
        }
    }
}
