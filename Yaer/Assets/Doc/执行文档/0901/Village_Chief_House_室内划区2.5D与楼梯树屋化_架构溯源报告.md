# Village_Chief_House — 室内划区 2.5D 与楼梯树屋化 — 架构溯源报告

**文档版本**：v1.0（2026-09-01）  
**文档性质**：【架构侦探】→ 施工已落地（见 `施工说明/0901/Village_Chief_House_室内划区2.5D与楼梯树屋化_施工说明.md`）  
**Unity**：2020.3.48f1  
**场景**：`Village_Chief_House.unity`  
**锚点**：`Map → Design → 村长家合层 → 楼梯`（美术 SR）  
**产品（两阶段）**：① 划区开村式 A/D+W/S → ② 对齐树屋楼梯（障碍方案1 + DepthZone）  
**提示词**：`提示词/0901/Village_Chief_House_室内划区2.5D与楼梯树屋化_架构侦探提示词.md`  
**样板**：`Village_KenMuNi1` + `TownPlayerLocomotion` + `VillageWalkArea` + DepthZone / Gate  

---

## 沟通摘要

### ① 结论一句话

**根因是村模式只认 `Village_KenMuNi1`，进屋永远 Default 无纵深；施工按 A1：白名单仅加 `Village_Chief_House`，摆窄 `VillageWalkArea`（含进门→楼梯条带）+ Y 标尺，再对楼梯挂方案1障碍与可选 DepthZone；合层「楼梯」只作美术锚点；其它 Home 一律不开。**

### ② 原因（通俗）

村里能上下走，是因为场景名叫肯姆尼村时程序才打开「村模式」。进了村长家名字变了，程序就当普通室内横移，W/S 没用。  
合层上的「楼梯」只是画出来的图，没有可走多边形和挡位，所以要对着树屋那套另摆碰撞区，不能指望贴图自己能走。

### ③ 用户检查清单（施工后）

| # | 操作 | 通过 |
|---|------|------|
| 1 | 进村长家，落点在划区内（或能走到楼梯带） | |
| 2 | 划定区内 A/D + W/S | 手感接近村里 |
| 3 | 区外 | 被夹回多边形（不能整屋乱飞） |
| 4 | 沿合层「楼梯」斜上斜下 | 可走；不穿栏/不卡死 |
| 5 | DepthZone（若摆了） | 进出前后景正确 |
| 6 | 回村 | 仍 KenMuNi1 2.5D；其它 Home 仍无纵深 |
| 7 | 续聊 / 黑幕换古莎 / 门换场 | 回归正常 |
| 8 | 区内 | 禁跳（跟村） |

### ④ 程序补充

见下文 §①～§⑩。

---

## ① 结论（程序向）

| 项 | 裁定 |
|----|------|
| 根因 | `RefreshVillageExplorationFromActiveScene` **仅** `scene.name == Village_KenMuNi1` → Chief_House 保持 `Default` |
| 阶段 A | **A1**：白名单加入 `Village_Chief_House` + **窄** `VillageWalkArea`（勿 A2 进出切换抖动） |
| WalkArea 名 | 仍叫 **`VillageWalkArea`**；绑定逻辑场景白名单与村模式同源扩展 |
| Y 标尺 | 本场景摆 `VillageDepthY_Min` / `VillageDepthY_Max`；注入函数白名单同步扩 |
| 阶段 B | 楼梯边 **`VillageWalkObstacle`（Trigger + 方案1 Cast）** + 按需 **`VillagePlayerDepthZone`** |
| 双 Trigger Gate | **本期不默认上**（楼梯简单）；仅当需「上楼才激活某层 Collider 包」再复用 |
| 合层「楼梯」 | **仅美术锚点**（SR only，无 Collider） |
| 区外 | **WalkArea ClosestPoint 夹死**（A1） |
| 禁跳 | `SetVillageExplorationMode(true)` → `isEnableJump=false`（跟村） |
| 其它 Home | **禁止**默认同开 Town |

---

## ② 室内现网缺口

### 为何不能 W/S（证伪）

| 锚点 | 磁盘行为 |
|------|----------|
| `PlayerLogic.RefreshVillageExplorationFromActiveScene` | `village = (active.name == Village_KenMuNi1)` → `SetVillageExplorationMode` |
| 调用时机 | 玩家创建 / 切场景后刷新（L171、L535） |
| `TownPlayerLocomotion` 纵深 | 仅 `LocomotionMode == Village2_5D` 时写权威 Y |
| `TryBindVillageWalkPolygonFromActiveScene` | **仅** KenMuNi1 下按名找 `VillageWalkArea` |
| `TryInjectVillageDepthYBoundsFromSceneMarkers` | **仅** KenMuNi1 |

