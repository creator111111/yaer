# CSV 导入工具 — 开场前奏选项 — 施工执行说明

**施工员交付（待实施）** | 日期：2026-05-25  
**依据**：
- `Assets/Doc/技术文档/CSV转NodeCanvas对话树导入工具_开发文档.md`（阶段 1 已落地）
- `Assets/Doc/执行文档/0525/CSV转NodeCanvas对话树_架构溯源与执行说明.md` §5、§7
- 对话前奏实测样例：`Assets/GameRes/Prefabs/Dialogue/Village_KenMuNiStart.prefab`
- DialogDebug 沙盒：`Assets/Doc/执行文档/0525/DialogDebug对话测试场景_施工执行说明.md`

**Unity 版本**：2020.3.48f1  

---

## 1. 目标（一句话）

在 **`Tools → Dialogue → Import CSV`** 窗口增加可选「开场前奏」勾选，使工具能自动插入 Action 链（藏战斗面板 / 立绘淡入 / 对话框 UI 淡入）；**默认全部不勾选时，行为与当前阶段 1 完全一致**，不影响已有 CSV → 纯对白 `.asset` 工作流。

---

## 2. 背景与痛点

| 现状 | 问题 |
|------|------|
| 阶段 1 只生成 `StatementNodeEx` + `MultipleChoiceNode` + 连线 | 成品 Prefab（如 `Village_KenMuNiStart`）入口多为 **ActionNode 前奏**，需每次手点 |
| 裸 `.asset` 的 `primeNode` 指向最小 ID 对白行 | 与运行时 Prefab 图结构不一致，合并前缺少演出节点 |
| DialogDebug 测试 Prefab `Village_KenMuNiStar_Test` 故意无前奏 | 工具不能「一刀切」给所有对话强加三个节点 |

**本任务范围**：扩展 **Editor 导入工具**（窗口 + 建图器），**不改** `StoryComponentGSM`、`NormalDialogueFormNewLogic`、`DialogueTMPUGUI` 等运行时逻辑。

**本任务非范围（后续单独立项）**：一键「合并进 Prefab」、CSV 增列 Face/三语、旁白 Action 行。

---

## 3. 兼容性约束（强制）

> **施工第一原则：不勾选任何前奏选项时，生成结果必须与改工具前字节级等价（节点类型、数量、连线、primeNode、actorParameters）。**

| 约束 | 实现要求 |
|------|----------|
| **默认关闭** | 三个勾选 + 「结束时恢复战斗面板」默认均为 **false** |
| **API 向后兼容** | `DialogueCsvGraphBuilder.TryBuild(...)` 保留现有 4 参数重载；前奏通过 **新增可选参数** 或 **重载** 传入，旧调用点无需改代码 |
| **前奏为空则短路** | `DialoguePreludeOptions.IsEmpty == true` 时，建图逻辑 **不得** 进入前奏分支，直接走现有 Step3～Step6 |
| **不碰 CSV 解析** | `DialogueCsvParser`、`DialogueRow`、`DialogueSpeakerMapping` 行为不变 |
| **不碰运行时** | 禁止修改 `Assets/Scripts/Game/**` 下对话播放链路（除非单独修 bug 立项） |
| **Undo 行为不变** | 仍 `Undo.RegisterCreatedObjectUndo` + Graph 内部 `AddNode`/`ConnectNodes` 自带 Undo |

---

## 4. 窗口 UI 变更（`DialogueCsvImportWindow`）

在「输出目录」与「生成按钮」之间增加折叠区 **「开场前奏（可选）」**：

| 控件 | 字段名（建议） | 默认值 | 说明 |
|------|----------------|--------|------|
| Toggle | `fadeDialogueUI` | **false** | 插入 `NormalDialogueUIAlphaAnimationTaskAction`（0→1，0.7s） |
| Toggle | `hideFightingPanelOnStart` | **false** | 插入 `FightingPanelVisibleActionTask`（Visible=false） |
| Toggle | `restoreFightingPanelOnEnd` | **false** | 在 **图末尾**（无出边叶子节点之后）插入 Visible=true；**仅当**上一项勾选时可编辑 |
| Toggle | `fadePortraitCanvasGroups` | **false** | 插入 `CanvasGroupAlphaActionTask` 序列（0→1，0.7s） |
| ObjectField | `portraitReferencePrefab` | **None** | 类型 `GameObject`；**仅当** `fadePortraitCanvasGroups` 勾选时可编辑 |
| FloatField（可选） | `preludeFadeDuration` | **0.7** | 与现有 Prefab 一致；v1 可写死常量，不必首版暴露 |

