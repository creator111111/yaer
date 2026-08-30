# Cursor Agent Prompt · 商店购买：堆叠达上限时 **Console 必有提示**

> **角色**：先【架构侦探】核实现网，缺口再交【施工员】  
> **日期**：2026-08-29  
> **产品目标（白话）**：购买道具时若会因 **背包堆叠上限** 买不成，**Console 必须打出明确提示**（让测试能立刻看出是「满了」而不是没钱/没反应）  
> **场景**：`Village_Shop` · 点「决定」  
> **关联**：`TryValidateBuyStackLimits` · `ShopDebugLogger.LogStackOverflow` · `MaxStackPerItem`（预扫 10）· ShopYes/ShopNo（满堆 **不**播没钱 No）· 背包数量调试工具  
> **本阶段侦探**：只读；禁止改代码  
> **报告落盘**：`Assets/Doc/执行文档/0829/Village_Shop_购买堆叠上限Console提示_架构溯源报告.md`

把下面「侦探」整段复制给 Cursor Agent（Agent Mode）执行。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 产品需求（钉死）

| 项 | 期望 |
|----|------|
| 何时提示 | 点「决定」后，因 **持有+购买量 > 堆叠上限** 整单取消时 |
| 提示通道 | **至少 Console**（`Log` / `LogWarning`，带统一前缀，可过滤） |
| 提示内容 | 能看懂：哪个道具、当前持有、想买多少、上限多少、整单已取消 |
| 不混淆 | **不是**金币不足；**不要**播 `Village_ShopNo`（没钱文案） |
| 玩家 Tips UI | **本期默认不要求**（除非侦探发现产品另有口径）；先保证 Console |

### 现网假说（须证伪 · 可能已有）

预扫 `ShopFormLogic`：

```
点决定
  → TryValidateBuyStackLimits
       held + qty > MaxStackPerItem
         → ShopDebugLogger.LogStackOverflow(itemId, held, buyQty, maxStack)
         → return（不扣款、不入包、不播 No）
```

预扫文案：

```
[Shop…] 背包将超堆叠上限：{itemId} 持有 {held} + 购买 {buyQty} > {maxStack}，整单取消
```

| 假说 | 侦探动作 |
|------|----------|
| 已有 Log 且路径可达 | 写清调用链 + 验收步骤；若文案/级别不够再开最小增强 |
| Log 有但从未打出 | 查旁路、校验顺序、held 读错 |
| 完全没有 | 施工补 LogStackOverflow 级提示 |
| 只要 Debug.Log 太弱 | 改为 LogWarning + 固定前缀（如 `[ShopBuy][StackCap]`） |

**禁止**未核实就写「从零新做一套提示系统」。

### 「达到上限」覆盖哪些情况

| 情况 | 是否应 Console | 说明 |
|------|----------------|------|
| held=10，再买 1 | ✅ | 已满 |
| held=8，买 3（上限 10） | ✅ | 将超 |
| held=8，买 2 | ❌ 不应报上限 | 刚好满，应成功 |
| 金币不足 | ❌ 走缺金 Log / ShopNo | 别混进堆叠文案 |
| 数量全 0 | ❌ 另 Warning | |

多行整单：任一行超限 → 整单取消；Console **至少报出第一个超限行**（或列出全部，侦探选最小够用）。

### 文案增强可选项（非必须，侦探按「够不够测」裁定）

| 级别 | 内容 |
|------|------|
| **L0 现网够用** | 保持 `LogStackOverflow`，只补验收文档 |
| **L1 加强 Console** | Warning + 中文显示名（Database displayName）+ 统一 tag |
| **L2 + 玩家 Tips** | 本期默认不做 |

### 与其它系统边界

| 系统 | 关系 |
|------|------|
| ShopYes / ShopNo | 堆叠失败 **不** Trigger No |
| 金币不足 Log | 前缀/文案须可区分 |
| 背包调试工具 | 可先把某货设为 10 再买，专测本提示 |
| `AddMainItem` 内部钳 10 | 预检必须在扣款前；Console 证明预检生效 |

