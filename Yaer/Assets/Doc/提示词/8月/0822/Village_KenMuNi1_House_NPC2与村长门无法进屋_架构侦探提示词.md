# Cursor Agent Prompt · Village_KenMuNi1：House_NPC2 / 村长门无法进屋

> **角色**：先【架构侦探】只读溯源两扇户外门的换场断点，报告拍板后【施工员】修复  
> **日期**：2026-08-22  
> **村场景**：`Assets/GameRes/Scenes/Village_KenMuNi1.unity`  
> **目标室内**：`Village_HomeScene2.unity`、`Village_Chief_House.unity`  
> **用户 Hierarchy（截图）**：`House_Chlef`（选中）、`House4 (6)`、`House_NPC2`  
> **产品现象**：两扇门 **无法进入** 对应室内；怀疑场景管理器或 **名称没对上**  
> **本阶段**：只读

把下面「侦探」整段复制给 Cursor Agent（Agent Mode）执行。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 产品需求（白话）

> `Village_KenMuNi1` 里 **House_NPC2** 进不了 `Village_HomeScene2`；**村长门**（Hierarchy 名 **`House_Chlef`**，疑为 Chief 拼写）进不了 `Village_Chief_House`。是 GSM 没写好还是场景名对不上？

### 换场通则（钉死）

```
户外 Stairs 门（SceneChangeDoor + Interactive 按 E）
  → NextSceneName = 目标场景名字符串（与 .unity 文件名 / Build / SceneName 一致）
  → LoadScene
  → 目标场景 GSM nowSceneName = LastSceneName 写入值
  → 双侧 EnterPosConfig：lastScene 字符串必须完全一致
```

**门物体名（House_NPC2）≠ 场景名（Village_HomeScene2）**——只要 `NextSceneName` 对即可；但 **GSM / EnterPos / Build** 必须三位一体。

### 磁盘预扫：两扇门对比表（2026-08-22 晚）

| 检查项 | **House_NPC2 → HomeScene2** | **村长门 → Chief_House** |
|--------|----------------------------|---------------------------|
| 村里物体是否在 YAML | ✅ `House_NPC2`，`NextSceneName: Village_HomeScene2` | ❌ **`House_Chlef` / `Village_Chief_House` 在 KenMuNi1 YAML 中未搜到**（可能仅编辑器未保存） |
| `sceneObjs` 登记 | ✅ fileID `1235839371` 在 GSM 列表 | ❓ 待核实（磁盘无门则未登记） |
| 目标在 `SceneName.cs` | ✅ `Village_HomeScene2` | ❌ **无 `Village_Chief_House` 常量** |
| Build Settings | ✅ `Village_HomeScene2.unity` | ✅ `Village_Chief_House.unity` |
| 目标 SceneManager | ✅ `Village_HomeScene2SceneManager`（guid `b2c3d4e5…`） | ❌ 仍挂 **`ForestSceneManager`**（guid `ed5ec3a1…`） |
| 目标 `EnterPos` 进村 | ✅ `lastScene: Village_KenMuNi1` → `EnterFrom_Village` | ❌ 仅 `HomeScene1` / `ForestEastScene`，**无 `Village_KenMuNi1`** |
| 村 `EnterPos` 出屋回村 | ✅ `Village_HomeScene2` → `ExitFrom_HomeScene2` | ❌ **无 `Village_Chief_House` 行** |
| 样板 | `House_Npc1` ↔ `Village_HomeScene1` | 需新建整条链 |

**预扫推论**：

- **HomeScene2**：户外 `NextSceneName` **已对**；若仍进不去，优先查 **交互链**（E 提示 / `SceneChangeDoor` 初始化 / `InteractiveComponent`），而非再改场景名字符串。室内 0606 文档中的缺口 **部分已修**（GSM/EnterPos/LeftDoor），侦探须 **证伪现网**。
- **Chief_House**：属于 **半成品场景**——错挂 `ForestSceneManager`、无 `SceneName`、村侧 **可能根本没有门配置**；`House_Chlef` 拼写也需在报告中裁定是否改名 `House_Chief`。

### 开发者截图注意

- `House_Chlef`：**Chief 拼写错误**风险（与 `Village_Chief_House` 不一致，仅影响辨认，不直接等于 NextSceneName）。
- `House4 (6)`：**勿混**——磁盘上多个 `House4` 实例 `NextSceneName: Village_House4`（23 号屋链），**不是**本需求两扇门。

