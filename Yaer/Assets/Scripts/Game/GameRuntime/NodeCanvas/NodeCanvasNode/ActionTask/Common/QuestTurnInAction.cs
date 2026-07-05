using Game.GameMgr.Component.Archive.ArchiveDataClass.Quest;
using NodeCanvas.Framework;
using NodeCanvas.Tasks.Actions;
using ParadoxNotion.Design;

namespace Game.GameRuntime.Story.Node
{
    /// <summary>
    /// NodeCanvas 对话图交付任务节点，仿 <see cref="QuestAcceptAction"/>。
    /// 须在交付对白<strong>最后一句话之后</strong>调用（雅尔收尾句之后），再 TurnIn + 发奖。
    /// 埃吉尔线：挂在 <c>Village_Aegir_QuestTurnIn</c> 末句与 FightingPanel 收尾之间。
    /// </summary>
    [Category("Story")]
    [Name("交付任务")]
    public class QuestTurnInAction : ActionTask
    {
        /// <summary>与 QuestConfig.json 中 questId 完全一致。</summary>
        public BBParameter<string> questId;

        protected override string info =>
            "交付任务:" + (questId != null ? questId.value : string.Empty);

        protected override void OnExecute()
        {
            var mgr = QuestManager.getInstance();
            if (mgr.TurnInQuest(questId.value))
            {
                mgr.GrantQuestRewards(questId.value);
            }

            EndAction();
        }
    }
}
