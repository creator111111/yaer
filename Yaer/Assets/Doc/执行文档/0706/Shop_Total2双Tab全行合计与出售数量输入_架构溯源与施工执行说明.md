# Shop · Total2 双 Tab 全行合计 + 出售数量输入 — 架构溯源与施工执行说明

**文档版本**：v1（2026-07-06）  
**文档性质**：架构侦探 + 施工指引  
**触发**：商店底部 **`Total2`** 需按 Tab 显示**全列表行**合计；`Number` 默认改为 **0**；出售列表 `Bar_ListScroll_Sell` 各行也需与购买列表相同的**可输入数量**能力。

**依据**：
- `Assets/Doc/00_MASTER_PROMPT.md`【架构侦探 / 施工员】
- `Assets/Doc/执行文档/0704/Shop_Editor烘焙双列表_MainItemDatabase一键刷行_施工执行说明.md`（EB 烘焙 · 行数据 `bakedPrice` 只读）
- `Assets/Doc/执行文档/0704/Shop_货单瘦身_MainItemDatabase驱动Shop_Bar刷新_架构溯源与施工执行说明.md`（买/卖过滤规则）
- `Assets/Doc/执行文档/0629/商店系统_策划拆解_执行说明.md`（阶段三合计 · 阶段四决定按钮 · 待升级）
- 关联场景：`Assets/GameRes/Scenes/Village_Shop.unity`
- 关联预制体：`Assets/GameRes/Prefabs/UI/Shop/Shop_Bar.prefab`
- 关联脚本：`ShopFormLogic.cs`、`ShopBuyRowQuantityInput.cs`、`ShopQuantityInputHelper.cs`、`ShopBarRowView.cs`、`ShopListBakeEditor.cs`

**Unity 版本**：2020.3.48f1  

---

## ① 结论（一句话）

**`Total2` 区域显示当前 Tab 的「全行合计」：购买页 = Σ(每行 Number × 买价)，出售页 = Σ(每行 Number × 卖价)；`Number` 默认值统一为 0；Bake 与运行时给 Sell 行也挂数量输入组件；废弃「只算 HpBall 一行」的 `TxtTotal` 逻辑，改绑 `Total2` 下文案节点。**

---

## ② 玩家在做什么 / 会遇到什么现象

| # | 策划期望 | 当前工程（施工前） | 生活类比 |
|---|----------|-------------------|----------|
| 1 | 购买 Tab：改任意行 **Number**，**Total2** 显示**所有行**数量×单价之和 | 只监听 **HpBall** 一行 → 旧 `TxtTotal`；改 MpBall 数量**不影响**合计 | 购物车只算第一件商品 |
| 2 | 出售 Tab：**Total2** 显示出售合计 | 切到 Sell **不刷新**合计；Sell 行 Number 是写死的 **「1」** 占位 | 卖东西时柜台计算器没开机 |
| 3 | 打开商店 / 切 Tab 时，每行 Number 默认 **0** | 默认 **1**（`ShopQuantityInputHelper.DefaultQuantity`） | 进店默认帮你塞了 1 件，合计却不是 0 |
| 4 | Sell 列表每行 Number **能点、能输整数** | Bake 只给 Buy 行挂 `ShopBuyRowQuantityInput`；Sell 用 `SetSellQuantityPlaceholder("1")` | 卖货柜台没有键盘 |

---

## ③ 架构溯源

### 3.1 合计显示：Total2 / TxtTotal / Total1

#### 3.1.1 场景快照（`Village_Shop.unity` · 2026-07-06）

| 节点 | 组件 | 现状 | 本任务角色 |
|------|------|------|------------|
| `Total1` | Image（「合计」标签底图） | 仅装饰 | 不动 |
| **`Total2`** | Image（数字底框） | **无子节点、无 Text** | **合计数字显示区（底框）** |
| `TxtTotal` | Legacy `Text`（**Enabled=0**） | `ShopFormLogic` 仍 `Find("TxtTotal")` | **弃用绑定**，改绑 Total2 下文案 |

**策划口径澄清**：「Total2 里面显示总和」= 在 **`Total2` 底框之上/之内** 有可读数字节点（Text 或 TMP），程序只改**文字**，不改底图 Image。

#### 3.1.2 当前程序链路（购买 · 仅 HpBall）

```
ShopFormLogic.Awake
  → CollectBuyRowViews()
  → CacheHpBallQuantityInput()          // 只找 HpBall 行
  → WireHpBallTotalRefresh()            // 只绑一行 OnQuantityValueChanged
  → RefreshHpBallBuyTotal()
       → GetCurrentHpBallBuyTotal()     // quantity × HpBall.Price
       → SetTotalText() → TxtTotal
```

**缺口**：无 `_sellRowViews`、无 `SwitchToSellTab` 合计刷新、公式非 Σ 全行。

