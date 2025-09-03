//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using System;
using System.Collections.Generic;
using GameFramework.UnityRuntime.Utility;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameFramework.UnityRuntime.Base
{
    /// <summary>
    /// 游戏入口。
    /// </summary>
    public static class GameEntry
    {
        private static readonly GameFrameworkLinkedList<GameFrameworkComponent> s_GameFrameworkComponents = new GameFrameworkLinkedList<GameFrameworkComponent>();

        /// <summary>
        /// 游戏框架所在的场景编号。
        /// </summary>
        internal const int GameFrameworkSceneId = 0;

        /// <summary>
        /// 获取游戏框架组件。
        /// </summary>
        /// <typeparam name="T">要获取的游戏框架组件类型。</typeparam>
        /// <returns>要获取的游戏框架组件。</returns>
        public static T GetComponent<T>() where T : GameFrameworkComponent
        {
            return (T)GetComponent(typeof(T));
        }

        /// <summary>
        /// 获取游戏框架组件。
        /// </summary>
        /// <param name="type">要获取的游戏框架组件类型。</param>
        /// <returns>要获取的游戏框架组件。</returns>
        public static GameFrameworkComponent GetComponent(Type type)
        {
            LinkedListNode<GameFrameworkComponent> current = s_GameFrameworkComponents.First;
            while (current != null)
            {
                if (current.Value.GetType() == type)
                {
                    return current.Value;
                }

                current = current.Next;
            }

            return null;
        }

        /// <summary>
        /// 获取游戏框架组件。
        /// </summary>
        /// <param name="typeName">要获取的游戏框架组件类型名称。</param>
        /// <returns>要获取的游戏框架组件。</returns>
        public static GameFrameworkComponent GetComponent(string typeName)
        {
            LinkedListNode<GameFrameworkComponent> current = s_GameFrameworkComponents.First;
            while (current != null)
            {
                Type type = current.Value.GetType();
                if (type.FullName == typeName || type.Name == typeName)
                {
                    return current.Value;
                }

                current = current.Next;
            }

            return null;
        }

        /// <summary>
        /// 关闭游戏框架。
        /// </summary>
        /// <param name="shutdownType">关闭游戏框架类型。</param>
        public static void Shutdown(ShutdownType shutdownType)
        {
            Log.Info("Shutdown Game Framework ({0})...", shutdownType.ToString());
            BaseComponent baseComponent = GetComponent<BaseComponent>();
            if (baseComponent != null)
            {
                baseComponent.Shutdown();
                baseComponent = null;
            }

            s_GameFrameworkComponents.Clear();

            if (shutdownType == ShutdownType.None)
            {
                return;
            }

            if (shutdownType == ShutdownType.Restart)
            {
                SceneManager.LoadScene(GameFrameworkSceneId);
                return;
            }

            if (shutdownType == ShutdownType.Quit)
            {
                Application.Quit();
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#endif
                return;
            }
        }

        /// <summary>
        /// 注册游戏框架组件。
        /// </summary>
        /// <param name="gameFrameworkComponent">要注册的游戏框架组件。</param>
        internal static void RegisterComponent(GameFrameworkComponent gameFrameworkComponent)
        {
            // 检查游戏框架组件是否为null，因为后续操作需要该对象
            if (gameFrameworkComponent == null)
            {
                // 若为null，则记录错误日志并退出当前方法
                Log.Error("Game Framework component is invalid.");
                return;
            }

            // 获取当前游戏框架组件的类型，用于后续的类型比较
            Type type = gameFrameworkComponent.GetType();

            // 遍历已存在的游戏框架组件链表，检查是否有相同类型的组件已经存在
            LinkedListNode<GameFrameworkComponent> current = s_GameFrameworkComponents.First;
            while (current != null)
            {
                // 如果发现相同类型的组件已经存在，则记录错误日志并退出方法
                if (current.Value.GetType() == type)
                {
                    Log.Error("Game Framework component type '{0}' is already exist.", type.FullName);
                    return;
                }

                // 移动到链表的下一个节点，继续检查
                current = current.Next;
            }

            // 如果没有找到相同类型的组件，则将当前游戏框架组件添加到链表中
            s_GameFrameworkComponents.AddLast(gameFrameworkComponent);

        }
    }
}
