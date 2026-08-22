# Village_HomeScene45 — 出屋回村门口落点 — 架构溯源报告

**文档版本**：v1.0（2026-08-22）  
**文档性质**：【架构侦探】只读盘点；**本文件不施工**（不改场景 / 代码）  
**Unity**：2020.3.48f1 / C#  
**主改场景（施工）**：`Assets/GameRes/Scenes/Village_KenMuNi1.unity`  
**对照场景**：`Village_HomeScene1.unity`、`Village_HomeScene45.unity`  
**产品需求**：从 45 号屋出村后，落在 **`House_Npc45` 门外**（与进村同一户），对齐 HomeScene1 的 `ExitFrom_HomeScene1` 惯例。

关联：

- 提示词：`Assets/Doc/提示词/0822/Village_HomeScene45_回村门口落点_架构侦探提示词.md`
- 出门链：`0822/Village_HomeScene45_RightDoor回村_架构溯源报告.md`（能不能出门）
- **本期**：出门后**落在哪**

---

## ① 结论一句话

**换场链已通（RightDoor 已启用、`nowSceneName=Village_HomeScene45`），但村侧 `EnterPosConfig` 把 `lastScene: Village_HomeScene45` 误绑在通用 `LeftBorn`（62.4, -6.1，靠近 23 号屋/Npc4），而 `House_Npc45` 在 (-4.39, 5.67)，相距约 **67 世界单位** → 体感「回错地方」。施工：在 `Map` 下新建 **`ExitFrom_HomeScene45`**（摆 Npc45 门外）+ EnterPos 改绑该节点；室内 `KenMuNi1 → RightBorn` 已齐，不必动。**

---

## ② 错位原因（LeftBorn vs 门口）

从张三家（45 号屋）后门出来，应站在**张三家大门口**（`House_Npc45`）。现网却把 `LastSceneName=Village_HomeScene45` 匹配到 **`LeftBorn`**——村子东侧、靠近 **`House_Npc4`（23 号屋，x≈62）** 的通用出生点，相当于出屋后被传到**隔壁村的公交站**。

`LeftBorn` 不是自动「从室内回来」的落点；只有写进 `EnterPosConfig.pos` 才会用。0820 改名 45 时只加了 `lastScene` 行，**复制了 23 号屋的 LeftBorn 引用**，未按 HomeScene1 建专用 `ExitFrom_*`。

---

## ③ 用户需要做什么

1. **认问题**：能出村，但落点不在 `House_Npc45` 门口 → **EnterPos 绑错**，不是 RightDoor 没开。  
2. **认施工范围**：主要改 **`Village_KenMuNi1`**；室内 45 通常不用动。  
3. **认样板**：与 1 号屋一样用 **`ExitFrom_HomeScene{N}`** 专用空节点。  
4. 施工后：**进屋 → RightDoor 出村 → 应站在 Npc45 门外**（不在 x≈62 一带）。

### 验收清单（施工后）

| # | 操作 | 通过 |
|---|------|------|
| 1 | 村 `House_Npc45` 按 E 进屋 | 进 `Village_HomeScene45`，落在室内门口（RightBorn 侧） |
| 2 | 屋内走 **RightDoor** 出村 | 换场到 `Village_KenMuNi1` |
| 3 | 落点 | **紧贴 `House_Npc45` 门外**，与步骤 1 进门位置对称 |
| 4 | 对比 | 出 1 号屋在 `ExitFrom_HomeScene1` 附近；出 45 号屋在 **`ExitFrom_HomeScene45`** 附近 |
| 5 | 不应出现 | 落在 **x≈62, y≈-6** 的 `LeftBorn` 一带 |
| 6 | 往返 3 次 | 无叠门 Trigger 卡死；Console 无落点 Null |

---

## ④ 给程序看的补充

### 4.1 样板对拍（HomeScene1）

