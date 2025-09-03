using System;
using GameFramework.UnityRuntimeExtend.Resource;

namespace Game.GameMgr.Manager.Res.SceneRes
{
    public interface IGameSceneResManager : IGameResourcesSubManager
    {
        T GetSubManager<T>() where T : class, IGameResourcesSubManager;
        void PreloadSceneRes(IGamePreloadHandler handler, string sceneName, Action<bool> onComplete = null);
        void ReleaseSceneRes(IGamePreloadHandler handler, string sceneName, Action<bool> onComplete = null);
        void PreloadSceneRes(string sceneName, Action<bool> onComplete);
        void ReleaseSceneRes(string sceneName, Action<bool> onComplete);
        void PreloadResources(Action<bool> onComplete, params PreloadAssetInfo[] resArray);
    }
}