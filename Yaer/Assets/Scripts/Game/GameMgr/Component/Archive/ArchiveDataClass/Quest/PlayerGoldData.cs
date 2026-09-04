using Game.GameMgr.Component.Archive.ArchiveDataClass.BaseDataClass;
using System;
using UnityEngine;

namespace Game.GameMgr.Component.Archive.ArchiveDataClass.Quest
{
    /// <summary>
    /// 玩家游戏币存档（方案 A：独立于道具背包）。
    /// 与 <see cref="PlayerQuestData"/> 同置于 Quest 命名空间，避免 QuestManager 跨命名空间引用失败。
    /// 任务发奖走 AddGold；商店购买走 TrySpendGold（调用方负责 Save）。
    /// 产品硬顶：<see cref="MaxGold"/>（0829 改口：存档/逻辑上限，非仅菜单显示钳制）。
    /// </summary>
    [Serializable]
    public class PlayerGoldData : BaseArchiveData
    {
        private const string GoldKey = "PlayerGold";

        /// <summary>
        /// 游戏币硬顶（含 0）：存档与逻辑一律 ≤ 此值。
        /// 原因：废止「仅显示 6 位、存档可无限」旧 C1；刷金/发奖不得再造出千万级脏数据。
        /// </summary>
        public const int MaxGold = 999999;

        /// <summary>当前持有游戏币数量；合法区间 [0, <see cref="MaxGold"/>]。</summary>
        [HideInInspector]
        public int gold;

        public override void ParseInternal(MasterGameData masterData)
        {
            gold = masterData.GetValue(GoldKey, 0);
            // F1：读档钳回合法区间，修复历史刷金造成的超标档（如 21100219）。
            if (ClampGoldToLegalRange())
            {
                Debug.LogWarning(
                    $"[PlayerGold] 读档后钳回合法区间 → gold={gold}（上限 {MaxGold}）。下次 Save 会写回磁盘。");
            }
        }

        public override void SerializeInternal(MasterGameData masterData)
        {
            masterData.SetValue(GoldKey, gold);
        }

        /// <summary>
        /// 将 <see cref="gold"/> 钳到 [0, <see cref="MaxGold"/>]。
        /// </summary>
        /// <returns>是否发生了修改（调用方可据此决定是否立刻 Save）。</returns>
        /// <remarks>
        /// 读档 F1 与 Debug F2 共用；禁止业务侧裸写 gold= 绕过本方法。
        /// </remarks>
        public bool ClampGoldToLegalRange()
        {
            var before = gold;
            if (gold < 0)
            {
                gold = 0;
            }

            if (gold > MaxGold)
            {
                gold = MaxGold;
            }

            return gold != before;
        }

        /// <summary>
        /// 增加游戏币（任务发奖 / 刷金等）；amount ≤ 0 时忽略。
        /// 触顶：多余丢弃，结果永不超 <see cref="MaxGold"/>（例 999990+100 → 999999）。
        /// </summary>
        /// <remarks>
        /// 用 long 累加再钳，避免 int 溢出后变负再「+=」污染钱包。
        /// 替代方案：只改刷金窗不改本方法——任务发奖仍可爆顶，已否决。
        /// </remarks>
        public void AddGold(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            // long 防溢：gold+amount 可能超过 int.MaxValue
            var sum = (long)gold + amount;
            if (sum >= MaxGold)
            {
                gold = MaxGold;
                return;
            }

            gold = (int)sum;
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
