# Village_KenMuNi1 Part3 — 摄像机框完全进入才切 Body — 施工说明

**日期**：2026-08-31  
**角色**：施工员  
**产品规则**：渲染相机正交白框 **完全 ⊆** Zone 绿框才 Apply Part3（ScreenY=0.88 等）；未完全进入 → 右街默认。主语是摄像机，不是角色。  
**溯源**：`Assets/Doc/执行文档/0831/Village_KenMuNi1_Part3_摄像机框完全进入才切Body_架构溯源报告.md`

---

## ① 结论一句话

判定改为 **相机 AABB ⊆ Zone + 滞回**；Part3 Profile 换用户表；Zone 缩到左翼（右缘≈-93）；离开 SoftH=1。

---

## ② 改了什么 & 原因

| 文件 | 变更 | 原因 |
|------|------|------|
| `VillageCameraDepthFollowZone.cs` | 删玩家 Trigger/Contains 主逻辑；`CameraUpdatedEvent` 上算白框 ⊆ 绿框 + 滞回 0.35 | 产品改口；与 Brain 同相 |
| `CameraComponent.cs` | Part3：ScreenY=0.88、YDamp=0、SoftH=0.351、Bias 0.5/0.5；Street SoftH **2→1** | 用户实测表 + Q4 |
| `Village_KenMuNi1.unity` | Zone 位 (-133,21) Size (80,58)；序列化两套 Profile 同步；`hysteresisWorldUnits=0.35` | 勿盖右街；场景序列化会盖代码默认 |

**未改**：CameraArea、开场 SetLock/CancelFollow、第二套 VCam、Profile 扩 Distance 等（P2）。

---

## ③ 验收清单

- [ ] 白框尚未完全进绿框 → ScreenY≈0.5，DeadZoneHeight=1  
- [ ] 白框完全进绿框 → ScreenY→0.88，DeadH=0，Bias 0.5/0.5，SoftH≈0.351  
- [ ] 白框任一边离开 → 恢复街道路  
- [ ] 边界来回 → 无明显狂切  
- [ ] 右街约 x>-93 的 W/S → **不被**误切 Part3  
- [ ] Follow 仍为 Player；开场剧情后跟拍正常  

Scene 建议开 **Cinemachine Game Window Guides** 看白框与绿框。

---

## ⑤ 返修（2026-08-31 · 反复进出后 Body 不再变）

**现象**：进出几次后 Framing Body 不再切换。  
**根因**：Apply 改 ScreenY/DeadZone 后机位挪动 → 下一帧「全进」变假 → 立刻切回；再叠加「只在边沿写 Body + 失败仍改标志」，标志与真实 Framing 脱节后永不再触发。  
**修复**（`VillageCameraDepthFollowZone.cs`）：

1. 切换成功后 **0.4s 冷却**，冷却期内不翻转模式  
2. 离开用 **外扩** 滞回盒（进入仍内缩）  
3. **仅成功下发后**才改 `_part3Active`  
4. 稳态 **每帧重申** 当前 Profile，防止脱节

请再验：边界来回多次 → Body 仍能进/出；右街不被误切。
