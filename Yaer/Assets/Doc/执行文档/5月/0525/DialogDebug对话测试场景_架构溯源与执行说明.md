# DialogDebug 对话预制体测试场景 — 架构溯源与执行说明

**文档性质**：架构侦探产出（只读分析 + 施工指引，**本阶段以文档修订为主**）  
**依据**：`Assets/Doc/00_MASTER_PROMPT.md`【架构侦探】；任务卡 `Assets/Doc/任务卡/0525/搭建DialogDebug对话预制体测试场景.md`；**2026-05-25 实测反馈**（GF + Tools 进场景不符合预期）  
**Unity 版本**：2020.3.48f1  
**关联能力**：CSV 导入链见 `Assets/Doc/执行文档/0525/CSV转NodeCanvas对话树_架构溯源与执行说明.md`

---

## 1. 结论（一句话）

**DialogDebug 应做成与 GF 正式场景管线解耦的「对话沙盒」：Unity 里直接 Open `DialogDebug.unity` → Play，在 Inspector 把 `GameRes/Prefabs/Dialogue/` 下的预制体拖到测试组件上即可跑字幕/立绘/选项——不走 InitScene、不登记 `SceneName`、不用 `Tools/Dialogue/Enter DialogDebug Scene`；场景内自备最小 `GameManager`（仅语言）+ 场景常驻 `DialogueTMPUGUI`，由新建的 `DialogDebugPlayground` 直接 `Instantiate` 预制体并调用 `DialogueTreeController.StartDialogue()`。**

---

## 2. 需求变更说明（为何推翻旧方案）

### 2.1 旧方案（GF 耦合）— 已验证**不符合**预期

| 步骤 | 旧流程 |
|------|--------|
| 启动 | 必须从 **InitScene** Play，走 GameManager / GF 初始化 |
| 进场景 | `Tools/Dialogue/Enter DialogDebug Scene` 或 `LoadSceneComponentGSM` |
| 测对话 | `DialogDebugStoryTester` 填 **字符串 prefab 名** → `StoryComponentGSM.TriggerStory` |
| 依赖 | `DialogDebugSceneManager`、`SceneName`、`SceneAssetPath`、Resource Editor、AB |

**问题**：本质仍是「正式游戏换场 + 正式剧情管线」，配置成本高，无法「打开场景就测、拖 prefab 就播」。

### 2.2 新方案（解耦沙盒）— 目标体验

| 步骤 | 新流程 |
|------|--------|
| 启动 | **直接 Open `DialogDebug.unity` → Play** |
| 换测试对象 | Inspector **拖入**对话 prefab（`GameObject` 引用），或拖 Project 里任意 `GameRes/Prefabs/Dialogue/*.prefab` |
| 测对话 | 勾选「进场景自动播」或按 **Play / 按键** 重播 |
| 依赖 | 场景内自洽；**不**经过 `StoryComponentGSM` / `ChangeSceneComponentGM` |

**生活类比**：旧方案像「先启动整台游戏机、登录账号、进主城再传送到试玩房间」；新方案像「单独打开的试玩台，把卡带插上去就能播片」。

---

## 3. 业务背景与 CSV 工作流

### 3.1 策划/程序在做什么

- CSV 导入 → Bind Graph → 重绑 Actor 后，要**零门槛**看对话表现。
- 期望：**拖 prefab → Play → 看字幕/立绘/分支 → 结束后再拖下一个 prefab 或重播**。

### 3.2 推荐工作流（修订后）

```
Tools/Dialogue/Import CSV
  → GameRes/DialogueTrees/Generated/*.asset
  → 在测试 Prefab 上 Bind Graph + 重绑雅尔/古莎 Actor
  → Open DialogDebug.unity
  → Inspector：DialogDebugPlayground.dialoguePrefab ← 拖 Village_KenMuNiStar_Test.prefab
  → Play（或勾选 playOnStart）
  → 核对字幕、立绘、Multiple Choice、播完可重播
```

**仍适用**：运行时验证的是 **`GameRes/Prefabs/Dialogue/{名}.prefab`** 上的 Bound Graph，不是裸 `.asset`。

---

## 4. 架构对比：解耦沙盒 vs GF 正式管线