**HelpBox 文案（建议）**：

```
· 全部不勾选：与阶段 1 相同，仅生成对白/选项节点。
· 对话框 UI 淡入 / 藏战斗面板：可写入 .asset，合并进 Prefab 后实机有效。
· 立绘淡入：须指定参考 Prefab（读取 Blackboard 中 CanvasGroup 变量名）；仅生成 .asset 且无 Blackboard 时节点无法绑定，导入时会警告或中止。
```

**按钮**：仍只有一个 **「生成 DialogueTree .asset」**（本阶段不新增「合并 Prefab」按钮，避免 scope 膨胀）。

**窗口尺寸**：`minSize` 高度建议增至 **420～480**，避免折叠区被裁切。

---

## 5. 拟新增 / 修改文件

| 文件 | 操作 | 职责 |
|------|------|------|
| `Assets/Editor/Tool/Dialogue/DialoguePreludeOptions.cs` | **新建** | 前奏勾选 DTO + `IsEmpty` 判断 + 默认工厂 `CreateDefault()`（全 false） |
| `Assets/Editor/Tool/Dialogue/DialoguePreludeBuilder.cs` | **新建** | 按选项创建 ActionNode 链、设置 BBParameter、返回 `(entryNode, tailNode, epilogueNodes)` |
| `Assets/Editor/Tool/Dialogue/DialoguePortraitReferenceResolver.cs` | **新建**（可合并进 Builder） | 从参考 Prefab 的 `DialogueTreeController` 解析 Blackboard 中 `CanvasGroup` 变量名列表 |
| `Assets/Editor/Tool/Dialogue/DialogueCsvGraphBuilder.cs` | **修改** | 新增带 `DialoguePreludeOptions` 的重载；前奏非空时调整布局与 primeNode |
| `Assets/Editor/Tool/Dialogue/DialogueCsvImportWindow.cs` | **修改** | 绘制勾选 UI；`GenerateAsset()` 组装 `DialoguePreludeOptions` 传入 Builder |
| `Assets/Doc/技术文档/CSV转NodeCanvas对话树导入工具_开发文档.md` | **修改（实施后）** | 补充前奏选项说明与验证项 |

**禁止新建或修改**：`StoryComponentGSM.cs`、`NormalDialogueFormNewLogic.cs`、任意 `Assets/Scripts/Game/GameRuntime/**` 运行时对话逻辑。

---

## 6. 前奏节点规格（对齐 `Village_KenMuNiStart`）

### 6.1 节点顺序（自上而下 = 执行顺序）

当 **全部相关项勾选** 时，入口链与成品 Prefab 对齐：

```
[Action] FightingPanel 显示开关 (Visible=false)
    ↓
[Action] ActionList (In Sequence)
         · CanvasGroupAlpha: {PaintingA} 0→1, 0.7s
         · CanvasGroupAlpha: {PaintingB} 0→1, 0.7s
    ↓
[Action] 对话框 UI 透明度动画 0→1, 0.7s
    ↓
[StatementNodeEx] CSV 第一句对白 …
    ↓
   …
    ↓
[Action] FightingPanel 显示开关 (Visible=true)   ← 仅 restoreFightingPanelOnEnd
```

**单项勾选时只插入对应子链**，顺序固定为：

1. 藏战斗面板（若勾）
2. 立绘淡入（若勾）
3. 对话框 UI 淡入（若勾）

### 6.2 对应 ActionTask 类型

| 功能 | 类型 | 路径 |
|------|------|------|
| 战斗面板显隐 | `Game.GameRuntime.Story.Node.FightingPanelVisibleActionTask` | `.../FightingPanelVisibleActionTask.cs` |
| 对话框 UI 淡入 | `Game.GameRuntime.Story.Node.NormalDialogueUIAlphaAnimationTaskAction` | `.../NormalDialogueUIAlphaAnimationTaskAction.cs` |
| 立绘 CanvasGroup 淡入 | `CanvasGroupAlphaActionTask` | `.../CanvasGroupAlphaActionTask.cs` |

### 6.3 布局坐标

- 现有对白节点：`BaseX=200`, `BaseY=100 + rowIndex * 120`（**保持不变**）。
- 前奏节点：放在对白区域 **上方**，建议：
  - `preludeBaseY = BaseY - preludeCount * PreludeSpacing`（`PreludeSpacing = 120`，与 `RowSpacing` 一致）
  - 同一前奏链内自上而下递增 Y（与 NodeCanvas Vertical flow 一致）。
