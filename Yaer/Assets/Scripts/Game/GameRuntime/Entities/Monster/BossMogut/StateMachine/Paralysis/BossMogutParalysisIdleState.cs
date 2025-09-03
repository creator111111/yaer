using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.GameRuntime.Entities.Monster.BossMogut
{
    public class BossMogutParalysisIdleState : BaseBossMogutBattleState
    {

        public override void Enter()
        {
            base.Enter();
        }

        public override void Update()
        {
            base.Update();
            bossMogutLogic.attackCdTimer -= Time.deltaTime;
            if (bossMogutLogic.attackCdTimer <= 0) { bossMogutLogic.attackCdTimer = 0; }
            if (bossMogutLogic.attackCdTimer <= 0)
            {
                SkillCastLogic();
            }
        }

        private void SkillCastLogic()
        {
            // 怪物被击倒不能起身之后只能放虫子
            if (bossMogutLogic.curBornWormCount < bossMogutLogic.maxBornWormCount)
            {
                bossMogutLogic.CreateWormWood();
            }
        }

        public override void Exit()
        {
            base.Exit();
        }
    }
}