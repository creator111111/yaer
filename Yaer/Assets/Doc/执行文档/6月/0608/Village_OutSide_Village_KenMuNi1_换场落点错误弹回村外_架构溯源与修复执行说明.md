# Village_OutSide ⇄ Village_KenMuNi1 — 换场落点错误 / 弹回村外 — 架构溯源与修复执行说明

**文档性质**：架构侦探产出（问题溯源 + Unity 关卡修复指引；**本阶段不改代码**）  
**依据**：
- 换场通则：`Assets/Doc/执行文档/0518/SceneChangeDoor场景切换_架构溯源与执行说明.md`
- 村里出村外：`Assets/Doc/执行文档/0608/Village_KenMuNi1_RightDoor换场Village_OutSide_架构溯源与施工执行说明.md`
- 村外回村：`Assets/Doc/执行文档/0608/Village_OutSide_RightDoor换场Village_KenMuNi1_架构溯源与施工执行说明.md`
- 技术文档：`Assets/Doc/技术文档/场景相关/场景切换.md`

**现象（用户描述）**：从 **`Village_OutSide`** 换场后落点不对，出现 **错误传送 / 闪回 `Village_OutSide`**（或刚进村里即被门带走）。  
**说明**：描述中「进入 Village_OutSide」若为笔误，实际链路为 **`Village_OutSide` → `Village_KenMuNi1` → 异常回弹**；本文按双向落点一并修复。  
**Unity 版本**：2020.3.48f1  

---

## 1. 结论（一句话）

**根因是 `Village_KenMuNi1` 的 `EnterPosConfig` 把来源 `Village_OutSide` 的落点绑到了 `Map/RightBorn`（x≈206.6），该点是为 `ForestEastScene` 东缘衔接预留的远右坐标，与村里实际出村外的 `MapRight/RightDoor`（x≈66.3）相距约 140 单位；玩家从村外进门后落在错误区域，易与 `TriggerWhenMoveIn` 门区叠加或走出有效地图，表现为闪回村外或传送异常。修复：为村外进门单独建 `EnterFrom_Village_OutSide` 空节点（门口内侧），并更新双侧 `EnterPosConfig`；勿挪动原 `RightBorn`（仍给森林东用）。**

---

## 2. 你要验收的现象（修复后）

| 步骤 | 期望 |
|------|------|
| 村里 `RightDoor` → 村外 | 落在 `Village_OutSide/LeftBorn` 附近，**不**立刻再换场 |
| 村外 `LeftDoor` 或 `RightDoor` → 村里 | 落在 **`Village_KenMuNi1` 右门内侧**（x≈58～64），能看见村内地面与右缘墙 |
| 进村后静止 2 秒 | **不**黑幕闪回 `Village_OutSide` |
| 往返 3 次 | 落点稳定、无门区连触 |
| Console | 无重复 `SceneChangeDoor已进入`；无加载失败 |

---

## 3. 架构溯源：落点怎么算

### 3.1 运行时规则（再次强调）

```
SceneChangeDoor.LoadScene(目标场景名)
  → ChangeSceneComponentGM.LastSceneName = 刚离开的场景名（如 Village_OutSide）
  → 目标场景 BaseGameSceneManager.SetPlayerPos
      → 遍历 EnterPosConfig，匹配 lastScene == LastSceneName
      → 使用对应 pos.Transform.position
      → 无匹配 → Map.DefaultBornPos
```

| 字段 | 运行时是否读取 |
|------|----------------|
| 门上 `bornPos` | ❌ **不读** |
| `Map/LeftBorn`、`RightBorn` | 仅作 Map 元数据；**只有被 EnterPosConfig 引用才参与落点** |
| `LastSceneName` | 来自 `ChangeSceneComponentGM`，值为加载参数 `sceneName`（如 `Village_OutSide`），**不依赖**场景管理器 `nowSceneName` 字段 |

### 3.2 当前门与落点配置（静态阅读 2026-06-08）

#### 门（`TriggerWhenMoveIn = true`，走进即换场）

