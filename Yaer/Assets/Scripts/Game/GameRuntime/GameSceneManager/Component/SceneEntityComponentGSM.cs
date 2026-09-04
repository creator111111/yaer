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

            // 运行时按 objRoot 重扫：避免场景 YAML 漏挂 sceneObjs 导致 NPC Start 报「未注册」且永不 OnInit。
            // OnValidate 仅编辑器刷新；打包/进游戏必须以磁盘子树为准。
            if (objRoot != null)
            {
                sceneObjs = objRoot.GetComponentsInChildren<SceneEntity>(true).ToList();
            }

            foreach (var obj in sceneObjs)
            {
                if (obj == null)
                {
                    continue;
                }
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
            // Find 未命中时不能对 null 取 EntityLogic，否则会直接 NullReferenceException 中断 SceneManager.Awake。
            var entity = sceneObjs.Find(o =>
                o != null && o.EntityLogic != null && o.EntityLogic.GetType() == typeof(T));
            if (entity == null || entity.EntityLogic == null)
            {
                Log.Error("未找到该场景实体逻辑" + typeof(T).Name);
                return null;
            }

            return entity.EntityLogic as T;
        }
    }
}