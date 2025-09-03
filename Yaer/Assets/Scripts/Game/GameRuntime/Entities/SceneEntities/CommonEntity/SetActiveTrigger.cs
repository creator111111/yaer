using Game.GameRuntime.Entities.Component.Interactive;
using Game.GameRuntime.Entities.Player;
using GameFramework.UnityRuntimeExtend.Component;
using UnityEngine;

namespace Game.GameRuntime.Entities.SceneEntities
{
    public class SetActiveTrigger : MonoBehaviour
    {
        [SerializeField]
        private GameObject TargetGo;
        [SerializeField]
        private bool active;

        private void Start()
        {
            var componentSystem = gameObject.GetComponent<ComponentSystemMono>();
            var interactiveComponent = componentSystem.GetComponent<InteractiveComponent>();
            interactiveComponent.onEnterInteractiveEvent += Do;
        }

        private void Do(InteractiveComponent component)
        {
            if (component.Entity?.Logic is PlayerLogic)
            {
                TargetGo.SetActive(active);
            }
        }
    }
}

