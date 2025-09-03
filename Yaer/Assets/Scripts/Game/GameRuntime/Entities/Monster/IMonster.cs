using Game.GameRuntime.Entities.Base.BaseSceneObj.Base;
using UnityEngine;

namespace Game.GameRuntime.Entities.Monster
{
    public interface IMonster
    {
        bool IsDead { get; }
        bool AllowControl { get; set; }
        Vector2 Wound(int value, Vector2 dir, float backDistance);
    }
}