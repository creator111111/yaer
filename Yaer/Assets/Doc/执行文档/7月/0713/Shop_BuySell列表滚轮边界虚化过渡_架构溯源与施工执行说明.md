# Shop · Buy/Sell 列表滚轮边界虚化过渡 — 架构溯源与施工执行说明

**文档版本**：v1（2026-07-13）  
**文档性质**：【架构侦探】产出 + 施工指引（**本阶段只写文档，不改代码 / 不改场景**）  
**触发**：策划反馈 `UI_Shop` 下 **`Bar_ListScroll_Buy` / `Bar_ListScroll_Sell`** 滚轮滚动时，列表上下边界 **太硬、像被刀切**，希望改成 **虚化过渡**（边缘渐隐），而不是直角裁剪。

**依据**：
- `Assets/Doc/00_MASTER_PROMPT.md`【架构侦探】
- `Assets/Doc/执行文档/0704/Shop_Bar列表滚动_架构溯源与施工执行说明.md`（Scroll 壳 · Viewport + RectMask2D）
- `Assets/Doc/执行文档/0704/Shop_购买出售双列表Tab切换_架构溯源与施工执行说明.md`（Buy/Sell 双 Scroll）
- `Assets/Doc/执行文档/0704/Shop_Editor烘焙双列表_MainItemDatabase一键刷行_施工执行说明.md`（Bake 校正 Viewport）
- `Assets/Doc/执行文档/0706/Shop_UI三小问题_滚动透明MpBall缺失分辨率缩放_架构溯源与修复执行说明.md`（Fix-S1/T1 · Viewport 无 Image）
- 关联场景：`Assets/GameRes/Scenes/Village_Shop.unity`（`UI_Shop/Bar`）
- 关联脚本：`ShopScrollShellHelper.cs`、`ShopListBakeEditor.cs`、`ShopBarListScrollSetupEditor.cs`

**Unity 版本**：2020.3.48f1  

---

## ① 结论（一句话）

**硬切割不是滚轮本身的问题，而是 Buy/Sell 两个 Viewport 上的 `RectMask2D` 把超出框外的行「一刀剪断」，且当前 `Softness = (0,0)` 完全没开软边；推荐最小改动：给两侧 Viewport 的 `RectMask2D` 打开纵向 Softness（约 24～40px），并写进 `ShopScrollShellHelper` + Bake，保证以后一键刷列表不会又变回硬切。**

**生活类比**：现在展示柜玻璃像「铁框裁纸刀」——货架一出框就齐刷刷断掉；目标是「磨砂渐隐边」——靠近上下边的商品慢慢变淡，再滚出去才看不见，眼睛不会被刀口硌一下。

---

## ② 玩家在做什么 / 会遇到什么现象

| # | 现象 | 原因（生活类比） |
|---|------|------------------|
| 1 | 购买 / 出售列表用滚轮上下滑时，**顶边或底边的一行突然被切成半截**，切口很齐 | 橱窗用的是「硬边玻璃」，没有雾边 |
| 2 | Buy 与 Sell 两边手感一样硬 | 两套 Scroll **各自**有 Viewport + RectMask2D，参数都是硬切 |
| 3 | 期望：靠近边界时行 **渐渐变淡**（虚化过渡），而不是直角切断 | 需要软遮罩 / 渐隐带，而不是只裁剪矩形 |

**说明（避免误解）**：

| 说法 | 本任务含义 |
|------|------------|
| **虚化过渡** | 边界附近 **透明度渐变**（alpha fade），露出底下的 `Bar_BG` |
| **不是** | 真·高斯模糊（Blur Shader / RT），成本高且本工程无现成先例 |
| **滚轮边界** | 指 **可视裁剪框上下沿**，不是 ScrollRect 滚到顶/底的回弹手感（Clamped 可另议，本任务不改） |

---

## ③ 架构溯源（只读）

### 3.1 Hierarchy（当前基线）

