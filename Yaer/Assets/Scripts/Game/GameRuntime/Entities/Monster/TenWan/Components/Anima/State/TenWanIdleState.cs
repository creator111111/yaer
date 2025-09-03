using System.Collections.Generic;
using Game.GameRuntime.Entities.Component.Move;
using Game.GameRuntime.Entities.Component.PhysicsDetect.FindTarget;
using Game.GameRuntime.Entities.Monster.Slime;
using Game.GameRuntime.Entities.Monster.WoodWorm;
using Game.GameRuntime.Entities.Player;
using UnityEngine;

namespace Game.GameRuntime.Entities.Monster.TenWan.Components.Anima.State
{
    public class TenWanIdleState : BaseTenWanState
    {
        private HashSet<BaseEntityLogic> targets = new HashSet<BaseEntityLogic>();

        public override void Enter()
        {
            base.Enter();
            
            targets.Clear();
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
            // 中立单位所有目标都攻击
            tenWanLogic.componentSystem.GetComponent<FindTargetComponent>().FindTarget(ref targets, "AttackDetector");
            // 去除自身
            var tempTargets = new HashSet<BaseEntityLogic>(targets);
            foreach (var entity in tempTargets)
            {
                if (tenWanLogic == entity || entity.isDead)
                {
                    targets.Remove(entity);
                }
            }
            tenWanLogic.attackCdTimer -= Time.deltaTime;
            if (tenWanLogic.attackCdTimer <= 0) { tenWanLogic.attackCdTimer = 0; }
            if (targets.Count > 0)
            {
                // 判断攻击方向
                foreach (var logic in targets)
                {
                    
                    var targetDir = (logic.transform.position - tenWanLogic.transform.position).normalized;

                    // 获取当前方向
                    if (targetDir.x <= 0 && moveCpn.Direction == EDirectionType.Right)
                    {
                        // 转向
                        moveCpn.StopMove();
                        moveCpn.MoveLeft(false);
                    }
                    else if (targetDir.x > 0 && moveCpn.Direction == EDirectionType.Left)
                    {
                        // 转向
                        moveCpn.StopMove();
                        moveCpn.MoveRight(false);
                    }
                }
                if (tenWanLogic.attackCdTimer <= 0)
                {
                    ChangeState<TenWanAttackState>();
                }
            }
        }
    }
}