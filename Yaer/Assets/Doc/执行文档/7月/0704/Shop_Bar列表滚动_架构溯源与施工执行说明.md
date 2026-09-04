# Shop · Bar 列表滚动 — 架构溯源与施工执行说明

**文档性质**：架构侦探产出（只读溯源 + 施工指引；**本阶段不改代码**）  
**调查日期**：2026-07-04  
**依据**：
- `Assets/Doc/00_MASTER_PROMPT.md`【架构侦探】
- `Assets/Doc/执行文档/0629/商店系统_策划拆解_执行说明.md` §3.4（滚轮 + 滚动条）
- `Assets/Doc/执行文档/0704/商店界面合层转UI组件_架构溯源与施工执行说明.md`
- 关联场景：`Assets/GameRes/Scenes/Village_Shop.unity`（`UI_Shop` / `Bar`）
- 关联 Prefab：`Assets/GameRes/Prefabs/UI/Shop/Shop_Bar.prefab`

**Unity 版本**：2020.3.48f1  

---

## ① 结论（一句话）

**在 `Bar` 下的可视区域 `Bar_BG` 里搭一套纵向 `ScrollRect`：`Content` 里放若干 `Shop_Bar` 空壳实例，Viewport 按 **8px 行距** 锁成「正好露出 6 行」；购买 / 出售 **共用这一条列表**；本阶段只做滚动壳，不接道具数据与 `ShopFormLogic` 行绑定。**

---

## ①.1 范围冻结（2026-07-04 · 策划 / 施工对齐）

| 项 | 本阶段约定 | 说明 |
|----|------------|------|
| **行间距** | **16 px** | `Content` 上 VerticalLayoutGroup **Spacing = 16**；2026-07-05 Play 测试由 8 调至 16 |
| **侧边滚动条** | **运行时隐藏** | `Scrollbar Vertical` **Disable** + ScrollRect **Vertical Scrollbar = None**；仅 **滚轮** 滚动 |
| **施工范围** | **只做滚动壳** | `Shop_Bar` 仅作列表行占位；**不**在本阶段补 `TxtName` / `TxtPrice` / `TxtStock`，**不**绑 `ShopFormLogic.rowHpBall` |
| **购买 / 出售 Tab** | **共用同一 `Bar_ListScroll`** | 不做第二个 ScrollView；后续切 Tab 时对同一 `Content` **Clear + 按页填充**（本阶段可只测购买侧占位行） |
| **0629 阶段一～四** | **本阶段不验收** | `TxtTotal`、决定 Log 等留待「Shop_Bar 接数据」任务，见 §7 备注 |

---

## ② 玩家在做什么 / 会遇到什么现象

| 现象 | 原因（生活类比） |
|------|------------------|
| 商品只有 2 条时列表正常，以后加多了**行会挤出卷轴底图** | 现在 `Shop_Bar` 直接摆在 `Bar` 下，**没有裁剪框**，像把菜单条贴在窗口外面 |
| 策划要求列表区**最多同时看见 6 行** | 需要「橱窗」（Viewport + Mask），裁剪区与 `Bar_BG` 同高 |
| 列表滚动方式 | **滚轮** 在列表区上下滑；**不显示** 右侧 Scrollbar（2026-07-05 定稿） |

**生活类比**：`Bar_BG` 是商店的「展示柜玻璃」；`Shop_Bar` 是柜里每一层货架；超过 6 层就要在玻璃里上下滑动看，而不是把整面墙撑高。

---

## ③ 工程现状快照（架构侦探 · 只读）

### 3.1 场景 `Village_Shop` · `Bar` 区域（当前）

```
UI_Shop（Canvas + ShopFormLogic）
└── Bar                               ← 列表区父节点（RectTransform 100×100 占位，子节点绝对定位）
    ├── BG                            ← ⚠ 场景里叫 BG，需求文档称 Bar_BG；482×559，仅 Image
    └── Shop_Bar（Prefab 实例 ×1）      ← 482×88，位置 (-449, 207)
```

