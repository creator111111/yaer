using Game.GameMgr.Component.Archive.ArchiveDataClass.Player;
using Game.GameMgr.Component.Archive.ArchiveDataClass.Scene;
using Game.GameRuntime.Entities.Base.BaseSceneObj;
using Game.GameRuntime.Entities.Component.Interactive;
using Game.GameRuntime.GameSceneManager.Component;
using Game.GameRuntime.GameSceneManager.Component.Story;
using Game.Static.Enum.Goods;

namespace Game.GameRuntime.Entities.SceneEntities.HomeScene1
{
    public class HomeScene1Xiaer : BaseSceneEntityLogic
    {
        protected internal override void OnInit(object userData)
        {
            base.OnInit(userData);

            componentSystem.GetComponent<InteractiveComponent>().onClickInteractiveEvent += OnInteractive;
        }

        private void OnInteractive(InteractiveComponent component)
        {
            // save data
            var homeScene1Data = SceneManager.GetArchiveData<HomeScene1Data>();
            if (homeScene1Data.xiaerDialogue == false)
            {
                SceneManager.GetModule<StoryComponentGSM>().TriggerStory("HomeScene1GoOutXiaer");
            }
            else
            {
                SceneManager.GetModule<StoryComponentGSM>().TriggerStory("HomeScene1XiaerFinally");
            }
        }

        public void OnHomeScene1_GetXiaerPower()
        {
            SceneManager.GetArchiveData<PlayerBagData>().AddMainItem(EMainItemName.XiaerPower);
            SceneManager.GetModule<TipsComponentGSM>().OpenTipsForm("GetXiaerPower");
        }

        public void OnHomeScene1GoOutXiaerEnd()
        {
            var homeScene1Data = SceneManager.GetArchiveData<HomeScene1Data>();
            if (homeScene1Data.xiaerDialogue)
            {
                return;
            }
            homeScene1Data.xiaerDialogue = true;

            var bagData = SceneManager.GetArchiveData<PlayerBagData>();
            bagData.AddMainItem(EMainItemName.MpBall, 3);
            SceneManager.GetModule<TipsComponentGSM>().OpenTipsForm("GetMpBall");

            bagData.AddMainItem(EMainItemName.HpBall, 3);
            SceneManager.GetModule<TipsComponentGSM>().OpenTipsForm("GetHpBall");
        }
    }
}