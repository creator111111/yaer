using Game.GameMgr.Component.Archive;
using Game.GameMgr.Component.Archive.ArchiveDataClass.Player;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.GameMgr.Component.Archive.ArchiveDataClass.Quest
{
    /// <summary>
    /// 任务运行时管理器（阶段 2～3 最小集）：接取、查询进度、存档。
    /// API 形态参考 <see cref="Player.AchievementDataMgr"/>，主键为 questId 字符串。
    /// </summary>
    [Serializable]
    public class QuestManager
    {
        private static QuestManager instance;

        /// <summary>任务接取成功时触发，供阶段 5 左侧追踪 UI 订阅。</summary>
        public event Action<string> OnQuestAccepted;

        /// <summary>击杀进度变更时触发（questId, current, target），供阶段 5 左侧追踪 UI 订阅。</summary>
        public event Action<string, int, int> OnQuestProgressChanged;

        public static QuestManager getInstance()
        {
            if (instance == null)
            {
                instance = new QuestManager();
            }

            return instance;
        }

        /// <summary>获取当前存档中的任务运行时数据；首次访问时由 ArchiveComponentGM 懒加载。</summary>
        public PlayerQuestData GetPlayerQuestData()
        {
            var sceneMgr = GameManager.GetGameSceneManager();
            if (sceneMgr == null)
            {
                // 对话接取等流程通常已有场景管理器；兜底走 GM 存档组件。
                return GameManager.GetGMComponent<ArchiveComponentGM>().GetData<PlayerQuestData>();
            }

            return sceneMgr.GetArchiveData<PlayerQuestData>();
        }

        /// <summary>
        /// 接取任务：校验配置 → 幂等检查 → 写入 InProgress + 0 进度 → 存档。
        /// </summary>
        /// <param name="questId">与 QuestConfig.json 中 questId 完全一致，如 Quest_001。</param>
        public void AcceptQuest(string questId)
        {
            if (string.IsNullOrEmpty(questId))
            {
                Debug.LogWarning("[Quest] AcceptQuest 收到空 questId");
                return;
            }

            var configRow = QuestConfigMgr.getInstance().GetQuestRow(questId);
            if (configRow == null)
            {
                Debug.LogWarning($"[Quest] Unknown questId: {questId}");
                return;
            }

            var questData = GetPlayerQuestData();
            if (questData.questStates.TryGetValue(questId, out var existingState))
            {
                // 进行中 / 已达标未交：禁止重复 Accept
                if (existingState == QuestState.InProgress || existingState == QuestState.Complete)
                {
                    Debug.Log($"[Quest] Already accepted: {questId}");
                    return;
                }

                // 已交付：一律拒绝重接（方案 A，0831 改口）。
                // repeatable=true 仅表示「可经 ResetQuest 清锁后再接」，不再在此直接放行。
                // 替代方案 B：仅 Quest_003 特判拒绝——语义分裂，本期不用。
                if (existingState == QuestState.TurnedIn)
                {
                    Debug.Log($"[Quest] Already turned in: {questId}");
                    return;
                }
            }

            questData.questStates[questId] = QuestState.InProgress;
            questData.questProgress[questId] = 0;

            SaveQuestProgress();

            Debug.Log($"[Quest] Accept {questId}");
            Debug.Log($"[Quest] Progress {questId}: 0/{configRow.targetCount} ({QuestState.InProgress})");

            OnQuestAccepted?.Invoke(questId);
        }

        /// <summary>
        /// 将任务恢复为「未接取」：移除 <c>questStates</c> / <c>questProgress</c> 中该 id。
        /// 供日后跳日 / 新一天调用；无日期系统时由 Debug 菜单验收。
        /// 不改背包、不发奖、不自动播对白。
        /// </summary>
        /// <param name="questId">与 QuestConfig.json 中 questId 完全一致，如 Quest_003。</param>
        public void ResetQuest(string questId)
        {
            if (string.IsNullOrEmpty(questId))
            {
                Debug.LogWarning("[Quest] ResetQuest 收到空 questId");
                return;
            }

            // 配置校验：避免 Debug 打错 id 静默落空
            var configRow = QuestConfigMgr.getInstance().GetQuestRow(questId);
            if (configRow == null)
            {
                Debug.LogWarning($"[Quest] ResetQuest 未知 questId: {questId}");
                return;
            }

            var questData = GetPlayerQuestData();
            questData.questStates.Remove(questId);
            questData.questProgress.Remove(questId);

            SaveQuestProgress();

            Debug.Log($"[Quest] Reset {questId}");
        }

        /// <summary>返回所有进行中任务的 questId 列表，供阶段 5 UI 使用。</summary>
        public List<string> GetActiveQuests()
        {
            var result = new List<string>();
            var questData = GetPlayerQuestData();

            foreach (var kvp in questData.questStates)
            {
                if (kvp.Value == QuestState.InProgress)
                {
                    result.Add(kvp.Key);
                }
            }

            return result;
        }

        /// <summary>查询任务状态；未接取返回 null。</summary>
        public QuestState? GetQuestState(string questId)
        {
            if (string.IsNullOrEmpty(questId))
            {
                return null;
            }

            var questData = GetPlayerQuestData();
            return questData.questStates.TryGetValue(questId, out var state) ? state : (QuestState?)null;
        }

        /// <summary>
        /// 返回当前进度与目标数量；(0, 0) 表示配置不存在或未接取。
        /// <para>
        /// CollectItem（如 Quest_002/003）：真进度在背包 <c>targetItem</c> 数量，
        /// <c>questProgress</c> 接取后恒为 0、井不写——若只读字典会假显示 0/4。
        /// KillMonster 仍读 <c>questProgress</c>。
        /// </para>
        /// </summary>
        public (int currentCount, int targetCount) GetQuestProgress(string questId)
        {
            if (string.IsNullOrEmpty(questId))
            {
                return (0, 0);
            }

            var configRow = QuestConfigMgr.getInstance().GetQuestRow(questId);
            if (configRow == null)
            {
                return (0, 0);
            }

            // Collect：交时查包样板——查询进度也应对齐背包，避免 UI/Log 假 0/N
            if (configRow.objectiveType == "CollectItem" && !string.IsNullOrEmpty(configRow.targetItem))
            {
                var bag = GetPlayerBagData();
                var held = bag != null ? bag.GetMainItemCount(configRow.targetItem) : 0;
                return (held, configRow.targetCount);
            }

            var questData = GetPlayerQuestData();
            var current = questData.questProgress.TryGetValue(questId, out var count) ? count : 0;
            return (current, configRow.targetCount);
        }

        /// <summary>
        /// 怪物死亡统一入口（阶段 4）。仅处理 objectiveType==KillMonster 且 state==InProgress 的任务。
        /// 未接取或非进行中任务静默跳过，不打 Progress 日志。
        /// 替代方案：在 WoodWormLogic 等子类单独调用——仅覆盖蠕虫，漏掉其他 KillMonster 目标怪，不推荐。
        /// </summary>
        /// <param name="monsterName">MonsterConfig.name，大小写须与 QuestConfig.targetMonster 完全一致。</param>
        public void OnMonsterKilled(string monsterName)
        {
            if (string.IsNullOrEmpty(monsterName))
            {
                return;
            }

            Debug.Log($"[Quest] Kill report: {monsterName}");

            var configMgr = QuestConfigMgr.getInstance();
            var questData = GetPlayerQuestData();
            var anyChanged = false;

            foreach (var kvp in questData.questStates)
            {
                if (kvp.Value != QuestState.InProgress)
                {
                    continue;
                }

                var questId = kvp.Key;
                var row = configMgr.GetQuestRow(questId);
                if (row == null || row.objectiveType != "KillMonster")
                {
                    continue;
                }

                if (row.targetMonster != monsterName)
                {
                    continue;
                }

                var current = questData.questProgress.TryGetValue(questId, out var c) ? c : 0;
                if (current >= row.targetCount)
                {
                    // 已达标不再累加（Complete 前封顶，防止第 11 只继续涨）
                    continue;
                }

                current = Mathf.Min(current + 1, row.targetCount);
                questData.questProgress[questId] = current;
                anyChanged = true;

                if (current >= row.targetCount)
                {
                    questData.questStates[questId] = QuestState.Complete;
                    Debug.Log($"[Quest] Progress {questId}: {current}/{row.targetCount} (Complete)");
                }
                else
                {
                    Debug.Log($"[Quest] Progress {questId}: {current}/{row.targetCount} (InProgress)");
                }

                OnQuestProgressChanged?.Invoke(questId, current, row.targetCount);
            }

            if (anyChanged)
            {
                SaveQuestProgress();
            }
        }

        /// <summary>
        /// 杀怪线是否可交付：仅 Complete 返回 true。
        /// CollectItem（交时查背包）请用 <see cref="CanTurnInCollectQuest"/>，勿复用本方法——否则永远 false。
        /// </summary>
        public bool CanTurnInQuest(string questId)
        {
            return GetQuestState(questId) == QuestState.Complete;
        }

        /// <summary>
        /// CollectItem 是否可交：已接取且为 InProgress，且背包 targetItem 数量 ≥ targetCount。
        /// 产品口径：不靠拾取推 Complete；交的那一刻数书包。替代方案 B 是先扫包写 Complete 再走旧 TurnIn——易与杀怪语义混淆，不采用。
        /// </summary>
        public bool CanTurnInCollectQuest(string questId)
        {
            if (string.IsNullOrEmpty(questId))
            {
                return false;
            }

            var row = QuestConfigMgr.getInstance().GetQuestRow(questId);
            if (row == null || row.objectiveType != "CollectItem")
            {
                return false;
            }

            if (string.IsNullOrEmpty(row.targetItem) || row.targetCount <= 0)
            {
                return false;
            }

            if (GetQuestState(questId) != QuestState.InProgress)
            {
                return false;
            }

            var bag = GetPlayerBagData();
            if (bag == null)
            {
                return false;
            }

            return bag.GetMainItemCount(row.targetItem) >= row.targetCount;
        }

        /// <summary>
        /// 交付任务（杀怪线）：仅 Complete → TurnedIn；已 TurnedIn 或非 Complete 时幂等跳过（不重复发奖）。
        /// CollectItem 请走 <see cref="TryTurnInCollectQuest"/>。
        /// </summary>
        /// <returns>本次是否成功从 Complete 转为 TurnedIn。</returns>
        public bool TurnInQuest(string questId)
        {
            if (string.IsNullOrEmpty(questId))
            {
                Debug.LogWarning("[Quest] TurnInQuest 收到空 questId");
                return false;
            }

            var questData = GetPlayerQuestData();
            if (!questData.questStates.TryGetValue(questId, out var state))
            {
                Debug.LogWarning($"[Quest] TurnIn 失败，未接取: {questId}");
                return false;
            }

            if (state == QuestState.TurnedIn)
            {
                Debug.Log($"[Quest] Already turned in: {questId}");
                return false;
            }

            if (state != QuestState.Complete)
            {
                Debug.LogWarning($"[Quest] TurnIn 失败，状态非 Complete: {questId} ({state})");
                return false;
            }

            questData.questStates[questId] = QuestState.TurnedIn;
            SaveQuestProgress();

            Debug.Log($"[Quest] TurnIn {questId}");
            return true;
        }

        /// <summary>
        /// CollectItem 交付（方案 A）：InProgress + 背包够 → 扣 targetItem×targetCount → TurnedIn（跳过 Complete）。
        /// 原子性：先判数量 → Remove 成功才改状态；Remove 失败则整次失败，不 TurnIn、不发奖。
        /// 调用方须在成功后再 <see cref="GrantQuestRewards"/>（与杀怪线一致）。
        /// </summary>
        /// <returns>本次是否成功扣果并转为 TurnedIn。</returns>
        public bool TryTurnInCollectQuest(string questId)
        {
            if (string.IsNullOrEmpty(questId))
            {
                Debug.LogWarning("[Quest] TryTurnInCollectQuest 收到空 questId");
                return false;
            }

            var row = QuestConfigMgr.getInstance().GetQuestRow(questId);
            if (row == null)
            {
                Debug.LogWarning($"[Quest] CollectTurnIn 未知 questId: {questId}");
                return false;
            }

            if (row.objectiveType != "CollectItem")
            {
                Debug.LogWarning(
                    $"[Quest] CollectTurnIn 跳过，非 CollectItem: {questId} ({row.objectiveType})");
                return false;
            }

            if (string.IsNullOrEmpty(row.targetItem) || row.targetCount <= 0)
            {
                Debug.LogWarning(
                    $"[Quest] CollectTurnIn 配置无效 targetItem/targetCount: {questId}");
                return false;
            }

            var questData = GetPlayerQuestData();
            if (!questData.questStates.TryGetValue(questId, out var state))
            {
                Debug.LogWarning($"[Quest] CollectTurnIn 失败，未接取: {questId}");
                return false;
            }

            if (state == QuestState.TurnedIn)
            {
                Debug.Log($"[Quest] Already turned in: {questId}");
                return false;
            }

            // 产品：交时查包，不要求先 Complete；非 InProgress 一律拒（含误写成 Complete 的脏数据）
            if (state != QuestState.InProgress)
            {
                Debug.LogWarning(
                    $"[Quest] CollectTurnIn 失败，状态非 InProgress: {questId} ({state})");
                return false;
            }

            var bag = GetPlayerBagData();
            if (bag == null)
            {
                Debug.LogWarning($"[Quest] CollectTurnIn 失败，无背包数据: {questId}");
                return false;
            }

            var held = bag.GetMainItemCount(row.targetItem);
            if (held < row.targetCount)
            {
                Debug.LogWarning(
                    $"[Quest] CollectTurnIn 背包不足: {questId} {row.targetItem} {held}/{row.targetCount}");
                return false;
            }

            // 扣果失败（并发/数量变了）→ 整次失败，不改任务状态
            if (!bag.TryRemoveMainItem(row.targetItem, row.targetCount))
            {
                Debug.LogWarning(
                    $"[Quest] CollectTurnIn 扣物品失败: {questId} {row.targetItem} x{row.targetCount}");
                return false;
            }

            SavePlayerBag();

            questData.questStates[questId] = QuestState.TurnedIn;
            // 交成功时把 progress 对齐目标，便于 UI/日志；非拾取累加路径
            questData.questProgress[questId] = row.targetCount;
            SaveQuestProgress();

            Debug.Log(
                $"[Quest] CollectTurnIn {questId} removed {row.targetItem} x{row.targetCount}");
            Debug.Log($"[Quest] TurnIn {questId}");
            return true;
        }

        /// <summary>获取玩家背包存档；路径与任务/金币懒加载一致。</summary>
        public PlayerBagData GetPlayerBagData()
        {
            var sceneMgr = GameManager.GetGameSceneManager();
            if (sceneMgr == null)
            {
                return GameManager.GetGMComponent<ArchiveComponentGM>().GetData<PlayerBagData>();
            }

            return sceneMgr.GetArchiveData<PlayerBagData>();
        }

        /// <summary>扣果成功后落盘背包，对标商店购买后的 SaveSpcData。</summary>
        public void SavePlayerBag()
        {
            var archiveComponentGM = GameManager.GetGMComponent<ArchiveComponentGM>();
            if (archiveComponentGM != null)
            {
                archiveComponentGM.SaveSpcData<PlayerBagData>();
            }
        }

        /// <summary>
        /// 发放任务配置表中的奖励（首版仅 Gold）。须在 TurnInQuest 成功后由 QuestTurnInAction 调用。
        /// 替代方案 B：仅 Debug.Log 不进存档——不符合交付发 60 币验收。
        /// </summary>
        public void GrantQuestRewards(string questId)
        {
            if (string.IsNullOrEmpty(questId))
            {
                return;
            }

            var configRow = QuestConfigMgr.getInstance().GetQuestRow(questId);
            if (configRow == null)
            {
                Debug.LogWarning($"[Quest] GrantRewards 未知 questId: {questId}");
                return;
            }

            if (GetQuestState(questId) != QuestState.TurnedIn)
            {
                Debug.LogWarning($"[Quest] GrantRewards 跳过，状态非 TurnedIn: {questId}");
                return;
            }

            foreach (var reward in configRow.rewards)
            {
                if (reward == null || string.IsNullOrEmpty(reward.type))
                {
                    continue;
                }

                if (reward.type == "Gold")
                {
                    var goldData = GetPlayerGoldData();
                    goldData.AddGold(reward.amount);
                    SavePlayerGold();
                    Debug.Log($"[Quest] Grant Gold {reward.amount}");
                }
            }
        }

        /// <summary>获取玩家游戏币存档；懒加载路径与 PlayerQuestData 一致。</summary>
        public PlayerGoldData GetPlayerGoldData()
        {
            var sceneMgr = GameManager.GetGameSceneManager();
            if (sceneMgr == null)
            {
                return GameManager.GetGMComponent<ArchiveComponentGM>().GetData<PlayerGoldData>();
            }

            return sceneMgr.GetArchiveData<PlayerGoldData>();
        }

        /// <summary>实时保存任务进度至当前存档，对标 AchievementDataMgr.SaveAchievementProgress。</summary>
        public void SaveQuestProgress()
        {
            var archiveComponentGM = GameManager.GetGMComponent<ArchiveComponentGM>();
            if (archiveComponentGM != null)
            {
                archiveComponentGM.SaveSpcData<PlayerQuestData>();
            }
        }

        /// <summary>保存游戏币至当前存档。</summary>
        public void SavePlayerGold()
        {
            var archiveComponentGM = GameManager.GetGMComponent<ArchiveComponentGM>();
            if (archiveComponentGM != null)
            {
                archiveComponentGM.SaveSpcData<PlayerGoldData>();
            }
        }

        /// <summary>
        /// 商店等消费侧门面：Get → TrySpendGold → Save。
        /// 失败不改写、不落盘；成功才扣款并 SavePlayerGold。
        /// 替代方案：UI 自行调 GetPlayerGoldData + TrySpendGold + Save——效果等价，门面与任务发奖同路更不易漏存。
        /// </summary>
        public bool TrySpendPlayerGold(int amount)
        {
            var goldData = GetPlayerGoldData();
            if (goldData == null || !goldData.TrySpendGold(amount))
            {
                return false;
            }

            SavePlayerGold();
            return true;
        }
    }
}