```
UI_Shop
└── Bar
    ├── Bar_BG                              ← 装饰底图，不滚动
    ├── Bar_ListScroll_Buy                  ← ScrollRect（购买）
    │   └── Viewport                        ← RectMask2D（硬裁剪）
    │       └── Content                     ← VerticalLayoutGroup Spacing=16
    │           └── Shop_Bar_* …
    └── Bar_ListScroll_Sell                 ← ScrollRect（出售 · Tab 互斥）
        └── Viewport                        ← RectMask2D（硬裁剪）
            └── Content
                └── Shop_Bar_* …
```

| 项 | 值 | 来源 |
|----|-----|------|
| Viewport 高 | ≈ **559**（= `Bar_BG`） | 0704 SC / EB Bake |
| 行高 / 行距 | **88** / **16** | `Shop_Bar` + VLG |
| 裁剪组件 | **`RectMask2D`** | Bake / Setup 强制挂载 |
| Viewport Image | **已剥离**（Fix-S1） | `ShopScrollShellHelper` |
| Scroll 根 Image | **alpha=0**，仅射线（Fix-T1） | 同上 |
| 侧边 Scrollbar | Disable / None | 仅滚轮 |

### 3.2 场景实机快照（`Village_Shop.unity`）

| 节点 | 组件 | `m_Softness` | 含义 |
|------|------|--------------|------|
| `Bar_ListScroll_Buy/Viewport` | `RectMask2D`（script guid `3312d773…`） | **`{x: 0, y: 0}`** | 左右/上下 **零软边 → 硬切** |
| `Bar_ListScroll_Sell/Viewport` | 同上（Duplicate 同源） | **`{x: 0, y: 0}`** | 同上 |

> Unity 2019.2+ / **2020.3** 的 `RectMask2D` 已内置 `softness`（`Vector2Int`，单位近似 **屏幕像素软边宽度**）。工程场景里字段已序列化，只是一直写成 0。

### 3.3 代码链路（谁保证「永远硬切」）

```
Tools → Shop → Bake Shop Lists…
    → ShopListBakeEditor.ConfigureViewport()
        → AddComponent<RectMask2D>()（若不存在）
        → Strip Viewport Image
        → ❌ 从不写 softness

Play Awake / 交互修正
    → ShopScrollShellHelper.ApplyInteractionFixes()
        → scrollSensitivity = 30
        → EnsureScrollRootRaycastTarget（alpha=0）
        → StripRedundantViewportImage
        → ❌ 从不写 softness

ShopBarListScrollSetupEditor.ConfigureViewport()
    → 与 Bake 同结构，同样 ❌ 不写 softness
```

**根因归纳**：

1. **主因**：`RectMask2D` 默认 Softness=0 → 像素级硬裁剪，滚动时半截行切口齐平。  
2. **固化原因**：Bake / Helper **只保证「有 RectMask2D + 无 Viewport Image」**，从未把 Softness 纳入校正清单 → 即使策划手调软边，下次 Bake 也可能被忽略（当前代码虽不重置已有 Softness，但**新建壳仍是 0**，且无统一默认值）。  
3. **非因**：滚轮灵敏度、Scroll 根透明、Clamped 回弹——这些都不制造「刀切」观感。

### 3.4 与既有文档的关系

| 文档 | 关系 |
|------|------|
| 0704 滚动壳 / 双列表 | 确立 Viewport + RectMask2D；**未提** Softness |
| 0706 Fix-S1 | Viewport **去掉 Image**，依赖 RectMask2D 裁剪 → 本任务 **继续保留 RectMask2D**，只加软边 |
| 0706 Fix-T1 | Scroll 根透明射线 → **不冲突**；虚化后仍靠根 Image 接滚轮 |

---

## ④ 方案对比（定稿前给策划/程序对齐）

### 方案 A · `RectMask2D.softness`（**推荐 · 最小改动**）