| 节点 | 尺寸（RectTransform） | 组件 | 说明 |
|------|----------------------|------|------|
| `Bar/BG` | 482 × **559** | Image（Sprite 未赋） | 可视上为列表背景框；**无 Mask、无 ScrollRect** |
| `Shop_Bar` 实例 | 482 × **88** | 见 §3.2 | 单行栏位；**不在 BG 子级下**，与 BG 并列 |

**Viewport 容量估算（行距 16px · 2026-07-05）**：

```
单行 Shop_Bar 高度 rowHeight = 88 px
行间距 spacing             = 16 px（VerticalLayoutGroup）
6 行 Content 总高          = 88 × 6 + 16 × 5 = 608 px
Bar_BG / Viewport 高度     = 559 px（与底图内缘对齐）
可见区                     ≈ 5 行完整 + 第 6 行部分（行距加大后取舍）
```

施工时 **Viewport 高度先设 568**；若与 `Bar_BG` 内缘对不齐，在 Play 模式下按 §4.2 公式反推行距或略扩底图内缘（**待测试调整**，不阻塞 SC-0）。

### 3.2 行预制体 `Shop_Bar.prefab`

**路径**：`Assets/GameRes/Prefabs/UI/Shop/Shop_Bar.prefab`

```
Shop_Bar（482×88）                    ← Image + Button（行底/选中，Sprite Swap）
└── Shadow（482×88，y:-14.9）          ← Image + Button（行选中阴影层）
```

| 项 | 现状 | 与旧 `Row_HpBall` 脚本关系 |
|----|------|---------------------------|
| 行高 | **88 px**（`SizeDelta.y`） | `ShopFormLogic.buyRowVerticalSpacing` 默认 80，**需改为以 88 为准** |
| 数据列 | **无** `TxtName` / `TxtPrice` / `TxtStock` / `Image` 图标 | **本阶段不补**；接数据任务再改 Prefab / 实例 |
| 行选中 | 根 + Shadow 各挂 Button | 与 ScrollRect 滚动手势可能冲突，见 §6.3 |
| `ShopFormLogic.rowHpBall` | 场景中已 **置空（null）** | 逻辑尚未绑到 `Shop_Bar` |

### 3.3 程序侧（滚动相关）

| 项 | 状态 |
|----|------|
| `ShopFormLogic` 列表布局 | `LayoutBuyRowsVertically()` **手动改 localPosition**；**无 ScrollRect** |
| 工程内 ScrollRect 先例 | `AchievementFormLogic` + `AchievementPanel.prefab`（Viewport / Content / 可选 Slider 同步） |
| `0629` §3.4 滚动验收 | **未做**（整系统验收 A9） |

### 3.4 命名对齐（BG → Bar_BG）

| 来源 | 名称 |
|------|------|
| 当前场景 | `Bar/BG` |
| 本需求 / 施工文档 | **`Bar_BG`** |

**建议**：施工时将 `BG` **重命名为 `Bar_BG`**，避免与 `UI_Shop/BG`（全屏背景）混淆；脚本 `Find` 时使用常量 `"Bar_BG"`。

---

## ④ 目标 Hierarchy（施工完成后）

### 4.1 推荐结构（ScrollRect 标准 + 与策划对齐）

```
Bar
├── Bar_BG                            ← 列表区域外框底图（Image，Raycast Target 关）
└── Bar_ListScroll                    ← 挂 ScrollRect；Rect 与 Bar_BG 内缘对齐（高 559）
    ├── Viewport                      ← RectMask2D；高度 **559**（= Bar_BG）
    │   └── Content                   ← VerticalLayoutGroup Spacing **16** + ContentSizeFitter
    │       ├── Shop_Bar
    │       └── …                     ← 第 7 条起需滚轮滚动才可见
    └── Scrollbar Vertical            ← **Disable**（保留节点供 Editor 调试，运行时不可见）
```

**要点**：

- **裁剪边界** = `Viewport`（必须在 `Bar_BG` 范围内，不超出边框图）
- **滚动的对象** = `Content` 的 Y 轴；`Shop_Bar` 只做 `Content` 的子物体
- **`Bar_BG` 本身不滚动**；仅作装饰底图
- **购买 / 出售共用**：仅维护 **一套** `Bar_ListScroll`；切 Tab 时不新建第二个 ScrollView（后续对 `Content` 换数据，见 §4.3）

