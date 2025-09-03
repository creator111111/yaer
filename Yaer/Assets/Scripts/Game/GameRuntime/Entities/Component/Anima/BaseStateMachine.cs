using System.Collections.Generic;
using Game.GameRuntime.Entities.Component.Anima.interf;
using GameFramework.UnityRuntime.Entity;
using UnityEngine;

namespace Game.GameRuntime.Entities.Component.Anima
{
    public class BaseStateMachine : IStateMachine
    {
        private float failTime = 1f; // 切换超时
        private ICsAnimator csAnimator;
        private IState currentState;
        private IStateMachine sub;
        private IStateMachine parent;
        private List<IState> states = new List<IState>();
        private Dictionary<string, bool> signDic = new Dictionary<string, bool>();
        private List<IStateMachine> subStateMachines = new List<IStateMachine>();

        private string name;
        private string enterArgs;

        public string Name => name;
        public ICsAnimator CsAnimator => csAnimator;

        public IStateMachine Sub
        {
            get => sub;
            set => sub = value;
        }

        public AnimatorStateInfo StateInfo => csAnimator.GetCurrentAnimatorStateInfo(0);

        private protected BaseStateMachine()
        {
        }

        // --------------------------------------------------------------------------------

        public virtual void Init(ICsAnimator csAnimator, string name, string enterArgs, IStateMachine parent)
        {
            this.name = name;
            this.enterArgs = enterArgs;
            this.parent = parent;
            this.csAnimator = csAnimator;

            if (this.csAnimator == null)
            {
                Debug.LogError("animator为空 " + GetType().Name);
            }
        }


        // --------------------------------------------------------------------------------


        public virtual void Enter()
        {
            if (string.IsNullOrEmpty(enterArgs) == false && parent != null) csAnimator.SetBool(enterArgs, true);
        }

        public virtual void Exit()
        {
            sub?.Exit();

            // 退出当前状态
            currentState?.Exit();
            currentState = null;

            // 子状态机才能 退出 parent != null
            if (enterArgs != null && parent != null) csAnimator.SetBool(enterArgs, false);
        }

        public void RegisterState<T>(string argsName, string stateName) where T : IState, new()
        {
            var s = new T();
            s.Init(this, argsName, stateName);
            RegisterState(s);
        }

        public void RegisterState(IState state)
        {
            if (states.Contains(state) == false) states.Add(state);
        }

        public void RegisterSubStateMachine<T>(string name, string enterArgs) where T : IStateMachine, new()
        {
            var sm = new T();
            sm.Init(csAnimator, name, enterArgs, this);
            RegisterSubStateMachine(sm);
        }

        public void RegisterSubStateMachine(IStateMachine stateMachine)
        {
            if (subStateMachines.Contains(stateMachine) == false) subStateMachines.Add(stateMachine);
        }

        public virtual void Update()
        {
            if (sub != null)
            {
                // 走子状态机Update
                sub.Update();
            }
            else
            {
                if (currentState != null)
                {
                    if (StateInfo.IsName(currentState.StateName))
                    {
                        // 真正进入
                        if (currentState.IsEnter == false)
                        {
                            currentState?.Enter();
                        }
                        
                        //Debug.Log(csAnimator.GetEntityLogic<EntityLogic>().gameObject.name + "当前状态:" + currentState?.StateName);
                        failTime = 1f;

                        currentState?.Update();
                    }
                    else
                    {
                        failTime -= Time.deltaTime;
                        if (failTime <= 0) Debug.LogWarning("状态切换失败: " + currentState?.StateName);
                    }
                }
            }
        }

        public void FixedUpdate()
        {
            if (sub != null)
            {
                sub.FixedUpdate();
            }
            else
            {
                if (currentState != null && StateInfo.IsName(currentState.StateName) && currentState.IsExit == false) currentState.FixedUpdate();
            }
        }

        // --------------------------------------------------------------------------------

        public void ChangeState(string subStateMachine, string stateName)
        {
            var s = subStateMachines.Find(x => x.Name == subStateMachine);
            if (s != null)

                // 找到进入子状态机
                EnterSubStateMachine(subStateMachine).ChangeState(stateName);
            else
                Debug.LogError("子状态机未注册: " + subStateMachine);
        }

