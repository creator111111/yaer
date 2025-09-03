using System;
using Game.GameRuntime.Entities.Base.BaseSceneObj.Base;
using UnityEngine;

namespace Game.GameRuntime.Entities.Player.Goods.Sword
{
    public class Sword : MonoBehaviour, IStateObject
    {
        [SerializeField] private Animator at;
        private SwordCsAnimator csAnimator;

        private void Awake()
        {
            at = GetComponent<Animator>();
            // csAnimator = new SwordCsAnimator(this, at);
            // csAnimator.RegisterRuntimeController(new SwordCsRuntimeControl(new SwordSM(this, at, "Main", null)), at.runtimeAnimatorController);

            onTriggerFallAnima1 += TriggerFallAnima1;
            onTriggerFallAnima2 += TriggerFallAnima2;

            // csAnimator.ChangeRuntimeController<SwordCsRuntimeControl>();
        }

        private void Update()
        {
            csAnimator?.OnUpdate();
        }

        private void OnDestroy()
        {
            onTriggerFallAnima1 -= TriggerFallAnima1;
            onTriggerFallAnima2 -= TriggerFallAnima2;
        }

        private void OnValidate()
        {
            at = GetComponent<Animator>();
        }

        public Animator Animator => at;
        public Transform Transform => transform;

        public event Action onTriggerFallAnima1;
        public event Action onTriggerFallAnima2;

        private void TriggerFallAnima1()
        {
            at.SetTrigger("Fall1");
        }

        private void TriggerFallAnima2()
        {
            at.SetTrigger("Fall2");
        }
    }
}