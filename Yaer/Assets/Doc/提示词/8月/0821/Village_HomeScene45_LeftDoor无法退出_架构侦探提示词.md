# Cursor Agent Prompt · Village_HomeScene45：玩家怎么离开 + LeftDoor 走不出去

> **角色**：【架构侦探】只溯源、不改代码  
> **日期**：2026-08-21  
> **场景**：`Assets/GameRes/Scenes/Village_HomeScene45.unity`（Hierarchy 已加载；`Village_HomeScene23` 未加载，勿混）  
> **测试现象（开发者已测）**：人在 **`Village_HomeScene45` 室内**，走进 **`Map/MapLeft/LeftDoor` 无法退出**（无换场 / 仍停在屋里）。  
> **产品问题**：现在这间屋**设计上 / 现网上**玩家该怎么离开？左门为什么走不通？  
> **本阶段**：只读扫描 + 写溯源报告，**不施工**  
> **先例**：`0820/Village_HomeScene3改名45与进屋黑屏`（改名前快照：左门禁用、右门曾指 `ForestScene`——**须用现网 YAML 证伪，勿当仍成立**）

把下面整段复制给 Cursor Agent（Agent Mode）执行。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 开发者问题（白话）

1. 人已经进了 **`Village_HomeScene45`**。  
2. 走到 **LeftDoor**，**出不去**。  
3. 想先搞清楚：**现在这间屋到底该从哪扇门离开、离开去哪、现网为什么左门没反应。**

### Hierarchy（开发者截图，2026-08-21）

```
Village_HomeScene45*
  Object
  Map
    Design / 村民家3合层 / …     ← 合层美术，不是门
    DefaultBornPos
    MapLeft
      LeftWall
      LeftDoor                 ← 开发者走进这扇，出不去
    MapRight
      RightWall
      RightDoor                ← 另一扇标准门，须一并盘点
    Ground
    LeftBorn
    RightBorn
    EnvironmentShadowCamera
    GroundColliders / Ground1
  SceneManager
  Camera
```

合层 `Design/村民家3合层` 是画，**真正换场物体是 `MapLeft/LeftDoor` / `MapRight/RightDoor`**，勿绑错。

### 换场通则（技术文档，侦探按代码/YAML 对拍）

门换场 = `SceneChangeDoor.NextSceneName` + 走进 Trigger（`TriggerWhenMoveIn`）或按 E  
落点 = **目标场景** `EnterPosConfig.lastScene` 匹配来源 `nowSceneName`（不是门上的 `bornPos`）  
室内回村目标通常是 **`Village_KenMuNi1`**，村里落在 **`House_Npc45` 门外**。

`SceneChangeDoor.bornPos`、`LeftBorn`/`RightBorn` **不会**被 `SetPlayerPos` 自动选用；Born 只是给 EnterPos 去拖的空物体。

### 可玩民居样板（两套，勿混）

| 屋 | 主出口 | 回村 |
|----|--------|------|
| `Village_HomeScene2` | 偏 **左 / HouseDoor** | `Village_KenMuNi1` |
| `Village_HomeScene23` | **右门 `RightDoor`**；左门常禁用防双出口 | `Village_KenMuNi1` |
| `Village_HomeScene1` | 施工后对齐可玩民居（侦探对拍现网哪扇启用） | `Village_KenMuNi1` |

**45 号屋主出口未在本期拍板。** 0820 建议对齐可玩民居、右门改指村、左门保持禁用或按样板补齐——开发者本次明确在测 **左门**，侦探必须分清：

- **现网配置**（哪扇门真能换场）  
- **样板惯例**（23 走右、2 走左）  
- **开发者期望**（想走 LeftDoor 离开）  

三者不一致时写入 OPEN，**不要擅自把「该走右门」写成已决议**。

### 0820 改名前快照（可证伪）

当时（文件还叫 `Village_HomeScene3`）室内：

