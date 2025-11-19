using Game.GameMgr;
using Game.GameMgr.Component;
using Game.GameRuntime.GameSceneManager.Base;
using Game.Static.Enum;
using System.Collections.Generic;
using UnityEngine;

namespace Game.GameRuntime.Entities.Component
{
    public class PlayerStaminaComponent : StaminaComponent
    {
        [SerializeField]
        public PlayerStaminaConfig config;
        //============消耗体力相关
        Dictionary<EGameHard, float> norAtk1Datas;
        Dictionary<EGameHard, float> norAtk2Datas;
        Dictionary<EGameHard, float> norAtk3Datas;
        Dictionary<EGameHard, float> smashAtk1Datas;
        Dictionary<EGameHard, float> smashAtk2Datas;
        Dictionary<EGameHard, float> dashAtkDatas;
        Dictionary<EGameHard, float> squatAtkDatas;
        Dictionary<EGameHard, float> climbDatas;
        Dictionary<EGameHard, float> jumpDatas;
        Dictionary<EGameHard, float> runDatas;
        //============恢复体力相关
        Dictionary<EGameHard, float> idleDatas;
        Dictionary<EGameHard, float> squatDatas;
        Dictionary<EGameHard, float> sitDatas;

        private void Start()
        {
            norAtk1Datas = new Dictionary<EGameHard, float>(){
                { EGameHard.Easy, config.NormalAttackCost },{ EGameHard.Normal, config.NormalAttackCost_1 },
                { EGameHard.Hard, config.NormalAttackCost_2 }, { EGameHard.Hardest, config.NormalAttackCost_3}
            };
            norAtk2Datas = new Dictionary<EGameHard, float>(){
                { EGameHard.Easy, config.NormalAttack2Cost },{ EGameHard.Normal, config.NormalAttack2Cost_1 },
                { EGameHard.Hard, config.NormalAttack2Cost_2 }, { EGameHard.Hardest, config.NormalAttack2Cost_3}
            };
            norAtk3Datas = new Dictionary<EGameHard, float>(){
                { EGameHard.Easy, config.NormalAttack3Cost },{ EGameHard.Normal, config.NormalAttack3Cost_1 },
                { EGameHard.Hard, config.NormalAttack3Cost_2 }, { EGameHard.Hardest, config.NormalAttack3Cost_3}
            };
            smashAtk1Datas = new Dictionary<EGameHard, float>(){
                { EGameHard.Easy, config.SmashAttackCost },{ EGameHard.Normal, config.SmashAttackCost_1 },
                { EGameHard.Hard, config.SmashAttackCost_2 }, { EGameHard.Hardest, config.SmashAttackCost_3}
            };
            smashAtk2Datas = new Dictionary<EGameHard, float>(){
                { EGameHard.Easy, config.SmashAttack2Cost },{ EGameHard.Normal, config.SmashAttack2Cost_1 },
                { EGameHard.Hard, config.SmashAttack2Cost_2 }, { EGameHard.Hardest, config.SmashAttack2Cost_3}
            };
            dashAtkDatas = new Dictionary<EGameHard, float>(){
                { EGameHard.Easy, config.DashAttackCost },{ EGameHard.Normal, config.DashAttackCost_1 },
                { EGameHard.Hard, config.DashAttackCost_2 }, { EGameHard.Hardest, config.DashAttackCost_3}
            };
            squatAtkDatas = new Dictionary<EGameHard, float>(){
                { EGameHard.Easy, config.SquatAtkCost },{ EGameHard.Normal, config.SquatAtkCost_1 },
                { EGameHard.Hard, config.SquatAtkCost_2 }, { EGameHard.Hardest, config.SquatAtkCost_3}
            };
            climbDatas = new Dictionary<EGameHard, float>(){
                { EGameHard.Easy, config.ClimbCostSpeed },{ EGameHard.Normal, config.ClimbCostSpeed_1 },
                { EGameHard.Hard, config.ClimbCostSpeed_2 }, { EGameHard.Hardest, config.ClimbCostSpeed_3}
            };
            jumpDatas = new Dictionary<EGameHard, float>(){
                { EGameHard.Easy, config.JumpCost },{ EGameHard.Normal, config.JumpCost_1 },
                { EGameHard.Hard, config.JumpCost_2 }, { EGameHard.Hardest, config.JumpCost_3}
            };
            runDatas = new Dictionary<EGameHard, float>(){
                { EGameHard.Easy, config.RunCostSpeed },{ EGameHard.Normal, config.RunCostSpeed_1 },
                { EGameHard.Hard, config.RunCostSpeed_2 }, { EGameHard.Hardest, config.RunCostSpeed_3}
            };
            idleDatas = new Dictionary<EGameHard, float>(){
                { EGameHard.Easy, config.StandIdleRecoverSpeed },{ EGameHard.Normal, config.StandIdleRecoverSpeed_1 },
                { EGameHard.Hard, config.StandIdleRecoverSpeed_2 }, { EGameHard.Hardest, config.StandIdleRecoverSpeed_3}
            };
            squatDatas = new Dictionary<EGameHard, float>(){
                { EGameHard.Easy, config.SquatIdleRecoverSpeed },{ EGameHard.Normal, config.SquatIdleRecoverSpeed_1 },
                { EGameHard.Hard, config.SquatIdleRecoverSpeed_2 }, { EGameHard.Hardest, config.SquatIdleRecoverSpeed_3}
            };
            sitDatas = new Dictionary<EGameHard, float>(){
                { EGameHard.Easy, config.SitIdleRecoverSpeed },{ EGameHard.Normal, config.SitIdleRecoverSpeed_1 },
                { EGameHard.Hard, config.SitIdleRecoverSpeed_2 }, { EGameHard.Hardest, config.SitIdleRecoverSpeed_3}
            };
        }

