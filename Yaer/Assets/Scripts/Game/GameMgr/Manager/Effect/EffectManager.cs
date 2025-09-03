using Game.GameMgr.Manager.Res;
using Game.GameMgr.Manager.Res.Prefabs;
using Game.GameRuntime.Entities.Component.Effect;
using Game.Static.Enum;
using UnityEngine;

namespace Game.GameMgr.Manager.Effect
{
    public class EffectManager : MonoBehaviour, IEffectManager
    {
        public EResLoadType DefaultLoadType { get; }

        public void Init()
        {
        }


        public T CreateEffect<T>(EResLoadType type, params string[] keys) where T : IEffectComponent
        {
            var prefab = GameManager.GetManager<IGameResourcesManager>().GetSubManager<IPrefabsResSubManager>().GetEffectPrefab(type, keys);
            if (prefab == null) return default;

            return Object.Instantiate(prefab, Vector3.zero, Quaternion.identity).GetComponent<T>();
        }

        public T CreateEffect<T>(params string[] keys) where T : IEffectComponent
        {
            return CreateEffect<T>(DefaultLoadType, keys);
        }
    }
}