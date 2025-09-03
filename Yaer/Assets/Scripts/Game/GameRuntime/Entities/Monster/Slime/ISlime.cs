using Game.GameRuntime.Entities.Player;

namespace Game.GameRuntime.Entities.Monster.Slime
{
    public interface ISlime : IMonster
    {
         IPlayer Target { get; }
    }
}