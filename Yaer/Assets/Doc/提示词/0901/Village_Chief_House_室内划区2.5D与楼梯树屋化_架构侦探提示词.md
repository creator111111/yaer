# Cursor Agent Prompt · Village_Chief_House：室内划区开村式 2.5D + 楼梯（对齐树屋）

> **角色**：先【架构侦探】只读定方案，报告后再【施工员】  
> **日期**：2026-09-01  
> **场景**：`Assets/GameRes/Scenes/Village_Chief_House.unity`  
> **Hierarchy 锚点（用户红箭头）**：`Map → Design → 村长家合层 →` **`楼梯`**（美术合层；旁有 `古莎待机` / `村长`）  
> **产品设定（钉死 · 两阶段）**：  
> 1. **先**：在室内划定一块 **可走区域**，让玩家在该区内能像村里一样 **上下左右（A/D + W/S 纵深）** 移动——因为整屋室内默认不是村移动，且 **只有这一部分** 需要  
> 2. **再**：在该能力之上，**模拟/还原村内树屋楼梯** 那套走楼梯体验（DepthZone / 挡位 / 前后景），对准合层 **`楼梯`**  
> **样板**：`Village_KenMuNi1` 树屋楼梯 + `VillageWalkArea` + `TownPlayerLocomotion` + `VillagePlayerDepthZone`（及树屋双 Trigger 门控若适用）  
> **不是**：整屋所有民居 Home 一起改成村移动；不是只给楼梯贴个假 Collider 却不开纵深  
> **本阶段（侦探）**：只读；禁止改场景 / 代码  
> **报告落盘**：`Assets/Doc/执行文档/0901/Village_Chief_House_室内划区2.5D与楼梯树屋化_架构溯源报告.md`

把下面「侦探」整段复制给 Cursor Agent（Agent Mode）执行。施工 Prompt 见文末。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 产品白话

> 村长家里现在基本是「室内横移」；楼梯那段要能像村里树屋楼梯一样 **斜上斜下还带左右**。  
> 但整屋不要全开村移动——**先圈一块区域**开上下左右，再在这块里把 **楼梯功能** 接上。

### 期望两阶段（顺序钉死）

```
【阶段 A · 室内划区开 2.5D】
  设定 Walk / Locomotion 区域（多边形或 Trigger 区）
  → 玩家在区内：Village2_5D 语义（A/D 横移 + W/S 世界 Y 纵深）
  → 区外：保持现网室内手感（或不可走 / 被夹回区）——报告拍板

【阶段 B · 楼梯树屋化】
  对齐 KenMuNi1 树屋楼梯：
  → VillageWalkObstacle 挡位（方案1：脚本 Cast，非硬碰）
  → DepthZone（脚进区切玩家 Sorting Layer；必要时双 Trigger 门控）
  → 脚点 PlayerFoot；合层「楼梯」美术可保留，交互/挡位旁挂
```

### 现网硬约束（预扫 · 须证伪）

| 项 | 现状（助手预扫） | 含义 |
|----|------------------|------|
| `SetVillageExplorationMode` | **仅当**激活场景名 == `Village_KenMuNi1` 才开 | 进屋 `Village_Chief_House` **默认不开** Town / 纵深 |
| 民居 OPEN | 「家里不开 Town，没有纵深」 | 与本期产品冲突 → **须有意扩展**，勿静默全开所有 Home |
| `TownPlayerLocomotion` WalkArea | 按名找 **`VillageWalkArea`** Polygon（现逻辑偏 KenMuNi1） | 室内须有等价多边形 / 或扩查找场景白名单 |
| 合层 `楼梯` | `村长家合层` 内 **美术 SR** | **≠** 可走碰撞；须另挂 Walk/障碍/Zone |
| 场景已有 | `LayerArea` / `MapLimit` / `Ground` | 侦探须分清：遮挡层？边界？**不是**自动等于村 WalkArea |

### 阶段 A · 开 2.5D 方案倾向

| 方案 | 做法 | 倾向 |
|------|------|------|
| **A1 · 整场景开 Village2_5D + 窄 WalkArea** | 进 `Village_Chief_House` 即 `SetVillageExplorationMode(true)`；`VillageWalkArea` 多边形 **只罩** 楼梯+必要平台条带；其余地板用障碍/区外夹住 | ✅ 复用村链路最多；「只有这一部分」靠 **多边形形状** 表达 |
| A2 · 脚进 Trigger 才开 Town，出区关** | 区触发 `ApplyVillageMode`；进出切换 Default↔Village2_5D | ⚠️ 边界抖动、跳跃/动画模式切换风险高 |
| A3 · 新建「室内局部纵深」不复用 Town | 另写一套 Y 移动 | ❌ 禁止叠床架屋 |
| A4 · 所有 HomeScene 一并开 Town | — | ❌ 超出本期；OPEN 明示不做 |

