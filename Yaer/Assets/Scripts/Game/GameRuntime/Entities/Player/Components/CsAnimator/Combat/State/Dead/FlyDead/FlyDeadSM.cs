using Game.GameRuntime.Entities.Component.Anima.interf;
using Game.GameRuntime.Entities.Player.Components.CsAnimator.Base;
using UnityEngine;

namespace Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat.State.Dead.FlyDead
{
    public class FlyDeadSM : BasePlayerSM
    {
        public Vector2 endPosition;

        public override void Init(ICsAnimator csAnimator, string name, string enterArgs, IStateMachine parent)
        {
            base.Init(csAnimator, name, enterArgs, parent);

            RegisterState<FlyDeadUpState>("FlyDead_Up", "FlyDeadUp");
            RegisterState<FlyDeadFallState>("FlyDead_Fall", "FlyDeadFall");
            RegisterState<FlyDeadEndState>("FlyDead_End", "FlyDeadEnd");

            RegisterState<FlyDeadClsState>("FlyDead_Cls", "FlyDeadCls");
            RegisterState<FlyDeadClsEndState>("FlyDead_ClsEnd", "FlyDeadClsEnd");
        }
    }
}