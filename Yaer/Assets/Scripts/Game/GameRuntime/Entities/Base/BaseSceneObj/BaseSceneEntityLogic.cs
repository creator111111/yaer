using System;
using System.Collections.Generic;
using Game.GameRuntime.Entities.Component.Battle.Damage;
using Game.GameRuntime.GameSceneManager.Base;
using Game.GameRuntime.GameSceneManager.Component;
using GameFramework.UnityRuntime.Entity;
using GameFramework.UnityRuntime.Utility;
using GameFramework.UnityRuntimeExtend.Component;
using UnityEngine;

namespace Game.GameRuntime.Entities.Base.BaseSceneObj
{
    public abstract class BaseSceneEntityLogic : BaseEntityLogic
    {
        public ComponentSystemMono componentSystem;
        private SceneEntityComponentGSM component;
        
        private Dictionary<string, Transform> cacheTsf = new Dictionary<string, Transform>();
        protected float Y => transform.position.y;

        private bool isInit;
        public bool IsExist { get; protected set; }

        public BaseGameSceneManager SceneManager => component?.SceneManager;

        private void OnValidate()
        {
            componentSystem = GetComponent<ComponentSystemMono>();
        }

        protected virtual void Start()
        {
            if (!isInit) Log.Error("场景对象未到场景管理器注册并初始化=>" + gameObject.name);
        }

        protected internal override void OnInit(object userData)
        {
            base.OnInit(userData);
            
            component = userData as SceneEntityComponentGSM;
            
            IsExist = true;
            
            componentSystem = GetComponent<ComponentSystemMono>();
            if (componentSystem == null)
            {
                Log.Error("请添加ComponentSystemMono --" + GetType().Name, gameObject);
            }
            
            InitComponentSystem();
            
            isInit = true;
        }

        /// <summary>
        /// 直接退出场景时执行
        /// </summary>
        public virtual void OnShutDown()
        {
            IsExist = false;
        }

        protected T GetSceneManager<T>() where T : class, IGameSceneManager
        {
            return SceneManager as T;
        }

        protected virtual void InitComponentSystem()
        {
            componentSystem.OnInit();
        }

        public Transform GetTsf(string path)
        {
            if (cacheTsf.ContainsKey(path)) return cacheTsf[path];

            var tsf = transform.Find(path);
            if (tsf is null)
            {
                Debug.LogError("场景对象未找到子对象=>" + path);
                return null;
            }

            cacheTsf.Add(path, tsf);
            return tsf;
        }

        public Vector2 GetTsfPos(string path)
        {
            var tsf = GetTsf(path);
            return tsf == default ? default : tsf.position;
        }
        
        public void SetObjActive(bool active)
        {
            gameObject.SetActive(active);
        }
    }
}