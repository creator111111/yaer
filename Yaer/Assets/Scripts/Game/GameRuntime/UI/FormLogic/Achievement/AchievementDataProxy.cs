using Game.GameMgr.Component.PureMVC.Base;

namespace Game.GameRuntime.UI.FormLogic.Achievement
{
    public class AchievementDataProxy : BaseProxy
    {
        public new const string NAME = "AchievementDataProxy";

        public AchievementDataProxy() : base(NAME)
        {
        }

        public void UpdateAchievement(int id, float count)
        {
        }
    }
}