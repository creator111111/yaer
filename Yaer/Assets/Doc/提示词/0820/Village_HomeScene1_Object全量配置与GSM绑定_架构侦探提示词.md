# Cursor Agent Prompt · Village_HomeScene1：Object 全量配置 + 场景管理器绑定

> **角色**：【架构侦探】只溯源、不改代码  
> **日期**：2026-08-20  
> **目标场景**：`Assets/GameRes/Scenes/Village_HomeScene1.unity`  
> **开发者要求**：该场景 Hierarchy 里 **`Object` 下全部物体都要配置好**，并与 **场景管理器绑定**（`SceneEntityComponentGSM` / `objRoot` / `sceneObjs`）。  
> **分类（已定）**：  
> - **`Npc1`** = **正常对话 NPC**（走近/近距点击，与现网 NPC 一致）  
> - **饼干 / 面包 / 土豆 / 木箱 / 木桶 / 米袋** = **互动物品**（远程鼠标点击播对白，**不要求走近**——见 0820 物品交互报告）  
> **对话 Prefab（工程已有，预扫）**：`Village_Npc1`、`Village_Npc1_bingan` / `_mianbao` / `_tudou` / `_muxiang` / `_muzhiyuantong` / `_huangmi`  
> **本阶段**：只读扫描 + 写配置溯源报告，**不施工**  
> **依赖**：`Assets/Doc/执行文档/0820/物品交互对话_远程点击触发_架构溯源报告.md`（远程点击判定；若未施工须写清「本场景配置卡在哪」）

把下面整段复制给 Cursor Agent（Agent Mode）执行。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 开发者问题（白话）

1. `Village_HomeScene1` 的 `Object` 里这些东西（Npc1 + 饼干面包土豆等）怎么配齐？  
2. 怎么和场景管理器绑定，进游戏才能点、能播？  
3. Npc1 和物品交互规则不一样：人正常对话，物品远处点就行。

### 现场 Hierarchy（开发者截图，红框 Object）

```
Village_HomeScene1
  … / Object
    饼干
    面包
    木桶
    米袋
    木箱
    土豆
    Npc1
  Map / Design / 村民家1合层 / …（美术层，一般不当交互实体）
```

对话资源名（工程内 Prefab，预扫应对）：

| Hierarchy 物体 | 建议 StoryPrefabName | 类型 |
|----------------|----------------------|------|
| Npc1 | `Village_Npc1` | 正常 NPC |
| 饼干 | `Village_Npc1_bingan` | 物品·远程 |
| 面包 | `Village_Npc1_mianbao` | 物品·远程 |
| 土豆 | `Village_Npc1_tudou` | 物品·远程 |
| 木箱 | `Village_Npc1_muxiang` | 物品·远程 |
| 木桶 | `Village_Npc1_muzhiyuantong` | 物品·远程 |
| 米袋 | `Village_Npc1_huangmi` | 物品·远程 |

（若场景还有其它 Object 子物体，侦探一并列入，勿漏。）

### 预扫结论方向（可证伪）

| 项 | 预判 |
|----|------|
| 「绑定场景管理器」 | = `SceneEntityComponentGSM.objRoot` 指向含这些物体的根（常见名 `Object`）；每个交互体有 `SceneEntity`；进 `sceneObjs`（运行时会按 objRoot **重扫**，但须确认挂对根、有 SceneEntity） |
| Manager | `Village_HomeScene1SceneManager`（勿与龙宫 `HomeScene1Manager` 混淆） |
| 每物体三件套 | `SceneEntity` + `InteractiveComponent`（+ Body/RaycastListener）+ `SimpleStoryTrigger`（或等价） |
| Npc1 | `TriggerType.Click`；**近距**（Listener **不**勾忽略距离）；`StoryPrefabName=Village_Npc1` |
| 物品 | 同上结构，但 Listener **须远程**（0820 报告方案 A 开关）；各自挂对 Prefab 名 |
| 风险 | 物体只有空壳/只有美术节点在 Map 下；StoryPrefabName 仍是旧 `HomeScene1Npc1`；未挂 SceneEntity；远程开关未施工导致远处点静默失败 |
| 非本期 | 改 Map 合层贴图；做任务系统；改龙宫 HomeScene1 |

