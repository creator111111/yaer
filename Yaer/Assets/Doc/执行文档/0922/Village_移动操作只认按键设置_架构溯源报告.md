# Village 移动操作只认按键设置（键族单通道）— 架构溯源报告

> 日期：2026-09-22  
> 角色：架构侦探（**只读**，未改代码 / 场景 / 设置资源）  
> Unity：2020.3.48f1  
> 范围：`Village_KenMuNi1` 与 `Village_Chief_House`（`Village2_5D`）；对照 Forest / 战斗 Default  
> 产品钉死：Left/Right → **键族** → 横移+纵深单通道；本期**不**加「向前/向后」设置槽  
> 提示词：`Assets/Doc/提示词/0922/Village_移动操作只认按键设置_架构侦探提示词.md`

---

## ① 结论一句话

产品「没对接设置」**成立**：设置只绑 Left/Right，村探索却用 **`GetAxisRaw` + 硬编码 WASD∪方向键`** 双通道，故村外两套都能动；纵深也不读键族。室内 Chief 已在 `Village2_5D` 白名单，与村外**同一套**读键——不是「缺室内 WASD 硬编码」，而是缺「按 Left/Right 推导键族后关掉另一族」。推荐 **方案 A**：公共解析键族 → 横/纵符号只认当前族；Town / HomeWalk / Intent 全改走它；**否决**室内补硬编码 WASD、否决设置 UI 加前后槽。

---

## ② 现网 vs 键族规则对照表

| 检查项 | 现网 | 键族规则期望 |
|--------|------|--------------|
| 设置 A/D 时方向键能否移 | **能**（`GetKey` 箭头 + `GetAxisRaw` 双族） | **不能** |
| 设置 ←→ 时 WASD 能否移 | **能**（同上） | **不能** |
| 设置 A/D 时 W/S 纵深 | **能**（轴 + 硬编码 W/S；与设置无关） | **能**（且仅 W/S） |
| 设置 ←→ 时 ↑↓ 纵深 | **能**（轴 + 硬编码箭头） | **能**（且仅 ↑↓） |
| `GetAxisRaw` 是否误开另一族 | **是**（Unity Horizontal/Vertical 默认两套都绑） | **不能**（关或按族过滤） |
| 室内与村外是否同一读键 | **是**（Chief 与 KenMuNi1 同 `Village2_5D` + `TownPlayerLocomotion` + `HasVillageExplore*`） | 必须同一 |
| 设置是否有 Forward/Back | **无**（仅 Left/Right/Squat…） | 本期不新增；纵深由键族推导 |
| 改键后热重载 | **弱**：`keyCodeToCmdDict` 仅在 `PlayerInputComponent.OnInit` 从设置灌入 | 改键后应立即重建 |

**裁定**：外边两套都能用 = Bug（双通道）；室内 WASD 废 ≠「再写死一套 WASD」。0901 已把 `Village_Chief_House` 纳入探索白名单；若设置已是 A/D 而室内仍完全无纵深，属同管线回归，应靠方案 A 统一修，**禁止**方案 B。

### 现网调用链（错在哪）

```
SettingsConfigData.KeyboardMouseInputConfig
  → Left/Right（默认 A/D）；无 Forward/Back；Squat 默认 S
  → PlayerInputComponent.OnInit → keyCodeToCmdDict（仅此时灌表）

村探索 Village2_5D：
  队列：只认设置 Left/Right（及被屏蔽的 Squat 等）
  另开旁路：
    HasVillageExploreHorizontalMoveIntent
      → 队列 + GetAxisRaw("Horizontal") + GetKey(A|D|←|→)   ← 双族
    HasVillageExploreVerticalMoveIntent
      → GetAxisRaw("Vertical") + GetKey(W|S|↑|↓)             ← 双族
    GetVillageExplore*Sign / IsVillageHorizontalKeyHeld       ← 同上
  TownPlayerLocomotion.OnFixedUpdate
      → 纵深加速度直接 Input.GetAxisRaw("Vertical")          ← 再开双族
  HomeWalk / Idle：HasVillageExploreDepthMoveIntent
      → Town.HasVillageDepthMoveForHomeStateMachine → 又读 Vertical 轴