进屋后：`Village_Chief_House` ≠ KenMuNi1 → **Default** → 无 Town 纵深 → W/S 无效。  
OPEN 曾记「家里不开 Town」——与本期产品冲突，须**有意**扩白名单，**禁止**静默全开所有 Home。

### Chief_House 现有物体（勿误认）

| 物体 | 核实 | 角色 |
|------|------|------|
| 合层 **`楼梯`** | 仅 `SpriteRenderer`；local≈`(5.47, 5.50, 8.67)`；order 2 | **视觉锚点**，≠ Walk / 障碍 |
| `LayerArea` | Layer 8；绑 `layerTsf` / `showLayerArea` | **相机/图层工具**，≠ `VillageWalkArea` |
| `MapLimit` | 有 `PolygonCollider2D`，**`m_IsActive: 0`** | 旧边界，未当村 Walk |
| `Ground` / `GroundColliders` | 室内地面物理 | Default 横移落地用；**不提供**村式纵深 |
| `EnterFrom_Village` | `(17.42, -3.65, 0)` | 从村进屋落点 |
| `VillageWalkArea` / DepthZone / Gate | **0** | 本期缺口 |

---

## ③ 阶段 A 方案（划区开 2.5D）

### 方案对照

| 方案 | 做法 | 裁定 |
|------|------|------|
| **A1** | 进 Chief_House 即 `SetVillageExplorationMode(true)`；窄 `VillageWalkArea` 罩「进门走廊 + 楼梯斜面/平台」 | ✅ **采用** |
| A2 | 脚进 Trigger 才开 Town | ❌ 边界抖动 / 模式切换风险高 |
| A3 | 新建第三套室内纵深 | ❌ |
| A4 | 所有 Home 开 Town | ❌ |

### 「只有这一部分」如何表达

- **不靠**「关 Town」表达局部，而靠 **多边形形状**：区外 `ClosestPoint` **夹死**。  
- WalkArea 建议呈 **条带**：`EnterFrom_Village` → 楼梯底 → 斜面 → 上层小平台（尽量窄，勿铺满整屋地板）。  
- 若落点在区外：玩家会被吸到多边形边缘 → **施工须保证落点在区内或条带含落点**（Q6）。

### 脚本最小改法（白名单）

| 位置 | 改法 |
|------|------|
| `RefreshVillageExplorationFromActiveScene` | 场景名 ∈ `{KenMuNi1, Village_Chief_House}`（抽私有 `IsVillageExplorationScene` 防散落魔法字符串） |
| `TryBindVillageWalkPolygonFromActiveScene` | 同上白名单 + 仍找名 `VillageWalkArea` |
| `TryInjectVillageDepthYBoundsFromSceneMarkers` | 同上白名单 |
| 场景 | 新建 `VillageWalkArea`（PolygonCollider2D）+ `VillageDepthY_Min/Max` 空物体 |

**禁止**：改战斗重力；在 Update 堆业务；抢写 Animator `Run`（Home 控制器用 `Walk`）。

### Y 标尺

村用 `VillageDepthY_Min/Max` 世界 Y。室内楼梯跨度约从落点 Y≈−3.65 到合层楼梯 Y≈5.5（合层根若在 0）：施工按实际世界坐标摆标尺，使 Clamp 覆盖可走带，**勿**抄村外标尺数值。

---

## ④ 阶段 B（楼梯树屋化）

### KenMuNi1 样板（磁盘）

| 能力 | 村场景 |
|------|--------|
| `VillageWalkArea` / `VillageWalkArea2` | ✅ |
| `DepthZone` / `DepthZone&Colliders` | ✅ |
| `VillagePlayerDepthZone` 脚本 | ✅（guid 命中场景） |
| `VillageTreehouseDepthZoneGate` | ✅（×2） |
| 障碍方案1 | `VillageWalkObstacle` Layer + Trigger + Town Cast |

### 室内楼梯应对

| 树屋能力 | Chief_House |
|----------|-------------|
| WalkArea 可走带 | 阶段 A 多边形覆盖斜面/平台（对齐合层「楼梯」斜角） |
| `VillageWalkObstacle` | 内/外栏杆、台阶边：Trigger + 方案1；**勿**与合层 SR 硬绑成唯一物理 |
| `VillagePlayerDepthZone` | 上下层前后景需要时摆；脚进切玩家 Sorting Layer |
| `VillageTreehouseDepthZoneGate` | **默认不上**；仅多层 Collider 包需上楼才激活时再加 |

**合层美术**：保留 `楼梯` / `内栏杆` / `外栏杆` SR；交互与挡位 **旁挂空物体**，禁止把 SR 当唯一 Collider。

