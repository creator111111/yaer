using Game.GameRuntime.Entities.Component.Effect;
using GameFramework.UnityRuntimeExtend.Component;
using UnityEngine;
using Game.GameRuntime.Entities.Component.Move;
using Game.GameRuntime.Entities.Effect.BossMogut;

namespace Game.GameRuntime.Entities.Monster.BossMogut
{
    public class BossMogutEffectComponent : BaseGFComponentMono
    {
        [SerializeField]
        private BossMogutLogic bossMogutLogic;

        [SerializeField]
        private EffectPlayerComponent Attack1DustPlayer;
        [SerializeField]
        private EffectPlayerComponent Attack2DustPlayer;
        [SerializeField]
        private EffectPlayerComponent TrampleDustPlayer;

        private MoveComponent moveComponent => bossMogutLogic.componentSystem.GetComponent<MoveComponent>();

        protected override void OnInit()
        {
            bossMogutLogic.OnPerformAttack += OnPerformAttack;
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

        private void OnPerformAttack(string skillName)
        {
            if (skillName == "Attack1")
            {
                Attack1DustPlayer.PlayEffect<BossMogutAttackDustEffect>(1, SetRotation);
            }
            else if (skillName == "Attack2")
            {
                Attack2DustPlayer.PlayEffect<BossMogutAttackDustEffect>(1, SetRotation);
            }
            else if (skillName == "Trample")
            {
                TrampleDustPlayer.PlayEffect<BossMogutAttackDustEffect>(1, SetRotation);
            }
        }
    }
}