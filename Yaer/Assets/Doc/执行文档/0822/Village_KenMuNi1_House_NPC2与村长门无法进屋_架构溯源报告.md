# Village_KenMuNi1 — House_NPC2 / 村长门无法进屋 — 架构溯源报告

**文档版本**：v1.0（2026-08-22）  
**文档性质**：【架构侦探】只读溯源 + 施工指引  
**Unity**：2020.3.48f1  
**村场景**：`Assets/GameRes/Scenes/Village_KenMuNi1.unity`  
**目标室内**：`Village_HomeScene2.unity`、`Village_Chief_House.unity`  

关联提示词：`Assets/Doc/提示词/0822/Village_KenMuNi1_House_NPC2与村长门无法进屋_架构侦探提示词.md`  
历史施工：`0606/Village_HomeScene2_House_NPC2进村换场_施工执行说明.md`、`0608/Village_HomeScene2_HouseDoor换场Village_KenMuNi1_架构溯源与施工执行说明.md`

---

## ① 两扇门结论各一句

| 门 | 结论 |
|----|------|
| **House_NPC2 → Village_HomeScene2** | **磁盘换场七件套 + 室内 GSM/EnterPos 已对齐 0606/0608 修完**；`NextSceneName`、Build、`sceneObjs`、双侧 `EnterPos` 均正确。**不是**场景名/GSM 缺链。若 Play 仍进不去，优先查 **交互可达性**（第三部分高台 y=8.5、Collider 重叠、Console 有无 `[SceneChangeDoor]`）。 |
| **村长门（Hierarchy `House_Chlef`）→ Village_Chief_House** | **整条链未建成**：村 YAML **无**该门；`SceneName.cs` **无**常量；室内仍挂 **`ForestSceneManager`**，`EnterPos` 仅 `HomeScene1`/`ForestEastScene`；出门指 **龙宫/森林**。须 **方案 A** 从零补齐（对齐 `House_Npc1` / `Village_HomeScene1`）。**施工前先 Ctrl+S 保存村场景。** |

---

## ② 根因（名 / GSM / 交互）

### 2.1 换场通则（钉死）

```
户外 Stairs（SceneChangeDoor + Interactive 按 E）
  → NextSceneName = 与 .unity 文件名 / Build / SceneName 常量一致
  → LoadScene → LastSceneName 写入
  → 目标 GSM EnterPosConfig.lastScene 必须完全一致
  → 室内出门再写回 Village_KenMuNi1 + 村侧 ExitFrom 落点
```

**门 GameObject 名 ≠ 场景名**；但 **NextSceneName / GSM / EnterPos / Build 必须三位一体**。

### 2.2 门 A：House_NPC2

| 根因 ID | 假说 | 裁定 |
|---------|------|------|
| R1 | `NextSceneName` 错（如 `HomeScene2`） | ❌ 磁盘 **`Village_HomeScene2`** |
| R2 | 室内仍挂 `HomeScene1Manager` / Build 未登记 | ❌ 已换 **`Village_HomeScene2SceneManager`** + Build 有场景 |
| R3 | 双侧 `EnterPos` 缺行 | ❌ 村 `Village_HomeScene2→ExitFrom_HomeScene2`；室内 `Village_KenMuNi1→EnterFrom_Village` |
| R4 | 未进 `sceneObjs` | ❌ fileID **`1235839371`** 已登记 |
| **R5** | **靠近无 E / Collider 够不着**（第三部分高台） | ⚠️ **Play 待证**；门位 **(-124.73, 8.5)**，回程落点 **(-124.73, 3.85)** 纵深不同 |
| R6 | `CheckNextSceneUnlock` 锁场景 | ❌ 仅 `Map.LeftDoor/RightDoor` 绑定；Stairs 门无锁 |

**0606 旧缺口证伪**：下列项在 **2026-08-22 磁盘** 均已满足，勿重复改场景名字符串。

### 2.3 门 B：村长门（Chief）

| 根因 ID | 假说 | 裁定 |
|---------|------|------|
| **C1** | 村侧 **无门** / 未保存 | ✅ **`House_Chlef` / `Village_Chief_House` 在 KenMuNi1 YAML 零匹配**；编辑器有、磁盘无 → **须 Ctrl+S** |
| **C2** | `SceneName.cs` 无常量 | ✅ 无 **`Village_Chief_House`** |
| **C3** | 室内 **错挂 `ForestSceneManager`** | ✅ guid `ed5ec3a1…`；`nowSceneName` 非酋长家 |
| **C4** | `EnterPos` 无 `Village_KenMuNi1` | ✅ 仅 **`HomeScene1`**、**`ForestEastScene`**（旧森林模板名） |
| **C5** | 室内出门错场景 | ✅ `LeftDoor→HomeScene1`；`RightDoor→ForestEastScene` |
| **C6** | 村 `EnterPos` 无回程行 | ✅ 无 **`Village_Chief_House`** |
| C7 | `House_Chlef` 拼写 | ⚠️ 建议改名 **`House_Chief`**（辨认用，不阻塞 `NextSceneName`） |

**只改户外 `NextSceneName` 不改 GSM（方案 C）→ 进屋黑屏/落点错，禁止。**

