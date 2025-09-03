using Game.GameRuntime.Entities.Component.Anima.interf;
using Game.GameRuntime.Entities.Component.Move;
using Game.GameRuntime.Entities.Component.Path;
using Game.GameRuntime.Entities.Monster.Slime.Anima.State.JumpAtkSubState;
using Game.GameRuntime.Entities.Monster.WoodWorm.Components.Anima.State;
using Game.GameRuntime.Entities.Monster.WoodWorm;
using Game.GameRuntime.Entities.Player;
using UnityEngine;
using System.Collections.Generic;
using Game.GameRuntime.Entities.Component.PhysicsDetect.FindTarget;
using System;
using UnityEngine.SceneManagement;
using Game.GameRuntime.GameSceneManager.Base;

namespace Game.GameRuntime.Entities.Monster.Slime.Anima.State
{
    public class SlimeMoveState : BaseSlimeState
    {
        private PlayerLogic playerLogic;
        private PathfindingComponent pathfindingCpn;
        private bool startPathfinding;
        private HashSet<PlayerLogic> moveTarget = new HashSet<PlayerLogic>();
        private HashSet<PlayerLogic> attackTarget = new HashSet<PlayerLogic>();
        private FindTargetComponent findTargetCpn;

        public override void Init(IStateMachine stateMachine, string argsName, string stateName)
        {
            base.Init(stateMachine, argsName, stateName);

            // pathfindingCpn = slime.componentSystem.GetComponent<PathfindingComponent>();
            findTargetCpn = slime.componentSystem.GetComponent<FindTargetComponent>();
        }

        public override void Enter()
        {
            base.Enter();
            // 目标丢失
            if (!findTargetCpn.HasTarget(playerLogic))
            {
                moveTarget.Clear();
            }

            attackTarget.Clear();
            slime.atkTargetLogic = null;
            slime.BodyRg.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        public override void Update()
        {
            if (slime.IsDead)
            {
                Debug.Log("=================已经死亡");
                return;
            }
            base.Update();
            if (CheckObjIsPause())
            {
                moveCpn.StopMove();
                return;
            }
            //if (IsExit) return;

            //// 索敌
            //if (slime.FinePlayer() == null)
            //{
            //    // 没有目标关闭寻路
            //    if (startPathfinding)
            //    {
            //        startPathfinding = false;
            //        pathfindingCpn.ShutDown();
            //    }dd
            //}
            //else
            //{
            //    slime.LookAtTarget();

            //    // 判断是否相同纵深
            //    if (slime.IsInSameDepth(slime.Target))
            //    {
            //        Debug.Log("相同纵深");

            //        // 攻击
            //        if (Mathf.Abs(slime.Target.GameObject.transform.position.x - slime.transform.position.x) < slime.AttackRange)
            //        {
            //            startPathfinding = false;

            //            switch (slime.AttackAction())
            //            {
            //                case 0:
            //                    // 普通攻击
            //                    ChangeState<SlimeAttackState>();
            //                    return;
            //                case 1:
            //                    // 跳跃攻击
            //                    EnterSubStateMachine<SlimeJumpAtkSubSM>().ChangeState<SlimeJumpAtkUpBefore>();
            //                    return;
            //            }

            //            return;
            //        }

            //        // 到达待机
            //        if (pathfindingCpn.IsArrived)
            //        {
            //            ChangeState<SlimeIdleState>();
            //            return;
            //        }
            //    }

            //    // 开启寻路
            //    // 判断目标最近的寻路点
            //    var p = slime.Target.GetPathfindingPos(slime.transform.position);
            //    pathfindingCpn.SetTarget(p);

            //    if (startPathfinding == false)
            //    {
            //        pathfindingCpn.StartUp();
            //        startPathfinding = true;
            //    }
            //}
            if (slime.HasMonsterState(MonsterState.Escape))
            {
                // 设置和玩家相反的方向进行逃跑
                if (slime.SceneManager != null)
                {
                    var playerLogic = slime.SceneManager.GetPlayerEntity();
                    var isInPlayerRight = playerLogic.gameObject.transform.position.x <= slime.gameObject.transform.position.x;
                    TurnToRight(isInPlayerRight);
                }
                return;
            }
            if (moveTarget.Count == 0)
            {
                findTargetCpn.FindTarget(ref moveTarget, "FindtArea");
            }
            
            if (moveTarget.Count > 0)
            {
                // 移动
                foreach (var logic in moveTarget)
                {
                    if (slime.atkTargetLogic == null) slime.atkTargetLogic = logic;
                    var targetDir = (logic.transform.position - slime.transform.position).normalized;
                    // 获取当前方向
                    TurnToRight(targetDir.x > 0);
                }
            }
            slime.attackCdTimer -= Time.deltaTime;
            if (slime.attackCdTimer <= 0) { slime.attackCdTimer = 0; }
            findTargetCpn.FindTarget(ref attackTarget, "AttackArea");
            if (attackTarget.Count > 0)
            {
                if (slime.attackCdTimer > 0)
                {
                    ChangeState<SlimeIdleState>();
                }
                else
                {
                    var hasJumpAtk = GameTools.randomRateHasGet(25);
                    if (!hasJumpAtk) { ChangeState<SlimeAttackState>(); }
                    else { EnterSubStateMachine<SlimeJumpAtkSubSM>().ChangeState<SlimeJumpAtkUpBefore>(); }
                }
            }
            // 没找到目标时会闲逛
            timeCount += Time.deltaTime;
            if (monsterLogic.canRandomMove && moveTarget.Count <= 0 && attackTarget.Count <= 0 && timeCount > timeDistance)
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
                ChangeState<SlimeIdleState>();
            }
            else
            {
                var moveRight = GameTools.getRandomIntNum(0, 1) == 0;
                TurnToRight(moveRight);
            }
        }

        void TurnToRight(bool isRight)
        {
            if (!isRight)
            {
                moveCpn.moveSpeedX = slime.baseMoveSpeed;
                moveCpn.MoveLeft(false);
            }
            else
            {
                moveCpn.moveSpeedX = -slime.baseMoveSpeed;
                moveCpn.MoveRight(false);
            }
        }

        public override void Exit()
        {
            base.Exit();

            //startPathfinding = false;
            //pathfindingCpn.ShutDown();
        }
    }
}