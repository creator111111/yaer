# Cursor Agent Prompt · 修复：进村长家落点被吸到楼梯上

> **角色**：先【验收员】用日志钉死时序（可短），再【施工员】最小修复  
> **日期**：2026-09-01  
> **现象（用户复测）**：进入 `Village_Chief_House` 后玩家**站在楼梯上面**（位置仍不对）  
> **已做过的修**：0901 将 `EnterFrom_Village` 对齐到 `DefaultBornPos≈(17.1,-6.61)` 进 WalkArea 底带（治「区外吸入底带」）；**未消除**「吸到楼梯」  
> **上游**：  
> - `执行文档/0901/Village_Chief_House_进场飞出DefaultBornPos_验收排查报告.md`  
> - `施工说明/0901/Village_Chief_House_进场飞出DefaultBornPos_施工说明.md`  
> **对话**：续聊 **不** SetPos；问题在进场落点 / 村模式 Flush  
> **报告落盘**：`Assets/Doc/执行文档/0901/Village_Chief_House_进场落点吸到楼梯_验收排查报告.md`  
> **施工落盘**：`Assets/Doc/施工说明/0901/Village_Chief_House_进场落点吸到楼梯_修复施工说明.md`

把「验收」段先复制给 Agent；根因拍板后把「施工」段另开或续跑。

---

## 提示词助手预梳理（须再证，勿当唯一真相）

### 产品期望

| 项 | 钉死 |
|----|------|
| 落点 | 从村 / Loading 进屋后，脚停在 **`EnterFrom_Village`（≈进门底带）**，**不得**停在合层「楼梯」斜面/上层 |
| 对白 | 续聊开始时脚位已正确；对白 Prefab **禁止**另写站位除非报告证明必须 |
| 保留 | WalkArea / W/S / ClosestPoint 机制本身；村街 KenMuNi1 手感 |

### 现网落点链（已知）

```
CreatePlayer → PlayerLogic.OnInit
  → RefreshVillageExplorationFromActiveScene()     // 常在默认位/原点
  → SetVillageExplorationMode + Flush + ClosestPoint  // ⚠ 可能已吸到楼梯带
→ InitPlayer 回调 SetPlayerPos
  → EnterPos(KenMuNi1) → SetPos(EnterFrom_Village) // 只改 transform.position
→（Loading 结束）再 RefreshVillageExploration + Flush
→ OnEnterScene → TriggerStory 续聊（不改坐标）
```

| 锚点 | 磁盘约 | 角色 |
|------|--------|------|
| `EnterFrom_Village` / `DefaultBornPos` | **(17.1, -6.61)** | EnterPos 目标 / 兜底（已对齐） |
| WalkArea 楼梯段 | 约 **x∈[-14,-3]、y 上探到 ~3** | ClosestPoint 从原点易吸到此 |
| 合层「楼梯」美术 | 合层根≈(-13.9,-7.7)+楼梯 local | 用户观感「站楼梯上」 |

### 关键假说（本轮优先）

| ID | 假说 | 预扫 | 证伪 |
|----|------|------|------|
| **H1** | **OnInit 在 SetPos 前 Flush**：脚在 (0,0)/默认位 → ClosestPoint **最近边=楼梯段** | OnInit L171 即 Refresh；楼梯点在多边形左上 | 日志：Flush 前坐标、校正后坐标 |
| **H2** | **`SetPos` 只写 Transform，不写 `Rigidbody2D.position` / Town 权威 Y** → 下帧/再 Flush 用 Rb 把人拉回楼梯 | `PlayerLogic.SetPos` 仅 `transform.position`；Walk 校正读 `_playerRootRb2D.position` | SetPos 后对比 transform vs Rb vs foot |
| **H3** | `LoadingSceneEndHandle` 再次 Refresh/Flush 在错误 Rb 上重夹 | `PlayerLogic` Loading 结束路径 | Loading 结束前后脚位 |
| **H4** | EnterFrom 仍形外 / 多边形又改 → 吸到楼梯边（非原点） | 现网 (17.1,-6.61) 预扫应在底带内 | OverlapPoint(EnterFrom) |
| **H5** | `archiveStart` 脏档已在楼梯高度 | InitPlayer 分支 | archiveStart 开关对比 |
| **H6** | 障碍保险从进门推到楼梯 | 障碍在楼梯侧 | 进门 Overlap 障碍？ |

