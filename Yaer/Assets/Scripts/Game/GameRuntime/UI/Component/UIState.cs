using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.GameRuntime.UI.Component
{
    [Serializable]
    public class UIState : MonoBehaviour
    {
        public bool isInit;
        public string currentStateName;

        public UIStateMachine stateMachine;
        public List<UIStateData> dataList = new List<UIStateData>();

        private void OnValidate()
        {
            if (GetComponents<UIState>().Length > 1)
            {
                Debug.LogError("不能重复添加UIState", gameObject);
                return;
            }

            if (stateMachine == null)
            {
                stateMachine = transform.parent.GetComponent<UIStateMachine>();

                if (stateMachine == null)
                {
                    Debug.LogWarning("需要绑定UIStateMachine");
                }
            }

            Init();
        }

        private void OnEnable()
        {
            Init();
        }

        public void Init()
        {
            if (stateMachine)
            {
                // 初始化绑定状态机
                stateMachine.AddState(gameObject);

                // 初始化数据
                foreach (var stateName in stateMachine.GetStateNames())
                {
                    var data = dataList.Find(x => x.stateName == stateName);
                    if (data == null)
                    {
                        var rTsf = transform as RectTransform;
                        dataList.Add(new UIStateData
                        {
                            stateName = stateName,
                            isActive = true,
                            position = rTsf.anchoredPosition,
                            size = rTsf.sizeDelta,
                            rotation = rTsf.rotation.eulerAngles,
                            scale = rTsf.localScale
                        });
                    }
                }


                // 默认进入初始状态
                if (stateMachine.GetStateNames().Count > 0 && string.IsNullOrEmpty(currentStateName)) Enter(stateMachine.GetStateNames()[0]);

                isInit = true;
            }
        }

        public virtual void Enter(string stateName)
        {
            var data = dataList.Find(d => d.stateName == stateName);
            if (data != null)
            {
                if (data.controlActive) gameObject.SetActive(data.isActive);

                if (data.controlTsf)
                {
                    var rTsf = transform as RectTransform;
                    rTsf.anchoredPosition = data.position;
                    rTsf.sizeDelta = data.size;
                    rTsf.rotation = Quaternion.Euler(data.rotation);
                    rTsf.localScale = data.scale;
                }
            }

            currentStateName = stateName;
        }

        public virtual void Exit()
        {
        }
    }

    [Serializable]
    public class UIStateData
    {
        public string stateName;

        public bool controlActive;
        public bool controlTsf;
        public bool controlCanvasGroup;

        public bool isActive;

        public Vector3 position;
        public Vector2 size;
        public Vector3 rotation;
        public Vector3 scale;

        public float alpha;
    }
}