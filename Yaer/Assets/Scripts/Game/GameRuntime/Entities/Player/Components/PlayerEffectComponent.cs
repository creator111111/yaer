using Game.GameMgr;
using Game.GameRuntime.Entities.Component.Effect;
using Game.GameRuntime.Entities.Effect.Player;
using Game.GameRuntime.Entities.Effect.Player.Dust;
using Game.GameRuntime.GameSceneManager.Base;
using GameFramework.UnityRuntimeExtend.Component;
using UnityEngine;

namespace Game.GameRuntime.Entities.Player.Components
{
    public class PlayerEffectComponent: BaseGFComponentMono, IPlayerComponent
    {
        public PlayerLogic PlayerLogic { get; set; }
        
        [SerializeField] private EffectPlayerComponent turnDirDustPlayer; // 跑步的尘土右方向
        [SerializeField] private EffectPlayerComponent jumpUpDustPlayer;
        [SerializeField] private EffectPlayerComponent jumpDownDustPlayer;

        [SerializeField]
        private EffectPlayerComponent dashAttackDust1Player;
        [SerializeField]
        private EffectPlayerComponent dashAttackDust2Player;
        [SerializeField]
        private EffectPlayerComponent fallGroundDustPlayer;
        [SerializeField]
        private EffectPlayerComponent flyHitClsWavePlayer;
        [SerializeField]
        private EffectPlayerComponent fallGroundWavePlayer;

        [SerializeField]
        private GameObject SitIdleDreamCake;
        [SerializeField]
        private GameObject SitIdleDreamXiaer;

        private PlayerMoveComponent moveComponent => PlayerLogic.componentSystem.GetComponent<PlayerMoveComponent>();

        protected override void OnInit()
        {
            var playerMove = PlayerLogic.componentSystem.GetComponent<PlayerMoveComponent>();
            playerMove.OnJumpUp += CreateJumpUpDust;
            playerMove.OnJumpDown += CreateJumpDownDust;
            playerMove.onTurnAction += CreateTurnDust;

            PlayerLogic.DashAttackDust1 += () => dashAttackDust1Player.PlayEffect<DashAttackDust1Effect>(1, SetRotation);
            PlayerLogic.DashAttackDust2 += () => dashAttackDust2Player.PlayEffect<DashAttackDust2Effect>(1, SetRotation);
            PlayerLogic.FallGroundEvent += () => fallGroundDustPlayer.PlayEffect<FallGroundDustEffect>(1, SetRotation);
            PlayerLogic.FallGroundEvent += () => fallGroundWavePlayer.PlayEffect<PlayerFallGroundWaveEffect>(1, SetRotation);
            PlayerLogic.OnFlyHitClsEvent += () => flyHitClsWavePlayer.PlayEffect<PlayerFlyHitClsWaveEffect>(1, SetRotation);
            PlayerLogic.OnEnterSitIdleEvent += () => PlaySitIdleDreamAnimation(true);
            PlayerLogic.OnExitSitIdleEvent += () => PlaySitIdleDreamAnimation(false);
        }

        private void SetRotation(AnimaEffectComponent effect)
        {
            if (moveComponent.Direction == Component.Move.EDirectionType.Right)
            {
                effect.transform.rotation = Quaternion.AngleAxis(0, Vector3.up);
            }
            else
            {
                effect.transform.rotation = Quaternion.AngleAxis(180, Vector3.up);
            }
        }

        private void CreateTurnDust(Vector2 dirV2)
        {
            if ((GameManager.GetGameSceneManager() as BaseGameSceneManager).Config.isFightingScene)
                turnDirDustPlayer.PlayEffect<ChangeDirDustEffect>(1, SetRotation);
        }
        
        private void CreateJumpUpDust()
        {
            jumpUpDustPlayer.PlayEffect<JumpUpDustEffect>(1, SetRotation);
        }
        
        private void CreateJumpDownDust()
        {
            jumpDownDustPlayer.PlayEffect<JumpDownDustEffect>(1, SetRotation);
        }

        public void PlaySitIdleDreamAnimation(bool f)
        {
            SitIdleDreamCake.SetActive(false);
            SitIdleDreamXiaer.SetActive(false);
            if (f)
            {
                float hpPercent = PlayerLogic.healthComponent.hp / PlayerLogic.healthComponent.maxHp;
                if (hpPercent > 0.25)
                {
                    SitIdleDreamCake.SetActive(f);
                }
                else
                {
                    SitIdleDreamXiaer.SetActive(f);
                }
            }
        }
    }
}