### 4.2 尺寸公式（6 行 Content · 行距 16px · 2026-07-05）

| 量 | 公式 / 值 | 说明 |
|----|-----------|------|
| 单行高 `rowHeight` | **88** | 来自 `Shop_Bar.prefab` `SizeDelta.y` |
| 行间距 `spacing` | **16** | VerticalLayoutGroup **Spacing = 16**（Play 测试由 8 加大） |
| **Bar_BG / Viewport 高** | **559** | 与列表底图内缘对齐；**不**用 608 撑出底图 |
| **Content 高度（6 行）** | `88 × 6 + 16 × 5 = **608**` | ContentSizeFitter 竖向 Preferred Size |
| ScrollRect | Vertical=ON，Horizontal=OFF | Movement: Clamped；**Vertical Scrollbar = None** |

**改 spacing 后 Content 总高速算**：

```
Content 高 = rowHeight × 行数 + spacing × (行数 - 1)
例：8 行 → 88×8 + 16×7 = 816
```

> **重要**：Viewport 高度跟 **Bar_BG**，不跟 Content 公式；行距加大后裁剪区内约 **5～6 行**，靠滚轮看其余行。

### 4.2.1 滚动条策略（2026-07-05 定稿）

| 项 | 约定 |
|----|------|
| 玩家操作 | **仅滚轮**（列表区 Hover + 滚轮） |
| `Scrollbar Vertical` | Hierarchy **保留但 Disable**；ScrollRect 不绑引用 |
| 0629 §3.4 滚动条 | 商店正式版 **不做** 右侧可见滚动条；与策划确认后偏离原长图 |

### 4.3 购买 / 出售 Tab · 共用同一滚动列表

| 项 | 约定 |
|----|------|
| Scroll 结构 | 全商店 **只有** `Bar` 下这一套 `Bar_ListScroll` |
| 购买 Tab | `Content` 下为购买货单行（本阶段：若干 `Shop_Bar` 空壳） |
| 出售 Tab | **同一 `Content`**；切 Tab 时 **Destroy/Clear 子节点再填出售行**（逻辑属 0629 阶段六，本阶段不实现） |
| 本阶段 | Tab 按钮可仍只做壳子；滚动验收 **不区分** 购买 / 出售，只在 `Content` 里堆 7+ 条 `Shop_Bar` 测滚动 |

**后续接数据（不在本阶段）**：

| 方案 | 说明 |
|------|------|
| 推荐 | `ShopBarListController` 对同一 `Content`：`RefreshBuyRows()` / `RefreshSellRows()` |
| 与旧脚本 | `Row_HpBall` 命名或 `ShopFormLogic` 绑定 **延至接数据任务**，本阶段不做了 |

---

---

## ⑤ 分阶段施工

### 阶段 SC-0 · 重命名 + 建 Scroll 壳（仅 Editor）

| 做 | 不做 |
|----|------|
| `Bar/BG` → **`Bar_BG`** | 不改 `ShopFormLogic` |
| 在 `Bar` 下建 **`Bar_ListScroll`**（UI → Scroll View） | 不补 TxtName / TxtStock 等数据列 |
| Viewport 高度先设 **559**（= Bar_BG）；Content **Spacing = 16** | **不显示** 侧边 Scrollbar（Disable + 解绑） |
| 确认全店 **仅这一套** Scroll（出售 Tab 不复用第二套） | 不实现 Tab 切 Content 逻辑 |

**Unity 操作摘要**：

1. 选中 `Bar` → 右键 **UI → Scroll View**，命名为 `Bar_ListScroll`。
2. 删除 Scroll View 自带 **Horizontal Scrollbar**；**Vertical Scrollbar 保留但 Disable**。
3. 选中 `Viewport`：RectMask2D；**Height = 559**（与 Bar_BG 一致）。
4. 选中 `Content`：Vertical Layout Group — Spacing **16**，Child Alignment Upper Center，Control Child Size 宽/高 ✓。
5. ScrollRect → **Vertical Scrollbar** 拖 **None**（或留空）。

