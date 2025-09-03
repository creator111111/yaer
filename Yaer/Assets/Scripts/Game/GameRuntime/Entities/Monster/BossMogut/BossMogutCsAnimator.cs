using Game.GameRuntime.Entities.Component.Anima;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.GameRuntime.Entities.Monster.BossMogut
{
    public class BossMogutCsAnimator : BaseCsAnimator
    {
        protected BossMogutLogic bossMogutLogic;

        protected override void OnInit()
        {
            base.OnInit();
            bossMogutLogic = GetEntityLogic<BossMogutLogic>();
            RegisterRuntimeController<BossMogutRuntimeController>(animator.runtimeAnimatorController);
            ChangeRuntimeController<BossMogutRuntimeController>();
        }
    }
}

