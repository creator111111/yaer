using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameFramework.CoreExtend.Generic
{
    /// <summary>
    /// 框架封装含有优先级的Action
    /// </summary>
    public class SortAction
    {
        private readonly List<ActionInfo> actions;

        /// <summary>
        /// 所有委托的数量
        /// </summary>
        public int Count => actions.Count;

        /// <summary>
        /// 当前最高优先级
        /// </summary>
        public int HighestPriority => actions.Count > 0 ? actions[actions.Count - 1].Priority : 0;

        /// <summary>
        /// 当前最低优先级
        /// </summary>
        public int LowestPriority => actions.Count > 0 ? actions[0].Priority : 0;

        public SortAction()
        {
            actions = new List<ActionInfo>();
        }

        public SortAction(int priority, Action action)
        {
            actions = new List<ActionInfo>();
            actions.Add(new ActionInfo(priority, action));
        }
        
        public SortAction(Action action)
        {
            actions = new List<ActionInfo>();
            actions.Add(new ActionInfo(0, action));
        }

        public void Add(Action action)
        {
            // 默认优先级为0
            Add(0, action);
        }

        public void Add(int priority, Action action)
        {
            if (action == null)
            {
                Debug.LogError("action is null");
                return;
            }

            actions.Add(new ActionInfo(priority, action));
            // 排序
            actions.Sort((x, y) => x.Priority.CompareTo(y.Priority));
        }
        
        /// <summary>
        /// 添加到最低优先级
        /// </summary>
        /// <param name="action"></param>
        public void AddLowestPriority(Action action)
        {
            Add(LowestPriority - 1, action);
        }
        
        /// <summary>
        /// 添加到最高优先级
        /// </summary>
        /// <param name="action"></param>
        public void AddHighestPriority(Action action)
        {
            Add(HighestPriority + 1, action);
        }

        public void Clear()
        {
            actions.Clear();
        }

        public void Invoke()
        {
            foreach (var action in actions)
            {
                action.Invoke();
            }
        }

        private readonly struct ActionInfo
        {
            readonly Action action;
            readonly int priority;

            public int Priority => priority;

            public ActionInfo(int priority, Action action)
            {
                this.action = action;
                this.priority = priority;
            }

            public void Invoke() => action?.Invoke();
        }
    }
}