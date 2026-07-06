# Village_Shop · Play 闪退（无 Console 报错）— 架构溯源与施工执行说明

**文档版本**：v1（2026-07-06）  
**文档性质**：架构侦探 + 验收员定位  
**触发**：`Village_Shop` 场景点击 **Play** 后 Unity **直接闪退**，Console **无 C# 报错；发生在 **Shop 数字图片化（IMG）+ IMG-R1 前导零修正** 之后。

**依据**：
- `Assets/Doc/00_MASTER_PROMPT.md`【架构侦探 / 验收员】
- `Assets/Doc/执行文档/0706/Shop_Bar数字图片化_Price_Number_Total_架构溯源与施工执行说明.md`（IMG 施工）
- 关联场景：`Assets/GameRes/Scenes/Village_Shop.unity`
- 关联脚本：`UiSpriteNumberDisplay.cs`、`ShopFormLogic.cs`、`ShopBarRowView.cs`、`ShopBuyRowQuantityInput.cs`

**Unity 版本**：2020.3.48f1  

---

## ① 结论（一句话）

**闪退主因不是场景 Hierarchy 坏了，而是 `UiSpriteNumberDisplay` 在 IMG-R1 改动中引入了 `EnsureInitialized` ↔ `HideAllDigits` 无限递归，Awake 时栈溢出导致进程崩溃；场景里 `商店界面合层` 仅为世界空间底图，与闪退无关。**

---

## ② 玩家在做什么 / 会遇到什么现象

| # | 操作 | 现象 | 生活类比 |
|---|------|------|----------|
| 1 | 打开 `Village_Shop` → Play | Unity **整窗消失**，无红字报错 | 程序一开机就断电，来不及亮故障灯 |
| 2 | 看 Hierarchy | 根节点有 `Main Camera`、`商店界面合层`、`UI_Shop`、`EventSystem` | 看起来像两套东西，容易怀疑场景配错 |
| 3 | 改 IMG 代码前 | 同场景可能能 Play | 最近那次「修前导零 / Digit 池」改坏了 |

---

## ③ 场景结构溯源（Village_Shop）

### 3.1 根节点快照（2026-07-06 · 场景文件）

| 根节点 | 类型 | 与闪退关系 |
|--------|------|------------|
| `Main Camera` | Camera | 无关 |
| **`商店界面合层`** | 空父节点 + 子节点 **SpriteRenderer** 世界坐标底图 | **无关**（非 UI Canvas，无 `ShopFormLogic`） |
| **`UI_Shop`** | Canvas + `ShopFormLogic` | **触发路径**：其下 Bake 出的 `Shop_Bar_*` 行挂大量 `UiSpriteNumberDisplay` |
| `EventSystem` | EventSystem | 无关 |

**澄清**：Hierarchy 里同时出现 `商店界面合层` 与 `UI_Shop` **不是重复商店 UI**，而是「场景手绘底图 + 屏幕 UI」分层，**不构成双份逻辑**。

### 3.2 UI_Shop 关键绑定（场景序列化）

| 项 | 状态 | 说明 |
|----|------|------|
| `ShopFormLogic` | 挂在 `UI_Shop` 根 | `buyContent` / `sellContent` 已绑定 |
| `total2Digits` | **未绑定（null）** | Play 时 `ResolveTotal2DigitsReference` 会 **运行时 `EnsureOn` 创建** Total2 DigitStrip |
| `Shop_Bar_*` 实例 | Buy + Sell 共 **11 行**（Prefab `fbb1826d…`） | 每行 **Price + Number** 各 1 个 `DigitStrip` → 约 **22+** 个 `UiSpriteNumberDisplay` |
| `Total2_Digits` | 场景内 **1 处** `UiSpriteNumberDisplay` | 与行内 DigitStrip 一起走同一套 Awake |

### 3.3 场景是否有「配错」？

| 检查项 | 结果 |
|--------|------|
| 双份 `ShopFormLogic` | **无**（仅 `UI_Shop` 上一处） |
| 双份 Canvas 抢 UI | **无**（商店交互 Canvas 仅 `UI_Shop`） |
| `商店界面合层` 重复逻辑 | **无**（纯 Sprite 装饰） |
| Bake 行数异常 | 11 行，正常 |
| **代码递归** | **有（CRASH-1）** ← 主因 |

---

## ④ 崩溃链路溯源（CRASH-1）

### 4.1 调用栈（逻辑）

```
Play
  → Shop_Bar 各行 ShopBarRowView.Awake
       → ApplyPrice → UiSpriteNumberDisplay.SetNumber
  → Number 列 ShopBuyRowQuantityInput.Awake
       → EnsureTmpIntegerInputField → EnsureNumberDigitStrip → EnsureOn → EnsureInitialized
  → 各 DigitStrip UiSpriteNumberDisplay.Awake
       → EnsureInitialized
  → ShopFormLogic.Awake
       → ResolveTotal2DigitsReference → EnsureOn → EnsureInitialized

EnsureInitialized()  【_initialized 仍为 false】
  … Prune / Collect / EnsurePoolSize …
  → HideAllDigits()
       → EnsureInitialized()   ← 又进入，_initialized 仍未 true
            → HideAllDigits()
                 → EnsureInitialized()
                      → … 无限递归 …
  → 栈溢出 StackOverflowException
  → Unity 进程崩溃（Console 往往来不及打印）
```

### 4.2 引入时间点