### 必读

- `Assets/Doc/执行文档/0820/物品交互对话_远程点击触发_架构溯源报告.md`
- `Assets/Doc/执行文档/6月/0607/Village_HomeScene2_NPC埃吉尔对话交互_施工执行说明.md`（sceneObjs 登记）
- `Assets/Doc/执行文档/0804/Village_HomeScene1_进屋残缺对齐HomeScene4_架构溯源报告.md`
- `Assets/Doc/技术文档/场景相关/Village_HomeScene23_可玩民居室内_技术说明.md`（objRoot 重扫）
- `SceneEntityComponentGSM.cs`、`Village_HomeScene1SceneManager.cs`
- 场景：`Village_HomeScene1.unity`（Object 下每个物体 Inspector 对拍）
- Prefab：`Assets/GameRes/Prefabs/Dialogue/Village_Npc1*.prefab`

---

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Project_context.md
@Assets/Doc/02_SYSTEM_SPEC.md
@Assets/Doc/OPEN_QUESTIONS.md
@Assets/Doc/执行文档/0820/物品交互对话_远程点击触发_架构溯源报告.md
@Assets/Doc/执行文档/6月/0607/Village_HomeScene2_NPC埃吉尔对话交互_施工执行说明.md
@Assets/Doc/执行文档/0804/Village_HomeScene1_进屋残缺对齐HomeScene4_架构溯源报告.md
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Component/SceneEntityComponentGSM.cs
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Scene/Village_House/Village_HomeScene1SceneManager.cs
@Assets/GameRes/Scenes/Village_HomeScene1.unity
@Assets/GameRes/Prefabs/Dialogue/Village_Npc1.prefab
@Assets/GameRes/Prefabs/Dialogue/Village_Npc1_bingan.prefab
@Assets/GameRes/Prefabs/Dialogue/Village_Npc1_muxiang.prefab

你现在是【架构侦探】。Unity 2020.3.48f1 / C#。
禁止修改场景、Prefab、代码。只读扫描 + 写配置溯源报告。

---

## 背景

1. 开发者已在 `Village_HomeScene1` 的 `Object` 下摆好 Npc1 与饼干/面包/土豆/木箱/木桶/米袋。
2. 要求：全部配置完成，并与场景管理器绑定。
3. Npc1 = 正常 NPC 对话；其余 = 物品远程点击对话（依赖 0820 远程点击方案）。
4. 对话 Prefab 名称已基本齐套；本期查「场景侧缺什么、怎么挂、怎么验收」。

---

## 必查

### A. 场景管理器绑定（「和场景管理器绑定」钉死定义）

对拍 `Village_HomeScene1`：

| 检查项 | 应是什么 | 现网 |
|--------|----------|------|
| SceneManager 脚本 | `Village_HomeScene1SceneManager` | |
| `SceneEntityComponentGSM.objRoot` | 指向含 Npc1/物品的 `Object`（或等价） | |
| `sceneObjs` | 含全部交互 `SceneEntity`（或 OnInit 能从 objRoot 扫到） | |
| 各物体 | 有 `SceneEntity`；在 objRoot 子树内 | |
| 空槽 / None | 有无 fileID:0 脏项 | |

写清：只拖进 Hierarchy **不够**；缺 SceneEntity / 不在 objRoot 下会怎样。

### B. Object 全量子物体盘点表（必出）

对 **Object 下每一个** 子物体填：

| 物体名 | 有 SceneEntity？ | Interactive 三件套？ | RaycastListener？ | SimpleStoryTrigger？ | StoryPrefabName 现值 | 远程忽略距离？ | 结论（OK/缺什么） |
|--------|------------------|----------------------|-------------------|----------------------|----------------------|----------------|-------------------|

