using Game.GameRuntime.Entities.Component.Effect;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.GameRuntime.Entities.Effect.Player.BeHurt
{
    public class PlayerBeHurtEffect : AnimaEffectComponent
    {
        protected override void Find()
        {
            base.Find();
            sr = transform.Find("Animation").GetComponent<SpriteRenderer>();
        }


        public override void Play(int times)
        {
            base.Play(times);
            if (animator == null && !animator.enabled && animator.runtimeAnimatorController == null) {
                Debug.LogError("===================动画出问题了，检查下");
                return; 
            }
            animator = gameObject.GetComponent<Animator>();// 偶尔出现animator错误的情况，这里动态绑定一次
            animator.SetTrigger("Trigger");
            var index = Random.Range(1, 4);
            index = 1;
            animator.SetInteger("Index", index);
        }
    }
}