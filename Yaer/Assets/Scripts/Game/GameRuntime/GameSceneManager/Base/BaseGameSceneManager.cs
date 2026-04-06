using System;
using System.Collections;
using System.Collections.Generic;
using Game.GameMgr;
using Game.GameMgr.Component;
using Game.GameMgr.Component.Archive;
using Game.GameMgr.Component.Archive.ArchiveDataClass.BaseDataClass;
using Game.GameMgr.Component.Archive.ArchiveDataClass.Date;
using Game.GameMgr.Component.Archive.ArchiveDataClass.Player;
using Game.GameMgr.Component.ChangeScene;
using Game.GameMgr.Component.PureMVC;
using Game.GameMgr.Component.UI;
using Game.GameMgr.Manager.Effect;
using Game.GameRuntime.Entities.Base;
using Game.GameRuntime.Entities.Base.BaseSceneObj;
using Game.GameRuntime.Entities.Component.Effect;
using Game.GameRuntime.Entities.Component.Interactive;
using Game.GameRuntime.Entities.Monster;
using Game.GameRuntime.Entities.Player;
using Game.GameRuntime.Entities.Player.Components.CsAnimator;
using Game.GameRuntime.GameSceneManager.Component;
using Game.GameRuntime.GameSceneManager.Component.CameraGSM;
using Game.GameRuntime.GameSceneManager.Component.Story;
using Game.GameRuntime.GameSceneManager.Config;
using Game.GameRuntime.GameSceneManager.SubManager;
using Game.GameRuntime.UI.FormLogic;
using Game.GameRuntime.UI.FormLogic.Tips;
using Game.Static.Path;
using GameFramework.UnityRuntime.Entity;
using GameFramework.UnityRuntime.UI;
using GameFramework.UnityRuntime.Utility;
using NodeCanvas.Tasks.Conditions;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum TerrainType
{
    IndoorType, // 室内
    GlassType, // 草地
    LandType, // 陆地
}

namespace Game.GameRuntime.GameSceneManager.Base
{
    /// <summary>
    ///     场景管理器基类
    /// </summary>
    public partial class BaseGameSceneManager : MonoBehaviour, IGameSceneManager
    {
        // config
        [SerializeField] private GameSceneManagerConfig config;
        public GameSceneManagerConfig Config => config;
        // 状态
        private bool inited; // 初始化完成标识
        private bool allowControl;
        public bool Pause => GameManager.Instance.Pause;
        private Coroutine waitForInvokeCoroutine;
        protected string nowSceneName;

        private AsyncActionCounter initAsyncCounter = new AsyncActionCounter();
        private List<string> sceneEventList = new List<string>();
        private List<ISubSceneManager> subMgrList = new List<ISubSceneManager>();
        private List<IComponentGSM> moduleList = new List<IComponentGSM>();
        private bool allSceneObjIsPause;// 游戏是否暂停，暂停只影响所有场景中的怪物和人物能否移动
        private bool allSceneObjAniIsPause; // 场景对象是否暂停动画
        public GameObject curStoryPrefab = null; // 当前添加到场景中的对话事件预制体
        // 每个场景怪物Sprite的排序数据,每生成一个怪物计数就增加
        public Dictionary<GroundType, int> monsterAniSortData = new Dictionary<GroundType, int>() {
            { GroundType.Center, 0 }, { GroundType.Up, 0 }, { GroundType.Down, 0 }
        };
        // 每个场景怪物死亡后尸体的排序计数
        public int deadMonsterSpriteSort = 0;// 每死亡一个怪物会向下减少

        public bool canShowSaveGame = true; // 是否能存档
        public bool canShowLoadGame = true; // 是否能读档
        public bool canShowItemBag = true; // 是否能使用道具
        public bool isCanTouchWithOther = true; // 是否能与其他场景交互
        [Serializable]
        public class EnterPos
        {
            public string lastScene;
            [SerializeField]
            public Transform pos;
            public Vector3Int DatePass;
        }

