using Game.GameRuntime.Entities.Component.Anima.interf;
using Game.Static.Utility;
using UnityEngine;

namespace Game.GameRuntime.Entities.Monster.Slime.Anima.State.JumpAtkSubState
{
    public class SlimeJumpAtkUpState : BaseSlimeState
    {
        private Rigidbody2D rg; // 物体的刚体
        private SlimeJumpAtkSubSM sm;
        private Vector2 startPosition; // 起始位置

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
            if (sm.endPos == null) sm.endPos = slime.atkTargetLogic.gameObject.transform.position;
            //slime.FootCld.isTrigger = true;
            // 记录当前为跳跃状态
            slime.componentSystem.GetComponent<SlimeCsAnimator>().SetSign("IsJump", true);
            
            // 设置攻击碰撞体的父节点
            var attckNode = UIUtils.findChild(slime.gameObject, "JumpAttack");
            if (attckNode != null)
            {
                slime.atkCollNodeParent = attckNode;
            }
        }

        public override void Update()
        {
            base.Update();
            if (monsterLogic.IsDead) { return; }
            if (IsFinished)
            {
                ChangeState<SlimeJumpAtkFallState>();
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (monsterLogic.IsDead) { return; }
            if (IsExit) return;

            // 计算抛物线位置（仅限上升）
            var maxHeightPos = new Vector2(sm.endPos.x, sm.endPos.y + 3);
            var targetPosition = Physics2DUtility.CalculateParabolicPositionUp(startPosition, maxHeightPos, NormalizedTime);

            // 使用 MovePosition 来平滑移动物体
            if (rg != null)
            {
                rg.MovePosition(targetPosition);
            }
        }
    }
}