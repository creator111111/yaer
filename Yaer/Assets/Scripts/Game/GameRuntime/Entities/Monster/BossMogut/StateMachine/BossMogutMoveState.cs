using Game.GameRuntime.Entities.Component.Anima.interf;
using Game.GameRuntime.Entities.Monster.WoodWorm.Components.Anima.State;
using Game.GameRuntime.Entities.Monster.WoodWorm;
using Game.GameRuntime.Entities.Player;
using System.Collections.Generic;
using UnityEngine;
using System;
using Game.GameMgr.Component;
using Game.GameMgr;
using UnityEngine.SceneManagement;
using UnityEditor;

namespace Game.GameRuntime.Entities.Monster.BossMogut
{
    public class BossMogutMoveState : BaseBossMogutBattleState
    {
        private HashSet<PlayerLogic> targets;

        private PlayerLogic targetPlayer;
        
        public override void Init(IStateMachine stateMachine, string argsName, string stateName)
        {
            base.Init(stateMachine, argsName, stateName);
        }

        public override void Enter()
        {
            base.Enter();
            targets = new HashSet<PlayerLogic>();

            lastDetectPlayer = false;
            lastStopMoveDetect = false;
            lastStartMoveDetect = false;
            IsFollowTarget = false;

            animationEventComponent.RegisterEvent("ShowCameraImpluse", ShowCameraImpluse);
        }

        public override void Update()
        {
            base.Update();
            if (targetPlayer != null && targetPlayer.isDead) { return; } // 玩家死亡后BOSS就不动了
            var sceneMgr = monsterLogic.GetSceneManager();
            if (sceneMgr != null && sceneMgr.GetSceneObjIsPause()) {
                return; 
            }
            // 每次BOSS攻击之后检测是否处于停顿状态
            moveCdTimeCount += Time.deltaTime;
            if (moveCdTimeCount < moveCdTargetTime)
            {
                csAnimator.SetFloat("Speed", 0);
                moveComponent.StopMove();
                return;
            }

            MoveLogic();
            monsterLogic.attackCdTimer -= Time.deltaTime;
            if (monsterLogic.attackCdTimer <= 0) { monsterLogic.attackCdTimer = 0; }
            if (monsterLogic.attackCdTimer <= 0)
            {
                SkillCastLogic();
            }
        }

        private bool lastDetectPlayer;
        private bool lastStopMoveDetect;
        private bool lastStartMoveDetect;

        private bool IsFollowTarget;

        private void MoveLogic()
        {
            // 第一次找到玩家之后之后就不需要再找了
            if (!targetPlayer) { targetPlayer = FindPlayer("PlayerDetector"); }
            bool detectPlayer = targetPlayer != null;
            bool stopMoveDetect = FindPlayer("StopMoveDetector") != null;
            bool startMoveDetect = FindPlayer("StartMoveDetector") != null;

            TurnDirection();

            if (!IsFollowTarget)
            {
                csAnimator.SetFloat("Speed", 0);
                moveComponent.StopMove();
                if (!lastDetectPlayer && detectPlayer) IsFollowTarget = true;
                if (lastStopMoveDetect && !startMoveDetect) IsFollowTarget = true;
            }
            else
            {
                csAnimator.SetFloat("Speed", targetPlayer != null ? 1 : 0);
                moveComponent.ApplyAnimatedMoveSpeed();

                if (!detectPlayer && lastDetectPlayer) IsFollowTarget = false;
                if (!lastStartMoveDetect && startMoveDetect) IsFollowTarget= false;
            }

            if (!detectPlayer || stopMoveDetect)
            {
                IsFollowTarget = false;
            }

            lastDetectPlayer = detectPlayer;
            lastStopMoveDetect = stopMoveDetect;
            lastStartMoveDetect = startMoveDetect;
        }

