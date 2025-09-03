using System;
using GameFramework.UnityRuntime.Entity;

namespace Game.GameMgr.Component
{
    public enum LoadingEntityType
    {
        Loading,
        Complete
    }
    public class LoadingEntityInfo
    {
        public LoadingEntityType loadState;
        public Action<EntityLogic> callBack;
    }
}