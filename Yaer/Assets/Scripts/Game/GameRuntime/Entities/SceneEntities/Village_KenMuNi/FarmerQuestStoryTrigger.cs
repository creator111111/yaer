using Game.GameMgr.Component.Archive.ArchiveDataClass.Quest;
using Game.GameRuntime.Entities.SceneEntities;

namespace Game.GameRuntime.Entities.SceneEntities.Village_KenMuNi
{
    /// <summary>
    /// 老农 <c>Npc_Farmer</c> 剧情触发器：按 Quest_003 CollectItem 状态切对话。
    /// <para>
    /// 未接 / <b>已交付（可重复）</b> → <c>Village_老农打水任务</c>（含帮/不帮）；
    /// InProgress 且满桶不足 → <c>Village_老农打水任务_进行中</c>；
    /// InProgress 且可交 → <c>Village_老农打水任务_完成结算</c>。
    /// </para>
    /// <para>
    /// 产品改口（0830）：交完同一档可再接，不必读接取前档。
    /// <c>Quest_003.repeatable=true</c> + <see cref="QuestManager.AcceptQuest"/> 允许 TurnedIn 重接。
    /// </para>
    /// <para>
    /// 重要原因：CollectItem 交时查背包，到不了 Complete；禁止照搬埃吉尔 Complete 切图。
    /// 样板：<see cref="Village_House.Npc23QuestStoryTrigger"/>（状态机）；回 Offer 对齐埃吉尔 TurnedIn。
    /// </para>
    /// </summary>
    public class FarmerQuestStoryTrigger : SimpleStoryTrigger
    {
        private const string QuestId = "Quest_003";
        private const string OfferPrefab = "Village_老农打水任务";
        private const string InProgressPrefab = "Village_老农打水任务_进行中";
        private const string TurnInPrefab = "Village_老农打水任务_完成结算";

        /// <summary>
        /// 按任务状态与 <see cref="QuestManager.CanTurnInCollectQuest"/> 解析 Prefab 名。
        /// </summary>
        protected override string ResolveStoryPrefabName()
        {
            var mgr = QuestManager.getInstance();
            var state = mgr.GetQuestState(QuestId);

            // 未接，或已交付可再帮：播含 Choice 的 Offer
            if (state == null || state == QuestState.TurnedIn)
            {
                return OfferPrefab;
            }

            if (state == QuestState.InProgress)
            {
                return mgr.CanTurnInCollectQuest(QuestId) ? TurnInPrefab : InProgressPrefab;
            }

            // 兜底（脏数据 Complete 等）：催促短句，勿误切交付
            return InProgressPrefab;
        }
    }
}