| 场景 | 门 | NextSceneName | 约略世界 X |
|------|-----|---------------|------------|
| `Village_KenMuNi1` | `MapRight/RightDoor` | `Village_OutSide` | **~66.3**（MapRight 锚点） |
| `Village_OutSide` | `MapLeft/LeftDoor` | `Village_KenMuNi1` | **~-14.0**（MapLeft 锚点） |
| `Village_OutSide` | `MapRight/RightDoor` | `Village_KenMuNi1` | **~223.4**（MapRight 锚点） |

#### EnterPosConfig（决定落点 — **问题集中处**）

| 目标场景 | lastScene | pos 节点 | 约略 X | 问题 |
|----------|-----------|----------|--------|------|
| `Village_OutSide` | `Village_KenMuNi1` | `LeftBorn` | -4.11 | ✅ 与左缘进门一致（村里出村外） |
| `Village_OutSide` | `ForestEastScene` | `LeftBorn` | -4.11 | 模板残留，暂可保留 |
| **`Village_KenMuNi1`** | **`Village_OutSide`** | **`RightBorn`** | **206.63** | ❌ **与 RightDoor ~66 脱节** |
| `Village_KenMuNi1` | `ForestEastScene` | `RightBorn` | 206.63 | ✅ 森林东进村的远右落点 |

### 3.3 坐标对照图（问题可视化）

```
Village_KenMuNi1 地图 X 轴（示意）

  MapLeft          村内可玩区              MapRight/RightDoor        RightBorn（当前误用）
  x≈-14            x≈0～60                    x≈66.3                  x≈206.6
    |-------------------|------------------------|                        |
    左缘门                                    实际村外门                  EnterPosConfig
                                              ↑ 应从村外落在这里内侧          错落在远处 →
```

```
Village_OutSide 地图 X 轴（示意）

  MapLeft/LeftDoor    LeftBorn（进村外）              MapRight/RightDoor    RightBorn
  x≈-14               x≈-4.1                         x≈223.4              x≈213.5
    |--------------------|--------------------------------|------------------|
    回村门               从村里进村外落点                  村外右缘回村门
```

**为何像「回到 Village_OutSide」：**

1. **落点偏离门内侧**（进村落在 x=206）：相机/碰撞异常，玩家误以为没换场或立刻又触发别的换场逻辑。  
2. **落点与 `TriggerWhenMoveIn` 触发区重叠**（村外 `LeftBorn` / `RightBorn` 距门仅 ~10 单位）：场景加载后碰撞体已在门区内，**进场景同一帧**再次 `EnterDoor` → 黑幕闪回（经典 Map 门 bug）。  
3. （次要）`Village_OutSide` 误挂 **`WestRappRoadSceneMgr`**，地形/地点显示不对，**不直接导致 LastSceneName 错误**，但应一并修正。

---

## 4. 根因归纳

| ID | 根因 | 严重度 |
|----|------|--------|
| **R1** | `KenMuNi1.EnterPosConfig[Village_OutSide]` 指向 **远右 `RightBorn`(206)**，非村外门旁 | **阻塞** |
| **R2** | `RightBorn` 同时服务 `ForestEastScene` 与 `Village_OutSide`，**不能**简单拖动 `RightBorn` | 设计债 |
| **R3** | 落点节点可能距 `TriggerWhenMoveIn` 门区过近 | 可能叠加 |
| R4 | `Village_OutSide` 无专用 `SceneManager`，挂 `WestRappRoadSceneMgr` | 建议修 |

---

## 5. 修复方案（推荐 · 仅改关卡配置）

### 5.1 原则

1. **进村外**（`KenMuNi1` → `OutSide`）：继续用 `Village_OutSide/LeftBorn`（或新建 `EnterFrom_KenMuNi1` 同坐标微调）。  
2. **从村外进村**（`OutSide` → `KenMuNi1`）：**新建** `EnterFrom_Village_OutSide`，放在 **`MapRight/RightDoor` 内侧约 4～8 单位**，**禁止**再用 `RightBorn`(206)。  
3. **`RightBorn`(206)**：仅保留给 `ForestEastScene` 的 `EnterPosConfig`。  
4. 落点空节点放在 **门触发区外**，避免加载即连触（见 §5.4）。

### 5.2 建议落点坐标（首版施工值）

