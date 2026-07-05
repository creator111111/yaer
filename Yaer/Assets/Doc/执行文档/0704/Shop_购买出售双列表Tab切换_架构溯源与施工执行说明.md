# Shop · 购买 / 出售双列表 Tab 切换 — 架构溯源与施工执行说明

**文档性质**：架构侦探产出（只读溯源 + 施工指引；**本阶段先写文档，代码待施工**）  
**调查日期**：2026-07-05  
**依据**：
- `Assets/Doc/00_MASTER_PROMPT.md`【架构侦探】
- `Assets/Doc/执行文档/0704/Shop_Bar列表滚动_架构溯源与施工执行说明.md`（SC-0～SC-4 滚动壳，**§4.3 共用单列表方案已被本任务取代**）
- `Assets/Doc/执行文档/0629/商店系统_策划拆解_执行说明.md` §3.4 / §3.5 / 阶段六（出售页）
- 关联场景：`Assets/GameRes/Scenes/Village_Shop.unity`（`UI_Shop` / `Bar`）
- 关联 Prefab：`Assets/GameRes/Prefabs/UI/Shop/Shop_Bar.prefab`

**Unity 版本**：2020.3.48f1  

---

## ① 结论（一句话）

**在 `Bar` 下维护两套纵向 ScrollRect：`Bar_ListScroll_Buy`（购买）与 `Bar_ListScroll_Sell`（出售），叠在同一可视区域；点击 `BUY` / `SELL` Tab 时只做 `SetActive` 互斥显示，各自 `Content` 独立行数与滚动位置，本阶段不接背包数据，出售侧用 `Shop_Bar` 空壳占位验收切换与滚动。**

---

## ①.1 范围冻结（2026-07-05 · 策划 / 施工对齐）

| 项 | 本阶段约定 | 说明 |
|----|------------|------|
| **列表结构** | **两套 ScrollView** | `Bar_ListScroll_Buy` + `Bar_ListScroll_Sell`；**不再**共用单一 `Bar_ListScroll` + Clear 换数据 |
| **Tab 切换方式** | **`SetActive` 互斥** | 买：`Buy=true, Sell=false`；卖：`Buy=false, Sell=true` |
| **行间距 / Viewport** | 继承 SC 滚动壳 | Spacing **16**；Viewport 高 **559**（= `Bar_BG`）；侧边 Scrollbar **Disable** |
| **列表行数** | **Editor 自行决定** | `Bar_ListScroll_Buy` / `_Sell` 的 `Content` 下**你放几行就是几行**；程序**不**写死行数、**不**强制 Buy/Sell 数量关系 |
| **程序改动** | **`ShopFormLogic` Tab 切换** | 绑 `btnBuy` / `btnSell` + 两个 Scroll 根节点；**不**在 `Update` 里轮询 |
| **本阶段不做** | Tab 按钮高亮、出售过滤、扣道具、合计文案「将获得」 | Tab 高亮留 UI 合层；出售数据属 `0629` 阶段六；本任务只验收 **可见性 + 独立滚动** |
| **0704 旧文档 Q5** | **已推翻** | 原「共用 `Bar_ListScroll`」→ 本任务改为双列表 |

---

## ② 玩家在做什么 / 会遇到什么现象

| 现象 | 原因（生活类比） |
|------|------------------|
| 点「购买」只看到药水，点「出售」应看到另一套货单 | 两个 Tab 对应**两个独立抽屉**，不是同一个抽屉换标签 |
| 购买和出售列表行数可能不一样 | 各抽屉里货架层数由你在 Editor 里摆放；需要**各自**的滚动区域 |
| 从购买切到出售再切回购买，购买列表应还在原来滚到的位置 | 双 ScrollView **隐藏而不销毁**，各自记住 `Content` 的 `anchoredPosition` |
| 现在点 SELL 没反应 | 场景里 `SELL` 按钮 **OnClick 为空**；`ShopFormLogic` 只绑了 `btnBuy` |

**生活类比**：`Bar_BG` 是固定大小的展示柜玻璃；购买页和出售页是**叠在一起的两本菜单**，同一时刻只翻开一本；换 Tab 等于换菜单，而不是把同一本菜单上的字擦掉重写。

---

## ③ 工程现状快照（架构侦探 · 只读）

