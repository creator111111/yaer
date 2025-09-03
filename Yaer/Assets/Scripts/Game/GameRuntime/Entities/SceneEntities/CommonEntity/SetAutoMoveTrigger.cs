using Game.GameRuntime.Entities.Base.BaseSceneObj;
using Game.GameRuntime.Entities.Component.Interactive;
using Game.GameRuntime.Entities.Player;
using Game.GameRuntime.Entities.Player.Components;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.GameRuntime.Entities.SceneEntities
{
    public class SetAutoMoveTrigger : BaseSceneEntityLogic
    {
        [SerializeField]
        private PlayerInputComponent.AutoInputMove AutoInputMove;

        private InteractiveComponent interactiveComponent;
        protected internal override void OnInit(object userData)
        {
            base.OnInit(userData);
            interactiveComponent = componentSystem.GetComponent<InteractiveComponent>();
            interactiveComponent.onEnterInteractiveEvent += SetAutoMove;
        }

        private void SetAutoMove(InteractiveComponent component)
        {
            if (component.Entity?.Logic is PlayerLogic)
            {
                var player = component.Entity?.Logic as PlayerLogic;
                var playerInputComponent = player.componentSystem.GetComponent<PlayerInputComponent>();
                playerInputComponent.AutoMoveState = AutoInputMove;
            }
        }
    }
}

