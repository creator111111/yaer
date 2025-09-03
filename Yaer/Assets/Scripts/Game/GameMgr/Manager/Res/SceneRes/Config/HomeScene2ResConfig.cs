using System;
using System.Collections.Generic;
using Game.GameMgr.Manager.Res.SceneRes.Config.Generic.Package;
using GameFramework.UnityRuntimeExtend.Resource;
using UnityEngine;

namespace Game.GameMgr.Manager.Res.SceneRes.Config
{
    [CreateAssetMenu(menuName = "SceneResConfig/HomeScene2")]
    public class HomeScene2ResConfig : BaseSceneResConfig
    {


        public override void Preload(Action<bool> onComplete)
        {
            List<PreloadAssetInfo> package = new List<PreloadAssetInfo>();
            package.AddRange(AtlasResPackage.Package);
            package.AddRange(PlayerAnimaResPackage.Package);

            sceneResManager.PreloadResources(onComplete, package.ToArray());
        }

        public override void Release(Action<bool> onComplete)
        {
            onComplete?.Invoke(true);
        }
    }
}