        // 获取某个动作状态消耗的体力值
        public float GetCostStamina(string stateName)
        {
            var sceneMgr = GameManager.GetGameSceneManager() as BaseGameSceneManager;
            if (sceneMgr.GetSceneObjIsPause())
            {
                return 0;
            }
            switch (stateName)
            {
                case "NorAtkState_1":
                    return GetStateCostStaminaByCostData(norAtk1Datas);
                case "NorAtkState_2":
                    return GetStateCostStaminaByCostData(norAtk2Datas);
                case "NorAtkState_3":
                    return GetStateCostStaminaByCostData(norAtk3Datas);
                case "SmashAtkState_1":
                    return GetStateCostStaminaByCostData(smashAtk1Datas);
                case "SmashAtkState_2":
                    return GetStateCostStaminaByCostData(smashAtk2Datas);
                case "DashAtkState":
                    return GetStateCostStaminaByCostData(dashAtkDatas);
                case "SquatAtkState":
                    return GetStateCostStaminaByCostData(squatAtkDatas);
                case "ClimbMoveState":
                    return GetStateCostStaminaByCostData(climbDatas);
                case "JumpState":
                    return GetStateCostStaminaByCostData(jumpDatas);
                case "RunState":
                    return GetStateCostStaminaByCostData(runDatas);
                case "IdleState":
                    return GetStateCostStaminaByCostData(idleDatas);
                case "SquatState":
                    return GetStateCostStaminaByCostData(squatDatas);
                case "SitState":
                    return GetStateCostStaminaByCostData(sitDatas);
                default:
                    return 0;
            }
        }

        private float GetStateCostStaminaByCostData(Dictionary<EGameHard, float> costDatas)
        {
            var hardCompont = GameManager.GetGMComponent<HardComponentGM>();
            var gameHard = hardCompont.Hard;
            if (costDatas.ContainsKey(gameHard))
            {
                return costDatas[gameHard];
            }
            else { return 0; }
        }
    }
}

