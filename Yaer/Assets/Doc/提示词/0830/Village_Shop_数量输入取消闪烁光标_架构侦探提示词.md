# Cursor Agent Prompt · Village_Shop：取消数量输入框闪烁光标（Caret）

> **角色**：先【架构侦探】只读溯源，报告拍板后再【施工员】  
> **日期**：2026-08-30  
> **场景 / UI**：`Village_Shop` · `UI_Shop` / `ShopPanel` · 货单行数量输入（`TxtStock` / Number）  
> **产品目标（白话）**：商店改数量时，**不要再出现输入框里那根闪烁的竖线光标**（Unity TMP caret）；数字仍用现有图片 DigitStrip 显示，键盘输入能力保留  
> **证据**：玩家截图红箭头指向数量框内竖线 caret（非鼠标红指针）  
> **本阶段**：只读；禁止改代码 / Prefab / 场景  
> **报告落盘**：`Assets/Doc/执行文档/0830/Village_Shop_数量输入取消闪烁光标_架构溯源报告.md`

把下面「侦探」整段复制给 Cursor Agent（Agent Mode）执行。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 用户原话

> 「这个能不能取消了，就是商店的输入这个闪烁光标」  
> 图：数量输入区域里一根竖线在闪（TMP caret），要关掉/藏掉。

**不是**：鼠标指针、Head 悬停 Catch 光标、对话 Prefab（`Village_ShopStart` / `ShopNo` 无关）。

### 现网架构假说（须证伪）

| 层 | 路径 | 作用 |
|----|------|------|
| 行组件 | `ShopBuyRowQuantityInput` | 隐形 `TMP_InputField` + DigitStrip 图片数字 |
| 装配 | `ShopQuantityInputHelper.EnsureTmpIntegerInputField` | 建/绑 InputField、整数限制 |
| 隐身样式 | `ApplyInvisibleInputTextStyle` | 文本/placeholder alpha=0；**已设** `customCaretColor` + `caretColor` alpha=0 |
| Prefab | `ShopPanel.prefab` 各行 | 预扫已有 `m_CaretColor a:0`，但仍有 `m_CaretBlinkRate: 0.85`、`m_CaretWidth: 1` |

**缺口假说**：只把 caret **颜色透明度**设 0 **仍可能闪**（TMP 版本 / 选中态 / caret 网格仍在画）；需再查 `caretWidth`、`caretBlinkRate`、selectionColor、运行时是否覆盖、焦点态是否另创 caret 子物体。

### 方案倾向（侦探拍板，勿当唯一真相）

| 方案 | 做法 | 倾向 |
|------|------|------|
| **A · 彻底藏 caret** | `caretWidth=0` 和/或 `caretBlinkRate=0` + caret/selection alpha=0；集中写在 `ApplyInvisibleInputTextStyle`（Bake/运行时同源） | **✅ 推荐**（保留焦点与打字） |
| **B · Prefab 逐行改** | 只改 ShopPanel YAML | ❌ 易漏行；新 Bake 行又冒出来 |
| **C · 禁用 InputField 改用按钮加减** | 无 caret | ❌ 本期过大；产品只说取消闪烁 |
| **D · 失焦后立刻 DeactivateInputField** | 减少闪烁时长 | ⚠️ 可作辅；焦点输入时仍可能闪 |

**必须保留**：点框可输入数字、DigitStrip 同步、合计 Total2、characterLimit=2。

### 本期边界

| 做 | 不做 |
|----|------|
| ✅ 查清闪烁 caret 从哪来、为何 alpha=0 仍可见 | ❌ 改对话 Prefab / 成败对白 |
| ✅ 拍板最小关闪方案（Helper 一处生效） | ❌ 重做数量 UI（加减钮） |
| ✅ 买/卖两 Tab 所有行是否同源 | ❌ 改全局 TMP 默认 caret |
| ✅ 最小施工 + 验收清单 | ❌ 动 Head 鼠标光标系统 |

### 严禁（本阶段）

- 改代码 / Prefab / 场景  
- 为关 caret 拆掉 `TMP_InputField` 导致无法输入  
- 只改一两行 Prefab、不改 Helper，导致下次 Bake 回潮  
- 与 `Village_Shop_Head悬停光标` 混为一谈  

### 须对拍资产

| 资产 | 用途 |
|------|------|
| `ShopQuantityInputHelper.cs` | `ApplyInvisibleInputTextStyle` / Ensure |
| `ShopBuyRowQuantityInput.cs` | 运行时绑定与焦点 |
| `ShopPanel.prefab` | 现网 caret 序列化值 |
| `ShopListBakeEditor` / `ShopQuantityInputSetupEditor` | Bake 是否走同一 Helper |
| 用户截图 | 竖线 = caret，非红鼠标 |

