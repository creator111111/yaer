using System.Collections.Generic;
using System.Linq;
using Game.GameRuntime.Entities.Base;
using Game.GameRuntime.Entities.Base.BaseSceneObj;
using Game.GameRuntime.GameSceneManager.Base;
using SingularityGroup.HotReload;
using UnityEngine;

namespace Game.GameRuntime.GameSceneManager.Component
{
    public class SceneEntityComponentGSM : BaseComponentGSM
    {
        [SerializeField] private Transform objRoot;
        [SerializeField] private List<SceneEntity> sceneObjs = new List<SceneEntity>();

        private void OnValidate()
        {
            if (objRoot)
            {
                sceneObjs = objRoot.GetComponentsInChildren<SceneEntity>(true).ToList();
            }
        }

        public override void OnInit(IGameSceneManager manager)
        {
            base.OnInit(manager);
            
            foreach (var obj in sceneObjs)
            {
                obj.OnInit(this);    
            }
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            
            foreach (var obj in sceneObjs)
            {
                obj.OnUpdate(Time.deltaTime, Time.unscaledDeltaTime);    
            }
        }

        public override void OnShutdown()
        {
            base.OnShutdown();
            
            foreach (var obj in sceneObjs)
            {
                obj.OnShutDown();  
            }
        }

        public List<SceneEntity> GetAllSceneEntities() 
        {
            return sceneObjs;
        }
        
        public T GetSceneEntityLogic<T>() where T : BaseSceneEntityLogic
        {
            var logic = sceneObjs.Find(o => o.EntityLogic.GetType() == typeof(T)).EntityLogic;
            if (logic is null)
            {
                Log.Error("未找到该场景实体逻辑" + typeof(T).Name);
                return null;
            }
            return logic as T;
        }
    }
}