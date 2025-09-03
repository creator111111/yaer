using Game.GameMgr.Manager.Base;
using Game.GameRuntime.Entities.Component.Effect;
using Game.Static.Enum;

namespace Game.GameMgr.Manager.Effect
{
    public interface IEffectManager : IManager
    {
        EResLoadType DefaultLoadType { get; }
        T CreateEffect<T>(EResLoadType type, params string[] keys) where T : IEffectComponent;
        T CreateEffect<T>(params string[] keys) where T : IEffectComponent;
    }
}