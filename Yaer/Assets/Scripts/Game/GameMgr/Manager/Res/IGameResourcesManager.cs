using System;
using Game.GameMgr.Manager.Base;
using GameFramework.UnityRuntimeExtend.Resource;

namespace Game.GameMgr.Manager.Res
{
    public interface IGameResourcesManager : IManager
    {
        void InitSubManager();
        T GetSubManager<T>() where T : class, IGameResourcesSubManager;
        void PreloadGameGenericRes(IGamePreloadHandler handler);

        void PreloadResources(IGamePreloadHandler handler, Action<bool> onComplete = null,
            params PreloadAssetInfo[] preloadInfos);

        void LoadAsset<T>(string assetName, Action<T> onComplete);
    }
}