**验证 SC-0**：Hierarchy 含 `Bar_BG` + `Bar_ListScroll/Viewport/Content`；Content Spacing = 16；Scrollbar Vertical 未激活。

### 阶段 SC-1 · 迁入 Shop_Bar 空壳（滚动占位）

1. 把现有 `Bar` 下并列的 `Shop_Bar` **拖入 `Content` 下**（或重新拖 Prefab 实例）。
2. 本阶段 **保持实例名 `Shop_Bar`**（或 `Shop_Bar (1)`…），**不改** `Row_HpBall`，**不绑** `ShopFormLogic`。
3. 先放 **2～3 条** 空壳即可确认 Layout；**不**在 Prefab 实例内加 `TxtName` / `TxtStock` 等。

**验证 SC-1**：Play → `Content` 内 2 条 `Shop_Bar` 纵向排列、**行距约 16px**；无侧边滚动条。

### 阶段 SC-2 · 验证「超过 6 行才滚动」

1. **临时**在 `Content` 下 **Duplicate `Shop_Bar` 至 7～8 条**（命名 `Row_Test3`…，无需接逻辑）。
2. Play：Viewport 内**最多看见 6 行**；第 7 行在框外。
3. **鼠标滚轮**在列表区滚动 → Content 上下移动。
4. 滚到底 / 顶时 **Clamped** 不越界。

**验证 SC-2（验收表 SC-1～SC-4）**：见 §7。

### 阶段 SC-3 · 程序改动（本阶段 · 可选 / 最小）

**本阶段以 Editor 搭 Scroll 壳为主**；`ShopFormLogic` **可不改**。若 Play 时旧测试行与 Scroll 冲突，仅 **禁用** 场景里 Canvas 下旧的 `Row_HpBall` 等节点，勿删脚本。

**接数据阶段再改（预告，非本任务）**：

```csharp
// ShopFormLogic.cs — 接 Shop_Bar + 共用列表时再实现
// [SerializeField] ScrollRect barListScroll;   // 购买/出售共用
// if (barListScroll != null) 跳过 LayoutBuyRowsVertically()
// Tab 切换：Clear Content 子节点 → 按页 Instantiate Shop_Bar
```

**验证 SC-3**：7 行空壳时间距仍为 16px、无重叠；**不要求** TxtTotal / 决定按钮。

### 阶段 SC-4 · 滚动条隐藏 + 滚轮验收

1. **`Scrollbar Vertical` → Disable**（或 `SetActive(false)`）。
2. ScrollRect **Vertical Scrollbar** 字段 **None**。
3. Play：滚轮可滚动；**Game 视图不出现** 右侧滚动条。

**验证 SC-4**：仅滚轮滚动；侧边 Scrollbar **不可见**。

---

## ⑥ 技术要点与踩坑

### 6.1 ScrollRect 与 Bar_BG 对齐

| 检查项 | 说明 |
|--------|------|
| Viewport ⊂ Bar_BG | Viewport 四角不超出底图透明区 |
| Pivot | Viewport / Content 建议 Pivot Y=**1**（从上往下排） |
| Canvas | `UI_Shop` 已有 GraphicRaycaster；Scroll View 需在同一 Canvas 下 |

### 6.2 行距 16px + Viewport 559

| 做法 | 说明 |
|------|------|
| **Content Spacing = 16** | 2026-07-05 Play 测试由 8 加大 |
| **Viewport 高 = Bar_BG（559）** | 裁剪框不超出底图；6 行 Content 总高 608，靠滚轮看全 |
| **Content Spacing** | 仅改 VerticalLayoutGroup **Spacing**，不要再用 `LayoutBuyRowsVertically` 手调 Y |

### 6.3 行 Button vs 滚动手势

`Shop_Bar` 根节点 **Button + Raycast Target** 会吃掉拖拽。若出现「拖不动列表」：

1. 行内仅 **Shadow / 选中框** 用 Button；或  
2. ScrollRect 勾选 **Drag Scroll Only On Begin Drag**（2020 无此项则用 EventTrigger 区分）；或  
3. 根 Image **Raycast Off**，子节点「数量输入框」单独 Raycast On。

