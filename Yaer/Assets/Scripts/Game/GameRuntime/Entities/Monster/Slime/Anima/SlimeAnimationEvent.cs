using System;
using UnityEngine;

namespace Game.GameRuntime.Entities.Monster.Slime.Anima
{
    public class SlimeAnimationEvent : MonoBehaviour
    {
        public Action onAttack;
        public Action onJumpAttack;

        public void AnimationEventAttack1()
        {
            onAttack?.Invoke();
        }

        public void AnimaEJumpAttack()
        {
            onJumpAttack?.Invoke();
        }
    }
}