### 本期边界

| 做 | 不做 |
|----|------|
| ✅ 核实/补齐堆叠失败 Console | ❌ 改 MaxStack 数值（除非报告单列） |
| ✅ 验收表（满堆点决定必见 Log） | ❌ 默认做 UI Tips |
| ✅ 与缺金提示区分 | ❌ 改 Yes/No 台本 |

### 严禁（侦探阶段）

- 改代码  
- 把堆叠失败接去播 ShopNo  
- 未搜现网 `LogStackOverflow` 就宣称缺失  

### 须对拍

| 资产 | 用途 |
|------|------|
| `ShopFormLogic.OnConfirmClick` / `TryValidateBuyStackLimits` | 是否调用 |
| `ShopDebugLogger.LogStackOverflow` | 文案与级别 |
| `PlayerBagData.MaxStackPerItem` | 上限值 |
| 背包数量调试工具提示词/报告 | 造满堆数据 |

---

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/提示词/0829/Village_Shop_购买成败对话_ShopYes_ShopNo_架构侦探提示词.md
@Assets/Doc/提示词/0829/开发工具_商店货单背包数量调试_架构侦探提示词.md
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Shop/ShopFormLogic.cs
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Shop/ShopDebugLogger.cs
@Assets/Scripts/Game/GameMgr/Component/Archive/ArchiveDataClass/Player/PlayerBagData.cs

你现在是【架构侦探】。Unity 2020.3.48f1 / C#。
禁止修改代码。只读 + 写「购买堆叠上限 Console 提示」短报告。

## 背景

购买因背包堆叠上限失败时，测试要在 Console 立刻看到明确提示。可能现网已有 `LogStackOverflow`——先核实是否够用，再决定免改 / 小增强。

## 任务

### A. 钉死现网链路
点决定 → 堆叠预检 → Log？→ return？扣款前？是否误播 No？

### B. 现网文案与级别
完整字符串、Log 还是 Warning、前缀、参数（id/held/qty/max）。

### C. 缺口裁定
| 结论 | 条件 |
|------|------|
| **免施工** | 路径通、文案可辨、验收能稳定打出 |
| **小增强 L1** | 有 Log 但不够醒目/缺中文名/缺统一 tag |
| **补日志 L0→有** | 预检有 return 无 Log，或 Log 死代码 |

### D. 验收步骤（必写）
1. 用背包工具或一键加满，使店内某货 held=Max  
2. 该行数量填 ≥1，点决定  
3. Console 必须出现上限提示；金币不变；不播 ShopNo  
4. 再测 held=Max-1 买 1 → 成功且无上限 Warning  

### E. 最小施工清单（仅当非免施工）
| # | 动作 | 优先级 |
|---|------|--------|
| 1 | 保证超限必打 Console | P0 |
| 2 | 文案含道具名/held/qty/max/整单取消 | P0 |
| 3 | Warning + 可过滤前缀 | P1 |
| 4 | 显示名 | P2 |

## 输出

写入：`Assets/Doc/执行文档/0829/Village_Shop_购买堆叠上限Console提示_架构溯源报告.md`

MASTER 四段式：①结论（已有够用 / 要增强哪点）②原因③用户怎么造满堆看 Console④给程序：调用链 + 是否 diff。
```

---

## 施工员续跑（仅当报告要求改代码时贴）

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0829/Village_Shop_购买堆叠上限Console提示_架构溯源报告.md
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Shop/ShopFormLogic.cs
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Shop/ShopDebugLogger.cs

你现在是【施工员】。按报告保证：购买因堆叠上限失败时 Console 有明确提示。

必须遵守：
- 仅动堆叠失败提示链路；不误接到 ShopNo；
- 预检仍在扣款前；整单取消；
- 若报告为免施工，只回复验收步骤，不改代码；
- 注释说明用途（测试识别满堆拒买）。

提交说明：改了什么 / 或免改依据、Console 样例原文、如何验收。
```
