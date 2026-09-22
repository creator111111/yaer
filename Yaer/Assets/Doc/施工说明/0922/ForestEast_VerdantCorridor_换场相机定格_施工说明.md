# ForestEast ↔ VerdantCorridor · 换场相机定格 — 施工说明

**文档版本**：v1.0（2026-09-22）  
**文档性质**：【施工员】最小改动（方案 A）  
**Unity**：2020.3.48f1  
**侦探报告**：`Assets/Doc/执行文档/0922/ForestEast_VerdantCorridor_换场相机闪滑_架构溯源报告.md`

---

## 沟通摘要

### ① 结论一句话

**东郊进苍翠走廊（以及走廊回东郊）揭幕时，镜头已定格在落点，不再闪一下再滑。**

### ② 原因（通俗）

换场后镜头还停在目标场景默认机位，人已经刷在门边。黑幕大约 0.3 秒就揭开，镜头还在用 0.3 秒慢慢追人——位移小像闪，位移大像平移。和龙宫下楼、出店回村是同一条缝。

### ③ 用户检查清单

| # | 操作 | 通过判据 |
|---|------|----------|
| 1 | Hierarchy：`VerdantCorridor` → `Camera` → `CameraComponent` | `smoothTime = 0` |
| 2 | Hierarchy：`ForestEastScene` → `Camera` → 同字段 | `smoothTime = 0` |
| 3 | 东郊右门进走廊（非首次 / 首次各一次） | 揭幕无闪、无平移；首次对白仍正常 |
| 4 | 走廊左门回东郊 | 揭幕定格（不再 Δx≈129 大滑） |
| 5 | 走廊内只左右走 | 日常跟拍仍顺（CM 阻尼） |
| 6 | （对照）东郊树洞进洞 / 史莱姆演出；龙宫 Stairs；出店回村 | 演出与对照仍正常 |

### ④ 程序补充

见下文。

---

## 改动清单

| 路径 | 改动 |
|------|------|
| `Assets/GameRes/Scenes/VerdantCorridor.unity` | `Camera` 上 `CameraComponent.smoothTime`：**0.3 → 0**（主诉） |
| `Assets/GameRes/Scenes/ForestEastScene.unity` | 同字段：**0.3 → 0**（盖反向大滑；OPEN Q1 按侦探建议同票） |

**未改**：`LoadSceneComponentGSM` / `CameraComponent.cs` 默认值；门 Trigger / EnterPos；首次进走廊剧情 Prefab；树洞 / 史莱姆演出；HomeScene1/2、KenMuNi1。

---

## 为什么这样改

`smoothTime` 只参与进场 `SetFollow(forceSnap)` 手推。改为 0 后当帧对齐，揭幕时已在落点机位。日常跟拍仍靠 CM FramingTransposer 的 XDamping/SoftZone。树洞 / 史莱姆走独立 `ChangeCamera` / DOMove，不读本字段。旧「Forest 禁止 smoothTime=0」针对林恩可见运镜，不套死本案黑幕定格。
