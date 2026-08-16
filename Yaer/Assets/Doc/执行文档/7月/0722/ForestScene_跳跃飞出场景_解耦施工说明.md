# ForestScene 跳跃飞出场景 · 解耦施工说明

**文档版本**：v1.0（2026-07-22）  
**性质**：【施工员】基于 `ForestScene_普通跳跃跳出屏幕_架构溯源报告.md` v1.3  
**约束**：不改 `TownPlayerLocomotion`；村内 DNF 纵深与战斗落地检测分开维护

---

## ① 做了什么

| 文件 | 改动 | 是否碰村庄 DNF |
|------|------|----------------|
| `CapsuleGroundChecker.cs` | 落地改为**只向下 Raycast**；忽略 Trigger；注释写明与 Town 解耦 | **否**（不引用 Town） |
| `Player.prefab` | `GroundLayerMask` **1064961→1064960**（去掉 Default） | **否**（对齐 DNF 迁移方案 §3.3 原定 Mask） |

**未改**：`TownPlayerLocomotion.cs`、村场景、`MoveComponent` 重力公式、`jumpHeight`。

---

## ② 为何这样解耦

- 飞出场景根因是**战斗落地检测**把 Default 层高墙当成地面，不是纵深脚本写错跳跃。  
- DNF 文档本就要求 Mask=`1064960`（不含 Default）；多出来的 Default 才让墙进落地判定。  
- 村内纵深继续只由 `TownPlayerLocomotion` 写 Y；落地仍靠地面层（GroundCenter）被向下探测命中，**不靠墙体**。

---

## ③ 如何验证

| # | 步骤 | 期望 |
|---|------|------|
| V1 | Forest 贴左/右墙普通跳 | 半空 `Is Grounded=false`；`Velocity.y` 递减；落回地面 |
| V2 | Forest 场地中央跳 | 正常抛物线，不飞出场景 |
| V3 | 进 `Village_KenMuNi1` | W/S 纵深、左右走、排序正常（DNF 未回归） |
| V4 | 村内贴墙走 | 不因墙体误判出现异常上抛（Mask 已无 Default） |

Pause 验收字段：`Is Grounded`、`Velocity`、`Gravity`、`Can Gravity`（与溯源报告 §② 相同）。

---

## ④ 回滚

若村内落地异常：先确认村地面是否在 **GroundCenter(14)**；勿把 Default 加回 Mask。必要时仅调 `groundProbeDownDistance`，仍不要改 Town 组件。
