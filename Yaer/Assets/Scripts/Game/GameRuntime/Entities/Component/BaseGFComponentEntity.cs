using Game.GameRuntime.Entities.Base;
using GameFramework.Entity;
using GameFramework.UnityRuntime.Utility;
using GameFramework.UnityRuntimeExtend.Component;
using UnityEngine;

namespace Game.GameRuntime.Entities.Component
{
    /// <summary>
    /// 实体上使用
    /// </summary>
    public class BaseGFComponentEntity : BaseGFComponentMono
    {
        [SerializeField] private GameFramework.UnityRuntime.Entity.Entity entity;
        [SerializeField] private Base.SceneEntity sceneEntity;
        public BaseEntityControll entityControll; // 实体控制类
        public GameFramework.UnityRuntime.Entity.Entity Entity
        {
            get
            {
                if (entity != null)
                {
                    return entity;
                }
                
                return null;
            }
            set { entity = value; }
        }

        public SceneEntity SceneEntity
        {
            
            get
            {
                if (sceneEntity != null)
                {
                    return sceneEntity;
                }
                
                return null;
            }
        }

        protected override void OnInit()
        {
        }
    }
}