using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.GameRuntime.UI.Component
{
    public class UIStateMachine : MonoBehaviour
    {
        [SerializeField] private string initStateName;
        [SerializeField] private string currentStateName;
        [SerializeField] private List<string> stateNames = new List<string>();
        [SerializeField] private List<GameObject> objList = new List<GameObject>();
        [SerializeField] private GameObject currentObj;

        public string InitStateName => initStateName;
        public string CurrentStateName => currentStateName;

        private void OnValidate()
        {
            // 切换了对象
            if (currentObj == null || currentObj != gameObject)
            {
                objList = new List<GameObject>();
                currentObj = gameObject;
            }
            
            ChangeTo(initStateName);
        }

        private void Start()
        {
            ChangeTo(initStateName);
        }

        public void SetDefaultState(string stateName)
        {
            initStateName = stateName;
        }

        public void ChangeTo(string stateName)
        {
            if (stateNames.Contains(stateName))
            {
                foreach (var obj in objList)
                {
                    var script = obj.GetComponent<UIState>();
                    if (script != null)
                        script.Enter(stateName);
                    else

                        // 移除失效的
                        objList.Remove(obj);
                }

                currentStateName = stateName;
            }
            else
            {
                Debug.LogError("没有这个状态:" + stateName, gameObject);
            }
        }

        public void RegisterState(string name)
        {
            if (stateNames.Contains(name)) return;

            stateNames.Add(name);
        }

        public void AddState(GameObject obj)
        {
            if (objList.Contains(obj) == false) objList.Add(obj);
        }

        public void RemoveState(string name)
        {
            if (!stateNames.Contains(name)) return;

            stateNames.Remove(name);
        }

        public void RefreshStateNames()
        {
        }

        public List<string> GetStateNames()
        {
            return stateNames;
        }
    }

    [Serializable]
    public class UIStateMachineData
    {
        public string stateName;
        public List<UIState> states = new List<UIState>();

        public void AddState(UIState state)
        {
            if (states.Contains(state) == false) states.Add(state);
        }

        public void RemoveState(UIState state)
        {
            if (states.Contains(state)) states.Remove(state);
        }
    }
}