# Shop UI 三小问题 — 滚动透明 / MpBall 缺失 / 分辨率缩放 — 架构溯源与修复执行说明

**文档版本**：v1（2026-07-06）  
**文档性质**：架构侦探产出（只读溯源 + 修复施工指引）  
**调查日期**：2026-07-06  
**依据**：
- `Assets/Doc/00_MASTER_PROMPT.md`【架构侦探】
- `Assets/Doc/执行文档/0704/Shop_Editor烘焙双列表_MainItemDatabase一键刷行_施工执行说明.md`（EB 烘焙方案 · 当前基线）
- `Assets/Doc/执行文档/0704/Shop_货单瘦身_MainItemDatabase驱动Shop_Bar刷新_架构溯源与施工执行说明.md`（购买过滤规则 · MpBall 约定）
- `Assets/Doc/执行文档/0704/商店界面合层转UI组件_架构溯源与施工执行说明.md`（工程 UI 标准 · CanvasScaler）
- 关联场景：`Assets/GameRes/Scenes/Village_Shop.unity`
- 关联脚本：`ShopScrollShellHelper.cs`、`ShopListBakeEditor.cs`、`ShopFormLogic.cs`
- 关联配置：`Assets/GameRes/Config/MainItem/MainItemDatabase.asset`

**Unity 版本**：2020.3.48f1  

---

## ① 结论（一句话）

**三个问题里：① Scroll 根 Image 半透明是 Unity 默认 Scroll View + 现有修正逻辑未强制写 alpha，应在 `ShopScrollShellHelper` 与 Bake 流水线里统一设为完全透明；② MpBall 未出现在购买列表不是 Bake 漏刷，而是 Database `buyPrice=-1` 被既定过滤规则排除，若策划要上架需先改买价再 Bake；③ UI_Shop 的 CanvasScaler 仍是「固定像素大小」且未对齐工程 1920×1080 标准，4K 下不会随屏幕放大。**

---

## ② 玩家在做什么 / 会遇到什么现象

| # | 现象 | 原因（生活类比） |
|---|------|------------------|
| 1 | 每次跑 **Bake** 或新建 `Bar_ListScroll_Buy/Sell` 后，Scroll 根节点 Image **带着一层灰蒙蒙的半透明底**，要手动把 Alpha 调成 0 | 像买卷帘门时厂家默认贴了层保护膜，我们的安装工具只保证「门能感应手指」，没顺手撕膜 |
| 2 | 点 Bake 刷新购买列表后，**没有 `Shop_Bar_MpBall`（体力之珠）** | 不是厨师忘了做菜——菜单上这道菜的标价是「不卖」（`buyPrice=-1`），厨房按规矩**故意不上架** |
| 3 | Game 窗口切到 **4K**，整个 `UI_Shop` **显得很小**，不像背包/设置面板那样跟着屏幕变大 | 商店 UI 还按「老电视固定像素」排版，大屏只是周围空了一大圈，字和按钮尺寸不变 |

---

## ③ 架构溯源

### 3.1 问题一：Bar_ListScroll_Buy / Sell 的 Image 不透明

#### 3.1.1 场景实机快照（`Village_Shop.unity` · 只读）

| 节点 | Image.m_Color.a | raycastTarget | 来源 |
|------|-----------------|---------------|------|
| `Bar_ListScroll_Buy` | **0.392** | true | Unity `DefaultControls.CreateScrollView` 默认背景 |
| `Bar_ListScroll_Sell` | **0.392** | true | Duplicate Buy 时一并复制 |
| Viewport | 无 Image（已按 Fix-S1 剥离） | — | ✅ 符合 EB 文档 |

#### 3.1.2 代码链路

```
Tools/Shop/Bake… 或 Play Awake
    → ShopListBakeEditor.EnsureScrollShell()     // 新建时用 DefaultControls.CreateScrollView
    → ShopScrollShellHelper.ApplyInteractionFixes()
        → EnsureScrollRootRaycastTarget()        // 仅保证有 Image + raycastTarget=true
        → StripRedundantViewportImage()          // 只处理 Viewport，不处理 Scroll 根
```

