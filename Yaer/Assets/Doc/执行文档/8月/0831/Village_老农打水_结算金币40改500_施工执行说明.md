# Village_老农打水 — 结算金币 40→500 — 施工执行说明

**文档版本**：v1.0（2026-08-31）  
**文档性质**：【施工员】配置改数 + 发奖路径只读核对  
**Unity**：2020.3.48f1  
**任务**：`Quest_003`「老农的浇地水」· `Village_KenMuNi1`  
**产品拍板**：结算 **500** 金币（废止暂定 40）  
**详细落盘**：`Assets/Doc/施工说明/0831/Village_老农打水_结算金币40改500_施工说明.md`

---

## ① 结论一句话

**`QuestConfig.json` 中 `Quest_003` Gold 已改为 500；结算只经 `QuestTurnInAction` → `GrantQuestRewards` 读表发奖，对话无硬编码加金、无配置副本，不双发。**

---

## ② 原因（通俗）

0830 接任务时奖金先写了 40，备注「待策划」。现在定了 500，只改表里那一个数就够——发奖代码本来就读配置，不用改逻辑。

---

## ③ 用户检查清单

| # | 操作 | 通过 |
|---|------|------|
| 1 | 接任务 → 满桶×4 → 走完完成结算对白 | 金币 **+500** |
| 2 | Console 过滤 `Grant Gold` | 出现 **`[Quest] Grant Gold 500`** |
| 3 | 再接再交一次 | 再 **+500**，不是 +40 / 不是一次 +1000 |
| 4 | 若刚改完仍见 40 | 停 Play 再进，确认读到新 JSON |

---

## ④ 给程序

### 改动

| 文件 | 变更 |
|------|------|
| `Assets/GameRes/Config/QuestConfig/QuestConfig.json` | `Quest_003.rewards[0].amount`：`"40"` → `"500"` |

### 调用链（已核实）

```
Village_老农打水任务_完成结算
  → QuestTurnInAction(questId=Quest_003)
  → TryTurnInCollectQuest 成功
  → GrantQuestRewards
  → QuestConfigMgr.GetQuestRow → rewards Gold amount
  → PlayerGoldData.AddGold + SavePlayerGold
```

| 自查项 | 结果 |
|--------|------|
| 其它 Quest 表 / SO 副本 | 无（`QuestConfigMgr` 仅 `QuestConfig.json`） |
| Prefab/CSV `AddGold(40/500)` | 无 |
| Tips/UI 写死「40」 | 无 |
| Quest_001/002 金额 | 未动 |

### 剩余风险

- 正式包若走 AB：发版前重打含本 JSON 的包。  
- 近 `MaxGold` 时钳顶丢弃多余，属既有经济规则。