### 3.1 场景 `Village_Shop` · `Bar` 区域（当前）

```
UI_Shop（Canvas + ShopFormLogic）
├── BUY                         ← Button，已绑 ShopFormLogic.btnBuy → SwitchToBuyTab
├── SELL                        ← Button，OnClick 为空，未绑程序
└── Bar
    ├── Bar_BG                  ← 482×559，列表底图
    └── Bar_ListScroll          ← ⚠ 单套 ScrollRect（0704 SC 成果）
        ├── Viewport            ← RectMask2D，高 559
        │   └── Content         ← VerticalLayoutGroup Spacing=16
        │       └── Shop_Bar ×8 ← 8 条测试空壳占位
        └── Scrollbar Vertical  ← Disable
```

| 节点 | 状态 | 说明 |
|------|------|------|
| `Bar_ListScroll` | ✅ 已搭好 | 0704 SC-0～SC-4；需 **重命名** 为 `Bar_ListScroll_Buy` |
| `Bar_ListScroll_Sell` | ❌ 不存在 | 本任务新建（Duplicate Buy 后，**自行**调整 Sell 的 Content 子节点） |
| `BUY` | ✅ 可点 | `ShopFormLogic` 仅 `SwitchToBuyTab()`，**未**控制 Scroll 显隐 |
| `SELL` | ⚠ 壳子 | 有 Button 组件，**无** `SwitchToSellTab` |
| `rowHpBall` / `rowMpBall` | Inspector **null** | 脚本仍 `FindDeepChild("Row_HpBall")`；Content 内目前是 `Shop_Bar` 命名 |

### 3.2 程序侧（Tab / 列表相关）

| 项 | 状态 |
|----|------|
| `ShopFormLogic.SwitchToBuyTab()` | ✅ 激活 `rowHpBall`/`rowMpBall`、重置数量、刷新 TxtTotal |
| `ShopFormLogic.SwitchToSellTab()` | ❌ **不存在** |
| `btnSell` 序列化字段 | ❌ **不存在** |
| `UsesScrollListLayout()` | 查找 `"Bar_ListScroll"` | 重命名后需改为识别 `_Buy` / `_Sell` |
| `ShopBarListScrollSetupEditor` | 常量 `BarListScrollName = "Bar_ListScroll"` | 施工后需扩展或新增菜单项 |

### 3.3 与 0704 滚动壳文档的关系

| 0704 原约定 | 本任务处理 |
|-------------|------------|
| 购买 / 出售 **共用** `Bar_ListScroll` | **废弃**；改为双 ScrollView |
| 切 Tab 时 Clear Content 再填充 | **废弃**；改为 SetActive 切换 |
| SC-0～SC-4 尺寸 / 行距 / 滚轮策略 | **保留**；两套 Scroll **复制同一套参数** |
| `Shop_Bar` 空壳占位 | **保留**；Buy / Sell 各自 Content 下独立实例 |

---

## ④ 目标 Hierarchy（施工完成后）

### 4.1 推荐结构（双 ScrollRect + Tab 互斥显隐）

```
Bar
├── Bar_BG                                    ← 不变；装饰底图，不滚动
├── Bar_ListScroll_Buy                        ← ScrollRect；默认 Active
│   ├── Viewport                              ← 高 559，与 Bar_BG 对齐
│   │   └── Content                           ← Spacing 16
│   │       └── Shop_Bar × N                  ← **N = 你在 Editor 里放几行就是几行**
│   └── Scrollbar Vertical                    ← Disable
└── Bar_ListScroll_Sell                       ← ScrollRect；默认 **Inactive**
    ├── Viewport
    │   └── Content
    │       └── Shop_Bar × M                  ← **M 由你自行决定，可与 N 不同**
    └── Scrollbar Vertical                    ← Disable
```

**要点**：

- 两个 Scroll **RectTransform 完全重叠**（同 Anchor / Position / SizeDelta），叠在 `Bar_BG` 内缘上。
- **同一时刻只激活一个** Scroll 根节点；隐藏的 Scroll **不要 Destroy**，以保留滚动位置。
- `Bar_BG` 始终 Active；两个 Scroll 共用同一块「玻璃框」视觉。
- Buy / Sell 的 **Content 子物体数量由 Editor 摆放决定，可以不同**；程序只切 Scroll 显隐，不干预行数；各自 ContentSizeFitter 独立计算高度。

