# Village_Shop — 非首次进店 Village_ShopRepeat — 施工说明

**日期**：2026-08-30  
**角色**：施工员  
**依据**：`执行文档/0830/Village_Shop_非首次进店Village_ShopRepeat_架构溯源报告.md`  
**范围**：R1 — GSM 进店管线分支 Start / Repeat；**未**改 Repeat Prefab / 购买 Yes·No / Head。

---

## ① 结论一句话

第 1 次进店仍播 `Village_ShopStart`；第 2 次及以后每次播 `Village_ShopRepeat`，结束后再买卖。

---

## ② 改了什么 & 原因

| 文件 | 变更 | 原因 |
|------|------|------|
| `Village_ShopSceneManager.cs` | 常量 `ShopRepeatStoryName`；`TryDefer` 分支 Start/Repeat；`OnInit` 总藏 UI；`OnEnterScene` 兜底 Repeat；结束走 Special 语义 | 0827「二进宫静默」作废；防闪要对齐黑幕内 Trigger |

**行为钉死**

- Repeat **每次**非首次都播；**不**用 `CheckStoryUsed(Repeat)` 闸门  
- 进店黑幕 Defer 防闪 ✅；结束慢黑幕 ❌（对齐 Head/Yes/No）  
- 无雅/古分层 Prepare  

**替代（未采用）**：仅 OnEnterScene Trigger（易闪）；UI Awake 播剧情。

---

## ③ 验收清单（Unity Play）

| # | 操作 | 通过 |
|---|------|------|
| 1 | 新档第一次进店 | 播 **ShopStart**；买卖 UI 对白中不闪 |
| 2 | 同档第二次进店 | 播 **ShopRepeat**；对白中藏 UI、热区关；结束回 Idle |
| 3 | 第三次进店 | **再播** ShopRepeat |
| 4 | Repeat 中点头 / ESC | 不叠 Head/Yes/No；ESC 不离店 |
| 5 | Console | `TriggerStory Village_ShopRepeat`；无 Missing Prefab |
| 6 | 点头 / 购买成败 | 仍走 Head / Yes / No，**不是** Repeat |

---

## ④ 给程序

- diff 主文件仅 `Village_ShopSceneManager.cs`  
- Repeat ≠ Start / Yes / No / Head  
