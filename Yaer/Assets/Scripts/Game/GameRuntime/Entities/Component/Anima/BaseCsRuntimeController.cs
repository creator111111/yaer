using Game.GameRuntime.Entities.Component.Anima.interf;
using GameFramework.UnityRuntime.Entity;
using UnityEngine;

namespace Game.GameRuntime.Entities.Component.Anima
{
    public class BaseCsRuntimeController : ICsRuntimeController
    {
        protected ICsAnimator csAnimator;
        protected RuntimeAnimatorController controllerAsset;
        public IStateMachine mainStateMachine;

        protected BaseCsRuntimeController()
        {
        }

        public virtual void Init(ICsAnimator csAnimator)
        {
            this.csAnimator = csAnimator;
        }

        protected void RegisterMainStateMachine<T>() where T : IStateMachine, new()
        {
            mainStateMachine = new T();
            mainStateMachine.Init(csAnimator, "Main", "", null);
        }

        public void SetControllerAsset(RuntimeAnimatorController asset) => controllerAsset = asset;


        // --------------------------------------------------------------------------------

        public virtual void Enter()
        {
            if (controllerAsset == null)
            {
                Debug.LogError(GetType().Name + "的ControllerAsset为空");
                return;
            }

            if (mainStateMachine == null)
            {
                Debug.LogError("MainStateMachine未注册 " + GetType().Name);
                return;
            }
            
            // 切换动画控制器
            csAnimator.GetAnimator().Rebind();
            csAnimator.GetAnimator().runtimeAnimatorController = controllerAsset;

            mainStateMachine.Enter();
        }

        public virtual void Exit()
        {
            mainStateMachine.Exit();
        }

        public virtual void Update()
        {
            mainStateMachine?.Update();
        }

        public void FixedUpdate()
        {
            mainStateMachine?.FixedUpdate();
        }

        // --------------------------------------------------------------------------------

        public void ChangeState<T>() where T : IState
        {
            mainStateMachine.ChangeState<T>();
        }

        public T EnterSubStateMachine<T>() where T : IStateMachine
        {
            return mainStateMachine.EnterSubStateMachine<T>();
        }

        public IStateMachine ExitCurrentSubStateMachine()
        {
            if (mainStateMachine.Sub != null)
            {
                mainStateMachine.Sub.ExitCurrentStateMachine();
            }
            return mainStateMachine;
        }

        public void ChangeState(string stateName)
        {
            mainStateMachine.ChangeState(stateName);
        }

        public void ChangeState(string subStateMachineName, string stateName)
        {
            mainStateMachine.ChangeState(subStateMachineName, stateName);
        }

        public void SetSign(string key, bool value) =>  mainStateMachine.SetSign(key,value);
        public bool GetSign(string key) => mainStateMachine.GetSign(key);

        // --------------------------------------------------------------------------------

        public T GetEntityLogic<T>() where T : EntityLogic
        {
            return csAnimator.GetEntityLogic<T>();
        }
    }
}