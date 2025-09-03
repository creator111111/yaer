using System;
using Game.GameRuntime.Entities.Component.Interactive;
using Game.GameRuntime.Entities.Player;
using GameFramework.UnityRuntimeExtend.Component;
using UnityEngine;

namespace Game.GameRuntime.Entities.Component.Map
{
    public class MapDoor : MonoBehaviour
    {
        private bool isEnter; // 只能触发一次
        public Action onEnterDoor;
        [SerializeField] private ComponentSystemMono componentSystemMono;

        private void Awake()
        {
            componentSystemMono = GetComponent<ComponentSystemMono>();

            if (componentSystemMono)
            {
                componentSystemMono.OnInit();
            
                componentSystemMono.GetComponent<InteractiveComponent>().onEnterInteractiveEvent += component =>
                {
                    // 只有玩家才能触发
                    if (component.Entity?.Logic is PlayerLogic)
                    {
                        if (isEnter) return;
                        isEnter = true;
                        onEnterDoor?.Invoke();
                    }
                };
            }
        }
    }
}