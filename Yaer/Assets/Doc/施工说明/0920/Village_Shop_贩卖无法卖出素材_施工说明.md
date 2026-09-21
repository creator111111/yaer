# Village_Shop · 贩卖无法卖出素材 — 施工说明

**文档版本**：v1.0（2026-09-20）  
**文档性质**：【施工员】最小改动  
**Unity**：2020.3.48f1  
**侦探报告**：`Assets/Doc/执行文档/0920/Village_Shop_贩卖无法卖出素材_架构溯源报告.md`

---

## 沟通摘要

### ① 结论一句话

**贩卖点「决定」现在会真的扣素材、加金币。**

### ② 原因（通俗）

以前贩卖页只让你看列表、填数量、算合计，结算故意没写，点决定就打一句「出售结算未接入」回来。现在按购买对称接上了：先检查背包够不够，够就整单出包、加钱、两边都存档。

### ③ 用户检查清单

正规从 InitScene 进游戏，进村商店。

| # | 操作 | 通过判据 |
|---|------|----------|
| 1 | 点「贩卖 / SELL」 | Console 有 `[ShopDebug] 切换到出售页` |
| 2 | 素材行填 qty&gt;0，看 Total2 | 合计跟着变（卖价×数量） |
| 3 | 点「决定」（背包够） | Console 有 `出售出包成功`；背包数量减少；金币按合计增加；数量清零；Total2 归零 |
| 4 | 读档后再看 | 金币与背包仍与卖完后一致 |
| 5 | 故意填超过持有的数量再点决定 | Console Warning「背包不足」；背包与金币都不动 |
| 6 | 切回购买，原流程再买一次 | 购买仍扣钱入包；Console **不应**再出现「出售结算未接入」 |
| 7 | 出售成功 / 失败 | **不应**播 `Village_ShopYes` / `ShopNo`（本期默认不接） |

### ④ 程序补充

见下文。

---

## 改动清单

| 路径 | 改动 |
|------|------|
| `Assets/Scripts/Game/GameRuntime/UI/FormLogic/Shop/ShopFormLogic.cs` | 出售分支走 `OnConfirmSellClick`：收集卖行 → 预检 held≥qty → `TryRemoveMainItem` → `AddGold` + `SavePlayerGold` + `SavePlayerBag` → 清零 |
| `Assets/Scripts/Game/GameRuntime/UI/FormLogic/Shop/ShopDebugLogger.cs` | 增 `LogSellFromBag` / `LogInsufficientBag` / `LogSellRemoveFailed`；`LogSellNotImplemented` 保留但不走正常路径；零数量文案改为「无法交易」 |
| `Assets/Doc/OPEN_QUESTIONS.md` | 贩卖结算节 Q2/Q3 标为已施工；Q1 对白仍待产品 |

**未改**：购买分支语义、`bypassGoldCheckForBagJoint`、Bake / Prefab / 场景、`QuestManager` / `PlayerGoldData` / `PlayerBagData` API、`Village_ShopYes` / `ShopNo`。

---

## 为什么这样改

出售与购买分叉，避免把卖行塞进购买结算（会按买价扣钱并往包里加素材）。

先整单预检再扣包，禁止「卖到没有」半单。落盘顺序：先出包内存 → 加币并 SaveGold → SaveBag，避免读档「有钱还在果 / 有果没钱」。

对白默认不播：OPEN Q1 未拍板，禁止擅自复用购买 Yes。

替代方案：先只加钱不扣包 —— 读档白嫖金币，已否决。
