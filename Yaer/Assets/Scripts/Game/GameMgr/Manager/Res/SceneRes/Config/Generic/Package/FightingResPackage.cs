using GameFramework.UnityRuntimeExtend.Resource;
using UnityEngine;

namespace Game.GameMgr.Manager.Res.SceneRes.Config.Generic.Package
{
    public class FightingResPackage
    {
        public static PreloadAssetInfo[] Package => new PreloadAssetInfo[]
        {
            new PreloadAssetInfo<GameObject>("FightingPanel")
        };
    }
}