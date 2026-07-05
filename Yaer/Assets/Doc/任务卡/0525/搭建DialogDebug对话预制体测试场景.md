# 搭建 DialogDebug 对话预制体测试场景

**任务日期**：2026-05-25  
**Unity**：2020.3.48f1  
**关联场景**：`Assets/GameRes/Scenes/DebugScene/DialogDebug.unity`（当前仅含 Main Camera，未接入正式场景管线）  
**关联能力**：对话预制体 `Assets/GameRes/Prefabs/Dialogue/`、`StoryComponentGSM.TriggerStory`、CSV 工具 `Tools/Dialogue/Import CSV`（见 `Assets/Doc/执行文档/0525/CSV转NodeCanvas对话树_架构溯源与执行说明.md`）

---

## 任务目标

将 **DialogDebug** 做成**专用对话预制体测试场景**：进入场景后，能以最小成本反复触发 `GameRes/Prefabs/Dialogue/` 下的剧情预制体（如 `Village_KenMuNiStar_Test`），验证字幕、立绘、选项分支与对话结束回调，**无需**每次从主线场景绕路。

---

## 当前现状（架构侦探结论）

| 项 | 状态 |
|----|------|
| 场景文件 | 已创建，内容几乎为空（Main Camera） |
| `SceneName` 常量 | **未**登记 `DialogDebug` |
| `GameSceneManagerConfig` | **无** `DialogDebug.asset` |
| 场景管理器脚本 | **无** `DialogDebugSceneManager` |
| `SceneManager` 根物体 / 玩家 / 剧情模块 | **无** |
| 进场景方式 | **无**（Editor 直接 Open Scene 无法走完整 GF 加载链） |

### 路径风险（施工前必须处理其一）

运行时换场使用：

```csharp
SceneAssetPath.GetSceneAssetPath(sceneName)
// => "Assets/GameRes/Scenes/" + sceneName + ".unity"
```

而场景实际在：`Assets/GameRes/Scenes/DebugScene/DialogDebug.unity`。

**若不处理，按常量名 `DialogDebug` 加载会找不到文件。**

| 方案 | 做法 | 推荐 |
|------|------|------|
| **A** | 将场景移到 `Assets/GameRes/Scenes/DialogDebug.unity`，常量 `DialogDebug` | ✅ 改动最小，不动 `SceneAssetPath` |
| **B** | 保留子目录，扩展 `SceneAssetPath`（如支持 `DebugScene/DialogDebug`）并统一常量 | 适合后续多个 Debug 场景 |

任务施工时**选定一种**并在提交说明中写明；若选 B，同步更新 Resource Editor / Scenes in Build。

---

## 范围

### 必须完成

1. **场景管线接入**：`DialogDebug` 可被 `ChangeSceneComponentGM` / `LoadSceneComponentGSM` 正常加载（含 AB / Scenes in Build，按项目 README 流程）。
2. **最小场景管理器**：`DialogDebugSceneManager` 继承 `BaseGameSceneManager`，挂载 `StoryComponentGSM` 等对话测试所需模块（参考 `NewGameSceneManager` / `HomeScene1`，**裁剪**战斗、村庄移动、地图门等无关逻辑）。
3. **场景配置**：`GameSceneManagerConfig`（建议：`canMove=0`，`canCreatePlayer=0`，`isPlayingScene=1`，`isFightingScene=0`，`canRaycast=0`，`canSave=0`）。
4. **对话触发入口（至少一种）**：
   - **推荐**：场景内 `DialogDebugStoryTester`（或等价脚本）— Inspector 填写 `StoryPrefabName` + 按键/按钮触发 `TriggerStory`；
   - **可选**：`SimpleStoryTrigger`（Click 触发），挂在测试用空物体上。
5. **进场景方式（至少一种）**：
   - **推荐**：Editor 菜单 `Tools/Dialogue/Enter DialogDebug Scene`（开发用，Play 模式下加载并初始化 GSM）；
   - **可选**：从 `StartScene` / 现有 Debug 入口增加临时按钮（需策划确认是否保留）。
6. **文档**：施工完成后在 `Assets/Doc/执行文档/0525/` 补一篇简短**施工执行说明**（改了哪些文件、如何验证）。

### 明确不做（禁止）

