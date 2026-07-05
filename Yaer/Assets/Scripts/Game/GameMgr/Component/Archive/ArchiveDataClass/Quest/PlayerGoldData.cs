using Game.GameMgr.Component.Archive.ArchiveDataClass.BaseDataClass;
using System;
using UnityEngine;

namespace Game.GameMgr.Component.Archive.ArchiveDataClass.Quest
{
    /// <summary>
    /// 玩家游戏币存档（方案 A：独立于道具背包）。
    /// 与 <see cref="PlayerQuestData"/> 同置于 Quest 命名空间，避免 QuestManager 跨命名空间引用失败。
    /// 首版仅支持任务奖励累加；Menu UI 对接留待后续。
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
    }
}
