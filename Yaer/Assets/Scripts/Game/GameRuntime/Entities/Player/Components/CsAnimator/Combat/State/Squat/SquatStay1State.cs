namespace Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat.State.Squat
{
    public class SquatStay1State : SquatStayState
    {
        public override void Update()
        {
            base.Update();
            FinishedChangeState<SquatStay2State>();
        }
    }
}