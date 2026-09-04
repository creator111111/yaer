# Village_HomeScene2 — NPC_埃吉尔 对话交互 — 施工执行说明

**施工员交付（待实施）** | 日期：2026-06-07  
**依据**：
- `Assets/Doc/00_MASTER_PROMPT.md`【施工员】最小化修改、先可运行
- 架构侦探：`Assets/Doc/执行文档/0601/Village_HomeScene23_NPC对话配置_执行说明.md`
- 换场前置：`Assets/Doc/执行文档/0606/Village_HomeScene2_House_NPC2进村换场_施工执行说明.md`
- 埃吉尔正式台本（后续替换用）：`Assets/Doc/执行文档/0607/Village_HomeScene2_埃吉尔接任务对白台本_架构溯源与执行说明.md`

**目标场景**：`Assets/GameRes/Scenes/Village_HomeScene2.unity`  
**Unity 版本**：2020.3.48f1  

---

## 1. 目标（一句话）

在 **`Village_HomeScene2` 屋内新增/配置 `NPC_埃吉尔`**，交互方式与现有 **`Npc1` 完全一致**：玩家靠近出现 **E** 提示，按 **E** 播放对话；**首版对话预制体暂用 `HomeScene1Npc1`**，埃吉尔专属 prefab 做好后只改 `StoryPrefabName` 即可替换。

---

## 2. 验收标准（你提出的）

| # | 操作 | 通过标准 |
|---|------|----------|
| Q1 | 从 **`InitScene`** 进游戏，经 `House_NPC2` 进入 `Village_HomeScene2` | 能正常进屋，无加载失败 |
| Q2 | 走到 **`NPC_埃吉尔`** 身旁 | 出现 **E** 交互提示 |
| Q3 | 按 **E** | 弹出对话 UI，播放 **`HomeScene1Npc1`** 剧情（龙宫 Npc1 占位台词） |
| Q4 | Console | **无** `加载资源失败: .../Dialogue/HomeScene1Npc1.prefab` |
| Q5 | Console | **无** `GetFirstCanTouchEntiy result=null`（贴身按 E 时） |
| Q6 | 对话结束后 | 可再次靠近按 E 重复触发（`SingleUseInArchive` 未勾选） |

> **不测项（本阶段不做）**：埃吉尔正式 CSV 台本、接任务 `QuestAcceptAction`、立绘替换——见 §9 后续替换。

---

## 3. 现状（静态阅读 2026-06-07）

### 3.1 场景里已有什么

| 物体 | Hierarchy 路径 | 状态 |
|------|----------------|------|
| **`Npc1`** | `SceneManager → Object → Npc1` | ✅ 已配 `SimpleStoryTrigger`，`StoryPrefabName = HomeScene1Npc1`，**可作复制模板** |
| **`NPC_埃吉尔`** | — | ❌ **场景中尚不存在**（需新建或从 `Npc1` 复制） |
| **`Object`（objRoot）** | `SceneManager` 下 | ✅ `SceneEntityComponentGSM.objRoot` 已指向此处 |
| **`SceneEntityComponentGSM`** | `SceneManager → Entity` | ⚠️ `sceneObjs` 仅登记了 **1 个**实体（`Npc1`），新 NPC **必须追加登记** |
| **场景管理器** | `Village_HomeScene2SceneManager` | ✅ 已挂；`isCanTouchWithOther = true`（允许与 NPC 交互） |
| **占位对话 prefab** | `Assets/GameRes/Prefabs/Dialogue/HomeScene1Npc1.prefab` | ✅ 在 `Dialogue/` **根目录**，与 `TriggerStory` 路径规则一致 |

### 3.2 与 `Npc1` 对齐的组件清单（复制后逐项核对）

```
NPC_埃吉尔（根）
├── SpriteRenderer / Animator          ← 埃吉尔外观（可先沿用复制体，后换图）
├── SceneEntity                        ← 须在 sceneObjs 登记
├── SimpleStoryTrigger                 ← StoryPrefabName = HomeScene1Npc1（首版）
├── EntityComponentSystem
├── BaseEntityControll                 ← entityType=NPC, canTouchWithPlayer=true
├── Components/
│   └── InteractiveComponent           ← 指向 Body 碰撞盒
└── Clds/
    └── Body/
        ├── BoxCollider2D (Is Trigger) ← 交互感应区
        └── RaycastListener            ← 须在 raycastListeners 列表
```