- 不修改 `StoryComponentGSM`、`NormalDialogueFormNewLogic`、`DialogueTMPUGUI` 核心逻辑（除非修阻塞测试的 bug，须单独立项）。
- 不修改村庄移动、`VillageWalkObstacle`、主线关卡流程。
- 不把 CSV 导入工具改成「一键生成可运行 Prefab」（仍属对话工具链独立任务）。
- 不在 DialogDebug 内实现完整关卡玩法或存档流程验收（仅需能触发对话）。

---

## 施工步骤（建议顺序）

### Step 1 — 场景路径与常量

1. **选定** §路径风险 方案 A 或 B。  
2. 在 `Assets/Scripts/Game/Static/Name/Res/SceneName.cs` 增加：  
   `public const string DialogDebug = "DialogDebug";`（若方案 B，常量值与路径一致）。  
3. 将场景纳入 **Resource Editor** 与 **Scenes in Build**（见根目录 `README.md`）。

### Step 2 — Config 与 SceneManager

1. 新建 `Assets/GameRes/Config/SceneManagerConfig/DialogDebug.asset`（字段见 §范围）。  
2. 新建目录 `Assets/Scripts/Game/GameRuntime/GameSceneManager/Scene/DialogDebug/`。  
3. 实现 `DialogDebugSceneManager`：
   - `OnInit`：`nowSceneName = SceneName.DialogDebug`（或与资源文件名一致）。  
   - `OnEnterScene`：可选自动打开对话 UI 依赖的 GM 模块；**不要**触发无关 BGM/漫画。  
   - `initAllSceneMonster`：空实现即可。

### Step 3 — 搭建 DialogDebug.unity 层级（Prefab 编辑模式或场景内）

参考结构（名称可微调，职责不变）：

```
DialogDebug.unity
├── SceneManager          ← DialogDebugSceneManager + GameSceneManagerConfig
├── Main Camera           ← 已有；需与 CameraComponentGSM 规范一致（或沿用项目相机预制）
├── EventSystem           ← UI 点击（若用按钮触发）
├── DialogDebugUI         ← 可选：Canvas + 按钮「播放对话」
└── StoryTestTrigger      ← DialogDebugStoryTester 或 SimpleStoryTrigger
```

`SceneManager` 上需具备 `BaseGameSceneManager` 常规子模块初始化能力；至少保证 **`StoryComponentGSM`** 在 `OnInit` 后可用。

### Step 4 — 对话测试脚本（推荐新建）

路径建议：`Assets/Scripts/Game/GameRuntime/Story/DialogDebugStoryTester.cs`

| 字段 / 行为 | 说明 |
|-------------|------|
| `storyPrefabName` | 默认 `Village_KenMuNiStar_Test`，可在 Inspector 改成任意 `GameRes/Prefabs/Dialogue/{名}` |
| `triggerOnEnterScene` | 可选：进入场景自动播一次 |
| `TriggerStory()` | 调用 `SceneManager.GetModule<StoryComponentGSM>().TriggerStory(storyPrefabName)` |
| Editor 按钮 | `[ContextMenu]` 或自定义 Inspector 按钮，便于 Play 模式重播 |

**注意**：`TriggerStory` 的参数是**预制体文件名（无 .prefab）**，与 `DialoguePath.GetPath` 一致。

### Step 5 — Editor 进场景入口（推荐）

路径建议：`Assets/Editor/Tool/Dialogue/DialogDebugSceneMenu.cs`

- 菜单：`Tools/Dialogue/Enter DialogDebug Scene`  
- 行为：在 Editor Play 下调用现有 `LoadSceneComponentGSM` 或 `ChangeSceneComponentGM.LoadScene`，目标场景名 `DialogDebug`。  
- 若 Editor 未走完整 GameManager 初始化，需在菜单项内注明前置条件（如从 InitScene 启动后使用），或提供最小化 Bootstrap。

### Step 6 — 与 CSV / Prefab 工作流衔接

测试对话预制体时的推荐流程：

1. `Tools/Dialogue/Import CSV` → 生成 `Assets/GameRes/DialogueTrees/Generated/*.asset`  
2. 在测试 Prefab（如 `Village_KenMuNiStar_Test`）上：**Delete Bound Graph → 拖 asset → Bind Graph → 重绑雅尔/古莎 Actor**  
3. Play → `Tools/Dialogue/Enter DialogDebug Scene`（或项目既定进场景方式）  
4. 点击测试按钮 / 自动触发 → 检查字幕、立绘、结束是否调用 `OnStoryEnd`

