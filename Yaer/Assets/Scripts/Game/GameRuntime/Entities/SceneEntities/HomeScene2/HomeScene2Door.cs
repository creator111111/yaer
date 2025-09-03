using Game.GameMgr.Component;
using Game.GameRuntime.Entities.Component.Map;
using UnityEngine;

namespace Game.GameRuntime.Entities.SceneEntities.HomeScene2
{
    public class HomeScene2Door : SceneChangeDoor
    {
        private Animator animator;

        public SoundToggleComponent soundSfxCpn;
        protected internal override void OnInit(object userData)
        {
            base.OnInit(userData);

            animator = GetComponent<Animator>();
        }
        
        public void OpenEnd()
        {
            animator.SetBool("Open", false);
        }

        protected override void OnEnterSuccess()
        {
            animator.SetBool("Open", true);
            soundSfxCpn.PlaySound();
        }
    }
}