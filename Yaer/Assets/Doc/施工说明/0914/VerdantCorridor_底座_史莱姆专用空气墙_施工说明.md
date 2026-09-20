# VerdantCorridor 底座 · 史莱姆专用空气墙 — 施工说明

**文档版本**：v1.0（2026-09-14）  
**文档性质**：【施工员】按侦探报告方案 **C**  
**Unity**：2020.3.48f1  
**侦探报告**：`Assets/Doc/执行文档/0914/VerdantCorridor_底座_史莱姆专用空气墙_架构溯源报告.md`

---

## 沟通摘要

### ① 结论一句话

**底座下挂了通用组件 `SlimeOnlyAirWall`：只拦史莱姆，走路和击退都会被夹在墙外；玩家和木虫不受影响。**

### ② 原因（通俗）

史莱姆看起来有碰撞，其实三个盒子都是「只检测、不硬顶」。被打飞时脚本直接改坐标，普通空气墙挡不住。所以做成触发盒认身份，再在物理帧末尾把坐标推回去，并停掉击退曲线。

### ③ 用户检查清单

| # | 操作 | 通过判据 |
|---|------|----------|
| 1 | Hierarchy：`Design/Near/Part5/底座/SlimeAirWall` | 有黄 Gizmo 盒；`Is Trigger`；Layer=`Ignore Raycast` |
| 2 | 史莱姆从一侧走向底座 | 贴墙停，不穿到图内侧 |
| 3 | 朝墙方向击退 / 击飞 / 跳攻 | **不穿**；可贴墙停或沿墙滑 |
| 4 | 玩家穿过同一位置 | **无阻挡** |
| 5 | 同图木虫穿过 | **无阻挡** |
| 6 | 打死史莱姆，尸体/掉落 | 不卡在墙上飞掉（默认不挡尸体） |
| 7 | 睡眠史莱姆被打向墙 | 默认仍不穿 |
| 8 | 另拖空物体挂同一组件 | 行为一致（证明不写死「底座」） |
| 9 | 可选：勾 `debugLog` | Console `[SlimeAirWall]` Awake 尺寸 + 夹紧前后坐标 |

Scene 里墙偏了只调 `SlimeAirWall` 的 Box `Size` / `Offset`，**不要**改底座原图。

### ④ 程序补充

见下文。

---

## 改动清单

| 项 | 路径 | 说明 |
|----|------|------|
| **C 组件** | `Entities/Component/Physics/SlimeOnlyAirWall.cs` | Trigger Overlap + 扫掠 + `MovePosition` 夹紧；`DefaultExecutionOrder(1000)` |
| **.meta** | 同上 `.meta` | `executionOrder: 1000` 与特性一致 |
| **层名常量** | `Static/Name/Settings/LayerName.cs` | 新增 `IgnoreRaycast`（不改矩阵） |
| **场景挂载** | `GameRes/Scenes/VerdantCorridor.unity` | `底座` 子物体 `SlimeAirWall`：Trigger Box + 组件 |

### 未改

- 史莱姆 Prefab / AI / 伤害 / GroundCld  
- Physics2D 全局矩阵 / `MapLimit`  
- `Physics2DComponent`（明确不复用）  
- 方案 D 实心探测盒（OPEN Q5）

---

## 场景盒尺寸（可微调）

挂在 `底座` 本地 `(0,0,0)`，不改 Sprite。

| 字段 | 现网值 | 原因 |
|------|--------|------|
| Layer | Ignore Raycast (2) | 少跟射线/互动误交；检测靠 Overlap 全层 + 身份 |
| `m_IsTrigger` | 1 | 禁止 Default 实心盒误挡玩家 |
| Size | **1.5 × 6** | 宽取报告 0.8～1.5 上限；高盖战斗轴 Y≈−6.61 到跳攻顶点+余量 |
| Offset | **(0, −0.3)** | 盒心略下移，贴战斗轴 |
| bidirectional | true | OPEN Q3 体积挤出 |
| stopKnockbackOnHit | true | OPEN Q4 |
| blockDeadSlimes | false | OPEN Q1 |
| blockSleepingSlimes | true | OPEN Q2 |

若史莱姆还能踩到图的左右翼：把 `Offset.x` 挪到「内侧那条边」，或把 `Size.x` 略加宽。加宽不会挡玩家。

---

## OPEN 施工默认（已落地）

| ID | 施工默认 |
|----|----------|
| Q1 尸体 | **不挡** |
| Q2 睡眠 | **仍挡** |
| Q3 双向 | **双向** |
| Q4 停击退 | **是** |
| Q5 方案 D | **本期否** |

---

## 替代方案（未采用）

- **A** 专用 Layer + 矩阵：活体无实心盒，硬碰不发生，且 Monster 层会误伤木虫。  
- **B** IgnoreCollision：要对上非 Trigger 盒，须改史莱姆 Prefab；击退仍抢 `MovePosition`。  
- **D** B+C：Play 仍穿再开。  
- **E/F** 全局矩阵 / `MapLimit`：侦探否决。

---

## 剩余风险

- 墙是细板，史莱姆身体盒很宽，肉眼可能仍「贴着图」。以**根坐标不穿到内侧**为准。  
- 跳攻单帧极大且从未靠近过墙（无 lastPos）时，扫掠可能漏一帧；`nearbyPadding=12` 已留余量。  
- 本机未能 Play；须在 Unity 按上表验收。