| 项 | 说明 |
|----|------|
| 做法 | Buy/Sell Viewport：`softness = (sx, sy)`，优先 **`sy > 0`**（上下渐隐） |
| 观感 | 靠近顶/底的行 **整体 alpha 渐变**，底下透出 `Bar_BG` |
| 优点 | 零新资源、零新节点、与现有 Mask 架构一致；Bake/Helper 一行赋值即可复现 |
| 缺点 | Softness 是 **矩形四边软化**（X/Y 轴对称），不能单独「只软底边不软顶边」；也不是真模糊 |
| 建议初值 | **`Softness X = 0`，`Softness Y = 32`**（约 ⅓～½ 行高可感知；Play 后可调 24～48） |

**替代初值说明**：

| Softness Y | 观感 |
|------------|------|
| 16 | 略软，仍偏「有一点刀口」 |
| **32（推荐起步）** | 明显渐隐，半行过渡自然 |
| 48～64 | 很软；可见区有效高度略「缩」，顶底始终偏淡 |

### 方案 B · 顶/底渐变遮罩叠层（Fade Overlay）

| 项 | 说明 |
|----|------|
| 做法 | 在 Scroll 根（或 `Bar`）下加 `FadeTop` / `FadeBottom` 两个 Image，Sprite 为竖向透明→底色渐变；`raycastTarget=false` |
| 优点 | 美术可控（高度、曲线、是否只在可滚动时显示） |
| 缺点 | 多节点 + 需渐变图；要跟 `Bar_BG` 底色对齐，否则发灰发脏；Tab 切换时 Buy/Sell 各一套或共用叠层需设计 |
| 适用 | 方案 A 软边仍不满意，或要 **非对称**（只软底边）时升级 |

### 方案 C · 第三方 Soft Mask / 自定义模糊 Shader

| 项 | 说明 |
|----|------|
| 做法 | Coffee Soft Mask、RT + Blur 等 |
| 结论 | **本阶段拒绝**：引入包/Shader、与 UGUI Mask 栈交互复杂，违背「最小改动 / 先可运行」 |

### 方案定稿（侦探建议）

| 决策 | 内容 |
|------|------|
| **采用** | **方案 A** |
| **不做** | 真模糊、改 Clamped/弹性、改行高/行距、动 `Shop_Bar` Prefab |
| **预留** | 方案 B 仅当策划验收 A 后仍喊「不够虚」时再开任务 |

---

## ⑤ 目标行为（施工完成后）

1. 购买列表滚轮：行滚出 **上/下沿** 时 **渐隐**，不再直角切断。  
2. 出售列表 **同样参数**（Tab 切过去手感一致）。  
3. 列表仍被框在 `Bar_BG` 内，**不会**露出框外半截清晰行。  
4. 滚轮命中、数量输入、Tab、Bake 流程 **行为不变**。  
5. 再次 Bake / Play Awake 后 Softness **仍保持约定值**（不靠策划每次手填）。

**目标 Softness（写入常量，可调）**：

```text
RectMask2D.softness = new Vector2Int(SoftnessX, SoftnessY);
// 建议常量：
// SoftnessX = 0;   // 左右仍贴边硬切（列表一般不横溢）
// SoftnessY = 32;  // 上下虚化带
```

> 若 Play 后发现左右也被 `Bar_BG` 内缘「硌」到，可将 SoftnessX 提到 **4～8**，仍以纵向为主。

---

## ⑥ 施工阶段（SF-0 ～ SF-3）

> 命名：SF = Soft Fade。施工员按阶段做；侦探本阶段 **不改代码**。

### SF-0 · 场景目检确认硬切（5 分钟）

| 做 | 说明 |
|----|------|
| 打开 `Village_Shop` | 选中 `Bar_ListScroll_Buy/Viewport` → `RectMask2D` |
| 看 Softness | 应为 **0 / 0**（与溯源一致） |
| Sell | 临时 Active 看一眼，Softness 同为 0 |

**验证**：Inspector 与本文 §3.2 一致即可进入 SF-1。

### SF-1 · Helper 统一写 Softness（程序 · 核心）

