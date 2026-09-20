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
            // 0912 ���� A������ Idle ͳһ Snap�������Ͱ������ϵĹ֣����в
            SnapToCombatAxisY();
            // վ���� X+Y�������ٶ�ƫ��JumpAtk UpBefore ��ⶳΪ FreezeRotation
            slime.BodyRg.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezePositionY;
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
            // ??????????????????????
            if (slime.HasMonsterState(MonsterState.Escape))
            {
                ChangeState<SlimeMoveState>();
                return;
            }
            //if (slime.FinePlayer() != null) ChangeState<SlimeMoveState>();
            // ??????????????
            slime.componentSystem.GetComponent<FindTargetComponent>().FindTarget(ref attackTarget, "AttackArea");
            slime.attackCdTimer -= Time.deltaTime;
            if (slime.attackCdTimer <= 0) { slime.attackCdTimer = 0; }
            if (attackTarget.Count > 0)
            {
                // ???????????
                foreach (var logic in attackTarget)
                {
                    if (slime.atkTargetLogic == null) slime.atkTargetLogic = logic;
                    var targetDir = (logic.transform.position - slime.transform.position).normalized;

                    // ??????????
                    if (targetDir.x <= 0 && moveCpn.Direction == EDirectionType.Right)
                    {
                        // ???
                        moveCpn.StopMove();
                        moveCpn.MoveLeft(false);
                    }
                    else if(targetDir.x > 0 && moveCpn.Direction == EDirectionType.Left)
                    {
                        // ???
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

            // ??????????????
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