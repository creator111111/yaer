# Cursor Agent Prompt · Village_HomeScene45：出屋回村落点修正（对齐 HomeScene1）

> **角色**：先【架构侦探】对拍样板与现网 EnterPos，报告拍板后【施工员】改村场景落点  
> **日期**：2026-08-22  
> **涉及场景**：  
> - 室内：`Assets/GameRes/Scenes/Village_HomeScene45.unity`（出门侧，通常已配 RightDoor）  
> - **主改**：`Assets/GameRes/Scenes/Village_KenMuNi1.unity`（回村落点）  
> **样板**：`Assets/GameRes/Scenes/Village_HomeScene1.unity` + 村里 `ExitFrom_HomeScene1`  
> **产品需求（开发者）**：从 45 号屋出村后，应落在 **进村时那扇门门口**（`House_Npc45` 外），和 1 号屋一样；**现网落点不对**。  
> **与 RightDoor 提示词关系**：`0822/Village_HomeScene45_RightDoor回村` 解决「能不能出门」；**本期解决「出门后落在哪」**。  
> **本阶段侦探**：只读、不改场景 / 代码

把下面「侦探」整段复制给 Cursor Agent（Agent Mode）执行。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 产品需求（白话）

> `Village_HomeScene45` 回村的位置不对，应该要在村里场景进这个房间的门口回来，和 `Village_HomeScene1` 一样。

拆解：

| # | 要求 | 说明 |
|---|------|------|
| 1 | **出屋 → 村** | 从室内 RightDoor（或已启用主出口）换场到 `Village_KenMuNi1` |
| 2 | **落点 = 进门处** | 站在 **`House_Npc45` 门外**，与从村按 E 进屋是同一户 |
| 3 | **对齐 HomeScene1** | 1 号屋用 **`ExitFrom_HomeScene1`** 专用空节点，不用通用 `LeftBorn` |
| 4 | **进村 → 屋** | 室内 `EnterPosConfig`：`lastScene=Village_KenMuNi1` → **`RightBorn`**（门口内侧） |

### 开发者 Hierarchy 截图（`Village_KenMuNi1`）

```
Map/
  DefaultBornPos, LeftBorn, RightBorn
  ExitFrom_HomeScene2
  EnterFrom_Village_OutSide, EnterFrom_Shop
  ExitFrom_HomeScene1          ← 1 号屋样板
  （无 ExitFrom_HomeScene45）  ← 红箭头指向 Objects/House_Npc45，缺专用落点
Objects/
  House_Npc4, House_Npc1, House_Npc45, Door_Shop
```

### 工程内「一户一门一 Exit」惯例（须对拍）

| 室内场景 | 村里出门落点空节点 | `KenMuNi1.EnterPosConfig` lastScene | pos 引用 |
|----------|-------------------|--------------------------------------|----------|
| `Village_HomeScene1` | **`ExitFrom_HomeScene1`** | `Village_HomeScene1` | `ExitFrom_HomeScene1`（约 x=45.5, y=-6.1） |
| `Village_HomeScene2` | **`ExitFrom_HomeScene2`** | `Village_HomeScene2` | `ExitFrom_HomeScene2`（约 x=-124.73, y=3.85） |
| **`Village_HomeScene45`** | **缺失** | `Village_HomeScene45` | **误绑 `LeftBorn`**（约 x=62.4, y=-6.1）❌ |

**磁盘预扫结论（施工前侦探再读 YAML）**：

- `Village_KenMuNi1` 已有 `EnterPosConfig` 行 `lastScene: Village_HomeScene45`，但 **pos 指向 `LeftBorn`**，与 `House_Npc45` 门位无关 → **落点错位根因**。
- 村里 **没有** `ExitFrom_HomeScene45` 空物体（对比 `ExitFrom_HomeScene1/2`）。
- `House_Npc45.NextSceneName = Village_HomeScene45` ✅（进村链通常已通）。
- 室内 `Village_HomeScene45`：`lastScene: Village_KenMuNi1` → **`RightBorn`**（约 x=-5.01, y=-3.65）✅，对齐 HomeScene1 进屋模式。

### 换场落点通则（钉死）

```
室内 RightDoor.EnterDoor
  → LoadScene(Village_KenMuNi1)
  → LastSceneName = Village_HomeScene45（= SceneManager.nowSceneName）
  → 村 GSM 查 EnterPosConfig：lastScene 匹配 → 把玩家放到 pos Transform
```

