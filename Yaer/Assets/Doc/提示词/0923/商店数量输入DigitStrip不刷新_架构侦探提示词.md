# Cursor Agent Prompt · 商店数量输入后 DigitStrip 不变化（覆盖输入回归）

> **角色**：【架构侦探】只读；查「按数字键后可见数字不刷新」  
> **日期**：2026-09-23  
> **现象（用户多次验收失败）**：商店买卖行右侧数量，**输入之后数字不会变化**（DigitStrip / 合计看起来卡住）  
> **场景 / UI**：`Village_Shop` · `ShopBuyRowQuantityInput`（隐形 TMP + DigitStrip）  
> **背景**：今日先做「点击全选覆盖输入」（0923），再叠 0922 联合钳制；施工后出现「不刷新」，已两轮热修仍未过验  
> **不是**：改货单价；关联合购买力钳制当修法；改加减钮；恢复闪烁 caret 当主交付  

把下面「侦探」整段交给 Agent。没对拍「TMP.text 是否变 / DigitStrip 是否变 / onValueChanged 是否进」之前不要施工。

---

## 提示词助手预梳理（须证伪）

### 产品钉死（最终体验）

| 项 | 要求 |
|----|------|
| 可见数字 | 按键后 **DigitStrip 立刻变**（玩家看见的就是它） |
| 覆盖 | 点框后再按数字 → **整段覆盖**（5→3，不是 53） |
| 多位数 | 覆盖首键后应能继续敲第二位（如 3→34），除非被钳回上限 |
| 钳制 | **保留** 0922：买=`min((gold−其它行)/price, stackRoom)`；卖=持有；边输边钳 |
| caret | **无** 0830 闪烁 caret 回潮 |

### 施工史（侦探须对照现网文件）

| 轮次 | 做法 | 用户反馈 |
|------|------|----------|
| 1 | `onFocusSelectAll` + 延帧 `SelectAll` + 钳后再 SelectAll | **数字不变化** |
| 2 | 键入取消延帧 SelectAll；禁 `stringPosition=Length`；聚焦空串不钳 0 | **仍不变化** |
| 3（现网） | 去掉 SelectAll；`onValidateInput` 首键 `SetTextWithoutNotify`+手动刷图；`onFocusSelectAll=false`；PointerDown Relay 置覆盖旗 | **仍不变化**（用户要求开侦探） |

### 假说（须证伪排序）

1. **H1 · DigitStrip 同步断链**：TMP.text 已变，但 `SyncNumberDigitDisplay` / `_quantityNode` 指错 / `FindUnder` 失败 → 图不动  
2. **H2 · onValueChanged / Validate 未进或被吞**：Input 实际没吃到键（焦点、raycast、IntegerNumber、`\0` 拒插后未手动刷）  
3. **H3 · 钳制立刻写回旧值**：`ResolveBuyMaxQuantity` 恒 0 / 联合回扫 `ReclampAllBuyRows` 把文本打回 → 图「不变」  
4. **H4 · 空串/覆盖中间态 HideAllDigits**：曾对 `""` Sync → 藏光；后续 SetDigitString 未恢复（池/scale）  
5. **H5 · 监听双订或 `_isClampingQuantity` 卡死**：重入导致刷图早退  
6. **H6 · Prefab 预绑 quantityInput 与运行时 Ensure 双源**：样式/钩子挂错实例  

### 强制排除

| 勿当主因 | 理由 |
|----------|------|
| 「用户不会用」 | 多轮验收 |
| 直接关 0922 联合钳制 | 产品仍要上限；最多证伪是否误伤刷新 |
| 只改 Total2 | 行内 DigitStrip 才是主诉 |

---

## 侦探（复制给 Agent）

