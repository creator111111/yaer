using Game.GameMgr;
using Game.GameMgr.Component;
using Game.GameMgr.Component.ChangeScene;
using Game.GameRuntime.GameSceneManager.Component;
using Game.Static.MVC;
using Game.Static.Name.Clothes;
using Game.Static.Name.Res;
using GameFramework.CoreExtend.Component;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameDebug
{
    public class GameTool
    {
        private string logMsg = "";
        private string[] sceneNames;
        private GameManager gm;
        private ComponentSystem componentSystem;

        public string LogMsg => logMsg;
        public string[] SceneNames => sceneNames;

        public GameTool()
        {
            sceneNames = new[]
            {
                SceneName.NewGameScene,
                SceneName.HomeScene1,
                SceneName.HomeScene2,
                SceneName.SelectClothesScene,
                SceneName.ForestScene,
                SceneName.WestRappRoad,
                SceneName.ShenLin

            };

            gm = GameManager.Instance;
            componentSystem = new ComponentSystem();
            InitAddComponents();
            componentSystem.InitComponents();
        }

        private void InitAddComponents()
        {
            AddComponent<AddItemComponentGT>();
            AddComponent<ChangeClothesComponentGT>();
        }

        private void WriteLog(string info)
        {
            logMsg = info;
            Debug.Log("GameTool:" + info);
        }

        public void SkipInitScene()
        {
            if (gm == null)
            {
                gm = GameManager.Instance;
            }
            // 判断是否在运行
            if (!Application.isPlaying)
            {
                WriteLog("请先运行游戏");
                return;
            }

            if (SceneManager.GetActiveScene().name == SceneName.InitScene)
            {
                gm.SendNotification(NotificationName.UI.HIDE_INIT_PANEL);
                // gm.GetProxy<InitPanelProxy>().LoadStartScene();
                WriteLog("跳过开头成功");
            }
            else
            {
                WriteLog("当前场景不是开头场景");
            }
        }

        public void SkipScene(string sName)
        {
            if (gm == null)
            {
                gm = GameManager.Instance;
            }
            
            if (!Application.isPlaying)
            {
                WriteLog("请先运行游戏");
                return;
            }

            if (GameManager.GetGMComponent<ProcedureComponentGM>().GameStart)
            {
                switch (sName)
                {
                    case SceneName.HomeScene1:
                        GameManager.GetGameSceneManager().GetModule<LoadSceneComponentGSM>().LoadScene(sName);
                        break;
                    case SceneName.HomeScene2:
                        GameManager.GetGameSceneManager().GetModule<LoadSceneComponentGSM>().LoadScene(sName);
                        break;
                    case SceneName.SelectClothesScene:
                        GameManager.GetGameSceneManager().GetModule<LoadSceneComponentGSM>().LoadScene(sName);
                        break;
                    case SceneName.ForestScene:
                        WearBattleClothes();
                        GameManager.GetGameSceneManager().GetModule<LoadSceneComponentGSM>().LoadScene(sName);
                        break;
                    default:
                        WearBattleClothes();
                        GameManager.GetGameSceneManager().GetModule<LoadSceneComponentGSM>().LoadScene(sName);
                        break;
                }
            }
            else
            {
                WriteLog("请先开始游戏");
            }
        }

        #region 跳转场景

        private void WearBattleClothes()
        {
            // 衣服数据
            // var pd = GameManager.GetManager<IPlayerDataManager>().PlayerData;
            // pd.PlayerClothesData.AddClothes(BoneName.Headwear, ClothesName.HeadWear.NoHeadWear);
            // pd.PlayerClothesData.AddClothes(BoneName.Clothes, ClothesName.Clothes.Armor);
            // pd.PlayerClothesData.AddClothes(BoneName.Trousers, ClothesName.Trousers.ArmorTrousers);
            // pd.PlayerClothesData.AddClothes(BoneName.Shoes, ClothesName.Shoes.ArmorShoes);
            var pd = GameManager.GetGMComponent<PlayerDataComponentGM>().GetClothesData();
            pd.AddClothes(BoneName.Headwear, ClothesName.HeadWear.NoHeadWear);
            pd.AddClothes(BoneName.Clothes, ClothesName.Clothes.Armor);
            pd.AddClothes(BoneName.Trousers, ClothesName.Trousers.ArmorTrousers);
            pd.AddClothes(BoneName.Shoes, ClothesName.Shoes.ArmorShoes);
            gm.SetCanChangeScene(true);
            // gm.LoadScene(args);
            
        }

        #endregion

        public T GetComponent<T>() where T : BaseGTComponent
        {
            return componentSystem.GetComponent<T>();
        }
        
        private void AddComponent<T>() where T : BaseGTComponent, new()
        {
            var component = new T();
            componentSystem.AddComponent(component);
        }
    }
}