### 2.4 误选物体

| 物体 | 说明 |
|------|------|
| **`House4 (6)`** | `NextSceneName: Village_House4`（23 号屋链），**不是**本需求 |
| **`House_Npc1`** | 样板门 → `Village_HomeScene1`，可正常对拍 |

---

## ③ 用户验收清单

| # | 操作 | 通过 |
|---|------|------|
| 1 | 村 **`House_NPC2`** 按 E | 进 **`Village_HomeScene2`**，落在 `EnterFrom_Village` 门口 |
| 2 | 室内 **`HouseDoor`** 按 E | 回村 **`ExitFrom_HomeScene2`** 外（约 x=-124.73） |
| 3 | 村 **村长门**（`House_Chief`）按 E | 进 **`Village_Chief_House`**（施工后） |
| 4 | 酋长家出门 | 回村门口对称 |
| 5 | Console | 无 `缺少 InteractiveComponent` / `NextSceneName 为空` / LoadScene 失败 |
| 6 | Debug | `[VillageHomeScene2Debug] lastScene=Village_KenMuNi1`；Chief 施工后同类日志 |

### Play 复现步骤（House_NPC2 仍失败时）

1. 打开 `Village_KenMuNi1`，走到 **Objects/House_NPC2**（世界约 **x=-125, y=8.5**）。  
2. Console 过滤 **`SceneChangeDoor`**。  
3. **无 E**：Scene 视图看 BoxCollider2D 是否与玩家重叠；查 `VillageWalkArea` 是否挡纵深。  
4. **有 E 无换场**：看是否打 `NextSceneName 为空` 或 `场景未解锁`。  
5. **能换场但落点怪**：查室内 `EnterFrom_Village` 与村 `ExitFrom_HomeScene2` 纵深差（8.5 vs 3.85）。

---

## ④ 七件套表 + 施工步骤

### 4.1 村里目标物体（磁盘 YAML）

| Hierarchy 名 | 磁盘存在 | 父节点 | 预制体 |
|--------------|----------|--------|--------|
| **`House_NPC2`** | ✅ | **`Objects`**（`objRoot`） | **`Stairs.prefab`** |
| **`House_Chlef` / 村长门** | ❌ **未保存到磁盘** | （编辑器有） | 建议同 **Stairs** |
| **`House4 (6)`** | ✅ | Objects | Stairs → `Village_House4`（无关） |

### 4.2 样板对拍 `House_Npc1` → `Village_HomeScene1`

| 层级 | Npc1 样板 | House_NPC2 | 村长门 Chief |
|------|-----------|------------|--------------|
| **NextSceneName** | `Village_HomeScene1` | `Village_HomeScene2` ✅ | **缺**（应 `Village_Chief_House`） |
| **TriggerWhenMoveIn** | 0（按 E） | 0 ✅ | 应 0 |
| **sceneObjs** | ✅ `2097443953` | ✅ `1235839371` | ❌ |
| **室内 GSM** | `Village_HomeScene1SceneManager` | `Village_HomeScene2SceneManager` ✅ | **`ForestSceneManager` ❌** |
| **SceneName.cs** | ✅ | ✅ | ❌ |
| **Build** | ✅ | ✅ | ✅ |
| **室内 EnterPos ←村** | `Village_KenMuNi1` | `Village_KenMuNi1` ✅ | ❌ |
| **村 EnterPos ←屋** | `Village_HomeScene1` | `Village_HomeScene2` ✅ | ❌ |
| **室内出门** | `MapRight/RightDoor` | `objRoot/HouseDoor` ✅ | `LeftDoor→HomeScene1` ❌ |

### 4.3 门 A 七件套（House_NPC2，磁盘现网）

| # | 检查项 | 现网 | 证据 |
|---|--------|------|------|
| 1 | GO Active | ✅ | Prefab 默认 Active，无禁用覆盖 |
| 2 | `SceneChangeDoor` Enabled | ✅ | Stairs 预制体链完整 |
| 3 | `NextSceneName` | ✅ **`Village_HomeScene2`** | YAML L10327 |
| 4 | `TriggerWhenMoveIn` | ✅ **0** | 预制体默认 |
| 5 | Interactive + Collider | ✅ | `componentsList` + BoxCollider2D（尺寸已覆盖） |
| 6 | `sceneObjs` | ✅ **`1235839371`** | GSM 列表 L2431 |
| 7 | Play E / Console | ⚠️ **待实测** | 过滤 `[SceneChangeDoor]` |

**室内现网（证伪 0606）**

| 项 | 现网 |
|----|------|
| SceneManager | **`Village_HomeScene2SceneManager`**（guid `b2c3d4e5…`） |
| `nowSceneName` | 代码 **`SceneName.Village_HomeScene2`** |
| `EnterPos` `Village_KenMuNi1` | ✅ → **`EnterFrom_Village`** (-24.12, -3.65) |
| 出门 | **`HouseDoor`** → `Village_KenMuNi1`（`LeftDoor` 禁用，符合 0608） |
| Config | **`Village_HomeScene2.asset`** |

**村侧回程**

