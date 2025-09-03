using Game.GameMgr.Component.Archive;
using Game.GameMgr;
using Game.GameRuntime.Entities.Component.Anima.interf;
using Game.GameRuntime.Entities.Player.Components.CsAnimator.Base;
using Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat.State.Squat.Climb;
using Game.GameMgr.Component.Archive.ArchiveDataClass.Player;

namespace Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat.State.Squat
{
    public class SquatSM : BasePlayerSM
    {
        public override void Init(ICsAnimator csAnimator, string name, string enterArgs, IStateMachine parent = null)
        {
            base.Init(csAnimator, name, enterArgs, parent);

            RegisterState<SquatDamageState>("Squat_Damage", "SquatDamage");
            RegisterState<SquatDeadState>("Squat_Dead", "SquatDead");
            RegisterState<SquatAtkState>("Squat_Atk", "SquatAtk");
            RegisterState<SquatDownState>("Squat_Down", "SquatDown");
            RegisterState<SquatStay1State>("Squat_Stay1", "SquatStay1");
            RegisterState<SquatStay2State>("Squat_Stay2", "SquatStay2");
            RegisterState<SquatUpState>("Squat_Up", "SquatUp");

            RegisterSubStateMachine<ClimbSM>("Squat_ClimbSub", "Squat_ClimbSub");
        }

        public override void Enter()
        {
            base.Enter();
            var playerSceneData = GameManager.GetGameSceneManager().GetArchiveData<PlayerSceneData>();
            playerSceneData.playerState = PlayerStateSign.Squat;
            SetSign(PlayerStateSign.Squat, true);
        }

        public override void Exit()
        {
            base.Exit();
            var playerSceneData = GameManager.GetGameSceneManager().GetArchiveData<PlayerSceneData>();
            playerSceneData.playerState = PlayerStateSign.Idle;
            SetSign(PlayerStateSign.Squat, false);
        }
    }
}