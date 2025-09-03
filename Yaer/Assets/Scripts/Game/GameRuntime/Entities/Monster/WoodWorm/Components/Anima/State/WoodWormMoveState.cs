using System;
using System.Collections.Generic;
using Game.GameRuntime.Component.Move;
using Game.GameRuntime.Entities.Component.Anima.interf;
using Game.GameRuntime.Entities.Component.Move;
using Game.GameRuntime.Entities.Component.PhysicsDetect.FindTarget;
using Game.GameRuntime.Entities.Monster.Slime;
using Game.GameRuntime.Entities.Player;
using Game.GameRuntime.GameSceneManager.Base;
using UnityEngine;

namespace Game.GameRuntime.Entities.Monster.WoodWorm.Components.Anima.State
{
    public class WoodWormMoveState : BaseWoodWormState
    {
        private PlayerLogic playerLogic;

        private FindTargetComponent findTargetCpn;
        
        private HashSet<PlayerLogic> moveTarget = new HashSet<PlayerLogic>();
        private HashSet<PlayerLogic> attackTarget = new HashSet<PlayerLogic>();

        public override void Init(IStateMachine stateMachine, string argsName, string stateName)
        {
            base.Init(stateMachine, argsName, stateName);

            findTargetCpn = woodWormLogic.componentSystem.GetComponent<FindTargetComponent>();
            
        }

        public override void Enter()
        {
            base.Enter();
            timeCount = timeDistance; // 第一次计时器设置最大值
            // 目标丢失
            if (!findTargetCpn.HasTarget(playerLogic))
            {
                moveTarget.Clear();
            }
            // 锁定玩家时直接全局查找玩家对象
            if (woodWormLogic.isLockPlayer)
            {
                var sceneMgr = woodWormLogic.GetSceneManager();
                var playerObj = sceneMgr.GetPlayerEntity();
                if (playerObj != null && playerObj.Logic is PlayerLogic playerLogic)
                {
                    moveTarget.Add(playerLogic);
                }
            }

            attackTarget.Clear();
        }

        public override void Update()
        {
            if (monsterLogic.IsDead) { return; }

            base.Update();
            if (CheckObjIsPause())
            {
                moveCpn.StopMove();
                return;
            }
            if (woodWormLogic.rootBorn && woodWormLogic.HasMonsterState(MonsterState.Escape))
            {
                // 转变为逃跑状态
                ChangeState<WoodWormEscapeState>();
                return;
            }
            if (moveTarget.Count == 0)
            {
                findTargetCpn.FindTarget(ref moveTarget, "MoveDetector");
            }

            var hasChaneDir = false;
            if (moveTarget.Count > 0)
            {
                // 移动
                var tempTarget = new HashSet<PlayerLogic>(moveTarget);
                foreach (var logic in tempTarget)
                {
                    playerLogic = logic;

                    var targetDir = (logic.transform.position - woodWormLogic.transform.position).normalized;

                    // 获取当前方向
                    hasChaneDir = TurnToRight(targetDir.x > 0);
                }

                if (hasChaneDir) return;
            }

            woodWormLogic.attackCdTimer -= Time.deltaTime;
            if (woodWormLogic.attackCdTimer <= 0) { woodWormLogic.attackCdTimer = 0; }
            findTargetCpn.FindTarget(ref attackTarget, "AttackDetector");
            if (attackTarget.Count > 0)
            {
                if (woodWormLogic.attackCdTimer > 0)
                {
                    ChangeState<WoodWormIdleState>();
                }
                else
                {
                    ChangeState<WoodWormAttackState>();
                }
                return;
            }

            // 没找到目标时会闲逛
            timeCount += Time.deltaTime;
            if (monsterLogic.canRandomMove && moveTarget.Count <= 0 && timeCount > timeDistance)
            {
                timeCount = 0;
                randomMove();
            }
        }

        void randomMove()
        {
            var hasIdle = GameTools.getRandomIntNum(0, 1) == 0;
            if (hasIdle)
            {
                ChangeState<WoodWormIdleState>();
            }
            else
            {
                var moveRight = GameTools.getRandomIntNum(0, 1) == 0;
                TurnToRight(moveRight);
            }
        }

        private bool TurnToRight(bool isRight)
        {
            if (!isRight)
            {
                // 先转向,再移动
                if (moveCpn.Direction == EDirectionType.Right)
                {
                    ChangeState<WoodWormTurnState>();
                    return true;
                }
                else
                {
                    moveCpn.moveSpeedX = woodWormLogic.baseMoveSpeed;
                    moveCpn.MoveLeft(false);
                }
            }
            else
            {
                // 先转向,再移动
                if (moveCpn.Direction == EDirectionType.Left)
                {
                    ChangeState<WoodWormTurnState>();
                    return true;
                }
                else
                {
                    moveCpn.moveSpeedX = -woodWormLogic.baseMoveSpeed;
                    moveCpn.MoveRight(false);
                }
            }
            return false;
        }

        public override void Exit()
        {
            base.Exit();
            moveCpn.StopMove();
            moveTarget.Clear();
            attackTarget.Clear();
        }
    }
}