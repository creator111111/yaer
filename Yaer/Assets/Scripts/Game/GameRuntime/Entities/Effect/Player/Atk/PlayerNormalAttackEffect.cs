using Game.GameRuntime.Entities.Component.Effect;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.GameRuntime.Entities.Effect.Player.Atk
{
    public class PlayerNormalAttackEffect : AnimaEffectComponent
    {
        protected override void Find()
        {
            base.Find();
            sr = transform.Find("Animation").GetComponent<SpriteRenderer>();
        }


        public override void Play(int times)
        {
            base.Play(times);
            if (animator == null && !animator.enabled && animator.runtimeAnimatorController == null) { return; }
            animator = gameObject.GetComponent<Animator>();// 偶尔出现animator错误的情况，这里动态绑定一次
            animator.SetTrigger("Trigger");
            animator.SetInteger("Index", Random.Range(0, 4));
        }
    }
}