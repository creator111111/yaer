using UnityEngine;

namespace Game.GameMgr.Manager.Res.AnimatorController
{
    public interface IAnimatorControllerResManager : IGameResourcesSubManager
    {
        RuntimeAnimatorController Get(params string[] strings);
        void Release(params string[] keys);
    }
}