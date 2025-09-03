using System;
using System.Collections.Generic;
using Game.GameMgr.Component;
using Game.GameMgr.Manager.Res.SceneRes.Config;
using Game.Static.Name.Res;
using GameFramework.UnityRuntimeExtend.Resource;
using UnityEngine;

namespace Game.GameMgr.Manager.Res.SceneRes
{
    public class GameSceneResManager : MonoBehaviour, IGameSceneResManager
    {
        [SerializeField] private List<BaseSceneResConfig> sceneResConfigsList = new List<BaseSceneResConfig>();

        private ResComponentGM manager;
        private readonly Dictionary<string, BaseSceneResConfig> sceneResConfigsDic = new Dictionary<string, BaseSceneResConfig>();

        public void Init(IGameResourcesManager manager = null)
        {
            this.manager = (ResComponentGM)manager;

            foreach (var config in sceneResConfigsList)
            {
                RegisterConfig(config.name, config);
            }

            // RegisterConfig(SceneName.InitScene, ScriptableObject.CreateInstance<InitSceneResConfig>());
            // RegisterConfig(SceneName.StartScene, ScriptableObject.CreateInstance<StartSceneResConfig>());
            // RegisterConfig(SceneName.NewGameScene, ScriptableObject.CreateInstance<NewGameSceneResConfig>());
            // RegisterConfig(SceneName.HomeScene1, ScriptableObject.CreateInstance<HomeScene1ResConfig>());
            // RegisterConfig(SceneName.HomeScene2, ScriptableObject.CreateInstance<HomeScene2ResConfig>());
            // RegisterConfig(SceneName.SelectClothesScene, ScriptableObject.CreateInstance<SelectClothesSceneResConfig>());
            // RegisterConfig(SceneName.ForestScene, ScriptableObject.CreateInstance<ForestSceneResConfig>());
            // RegisterConfig(SceneName.ForestEastScene, ScriptableObject.CreateInstance<ForestEastSceneResConfig>());
            // RegisterConfig(SceneName.VerdantCorridor, ScriptableObject.CreateInstance<VerdantCorridorSceneResConfig>());
            // RegisterConfig(SceneName.WestRappRoad, ScriptableObject.CreateInstance<WestRappRoadSceneResConfig>());
        }

        public T GetSubManager<T>() where T : class, IGameResourcesSubManager
        {
            return null;
        }

        public void PreloadSceneRes(IGamePreloadHandler handler, string sceneName, Action<bool> onComplete)
        {
            onComplete += b => handler.Done();
            PreloadSceneRes(sceneName, onComplete);
        }

        public void ReleaseSceneRes(IGamePreloadHandler handler, string sceneName, Action<bool> onComplete)
        {
            onComplete += b => handler.Done();
            ReleaseSceneRes(sceneName, onComplete);
        }

        public void PreloadSceneRes(string sceneName, Action<bool> onComplete)
        {
            if (sceneResConfigsDic.ContainsKey(sceneName) == false)
            {
                Debug.LogWarning($"PreloadRes Error: sceneName {sceneName} is not exist");
                return;
            }

            sceneResConfigsDic[sceneName].Preload(onComplete);
        }

        public void PreloadResources(Action<bool> onComplete, params PreloadAssetInfo[] resArray)
        {
/*            manager.AddressableSystem.PreloadMultSingleAssetsAsync(success =>
            {
                if (!success) Debug.LogError("资源预加载失败！");
                onComplete?.Invoke(success);
            }, resArray);*/
        }

        public void ReleaseSceneRes(string sceneName, Action<bool> onComplete)
        {
#if UNITY_EDITOR
            if (sceneName == SceneName.Editor)
            {
                onComplete.Invoke(true);
                return;
            }
#endif

            if (sceneName == SceneName.Archive)
            {
                onComplete.Invoke(true);
                return;
            }

            if (sceneResConfigsDic.ContainsKey(sceneName) == false)
            {
                Debug.LogWarning($"ReleaseRes Error: sceneName {sceneName} is not exist");
                return;
            }

            sceneResConfigsDic[sceneName].Release(onComplete);
        }

        private void RegisterConfig(string sceneName, BaseSceneResConfig config)
        {
            sceneResConfigsDic[sceneName] = config;
            config.SetGameSceneResManager(this);
        }
    }
}