using Game.GameRuntime.Entities.Monster.Slime;
using Game.GameRuntime.Entities.Monster.TenWan;
using UnityEngine;

namespace Game.GameRuntime.Entities.Monster.WoodWorm.Components.Anima.State
{
    public class WoodWormDeadState : BaseWoodWormState
    {
        public override void Enter()
        {
            base.Enter();
            //moveCpn.StopMove();
            woodWormLogic.isProtect = true;
            var bodyRg = woodWormLogic.GetComponent<Rigidbody2D>();
            if (bodyRg != null )
            {
                bodyRg.constraints = RigidbodyConstraints2D.FreezeAll;
                bodyRg.isKinematic = true;
            }
        }
    }
}