---

## 4. 原理（按 E 为什么会出对话）

生活类比：NPC 是「带感应区的自动售货机」，**E 键**是投币口。

```
玩家走进 NPC 的 Body 碰撞盒（与玩家碰撞盒重叠）
  → 屏幕出现 E 提示
  → 按 E（InputComponentGSM.OnEKeyPressed）
  → GetFirstCanTouchEntiy 找最近可交互体
  → InteractiveComponent.OnInteractive()
  → SimpleStoryTrigger.onClickInteractiveEvent
  → StoryComponentGSM.TriggerStory("HomeScene1Npc1")
  → 加载 GameRes/Prefabs/Dialogue/HomeScene1Npc1.prefab
  → 对话 UI 播放
```

**注意**：`SimpleStoryTrigger` 的 `Trigger Type` 在 Inspector 里显示为 **Click**，但村庄场景里 **Click = 按 E 交互**，不是鼠标左键。

---

## 5. Unity 施工步骤（推荐：复制 `Npc1`）

### 5.1 复制并命名

1. 打开 **`Village_HomeScene2.unity`**。  
2. Hierarchy：`SceneManager → **Object** → **Npc1`**。  
3. **Duplicate**（Ctrl+D）→ 重命名为 **`NPC_埃吉尔`**。  
4. 把 Transform 摆到屋内埃吉尔站立位置（**不要与 `Npc1` 叠在一起**）。  
5. 确认根物体 **Position Z = 0**（村庄交互体约定）。

### 5.2 配置 `SimpleStoryTrigger`（首版占位）

选中 **`NPC_埃吉尔`** 根物体，Inspector → **Simple Story Trigger**：

| 字段 | 首版值 | 说明 |
|------|--------|------|
| **Story Prefab Name** | `HomeScene1Npc1` | 与 `Npc1` 相同，**不含** `.prefab` |
| **Trigger Type** | `Click` | 与 `Npc1` 一致（按 E 触发） |
| **Single Use In Archive** | **不勾选** | 方便反复测试 |
| **Stay Time To Trigger Story** | `0` | 仅 Stay 模式用，保持 0 |

### 5.3 核对 `BaseEntityControll`

| 字段 | 期望值 |
|------|--------|
| **Entity Type** | `NPC` |
| **Can Touch With Player** | ✅ 勾选 |
| **Interactive Component** | 指向 `Components/InteractiveComponent` |
| **Scene Entity** | 指向根上的 `SceneEntity` |

### 5.4 核对 `InteractiveComponent`（点不到 / 没 E 时查）

路径：`NPC_埃吉尔 → Components → InteractiveComponent`

| 字段 | 期望值 |
|------|--------|
| **Interactive Collider** | `Clds/Body` 上的 **BoxCollider2D** |
| **Raycast Listeners** | 含 `Body` 上的 **Listener** 组件（与 `Npc1` 一致） |
| **Entity Controll** | 指向根 `BaseEntityControll` |
| **Scene Entity** | 指向根 `SceneEntity` |

`Body` 上 **BoxCollider2D**：**Is Trigger = 勾选**，尺寸能罩住角色半身～全身。

### 5.5 【关键】登记 `sceneObjs`

1. 选中 **`SceneManager → Entity`**（挂 `SceneEntityComponentGSM` 的物体）。  
2. Inspector → **Scene Objs** 列表 → **Add**。  
3. 拖入 **`NPC_埃吉尔`** 根物体上的 **`SceneEntity`** 组件。  
4. 保存场景。

> **不登记的后果**：`GetAllSceneEntities()` 找不到该 NPC，按 E 时 `closestComponent=null`，**永远点不到**。这是屋内 NPC 最常见踩坑。

### 5.6 换埃吉尔立绘（可选，不阻塞验收）

| 做法 | 说明 |
|------|------|
| **A（推荐）** | 只改 `NPC_埃吉尔` 根上 **SpriteRenderer.sprite** / **Animator** |
| **B** | 从美术 prefab 拖 Sprite 参考，**不要**只拖纯美术 prefab 到场景根（会缺交互组件） |

### 5.7 保存

**Ctrl+S** 保存 `Village_HomeScene2.unity`。

---

## 6. 替代方案说明

