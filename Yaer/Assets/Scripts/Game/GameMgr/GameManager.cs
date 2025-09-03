using System;
using System.Collections.Generic;
using Game.GameMgr.Component.Archive.ArchiveDataClass.Player;
using Game.GameMgr.Component.Base;
using Game.GameMgr.Component.PureMVC;
using Game.GameMgr.Manager.Base;
using Game.GameMgr.Manager.Settings.Helper;
using Game.GameMgr.Manager.Settings;
using Game.GameRuntime.GameSceneManager.Base;
using Game.Static.Enum;
using Game.Static.Name.Settings;
using GameFramework.CoreExtend.Generic;
using GameFramework.UnityRuntime.Base;
using GameFramework.UnityRuntime.Utility;
using UnityEngine;
using Game.GameMgr.Component.UI;
using Game.Static.Path;

namespace Game.GameMgr
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public LanguageEnumType language { get; set; } = LanguageType.Chinese;
        private bool inConversation; // 是否在对话中
        private bool pause;          // 暂停标识
        private bool playerControl;  // 禁止所有交互
        private bool raycast;
        private ChangeSceneInfo nowChangeSceneInfo;
        private SaveSceneInfo saveSceneInfo;

        public bool IsPlaying { get; private set; }

        public bool Pause
        {
            get => pause;
            set
            {
                Debug.Log("Pause: " + value);
                pause = value;
                if (value)
                {
                    PlayerControl = false;
                    RayCast = false;
                }
                else
                {
                    if (!InConversation)
                    {
                        PlayerControl = true;
                        RayCast = true;
                    }
                }
            }
        }

        public bool InConversation
        {
            get => inConversation;
            set
            {
                Debug.Log("InConversation: " + value);
                inConversation = value;
                if (value)
                {
                    PlayerControl = false;
                    RayCast = false;
                }
                else
                {
                    if (!Pause)
                    {
                        PlayerControl = true;
                        RayCast = true;
                    }
                }
            }
        }

        public bool RayCast
        {
            get => raycast;
            set
            {
                Debug.Log("RayCast:" + value);
                raycast = value;
            }
        }

        public bool PlayerControl
        {
            get => playerControl;
            set
            {
                playerControl = value;
            }
        }

        public bool CanChangeScene { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        private void Update()
        {
            //Debug.Log("GameManager Update");
            UpdateComponents();
        }

        public void OnInit()
        {
            IsPlaying = false;
            pause = true;
            playerControl = false;
            CanChangeScene = true;
            inConversation = false;

            OnStartGameAction = new SortAction();
            OnSaveGameAction = new SortAction();
            OnExitGameAction = new SortAction();
            OnLoadGameAction = new SortAction();
            OnReturnToMenuAction = new SortAction();
            OnNewGameAction = new SortAction();
            OnSceneExitAction = new SortAction();

            RegisterManagers();
            RegisterComponents();

            InitComponents();
        }

        public void OnEnter()
        {
            OnEnterComponents();
        }


        // --------------------------------------------------------------------------------
        // Mvc
        public void SendNotification(string notificationName, object body = null)
        {
            GameFacade.SendNotification(notificationName, body);
        }

        #region 编辑器

        public void SetCanChangeScene(bool value)
        {
            CanChangeScene = value;
        }

        #endregion


        #region Manager

        private static IGameSceneManager gameSceneManager;
        public GameFacade GameFacade => GameFacade.Instance;

        // GameFramework

        #endregion


        #region Event

        public SortAction OnNewGameAction { get; private set; }
        public SortAction OnSaveGameAction { get; private set; }
        public SortAction OnExitGameAction { get; private set; }
        public SortAction OnLoadGameAction { get; private set; }
        public SortAction OnSceneExitAction { get; private set; }
        public SortAction OnStartGameAction { get; private set; }
        public SortAction OnReturnToMenuAction { get; private set; }

        #endregion


        #region GameSceneManager
        
        public event Action<IGameSceneManager> onGameSceneManagerReady;

        public T GetGameSceneManager<T>() where T : class, IGameSceneManager => GetGameSceneManager() as T;

        public static IGameSceneManager GetGameSceneManager()
        {
            if (gameSceneManager == null)
            {
                Debug.LogWarning("当前场景Manager为空");
                return null;
            }
            
            return gameSceneManager;
        }

        /// <summary>
        /// 场景Manager开始初始化
        /// </summary>
        public void OnGameSceneManagerReady(IGameSceneManager m)
        {
            gameSceneManager = m;
            onGameSceneManagerReady?.Invoke(gameSceneManager);
            onGameSceneManagerReady = null;
        }

        public void RemoveGameSceneManager()
        {
            gameSceneManager = null;
        }

        #endregion


        //-----------------------------------------------------------------------------------

        private static Dictionary<string, IManager> managers = new Dictionary<string, IManager>();
        private static Dictionary<string, IComponentGM> components = new Dictionary<string, IComponentGM>();

        private void InitComponents()
        {
            foreach (var cpn in components.Values)
            {
                cpn.OnInit();
            }
        }

        private void UpdateComponents()
        {
            foreach (var cpn in components.Values)
            {
                cpn.OnUpdate();
            }
        }

        private void RegisterComponents()
        {
            // 注册gamemanagr的component
            foreach (var component in GetComponentsInChildren<IComponentGM>())
            {
                components.Add(component.GetType().Name, component);
            }
        }

        private void RegisterManagers()
        {
            foreach (var manager in GetComponentsInChildren<IManager>())
            {
                RegisterManager(manager);
            }
        }

        private void RegisterManager(IManager manager)
        {
            var managerName = manager.GetType().Name;
            if (managers.ContainsKey(managerName))
            {
                Debug.LogError("该Manager已经注册过:" + manager.GetType().Name);
                return;
            }

            manager.Init();
            managers.Add(managerName, manager);
        }

        public static T GetManager<T>() where T : class, IManager
        {
            if (managers.TryGetValue(typeof(T).Name, out var manager))
            {
                return manager as T;
            }

            Debug.LogWarning("未注册该Manager:" + typeof(T).Name);
            return null;
        }

        public static T GetGMComponent<T>() where T : class, IComponentGM
        {
            if (components.TryGetValue(typeof(T).Name, out var component))
            {
                return component as T;
            }

            Debug.LogWarning("未注册" + typeof(T).Name);
            return null;
        }

        public static T GetGFComponent<T>() where T : GameFrameworkComponent
        {
            return GameEntry.GetComponent<T>(); // 针对 GameFrameworkComponent 的逻辑
        }

        public static void OnEnterComponents()
        {
            foreach (var component in components.Values)
            {
                component.OnEnter();
            }
        }

        public static void OnExitComponents()
        {
            foreach (var component in components.Values)
            {
                component.OnExit();
            }
        }

        // 获取某个指令输入对应的字符串
        public static string GetKeyStrByInputType(ControlInputType inputType)
        {
            var configData = GetManager<SettingManager>().LoadSetting<SettingsConfigData>();
            string touchKey = "";
            if (configData.KeyboardMouseInputConfig.ContainsKey(inputType))
            {
                var interactKey = configData.KeyboardMouseInputConfig[inputType];
                touchKey = KeyCodeStrConfig.GetKeyString(interactKey);
            }
            return touchKey;
        }
        // 获取某个指令对应的按键类型
        public static KeyCode GetKeyCodeByInputType(ControlInputType inputType)
        {
            var configData = GetManager<SettingManager>().LoadSetting<SettingsConfigData>();
            if (configData.KeyboardMouseInputConfig.ContainsKey(inputType))
            {
                var interactKey = configData.KeyboardMouseInputConfig[inputType];
                return interactKey;
            }
            return KeyCode.None;
        }

        // 获取当前语言对应的资源文件后缀
        public static string GetCurLanguageResTag()
        {
            return LanguageType.GetLanaguageResTag(Instance.language);
        }

        public static void ShowUnOpenTipsPanel()
        {
            string uiPrefabPath = UIPrefabPath.GetUIPrefabPath("UnOpenTipsPanel");
            var uiForm = GetGMComponent<UIComponentGM>().GetUIForm(uiPrefabPath);
            if (uiForm == null) {
                GetGMComponent<UIComponentGM>().OpenUIForm(uiPrefabPath, EUIGroup.Top, new OpenFormArgs()
                {

                });
            }
        }
    }
}