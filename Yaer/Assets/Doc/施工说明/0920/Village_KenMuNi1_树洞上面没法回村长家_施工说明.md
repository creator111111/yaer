# Village_KenMuNi1 · 树洞上面没法回村长家 — 施工说明

**文档版本**：v1.0（2026-09-20）  
**文档性质**：【施工员】最小改动  
**Unity**：2020.3.48f1  
**侦探报告**：`Assets/Doc/执行文档/0920/Village_KenMuNi1_树洞上面没法回村长家_架构溯源报告.md`

---

## 沟通摘要

### ① 结论一句话

**巨树 2 楼树洞旁可以回村长家了，会落到楼梯顶，不是大门。**

### ② 原因（通俗）

以前只有上楼这条门，回程没做。树洞只是画，走进去不会切场。1 楼大门在地面高度，人在树枝上够不着。现在在 2 楼落点旁加了回程门，进屋落到楼梯顶。

### ③ 用户检查清单

| # | 操作 | 通过判据 |
|---|------|----------|
| 1 | 切回 Unity，等场景导入；若 Hierarchy 还没有 `StairsDoor_BackToChief`，跑一次菜单 `Tools/Scene/Setup 巨树2楼回程进村长家` | 有该门；`EnterPosKey=Village_KenMuNi1_Tree2f` |
| 2 | 从村长家楼梯上到 2 楼，走到树洞旁门（约 -156, 41.5） | Console 有 `[SceneChangeDoor]`；黑幕进村长家 |
| 3 | 看落点 | 在楼梯顶附近（`EnterFrom_Tree2f` 约 -2.8, 4），**不是**大门 `EnterFrom_Village` |
| 4 | 下到村街再进 `House_Chief` | 仍落大门内侧（大门没坏） |
| 5 | 上楼去程 | `StairsDoor_ToTree2f` 仍正常到 2 楼 |

### ④ 程序补充

见下文。

---

## 改动清单

| 路径 | 改动 |
|------|------|
| `SceneName.cs` | 常量 `Village_KenMuNi1_Tree2f` |
| `Village_KenMuNi1.unity` | Prefab 实例 `StairsDoor_BackToChief`（Stairs）：`NextSceneName=Village_Chief_House`，`TriggerWhenMoveIn=1`，`ShowLoadingUI=0`，`EnterPosKey=Village_KenMuNi1_Tree2f`；世界约 **(-156, 41.5)**；小触发盒 1.8×2.5；进 `sceneObjs` / Objects |
| `Village_Chief_House.unity` | 落点 `EnterFrom_Tree2f` **(-2.8, 4)**；EnterPosConfig 新行 `Village_KenMuNi1_Tree2f` → 该 Transform |
| `Tree2fBackToChiefSetupEditor.cs` | 幂等菜单；可再跑对齐 |

**未改**：`House_Chief`、`StairsDoor_ToTree2f`、`VillageWalkArea2` 点集、东郊 TreeBridge。

---

## 为什么这样改

回程必须用独立 EnterPos 键。空键会走大门 `EnterFrom_Village`，体感像从门口闪进屋。门与 2f 落点错开半身，避免一落地立刻再触发。

替代：指望走进美术黑洞 —— 没有 Collider + SceneChangeDoor，永远不会换场。