| 场景 | 新节点名 | 建议 localPosition（相对 `Map`） | 用途 |
|------|----------|----------------------------------|------|
| `Village_KenMuNi1` | **`EnterFrom_Village_OutSide`** | **(58, -6.61, 0)** | 村外左/右门进村后落点（`lastScene: Village_OutSide`） |
| `Village_OutSide` | **`EnterFrom_KenMuNi1`**（可选） | **(-8, -6.61, 0)** | 村里 `RightDoor` 进村外（替代直接绑 `LeftBorn`，略离 `LeftDoor`） |

> `RightDoor` 锚点 x≈66.3，`EnterFrom_Village_OutSide` x=58 表示站在门内侧约 8 单位。Play 后可在 Scene 视图微调至不压线。

### 5.3 双侧 EnterPosConfig 目标表

| 场景 | lastScene | pos（修复后） |
|------|-----------|---------------|
| `Village_KenMuNi1` | `Village_OutSide` | **`Map/EnterFrom_Village_OutSide`**（新建） |
| `Village_KenMuNi1` | `ForestEastScene` | `Map/RightBorn`（**保持**） |
| `Village_OutSide` | `Village_KenMuNi1` | `Map/EnterFrom_KenMuNi1` 或 `Map/LeftBorn`（微调后） |

---

## 6. Unity 施工步骤

### 6.1 `Village_KenMuNi1` — 新建进门落点（核心）

1. 打开 **`Village_KenMuNi1.unity`**。  
2. Hierarchy：`Map` → 右键 **Create Empty** → 命名 **`EnterFrom_Village_OutSide`**。  
3. Transform（相对 Map）：

| 字段 | 值 |
|------|-----|
| Position | **(58, -6.61, 0)**（或 RightDoor 左内侧，以 Scene 视图为准） |
| Layer | 8（与 `LeftBorn`/`RightBorn` 一致） |

4. 选中 **`SceneManager`** → **Enter Pos Config** → 找到 **`lastScene: Village_OutSide`** 项：  
   - **Pos** 从 `RightBorn` 改为 **`EnterFrom_Village_OutSide`**。  
5. **不要**修改 `RightBorn` 位置（森林东仍需要）。  
6. 确认 `Map/MapRight/RightDoor` → `NextSceneName = Village_OutSide`，`Trigger When Move In` ✅。

### 6.2 `Village_OutSide` — 进村外落点微调（推荐）

1. 打开 **`Village_OutSide.unity`**。  
2. `Map` 下新建 **`EnterFrom_KenMuNi1`**，Position **(-8, -6.61, 0)**。  
3. `SceneManager` → **Enter Pos Config** → `lastScene: Village_KenMuNi1`：  
   - **Pos** 改为 **`EnterFrom_KenMuNi1`**（原 `LeftBorn` 可保留作 Map 元数据）。  
4. 确认 `LeftDoor`、`RightDoor` 的 `NextSceneName` 均为 **`Village_KenMuNi1`**。  
5. 在 Scene 视图打开 **Gizmos**，确认落点 **不在** `LeftDoor`/`RightDoor` 的 BoxCollider2D 内。

### 6.3 （推荐）`Village_OutSide` 专用场景管理器

当前 `SceneManager` 挂 **`WestRappRoadSceneMgr`**（`guid: 993eab29c009431986907425a1df716d`），与村外场景不符。

| 项 | 建议 |
|----|------|
| 新建 | `Village_OutSideSceneManager`（仿 `Village_KenMuNiSceneManager`） |
| `nowSceneName` | `SceneName.Village_OutSide` |
| `GetCurSceneTerrainType` | 户外草地类型（与村外战斗一致，勿用 `GlassType`） |
| `SetNowPlace` | `PlaceName.KenMuNi` |

**本修复以落点为主**；场景管理器可同 PR 或下一批，不阻塞落点验收。

### 6.4 保存

两场景 **Ctrl+S**。

---

## 7. 替代方案说明

| 方案 | 做法 | 适用 |
|------|------|------|
| **A（推荐）** | 新建 `EnterFrom_*` 空节点 + 改 `EnterPosConfig` | 最小改动、不破坏森林东 |
| **B** | 整体平移 `RightBorn` 到 x≈60 | **会破坏** `ForestEastScene` 进村落点 |
| **C** | 代码：换场后 0.5s 内禁用门 `TriggerWhenMoveIn` | 掩盖重叠，不解决坐标错误；仅作兜底 |
| **D** | 用不同虚拟 `lastScene` 字符串区分左右门 | 须改 `SceneChangeDoor` 传参，**本任务不采用** |

