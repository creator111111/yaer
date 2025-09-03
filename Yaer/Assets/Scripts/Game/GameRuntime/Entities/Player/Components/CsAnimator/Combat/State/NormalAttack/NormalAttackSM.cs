using Game.GameRuntime.Entities.Component.Anima.interf;
using Game.GameRuntime.Entities.Player.Components.CsAnimator.Base;

namespace Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat.State.NormalAttack
{
    public class NormalAttackSM : BasePlayerSM
    {
        private  PlayerLogic playerLogic;

        public override void Init(ICsAnimator csAnimator, string name, string enterArgs, IStateMachine parent)
        {
            base.Init(csAnimator, name, enterArgs, parent);

            playerLogic = GetEntityLogic<PlayerLogic>();
            RegisterState<NormalAttackPart1>("NormalAttack_Part1", "NormalAttack_Part1");
            RegisterState<NormalAttackPart1End>("NormalAttack_Part1End", "NormalAttack_Part1End");
            RegisterState<NormalAttackPart2>("NormalAttack_Part2", "NormalAttack_Part2");
            RegisterState<NormalAttackPart2End>("NormalAttack_Part2End", "NormalAttack_Part2End");
            RegisterState<NormalAttackPart3>("NormalAttack_Part3", "NormalAttack_Part3");
        }

        public override void Enter()
        {
            base.Enter();
            
            SetSign("IsNormalAttacking", true);
            // playerLogic.SceneManager.SetCinemachineFollow(playerLogic.transform);
        }

        public override void Exit()
        {
            base.Exit();

            SetSign("IsNormalAttacking", false);
            // playerLogic.SceneManager.SetCinemachineFollow(playerLogic.transform);
        }
    }
}