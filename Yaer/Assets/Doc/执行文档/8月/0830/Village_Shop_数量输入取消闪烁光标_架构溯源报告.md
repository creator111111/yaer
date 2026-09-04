# Village_Shop — 数量输入取消闪烁光标（Caret）— 架构溯源报告

**文档版本**：v1.0（2026-08-30）  
**文档性质**：【架构侦探】只读溯源 + 关闪拍板（**本阶段未改代码 / Prefab / 场景**）  
**Unity**：2020.3.48f1 · TextMeshPro  
**场景 / UI**：`Village_Shop` · `UI_Shop` / `ShopPanel` · 货单行数量（`TxtStock` / `Number`）  
**产品目标**：取消数量框内**闪烁竖线**（TMP caret）；保留键盘输入 + DigitStrip 图片数字  
**证据**：玩家截图红箭头指向框内竖线 caret（**非**鼠标 Catch 指针、非对话 Prefab）

关联：`ShopQuantityInputHelper.ApplyInvisibleInputTextStyle` · `ShopBuyRowQuantityInput` · Bake Editor · `ShopPanel` / 场景序列化 caret 字段

---

## ① 结论一句话

**闪的是 `TMP_InputField` 自带 caret（挂在行内 `TxtStock`/`Number` 上），不是鼠标、不是 DigitStrip。现网已把 `caretColor` alpha 设 0，但 `caretWidth=1` + `caretBlinkRate=0.85` 仍在画/闪网格，透明色挡不住；推荐方案 A：在 `ApplyInvisibleInputTextStyle` 一处补 `caretWidth=0`、`caretBlinkRate=0`（caret/selection 全透明），Ensure/Bake 同源覆盖，运行时无需手改旧 Prefab 也能关闪。**

---

## ② 原因（通俗）

商店数量框是「隐形文字输入 + 上面盖一层图片数字」。  
输入框聚焦时 Unity/TMP 还会画一根**闪烁竖线**告诉你「正在打字」。

工程里已经把这根线的**颜色透明度调成 0**，但线的**宽度还是 1、还在按 0.85 闪**——等于「看不见的墨水」仍在那儿描边闪，叠在图片数字上就还能看出一根竖线。

关掉宽度/闪烁（或两样一起关）就能消掉，不必拆掉输入框、也不必改成加减按钮。

---

## ③ 用户检查清单（施工后）

| # | 操作 | 通过 |
|---|------|------|
| 1 | 进店 → 点任一数量框聚焦 | **无**闪烁竖线 caret |
| 2 | 键盘输入 1～99 | DigitStrip 数字更新；Total2 变；仍可点「决定」购买 |
| 3 | 买 Tab / 卖 Tab 各测一行 | 都不闪 |
| 4 | 失焦再聚焦 | 仍不闪 |
| 5 | 不回归 | 点不到框 / 无法输入 / 数字变成 TMP 字而非图片 → **失败** |
| 6 | （可选）编辑器未 Play 点 Prefab 行 | 若做了 P1 序列化同步则预览也不闪；仅改 Helper 时以 Play 为准 |

**不是本期**：Head 悬停 Catch 鼠标光标、`Village_ShopStart`/`ShopYes`/`ShopNo` 对话。

---

## ④ 给程序

### A. 「闪的是什么」（钉死）

| 项 | 结论 |
|----|------|
| 对象 | `TMP_InputField` **caret**（输入竖线） |
| 非 | 系统/游戏鼠标指针、DigitStrip Sprite、对话 Head Catch |
| 层级 | `Shop_Bar`（行）→ `TxtStock` 或 `Number` → 同节点 `TMP_InputField`；可见数字为同节点下 DigitStrip |
| 行组件 | `ShopBuyRowQuantityInput`（买/卖共用） |

### B. 现网属性表（磁盘真源）

`ApplyInvisibleInputTextStyle` 现网只做：

```csharp
textComponent.color.a = 0;
placeholder.color.a = 0;
customCaretColor = true;
caretColor = (1,1,1,0);  // 仅颜色透明
// ❌ 未写 caretWidth / caretBlinkRate / selectionColor
```

| 属性 | Helper 现网 | `ShopPanel.prefab`（11 处） | `Village_Shop.unity`（11 处） | 是否足以关闪 |
|------|-------------|-----------------------------|-------------------------------|--------------|
| `customCaretColor` | `true` | `1` | `1` | 仅配合颜色 |
| `caretColor` | a=0 | a=0 | a=0 | ❌ **不够** |
| `caretWidth` | **未改**（默认/盘上 1） | **1** | **1** | ❌ 仍画网格 |
| `caretBlinkRate` | **未改**（默认/盘上 0.85） | **0.85** | **0.85** | ❌ 仍闪 |
| `selectionColor` | **未改** | 蓝 a≈0.75 | （同系） | 焦点全选时有蓝块；用户主诉是竖线，次因 |
| text / placeholder alpha | 0 | Bake 后 0 | 0 | ✅ 字隐形（数字靠 DigitStrip） |
| `m_OnFocusSelectAll` | — | `1` | — | 聚焦全选 → selection 更明显 |