        [SerializeField]
        public List<EnterPos> EnterPosConfig;

        // --------------------------------------------------------------------------------


        public T GetSubManager<T>() where T : class, ISubSceneManager
        {
            return subMgrList.Find(mgr => mgr is T) as T;
        }

        public T GetModule<T>() where T : class, IComponentGSM
        {
            var module = moduleList.Find(mgr => mgr is T) as T;
            if (module == null)
            {
                Log.Error("{0}未找到", typeof(T).Name, gameObject);
                return null;
            }

            return module;
        }

        protected void AddModule<T>() where T : MonoBehaviour, IComponentGSM
        {
            var cpn = transform.GetComponentInChildren<T>();
            if (cpn == null)
            {
                var obj = new GameObject(typeof(T).Name);
                obj.transform.parent = transform;
                obj.transform.localPosition = Vector3.zero;
                cpn = obj.AddComponent<T>();
            }

            moduleList.Add(cpn);
        }

        private void InitModules()
        {
            foreach (var cpn in moduleList)
            {
                cpn.OnInit(this);
            }
        }

        private void ShutdownModules()
        {
            foreach (var cpn in moduleList)
            {
                cpn.OnShutdown();
            }

            moduleList.Clear();
        }

        // --------------------------------------------------------------------------------


        #region Unity回调

        protected virtual void OnValidate()
        {
            if (config == null) Debug.LogWarning(GetType().Name + "没有找到Config, 可能需要手动挂载", gameObject);
        }

        private void Awake()
        {
            inited = false;
            initAsyncCounter.Start();

            OnInit();
        }

        private void Update()
        {
            if (initAsyncCounter.IsDone && !inited)
            {
                GameManager.Instance.OnGameSceneManagerReady(this);
                inited = true;
            }

            if (inited)
            {
                OnUpdate();
            }
#if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.F1))
            {
                string uiPrefabPath = UIPrefabPath.GetUIPrefabPath("AA_TestPanel");
                var uiForm = GameManager.GetGMComponent<UIComponentGM>().GetUIForm(uiPrefabPath);
                if (uiForm == null)
                {
                    GameManager.GetGMComponent<UIComponentGM>().OpenUIForm(uiPrefabPath, EUIGroup.Top, new OpenFormArgs()
                    {

                    });
                }
            }
#endif
        }

        #endregion


        protected T GetSceneEntityLogic<T>() where T : BaseSceneEntityLogic => GetModule<SceneEntityComponentGSM>().GetSceneEntityLogic<T>();


        #region SceneManager回调

        public virtual void OnInit()
        {
            // 初始化模块
            OnInitAddModules();
            InitModules();
            
            // 注册Proxy
            OnInitRegisterProxy();

            // 初始化玩家
            InitPlayer();
            
            // 监听切换场景结束黑幕事件
            GameManager.GetGMComponent<ProcedureComponentGM>().onCompleteLoadingSceneEvent += OnEnterScene;
            GameManager.GetGMComponent<ProcedureComponentGM>().onInitAllSceneMonsterEvent += initAllSceneMonster;
            GetModule<LoadSceneComponentGSM>().onEndLoadingSceneEvent += OnEnterScene;
            GetModule<LoadSceneComponentGSM>().onInitSceneMonsterEvent += initAllSceneMonster;
        }


        public virtual void OnEnterScene()
        {
            string sceneName = SceneManager.GetActiveScene().name;

            GetModule<TipsComponentGSM>().OpenTipsArriveScene(sceneName);

            Debug.Log($"进入场景: {sceneName}");
        }


        public virtual void OnUpdate()
        {
            foreach (var m in moduleList)
            {
                m.OnUpdate();
            }
        }