| 步骤 | 文件 | 改动 |
|------|------|------|
| SF-1-1 | `ShopScrollShellHelper.cs` | 增加常量 `DefaultMaskSoftnessX/Y`（如 0 / 32） |
| SF-1-2 | 同上 | 在 `ApplyInteractionFixes` 末尾调 `EnsureViewportSoftMask(scrollRoot)` |
| SF-1-3 | 同上 | `EnsureViewportSoftMask`：`Find("Viewport")` → 取/加 `RectMask2D` → 写 `softness` |
| SF-1-4 | 注释 | 说明：虚化 = alpha 渐隐，非高斯模糊；与 Fix-S1「无 Viewport Image」并存 |

**伪代码（给程序）**：

```csharp
// ShopScrollShellHelper — 示意，施工时按工程风格落盘并加详细注释
public const int DefaultMaskSoftnessX = 0;
public const int DefaultMaskSoftnessY = 32; // 可调：24～48

private static void EnsureViewportSoftMask(Transform scrollRoot)
{
    var viewport = scrollRoot.Find(ViewportName);
    if (viewport == null) return;

    var mask = viewport.GetComponent<RectMask2D>();
    if (mask == null) return; // 无 Mask 时不擅自加（Bake 负责建壳）

    // 强制写入默认软边，避免场景残留 0 或手改不一致
    mask.softness = new Vector2Int(DefaultMaskSoftnessX, DefaultMaskSoftnessY);
}
```

**替代方案（若不想每次强制覆盖策划手调）**：

| 策略 | 说明 |
|------|------|
| **强制写默认（推荐本阶段）** | Buy/Sell 观感统一；调参只改常量 |
| 仅当 softness==(0,0) 时写入 | 允许场景里 Fine-tune；但两边易漂移 |

### SF-2 · Bake / Setup 同步（程序 · 防回退）

| 步骤 | 文件 | 改动 |
|------|------|------|
| SF-2-1 | `ShopListBakeEditor.ConfigureViewport` | 挂好 RectMask2D 后 **调用** `ShopScrollShellHelper.ApplyInteractionFixes`（若末尾已调则确认 Softness 落在 Fixes 内即可） |
| SF-2-2 | `ShopBarListScrollSetupEditor.ConfigureViewport` | 同上，避免旧菜单建壳仍硬切 |
| SF-2-3 | 注释 | 「Viewport 软边由 Helper 统一，禁止在两处各写魔法数」 |

> 现有 Bake 流水线末尾若已 `ApplyInteractionFixes`，**SF-1 完成后 Bake 自动带 Softness**；本步重点是 **审计调用链**，避免漏网。

### SF-3 · 场景验收 + 微调（策划 / 程序）

1. Play → 购买页滚轮上下扫，盯顶/底半截行。  
2. 切 SELL → 同样滚一轮。  
3. 若偏硬：把 `DefaultMaskSoftnessY` **+8**；若顶底太虚、有效区显矮： **-8**。  
4. 满意后 Ctrl+S；可选再跑一次 Bake 确认 Softness 不被打回 0。

---

## ⑦ 验收清单

| ID | 操作 | 期望 |
|----|------|------|
| SF-V1 | 选中 Buy `Viewport` → RectMask2D | Softness **Y ≥ 24**（目标 32），X 按常量 |
| SF-V2 | 选中 Sell `Viewport` | **与 Buy 相同** Softness |
| SF-V3 | Play · 购买列表滚轮 | 顶/底行 **渐隐**，无齐刷刷刀口感 |
| SF-V4 | Play · 出售列表滚轮 | 同 V3 |
| SF-V5 | 半截行仍在框内渐隐区 | **不**清晰露出 `Bar_BG` 外 |
| SF-V6 | 滚轮仍可滚、数量框可点 | 射线未被破坏（根 Image alpha=0 仍命中） |
| SF-V7 | 再跑 Bake | Softness **仍为约定值**，Console 无新增 Error |
| SF-V8 | （回归）Tab Buy↔Sell | 显隐互斥正常；滚动位置各自保留 |

