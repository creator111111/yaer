using System;
using Game.GameMgr;
using Game.GameMgr.Component;
using Game.GameMgr.Component.ChangeScene;
using Game.GameMgr.Component.UI;
using Game.GameRuntime.GameSceneManager.Base;
using Game.GameRuntime.UI.FormLogic.Black;
using Game.Static.Path;
using UnityEngine;

namespace Game.GameRuntime.GameSceneManager.Component
{
    public class LoadSceneComponentGSM : BaseComponentGSM
    {
        [Tooltip("地图黑幕转场：场景就绪后全黑多停留的秒数，掩盖玩家 Home 控制器异步加载前的战斗待机闪现")]
        [SerializeField] private float mapTransitionBlackHoldSeconds = 0.3f;

        /// <summary>供转场回调读取「目标场景」上配置的停留时长。</summary>
        public float MapTransitionBlackHoldSeconds => mapTransitionBlackHoldSeconds;

        public event Action onStartLoadingSceneEvent;
        public event Action onEndLoadingSceneEvent;
        public event Action onInitSceneMonsterEvent;

        /// <summary>
        /// 跳转场景黑幕
        /// </summary>
        /// <param name="sceneName">场景名</param>
        /// <param name="stayAction">黑幕完全打开时执行</param>
        public void LoadScene(string sceneName, Action stayAction = null, bool blackFade=true)
        {
            // 0722 章末被跳过溯源：统一换场入口打栈，过滤 Console「SceneLoad」即可看到真正调用方
            // （正规进村应先有 [MapSelect]；若无 MapSelect 却有本日志 → R7 后门）
            // 替代方案：只在进 Village_KenMuNi1 时打日志——覆盖面窄，漏掉其它误跳，故入口全量记录。
            Debug.Log(
                $"[SceneLoad] scene={sceneName} blackFade={blackFade} from={gameObject.name}\n" +
                UnityEngine.StackTraceUtility.ExtractStackTrace());

            onStartLoadingSceneEvent?.Invoke();
            if (blackFade)
            {
                // 打开黑幕
                GameManager.GetGMComponent<UIComponentGM>().OpenUIForm(UIPrefabPath.GetUIPrefabPath("BlackPanel"), EUIGroup.System, new OpenFormArgs()
                {
                    userData = new ShowBlackFormArgs()
                    {
                        showType = BlackFadeType.FadeShow,
                        onShowEnd = blackFormLogic =>
                        {
                            stayAction?.Invoke();

                            SceneManager.OnExitScene();
                            SceneManager.OnShutDown();

                            // 监听场景Manger初始化完成事件
                            GameManager.Instance.onGameSceneManagerReady += (manager) =>
                            {
                                var loadModule = manager.GetModule<LoadSceneComponentGSM>();
                                loadModule.OnSceneManagerInit();

                                void CloseBlackAndNotify()
                                {
                                    blackFormLogic.CloseFormFade(() =>
                                    {
                                        // 触发黑幕结束事件
                                        // 获取下一个场景上的该组件触发
                                        manager.GetModule<LoadSceneComponentGSM>().OnBlackFadeEnd();
                                    });
                                }

                                // 村开场等旁路：仍全黑时先挂对话遮罩，Ready 后再 CloseFormFade（见 0804 禁止露景漏缝）。
                                // 未接管则保持默认 hold → 淡出契约，其它换场零回归。
                                if (manager is BaseGameSceneManager deferGsm
                                    && deferGsm.TryDeferBlackFadeForCover(CloseBlackAndNotify))
                                {
                                    return;
                                }

                                float hold = loadModule.MapTransitionBlackHoldSeconds;
                                if (hold > 0f && manager is BaseGameSceneManager baseGsm)
                                {
                                    baseGsm.WaitForInvoke(hold, CloseBlackAndNotify);
                                }
                                else
                                {
                                    CloseBlackAndNotify();
                                }
                            };
                            // 加载场景
                            GameManager.GetGMComponent<ChangeSceneComponentGM>().LoadScene(new LoadSceneArgs()
                            {
                                sceneName = sceneName
                            });
                        }
                    },
                });
            }
            else
            {
                stayAction?.Invoke();

                SceneManager.OnExitScene();
                SceneManager.OnShutDown();

                // 监听场景Manger初始化完成事件
                GameManager.Instance.onGameSceneManagerReady += manager =>
                {
                    manager.GetModule<LoadSceneComponentGSM>().OnSceneManagerInit();
                    // 获取下一个场景上的该组件触发
                    manager.GetModule<LoadSceneComponentGSM>().OnBlackFadeEnd();
                };
                // 加载场景
                GameManager.GetGMComponent<ChangeSceneComponentGM>().LoadScene(new LoadSceneArgs()
                {
                    sceneName = sceneName
                });
            }
            
        }
        
        public void OnBlackFadeEnd() => onEndLoadingSceneEvent?.Invoke();
        public void OnSceneManagerInit()
        {
            onInitSceneMonsterEvent?.Invoke();
        }
    }
}