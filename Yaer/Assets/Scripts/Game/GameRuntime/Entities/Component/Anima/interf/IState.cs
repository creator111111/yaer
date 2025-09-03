using UnityEngine;

namespace Game.GameRuntime.Entities.Component.Anima.interf
{
    public interface IState
    {
        AnimatorStateInfo StateInfo { get; }
        bool InAnimation { get; }
        bool IsFinished { get; }
        bool IsEnter { get; }
        bool IsExit { get; }
        string StateName { get; }
        void Init(IStateMachine stateMachine, string argsName, string stateName);
        void Update();
        void FixedUpdate();
        void Enter();
        void SetAnimatorEnter();
        void Exit();

    }
}