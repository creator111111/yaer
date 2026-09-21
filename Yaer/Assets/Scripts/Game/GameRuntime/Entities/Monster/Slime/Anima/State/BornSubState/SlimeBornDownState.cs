using UnityEngine;

namespace Game.GameRuntime.Entities.Monster.Slime.Anima.State.BornSubState
{
    public class SlimeBornDownState : BaseSlimeState
    {
        public override void Enter()
        {
            base.Enter();

            slime.BodyRg.velocity = Vector2.zero;
            // 0912 方案 A：掉树落地瞬间对齐玩家战斗轴 Y（此前只认 IsGrounded，落偏后 Move 只改 X）
            SnapToCombatAxisY();
        }

        public override void Update()
        {
            base.Update();

            if (IsFinished) ExitCurrentStateMachine().ChangeState<SlimeIdleState>();
        }

        public override void Exit()
        {
            base.Exit();
            slime.isFallDownAtk = false;
            slime.isProtect = false;
        }
    }
}
