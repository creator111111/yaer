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

        /// <summary>
        /// 可选：写入 <see cref="ChangeSceneComponentGM.LastSceneName"/> 的 EnterPos 匹配键。
        /// 空则仍用卸场时的真实场景名。原因（0901）：村长家 1 楼门与楼梯同目标村，须拆键避免抢同一落点。
        /// </summary>
        public string enterPosKey;
    }
}