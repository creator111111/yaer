# Village_KenMuNi1 Part3 — 双 VirtualCamera 切换 — 施工说明

**日期**：2026-08-31  
**角色**：施工员  
**产品改口**：弃用单机运行时 `ApplyFramingTransposerProfile`；改 **VCam_Street + VCam_Part3**，Zone 只切 Priority；判定用 **A1 街道路算框**。  
**溯源**：`Assets/Doc/执行文档/0831/Village_KenMuNi1_Part3_双VirtualCamera切换_架构溯源报告.md`

---

## ① 结论一句话

两台机 Body 写死在 Inspector；进区把 Part3 Priority 拉到 20，出区回 0；用 Street.State 算白框 ⊆ Zone，打断反馈环。

---

## ② 改了什么

| 文件 | 变更 |
|------|------|
| `CameraComponent.cs` | 序列化 `virtualCameraPart3`；SetFollow/Cancel/Size/Confiner/Impulse **双写**；手推 E2 对齐两台；`SetKenMuNiPart3CameraMode`→Priority |
| `CameraComponentGSM.cs` | 注释更新；保留透传 |
| `VillageCameraDepthFollowZone.cs` | **A1** Street 算框；边沿切 Priority；**删除**每帧 Reassert Apply |
| `Village_KenMuNi1.unity` | `Cinemachine`→`VCam_Street`；新建 `VCam_Part3`（ScreenY=0.88 等）；两台 Confiner→CameraArea；StandbyUpdate=Always；Brain CustomBlends |
| `KenMuNi1_StreetPart3_Blends.asset` | Street↔Part3 **0.4s EaseInOut** |

**未改**：CameraArea 多边形、开场剧情图、Zone 绿盒尺寸 (-133,21)/(80,58)。

---

## ③ 验收

- [ ] 右街 / 街道路框未全进绿框 → Live=Street，ScreenY≈0.5 DeadH=1  
- [ ] 街道路框完全进绿框 → Blend 到 Part3，ScreenY=0.88  
- [ ] 框离开 → 回 Street  
- [ ] 边界来回多次 → 无狂切、无「再也不切」  
- [ ] 右街 x>-93 不被误切  
- [ ] Play 后两台 Follow=Player；开场 CancelFollow 后剧情结束跟拍恢复  
- [ ] 改 Size / Confiner 两台一致  

---

## ④ 剩余风险

| 风险 | 说明 |
|------|------|
| CustomBlends 名依赖 GO 名 | 须保持 `VCam_Street` / `VCam_Part3` |
| 其它场景无 Part3 引用 | 自动退回旧 Apply，行为不变 |
| Impulse 运行时 Add | Init 时两台都会挂 Listener |
