using System;
using Game.GameMgr;
using Game.GameMgr.Component;
using GameFramework.UnityRuntimeExtend.Base;
using UnityEngine;

namespace Game.Entry
{
    /// <summary>
    ///     游戏启动入口
    /// </summary>
    public partial class GameRuntimeEntry : MonoBehaviour
    {
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        public void Start()
        {
            InitBuiltinComponents();
            InitCustomComponents();
        }
    }
}