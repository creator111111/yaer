using Game.GameRuntime.Entities.Component.Anima;
using Game.GameRuntime.Entities.Component.Anima.interf;

namespace Game.GameRuntime.Entities.Monster.BossMogut
{
    public class BossMogutSM : BaseStateMachine
    {
        protected BossMogutLogic bossMogutLogic;

        public override void Init(ICsAnimator csAnimator, string name, string enterArgs, IStateMachine parent)
        {
            base.Init(csAnimator, name, enterArgs, parent);
            bossMogutLogic = csAnimator.GetEntityLogic<BossMogutLogic>();

            RegisterState<BossMogutMoveState>("Move", "Move");
            RegisterState<BossMogutAttack1State>("Attack1", "Attack1");
            RegisterState<BossMogutAttack2State>("Attack2", "Attack2");
            RegisterState<BossMogutTrampleState>("Trample", "Trample");

            RegisterSubStateMachine<BossMogutStorySubSM>("StorySub", "StorySub");
            RegisterSubStateMachine<BossMogutParalysisSubSM>("ParalysisSub", "ParalysisSub");
        }
    }
}

