using System;

namespace GameFramework.UnityRuntimeExtend.Resource
{
    /// <summary>
    ///     多资源加载处理器
    /// </summary>
    public class PreloadAssetInfoHandle
    {
        private readonly Action<bool> callBack; // 全部资源加载完成回调
        public int count; // 总共要加载的资源数量

        public PreloadAssetInfoHandle(int count, Action<bool> callBack)
        {
            this.count = count;
            this.callBack = callBack;
        }

        public void Done()
        {
            count--;
            if (count == 0) callBack?.Invoke(true);
        }
    }
}