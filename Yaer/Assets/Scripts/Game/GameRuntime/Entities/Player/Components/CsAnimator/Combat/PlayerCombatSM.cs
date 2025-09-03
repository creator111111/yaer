using Game.GameMgr;
using Game.GameMgr.Component.Archive.ArchiveDataClass.Player;
using Game.GameRuntime.Entities.Component.Anima.interf;
using Game.GameRuntime.Entities.Player.Components.CsAnimator.Base;
using Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat.State;
using Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat.State.Damage;
using Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat.State.Damage.DamageFly;
using Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat.State.Dead;
using Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat.State.Dead.FlyDead;
using Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat.State.Ground;
using Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat.State.Jump;
using Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat.State.NormalAttack;
using Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat.State.Sit;
using Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat.State.SmashAttack;
using Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat.State.Squat;
using Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat.State.Squat.Climb;

namespace Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat
{
    public class PlayerCombatSM : BasePlayerSM
    {
        bool hasChangePlayerState = false;
        public override void Init(ICsAnimator csAnimator, string name, string enterArgs, IStateMachine parent = null)
        {
            base.Init(csAnimator, name, enterArgs, parent);
            
            RegisterState<CombatIdleState>("Idle", "Idle");
            RegisterState<CombatRunState>("Run", "Run");
            RegisterState<Damage1State>("Damage1", "Damage1");
            RegisterState<Damage2State>("Damage2", "Damage2");
            RegisterState<Dead1State>("Dead1", "Dead1");
            RegisterState<Dead2State>("Dead2", "Dead2");
            RegisterState<DashAttackState>("DashAttack", "DashAttack");
            RegisterState<ClimbUpState>("Squat_Climb_Up", "ClimbUp");
            RegisterState<AttackBossMogutState>("AttackBossMogut", "AttackBossMogut");

            RegisterSubStateMachine<CombatJumpSM>("JumpSub", "JumpSub");
            RegisterSubStateMachine<NormalAttackSM>("NormalAttackSub", "NormalAttackSub");
            RegisterSubStateMachine<SmashAttackSubSM>("SmashAttackSub", "SmashAttackSub");
            RegisterSubStateMachine<SquatSM>("SquatSub", "SquatSub");
            RegisterSubStateMachine<DamageFlySM>("DamageFlySub", "DamageFlySub");
            RegisterSubStateMachine<FlyDeadSM>("FlyDeadSub", "FlyDeadSub");
            RegisterSubStateMachine<SitSubSM>("SitSub", "SitSub");
            

        }

        public override void Update()
        {
            base.Update();
            // 检测人物当前状态
            if (!hasChangePlayerState)
            {
                UpdatePlayerState();
            }
        }
        public override void Enter()
        {
            base.Enter();
            hasChangePlayerState = false;
        }

        void UpdatePlayerState()
        {
            var sceneMgr = GameManager.GetGameSceneManager();
            if (sceneMgr == null) { return; }
            hasChangePlayerState = true;
            var playerSceneData = GameManager.GetGameSceneManager().GetArchiveData<PlayerSceneData>();
            var playerState = playerSceneData.playerState;
            if (playerState == PlayerStateSign.Squat)
            {
                ChangeState<CombatIdleState>();
                EnterSubStateMachine<SquatSM>().ChangeState<SquatDownState>();
            }
            else
            {
                ChangeState<CombatIdleState>();
            }
        }
    }
}