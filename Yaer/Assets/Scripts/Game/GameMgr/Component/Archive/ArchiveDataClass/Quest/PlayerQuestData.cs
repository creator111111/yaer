using Game.GameMgr.Component.Archive.ArchiveDataClass.BaseDataClass;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.GameMgr.Component.Archive.ArchiveDataClass.Quest
{
    /// <summary>
    /// 玩家任务运行时存档数据，形态对齐 <see cref="Player.AchievementData"/>。
    /// 内存用字典维护；落盘时拆分为 Quest_{questId}_State / Quest_{questId}_Count 分列键，便于调试。
    /// 替代方案：单 JSON blob 存整表，首版分列键与成就 Achievement_{id} 风格一致。
    /// </summary>
    [Serializable]
    public class PlayerQuestData : BaseArchiveData
    {
        private const string StateKeyPrefix = "Quest_";
        private const string StateKeySuffix = "_State";
        private const string CountKeySuffix = "_Count";

        /// <summary>questId → 当前击杀/收集进度。</summary>
        [HideInInspector]
        public Dictionary<string, int> questProgress = new Dictionary<string, int>();

        /// <summary>questId → 任务状态。</summary>
        [HideInInspector]
        public Dictionary<string, QuestState> questStates = new Dictionary<string, QuestState>();

        /// <summary>
        /// 从主存档解析任务数据：扫描所有 Quest_*_State / Quest_*_Count 键，不依赖配置表是否已加载。
        /// </summary>
        public override void ParseInternal(MasterGameData masterData)
        {
            questProgress.Clear();
            questStates.Clear();

            if (masterData?.data == null)
            {
                return;
            }

            foreach (var kvp in masterData.data)
            {
                if (TryParseQuestIdFromKey(kvp.Key, StateKeySuffix, out var questIdForState))
                {
                    questStates[questIdForState] = (QuestState)Convert.ToInt32(kvp.Value);
                    continue;
                }

                if (TryParseQuestIdFromKey(kvp.Key, CountKeySuffix, out var questIdForCount))
                {
                    questProgress[questIdForCount] = Convert.ToInt32(kvp.Value);
                }
            }
        }

        /// <summary>将内存字典写回主存档分列键。</summary>
        public override void SerializeInternal(MasterGameData masterData)
        {
            foreach (var kvp in questStates)
            {
                masterData.SetValue(BuildKey(kvp.Key, StateKeySuffix), (int)kvp.Value);
            }

            foreach (var kvp in questProgress)
            {
                masterData.SetValue(BuildKey(kvp.Key, CountKeySuffix), kvp.Value);
            }
        }

        private static string BuildKey(string questId, string suffix)
        {
            return StateKeyPrefix + questId + suffix;
        }

        /// <summary>从 Quest_{questId}_State 或 Quest_{questId}_Count 解析 questId。</summary>
        private static bool TryParseQuestIdFromKey(string key, string suffix, out string questId)
        {
            questId = null;
            if (string.IsNullOrEmpty(key)
                || !key.StartsWith(StateKeyPrefix, StringComparison.Ordinal)
                || !key.EndsWith(suffix, StringComparison.Ordinal))
            {
                return false;
            }

            var innerLength = key.Length - StateKeyPrefix.Length - suffix.Length;
            if (innerLength <= 0)
            {
                return false;
            }

            questId = key.Substring(StateKeyPrefix.Length, innerLength);
            return !string.IsNullOrEmpty(questId);
        }
    }
}
