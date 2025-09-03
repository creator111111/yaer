using Game.GameRuntime.Entities.Component.Anima.interf;
using UnityEngine;

namespace Game.GameRuntime.Entities.Monster.Slime.Anima.State.JumpAtkSubState
{
    public class SlimeJumpAtkSubSM : BaseSlimeStateMachine
    {
        public Vector2 endPos;

        public override void Init(ICsAnimator csAnimator, string name, string enterArgs, IStateMachine parent = null)
        {
            base.Init(csAnimator, name, enterArgs, parent);
            
            RegisterState<SlimeJumpAtkUpBefore>("JumpAtkSubState_UpBefore", "JumpAtk_UpBefore");
            RegisterState<SlimeJumpAtkUpState>("JumpAtkSubState_Up", "JumpAtk_Up");
            RegisterState<SlimeJumpAtkFallState>("JumpAtkSubState_Fall", "JumpAtk_Fall");
            RegisterState<SlimeJumpAtkDownState>("JumpAtkSubState_Down", "JumpAtk_Down");

        }


        public override void Enter()
        {
            base.Enter();

            slime.isJumpAttacking = true;
        }

        public override void Exit()
        {
            base.Exit();

            slime.isJumpAttacking = false;
        }
    }
}