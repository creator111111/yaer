using UnityEngine;

namespace Game.GameRuntime.Entities.Base.BaseSceneObj.Base
{
    public interface IPathfindingTarget : IMonoObject
    {
        Vector2 GetPathfindingPos(Vector2 pos);
    }
}