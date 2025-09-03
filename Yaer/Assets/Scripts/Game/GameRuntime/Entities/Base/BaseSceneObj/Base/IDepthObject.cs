using Game.GameRuntime.Entities.Component.Physics;

namespace Game.GameRuntime.Entities.Base.BaseSceneObj.Base
{
    public interface IDepthObject : IMonoObject
    {
        IDepthComponent DepthComponent { get; }
        bool IsInSameDepth(float y, float width = 0);
        bool IsInSameDepth(IDepthObject other, float multiple = 0);
    }
}