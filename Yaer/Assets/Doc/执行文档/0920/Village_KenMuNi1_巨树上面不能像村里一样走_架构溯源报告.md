# Village_KenMuNi1 巨树上面不能像村里一样走 — 架构溯源报告

> 日期：2026-09-20  
> 角色：架构侦探（只读，未改代码 / 场景 / Prefab / 多边形）  
> Unity：2020.3.48f1  
> 对照：0903「纵深上限仍是 8」。以现网 YAML / 代码为准。

---

## ① 结论一句话

**不是纵深对拉。** 0903 的修复还在：标尺 Max=46，上楼会绑 `VillageWalkArea2` 并落到框内。现在卡在一个点上，是因为这块绿框是**实心碰撞**（Is Trigger 没勾），街上那块是触发器；人站在框里面会被物理往外顶、又被 ClosestPoint 拉回去。不要改点集。

---

## ② 原因

大白话：尺子已经够高，人也能落在 2 楼绿框里。可这块绿框和 1 楼不一样——1 楼是「只用来画可走范围」的触发器，2 楼是一堵实心墙。人（动态刚体）和 Map 层会撞。人一站进实心多边形，物理就把人往边界外推，走路脚本再把人拉回框内，两下对拉，脚就像焊在落点上。左右、前后都会被吃掉。

### A. 0903 修复还在

| 项 | 现网 |
|----|------|
| 纵深标尺 | 有。`VillageDepthY_Min` 世界 Y=**-20**，`VillageDepthY_Max` 世界 Y=**46**（父节点 Map 在原点） |
| Prefab 默认 Max | 仍是 **8**（`Player.prefab`），进村后会被场景标尺盖掉 |
| 2 楼多边形上沿 | 世界 Y **33.65～45.35**。46 ≥ 上沿，上楼时 `ExpandDepthYMaxForWalkArea2` 只升不降，Max 仍是 46，**不是 8** |
| 绑区 | `LastScene == Village_Chief_House` 时：先抬 Max → `SetVillageWalkAreaOverride(VillageWalkArea2)` → `Teleport` 到 EnterPos |
| EnterPos | `Village_Chief_House` → `ExitFrom_HomeSceneChief2f`，本地 **(-157.65, 41.66)** |
| 落点是否在框内 | 相对绿框本地 **(-18.65, 4.16)**，偶数-奇数判定 **在框内**；上下左右各偏 0.5 仍在框内 |

大门键 `Village_Chief_House_Door` 不绑 2 楼，落在 1 楼 `ExitFrom_HomeSceneChief`（Y≈-5.5）。这是对的。

### B. 和街上比，为什么还不像村里走

| 对比 | 街道 `VillageWalkArea` | 大树 `VillageWalkArea2` |
|------|------------------------|-------------------------|
| 模式 | Village2_5D（同一套 `TownPlayerLocomotion`） | 同左 |
| 世界包围 | 宽约 **221**，纵深高约 **12.4**（Y -7.83～4.59） | 宽约 **57**（X -164～-107），纵深高约 **11.7**（Y 33.65～45.35） |
| 纵深 Min/Max | 同一把尺子：-20～46 | 同左，2 楼在尺子里面 |
| Is Trigger | **勾上（1）** | **没勾（0）** |
| Layer | 8 = Map | 8 = Map |
| 落点附近障碍 | — | 全场景只有几块 `VillageWalkObstacle`（层 6），父物体都在街道装饰上，**没有一块盖住 (-157, 42)** |

玩家根刚体是 **Dynamic**（`m_BodyType: 0`），层 Player。2D 碰撞矩阵里 **Player 与 Map 互相碰撞**。实心多边形的内部也算「撞在墙里」。

按键被谁吃掉：

| 按键 | 谁吃掉 |
|------|--------|
| A/D | 不是纵深 Clamp。物理把人从实心框里往外挤，`ApplyVillageWalkPolygonPostCorrection` 的 ClosestPoint 再拉回框内，水平位移被抵消 |
| W/S | 同上。脚在 Y≈41.7，Max=46，**不会**打出 `CLAMP_AT_YMAX`。纵深积分加一点，马上被「挤出 + 拉回」抵消 |

绿框本身不是一个点：落点附近至少能偏出 0.5，往右至少约 5 个单位仍在框内。所以**禁止把「改点集、加宽」当主修**。纵深高度和街道差不多（约 12），差的是实心/触发器。

### 调用链

```
村长家楼梯 → LastScene = Village_Chief_House
  Village_KenMuNiSceneManager.TryApplyChiefStairsLandingToTree2f
    标尺已是 46（VillageDepthY_Max）
    ExpandDepthYMaxForWalkArea2（只升不降）
    SetVillageWalkAreaOverride(VillageWalkArea2)
    Teleport → ExitFrom_HomeSceneChief2f（框内）
  之后每帧 FixedUpdate
    纵深积分（W/S），Clamp 在 -20～46  → 不削 2 楼
    ClosestPoint 把人留在绿框内
    同时 Physics2D：Dynamic 玩家撞上「没勾 Trigger」的 Map 多边形 → 往外推
    两套对拉 → 脚焊在落点
```

---

## ③ 用户需要做什么

1. 上楼后看脚的 Y：应在 **40 上下**（落点 41.66），不是被压到 8。若 Y 掉到个位数，才是旧的纵深问题，把 Console 发出来。  
2. Console 滤 `Village2f`：应有 `depthYMax→46`（或 ≥45）和 `已 SetVillageWalkAreaOverride(VillageWalkArea2)`。  
3. 再滤 `CLAMP_AT_YMAX`。按这份现网，**不应该刷**。若刷了，说明本机场景没有带上 `VillageDepthY_Max`。  
4. 在 Inspector 点 `VillageWalkArea2` 的 PolygonCollider2D，看 **Is Trigger 是否没勾**。没勾就是本案。

---

## ④ 给施工员的补充

### 推荐方案（只此一种）

只改场景里这一处，**不改点集**：

| 文件 | 物体 | 字段 | 现网 | 改为 |
|------|------|------|------|------|
| `Assets/GameRes/Scenes/Village_KenMuNi1.unity` | `VillageWalkArea2` 的 `PolygonCollider2D` | `m_IsTrigger` | **0** | **1**（与 `VillageWalkArea` 相同） |

原因：可走区只该被 ClosestPoint 使用，不该参与 Map 物理。1 楼已经是触发器，所以街上能走。勾上之后，人可以离开落点，在现有绿框里左右、前后走；1 楼规则不动。

### 不要改

| 不要改 | 原因 |
|--------|------|
| `VillageWalkArea2` 的点集 / 尺寸 | 框够大，落点在框内；0903 禁止先改形状，这次也没证明是太窄 |
| `VillageDepthY_Min/Max`、上楼绑区、EnterPos | 已经生效 |
| 1 楼 `VillageWalkArea` | 街道手感保持 |
| Player Prefab 的 `depthYMaxWorld=8` | 场景标尺会盖住；改 Prefab 会放开没摆标尺的村子 |

### 否决方案

- 关掉 ClosestPoint：人会走出绿框，1 楼的可走区约束一起没了。  
- 用 1 楼 `VillageWalkArea` 罩住整棵树：2 楼和街道变成同一块地，上下楼串台。

### 会误伤的其它场景

按推荐方案：**无。** 只改 KenMuNi1 这一块多边形的触发器开关。