**根因**：

| 项 | 现状 | 缺口 |
|----|------|------|
| `EnsureScrollRootRaycastTarget` | 无 Image 时 **AddComponent** 并设 `alpha=0.02` | **已有 Image 时不改 color**，保留 Unity 默认 0.392 |
| `ShopListBakeEditor.EnsureScrollShell` | 创建/校正壳后调 `ApplyInteractionFixes` | 未单独校正 Scroll 根 Image 透明度 |
| EB 文档 Fix-S2 | 写「透明即可」示例 alpha=0.02 | 策划现要求 **完全透明（alpha=0）** |

**为何需要根节点 Image**：Viewport 去掉 Image 后，ScrollRect 根节点仍需 **raycastTarget** 才能接收滚轮/拖拽命中（见 `0704/Shop_货单瘦身…` §⑪ Fix-S2）。  
**正确做法**：保留 Image 组件，**Color.a = 0**，`sprite = null`（或 None），`raycastTarget = true`。

#### 3.1.3 与 Viewport Image 的区分

| 节点 | 要不要 Image | 本任务 |
|------|-------------|--------|
| `Viewport` | **不要**（RectMask2D 足够） | 已实现，无需改 |
| `Bar_ListScroll_*` **根** | **要**（仅作射线命中，视觉全透明） | **本次修复目标** |

---

### 3.2 问题二：Bake 购买列表为何没有 MpBall

#### 3.2.1 过滤规则（EB 方案 · 与货单瘦身一致）

`ShopListBakeEditor.FilterEntries`：

```csharp
// Buy：CostItem 且 buyPrice >= 0
if (entry.itemType == BagItemType.CostItem && entry.buyPrice >= 0)
```

#### 3.2.2 MainItemDatabase 中 MpBall 当前数据

| 字段 | 值 | 说明 |
|------|-----|------|
| `itemId` | `3` → `EMainItemName.MpBall` | 体力之珠 |
| `itemType` | `1`（CostItem） | 类型符合购买候选 |
| `buyPrice` | **-1** | **表示不可购买** |
| `sellPrice` | -1 | 不可出售 |

**结论**：MpBall **被 `buyPrice >= 0` 条件排除**，属于 **0704 货单瘦身已定稿的设计**，不是 Bake 漏行。

#### 3.2.3 文档与场景交叉验证

| 来源 | 约定 |
|------|------|
| `Shop_货单瘦身…` §3.2 / DB-V2 | 购买页 **7 行，不含 MpBall** |
| `Shop_Editor烘焙双列表…` §8.3 | **Buy=7（无 MpBall）** |
| `Village_Shop.unity` Bake 后 Content | 现有 7 行：`HpBall`、`SmallHp/SmallMp/LargeHp/LargeMp Potion`、`BowlLiquid`、`Fish`；**无 `Shop_Bar_MpBall`** |

#### 3.2.4 与旧版 Row_MpBall 的关系

| 时期 | MpBall 出现方式 |
|------|----------------|
| 0629 阶段一～四 | 场景预置 `Row_MpBall` 或 `autoCreateMpBallRow` 克隆 |
| EB 烘焙方案（当前） | **完全由 Database 过滤 + Bake 决定行集合**；运行时 **不再** Instantiate / Refresh |

若策划期望购买页仍有 MpBall，**不是改 Bake 代码绕过过滤**，而是：

1. 在 `MainItemDatabase` 将 MpBall 的 `buyPrice` 改为 **≥ 0 的正式售价**（例如 200，与 HpBall 对齐需策划定）；
2. 再执行 `Tools → Shop → Bake Shop Lists From MainItemDatabase`；
3. 购买行数应变 **7 → 8**，Hierarchy 出现 `Shop_Bar_MpBall`。

