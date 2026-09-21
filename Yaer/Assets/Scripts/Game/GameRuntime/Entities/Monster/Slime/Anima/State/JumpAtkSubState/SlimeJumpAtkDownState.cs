using Game.GameRuntime.Entities.Monster.WoodWorm;

namespace Game.GameRuntime.Entities.Monster.Slime.Anima.State.JumpAtkSubState
{
    public class SlimeJumpAtkDownState : BaseSlimeState
    {

        public override void Enter()
        {
            base.Enter();
            // 0912 方案 A：跳攻落地后再对齐轴线；升空过程允许不同轴（OPEN Q3）
            SnapToCombatAxisY();
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