---

## 验收标准

- [ ] 通过**既定进场景方式**进入 `DialogDebug` 后，Console **无**场景加载路径错误、`SceneManager` 初始化错误。  
- [ ] Inspector 中修改 `storyPrefabName` 为 `Village_KenMuNiStar_Test`，触发后 **NormalDialogueNewPanel** 正常打开，三句对白与 CSV 一致。  
- [ ] 对话结束后 `StoryComponentGSM` 恢复（`HasRunningStory == false`），可再次触发同一条对话。  
- [ ] 替换为带 **Multiple Choice** 的预制体时，选项显示与分支正确（若有此类测试资源）。  
- [ ] **未**改动村庄移动、主线 SceneManager、存档核心代码（diff 可审查）。  
- [ ] `Assets/Doc/执行文档/0525/` 下有对应施工说明，含验证步骤截图或文字清单。

---

## 验证清单（给施工员自测）

1. 确认 `GameRes/Prefabs/Dialogue/Village_KenMuNiStar_Test.prefab` 已 Bind CSV 图且 Actor 已拖好。  
2. Play → 进入 DialogDebug → 触发剧情。  
3. 观察：立绘是否刷新、表情是否默认/符合节点、跳过/自动是否正常。  
4. 结束后再触发一次，确认无「剧情仍在运行」拦截。  
5. 构建 AB / Standalone 前确认场景已加入 Resource Editor（与团队打包流程一致）。

---

## 参考文件（溯源锚点）

| 主题 | 路径 |
|------|------|
| 场景路径 | `Assets/Scripts/Game/Static/Path/SceneAssetPath.cs` |
| 剧情触发 | `Assets/Scripts/Game/GameRuntime/GameSceneManager/Component/StoryComponentGSM.cs` |
| 对话 UI | `Assets/Scripts/Game/GameRuntime/UI/FormLogic/Story/NormalDialogueFormNewLogic.cs` |
| 简单触发器 | `Assets/Scripts/Game/GameRuntime/Entities/SceneEntities/CommonEntity/SimpleStoryTrigger.cs` |
| 新村/scene 示例 | `Assets/Scripts/Game/GameRuntime/GameSceneManager/Scene/NewGame/NewGameSceneManager.cs` |
| CSV 架构说明 | `Assets/Doc/执行文档/0525/CSV转NodeCanvas对话树_架构溯源与执行说明.md` |
| 测试预制体 | `Assets/GameRes/Prefabs/Dialogue/Village_KenMuNiStar_Test.prefab` |

---

## 产出物清单

| 类型 | 预期路径 |
|------|----------|
| 任务卡 | 本文档 |
| 场景管理器 | `.../Scene/DialogDebug/DialogDebugSceneManager.cs` |
| 测试脚本 | `.../Story/DialogDebugStoryTester.cs`（名可微调） |
| Editor 菜单 | `Assets/Editor/Tool/Dialogue/DialogDebugSceneMenu.cs` |
| Config | `Assets/GameRes/Config/SceneManagerConfig/DialogDebug.asset` |
| 场景 | `DialogDebug.unity`（路径随 §路径风险 方案确定） |
| 执行说明 | `Assets/Doc/执行文档/0525/DialogDebug对话测试场景_施工执行说明.md` |

---

## 待确认（可记入 `Docs/OPEN_QUESTIONS.md`）

1. 场景文件最终放在 `Scenes/DialogDebug.unity` 还是保留 `Scenes/DebugScene/` 子目录？  
2. 进场景入口仅 Editor 菜单是否足够，是否需要在游戏内 Debug 面板保留入口？  
3. 测试场景是否需要默认玩家实体（`canCreatePlayer=1`）还是纯 UI 对话即可？  
4. 是否需要在 DialogDebug 内预置多套 `SimpleStoryTrigger` 对应当前所有在测 Prefab？

---

**下一步**：由【施工员】按本任务卡 Step 1～6 实施；实施前建议先与负责人确认 §路径风险 方案 A/B。
