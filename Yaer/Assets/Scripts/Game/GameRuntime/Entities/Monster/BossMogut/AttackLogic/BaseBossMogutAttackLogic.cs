using Game.GameRuntime.Entities.Component.Battle;
using Game.GameRuntime.Entities.Component.Battle.Attack;
using Game.GameRuntime.Entities.Component.Battle.SkillInfos;

namespace Game.GameRuntime.Entities.Monster
{
    public class BaseBossMogutAttackLogic : AttackLogic
    {
        private SimpleDamageAttackData data;

        public override void Initialize(SkillInfo info, BattleComponent attacker)
        {
            base.Initialize(info, attacker);
            data = info.data as SimpleDamageAttackData;
        }

        public override void Cancel()
        {
            
        }

        public override bool CanExecute()
        {
            return true;
        }

        public override void Execute()
        {
            //throw new System.NotImplementedException();
        }
    }
}

