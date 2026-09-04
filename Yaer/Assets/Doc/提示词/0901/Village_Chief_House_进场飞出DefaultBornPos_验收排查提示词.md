# Cursor Agent Prompt · Bug：进村长家不在 DefaultBornPos / 玩家飞出

> **角色**：先【验收员 / 架构侦探】只读复现 + 定根因（可加短诊断日志），报告拍板后再【施工员】最小修复  
> **日期**：2026-09-01  
> **现象（用户测试）**：一进入 `Village_Chief_House`，玩家**不在** Hierarchy 所指的 **`Map/DefaultBornPos`**，并会**直接飞出去**  
> **用户 Hierarchy 锚点**：`Village_Chief_House / Map / DefaultBornPos`（红箭头）  
> **关联施工**：0901 室内划区 2.5D（`VillageWalkArea` + DepthY + Obstacles）刚上线；用户刚手调过 WalkArea 一带  
> **本阶段**：排查定根因；禁止大范围重构  
> **报告落盘**：`Assets/Doc/执行文档/0901/Village_Chief_House_进场飞出DefaultBornPos_验收排查报告.md`

把下面「排查」整段复制给 Cursor Agent（Agent Mode）。修复 Prompt 见文末。

---

## 提示词助手预梳理（排查须核实，勿当唯一真相）

### 产品期望（与用户表述对齐）

| 项 | 钉死 |
|----|------|
| 稳定落点 | 进村长家后人物停在**合理进门位**，镜头跟得上，**不一帧飞到远处 / 楼梯顶 / 地图外** |
| `DefaultBornPos` | 用户认为应对齐此 Transform；侦探须裁定：**从村进屋是否本就应走它**，还是应走 `EnterFrom_Village`，抑或两者应对齐到同一世界坐标 |
| 可走区 | 落点必须落在生效 `VillageWalkArea` **形内**（或深入区内），否则 Town 模式 `ClosestPoint` 会校正 |

### 现网进场落点链（预扫 · 磁盘）

```
CreatePlayer
  → archiveStart? → 存档 pos
  → else SetPlayerPos：
       EnterPosConfig 命中 lastScene
         → Village_KenMuNi1 → EnterFrom_Village (17.42, -3.65)   ← 从村进屋主路径
       未命中 / 无表
         → Map.defaultBornTsf = DefaultBornPos (磁盘约 17.1, -6.61)
  → 白名单开村模式 → 绑 VillageWalkArea → DepthY 注入
  → Flush / 每帧 ClosestPoint + 障碍保险
```

| 物体 | 磁盘预扫坐标 | 用途 |
|------|--------------|------|
| **`EnterFrom_Village`** | **(17.42, -3.65)** | `EnterPosConfig[lastScene=Village_KenMuNi1].pos` |
| **`DefaultBornPos`** | **(17.1, -6.61)** | 仅无匹配 EnterPos / 空表时的默认出生 |
| **`VillageWalkArea`** | 用户手调多边形（点集已与 Setup 默认不同） | Town `ClosestPoint` 夹区 |
| RightDoor | **Inactive** | R0 踩门飞出概率低（仍须证伪） |
| LeftDoor | Active；`TriggerWhenMoveIn:0`；`EnterPosKey=Village_Chief_House_Door` | 出门键；走进不自动换场 |

**关键预判（须证伪）**：从村进屋 **本来就不会** 用 `DefaultBornPos`，而是 `EnterFrom_Village`。若用户把 `DefaultBornPos` 挪到「正确位置」却不改 `EnterFrom_Village` / EnterPos，会感觉「不在 DefaultBorn」。  
同时：**手调后的 WalkArea** 点集右缘多在 Y≈−7 一带，而 `EnterFrom_Village` Y=−3.65 —— **高度可疑落在多边形外** → ClosestPoint / 凹角校正表现为「飞出去」。施工说明已写：「落点被吸到奇怪边角：扩大 WalkArea 使 EnterFrom 深入区内」。

### 关键假说（按优先级）

