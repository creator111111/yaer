using Game.GameRuntime.Entities.Monster.WoodWorm;

namespace Game.GameRuntime.Entities.Monster.Slime.Anima.State.JumpAtkSubState
{
    public class SlimeJumpAtkDownState : BaseSlimeState
    {

        public override void Enter()
        {
            base.Enter();
        }

        public override void Update()
        {
            if (monsterLogic.IsDead) { return; }
            base.Update();
            if (IsFinished) ExitCurrentStateMachine().ChangeState<SlimeIdleState>();
        }

        public override void Exit()
        {
            base.Exit();
            slime.EnterAttackCd();
        }
    }
}