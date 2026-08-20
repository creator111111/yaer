using GameFramework.UnityRuntime.DataTable;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Game.DataTable.QuestConfig
{
    /// <summary>
    /// 单条任务奖励（首版仅支持 Gold，结构预留扩展 Exp 等类型）。
    /// </summary>
    [System.Serializable]
    public class QuestRewardEntry
    {
        /// <summary>奖励类型，首版固定为 Gold。</summary>
        public string type;

        /// <summary>奖励数量。</summary>
        public int amount;
    }

    /// <summary>
    /// 任务配置表行，对应 QuestConfig.json 中一条任务。
    /// 逻辑主键为 questId（字符串），id 仅作 DataTable 行编号。
    /// </summary>
    public class QuestDataTableRow : DataRowBase
    {
        public int id;
        public string questId;
        public string title;
        public string title_en;
        public string title_jp;
        public string objectiveText;

        /// <summary>
        /// 目标类型：KillMonster（杀怪）/ CollectItem（提交物品）等。
        /// AcceptQuest 不校验此字段；KillMonster 校验与 OnMonsterKilled 只认 KillMonster。
        /// </summary>
        public string objectiveType;

        /// <summary>KillMonster：绑定 MonsterConfig.name，大小写须一致。CollectItem 行可留空。</summary>
        public string targetMonster;

        /// <summary>
        /// CollectItem：绑定主物品名（如 TenWangFruit / EMainItemName），与 MainItemConfig.name 一致。
        /// 接取不依赖本字段；交付查背包时再读。替代方案：临时把物品名塞进 targetMonster（不推荐）。
        /// </summary>
        public string targetItem;

        public int targetCount;
        public List<QuestRewardEntry> rewards = new List<QuestRewardEntry>();
        public List<string> prerequisiteQuestIds = new List<string>();

        /// <summary>预留：是否自动接取。</summary>
        public bool autoAccept;

        /// <summary>预留：是否可重复接取。</summary>
        public bool repeatable;
        public int sortOrder;

        public override int Id => id;

        /// <summary>
        /// 兼容 GF DataTable 扁平字典解析（不含 rewards 等嵌套字段）。
        /// 任务配置实际由 QuestConfigMgr 通过 JObject 完整解析，本方法作兜底。
        /// </summary>
        public override bool ParseDataRow(string dataRowString, object userData)
        {
            if (userData is Dictionary<string, string> jsonData)
            {
                try
                {
                    id = int.Parse(jsonData["id"]);
                    questId = jsonData["questId"];
                    title = jsonData["title"];
                    title_en = jsonData.ContainsKey("title_en") ? jsonData["title_en"] : "";
                    title_jp = jsonData.ContainsKey("title_jp") ? jsonData["title_jp"] : "";
                    objectiveText = jsonData["objectiveText"];
                    objectiveType = jsonData["objectiveType"];
                    targetMonster = jsonData.ContainsKey("targetMonster") ? jsonData["targetMonster"] : "";
                    // CollectItem 专用；旧表无此键时为空，不影响 KillMonster 行
                    targetItem = jsonData.ContainsKey("targetItem") ? jsonData["targetItem"] : "";
                    targetCount = int.Parse(jsonData["targetCount"]);
                    sortOrder = jsonData.ContainsKey("sortOrder") ? int.Parse(jsonData["sortOrder"]) : 0;
                }
                catch
                {
                    Debug.LogError("[QuestConfig] 任务配置扁平字段解析失败");
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 从 JSON 对象完整解析一行任务（含 rewards、prerequisiteQuestIds 等嵌套字段）。
        /// 替代方案：若全项目统一升级 LoadConfig 支持 JObject，可移除此静态工厂。
        /// </summary>
        internal static QuestDataTableRow FromJsonObject(JObject obj)
        {
            var row = new QuestDataTableRow
            {
                id = ParseInt(obj["id"]),
                questId = obj["questId"]?.ToString(),
                title = obj["title"]?.ToString(),
                title_en = obj["title_en"]?.ToString() ?? "",
                title_jp = obj["title_jp"]?.ToString() ?? "",
                objectiveText = obj["objectiveText"]?.ToString(),
                objectiveType = obj["objectiveType"]?.ToString(),
                targetMonster = obj["targetMonster"]?.ToString() ?? "",
                // 无 targetItem 键时为空串，KillMonster 行保持兼容
                targetItem = obj["targetItem"]?.ToString() ?? "",
                targetCount = ParseInt(obj["targetCount"]),
                autoAccept = ParseBool(obj["autoAccept"]),
                repeatable = ParseBool(obj["repeatable"]),
                sortOrder = ParseInt(obj["sortOrder"], defaultValue: 0),
            };

            if (obj["rewards"] is JArray rewardsArr)
            {
                foreach (var token in rewardsArr)
                {
                    if (token is JObject rewardObj)
                    {
                        row.rewards.Add(new QuestRewardEntry
                        {
                            type = rewardObj["type"]?.ToString(),
                            amount = ParseInt(rewardObj["amount"]),
                        });
                    }
                }
            }

            if (obj["prerequisiteQuestIds"] is JArray prereqArr)
            {
                foreach (var token in prereqArr)
                {
                    var questId = token?.ToString();
                    if (!string.IsNullOrEmpty(questId))
                    {
                        row.prerequisiteQuestIds.Add(questId);
                    }
                }
            }

            return row;
        }

        private static int ParseInt(JToken token, int defaultValue = 0)
        {
            if (token == null || token.Type == JTokenType.Null)
            {
                return defaultValue;
            }

            return int.TryParse(token.ToString(), out var value) ? value : defaultValue;
        }

        private static bool ParseBool(JToken token)
        {
            if (token == null || token.Type == JTokenType.Null)
            {
                return false;
            }

            var text = token.ToString();
            return text == "1" || text.Equals("true", System.StringComparison.OrdinalIgnoreCase);
        }
    }
}
