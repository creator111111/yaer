using System.Collections.Generic;
using Game.GameRuntime.Entities.Component.Anima.interf;
using GameFramework.UnityRuntime.Entity;
using GameFramework.UnityRuntime.Utility;
using UnityEngine;

namespace Game.GameRuntime.Entities.Component.Anima
{
    public abstract class BaseCsAnimator : BaseGFComponentEntity, ICsAnimator
    {
        [SerializeField] protected Animator animator;
        [SerializeField] private SpriteRenderer animaSr;
        private List<ICsRuntimeController> runtimeControllerList;
        private ICsRuntimeController currentCsRuntimeController;
        private ICsAnimator csAnimatorImplementation;

        public ICsRuntimeController CurrentCsRuntimeController => currentCsRuntimeController;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (animator == null) Log.Error($"{GetType().Name}未绑定Animator");
        }
#endif

        protected override void OnInit()
        {
            if (animator is null)
            {
                Log.Error("animator为空 " + GetType().Name);
            }

            runtimeControllerList = new List<ICsRuntimeController>();
        }

        public override void Check()
        {
            base.Check();

            if (currentCsRuntimeController == null)
            {
                Log.Error("CurrentRuntimeController为空 " + GetType().Name);
            }
        }

        public override void OnUpdate()
        {
            currentCsRuntimeController?.Update();
        }

        public override void OnFixedUpdate()
        {
            currentCsRuntimeController?.FixedUpdate();
        }


        // --------------------------------------------------------------------------------
        public void ChangeState<T>() where T : IState => currentCsRuntimeController.ChangeState<T>();

        public void ChangeState<T, TK>() where T : IStateMachine where TK : IState =>
            currentCsRuntimeController.EnterSubStateMachine<T>().ChangeState<TK>();

        public void ChangeState(string stateName) => currentCsRuntimeController.ChangeState(stateName);

        public void ChangeState
            (string subStateMachineName, string stateName) => currentCsRuntimeController.ChangeState(subStateMachineName, stateName);

        // --------------------------------------------------------------------------------
        public bool IsState<T>() where T : IState
        {
            return false;
        }

        public T GetEntityLogic<T>() where T : EntityLogic
        {
            if (Entity)
            {
                return Entity.Logic as T;
            }
            
            if (SceneEntity)
            {
                return SceneEntity.EntityLogic as T;
            }

            Log.Warning("Entity的引用为空", gameObject);
            return null;
        }
        
        public Animator GetAnimator()
        {
            return animator;
        }

        //-----------------------------------------------------------------------------------
        // 控制animator参数

        public void SetBool(string name, bool value)
        {
            animator.SetBool(name, value);
        }

        public void SetTrigger(string name)
        {
            animator.SetTrigger(name);
        }

        public void SetFloat(string name, float value)
        {
            animator.SetFloat(name, value);
        }

        public void SetInt(string name, int value)
        {
            animator.SetInteger(name, value);
        }

        public void SetSpeed(float speed)
        {
            animator.speed = speed;
        }

        public AnimatorStateInfo GetCurrentAnimatorStateInfo(int i = 0) => animator.GetCurrentAnimatorStateInfo(i);

        //-----------------------------------------------------------------------------------
        // RuntimeController

        public void RegisterRuntimeController<T>(RuntimeAnimatorController controllerAsset = null) where T : ICsRuntimeController, new()
        {
            var csRuntimeController = new T();
            if (controllerAsset != null)
            {
                csRuntimeController.SetControllerAsset(controllerAsset);
            }

            csRuntimeController.Init(this);

            RegisterRuntimeController(csRuntimeController);
        }

        public void RegisterRuntimeController(ICsRuntimeController controller)
        {
            if (runtimeControllerList.Contains(controller) == false) runtimeControllerList.Add(controller);
        }


        public void RegisterRuntimeController(BaseCsRuntimeController csController, RuntimeAnimatorController controllerAsset)
        {
            if (csController == null || controllerAsset == null)
            {
                Log.Error("controllerAsset或者csController为空");
                return;
            }

            if (runtimeControllerList.Contains(csController) == false)
            {
                csController.SetControllerAsset(controllerAsset);
                runtimeControllerList.Add(csController);
            }
            else
            {
                Log.Error("已经注册过controller: " + csController.GetType().Name);
            }
        }

        public void ChangeRuntimeController<T>() where T : ICsRuntimeController
        {
            var newRc = runtimeControllerList.Find(x => x.GetType() == typeof(T));

            if (newRc == null)
            {
                Log.Error("没有找到csRuntimeController: " + typeof(T).Name);
                return;
            }

            currentCsRuntimeController?.Exit();
            currentCsRuntimeController = newRc;
            currentCsRuntimeController.Enter();
        }

        public void ChangeRuntimeController<T>(RuntimeAnimatorController controllerAsset) where T : ICsRuntimeController
        {
            var r = runtimeControllerList.Find(x => x.GetType() == typeof(T));

            if (r == null)
            {
                Log.Error("没有找到csRuntimeController: " + typeof(T).Name);
                return;
            }

            r.SetControllerAsset(controllerAsset);

            currentCsRuntimeController?.Exit();
            currentCsRuntimeController = r;
            currentCsRuntimeController.Enter();
        }

        //-----------------------------------------------------------------------------------
        // sign

        public void SetSign(string key, bool value) => currentCsRuntimeController.SetSign(key, value);

        public bool GetSign(string key) { 
            if (currentCsRuntimeController == null) { return false; }
            return currentCsRuntimeController.GetSign(key); 
        }

        // --------------------------------------------------------------------------------


    }
}