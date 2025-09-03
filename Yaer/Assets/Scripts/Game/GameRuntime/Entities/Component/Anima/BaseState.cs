using Game.GameRuntime.Entities.Base.BaseSceneObj.Base;
using Game.GameRuntime.Entities.Component.Anima.interf;
using UnityEngine;

namespace Game.GameRuntime.Entities.Component.Anima
{
    public abstract class BaseState : IState
    {
        private string argsName;
        private string stateName;
        protected IStateMachine stateMachine;
        private IStateObject stateObject;
        
        protected float NormalizedTime => stateMachine.StateInfo.normalizedTime;
        public bool InAnimation => stateMachine.StateInfo.IsName(stateName);
        public bool IsEnter { get; private set; }
        public bool IsExit { get; private set; }
        public string StateName => stateName;

        public bool IsFinished
        {
            get
            {
                if (StateInfo.loop) Debug.LogWarning(stateName + "是循环动画IsFinished无法判断是否结束?");

                return stateMachine.StateInfo.IsName(stateName) && stateMachine.StateInfo.normalizedTime >= 1;
            }
        }

        public AnimatorStateInfo StateInfo => stateMachine.StateInfo;


        public virtual void Init(IStateMachine stateMachine, string argsName, string stateName)
        {
            this.stateMachine = stateMachine;
            this.argsName = argsName;
            this.stateName = stateName;
        }

        public virtual void Update()
        {
        }

        public virtual void FixedUpdate()
        {
        }

        /// <summary>
        /// 设置状态机进入
        /// </summary>
        public void SetAnimatorEnter()
        {
            if (argsName != null) stateMachine.SetBool(argsName, true);
            stateMachine.SetSpeed(1);
        }
        
        /// <summary>
        /// 设置状态机退出
        /// </summary>
        public void SetAnimatorExit()
        {
            if (argsName != null) stateMachine.SetBool(argsName, false);
            stateMachine.SetSpeed(1);
        }

        /// <summary>
        ///  代码进入
        /// </summary>
        public virtual void Enter()
        {
            IsEnter = true;
            IsExit = false;
        }

        /// <summary>
        /// 代码退出
        /// </summary>
        public virtual void Exit()
        {
            IsEnter = false;
            IsExit = true;
            SetAnimatorExit();
        }

        protected virtual void ChangeState<T>() where T : IState
        {
            stateMachine.ChangeState<T>();
        }

        protected virtual T EnterSubStateMachine<T>() where T : IStateMachine
        {
            return stateMachine.EnterSubStateMachine<T>();
        }

        protected T ExitCurrentStateMachine<T>() where T : IStateMachine
        {
            return stateMachine.ExitCurrentStateMachine<T>();
        }

        protected IStateMachine ExitCurrentStateMachine()
        {
            return stateMachine.ExitCurrentStateMachine();
        }

        protected void FinishedChangeState<T>() where T : IState
        {
            if (IsFinished) ChangeState<T>();
        }
        
        protected void SetSign(string key, bool value) => stateMachine.SetSign(key, value);

        protected bool GetSign(string key) => stateMachine.GetSign(key);
    }
}