using Game.GameMgr;
using Game.GameMgr.Component;
using Game.GameMgr.Component.Archive.ArchiveDataClass.Player;
using Game.GameMgr.Component.Archive.ArchiveDataClass.Scene;
using Game.GameMgr.Component.UI;
using Game.GameRuntime.Entities.Base.BaseSceneObj;
using Game.GameRuntime.Entities.Component.Interactive;
using Game.GameRuntime.Entities.Player.Components;
using Game.GameRuntime.Entities.Player;
using Game.GameRuntime.GameSceneManager.Component;
using Game.GameRuntime.GameSceneManager.Component.Story;
using Game.GameRuntime.UI.FormLogic.SystemTips;
using Game.Static.Enum.Goods;
using Game.Static.Enum.Map;
using Game.Static.Path;

namespace Game.GameRuntime.Entities.SceneEntities.HomeScene1
{
    public class HomeScene1GoOutStoryCollider : BaseSceneEntityLogic
    {
        protected internal override void OnInit(object userData)
        {
            base.OnInit(userData);
            
            // RegisterEvent("SetSignToHome", SetSignToHome);
            // RegisterEvent("SetSignToCity", SetSignToCity);
            // RegisterEvent("GetMap", GetMap);
            // RegisterEvent("GotoForest", GotoForest);
            
            componentSystem.GetComponent<InteractiveComponent>().onEnterInteractiveEvent += EnterEvent;
        }

        public override void OnShutDown()
        {
            base.OnShutDown();
            
            componentSystem.GetComponent<InteractiveComponent>().onEnterInteractiveEvent -= EnterEvent;
        }

        private void EnterEvent(InteractiveComponent component)
        {
            // 获取HomeScene1数据
            var homeScene1Data = SceneManager.GetArchiveData<HomeScene1Data>();
            var changeClothesSceneData = SceneManager.GetArchiveData<SelectClothesSceneData>();
            
            // 是否获取武器
            if (!SceneManager.GetArchiveData<PlayerBagData>().HasMainItem(EMainItemName.AiLinSword))
            {
                // 提示拿剑
                SceneManager.GetModule<StoryComponentGSM>().TriggerStory("HomeScene1TipsGetSword");
                return;
            }

            // already get map 换装和对话完成才触发
            if (homeScene1Data.getMap == false && changeClothesSceneData.exitTimes > 0 && homeScene1Data.xiaerDialogue)
            {
                GameManager.GetGMComponent<EntityComponentGM>().GetEntityLogic<PlayerLogic>().componentSystem.GetComponent<PlayerInputComponent>().SetAllowMove(false);
				// 触发剧情
				SceneManager.GetModule<StoryComponentGSM>().TriggerStory("HomeScene1GoOutStory");

/*				GameManager.GetGMComponent<UIComponentGM>().OpenUIForm(UIPrefabPath.GetUIPrefabPath("SystemTipsPanel"), EUIGroup.Top, new OpenFormArgs()
                {
                    userData = ESystemTipsType.GoOutHome,
                    callBack = logic =>
                    {
                        if (logic is SystemTipsFormLogic systemTipsFormLogic)
                        {
                            systemTipsFormLogic.GetProxy<SystemTipsFormProxy>().onSureEvent = () =>
                            {
                                // 触发剧情
                                SceneManager.GetModule<StoryComponentGSM>().TriggerStory("HomeScene1GoOutStory");
                            };
                            systemTipsFormLogic.GetProxy<SystemTipsFormProxy>().onCancelEvent = () =>
                            {
                                GameManager.GetGMComponent<EntityComponentGM>().GetEntityLogic<PlayerLogic>().componentSystem.GetComponent<PlayerInputComponent>().SetAllowMove(true);
                            };
                        }
                    }
                });
*/
                
            }
        }

        #region 出门剧情事件

        public void OnHomeScene1_GetMap()
        {
            SceneManager.GetModule<PlayerHandlerComponentGSM>().UnlockRoad(PlaceName.HomeToJingLingVillage);
        }
        #endregion
    }
}