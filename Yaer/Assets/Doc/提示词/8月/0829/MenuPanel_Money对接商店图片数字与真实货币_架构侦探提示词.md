# Cursor Agent Prompt · MenuPanel·Money：对接真实货币 + 复用商店图片数字逻辑

> **角色**：先【架构侦探】只读溯源，报告拍板后再【施工员】  
> **日期**：2026-08-29  
> **目标 UI**：`Assets/GameRes/Prefabs/UI/MenuPanel.prefab` 的 **Money** 区域（含 `ButtonMoney` / `Money` / `Money (1)` 等子节点，侦探以 Hierarchy 为准钉死）  
> **产品目标（白话）**：  
> 1. **先查清商店**里价格/合计的 **图片数字**是怎么刷的（组件、素材、SetNumber、间距、位数）  
> 2. 再把 **同一套显示逻辑**接到 Menu 的 Money 上  
> 3. 数字内容对接 **真实玩家货币**（`PlayerGoldData.gold`），打开菜单能看见当前持有金币  
> **本阶段**：只读；禁止改代码 / Prefab / 场景  
> **报告落盘**：`Assets/Doc/执行文档/0829/MenuPanel_Money对接商店图片数字与真实货币_架构溯源报告.md`

把下面「侦探」整段复制给 Cursor Agent（Agent Mode）执行。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 产品需求（钉死）

| 项 | 期望 |
|----|------|
| 显示形态 | **图片数字**（0～9 Sprite 横排），**与商店 Price / Total2 同一套逻辑** |
| 数据真源 | **真实玩家货币** `PlayerGoldData.gold`（经 `QuestManager.GetPlayerGoldData` 等现网门面） |
| 刷新时机 | 至少 **打开 MenuPanel 时**刷新；若有更稳妥事件（关店回村后再 ESC）写入清单 |
| 禁止 | 系统字体 Text/TMP 冒充；菜单自建第二套「本地金币」；固定补零（200→0200） |

### 工作顺序（强制 · 侦探报告结构也按此）

```
第一步：只读溯源「商店数字逻辑」
  → UiSpriteNumberDisplay / Price / Number / Total2
  → 素材路径、SetNumber、spacing、poolCapacity、Bake/Ensure

第二步：只读溯源「MenuPanel Money」现状
  → Hierarchy、现有 Image 是否占位、有无 DigitStrip、MenuFormLogic 是否已读金币

第三步：对接方案
  → 复用商店组件如何挂到 Money
  → 读金 API + 何时 Refresh
  → 最小施工清单
```

**不要**一上来改 Menu；报告必须先有「商店数字逻辑说明书」小节。

### 商店数字逻辑假说（须证伪成表）

| 组件 / 文档 | 预扫 |
|-------------|------|
| 通用组件 | `UiSpriteNumberDisplay`（`Assets/Scripts/Game/GameRuntime/UI/Component/UiSpriteNumberDisplay.cs`） |
| 素材 | `Assets/ArtRes/UI/Text/0.png`～`9.png` |
| 商店用法 | 行 **Price** Bake 刷图；**Number** 输入同步；**Total2** `SetNumber(total)` |
| 间距常量 | `ShopPriceSpacing` / `ShopNumberSpacing` / `ShopTotalSpacing`（多为 **-12**） |
| 位数 | **自然位数**，禁止 PadLeft；Total2 `poolCapacity` 约 5～6 |
| 文档 | `0706/Shop_Bar数字图片化_Price_Number_Total_…`；栈溢出修复文 |

生活类比：商店价签是「木牌数字贴片」；菜单 Money 要用同一盒贴片，只是贴的数字改成「钱包余额」。

### MenuPanel Money 假说（须证伪）

| 项 | 预扫 |
|----|------|
| Prefab | `MenuPanel` → 似有 **`ButtonMoney`**，子节点 **`Money`**、**`Money (1)`** |
| 现网组件 | 预扫多为 **裸 Image**（可能金币图标或占位图），**未见** `UiSpriteNumberDisplay` / `DigitStrip` |
| `MenuFormLogic` | 预扫 **无** Money/Gold 刷新；日历用的是另一套 `MenuCalendarDayNumDisplay`（十位+个位，**不是**商店可变位数逻辑） |
| 易混 | `Money` 节点上的 Sprite guid 可能碰巧是 `0.png`——**不等于**已接图片数字条 |

**裁定倾向（可推翻）**：Menu 应 **新增/挂载 `UiSpriteNumberDisplay`**（或子节点 DigitStrip），**不要**扩 `MenuCalendarDayNumDisplay` 硬撑多位金币。

### 货币数据（对接侧 · 与 0829 购买检测衔接）

