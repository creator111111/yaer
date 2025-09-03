using System;

namespace GameFramework.CoreExtend.Resource
{
    public interface IResourceSubSystem
    {
        void Preload(string fullName);
        void Preload(params string[] keys);
        void Unload(string fullName);
        T Load<T>(string fullName);
        T Load<T> (params string[] keys);
        void LoadAsync<T>(string fullName, Action<T> callBack);
    }
}