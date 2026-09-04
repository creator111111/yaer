# Village_Chief_House — 进场落点吸到楼梯 — 施工说明

**文档版本**：v1.0（2026-09-01）  
**文档性质**：【施工员】按验收排查 F1+F2(+F3) 落地  
**依据**：`执行文档/0901/Village_Chief_House_进场落点吸到楼梯_验收排查报告.md`

---

## ① 结论

EnterFrom 已在形内仍站楼梯：OnInit 过早 ClosestPoint + `SetPos` 只改 Transform。  
**F1** 村模式权威 Teleport（Transform+Rb+authY）；**F2** 权威落点前跳过夹区；**F3** Chief SetPlayerPos 后再 Flush。

## ② 你要验

1. KenMuNi1 进屋：脚在 EnterFrom≈(17.1,-6.61)，**不在**楼梯  
2. `[ChiefEnterPos]`：`rbMismatch≈0`，Overlap=True  
3. Loading 结束后不被拉回楼梯；楼梯仍可走  
4. 村街进出 / 黑幕传送 / 其它 Home 回归  

## ③ 改动

| 项 | 说明 |
|----|------|
| `TownPlayerLocomotion.TeleportAuthoritativeVillagePos` | F1 |
| defer 至权威落点 | F2：ApplyVillageMode / Flush 跳过夹区 |
| `EnsureAuthoritativeVillageSpawnCommitted` | LoadingEnd 兜底 |
| `PlayerLogic.SetPos` | 村模式走 Teleport |
| Chief `SetPlayerPos` | F3 再 Flush + rbMismatch 日志 |

**未改**：WalkArea 多边形、ClosestPoint 总开关、白名单、续聊 SetPos。
