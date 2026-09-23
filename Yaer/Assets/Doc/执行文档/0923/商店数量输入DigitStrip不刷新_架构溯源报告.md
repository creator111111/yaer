# 商店数量输入 DigitStrip 不刷新 · 架构溯源报告

> 日期：2026-09-23  
> 角色：架构侦探（**只读**，未改代码 / 场景 / Prefab）  
> Unity：2020.3.48f1 · TextMeshPro  
> 现象：买卖行数量**按键后可见 DigitStrip 不变化**（多轮热修仍失败）  
> 背景：0923「点后覆盖」叠 0922 联合钳制；现网为第 3 轮（`onValidateInput` + `SetTextWithoutNotify` + `\0`）  
> 提示词：`Assets/Doc/提示词/0923/商店数量输入DigitStrip不刷新_架构侦探提示词.md`

---

## ① 结论一句话

**「不变化」在现网第 3 轮上，代码上最像 B 类（刷图链/覆盖写入路径坏了）与 C 类（钳回后看起来像没变）的组合；必须以 Play 分列 TMP.text vs DigitStrip 一锤定音。**  

覆盖实现从「SelectAll」改到「Validate 内 `SetTextWithoutNotify` + 返回 `\0`」后，**绕开了原先能刷图的 `onValueChanged` 主路**，改走手搓 `ApplyValueChangedPipeline`；该路径对 TMP 在 `onValidateInput` 中途改 `text` 的兼容性差，且 Sync 存在**静默失败**（`_quantityNode` / `FindUnder` null 直接 return）。  

另：默认数量 0 + `max==0`（无购买力/无持有）时钳回 0，DigitStrip 一直为 0，体感也是「不变化」——须与真·刷图失败分表。

**产品目标保留**：点后覆盖 + 0922 联合钳制 + 无闪烁 caret。  
**推荐**：改覆盖实现为「走正常插入 / onValueChanged，再截成覆盖结果」（复活已知能 Sync 的路径）+ 一律 `RefreshDigitDisplay`；**禁止**关联合钳制交差。

---

## ② 复现矩阵（Play 必填；下列为代码推演占位）

> 侦探本阶段**未能进 Unity Play**；施工前须用 Console/断点填实列。建议临时 Log：`Validate` / `Pipeline` / `TMP.text` / `FindUnder!=null`。

| # | 操作 | TMP.text | DigitStrip 可见 | Total2 | 推演备注 |
|---|------|----------|-----------------|--------|----------|
| 1 | 默认 0，点击，按 2 | ？应变 2 | ？ | ？ | 若 TMP=2 图=0 → **B**；皆 0 → **A 或 C(max0)** |
| 2 | 先显示 5，点击，按 3 | ？应变 3 | ？ | ？ | 覆盖主诉 |
| 3 | 在 3 上再按 4（不失焦） | ？34 | ？ | ？ | 多位；覆盖旗应已清 |
| 4 | 买：钱只够 1，输入 9 | ？钳 1 | ？跟 1 | ？ | 0922；图须跟钳 |
| 5 | 卖：持有 6，输入 7 | ？钳 6 | ？跟 6 | ？ | 0922 |

**额外钉死（Play）**

| 检查 | 用途 |
|------|------|
| 是否进入 `OnValidateQuantityInput` / `ApplyValueChangedPipeline` | 分 A vs B |
| `_quantityNode` 名；`FindUnder` 是否非 null | 分 B |
| `quantityInput` 是否预绑；与 DigitStrip 是否同 Number 节点 | H6 |
| 覆盖后 `SetTextWithoutNotify` 再读 `quantityInput.text` | 是否被 TMP 回滚 |
| 该行 `_resolveMaxQuantity()` 返回值 | 分 C |

---

## ③ 调用链与证伪

### 现网第 3 轮覆盖路径

```
PointerDown Relay / onSelect → _overwriteOnNextDigit=true
→ onValidateInput(digit)
   → SetTextWithoutNotify(digit)     // 不触发 onValueChanged
   → ApplyValueChangedPipeline(text)
        → (空串且聚焦 → early return，不刷图)
        → TryClamp → RefreshDigitDisplay 或 SyncNumberDigitDisplay
        → OnQuantityValueChanged →（买）ReclampAllBuyRows → RefreshTotal2
   → return '\0'                     // 拒掉本次插入
```

```205:228:Assets/Scripts/Game/GameRuntime/UI/FormLogic/Shop/ShopBuyRowQuantityInput.cs
        private char OnValidateQuantityInput(...)
        {
            // ...
            quantityInput.SetTextWithoutNotify(addedChar.ToString());
            ApplyValueChangedPipeline(quantityInput.text);
            return '\0';
        }
```

### 普通追加（历史上 DigitStrip 能跟着变的路径）

```
按键 → TMP 插入 → onValueChanged → ApplyValueChangedPipeline → Sync
```

0923 覆盖前用户能打出「53」类累加，说明 **当时 Sync 链大体可用**；损坏点高度集中在**覆盖专用写入**。

### 假说裁定（代码强度）