### 侦探须逐门填的「换场七件套」

| # | 检查项 |
|---|--------|
| 1 | GO Active |
| 2 | `SceneChangeDoor` Enabled |
| 3 | `NextSceneName`（与 Build 场景名一致） |
| 4 | `TriggerWhenMoveIn`（户外门通常 **0**，按 E） |
| 5 | `InteractiveComponent` + `componentsList` + Collider |
| 6 | `SceneEntity` 在 `SceneEntityComponentGSM.sceneObjs` |
| 7 | Play：靠近是否出 **E**；Console `[SceneChangeDoor]` |

### 室内侧额外检查（进屋后黑屏/秒退也算失败）

| 场景 | 必查 |
|------|------|
| `Village_HomeScene2` | `nowSceneName`；`HouseDoor` 或 `LeftDoor` 回村；`EnterPos` |
| `Village_Chief_House` | **须新建** `Village_Chief_HouseSceneManager`？；替换 `ForestSceneManager`；`EnterPos` 双侧 |

### 须比较的方案（Chief 屋）

| 方案 | 说明 |
|------|------|
| **A（推荐）** | 对齐 `Village_HomeScene1`：补 `SceneName` + 专用 GSM/Config + 村门 `NextSceneName` + 双侧 `EnterPos` + `ExitFrom_HomeSceneChief` |
| B | 临时复用 `Village_House4` 场景 | ❌ 产品要 Chief_House |
| C | 只改门 `NextSceneName` 不改 GSM | ❌ 进屋必黑屏/落点错 |

### 严禁

- 把 `House_NPC2` 的 `NextSceneName` 改成 `HomeScene2`（龙宫旧名，与 `Village_HomeScene2` 不同）  
- 把村长门指到 `Village_House4`  
- 未保存场景就断定「磁盘无 House_Chlef」而不提醒用户 **Ctrl+S**  
- 只改户外门不改室内 `ForestSceneManager`

---

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/02_SYSTEM_SPEC.md
@Assets/Doc/技术文档/场景相关/场景切换.md
@Assets/Doc/执行文档/6月/0606/Village_HomeScene2_House_NPC2进村换场_施工执行说明.md
@Assets/Doc/执行文档/6月/0608/Village_HomeScene2_HouseDoor换场Village_KenMuNi1_架构溯源与施工执行说明.md
@Assets/Doc/执行文档/0820/Village_HomeScene3改名45与进屋黑屏_架构溯源报告.md
@Assets/Scripts/Game/Static/Name/Res/SceneName.cs
@Assets/Scripts/Game/GameRuntime/Entities/SceneEntities/CommonEntity/SceneChangeDoor.cs
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Scene/Village_House/Village_HomeScene2SceneManager.cs
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Scene/Village_House/Village_HomeScene1SceneManager.cs
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Scene/Forest/ForestSceneManager.cs
@Assets/Prefabs/Stairs.prefab
@Assets/GameRes/Scenes/Village_KenMuNi1.unity
@Assets/GameRes/Scenes/Village_HomeScene2.unity
@Assets/GameRes/Scenes/Village_Chief_House.unity
@Assets/GameRes/Scenes/Village_HomeScene1.unity
@ProjectSettings/EditorBuildSettings.asset

你现在是【架构侦探】。Unity 2020.3.48f1。
禁止改场景/代码。只读 + 写溯源报告；Play 复现步骤写清。

---

## 背景

村里两扇户门进不了室内。用户怀疑 GSM 或名称不一致。要对拍 **能用的 House_Npc1 → HomeScene1** 样板，分别诊断 **House_NPC2** 与 **村长门（House_Chlef）**。

---

## 侦探任务清单

### A. 确认村里目标物体（先_save 再读 YAML）

| 物体名（Hierarchy） | 磁盘是否存在 | 父节点 | 预制体 |
|--------------------|-------------|--------|--------|
| `House_NPC2` | | `Objects`? | Stairs? |
| `House_Chlef` / 村长门 | | | |
| 误选 `House4 (6)` | 是否 Village_House4 | | |

### B. 门 A：`House_NPC2` → `Village_HomeScene2`

**户外七件套表** + **Play**：E 提示？`[SceneChangeDoor] Enter`？LoadScene 失败日志？