| API | 用途 |
|-----|------|
| `QuestManager.GetPlayerGoldData()` | 取存档 |
| `PlayerGoldData.gold` | 剩余货币 int |
| `CanAfford` / `TrySpendPlayerGold` | 本期 **只读显示**，不在菜单扣款 |

打开菜单时：`digits.SetNumber(goldData != null ? goldData.gold : 0)`（空档策略侦探拍板）。

### 方案候选（第二步之后拍板）

| 方案 | 做法 | 倾向 |
|------|------|------|
| **A · Money 下挂 `UiSpriteNumberDisplay`** | 与 Total2 同款；`MenuFormLogic.OnOpen` Refresh | **✅ 推荐倾向** |
| **B · 复用 `MenuCalendarDayNumDisplay`** | 固定两位 | ❌ 金币可多位；与商店逻辑不一致 |
| **C · 新写 MenuMoneyDisplay** | 复制粘贴商店代码 | ❌ 除非组件无法复用，须论证 |
| **D · 仅改 Sprite 为静态图** | 不读存档 | ❌ 不满足真实货币 |

菜单专用 spacing/capacity：可新建 `MenuMoneySpacing` 常量，或先复用 `ShopTotalSpacing` + 足够 `poolCapacity`（侦探按 Money 底框宽度建议）。

### 本期边界

| 做 | 不做 |
|----|------|
| ✅ 商店数字逻辑溯源表（第一步交付） | ❌ 重做商店 Price Bake |
| ✅ Money 对接方案 + 读金刷新 | ❌ 菜单内购买/扣款 |
| ✅ 与商店同组件、同素材、同自然位数规则 | ❌ 出售经济；限购 |
| ✅ 最小 Prefab/脚本清单 | ❌ 改 `PlayerGoldData` 存档结构 |

### 严禁（本阶段）

- 改代码 / Prefab / 场景  
- 跳过商店溯源直接拍 Menu 方案  
- 用 TMP/Text 显示金币充数  
- 菜单维护独立 `int displayGold` 不读存档  

### 须对拍资产

| 资产 | 用途 |
|------|------|
| `UiSpriteNumberDisplay.cs` | **商店数字逻辑真源** |
| `ShopFormLogic` · Total2 / `ShopBarRowView` Price | 运行时 SetNumber 样例 |
| `0706/Shop_Bar数字图片化_…` | 设计口径 |
| `MenuPanel.prefab` · Money / ButtonMoney | 挂载点 |
| `MenuFormLogic.cs` / `MenuCalendarDayNumDisplay.cs` | 勿混日历逻辑 |
| `PlayerGoldData` / `QuestManager` | 货币真源 |
| `0829/…购买成功失败与剩余货币检测…` 提示词/报告（若已有） | 读金 API 对照 |

---

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/02_SYSTEM_SPEC.md
@Assets/Doc/执行文档/7月/0706/Shop_Bar数字图片化_Price_Number_Total_架构溯源与施工执行说明.md
@Assets/Doc/执行文档/7月/0706/Village_Shop_Play闪退_UiSpriteNumberDisplay栈溢出_架构溯源与施工执行说明.md
@Assets/Doc/提示词/0829/Village_Shop_购买成功失败与剩余货币检测_架构侦探提示词.md
@Assets/Scripts/Game/GameRuntime/UI/Component/UiSpriteNumberDisplay.cs
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Shop/ShopFormLogic.cs
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Shop/ShopBarRowView.cs
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Menu/MenuFormLogic.cs
@Assets/Scripts/Game/GameRuntime/UI/Component/MenuCalendarDayNumDisplay.cs
@Assets/Scripts/Game/GameMgr/Component/Archive/ArchiveDataClass/Quest/PlayerGoldData.cs
@Assets/Scripts/Game/GameMgr/Component/Archive/ArchiveDataClass/Quest/QuestManager.cs
@Assets/GameRes/Prefabs/UI/MenuPanel.prefab
@Assets/ArtRes/UI/Text/

你现在是【架构侦探】。Unity 2020.3.48f1 / C#。
禁止修改任何代码、Prefab、场景、存档。只读扫描 + 写溯源报告。

---

## 背景（策划白话）

1. ESC 菜单里有一块 **Money**，现在要显示 **玩家真实金币**。  
2. 数字外观要和 **商店价格**一样：用 0～9 图片，不要系统字。  
3. 你必须 **先把商店数字逻辑查明白**，再设计怎么接到 MenuPanel Money。

---

## 侦探任务清单

### 第一步 · 商店图片数字逻辑说明书（必出专节）

用表写清，让非商店同学也能照做：

| 项 | 内容 |
|----|------|
| 组件类名 / 路径 | |
| 对外 API | `SetNumber(int)` / EnsureOn / FindUnder … |
| 素材目录与绑定方式 | |
| Price / Number / Total2 各自谁调用、何时刷 | |
| spacing / alignment / poolCapacity / fitWithinParent | 商店定稿值 |
| 禁止事项 | 前导零、Update 轮询等 |
| 与 `MenuCalendarDayNumDisplay` 差异 | 为何菜单金币不要用日历那套 |

