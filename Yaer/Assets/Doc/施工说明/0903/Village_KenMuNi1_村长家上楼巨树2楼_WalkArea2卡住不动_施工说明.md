# Village_KenMuNi1 — 上楼巨树 2 楼 WalkArea2 卡住不动 — 施工说明

**文档版本**：v1.0（2026-09-03）  
**文档性质**：【施工员】按侦探报告最小落地  
**Unity**：2020.3.48f1  
**侦探报告**：`执行文档/0903/Village_KenMuNi1_村长家上楼巨树2楼_WalkArea2卡住不动_架构溯源报告.md`  
**上游**：0901 楼梯换场 + W1 Override + E3′（保留）

---

## 沟通摘要

### ① 结论一句话

**已补 KenMuNi1 纵深标尺（Max=46）+ 楼梯路径先扩 bounds/绑 WalkArea2 再权威 Teleport，消除 maxY=8 与 2 楼夹区撕扯。**

### ② 原因（通俗）

2 楼出口大约在 Y=40，但村里「能站多高」以前默认最高只到 8。  
程序一落地就把人往下压，同时又按 2 楼地板往上吸——人被两头拽，看起来站在绿线框附近却动不了。

### ③ 用户检查清单

| # | 操作 | 通过判据 |
|---|------|----------|
| 1 | 村长家楼梯上楼进村后 Pause | 脚距 `ExitFrom_HomeSceneChief2f≈(-159.34,41.66)` 很小 |
| 2 | Console | `[Village2f] depthYMax→…` 与 `已 SetVillageWalkAreaOverride(VillageWalkArea2)` |
| 3 | Hierarchy / 临时读 `DebugDepthYMaxWorld` | **≥** WalkArea2 上沿（约 ≥45），**不是**仍为 8 |
| 4 | 区内 A/D+W/S | 可走；`OverlapPoint(WalkArea2)=true`；不吸回 1 楼 |
| 5 | 对比修前 | `VillageWalkArea2` 点集/尺寸未改 |
| 6 | `LeftDoor` 出门 | 仍落 `ExitFrom_HomeSceneChief`（1f），不绑 WalkArea2 |
| 7 | Console | 无相关 Error；过滤 `[Village2f]` |

可选：若场景未带标尺，跑菜单 `Tools / Scene / Setup KenMuNi1 巨树纵深标尺 DepthY`（幂等）。

### ④ 程序补充

见下文。

---

## 改动清单

| 项 | 路径 | 说明 |
|----|------|------|
| **F_D1** | `Village_KenMuNi1.unity` | `Map/VillageDepthY_Min` Y=−20；`VillageDepthY_Max` Y=46 |
| **F_D1 Editor** | `Editor/.../KenMuNi1VillageDepthYSetupEditor.cs` | 幂等菜单 + Library request 自动跑 |
| **F_D2 + F_Order** | `Village_KenMuNiSceneManager.SetPlayerPos` | 楼梯键：先 `SetDepthYBounds` 抬 Max → Override → `TeleportAuthoritative(ExitFrom)` |

**未改**：`VillageWalkArea2` 多边形；1 楼大门键路径；宝箱；送树屋；Player Prefab 全局 max；ClosestPoint 开关。

---

## 时序（楼梯键）

```
LastScene == Village_Chief_House
  → Find VillageWalkArea2
  → SetDepthYBounds(min, max(max, poly.bounds.max.y+0.5))   // F_D2
  → SetVillageWalkAreaOverride(WalkArea2)
  → TeleportAuthoritative(ExitFrom_HomeSceneChief2f) + Flush // F_Order
```

进村 `TryInjectVillageDepthYBoundsFromSceneMarkers` 仍会读场景标尺（F_D1），与 F_D2 双保险。

---

## 日志锚点

| 日志 | 含义 |
|------|------|
| `[Village2f] depthYMax→…` | F_D2 已抬 Max |
| `[Village2f] 已 SetVillageWalkAreaOverride(VillageWalkArea2)` | W1 生效 |
| `CLAMP_AT_YMAX` 持续刷 | 标尺仍不够 / 注入失败，重查 Hierarchy |

---

## 验收对照（报告 §⑧）

- [ ] 脚稳 ExitFrom2f；区内可走；不吸 1 楼  
- [ ] Console 两条 `[Village2f]`  
- [ ] `DebugDepthYMaxWorld` ≥ 2 楼高度  
- [ ] WalkArea2 形状未改  
- [ ] LeftDoor 仍 1f、不绑 2  
- [ ] 无相关 Error  
