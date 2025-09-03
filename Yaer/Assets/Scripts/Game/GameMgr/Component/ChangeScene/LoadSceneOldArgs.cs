using System;
using Game.GameMgr.Component.Archive.ArchiveDataClass.Player;
using Game.GameMgr.Manager.Res;
using Game.GameRuntime.UI.Args;
using UnityEngine;

namespace Game.GameMgr.Component.ChangeScene
{
    public class LoadSceneOldArgs : ILoadSceneArgs
    {
        private GamePreloadHandler handler;


        public LoadSceneOldArgs(ChangeSceneInfo info, Action callBack = null)
        {
            ChangeSceneInfo = info;
            LoadEndCallBack = callBack;
        }

        /// <summary>
        ///     场景跳转
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="pos"></param>
        /// <param name="callBack"></param>
        public LoadSceneOldArgs(string from, string to, Transform pos = null, Action callBack = null)
        {
            ChangeSceneInfo = new ChangeSceneInfo(from, to, pos);
            LoadEndCallBack = callBack;
        }

        public IGamePreloadHandler Handler
        {
            get
            {
                if (handler == null) handler = new GamePreloadHandler();

                return handler;
            }
        }

        public ChangeSceneInfo ChangeSceneInfo { get; }

        public Action LoadEndCallBack { get; set; }

        public ChangeSceneInfo GetChangeSceneInfo()
        {
            return ChangeSceneInfo;
        }
    }
}