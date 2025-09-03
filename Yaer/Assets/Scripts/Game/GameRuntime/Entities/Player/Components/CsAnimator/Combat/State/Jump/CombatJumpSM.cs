using Game.GameRuntime.Entities.Component.Anima.interf;
using Game.GameRuntime.Entities.Player.Components.CsAnimator.Base;
using Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat.State.Jump.State;
using UnityEngine;

namespace Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat.State.Jump
{
    public class CombatJumpSM : BasePlayerSM
    {
        public Vector2 endPos;
        public Vector2 startPos;
        

        public override void Init(ICsAnimator csAnimator, string name, string enterArgs, IStateMachine parent)
        {
            base.Init(csAnimator, name, enterArgs, parent);
            
            RegisterState<FallDownIdleState>("Jump_FallDownIdle", "FallDownIdle");
            RegisterState<FallDownRunState>("Jump_FallDownRun", "FallDownRun");
            RegisterState<JumpUpState>("Jump_JumpUp", "JumpUp");
            RegisterState<JumpFallState>("Jump_JumpFall", "JumpFall");
            RegisterState<RunToJumpState>("Jump_RunToJump", "RunToJump");
            
        }
    }
}