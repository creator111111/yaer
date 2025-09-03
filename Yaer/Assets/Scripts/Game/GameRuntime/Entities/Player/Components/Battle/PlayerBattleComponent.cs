using Game.GameRuntime.Entities.Component.Battle;
using Game.GameRuntime.GameSceneManager.Base;

namespace Game.GameRuntime.Entities.Player.Components.Battle
{
    public class PlayerBattleComponent : BattleComponent, IPlayerComponent
    {
        public IGameSceneManager SceneManager { get; set; }
        public PlayerLogic PlayerLogic { get; set; }
    }
}