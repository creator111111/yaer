using System.Collections.Generic;
using Game.GameRuntime.Entities.Base.BaseSceneObj.Base;
using Game.GameRuntime.GameSceneManager.Base;
using Game.Static.Enum;
using UnityEngine;

namespace Game.GameRuntime.GameSceneManager.SubManager.Buff
{
    public class BuffManager : MonoBehaviour, IBuffManager
    {
        private readonly Dictionary<GameObject, List<IBuff>> buffDic = new Dictionary<GameObject, List<IBuff>>();
        private IGameSceneManager sceneManager;

        private void Update()
        {
            // 更新
            foreach (var obj in buffDic.Keys)
            foreach (var buff in buffDic[obj])
                if (buff.IsApply)
                    buff.Update();
        }

        public void Init(IGameSceneManager m)
        {
            sceneManager = m;
        }

        public T AddBuff<T>(ISceneObject obj) where T : class, IBuff, new()
        {
            var buff = new T();
            if (buffDic.ContainsKey(obj.GameObject))
                buffDic[obj.GameObject].Add(buff);
            else
                buffDic.Add(obj.GameObject, new List<IBuff> { buff });
            buff.Init(this);

            return buff;
        }

        public void RemoveBuff(ISceneObject obj, string buffName)
        {
            if (buffDic.ContainsKey(obj.GameObject))
                for (var i = 0; i < buffDic[obj.GameObject].Count; i++)
                    if (buffDic[obj.GameObject][i].GetType().Name == buffName)
                    {
                        buffDic[obj.GameObject].Remove(buffDic[obj.GameObject][i]);
                        break;
                    }
        }

        public T GetBuff<T>(ISceneObject obj) where T : IBuff
        {
            if (buffDic.ContainsKey(obj.GameObject))
                foreach (var buff in buffDic[obj.GameObject])
                    if (buff.GetType() == typeof(T))
                        return (T)buff;

            return default;
        }

        public GameObject GetPrefabsAsset(params string[] keys)
        {
            // return sceneManager.GetPrefabsAsset(EResLoadType.Addressable, keys);
            return null;
        }
    }
}