        /// <summary>
        /// 退出场景处理
        /// </summary>
        public virtual void OnExitScene()
        {
            // 退出场景时计数归0
            foreach (var key in new List<GroundType>(monsterAniSortData.Keys))
            {
                monsterAniSortData[key] = 0; 
            }
            deadMonsterSpriteSort = 0;
        }

        /// <summary>
        /// SceneManager退出
        /// </summary>
        public virtual void OnShutDown()
        {
            // 监听切换场景结束黑幕事件
            GameManager.GetGMComponent<ProcedureComponentGM>().onCompleteLoadingSceneEvent -= OnEnterScene;
            GameManager.GetGMComponent<ProcedureComponentGM>().onInitAllSceneMonsterEvent -= initAllSceneMonster;
            //if (GetModule<LoadSceneComponentGSM>() != null)
            //{
            //    GetModule<LoadSceneComponentGSM>().onEndLoadingSceneEvent -= OnEnterScene;
            //    GetModule<LoadSceneComponentGSM>().onInitSceneMonsterEvent -= initAllSceneMonster;
            //}

            ShutdownModules();
            GameManager.Instance.RemoveGameSceneManager();
        }

        #endregion


        #region 初始化重写

        /// <summary>
        /// 初始化添加GSM模块
        /// </summary>
        protected virtual void OnInitAddModules()
        {
            AddModule<ResComponentGSM>();
            AddModule<BuffComponentGSM>();
            AddModule<MapControlComponentGSM>();
            AddModule<PlayerHandlerComponentGSM>();
            AddModule<EventComponentGSM>();
            AddModule<CameraComponentGSM>();
            AddModule<RaycastComponentGSM>();
            AddModule<SceneEntityComponentGSM>();
            AddModule<StoryComponentGSM>();
            AddModule<InputComponentGSM>();
            AddModule<LoadSceneComponentGSM>();
            AddModule<TipsComponentGSM>();
            AddModule<EffectComponentGSM>();
        }

        /// <summary>
        /// 初始化Proxy
        /// </summary>
        protected virtual void OnInitRegisterProxy()
        {
            GameManager.GetGMComponent<MVCComponentGM>().GetProxy<TipsFormProxy>();
        }


        private void InitPlayer()
        {
            if (!config.canCreatePlayer)
            {
                return;
            }

            initAsyncCounter.Add();

            GetModule<PlayerHandlerComponentGSM>().CreatePlayer(logic =>
            {
                var playerSceneData = GetArchiveData<PlayerSceneData>();
                if (GameManager.GetGMComponent<ProcedureComponentGM>().archiveStart)
                {
                    // 优先存档位置
                    logic.SetPos(playerSceneData.pos);
                }
                else
                {
                    // 地图位置
                    SetPlayerPos(logic);
                }

                // 设置摄像机跟随
                GetModule<CameraComponentGSM>().SetFollow(logic.gameObject.transform);
                // 检测人物是否处于某些特殊区域
                CheckPlayerHasInSpcArea(playerSceneData);

                OpenFightingPanel();

/*                string pointerHoldPanelPath = UIPrefabPath.GetUIPrefabPath("PointerHoldPanel");
                bool pointerHoldPanelIsOpened = GameManager.GetGMComponent<UIComponentGM>().GetUIForm(pointerHoldPanelPath) != null;
                if (!pointerHoldPanelIsOpened)
                {
                    GameManager.GetGMComponent<UIComponentGM>().OpenUIForm(pointerHoldPanelPath, EUIGroup.System, new OpenFormArgs());
                }*/

                initAsyncCounter.Done();
            });
        }

        private void CheckPlayerHasInSpcArea(PlayerSceneData playerSceneData)
        {
            if (playerSceneData.isInTreeBridge)
            {
                // 在树洞中需要调整摄像机数据
                var cameraMgr = GetModule<CameraComponentGSM>();
                ForestEastTreeBridgeStoryMgr.getInstance().ChangeCamera(true, cameraMgr);
            }
        }