#### 3.1.3 目标公式（定稿）

| Tab | 合计公式 | 单价来源 | 数量来源 |
|-----|----------|----------|----------|
| **购买** | `BuyTotal = Σ (qty_i × price_i)` | `ShopBarRowView.Price`（Bake 写入的 **买价**） | 该行 `ShopBuyRowQuantityInput.QuantityForTotal` |
| **出售** | `SellTotal = Σ (qty_i × price_i)` | 同上（Bake 写入的 **卖价**） | 同上 |

- `QuantityForTotal`：空串 / 非法 → **0**（已有 `ParseQuantityForTotal`，与默认 0 一致）。
- **行集合**：当前可见 Tab 对应 Content 下**全部** `Shop_Bar_*` 子节点，不按道具 ID 过滤。
- **示例**：Buy 8 行全为 0 → Total2 显示 `0`；HpBall qty=2 单价 200、MpBall qty=1 单价 200 → `600`。

---

### 3.2 数量列 Number：默认值与输入能力

#### 3.2.1 节点约定（`Shop_Bar.prefab`）

| 节点名 | 位置 | 购买行 | 出售行（施工前） |
|--------|------|--------|------------------|
| `Number` | 第四列 | TMP InputField（`ShopBuyRowQuantityInput`） | Legacy Text 占位 `"1"` |
| `TxtStock` | 0629 别名 | 代码兜底 Find | 同左 |

#### 3.2.2 默认值常量

| 位置 | 当前 | 目标 |
|------|------|------|
| `ShopQuantityInputHelper.DefaultQuantity` | **1** | **0** |
| `EnsureTmpIntegerInputField(..., defaultQuantity)` | 默认 1 | 默认 **0** |
| `ShopBuyRowQuantityInput.ResetToDefault()` | 重置为 1 | 重置为 **0** |
| Bake `SetSellQuantityPlaceholder` | 写 `"1"` | **删除**；改挂输入组件并写 `"0"` |

**重要**：`Quantity`（失焦回退用）与 `QuantityForTotal`（合计用）行为分化保留——合计始终用 `QuantityForTotal`（空→0）；若将来交易逻辑需要「空=默认」，单独评估，**本任务合计只认 0**。

#### 3.2.3 出售行输入（施工前缺口）

```
ShopListBakeEditor.BakeContent
  → isBuyRow ? EnsureShopBuyRowQuantityInput(row)
  → else SetSellQuantityPlaceholder(row)   // 只写 Text "1"，无 InputField
```

**目标**：Buy / Sell **同一套** `EnsureShopBuyRowQuantityInput`（或重命名后的共用组件）。

---

### 3.3 与阶段四「决定」按钮的关系

| 项 | 现状 | 本任务 |
|----|------|--------|
| `OnConfirmClick` | 仍只验 **HpBall** 数量 + `GetCurrentHpBallBuyTotal()` | **可暂不修改**（假购买 Log 范围不变） |
| Total2 显示 | 与决定按钮**独立** | 合计给用户看；决定按钮升级另开 **ST-后续 / 阶段五** 任务 |

> 若策划要求「决定」也按全行合计校验，在 §⑧ Q2 确认后追加，**勿在本任务悄悄改交易规则**。

---

## ④ 修复方案定稿

### 4.1 Fix-ST1 · Total2 绑定与双 Tab 合计（程序）

| 步骤 | 文件 | 改动 |
|------|------|------|
| ST1-1 | `ShopFormLogic.cs` | 常量：`TxtTotalName` → 优先 `Total2`；子节点名 `TxtTotal2`（或 Total2 自身 TMP/Text） |
| ST1-2 | 同上 | 字段：`_buyRowViews` 保留；新增 `_sellRowViews`；新增 `_activeTabIsBuy` 或用枚举 |
| ST1-3 | 同上 | `CollectSellRowViews()`：遍历 `sellContent` 子节点 `GetComponent<ShopBarRowView>()` |
| ST1-4 | 同上 | `GetCurrentBuyTotal()` / `GetCurrentSellTotal()`：Σ qty×price |
| ST1-5 | 同上 | `RefreshTotal2()`：按当前 Tab 写 Buy 或 Sell 合计到 Total2 文案 |
| ST1-6 | 同上 | **删除** `CacheHpBallQuantityInput` / `WireHpBallTotalRefresh` / `GetCurrentHpBallBuyTotal` / `RefreshHpBallBuyTotal` |
| ST1-7 | 同上 | `WireAllRowQuantityRefresh()`：Buy+Sell **所有行** `OnQuantityValueChanged += RefreshTotal2` |
| ST1-8 | 同上 | `SwitchToBuyTab` → `ResetAllBuyQuantityInputs(0)` + `RefreshTotal2()` |
| ST1-9 | 同上 | `SwitchToSellTab` → `ResetAllSellQuantityInputs(0)` + `RefreshTotal2()` |
| ST1-10 | 同上 | `OnDestroy`：`UnwireAllRowQuantityRefresh()` |

