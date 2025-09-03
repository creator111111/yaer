using UnityEngine;

namespace Game.GameRuntime.Entities.Component
{
    [CreateAssetMenu(fileName = "PlayerStaminaConfig", menuName = "ScriptableObjects/PlayerStaminaConfig")]
    public class PlayerStaminaConfig : ScriptableObject
    {
        [Header("ÏûºÄ")]
        public float RunCostSpeed;
        public float RunCostSpeed_1;
        public float RunCostSpeed_2;
        public float RunCostSpeed_3;
        public float ClimbCostSpeed;
        public float ClimbCostSpeed_1;
        public float ClimbCostSpeed_2;
        public float ClimbCostSpeed_3;

        public float JumpCost;
        public float JumpCost_1;
        public float JumpCost_2;
        public float JumpCost_3;
        public float NormalAttackCost;
        public float NormalAttackCost_1;
        public float NormalAttackCost_2;
        public float NormalAttackCost_3;
        public float NormalAttack2Cost;
        public float NormalAttack2Cost_1;
        public float NormalAttack2Cost_2;
        public float NormalAttack2Cost_3;
        public float NormalAttack3Cost;
        public float NormalAttack3Cost_1;
        public float NormalAttack3Cost_2;
        public float NormalAttack3Cost_3;
        public float SmashAttackCost;
        public float SmashAttackCost_1;
        public float SmashAttackCost_2;
        public float SmashAttackCost_3;
        public float SmashAttack2Cost;
        public float SmashAttack2Cost_1;
        public float SmashAttack2Cost_2;
        public float SmashAttack2Cost_3;
        public float DashAttackCost;
        public float DashAttackCost_1;
        public float DashAttackCost_2;
        public float DashAttackCost_3;
        public float SquatAtkCost;
        public float SquatAtkCost_1;
        public float SquatAtkCost_2;
        public float SquatAtkCost_3;

        [Header("»Ö¸´")]
        public float StandIdleRecoverSpeed;
        public float StandIdleRecoverSpeed_1;
        public float StandIdleRecoverSpeed_2;
        public float StandIdleRecoverSpeed_3;
        public float SquatIdleRecoverSpeed;
        public float SquatIdleRecoverSpeed_1;
        public float SquatIdleRecoverSpeed_2;
        public float SquatIdleRecoverSpeed_3;
        public float SitIdleRecoverSpeed;
        public float SitIdleRecoverSpeed_1;
        public float SitIdleRecoverSpeed_2;
        public float SitIdleRecoverSpeed_3;
    }
}

