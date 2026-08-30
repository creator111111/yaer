# MenuPanel·Money — 显示上限锁定为 6 位数 — 架构溯源报告

**文档版本**：v1.0（2026-08-29）  
**文档性质**：【架构侦探】只读核实 + 溢出策略拍板（**本阶段未改代码 / Prefab**）  
**Unity**：2020.3.48f1  
**目标**：`MenuPanel` · `ButtonMoney` / `Money_Digits` / `DigitStrip`  
**产品拍板**：图片数字显示上限 **6 位**（完整显示 **0～999999**）；自然位数、禁止前导零

> **⚠️ 2026-08-29 改口**：下文 **C1「仅显示钳制、存档/刷金不钳」已作废**。  
> 产品确认 **存档/逻辑上限亦为 999999**。真源见：  
> `0829/游戏金币数据上限999999_架构溯源报告.md`（数据硬顶 `PlayerGoldData.MaxGold`）。  
> 本报告仍保留「池=6 / Digit 防御」价值；**勿再按「存档可不钳」施工**。

关联：`0829/MenuPanel_Money对接商店图片数字与真实货币_架构溯源报告.md`  
组件：`UiSpriteNumberDisplay`（`ShopTotalPoolCapacity = 6`）

---

## ① 结论一句话

**池与 Prefab 已按 6 位装配（Digit_0～5、`poolCapacity=6`、对齐 Total2），但 `SetNumber` 对超大 gold 会按 `ToString()` 长度 `EnsurePoolSize` 扩出 Digit_6+，产品上限并未锁死。推荐溢出方案 C1：仅在 `RefreshMoneyFromArchive` 显示层 `Min(gold, 999999)` 再 `SetNumber`；存档/刷金工具不钳；常量 `MenuMoneyMaxDisplayValue = 999999`；默认不改 `UiSpriteNumberDisplay` 中枢。**  
**（↑ C1 存档不钳条款已废止，见文首改口；显示防御可保留，数据须硬顶。）**

---

## ② 原因（通俗）

盒子里准备了 6 个数字格，但往里塞「1000000」时，组件会**再砍一块木头做第 7 格**，底框和币标就可能挤坏。  
产品说「最多 6 位」= 菜单上永远只亮 ≤6 格；钱包里暂时堆更多金可以，菜单顶格显示 **999999**（并 Log 说明不一致）。

---

## ③ 用户怎么验

| # | 操作 | 通过 |
|---|------|------|
| 1 | gold=0 / 9999 / **999999** | 自然位数；**6 位完整在底框内**；无前导零 |
| 2 | gold≥1000000（测试改内存或多次 +9999） | 菜单仍最多 **6 位**（显示 999999）；**无 Digit_6** |
| 3 | 右侧币标 `Z` | 不被数字明显挡住 |
| 4 | 123 | 三位 `123`，不是 `000123` |

---

## ④ 给程序

### A. 「6」出现在哪里（核对表）

| 位置 | 当前值 | 与产品一致？ |
|------|--------|--------------|
| `UiSpriteNumberDisplay.ShopTotalPoolCapacity` | **6** | ✅ 常量齐 |
| `MenuFormLogic` EnsureOn `capacity` | `ShopTotalPoolCapacity` | ✅ |
| `ApplyShopTotalLayout` | 强制 `poolCapacity = 6` + fit | ✅ |
| `MenuMoneyDigitsSetupEditor` Bake | `capacity: ShopTotalPoolCapacity` + `ApplyShopTotalLayoutForBake` | ✅ |
| Prefab `DigitStrip.poolCapacity` | **6**；`fitWithinParentWidth=1`；`spacing=-12`；`digitAlignment=5`（MiddleRight） | ✅ |
| Prefab `Digit_*` | **Digit_0～Digit_5**（6 槽） | ✅ |
| Total2（对照） | 同常量 6 | ✅ 对齐 |
| `RefreshMoneyFromArchive` | `SetNumber(gold)` **无上限** | ❌ **缺口** |

### B. 超过 6 位时现网行为

```csharp
SetNumber(value) → SetDigitString(value.ToString())
  → EnsurePoolSize(digits.Length)
       // target = Max(requiredCount, poolCapacity, 1)
       // → gold=1000000 → requiredCount=7 → 创建 Digit_6
  → fitWithinParentWidth 可能整体缩小
```