| ID | 假说 | 预扫线索 | 怎么证伪 |
|----|------|----------|----------|
| **H1** | **落点在 WalkArea 外** → ClosestPoint / 凹角策略把人一帧吸到远处边 | 手调多边形；EnterFrom Y 与底带 Y 错位 | Pause：脚位 vs Polygon Gizmo；打日志校正前后坐标与距离 |
| **H2** | 用户期望错位：应落 **EnterFrom**，不是 DefaultBorn；DefaultBorn 与 EnterFrom **Y 差约 3** | EnterPos 表已绑 EnterFrom | 对照 LastSceneName + SetPlayerPos 实际目标 |
| **H3** | `archiveStart` 用了脏存档坐标（村/2 楼高度）进房 → 再被 WalkArea 拉飞 | `ProcedureComponentGM.archiveStart` | 关读档 / 新进房对比 |
| **H4** | CreatePlayer 短暂 (0,0) 踩门 / 触发换场（R0 类） | Home 曾踩 RightDoor；本场景 RightDoor 关 | Console 是否双 SceneLoad；进房瞬间门回调 |
| **H5** | DepthY Min/Max 与落点冲突 + Flush 连踢 | DepthY_Min≈−5.25；EnterFrom Y=−3.65 尚可；DefaultBorn Y=−6.61 **可能低于 Min** | 注入后 Y 是否被 Clamp 出 Walk |
| **H6** | `VillageWalkObstacles` 与落点重叠 → 保险法向推出「飞」 | Outer/Inner/StairsSide | 落点 Overlap 障碍？ |
| **H7** | 楼梯顶换场门 Trigger 盖住进门带误触发 LoadScene | 0901 楼梯门 | 进房瞬间是否二次 Load |
| **H8** | 重力/战斗模式未切 Town，竖直坠出 | 白名单 `IsVillageExplorationScene` | 进房后是否 VillageExplorationMode |

### 产品裁定建议（侦探报告必写，施工跟）

| 问题 | 建议默认（可改口） |
|------|-------------------|
| 从村进屋落哪？ | **以 `EnterFrom_Village` 为准**（现网 EnterPos）；若产品坚持「就是 DefaultBorn」，则 **把 EnterPos.pos 改绑 DefaultBorn，或把两 Transform 对齐同一世界坐标** |
| 飞出怎么修？ | **优先保证落点深入 WalkArea 形内**（扩/移多边形或移落点）；禁止关掉 ClosestPoint 当「修复」 |
| DefaultBorn 用途 | 保留作无 lastScene 匹配时的兜底；建议与进门位 **同区同高**，避免读档/兜底再飞 |

### 本期边界

| 做 | 不做 |
|----|------|
| ✅ 定为何不在用户期望点 + 为何飞 | ❌ 重做整屋 WalkArea 系统设计 |
| ✅ 对齐 EnterFrom / DefaultBorn / WalkArea | ❌ 改 KenMuNi1 `VillageWalkArea2` 形状 |
| ✅ 可加 `[ChiefEnterPos]` / `[VillageWalk]` 短日志 | ❌ 改续聊 / 古莎换人 / 出屋送树屋台本 |
| ✅ 最小场景或最小代码（仅若时序 bug） | ❌ 关村模式白名单逃避症状 |

### 严禁

- 未量「校正前后世界坐标」就猜修脚位  
- 用 `SetVillageWalkAreaOverride(null)` / 关 ClosestPoint 掩盖区外落点  
- 把「飞出」当成纯相机问题却不查脚根 Transform  

---

## 排查 Prompt（复制给 Agent）

