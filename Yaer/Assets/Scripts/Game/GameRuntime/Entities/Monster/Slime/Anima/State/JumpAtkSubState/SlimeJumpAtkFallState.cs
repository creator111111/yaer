using Game.GameRuntime.Entities.Component.Anima.interf;
using Game.Static.Utility;
using UnityEngine;

namespace Game.GameRuntime.Entities.Monster.Slime.Anima.State.JumpAtkSubState
{
    public class SlimeJumpAtkFallState : BaseSlimeState
    {
        private Vector2 endPosition; // 目标位置（落点）
        private Rigidbody2D rg;      // 物体的刚体
        private  SlimeJumpAtkSubSM sm;
        private Vector2 startPosition; // 起始位置（最大高度）
        
        public override void Init(IStateMachine stateMachine, string argsName, string stateName)
        {
            base.Init(stateMachine, argsName, stateName);
            
            sm = stateMachine as SlimeJumpAtkSubSM;
        }

        public override void Enter()
        {
            base.Enter();

            rg = slime.BodyRg;

            startPosition = rg.position;
            endPosition = sm.endPos;
        }

        public override void Update()
        {
            base.Update();

            //if (rg.position.y < sm.endPos.y) ChangeState<SlimeJumpAtkDownState>();
            if (moveCpn.IsGrounded) ChangeState<SlimeJumpAtkDownState>();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (monsterLogic.IsDead) { return; }
            if (IsExit) return;

            // 计算物体的位置，基于动画进度进行下落
            var targetPosition = Physics2DUtility.CalculateParabolicPositionFall(startPosition, endPosition, NormalizedTime);
            if (rg != null)
            {
                rg.MovePosition(targetPosition);
            }
        }

        public override void Exit()
        {
            base.Exit();
            // 记录当前退出跳跃状态
            slime.componentSystem.GetComponent<SlimeCsAnimator>().SetSign("IsJump", false);
        }
    }
}