| 维度 | ❌ GF 耦合（旧文档 / 已实现但弃用方向） | ✅ 解耦沙盒（本文档目标） |
|------|----------------------------------------|---------------------------|
| 进场景 | InitScene → Tools 菜单 / LoadScene | **Open Scene + Play** |
| 场景路径 | 须对齐 `SceneAssetPath`（方案 A/B） | 可保留 `Scenes/DebugScene/DialogDebug.unity`，**无需**登记 `SceneName` |
| 场景管理器 | `DialogDebugSceneManager : BaseGameSceneManager` | **不需要**（或从场景中移除） |
| 触发对话 | `StoryComponentGSM.TriggerStory(字符串名)` | **`DialogDebugPlayground` 拖 prefab 引用** |
| 对话 UI | `NormalDialogueNewPanel` 经 `UIComponentGM` 打开 | 场景内**常驻** `DialogueTMPUGUI`（从 UI prefab 拆出或嵌实例） |
| 资源加载 | `ResComponentGM.LoadAsset`（AB 链） | **`Instantiate` 拖入的 prefab** 或 `Resources`/Editor 直引（开发期） |
| AB / Resource Editor | 日常开发也建议配置 | **日常测对话不必**；打包主线时再管 |
| 与主线 diff | 易误触 GSM / 存档 / 战斗 HUD | **隔离**，不改 `StoryComponentGSM` 等核心 |

**重要修改原因**：测试场景的职责是「验证 NodeCanvas 图 + 立绘 + UI」，不是「验证 GF 换场是否正确」——二者应拆分。

---

## 5. 独立运行链路溯源（新方案核心）

### 5.1 最小运行时链

```
DialogDebug.unity Play
        │
        ▼
DialogDebugRuntimeBootstrap（Awake）
        │  若场景无 GameManager → 创建仅含 language 的最小单例
        │  （DialogueTMPUGUI 读 GameManager.Instance.language 取中文/英/日字幕）
        ▼
DialogDebugPlayground.Start / 按键 / ContextMenu
        │  dialoguePrefab 为 Inspector 拖入的 GameObject 引用（非字符串路径）
        ▼
Instantiate(dialoguePrefab, dialogueContainer)
        │
        ▼
DialogueTreeController.StartDialogue()
        │  NodeCanvas 图内 StatementNodeEx / MultipleChoice / Action 等
        ▼
DialogueTree 静态事件 → 场景内 DialogueTMPUGUI
        │  OnSubtitlesRequest → 字幕 + DialogueActorEx 立绘
        │  OnMultipleChoiceRequest → 选项
        ▼
OnDialogueFinished → DialogueTMPUGUI.OnDialogueEnd
        ▼
Playground 销毁实例 / 允许再次 Play
```

### 5.2 与正式管线的关系（只读对照，不调用）

正式游戏中仍为：

```
StoryComponentGSM.TriggerStory(name)
  → ResComponentGM → NormalDialogueFormNewLogic.StartDialogue(go)
  → DialogueTreeController.StartDialogue()
  → DialogueTMPUGUI（同上）
```

**沙盒与正式管线共享**：`DialogueTMPUGUI`、`StatementNodeEx`、`DialogueActorEx`、对话 prefab 资产本身。  
**沙盒刻意不共享**：`StoryComponentGSM`、`BaseGameSceneManager`、`UIComponentGM` 开 Form。

### 5.3 硬依赖与最小补齐

| 依赖点 | 来源 | 沙盒处理方式 |
|--------|------|----------------|
| `GameManager.Instance.language` | `DialogueTMPUGUI` L199、L345 | 场景放 **空 GameManager** 物体（仅 `Awake` 设单例 + 默认中文），或 `DialogDebugRuntimeBootstrap` 保证存在 |
| `DialogueTreeController` | 对话 prefab 根/子节点 | 拖入的 prefab 必须含此组件（现有 `GameRes/Prefabs/Dialogue/*` 均满足） |
| `DialogueTMPUGUI` | 字幕/选项 UI | **场景预置**，Playground 可 `[SerializeField]` 引用 |
| `EventSystem` | UI 点击 / 选项 | 场景预置 |
| `SubtitlesRequestInfoEx` / `DialogueActorEx` | 项目扩展节点 | 随 prefab 与 UI 自带，无需 GF |

**无需**：`StoryComponentGSM`、`HistoryDialogueData`、存档面板、战斗立绘开关（沙盒不测这些）。

### 5.4 已知风险（须在文档与 Console 提示）

