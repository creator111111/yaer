using Game.GameMgr.Component.UI;
using Game.GameMgr;
using Game.GameRuntime.Entities.Base;
using Game.GameRuntime.Entities.Base.BaseSceneObj;
using Game.GameRuntime.Entities.Component.Interactive;
using Game.GameRuntime.Entities.Player;
using Game.GameRuntime.GameSceneManager.Component;
using Game.Static.Name.Res;
using Game.Static.Path;
using GameFramework.UnityRuntimeExtend.Component;
using System;
using System.Text;
using UnityEngine;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;

namespace Game.GameRuntime.Entities.Component.Map
{
    [RequireComponent(typeof(SceneEntity), typeof(ComponentSystemMono))]
    public class SceneChangeDoor : BaseSceneEntityLogic
    {
        [SerializeField] protected Transform bornPos;
        [SerializeField] protected string NextSceneName;
        [SerializeField] protected bool TriggerWhenMoveIn = false;
        [SerializeField] protected bool ShowLoadingUI = false;

        public Transform BornPos => bornPos;
        protected InteractiveComponent interactiveComponent;

        private bool isEnter;
        public Func<bool> CheckNextSceneUnlock = null;

        protected internal override void OnInit(object userData)
        {
            if (!enabled)
            {
                return;
            }

            base.OnInit(userData);

            // 半成品门常缺 InteractiveComponent；GetComponent 抛异常会中断 Map/SceneManager 初始化导致黑屏。
            // 替代方案：强制要求场景 YAML 齐件——仍应齐件，但这里降级为 Error，避免整场景挂死。
            interactiveComponent = componentSystem.TryGetComponent<InteractiveComponent>();
            if (interactiveComponent == null)
            {
                Debug.LogError(
                    $"[SceneChangeDoor] 缺少 InteractiveComponent，跳过门初始化。name={gameObject.name}",
                    this);
                return;
            }

            interactiveComponent.onClickInteractiveEvent += EnterDoor;
            if (TriggerWhenMoveIn)
            {
                interactiveComponent.onEnterInteractiveEvent += (component) =>
                {
                    if (component.Entity?.Logic is PlayerLogic playuerLogic)
                    {
                        if (!playuerLogic.isDead)
                        {
                            EnterDoor(component);
                        }
                    }
                };
            }
        }

        protected virtual void OnEnterSuccess()
        {

        }

        protected virtual void OnEnterFail()
        {

        }

        protected virtual void EnterDoor(InteractiveComponent component)
        {
            // 0722 验收：过滤 Console「SceneChangeDoor」可判断是否门换场（相对地图点选的 [MapSelect]）
            Debug.Log(
                $"[SceneChangeDoor] Enter name={gameObject.name} path={BuildHierarchyPath(transform)} " +
                $"next={NextSceneName} triggerWhenMoveIn={TriggerWhenMoveIn} " +
                $"activeScene={UnitySceneManager.GetActiveScene().name}");

            if (string.IsNullOrEmpty(NextSceneName))
            {
                Debug.LogError($"[SceneChangeDoor] NextSceneName 为空，无法换场。path={BuildHierarchyPath(transform)}");
            }
            else
            {
                // 产品定稿（0721/0722）：离开拉普路西后必须「章末 → 地图 → 点肯姆尼」。
                // 若本地脏场景把 RightDoor 启用并填了 Village_KenMuNi1，会无 [MapSelect] 直跳村（R7）。
                // 此处拒绝该配置，逼回正规链；仓库磁盘本就应为「组件禁用 + 空名」。
                // 替代方案：只打 Error 仍放行——便于临时测门，但会再次跳过章末，故不采用。
                if (IsForbiddenWestRappRoadRightDoorToVillage())
                {
                    Debug.LogError(
                        "[SceneChangeDoor] 拒绝换场：WestRappRoad/RightDoor → Village_KenMuNi1 " +
                        "（禁止绕过章末地图选关；请恢复 RightDoor 为禁用且 NextSceneName 为空）");
                    OnEnterFail();
                    return;
                }

                if (CheckNextSceneUnlock == null || CheckNextSceneUnlock())
                {
                    if (isEnter)
                    {
                        Debug.LogWarning($"[SceneChangeDoor] 已进入过，忽略重复触发。path={BuildHierarchyPath(transform)}");
                    }
                    else
                    {
                        isEnter = true;
                        OnEnterSuccess();
                        if (ShowLoadingUI)
                        {
                            GameManager.GetGMComponent<UIComponentGM>().OpenUIForm(UIPrefabPath.GetUIPrefabPath("LoadingPanel"), EUIGroup.Top, new OpenFormArgs()
                            {
                                userData = new Action(() =>
                                {
                                    
                                }),
                                callBack = (uiFormLogic) =>
                                {
                                    SceneManager.GetModule<LoadSceneComponentGSM>().LoadScene(NextSceneName, null, false);
                                }
                            });
                            
                        }
                        else
                        {
                            SceneManager.GetModule<LoadSceneComponentGSM>().LoadScene(NextSceneName, null, true);
                        }
                    }
                }
                else
                {
                    OnEnterFail();
                    Debug.Log($"[SceneChangeDoor] 场景未解锁：{NextSceneName}");
                }
            }
        }

        /// <summary>
        /// 拉普路西右门不得直跳肯姆尼村（正规入口：地图 ButtonJingLingVillage）。
        /// </summary>
        bool IsForbiddenWestRappRoadRightDoorToVillage()
        {
            if (gameObject.name != "RightDoor")
            {
                return false;
            }

            if (NextSceneName != SceneName.Village_KenMuNi1)
            {
                return false;
            }

            return UnitySceneManager.GetActiveScene().name == SceneName.WestRappRoad;
        }

        static string BuildHierarchyPath(Transform t)
        {
            var sb = new StringBuilder(t.name);
            while (t.parent != null)
            {
                t = t.parent;
                sb.Insert(0, t.name + "/");
            }
            return sb.ToString();
        }
    }
}