**本阶段优先**：保证 **滚轮** 可用；行点击选中可后置。

### 6.4 与 `0629` 阶段关系

| 0629 阶段 | 与滚动关系 |
|-----------|------------|
| 阶段一～四 | **本滚动壳任务不验收**；接数据后再回归 P1～P4 |
| 阶段五～六 | 动态货单；**出售 Tab 填同一 Content**（`RefreshSellRows`） |
| 阶段九 A9 | 勾选 **滚轮**；侧边 Scrollbar **不验收** |

---

## ⑦ 验收清单（本任务 · 滚动专项）

| # | 操作 | 期望 |
|---|------|------|
| SC-V1 | 看 Hierarchy | `Bar` 下有 `Bar_BG` + `Bar_ListScroll/Viewport/Content` |
| SC-V2 | `Content` 内 2 条 `Shop_Bar` 空壳 | 2 行可见、**行距约 16px**；**无侧边滚动条** |
| SC-V3 | `Content` 临时 **8** 条空壳 | 裁剪区内约 **5～6 行**；第 7、8 行需滚轮 |
| SC-V4 | 鼠标在列表区滚轮 | Content 纵向滚动；不滚整个场景 |
| SC-V5 | 看 Game 视图 | **不出现** 右侧 Scrollbar |
| SC-V6 | — | **本阶段跳过**（无 TxtStock / 决定逻辑） |
| SC-V7 | 改 Game 窗口分辨率 | 6 行裁剪区仍对齐 `Bar_BG`，行距视觉可接受 |

> **接数据后追加验收**：`0629` P1～P4、SC-V6 改数量 + `[ShopDebug]`。

---

## ⑧ 待确认问题（剩余）

| ID | 问题 | 影响 | 状态 |
|----|------|------|------|
| ~~Q1~~ | ~~行间距~~ | — | ✅ **16px**（2026-07-05 由 8 调大） |
| ~~Q2~~ | ~~Bar_BG 与 Viewport 差值~~ | — | ✅ Viewport **= Bar_BG 559** |
| ~~Q3~~ | ~~Scrollbar 显示策略~~ | — | ✅ **运行时隐藏**，仅滚轮 |
| ~~Q4~~ | ~~本阶段是否接数据列~~ | — | ✅ **只做滚动壳** |
| ~~Q5~~ | ~~出售 Tab 是否共用列表~~ | — | ✅ **共用 `Bar_ListScroll`** |

> 仍无结论的写入 `Assets/Doc/OPEN_QUESTIONS.md`。

---

## ⑨ 给程序看的入口索引

| 主题 | 路径 |
|------|------|
| 行预制体 | `Assets/GameRes/Prefabs/UI/Shop/Shop_Bar.prefab` |
| 测试场景 Bar 区 | `Assets/GameRes/Scenes/Village_Shop.unity` → `UI_Shop/Bar` |
| 商店逻辑 | `Assets/Scripts/Game/GameRuntime/UI/FormLogic/Shop/ShopFormLogic.cs` |
| 行数量输入 | `ShopBuyRowQuantityInput.cs` |
| ScrollRect 参考 | `AchievementFormLogic.cs`、`AchievementPanel.prefab` |
| 策划滚动要求 | `0629/商店系统_策划拆解_执行说明.md` §3.4 |
| UI 合层文档 | `0704/商店界面合层转UI组件_架构溯源与施工执行说明.md` |

---

## ⑩ 文档变更记录

| 日期 | 说明 |
|------|------|
| 2026-07-04 | 初稿：扫描 `Bar`/`BG`/`Shop_Bar` 现状；定义 Bar_BG 内 6 行 Viewport + ScrollRect 施工阶段 SC-0～SC-4 |
| 2026-07-04 | 范围冻结：行距 **8px**（Viewport **568**，测试可调）；**只做滚动壳**；购买/出售 **共用** `Bar_ListScroll`；SC-1/验收/0629 关系同步修订 |
| 2026-07-05 | Play 反馈：行距 **8→16px**；Viewport **559**（= Bar_BG）；**侧边 Scrollbar 运行时隐藏**，仅滚轮；SC-4 / 验收 / Q1～Q3 同步 |