- 对话 prefab 内 **ActionNode** 若调用 `LoadSceneTaskAction`、`TriggerStory` 等 **GF/换场 API**，在沙盒中可能 NRE 或行为异常 → 测「纯对白/选项」图最稳；含换场 Action 的图需回正式场景测。
- `NormalDialogueFormNewLogic` 的存读档/设置按钮在沙盒中**不提供**（或 UI 只保留 `DialogueTMPUGUI` 子树，去掉顶部工具栏）。

---

## 6. 现状与既有实现（施工员如何处理）

| 资产 | 状态 | 新方案处置 |
|------|------|------------|
| `Assets/GameRes/Scenes/DialogDebug.unity`（或 `DebugScene/` 下） | 可能已按旧方案搭 SceneManager | **移除** `DialogDebugSceneManager` 及 GSM 子树；改挂沙盒层级（§9） |
| `DialogDebugSceneManager.cs` | 已实现 | **不再使用**；可保留文件避免他人误引，或标记 `[Obsolete]`（单独立项） |
| `DialogDebugStoryTester.cs` | 已实现，依赖 GSM | **替换为** `DialogDebugPlayground.cs`（拖 prefab 引用） |
| `DialogDebugSceneMenu.cs` | Enter DialogDebug 菜单 | **降级为可选**；日常不用；或删除菜单项 |
| `DialogDebugSceneSetupMenu.cs` | Setup 场景 | **改菜单逻辑**：搭建沙盒层级而非 SceneManager |
| `SceneName.DialogDebug` | 可能已加 | GF 换场**不需要**；保留无害，新方案不依赖 |
| `DialogDebug.asset` Config | 可能已建 | 新方案**不需要** |

---

## 7. 场景路径

解耦方案下 **不必** 为 GF 扁平化路径：

| | 说明 |
|---|------|
| **推荐保留** | `Assets/GameRes/Scenes/DebugScene/DialogDebug.unity` — 与「Debug 专用、不进正式换场表」语义一致 |
| **不再强制** | 移到 `Scenes/DialogDebug.unity`、登记 `SceneName`、配 Resource Editor |

若团队仍希望 Build 列表里有该场景（仅 Editor 试玩），可加 Scenes in Build，**与 AB 解耦**。

---

## 8. `DialogDebugPlayground` 设计（施工员实现 — 核心）

### 8.1 路径与职责

- **路径**：`Assets/Scripts/Game/GameRuntime/Story/DialogDebugPlayground.cs`
- **挂载**：`DialogDebugPlayground` 空物体（场景根或 `Playground` 下）

### 8.2 Inspector 字段

| 字段 | 类型 | 说明 |
|------|------|------|
| `dialoguePrefab` | **GameObject** | **拖 Project 中对话 prefab**；换测试对象只改此引用 |
| `dialogueContainer` | Transform | 实例化父节点（空物体 `DialogueInstanceRoot`） |
| `dialogueUI` | DialogueTMPUGUI | 场景内字幕 UI 引用 |
| `playOnStart` | bool | 进 Play 自动播一次 |
| `replayKey` | KeyCode | 默认 `T`，播完可重播 |
| `destroyPreviousOnReplay` | bool | 重播前 Destroy 上一实例 |

### 8.3 推荐代码骨架（含注释）

