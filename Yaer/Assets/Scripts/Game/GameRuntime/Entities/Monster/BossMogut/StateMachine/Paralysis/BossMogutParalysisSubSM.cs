using Game.GameRuntime.Entities.Component.Anima.interf;
using Game.GameRuntime.Entities.Component.Anima;

namespace Game.GameRuntime.Entities.Monster.BossMogut
{
    public class BossMogutParalysisSubSM : BaseStateMachine
    {
        private BossMogutLogic bossMogutLogic;

        public override void Init(ICsAnimator csAnimator, string name, string enterArgs, IStateMachine parent)
        {
            base.Init(csAnimator, name, enterArgs, parent);
            bossMogutLogic = csAnimator.GetEntityLogic<BossMogutLogic>();
            RegisterState<BossMogutParalysisDownState>("ParalysisDown", "ParalysisDown");
            RegisterState<BossMogutParalysisIdleState>("ParalysisIdle", "ParalysisIdle");
            RegisterState<BossMogutParalysisDefeatState>("ParalysisDefeat", "ParalysisDefeat");
            RegisterState<BossMogutParalysisUpState>("ParalysisUp", "ParalysisUp");
            RegisterState<BossMogutBrokenLegParalysisDownState>("BrokenLegParalysisDown", "BrokenLegParalysisDown");
            RegisterState<BossMogutBrokenLegParalysisIdleState>("BrokenLegParalysisIdle", "BrokenLegParalysisIdle");
            RegisterState<BossMogutBrokenLegParalysisDefeatState>("BrokenLegParalysisDefeat", "BrokenLegParalysisDefeat");
            RegisterState<BossMogutParalysisFaceBroken1State>("ParalysisFaceBroken1", "ParalysisFaceBroken1");
            RegisterState<BossMogutParalysisFaceBroken2State>("ParalysisFaceBroken2", "ParalysisFaceBroken2");
        }

        public override void Enter()
        {
            base.Enter();
            bossMogutLogic.IsParalysis = true;
            ShowInParalysisState(true);
        }

        public override void Exit()
        {
            base.Exit();
            bossMogutLogic.IsParalysis = false;
            ShowInParalysisState(false);
        }

        public void ShowInParalysisState(bool isInState)
        {
            // 怪物进入瘫痪倒地状态时，碰撞箱需要发生变化
            bossMogutLogic.bodyCld.gameObject.SetActive(isInState);
            bossMogutLogic.footCld.gameObject.SetActive(!isInState);
            bossMogutLogic.hitPointCld.gameObject.SetActive(!isInState);
            bossMogutLogic.faceCld.gameObject.SetActive(isInState);
        }
    }
}