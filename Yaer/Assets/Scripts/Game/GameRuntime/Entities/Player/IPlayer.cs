using Game.GameRuntime.Entities.Base.BaseSceneObj.Base;
using UnityEngine;

namespace Game.GameRuntime.Entities.Player
{
    public interface IPlayer : IStateObject, ISceneObject, IDepthObject, IPathfindingTarget
    {
        // 数值状态
        bool IsDead { get; }
        int Hp { get; }
        int HpMax { get; }
        int Mp { get; }

        int MpMax { get; }

        // 动画状态
        bool IsRunning { get; }

        // 控制状态
        bool IsCombatState { get; set; }
        bool AllowControl { get; set; }

        bool AllowDepthMove { get; set; }

        // 
        IPlayerProxy Proxy { get; set; }

        // --------------------------------------------------------------------------------
        // phy
        Bounds BodySize { get; }

        // 战斗相关方法
        void Wound(int value, Vector2 dir, float backDistance);
    }
}