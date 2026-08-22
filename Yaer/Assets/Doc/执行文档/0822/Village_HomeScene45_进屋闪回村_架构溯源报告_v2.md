# Village_HomeScene45 — 进屋立刻闪回村 — 架构溯源报告 v2

**文档版本**：v2.0（2026-08-22 晚）  
**文档性质**：【架构侦探】第二轮只读复盘；**本文件不施工**  
**Unity**：2020.3.48f1 / C#  
**用户反馈**：已按 v1 施工，**闪回依旧**  
**前序**：`Village_HomeScene45_进屋闪回村_架构溯源报告.md`（v1.0，R1）

关联：

- 提示词：`Assets/Doc/提示词/0822/Village_HomeScene45_进屋闪回村_第二轮_架构侦探提示词.md`
- 出村落点（另案）：`0822/回村门口落点`（`ExitFrom_HomeScene45` 已建，与**进屋闪回**无直接关系）

---

## ① 结论一句话（主因 **R1b**）

**v1 判 R1 正确，但施工只完成「改名 + EnterPos 改绑」，未把 `EnterFrom_Village` 坐标挪离右门区。现网该节点仍在 **(-5.41, -3.65)**，与 `RightBorn`/`DefaultBornPos` 几乎重合，距 `RightDoor` Trigger 最近边仅约 **3.0** 世界单位 → `TriggerWhenMoveIn=1` 机制未变，闪回必然复现。施工：方案 **A′** — 将 `EnterFrom_Village` 移至 **(-24.12, -3.65)**（对齐 HomeScene2），同步左移 `DefaultBornPos`，手动绑定 `Map.leftBornTsf`。**

---

## ② v1 为何没修好（施工核验表）

| 项 | v1 要求 | 现网 YAML（2026-08-22 晚） | Play 实测 | 结论 |
|----|---------|---------------------------|-----------|------|
| EnterPos `Village_KenMuNi1` → 节点 | 绑 `EnterFrom_Village` | 绑 **`3588313296614849449`（EnterFrom_Village）** ✅ | 待补日志 | **引用已对** |
| **`EnterFrom_Village` 坐标** | **(-24.12, -3.65)** | **(-5.41, -3.65)** ❌ | 预期落点 **x≈-5.4** | **R1b 半施工** |
| `RightBorn`（Map 元数据） | 保留 | **(-5.82, -3.65)** | — | 仍在门区（正常） |
| **`DefaultBornPos`（fallback）** | 应远离门 | **(-5.73, -3.65)** ❌ | R2 未命中时仍踩门 | **隐患未清** |
| `Map.leftBornTsf` | 宜指向进屋点 | **`{fileID: 0}` 空** ⚠️ | `Map.Find("LeftBorn")` 失败 | 改名副作用 |
| RightDoor Enabled / Trigger | 保持出门 | **Enable=1, Trigger=1** ✅ | — | 符合 v1 |
| LeftDoor | Disable | **Enable=0** ✅ | — | 非左门问题 |

**半施工示意图**：

```
v1 做了：  LeftBorn 改名 → EnterFrom_Village  ✅
          EnterPos 改绑新节点               ✅
v1 没做：  节点坐标仍 ≈ RightBorn (-5.4)    ❌  → 闪回依旧
```

生活类比：换了「安全门垫」的牌子，门垫还摆在后门感应器上。

---

## ③ 用户验收清单（v2 施工后）

| # | 操作 | 通过 |
|---|------|------|
| 1 | **新游戏** + **读档** 各进村 **20 次** | **0 次**立刻回村 |
| 2 | 进屋落点 X | **≤ -20**（与 HomeScene2 同量级），**非** -5.4 |
| 3 | Scene 视图 Gizmos | `EnterFrom_Village` **不在** RightDoor 绿框内 |
| 4 | Console | 进村后、未走向右门前 **无** `RightDoor` EnterDoor |
| 5 | `[VillageHomeScene45Debug] lastScene=` | 恒为 **`Village_KenMuNi1`** |
| 6 | 主动走向 RightDoor | 仍能正常出村 |

### Console 过滤关键字

`SceneChangeDoor` · `VillageHomeScene45Debug` · `LoadScene` · `进入场景` · `archiveStart` · `没有设置左出生点`（Map 警告）

