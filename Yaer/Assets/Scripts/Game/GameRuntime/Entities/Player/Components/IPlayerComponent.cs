using Game.GameRuntime.GameSceneManager.Base;

namespace Game.GameRuntime.Entities.Player.Components
{
    public interface IPlayerComponent
    {
        PlayerLogic PlayerLogic { get; set; }
    }
}