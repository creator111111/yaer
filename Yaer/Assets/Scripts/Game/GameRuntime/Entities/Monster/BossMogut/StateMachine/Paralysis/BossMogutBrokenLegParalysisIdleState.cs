using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Game.GameRuntime.Entities.Monster.BossMogut
{
    public class BossMogutBrokenLegParalysisIdleState : BaseBossMogutBattleState
    {
        public override void Enter()
        {
            base.Enter();
        }

        public override void Update()
        {
            base.Update();
            monsterLogic.attackCdTimer -= Time.deltaTime;
            if (monsterLogic.attackCdTimer <= 0) { monsterLogic.attackCdTimer = 0; }
            if (monsterLogic.attackCdTimer <= 0)
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

    }
}