### Play 时间线模板（单次闪回取证）

侦探本阶段未代跑 Play；施工前后请按此表填空：

| 顺序 | 日志 / 观测 | v1 半施工后（预期） | v2 施工后（预期） |
|------|-------------|---------------------|-------------------|
| 1 | 村门 `House_Npc45` EnterDoor | `[SceneChangeDoor] Enter ... House_Npc45` | 同左 |
| 2 | LoadScene | → `Village_HomeScene45` | 同左 |
| 3 | `[VillageHomeScene45Debug] lastScene=` | **`Village_KenMuNi1`** | 同左 |
| 4 | 玩家 `transform.position` | **x≈-5.4** ❌ | **x≈-24.1** ✅ |
| 5 | 意外 RightDoor Enter | **可能出现** → 闪回 | **不应出现** |
| 6 | 第二条 LoadScene | → `Village_KenMuNi1` | 仅主动走向右门时出现 |

**施工员建议临时日志**（验收后可删）：在 `SetPlayerPos` 返回前 `Debug.Log($"[EnterPosDebug] pos={playerLogic.transform.position} lastScene={lastSceneName}")`。

---

## ④ 给程序看的补充

### 4.1 落点与 Trigger 几何（Gizmos）

**RightDoor Trigger 世界 X 范围（YAML 推算）**：

- `MapRight` (18.36) + `RightDoor` (-18.16) = **0.20**
- Collider Offset **-1.467**, Size **2.316** → AABB X ≈ **[-2.42, -0.11]**

| 节点 | 世界坐标 (x, y) | 距 Trigger 最近边（水平） | 与 HomeScene2 对拍 |
|------|-----------------|---------------------------|---------------------|
| **`EnterFrom_Village`（进屋 EnterPos）** | **(-5.41, -3.65)** ❌ | **~3.0** | HomeScene2：**(-24.12, -3.65)** → **~22** ✅ |
| `RightBorn` | (-5.82, -3.65) | ~3.4 | Map 元数据，勿作进屋点 |
| `DefaultBornPos`（fallback） | (-5.73, -3.65) | ~3.3 | **须一并左移** |
| HomeScene1 `RightBorn`（稳） | (-6.91, -3.65) | ~3.5+ | 1 号屋 MapRight=28.8，门更远 |

**Gizmos 验收**：选中 `Map/MapRight/RightDoor` 看 Collider 绿框；`EnterFrom_Village` 应在框**左侧 ≥15 单位**外（对齐 -24.12）。

### 4.2 LastSceneName / EnterPos（R2）

| 检查项 | 现网 |
|--------|------|
| `EnterPosConfig.lastScene` | **`Village_KenMuNi1`** ✅ 仅一条 |
| 进村写入 `LastSceneName` | `ChangeSceneComponentGM.LoadScene` 卸载前 `lastSceneName = nowSceneName` → 应为 **`Village_KenMuNi1`** |
| 旧字符串 `Village_HomeScene3` / `HomeScene45` | **EnterPos 无条目**；若存档 lastScene 脏可能 fallback |
| fallback 落点 | **`DefaultBornPos` (-5.73)** ❌ 仍在门区 |

**R2 裁定**：**次要风险**（读档/旧存档）；主因仍是 **R1b 坐标**。即使 EnterPos 命中，现网落点仍在门区。

### 4.3 存档路径（R3）

`BaseGameSceneManager.InitPlayer`：`archiveStart == true` 时 **跳过 EnterPos**，用 `PlayerSceneData.pos`。

| 情况 | 闪回概率 |
|------|----------|
| 新游戏进村 | 走 EnterPos → 现网仍 **-5.41** → **高** |
| 读档在屋内门口旧坐标 | **跳过 EnterPos** → 若 pos 在门区 → **高** |
| 读档在安全区再进村 | 取决于往返链 |

**解释「有概率」**：新游戏 vs 读档、是否按住右方向键、物理步长不同。

### 4.4 室内全域换场源（R7）

`Village_HomeScene45.unity` 内 **`SceneChangeDoor` 仅 2 处**：

| 门 | Enabled | NextSceneName | TriggerWhenMoveIn |
|----|---------|---------------|-------------------|
| LeftDoor | **0** | 空 | 0 |
| RightDoor | **1** | `Village_KenMuNi1` | **1** |

