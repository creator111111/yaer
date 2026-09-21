# Village_KenMuNi1 · 巨树上面不能像村里一样走 — 施工说明

**文档版本**：v1.0（2026-09-20）  
**文档性质**：【施工员】最小改动  
**Unity**：2020.3.48f1  
**侦探报告**：`Assets/Doc/执行文档/0920/Village_KenMuNi1_巨树上面不能像村里一样走_架构溯源报告.md`

---

## 沟通摘要

### ① 结论一句话

**2 楼绿框改成和街上一样的触发器，人可以在框里走了。**

### ② 原因（通俗）

尺子已经够高，人也能落在 2 楼绿框里。可这块绿框是实心墙，街上那块只用来画可走范围。人站进实心框，物理往外顶，走路脚本再拉回来，脚就像焊在落点上。

### ③ 用户检查清单

从村长家楼梯上到巨树 2 楼。

| # | 操作 | 通过判据 |
|---|------|----------|
| 1 | Inspector 点 `VillageWalkArea2` 的 PolygonCollider2D | **Is Trigger 已勾**（与 `VillageWalkArea` 相同） |
| 2 | 看脚的 Y | 大约 **40 上下**（落点 41.66），不是被压到 8 |
| 3 | Console 滤 `Village2f` | 有 `depthYMax→46`（或 ≥45）和 `已 SetVillageWalkAreaOverride(VillageWalkArea2)` |
| 4 | 滤 `CLAMP_AT_YMAX` | **不应刷** |
| 5 | 在 2 楼左右、前后走 | 能离开落点，仍出不了绿框 |
| 6 | 回到 1 楼街上走 | 手感与改前一样 |

### ④ 程序补充

见下文。

---

## 改动清单

| 路径 | 物体 | 字段 | 现网 | 改为 |
|------|------|------|------|------|
| `Assets/GameRes/Scenes/Village_KenMuNi1.unity` | `VillageWalkArea2` 的 `PolygonCollider2D` | `m_IsTrigger` | 0 | **1** |

点集没动。1 楼 `VillageWalkArea`、纵深标尺、上楼绑区、EnterPos、Player Prefab 的 `depthYMaxWorld=8` 都没改。

---

## 为什么这样改

可走区只该被 ClosestPoint 使用，不该参与 Map 物理。1 楼已经是触发器，所以街上能走。勾上之后，实心对拉消失，人可以在现有绿框里走。

否决：关掉 ClosestPoint（人会走出绿框）；用 1 楼框罩住整棵树（2 楼和街道串台）。
