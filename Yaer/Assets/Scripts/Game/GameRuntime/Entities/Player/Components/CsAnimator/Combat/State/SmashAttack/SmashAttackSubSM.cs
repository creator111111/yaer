using Game.GameRuntime.Entities.Component.Anima.interf;
using Game.GameRuntime.Entities.Player.Components.CsAnimator.Base;


namespace Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat.State.SmashAttack
{
    public class SmashAttackSubSM : BasePlayerSM
    {
        public override void Init(ICsAnimator csAnimator, string name, string enterArgs, IStateMachine parent)
        {
            base.Init(csAnimator, name, enterArgs, parent);
            RegisterState<SmashAttack1State>("SmashAttack1", "SmashAttack1");
            RegisterState<SmashAttack2State>("SmashAttack2", "SmashAttack2");
        }

        public override void Enter()
        {
            base.Enter();
            SetSign("IsSmashAttacking", true);
        }

        public override void Exit()
        {
            base.Exit();
            SetSign("IsSmashAttacking", false);
        }
    }
}