| 检查项 | HomeScene1 | 证据（YAML） |
|--------|------------|--------------|
| 村里出门落点空节点 | **`ExitFrom_HomeScene1`** | `Map` 下，`localPosition (45.5, -6.1, 0)` |
| `KenMuNi1.EnterPos` lastScene | `Village_HomeScene1` | ✅ |
| pos 引用 | **`ExitFrom_HomeScene1`**（`5601461771111111002`） | ✅ 非 LeftBorn |
| 村里进门物体 | **`House_Npc1`** | `Objects` 下，`localPosition (45.41, 1.9, 0)` |
| 门 ↔ Exit 偏移 | Δx **+0.09**，Δy **-8.0** | Exit 在门外偏下 |
| 室内进屋落点 | **`RightBorn`** | `localPosition (-6.91, -3.65, 0)` |
| 室内 `EnterPos` | `lastScene: Village_KenMuNi1` → **RightBorn** | ✅ |

### 4.2 HomeScene45 现网缺口表（B）

| 检查项 | 现网 | 应有（对齐 1 号屋） |
|--------|------|---------------------|
| `ExitFrom_HomeScene45` 存在 | **无** ❌ | **有**，在 `House_Npc45` 门外 |
| `KenMuNi1` EnterPos `Village_HomeScene45` → pos | **`LeftBorn`** ❌ | **`ExitFrom_HomeScene45`** |
| `House_Npc45.NextSceneName` | **`Village_HomeScene45`** ✅ | 不变 |
| 室内 `EnterPos` `KenMuNi1` → | **`RightBorn`** ✅ `(-5.4, -3.65, 0)` | 已齐 |
| `nowSceneName` | **`Village_HomeScene45`** ✅（`Village_HomeScene45SceneManager.cs`） | 与 EnterPos 字符串一致 |

### 4.3 坐标与门对齐（C）

**`Map` / `Objects` 根均为 `(0,0,0)`**，下列坐标可直接当世界坐标使用。

| 节点 | localPosition (x, y) | 说明 |
|------|---------------------|------|
| **`House_Npc1`** | (45.41, 1.9) | 1 号屋门 |
| **`ExitFrom_HomeScene1`** | (45.5, -6.1) | 1 号屋出村落点 |
| **`House_Npc45`** | **(-4.39, 5.67)** | 45 号屋门 |
| **`LeftBorn`（现网误绑）** | **(62.4, -6.1)** | 近 `House_Npc4`（23 号，x≈61.98） |
| **`House_Npc4`**（23 号） | (61.98, -1.445) | 与 LeftBorn 同侧，**非 Npc45** |

**错位距离（解释体感）**：

- `House_Npc45` → 现网落点 `LeftBorn`：Δx ≈ **+66.8**，Δy ≈ **-11.8**，直线距离 ≈ **68 单位**。  
- `House_Npc45` → 建议 `ExitFrom_HomeScene45`：按 1 号屋偏移 **( +0.09, -8.0 )** → 约 **(-4.30, -2.33)**（Scene 视图须微调可站立、不压门 Trigger）。

**建议 `ExitFrom_HomeScene45` 初始坐标（施工默认）**：

| 轴 | 值 | 依据 |
|----|-----|------|
| **x** | **-4.30** | `House_Npc45.x + 0.09`（对齐 ExitFrom_HomeScene1 相对 Npc1 的 Δx） |
| **y** | **-2.33** | `House_Npc45.y - 8.0`（对齐 1 号屋门外 Δy） |
| **z** | **0** | 与现有 Exit 节点一致 |

> 若门洞朝向/Walk 区与 1 号屋不同，允许在 **±0.5～1.0** 内微调；**禁止**为省事继续用 LeftBorn。

### 4.4 双侧 EnterPos 完整表（D）

| 场景 | lastScene | pos Transform | 现网 | 施工后 |
|------|-----------|---------------|------|--------|
| **`Village_KenMuNi1`** | `Village_HomeScene45` | 出 45 号屋回村落点 | **`LeftBorn`** ❌ | **`ExitFrom_HomeScene45`**（新建） |
| **`Village_HomeScene45`** | `Village_KenMuNi1` | 从村进屋落点 | **`RightBorn`** ✅ | **不变** |

**村侧 EnterPos 全表（节选，勿误改它行）**：

