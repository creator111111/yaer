# Village_HomeScene45 — 进屋立刻闪回村 — 架构溯源报告 v3（终版）

**文档版本**：v3.0（2026-08-22 晚 · 终版）  
**文档性质**：【架构侦探】终版 + **施工执行说明**  
**Unity**：2020.3.48f1 / C#  
**Play 铁证**：`[SceneLoad] Village_HomeScene45` → 约 2s 后 `[SceneLoad] Village_KenMuNi1`；**取消勾选 RightDoor `SceneChangeDoor` 后不闪**  
**前序**：v1（R1 落点）、v2（R1b 半施工）— 均被 Play 推翻或降级为次要因素

关联：

- 对拍基准：`Assets/GameRes/Scenes/Village_HomeScene1.unity`（**无此问题**）
- 稳态参考：`Assets/GameRes/Scenes/Village_HomeScene2.unity`（`EnterFrom_Village` + 左门出）

---

## ① 结论一句话（真根因 **R0**）

**不是「进屋落点离门太近」，而是 `MapRight`/`RightDoor` 布局跑偏，使 `RightDoor` 的 `TriggerWhenMoveIn` 碰撞体世界 X 横跨原点 (~0)。玩家 `CreatePlayer` 异步实例化时默认在 **(0,0)**，`SetPos(EnterFrom_Village)` 尚未执行（或晚于首帧 `OnTriggerEnter2D`）→ 黑幕 hold 期间 `RightDoor` 抢先 `LoadScene(Village_KenMuNi1)` → 表现为闪回。HomeScene1 的门在 x≈**-1.87**，原点在其 Trigger **外侧**，故同款机制不触发。**

---

## ② 为什么 HomeScene1 没事、HomeScene45 有事

### 2.1 三门几何对拍（磁盘 YAML）

| 项 | **HomeScene1** ✅ | **HomeScene45** ❌ | **HomeScene2**（参考） |
|----|-------------------|---------------------|------------------------|
| `MapRight.localPosition.x` | **28.8** | **18.36** | **28.8** |
| `RightDoor.localPosition.x` | **-30.67** | **-18.16** | 0（右门 GO 禁用） |
| **门 pivot 世界 X** | **≈ -1.87** | **≈ 0.20** | — |
| Trigger Offset / Size | -1 / 2×20 | -1.47 / 2.32×20 | — |
| **Trigger 世界 X 约** | **[-3.9, -1.9]** | **[-2.4, -0.1]** | — |
| 玩家实例化默认点 | (0, 0) | (0, 0) | (0, 0) |
| **(0,0) 在 Trigger 内？** | **否**（0 > -1.9） | **是**（0 ∈ [-2.4,-0.1]） | — |
| 进村 EnterPos 绑 | `RightBorn` (-6.91) | `EnterFrom_Village` (-18.74) | `EnterFrom_Village` (-24.12) |
| 进村后是否闪 | 否 | **是** | 否 |

**示意图**：

```
世界 X 轴（俯视图）：

HomeScene1 门区：          HomeScene45 门区：
    Trigger                    Trigger
  [====]                         [====]
   -3.9  -1.9    0              -2.4  -0.1   0
                  ^玩家出生默认点^  ^玩家出生默认点^
                  （门外，安全）    （门内，立刻回村！）
```

### 2.2 时序（为何挪 EnterFrom 无效）

```
进村 LoadScene(HomeScene45)
  → 黑幕 onShowEnd → 卸载村 → 加载 45
  → GSM Awake → Map.OnInit → RightDoor 订阅 onEnterInteractiveEvent
  → CreatePlayer 异步 → 实体 Show 在 (0,0)          ← 关键
  → OnTriggerEnter2D：玩家 ∩ RightDoor Trigger     ← 早于或并行 SetPos
  → SceneChangeDoor.EnterDoor → LoadScene(KenMuNi1)  ← 第二条 [SceneLoad]
  → （可能）SetPos(EnterFrom_Village)                ← 来不及
  → Village_HomeScene45SceneManager 被销毁
  → 第一次换场 CloseBlackAndNotify 访问已销毁 manager → MissingReferenceException
```

代码锚点：

- 玩家创建：`PlayerHandlerComponentGSM.CreatePlayer` → `ShowPlayerEntity`（无初始坐标）
- 落点：`BaseGameSceneManager.InitPlayer` 回调内 `SetPlayerPos`
- 门触发：`SceneChangeDoor` + `TriggerWhenMoveIn=1` + `InteractiveComponent.onEnterInteractiveEvent`
- 二次换场日志：`LoadSceneComponentGSM.LoadScene` 行 35 `[SceneLoad]`

### 2.3 v1/v2 假设裁定

| 假设 | 裁定 |
|------|------|
| R0 原点踩门（spawn-at-origin） | ✅ **真根因** |
| R1/R1b EnterPos 离门太近 | ⚠️ 读档 `archiveStart` 时仍可能叠加；**正常进村主因是 R0** |
| 挪 EnterFrom 到场景中间 | ❌ 不能修：闪回发生在 SetPos 之前 |
| MissingReferenceException | 症状：二次换场销毁 GSM 后旧黑幕回调访问 manager |

---

## ③ 用户验收清单（施工后）

