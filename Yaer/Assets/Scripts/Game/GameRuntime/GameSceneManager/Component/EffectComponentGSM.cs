using System;
using Game.GameMgr;
using Game.GameMgr.Component;
using GameFramework.UnityRuntime.Entity;

namespace Game.GameRuntime.GameSceneManager.Component
{
    public class EffectComponentGSM: BaseComponentGSM
    {
        public void PlayEffect<T>(string assetPath, Action<T> callBack) where T : EntityLogic
        {
            GameManager.GetGMComponent<EntityComponentGM>().ShowEffectEntity<T>(assetPath, 0, null, callBack);
        }
    }
}