using Game.GameRuntime.Entities.Base.BaseSceneObj.Base;
using UnityEngine;

namespace Game.GameRuntime.GameSceneManager.SubManager.Buff
{
    public interface IBuffManager : ISubSceneManager
    {
        T AddBuff<T>(ISceneObject obj) where T : class, IBuff, new();
        void RemoveBuff(ISceneObject obj, string buff);
        T GetBuff<T>(ISceneObject obj) where T : IBuff;
        GameObject GetPrefabsAsset(params string[] keys);
    }
}