| 方案 | 适用 | 风险 |
|------|------|------|
| **§5：复制 `Npc1`**（推荐） | 首版最快验收 | 需改位置、登记 sceneObjs |
| 已有 `NPC_埃吉尔` 仅美术 | 场景里已摆好人 | 须**手动补** §3.2 全部组件，易漏 `sceneObjs` |
| 改 `Npc1` 当埃吉尔 | 屋里只要一个 NPC | 失去 `Npc1` 模板；不推荐 |
| 新建空物体从零挂组件 | 熟悉结构后 | 引用链易断，**不如复制** |

---

## 7. 验收步骤（Play Mode）

**必须从 `InitScene` 启动**（不要单独 Open `Village_HomeScene2` 再 Play）。

1. Play → 进村 → `House_NPC2` 按 E 进屋。  
2. 走到 **`NPC_埃吉尔`**，确认 **E** 提示。  
3. 按 **E**，对话 UI 弹出，字幕正常（内容为龙宫 Npc1 占位，属预期）。  
4. 关对话后再按 E，可再次触发。  
5. 看 Console：无加载失败、无 `result=null`。

### 7.1 故障排查

| 现象 | 优先检查 |
|------|----------|
| 没有 E 提示 | `canTouchWithPlayer`；Body 碰撞盒大小；Z=0；是否走进碰撞区 |
| 有 E 但按了没反应 | `sceneObjs` 是否登记；`isCanTouchWithOther`；是否从 InitScene 进 |
| `加载资源失败: HomeScene1Npc1` | prefab 是否在 `GameRes/Prefabs/Dialogue/` 根目录；AB/Resource Editor 是否登记 |
| `GetFirstCanTouchEntiy result=null` | `sceneObjs` 未登记或 `InteractiveComponent` 引用断链 |
| 对话播了没字 | prefab 内 Graph 是否 Bind；Actor 是否绑定（占位 prefab 一般已配好） |
| 误触发门而不是 NPC | 两物体碰撞区重叠；调 priority 或拉开摆放距离 |

---

## 8. 本任务改动范围

| 类型 | 路径 | 改动 |
|------|------|------|
| 场景 | `Assets/GameRes/Scenes/Village_HomeScene2.unity` | 新增/配置 `NPC_埃吉尔`；`sceneObjs` +1 |
| 对话 prefab | `HomeScene1Npc1.prefab` | **首版不改**（直接引用） |
| C# 脚本 | — | **本阶段不改** |

---

## 9. 后续替换正式埃吉尔对话（你做完 prefab 之后）

正式资源见台本文档与 CSV：`Assets/Dialog/Village_HomeScene2_Aegir_QuestOffer.csv`。

| 步骤 | 操作 |
|------|------|
| 1 | CSV 导入 → 合并 prefab → 保存为 **`Assets/GameRes/Prefabs/Dialogue/Village_HomeScene2_Aegir_QuestOffer.prefab`** |
| 2 | （可选）DialogDebug 试播 |
| 3 | 选中 `NPC_埃吉尔` → `SimpleStoryTrigger` → **Story Prefab Name** 改为 `Village_HomeScene2_Aegir_QuestOffer` |
| 4 | 从 InitScene 进屋复测 §2 验收表 |

**只改一个字段即可切换**，无需动交互碰撞与 `sceneObjs`。

---

## 10. 相关文档

| 主题 | 文档 |
|------|------|
| NPC 三件套原理 | `0601/Village_HomeScene23_NPC对话配置_执行说明.md` |
| 进村换场 | `0606/Village_HomeScene2_House_NPC2进村换场_施工执行说明.md` |
| 埃吉尔正式台本 | `0607/Village_HomeScene2_埃吉尔接任务对白台本_架构溯源与执行说明.md` |
| 对话试播 | `0525/DialogDebug对话测试场景_施工执行说明.md` |
| E 键交互链 | `0512/场景切换与对话触发跳转_架构溯源报告.md` §3 |

---

## 11. 文档修订

| 日期 | 说明 |
|------|------|
| 2026-06-07 | 初版：NPC_埃吉尔 按 E 对话；首版占位 `HomeScene1Npc1`；复制 `Npc1` + 登记 sceneObjs |

**文档路径**：`Assets/Doc/执行文档/0607/Village_HomeScene2_NPC埃吉尔对话交互_施工执行说明.md`
