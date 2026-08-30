# Village_Shop — 购买堆叠上限 Console 提示 — 架构溯源报告

**文档版本**：v1.0（2026-08-29）  
**文档性质**：【架构侦探】只读核实（**本阶段未改代码**）  
**Unity**：2020.3.48f1  
**场景**：`Village_Shop` · 点「决定」  
**产品目标**：因 **背包堆叠上限** 买不成时，Console **必须**有明确提示（区分满堆 vs 没钱 / 无反应）  
**关联**：`TryValidateBuyStackLimits` · `ShopDebugLogger.LogStackOverflow` · `MaxStackPerItem=10` · ShopYes/ShopNo · 商店背包数量调试工具

---

## ① 结论一句话

**现网已闭环：堆叠预检在扣款前，超限必打 `LogWarning` + `[ShopDebug]`，文案含道具 ID / 持有 / 购买量 / 上限 /「整单取消」；不扣款、不入包、不播 ShopNo。裁定 L0 免施工——只补验收步骤；可选 P2 加中文显示名或 `[StackCap]` 子标签，非本期硬门槛。**

---

## ② 原因（通俗）

商店早就知道「再买会超 10 个就整单作废」，并且会在 Console 黄字喊出来。  
测试若感觉「没提示」，多半是没造满堆（held&lt;10 且 qty 不够超）、或 Console 没按 `[ShopDebug]` 过滤 / 被别的日志淹没——不是缺功能。

---

## ③ 用户怎么造满堆看 Console

| # | 操作 | 通过 |
|---|------|------|
| 1 | Play → `Tools/Debug/Shop Bag Quantity Tool…` → 某店货设为 **10**（或「商店货全满」） | 持有=Max |
| 2 | 进店，该行数量填 **≥1**，点「决定」 | Console **黄字**含堆叠上限文案；金币不变；**不**播 ShopNo |
| 3 | 过滤 Console：`ShopDebug` 或 `堆叠` | 一眼能找到 |
| 4 | 对照：该货设为 **9**，买 **1** | 成功入包（可播 Yes）；**无**堆叠 Warning |
| 5 | 对照：钱不够（旁路关） | 文案是「金币不足…」，**不是**堆叠句 |

样例原文（现网）：

```
[ShopDebug] 背包将超堆叠上限：SmallHpPotion 持有 10 + 购买 1 > 10，整单取消
```

（`itemId` 为 `EMainItemName.ToString()`，随道具变化。）

---

## ④ 给程序

### A. 现网调用链（已核实 · 路径可达）

```
OnConfirmClick
  → 出售 / 数量0 / bag空 → 其它 Log，return
  → TryValidateBuyStackLimits(bag, lines)     // 先于旁路扣款
       for 每行:
         held = GetMainItemCount(ItemId)
         if held + Quantity > MaxStackPerItem(=10):
           ShopDebugLogger.LogStackOverflow(id, held, qty, 10)  // LogWarning
           return false
       → false 时 OnConfirmClick 直接 return
            ❌ 不 TrySpend / 不 AddMainItem / 不 TryNotifyPurchaseDialogue
  →（通过后）才扣款 / 入包 / Yes·No
```

| 问 | 答 |
|----|-----|
| 扣款前？ | ✅ 预检在 `bypass`/`TrySpend` **之前** |
| 误播 No？ | ❌ 堆叠分支 **无** `TryNotifyPurchaseDialogue` |
| 误播 Yes？ | ❌ |
| 多行整单 | 遇 **第一个**超限行即 Log + 整单取消（够用） |
| `AddMainItem` 钳 10 | 预检阻止「钱已扣、道具被钳少到账」 |

### B. 现网文案与级别

| 项 | 值 |
|----|-----|
| API | `ShopDebugLogger.LogStackOverflow(string itemId, int held, int buyQty, int maxStack)` |
| 级别 | **`Debug.LogWarning`**（黄字，已够醒目） |
| 前缀 | **`[ShopDebug]`**（与缺金/零数量统一，可过滤） |
| 完整模板 | `{LogPrefix} 背包将超堆叠上限：{itemId} 持有 {held} + 购买 {buyQty} > {maxStack}，整单取消` |
| 缺金对照 | `[ShopDebug] 金币不足，需要 {need}，当前持有 {have}` —— **可区分** |

覆盖裁定：

| 情况 | 应打堆叠 Log？ | 现网 |
|------|----------------|------|
| held=10，买 1 | ✅ | ✅ `10+1>10` |
| held=8，买 3 | ✅ | ✅ |
| held=8，买 2 | ❌（刚好满应成功） | ✅ `8+2` 不 `>` |
| 金币不足 | ❌ | 走 `LogInsufficientGold` + No |
| 数量 0 | ❌ | `LogZeroQuantityWarning` |

### C. 缺口裁定

| 结论 | 条件 | 本期 |
|------|------|------|
| **免施工（L0）** | 路径通、Warning、文案可辨、与缺金可区分 | **✅ 采纳** |
| L1 加强 | 中文 displayName / `[ShopBuy][StackCap]` 子 tag | P2 可选，非必须 |
| 补日志 | 预检无 Log | ❌ 不适用（已有） |

**禁止**未核实就新做 Tips 系统；玩家 Tips UI **本期不做**。

### D. 最小施工清单

| # | 动作 | 优先级 |
|---|------|--------|
| — | **代码 diff：无** | — |
| 1 | 按 §③ 验收（满堆 / 差 1 / 缺金对照） | **P0（验收）** |
| 2 | （可选）文案加 Database `displayName` | P2 |
| 3 | （可选）前缀加 `[StackCap]` | P2 |
| 4 | 玩家 TipsForm | ❌ 本期不做 |

### E. 验收清单（必做）

| # | 操作 | 通过 |
|---|------|------|
| 1 | 店货 held=10，买 ≥1，决定 | 出现上列 Warning；金币不变；不播 No |
| 2 | held=9，买 1 | 成功；无堆叠 Warning |
| 3 | 过滤 `[ShopDebug]` | 堆叠句与「金币不足」可并存时语义不混 |
| 4 | 多行一超一未超 | 整单取消；至少打出超限那一行 |

### F. 开放问题

| ID | 问题 | 决议 | 状态 |
|----|------|------|------|
| Q1 | 现网够不够？ | **L0 够用，免施工** | ✅ |
| Q2 | 要不要 Tips UI？ | **本期否** | ✅ |
| Q3 | 是否列出全部超限行？ | **首行即可** | ✅ |
| Q4 | L1 显示名/子 tag？ | **P2 可选** | ✅ |

（已追加 `OPEN_QUESTIONS.md`。）
