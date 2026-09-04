# Cursor Agent Prompt · MenuPanel·Money：显示上限锁定为 **6 位数**

> **角色**：先【架构侦探】只读核实 + 溢出策略拍板，再【施工员】  
> **日期**：2026-08-29  
> **目标**：`MenuPanel` · `ButtonMoney` / `Money_Digits` / `DigitStrip`  
> **产品拍板（用户已确认）**：金钱 **图片数字显示上限 = 6 位**（最大可完整显示 **0～999999**）  
> **视觉上下文**：Money 底框右侧为币标（如 `Z`），左侧/中部留给最多 6 位 DigitStrip（用户 Scene 框选区域）  
> **关联**：`0829/MenuPanel_Money对接商店图片数字与真实货币_架构溯源报告.md`（曾写 poolCapacity=6 对齐 Total2）  
> **本阶段侦探**：只读；禁止改代码 / Prefab  
> **报告落盘**：`Assets/Doc/执行文档/0829/MenuPanel_Money显示上限6位数_架构溯源报告.md`

把下面「侦探」整段复制给 Cursor Agent（Agent Mode）执行。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 产品需求（钉死 · 勿再开放成 5/7 位）

| 项 | 拍板 |
|----|------|
| 显示位数上限 | **6** |
| 完整显示范围 | **0 ～ 999999** |
| 与商店 | 对齐 `UiSpriteNumberDisplay.ShopTotalPoolCapacity = 6`（Total2 同款池） |
| 超过 6 位时 | **须拍板**：钳制显示 / 仍 ToString 截断视觉 / 禁止加到超限（见方案表） |

### 现网假说（须证伪）

| 层 | 预扫 |
|----|------|
| 常量 | `ShopTotalPoolCapacity = 6` ✅ 已存在 |
| Menu 装配 | `MenuMoneyDigitsSetupEditor` / `MenuFormLogic.ResolveMoneyDigits` 传 `capacity: ShopTotalPoolCapacity` |
| Prefab | `Digit_0`～`Digit_5` 共 6 槽（Bake 池） |
| `SetNumber` | `value.ToString()` **自然位数**；若 gold≥1000000（7 位+），`EnsurePoolSize` 可能 **扩池超过 6** 或挤爆底框——**缺口假说** |
| 存档 | `PlayerGoldData.gold` 为 `int`，理论可远超 999999；显示层与存档层是否钳制未统一 |

> **推论**：池「设计按 6」≠「运行时永不显示第 7 位」。本期要把产品上限 **写死到行为**：显示不超过 6 位，底框内 fit 不裁切乱版。

### 超过 6 位时的策略（侦探必选一个）

| 方案 | 行为 | 优点 | 风险 | 倾向 |
|------|------|------|------|------|
| **C1 · 显示钳制** | `SetNumber(Mathf.Min(gold, 999999))`；存档可更大 | 底框永远 ≤6 位；实现小 | 菜单显示与真实 gold 不一致（须 Log/注释） | **✅ 显示层推荐** |
| **C2 · 存档也钳** | AddGold / 刷金工具也不让超过 999999 | 显示=真实 | 改经济规则，影响面大 | ❌ 除非产品要金钱软顶 |
| **C3 · 池扩到自然位** | 7 位也显示，靠 fit 缩小 | — | **违背「上限 6 位」拍板** | ❌ |
| **C4 · 显示 `999999` + 标记** | 满溢符号 | 需美术 | 本期无素材则不做 | P2 |

刷金工具 +9999：若多次累加超过 999999，按 C1 菜单仍显示 999999（报告写清）。

### 布局验收（配合用户底框）

| 位数 | 期望 |
|------|------|
| 1（如 0、9） | 靠币标一侧对齐（现网 MiddleRight 等），不漂 |
| 4（如 9999） | 完整可见 |
| **6（999999）** | **完整可见、不裁切、不明显压住 `Z` 币标** |
| 7+（若未钳） | 按拍板 C1 不应出现第 7 格 |

### 本期边界

| 做 | 不做 |
|----|------|
| ✅ 核实 Menu/Total2 池=6 是否一致 | ❌ 改商店 Price 行默认 5 位池（除非误伤） |
| ✅ 拍板溢出 C1/C2… | ❌ 新做满溢美术（除非选 C4） |
| ✅ 最小改：显示钳制 + 注释/常量 `MenuMoneyMaxDisplay=999999` | ❌ 改 `PlayerGoldData` 存档类型 |
| ✅ 验收 6 位撑满底框 | ❌ 重做 ButtonMoney 美术框 |

### 严禁（侦探阶段）