```csharp
using Game.GameRuntime.Story.NodeCanvasExtend;
using NodeCanvas.DialogueTrees;
using UnityEngine;

namespace Game.GameRuntime.Story
{
    /// <summary>
    /// DialogDebug 解耦沙盒：Inspector 拖入对话 prefab，直接 Instantiate + StartDialogue。
    /// 不经过 StoryComponentGSM / GF 换场。
    /// </summary>
  public class DialogDebugPlayground : MonoBehaviour
    {
        [Tooltip("从 Project 拖入 GameRes/Prefabs/Dialogue/*.prefab")]
        [SerializeField] private GameObject dialoguePrefab;

        [SerializeField] private Transform dialogueContainer;
        [SerializeField] private DialogueTMPUGUI dialogueUI;
        [SerializeField] private bool playOnStart = true;
        [SerializeField] private KeyCode replayKey = KeyCode.T;
        [SerializeField] private bool destroyPreviousOnReplay = true;

        private DialogueTreeController runningTree;
        private bool isRunning;

        private void Start()
        {
            if (dialogueUI != null)
            {
                // 播完一次后允许重播（DialogueTMPUGUI 在 OnDialogueFinished 链末尾触发 OnDialogueEnd）
                dialogueUI.OnDialogueEnd += OnDialogueUIEnd;
            }

            if (playOnStart && dialoguePrefab != null)
            {
                PlayDialogue();
            }
        }

        private void OnDestroy()
        {
            if (dialogueUI != null)
            {
                dialogueUI.OnDialogueEnd -= OnDialogueUIEnd;
            }
        }

        private void Update()
        {
            if (replayKey != KeyCode.None && Input.GetKeyDown(replayKey) && !isRunning)
            {
                PlayDialogue();
            }
        }

        [ContextMenu("Play Dialogue")]
        public void PlayDialogue()
        {
            if (dialoguePrefab == null)
            {
                Debug.LogError("[DialogDebugPlayground] 请在 Inspector 拖入 dialoguePrefab。");
                return;
            }

            if (dialogueContainer == null)
            {
                Debug.LogError("[DialogDebugPlayground] 请指定 dialogueContainer。");
                return;
            }

            if (destroyPreviousOnReplay && runningTree != null)
            {
                Destroy(runningTree.gameObject);
                runningTree = null;
            }

            if (isRunning)
            {
                Debug.LogWarning("[DialogDebugPlayground] 对话进行中，请等待结束或开启 destroyPreviousOnReplay。");
                return;
            }

            var instance = Instantiate(dialoguePrefab, dialogueContainer);
            runningTree = instance.GetComponentInChildren<DialogueTreeController>(true);
            if (runningTree == null)
            {
                Debug.LogError("[DialogDebugPlayground] prefab 上未找到 DialogueTreeController。");
                Destroy(instance);
                return;
            }

            isRunning = true;
            runningTree.StartDialogue();
            Debug.Log($"[DialogDebugPlayground] 开始播放: {dialoguePrefab.name}");
        }

        private void OnDialogueUIEnd()
        {
            isRunning = false;
            // 可选：播完即删实例，保持 Hierarchy 干净
            if (destroyPreviousOnReplay && runningTree != null)
            {
                Destroy(runningTree.gameObject);
                runningTree = null;
            }
        }
    }
}
```

**替代方案说明**：

| 方案 | 做法 | 适用 |
|------|------|------|
| **A（推荐）** | 上列 Playground + 场景常驻 `DialogueTMPUGUI` | 拖 prefab 即测，与正式 UI 一致 |
| B | 仍用字符串 + `AssetDatabase.LoadAssetAtPath`（仅 Editor） | 不如拖引用直观，Build 外也不稳 |
| C | 继续 `StoryComponentGSM` | 即旧 GF 方案，**已否决** |

---

## 9. `DialogDebugRuntimeBootstrap` 设计（最小 GameManager）

### 9.1 原因

`DialogueTMPUGUI` 通过 `GameManager.Instance.language` 选中/英/日文本；直接 Play 空场景时 **无 InitScene**，须场景内自备单例。

### 9.2 推荐骨架

```csharp
using Game.GameMgr;
using Game.Static.Enum;
using UnityEngine;

namespace Game.GameRuntime.Story
{
    /// <summary>
    /// DialogDebug 专用：保证 GameManager.Instance 存在且 language 有默认值。
    /// 不调用 GameManager.OnInit()，不注册 GF 组件。
    /// </summary>
    public class DialogDebugRuntimeBootstrap : MonoBehaviour
    {
        [SerializeField] private LanguageEnumType defaultLanguage = LanguageEnumType.Chinese;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void EnsureBootstrapForDialogDebug()
        {
            // 可选：仅当 active 场景名为 DialogDebug 时自动创建；首版也可完全靠场景内手动挂载
        }

        private void Awake()
        {
            if (GameManager.Instance == null)
            {
                var go = new GameObject("[DialogDebug] GameManager");
                var gm = go.AddComponent<GameManager>();
                gm.language = defaultLanguage;
                DontDestroyOnLoad(go); // 若只希望本场景有效，可去掉并用场景内物体
            }
            else
            {
                GameManager.Instance.language = defaultLanguage;
            }
        }
    }
}
```

**替代方案**：在 `_Bootstrap` 下手动摆一个带 `GameManager` 组件的空物体（不挂子 GM），**不调用** `OnInit()`——`Awake` 足够提供 `Instance.language`。

---

## 10. 场景层级（DialogDebug.unity — 沙盒版）

### 10.1 推荐 Hierarchy

