using System;

namespace Game.GameMgr.Manager.Res
{
    public interface IGamePreloadHandler
    {
        Action OnComplete { get; set; }
        void StartLoad();
        void AddLoadingAction(Action<IGamePreloadHandler> action);
        void AddLastAction(Action<IGamePreloadHandler> action);
        void Done();
    }
}