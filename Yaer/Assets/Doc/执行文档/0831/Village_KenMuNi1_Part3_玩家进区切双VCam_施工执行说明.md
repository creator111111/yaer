# Village_KenMuNi1 Part3 — 玩家进区切双 VCam — 施工执行说明

**文档版本**：v1.0（2026-08-31）  
**文档性质**：【施工员】判定改口落地  
**Unity**：2020.3.48f1 + Cinemachine  
**场景**：`Village_KenMuNi1` · `Map/CameraDepthFollowZone_Part3`  
**详细落盘**：`Assets/Doc/施工说明/0831/Village_KenMuNi1_Part3_玩家进区切双VCam_施工说明.md`

---

## ① 结论一句话

**双机保留；进区条件改为玩家在 Zone 内（Contains + 滞回），废弃 A1 白框全进。**

---

## ② 原因（通俗）

白框要整块塞进绿盒才切机——人已经站在高台上，镜头常有一边还在外面，就一直切不到 Part3。  
改成「人进绿区就切」更直观；因为只换哪台相机 Live、不改 Body，不会再出现改 ScreenY 狂切。

---

## ③ 用户检查清单

| # | 操作 | 通过 |
|---|------|------|
| 1 | 走进左翼高台绿区 | Blend 到 Part3（高台跟拍手感） |
| 2 | 走出绿区 | 回 Street |
| 3 | 边界来回 | 能反复切，不卡死 |
| 4 | 右街约 x&gt;-93 | 不误切 Part3 |

---

## ④ 给程序

### 调用链（改后）

```
LateUpdate
  → IsPlayerInsideZone：滞回盒 Contains(player.position)
  → want != _part3Active 且过冷却
  → SetKenMuNiPart3CameraMode → Part3.Priority 20/0（Street 保持 10）
```

### 改动文件

| 文件 | 变更 |
|------|------|
| `VillageCameraDepthFollowZone.cs` | A1 → 玩家进区；去 CameraUpdated；保留 Priority 切机 |

### 明确不恢复

- 每帧 `ApplyFramingTransposerProfile` / `ReassertCurrentProfile`
- 单机改 Body 当主方案