**ResolveTotal2Text 优先级**（伪代码）：

```csharp
// 1. UI_Shop/Total2/TxtTotal2
// 2. UI_Shop/Total2 自身 Text 或 TMP
// 3. 兼容回退 UI_Shop/TxtTotal
```

**替代方案（不推荐）**：

| 方案 | 缺点 |
|------|------|
| 继续用 TxtTotal、不理 Total2 | 与美术 Hierarchy 不一致，策划验收对不上节点名 |
| 只扩 HpBall+MpBall 两行合计 | 仍非「每一项」，违背本需求 |

---

### 4.2 Fix-ST2 · 默认数量 0（程序 + Bake）

| 步骤 | 文件 | 改动 |
|------|------|------|
| ST2-1 | `ShopQuantityInputHelper.cs` | `DefaultQuantity = 0`；`BuildInputFieldHierarchy` 初始文本 `"0"` |
| ST2-2 | `ShopBuyRowQuantityInput.cs` | `ResetToDefault` 默认参数改为 0；注释更新 |
| ST2-3 | `ShopListBakeEditor.cs` | **删除** `SetSellQuantityPlaceholder` 调用 |
| ST2-4 | 同上 | Buy **与** Sell 均 `EnsureShopBuyRowQuantityInput(row)`，Bake 时 `EnsureTmpIntegerInputField(..., 0)` |
| ST2-5 | `ShopBarRowView.cs` | **删除或废弃** `ApplySellQuantityPlaceholder`（Bind 路径保留与否见 §⑧ Q3） |

**Bake 后 Editor 目检**：Buy/Sell 每行 `Number` 显示 **0**，且为可点击 TMP 输入框。

---

### 4.3 Fix-ST3 · 场景 Total2 文案节点（美术 / 策划）

`Total2` 当前**仅有 Image**，需补显示数字的节点（二选一）：

| 方案 | 操作 |
|------|------|
| **A（推荐）** | 在 `Total2` 下新建子节点 **`TxtTotal2`**（TMP 或 Text），右对齐、叠在底框内 |
| **B** | 在 `Total2` 同位置保留 `TxtTotal`，**拖为 Total2 子节点**并改名 `TxtTotal2` |

施工后 Hierarchy 示意：

```
UI_Shop
├── Total1          ← 「合计」标签图（不动）
├── Total2          ← 数字底框 Image
│   └── TxtTotal2   ← 程序刷新：纯数字，如 "0" / "600"
└── …
```

可选：隐藏或删除根级旧 `TxtTotal`，避免重复显示。

---

### 4.4 组件命名（可选 · 非阻塞）

| 现状 | 建议 | 说明 |
|------|------|------|
| `ShopBuyRowQuantityInput` | 重命名为 `ShopRowQuantityInput` | Buy/Sell 共用，语义准确 |
| 暂不重命名 | Sell 行也挂 `ShopBuyRowQuantityInput` | **最小 diff**，文档与代码注释标明「买卖共用」 |

**本任务允许方案 B**；若重命名，须同步 `ShopListBakeEditor` / `ShopQuantityInputSetupEditor` 引用。

---

## ⑤ 施工阶段（ST-0 ～ ST-4）

| 阶段 | 内容 | 负责 | 验证 |
|------|------|------|------|
| **ST-0** | 场景：`Total2/TxtTotal2` 文案节点就位 | 美术/策划 | Inspector 可见 TMP/Text |
| **ST-1** | `ShopQuantityInputHelper` + `ShopBuyRowQuantityInput` 默认 0 | 程序 | 新行 Number 显示 0 |
| **ST-2** | `ShopListBakeEditor`：Sell 行也挂数量输入；去掉占位「1」 | 程序 | 跑 Bake 后 Sell 行可输入 |
| **ST-3** | `ShopFormLogic`：Collect 双列表 + Σ 合计 + Total2 绑定 | 程序 | 改任意行 Number → Total2 变 |
| **ST-4** | 全量 Bake `Village_Shop` + Play 双 Tab 验收 | 策划 | ST-V1～V8 |

---

## ⑥ 验收清单