**为什么 alpha=0 仍可能看见闪烁**

1. **主因**：只透明颜色，**宽度 1 + 闪烁率 0.85** 仍生成/开关 caret 网格；叠在 DigitStrip 上常见 1px 竖线残影（抗锯齿/叠图）。  
2. **次因**：`selectionColor` 仍半透明蓝 + `OnFocusSelectAll`，聚焦瞬间可能像「闪一下」（形态是块不是竖线）。  
3. **非主因（现网）**：运行时并未把 caret 改回不透明——`EnsureTmpIntegerInputField` 仍会 `ApplyInvisible` 再刷一遍 alpha=0。

### C. 接线与覆盖点

```
Bake / Setup Editor
  → ShopQuantityInputHelper.EnsureTmpIntegerInputField
       → ApplyIntegerInputSettings
       → ApplyInvisibleInputTextStyle   ← 关闪应扩在这里
       → EnsureNumberDigitStrip

运行时 ShopBuyRowQuantityInput.BindQuantityInput
  → quantityInput==null 时 Find TxtStock|Number → Ensure…（同上）
  → 已非 null 则直接 return（不再 Ensure）
```

| 点 | 核实 |
|----|------|
| Bake | `ShopListBakeEditor` / `ShopQuantityInputSetupEditor` 均调 `EnsureTmpIntegerInputField` ✅ 同源 |
| 买/卖 | 同一 `ShopBuyRowQuantityInput` + Helper ✅ |
| Prefab `quantityInput` | 现网多为 `{fileID: 0}` → 进 Play 会走 Ensure ✅ |
| 早退风险 | 若将来 Prefab **预绑** `quantityInput`，Bind 早退 → **不再** Apply 隐身样式。施工建议：Bind 在已有引用时也调用一次公开的 `ApplyInvisible…`，或 Ensure 幂等强制刷样式 |
| 运行时会不会改回默认 caret | ❌ 无代码把 width/blink 改回；现网是「从未关死」 |

**结论**：只改 Helper 一处 + 保证运行时必 Apply → **不必手改 11 行 YAML 也能进 Play 关闪**；P1 可选同步 Prefab/场景序列化，方便编辑器预览一致。

### D. 方案拍板

| 方案 | 裁定 |
|------|------|
| **A · Helper 彻底藏 caret** | ✅ **推荐**：`caretWidth=0` + `caretBlinkRate=0` + caret alpha=0；**selectionColor alpha=0**（避免全选蓝块） |
| B · 只改 ShopPanel 逐行 YAML | ❌ 易漏；下次 Bake 用旧 Helper 又冒出来 |
| C · 拆 InputField 改加减钮 | ❌ 本期过大；产品只要关闪 |
| D · 失焦立刻 Deactivate | ⚠️ 可作辅；聚焦输入时仍可能闪，不单独作为主方案 |

**重 Bake？**  
- **P0 不强制**：运行时 Ensure/Apply 覆盖磁盘旧值即可。  
- **P1**：改完 Helper 后跑一次 Bake 或 Setup，把 Prefab/场景序列化写成 0，编辑器未 Play 也不闪。

### E. 最小施工清单（本阶段不执行）

| # | 动作 | 优先级 |
|---|------|--------|
| 1 | `ApplyInvisibleInputTextStyle`：`caretWidth=0`、`caretBlinkRate=0`、caret/selection 全透明；注释写清「隐形 Input + 图片数字，caret 必须不可见」 | **P0** |
| 2 | 保证运行时必刷：`BindQuantityInput` 在已有 `quantityInput` 时也 Apply 一次（或 Ensure 始终刷样式），旧 Prefab/场景无需手改 | **P0** |
| 3 | 买/卖所有数量行 Play 验收不闪、可输入、DigitStrip/Total2 正常 | **P0** |
| 4 | 可选：ShopPanel + Village_Shop 场景序列化同步 width/blink/selection | P1 |
| 5 | 全局 TMP 默认 / Head 鼠标光标 / 对话 Prefab | ❌ 不动 |

**预期 diff**

- `ShopQuantityInputHelper.cs`（主）  
- 可选 `ShopBuyRowQuantityInput.cs`（已有引用也 Apply）  
- 可选 `ShopPanel.prefab` / `Village_Shop.unity`（P1）

### F. 验收清单

同 §③。

### G. 开放问题

| ID | 问题 | 倾向 | 状态 |
|----|------|------|------|
| Q1 | 是否保留极淡选区高亮？ | **否**：与隐形输入一致，selection 也全透明 | ✅ 本报告拍板 |
| Q2 | 无障碍/手柄是否依赖 caret？ | 本项目商店无此需求 → **忽略** | ✅ |
| Q3 | Prefab 预绑 quantityInput 早退？ | 施工时一并堵上（已有引用也 Apply） | ✅ 倾向做 |

（已追加 `OPEN_QUESTIONS.md`。）
