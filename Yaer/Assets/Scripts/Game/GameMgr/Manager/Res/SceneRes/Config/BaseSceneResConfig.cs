using System;
using System.Collections.Generic;
using GameFramework.UnityRuntimeExtend.Resource;
using UnityEngine;
using UnityEngine.U2D;

namespace Game.GameMgr.Manager.Res.SceneRes.Config
{
    public enum AssetType
    {
        GameObject,
        SpriteAtlas,
        AnimationRuntimeController
    }

    [Serializable]
    public class SceneResConfigAssetInfo
    {
        public AssetType assetType;
        public string path;

        public Type GetAssetType()
        {
            switch (assetType)
            {
                case AssetType.GameObject:
                    return typeof(GameObject);
                case AssetType.SpriteAtlas:
                    return typeof(SpriteAtlas);
                case AssetType.AnimationRuntimeController:
                    return typeof(RuntimeAnimatorController);
                default:
                    return null;
            }
        }
    }

    public abstract class BaseSceneResConfig : ScriptableObject
    {
        [SerializeField] private string sceneName;
        [SerializeField] protected List<SceneResConfigAssetInfo> assetInfos = new List<SceneResConfigAssetInfo>();
        private readonly List<PreloadAssetInfo> package = new List<PreloadAssetInfo>();
        protected GameSceneResManager sceneResManager;
        
        protected event Action<bool> onComplete;
        protected event Action<string> onRelease;

        public void SetGameSceneResManager(IGameSceneResManager manager) => sceneResManager = manager as GameSceneResManager;

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                sceneName = GetType().Name;
            }
        }

        public virtual void Preload(Action<bool> onComplete)
        {
            onComplete += b => this.onComplete?.Invoke(b);
            sceneResManager.PreloadResources(onComplete, package.ToArray());
        }

        public virtual void Release(Action<bool> onComplete)
        {
            onComplete += b => this.onComplete?.Invoke(b);
            onComplete?.Invoke(true);
        }

        protected void Load(params PreloadAssetInfo[] infos)
        {
            package.AddRange(infos);
        }

        protected void ReleasePackage(string name)
        {
        }

        protected void Release(params PreloadAssetInfo[] infos)
        {
        }
    }
}