**Y 标尺**：村用场景物体注入 depth min/max；室内须自备标尺或写死相对楼梯高度——侦探对照 `TryInjectVillageDepthYBoundsFromSceneMarkers`。

### 阶段 B · 树屋楼梯对齐清单

| 村树屋能力 | 室内楼梯应对 |
|------------|--------------|
| `VillageWalkArea` 可走带 | 阶段 A 多边形覆盖楼梯斜面/平台 |
| `VillageWalkObstacle` + 方案1 Cast | 楼梯边/栏杆挡位（Trigger 查询） |
| `VillagePlayerDepthZone` | 上下层前后景 Sorting |
| `VillageTreehouseDepthZoneGate` 双 Trigger | **仅当**需要「上楼梯才开某层 Collider」时复用；否则可只摆 Zone |
| 合层美术 | 保留 `楼梯` SR；**勿**当唯一物理体 |

参考文档（必读）：

- `执行文档/5月/0513/树屋楼梯深度区域系统_架构逻辑分析_执行说明.md`
- `执行文档/5月/0513/树屋双触发顺序激活DepthZoneColliders_架构溯源与施工执行说明.md`
- `技术文档/村庄探索_Walk区楼梯与障碍_方案1_技术说明.md`
- `02_SYSTEM_SPEC` 村庄纵深 / Walk 动画契约

### 与合层美术的关系

| 物体 | 角色 |
|------|------|
| **`楼梯`**（红箭头） | **视觉锚点**；对齐斜面角度与可走带 |
| `内栏杆` / `外栏杆` | 可能需障碍或 Sorting；勿误删 |
| `LayerArea` | 先读现网用途，**勿假设**已是 WalkArea |

### 本期边界

| 做 | 不做 |
|----|------|
| ✅ Chief_House：**划区** + **楼梯树屋化** 两阶段方案与最小清单 | ❌ 同步改 HomeScene1/2/23/45 全开 Town |
| ✅ 复用 `TownPlayerLocomotion` / WalkArea / DepthZone / 障碍方案1 | ❌ 新造第三套移动 |
| ✅ 场景配置为主 + 必要白名单扩场景名 | ❌ 为楼梯改战斗场景重力 |
| ✅ 验收：区内 W/S+A/D；楼梯可上下不穿；区外符合决议 | ❌ Update 堆业务；抢写 Animator `Run` |

### 严禁

- 只开 `Village2_5D` 却不摆 `VillageWalkArea` → 全屋乱飞 Y  
- 合层 `楼梯` 上硬挂物理当唯一挡位且与 Town Cast 双拽  
- 把 DepthZone（改玩家 Layer）当成「能走路」的唯一手段  
- 未写清就改 `PlayerLogic` 场景名判断导致出村/进民居模式错乱  

### 开放（侦探须答）

| ID | 问题 | 助手倾向 |
|----|------|----------|
| Q1 | A1 整屋开 Town + 窄 WalkArea，还是 A2 进区才开？ | **A1**（稳）；若产品坚持「区外仍纯横移」再评 A2 |
| Q2 | WalkArea 物体名是否仍叫 `VillageWalkArea`？ | **是**（少改查找）；或扩查找支持场景自定义名 |
| Q3 | 是否需要树屋双 Trigger 门控？ | 先对拍 KenMuNi1 树屋；楼梯简单则可 **只 Zone+障碍** |
| Q4 | 区外地板：完全不能走 vs 仍可横移不能 W/S？ | **WalkArea 外夹死**（A1）或报告另定 |
| Q5 | 跳跃：区内是否禁跳（村规则）？ | ✅ 跟村：`isEnableJump=false` |
| Q6 | 与续聊黑幕换古莎、门进出是否冲突？ | 正交；进房落点须落在 WalkArea 内或可走到楼梯 |

---

## 侦探 Prompt（复制给 Agent）

