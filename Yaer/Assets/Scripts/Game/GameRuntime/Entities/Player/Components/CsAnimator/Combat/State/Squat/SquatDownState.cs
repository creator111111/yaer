namespace Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat.State.Squat
{
    public class SquatDownState : BaseCombatState
    {
        public override void Enter()
        {
            base.Enter();
        }


        public override void Update()
        {
            base.Update();

            FinishedChangeState<SquatStay1State>();
        }
    }
}