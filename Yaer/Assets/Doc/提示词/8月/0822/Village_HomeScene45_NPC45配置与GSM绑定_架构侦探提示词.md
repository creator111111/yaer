# Cursor Agent Prompt · Village_HomeScene45：新增 NPC45 配置 + 场景管理器绑定

> **角色**：先【架构侦探】盘点缺口，报告拍板后【施工员】配置场景与对话资源  
> **日期**：2026-08-22  
> **目标场景**：`Assets/GameRes/Scenes/Village_HomeScene45.unity`  
> **现场（开发者 Hierarchy）**：`Object` 下已有 `Npc1`；**新加 `NPC45`**（目前为空壳，无 `Components` / `Clds` 子节点）  
> **对话台本（已有）**：`Assets/Dialog/Village_NPC45_对话交互.csv`（Speaker：`4` / `5` / `雅`）  
> **本阶段侦探**：只读、不改场景 / Prefab / 代码  
> **依赖**：`0820` 村民家 NPC 配置范式、`0821` HomeScene45 专用 Manager 已落地

把下面「侦探」整段复制给 Cursor Agent（Agent Mode）执行。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 产品需求（开发者原话）

> 场景里面新增 NPC45，帮我配置一下它，顺便改一下场景管理器。

拆解为三件事：

| # | 任务 | 说明 |
|---|------|------|
| 1 | **配 NPC45 实体** | 仿同场景 `Npc1`，补交互三件套，能走近出 E、能播对白 |
| 2 | **绑场景管理器** | `Entity` 上 `SceneEntityComponentGSM`：`objRoot=Object`；NPC45 须在 `Object` 下且挂 `SceneEntity`（运行时会重扫；YAML `sceneObjs` 建议同步登记） |
| 3 | **对话资源** | CSV 已存在；须确认是否已导入为 `GameRes/Prefabs/Dialogue/Village_NPC45.prefab`（或工程约定名），`StoryPrefabName` 与磁盘 **逐字一致** |

### 「改场景管理器」指什么（常见误解）

| 对象 | 要不要改 | 说明 |
|------|----------|------|
| `Village_HomeScene45SceneManager.cs` | **通常不改** | 无 per-NPC 逻辑；`nowSceneName` / `PlaceName.KenMuNi` / 室内脚步已齐 |
| `SceneManager` 物体上的 **GSM 脚本** | **可能要** | `Map` 组件里 `sceneEntityComponentGSM` 已指 `Entity`；重点在 **Object 下实体是否齐** |
| `Entity` → `SceneEntityComponentGSM` | **要核对** | `objRoot` → `Object`；`sceneObjs` 现网仅 `Npc1` 的 `SceneEntity`，**缺 NPC45** |
| `Village_HomeScene45.asset` Config | **通常不改** | `isFightingScene:0` 已正确 |

生活类比：场景管理器是「住户登记表」——不是改登记表格式，而是把 **NPC45 登记进表**，并给他装门铃（碰撞 + 对话组件）。

### 现网预扫（磁盘 YAML，侦探须对拍开发者是否已保存场景）

| 项 | 现网 `Village_HomeScene45.unity` |
|----|----------------------------------|
| `Object` 子物体 | 仅 **`Npc1`**（`NPC45` **未出现在磁盘**，可能仅 Editor 未保存） |
| `Npc1.StoryPrefabName` | `HomeScene1Npc1`（龙宫拷贝残留；**本期可不动 Npc1**，专注 NPC45） |
| `Entity.sceneObjs` | 仅 1 条（Npc1 的 `SceneEntity`） |
| `Npc1` 根 Position Z | `0`（✅ 符合村庄交互约定） |
| 对话 Prefab | **无** `Village_NPC45.prefab` / `Village_Npc45.prefab`（仅 CSV） |
| Speaker 4/5 映射 | `0820` 报告要求 Import 映射 `4→NPC4`、`5→NPC5`；侦探须核实 Import 器是否已补 |

### 样板：同场景 `Npc1` 应具备的结构（施工照抄）

