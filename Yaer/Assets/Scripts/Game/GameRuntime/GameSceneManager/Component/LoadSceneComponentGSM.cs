using System;
using Game.GameMgr;
using Game.GameMgr.Component;
using Game.GameMgr.Component.ChangeScene;
using Game.GameMgr.Component.UI;
using Game.GameRuntime.UI.FormLogic.Black;
using Game.Static.Path;

namespace Game.GameRuntime.GameSceneManager.Component
{
    public class LoadSceneComponentGSM : BaseComponentGSM
    {
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
                                // 初始化场景中的怪物
                                manager.GetModule<LoadSceneComponentGSM>().OnSceneManagerInit();
                                blackFormLogic.CloseFormFade(() =>
                                {
                                    // 触发黑幕结束事件
                                    // 获取下一个场景上的该组件触发
                                    manager.GetModule<LoadSceneComponentGSM>().OnBlackFadeEnd();
                                });
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