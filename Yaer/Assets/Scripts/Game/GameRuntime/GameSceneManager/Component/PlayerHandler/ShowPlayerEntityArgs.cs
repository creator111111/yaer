using System;
using Game.GameRuntime.Entities.Player;

namespace Game.GameRuntime.GameSceneManager.Component
{
    public class ShowPlayerEntityArgs
    {
        public Action<PlayerLogic> initAcion;
    }
}