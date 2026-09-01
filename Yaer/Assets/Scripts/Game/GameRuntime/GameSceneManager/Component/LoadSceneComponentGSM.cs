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
        /// 先开 <c>LoadingPanel</c> 进度条，再 <see cref="LoadScene"/>(blackFade:false)。
        /// 与 <c>SceneChangeDoor.ShowLoadingUI</c> 同路；门口对白结束自动进屋亦走此 API。
        /// <para>
        /// 原因：裸调 <c>LoadScene(name)</c> 默认黑幕，违背「进屋主表现=进度条」。
        /// 替代方案：扩 <c>LoadSceneTaskAction</c> 挂图末——现网 Action 无 Loading，故本期用本助手 + onStoryEnd。
        /// </para>
        /// </summary>
        /// <param name="sceneName">目标场景名（如 <c>Village_Chief_House</c>）。</param>
        /// <param name="enterPosKey">可选 EnterPos 键；空则用卸场真实场景名。</param>
        public void LoadSceneWithLoadingPanel(string sceneName, string enterPosKey = null)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogError("[SceneLoad] LoadSceneWithLoadingPanel: sceneName 为空");
                return;
            }

            Debug.Log($"[SceneLoad] LoadSceneWithLoadingPanel scene={sceneName} enterPosKey={enterPosKey}");

            GameManager.GetGMComponent<UIComponentGM>().OpenUIForm(
                UIPrefabPath.GetUIPrefabPath("LoadingPanel"),
                EUIGroup.Top,
                new OpenFormArgs
                {
                    // 与门 ShowLoadingUI 分支保持同一 OpenFormArgs 形态（空 userData Action）
                    userData = new Action(() => { }),
                    callBack = _ =>
                    {
                        // 进度条已开：切场禁止再主控 BlackPanel
                        LoadScene(sceneName, null, false, enterPosKey);
                    }
                });
        }

        /// <summary>
        /// 跳转场景黑幕
        /// </summary>
        /// <param name="sceneName">场景名</param>
        /// <param name="stayAction">黑幕完全打开时执行</param>
        /// <param name="blackFade">是否黑幕转场</param>
        /// <param name="enterPosKey">可选 EnterPos 键（E3′）；空=真实场景名</param>
        public void LoadScene(string sceneName, Action stayAction = null, bool blackFade=true, string enterPosKey = null)
        {
            // 0722 章末被跳过溯源：统一换场入口打栈，过滤 Console「SceneLoad」即可看到真正调用方
            // （正规进村应先有 [MapSelect]；若无 MapSelect 却有本日志 → R7 后门）
            // 替代方案：只在进 Village_KenMuNi1 时打日志——覆盖面窄，漏掉其它误跳，故入口全量记录。
            Debug.Log(
                $"[SceneLoad] scene={sceneName} blackFade={blackFade} enterPosKey={enterPosKey} from={gameObject.name}\n" +
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
                                        // 二次换场时旧 GSM 可能已销毁（如进屋闪回村）；取当前场景 manager 避免 MissingReferenceException。
                                        // 替代方案：捕获 sceneName 比对后再回调——仍无法单独止闪回，故仅防御性判空。
                                        var currentManager = GameManager.GetGameSceneManager();
                                        currentManager?.GetModule<LoadSceneComponentGSM>()?.OnBlackFadeEnd();
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
                                sceneName = sceneName,
                                enterPosKey = enterPosKey
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
                    sceneName = sceneName,
                    enterPosKey = enterPosKey
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