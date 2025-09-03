using System;

namespace GameFramework.UnityRuntimeExtend.Resource
{
    /// <summary>
    ///     单资源加载信息
    /// </summary>
    [Serializable]
    public class PreloadAssetInfo
    {
        public string[] keys;
        public Action<bool> callBack;
        public Type type;

        public PreloadAssetInfo(Type type, Action<bool> callBack = null, params string[] keys)
        {
            this.type = type;
            this.callBack = callBack;
            this.keys = keys;
        }

        public PreloadAssetInfo(Type type, params string[] keys)
        {
            this.type = type;
            callBack = null;
            this.keys = keys;
        }
    }
}