| 问 | 答 |
|----|-----|
| 会不会创建 Digit_6？ | **会**（若未先钳制） |
| fit 能否救底框？ | 可能缩进框内，但仍是 **7 位语义**，违背「上限 6 位」；且易挤币标 |
| 压住 `Z`？ | Money_Digits 已 `sizeDelta.x=-36` 给币标留白；7 位+缩放过仍有风险 |

**推论**：设计池=6 ≠ 运行时锁死；**必须在 SetNumber 前钳显示值**。

### C. 溢出方案拍板

| 方案 | 裁定 |
|------|------|
| **C1 · 显示钳制** `Min(gold, 999999)` | **✅ 推荐** |
| C2 · 存档也钳 AddGold | ❌ 改经济规则，本期不做 |
| C3 · 允许 7+ 位 | ❌ 违背产品 |
| C4 · 满溢美术符号 | P2，无素材不做 |

**C1 细则**

- 挂点：`MenuFormLogic.RefreshMoneyFromArchive`（刷金工具也会调此 public 方法 → 一并受益）。  
- 常量（建议放 `MenuFormLogic` 或紧挨 Money 的静态常量）：  
  `MenuMoneyMaxDisplayValue = 999999`（= 10^6 - 1）；注释写清「产品显示上限 6 位」。  
- 可选复用：`ShopTotalPoolCapacity` 推导 `MaxDisplay = 10^capacity - 1`——可读，但 Menu 显式常量更不易误伤商店。  
- 当 `gold > 999999`：`SetNumber(999999)` + `Debug.LogWarning` 真实 gold vs 显示值。  
- **禁止**改 `PlayerGoldData` / `AddGold` / 刷金工具累加上限（除非产品另要金钱软顶）。  
- **禁止**用 PadLeft 凑满 6 位。  
- 默认 **不改** `EnsurePoolSize` 中枢（改全局会影响「故意显示超长串」的其它调用方）；Menu 侧钳制即可。

### D. 布局 / fit

| 项 | 结论 |
|----|------|
| ButtonMoney | ≈ 208×50 |
| Money_Digits | stretch 父，`sizeDelta=(-36,0)` 左/中留给数字、右侧留给 `Money (1)`/`Z` |
| spacing -12 | 与 Total2 一致，**保持** |
| 6 位 + fit | Prefab 已开 `fitWithinParentWidth`；施工后验 999999；若裁切再 P1 微调 Rect |

### E. 最小施工清单（本阶段不执行）

| # | 动作 | 优先级 |
|---|------|--------|
| 1 | `RefreshMoneyFromArchive`：`displayGold = Min(max(0,gold), 999999)` → `SetNumber` | **P0** |
| 2 | 常量 + 注释：产品上限 6 位 | **P0** |
| 3 | gold>上限时 Warning Log | P0 |
| 4 | 池保持 6；勿在未钳制时对超大 gold 调 SetNumber | **P0**（由 1 保证） |
| 5 | 验收 999999 / 1000000 | P0 |
| 6 | Prefab 底框微调（仅裁切时） | P1 |
| 7 | OPEN_QUESTIONS：位数已拍板；补「溢出仅显示钳制」 | P2 |

**预期 diff**

- 主要：`MenuFormLogic.cs`（常量 + Refresh 钳制）  
- 一般不改：`UiSpriteNumberDisplay.cs`、商店 Total2、Prefab（除非验收裁切）

### F. 验收清单

| # | 操作 | 通过 |
|---|------|------|
| 1 | 0 / 9999 / 999999 | 自然位；6 位进框 |
| 2 | ≥1000000 | 显示 999999；无 Digit_6 |
| 3 | 币标 | 不被挡 |
| 4 | 无前导零 | |

### G. 开放问题

| ID | 问题 | 决议 | 状态 |
|----|------|------|------|
| Q1 | 显示上限位数？ | **6（0～999999）** | ✅ 产品已确认 |
| Q2 | 溢出策略？ | **C1 仅显示钳制** | ✅ 本报告拍板 |
| Q3 | 存档/刷金是否软顶？ | **否** | ✅ |
| Q4 | 6 位是否裁切需改 Prefab？ | Play 验后再定 | 待验收 |

（已追加 / 更新 `OPEN_QUESTIONS.md`。）