| 版本 | 改动 | 后果 |
|------|------|------|
| IMG 首版 | `EnsureInitialized` 末尾 `HideAllDigits()` 隐藏未刷图占位 | 正常 |
| **IMG-R1**（修 `0200` 前导零） | `HideAllDigits()` 开头增加 `EnsureInitialized()` | **与上行形成环，必崩** |

### 4.3 为何 Console 没报错？

- **栈溢出**在原生层 / 极深递归中发生，Unity Editor 常表现为 **整进程退出**，不一定留下 C# 红字。
- 可在 `%LOCALAPPDATA%\Unity\Editor\Editor.log` 末尾搜 `StackOverflowException` 或 `UiSpriteNumberDisplay` 验证。

### 4.4 与「前导零 0200」的关系

- **前导零**是 **显示逻辑 / 池子残留** 问题（IMG-R1 另一条线）。
- **闪退**是 **同一次 IMG-R1 补丁里误加的递归**，两个问题同源不同症状；**先修 CRASH-1 才能 Play 验收前导零**。

---

## ⑤ 修复方案定稿

### Fix-CRASH-1 · 打断递归（程序 · 必做）

| 步骤 | 文件 | 改动 |
|------|------|------|
| C1-1 | `UiSpriteNumberDisplay.cs` | **`HideAllDigits()` 内删除 `EnsureInitialized()` 调用** |
| C1-2 | 同上 | `HideAllDigits` 仅在 `_initialized == true` 时遍历 `_digitImages`；初始化阶段由 `EnsureInitialized` 在池建完后 **直接调用** |
| C1-3 | 全工程 | 确认无其他地方在 `EnsureInitialized` 完成前间接再入 `EnsureInitialized` |

**正确分工**：

```csharp
// EnsureInitialized：建池 → 末尾 HideAllDigits() → _initialized = true
// HideAllDigits：只隐藏，不再 EnsureInitialized
// SetDigitString / SetNumber：先 EnsureInitialized()，再业务逻辑
```

**替代方案（不推荐）**：在 `HideAllDigits` 前先把 `_initialized = true` —— 中间状态半初始化，后续难维护。

### Fix-CRASH-2 · 场景绑定（可选 · 非闪退必做）

| 步骤 | 操作 |
|------|------|
| C2-1 | Bake 后把 `UI_Shop/ShopFormLogic.total2Digits` 拖上 `Total2/Total2_Digits` |
| C2-2 | 避免 Play 时再 `EnsureOn` 动态创建（减少首帧开销，**不解决闪退**） |

---

## ⑥ 施工阶段

| 阶段 | 内容 | 验证 |
|------|------|------|
| **CRASH-0** | 读 `Editor.log` 确认 StackOverflow（可选） | 日志含 `UiSpriteNumberDisplay` 深栈 |
| **CRASH-1** | 应用 Fix-CRASH-1 补丁 | 编译无 Error |
| **CRASH-2** | Play `Village_Shop` | **不闪退**；Console 无 Error |
| **CRASH-3** | 继续 IMG-V9 / V10 | Price 200 非 0200；间距 OK |

---

## ⑦ 验收清单

| ID | 操作 | 期望 |
|----|------|------|
| CR-V1 | 改代码后 Play `Village_Shop` | Unity **不闪退** |
| CR-V2 | Console | 无 Error；可有 Warning（未 Bake 等） |
| CR-V3 | 购买 Tab | 各行 Price 图片可见；HpBall **200** 为 3 位图 |
| CR-V4 | 改 Number | Total2 合计刷新；无 NullReference |
| CR-V5 | `商店界面合层` | 可保留；不影响商店 UI |

---

## ⑧ 踩坑与约束

### 8.1 初始化互调禁令

- **`HideAllDigits` / `RebuildLayout` 等辅助方法不得调用 `EnsureInitialized`**，除非文档明确单向依赖。
- 需要「确保已初始化」时，只在 **对外 API 入口**（`SetNumber`、`SetDigitString`、`Awake`）调用一次。

### 8.2 场景根节点勿误删

- `商店界面合层` 是关卡美术合图，删了只影响画面，**不修闪退**。

### 8.3 大量 DigitStrip 放大问题

- 11 行 × 2 DigitStrip ≈ 22 次 Awake；递归一次就会崩，**与行数无关，一行也会崩**。

### 8.4 回归 IMG-R1

- 修完 CRASH-1 后须再验 **IMG-V9**（无前导零），避免为躲闪退回退池化逻辑。

---

## ⑨ 改动文件清单

| 文件 | 改动 |
|------|------|
| `Assets/Scripts/Game/GameRuntime/UI/Component/UiSpriteNumberDisplay.cs` | **CRASH-1**：`HideAllDigits` 去掉对 `EnsureInitialized` 的调用 |
| `Village_Shop.unity` | 可选：绑定 `total2Digits`（CRASH-2） |

---

## ⑩ 文档关系

| 文档 | 关系 |
|------|------|
| `Shop_Bar数字图片化_…md` | IMG 主任务；IMG-R1 引入本缺陷 |
| `Shop_Total2双Tab全行合计_…md` | Total2 逻辑不变；闪退阻断 ST/IMG 验收 |

---

## ⑪ 修订记录

| 日期 | 说明 |
|------|------|
| 2026-07-06 | 首版：定位 CRASH-1 递归栈溢出；澄清 `商店界面合层` 非场景双 UI；Fix-CRASH-1 + CR-V1～V5 |
