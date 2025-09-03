using Game.GameRuntime.Entities.Monster.Slime;
using Game.GameRuntime.Entities.Monster.WoodWorm;
using UnityEngine;
using static Game.Static.Name.Clothes.ClothesName;

namespace Game.GameRuntime.Entities.Monster.TenWan.Components.Anima.State
{
    public class TenWanDeadState : BaseTenWanState
    {
        public override void Enter()
        {
            base.Enter();
            tenWanLogic.isProtect = true;
            var bodyRg = tenWanLogic.GetComponent<Rigidbody2D>();
            if (bodyRg != null)
            {
                bodyRg.constraints = RigidbodyConstraints2D.FreezeAll;
                bodyRg.isKinematic = true;// 设置为不能被碰撞
            }
        }
    }
}