```text
你是【验收员 + 架构侦探】。Unity 2020.3.48f1 / C#。
默认只读排查；允许添加可开关短诊断日志（建议标签 [ChiefEnterPos] / [VillageWalk]）。
禁止大范围重构。默认中文。

@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0901/Village_Chief_House_室内划区2.5D与楼梯树屋化_架构溯源报告.md
@Assets/Doc/施工说明/0901/Village_Chief_House_室内划区2.5D与楼梯树屋化_施工说明.md

## 现象
进入 Village_Chief_House 后，玩家不在 Map/DefaultBornPos，并直接飞出可玩区/镜头外。

## 必读
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Base/BaseGameSceneManager.cs
（InitPlayer / SetPlayerPos / EnterPosConfig）
@Assets/Scripts/Game/GameRuntime/Entities/Component/Map/Map.cs
@Assets/Scripts/Game/GameRuntime/Entities/Player/Components/TownPlayerLocomotion.cs
（WalkArea 绑定、ClosestPoint、凹角上限、障碍保险、Flush）
@Assets/Scripts/Game/GameRuntime/Entities/Player/PlayerLogic.cs
（DepthY 注入、VillageExploration）
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Scene/Village_House/Village_Chief_HouseSceneManager.cs
@Assets/GameRes/Scenes/Village_Chief_House.unity
@Assets/Editor/Tool/Scene/ChiefHouseIndoor25DSetupEditor.cs

检索：EnterFrom_Village、DefaultBornPos、EnterPosConfig、VillageWalkArea、
ClosestPoint、FlushAuthoritative、archiveStart、maxCorrectionDistance、
VillageWalkObstacle、IsVillageExplorationScene。

## 排查任务
1. 写清复现：从村门口进屋 / Loading 进房 / 读档进房，三种是否都飞。
2. 记录进场瞬间：LastSceneName、archiveStart、SetPlayerPos 目标 Transform 名与世界坐标、DefaultBornPos / EnterFrom_Village 世界坐标。
3. 判定 EnterFrom / DefaultBorn 是否在 VillageWalkArea.OverlapPoint 内；打印校正前后脚位与位移距离。
4. 按 H1～H8 填「成立/否/证据」。
5. 裁定：用户期望应对齐哪一锚点；飞出的唯一主因；最小修复清单（场景点 / EnterPos 绑点 / 多边形 / 代码时序）。
6. 若需日志：列出插点，勿刷屏。

## 报告落盘
Assets/Doc/执行文档/0901/Village_Chief_House_进场飞出DefaultBornPos_验收排查报告.md

结构：①结论一句话 ②复现 ③假说表 ④证据（坐标表）⑤主因 ⑥最小修复清单 ⑦验收 ⑧OPEN ⑨程序补充

沟通：①结论 ②原因 ③用户检查清单 ④程序补充
```

---

## 修复 Prompt（根因拍板后另开 Agent 再复制）

```text
你是【施工员】。Unity 2020.3.48f1 / C#。最小化修改。默认中文。

@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0901/Village_Chief_House_进场飞出DefaultBornPos_验收排查报告.md
@Assets/Doc/施工说明/0901/Village_Chief_House_室内划区2.5D与楼梯树屋化_施工说明.md

## 目标
进 Village_Chief_House 后人物稳定停在报告裁定的进门锚点（EnterFrom_Village 与/或 DefaultBornPos 对齐），
不出现一帧飞出 / ClosestPoint 跨图吸走；落点深入 VillageWalkArea 形内。

## 约束
- 禁止关闭 ClosestPoint / 取消 Chief_House 村模式白名单来「修好飞出」
- 禁止改 VillageWalkArea2（KenMuNi1）点集
- 禁止改续聊 / 古莎换人 / 出屋送树屋台本逻辑（除非报告证明踩门二次 Load）
- 场景改动优先于大段 locomotion 重写；若动 TownPlayerLocomotion 仅限进场 Flush 时序且须说明原因
- 保留关键诊断日志直至验收通过（可开关）
- 生成代码须含详细注释；重要修改解释原因

## 落盘
Assets/Doc/施工说明/0901/Village_Chief_House_进场飞出DefaultBornPos_修复施工说明.md
同步 OPEN_QUESTIONS.md。

## 验收
- [ ] 从 Village_KenMuNi1 进屋：落在裁定锚点附近（±合理误差），不飞
- [ ] Hierarchy：脚位与 EnterFrom_Village（或已对齐的 DefaultBornPos）一致
- [ ] OverlapPoint(落点) == true；Console 无跨图级校正位移
- [ ] A/D + W/S 仍仅在划区内；楼梯可走；障碍仍挡
- [ ] 读档进房 / 无 EnterPos 匹配兜底 DefaultBorn：不飞
- [ ] 续聊 / 换古莎 / LeftDoor 出门 EnterPosKey 回归

## 沟通风格
①结论 ②原因 ③用户检查清单 ④程序补充
```

---

## 给开发者（先自查 30 秒）

1. 从村进屋时看 Scene 视图：**脚在 `EnterFrom_Village` 还是 `DefaultBornPos`？**（现网 EnterPos 绑的是前者。）  
2. 打开 `VillageWalkArea` Polygon Gizmo：落点是否在**绿区内部**？在外面就会被吸走/飞。  
3. 若你手调过 WalkArea：把多边形盖住 `(17.42, -3.65)`，或把 `EnterFrom_Village`（必要时连同 `DefaultBornPos`）挪进区内。  
4. 过滤 Console：进房瞬间有无二次 `LoadScene` / 异常大位移日志。
