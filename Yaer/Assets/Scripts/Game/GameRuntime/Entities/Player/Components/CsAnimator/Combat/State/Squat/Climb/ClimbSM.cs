using Game.GameRuntime.Entities.Component.Anima.interf;
using Game.GameRuntime.Entities.Player.Components.CsAnimator.Base;

namespace Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat.State.Squat.Climb
{
    public class ClimbSM : BasePlayerSM
    {
        public override void Init(ICsAnimator csAnimator, string name, string enterArgs, IStateMachine parent)
        {
            base.Init(csAnimator, name, enterArgs, parent);
            
            RegisterState<ClimbDownState>("Squat_Climb_Down", "ClimbDown");
            RegisterState<ClimbUpState>("Squat_Climb_Up", "ClimbUp");
            RegisterState<ClimbMoveState>("Squat_Climb_Move", "ClimbMove");
        }

        public override void Enter()
        {
            base.Enter();
            SetSign("IsClimb", true);
        }

        public override void Exit()
        {
            base.Exit();

            SetSign("IsClimb", false);
        }
    }
}