```
你是【架构侦探】。只读分析，不改代码、不改场景、不提 MR。

任务：查清商店数量「输入后可见数字不变化」的根因；区分
（A）TMP 文本没变、（B）文本变了 DigitStrip 没刷、（C）刷了又被钳回旧值。
给出最小修法，且必须保留 0922 联合购买/持有钳制与「点后覆盖输入」产品目标。

### 必读

@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Shop/ShopBuyRowQuantityInput.cs
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Shop/ShopQuantityInputHelper.cs
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Shop/ShopFormLogic.cs
@Assets/Scripts/Game/GameRuntime/UI/Component/UiSpriteNumberDisplay.cs
@Assets/Doc/施工说明/0923/商店数量点击全选覆盖输入_施工说明.md
@Assets/Doc/施工说明/0922/商店买卖数量输入自动钳上限_施工说明.md
@Assets/Doc/执行文档/0923/商店数量点击全选覆盖输入_架构溯源报告.md

Grep：`OnValidateQuantityInput` / `SyncNumberDigitDisplay` / `ApplyValueChangedPipeline` /
`ReclampAllBuyRowsForJointGold` / `SetTextWithoutNotify` / `onValidateInput` /
`ShopQuantityOverwritePointerRelay` / `_overwriteOnNextDigit` / `HideAllDigits`。

### A. Play 复现矩阵（必须填，禁止只推演）

进店后任选一行（买/卖各测）：

| # | 操作 | TMP.text（Console/断点） | DigitStrip 可见 | Total2 | 备注 |
|---|------|--------------------------|-----------------|--------|------|
| 1 | 默认 0，点击，按 2 | ？ | ？ | ？ | |
| 2 | 先设法显示 5，点击，按 3 | ？应变 3？ | ？ | ？ | 覆盖 |
| 3 | 在 3 上再按 4（不先失焦） | ？34 或仍 3？ | ？ | ？ | 多位 |
| 4 | 买：金币只够 1，输入 9 | ？钳成 1？ | ？ | ？ | 0922 |
| 5 | 卖：持有 6，输入 7 | ？钳成 6？ | ？ | ？ | 0922 |

额外钉死：

- 按键时 `OnValidateQuantityInput` / `OnQuantityValueChangedInternal` / `ApplyValueChangedPipeline` 是否进入？  
- `_quantityNode` 名（TxtStock / Number）？其下是否有 `UiSpriteNumberDisplay`？  
- `quantityInput` 是否序列化预绑？与 Ensure 是否同一实例？  
- 覆盖路径：`SetTextWithoutNotify` 后是否调用了 `ApplyValueChangedPipeline`？返回 `\0` 后 TMP 是否又改写 text？

### B. 对拍调用链

现网覆盖路径（施工声称）：

```
PointerDown/onSelect → _overwriteOnNextDigit=true
→ onValidateInput(digit)
→ SetTextWithoutNotify(digit) + ApplyValueChangedPipeline + return '\0'
→ TryClamp → SyncNumberDigitDisplay → OnQuantityValueChanged
→（买）ReclampAllBuyRowsForJointGold → RefreshTotal2
```

普通追加路径：

```
onValueChanged → ApplyValueChangedPipeline → Sync / Clamp
```

必答：

1. 「不变化」属于 A/B/C 哪一类（或组合）？证据？  
2. 若 B：Sync 失败点在 FindUnder / SetDigitString / 空串 HideAll / 节点指错？  
3. 若 C：哪次写回（本行 Clamp / 联合回扫 / EndEdit）？max 公式是否误成 0？  
4. 若 A：焦点、raycast、Validate 拒插、旁路钩子谁吃了键？  
5. 最小修挂点文件？0922 `ResolveBuyMaxQuantity` 是否允许动（默认否，除非证伪为根因）？

### C. 方案对比

| 方案 | 做法 | 何时选 |
|------|------|--------|
| **A** | 修 Sync 链（节点/Find/空串策略）保证 text→DigitStrip | B 类 |
| **B** | 修 Validate/焦点，保证键入进 text | A 类 |
| **C** | 钳制写回后强制 RefreshDigitDisplay；回扫跳过正在编辑行 | C 类 |
| **D** | 回退今日覆盖交互，只保留 0922 钳制 | 仅当覆盖实现不可救且产品同意暂缓覆盖 |
| **E** | 关联合钳制 | **否决** |

推荐须写清：主方案 + 是否保留「点后覆盖」。

### D. 验收表（报告内）

| # | 步骤 | 期望 |
|---|------|------|
| 1 | 点数量按 2 | DigitStrip=2，TMP=2 |
| 2 | 再点按 5 | DigitStrip=5（覆盖） |
| 3 | 不点再按 6 | DigitStrip=56 或被钳到 max |
| 4 | 买超购买力 | 立刻变最大可买，图跟着变 |
| 5 | 卖超持有 | 变持有 |
| 6 | 无闪烁 caret | 同 0830 |

### E. 报告

写入：`Assets/Doc/执行文档/0923/商店数量输入DigitStrip不刷新_架构溯源报告.md`

结构：①结论 ②复现矩阵（含 TMP vs DigitStrip）③调用链与证伪 ④方案 ⑤文件路径 ⑥验收 ⑦风险
```

---

## 施工员（侦探闭环后再复制）

```
@Assets/Doc/执行文档/0923/商店数量输入DigitStrip不刷新_架构溯源报告.md
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Shop/ShopBuyRowQuantityInput.cs
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Shop/ShopQuantityInputHelper.cs
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Shop/ShopFormLogic.cs
@Assets/Scripts/Game/GameRuntime/UI/Component/UiSpriteNumberDisplay.cs

你是【施工员】。按报告最小修：输入后 DigitStrip 必须变；保留点后覆盖与 0922 联合钳制。

约束：
- 禁止关联合钳制交差
- 禁止只改 Warning
- 0830 无闪烁 caret 不回潮
- 写入：`Assets/Doc/施工说明/0923/商店数量输入DigitStrip不刷新_施工说明.md`
```

---

## 使用顺序

1. 复制「侦探」→ Play 填矩阵（TMP 与 DigitStrip 分列）  
2. 确认 A/B/C 类根因与方案  
3. 复制「施工员」→ 按验收表打买/卖两 Tab