Npc1 与物品分栏标注期望差异。

### C. StoryPrefabName 映射裁定

用工程内 Prefab 名与 CSV 对拍，给出最终映射表（可微调拼音名，但须与磁盘 Prefab **逐字一致**）。

### D. Npc1 vs 物品配置差异清单

| 项 | Npc1 | 物品 |
|----|------|------|
| 走近要求 | 要（默认 Listener） | 不要（远程开关） |
| Prefab | Village_Npc1 | Village_Npc1_* |
| entityType / canTouchWithPlayer | 对拍样板 | 物品是否也要 E 提示？OPEN |
| 立绘 Actor | NPC1 + 雅尔 | 多半仅雅尔自语 |

若 0820 远程开关 **尚未进工程**：报告须写「场景可先配齐 Story 名与三件套，但远处点仍失败，直到 Listener 开关落地」。

### E. 与美术层边界

`Map/Design/村民家1合层` 下的桌/箱精灵 ≠ Object 下交互体。钉死：点击打的是 Object 碰撞，还是误打在合层上？交互 Collider 应对准可点区域。

### F. 验收清单（施工后）

| # | 操作 | 通过 |
|---|------|------|
| 1 | InitScene → House_Npc1 → 进 Village_HomeScene1 | 无 SceneEntity/Interactive 初始化异常 |
| 2 | 走近 Npc1 点/按 E | 播 `Village_Npc1` |
| 3 | 门口远处点饼干/面包/土豆/木箱/木桶/米袋 | 各播对应 Prefab；**无需走近** |
| 4 | 远处点 Npc1 | 仍不播（保持近距） |
| 5 | Console | 无加载 Dialogue 失败 |

---

## 侦探任务

1. **结论一句话**：Object 全量怎么配、怎么绑 GSM；Npc1 与物品差在哪。  
2. **绑定定义** + 现网对拍结果。  
3. **全量盘点表**（每个物体缺什么）。  
4. **StoryPrefabName 最终映射表**。  
5. **最小施工顺序**（先远程开关？先补组件？先登记 objRoot？）。  
6. OPEN：物品要不要 E 提示；Collider 尺寸；是否还有未列出的 Object 子物体。  
7. **禁止**：改资产；把 Map 合层当交互实体；改龙宫 HomeScene1；把 Npc1 改成远程。

---

## 输出

写入：`Assets/Doc/执行文档/0820/Village_HomeScene1_Object全量配置与GSM绑定_架构溯源报告.md`

① 结论一句话  
② 原因（生活类比：Object=货架上的按钮；GSM=总电源接线板，没插上按了没电）  
③ 用户检查清单（每个物体勾选表 + 绑定步骤）  
④ 程序：objRoot/sceneObjs、Npc1 vs 物品差异、Prefab 映射、与 0820 远程点击依赖、OPEN  

MASTER 四段式口头汇报。
```

---

## 施工员续跑（报告拍板后再贴）

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0820/Village_HomeScene1_Object全量配置与GSM绑定_架构溯源报告.md
@Assets/Doc/执行文档/0820/物品交互对话_远程点击触发_架构溯源报告.md
@Assets/Doc/OPEN_QUESTIONS.md
@Assets/GameRes/Scenes/Village_HomeScene1.unity

你现在是【施工员】。按报告配置 Village_HomeScene1 的 Object 全量交互体并绑定场景管理器。

必须：
- Npc1 = 正常近距对话，StoryPrefabName=Village_Npc1（或报告裁定名）
- 饼干/面包/土豆/木箱/木桶/米袋 = 物品远程点击，挂对 Village_Npc1_* Prefab
- objRoot / SceneEntity / sceneObjs 按报告登记
- 远程点击依赖若未落地，先按物品报告补 Listener 开关，再配场景
- 不改龙宫 HomeScene1；不改 Map 合层当交互体

提交说明：绑了哪些物体、StoryPrefabName 对照表、远处点物品与近距 Npc1 如何验收。
```