| lastScene | pos（现网） | 本期 |
|-----------|-------------|------|
| `Village_HomeScene45` | LeftBorn | **改** ExitFrom_HomeScene45 |
| `Village_HomeScene23` | LeftBorn | **不动**（另案） |
| `Village_HomeScene2` | ExitFrom_HomeScene2 | 不动 |
| `Village_HomeScene1` | ExitFrom_HomeScene1 | 不动 |
| `ForestEastScene` | 专用点 | 不动 |

### 4.5 与 RightDoor 报告交叉验证（E）

| 项 | 现网（2026-08-22 YAML） | 与落点关系 |
|----|-------------------------|------------|
| RightDoor `SceneChangeDoor` | **`m_Enabled: 1`** ✅ | 已能 `LoadScene`；落点错**独立**于此 |
| LeftDoor `SceneChangeDoor` | **`m_Enabled: 0`** ✅ | 对齐 23 单出口 |
| `LastSceneName` 写入 | `Village_HomeScene45SceneManager.nowSceneName` | 与 EnterPos `lastScene` **字符串一致** ✅ |
| 进村 `House_Npc45.NextSceneName` | `Village_HomeScene45` ✅ | 不需改 |

### 4.6 推荐施工方案（F，最小改动）

1. 打开 **`Village_KenMuNi1.unity`**。  
2. 在 **`Map`** 下（与 `ExitFrom_HomeScene1` / `ExitFrom_HomeScene2` 同级）新建空物体 **`ExitFrom_HomeScene45`**（Layer 8，与现有 Exit 一致）。  
3. 摆到 **`House_Npc45` 门外**可站立处；初始坐标建议 **(-4.30, -2.33, 0)**，Scene 视图对齐门 Trigger。  
4. 选中 **SceneManager**（村 GSM）→ `EnterPosConfig`：找到 **`lastScene: Village_HomeScene45`**，将 **pos** 从 `LeftBorn` 改为 **`ExitFrom_HomeScene45`** Transform。  
5. **不改** `LeftBorn` / `RightBorn` 坐标与其它 EnterPos 行。  
6. **不改** `House_Npc45.NextSceneName`、室内 `Village_HomeScene45` EnterPos、`Village_HomeScene45SceneManager.cs`。  
7. Play：进屋 → RightDoor 出村 → 验 §3 清单。

### 4.7 方案对比与否决

| 方案 | 裁定 |
|------|------|
| **A：新建 ExitFrom + 改 EnterPos** | ✅ **推荐** |
| B：只挪 `LeftBorn` | ❌ 破坏森林东/23 号等依赖 |
| C：绑 `House_Npc45` Transform | ❌ 门预制体移动会带走落点 |
| D：改 C# SceneManager | ❌ 过度 |

### 4.8 与 HomeScene23 边界

`Village_HomeScene23` 的 EnterPos **同样绑 `LeftBorn`**；`House_Npc4` 在 x≈62，与 LeftBorn **碰巧较近**，23 号屋落点可能「勉强能玩」。**本期只修 45**；23 是否也要 `ExitFrom_HomeScene23` 记入 OPEN_QUESTIONS。

### 4.9 最小改动文件列表

| 文件 | 动作 |
|------|------|
| `Assets/GameRes/Scenes/Village_KenMuNi1.unity` | 新建 `ExitFrom_HomeScene45`；EnterPos 改绑 |
| `Village_HomeScene45.unity` | **不改**（RightBorn 已齐） |
| `Village_HomeScene45SceneManager.cs` | **不改** |
| `House_Npc45` Prefab 实例 | **不改** NextSceneName |

### 4.10 严禁

- 45 回村落点继续绑 **`LeftBorn`**  
- 未建 `ExitFrom_HomeScene45` 就宣称已对齐 HomeScene1  
- 顺手改 23 号屋 EnterPos（除非单独立项）  
- 移动 `RightBorn`（室内进屋落点）

### 4.11 开放问题

见 `OPEN_QUESTIONS.md` §「Village_HomeScene45 回村门口落点 · 2026-08-22」。

---

## 修订记录

| 日期 | 说明 |
|------|------|
| 2026-08-22 | v1.0 侦探：EnterPos 误绑 LeftBorn；缺 ExitFrom_HomeScene45；建议坐标 (-4.30,-2.33)；RightDoor 已启用 |
