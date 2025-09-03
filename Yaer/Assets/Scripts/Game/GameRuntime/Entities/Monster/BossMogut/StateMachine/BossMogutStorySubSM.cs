using Game.GameRuntime.Entities.Component.Anima;
using Game.GameRuntime.Entities.Component.Anima.interf;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.GameRuntime.Entities.Monster.BossMogut
{
    public class BossMogutStorySubSM : BaseStateMachine
    {
        public override void Init(ICsAnimator csAnimator, string name, string enterArgs, IStateMachine parent)
        {
            base.Init(csAnimator, name, enterArgs, parent);
        }
    }
}


