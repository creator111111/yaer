using Game.GameRuntime.Entities.Player.Components.CsAnimator.Base;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.GameRuntime.Entities.Player.Components.CsAnimator.Combat.State.Sit
{
    public class SitDownState : BasePlayerState
    {
        public override void Enter()
        {
            base.Enter();
            moveComponent.TurnRight();
        }

        public override void Update()
        {
            base.Update();
            FinishedChangeState<SitIdleStartState>();
        }
    }
}

