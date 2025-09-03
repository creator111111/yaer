using UnityEngine;

namespace Game.GameRuntime.Entities.Component.Effect
{
    public interface IEffectComponent
    {
        GameObject GameObject { get; }
        void Play(int times);
        void Play();
    }
}