        private void TurnDirection()
        {
            if (targetPlayer == null) return;
            bool targetOnRight = targetPlayer.transform.position.x > moveComponent.Root.position.x;
            //if (targetOnRight && moveComponent.Direction == Component.Move.EDirectionType.Left)
            //{
            //    moveComponent.MoveRight();
            //}
            //else if (!targetOnRight && moveComponent.Direction == Component.Move.EDirectionType.Right)
            //{
            //    moveComponent.MoveLeft();
            //}
            if (targetOnRight)
            {
                moveComponent.MoveRight();
            }
            else if (!targetOnRight)
            {
                moveComponent.MoveLeft();
            }
        }

        //private List<string> SkillNames = new List<string>() { "Attack1", "Attack2", "Trample" };
        private List<string> SkillNames = new List<string>() { "Attack1", "Attack2", "Trample", "CreartWormWood" };


        //private void SkillCastLogic()
        //{
        //    foreach (var skillName in SkillNames)
        //    {
        //        if (battleComponent.SkillInCDTime(skillName)) continue;
        //        string detectorName = $"{skillName}Detector";
        //        targetPlayer = FindPlayer(detectorName);
        //        if (targetPlayer != null)
        //        {
        //            switch (skillName)
        //            {
        //                case "Attack1":
        //                    ChangeState<BossMogutAttack1State>();
        //                    break;
        //                case "Attack2":
        //                    ChangeState<BossMogutAttack2State>(); 
        //                    break;
        //                case "Trample":
        //                    ChangeState<BossMogutTrampleState>(); 
        //                    break;
        //            }
        //            break;
        //        }
        //    }
        //}

        private void SkillCastLogic()
        {
            var randomName = GameTools.getRandomValueFromList(SkillNames);
            if (!targetPlayer) { return; }
            var distance = Math.Abs(targetPlayer.transform.position.x - bossMogutLogic.transform.position.x);
            bool isMustAtk2 = false;
            if (distance <= 6 && bossMogutLogic.lastSkillName != "Attack2")
            {
                // 距离过小并且上一招不是重击，则下一招必定是重击
                isMustAtk2 = true;
                randomName = "Attack2";
            }
            if (bossMogutLogic.IsBrokenLeg)
            {
                // 怪物被击倒不能起身之后只能放虫子
                randomName = "CreartWormWood";
            }
            bossMogutLogic.lastSkillName = randomName;
            if (targetPlayer != null)
            {
                switch (randomName)
                {
                    case "Attack1":
                        ChangeState<BossMogutAttack1State>();
                        break;
                    case "Attack2":
                        // 选中重击还是有概率使用轻击
                        if (!isMustAtk2 && GameTools.getRandomIntNum(0, 1) == 0)
                        {
                            ChangeState<BossMogutAttack1State>();
                        }
                        else
                        {
                            ChangeState<BossMogutAttack2State>();
                        }
                        break;
                    case "Trample":
                        ChangeState<BossMogutTrampleState>();
                        break;
                    case "CreartWormWood":
                        if (bossMogutLogic.curBornWormCount >= bossMogutLogic.maxBornWormCount
                            && !bossMogutLogic.IsBrokenLeg)
                        {
                            // 如果当前产生的蠕虫数量已经达到上限则使用轻击
                            ChangeState<BossMogutAttack1State>();
                        }
                        else
                        {
                            bossMogutLogic.CreateWormWood();
                            // 设置进入CD
                            bossMogutLogic.EnterAttackCd();
                            bossMogutLogic.attackCdTimer /= 2; // 释放虫子之后的攻击间隔减半
                            moveCdTimeCount = 1;
                        }
                        break;
                    default:
                        break;
                }
                
            }
            
        }


        public override void Exit() 
        { 
            base.Exit();
            moveComponent.StopMove();
            csAnimator.SetFloat("Speed", 0);
        }

        private PlayerLogic FindPlayer(string detectorName)
        {
            targets.Clear();
            bossMogutLogic.findTargetComponent.FindTarget<PlayerLogic>(ref targets, detectorName);

            if (targets.Count > 0)
            {
                foreach (PlayerLogic target in targets)
                {
                    return target;
                }
            }
            return null;
        }
    }
}
