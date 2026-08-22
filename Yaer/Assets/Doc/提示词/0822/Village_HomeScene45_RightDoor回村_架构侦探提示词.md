# Cursor Agent Prompt · Village_HomeScene45：RightDoor 回村出口配置

> **角色**：先【架构侦探】对拍现网，报告拍板后【施工员】改门与 EnterPos  
> **日期**：2026-08-22  
> **场景**：`Assets/GameRes/Scenes/Village_HomeScene45.unity`（开发者已更新物体位置，须以**当前 YAML**为准）  
> **产品拍板（2026-08-22，开发者）**：**主出口 = `Map/MapRight/RightDoor`**，走进回 **`Village_KenMuNi1`**（`House_Npc45` 门外落点）  
> **本阶段侦探**：只读、不改场景 / 代码  
> **先例**：`0821/Village_HomeScene45_LeftDoor无法退出`（当时左门半成品、右门 `SceneChangeDoor` 禁用——**须证伪现网是否已变**）

把下面「侦探」整段复制给 Cursor Agent（Agent Mode）执行。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 产品需求（开发者原话）

> 这个场景又改了，要走 **RightDoor** 回村子。场景中物体位置我已更新。

### Hierarchy（开发者截图，2026-08-22）

```
Village_HomeScene45*
  Map
    MapLeft / LeftDoor
    MapRight / RightDoor    ← 主出口（本期）
    LeftBorn / RightBorn
  SceneManager
```

**勿混**：`Village_HomeScene23` 未加载；样板可对拍 **23 右门出门**，但改的是 **45** 场景文件。

### 换场通则（钉死定义）

| 概念 | 说明 |
|------|------|
| 触发 | `SceneChangeDoor` + `TriggerWhenMoveIn=1` 走进 Trigger；须 **组件 Enabled** 且 **Interactive 链齐全** |
| 目标场景 | `NextSceneName = Village_KenMuNi1`（与 Build / `SceneName` 常量一致） |
| 进屋落点 | 目标场景 `EnterPosConfig`：`lastScene == 来源场景的 nowSceneName`（`Village_HomeScene45`） |
| 出屋落点（从村再进屋） | 室内 `EnterPosConfig`：`lastScene == Village_KenMuNi1` → 通常绑 **`RightBorn`** |
| `bornPos` / Born 空物体 | **不会**自动当落点；只供拖 EnterPos 引用 |

完整链：

```
走进 RightDoor Trigger
  → SceneChangeDoor.EnterDoor
  → LoadScene(Village_KenMuNi1)
  → LastSceneName = Village_HomeScene45
  → 村 GSM 匹配 EnterPos：lastScene=Village_HomeScene45 → House_Npc45 门外
```

### 可玩民居样板（主出口惯例）

| 场景 | 主出口 | 左门 | 右门 |
|------|--------|------|------|
| `Village_HomeScene23` | **RightDoor** | 常 **禁用** SceneChangeDoor / GO 关 | 启用 → `Village_KenMuNi1` |
| `Village_HomeScene1` | **RightDoor** | 禁用 | 启用 → 村 |
| **`Village_HomeScene45`（本期）** | **RightDoor**（已拍板） | 侦探须裁定：禁用或清空 Next，**避免双出口** | 启用并可走 |

### 0821 → 现网预扫疑点（开发者改布局后须重验）

| 门 | 0821 结论 | 磁盘预扫（施工前侦探再读一遍） |
|----|-----------|-------------------------------|
| **RightDoor** | `NextSceneName=Village_KenMuNi1` ✅；**`SceneChangeDoor.m_Enabled: 0`** ❌；Interactive 链齐 | **主因仍是组件被禁用** |
| **LeftDoor** | 0821 缺 Interactive；后序可能已补 | 现网也有 `Next=Village_KenMuNi1`、`Trigger=1`、有 Interactive — **若两扇都启用会双出口** |
| **室内 EnterPos** | `Village_KenMuNi1` → `RightBorn` ✅；残留 `ForestScene` | 可施工时清 Forest 残留 |
| **村 EnterPos** | `Village_HomeScene45` → `House_Npc45` 门外 ✅ | 进屋/出屋配对应已齐 |
| **碰撞体位置** | 开发者已挪场景 | **须对拍**：RightDoor BoxCollider2D 是否仍盖住玩家可走区域（改布局后常见 Trigger 偏了） |

### 生活类比

屋里有两扇门，右门已经挂了「回村子」的牌子，但 **门锁电路被关了**（`SceneChangeDoor` 禁用）；左门也可能挂着同样牌子。产品说 **只走右门**——要把右门锁打开，左门最好焊死或撕牌子，免得玩家走错。

### 严禁

- 右门再指 `ForestScene`  
- 只改 `NextSceneName` 不启用 `SceneChangeDoor`  
- 改龙宫 / HomeScene23 场景  
- 用 `bornPos` 当落点而不配 EnterPos  
- 不验碰撞体就宣称「已能出门」（布局已改）

