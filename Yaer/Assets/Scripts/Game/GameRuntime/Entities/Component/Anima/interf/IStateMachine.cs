using GameFramework.UnityRuntime.Entity;
using UnityEngine;

namespace Game.GameRuntime.Entities.Component.Anima.interf
{
    public interface IStateMachine
    {
        string Name { get; }
        ICsAnimator CsAnimator { get; }
        IStateMachine Sub { get; set; }
        AnimatorStateInfo StateInfo { get; }
        void Init(ICsAnimator csAnimator, string name, string enterArgs, IStateMachine parent);
        void Enter();
        void Exit();
        void Update();
        void FixedUpdate();
        void RegisterState<T>(string argsName, string stateName) where T : IState, new();
        void RegisterState(IState state);
        void RegisterSubStateMachine<T>(string name, string enterArgs) where T : IStateMachine, new();
        void RegisterSubStateMachine(IStateMachine subStateMachine);
        void ChangeState<T>() where T : IState;
        void ChangeState<T, TK>() where T : IStateMachine where TK : IState;
        void ChangeState(string stateName);
        void ChangeState(string subStateMachine, string stateName);
        T EnterSubStateMachine<T>() where T : IStateMachine;
        IStateMachine EnterSubStateMachine(string smName);

        /// <summary>
        ///     退出当前状态机
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>父状态机</returns>
        T ExitCurrentStateMachine<T>() where T : IStateMachine;

        IStateMachine ExitCurrentStateMachine();
        bool IsState<T>() where T : IState;
        bool GetSign(string key);
        void SetSign(string key, bool value);

        // --------------------------------------------------------------------------------
        // animator
        void SetBool(string name, bool value);
        void SetTrigger(string name);
        void SetFloat(string name, float value);
        void SetInt(string name, int value);
        void SetSpeed(float value);

        //-----------------------------------------------------------------------------------
        // entity
        T GetEntityLogic<T>() where T : EntityLogic;
    }
}