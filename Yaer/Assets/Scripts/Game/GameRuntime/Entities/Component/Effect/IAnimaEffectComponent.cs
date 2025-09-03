using UnityEngine;

namespace Game.GameRuntime.Entities.Component.Effect
{
    public interface IAnimaEffectComponent : IEffectComponent
    {
        void FollowSrSortLayer(SpriteRenderer sr, bool up);
    }
}