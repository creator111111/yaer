using Game.GameMgr.Component.UI;
using Game.GameMgr;
using Game.GameRuntime.Entities.Player.Components.CsAnimator.Base;
using Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat.State.Ground;
using Game.Static.Path;

namespace Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat.State.Dead.FlyDead
{
    public class FlyDeadClsEndState : BasePlayerState
    {
        public override void Update()
        {
            base.Update();
            if (IsFinished)
            {
                if (IsTest)
                {
                    ExitCurrentStateMachine().ChangeState<CombatIdleState>();
                }
                ShowDeadPanel();
            }
        }
    }
}