---

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/02_SYSTEM_SPEC.md
@Assets/GameRes/Prefabs/UI/ShopPanel.prefab
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Shop/ShopQuantityInputHelper.cs
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Shop/ShopBuyRowQuantityInput.cs
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Shop/Editor/ShopListBakeEditor.cs
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Shop/Editor/ShopQuantityInputSetupEditor.cs

你现在是【架构侦探】。Unity 2020.3.48f1 / C# / TextMeshPro。
禁止修改任何代码、Prefab、场景。只读扫描 + 写「商店数量输入取消闪烁光标」溯源报告。

---

## 背景（策划白话）

1. 商店货单改购买/出售数量时，输入框里有一根**闪烁竖线**（输入光标），看着烦，要取消/隐藏。  
2. 数字本身继续用图片数字显示；玩家仍要能点框打数字。  
3. 本阶段只摸清：caret 谁画的、为何现有透明 caret 仍可能看见、最小改哪一处一劳永逸。

---

## 侦探任务清单

### A. 钉死「闪的是什么」
确认是 `TMP_InputField` caret（非鼠标、非 DigitStrip、非选区高亮误认）。  
指出层级：哪行 `TxtStock` / InputField 子节点。

### B. 钉死现网已做与仍漏
读 `ApplyInvisibleInputTextStyle` 与 `ShopPanel` 序列化：

| 属性 | 现网值（须填实） | 是否足以关闪 |
|------|------------------|--------------|
| customCaretColor / caretColor | ? | ? |
| caretWidth | ? | ? |
| caretBlinkRate | ? | ? |
| selectionColor / selectionStringColor | ? | ? |
| text / placeholder alpha | ? | — |

回答：**为什么 alpha=0 仍可能看见闪烁**（本机 TMP 行为 / 宽度仍 1 / 运行时未再 Apply / 选区等）。

### C. 接线与覆盖点
- `EnsureTmpIntegerInputField` / Bake / 运行时 `BindQuantityInput` 是否每次都走隐身样式？  
- Prefab 已 bake 的行，进 Play 后有没有被代码改回默认 caret？  
- 买 Tab / 卖 Tab 是否同一套？

### D. 方案拍板
推荐 A（Helper 内：`caretWidth=0` 和/或 `blinkRate=0` + 颜色全透明；必要时 selection 也透明）。  
说明是否需要 **重 Bake** 或仅运行时 Apply 即可覆盖旧 Prefab。  
列出否决项（拆 InputField、只改单行 YAML）。

### E. 最小施工清单（本阶段不执行）

| # | 动作 | 优先级 |
|---|------|--------|
| 1 | `ApplyInvisibleInputTextStyle`（或等价）关死 caret 可见性 | **P0** |
| 2 | 确保运行时 Bind/Ensure 必调用，旧 Prefab 无需手改也能生效 | **P0** |
| 3 | 买/卖所有数量行验收一致 | **P0** |
| 4 | 可选：ShopPanel 序列化同步，避免编辑器预览仍闪 | P1 |
| 5 | 注释写清：隐形 Input + 图片数字，caret 必须不可见 | P1 |

### F. 验收清单

| # | 操作 | 通过 |
|---|------|------|
| 1 | 进店 → 点任一数量框聚焦 | **无**闪烁竖线 caret |
| 2 | 键盘输入 1～99 | DigitStrip 数字更新；Total2 变；仍可决定购买 |
| 3 | 买/卖 Tab 各测一行 | 都不闪 |
| 4 | 失焦 / 再聚焦 | 仍不闪 |
| 5 | 不回归 | 点不到框、无法输入、数字变 TMP 字而非图片 → 失败 |

### G. 开放问题
- 是否允许保留极淡选区高亮，还是连 selection 一并全透明？  
- 无障碍/手柄是否依赖 caret（本项目若无则忽略）？  

---

## 输出要求

写入：`Assets/Doc/执行文档/0830/Village_Shop_数量输入取消闪烁光标_架构溯源报告.md`

MASTER 四段式：  
① 结论（关闪挂点 + 推荐方案）  
② 原因（通俗：为什么现在还闪）  
③ 用户检查清单（进店点数量框看什么）  
④ 给程序：属性表 + 最小 diff + 开放问题
```

---

## 施工员续跑（侦探报告拍板后贴）

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0830/Village_Shop_数量输入取消闪烁光标_架构溯源报告.md
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Shop/ShopQuantityInputHelper.cs
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Shop/ShopBuyRowQuantityInput.cs
@Assets/GameRes/Prefabs/UI/ShopPanel.prefab

你现在是【施工员】。按报告取消商店数量输入的闪烁 caret。

必须遵守：
- 保留 TMP_InputField 可聚焦输入与 DigitStrip 图片数字；
- 优先在 ShopQuantityInputHelper 隐身样式一处改全，避免只改单行 Prefab；
- 运行时须覆盖旧 Prefab，进 Play 即不闪；
- 代码含详细注释；重要取舍写清原因。

提交说明：改了哪些属性、如何验收、未做项。
```
