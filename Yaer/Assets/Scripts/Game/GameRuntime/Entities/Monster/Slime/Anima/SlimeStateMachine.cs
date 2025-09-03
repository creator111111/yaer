using Game.GameRuntime.Entities.Component.Anima.interf;
using Game.GameRuntime.Entities.Monster.Slime.Anima.State;
using Game.GameRuntime.Entities.Monster.Slime.Anima.State.BornSubState;
using Game.GameRuntime.Entities.Monster.Slime.Anima.State.JumpAtkSubState;

namespace Game.GameRuntime.Entities.Monster.Slime.Anima
{
    public class SlimeStateMachine : BaseSlimeStateMachine
    {
        public override void Init(ICsAnimator csAnimator, string name, string enterArgs, IStateMachine parent = null)
        {
            base.Init(csAnimator, name, enterArgs, parent);
            
            // RegisterState(new SlimeSleepState(slime, this, "Sleep", "Sleep"));
            // RegisterState(new SlimeIdleState(slime, this, "Idle", "Idle"));
            // RegisterState(new SlimeMoveState(slime, this, "Move", "Move"));
            // RegisterState(new SlimeAttackState(slime, this, "Attack", "Attack"));
            // RegisterState(new SlimeWoundState(slime, this, "Wound", "Wound"));
            // RegisterState(new SlimeDeadState(slime, this, "Dead", "Dead"));
            // RegisterState(new SlimeJumpAttackState(slime, this, "JumpAttack", "JumpAttack"));
            //
            // RegisterSubStateMachine(new SlimeBornSubSM(slime, animator, "", "BornSubState", this));
            // RegisterSubStateMachine(new SlimeJumpAtkSubSM(slime, animator, "", "JumpAtkSubState", this));
            
            RegisterState<SlimeSleepState>("Sleep", "Sleep");
            RegisterState<SlimeIdleState>("Idle", "Idle");
            RegisterState<SlimeMoveState>("Move", "Move");
            RegisterState<SlimeAttackState>("Attack", "Attack");
            RegisterState<SlimeWoundState>("Wound", "Wound");
            RegisterState<SlimeDeadState>("Dead", "Dead");
            RegisterState<SlimeJumpAttackState>("JumpAttack", "JumpAttack");

            RegisterSubStateMachine<SlimeBornSubSM>("BornSubState", "BornSubState");
            RegisterSubStateMachine<SlimeJumpAtkSubSM>("JumpAtkSubState", "JumpAtkSubState");
        }

        public override void Enter()
        {
            base.Enter();
            var stateName = slime.getAniNameByState(slime.baseAniState);
            ChangeState(stateName);
            //ChangeState<SlimeIdleState>();
        }
    }
}