可附一条最小伪代码：

```
display.SetNumber(priceOrTotal); // 自然位数刷 Digit_* Image
```

### 第二步 · MenuPanel Money 现状

| 项 | 填 |
|----|-----|
| Hierarchy 全路径（ButtonMoney / Money / Money(1)…） | |
| 各节点组件（Image / 是否 DigitStrip / UiSpriteNumberDisplay） | |
| 现 Sprite 是图标还是数字图 | |
| MenuFormLogic 是否已有刷新 | |
| 底框尺寸 → 建议 poolCapacity / spacing | |

### 第三步 · 对接方案拍板

1. 推荐方案 A/B/C/D（见预梳理）  
2. DigitStrip 挂在哪个 GO（Money 本体 vs 新建 Money_Digits 子节点；原图标是否保留）  
3. 读金调用链；`gold==null` 显示 0 还是隐藏  
4. 刷新挂点：`OnOpen` / `OnEnable` / 菜单激活事件  
5. 是否从商店买完再开菜单能看到余额变化（同档存档）  
6. 默认 **不改** `UiSpriteNumberDisplay` 中枢；若必须加 Menu 常量，写明最小 diff  

### 第四步 · 最小施工清单（本阶段不执行）

| # | 模块 | 动作 | 优先级 |
|---|------|------|--------|
| 1 | Prefab | Money 区挂 `UiSpriteNumberDisplay` + 绑 0～9 | P0 |
| 2 | MenuFormLogic | OnOpen（或等价）读 gold → SetNumber | P0 |
| 3 | 验收 | ESC 开菜单数字=存档金币；改金后再开菜单更新 | P0 |
| 4 | spacing/capacity 手调 | | P1 |
| 5 | 与商店旁路/购买检测文档交叉备注 | | P2 |

**排除**：菜单扣款；重做商店 Bake；用日历双图组件硬撑多位金币。

### 验收清单

| # | 操作 | 通过 |
|---|------|------|
| 1 | 报告含完整「商店数字逻辑」专节 | |
| 2 | 存档 gold=0 / 123 / 99999（在容量内）开菜单 | 图片数字正确、无前导零 |
| 3 | 外观 | 与商店价签同素材风格（同一套 0～9） |
| 4 | 非 InitScene 直开等异常 | 有降级策略、无 NRE |
| 5 | Console | 无 Digit 池 / 栈溢出类旧坑复现 |

### 开放问题

- Money 旁硬币图标保留还是并入数字条？  
- 最大显示位数（与 Total2 对齐 5～6？）  
- 商店内能否开菜单（Village_Shop ESC 改口后可能不能）——刷新是否只依赖 OnOpen 即可？  

---

## 输出要求

写入：`Assets/Doc/执行文档/0829/MenuPanel_Money对接商店图片数字与真实货币_架构溯源报告.md`

结构（MASTER 四段式 + 表）：

① 结论一句话（商店逻辑复用点 + Menu 怎么挂 + 读金 API）  
② 原因（通俗：先价签说明书，再挂到菜单钱包；为何不用日历双位）  
③ 用户检查清单（开菜单应看到什么）  
④ 给程序：**§商店数字逻辑** + Money 现状表 + 方案 + 最小 diff + 开放问题

口头汇报同样用 MASTER 四段式；**必须先讲商店数字怎么工作，再讲 Menu 怎么接**。
```

---

## 施工员续跑（侦探报告拍板后再贴）

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0829/MenuPanel_Money对接商店图片数字与真实货币_架构溯源报告.md
@Assets/GameRes/Prefabs/UI/MenuPanel.prefab
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Menu/MenuFormLogic.cs
@Assets/Scripts/Game/GameRuntime/UI/Component/UiSpriteNumberDisplay.cs
@Assets/Scripts/Game/GameMgr/Component/Archive/ArchiveDataClass/Quest/QuestManager.cs

你现在是【施工员】。按报告把 MenuPanel Money 接到真实 PlayerGold，并用与商店相同的 UiSpriteNumberDisplay 显示。

必须遵守：
- 先保证显示组件与商店同逻辑（自然位数、同套 0～9 图），再绑 gold；
- 禁止 TMP/Text 充数；禁止 MenuCalendarDayNumDisplay 硬撑多位金币（除非报告改口）；
- 禁止第二钱包；打开菜单必须 Refresh；
- 默认不改商店 Bake；改 UiSpriteNumberDisplay 中枢仅当报告写明；
- 代码含详细注释；重要取舍写清原因。

提交说明：Prefab 挂了什么、何时 SetNumber、如何验收 0/多位金币、未做项。
```
