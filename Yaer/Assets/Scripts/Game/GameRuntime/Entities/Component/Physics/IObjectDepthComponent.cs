using Game.GameRuntime.Entities.Base.BaseSceneObj.Base;
using UnityEngine;

namespace Game.GameRuntime.Entities.Component.Physics
{
    public interface IDepthComponent
    {
        int LayerID { get; set; }

        float BoxUpY { get; }
        float BoxDownY { get; }
        Collider2D FootCld { get; }
        bool IsInSameDepth(float y, float width = 0);
        bool IsInSameDepth(IDepthObject other, float multiple = 0);
    }
}