### 4.2 尺寸与滚动策略（继承 0704，两套一致）

| 量 | 值 | 说明 |
|----|-----|------|
| 单行高 | **88** | `Shop_Bar.prefab` |
| Content Spacing | **16** | VerticalLayoutGroup |
| Viewport 高 | **559** | = `Bar_BG` 内缘 |
| ScrollRect | Vertical=ON，Horizontal=OFF | Movement: Clamped |
| Vertical Scrollbar | **None** + 子节点 Disable | 仅滚轮滚动 |

### 4.3 Tab 切换逻辑（程序目标）

```
打开商店 / Start
  → SwitchToBuyTab()
      → barListScrollBuy.SetActive(true)
      → barListScrollSell.SetActive(false)
      → （已有）激活购买行、重置数量、RefreshHpBallBuyTotal

点击 SELL
  → SwitchToSellTab()
      → barListScrollBuy.SetActive(false)
      → barListScrollSell.SetActive(true)
      → （本阶段）可选 Debug.Log [ShopDebug] 切到出售页
      → （阶段六）RefreshSellRowsFromBag()、刷新出售合计

点击 BUY
  → SwitchToBuyTab()（同上）
```

**替代方案说明**（本任务已选定方案 A）：

| 方案 | 做法 | 优点 | 缺点 |
|------|------|------|------|
| **A · 双 Scroll + SetActive（推荐 · 本任务）** | 两套 Scroll 叠放，Tab 切显隐 | 购买 / 出售**行数不同**无冲突；各自保留滚动位置；切 Tab **O(1)** | 多占一份 Scroll 壳内存；Hierarchy 节点略多 |
| B · 单 Scroll + Clear 换子节点（0704 旧方案） | 一个 Content，Tab 时 Destroy 再 Instantiate | 节点少 | 切 Tab 丢滚动位置；长短列表切换要重建 UI，易闪帧 |
| C · 单 Scroll + 两组 Content 子树 Toggle | 一个 ScrollRect，Content 下 `BuyRows` / `SellRows` 两个父节点 SetActive | 比 B 保留滚动（若分 Content） | 仍共用一条 ScrollRect，**Content 高度随当前页变化**时 normalizedPosition 需额外复位；不如 A 直观 |

---

## ⑤ 分阶段施工

### 阶段 TB-0 · 拆分 Scroll Hierarchy（Editor）

| 做 | 不做 |
|----|------|
| `Bar_ListScroll` **重命名** → `Bar_ListScroll_Buy` | 不改 `ShopFormLogic`（可先改场景） |
| **Duplicate** → `Bar_ListScroll_Sell`，与 Buy **同位置同尺寸** | 不删 `Bar_BG` |
| Buy / Sell 的 `Content` 子节点：**按你的需要增删** `Shop_Bar` | 程序**不**规定行数；不补 TxtName / 背包数据 |
| `Bar_ListScroll_Sell` 默认 **SetActive(false)** | 不显示侧边 Scrollbar；**不做** Tab 按钮高亮 |

**Unity 操作摘要**：

1. 选中 `Bar/Bar_ListScroll` → 重命名为 `Bar_ListScroll_Buy`。
2. 在 Buy 的 `Content` 下按需摆放 `Shop_Bar`（**几行由你定**；现有 8 条可保留或删减）。
3. Ctrl+D 复制整棵 `Bar_ListScroll_Buy` → 命名为 `Bar_ListScroll_Sell`；确认 RectTransform 与 Buy 一致。
4. 在 Sell 的 `Content` 下**自行**调整子节点数量（可与 Buy 不同）。
5. 禁用 `Bar_ListScroll_Sell` 根节点；确认两套 `Scrollbar Vertical` 仍为 Disable。

**验证 TB-0**：Hierarchy 含 `Bar_ListScroll_Buy` + `Bar_ListScroll_Sell`；Sell 默认隐藏；两套 Viewport 高 559、Spacing 16；Buy/Sell 行数符合你在 Editor 中的摆放。

### 阶段 TB-1 · Tab 按钮绑程序

**改动文件**：`ShopFormLogic.cs`