- **`bornPos` / `LeftBorn` / `RightBorn`** 是地图级出生点，**不会**自动当「从某室内回来」的落点，除非 **显式写进 EnterPosConfig**。
- **正确做法**：在 **`House_Npc45` 门外** 摆 **`ExitFrom_HomeScene45`**，EnterPos 改绑该节点（同 HomeScene1）。

### 生活类比

你从张三家后门出来，应该站在张三家大门口；现网却把你传送到村子东边通用公交站（`LeftBorn`），所以感觉「回错地方了」。

### 侦探须比较的方案

| 方案 | 做法 | 优点 | 风险 |
|------|------|------|------|
| **A（推荐）** | 新建 `Map/ExitFrom_HomeScene45` + 改 EnterPos 指向它 | 与 HomeScene1/2 一致；语义清晰 | 须 Scene 视图对齐 `House_Npc45` 门 |
| B | 只挪 `LeftBorn` 到 Npc45 门口 | 改一行配置 | **破坏** LeftBorn 其它用途（森林/默认出生等） |
| C | 绑 `House_Npc45` Transform | 少一个空节点 | 门预制体移动会带走落点；不推荐 |
| D | 改 C# SceneManager | 无 | 过度；违最小改动 |

### 与 HomeScene23 的边界

- `Village_HomeScene23` 的 EnterPos 也指向 **`LeftBorn`**（与 45 共用）。**本期只修 45**；若 23 也错位，记入 OPEN_QUESTIONS，勿顺手大改。

### 严禁

- 把 45 的回村落点继续绑在 **`LeftBorn`**（x≈62.4 的通用点）  
- 未新建 `ExitFrom_HomeScene45` 就宣称「已对齐 HomeScene1」  
- 改动 `House_Npc45` 的 `NextSceneName`（除非侦探证明进村也断）  
- 混淆本期与 RightDoor「组件禁用」问题（门已能换场时仍可能落点错）  
- 动 `RightBorn`（森林东等其它 EnterPos 可能依赖）

---

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/02_SYSTEM_SPEC.md
@Assets/Doc/技术文档/场景相关/场景切换.md
@Assets/Doc/执行文档/6月/0608/Village_HomeScene2_HouseDoor换场Village_KenMuNi1_架构溯源与施工执行说明.md
@Assets/Doc/执行文档/0822/Village_HomeScene45_RightDoor回村_架构溯源报告.md
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Scene/Village_House/Village_HomeScene45SceneManager.cs
@Assets/Scripts/Game/Static/Name/Res/SceneName.cs
@Assets/GameRes/Scenes/Village_HomeScene1.unity
@Assets/GameRes/Scenes/Village_HomeScene45.unity
@Assets/GameRes/Scenes/Village_KenMuNi1.unity

你现在是【架构侦探】。Unity 2020.3.48f1。
禁止修改场景、代码、Config。只读扫描 + 写溯源报告。

---

## 背景

45 号村民家已能（或即将能）从 RightDoor 回村，但玩家落在村里**错误坐标**，不在 `House_Npc45` 门口。要对拍 HomeScene1 的「ExitFrom + EnterPos」双端配置并给出最小修复。

---

## 侦探任务清单

### A. 样板对拍（HomeScene1，必填）

| 检查项 | HomeScene1 | 证据（Transform 名 / 坐标） |
|--------|------------|------------------------------|
| 村里出门落点空节点 | `ExitFrom_HomeScene1` | |
| `KenMuNi1.EnterPos` lastScene | `Village_HomeScene1` | |
| pos 引用 | `ExitFrom_HomeScene1` | |
| 村里进门物体 | `House_Npc1` | |
| 室内进屋落点 | `RightBorn` | |
| 室内 `EnterPos` lastScene | `Village_KenMuNi1` | |

### B. HomeScene45 现网缺口表

| 检查项 | 现网 | 应有（对齐 1 号屋） |
|--------|------|---------------------|
| `ExitFrom_HomeScene45` 存在 | | 有，在 `House_Npc45` 门外 |
| `KenMuNi1` EnterPos pos | 是否误绑 `LeftBorn` | 绑 `ExitFrom_HomeScene45` |
| `House_Npc45.NextSceneName` | | `Village_HomeScene45` |
| 室内 `EnterPos` KenMuNi1 → | | `RightBorn` |
| `nowSceneName` 常量 | | `Village_HomeScene45`（匹配 EnterPos lastScene 字符串） |

### C. 坐标与门对齐

