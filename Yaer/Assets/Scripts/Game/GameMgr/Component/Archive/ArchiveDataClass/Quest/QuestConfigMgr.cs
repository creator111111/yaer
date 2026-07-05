using Game.DataTable.QuestConfig;
using Game.GameMgr.Component.Archive.ArchiveDataClass.Player;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.GameMgr.Component.Archive.ArchiveDataClass.Quest
{
    /// <summary>
    /// 任务静态配置只读管理器（阶段 1）。
    /// API 形态对齐 AchievementDataMgr.Init，但主键为 questId 字符串而非枚举。
    /// </summary>
    [Serializable]
    public class QuestConfigMgr
    {
        private const string ConfigPath = "Assets/GameRes/Config/QuestConfig/QuestConfig.json";

        private static QuestConfigMgr instance;
        private readonly Dictionary<string, QuestDataTableRow> questRowById = new Dictionary<string, QuestDataTableRow>();
        private bool isInitialized;

        public static QuestConfigMgr getInstance()
        {
            if (instance == null)
            {
                instance = new QuestConfigMgr();
            }

            return instance;
        }

        /// <summary>
        /// 异步加载 QuestConfig.json；重复调用幂等。
        /// 不使用 ResComponentGM.LoadConfig，因其 Dictionary&lt;string,string&gt; 无法解析 rewards 等嵌套数组。
        /// </summary>
        public void Init()
        {
            if (isInitialized)
            {
                return;
            }

            var resComponent = GameManager.GetGMComponent<ResComponentGM>();
            if (resComponent == null)
            {
                Debug.LogWarning("[QuestConfig] ResComponentGM 未就绪，跳过任务配置加载");
                return;
            }

            resComponent.LoadAsset<TextAsset>(ConfigPath, asset =>
            {
                if (asset == null || string.IsNullOrEmpty(asset.text))
                {
                    Debug.LogError("[QuestConfig] QuestConfig.json 加载失败或内容为空");
                    return;
                }

                try
                {
                    questRowById.Clear();
                    var jsonArray = JArray.Parse(asset.text);
                    foreach (var token in jsonArray)
                    {
                        if (token is JObject obj)
                        {
                            var row = QuestDataTableRow.FromJsonObject(obj);
                            if (string.IsNullOrEmpty(row.questId))
                            {
                                Debug.LogWarning("[QuestConfig] 跳过缺少 questId 的配置行");
                                continue;
                            }

                            if (questRowById.ContainsKey(row.questId))
                            {
                                Debug.LogWarning($"[QuestConfig] questId 重复: {row.questId}，后行覆盖前行");
                            }

                            questRowById[row.questId] = row;
                        }
                    }

                    isInitialized = true;
                    Debug.Log($"[QuestConfig] Loaded {questRowById.Count} quest(s).");
                    ValidateTargetMonsters();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[QuestConfig] JSON 解析失败: {ex.Message}");
                }
            });
        }

        /// <summary>按逻辑主键 questId 查询任务配置行，不存在返回 null。</summary>
        public QuestDataTableRow GetQuestRow(string questId)
        {
            if (string.IsNullOrEmpty(questId))
            {
                return null;
            }

            questRowById.TryGetValue(questId, out var row);
            return row;
        }

        /// <summary>已加载任务条数，供验收 Console 核对。</summary>
        public int GetQuestCount()
        {
            return questRowById.Count;
        }

        /// <summary>配置是否已完成首次加载。</summary>
        public bool IsInitialized => isInitialized;

        /// <summary>
        /// 校验 KillMonster 任务的 targetMonster 是否在 MonsterConfig.name 中存在。
        /// 怪物表未加载时静默跳过；MonsterDataMgr.Init 完成后会再次调用。
        /// </summary>
        public void ValidateTargetMonsters()
        {
            if (!isInitialized || questRowById.Count == 0)
            {
                return;
            }

            var monsterMgr = MonsterDataMgr.getInstance();
            if (!monsterMgr.IsTableLoaded)
            {
                return;
            }

            foreach (var row in questRowById.Values)
            {
                if (row.objectiveType != "KillMonster" || string.IsNullOrEmpty(row.targetMonster))
                {
                    continue;
                }

                if (!monsterMgr.TryGetMonsterIdByName(row.targetMonster, out _))
                {
                    Debug.LogWarning(
                        $"[QuestConfig] Quest '{row.questId}' 的 targetMonster '{row.targetMonster}' " +
                        "未在 MonsterConfig.name 中找到（大小写须一致）");
                }
            }
        }
    }
}
