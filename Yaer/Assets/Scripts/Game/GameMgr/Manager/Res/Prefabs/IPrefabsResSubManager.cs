using Game.Static.Enum;
using UnityEngine;

namespace Game.GameMgr.Manager.Res.Prefabs
{
    public interface IPrefabsResSubManager : IGameResourcesSubManager
    {
        GameObject GetEffectPrefab(EResLoadType type, params string[] keys);
    }
}