```text
你是【架构侦探】。Unity 2020.3.48f1 / C#。禁止修改代码、场景、Prefab。默认中文。

@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Project_context.md
@Assets/Doc/02_SYSTEM_SPEC.md

## 产品目标（两阶段）
1. Village_Chief_House 内先设定区域，使玩家在该室内区域可像村里一样上下左右移动。
2. 再对齐村内树屋楼梯，让合层「楼梯」可走（纵深+横移+必要前后景）。
用户 Hierarchy 红箭头：村长家合层 / 楼梯。
整屋其它民居默认不动；禁止新造第三套移动。

## 必读（移动 / 树屋）
@Assets/Doc/02_SYSTEM_SPEC.md
@Assets/Doc/技术文档/村庄探索_Walk区楼梯与障碍_方案1_技术说明.md
@Assets/Doc/执行文档/5月/0513/树屋楼梯深度区域系统_架构逻辑分析_执行说明.md
@Assets/Doc/执行文档/5月/0513/树屋双触发顺序激活DepthZoneColliders_架构溯源与施工执行说明.md
@Assets/Scripts/Game/GameRuntime/Entities/Player/PlayerLogic.cs
（SetVillageExplorationMode / 场景名仅 KenMuNi1）
@Assets/Scripts/Game/GameRuntime/Entities/Player/Components/TownPlayerLocomotion.cs
@Assets/Scripts/Game/GameRuntime/Entities/Component/Physics/VillagePlayerDepthZone.cs
@Assets/Scripts/Game/GameRuntime/Entities/Component/Physics/VillageTreehouseDepthZoneGate.cs

## 必读（本场景）
@Assets/GameRes/Scenes/Village_Chief_House.unity
@Assets/ArtRes/Scene/Village/村长家合层.prefab
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Scene/Village_House/Village_Chief_HouseSceneManager.cs

对照样板场景树屋：
@Assets/GameRes/Scenes/Village_KenMuNi1.unity
（检索：VillageWalkArea、树屋、DepthZone、TreeDoor、VillageWalkObstacle、楼梯）

## 侦探任务
1. 证伪：Chief_House / 其它 Home 当前 LocomotionMode；为何不能 W/S。
2. 拍板阶段 A：A1/A2；WalkArea 形状相对「楼梯」；Y 标尺来源；场景白名单最小改法。
3. 拍板阶段 B：要对齐树屋的哪些组件（障碍 / DepthZone / 双门控）；合层「楼梯」只作美术锚点。
4. 画序列：进屋落点 → 走入划区 → 上楼梯 → Sorting/挡位 → 出门回村模式恢复。
5. 最小清单（场景物体表 + 允许改动的脚本白名单）+ 验收 + OPEN。
6. 写清不做：其它 Home 全开 Town；改战斗重力；Update 堆逻辑。

## 报告落盘
Assets/Doc/执行文档/0901/Village_Chief_House_室内划区2.5D与楼梯树屋化_架构溯源报告.md

结构：①结论 ②室内现网缺口 ③阶段A方案 ④阶段B树屋对齐 ⑤合层楼梯锚点
⑥进出房模式恢复 ⑦最小清单 ⑧验收 ⑨OPEN ⑩程序补充

沟通：①结论一句话 ②原因 ③用户检查清单 ④程序补充
```

---

## 施工 Prompt（报告拍板后另开 Agent 再复制）

```text
你是【施工员】。Unity 2020.3.48f1 / C#。最小化修改。默认中文。

@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0901/Village_Chief_House_室内划区2.5D与楼梯树屋化_架构溯源报告.md

## 目标
1. 阶段 A：按报告在 Village_Chief_House 划定可走区，启用村式上下左右（复用 TownPlayerLocomotion）。
2. 阶段 B：按报告对齐树屋楼梯（Walk 障碍方案1 + DepthZone 等），对准合层「楼梯」。
3. 仅本场景；禁止其它 Home 默认同开；禁止新造第三套移动；禁止抢写 Animator Run。
4. 进出房后 LocomotionMode 按报告正确恢复（回村仍 KenMuNi1 规则）。

## 落盘
Assets/Doc/施工说明/0901/Village_Chief_House_室内划区2.5D与楼梯树屋化_施工说明.md
同步 OPEN_QUESTIONS。

## 验收
- [ ] 划定区内：A/D + W/S 可用；手感接近村里
- [ ] 区外行为符合报告（夹死或仍横移）
- [ ] 楼梯可上下，不穿栏/不卡死；左右在斜面上可用
- [ ] DepthZone/前后景（若报告要求）进出正确
- [ ] 进村长家 / 回村：模式不串（村仍 2.5D，其它 Home 不被误开）
- [ ] 续聊 / 黑幕换古莎 / 门换场回归正常
- [ ] Console 无空引用；无每帧刷屏

## 沟通风格
①结论 ②原因 ③用户检查清单 ④程序补充
```

---

## 给开发者

1. **先跑侦探**——室内现在 **故意不开** 村移动（只认 `Village_KenMuNi1`），这是根因。  
2. 产品顺序：**先划区开 2.5D → 再上树屋楼梯件**；合层 `楼梯` 是美术锚点，不是碰撞本体。  
3. 倾向：**整屋开 Town + 很窄的 `VillageWalkArea` 只罩楼梯带**（少切换抖动），而不是整屋随意乱走。
