using Game.GameRuntime.Entities.Component.Move;
using Game.GameRuntime.Entities.Component.PhysicsDetect.FindTarget;
using Game.GameRuntime.Entities.Monster.WoodWorm.Components.Anima.State;
using Game.GameRuntime.Entities.Monster.WoodWorm;
using UnityEngine;
using Game.GameRuntime.Entities.Player;
using System.Collections.Generic;
using Game.GameRuntime.Entities.Monster.Slime.Anima.State.JumpAtkSubState;
using Game.GameRuntime.Entities.Monster.TenWan;

namespace Game.GameRuntime.Entities.Monster.Slime.Anima.State
{
    public class SlimeIdleState : BaseSlimeState
    {
        private HashSet<PlayerLogic> moveTarget = new HashSet<PlayerLogic>();
        private HashSet<PlayerLogic> attackTarget = new HashSet<PlayerLogic>();
        public override void Enter()
        {
            base.Enter();
            //slime.BodyRg.velocity = Vector2.zero;
            moveCpn.StopMove();
            //moveCpn.moveSpeedY = -1f;
            moveTarget.Clear();
            attackTarget.Clear();
            moveCpn.IsGrounded = true;
            slime.atkTargetLogic = null;
            // 站立状态下设置不能移动
            slime.BodyRg.constraints = RigidbodyConstraints2D.FreezePositionX;
        }

        public override void Update()
        {
            if (slime.IsDead)
            {
                return;
            }
            base.Update();
            if (CheckObjIsPause())
            {
                moveCpn.StopMove();
                return;
            }
            // 逃跑状态需要直接转换成移动状态
            if (slime.HasMonsterState(MonsterState.Escape))
            {
                ChangeState<SlimeMoveState>();
                return;
            }
            //if (slime.FinePlayer() != null) ChangeState<SlimeMoveState>();
            // 优先判断攻击逻辑
            slime.componentSystem.GetComponent<FindTargetComponent>().FindTarget(ref attackTarget, "AttackArea");
            slime.attackCdTimer -= Time.deltaTime;
            if (slime.attackCdTimer <= 0) { slime.attackCdTimer = 0; }
            if (attackTarget.Count > 0)
            {
                // 判断攻击方向
                foreach (var logic in attackTarget)
                {
                    if (slime.atkTargetLogic == null) slime.atkTargetLogic = logic;
                    var targetDir = (logic.transform.position - slime.transform.position).normalized;

                    // 获取当前方向
                    if (targetDir.x <= 0 && moveCpn.Direction == EDirectionType.Right)
                    {
                        // 转向
                        moveCpn.StopMove();
                        moveCpn.MoveLeft(false);
                    }
                    else if(targetDir.x > 0 && moveCpn.Direction == EDirectionType.Left)
                    {
                        // 转向
                        moveCpn.StopMove();
                        moveCpn.MoveRight(false);
                    }
                }
                if (slime.atkTargetLogic == null) { return; }
                if (slime.attackCdTimer <= 0)
                {
                    var hasJumpAtk = GameTools.randomRateHasGet(25);
                    if (!hasJumpAtk) { ChangeState<SlimeAttackState>(); }
                    else { EnterSubStateMachine<SlimeJumpAtkSubSM>().ChangeState<SlimeJumpAtkUpBefore>(); }
                }
                else
                {
                    attackTarget.Clear();
                }
                return;
            }

            slime.componentSystem.GetComponent<FindTargetComponent>().FindTarget(ref moveTarget, "FindtArea");

            if (moveTarget.Count > 0)
            {
                ChangeState<SlimeMoveState>();
                return;
            }

            // 没找到目标时会闲逛
            timeCount += Time.deltaTime;
            if (monsterLogic.canRandomMove && timeCount > timeDistance)
            {
                timeCount = 0;
                ChangeState<SlimeMoveState>();
            }
        }

        public override void Exit()
        {
            base.Exit();
            slime.BodyRg.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
    }
}