- `House_Npc45` 世界坐标 / 门 Trigger 中心  
- `ExitFrom_HomeScene1` 与 `House_Npc1` 的相对偏移（作摆放参考）  
- 建议 `ExitFrom_HomeScene45` 初始坐标（门外可站立、不压碰撞体）  
- 与 `LeftBorn`(62.4,-6.1) 的距离差（解释玩家体感「传送到远处」）

### D. 双侧 EnterPos 完整表

| 场景 | lastScene | pos Transform | 现网 | 施工后 |
|------|-----------|---------------|------|--------|
| `Village_KenMuNi1` | `Village_HomeScene45` | | | |
| `Village_HomeScene45` | `Village_KenMuNi1` | | | |

### E. 与 RightDoor 报告交叉验证

- RightDoor `SceneChangeDoor` 是否已启用（影响验收路径，非落点根因）  
- `LastSceneName` 写入值是否确为 `Village_HomeScene45`（字符串必须与 EnterPos 一致）

### F. 推荐施工方案（最小改动）

1. 打开 `Village_KenMuNi1.unity`  
2. 在 `Map` 下（与 `ExitFrom_HomeScene1` 同级）新建空物体 **`ExitFrom_HomeScene45`**  
3. 拖到 **`House_Npc45` 门外**可站立处（参考 HomeScene1 相对偏移）  
4. `SceneManager` → `EnterPosConfig`：将 `lastScene: Village_HomeScene45` 的 **pos** 改为 **`ExitFrom_HomeScene45`**  
5. **不改** `LeftBorn` / `RightBorn` / `House_Npc45` 换场目标  
6. 核对室内 `Village_HomeScene45` 的 `Village_KenMuNi1` → `RightBorn`（通常已齐）  
7. **不改** `Village_HomeScene45SceneManager.cs`（除非 nowSceneName 错）

### G. 验收清单

| # | 操作 | 通过 |
|---|------|------|
| 1 | 村 `House_Npc45` 按 E 进屋 | 进 `Village_HomeScene45`，近室内门口（RightBorn 侧） |
| 2 | 屋内走 **RightDoor** 出村 | 换场到 `Village_KenMuNi1` |
| 3 | 落点 | **紧贴 `House_Npc45` 门外**（与步骤 1 进门位置对称），**不在** x≈62 的 LeftBorn 一带 |
| 4 | 与 HomeScene1 对比 | 出 1 号屋落在 `ExitFrom_HomeScene1` 附近；出 45 号屋落在 `ExitFrom_HomeScene45` 附近 |
| 5 | 再进屋往返 3 次 | 无叠门 Trigger 卡死 |
| 6 | Console | 无 LoadScene 失败 / 落点 Null |

### H. 开放问题

追加 `OPEN_QUESTIONS.md`「Village_HomeScene45 回村门口落点 · 2026-08-22」（如 Exit 节点微调像素、HomeScene23 是否也要独立 Exit）。

---

## 输出要求

写入：`Assets/Doc/执行文档/0822/Village_HomeScene45_回村门口落点_架构溯源报告.md`

报告结构：① 结论一句话 ② 错位原因（LeftBorn vs 门口） ③ 用户验收 ④ HomeScene1 样板证据 + 施工步骤 + 建议坐标

MASTER 四段式口头汇报。
```

---

## 施工员续跑（侦探报告拍板后再贴）

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0822/Village_HomeScene45_回村门口落点_架构溯源报告.md
@Assets/Doc/技术文档/场景相关/场景切换.md
@Assets/GameRes/Scenes/Village_KenMuNi1.unity

你现在是【施工员】。按报告修正 45 号屋出村后的村里落点，对齐 HomeScene1。

必须遵守：
- 在 `Village_KenMuNi1` 的 `Map` 下新建 **`ExitFrom_HomeScene45`**，摆在 `House_Npc45` 门外；
- 将 `EnterPosConfig` 中 `lastScene: Village_HomeScene45` 的 pos 改绑 **`ExitFrom_HomeScene45`**（禁止继续用 LeftBorn）；
- 不移动 LeftBorn/RightBorn（除非报告明确要求且说明影响面）；
- 不改 House_Npc45 的 NextSceneName、不改室内 SceneManager C#；
- Play 验收：进屋 → RightDoor 出村 → 落在 Npc45 门口。

提交说明：ExitFrom 坐标、EnterPos 改绑前后、与 HomeScene1 对拍截图描述、验收步骤结果。
```
