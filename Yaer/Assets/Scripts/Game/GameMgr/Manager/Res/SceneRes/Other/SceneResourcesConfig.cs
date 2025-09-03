using System.Collections.Generic;
using GameFramework.UnityRuntimeExtend.Resource;
using UnityEngine;

namespace Game.GameMgr.Manager.Res.SceneRes.Other
{
    [CreateAssetMenu(fileName = "SceneResourcesConfig", menuName = "ConfigData/SceneResourcesConfig", order = 1)]
    public class SceneResourcesConfig : ScriptableObject
    {
        public string sceneName;
        public List<PreloadAssetInfo> resList;
    }
}