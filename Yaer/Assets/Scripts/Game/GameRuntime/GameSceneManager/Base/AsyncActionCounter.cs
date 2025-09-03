using GameFramework.UnityRuntime.Utility;
using UnityEngine;

namespace Game.GameRuntime.GameSceneManager.Base
{
    /// <summary>
    /// 异步方法计数器
    /// </summary>
    public class AsyncActionCounter
    {
        private int count = 0;

        public bool IsDone => count <= 0;

        // 调用此方法时表示新增一个异步任务
        public void Add() => count++;

        // 调用此方法表示一个任务完成
        public void Done() => count = Mathf.Max(0, count - 1);

        // 重置计数器
        public void Start() => count = 0;
    }
}