```

战斗 / Forest（`LocomotionMode.Default`）：横移走命令队列；**不**走 `HasVillageExplore*`。方案 A 应只收紧村探索路径。

---

## ③ 键族识别与纵深推导设计

### 键族判定（从设置 Left / Right）

| 族 | 判定（建议精确集合） | 横移 | 纵深推导 |
|----|----------------------|------|----------|
| **WASD 族** | Left∈{A} 且 Right∈{D}（或二者均属 `{W,A,S,D}` 且为左右对） | 仅 A / D（及表内 Left/Right） | **W 上、S 下** |
| **方向键族** | Left∈{LeftArrow} 且 Right∈{RightArrow} | 仅 ← / → | **↑ 上、↓ 下** |
| **Custom**（J/L 等） | 其余 | **仅**绑定的 Left/Right 键 | 纵深 **0**（或仅当产品后补规则）；验收可不测 |

不新增 `ControlInputType.Forward/Back`。

### 落地落点（最小面）

| 位置 | 改法 |
|------|------|
| `PlayerInputComponent` | 新增：`ResolveVillageMoveKeyFamily()`；`GetVillageExploreHorizontalSign/Intent`、`GetVillageExploreVerticalSign/Intent`、`IsVillageHorizontalKeyHeld`、`ResolveVillageEnterHorizontalCommand` **去掉**另一族 `GetKey` 与未过滤 `GetAxisRaw` |
| `TownPlayerLocomotion` | `OnFixedUpdate` / `HasVillageDepthMoveForHomeStateMachine` / `SyncWalkAnimatorParameter`：**禁止**裸 `GetAxisRaw`；改调 Input 推导符号/意图 |
| Axis | **默认关掉作移动真源**；若保留，必须按当前族过滤（否则必漏双通道） |
| 设置改键 | `SetKeyBinding` / 关设置面板后调用 **Rebuild** `keyCodeToCmdDict`（及缓存的 KeyFamily） |

室内外：无场景分叉；只要 `IsVillageExplorationScene` → 同一函数。

---

## ④ 方案对比与推荐

| 方案 | 做法 | 裁决 |
|------|------|------|
| **A** | 公共「Left/Right → 键族 → 横+纵符号」；村外室内只调它；去掉另一族硬编码与双轴 | **推荐** |
| **B** | 室内补 WASD 硬编码对齐村外双通道 | **否决**（违反单通道） |
| **C** | 设置 UI 增加向前/向后 | **本期否** |

### 必答

1. **Squat=S 与 WASD「S=向下」**  
   现网：`Village2_5D` 下 `IsBlockedInVillageExploration(Squat)` **已不入队下蹲**，S 主要喂 Vertical 轴纵深。  
   **推荐**：保持探索禁蹲（已有）；设置文案可提示村内 S=后退。勿用「两套都能用」回避。若产品坚持村内也要蹲：改默认蹲键（非 S）另票。

2. **战斗 / Forest Default**  
   不受纵深推导；继续只吃 Left/Right 队列。方案 A 改动锁在 `LocomotionMode == Village2_5D` 分支。

3. **改键热重载**  
   现网仅 `OnInit` 灌表 → 设置里改完回村可能仍用旧表。施工须加 Rebuild，验收项 7 才稳。

---

## ⑤ 要改文件（路径级；本阶段未改）

| 全路径 | 预期改动 |
|--------|----------|
| `…/PlayerInputComponent.cs` | 键族解析；收紧 Explore Intent/Sign；Rebuild 键表 API |
| `…/TownPlayerLocomotion.cs` | 纵深/Walk 意图改走推导，去掉裸 Axis |
| `…/SettingFormLogic.cs`（或 KeyBindingHelper） | 改键后触发 Rebuild |
| `ControlInputType.cs` / 设置 UI 槽位 | **不新增** Forward/Back |
| 各场景 YAML | **不改** |

施工说明建议：`Assets/Doc/施工说明/0922/Village_移动操作只认按键设置_施工说明.md`（侦探阶段不写施工）。

---

## ⑥ 验收表（村外 + 村长家室内相同）

| # | 设置 Left/Right | 操作 | 期望 |
|---|-----------------|------|------|
| 1 | A / D | 只按 A/D | 左右移 |
| 2 | A / D | 只按 W/S | 上下/纵深移 |
| 3 | A / D | 只按 ←→↑↓ | **完全不动** |
| 4 | ← / → | 只按 ←→ | 左右移 |
| 5 | ← / → | 只按 ↑↓ | 上下/纵深移 |
| 6 | ← / → | 只按 WASD | **完全不动** |
| 7 | A/D → 改成 ←→ 后立刻测 | 再按 WASD | 应失效；方向键生效 |
| 8 | 回归 | Forest 跳击、0920 进屋转身 | 不坏 |

---

## ⑦ 风险与回滚

| 风险 | 说明 | 回滚 |
|------|------|------|
| 关掉 Axis 后手柄/键位映射异常 | 本票主验键盘；手柄另开 | 族过滤 Axis 而非全关 |
| Custom 键无纵深 | 预期降级 | 产品后补推导规则 |
| Rebuild 漏调 | 验收 7 失败 | 关设置必调 / 进村 Refresh |
| 误伤战斗 | 若改到 Default 队列 | 用 LocomotionMode 门闸 |

**OPEN（仅蹲策略，不改键族表）**：村内是否永久禁蹲 vs 改默认蹲键——默认沿用现网探索禁蹲。
