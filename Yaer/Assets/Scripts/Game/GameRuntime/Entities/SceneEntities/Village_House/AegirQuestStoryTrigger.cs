using Game.GameMgr.Component.Archive.ArchiveDataClass.Quest;
using Game.GameRuntime.Entities.SceneEntities;

namespace Game.GameRuntime.Entities.SceneEntities.Village_House
{
    /// <summary>
    /// 埃吉尔 NPC 专用剧情触发器：按 <see cref="Quest_001"/> 状态切换接任务/交付对白。
    /// <para>Complete → <c>Village_Aegir_QuestTurnIn</c>；其余 → <c>Village_Aegir_QuestOffer</c>。</para>
    /// <para>模式对齐 <see cref="HomeScene1.HomeScene1Xiaer"/> 双对话切换，避免改场景写死单个 StoryPrefabName。</para>
    /// </summary>
    public class AegirQuestStoryTrigger : SimpleStoryTrigger
    {
        private const string QuestId = "Quest_001";
        private const string OfferPrefab = "Village_Aegir_QuestOffer";
        private const string TurnInPrefab = "Village_Aegir_QuestTurnIn";

        /// <summary>
        /// 杀满 10 只虫子（Complete）时播交付对白；InProgress / 未接 / TurnedIn 首版仍播接任务对白（幂等）。
        /// </summary>
        protected override string ResolveStoryPrefabName()
        {
            var state = QuestManager.getInstance().GetQuestState(QuestId);
            return state == QuestState.Complete ? TurnInPrefab : OfferPrefab;
        }
    }
}