- **重要**：启用前奏时，对白节点 Y **不要** 因前奏而整体下移（避免 diff 过大）；前奏占用负 Y / 更小 Y 区域即可。

### 6.4 primeNode 规则

| 前奏 | primeNode |
|------|-----------|
| `IsEmpty` | **不变**：最小 ID 对白/选项节点（或 `startRowId`） |
| 非空 | 前奏链 **第一个** ActionNode |

### 6.5 连线规则

1. 前奏链内顺序 `ConnectNodes`。
2. 前奏链尾 → CSV 图入口节点（`startRowId ?? Min(id)` 对应节点）。
3. CSV 内部连线逻辑 **完全沿用** 现有 Step5，不改。
4. **结束恢复战斗面板**：
   - 找出所有 **无出边** 的叶子节点（可能多个，如分支结束）。
   - 对每个叶子节点 `ConnectNodes(leaf, restoreFightingPanelNode)`；若多个叶子，可共用同一个 restore 节点（NodeCanvas 允许多入边）或每叶子一个 restore（v1 推荐 **共用一个 restore 节点**，与 `Village_KenMuNiStart` 单出口一致；多分支时每个 dead-end 都连到 restore）。

### 6.6 立绘引用解析

**参考 Prefab**（如 `Village_KenMuNiStart.prefab`）上 `DialogueTreeController` 的 Blackboard 变量：

- 筛选类型为 `UnityEngine.CanvasGroup` 的变量（如 `GoOutStoryYaerPainting`、`GushaPainting`）。
- 在 **新生成的 DialogueTree** 上：
  - **方案 A（推荐 v1）**：仅写入 Action 节点的 `canvasGroup._name` 字符串，**不**复制 Blackboard 引用；生成 HelpBox 提示「须合并进含同名 Blackboard 的 Prefab 后生效」。
  - **方案 B（可选增强）**：从参考 Prefab 复制 Blackboard 变量定义到新 `tree` 的 blackboard（仍无 Scene 对象引用，实机仍依赖 Prefab 合并）。

**未指定 `portraitReferencePrefab` 却勾选立绘淡入**：`TryBuild` 返回 null，窗口显示错误，**不**写 `.asset`。

---

## 7. 建图算法变更（`DialogueCsvGraphBuilder`）

在现有六步基础上，**仅当 `!preludeOptions.IsEmpty`** 时插入 **Step 0** 与 **Step 7**：

```
Step0  （可选）DialoguePreludeBuilder.TryCreatePreludeNodes(tree, options, out entry, out tail, out restoreNode)
Step3  actorParameters（不变）
Step4  创建 CSV 对白/选项节点（不变）
Step5  CSV 连线（不变）
Step5b （可选）tail → CSV 入口；各叶子 → restoreNode
Step6  primeNode = entry ?? CSV 入口（不变量见 §6.4）
Step7  （可选）epilogue restore 节点创建并入链
```

**`TryBuild` 签名建议**：

```csharp
// 保留现有签名，内部转调：
public static DialogueTree TryBuild(
    IReadOnlyList<DialogueRow> rows,
    DialogueSpeakerMapping mapping,
    int? startRowId,
    string assetName)
    => TryBuild(rows, mapping, startRowId, assetName, DialoguePreludeOptions.CreateDefault());

// 新增
public static DialogueTree TryBuild(
    IReadOnlyList<DialogueRow> rows,
    DialogueSpeakerMapping mapping,
    int? startRowId,
    string assetName,
    DialoguePreludeOptions preludeOptions);
```

---

## 8. ActionNode 创建实现要点

NodeCanvas 的 `ActionNode` 通过 `AddNode<ActionNode>` 创建后，需设置 `_action` 字段（类型为 `ActionTask` 或 `ActionList`）。

| 难点 | 处理方式 |
|------|----------|
| `_action` 为私有 / 非 Unity Object | 与 `_actorName` 相同：**反射**写入，或 NodeCanvas 提供的 `ActionNode` 公开 API（施工前先读 `ActionNode.cs` 确认） |
| `ActionList` 多立绘顺序执行 | `executionMode = ActionsRunInSequence`；子 action 数组按 Blackboard 变量顺序 |
| `BBParameter<bool> Visible` | 藏面板：`false`；恢复：`true` |
| `BBParameter<float>` Start/End/Duration | StartAlpha 默认 0，EndAlpha=1，Duration=0.7 |

**替代方案（若反射 `_action` 成本过高）**：

- 使用 NodeCanvas 编辑器同款 `Task` 工厂 / JSON 模板克隆一个最小 ActionNode 再改参数（维护成本高，**不推荐**）。