| 项 | 当时 |
|----|------|
| LeftDoor | **禁用**、Next 空、`componentsList: []` |
| RightDoor | 启用，`NextSceneName=ForestScene`（飞森林，错） |
| 室内 EnterPos | 仅 `ForestScene` / `HomeScene2`，**无** `Village_KenMuNi1` |
| 村门 | `House_Npc45` → 缺失的 `Village_House4` |

磁盘现已有 `Village_HomeScene45.unity`、`Village_HomeScene45SceneManager`、`Village_HomeScene45.asset`——改名/专用 Manager **可能已施工**。侦探必须以 **当前场景 YAML + Manager + 村门** 为准，逐项写「0820 → 现网」diff，禁止直接复读旧表当现状。

### LeftDoor 走不出去：预扫嫌疑（须用证据钉死主因，可多因叠加）

| # | 嫌疑 | 生活类比 |
|---|------|----------|
| 1 | GameObject **未激活** / 父节点关 | 门焊死在墙上 |
| 2 | 无 `SceneChangeDoor` 或 `NextSceneName` 空 / 错名 | 门牌空白或写错地址 |
| 3 | `TriggerWhenMoveIn=false` 且缺按 E / Interactive | 走进去没人理，还得按门铃 |
| 4 | 无 Trigger Collider / Layer 对不上玩家 | 人穿过去，开关没碰到 |
| 5 | `componentsList` 空或缺 Interactive（0820 左门即此） | 门只有门框没有门锁电路 |
| 6 | 未进 `sceneObjs` / GSM 未 OnInit | 物业没登记这扇门 |
| 7 | `CheckNextSceneUnlock` 拦了 | 关卡锁还没开 |
| 8 | **产品就是右门出门**，左门故意禁用 | 走错了那扇装饰门 |
| 9 | Next 填了但目标不在 Build / 字符串 ≠ 文件名 | 地址对了派出所没这户 |
| 10 | 换场触发了但回村 EnterPos 错（会「离开」但落点怪） | 出了门却传送到别处——与「完全出不去」要分开写 |

开发者描述是 **走 LeftDoor 无法退出**，优先查 **没触发 LoadScene**；若其实切了场但像没出去，另行列「假阴性」。

### 必读

- `Assets/Doc/00_MASTER_PROMPT.md`
- `Assets/Doc/技术文档/场景相关/场景切换.md`
- `Assets/Doc/执行文档/5月/0518/SceneChangeDoor场景切换_架构溯源与执行说明.md`
- `Assets/Doc/执行文档/0820/Village_HomeScene3改名45与进屋黑屏_架构溯源报告.md`
- `Assets/Doc/执行文档/6月/0608/Village_HomeScene2_HouseDoor换场Village_KenMuNi1_架构溯源与施工执行说明.md`
- `Assets/Doc/提示词/0804/Village_HomeScene23_右门回村与Npc4对话组件_架构侦探提示词.md`（右门主链样板）
- `SceneChangeDoor.cs`、`LoadSceneComponentGSM`、`BaseGameSceneManager.SetPlayerPos`
- 场景：`Village_HomeScene45.unity`（LeftDoor / RightDoor / EnterPos / SceneManager）
- 对照：`Village_HomeScene2`、`Village_HomeScene23`、`Village_HomeScene1` 室内门 YAML
- 村：`Village_KenMuNi1` 的 `House_Npc45` + EnterPos `lastScene`

---

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Project_context.md
@Assets/Doc/02_SYSTEM_SPEC.md
@Assets/Doc/OPEN_QUESTIONS.md
@Assets/Doc/技术文档/场景相关/场景切换.md
@Assets/Doc/执行文档/5月/0518/SceneChangeDoor场景切换_架构溯源与执行说明.md
@Assets/Doc/执行文档/0820/Village_HomeScene3改名45与进屋黑屏_架构溯源报告.md
@Assets/Doc/执行文档/6月/0608/Village_HomeScene2_HouseDoor换场Village_KenMuNi1_架构溯源与施工执行说明.md
@Assets/Scripts/Game/GameRuntime/Entities/SceneEntities/CommonEntity/SceneChangeDoor.cs
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Component/LoadSceneComponentGSM.cs
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Scene/Village_House/Village_HomeScene45SceneManager.cs
@Assets/GameRes/Scenes/Village_HomeScene45.unity
@Assets/GameRes/Config/SceneManagerConfig/Village_HomeScene45.asset
@Assets/ProjectSettings/EditorBuildSettings.asset