| ID | 假说 | 裁定 |
|----|------|------|
| **H2/覆盖写** | Validate 内改 text + `\0` 导致 TMP 状态/回调异常，Pipeline 刷了又回滚或未真写入 | **主嫌疑（→ A/B）** |
| **H1** | `SyncNumberDigitDisplay`：`numberNode==null` / `FindUnder==null` **静默 return**；Pipeline 非钳分支不走 `RefreshDigitDisplay` 重找节点 | **主嫌疑（→ B）** |
| **H3** | `max==0` 或联合回扫把值钳回原值（常为 0）→ 图「不变」 | **须 Play 分表（→ C）**；公式默认勿改 |
| **H4** | 空串 Sync → `HideAllDigits`；现网聚焦空串 early return 是为防此，但也可能让中间态跳过刷图 | 次；与清空式覆盖冲突史一致 |
| **H5** | `_isClampingQuantity` 卡死 | 弱：finally 有复位 |
| **H6** | 预绑 vs Ensure 双源 | 弱：Bind 两路都挂 Validate/Relay |

### 必答

1. **类属**：优先按 Play 判；代码默认按 **B（覆盖路径刷图失败）为主，C 为易混淆伴生**。  
2. **若 B**：先查 `FindUnder` / `_quantityNode`；再查 Validate 内 `SetText`+`\0` 后 text 是否回滚。  
3. **若 C**：查该行 max；回扫 `ApplyBusinessMaxClampSilent` 是否把编辑行打回；修感应是「钳后强制 RefreshDigitDisplay」（已有）+ 可选跳过正在编辑行的误伤，**不是关联合钳制**。  
4. **若 A**：焦点、`onValidateInput` 是否被 ContentType 冲掉、Raycast。  
5. **最小挂点**：`ShopBuyRowQuantityInput.cs`（覆盖实现）；Helper Sync/Refresh 加固。**默认不动** `ResolveBuyMaxQuantity` 公式。

---

## ④ 方案对比

| 方案 | 做法 | 何时选 | 裁决 |
|------|------|--------|------|
| **A** | 加固 Sync：一律 `RefreshDigitDisplay`；Find 失败打 Error；禁静默 | B 坐实 | **必做加固** |
| **B** | **改覆盖实现**：允许正常插入走 `onValueChanged`，若 `_overwriteOnNextDigit` 则把 text **收成末位/整段替换**再 Sync（或 EndOfFrame 设字 + ForceLabelUpdate）；**去掉** Validate 内 `SetTextWithoutNotify`+`\0` | 覆盖路径为根 | **主推**（保覆盖产品） |
| **C** | 钳制/回扫后强制刷图；聚焦行回扫策略收紧 | C 坐实 | **与 A/B 同批可做** |
| **D** | 回退覆盖，只留 0922 | 产品同意暂缓覆盖 | 备 |
| **E** | 关联合钳制 | — | **否决** |

### 推荐组合

**B + A（+ 按需 C）**：用「onValueChanged 覆盖收束」复活刷图主路；Sync 不再静默；钳制保留。  
例（施工方向，非本阶段改码）：

```
onValueChanged(text):
  if (_overwriteOnNextDigit && text.Length>=1):
      _overwriteOnNextDigit=false
      keep = lastDigit(text)   // 或首键字符
      SetTextWithoutNotify(keep)
  ApplyValueChangedPipeline → 总是 RefreshDigitDisplay()
```

勿再依赖延帧 SelectAll（第 1 轮已证伤刷图）。

---

## ⑤ 要改文件（路径级；本阶段未改）

| 全路径 | 预期 |
|--------|------|
| `…/ShopBuyRowQuantityInput.cs` | 重做覆盖（方案 B）；Pipeline 末尾一律 RefreshDigitDisplay |
| `…/ShopQuantityInputHelper.cs` | Sync 失败打日志；可选 Ensure DigitStrip |
| `…/ShopFormLogic.cs` | **默认不改公式**；仅当 C 证伪回扫误伤再动回扫策略 |
| `UiSpriteNumberDisplay.cs` | 一般不动；除非证伪池/HideAll |

施工说明：`Assets/Doc/施工说明/0923/商店数量输入DigitStrip不刷新_施工说明.md`

---

## ⑥ 验收表

| # | 步骤 | 期望 |
|---|------|------|
| 1 | 点数量按 2 | DigitStrip=**2**，TMP=**2** |
| 2 | 再点按 5 | DigitStrip=**5**（覆盖） |
| 3 | 不点再按 6 | DigitStrip=**56** 或被钳到 max |
| 4 | 买超购买力 | 立刻变最大可买，**图跟着变** |
| 5 | 卖超持有 | 变持有，图跟着变 |
| 6 | 无闪烁 caret | 同 0830 |
| 7 | Total2 | 随数量变（非唯一主诉，须跟） |

---

## ⑦ 风险与回滚

| 风险 | 处置 |
|------|------|
| 新覆盖又打断多位数 | 仅首键消费 `_overwriteOnNextDigit`，其后追加 |
| 空串中间态藏光 | 避免先清空再插入；或空串不 Hide、只跳过 |
| 误关 0922 | Code review 禁止动联合 max 语义 |
| 只加 Log 交差 | 禁止；须 DigitStrip 真变 |

**回滚**：恢复覆盖前「能累加但会刷图」的 onValueChanged Sync；再按方案 B 重做覆盖。
