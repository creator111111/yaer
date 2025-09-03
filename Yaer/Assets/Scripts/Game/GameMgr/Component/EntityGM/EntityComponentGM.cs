using System;
using System.Collections.Generic;
using Game.GameMgr.Component.Base;
using GameFramework.Event;
using GameFramework.ObjectPool;
using GameFramework.UnityRuntime.Base;
using GameFramework.UnityRuntime.Entity;
using GameFramework.UnityRuntime.Event;
using GameFramework.UnityRuntime.Utility;

namespace Game.GameMgr.Component
{
    public class EntityComponentGM : BaseComponentGM
    {
        private int serialId = 0;
        
        private EntityComponent entityComponent;
        private Dictionary<int, LoadingEntityInfo> entityCallBackDic = new Dictionary<int, LoadingEntityInfo>(); // 实体加载回调
        private Dictionary<Type, string> entityPathDic = new Dictionary<Type, string>(); // 已加载的实体

        public override void OnInit()
        {
            base.OnInit();

            entityComponent = GameManager.GetGFComponent<EntityComponent>();
            GameManager.GetGFComponent<EventComponent>().Subscribe(ShowEntitySuccessEventArgs.EventId, OnShowEntitySuccess);
        }

        private void OnShowEntitySuccess(object sender, GameEventArgs e)
        {
            if (e is ShowEntitySuccessEventArgs args)
            {
                if (entityCallBackDic.TryGetValue(args.EntityId, out var info))
                {
                    info.loadState = LoadingEntityType.Complete;
                    entityPathDic[args.EntityLogicType] = args.Entity.EntityAssetName;
                    info.callBack?.Invoke(args.Entity.Logic);
                    entityCallBackDic.Remove(args.EntityId);
                }
            }
        }

        public Entity GetEntity(string assetName)
        {
            return entityComponent.GetEntity(assetName);
        }

        public T GetEntityLogic<T>() where T : EntityLogic
        {
            if (entityPathDic.TryGetValue(typeof(T), out var path))
            {
                return entityComponent.GetEntity(path).Logic as T;
            }

            Log.Warning("未加载EntityLogic哦~=>" + typeof(T).Name);
            
            return null;    
        }

        public void ShowEntity<T>(int id, string assetName, string groupName, int priority, object userData, Action<T> callBack) where T : EntityLogic
        {
            var info = new LoadingEntityInfo()
            {
                loadState = LoadingEntityType.Loading,
                callBack = logic => callBack?.Invoke(logic as T)
            };
            entityCallBackDic.Add(id, info);
            entityComponent.ShowEntity<T>(id, assetName, groupName, priority, userData);
        }
        
        public void ShowEntity<T>(string assetName, string groupName, int priority, object userData, Action<T> callBack) where T : EntityLogic
        {
            ShowEntity(serialId, assetName, groupName, priority, userData, callBack);
            serialId++;
        }
        
        public void ShowPlayerEntity<T>(string assetName, int priority, object userData, Action<T> callBack) where T : EntityLogic
        {
            ShowEntity(assetName, "Player", priority, userData, callBack);
        }
        
        public void ShowEffectEntity<T>(string assetName, int priority, object userData, Action<T> callBack) where T : EntityLogic
        {
            ShowEntity(assetName, "Effect", priority, userData, callBack);
        }

        public void ShowMonsterEntity<T>(string assetName, int priority, object userData, Action<T> callBack) where T : EntityLogic
        {
            if (!entityComponent.HasEntityGroup("Monster"))
            {
                entityComponent.AddEntityGroup("Monster", 0.1f, 100, 0.1f, 1);
            }
            ShowEntity(assetName, "Monster", priority, userData, callBack);
        }

        public void HideEntity(int id)
        {
            entityComponent.HideEntity(id);
        }

        public void HideEntity(Entity entity)
        {
            if (entity == null)
            {
                Log.Warning("Entity is invalid.");
                return;
            }

            if (entityPathDic.TryGetValue(entity.Logic.GetType(), out var path))
            {
                entityPathDic.Remove(entity.Logic.GetType());
            }
            
            entityComponent.HideEntity(entity);
        }

        public bool HasEntity(int id)
        {
            return entityComponent.HasEntity(id);
        }

        public void HideAllLoadedEntities()
        {
            entityPathDic.Clear();
            entityComponent.HideAllLoadedEntities();
        }

        public override void OnExit()
        {
            base.OnExit();

            // 回收
            entityComponent.HideAllLoadedEntities();
            entityComponent.HideAllLoadingEntities();
        }
    }
}