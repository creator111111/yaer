using System;
using Game.GameMgr.Component.Archive;
using Game.GameMgr.Component.Archive.ArchiveDataClass.Player;
using Game.GameMgr.Component.Archive.ArchiveDataClass.Quest;
using Game.GameMgr.Component.Base;
using Game.GameMgr.Component.ChangeScene;
using Game.GameMgr.Component.PureMVC;
using Game.GameMgr.Component.UI;
using Game.GameRuntime.UI.FormLogic;
using Game.GameRuntime.UI.FormLogic.Archive.LoadGamePanel;
using Game.GameRuntime.UI.FormLogic.Archive.SaveGamePanel;
using Game.GameRuntime.UI.FormLogic.Black;
using Game.GameRuntime.UI.FormLogic.Menu;
using Game.GameRuntime.UI.FormLogic.SelectHard;
using Game.GameRuntime.UI.FormLogic.Start;
using Game.Static.Enum;
using Game.Static.Name.Res;
using Game.Static.Path;
using GameFramework.UnityRuntime.Utility;
using UnityEngine.SceneManagement;

namespace Game.GameMgr.Component
{
    /// <summary>
    /// 游戏进程
    /// </summary>
    public class ProcedureComponentGM : BaseComponentGM
    {
        private bool start;
        private bool pause;
        public bool archiveStart;
        private ArchiveComponentGM archiveComponentGM;
        private MVCComponentGM mvcComponentGM;
        private UIComponentGM uiComponentGM;

        /// <summary>
        /// 游戏开始
        /// </summary>
        public bool GameStart => start;

        public bool IsPlaying { get; set; }

        public bool Pause
        {
            get=> pause;
            set
            {
                pause = value;

                if (pause)
                {
                    onPauseGameAction?.Invoke();
                }
                else
                {
                    onResumeGameAction?.Invoke();
                }
            }
        }
        
        public event Action onStartGameAction;
        public event Action onReturnToMenuAction;
        public event Action onStartLoadingSceneEvent;
        public event Action onCompleteLoadingSceneEvent;
        public event Action onPauseGameAction; 
        public event Action onResumeGameAction;
        public event Action onInitAllSceneMonsterEvent;

        public override void OnInit()
        {
            base.OnInit();

            // 存档加载完成事件
            archiveComponentGM = GameManager.GetGMComponent<ArchiveComponentGM>();
            mvcComponentGM = GameManager.GetGMComponent<MVCComponentGM>();
            uiComponentGM = GameManager.GetGMComponent<UIComponentGM>();

            // 监听是否加载存档
            mvcComponentGM.GetProxy<LoadGameFormProxy>().onLoadGameAction += LoadGame;

            // 监听是否保存
            mvcComponentGM.GetProxy<SaveGameFormProxy>().onSaveNewArchiveAction = SaveNewGame;
            mvcComponentGM.GetProxy<SaveGameFormProxy>().onSaveOldArchiveAction = SaveGame;
            mvcComponentGM.GetProxy<SaveGameFormProxy>().onCoverArchiveAction = CoverGame;

            // 监听是否退出
            mvcComponentGM.GetProxy<MenuFormProxy>().onMenuActiveEvent += b => Pause = b;
            mvcComponentGM.GetProxy<MenuFormProxy>().onReturnMainMenuEvent = ReturnToMainMenu;

            mvcComponentGM.GetProxy<DeadPanelProxy>().onReturnMainMenuEvent = ReturnToMainMenu;
            mvcComponentGM.GetProxy<DeadPanelProxy>().onLoadGameAction = LoadGame;
        }

        public void OpenMainMenu()
        {
            // 加载部分配置（成就、任务静态表与 Achievement / Monster 并列）
            AchievementDataMgr.getInstance().Init();
            QuestConfigMgr.getInstance().Init();
            // 进入主菜单
            uiComponentGM.OpenUIForm(UIPrefabPath.StartPanel, EUIGroup.Bottom, new OpenFormArgs()
            {
                userData = this,
                callBack = logic =>
                {
                    if (logic is StartFormLogic startFormLogic)
                    {
                        // 监听是否选择难度
                        mvcComponentGM.GetProxy<SelectHardFormProxy>().onSelect = hard =>
                        {
                            // 关闭菜单
                            startFormLogic.CloseForm();

                            // 开始新游戏
                            NewGame(hard);
                        };
                    }
                }
            });
        }

