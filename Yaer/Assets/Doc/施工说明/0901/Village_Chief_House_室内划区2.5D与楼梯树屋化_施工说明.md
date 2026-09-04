# Village_Chief_House — 室内划区 2.5D 与楼梯树屋化 — 施工说明

**文档版本**：v1.0（2026-09-01）  
**文档性质**：【施工员】按侦探报告最小落地  
**Unity**：2020.3.48f1  
**侦探报告**：`执行文档/0901/Village_Chief_House_室内划区2.5D与楼梯树屋化_架构溯源报告.md`  
**场景**：`Assets/GameRes/Scenes/Village_Chief_House.unity`

---

## 沟通摘要

### ① 结论一句话

**白名单扩至 `Village_Chief_House` 开村模式；场景已摆窄 `VillageWalkArea`（含进门落点）+ DepthY + 方案1障碍 + 上层 DepthZone；合层「楼梯」未改。**

### ② 原因（通俗）

进屋原先只认村名才开纵深，所以 W/S 无效。现在只有村长家额外开同一套；可走范围靠多边形夹死，楼梯边用障碍挡，不靠贴图当墙。

### ③ 用户检查清单

| # | 操作 | 通过 |
|---|------|------|
| 1 | 进村长家，落点在划区内 | |
| 2 | 划定区内 A/D + W/S | |
| 3 | 区外 | 被夹回多边形 |
| 4 | 沿合层「楼梯」斜上斜下 | 可走；不穿栏 |
| 5 | 上层 DepthZone | 进出前后景正确 |
| 6 | 回村 | KenMuNi1 仍 2.5D；其它 Home 仍无纵深 |
| 7 | 续聊 / 换古莎 / 门换场 | 回归 |
| 8 | 区内 | 禁跳 |

菜单（可重跑微调后回写）：`Tools / Scene / Setup Chief House 室内划区2.5D与楼梯`

### ④ 程序补充

见下文。

---

## 改动清单

| 项 | 路径 / 物体 | 说明 |
|----|-------------|------|
| 白名单 | `SceneName.IsVillageExplorationScene` | `KenMuNi1` ∪ `Village_Chief_House` |
| 闸门 | `PlayerLogic.RefreshVillageExplorationFromActiveScene` | 调白名单 |
| Y 注入 | `TryInjectVillageDepthYBoundsFromSceneMarkers` | 同上 |
| Walk 绑 | `TownPlayerLocomotion.TryBindVillageWalkPolygonFromActiveScene` | 同上 |
| 场景 | `Map/VillageWalkArea` | Trigger Polygon；含 `EnterFrom_Village` |
| 场景 | `Map/VillageDepthY_Min`≈−5.25、`Max`≈3.3 | 勿抄村外数值 |
| 场景 | `Map/VillageWalkObstacles/*` | Layer=`VillageWalkObstacle`；Trigger+Static |
| 场景 | `Map/DepthZone_StairsUpper` | `VillagePlayerDepthZone`；Gate **未上** |
| Editor | `ChiefHouseIndoor25DSetupEditor.cs` | 幂等重摆 |

**未改**：其它 Home；战斗重力；合层「楼梯」SR；树屋双 Trigger Gate。

---

## Q7 多边形说明

初始顶点为「进门条带 → 楼梯斜面 → 上层小平台」近似，**须在 Scene 视图对照合层楼梯肉眼调点**。  
重跑 Setup 菜单会按脚本默认点集覆盖——调完后勿盲目重跑，或先改 Editor 默认点再跑。

---

## 验收注意

- 若落点被吸到奇怪边角：扩大 WalkArea 使 `EnterFrom_Village (17.42,-3.65)` 深入区内。  
- 若能穿栏：微调 `Obstacle_OuterRail` / `InnerRail` / `StairsSide` 的位置与旋转（对齐美术栏杆）。  
- DepthZone 前后景不对：改 `targetSortingLayer` / `sortingOrderInZone`。