> **待策划确认（见 §⑧ Q1）**：MpBall 是继续「不上架」还是补买价上架。在确认前，**不建议**放宽 `buyPrice>=0` 过滤或硬编码插入 MpBall 行。

---

### 3.3 问题三：UI_Shop 不随分辨率缩放（4K 显小）

#### 3.3.1 场景实机快照（`UI_Shop` 根节点）

| 组件 | 当前值 | 工程标准（`ItemShowPanel.prefab`） |
|------|--------|-----------------------------------|
| `CanvasScaler.m_UiScaleMode` | **0 = Constant Pixel Size** | **1 = Scale With Screen Size** |
| `CanvasScaler.m_ReferenceResolution` | 800 × 600（恒定像素模式下无效） | **1920 × 1080** |
| `CanvasScaler.m_ScreenMatchMode` | 0（Match Width Or Height） | 0 |
| `CanvasScaler.m_MatchWidthOrHeight` | 0 | 0 |
| `RectTransform` 锚点 | **(0,0)-(0,0)** 左下角单点 | **(0,0)-(1,1)** 全屏拉伸 |
| 子节点布局 | 大量 **中心锚点 + 固定像素** `anchoredPosition` / `sizeDelta` | 同左，但依赖 Scaler 整体缩放 |

#### 3.3.2 根因归纳

1. **主因**：`Constant Pixel Size` → 分辨率从 1080p 升到 4K，UI **像素尺寸不变**，占屏比例变小。  
2. **次因**：`UI_Shop` 未按 `0704/商店界面合层转UI组件…` §3.5 对齐 **1920×1080 Scale With Screen Size**。  
3. **布局特征**：`Bar`、Tab、合计区等节点使用固定坐标（如 `Bar_ListScroll_Buy` sizeDelta ≈ 482×559），在 Scaler 修正后会 **整体同比缩放**，一般无需逐控件改坐标（需目检边距）。

#### 3.3.3 与「合层 Sprite」双轨的遗留关系

`Village_Shop` 仍可能是 **场景内直接摆 `UI_Shop` Canvas** 的测试场，尚未做成独立 `ShopPanel.prefab` 走 GF 打开。  
**本任务只修场景内 `UI_Shop` 的 Scaler**；将来 UI-4 做 `ShopPanel.prefab` 时 **应复用同一套 Scaler 参数**，避免二次踩坑。

---

## ④ 修复方案定稿

### 4.1 Fix-T1 · Scroll 根 Image 完全透明（程序 · 最小改动）

| 步骤 | 文件 | 改动 |
|------|------|------|
| T1-1 | `ShopScrollShellHelper.cs` | `EnsureScrollRootRaycastTarget`：**无论新建或已有 Image**，统一 `color = (1,1,1,0)`；`sprite = null`；`raycastTarget = true` |
| T1-2 | 同上 | 删除/替换原 `alpha=0.02` 魔法数；加注释说明「alpha=0 仍可射线命中」 |
| T1-3 | `ShopListBakeEditor.cs`（可选加固） | `EnsureScrollShell` 末尾已调 `ApplyInteractionFixes` → T1-1 完成后 **Bake 自动生效**；无需重复逻辑 |
| T1-4 | `Village_Shop.unity` | 跑一次 Bake 或 Play 一次（Awake 调 `ApplyScrollInteractionFixes`）后，Inspector 确认 Buy/Sell 根 Image **Alpha=0** |

**替代方案（不推荐）**：

| 方案 | 缺点 |
|------|------|
| 每次 Bake 后策划手调 Alpha | 违背 EB「一键验收」目标 |
| 去掉 Scroll 根 Image，只靠子节点接射线 | 滚轮命中不稳定，与 Fix-S2 结论冲突 |

---

### 4.2 Fix-T2 · MpBall 缺失（策划 + 数据 · 非程序 Bug）

