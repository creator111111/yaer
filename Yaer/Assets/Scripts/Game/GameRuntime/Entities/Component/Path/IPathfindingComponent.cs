using UnityEngine;

namespace Game.GameRuntime.Entities.Component.Path
{
    public interface IPathfindingComponent
    {
        void Init(Rigidbody2D rg, float speed);
        void StartUp();
        void ShutDown();
    }
}