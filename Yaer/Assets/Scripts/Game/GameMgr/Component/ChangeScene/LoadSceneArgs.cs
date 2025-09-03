using System;

namespace Game.GameMgr.Component.ChangeScene
{
    public class LoadSceneArgs
    {
        public string sceneName;
        public object userData;
        /// <summary>
        /// 场景加载完成回调
        /// </summary>
        public Action callBack;
    }
}