        private void OpenFightingPanel()
        {
            string fightingPanelPath = UIPrefabPath.GetUIPrefabPath("FightingPanel");
            bool fightingPanelIsOpened = GameManager.GetGMComponent<UIComponentGM>().GetUIForm(fightingPanelPath) != null;
            if (Config.isFightingScene)
            {
                if (!fightingPanelIsOpened)
                {
                    GameManager.GetGMComponent<UIComponentGM>().OpenUIForm(fightingPanelPath, EUIGroup.Bottom, new OpenFormArgs()
                    {
                        callBack = OnOpenFightingPanel
                    });
                }
            }
            else
            {
                if (fightingPanelIsOpened)
                {
                    GameManager.GetGMComponent<UIComponentGM>().CloseUIForm(UIPrefabPath.GetUIPrefabPath("FightingPanel"));
                }
            }
        }

        protected virtual void OnOpenFightingPanel(UIFormLogic uIFormLogic)
        {

        }

        /// <summary>
        /// 可以重写设置玩家位置
        /// </summary>
        /// <param name="playerLogic"></param>
        protected virtual void SetPlayerPos(PlayerLogic playerLogic)
        {
            if (EnterPosConfig == null || EnterPosConfig.Count == 0)
            {
                // 默认地图出生点
                playerLogic.SetPos(GetModule<MapControlComponentGSM>().DefaultBornTsf.position);
            }
            else
            {
                var lastSceneName = GameManager.GetGMComponent<ChangeSceneComponentGM>().LastSceneName;
                foreach (var enterPos in EnterPosConfig)
                {
                    if (enterPos.lastScene == lastSceneName)
                    {
                        playerLogic.SetPos(enterPos.pos.position);
                        GetArchiveData<DateData>().ThroughDate(enterPos.DatePass.x, enterPos.DatePass.y, enterPos.DatePass.z);
                        return;
                    }
                }
                // 默认地图出生点
                playerLogic.SetPos(GetModule<MapControlComponentGSM>().DefaultBornTsf.position);
            }
            
        }

        #endregion


        #region 协程

        public void WaitForInvoke(float time, Action action)
        {
            waitForInvokeCoroutine = StartCoroutine(WaitForSeconds(time, action));
        }

        private IEnumerator WaitForSeconds(float seconds, Action action)
        {
            yield return new WaitForSeconds(seconds);
            action?.Invoke();
        }

        #endregion


        //-----------------------------------------------------------------------------------
        // effect
        public T CreateEffect<T>(string[] keys, Transform parent = null) where T : IEffectComponent, new()
        {
            return GameManager.GetManager<IEffectManager>().CreateEffect<T>(keys);
        }

        public T PlayEffect<T>(string[] keys, int times, Transform parent = null) where T : IEffectComponent, new()
        {
            var cpn = CreateEffect<T>(keys, parent);
            cpn.Play(times);
            return cpn;
        }

        public T PlayEffect<T>(string[] keys, int times, Vector2 localPos, Transform parent) where T : IEffectComponent, new()
        {
            var cpn = CreateEffect<T>(keys, parent);
            cpn.GameObject.transform.localPosition = localPos;
            cpn.Play(times);
            return cpn;
        }

        public T PlayEffect<T>(string[] keys, int times, Vector2 worldPos) where T : IEffectComponent, new()
        {
            var cpn = CreateEffect<T>(keys);
            cpn.GameObject.transform.position = worldPos;
            cpn.Play(times);
            return cpn;
        }

        // --------------------------------------------------------------------------------
        public T GetArchiveData<T>() where T : BaseArchiveData, new()
        {
            return GameManager.GetGMComponent<ArchiveComponentGM>().GetData<T>();
        }

        // --------------------------------------------------------------------------------
        public List<SceneEntity> GetAllSceneEntities() => GetModule<SceneEntityComponentGSM>().GetAllSceneEntities();