---

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/02_SYSTEM_SPEC.md
@Assets/Doc/OPEN_QUESTIONS.md
@Assets/Doc/技术文档/场景相关/场景切换.md
@Assets/Doc/技术文档/场景相关/Village_HomeScene23_可玩民居室内_技术说明.md
@Assets/Doc/执行文档/0821/Village_HomeScene45_LeftDoor无法退出_架构溯源报告.md
@Assets/Doc/执行文档/0820/Village_HomeScene3改名45与进屋黑屏_架构溯源报告.md
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Scene/Village_House/Village_HomeScene45SceneManager.cs
@Assets/Scripts/Game/Static/Name/Res/SceneName.cs
@Assets/GameRes/Scenes/Village_HomeScene45.unity
@Assets/GameRes/Scenes/Village_KenMuNi1.unity
@Assets/GameRes/Config/SceneManagerConfig/Village_HomeScene45.asset

你现在是【架构侦探】。Unity 2020.3.48f1。
禁止修改场景、代码、Config。只读扫描 + 写溯源报告。

---

## 背景

45 号村民家布局已调整。产品定：**从 RightDoor 走回村**。要摸清现网右门还差什么、左门要不要关、EnterPos 是否配对、碰撞是否盖住通道。

---

## 侦探任务清单

### A. 双门现网对拍表（必填）

| 检查项 | LeftDoor | RightDoor |
|--------|----------|-----------|
| GameObject Active | | |
| SceneChangeDoor Enabled | | |
| NextSceneName | | |
| TriggerWhenMoveIn | | |
| BoxCollider2D IsTrigger + 尺寸/Offset | | |
| componentsList 有 Interactive | | |
| InteractiveColliderListener | | |

### B. 主出口裁定（已拍板 RightDoor）

- 现网离「能走 RightDoor 回村」差几步？（预期：启用 SceneChangeDoor + 确认 Trigger 位置）
- 左门建议：**禁用 GO / 禁用 SceneChangeDoor / 清空 Next** 选哪一种（对齐 HomeScene23）

### C. EnterPos 双侧配对

| 场景 | lastScene | pos Transform | 现网 |
|------|-----------|---------------|------|
| `Village_HomeScene45` | `Village_KenMuNi1` | RightBorn（进屋从村来） | |
| `Village_KenMuNi1` | `Village_HomeScene45` | House_Npc45 门外 | |
| 残留 `ForestScene` 等 | 是否删 | | |

### D. 场景管理器 / 三位一体

- `Village_HomeScene45SceneManager.nowSceneName`  
- Build 含本场景  
- `House_Npc45.NextSceneName`  
- 与 0821 diff：哪些已修、哪些仍断

### E. 布局更新后的碰撞验收点

- RightDoor Trigger 与 `MapRight/RightWall`、地面 Walk 区域是否对齐（开发者已挪物体）  
- Scene 视图 Gizmos：玩家从屋内走向右侧能否进入 Trigger

### F. 推荐施工方案（最小改动）

1. **启用** RightDoor 的 `SceneChangeDoor`  
2. 确认 `NextSceneName=Village_KenMuNi1`、`TriggerWhenMoveIn=1`  
3. **关闭** LeftDoor 换场（对齐 23：Disable 组件或 GO）  
4. 室内 EnterPos：`Village_KenMuNi1` → `RightBorn`；清 `ForestScene` 残留（可选）  
5. **不改** `Village_HomeScene45SceneManager.cs`（除非 nowSceneName 错）  
6. 若 Trigger 偏了：只调 RightDoor Collider Offset/Size，勿改 Map 逻辑脚本

### G. 验收清单

| # | 操作 | 通过 |
|---|------|------|
| 1 | 村 `House_Npc45` 进屋 | 不黑屏，落点合理 |
| 2 | 屋内走至 **RightDoor** | 换场到 `Village_KenMuNi1` |
| 3 | 落点 | `House_Npc45` 门外（非森林、非错位） |
| 4 | 再走 **LeftDoor** | **不应**换场（或 GO 不可达） |
| 5 | 再进屋 | `lastScene=村` 时落在 RightBorn 一侧合理 |
| 6 | Console | 无 LoadScene 失败 / NRE |

### H. 开放问题

追加 `OPEN_QUESTIONS.md`「Village_HomeScene45 RightDoor 回村 · 2026-08-22」；产品主出口已拍板，只记技术例外（如 Collider 要美术改门洞）。

---

## 输出要求

写入：`Assets/Doc/执行文档/0822/Village_HomeScene45_RightDoor回村_架构溯源报告.md`

报告结构：① 结论一句话 ② 原因 ③ 用户验收清单 ④ 双门 YAML 证据 + EnterPos 表 + 最小施工步骤

MASTER 四段式口头汇报。
```

---

## 施工员续跑（侦探报告拍板后再贴）

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0822/Village_HomeScene45_RightDoor回村_架构溯源报告.md
@Assets/Doc/技术文档/场景相关/场景切换.md
@Assets/GameRes/Scenes/Village_HomeScene45.unity

你现在是【施工员】。按报告使 **RightDoor** 成为唯一主出口，回 **Village_KenMuNi1**。

必须遵守：
- 启用 RightDoor 的 SceneChangeDoor；NextSceneName=Village_KenMuNi1；TriggerWhenMoveIn=1；
- 左门按报告禁用（对齐 HomeScene23），避免双出口；
- 双侧 EnterPos 配对；禁止 RightDoor 指 ForestScene；
- 布局改过后若走不进 Trigger，只调 RightDoor 碰撞体，不改换场代码；
- 不改龙宫 / HomeScene23 / Village_HomeScene45SceneManager（除非报告明确要求修 nowSceneName）。

提交说明：修了哪扇门、LeftDoor 如何处理、回村落点、走进 RightDoor 是否换场。
```