        /// <summary>
        /// 局内（如 MapPanel ButtonHome）重开新游戏：清档 → 初始化 → 加载 NewGameScene → 漫画开场。
        /// 难度沿用当前 <see cref="HardComponentGM.Hard"/>，不再弹 SelectHardPanel。
        /// 与主菜单 <see cref="NewGame"/> 的差异：局内会先 <see cref="ExitGame"/>、卸载 GSM、<see cref="ArchiveComponentGM.ClearNowArchive"/>，
        /// 再 <see cref="ArchiveComponentGM.CreateTempGameArchive"/>，并在黑幕内调用 <see cref="GameManager.OnEnterComponents"/>（对齐 LoadGame / ReturnToMainMenu）。
        /// 替代方案：先 ReturnToMainMenu 再自动选难度开新局 — 会闪主菜单，体验差，故不采用。
        /// </summary>
        public void RestartNewGameFromProgress()
        {
            onStartLoadingSceneEvent?.Invoke();

            GameManager.GetGMComponent<SoundComponentGM>().StopBGM();

            uiComponentGM.OpenUIForm(UIPrefabPath.GetUIPrefabPath("BlackPanel"), EUIGroup.System, new OpenFormArgs()
            {
                userData = new ShowBlackFormArgs()
                {
                    showType = BlackFadeType.FadeShow,
                    onShowEnd = blackFormLogic =>
                    {
                        // 已在局中：先结束当前局，避免旧 GSM / 实体 / 存档引用残留（主菜单 NewGame 时 start==false，此处与 LoadGame 一致）
                        if (start)
                        {
                            ExitGame();
                        }

                        var sceneMgr = GameManager.GetGameSceneManager();
                        if (sceneMgr != null)
                        {
                            sceneMgr.OnExitScene();
                            sceneMgr.OnShutDown();
                        }

                        uiComponentGM.CloseAllUIForm(blackFormLogic.UIForm);

                        // 局内必有活跃档；清档后再建全新临时档（主菜单 NewGame 无活跃档，故不调 ClearNowArchive）
                        archiveComponentGM.ClearNowArchive();
                        archiveComponentGM.CreateTempGameArchive();

                        // 难度沿用当前 HardComponentGM.Hard，不重新弹出 SelectHardPanel
                        GameManager.GetGMComponent<PlayerDataComponentGM>().InitNewGameData();

                        QuestConfigMgr.getInstance().Init();
                        MonsterDataMgr.getInstance().Init();

                        GameManager.Instance.onGameSceneManagerReady += (manager) =>
                        {
                            blackFormLogic.CloseFormFade(() =>
                            {
                                StartGame();
                                onCompleteLoadingSceneEvent?.Invoke();
                            });
                        };

                        GameManager.GetGMComponent<ChangeSceneComponentGM>().LoadScene(new LoadSceneArgs()
                        {
                            sceneName = SceneName.NewGameScene
                        });

                        GameManager.OnEnterComponents();
                    }
                },
            });
        }

        /// <summary>
        /// 新游戏
        /// </summary>
        private void NewGame(EGameHard hard)
        {
            onStartLoadingSceneEvent?.Invoke();
            
            // 创建临时存档
            archiveComponentGM.CreateTempGameArchive();

            // 设置难度
            GameManager.GetGMComponent<HardComponentGM>().SetHard(hard);

            // 初始化新游戏数据
            GameManager.GetGMComponent<PlayerDataComponentGM>().InitNewGameData();
            
            // 打开黑幕
            uiComponentGM.OpenUIForm(UIPrefabPath.GetUIPrefabPath("BlackPanel"), EUIGroup.System, new OpenFormArgs()
            {
                userData = new ShowBlackFormArgs()
                {
                    showType = BlackFadeType.FadeShow,
                    onShowEnd = blackFormLogic =>
                    {
                        // 关闭所有ui
                        uiComponentGM.CloseAllUIForm(blackFormLogic.UIForm);
                        // 初始化怪物、任务静态配置
                        QuestConfigMgr.getInstance().Init();
                        MonsterDataMgr.getInstance().Init();

                        // 监听场景Manger初始化完成事件
                        GameManager.Instance.onGameSceneManagerReady += (manager) =>
                        {
                            blackFormLogic.CloseFormFade(() =>
                            {
                                StartGame();
                                onCompleteLoadingSceneEvent?.Invoke();
                            });
                        };

                        // 清空 数据
                        GameManager.GetGMComponent<ChangeSceneComponentGM>().LoadScene(new LoadSceneArgs()
                        {
                            sceneName = SceneName.NewGameScene
                        });
                    }
                },
            });
        }