| 路径 | 操作 | 结果 |
|------|------|------|
| **A · 保持不上架（默认）** | 不改 Database；文档告知策划 | Buy 继续 **7 行**，符合现有验收 |
| **B · 要上架 MpBall** | `MainItemDatabase` → MpBall → `buyPrice` 改为 **≥0** → 再 Bake | Buy **8 行**，含 `Shop_Bar_MpBall` |

**程序侧仅当策划选 B 且需合计/购买逻辑覆盖 MpBall 时**（本阶段可选、非 T2 必须）：

| 项 | 说明 |
|----|------|
| `ShopFormLogic` 合计 | 当前只绑 **HpBall** 数量 → `TxtTotal`；MpBall 上架后合计规则需策划另开任务 |
| `ShopBuyRowQuantityInput` | Bake 时已对 **所有购买行** `EnsureShopBuyRowQuantityInput`，MpBall 行会自动有数量框 |

**禁止**：为 MpBall 单独写 `if (itemId == MpBall) Instantiate` 绕过 Database 过滤——破坏 EB 单一数据源。

---

### 4.3 Fix-T3 · UI_Shop 分辨率自适应（场景 + 可选 Editor 加固）

#### 4.3.1 场景手工修改（推荐先做）

选中 `Village_Shop` → `UI_Shop`：

| 组件 | 目标值 |
|------|--------|
| `CanvasScaler` → UI Scale Mode | **Scale With Screen Size** |
| Reference Resolution | **1920 × 1080** |
| Screen Match Mode | Match Width Or Height |
| Match | **0**（偏宽屏；若竖屏测试多可改 0.5） |
| `RectTransform` | Anchor Min **(0,0)**，Max **(1,1)**，Pos/Size **0**（四向拉满） |

#### 4.3.2 验证分辨率

| Game 窗口 | 期望 |
|-----------|------|
| 1920×1080 | 与改前视觉接近（基准） |
| 2560×1440 | 整体 **同比变大** |
| 3840×2160（4K） | 不再「缩在角落」，按钮可点区域与视觉一致 |

#### 4.3.3 可选程序加固（防止以后再改回 Constant Pixel Size）

在 `ShopListBakeEditor.RunBake` 开头增加 `EnsureUiShopCanvasScaler(uiShop)`：

- 若 `CanvasScaler.uiScaleMode != ScaleWithScreenSize` → 校正为 1920×1080；
- 若根 `RectTransform` 非全屏 stretch → 校正锚点。

**替代方案**：

| 方案 | 适用 |
|------|------|
| 仅改场景、不改代码 | 个人测试场足够 |
| 抽 `ShopUiLayoutHelper` 供 Bake + 将来 `ShopPanel.prefab` 共用 | 准备正式 Prefab 化时 |

---

## ⑤ 施工阶段（FX-0 ～ FX-3）

| 阶段 | 内容 | 负责 | 验证 |
|------|------|------|------|
| **FX-0** | `ShopScrollShellHelper` 强制 Scroll 根 alpha=0 | 程序 | Bake 后 Buy/Sell 根 Image Alpha=0，滚轮仍可用 |
| **FX-1** | 策划确认 MpBall：不上架 **或** 填 buyPrice 后 Bake | 策划 | Buy 行数 7 或 8，与 Database 一致 |
| **FX-2** | `UI_Shop` CanvasScaler + 根 RectTransform 对齐 1920 全屏 | 程序/美术 | 1080p / 4K Game 窗口目检 |
| **FX-3** | （可选）Bake 工具内 `EnsureUiShopCanvasScaler` | 程序 | 新克隆场景不会回退 Constant Pixel Size |

---

## ⑥ 验收清单

