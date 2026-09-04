# Village_Chief_House — 楼梯上楼换场巨树 2 楼 — 施工说明

**文档版本**：v1.0（2026-09-01）  
**文档性质**：【施工员】按侦探报告最小落地  
**Unity**：2020.3.48f1  
**侦探报告**：`执行文档/0901/Village_Chief_House_楼梯上楼换场巨树2楼_架构溯源报告.md`  
**依赖**：室内划区 A1（须能走上楼）

---

## 沟通摘要

### ① 结论一句话

**楼梯顶新建换场门（走进黑幕进村→2 楼）；`VillageWalkArea2` 靠 Override 生效（不改形状）；1 楼 `LeftDoor` 用 `enterPosKey=Village_Chief_House_Door` 另落门前。**

### ② 原因（通俗）

回村只看上一场景名选出生点，大门和楼梯以前会抢同一个点。  
村里走路又只认 1 楼那块地板，2 楼区域摆了却没用上，人会被吸下去。

### ③ 用户检查清单

| # | 操作 | 通过 |
|---|------|------|
| 1 | 开 Unity 跑菜单 `Tools / Scene / Setup Chief House 楼梯上楼换场巨树2楼`（若尚未自动跑） | |
| 2 | 室内走上楼 → 触发 → 进 `Village_KenMuNi1` | |
| 3 | 落点 ≈ `ExitFrom_HomeSceneChief2f`（Y≈41） | |
| 4 | 不被拉回 1 楼；`VillageWalkArea2` 内可走 | |
| 5 | WalkArea2 点集/尺寸未改 | |
| 6 | `LeftDoor` 出门落 `ExitFrom_HomeSceneChief`（1 楼门前） | |
| 7 | Console 无相关 Error；回村仍 2.5D | |

### ④ 程序补充

见下文。

---

## 改动清单

| 项 | 路径 | 说明 |
|----|------|------|
| `LoadSceneArgs.enterPosKey` | ChangeScene | E3′ 拆键 |
| `ChangeSceneComponentGM` | 记 LastScene 优先键 | |
| `SceneChangeDoor.EnterPosKey` | 门序列化 | LeftDoor=`Village_Chief_House_Door`；楼梯空 |
| `LoadSceneComponentGSM` | 透传 enterPosKey | |
| `TownPlayerLocomotion.SetVillageWalkAreaOverride` | W1 | |
| `Village_KenMuNiSceneManager.SetPlayerPos` | last=`Village_Chief_House` → 绑 WalkArea2 | |
| KenMuNi1 | `ExitFrom_HomeSceneChief` + EnterPos 行 | |
| Chief | Objects Active；LeftDoor EnterPosKey | |
| Editor | `ChiefHouseStairsToTree2fSetupEditor` | 摆 `StairsDoor_ToTree2f` |

**未改**：`VillageWalkArea2` 多边形；回程下楼回村长家（Q5）；其它 Home Town。

---

## 换场键对照

| 出口 | enterPosKey | KenMuNi1 EnterPos | 落点 |
|------|-------------|-------------------|------|
| 楼梯顶门 | （空） | `Village_Chief_House` | `ExitFrom_HomeSceneChief2f` |
| LeftDoor | `Village_Chief_House_Door` | 同名新行 | `ExitFrom_HomeSceneChief` |