你现在是【架构侦探】。Unity 2020.3.48f1 / C#。
禁止修改场景、Prefab、代码、Build。只读扫描 + 写溯源报告。

---

## 背景

1. 开发者人在 `Village_HomeScene45`，走进 `Map/MapLeft/LeftDoor` **无法离开该屋**。
2. 要先回答：现网玩家**怎么离开这间屋**（哪扇门、去哪、要不要按 E）。
3. 再钉死：LeftDoor 走不出去的**第一原因**（配置 / 组件 / 碰撞 / 产品就是不用左门）。
4. 0820 是改名前快照；现网已有专用 `Village_HomeScene45SceneManager`——必须重新扫 YAML，写 diff。
5. 主出口走左还是走右 **未拍板**；不一致记 OPEN，不要替策划改门。

---

## 必查

### A. 现网「离开 45 号屋」全链路（先画图再下结论）

从室内到村（或其它目标）画一条应然链，并填现网是否接通：

```
玩家走进 LeftDoor 或 RightDoor
  → SceneChangeDoor（Active / NextSceneName / TriggerWhenMoveIn / Unlock）
  → LoadScene(目标)
  → 目标场景 EnterPosConfig.lastScene == Village_HomeScene45
  → 落点 Transform（村里应是 House_Npc45 门外）
```

同时扫其它离开入口：对话 `LoadSceneTaskAction`、流程、地图 UI——有则列出，无则写「仅门」。

输出表：

| 出口 | Active | SceneChangeDoor | NextSceneName | TriggerWhenMoveIn | Collider/Layer | Interactive / componentsList | 在 sceneObjs | 走进会换场？ |
|------|--------|-----------------|---------------|-------------------|----------------|------------------------------|--------------|--------------|
| MapLeft/LeftDoor | | | | | | | | |
| MapRight/RightDoor | | | | | | | | |

### B. LeftDoor 走不出去（主因）

对 `LeftDoor` 逐项钉死，标 **主因 / 次因 / 无关**：

1. 物体或父节点是否 `m_IsActive: 0`。  
2. 是否挂 `SceneChangeDoor`；`NextSceneName` 是否空、`ForestScene`、旧 `Village_HomeScene3`、`Village_House4`、或正确 `Village_KenMuNi1`。  
3. `TriggerWhenMoveIn`：false 时走进是否本就不该换场；有无 E 提示链路。  
4. Trigger Collider、Layer 是否能碰到玩家。  
5. `ComponentSystemMono.componentsList` 是否空 / None；有无 Interactive。  
6. GSM `objRoot` / `sceneObjs` 是否包含此门；`isInit`。  
7. `CheckNextSceneUnlock` 是否绑定且会拒绝。  
8. 对照 `Village_HomeScene2` 的可走之门、`Village_HomeScene23` 的 LeftDoor（常禁用）——45 更像哪套。

结论必须回答：**走进 LeftDoor 时有没有调用 `LoadScene`。**  
- 没调用 → 卡在门配置/碰撞/禁用。  
- 调用了但失败 → Build / 场景名 / Unlock。  
- 调用成功但人像没出去 → EnterPos / 相机（与「无法退出」分开）。

开发者未贴本次 Console：写「静态最可能主因」+「进 Play 须看的日志」（LoadScene、Unlock、未注册、NRE）。

### C. 回程双侧 EnterPos（离开后会不会落错）

| 场景 | 应有 lastScene | 应有 pos | 现网 |
|------|----------------|----------|------|
| `Village_KenMuNi1` | `Village_HomeScene45` | `House_Npc45` 门外 | |
| `Village_HomeScene45` | `Village_KenMuNi1` | 建议门口 Born（左门主链→LeftBorn，右门主链→RightBorn） | |

