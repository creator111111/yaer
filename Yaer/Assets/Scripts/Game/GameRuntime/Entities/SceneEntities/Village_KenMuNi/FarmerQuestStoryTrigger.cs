using Game.GameMgr.Component.Archive.ArchiveDataClass.Quest;
using Game.GameRuntime.Entities.SceneEntities;

namespace Game.GameRuntime.Entities.SceneEntities.Village_KenMuNi
{
    /// <summary>
    /// 老农 <c>Npc_Farmer</c> 剧情触发器：按 Quest_003 CollectItem 状态切对话。
    /// <para>
    /// 未接（含 <see cref="QuestManager.ResetQuest"/> 清锁后）→ <c>Village_老农打水任务</c>（含帮/不帮）；
    /// InProgress 且满桶不足 → <c>Village_老农打水任务_进行中</c>；
    /// InProgress 且可交 → <c>Village_老农打水任务_完成结算</c>（仅此门禁，防空跑）；
    /// TurnedIn（当日已交、未 Reset）→ <c>Village_老农打水任务_今日已完成</c> 短循环。
    /// </para>
    /// <para>
    /// 产品再改口（0831）：交完当日不可再接；重接仅经 <c>ResetQuest</c>。
    /// 覆盖 0830「TurnedIn→Offer / Accept 放行重接」。样板：<see cref="Village_House.Npc23QuestStoryTrigger"/>。
    /// </para>
    /// <para>
    /// 重要原因：CollectItem 交时查背包，到不了 Complete；禁止照搬埃吉尔 Complete 切图。
    /// 禁空跑硬约束：非「InProgress &amp;&amp; CanTurnIn」绝不准进结算图。
    /// </para>
    /// </summary>
    public class FarmerQuestStoryTrigger : SimpleStoryTrigger
    {
        private const string QuestId = "Quest_003";
        private const string OfferPrefab = "Village_老农打水任务";
        private const string InProgressPrefab = "Village_老农打水任务_进行中";
        private const string TurnInPrefab = "Village_老农打水任务_完成结算";
        /// <summary>交完当日短循环；无 Accept / 无 TurnIn / 无发桶。</summary>
        private const string DoneTodayPrefab = "Village_老农打水任务_今日已完成";

        /// <summary>
        /// 按任务状态与 <see cref="QuestManager.CanTurnInCollectQuest"/> 解析 Prefab 名。
        /// </summary>
        protected override string ResolveStoryPrefabName()
        {
            var mgr = QuestManager.getInstance();
            var state = mgr.GetQuestState(QuestId);

            // 未接取（含 Reset 后无键）：推销 / 接任务长对白
            if (state == null)
            {
                return OfferPrefab;
            }

            // 已交付：短循环，禁止回 Offer，禁止进结算（防空跑）
            if (state == QuestState.TurnedIn)
            {
                return DoneTodayPrefab;
            }

            // 进行中：够桶走交付图，不够走催促
            if (state == QuestState.InProgress)
            {
                return mgr.CanTurnInCollectQuest(QuestId) ? TurnInPrefab : InProgressPrefab;
            }

            // 兜底（脏数据 Complete 等）：催促短句，勿误切交付 / Offer
            return InProgressPrefab;
        }
    }
}