```
DialogDebug.unity
├── _Bootstrap
│   └── GameManager              ← 仅 GameManager 组件（或 DialogDebugRuntimeBootstrap）
├── Main Camera
├── EventSystem
├── DialogUI                     ← Canvas（Screen Space）
│   └── （从 NormalDialogueNewPanel 取 DialogueTMPUGUI 所需子树，或嵌 UI prefab 后删掉 NormalDialogueFormNewLogic）
│       └── DialogueTMPUGUI      ← 必须启用、Awake 时会 Subscribe DialogueTree 事件
├── DialogueInstanceRoot         ← 空 Transform，Playground.dialogueContainer
└── DialogDebugPlayground        ← dialoguePrefab 拖引用；playOnStart / replayKey
```

### 10.2 UI 搭建要点

1. **来源**：`Assets/GameRes/Prefabs/UI/NormalDialogueNewPanel.prefab` 拖入场景 → **移除** `NormalDialogueFormNewLogic`（及 GF Form 基类依赖）→ **保留** `DialogueTMPUGUI` 及其 `subtitlesGroup`、选项按钮等序列化引用。
2. **简化版（首版可接受）**：只保留字幕条 + 选项区 + `DialogueTMPUGUI`；去掉存读档/设置/历史按钮，避免点击时找 `UIComponentGM`。
3. `DialogDebugPlayground.dialogueUI` 指向该 `DialogueTMPUGUI`。

### 10.3 明确不需要（沙盒）

- `SceneManager` / `DialogDebugSceneManager` / 任意 `*ComponentGSM`
- `Map`、玩家出生、战斗 HUD
- `Tools/Dialogue/Enter DialogDebug Scene` 作为日常入口

---

## 11. Editor 工具（修订）

| 菜单 | 旧职责 | 新职责 |
|------|--------|--------|
| `Tools/Dialogue/Import CSV` | 不变 | 不变 |
| `Tools/Dialogue/Enter DialogDebug Scene` | GF 换场 | **删除或标记 Deprecated**；HelpBox 改为「请直接 Open DialogDebug.unity Play」 |
| `Tools/Dialogue/Setup DialogDebug Scene` | 创建 SceneManager | **改为** 创建 §10 沙盒层级（Bootstrap + DialogUI + Playground + EventSystem） |

---

## 12. 施工步骤（建议顺序 — 修订）

### Step 1 — 清理旧 GF 搭建（若已做）

1. 打开 `DialogDebug.unity`，删除 `SceneManager` 及 `DialogDebugSceneManager`。
2. 删除或停用 `DialogDebugStoryTester`（字符串 + GSM 方案）。

### Step 2 — 沙盒层级

1. 按 §10 创建 `_Bootstrap`、`DialogUI`、`DialogueInstanceRoot`、`DialogDebugPlayground`、`EventSystem`。
2. 从 `NormalDialogueNewPanel` 提取 `DialogueTMPUGUI` 到 `DialogUI`（§10.2）。

### Step 3 — 脚本

1. 新建 `DialogDebugPlayground.cs`（§8.3）。
2. 新建 `DialogDebugRuntimeBootstrap.cs` 或手动挂最小 `GameManager`（§9）。
3. 更新 `DialogDebugSceneSetupMenu` 生成沙盒而非 SceneManager（§11）。

### Step 4 — 联调

1. Inspector：`dialoguePrefab` ← 拖 `Village_KenMuNiStar_Test.prefab`。
2. **直接 Play 本场景**（不要从 InitScene 启动）。
3. 确认字幕三句、立绘、播完按 `T` 可重播。

### Step 5 — 文档与可选 GF

1. 施工完成后写简短施工执行说明（改了哪些文件、验证截图）。
2. **可选**：若将来要从主线跳进 DialogDebug，再单独立项接 GF——**不在本沙盒任务范围内**。

---

## 13. 验收标准（修订）

- [ ] **Open `DialogDebug.unity` → Play**，无需 InitScene、无需 Tools 进场景。
- [ ] Inspector **拖入** `Village_KenMuNiStar_Test.prefab` 后，自动或手动触发，**NormalDialogue 字幕 UI** 正常，三句与 CSV 一致。
- [ ] 立绘随说话人切换；带 **Multiple Choice** 的 prefab 选项与分支正确。
- [ ] 对话结束后可 **再次 Play / 按 T 重播**，无卡死。
- [ ] Console 无 `StoryComponentGSM 未就绪`、无 GF 场景路径错误。
- [ ] **未**修改 `StoryComponentGSM`、`NormalDialogueFormNewLogic` 核心逻辑（沙盒新脚本除外）。
- [ ] 更换 Inspector 中 `dialoguePrefab` 为另一 prefab，**无需改代码**即可测下一条。

