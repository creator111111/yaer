using Game.GameRuntime.Entities.Player.Components.CsAnimator.Base;

namespace Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat.State.Damage
{
    public class BasePlayerDamageState: BasePlayerState
    {
        public override void Enter()
        {
            base.Enter();
            playerLogic.isProtect = true;
            playerLogic.canInStateSetPos = true;
            SetSign("IsDamaging", true);
            moveComponent.StopMove();
            csAnimator.SetAnimationTsf(playerLogic.transform.position);
        }
    }
}