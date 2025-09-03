using Cysharp.Threading.Tasks;
using Game.GameMgr.Component;
using Game.GameRuntime.Entities.Base.BaseSceneObj;
using Game.GameRuntime.Entities.Component.Interactive;
using Game.GameRuntime.Entities.Player;
using UnityEngine;

namespace Game.GameRuntime.Entities.SceneEntities
{
    public class SoundSwitchAreaComponent : BaseSceneEntityLogic
    {
        [SerializeField]
        private SoundToggleComponent ExitSoundToggle;
        [SerializeField]
        private SoundToggleComponent EnterSoundToggle;

        private InteractiveComponent interactiveComponent;

        protected internal override void OnInit(object userData)
        {
            base.OnInit(userData);
            interactiveComponent = componentSystem.GetComponent<InteractiveComponent>();

            interactiveComponent.onEnterInteractiveEvent += OnPlayerEnter;
            interactiveComponent.onExitInteractiveEvent += OnPlayerExit;
        }

        private void OnPlayerEnter(InteractiveComponent component)
        {
            if (component.Entity?.Logic is PlayerLogic)
            {
                SoundTransition(EnterSoundToggle, ExitSoundToggle).Forget();
            }
        }

        private void OnPlayerExit(InteractiveComponent component)
        {
            if (component.Entity?.Logic is PlayerLogic)
            {
                SoundTransition(ExitSoundToggle, EnterSoundToggle).Forget();
            }
        }

        private async UniTask SoundTransition(SoundToggleComponent playSound, SoundToggleComponent stopSound)
        {
            if (stopSound != null) { stopSound.enabled = false; }
            await UniTask.WaitForSeconds(0.7f);
            if (playSound != null) { playSound.enabled = true; }
        }
    }
}

