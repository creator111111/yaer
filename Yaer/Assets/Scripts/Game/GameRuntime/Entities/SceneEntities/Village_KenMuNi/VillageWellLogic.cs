using Game.GameMgr;
using Game.GameMgr.Component.Archive.ArchiveDataClass.Player;
using Game.GameMgr.Component.Archive.ArchiveDataClass.Quest;
using Game.GameRuntime.Entities.Base.BaseSceneObj;
using Game.GameRuntime.Entities.Component.Interactive;
using Game.GameRuntime.GameSceneManager.Component;
using Game.GameRuntime.GameSceneManager.Component.Story;
using Game.Static.Enum.Goods;
using UnityEngine;

namespace Game.GameRuntime.Entities.SceneEntities.Village_KenMuNi
{
    /// <summary>
    /// 村井交互：任务进行中且持有空桶时，空桶-1 / 满桶+1，并弹出同款获得道具 Tips。
    /// <para>
    /// 对齐卧室宝箱 <c>HomeScene2Box.OnHomeScene2Box_GetSword</c>：入包与横幅分两步；
    /// 合层「井」仅美术，本 Logic 挂在 Objects/<c>Well</c>（Z=0）上。
    /// </para>
    /// <para>
    /// 状态机（报告拍板）：
    /// - Quest_003 非 InProgress → 短对白 <see cref="NeedQuestStory"/>（不成兑换）；
    /// - InProgress 且空桶&lt;1 → 短对白 <see cref="NoEmptyBucketStory"/>；
    /// - 成功 → TryRemove 空桶 + Add 满桶 + OpenTipsForm + <see cref="QuestManager.SavePlayerBag"/>。
    /// 满桶可&gt;4，允许打到空桶用尽；交任务只扣 4。
    /// </para>
    /// 替代方案：对话图 GetItemActionTask——井是场景物，点击路径用 C# 更稳。
    /// </summary>
    public class VillageWellLogic : BaseSceneEntityLogic
    {
        private const string QuestId = "Quest_003";

        /// <summary>未接任务点井时的短反馈 Prefab 名。</summary>
        [SerializeField]
        private string NeedQuestStory = "Village_Well_NeedQuest";

        /// <summary>任务中但无空桶时的短反馈 Prefab 名。</summary>
        [SerializeField]
        private string NoEmptyBucketStory = "Village_Well_NoEmptyBucket";

        /// <summary>每次成功打水的 Tips Key（须进 tipsInfo 图集）。</summary>
        [SerializeField]
        private string FullBucketTipKey = "GetFullWaterBucket";

        private InteractiveComponent interactive;

        protected internal override void OnInit(object userData)
        {
            base.OnInit(userData);
            interactive = componentSystem.GetComponent<InteractiveComponent>();
            if (interactive != null)
            {
                interactive.onClickInteractiveEvent += OnClickWell;
            }
        }

        public override void OnShutDown()
        {
            if (interactive != null)
            {
                interactive.onClickInteractiveEvent -= OnClickWell;
            }

            base.OnShutDown();
        }

        /// <summary>点井：校验任务与空桶后换桶并弹 Tips。</summary>
        private void OnClickWell(InteractiveComponent component)
        {
            var questState = QuestManager.getInstance().GetQuestState(QuestId);
            if (questState != QuestState.InProgress)
            {
                // 可点但不成兑换：短对白提示先找老农
                TriggerFeedback(NeedQuestStory);
                return;
            }

            var bag = SceneManager.GetArchiveData<PlayerBagData>();
            if (bag == null)
            {
                Debug.LogError("[VillageWell] PlayerBagData 为空，无法换桶。");
                return;
            }

            if (bag.GetMainItemCount(EMainItemName.EmptyWaterBucket) < 1)
            {
                TriggerFeedback(NoEmptyBucketStory);
                return;
            }

            // 先扣空桶再加满桶，避免扣失败却已加满
            if (!bag.TryRemoveMainItem(EMainItemName.EmptyWaterBucket, 1))
            {
                Debug.LogWarning("[VillageWell] TryRemove 空桶失败。");
                TriggerFeedback(NoEmptyBucketStory);
                return;
            }

            bag.AddMainItem(EMainItemName.FullWaterBucket, 1);
            SceneManager.GetModule<TipsComponentGSM>().OpenTipsForm(FullBucketTipKey);

            // 重要原因：AcceptQuest 会立刻 SaveSpcData 写整份盘，但背包若未 Serialize，
            // 盘上仍是旧桶数；井后再有任意 SaveSpcData 还会用旧背包盖文件。
            // 对齐商店买完 / Collect 交付：换桶成功立刻 SavePlayerBag。
            QuestManager.getInstance().SavePlayerBag();
        }

        private void TriggerFeedback(string storyName)
        {
            if (string.IsNullOrEmpty(storyName))
            {
                return;
            }

            SceneManager.GetModule<StoryComponentGSM>()?.TriggerStory(storyName);
        }
    }
}
