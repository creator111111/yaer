# Cursor Agent Prompt · Village_KenMuNi1：`House_Tree` 交互播 `Village_TreeHouseLock`

> **角色**：先【架构侦探】只读溯源，报告拍板后再【施工员】  
> **日期**：2026-08-30  
> **场景**：`Assets/GameRes/Scenes/Village_KenMuNi1.unity`  
> **交互物体（用户 Hierarchy 红箭头）**：`Objects` → **`House_Tree`**（Prefab 实例，蓝立方图标）  
> **对白资产（产品指定）**：`Assets/GameRes/Prefabs/Dialogue/Village_TreeHouseLock.prefab`  
> **产品目标（白话）**：玩家与 **树屋（House_Tree）** 交互后，弹出对话——**和村里「物体交互」差不多**；对白用 **`Village_TreeHouseLock`**（预扫文案：雅尔「锁上了打不开」）  
> **关联**：`SimpleStoryTrigger` · `InteractiveComponent` · `RaycastListener` · 物品远程点击 · GSM `sceneObjs` · 代办「树屋门口的交互对话」  
> **本阶段**：只读；禁止改代码 / Prefab / 场景 / CSV  
> **报告落盘**：`Assets/Doc/执行文档/0830/Village_KenMuNi1_House_Tree交互Village_TreeHouseLock_架构溯源报告.md`

把下面「侦探」整段复制给 Cursor Agent（Agent Mode）执行。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 产品需求（钉死）

| 项 | 期望 |
|----|------|
| 物体 | `Village_KenMuNi1` / `Objects` / **`House_Tree`** |
| 交互 | 与现网 **物体交互** 同套路（点/可交互 → 播对白），不是进店、不是商店点胸 C6 |
| 对白 | **`Village_TreeHouseLock`**（故事名 = Prefab 文件名） |
| 可重复？ | 倾向 **可多次**（锁还在就能再说）；若产品要只播一次再写开放问题 |
| 本期不做 | 真的进屋 / LoadScene 树屋内部 / 商店 Chest C6 黑屏转树屋 |

### 现场 Hierarchy（用户截图）

```
Village_KenMuNi1
  Objects/
    House_Npc4 / House_Npc1 / House_Npc45 / House_NPC2 / House_Chief
    Door_Shop
    SetAutoMoveTrigger
    House4 (4)(5)(6)
    ★ House_Tree          ← 红箭头：要加「交互 → 对话」
    （另有灰显：StoneBrand / FallDownTreeCollider / Enemy …）
```

预扫：磁盘 `Village_KenMuNi1.unity` 文本检索未必立刻命中 `House_Tree`（可能未保存、Prefab 嵌套名、或 guid 实例）——**侦探必须在场景/Prefab 真源定位该物体**，写清路径与 fileID。

### 现网「物体交互」金样（复用，勿另起炉灶）

生活类比：面包/饼干/木箱 =「点一下展品播讲解」；树屋门 =「点一下门锁，雅儿说打不开」——铃还是 `SimpleStoryTrigger`，只换故事名。

| 层 | 样板（HomeScene / Item） | 树屋本期 |
|----|--------------------------|----------|
| 登记 | `SceneEntity` + GSM `sceneObjs` / objRoot 子树 | ✅ 须齐 |
| 交互 | `InteractiveComponent` + `Clds/Body`（Collider2D + `RaycastListener`） | ✅ 须齐 |
| 播对白 | `SimpleStoryTrigger`，`TriggerType=Click`，`StoryPrefabName`=对话 Prefab 名 | → **`Village_TreeHouseLock`** |
| 距离 | 物品常 `requirePlayerOverlap=0`（远程点）；NPC 常须靠近 | 用户说「和物体交互差不多」→ 倾向 **远程可点**（侦探对照同村 Door_Shop / 已有 Item 拍板） |
| 光标 | 部分物体挂 `CursorChangeTrigger`（View/Chat/Catch） | 开放：是否对齐村内可互动物 |

**禁止**：为树屋新建第二套 TriggerStory 管线；挂商店 `TryTriggerShopkeeperSpecial`；把本期做成切场景进屋。

### Prefab 对白预扫（须核实）

| 项 | 预扫 |
|----|------|
| 根名 | `Village_TreeHouseLock` ✅ |
| 句 | 雅尔：「**锁上了打不开**」（FaceType 预扫 12） |
| Actor | Yaer |
| 壳层 | 短句；是否缺 Fighting/UIAlpha？（对照近期 ShopChest「无框」教训——短对白也要确认 Panel 能出） |
| 进门 | ❌ 无 LoadScene（符合「只说话」） |

