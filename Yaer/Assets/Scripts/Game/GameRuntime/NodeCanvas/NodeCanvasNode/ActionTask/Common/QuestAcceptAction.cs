using Game.GameMgr.Component.Archive.ArchiveDataClass.Quest;
using NodeCanvas.Framework;
using NodeCanvas.Tasks.Actions;
using ParadoxNotion.Design;

namespace Game.GameRuntime.Story.Node
{
    /// <summary>
    /// NodeCanvas 对话图接取任务节点，仿 <see cref="AchievementRecordAction"/>。
    /// 须在玩家台词（如「我会努力的！」）播完之后再调用，避免未演出就签收任务。
    /// 埃吉尔线：挂在 Statement #15 与收尾 Action #17 之间，参数 questId = Quest_001。
    /// </summary>
    [Category("Story")]
    [Name("接取任务")]
    public class QuestAcceptAction : ActionTask
    {
        /// <summary>与 QuestConfig.json 中 questId 完全一致。</summary>
        public BBParameter<string> questId;

        protected override string info =>
            "接取任务:" + (questId != null ? questId.value : string.Empty);

        protected override void OnExecute()
        {
            QuestManager.getInstance().AcceptQuest(questId.value);
            EndAction();
        }
    }
}
