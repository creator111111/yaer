using Game.GameRuntime.Entities.Component.Anima.interf;
using Game.GameRuntime.Entities.Player.Components.CsAnimator.Base;
using UnityEngine;

namespace Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat.State.Damage.DamageFly
{
    public class DamageFlySM : BasePlayerSM
    {
        public Vector2 endPosition;

        public override void Init(ICsAnimator csAnimator, string name, string enterArgs, IStateMachine parent = null)
        {
            base.Init(csAnimator, name, enterArgs, parent);
            
            RegisterState<DamageFlyUpState>("DamageFly_FlyUp", "FlyUp");

            RegisterState<DamageFlyFallState>("DamageFly_FlyFall", "FlyFall");
            RegisterState<DamageFlyClimbUpState>("DamageFly_FlyClimbUp", "FlyClimbUp");

            RegisterState<DamageFlyClsState>("DamageFly_FlyCls", "FlyCls");
            RegisterState<DamageFlyClsClimbUpState>("DamageFly_FlyClsClimbUp", "FlyClsClimbUp");

        }
    }
}