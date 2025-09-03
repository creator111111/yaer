using Game.GameRuntime.Entities.Component.Anima;
using Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat;
using Game.GameRuntime.Entities.Player.Components.CsAnimator.Home;
using UnityEngine;

namespace Game.GameRuntime.Entities.Player.Components.CsAnimator
{
    public class PlayerCsAnimator : BaseCsAnimator, IPlayerCsAnimator, IPlayerComponent
    {
        public Transform animationRoot;
        public PlayerLogic PlayerLogic { get; set; }

        public bool IsNormalAttacking { get; set; }

        protected override void OnInit()
        {
            base.OnInit();

            RegisterRuntimeController<PlayerHomeCsRuntimeController>();
            RegisterRuntimeController<PlayerCombatCsRuntimeController>();
        }

        public void SetAnimationTsf(Vector3 pos)
        {
            if (animationRoot == null)
            {
                Debug.LogError("animationRoot未绑定");
                return;
            }

            animationRoot.position = pos;
        }

        public Vector2 GetAnimationPos()
        {
            return animationRoot.position;
        }

    }
}