| ID | 操作 | 期望 |
|----|------|------|
| ST-V1 | 跑 Bake → **不 Play**，看 Buy/Sell 任一行 `Number` | 显示 **0**，且为 TMP 输入框 |
| ST-V2 | Play → **购买 Tab**，不改任何数量 | **Total2** = **`0`** |
| ST-V3 | 购买 Tab：HpBall Number=**2**（单价 200），其余保持 0 | Total2 = **`400`** |
| ST-V4 | 购买 Tab：HpBall=2、MpBall=**1**（单价 200） | Total2 = **`600`** |
| ST-V5 | 购买 Tab：某行 Number 清空后失焦 | 该行按 **0** 计；Total2 相应减少 |
| ST-V6 | 切 **出售 Tab** | Total2 变为 Sell 列表 Σ(qty×**卖价**)；初始全 0 则 **`0`** |
| ST-V7 | 出售 Tab：某 Material 行 Number=**3**、卖价 5 | Total2 含该项 **15**；多行相加正确 |
| ST-V8 | 购买 ↔ 出售 Tab 来回切 | Total2 **切换公式**；各自 Number 重置为 **0**（若实现 Reset） |
| ST-V9 | Console | 无 Error；无旧 `RefreshHpBallBuyTotal` 孤立 Log |

---

## ⑦ 踩坑与约束

### 7.1 单价不要用 Database 运行时重查

EB 方案下行单价以 **`ShopBarRowView.Price`（Bake 序列化）** 为准，避免 Play 时异步加载 Database 导致合计与列表价不一致。

### 7.2 合计与背包持有数无关

出售 Tab 的 Number 本阶段表示「**要卖几件**」，**不是**背包库存；库存校验留给阶段六。

### 7.3 位数上限

`MaxQuantityDigits = 2` → 单行最多 **99**；合计可能超过 99×行数，Total2 显示整数**不截断位数**（仅输入框限制位数）。

### 7.4 改合计逻辑不必重 Bake

仅改 `ShopFormLogic` 时 **不需** Bake；改默认 0 或 Sell 输入组件时 **需** 再 Bake 刷新场景行。

### 7.5 旧文档 P3-4「只改 MpBall 不影响 TxtTotal」

本任务**作废**该条验收；改为「改任意行 Number 都应影响 Total2」。

---

## ⑧ 待确认问题

| ID | 问题 | 影响 | 建议 |
|----|------|------|------|
| Q1 | Total2 文案节点命名：`TxtTotal2` 还是沿用 `TxtTotal` 作子节点？ | `ResolveTotal2Text` 查找路径 | 新建 **`TxtTotal2`**，旧根级 `TxtTotal` 隐藏 |
| Q2 | 「决定」按钮是否改为校验 **全行 Buy 合计 > 0**？ | `OnConfirmClick` 范围 | 本任务 **不改**；单独立项 |
| Q3 | `ShopBarRowView.Bind` 里 `ApplySellQuantityPlaceholder` 是否删除？ | 动态商店将来是否还用 Bind | EB 商店不用 Bind，可标 `[Obsolete]` |
| Q4 | 切 Tab 是否 **重置** 对侧列表 Number 为 0？ | ST-V8 | **是**——与购买 Tab 现有 `ResetAllBuyQuantityInputs` 对称 |

> 无结论时写入 `Assets/Doc/OPEN_QUESTIONS.md`，勿擅自改阶段四扣款规则。

---

## ⑨ 给程序看的入口索引

| 主题 | 路径 |
|------|------|
| Tab / 合计总控 | `Assets/Scripts/Game/GameRuntime/UI/FormLogic/Shop/ShopFormLogic.cs` |
| 行数量输入 | `Assets/Scripts/Game/GameRuntime/UI/FormLogic/Shop/ShopBuyRowQuantityInput.cs` |
| 输入框工具 | `Assets/Scripts/Game/GameRuntime/UI/FormLogic/Shop/ShopQuantityInputHelper.cs` |
| 行单价 / itemId | `Assets/Scripts/Game/GameRuntime/UI/FormLogic/Shop/ShopBarRowView.cs` |
| Bake 挂组件 | `Assets/Scripts/Game/GameRuntime/UI/FormLogic/Shop/Editor/ShopListBakeEditor.cs` |
| 测试场景 | `Assets/GameRes/Scenes/Village_Shop.unity`（`Total2`、`Bar_ListScroll_Buy/Sell`） |

---

## ⑩ 文档关系

| 文档 | 关系 |
|------|------|
| `Shop_Editor烘焙双列表…`（0704） | 行 `bakedPrice` 为合计单价来源；Bake 须给 Sell 也挂数量组件 |
| `Shop_货单瘦身…`（0704） | 买/卖候选过滤不变；合计对**已 Bake 行**全量求和 |
| `商店系统_策划拆解…`（0629） | 阶段三合计从「HpBall×200」升级为「全行 Σ」；阶段四决定按钮待跟进 |
| `Shop_UI三小问题…`（0706） | MpBall 上架后计入 Buy Σ；与 Total2 无冲突 |

---

## ⑪ 修订记录

| 日期 | 说明 |
|------|------|
| 2026-07-06 | 首版：Total2 双 Tab Σ 合计、Number 默认 0、Sell 数量输入、ST-0～ST-4 + ST-V1～V9 |
