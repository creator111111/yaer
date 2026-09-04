# Village_Chief_House — 进场飞出 / DefaultBornPos — 施工说明

**文档版本**：v1.0（2026-09-01）  
**文档性质**：【施工员】按验收排查最小修复  
**依据**：`执行文档/0901/Village_Chief_House_进场飞出DefaultBornPos_验收排查报告.md`  
**范围**：只对齐 `EnterFrom_Village` 进 WalkArea（与 `DefaultBornPos` 同点）+ 短诊断日志；**不改** WalkArea 多边形 / ClosestPoint / 白名单 / 续聊古莎。

---

## ① 结论

从村进屋走 `EnterFrom_Village`（不是 DefaultBorn）。原 EnterFrom `(17.42,-3.65)` 在手调 WalkArea **外** → 一开 2.5D 被 ClosestPoint 吸入底带（飞一下）。  
已将 `EnterFrom_Village` 移到与 `DefaultBornPos` 同点 **`(17.1, -6.61, 0)`**（形内）。

## ② 你要做的

1. 从 KenMuNi1 进屋：脚在 `(17.1,-6.61)` 一带，**无**大位移飞  
2. Console `[ChiefEnterPos]`：`OverlapWalkArea=True`，`distToTarget`≈0  
3. 区内移动 / 楼梯 / LeftDoor EnterPosKey 回归  
4. 验收通过后可关 GSM `enableEnterPosDebugLog`

## ③ 改动

| 项 | 说明 |
|----|------|
| 场景 `EnterFrom_Village` | `(17.42,-3.65)` → `(17.1,-6.61)` |
| `DefaultBornPos` | 未改（已是形内锚） |
| `Village_Chief_HouseSceneManager` | `[ChiefEnterPos]` 诊断（可关） |

**未动**：`VillageWalkArea` 点集、Town ClosestPoint、WalkArea2、出屋/续聊台本。