```
Object/Npc1
├── SpriteRenderer（立绘）
├── SceneEntity
├── SimpleStoryTrigger（StoryPrefabName = 对话 Prefab 名）
├── ComponentSystemMono（componentsList → InteractiveComponent）
├── BaseEntityControll（canTouchWithPlayer=1, entityType=3）
├── Components/
│   └── InteractiveComponent（interactiveCollider → Clds/Body）
└── Clds/
    └── Body（BoxCollider2D IsTrigger + RaycastListener + Priority）
```

**村庄 NPC 硬约定**（见 `0820` Npc1 无 E 报告）：

- 根 Transform **Z = 0**（否则 `Bounds.Intersects` 永假、无 E）
- `requirePlayerOverlap = true`（近距 NPC，**不是**远程物品）
- `raycastListeners` 含 Body 上 Listener
- `StoryPrefabName` 勿写龙宫 `HomeScene1Npc*`

### StoryPrefabName 建议（侦探裁定，须与磁盘 Prefab 一致）

| 物体 | CSV | 建议 Prefab / StoryPrefabName |
|------|-----|------------------------------|
| NPC45 | `Village_NPC45_对话交互.csv` | **`Village_NPC45`**（Import 后路径 `Assets/GameRes/Prefabs/Dialogue/Village_NPC45.prefab`） |

若 Import 工具产出名不同，以 **Import 日志 + 磁盘实际文件名** 为准，写入报告映射表。

### 严禁

- 改龙宫 `HomeScene1` / `HomeScene1Manager`
- 把 NPC45 做成远程物品（勿勾「忽略距离」类开关）
- 只拖 Hierarchy 不挂 `SceneEntity` / `InteractiveComponent`
- 改 `Map/Design/村民家3合层` 美术层当交互体（交互在 `Object` 下）

---

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/02_SYSTEM_SPEC.md
@Assets/Doc/OPEN_QUESTIONS.md
@Assets/Doc/执行文档/0820/Village_HomeScene1_Npc1无E对照HomeScene23_架构溯源报告.md
@Assets/Doc/执行文档/0820/Village_HomeScene1_Object全量配置与GSM绑定_架构溯源报告.md
@Assets/Doc/执行文档/0821/Village_HomeScene45_LeftDoor无法退出_架构溯源报告.md
@Assets/Doc/执行文档/0820/CSV导入_Speaker1与4与5映射缺失_架构溯源报告.md
@Assets/Doc/执行文档/6月/0607/Village_HomeScene2_NPC埃吉尔对话交互_施工执行说明.md
@Assets/Doc/技术文档/场景相关/Village_HomeScene23_可玩民居室内_技术说明.md
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Component/SceneEntityComponentGSM.cs
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Scene/Village_House/Village_HomeScene45SceneManager.cs
@Assets/GameRes/Scenes/Village_HomeScene45.unity
@Assets/GameRes/Config/SceneManagerConfig/Village_HomeScene45.asset
@Assets/Dialog/Village_NPC45_对话交互.csv
@Assets/GameRes/Prefabs/Dialogue

你现在是【架构侦探】。Unity 2020.3.48f1 / C#。
禁止修改场景、Prefab、代码、CSV。只读扫描 + 写溯源报告。

---

## 背景（策划白话）

45 号村民家屋里新加了一个 NPC（Hierarchy 名 `NPC45`），要能像 `Npc1` 一样走近对话。对话 CSV 已写好，但场景里 NPC 还是空壳，场景管理器也没登记他。

---

## 侦探任务清单

### A. 确认开发者是否已保存场景

- 磁盘 YAML 有无 `NPC45` / `Npc45` GameObject
- 若无：报告写明「须先 Ctrl+S 保存场景再施工」

### B. NPC45 现网盘点表

| 检查项 | 期望 | 现网 |
|--------|------|------|
| 在 `Object` 下 | 是 | |
| 有 `SceneEntity` | 是 | |
| 有 `SimpleStoryTrigger` | 是 | |
| 有 `InteractiveComponent` + Body 碰撞 | 是 | |
| 根 Z = 0 | 是 | |
| `StoryPrefabName` | `Village_NPC45`（或裁定名） | |
| Layer | 与 Npc1 一致（21） | |

