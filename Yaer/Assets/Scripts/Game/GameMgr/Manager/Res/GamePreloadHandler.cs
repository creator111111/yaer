using System;
using System.Collections.Generic;

namespace Game.GameMgr.Manager.Res
{
    public class GamePreloadHandler : IGamePreloadHandler
    {
        private int count;
        private bool lastActionRun;
        private bool loading;
        private Action<GamePreloadHandler> onLastAction;
        private Action<GamePreloadHandler> onLoadingAction;
        private readonly Queue<Action<GamePreloadHandler>> queue = new Queue<Action<GamePreloadHandler>>();
        private int totalCount;
        public Action OnComplete { get; set; }

        public void StartLoad()
        {
            if (queue.Count > 0)
            {
                lastActionRun = false;
                queue.Dequeue()?.Invoke(this);
            }
        }

        public void AddLoadingAction(Action<IGamePreloadHandler> action)
        {
            queue.Enqueue(action);
        }

        public void AddLastAction(Action<IGamePreloadHandler> action)
        {
            onLastAction += action;
        }

        public void Done()
        {
            if (queue.Count <= 0)
            {
                // 最后一个Action
                if (onLastAction != null && !lastActionRun)
                {
                    lastActionRun = true;
                    onLastAction?.Invoke(this);
                    return;
                }

                OnComplete?.Invoke();
            }
            else
            {
                // 继续执行
                queue.Dequeue()?.Invoke(this);
            }
        }
    }
}