using Game.GameMgr.Component.Archive.ArchiveDataClass.BaseDataClass;
using System;
using UnityEngine;

namespace Game.GameMgr.Component.Archive.ArchiveDataClass.Quest
{
    /// <summary>
    /// 玩家游戏币存档（方案 A：独立于道具背包）。
    /// 与 <see cref="PlayerQuestData"/> 同置于 Quest 命名空间，避免 QuestManager 跨命名空间引用失败。
    /// 任务发奖走 AddGold；商店购买走 TrySpendGold（调用方负责 Save）。
    /// </summary>
    [Serializable]
    public class PlayerGoldData : BaseArchiveData
    {
        private const string GoldKey = "PlayerGold";

        /// <summary>当前持有游戏币数量。</summary>
        [HideInInspector]
        public int gold;

        public override void ParseInternal(MasterGameData masterData)
        {
            gold = masterData.GetValue(GoldKey, 0);
        }

        public override void SerializeInternal(MasterGameData masterData)
        {
            masterData.SetValue(GoldKey, gold);
        }

        /// <summary>增加游戏币（任务发奖等）；amount ≤ 0 时忽略。</summary>
        public void AddGold(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            gold += amount;
        }

        /// <summary>
        /// 当前持有是否够付 amount（amount ≤ 0 视为不可付）。
        /// 替代方案：调用方自行比较 gold &gt;= amount——集中在此避免商店/任务各写一套。
        /// </summary>
        public bool CanAfford(int amount)
        {
            return amount > 0 && gold >= amount;
        }

        /// <summary>
        /// 尝试扣除游戏币。
        /// 失败条件：amount ≤ 0，或 gold &lt; amount（不足时不修改 gold）。
        /// 成功则 gold -= amount 并返回 true。
        /// 调用方负责 SavePlayerGold（本方法只改内存，不落盘）。
        /// 替代方案：SpendGold 不足时钳到 0 / 抛异常——禁止，商店需要明确失败分支。
        /// </summary>
        public bool TrySpendGold(int amount)
        {
            if (amount <= 0)
            {
                return false;
            }

            if (gold < amount)
            {
                return false;
            }

            gold -= amount;
            return true;
        }
    }
}
