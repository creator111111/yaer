using GameFramework.UnityRuntime.Entity;
using UnityEngine;

namespace Game.GameRuntime.Entities.Component.Anima.interf
{
    public interface ICsAnimator
    {
        void RegisterRuntimeController(ICsRuntimeController csRuntimeController);
        void ChangeRuntimeController<T>() where T : ICsRuntimeController;
        void ChangeState<T>() where T : IState;
        bool IsState<T>() where T : IState;
        T GetEntityLogic<T>() where T : EntityLogic;
        Animator GetAnimator();

        //-----------------------------------------------------------------------------------

        void SetBool(string name, bool value);
        void SetTrigger(string name);
        void SetFloat(string name, float value);
        void SetInt(string name, int value);
        void SetSpeed(float speed);
        AnimatorStateInfo GetCurrentAnimatorStateInfo(int i);
    }
}