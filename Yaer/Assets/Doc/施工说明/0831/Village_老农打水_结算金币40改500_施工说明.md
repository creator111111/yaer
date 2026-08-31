# Village_老农打水 — 结算金币 40→500 — 施工说明

**日期**：2026-08-31  
**角色**：施工员  
**产品拍板**：`Quest_003`「老农的浇地水」交满桶×4 结算应得 **500** 金币（原暂定 40 已作废）  
**关联提示词**：`Assets/Doc/提示词/0831/Village_老农打水_结算金币40改500_施工员提示词.md`

---

## ① 结论一句话

只改了 `QuestConfig.json` 里 `Quest_003` 的 Gold `amount`：**40 → 500**；发奖仍只走 TurnIn → 读表，无双发。

---

## ② 改了什么 & 原因

| 文件 | 变更 | 原因 |
|------|------|------|
| `Assets/GameRes/Config/QuestConfig/QuestConfig.json` | `Quest_003.rewards[0].amount`：`"40"` → `"500"` | 策划拍板结算 500 金 |

**未改**（自查后确认无需动）：

| 候选 | 结果 |
|------|------|
| Editor / SO / 其它 Quest 表副本 | **无**；`QuestConfigMgr` 只读上述 JSON |
| `_完成结算` Prefab / CSV 硬编码 `AddGold` | **无**；图内仅有 `QuestTurnInAction` |
| UI / Tips 文案写死「40」 | **无** |
| 桶道具 / 井 / 接任务 / 其它 Quest 金额 | **不动**（本期边界） |

---

## ③ 发奖路径（一句话）

```
完成结算对白末尾 QuestTurnInAction
  → TryTurnInCollectQuest(Quest_003) 成功
  → QuestManager.GrantQuestRewards
  → 读 QuestConfig.rewards → AddGold(amount) + Save
```

数额唯一来源是配置表；对话图不再加金。

---

## ④ 验收清单

- [ ] 接 `Quest_003` → 打满桶×4 → 老农完成结算对白结束
- [ ] 金币 **+500**（不是 +40）
- [ ] Console 可见 `[Quest] Grant Gold 500`
- [ ] 交完可再接流程下，**再交一次仍 +500**（`repeatable=true`）
- [ ] 一次结算不应 +1000（无双发）；Console 无异常

**注意**：若 Editor 已 Play 过且配置缓存未刷新，请 **停 Play 再进**，确认加载的是改后的 JSON。

---

## ⑤ 剩余风险

| 风险 | 说明 |
|------|------|
| AB / StreamingAssets 旧包 | 若正式包体从 AB 读配置，发版前须按 README 重打/拷贝含本 JSON 的包；Editor 直读 `GameRes` 一般即生效 |
| 近顶金 | `PlayerGoldData.MaxGold=999999`；接近顶时 +500 可能被钳，属既有规则 |