### C. 场景管理器 / GSM 绑定

- `Entity` → `SceneEntityComponentGSM.objRoot` 是否仍指 `Object`
- `sceneObjs` 是否含 NPC45 的 `SceneEntity`（运行时会 `GetComponentsInChildren` 重扫，但 YAML 漏登记要记）
- `Map.sceneEntityComponentGSM` 引用链是否断
- **`Village_HomeScene45SceneManager.cs` 是否需要改**（预期：否，写理由）

### D. 对话资源链

- CSV → Import 是否已跑；`Village_NPC45.prefab` 是否存在
- Speaker `4`/`5`/`雅` 在 Import 映射表与 Prefab `actorParameters` 是否齐
- `SimpleStoryTrigger` 的 `StoryPrefabName` 与 Prefab 名是否逐字一致

### E. 推荐施工方案（最小改动）

1. Unity 内 **Duplicate `Npc1`** → 改名 `NPC45` → 换立绘 Sprite / 站位  
2. 改 `StoryPrefabName` → `Village_NPC45`  
3. 若无 Prefab：按工程 CSV Import 流程生成并保存到 `GameRes/Prefabs/Dialogue/`  
4. `Entity.sceneObjs` 追加 NPC45（或保存场景后依赖重扫，二选一写清）  
5. **不改** `Village_HomeScene45SceneManager.cs`（除非发现缺模块）

### F. 验收清单

| # | 操作 | 通过 |
|---|------|------|
| 1 | 村 `House_Npc45` 进屋 | 不黑屏 |
| 2 | 走近 NPC45 | 出 **E** |
| 3 | 按 E | 播 CSV 对白（Speaker 4/5/雅 立绘正常） |
| 4 | Console | 无「未注册」/ Dialogue 加载失败 |
| 5 | 左/右门出屋 | 仍正常（勿回归 0821） |

### G. 开放问题

写入 `OPEN_QUESTIONS.md`「Village_HomeScene45 NPC45 · 2026-08-22」（仅技术项：如 NPC45 用哪张立绘、是否替换 Npc1 的 `HomeScene1Npc1` 等）。

---

## 输出要求

写入：`Assets/Doc/执行文档/0822/Village_HomeScene45_NPC45配置与GSM绑定_架构溯源报告.md`

报告结构：① 结论一句话 ② 原因（登记表没登记 / 三件套缺失） ③ 用户检查清单 ④ 最小文件列表 + StoryPrefabName 映射表

口头汇报用 MASTER 四段式。
```

---

## 施工员续跑（侦探报告拍板后再贴）

> **前置**：开发者已在 Unity 保存场景（磁盘能看见 `NPC45`）。若只有 Editor 未保存，先保存再施工。

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0822/Village_HomeScene45_NPC45配置与GSM绑定_架构溯源报告.md
@Assets/GameRes/Scenes/Village_HomeScene45.unity
@Assets/Dialog/Village_NPC45_对话交互.csv
@Assets/GameRes/Prefabs/Dialogue

你现在是【施工员】。按溯源报告做**最小化修改**，使 Village_HomeScene45 内 NPC45 可走近对话。

必须遵守：
- 仿同场景 Npc1 结构（SceneEntity + Interactive + SimpleStoryTrigger + Clds/Body）；
- 根 Transform Z=0；近距 NPC（requirePlayerOverlap=true）；
- StoryPrefabName 与对话 Prefab 磁盘名逐字一致（建议 Village_NPC45）；
- 若无 Prefab：先 CSV Import 生成，再绑 StoryPrefabName；
- Entity/SceneEntityComponentGSM：objRoot=Object；sceneObjs 含 NPC45（或确认重扫可覆盖）；
- **不要**改 Village_HomeScene45SceneManager.cs（除非报告明确要求）；
- **不要**破坏 0821 左右门出屋链路。

每次提交说明：改了哪些文件、StoryPrefabName、如何验收走近按 E。
```
