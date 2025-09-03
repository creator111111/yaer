using System;
using System.Collections;
using GameFramework.CoreExtend.Generic;
using GameFramework.UnityRuntimeExtend.Mono;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GameFramework.UnityRuntimeExtend.Resource.UnityResources
{
    public class ResourcesSystem : BaseSingleton<ResourcesSystem>
    {
        // 同步资源加载
        public T Load<T>(string fullName) where T : Object
        {
            return Resources.Load<T>(fullName);
        }

        // 异步资源加载
        public void LoadAsync<T>(string fullName, Action<T> callBack) where T : Object
        {
            MonoSystem.Instance.StartCoroutineFrameWork(LoadAsyncCoroutine(fullName, callBack));
        }

        // 异步加载协程
        private IEnumerator LoadAsyncCoroutine<T>(string fullName, Action<T> callBack) where T : Object
        {
            var rr = Resources.LoadAsync<T>(fullName);
            yield return rr;
            // 带泛型Action表示执行要带的时的参数类型
            callBack.Invoke(rr.asset as T);
        }
    }
}