# Village_KenMuNi1 · 地图边缘双机硬切 — 施工说明

**文档版本**：v1.0（2026-09-20）  
**文档性质**：【施工员】最小改动  
**Unity**：2020.3.48f1  
**侦探报告**：`Assets/Doc/执行文档/0920/Village_KenMuNi1_地图边缘双机硬切_架构溯源报告.md`

---

## 沟通摘要

### ① 结论一句话

**贴灰边换相机时，镜头会先滑一段，不再像跳一下。**

### ② 原因（通俗）

高台和街道是两台相机。感应区的边画得和地图外缘一样齐，人贴灰边时两台镜头都被围栏按在同一条边上，中间那 0.4 秒几乎没有可滑的距离，看起来像硬切。现在感应区往里收，人还没贴边就换机，并多给 0.8 秒滑完。

### ③ 用户检查清单

| # | 操作 | 通过判据 |
|---|------|----------|
| 1 | 走到高台左侧或顶/底贴灰边 | 能看出一段过渡，不是一帧跳完 |
| 2 | 勾 `CameraDepthFollowZone_Part3` 的 `logStateTransitions`，再贴边 | Console 仍有 `part3Live=true/false`（还是在切机） |
| 3 | 高台里上下走 | 人仍偏画面上方（ScreenY 0.88 没动） |
| 4 | 街上左右走 | 手感不要变硬 |
| 5 | 感应区绿框 | 世界大约 X -164～-100、Y 0～42，不再贴灰底外缘 |

### ④ 程序补充

见下文。

---

## 改动清单

| 路径 | 字段 | 现网 | 改为 |
|------|------|------|------|
| `Assets/GameRes/Config/Camera/KenMuNi1_StreetPart3_Blends.asset` | 双向 `m_Time` | 0.4 | **0.8**（`m_Style` 仍为 1 / EaseInOut） |
| `Village_KenMuNi1` · `CameraDepthFollowZone_Part3` | `modeSwitchCooldownSeconds` | 0.4 | **0.8** |
| 同上 Transform | 本地中心 | (-133, 21) | **(-132, 21)** |
| 同上 BoxCollider2D | 尺寸 | 80×58 | **64×42**（offset 仍 0） |

世界盒约 **X -164～-100，Y 0～42**。`hysteresisWorldUnits` 仍 **0.35**。

**未改**：双机、ScreenY、Street 的 XDamping / SoftZone、Part3 纵深、`CameraArea`、商店与龙宫 `smoothTime`。

---

## 为什么这样改

只加时间不够：贴死在灰边上时，两台输出几乎重合，插值再长也像跳。只往里收感应区也不够：台阶内侧仍可能用 0.4 秒滑完约 6 单位的 Y 差。所以两处一起改。冷却与 Blend 等长，避免过渡没完又翻一次。

替代方案：放大 `CameraArea` —— 灰底更宽，贴边时两台仍被钳在新边上，硬切还在，已否决。
