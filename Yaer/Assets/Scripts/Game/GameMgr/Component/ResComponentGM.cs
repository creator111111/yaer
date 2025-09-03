using System;
using System.Collections.Generic;
using Game.GameMgr.Component.Base;
using Game.GameMgr.Manager.Res;
using Game.GameMgr.Manager.Res.SceneRes.Config.Generic;
using GameFramework.DataTable;
using GameFramework.Resource;
using GameFramework.UnityRuntime.DataTable;
using GameFramework.UnityRuntime.Resource;
using GameFramework.UnityRuntime.Utility;
using Newtonsoft.Json;
using UnityEngine;

namespace Game.GameMgr.Component
{
    public class ResComponentGM : BaseComponentGM
    {
        private IGameResourcesManagerHelper helper;

        private Dictionary<string, IGameResourcesSubManager> subManagersDic = new Dictionary<string, IGameResourcesSubManager>();


        public T GetSubManager<T>() where T : class, IGameResourcesSubManager
        {
            if (subManagersDic.TryGetValue(typeof(T).Name, out var value)) return value as T;

            Debug.LogError("请通过接口访问或未注册该子管理器:" + typeof(T).Name);
            return null;
        }

        //-----------------------------------------------------------------------------------


        /// <summary>
        /// 异步加载资源
        /// </summary>
        /// <param name="assetName">资源路径</param>
        /// <param name="onComplete">完成回调</param>
        /// <typeparam name="T">泛型</typeparam>
        public void LoadAsset<T>(string assetName, Action<T> onComplete)
        {
            GameManager.GetGFComponent<ResourceComponent>().LoadAsset(assetName, typeof(T), new LoadAssetCallbacks(LoadAssetSuccessCallback, LoadAssetFailureCallback));
            return;

            // 加载成功回调
            void LoadAssetSuccessCallback(string assetname, object asset, float duration, object userdata)
            {
                if (asset is T tAsset)
                {
                    onComplete?.Invoke(tAsset);
                }
            }

            // 加载失败回调
            void LoadAssetFailureCallback(string assetname, LoadResourceStatus status, string errormessage, object userdata)
            {
                Log.Error("加载资源失败:{0}", assetname);
            }
        }

        public void LoadConfig<T>(string assetPath, Action<IDataTable<T>> onComplete) where T : class, IDataRow, new()
        {
            // 先从缓存获取
            var cache = GameManager.GetGFComponent<DataTableComponent>().GetDataTable<T>(assetPath) ;
            if (cache != null)
            {
                onComplete?.Invoke(cache);
                return;
            }
            
            // 缓存不存在再加载
            Action<TextAsset> callBack = asset =>
            {
                var json = asset.text;
                if (json != null)
                {
                    var jsonClass = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(json);
                    // 创建表
                    var table = GameManager.GetGFComponent<DataTableComponent>().CreateDataTable<T>(assetPath);

                    foreach (var item in jsonClass)
                    {
                        // 添加数据
                        table.AddDataRow("", item);
                    }

                    onComplete?.Invoke(table);
                }
            };
            
            LoadAsset<TextAsset>(assetPath, callBack);
        }
    }
}