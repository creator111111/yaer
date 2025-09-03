using System.Collections.Generic;
using UnityEngine;

namespace Game.GameRuntime.Entities.Component.Physics
{
    public interface IPhysics2DComponent
    {
        bool XAxisIsTrigger { get; set; }
        bool YAxisIsTrigger { get; set; }
        Vector2 Velocity { get; }
        Vector2 ClsDir { get; }
        void Init(Collider2D c);
        void SetVelocity(Vector2 v);
        ICollection<Collider2D> GetCollisionObjs();
    }
}