using System;
using GameFramework.UnityRuntime.UI;

namespace Game.GameMgr.Component.UI
{
    public class OpenFormArgs
    {
        /// <summary>
        /// 数据
        /// </summary>
        public object userData;

        /// <summary>
        /// 打开界面成功回调
        /// </summary>
        public Action<UIFormLogic> callBack;
    }
}