using Game.GameRuntime.Entities.Component.Effect;
using GameFramework.CoreExtend.Generic;
using UnityEngine;

namespace Game.GameRuntime.Entities.Player
{
    public interface IPlayerProxy
    {
        RuntimeAnimatorController GetAnimatorController();
        void RefreshCameraFollow(Transform tar);
        void UpdateFightingPanel();
        T GetEffectPrefabs<T>(params string[] key) where T : IEffectComponent;

        ValuePro GetRuntimeConfig(string key);
        ValuePro GetStaticConfig(string key);
    }
}