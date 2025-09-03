using System;
using UnityEngine;

namespace Game.GameRuntime.Entities.Player
{
    public class PlayerAnimationEvent : MonoBehaviour
    {
        public Action onJumpHeight;
        public Action<Vector2> onMovePosition;
        public Action onNormalAttack1;
        public Action onNormalAttack2;
        public Action onNormalAttack3;
        public Action onSwordFall1;
        public Action onSwordFall2;
        public Action<string> onAttack;

        private void AnimaEventNormalAttack1()
        {
            onNormalAttack1?.Invoke();
        }

        private void AnimaEventNormalAttack2()
        {
            onNormalAttack2?.Invoke();
        }

        private void AnimaEventNormalAttack3()
        {
            onNormalAttack3?.Invoke();
        }

        private void AnimaEventAttack(string attackName)
        {
            onAttack?.Invoke(attackName);
        }

        private void AnimaEventJumpHeight()
        {
            onJumpHeight?.Invoke();
        }

        private void AnimaEventMovePosition(string v)
        {
            var list = v.Split(',');
            onMovePosition?.Invoke(new Vector2(float.Parse(list[0]), float.Parse(list[1])));
        }

        private void AnimaEventSwordFall1()
        {
            onSwordFall1?.Invoke();
        }

        private void AnimaEventSwordFall2()
        {
            onSwordFall2?.Invoke();
        }
    }
}