# Village_KenMuNi1 Part3 — 玩家进区切双 VCam — 施工说明

**日期**：2026-08-31  
**角色**：施工员  
**产品再改口**：保留 `VCam_Street` + `VCam_Part3` + Priority/Blend；进/出条件从「街道路白框完全 ⊆ Zone」（A1）改为 **玩家位置在 Zone 内**。  
**关联**：`执行文档/0831/Village_KenMuNi1_Part3_双VirtualCamera切换_架构溯源报告.md` · 提示词 `提示词/0831/…玩家进区切双VCam_施工员提示词.md`

---

## ① 结论一句话

人走进绿区就切 Part3、走出回 Street；不再要求白框完全进绿框。

---

## ② 改了什么 & 原因

| 文件 | 变更 | 原因 |
|------|------|------|
| `VillageCameraDepthFollowZone.cs` | 主条件改为 `bounds.Contains(玩家根坐标)` + 滞回；删 A1 街道路算框 / `CameraUpdatedEvent` | A1 过严，人在高台常切不上 Part3；玩家判定可预期且不复活改 Body 反馈环 |
| 同上 | 冷却仍 0.4s（对齐 Blend）；稳态不改 Priority、不 ApplyFraming | 防连翻；禁止单机改 Body |

**未改**：双机 Follow/Confiner/Size、`SetKenMuNiPart3CameraMode`→Priority、Part3 Inspector Body、Zone 几何（右缘≈-93）、Blends 资源。

**方案选择**：P1 Contains 玩家根坐标（读档补检友好）优于 P2 仅靠 PlayerFoot Trigger。

---

## ③ 验收清单

- [ ] 玩家走进 `CameraDepthFollowZone_Part3` → Live 切到 `VCam_Part3`（ScreenY=0.88 手感）
- [ ] 玩家走出 Zone → 回 `VCam_Street`
- [ ] 边界来回多次：能反复切，无明显狂切、无「再也不切」
- [ ] 右街主道（约 x&gt;-93）不被误切
- [ ] Play 后两台 Follow 仍为 Player；开场锁相机契约不坏
- [ ] **不再**依赖「白框完全进绿框」才能切机

若仍不切：查 `virtualCameraPart3` 引用、Priority、两台 Follow，再查 Zone 盒是否罩到角色。
