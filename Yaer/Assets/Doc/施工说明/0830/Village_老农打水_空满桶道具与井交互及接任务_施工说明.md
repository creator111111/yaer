# Village_KenMuNi1 — 老农打水：空/满桶 · 井 · 接任务 — 施工说明

**日期**：2026-08-30  
**角色**：施工员  
**依据**：`执行文档/0830/Village_老农打水_空满桶道具与井交互及接任务_架构溯源报告.md`  
**范围**：P0-A/B + P1 + P2 挂钩（完成结算 TurnIn）；Tip/Icon 为**占位图**（文案仍是旧血珠图，须美术替换）。

---

## ① 结论一句话

空桶/满桶已入库；井可换桶并弹 Tips；老农末句有帮/不帮，帮则接 `Quest_003`、发空桶×4+Tips；满×4 回老农可交付拿金（暂定 40）。

---

## ② 改了什么 & 原因

| 层 | 变更 | 原因 |
|----|------|------|
| `EMainItemName` + Database + JSON | `EmptyWaterBucket` / `FullWaterBucket`（Material、买卖 -1） | 表里原先无桶 |
| Tip 占位 | `GetEmptyWaterBucketx4` / `GetFullWaterBucket` ×三语 + atlas 登记 | 无图则静默不弹；**占位=血珠图，错字** |
| Icon | 暂复用藤蔓果 Icon | 可先验收背包 |
| `QuestConfig` | `Quest_003` CollectItem 满桶×4，Gold **40**（金额待策划） | Collect 样板对齐 Npc23 |
| `VillageWellLogic` + Objects/`Well` | 点井：InProgress+空≥1 → 空-1满+1+Tips；否则短对白 | 合层井 Z≠0 不能挂交互 |
| 短对白 | `Village_Well_NeedQuest` / `NoEmptyBucket` | 勿静默失败 |
| 主对白 | Choice 不帮/帮 → Accept+GetItem×4+OpenTips | 对齐报告 P1 |
| `FarmerQuestStoryTrigger` | 按 Collect 状态切 Offer / 进行中 / 完成结算 | **勿**抄埃吉尔 Complete |
| `_进行中` / `_完成结算` Prefab | 催促；结算句后 `QuestTurnInAction` | P1/P2 |

**未做**：正式 Tip/Icon 美术；报酬金额定稿；`_拒绝之后接受` 专线（拒后仍可再谈 Offer）。

---

## ③ 验收清单（Unity Play）

| # | 操作 | 通过 |
|---|------|------|
| 1 | Inspector 道具表 | 有空桶、满桶 |
| 2 | 未接任务点 `Well` | 短句「得先跟老农…」；**不**换桶 |
| 3 | 老农 → **帮** | 接任务；空桶×4；Tips（占位图可错字但应弹出） |
| 4 | **不帮** | 拒绝句；不发桶 |
| 5 | 点井 4 次 | 满×4、空×0；每次满桶 Tips |
| 6 | 满×4 回老农 | 结算对白 → 扣满×4 + 金币 |
| 7 | 再谈老农 | 不回接任务选项（进行中/已交循环） |
| 8 | Console | 无 Missing；缺 Tip 图才报「未找到Tips图片」 |

进 Unity 后请 **Pack Sprite Atlas**（或等自动）确认 tipsInfo 含新 Key。

---

## ④ 给程序

- 美术替换：`TipInfoAtlas*/GetEmptyWaterBucketx4.png`、`GetFullWaterBucket.png` 及专用 Icon。  
- Q4 报酬金额改 `QuestConfig.json` 即可。  
- Well 位可 Scene 微调贴合层「井」脚位 `(-68.115, 5.08, 0)`。