| 新增 / 修改 | 说明 |
|-------------|------|
| `[SerializeField] Button btnSell` | Inspector 拖 `SELL` 按钮 |
| `[SerializeField] GameObject barListScrollBuy` | 拖 `Bar_ListScroll_Buy` |
| `[SerializeField] GameObject barListScrollSell` | 拖 `Bar_ListScroll_Sell` |
| `SwitchToSellTab()` | 互斥 SetActive + 本阶段最小逻辑 |
| `WireSellTabButton()` | `Awake` 中注册，与 `WireBuyTabButton` 对称 |
| `SwitchToBuyTab()` **补充** | 增加 Buy Scroll 显示、Sell Scroll 隐藏 |
| `UsesScrollListLayout()` | 改为检测 `Bar_ListScroll_Buy` 或 `_Sell` 是否存在 |

**Find 兜底（可选）**：若 Inspector 未拖引用，可按常量名 `Find("Bar")` 下 `Bar_ListScroll_Buy` / `_Sell`，与现有 `FindDeepChild` 风格一致。

**验证 TB-1**：Play → 默认购买列表可见；点 SELL → 出售列表可见、购买列表消失；点 BUY → 切回。

### 阶段 TB-2 · 独立滚动验收

1. 若某侧 `Content` 行数**超过视口可显示行数** → Play → 滚轮仅在该侧列表内滚动。
2. 在一侧滚到底 → 切 Tab → 切回 → 该侧滚动位置**应保持**。
3. Game 视图**无**右侧 Scrollbar。

**验证 TB-2**：两套列表滚动互不影响；Tab 切换无报错、无双 Scroll 同时可见。（行数少于一屏时，滚轮无位移属正常。）

### 阶段 TB-3 · Editor 工具同步（可选）

**改动文件**：`ShopBarListScrollSetupEditor.cs` 或新增 `ShopBarDualListScrollSetupEditor.cs`

- 菜单建议：`Tools / Shop / Setup Bar Dual List Scroll (TB-0~TB-2)`
- 逻辑：由现有 SC 脚本扩展——创建 Buy、Duplicate Sell、设默认 Active 状态。
- **替代**：不跑脚本，纯手动按 §4.1 搭 Hierarchy（与 0704 一致）。

**验证 TB-3**：跑菜单后场景结构与 §4.1 一致。

### 阶段 TB-4 · 接数据预告（不在本任务）

| 后续任务 | 说明 |
|----------|------|
| Buy Content | `Row_HpBall` / `Row_MpBall` 接 `ShopFormLogic` 现有逻辑 |
| Sell Content | `RefreshSellRowsFromBag()` 动态生成或预置行 + 绑背包数量 |
| 0629 阶段六 | 出售过滤、`TryRemoveMainItem`、TxtTotal 文案区分买/卖 |

---

## ⑥ 技术要点与踩坑

### 6.1 两 Scroll 叠放对齐

| 检查项 | 说明 |
|--------|------|
| Buy / Sell Rect 一致 | Anchor、Pivot、AnchoredPosition、SizeDelta 与现 `Bar_ListScroll` 相同 |
| 仅一个 Active | 若两个同时 Active，会看到两套 UI 叠影或滚轮命中两个 ScrollRect |
| Raycast | 隐藏 Scroll 不参与射线；显示侧 Scroll 的 Viewport 需 Raycast Target |

### 6.2 `ShopFormLogic` 与行引用

| 项 | 说明 |
|----|------|
| `rowHpBall` / `rowMpBall` | 应指向 **Buy** 的 `Content` 下子节点；Inspector 手动拖或 Find 路径含 `Bar_ListScroll_Buy` |
| `SwitchToBuyTab` 里 `SetRowActive` | 只影响购买行；**不要**误操作 Sell Content 子节点 |
| `LayoutBuyRowsVertically` | 有 Scroll 时仍应 **skip**（由 VerticalLayoutGroup 排布） |

### 6.3 Tab 按钮

| 场景节点名 | 程序字段建议 | 说明 |
|------------|--------------|------|
| `BUY` | `btnBuy` | 已存在；**本任务不做** Selected / 高亮态切换 |
| `SELL` | `btnSell` | 新增；与 0629 `BtnSell` 命名略有差异，**以场景为准** |

### 6.4 行 Button vs 滚轮