### 方案倾向（施工默认，侦探可改口）

| 方案 | 做法 | 倾向 |
|------|------|------|
| **F1 · 进场权威落点 API** | 新增或扩展：`SetPos` 在 Village2_5D 下同步 **Transform + Rigidbody2D.position + Town `_villageWorldY`**，再可选 **一次** Flush；或 `TownPlayerLocomotion.TeleportAuthoritativeVillagePos` | ✅ 根治 H2；惠及所有村模式传送 |
| **F2 · 推迟首次 Flush** | OnInit 开模式时 **不** ClosestPoint；等 `SetPlayerPos` / LoadingEnd 后再 Flush | ✅ 治 H1；须防首帧漏夹 |
| **F3 · Chief GSM 落点后强制再 Flush** | `SetPlayerPos`/`OnEnterScene` 末调用权威 Teleport+Flush | ⚠️ 可作兜底；勿只治一家而忽略全局 SetPos |
| F4 · 关 ClosestPoint / 撤白名单 | 症状掩盖 | ❌ |
| F5 · 只改楼梯多边形挖掉上段 | 不修时序，原点仍可能吸别处 | ❌ 单独不够 |

**推荐组合：F1（必做）+ 验收 H1 后必要时 F2（OnInit 首次跳过多边形校正，或「无有效落点前不 Flush」）。**  
F3 可作 Chief 双保险，但注释写明原因：应对进房 Loading 二次 Refresh。

### 本期边界

| 做 | 不做 |
|----|------|
| ✅ 钉死吸楼梯时序 + 最小代码修 | ❌ 重画整屋 WalkArea 当主修 |
| ✅ 同步 Rb / 权威 Y；进场后再 Flush | ❌ 改续聊台本站位 |
| ✅ 保留 `[ChiefEnterPos]` 或扩 `[VillageEnterFlush]` | ❌ 改 WalkArea2；关村模式 |
| ✅ 回归：村街传送、其它 Home、楼梯可走 | ❌ 全局关掉 ClosestPoint |

### 严禁

- 用「对话里再 SetPos」掩盖进场 bug  
- 只移 `EnterFrom` 坐标却不修 Transform/Rb 不同步（上一轮已移点，用户仍站楼梯）  
- 为修落点把 `villagePlanarMoveSpeed` / 室内降速案绑进本案  

---

## 验收 Prompt（复制给 Agent · 先跑）