### 现网缺口假说

```
House_Tree（现网？）
  → 可能仅装饰 Prefab / 无 Interactive / 无 SimpleStoryTrigger / 未进 sceneObjs
  → 点了没反应

目标：
  House_Tree
    → SceneEntity + Interactive 三件套 + SimpleStoryTrigger(Village_TreeHouseLock)
    → （可选）远程点击 Flag
    → 点中 → TriggerStory → 雅儿「锁上了打不开」
```

### 方案倾向（侦探拍板）

| 方案 | 做法 | 倾向 |
|------|------|------|
| **A · 在 House_Tree 上补物体交互三件套** | 仿面包/NpcChair；`StoryPrefabName=Village_TreeHouseLock` | **✅ 推荐** |
| B · 旁挂空壳交互子物体 | 视觉 Mesh 不动，子物体吃点击 | 仅当根 Prefab 不宜改组件时 |
| C · GSM 硬编码点树屋 | C# 特判 | ❌ |
| D · 进范围自动播 Enter | 非「交互」语义 | ❌ 除非产品改口 |

### 与「商店点胸 C6 树屋」边界

| 线 | 是什么 | 本期 |
|----|--------|------|
| **House_Tree + TreeHouseLock** | 村里树屋外观/门口，锁了打不开的**吐槽对白** | ✅ **本期** |
| ShopChest C6+ | 店内点胸后黑屏切树屋场景 | ❌ **不做**、勿混文档 |

### 本期边界

| 做 | 不做 |
|----|------|
| ✅ House_Tree → 播 TreeHouseLock 闭环设计 | ❌ 解锁进树屋 / 新 Scene |
| ✅ Prefab 可播性（有无对话框壳） | ❌ 改对白文案（除非缺句导致播不出） |
| ✅ GSM 登记 / Layer / 远程点击拍板 | ❌ 商店特殊对白管线 |
| ✅ 最小施工清单 | ❌ 与 House_Npc* 进屋逻辑绑死 |

### 严禁（本阶段）

- 改代码 / Prefab / 场景 / CSV  
- 把 TreeHouseLock 接到 Door_Shop 或商店 Chest  
- 未核实 House_Tree 组件就断言「已经能交互」  
- 用放大碰撞冒充「物体交互」却不走 Interactive + StoryTrigger  

### 须对拍资产

| 资产 | 用途 |
|------|------|
| `Village_KenMuNi1.unity` · `Objects/House_Tree` | 交互挂点真源 |
| `Village_TreeHouseLock.prefab` | 对白真源 |
| `SimpleStoryTrigger.cs` / `RaycastListener.cs` / `InteractiveComponent.cs` | 物体交互管线 |
| HomeScene 面包/饼干 或 NpcChair | 三件套样板 |
| `Door_Shop`（同 Objects 下） | 对照：门是换景还是对话；树屋勿误做成门 |
| `Village_KenMuNiSceneManager` | sceneObjs / objRoot |
| `0820/物品交互对话_远程点击触发_…` | 远程点物品先例 |
| 代办 canvas「树屋门口的交互对话」 | 产品意图对齐 |

---

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/02_SYSTEM_SPEC.md
@Assets/Doc/执行文档/0820/物品交互对话_远程点击触发_架构溯源报告.md
@Assets/Doc/执行文档/0822/Village_HomeScene45_面包饼干Item替换与GSM绑定_架构溯源报告.md
@Assets/Doc/执行文档/0820/Village_HomeScene1_Object全量配置与GSM绑定_架构溯源报告.md
@Assets/GameRes/Scenes/Village_KenMuNi1.unity
@Assets/GameRes/Prefabs/Dialogue/Village_TreeHouseLock.prefab
@Assets/Scripts/Game/GameRuntime/Entities/SceneEntities/CommonEntity/SimpleStoryTrigger.cs
@Assets/Scripts/Game/GameRuntime/Entities/Component/Interactive/RaycastListener.cs
@Assets/Scripts/Game/GameRuntime/Entities/Component/Interactive/InteractiveComponent.cs
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Scene/Village_KenMuNi/Village_KenMuNiSceneManager.cs

你现在是【架构侦探】。Unity 2020.3.48f1 / C#。
禁止修改任何代码、Prefab、场景、CSV。只读扫描 + 写「House_Tree → Village_TreeHouseLock」溯源报告。

---

## 背景（策划白话）