---

## 14. 验证清单（给施工员自测）

1. Prefab 已 Bind Graph，Actor（雅尔/古莎）已绑。  
2. **仅 Open DialogDebug → Play**（确认 Build Settings 里当前 Play Mode 启动场景可以是 DialogDebug，或手动 Open 后 Play）。  
3. 拖 prefab → 看字幕/立绘/跳过。  
4. 播完重播；换另一个 prefab 拖入再 Play。  
5. （可选）含 `LoadScene` Action 的图在沙盒中报错属预期，回正式场景测。

---

## 15. 明确不做（禁止）

- 不把 DialogDebug 再做成 **必须走 GF 换场** 的正式关卡场景。
- 不修改 `StoryComponentGSM`、`NormalDialogueFormNewLogic`、`DialogueTMPUGUI` **核心逻辑**（除非沙盒 NRE 且需 null 守卫，单独立项）。
- 不修改村庄移动、主线 SceneManager。
- 不把 CSV 导入改成「一键可运行 Prefab」。

---

## 16. 待确认

1. 沙盒 UI 用 **完整** NormalDialogue 工具栏还是 **精简** 仅字幕+选项？（建议首版精简）  
2. `GameManager` 用 `DontDestroyOnLoad` 还是仅本场景有效？（建议仅本场景，避免污染其它 Open Scene 测试）  
3. 旧版 `DialogDebugSceneManager` / Enter 菜单是 **删除** 还是 **保留 Deprecated**？  
4. 是否需要「场景内多槽位」同时摆多个 prefab 对比？（首版单槽拖引用即可）

---

## 17. 附录 A — GF 耦合方案（归档，非目标）

以下为初版文档内容，**仅作正式游戏若将来需「从主线进入 Debug 场景」时参考**，**不是**当前 DialogDebug 测试目标：

- `SceneName.DialogDebug` + `SceneAssetPath` 方案 A  
- `DialogDebugSceneManager : BaseGameSceneManager`  
- `DialogDebugStoryTester` + `StoryComponentGSM.TriggerStory(字符串)`  
- `Tools/Dialogue/Enter DialogDebug Scene`  

---

## 18. 相关文档与代码索引

| 主题 | 路径 |
|------|------|
| 任务卡 | `Assets/Doc/任务卡/0525/搭建DialogDebug对话预制体测试场景.md` |
| 字幕 UI | `Assets/Scripts/Game/GameRuntime/NodeCanvas/NodeCanvasExtend/DialogueTMPUGUI.cs` |
| UI prefab 来源 | `Assets/GameRes/Prefabs/UI/NormalDialogueNewPanel.prefab` |
| 正式剧情触发（对照） | `Assets/Scripts/Game/GameRuntime/GameSceneManager/Component/StoryComponentGSM.cs` |
| 正式 Form（对照） | `Assets/Scripts/Game/GameRuntime/UI/FormLogic/Story/NormalDialogueFormNewLogic.cs` |
| GameManager 语言 | `Assets/Scripts/Game/GameMgr/GameManager.cs` |
| CSV 工具链 | `Assets/Doc/执行文档/0525/CSV转NodeCanvas对话树_架构溯源与执行说明.md` |
| 测试 prefab | `Assets/GameRes/Prefabs/Dialogue/Village_KenMuNiStar_Test.prefab` |
| 旧 GF 菜单（待废弃） | `Assets/Editor/Tool/Dialogue/DialogDebugSceneMenu.cs` |
| 旧 Tester（待替换） | `Assets/Scripts/Game/GameRuntime/Story/DialogDebugStoryTester.cs` |

---

## 19. 文档修订

| 日期 | 说明 |
|------|------|
| 2026-05-25 | 初版：GF 场景管线 + Tools 进场景方案。 |
| 2026-05-25 | **修订**：实测不符合「拖 prefab 即测」预期；改为解耦沙盒方案（`DialogDebugPlayground` + 场景内 UI + 最小 GameManager）；GF 方案降入附录 A。 |

**文档路径**：`Assets/Doc/执行文档/0525/DialogDebug对话测试场景_架构溯源与执行说明.md`