| 项 | 值 |
|----|-----|
| `EnterPosConfig` | `lastScene: Village_HomeScene2` → **`ExitFrom_HomeScene2`** (-124.73, **3.85**) |
| 门位 | **(-124.73, 8.5)** — 回程纵深略低，属摆点微调，非阻塞换场 |

#### 门 A 施工（仅 Play 仍失败时）

| 步骤 | 动作 |
|------|------|
| 0 | **勿**再改 `NextSceneName` / GSM（已齐） |
| 1 | Play 按 §③ 复现；若无 E → 调 **Collider Offset/Size** 或门 **y** 贴 Walk 可达带 |
| 2 | 若有 E 无 Load → 查 Console 完整栈 |
| 3 | 进屋成功后 → 微调 **`ExitFrom_HomeScene2` y** 与门外视觉对齐（可选） |

---

### 4.4 门 B 七件套（村长门 — 全链待建）

| # | 检查项 | 现网 | 应有 |
|---|--------|------|------|
| 1–7 户外 | 村门 | ❌ 磁盘无 | `Objects` 下 **`House_Chief`**，Stairs，`NextSceneName=Village_Chief_House`，进 `sceneObjs` |
| 室内 GSM | `ForestSceneManager` | ❌ | **`Village_Chief_HouseSceneManager`** |
| SceneName | 无常量 | ❌ | **`public const string Village_Chief_House`** |
| Config | Forest `.asset` | ❌ | **`Village_Chief_House.asset`**（可复制 HomeScene1，`isFightingScene:0`） |
| 室内 EnterPos | `HomeScene1` / `ForestEast` | ❌ | **`Village_KenMuNi1` → `EnterFrom_Village`** |
| 室内出门 | `LeftDoor→HomeScene1` | ❌ | **`HouseDoor` 或 `RightDoor` → `Village_KenMuNi1`** |
| 村回程 | 无 | ❌ | **`ExitFrom_HomeSceneChief`** + `EnterPos` 行 `Village_Chief_House` |

#### 门 B 施工 — 方案 A（推荐，对齐 HomeScene1）

**C#**

1. `SceneName.cs` 增加 **`Village_Chief_House`**（注释与 `.unity` 路径一致）。  
2. 新建 **`Village_Chief_HouseSceneManager.cs`**（照抄 `Village_HomeScene1SceneManager`，改常量 + Debug 标签 `[VillageChiefHouseDebug]`）。  
3. 新建 **`Village_Chief_House.asset`** SceneManagerConfig。  
4. 生成 `.meta` guid 后绑到 `Village_Chief_House.unity` 的 `SceneManager`。

**`Village_Chief_House.unity`**

5. `SceneManager`：**替换** `ForestSceneManager` → `Village_Chief_HouseSceneManager`，换 Config。  
6. `EnterPosConfig`：删/覆盖旧 `HomeScene1`、`ForestEastScene` 行；增 **`lastScene: Village_KenMuNi1`** → 新建 **`EnterFrom_Village`**。  
7. 出门：改 **`RightDoor`**（或新建 **`HouseDoor`** Stairs）`NextSceneName → **Village_KenMuNi1**；`TriggerWhenMoveIn=0`；登记 **sceneObjs**。  
8. 禁用或清空 **`RightDoor→ForestEastScene`** 误触链。

**`Village_KenMuNi1.unity`**

9. **Ctrl+S** 保存后确认 **`House_Chief`**（建议由 `House_Chlef` 改名）在磁盘。  
10. 拖 **Stairs.prefab** 到 `Objects`；`NextSceneName=**Village_Chief_House**`；位置对齐酋长屋外。  
11. `SceneEntity` 拖入 **`sceneObjs`**。  
12. `EnterPosConfig` 增 **`lastScene: Village_Chief_House`** → **`ExitFrom_HomeSceneChief`**（与门 x 对齐，y 贴 Walk 带）。  

**Build**：已有 `Village_Chief_House.unity` ✅，施工后复测即可。

### 4.5 严禁

- 把 `House_NPC2` 改成 `HomeScene2`（龙宫旧名）  
- 村长门指到 `Village_House4`  
- 只改户外门不改 `ForestSceneManager`  
- 未保存村场景就提交「无村长门」

### 4.6 最小改动文件清单

| 门 | 文件 | 动作 |
|----|------|------|
| NPC2 | 可能 **0**（仅 Play 调 Collider/落点） | 链路已齐 |
| Chief | `SceneName.cs` | 增常量 |
| Chief | `Village_Chief_HouseSceneManager.cs` | 新建 |
| Chief | `Village_Chief_House.asset` | 新建 |
| Chief | `Village_Chief_House.unity` | 换 GSM、EnterPos、出门 |
| Chief | `Village_KenMuNi1.unity` | 村门 + ExitFrom + EnterPos |

---

## ⑤ 开放问题

见 `OPEN_QUESTIONS.md` §「KenMuNi1 两户门换场 · 2026-08-22」。

---

## 修订记录

| 日期 | 说明 |
|------|------|
| 2026-08-22 | v1.0：NPC2 证伪 0606 缺口；Chief 全链待建；House_Chlef 未落盘 |