| ID | 操作 | 期望 |
|----|------|------|
| FX-V1 | 跑 **Bake** → 选中 `Bar_ListScroll_Buy` 根 Image | **Color.a = 0**；Scene 视图无灰底 |
| FX-V2 | 同上 `Bar_ListScroll_Sell` | **Color.a = 0** |
| FX-V3 | Play → 购买列表区滚轮 | 列表 **仍可滚动**（射线命中未丢） |
| FX-V4 | 不 Play，展开 Buy Content | **7 行**（MpBall buyPrice 仍为 -1 时） |
| FX-V5 | （若策划改 MpBall buyPrice≥0）再 Bake | 出现 **`Shop_Bar_MpBall`**，Buy=8 |
| FX-V6 | Game 窗口 **1920×1080** Play | 布局与改 Scaler 前基本一致 |
| FX-V7 | Game 窗口 **3840×2160** Play | `UI_Shop` **明显大于** FX-V6，Tab/列表/决定按钮可正常点 |
| FX-V8 | Console | 无新增 Error；无 `[ShopFormLogic] RefreshBuyList` 类旧 Log |

---

## ⑦ 踩坑与约束

### 7.1 alpha=0 会不会点不到？

Unity UGUI 的 `Graphic.raycastTarget=true` 时，**alpha=0 仍参与射线检测**。若极端机型有问题，回退 alpha=0.01（肉眼仍视为全透明）。

### 7.2 MpBall 不是「刷新遗漏」

勿在 `BakeContent` 里硬编码补 MpBall；一切以 `MainItemDatabase` + 过滤规则为准。

### 7.3 Scaler 改了但个别控件仍偏

子节点若是 **锚在屏幕某一角的固定像素**，Scaler 会整体缩放；若个别装饰图仍错位，再单独调该节点锚点——**本任务不要求像素级重搭合层**。

### 7.4 Bake 与 Scaler 独立

改 CanvasScaler **不需要**重 Bake 列表；改 Database 买价 **需要**重 Bake。

---

## ⑧ 待确认问题

| ID | 问题 | 影响 | 建议 |
|----|------|------|------|
| Q1 | MpBall 是否要在商店购买页上架？若上架，买价多少？ | FX-1 / Buy 行数 | 暂保持 `-1`；要上架则策划改 `buyPrice` 后 Bake |
| Q2 | `Match Width Or Height` 用 0 还是 0.5？ | 超宽/超高屏边距 | 与 `ItemShowPanel` 一致先用 **0** |
| Q3 | 是否在 Bake 菜单强制校正 CanvasScaler？ | 仅 Village_Shop 测试场 vs 将来多场景 | 短期可只改场景；正式 Prefab 化时抽 Helper |

> 无结论时写入 `Assets/Doc/OPEN_QUESTIONS.md`，勿擅自改核心过滤规则或合计业务。

---

## ⑨ 给程序看的入口索引

| 主题 | 路径 |
|------|------|
| Scroll 交互修正 | `Assets/Scripts/Game/GameRuntime/UI/FormLogic/Shop/ShopScrollShellHelper.cs` |
| Bake 流水线 | `Assets/Scripts/Game/GameRuntime/UI/FormLogic/Shop/Editor/ShopListBakeEditor.cs` |
| 运行时 Tab/合计 | `Assets/Scripts/Game/GameRuntime/UI/FormLogic/Shop/ShopFormLogic.cs` |
| 道具表 | `Assets/GameRes/Config/MainItem/MainItemDatabase.asset` |
| 测试场景 | `Assets/GameRes/Scenes/Village_Shop.unity` |
| UI 标准参考 | `Assets/GameRes/Prefabs/UI/ItemShowPanel.prefab`（CanvasScaler 段） |

---

## ⑩ 文档关系

| 文档 | 关系 |
|------|------|
| `Shop_Editor烘焙双列表…`（0704） | EB 基线；T1 为其 Fix-S2 的补充（alpha 从 0.02 升级为 0 + 强制写已有节点） |
| `Shop_货单瘦身…`（0704） | MpBall 过滤规则权威来源；T2 不修改规则 |
| `商店界面合层转UI组件…`（0704） | T3 Scaler 标准来源 |

---

## ⑪ 修订记录

| 日期 | 说明 |
|------|------|
| 2026-07-06 | 首版：三问题溯源 + Fix-T1～T3 + FX 阶段与验收表 |
