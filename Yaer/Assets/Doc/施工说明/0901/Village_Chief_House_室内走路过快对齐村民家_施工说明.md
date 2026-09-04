# Village_Chief_House — 室内走路过快对齐村民家 — 施工说明

**文档版本**：v1.0（2026-09-01）  
**文档性质**：【施工员】按侦探报告 S1 落地  
**依据**：`执行文档/0901/Village_Chief_House_室内走路过快对齐村民家_架构溯源报告.md`

---

## ① 结论

村长家开 Town 后误吃村街 planar **11.2**；其它 Home 用 **walkSpeed=4.2**。  
**S1**：仅 `Village_Chief_House` 把 `ResolveVillagePlanarMoveSpeed` 覆写为 `WalkSpeed`；KenMuNi1 仍 11.2；白名单不撤。

## ② 你要验

1. 村长家体感 ≈ Home 室内 walk（慢于改前）  
2. 回 KenMuNi1 仍 ≈ 11.2  
3. 楼梯 W/S / WalkArea / 其它 Home 未伤  
4. 可选：Town `acceptanceDebugLog` → `scene=Village_Chief_House planar≈4.2`

## ③ 改动

| 项 | 说明 |
|----|------|
| `PlayerMoveComponent.WalkSpeed` | 只读公开 |
| `SceneName.IsIndoorVillageExplorationScene` | 仅 Chief |
| `TownPlayerLocomotion.ResolveVillagePlanarMoveSpeed` | 室内 → walkSpeed；否则原 11.2 |

**未改**：白名单、`villagePlanarMoveSpeed` Prefab 初值、Animator、WalkArea。