1. 村里 `House_Tree`（树屋）要能交互：玩家点了之后出对话。  
2. 对话用现成 Prefab **`Village_TreeHouseLock`**（锁上了打不开）。  
3. 做法对齐现网 **物体交互**（Interactive + SimpleStoryTrigger），不要新造系统。  
4. 本阶段只摸清：House_Tree 现在缺什么组件、故事名怎么挂、要不要远程点击、对白 Prefab 能否直接播。

---

## 侦探任务清单

### A. 定位 House_Tree
Hierarchy / 场景 YAML / 源 Prefab：完整路径、是否 Prefab 实例、现有组件表（SceneEntity / Interactive / Collider / StoryTrigger / Cursor）。  
若磁盘暂无该名：写清可能原因（未保存等）与在编辑器应检查的位置。

### B. 钉死 TreeHouseLock Prefab
根名、Actor、文案、Face、有无 UIAlpha/Fighting 壳、能否 `TriggerStory("Village_TreeHouseLock")`。  
对照「无对话框」类风险：短句是否也要最小壳层。

### C. 对照物体交互金样
选 1 个同工程样板（面包 / NpcChair / 村内已互动物）：组件差异表 → House_Tree 缺什么。

### D. 接线方案拍板
推荐 A：三件套 + `StoryPrefabName=Village_TreeHouseLock`。  
拍板：`requirePlayerOverlap` 开/关；是否挂悬停光标；`SingleUseInArchive` 否（默认可重复）。  
GSM：`sceneObjs` / Layer（常 21）是否要登记。

### E. 与 Door_Shop / 进屋门区分
House_Tree **不是** `SceneChangeDoor`；点了只播锁对白，不 LoadScene。

### F. 最小施工清单（本阶段不执行）

| # | 动作 | 优先级 |
|---|------|--------|
| 1 | House_Tree 补 SceneEntity + Interactive + Body/RaycastListener | **P0** |
| 2 | SimpleStoryTrigger → `Village_TreeHouseLock`，Click | **P0** |
| 3 | 远程点击 Flag 按拍板 | **P0** |
| 4 | GSM sceneObjs / Layer 登记 | **P0** |
| 5 | Prefab 对话框壳（若缺） | P0/P1 |
| 6 | 悬停光标（可选对齐物体） | P1 |
| 7 | 进树屋场景 | ❌ |

### G. 验收清单

| # | 操作 | 通过 |
|---|------|------|
| 1 | 村内靠近/点击 House_Tree（按拍板距离） | 播 **Village_TreeHouseLock** |
| 2 | 听到/看到 | 雅尔「锁上了打不开」；**对话框可见** |
| 3 | 结束后 | 可继续逛村；再点可再播（若拍板可重复） |
| 4 | Console | `TriggerStory Village_TreeHouseLock`；无 Missing Prefab |
| 5 | 回归 | Door_Shop 仍进店；其它 House_* 进屋逻辑不坏 |

### H. 开放问题
- 锁对白是否永远可重复，还是钥匙任务后改对白/禁用？  
- 点击区是整棵树屋碰撞还是只门口小盒？  
- 悬停用 View 还是 Chat 光标？  

---

## 输出要求

写入：`Assets/Doc/执行文档/0830/Village_KenMuNi1_House_Tree交互Village_TreeHouseLock_架构溯源报告.md`

MASTER 四段式：  
① 结论（挂点方案 + Story 名 + 距离策略）  
② 原因（现网缺什么；为何走物体交互而非门/商店）  
③ 用户检查清单（怎么点树屋验收）  
④ 给程序：组件差异表 + Prefab 表 + 最小 diff + 开放问题
```

---

## 施工员续跑（侦探报告拍板后贴）

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0830/Village_KenMuNi1_House_Tree交互Village_TreeHouseLock_架构溯源报告.md
@Assets/GameRes/Scenes/Village_KenMuNi1.unity
@Assets/GameRes/Prefabs/Dialogue/Village_TreeHouseLock.prefab
@Assets/Scripts/Game/GameRuntime/Entities/SceneEntities/CommonEntity/SimpleStoryTrigger.cs

你现在是【施工员】。按报告让 House_Tree 交互后播 Village_TreeHouseLock。

必须遵守：
- 复用物体交互三件套（SceneEntity + Interactive + RaycastListener + SimpleStoryTrigger）；
- StoryPrefabName 与 Prefab 文件名一致：Village_TreeHouseLock；
- 不要做成 SceneChangeDoor 进屋；不要接商店 Special / Chest C6；
- 远程点击与可重复性按报告拍板；
- 代码/场景配置含必要注释；重要取舍写清原因。

提交说明：改了哪些组件/登记、如何验收、未做项。
```