`Object` 下 **无** 第二道回村门。**R7 排除**。

### 4.5 村侧连触（R5 / R8）

| 项 | 现网 |
|----|------|
| `House_Npc45`（`Stairs.prefab`） | **`TriggerWhenMoveIn: 0`**（按 E 进村，非走进触发） |
| `ExitFrom_HomeScene45` | **(-4.3, 7.41)**，EnterPos 已绑；**出村落点** |
| `House_Npc45` 位置 | **(-4.39, 5.67)** |

**R5 低**（村门非走进连触）。**R8** 仅当用户描述含「出村后异常」时查；本期「进屋闪回」**优先 R1b**。

### 4.6 首帧位移（R6）

`TownPlayerLocomotion` 可能在首帧微调刚体；**仅当坐标已修到 -24 仍闪回**时再深挖。**本轮不优先**。

### 4.7 `Map.leftBornTsf` 空引用

`Map.cs` `FindObject()`：`leftBornTsf = transform.Find("LeftBorn")`。节点已改名 **`EnterFrom_Village`** → 运行时 **Find 失败**，序列化 **`leftBornTsf: {fileID: 0}`**。

- **不影响** `BaseGameSceneManager.SetPlayerPos`（走 SceneManager `EnterPosConfig` 显式引用）。
- **会影响** Map Start 警告、以及依赖 `leftBornTsf` 的其它逻辑。
- **施工**：Inspector 手动将 `Map.leftBornTsf` 拖至 **`EnterFrom_Village`**。

### 4.8 方案对比

| 方案 | 说明 | 裁定 |
|------|------|------|
| **A′（推荐）** | `EnterFrom_Village` → **(-24.12, -3.65)**；`DefaultBornPos` 同步左移；`Map.leftBornTsf` 手绑 | ✅ **本期必做** |
| B | 外移/缩小 RightDoor Collider | 备选；A′ 未修再考虑 |
| C | `SceneChangeDoor` 免疫 N 帧（C#） | 回归面大；**不优先** |
| D | RightDoor 改按 E | 与 1/23 不一致；**不优先** |
| E | 补 EnterPos 旧 lastScene 字符串 | 仅当 R2 日志证实；**次要** |

### 4.9 最小施工步骤（方案 A′）

1. 打开 `Village_HomeScene45.unity`。  
2. 选中 **`Map/EnterFrom_Village`**：`localPosition` → **(-24.12, -3.65, 0)**（对齐 HomeScene2）。  
3. 选中 **`Map/DefaultBornPos`**：同步移至 **(-24.12, -3.65)** 或同安全区（避免 R2 fallback 踩门）。  
4. **`Map` 组件**：`leftBornTsf` **手动拖入** `EnterFrom_Village`（勿依赖 Find LeftBorn）。  
5. 确认 **SceneManager** `EnterPosConfig` `Village_KenMuNi1` → pos 仍绑 **`EnterFrom_Village`**（勿改回 RightBorn）。  
6. **保留** `RightBorn` (-5.82) 作 `rightBornTsf` 元数据；**不改** RightDoor。  
7. Play：§3 验收 + 打印落点坐标 + Console 过滤。

### 4.10 最小改动文件

| 文件 | 动作 |
|------|------|
| `Village_HomeScene45.unity` | 挪 `EnterFrom_Village` + `DefaultBornPos`；绑 `Map.leftBornTsf` |
| `SceneChangeDoor.cs` | **不改**（除非 A′ 后仍闪回） |
| `Village_KenMuNi1.unity` | **不改**（进屋闪回） |

### 4.11 严禁

- 未读坐标就写「EnterPos 已改绑，R1 已排除」  
- 把 `ExitFrom_HomeScene45` 当进屋闪回主因  
- 只改 EnterPos 引用、**不挪 Transform**  
- 禁用 RightDoor 作为最终方案（除非 A′ 验收失败后再议）

### 4.12 开放问题

见 `OPEN_QUESTIONS.md` §「Village_HomeScene45 进屋闪回村 · v2 · 2026-08-22」。

---

## 修订记录

| 日期 | 说明 |
|------|------|
| 2026-08-22 | v2.0 第二轮：证实 v1 半施工 R1b；A′ 挪坐标 + DefaultBornPos + leftBornTsf |