---

## 8. 验收清单

**从 `InitScene` 启动。**

| # | 操作 | 通过标准 |
|---|------|----------|
| S1 | 村里向右出 `RightDoor` | 村外 `EnterFrom_KenMuNi1` / `LeftBorn` 附近；不连触 |
| S2 | 村外 `LeftDoor` 向左进村 | 村里 **x≈58～64**，近右缘墙；**不**闪回村外 |
| S3 | 村外 `RightDoor` 向右进村 | 同上（两扇门进村 `LastSceneName` 相同，落点一致 — 架构限制，见 §7 方案 D） |
| S4 | 静止 2 秒 / 往返 3 次 | 无自动黑幕换场 |
| S5 | 从森林东进村（若关卡可达） | 仍落 `RightBorn`(206)，**不退化** |

### 8.1 故障排查

| 现象 | 处理 |
|------|------|
| 进村仍落 x≈206 | `KenMuNi1.EnterPosConfig` 仍指向 `RightBorn` → 改 `EnterFrom_Village_OutSide` |
| 进村瞬间回村外 | 落点压在门 Trigger 内 → 节点向门**内侧**再移 4～6 单位 |
| 落点进地底/空中 | 对齐既有 `LeftBorn` 的 **y = -6.61** |
| `LastSceneName` 对不上 | 门上 `NextSceneName` 必须与 `.unity` 文件名一致（`Village_OutSide` / `Village_KenMuNi1`） |
| 仅 UI 地点不对 | 换 `Village_OutSideSceneManager`（§6.3） |

### 8.2 调试日志（可选）

在 `BaseGameSceneManager.SetPlayerPos` 临时加一行（施工员调试用，合入前可删）：

```csharp
Debug.Log($"[SetPlayerPos] scene={nowSceneName} lastScene={lastSceneName} pos={选中落点}");
```

用于确认匹配到哪条 `EnterPosConfig`。

---

## 9. 改动范围

| 路径 | 改动 |
|------|------|
| `Village_KenMuNi1.unity` | 新建 `EnterFrom_Village_OutSide`；`EnterPosConfig` 改绑 |
| `Village_OutSide.unity` | 新建 `EnterFrom_KenMuNi1`（推荐）；`EnterPosConfig` 改绑 |
| `RightBorn` / 门 `NextSceneName` | **位置与目标名保持**，除非验收仍连触再微调 |
| C# | **非必须**；可选 `Village_OutSideSceneManager` |
| 既有换场文档 | 落点描述以 **本文 §5.3** 为准（旧文档写「村外回村落 RightBorn」对 **OutSide 侧** 正确，对 **KenMuNi1 侧进村** 应改为 `EnterFrom_Village_OutSide`） |

---

## 10. 相关文档

| 主题 | 路径 |
|------|------|
| 换场通则 | `Assets/Doc/执行文档/0518/SceneChangeDoor场景切换_架构溯源与执行说明.md` |
| 村里出村外 | `Assets/Doc/执行文档/0608/Village_KenMuNi1_RightDoor换场Village_OutSide_架构溯源与施工执行说明.md` |
| 村外回村 | `Assets/Doc/执行文档/0608/Village_OutSide_RightDoor换场Village_KenMuNi1_架构溯源与施工执行说明.md` |
| `SetPlayerPos` | `Assets/Scripts/Game/GameRuntime/GameSceneManager/Base/BaseGameSceneManager.cs` |
| `SceneChangeDoor` | `Assets/Scripts/Game/GameRuntime/Entities/SceneEntities/CommonEntity/SceneChangeDoor.cs` |

---

## 11. 文档修订

| 日期 | 说明 |
|------|------|
| 2026-06-08 | 初版：KenMuNi1 进村落点误绑 RightBorn(206) 导致换场异常；双侧 EnterPosConfig 修复指引 |

**文档路径**：`Assets/Doc/执行文档/0608/Village_OutSide_Village_KenMuNi1_换场落点错误弹回村外_架构溯源与修复执行说明.md`
