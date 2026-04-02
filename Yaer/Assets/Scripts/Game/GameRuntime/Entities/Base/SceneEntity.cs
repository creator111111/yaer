using Game.GameRuntime.Entities.Base.BaseSceneObj;
using Game.GameRuntime.GameSceneManager.Component;
using GameFramework.Entity;
using GameFramework.UnityRuntime.Entity;
using GameFramework.UnityRuntime.Utility;
using UnityEngine;

namespace Game.GameRuntime.Entities.Base
{
    /// <summary>
    /// 场景实体
    /// </summary>
    public sealed class SceneEntity : MonoBehaviour, IEntity//场景实体容器，负责把“逻辑类”接起来
    {
        private int id;
        private bool isInit;
        private string entityAssetName;
        private object handle;
        private IEntityGroup entityGroup;
        private BaseSceneEntityLogic entityLogic;
        private SceneEntityComponentGSM entityComponentGSM;

        public int Id => id;
        public string EntityAssetName => entityAssetName;
        public object Handle => handle;
        public IEntityGroup EntityGroup => entityGroup;

        public EntityLogic EntityLogic => entityLogic;

        /// <summary>
        ///  场景实体初始化
        /// </summary>
        public void OnInit(SceneEntityComponentGSM entityComponent)
        {
            entityLogic = GetComponent<BaseSceneEntityLogic>();
            if (entityLogic == null)
            {
                Log.Error("SceneEntity必须添加BaseSceneEntityLogic组件");
                return;
            }

            entityLogic.OnInit(entityComponent);
            isInit = true;
        }

        /// <summary>
        /// 实体初始化
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="entityAssetName"></param>
        /// <param name="entityGroup"></param>
        /// <param name="isNewInstance"></param>
        /// <param name="userData"></param>
        public void OnInit(int entityId, string entityAssetName, IEntityGroup entityGroup, bool isNewInstance, object userData)
        {
            Log.Error("SceneEntity弃用该初始化方法");
        }
        

        /// <summary>
        /// 退出场景时执行
        /// </summary>
        public void OnShutDown()
        {
            entityLogic.OnShutDown();
        }

        public void OnRecycle()
        {
            Log.Error("SceneEntity弃用此方法");
        }

        public void OnShow(object userData)
        {
            Log.Error("SceneEntity弃用此方法");
        }

        public void OnHide(bool isShutdown, object userData)
        {
            Log.Error("SceneEntity弃用此方法");
        }

        public void OnAttached(IEntity childEntity, object userData)
        {
            Log.Error("SceneEntity弃用此方法");
        }

        public void OnDetached(IEntity childEntity, object userData)
        {
            Log.Error("SceneEntity弃用此方法");
        }

        public void OnAttachTo(IEntity parentEntity, object userData)
        {
            Log.Error("SceneEntity弃用此方法");
        }

        public void OnDetachFrom(IEntity parentEntity, object userData)
        {
            Log.Error("场景实体弃用此方法");
        }

        public void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            if (isInit)
            {
                entityLogic.OnUpdate(elapseSeconds, realElapseSeconds);
            }
        }
    }
}