继承 0704 §6.3：`Shop_Bar` 根 Button 可能干扰拖拽；**本阶段优先保证滚轮**。

### 6.5 与 0629 验收关系

| 0629 条目 | 本任务覆盖 | 剩余 |
|-----------|------------|------|
| P1-2 购买 Tab 显示货单行 | 部分（行数 = Editor 摆放；接数据后再验内容） | 接数据后完整验收 |
| P1-3 出售 Tab | ✅ 本任务：出售 Scroll **可见** | 出售**数据**仍待阶段六 |
| A9 滚轮 | ✅ 两套列表分别验收 | — |

---

## ⑦ 验收清单（本任务 · Tab + 双滚动）

| # | 操作 | 期望 |
|---|------|------|
| TB-V1 | 看 Hierarchy | `Bar` 下有 `Bar_ListScroll_Buy` + `Bar_ListScroll_Sell`；Sell 默认 Inactive |
| TB-V2 | Play 默认状态 | 仅 **购买** 列表可见；显示行数 = `Bar_ListScroll_Buy/Content` 下你摆放的子节点数 |
| TB-V3 | 点 **SELL** | 购买列表隐藏，**出售** 列表显示；行数 = Sell Content 下你摆放的子节点数 |
| TB-V4 | 点 **BUY** | 切回购买列表；出售列表隐藏 |
| TB-V5 | 购买列表滚到底 → 切 SELL → 切 BUY | 购买列表**仍在底部**（滚动位置保留） |
| TB-V6 | 任一侧行数超过视口，滚轮 | 仅当前可见侧 Content 滚动；无侧边 Scrollbar |
| TB-V7 | Console | 无 NullReference；可选 `[ShopDebug]` 记录 Tab 切换 |

> **接数据后追加**：0629 P1-2 / P1-3 / A7 / SC-V6 数量输入与决定按钮。

---

## ⑧ 待确认问题

| ID | 问题 | 影响 | 状态 |
|----|------|------|------|
| ~~Q1~~ | 单列表还是双列表 | — | ✅ **双 ScrollView**（本任务定稿，推翻 0704 Q5） |
| ~~Q2~~ | 购买 / 出售侧各多少行 | — | ✅ **Editor 自行决定**；`Content` 下有几行就是几行，程序不写死 |
| ~~Q3~~ | Tab 高亮是否本任务做 | — | ✅ **不做**；留 UI 合层任务 |
| Q4 | `0629` 阶段六出售行是否仍用 `Shop_Bar.prefab` | Sell 接数据 | 📝 暂定 **是**；素材类行复用同一行壳 |

> 仍无结论的写入 `Assets/Doc/OPEN_QUESTIONS.md`。

---

## ⑨ 给程序看的入口索引

| 主题 | 路径 |
|------|------|
| 商店逻辑（Tab 切换入口） | `Assets/Scripts/Game/GameRuntime/UI/FormLogic/Shop/ShopFormLogic.cs` |
| 滚动壳 Editor 工具 | `Assets/Scripts/Game/GameRuntime/UI/FormLogic/Shop/Editor/ShopBarListScrollSetupEditor.cs` |
| 行预制体 | `Assets/GameRes/Prefabs/UI/Shop/Shop_Bar.prefab` |
| 测试场景 | `Assets/GameRes/Scenes/Village_Shop.unity` → `UI_Shop/Bar` |
| 0704 滚动壳（尺寸 / 行距） | `Assets/Doc/执行文档/0704/Shop_Bar列表滚动_架构溯源与施工执行说明.md` |
| 策划双 Tab 需求 | `Assets/Doc/执行文档/0629/商店系统_策划拆解_执行说明.md` §3.5 / 阶段六 |
| Debug 日志规范 | `ShopDebugLogger.cs` |

---

## ⑩ 文档变更记录

| 日期 | 说明 |
|------|------|
| 2026-07-05 | 初稿：扫描 Village_Shop 单 `Bar_ListScroll` + 未绑 SELL；定稿双 Scroll `Bar_ListScroll_Buy/Sell` + Tab SetActive 方案；分阶段 TB-0～TB-4 与验收 TB-V1～V7 |
| 2026-07-05 | 策划对齐：行数由 Editor 自行摆放（程序不写死）；Tab 按钮高亮明确**不做**；Q2/Q3 关闭 |
