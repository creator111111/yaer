using System;

namespace GameFramework.UnityRuntimeExtend.Resource
{
    public class PreloadAssetInfo<T> : PreloadAssetInfo where T : class
    {
        public PreloadAssetInfo(Action<bool> callBack = null, params string[] keys) : base(typeof(T), callBack, keys)
        {
        }

        public PreloadAssetInfo(params string[] keys) : base(typeof(T), null, keys)
        {
        }
    }
}