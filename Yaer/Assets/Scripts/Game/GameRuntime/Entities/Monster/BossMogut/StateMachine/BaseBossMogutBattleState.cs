using DG.Tweening;
using Game.GameMgr.Component.Archive.ArchiveDataClass.Player;
using Game.GameRuntime.Entities.Component.Anima;
using Game.GameRuntime.Entities.Component.Anima.interf;
using Game.GameRuntime.Entities.Component.Battle;
using Game.GameRuntime.Entities.Component.Move;
using Game.GameRuntime.Entities.Player.Components.CsAnimator.Base;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.GameRuntime.Entities.Monster.BossMogut
{
    public class BaseBossMogutBattleState : BaseMonsterState
    {
        protected BossMogutLogic bossMogutLogic;
        protected MoveComponent moveComponent;
        protected BossMogutCsAnimator csAnimator;
        protected BattleComponent battleComponent;

        protected float moveCdTimeCount = 0; // 每次行动后的冷却时间计数器
        protected float moveCdTargetTime = 3; // 每次行动后的冷却时间


        public override void Init(IStateMachine stateMachine, string argsName, string stateName)
        {
            base.Init(stateMachine, argsName, stateName);
            bossMogutLogic = stateMachine.GetEntityLogic<BossMogutLogic>();
            moveComponent = bossMogutLogic.componentSystem.GetComponent<MoveComponent>();
            csAnimator = bossMogutLogic.componentSystem.GetComponent<BossMogutCsAnimator>();
            battleComponent = bossMogutLogic.componentSystem.GetComponent<BattleComponent>();
            animationEventComponent = bossMogutLogic.GetComponent<AnimationEventComponent>();
            animationEventComponent.RegisterEvent("PlayAudioSfx", PlayAudioSfx);
        }

        public override void Enter()
        {
            base.Enter();
            animationEventComponent.RegisterEvent("PerformAttack", PerformAttack);
        }

        public override void Update()
        {
            base.Update();
        }



        private void PerformAttack(string skillName)
        {
            battleComponent.PerformAttack(skillName);
            bossMogutLogic.OnPerformAttack?.Invoke(skillName);
        }

        protected override void CreateMAtkCollsion(string atkArgs)
        {
            // BOSS怪物的攻击碰撞体和特效是直接添加到怪物预支体上的，所以只需要SetActive就行
            var valueList = atkArgs.Split(',');
            var monsterName = valueList.Count() > 0 ? valueList[0] : ""; // 怪物名字
            var atkTypeName = valueList.Count() > 1 ? valueList[1] : ""; // 怪物招式类型

            bossMogutLogic.CheckBossAtkCollisonShow(atkTypeName, true);
        }

        protected override void RemoveMAtkCollsion(string atkArgs)
        {
            var valueList = atkArgs.Split(',');
            var monsterName = valueList.Count() > 0 ? valueList[0] : ""; // 怪物名字
            var atkTypeName = valueList.Count() > 1 ? valueList[1] : ""; // 怪物招式类型
            bossMogutLogic.CheckBossAtkCollisonShow(atkTypeName, false);
        }


        // 造成屏幕震动
        public override void ShowCameraImpluse(string args)
        {
            var posList = args.Split(',');
            if (posList.Length < 3) { return; }
            var newVec3 = new Vector3(float.Parse(posList[0]), float.Parse(posList[1]), float.Parse(posList[2]));
            WestRappRoadBossBattleMgr.getInstance().StartCameraImpluse(newVec3);
        }
    }
}

