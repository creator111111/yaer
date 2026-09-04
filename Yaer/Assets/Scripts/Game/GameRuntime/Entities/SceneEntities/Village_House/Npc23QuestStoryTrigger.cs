using Game.GameMgr.Component.Archive.ArchiveDataClass.Quest;
using Game.GameRuntime.Entities.SceneEntities;

namespace Game.GameRuntime.Entities.SceneEntities.Village_House
{
    /// <summary>
    /// HomeScene23 椅子 NPC（NpcChair）专用剧情触发器：按 Quest_002 状态 + 背包切对话。
    /// <para>
    /// 未接 → <c>Village_QuestOffer_NPC23</c>；
    /// InProgress 且藤蔓果不足 → <c>Village_QuestThanks_NPC23</c>（「感谢你」可循环）；
    /// InProgress 且够 5 个 → <c>Village_QuestTurnIn_NPC23</c>（交付句 + 扣果发奖）；
    /// TurnedIn → 暂播 Thanks（OPEN：可另做短句）。
    /// </para>
    /// <para>
    /// 禁止照搬埃吉尔用 Complete 切对话——CollectItem 交时查背包，到不了 Complete。
    /// 替代方案：场景里手改 StoryPrefabName——接取后不会自动换，正是本 bug 根因。
    /// </para>
    /// </summary>
    public class Npc23QuestStoryTrigger : SimpleStoryTrigger
    {
        private const string QuestId = "Quest_002";
        private const string OfferPrefab = "Village_QuestOffer_NPC23";
        private const string ThanksPrefab = "Village_QuestThanks_NPC23";
        private const string TurnInPrefab = "Village_QuestTurnIn_NPC23";

        /// <summary>
        /// 按任务状态与 <see cref="QuestManager.CanTurnInCollectQuest"/>（交时查背包）解析 Prefab 名。
        /// </summary>
        protected override string ResolveStoryPrefabName()
        {
            var mgr = QuestManager.getInstance();
            var state = mgr.GetQuestState(QuestId);

            // 未接取：推销/接任务长对白
            if (state == null)
            {
                return OfferPrefab;
            }

            // 已交付：首版仍播感谢循环，避免再回 Offer 选项
            if (state == QuestState.TurnedIn)
            {
                return ThanksPrefab;
            }

            // 进行中：够果走交付图，不够走「感谢你」短循环
            if (state == QuestState.InProgress)
            {
                return mgr.CanTurnInCollectQuest(QuestId) ? TurnInPrefab : ThanksPrefab;
            }

            // 兜底（如脏数据 Complete）：勿回 Offer 再 Accept
            return ThanksPrefab;
        }
    }
}
