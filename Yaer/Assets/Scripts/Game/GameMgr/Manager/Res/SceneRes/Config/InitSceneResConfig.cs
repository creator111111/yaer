using System;
using GameFramework.UnityRuntimeExtend.Resource;
using UnityEngine;

namespace Game.GameMgr.Manager.Res.SceneRes.Config
{
    [CreateAssetMenu(menuName = "SceneResConfig/InitScene")]
    public class InitSceneResConfig : BaseSceneResConfig
    {
        public override void Preload(Action<bool> onComplete)
        {
            sceneResManager.PreloadResources(onComplete, new PreloadAssetInfo<GameObject>("BlackPanel"), new PreloadAssetInfo<GameObject>("LoadingPanel"));
        }

        public override void Release(Action<bool> onComplete)
        {
            onComplete?.Invoke(true);
        }
    }
}