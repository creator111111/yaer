using UnityEngine;

namespace Game.GameRuntime.Entities.Component.Anima.interf
{
    public interface ICsRuntimeController
    {
        void Init(ICsAnimator csAnimator);
        void SetControllerAsset(RuntimeAnimatorController asset);
        void Enter();
        void Exit();
        void Update();
        void FixedUpdate();
        void ChangeState<T>() where T : IState;
        T EnterSubStateMachine<T>() where T : IStateMachine;
        IStateMachine ExitCurrentSubStateMachine();
        void ChangeState(string stateName);
        void ChangeState(string subStateMachineName, string stateName);
        void SetSign(string key, bool value);
        bool GetSign(string key);
    }
}