---

## ⑤ 合层楼梯锚点

| 项 | 值 |
|----|-----|
| 路径 | `Prefab/村长家合层`（场景引用份）→ `楼梯` |
| 组件 | Transform + SpriteRenderer **only** |
| 用途 | 画多边形与障碍时的 **视觉对齐参考** |
| 禁止 | 在 `楼梯` 上硬挂物理当唯一挡位且与 Town Cast 双拽 |

---

## ⑥ 进出房模式恢复

```
村 KenMuNi1（Village2_5D）
  → 门 / Loading → Village_Chief_House
       RefreshVillageExploration →（施工后）true
       绑本场景 VillageWalkArea + Y 标尺
  → 区内 2.5D；楼梯障碍 / DepthZone
  → 出门回村
       Refresh → KenMuNi1 → true（村链路恢复）
  → 若进其它 HomeScene
       名不在白名单 → Default（家里仍无纵深）✅
```

与续聊 / 黑幕换古莎：**正交**（对白与涂层）；注意落点在 WalkArea 内，避免黑幕亮后被夹到奇怪边角。

---

## ⑦ 最小施工清单

| # | 动作 | 优先级 |
|---|------|--------|
| 1 | 抽/扩场景白名单：`KenMuNi1` + `Village_Chief_House`（三处：Refresh / BindWalk / InjectY） | **P0** |
| 2 | 场景摆 `VillageWalkArea` 窄条带（含 EnterFrom_Village→楼梯） | **P0** |
| 3 | 摆 `VillageDepthY_Min` / `Max` | **P0** |
| 4 | 楼梯边 `VillageWalkObstacle` Trigger（方案1） | **P0** |
| 5 | 按需 `VillagePlayerDepthZone`；Gate 默认跳过 | **P1** |
| 6 | 施工说明 → `施工说明/0901/`；同步 OPEN | — |
| 7 | **不做**：其它 Home 开 Town；改战斗重力；第三套移动；改合层 SR 当唯一物理 | — |

---

## ⑧ 验收清单

- [ ] 划定区内：A/D + W/S；手感接近村里  
- [ ] 区外：夹死（不能整屋飞 Y）  
- [ ] 楼梯可上下；斜面左右可用；不穿栏不卡死  
- [ ] DepthZone（若有）进出正确  
- [ ] 进村长家 / 回村模式不串；其它 Home 仍无纵深  
- [ ] 区内禁跳  
- [ ] 续聊 / 换古莎 / 门换场回归  
- [ ] Console 无空引用、无每帧刷屏  

---

## ⑨ OPEN

| ID | 问题 | 侦探倾向 | 状态 |
|----|------|----------|------|
| Q1 | A1 vs A2？ | **A1** | ✅ |
| Q2 | WalkArea 名？ | **VillageWalkArea** | ✅ |
| Q3 | 双 Trigger Gate？ | 先 **Zone+障碍**；简单楼梯不上 Gate | ✅ |
| Q4 | 区外？ | **夹死** | ✅ |
| Q5 | 禁跳？ | **是** | ✅ |
| Q6 | 与续聊/换古莎？ | 正交；**落点须进区** | ✅ |
| Q7 | WalkArea 覆盖范围？ | **尽量窄**（进门条带+楼梯+小平台）；精确顶点施工时肉眼调 | ⏳ |

---

## ⑩ 程序补充（速查）

| 锚点 | 用途 |
|------|------|
| `PlayerLogic.RefreshVillageExplorationFromActiveScene` | 村模式总闸（现仅 KenMuNi1） |
| `PlayerLogic.SetVillageExplorationMode` | `Village2_5D` + 禁跳 + `Town.ApplyVillageMode` |
| `TownPlayerLocomotion.TryBindVillageWalkPolygonFromActiveScene` | 绑 `VillageWalkArea` |
| `TryInjectVillageDepthYBoundsFromSceneMarkers` | Y 标尺 |
| 方案1 技术说明 | `VillageWalkObstacle` Cast / 禁物理硬碰 |
| `VillagePlayerDepthZone` / `Listener` | 脚进切 Sorting Layer |
| `VillageTreehouseDepthZoneGate` | 树屋双 Trigger 门控（本期可选） |
| 合层 `楼梯` | 美术锚点 only |
| `EnterFrom_Village` `(17.42, -3.65)` | 落点；须落入 WalkArea 条带 |

**一句话**：先让 Chief_House 进入与村同一套 Town 闸门，再用很窄的 WalkArea 把人关在楼梯带里，再按树屋摆障碍和 DepthZone——合层「楼梯」只负责好看。