        /// <summary>
        /// 存档加载游戏
        /// </summary>
        /// <param name="guid">存档guid</param>
        private void LoadGame(string guid)
        {
            onStartLoadingSceneEvent?.Invoke();
            
            archiveStart = true;

            GameManager.GetGMComponent<SoundComponentGM>().StopBGM();

            // 打开黑幕
            uiComponentGM.OpenUIForm(UIPrefabPath.GetUIPrefabPath("BlackPanel"), EUIGroup.System, new OpenFormArgs()
            {
                userData = new ShowBlackFormArgs()
                {
                    showType = BlackFadeType.FadeShow,
                    onShowEnd = blackFormLogic =>
                    {
                        // 已经开始游戏就先结束
                        if (start)
                        {
                            ExitGame();
                        }
                        var sceneMgr = GameManager.GetGameSceneManager();
                        if (sceneMgr != null)
                        {
                            sceneMgr.OnExitScene();
                            sceneMgr.OnShutDown();
                        }
                        
                        // 关闭所有ui
                        uiComponentGM.CloseAllUIForm(blackFormLogic.UIForm);
                        // 加载存档
                        archiveComponentGM.LoadArchive(guid);
                        // 初始化怪物、任务静态配置
                        QuestConfigMgr.getInstance().Init();
                        MonsterDataMgr.getInstance().Init();

                        // 监听场景Manger初始化完成事件
                        GameManager.Instance.onGameSceneManagerReady += (manager) =>
                        {
                            onInitAllSceneMonsterEvent?.Invoke();
                            blackFormLogic.CloseFormFade(() =>
                            {
                                archiveStart = false;
                                StartGame();
                                onCompleteLoadingSceneEvent?.Invoke();
                            });
                        };
                        
                        // 加载场景
                        GameManager.GetGMComponent<ChangeSceneComponentGM>().LoadScene(new LoadSceneArgs()
                        {
                            sceneName = archiveComponentGM.GetData<PlayerSceneData>().sceneName,
                        });

                        // 重新加载组件
                        GameManager.OnEnterComponents();
                    }
                },
            });
        }

        private void SaveGame()
        {
            GameManager.GetGMComponent<SaveGameHandleComponent>().SaveGame();
            archiveComponentGM.SaveOldArchive();
        }

        private void SaveNewGame()
        {
            GameManager.GetGMComponent<SaveGameHandleComponent>().SaveGame();
            archiveComponentGM.SaveNewArchive();
        }

        private void CoverGame(string guid)
        {
            //SaveGame();
            GameManager.GetGMComponent<SaveGameHandleComponent>().SaveGame();
            archiveComponentGM.CoverArchive(guid);
        }

        /// <summary>
        /// 开始游戏
        /// </summary>
        private void StartGame()
        {
            Log.Info("StartGame");
            start = true;
            Pause = false;
            IsPlaying = true;
            
            onStartGameAction?.Invoke();
            onStartGameAction = null;
        }

        /// <summary>
        /// 退出当前游戏
        /// </summary>
        private void ExitGame()
        {
            // 退出场景管理器
            if (GameManager.GetGameSceneManager() != null)
            {
                GameManager.GetGameSceneManager().OnShutDown();
            }
            
            // 重置组件
            GameManager.OnExitComponents();

            Pause = true;
            IsPlaying = false;
        }

        public void ReturnToMainMenu()
        {
            Log.Info("ReturnToMainMenu called.");
            // 开启黑幕
            uiComponentGM.OpenUIForm(UIPrefabPath.GetUIPrefabPath("BlackPanel"), EUIGroup.System, new OpenFormArgs()
            {
                userData = new ShowBlackFormArgs()
                {
                    showType = BlackFadeType.FadeShow,
                    onShowEnd = blackFormLogic =>
                    {
                        ExitGame();
                        
                        // 关闭所有ui
                        uiComponentGM.CloseAllUIForm(blackFormLogic.UIForm);
                        
                        // 清空存档
                        archiveComponentGM.ClearNowArchive();
                        
                        // 切换场景
                        GameManager.GetGMComponent<ChangeSceneComponentGM>().LoadScene(new LoadSceneArgs()
                        {
                            sceneName = SceneName.StartScene,
                            callBack = () =>
                            {
                                // 切换流程
                                onReturnToMenuAction?.Invoke();
                                onReturnToMenuAction = null;

                                // 关闭黑幕 
                                blackFormLogic.CloseForm();
                            }
                        });
                        
                        // 重新加载组件
                        GameManager.OnEnterComponents();
                    }
                },
            });
        }
    }
}