        public void ChangeState<T, TK>() where T : IStateMachine where TK : IState
        {
            var s = subStateMachines.Find(x => x.GetType() == typeof(T));
            if (s != null)

                // 找到进入子状态机
                EnterSubStateMachine<T>().ChangeState<TK>();
            else
                Debug.LogError("子状态机状态未注册: " + typeof(T).Name);
        }

        public void ChangeState(string stateName)
        {
            if (string.IsNullOrEmpty(stateName))
            {
                Debug.LogError("状态名为空");
                return;
            }

            ApplyChange(states.Find(x => x.StateName == stateName));
        }

        public virtual void ChangeState<T>() where T : IState
        {
            var s = states.Find(x => x.GetType() == typeof(T));

            if (s == null)
            {
                Debug.LogError(GetType().Name + "状态未注册: " + typeof(T).Name);
                return;
            }

            ApplyChange(s);
        }

        public T EnterSubStateMachine<T>() where T : IStateMachine
        {
            var s = subStateMachines.Find(x => x.GetType() == typeof(T));

            if (s == null)
            {
                Debug.LogError("子状态机未注册: " + typeof(T).Name);
                return default;
            }

            if (sub != null && sub is T == false)
            {
                Debug.LogError("子状态机未退出");
                return default;
            }

            // 退出当前状态
            if (currentState != null)
            {
                currentState.Exit();
                currentState = null;
            }

            sub = s;
            sub.Enter();

            return (T)s;
        }

        public IStateMachine EnterSubStateMachine(string smName)
        {
            var s = subStateMachines.Find(x => x.Name == smName);

            if (s == null)
            {
                Debug.LogError("子状态机未注册: " + smName);
                return null;
            }

            if (sub != null && sub.GetType() != s.GetType())
            {
                Debug.LogError("子状态机未退出");
                return null;
            }

            // 退出当前状态
            if (currentState != null)
            {
                currentState.Exit();
                currentState = null;
            }

            sub = s;
            sub.Enter();

            return s;
        }

        public T ExitCurrentStateMachine<T>() where T : IStateMachine
        {
            if (parent == null)
            {
                Debug.LogError("该父状态机未注册:" + typeof(T).Name);
                return default;
            }

            // 如果父状态机类型正确
            if (parent is T)
            {
                // 退出当前状态
                if (currentState != null)
                {
                    currentState.Exit();
                    currentState = null;
                }

                // 退出当前状态机
                Exit();
                parent.Sub = null;
                return (T)parent;
            }

            Debug.LogError("当前状态机的父状态机不是该类型" + typeof(T).Name);
            return default;
        }

        public IStateMachine ExitCurrentStateMachine()
        {
            if (parent == null)
            {
                Debug.LogError("没有父状态机");
                return this;
            }

            // 退出当前状态
            if (currentState != null)
            {
                currentState.Exit();
                currentState = null;
            }

            Exit();
            parent.Sub = null;
            return parent;
        }

        // --------------------------------------------------------------------------------
        // 状态判断
        public bool IsState<T>() where T : IState
        {
            if (currentState.GetType() == typeof(T)) return true;
            return false;
        }

        public void SetSign(string key, bool value)
        {
            // 设置在父状态机
            if (parent != null)
            {
                parent.SetSign(key, value);
            }
            
            signDic[key] = value;
        }

        public bool GetSign(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogError("key为空");
                return false;
            }

            if (parent != null) return parent.GetSign(key);

            if (signDic.TryGetValue(key, out var sign)) return sign;
            
            return false;
        }

        public void ApplyChange(IState s)
        {
            if (currentState == s) return;

            if (currentState == null)
            {
                currentState = s;
                currentState.SetAnimatorEnter();
            }
            else
            {
                currentState.Exit();
                currentState = s;
                currentState.SetAnimatorEnter();
            }
        }

        public void RemoveSign(string key)
        {
            if (signDic.ContainsKey(key)) signDic.Remove(key);
        }

        // --------------------------------------------------------------------------------
        // animator
        public void SetBool(string name, bool value) => csAnimator.SetBool(name, value);

        public void SetTrigger(string name) => csAnimator.SetTrigger(name);

        public void SetFloat(string name, float value) => csAnimator.SetFloat(name, value);

        public void SetInt(string name, int value) => csAnimator.SetInt(name, value);

        public void SetSpeed(float speed) => csAnimator.SetSpeed(speed);
        public T GetEntityLogic<T>() where T : EntityLogic => csAnimator.GetEntityLogic<T>();
    }
}