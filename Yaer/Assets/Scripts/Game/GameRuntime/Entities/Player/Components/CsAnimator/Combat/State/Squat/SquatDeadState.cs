using Game.GameMgr.Component.UI;
using Game.GameMgr;
using Game.GameRuntime.Entities.Player.Components.CsAnimator.Base;
using Game.Static.Path;

namespace Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat.State.Squat
{
    public class SquatDeadState : BasePlayerState
    {
        public override void Update()
        {
            base.Update();
            if (IsTest)
            {
                FinishedChangeState<SquatStay2State>();
            }
            if (IsFinished)
            {
                ShowDeadPanel();
            }
        }
    }
}