```text
你是【验收员 + 架构侦探】。Unity 2020.3.48f1 / C#。
允许短诊断日志（[ChiefEnterPos] / [VillageEnterFlush]）。禁止大范围重构。默认中文。

@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0901/Village_Chief_House_进场飞出DefaultBornPos_验收排查报告.md
@Assets/Doc/施工说明/0901/Village_Chief_House_进场飞出DefaultBornPos_施工说明.md

## 现象
进 Village_Chief_House 后玩家站在楼梯上（非 EnterFrom_Village 进门位）。
续聊不负责站位。上一轮已把 EnterFrom 对齐到 (17.1,-6.61)。

## 必读
@Assets/Scripts/Game/GameRuntime/Entities/Player/PlayerLogic.cs
（SetPos、RefreshVillageExploration、OnInit、LoadingSceneEndHandle）
@Assets/Scripts/Game/GameRuntime/Entities/Player/Components/TownPlayerLocomotion.cs
（ApplyVillageMode、FlushAuthoritative、ApplyVillageWalkPolygonPostCorrection）
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Base/BaseGameSceneManager.cs
（InitPlayer / SetPlayerPos）
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Scene/Village_House/Village_Chief_HouseSceneManager.cs
@Assets/GameRes/Scenes/Village_Chief_House.unity
（EnterFrom_Village、VillageWalkArea 点集）

检索：SetPos、FlushAuthoritative、ClosestPoint、RefreshVillageExploration、
EnterFrom_Village、ApplyVillageWalkPolygonPostCorrection、Rigidbody2D。

## 任务
1. 复现：从 KenMuNi1 Loading/门进屋；Pause 看脚是否在楼梯美术上。
2. 按 H1～H6 证伪；打印时间线：OnInit Flush 前/后、SetPos 后 transform vs Rb、LoadingEnd Flush 后。
3. 确认 EnterFrom OverlapPoint；裁定主因（时序 / Rb 不同步 / 二者皆有）。
4. 推荐 F1/F2/F3 组合；写最小修复清单。

## 报告
Assets/Doc/执行文档/0901/Village_Chief_House_进场落点吸到楼梯_验收排查报告.md

沟通：①结论 ②原因 ③用户检查清单 ④程序补充
```

---

## 施工 Prompt（根因拍板后复制给 Agent）

```text
你是【施工员】。Unity 2020.3.48f1 / C#。最小化修改。默认中文。

@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0901/Village_Chief_House_进场落点吸到楼梯_验收排查报告.md
@Assets/Doc/施工说明/0901/Village_Chief_House_进场飞出DefaultBornPos_施工说明.md

## 目标
从村进入 Village_Chief_House 后，玩家稳定停在 EnterFrom_Village（进门底带），
不得被 WalkArea ClosestPoint 吸到楼梯段；续聊开始时脚位正确。

## 默认施工方向（若报告未改口）
1. **F1**：Village2_5D 下进场/传送 SetPos 必须同步 Rigidbody2D.position 与 Town 权威 Y（可抽 TeleportAuthoritative API）；注释写明原因（只改 transform 会被多边形校正打回）。
2. **按需 F2**：OnInit 首次开村模式时避免在「尚未 SetPlayerPos」前做跨图级 ClosestPoint（或延后到落点之后 Flush）。
3. **按需 F3**：Chief `SetPlayerPos` 成功后强制权威落点 + Flush 一次（双保险）。
4. 保留短日志直至验收；通过后可关。

## 约束
- 禁止关闭 ClosestPoint / 撤 Chief 白名单 / 改 WalkArea2 点集当主修
- 禁止用续聊 Action 传送掩盖
- 禁止绑室内降速、古莎换人、出屋送树屋
- 改 SetPos 全局行为时须回归：KenMuNi1 落点、BlackFadeTeleport、其它 Home
- 代码含详细注释；重要修改写原因；复杂逻辑注明替代方案（F2/F3）

## 落盘
Assets/Doc/施工说明/0901/Village_Chief_House_进场落点吸到楼梯_修复施工说明.md
同步 OPEN_QUESTIONS.md。

## 验收
- [ ] 从 KenMuNi1 进屋：脚在 EnterFrom≈(17.1,-6.61) 一带，目视不在楼梯上
- [ ] `[ChiefEnterPos]`：kind=EnterPos，distToTarget 小，OverlapWalkArea=True
- [ ] SetPos 后 transform 与 Rb 一致；Loading 结束后不被拉回楼梯
- [ ] 楼梯仍可走；WalkArea / 障碍仍有效
- [ ] 村街进出落点 / 其它 Home / 续聊换古莎回归

## 沟通风格
①结论 ②原因 ③用户检查清单 ④程序补充
```

---

## 给开发者（一句话）

进门点已经指对 **`EnterFrom_Village`**；人还会站楼梯，是因为 **开 2.5D 时先在原点被夹到楼梯，且 `SetPos` 没把刚体/权威坐标一起写上**。按本文施工 Prompt 修时序与权威传送即可。