---

## 9. 验证清单

### 9.1 回归 — 默认行为（必过，优先级最高）

使用 `Assets/Dialog/村内第一段对话.csv`，**三个勾选全部 false**，生成 `.asset`：

- [ ] 节点数 = **3** 个 `StatementNodeEx`，无 `ActionNode`
- [ ] `primeNode` = ID 1 对白节点
- [ ] 三句 `statement.text` 与 CSV 一致
- [ ] `actorParameters` 含「雅尔」「古莎」
- [ ] 连线 1→2→3，末节点无出边
- [ ] Ctrl+Z 可撤销整次导入
- [ ] Console 无新增 Error

> 建议：改工具前后各导出一次 `.asset` 的图 JSON（或 Node 数、类型序列）做对比，确认默认路径零 diff。

### 9.2 前奏 — 仅对话框 UI 淡入

- [ ] 勾选 `fadeDialogueUI`，生成 `.asset` 含 **1** 个 ActionNode + **3** 个 StatementNodeEx
- [ ] `primeNode` = UI 淡入 ActionNode
- [ ] ActionNode → 第一句对白 有连线
- [ ] 在 NodeCanvas 编辑器中图结构可读

### 9.3 前奏 — 藏/恢复战斗面板

- [ ] 勾选 start + end：入口 Hide、图末 Restore 各 1 节点
- [ ] 仅 start 不勾 end：只有 Hide，无 Restore
- [ ] end 单独勾选而 start 未勾：UI 禁用或导入时报错

### 9.4 前奏 — 立绘淡入

- [ ] 指定 `Village_KenMuNiStart.prefab` 为参考，勾选立绘淡入
- [ ] 生成 ActionList 含 ≥2 个 `CanvasGroupAlphaActionTask`，变量名与参考 Prefab Blackboard 一致
- [ ] 未指定参考 Prefab：导入失败并提示，**不**生成半成品 asset

### 9.5 组合 — 全选（对齐村线开场）

- [ ] 入口链顺序：Hide → 立绘 → UI 淡入 → 对白
- [ ] 末尾 Restore FightingPanel（若勾选 end）
- [ ] DialogDebug 拖 **合并后** 的测试 Prefab（阶段 2 前：手动把生成图拷入 Prefab 验证一次）

### 9.6 回归 — 周边系统

- [ ] `DialogueConfigExcelTool` 仍可用
- [ ] `StoryComponentGSM`、`DialogueTMPUGUI` 无 diff
- [ ] DialogDebug 使用 `Village_KenMuNiStar_Test`（无前奏）仍正常

---

## 10. 施工顺序建议

1. 新建 `DialoguePreludeOptions` + 单元测试式 Editor 菜单（可选）验证 `IsEmpty`
2. 实现 `DialoguePreludeBuilder`（先 UI 淡入单节点，再 ActionList，再 FightingPanel）
3. 扩展 `DialogueCsvGraphBuilder.TryBuild` 重载，**先写回归测试**（§9.1）再写前奏
4. 改 `DialogueCsvImportWindow` UI，串接 options
5. 实现立绘 Resolver + 校验
6. 跑完 §9 全部清单，更新技术文档 §三、§八

---

## 11. 风险与规避

| 风险 | 规避 |
|------|------|
| 破坏默认纯对白生成 | §3 强制默认 false + §9.1 回归优先 |
| ActionNode 反射失败编译通过但运行空 action | 生成后在 Editor 中打开图，目视 Action 摘要文案（如「对话框UI透明度动画」） |
| 立绘变量名与目标 Prefab 不一致 | 必须从参考 Prefab 解析，禁止手填字符串常量 |
| 与 `StoryComponentGSM` 战斗立绘逻辑重复 | 本工具只控制 **整块 FightingPanel**；不在运行时再加一层；文档注明与 GSM 职责区别 |
| 多分支对话多个叶子 | restore 节点共用一个，所有 dead-end 都连入 |

---

## 12. 阶段 2 预留（本文档不实施）

| 项 | 说明 |
|----|------|
| 「合并进 Prefab」按钮 | 克隆模板 Prefab，替换 bound graph，保留 Blackboard 对象引用 |
| 前奏模板 Preset SO | `None / UIOnly / WithPaintings / Combat` 一键填勾选 |
| CSV 列驱动 | Face、旁白 Action 行 |

---

## 13. 版本记录

| 版本 | 日期 | 说明 |
|------|------|------|
| 0.1 | 2026-05-25 | 首版：开场前奏勾选扩展施工说明；强调默认行为与阶段 1 完全兼容 |
