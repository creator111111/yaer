using Game.GameRuntime.Entities.Component.Anima.interf;
using UnityEngine;

namespace Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat.State.Damage.DamageFly
{
    public class DamageFlyFallState : BasePlayerDamageState
    {
        private DamageFlySM sm;

        public override void Init(IStateMachine stateMachine, string argsName, string stateName)
        {
            base.Init(stateMachine, argsName, stateName);
            
            sm = stateMachine as DamageFlySM;
        }

        public override void Enter()
        {
            base.Enter();
            moveComponent.moveSpeedX = playerLogic.flyMoveSpeedX;
        }

        public override void Update()
        {
            base.Update();
            if (moveComponent.IsGrounded)
            {
                playerLogic.FallGroundEvent?.Invoke();
                ChangeState<DamageFlyClimbUpState>();
            }
        }

        public override void Exit()
        {
            base.Exit();

        }
    }
}