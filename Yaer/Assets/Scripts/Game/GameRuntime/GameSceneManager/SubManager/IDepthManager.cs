using System.Collections.Generic;
using Game.GameRuntime.Entities.Base.BaseSceneObj.Base;
using Game.GameRuntime.Entities.Player;

namespace Game.GameRuntime.GameSceneManager.SubManager
{
    public interface IDepthManager : ISubSceneManager
    {
        void RegisterPlayerAndObjs(List<ISceneObject> objs, IPlayer player);
        void ExitScene();
    }
}