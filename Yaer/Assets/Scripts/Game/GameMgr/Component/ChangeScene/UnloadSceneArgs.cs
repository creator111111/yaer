using System;

namespace Game.GameMgr.Component.ChangeScene
{
    public class UnloadSceneArgs
    {
        public string unloadSceneName;
        /// <summary>
        /// 卸载完成回调
        /// </summary>
        public Action callBack;
    }
}