| # | 操作 | 通过 |
|---|------|------|
| 1 | **新游戏**进村 **20 次** | **0 次**闪回 |
| 2 | **读档**在村内再进 **20 次** | **0 次**闪回 |
| 3 | Console 过滤 `SceneLoad` | 每次进屋**仅 1 条** `Village_HomeScene45`，**无** 2s 内第二条 `Village_KenMuNi1` |
| 4 | Console 过滤 `SceneChangeDoor` | 进村后、未走向右门前 **无** `RightDoor Enter` |
| 5 | Scene Gizmos | 玩家实例化默认 (0,0) **不在** RightDoor 绿框内（可用临时日志 `transform.position` 验证） |
| 6 | 主动走向 RightDoor | 仍能正常出村到 `Village_KenMuNi1` |
| 7 | 无 `MissingReferenceException` on `Village_HomeScene45SceneManager` | ✅ |

### Console 过滤

`SceneLoad` · `SceneChangeDoor` · `VillageHomeScene45Debug` · `MissingReferenceException`

---

## ④ 给施工员的修复说明

### 4.1 推荐方案 **A**（场景对齐 HomeScene1，本期必做）

**只改** `Assets/GameRes/Scenes/Village_HomeScene45.unity`。

| 步骤 | 对象 | 动作 | 目标值（对齐 HomeScene1） |
|------|------|------|---------------------------|
| 1 | `Map/MapRight` | `localPosition.x` | **28.8**（现 18.36） |
| 2 | `Map/MapRight/RightDoor` | `localPosition.x` | **-30.67**（现 -18.16） |
| 3 | `Map/MapRight/RightWall` | 随 MapRight 一并右移保持相对关系 | 参考 HomeScene1 差值 |
| 4 | `Map/EnterFrom_Village` | 进村落点 | **(-24.12, -3.65, 0)**（对齐 HomeScene2） |
| 5 | `Map/DefaultBornPos` | fallback 落点 | **(-24.12, -3.65, 0)** |
| 6 | `Map` 组件 | `leftBornTsf` 手绑 | → `EnterFrom_Village` |
| 7 | SceneManager `EnterPosConfig` | `Village_KenMuNi1` → pos | 保持绑 **`EnterFrom_Village`** |
| 8 | `RightDoor` | **保持** `SceneChangeDoor` Enable、`TriggerWhenMoveIn=1`、`Next=Village_KenMuNi1` | 勿再禁用 |

**验收几何**：RightDoor Trigger 世界 X 应 ≈ **[-3.9, -1.9]**，**(0,0) 在其右侧外**。

> **原因**：步骤 1–2 把门从原点挪走，从根上消除 spawn-at-origin 踩门；步骤 4–5 对齐可玩民居惯例。

### 4.2 备选方案 **B**（仅当改布局后美术穿帮）

缩小 `RightDoor` BoxCollider2D 的 X（勿盖住 x=0），或把 Collider Offset 再向左偏 ≥2 单位。  
**优先 A**；B 易与美术门洞错位。

### 4.3 备选方案 **C**（代码层，本期不优先）

`SceneChangeDoor` 增加「场景进入后 N 帧 / 黑幕结束前不响应 `TriggerWhenMoveIn`」。  
回归面大；**先 A，A 失败再议**。

### 4.4 备选方案 **D**（LoadSceneComponentGSM 防御）

`CloseBlackAndNotify` 回调访问 `manager` 前判空 / 用 `GameManager.GetGameSceneManager()` 取当前 GSM。  
**建议顺手修**（消 MissingReferenceException），**不能单独止闪回**。

### 4.5 严禁

- 长期禁用 `RightDoor.SceneChangeDoor` 作为最终方案（已证实能止闪，但无法出村）
- 只挪 `EnterFrom_Village`、不改 `MapRight`/`RightDoor`（v2 已证无效）
- 把问题归因于「玩家走太快」而不改场景

### 4.6 顺带清理（非进屋闪回主链，可同 PR）

| 项 | 位置 | 说明 |
|----|------|------|
| `Village_KenMuNiStart` Missing Prefab | `Village_KenMuNi1.unity` | GUID `397659d3...` 断链，重绑 `Assets/GameRes/Prefabs/Dialogue/Village_KenMuNiStart.prefab` |
| `Arrive_Village_KenMuNi1` Tips 图缺失 | Tips 资源配置 | 回村后 Console 报错，不影响闪回判定 |

### 4.7 最小改动文件

| 文件 | 动作 |
|------|------|
| `Village_HomeScene45.unity` | 方案 A 步骤 1–8 |
| `LoadSceneComponentGSM.cs` | 可选：方案 D |
| `SceneChangeDoor.cs` | **本期不改**（除非 A 失败） |

---

## ⑤ Play 铁证摘录（2026-08-22 18:46）

```
[SceneLoad] scene=Village_HomeScene45 blackFade=True from=LoadSceneComponentGSM
（约 2 秒后）
[SceneLoad] scene=Village_KenMuNi1 blackFade=True from=LoadSceneComponentGSM
```

用户实验：**取消勾选 `RightDoor.SceneChangeDoor` → 不闪**。与 R0 一致。

---

## 修订记录

| 日期 | 说明 |
|------|------|
| 2026-08-22 | v3 终版：R0 原点踩门；对拍 HomeScene1；方案 A 对齐 MapRight/RightDoor |
