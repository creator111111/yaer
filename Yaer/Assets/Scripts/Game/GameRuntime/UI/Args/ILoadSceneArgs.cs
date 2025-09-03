using System;
using Game.GameMgr.Component.Archive.ArchiveDataClass.Player;
using Game.GameMgr.Manager.Res;

namespace Game.GameRuntime.UI.Args
{
    public interface ILoadSceneArgs
    {
        Action LoadEndCallBack { get; set; }
        IGamePreloadHandler Handler { get; }
        ChangeSceneInfo ChangeSceneInfo { get; }
    }
}