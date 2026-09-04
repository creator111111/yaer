using Game.GameMgr.Component.Archive.ArchiveDataClass.Quest;
using NodeCanvas.Framework;
using NodeCanvas.Tasks.Actions;
using ParadoxNotion.Design;
using UnityEngine;

namespace Game.GameRuntime.Story.Node
{
    /// <summary>
    /// NodeCanvas 对话图交付任务节点，仿 <see cref="QuestAcceptAction"/>。
    /// 须在交付对白<strong>最后一句话之后</strong>调用，再 TurnIn + 发奖。
    /// 按 QuestConfig.objectiveType 分支：
    /// - KillMonster：旧 <see cref="QuestManager.TurnInQuest"/>（须已 Complete，埃吉尔线不变）
    /// - CollectItem：<see cref="QuestManager.TryTurnInCollectQuest"/>（交时查背包扣物，跳过 Complete）
    /// 替代方案：单独做「提交物品任务」Action——Prefab 要认两个节点名；同 Action 分支更省事且不误伤埃吉尔。
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
            var id = questId != null ? questId.value : null;
            var mgr = QuestManager.getInstance();
            var row = QuestConfigMgr.getInstance().GetQuestRow(id);

            bool turnedIn;
            if (row != null && row.objectiveType == "CollectItem")
            {
                // Quest_002：InProgress + 背包够 → 扣果 → TurnedIn；不够则 false，不发奖
                turnedIn = mgr.TryTurnInCollectQuest(id);
            }
            else
            {
                // Quest_001 等杀怪线：仅 Complete → TurnedIn
                turnedIn = mgr.TurnInQuest(id);
            }

            if (turnedIn)
            {
                mgr.GrantQuestRewards(id);
            }
            else if (row != null && row.objectiveType == "CollectItem")
            {
                // 对白层「果不够」由交付 Prefab/触发器另定；此处只保证不发奖
                Debug.Log($"[Quest] CollectTurnIn Action 未成功（不发奖）: {id}");
            }

            EndAction();
        }
    }
}