---

## ⑧ 踩坑与约束

### 8.1 Softness 会不会让列表「变矮」？

软边带内内容偏淡，**心理上**可视区略缩。不要用加大 Viewport 硬补；先调 SoftnessY。若必须补高度，属布局任务，**另开文档**，勿与虚化绑死。

### 8.2 与 Padding 的区别

| 字段 | 作用 |
|------|------|
| `padding` | 缩小 **硬** 裁剪矩形 |
| `softness` | 在裁剪边内侧做 **软** 过渡 |

本任务 **只动 softness**；不要叠加大 Padding，否则有效区双重缩小。

### 8.3 Maskable / 子 Graphic

`Shop_Bar` 下 Image / TMP 默认 `maskable=true`，RectMask2D Softness 才会作用到子节点。若某装饰图故意 `maskable=false`，它会 **穿出软边**——验收时若见「字淡了图标还硬切/穿模」，检查该 Graphic 的 Maskable。

### 8.4 不要恢复 Viewport Image

0706 已定：Viewport **无 Image**。软边 **不需要** 再给 Viewport 加半透明 Image；加回去可能干扰射线（历史坑）。

### 8.5 「虚化」≠ 高斯模糊

若策划验收时指着说要「糊成一片」，先确认是不是 **渐隐就够**。真模糊走方案 C，需单独评估性能与合层。

---

## ⑨ 待确认问题

| ID | 问题 | 影响 | 建议 |
|----|------|------|------|
| Q1 | SoftnessY 最终用 24 / **32** / 48？ | 观感强弱 | 施工默认 **32**，Play 一次定稿写回常量 |
| Q2 | SoftnessX 是否保持 0？ | 左右是否也要软 | 默认 **0**；仅当左右也有刀口再加 4～8 |
| Q3 | 是否强制覆盖场景手调 Softness？ | Helper 策略 | 本阶段 **强制写常量**；要 Fine-tune 自由再改「仅 0 时写入」 |
| Q4 | 方案 A 不够时是否立刻做方案 B？ | 排期 | **先验收 A**；不够再开渐变叠层任务 |

> 无结论时写入 `Assets/Doc/OPEN_QUESTIONS.md`，勿擅自上第三方 Soft Mask 或改滚动业务逻辑。

---

## ⑩ 给程序看的入口索引

| 主题 | 路径 |
|------|------|
| 运行时 / Bake 共用修正 | `Assets/Scripts/Game/GameRuntime/UI/FormLogic/Shop/ShopScrollShellHelper.cs` |
| Bake Viewport | `Assets/Scripts/Game/GameRuntime/UI/FormLogic/Shop/Editor/ShopListBakeEditor.cs` → `ConfigureViewport` |
| 旧 Setup 菜单 | `Assets/Scripts/Game/GameRuntime/UI/FormLogic/Shop/Editor/ShopBarListScrollSetupEditor.cs` |
| 测试场景 | `Assets/GameRes/Scenes/Village_Shop.unity` → `UI_Shop/Bar/Bar_ListScroll_Buy|Sell/Viewport` |
| Unity API | `UnityEngine.UI.RectMask2D.softness`（`Vector2Int`） |

---

## ⑪ 文档关系

| 文档 | 关系 |
|------|------|
| 0704 `Shop_Bar列表滚动…` | 确立硬裁剪壳；本文在其上加 Softness |
| 0704 双列表 Tab | Buy/Sell **两侧都要**改，不可只改 Buy |
| 0704 EB Bake | Softness 必须进 Helper/Bake，避免「手调一次、Bake 又硬」 |
| 0706 三小问题 | Fix-S1/T1 继续有效；本文不回退 Viewport Image |

---

## ⑫ 修订记录

| 日期 | 说明 |
|------|------|
| 2026-07-13 | 首版：溯源硬切来自 RectMask2D Softness=0；定稿方案 A；SF-0～SF-3 + 验收表 |
