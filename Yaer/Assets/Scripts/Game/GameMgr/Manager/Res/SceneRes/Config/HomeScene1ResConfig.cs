using System;
using System.Collections.Generic;
using Game.GameMgr.Manager.Res.SceneRes.Config.Generic.Package;
using GameFramework.UnityRuntimeExtend.Resource;
using UnityEngine;

namespace Game.GameMgr.Manager.Res.SceneRes.Config
{
    [CreateAssetMenu(menuName = "SceneResConfig/HomeScene1")]
    public class HomeScene1ResConfig : BaseSceneResConfig
    {

        public override void Preload(Action<bool> onComplete)
        {
            List<PreloadAssetInfo> package = new List<PreloadAssetInfo>();
            package.AddRange(AtlasResPackage.Package);
            package.AddRange(PlayerAnimaResPackage.Package);
            package.Add(new PreloadAssetInfo<GameObject>("GoOutStoryPanel"));
            package.Add(new PreloadAssetInfo<GameObject>("MapPanel"));

            sceneResManager.PreloadResources(onComplete, package.ToArray());
        }

        public override void Release(Action<bool> onComplete)
        {
            onComplete?.Invoke(true);
        }
    }
}