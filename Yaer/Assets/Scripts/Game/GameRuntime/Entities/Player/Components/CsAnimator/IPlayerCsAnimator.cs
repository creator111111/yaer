using Game.GameRuntime.Entities.Component.Anima.interf;

namespace Game.GameRuntime.Entities.Player.Components.CsAnimator
{
    public interface IPlayerCsAnimator : ICsAnimator
    {
        bool IsNormalAttacking { get; set; }
    }
}