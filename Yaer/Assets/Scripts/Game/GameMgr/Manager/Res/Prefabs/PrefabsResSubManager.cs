using Game.GameMgr.Component;
using Game.Static.Enum;
using UnityEngine;

namespace Game.GameMgr.Manager.Res.Prefabs
{
    public class PrefabsResSubManager : IPrefabsResSubManager
    {
        private readonly ResComponentGM resSystem;

        public PrefabsResSubManager(ResComponentGM resSystem)
        {
            this.resSystem = resSystem;
        }

        public GameObject GetEffectPrefab(EResLoadType type, params string[] keys)
        {
            if (type == EResLoadType.Addressable)
            {
/*                var prefab = resSystem.LoadAsset<GameObject>(keys)
                if (prefab != null) return prefab;*/

                Debug.LogError($"该特效预制体未预加载 key:{string.Join(",", keys)}");
                return null;
            }

            return null;
        }
        

        public void Init(IGameResourcesManager manager)
        {
            
        }
    }
}