残留 `Village_HomeScene3` / `Village_House4` / `ForestScene` 单独列表。  
`nowSceneName` 必须是 `Village_HomeScene45`，否则村里按 lastScene 对不上。

### D. 村门对拍（进屋是否对称）

`House_Npc45.NextSceneName` 是否已是 `Village_HomeScene45`。  
进屋能进、出门不能出 → 重点在室内门；两边都断 → 三位一体仍裂。

### E. 0820 → 现网 diff

至少对拍：Manager 脚本、Config、nowSceneName、Place、Build、左门、右门、双侧 EnterPos、村门 Next。  
写清哪些已修、哪些仍是「出不去」的锅。

### F. 产品 OPEN（只记录，不施工）

| ID | 问题 | 建议默认（待确认） |
|----|------|-------------------|
| Q1 | 45 号屋主出口是 LeftDoor 还是 RightDoor？ | 对拍 1/2/23 现网；开发者正在测左门，若要左门出门须把左门补成可玩门并处理右门（禁用或勿双出口） |
| Q2 | 走出后唯一目标是否 `Village_KenMuNi1`？ | 是（可玩民居惯例）；禁止再指 ForestScene |
| Q3 | 走进即走还是按 E？ | 对拍其它村民家室内门 |

### G. 验收（施工后，本期只写标准）

| # | 操作 | 通过 |
|---|------|------|
| 1 | 从村 `House_Npc45` 进 45 | 不黑屏 |
| 2 | 按报告指定的主出门离开 | 回到 `Village_KenMuNi1`，落在该屋门外 |
| 3 | 走 LeftDoor | 若主出口是左：换场；若产品禁用左：走进无换场且报告写明「非故障」 |
| 4 | 非主出口那扇 | 不误飞森林、不双门打架 |
| 5 | Console | 无门相关 NRE / 未注册 |

---

## 侦探任务

1. **结论一句话**：现在离开 45 号屋的现网路径是什么；LeftDoor 走不出去的主因是什么。  
2. **左右门现网对照表** + 与 HomeScene2 / 23 / 1 的差异。  
3. **因果**：走进左门为何不 LoadScene（或 Load 了为何像没出）。  
4. **0820 → 现网 diff**。  
5. **回村 EnterPos / 村门** 是否对称。  
6. **OPEN**：主出口左还是右。  
7. **禁止**：改资产；把 0820 旧表当现网；未拍板就宣布必须走右门或必须启用左门；动龙宫 `HomeScene1`、`Village_HomeScene23` 场景。

---

## 输出

写入：`Assets/Doc/执行文档/0821/Village_HomeScene45_LeftDoor无法退出_架构溯源报告.md`

① 结论一句话  
② 原因（生活类比：装饰门 vs 真门；门牌空白 vs 走错侧门）  
③ 用户检查清单（左门逐项勾；主出口待确认单独一行）  
④ 程序：YAML 字段证据、换场调用链、EnterPos 表、0820 diff、OPEN  

MASTER 四段式口头汇报。
```

---

## 施工员续跑（报告拍板后再贴）

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0821/Village_HomeScene45_LeftDoor无法退出_架构溯源报告.md
@Assets/Doc/技术文档/场景相关/场景切换.md
@Assets/Doc/OPEN_QUESTIONS.md
@Assets/GameRes/Scenes/Village_HomeScene45.unity

你现在是【施工员】。按已拍板的主出口，让玩家能从 Village_HomeScene45 离开并回到 Village_KenMuNi1。

必须：NextSceneName 与文件名/SceneName/nowSceneName 一致；双侧 EnterPos 配对；禁止右门再指 ForestScene；非主出口那扇按报告禁用或去交互，避免双门打架；不改龙宫与 HomeScene23。

若报告 OPEN 主出口未拍板：先停，只列「启用左门」与「改走右门」两套最小改动，等开发者选一套再改。

提交说明：现网原先哪扇能走、修了哪扇、回村落点、左门走进是否换场。
```
