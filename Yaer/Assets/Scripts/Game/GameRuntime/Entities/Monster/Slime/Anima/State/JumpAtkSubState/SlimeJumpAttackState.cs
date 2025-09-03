namespace Game.GameRuntime.Entities.Monster.Slime.Anima.State.JumpAtkSubState
{
    public class SlimeJumpAttackState : BaseSlimeState
    {
        private bool isAttack;

        public override void Enter()
        {
            base.Enter();

            isAttack = false;
        }

        public override void Update()
        {
            base.Update();

            // 只触发一次伤害
            if (isAttack == false && StateInfo.normalizedTime > 0.5f) isAttack = slime.JumpAttackDetect();

            if (IsFinished) ChangeState<SlimeIdleState>();
        }
    }
}