        public Entity GetPlayerEntity()
        {
            var e = GameManager.GetGMComponent<EntityComponentGM>().GetEntity("Assets/GameRes/Prefabs/Entity/Player/Player.prefab");
            if (e == null)
            {
                Log.Error("未找到玩家实体");
                return null;
            }

            return e;
        }

        // 获取当前玩家第一个可交互的对象
        public InteractiveComponent GetFirstCanTouchEntiy(InteractiveComponent playerInteractiveComponent)
        {
            // 获取所有实体
            var entities = GetAllSceneEntities();
            InteractiveComponent closestComponent = null;
            float minDistance = float.MaxValue;

            // 获取玩家的交互组件
            if (playerInteractiveComponent == null)
            {
                var playerEntity = GetPlayerEntity();
                if (playerEntity != null && playerEntity.Logic is PlayerLogic playerLogic)
                {
                    playerInteractiveComponent = playerLogic.componentSystem.GetComponent<InteractiveComponent>();
                }
            }

            if (playerInteractiveComponent == null)
            {
                Log.Warning("PlayerInteractiveComponent 未找到，无法交互");
                return null;
            }

            // 遍历所有实体，查找最近的交互组件
            foreach (var entity in entities)
            {
                if (entity == null) { continue; }
                if (entity.gameObject == null) { continue; }
                if (!entity.gameObject.activeSelf) { continue; }
                if (entity.EntityLogic is BaseSceneEntityLogic entityLogic)
                {
                    var component = entityLogic.componentSystem.TryGetComponent<InteractiveComponent>();
                    if (component != null)
                    {
                        var overlap = playerInteractiveComponent.AreCollidersOverlapping(component.InteractiveCollider);
                        if (overlap)
                        {
                            // 计算距离
                            float distance = playerInteractiveComponent.DistanceTo(component);
                            if (distance < minDistance)
                            {
                                minDistance = distance;
                                closestComponent = component;
                            }
                        }
                    }
                }
            }

            Debug.Log($"[BaseGameSceneManager] GetFirstCanTouchEntiy result={(closestComponent != null ? closestComponent.gameObject.name : "null")}");
            return closestComponent;
        }

        public void SetSceneObjIsPause(bool value)
        {
            allSceneObjIsPause = value;
        }

        public bool GetSceneObjIsPause()
        {
            return allSceneObjIsPause;
        }

        public void SetSceneObjAniIsPause(bool value)
        {
            allSceneObjAniIsPause = value;
        }

        public bool GetSceneObjAniIsPause()
        {
            return allSceneObjAniIsPause;
        }

        public virtual TerrainType GetCurSceneTerrainType()
        {
            return TerrainType.IndoorType;
        }


        // 初始化所有场景的中怪物
        public virtual void initAllSceneMonster()
        {
            var entities = GetAllSceneEntities();
            int monsterTag = 0;
            foreach (var entity in entities)
            {
                if (entity == null) { continue; }
                if (entity.gameObject == null) { continue; }
                if (!entity.gameObject.activeSelf) { continue; }
                if (entity.EntityLogic is BaseMonster entityLogic)
                {
                    entityLogic.sceneMonsterTag = monsterTag; // 设置怪物的标志
                    // 检测当前怪物是否已经死亡
                    if (SceneMonsterDataMgr.getInstance().MonterHasDead(entityLogic))
                    {
                        entity.gameObject.SetActive(false);// 被识别为死亡的怪物在场景中需要隐藏起来
                    }
                    else
                    {
                        entity.gameObject.SetActive(true);
                    }
                    monsterTag++;
                }
            }
        }

        // 记录怪物已经死亡，已经死亡的怪物切换场景前不会刷新
        public virtual void recordMonsterHasDead(BaseMonster monster)
        {
            // 添加当前怪物
            SceneMonsterDataMgr.getInstance().RecordMonsterHasDead(monster);
        }
    }
}