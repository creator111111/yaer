# Village_Chief_House · 大树进屋后向后走不转身 — 施工说明

**文档版本**：v1.0（2026-09-20）  
**文档性质**：【施工员】最小改动  
**Unity**：2020.3.48f1  
**侦探报告**：`Assets/Doc/执行文档/0920/Village_Chief_House_大树进屋后向后走不转身_架构溯源报告.md`

---

## 沟通摘要

### ① 结论一句话

**从 Idle 切进走路的那一帧，会按住的 A/D 再补一次转向，大树进屋立刻往回走也能翻面扬尘。**

### ② 原因（通俗）

人落地还在待机，一按 A 才切走路；切过去才开始听左右键，这一帧的 A 已经错过了，所以脸不转。现在进走路时主动看一眼还按着没有，有就补一次转向。W/S 上下走仍然不翻面。

### ③ 用户检查清单

1. 巨树 2 楼 `StairsDoor_BackToChief` → 村长家落地，**立刻点/按住 A**（往楼梯）。
2. 预期：脸朝左 + 有扬尘（不再偶发朝右蹭）。
3. 先按 D 走几步再按 A：仍翻面。
4. 只按 W/S：不翻面。
5. 村门进屋立刻按 A：同样应稳翻（同构修复）。

### ④ 程序补充

见下文。

---

## 改动清单

| 路径 | 改动 |
|------|------|
| `PlayerInputComponent.cs` | 新增 `ResolveVillageEnterHorizontalCommand`（队列→GetKey→Raw） |
| `HomeWalkState.cs` | Village2_5D 下 Enter 订阅+SetWalkSpeed 后补 MoveLeft/Right |
| `CombatRunState.cs` | 进跑补横向改为调用同一 Resolve，删私有重复队列扫描 |

**未改**：转身护栏、W/S 翻面、EnterPos、Animator、回程门坐标。

### 补丁（同日，倒着走仍在）

只在 `Enter` 补一次转向不够：村里横向速度按输入符号写，脸只在 `MoveLeft`/`MoveRight` 时才转。进门按住 A 没有新的 KeyDown，或队首不是左右时，人已经往左走、脸还朝右。

| 路径 | 补丁 |
|------|------|
| `TownPlayerLocomotion` | 写横向速度前，脸跟着同一个符号转；纯 W/S 不转 |
| `HomeWalkState` | 走路期间每帧用同一符号对齐；有横向意图就不退回待机 |
| `HomeIdleState` / `HomeBinkState` | 按住 A/D（队是空的也算）进入走路 |

---

## 为什么这样改

Idle→Walk 与 Combat Idle→Run 同构：订阅挂在 Enter，当帧 KeyDown 回调空跑。Combat 已有 Enter 补票；Home 对齐即可。抽共用 Resolve 避免两处优先级漂移。
