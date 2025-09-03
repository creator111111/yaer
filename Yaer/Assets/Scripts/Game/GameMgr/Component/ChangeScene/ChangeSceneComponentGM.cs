using System.Collections.Generic;
using Game.GameMgr.Component.Base;
using Game.Static.Name.Res;
using Game.Static.Path;
using GameFramework.Event;
using GameFramework.UnityRuntime.Event;
using GameFramework.UnityRuntime.Scene;
using GameFramework.UnityRuntime.Utility;

namespace Game.GameMgr.Component.ChangeScene
{
    public class ChangeSceneComponentGM : BaseComponentGM
    {
        private bool canChange;
        private string nowSceneName;
        private string lastSceneName;
        private List<LoadSceneArgs> loadArgsList = new List<LoadSceneArgs>();
        private List<UnloadSceneArgs> unloadArgsList = new List<UnloadSceneArgs>();

        public bool CanChange => canChange;
        public string NowSceneName => nowSceneName;
        public string LastSceneName => lastSceneName;
        
        public override void OnInit()
        {
            base.OnInit();

            // 监听场景加载成功
            GameManager.GetGFComponent<EventComponent>().Subscribe(LoadSceneSuccessEventArgs.EventId, OnSceneLoadSuccessHandler);
            GameManager.GetGFComponent<EventComponent>().Subscribe(UnloadSceneSuccessEventArgs.EventId, OnSceneUnloadSuccessHandler);
        }

        public override void OnEnter()
        {
            base.OnEnter();
            
            nowSceneName = SceneName.InitScene;
            lastSceneName = SceneName.InitScene;
        }

        /// <summary>
        ///  监听场景卸载成功事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSceneUnloadSuccessHandler(object sender, GameEventArgs e)
        {
            if (e is UnloadSceneSuccessEventArgs successEventArgs)
            {
                var unloadArgs = unloadArgsList.Find(x => x == successEventArgs.UserData);
                if (unloadArgs == null) return;
                unloadArgs.callBack?.Invoke();
                unloadArgsList.Remove(unloadArgs);
            }
        }

        /// <summary>
        /// 监听场景加载成功事件，并触发回调。
        /// </summary>
        private void OnSceneLoadSuccessHandler(object sender, GameEventArgs e)
        {
            if (e is LoadSceneSuccessEventArgs successEventArgs)
            {
                var loadArgs = loadArgsList.Find(x => x == successEventArgs.UserData);
                if (loadArgs == null) return;
                loadArgs.callBack?.Invoke();
                loadArgsList.Remove(loadArgs);
                nowSceneName = loadArgs.sceneName;
            }
        }

        /// <summary>
        /// 加载场景
        /// </summary>
        public void LoadScene(LoadSceneArgs args)
        {
            if (args == null)
            {
                Log.Error("参数错误");
                return;
            }
            
            // initscene不需要卸载
            if (nowSceneName == SceneName.InitScene)
            {
                // 保存上一个场景名
                lastSceneName = nowSceneName;
                loadArgsList.Add(args);
                GameManager.GetGFComponent<SceneComponent>().LoadScene(SceneAssetPath.GetSceneAssetPath(args.sceneName), args);
            }
            else
            {
                var unloadArgs = new UnloadSceneArgs()
                {
                    unloadSceneName = nowSceneName,
                    callBack = () =>
                    {
                        // 保存上一个场景名
                        lastSceneName = nowSceneName;
                        if (args.sceneName == SceneName.InitScene)
                        {
                            // 如果加载的场景有问题则回到游戏开始界面
                            GameManager.GetGMComponent<ProcedureComponentGM>().ReturnToMainMenu();
                            return;
                        }
                        loadArgsList.Add(args);
                        // 加载新场景
                        GameManager.GetGFComponent<SceneComponent>().LoadScene(SceneAssetPath.GetSceneAssetPath(args.sceneName), args);
                    }
                };

                unloadArgsList.Add(unloadArgs);
                // 卸载当前场景
                GameManager.GetGFComponent<SceneComponent>().UnloadScene(SceneAssetPath.GetSceneAssetPath(nowSceneName), unloadArgs);
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            
            // 卸载当前场景
            // GameManager.GetGFComponent<SceneComponent>().UnloadScene(SceneAssetPath.GetSceneAssetPath(nowSceneName), null);
        }
    }
}