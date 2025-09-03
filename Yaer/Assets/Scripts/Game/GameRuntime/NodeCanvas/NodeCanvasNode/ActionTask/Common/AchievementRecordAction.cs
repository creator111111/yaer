using Game.GameMgr.Component.UI;
using Game.GameMgr;
using Game.Static.Path;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using Game.GameMgr.Component.Archive.ArchiveDataClass.Player;
using NodeCanvas.Tasks.Actions;

namespace Game.GameRuntime.Story.Node
{
    [Category("Story")]
    [Name("记录某个成就的进度")]
    // 用于剧情对话系统中的事件处理
    public class AchievementRecordAction : ActionTask
    {
        public BBParameter<AchievementType> achievementId; // 成就ID
        public BBParameter<int> achievementAddValue; // 成就增加的进度值

        string achievementName;
        protected override string OnInit()
        {
            return base.OnInit();
        }

        protected override string info { 
            get
            {
                return "成就:" + achievementId.value + "进度增加" + achievementAddValue.value;
            }
        }

        protected override void OnExecute()
        {
            AchievementDataMgr.getInstance().RecordAchievementProgress(achievementId.value, achievementAddValue.value);

            EndAction();
        }
    }
}