**室内现网（证伪 0606 旧缺口）**：

| 项 | 现网 |
|----|------|
| SceneManager 脚本 | |
| `nowSceneName` | |
| `EnterPos` `Village_KenMuNi1` | |
| 出门 `HouseDoor` / `LeftDoor` `Next` | |
| Config `.asset` | |

**村侧回程**：`EnterPos` `Village_HomeScene2` → `ExitFrom_HomeScene2`

**根因裁定**：交互断 / 名错 / GSM / 已修仅未测 / 其它

### C. 门 B：村长门 → `Village_Chief_House`

**户外**：`NextSceneName` 是否为空或错名？是否在 `sceneObjs`？

**室内现网（重点）**：

| 项 | 现网 | 应有（对齐 HomeScene1） |
|----|------|-------------------------|
| SceneManager | `ForestSceneManager`? | **专用 Chief GSM** |
| `SceneName.cs` | 无常量? | **`Village_Chief_House`** |
| `EnterPos` | `HomeScene1`? | **`Village_KenMuNi1`** |
| Config | Forest? | 室内 Config |
| Build | | |

**村侧**：是否缺 `ExitFrom_HomeSceneChief` + EnterPos 行

**命名**：`House_Chlef` 是否应改为 `House_Chief`（建议写入报告，非阻塞 NextSceneName）

### D. 样板对拍 `House_Npc1` → `Village_HomeScene1`（必填）

| 层级 | Npc1 样板 | NPC2 | Chief |
|------|-----------|------|-------|
| NextSceneName | | | |
| sceneObjs | | | |
| 室内 GSM | | | |
| 双侧 EnterPos | | | |

### E. 推荐施工（最小改动，分两扇门）

**HomeScene2**：仅列 **仍断** 的项（可能 0～N 步）  
**Chief_House**：完整清单（SceneName、GSM、Config、门、EnterPos、ExitFrom、Build 复核）

### F. 验收清单

| # | 操作 | 通过 |
|---|------|------|
| 1 | 村 `House_NPC2` 按 E | 进 `Village_HomeScene2`，门口落点合理 |
| 2 | 室内出门 | 回村 `ExitFrom_HomeScene2` 外 |
| 3 | 村村长门按 E | 进 `Village_Chief_House` |
| 4 | 酋长家出门 | 回村门口对称 |
| 5 | Console | 无 `SceneChangeDoor` 缺组件 / LoadScene 失败 |
| 6 | `[VillageHomeScene2Debug]` / Chief Debug | `lastScene=Village_KenMuNi1` |

### G. 开放问题

`OPEN_QUESTIONS.md`「KenMuNi1 两户门换场 · 2026-08-22」（Chief 是否改名 House_Chief、是否复用 Stairs 预制体）

---

## 输出要求

写入：`Assets/Doc/执行文档/0822/Village_KenMuNi1_House_NPC2与村长门无法进屋_架构溯源报告.md`

结构：① 两扇门结论各一句 ② 根因（名/GSM/交互） ③ 用户验收 ④ 七件套表 + 施工步骤

MASTER 四段式口头汇报。
```

---

## 施工员续跑（侦探报告拍板后再贴）

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0822/Village_KenMuNi1_House_NPC2与村长门无法进屋_架构溯源报告.md
@Assets/Doc/技术文档/场景相关/场景切换.md
@Assets/GameRes/Scenes/Village_KenMuNi1.unity
@Assets/GameRes/Scenes/Village_HomeScene2.unity
@Assets/GameRes/Scenes/Village_Chief_House.unity

你现在是【施工员】。按报告修复两扇户外门进屋链路。

必须遵守：
- NextSceneName 用 `Village_HomeScene2` / `Village_Chief_House`（与 Build、SceneName 一致）；
- Chief 屋：替换 ForestSceneManager，新建专用 GSM+Config+SceneName 常量（若报告要求）；
- 双侧 EnterPos 配对；户外门进 sceneObjs；
- Stairs 门：TriggerWhenMoveIn=0，Interactive 链齐全；
- 对齐 House_Npc1 / 0606 HomeScene2 文档；最小 diff；
- 施工前确认村场景已保存（House_Chlef 在磁盘）。

提交说明：每扇门改了什么、进屋/出屋验收、Console 过滤结果。
```
