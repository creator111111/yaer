using System.Collections.Generic;
using Game.GameRuntime.Entities.Component.Move;
using Game.GameRuntime.Entities.Component.PhysicsDetect.FindTarget;
using Game.GameRuntime.Entities.Monster.TenWan;
using Game.GameRuntime.Entities.Player;
using Game.GameRuntime.GameSceneManager.Base;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.GameRuntime.Entities.Monster.WoodWorm.Components.Anima.State
{
    public class WoodWormIdleState : BaseWoodWormState
    {
        private HashSet<PlayerLogic> moveTarget = new HashSet<PlayerLogic>();
        private HashSet<PlayerLogic> attackTarget = new HashSet<PlayerLogic>();
        
        public override void Enter()
        {
            base.Enter();
            moveCpn.StopMove();
            moveTarget.Clear();
            attackTarget.Clear();
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
        }

        public override void Update()
        {
            if (monsterLogic.IsDead) { return; }
            base.Update();
            if (CheckObjIsPause()) {
                moveCpn.StopMove();
                return; 
            }
            // 逃跑状态
            if (woodWormLogic.rootBorn && woodWormLogic.HasMonsterState(MonsterState.Escape))
            {
                ChangeState<WoodWormEscapeState>();
                return;
            }
            // 优先判断攻击逻辑
            woodWormLogic.componentSystem.GetComponent<FindTargetComponent>().FindTarget(ref attackTarget, "AttackDetector");
            woodWormLogic.attackCdTimer -= Time.deltaTime;
            if (woodWormLogic.attackCdTimer <= 0) { woodWormLogic.attackCdTimer = 0; }
            if (attackTarget.Count > 0)
            {
                // 判断攻击方向
                foreach (var logic in attackTarget)
                {
                    var targetDir = (logic.transform.position - woodWormLogic.transform.position).normalized;

                    // 获取当前方向
                    if (targetDir.x <= 0)
                    {
                        // 先转向,再移动
                        if (moveCpn.Direction == EDirectionType.Right)
                        {
                            ChangeState<WoodWormTurnState>();
                        }
                    }
                    else
                    {
                        // 先转向,再移动
                        if (moveCpn.Direction == EDirectionType.Left)
                        {
                            ChangeState<WoodWormTurnState>();
                        }
                    }
                }
                if (woodWormLogic.attackCdTimer <= 0)
                {
                    ChangeState<WoodWormAttackState>();
                }
                else
                {
                    attackTarget.Clear();
                }
                return;
            }

            woodWormLogic.componentSystem.GetComponent<FindTargetComponent>().FindTarget(ref moveTarget, "MoveDetector");

            if (moveTarget.Count > 0)
            {
                ChangeState<WoodWormMoveState>();
                return;
            }

            // 没找到目标时会闲逛
            timeCount += Time.deltaTime;
            if (monsterLogic.canRandomMove && timeCount > timeDistance)
            {
                timeCount = 0;
                ChangeState<WoodWormMoveState>();
            }
        }
    }
}