- 改代码 / Prefab  
- 把上限改回 5 或扩到 7+  
- 用前导零凑满 6 位（`000123`）——仍禁止，保持自然位数  

---

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0829/MenuPanel_Money对接商店图片数字与真实货币_架构溯源报告.md
@Assets/Doc/提示词/0829/开发工具_一键加9999金币_架构侦探提示词.md
@Assets/Scripts/Game/GameRuntime/UI/Component/UiSpriteNumberDisplay.cs
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Menu/MenuFormLogic.cs
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Menu/Editor/MenuMoneyDigitsSetupEditor.cs
@Assets/GameRes/Prefabs/UI/MenuPanel.prefab

你现在是【架构侦探】。Unity 2020.3.48f1 / C#。
禁止修改代码、Prefab、场景。只读 + 写「Money 显示上限 6 位」报告。

## 背景

用户已确认：Menu 金钱图片数字 **上限 6 位数**。底框右侧币标、左侧留给最多 6 位。查清现网是否真锁死，以及 gold>999999 时怎么办。

## 任务清单

### A. 钉死「6」出现在哪些地方

| 位置 | 当前值 | 是否与产品一致 |
|------|--------|----------------|
| `ShopTotalPoolCapacity` | | |
| Menu EnsureOn / ApplyShopTotalLayout capacity | | |
| Prefab Digit_* 数量 | | |
| Inspector 序列化 poolCapacity | | |
| Total2（对照） | | |

### B. 钉死「超过 6 位」现网行为

`SetNumber(1000000)` 或 gold 很大时：

- 会不会创建 Digit_6？  
- fitWithinParentWidth 是否把字缩到框内？  
- 会否压住 `Z`？  

### C. 溢出方案拍板

在预梳理 C1～C4 中选推荐；写清：

- 显示 API 是否 `Min(gold, 999999)`  
- 是否新增命名常量（如 `MenuMoneyMaxDisplayValue = 999999`）避免魔法数  
- 刷金工具 / AddGold 是否联动钳存档（默认 **否**，只钳显示）  

### D. 布局 / fit

ButtonMoney 底框能否稳定放下 6 位 + 币标？间距 -12 是否仍适用？要否只调 Prefab Rect（施工 P1）。

### E. 最小施工清单（本阶段不执行）

| # | 动作 | 优先级 |
|---|------|--------|
| 1 | `RefreshMoneyFromArchive`（或等价）显示钳制到 6 位 | **P0** |
| 2 | 常量注释：产品上限 6 位 = 999999 | P0 |
| 3 | 池 capacity 保持 6；禁止 Ensure 时按超大 ToString 无脑扩到 7+（若 SetNumber 会扩池，须在钳制后调用） | **P0** |
| 4 | 验收 999999 撑满；1000000 显示仍 6 位 | P0 |
| 5 | Prefab 底框微调（仅当 6 位裁切） | P1 |
| 6 | 文档 / OPEN_QUESTIONS 关掉「位数未定」 | P2 |

### F. 验收清单

| # | 操作 | 通过 |
|---|------|------|
| 1 | gold=0 / 9999 / **999999** | 自然位数；6 位完整在框内 |
| 2 | gold≥1000000（测试可改内存） | 按拍板最多显示 6 位；不出现第 7 个 Digit |
| 3 | 币标 `Z` | 不被数字明显遮挡 |
| 4 | 无前导零 | 123 → 三位不是 000123 |

## 输出

写入：`Assets/Doc/执行文档/0829/MenuPanel_Money显示上限6位数_架构溯源报告.md`

MASTER 四段式：①结论（6 位已锁 + 溢出用 C?）②原因③用户怎么验 6 位④给程序：核对表 + 钳制挂点 + 最小 diff。
```

---

## 施工员续跑（侦探报告拍板后贴）

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0829/MenuPanel_Money显示上限6位数_架构溯源报告.md
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Menu/MenuFormLogic.cs
@Assets/Scripts/Game/GameRuntime/UI/Component/UiSpriteNumberDisplay.cs
@Assets/GameRes/Prefabs/UI/MenuPanel.prefab

你现在是【施工员】。按报告把 Menu 金钱显示上限落实为 **6 位数（0～999999）**。

必须遵守：
- 产品上限 6 位已拍板，禁止扩到 7+ 或改回 5；
- 溢出按报告方案（默认倾向显示钳制，不擅自改存档金钱上限）；
- SetNumber 前保证传入值 ≤999999，避免 Digit 池涨到 7；
- 保持自然位数、无前导零；代码含详细注释说明上限原因。

提交说明：改了哪些文件、如何验 999999 与超限、未做项。
```
