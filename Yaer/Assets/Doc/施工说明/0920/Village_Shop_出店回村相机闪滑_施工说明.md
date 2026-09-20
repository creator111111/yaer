# Village_Shop · 出店回村相机闪滑 — 施工说明

**文档版本**：v1.0（2026-09-20）  
**文档性质**：【施工员】最小改动  
**Unity**：2020.3.48f1  
**侦探报告**：`Assets/Doc/执行文档/0920/Village_Shop_出店回村相机闪滑_架构溯源报告.md`

---

## 沟通摘要

### ① 结论一句话

**从商店回村揭幕时，镜头已定格在店门外，不再闪一下再大滑。**

### ② 原因（通俗）

回村时人在店门外偏左，默认镜头还在村街偏右。黑幕大约 0.3 秒就揭开，但镜头还在用 0.3 秒慢慢追人，所以你会看到闪一下又大滑。和龙宫下楼是同一条缝。

### ③ 用户检查清单

| # | 操作 | 通过判据 |
|---|------|----------|
| 1 | Hierarchy：`Village_KenMuNi1` → `Camera` → `CameraComponent` | `smoothTime = 0`（不是 0.3） |
| 2 | 村里进店 → ESC 或离开回村 | 揭幕无闪、无可见大滑 |
| 3 | 再进再出 ≥2 次 | 同上 |
| 4 | 回村后只左右走 | 日常跟拍仍顺 |
| 5 | （对照）龙宫 Stairs 上下 | 仍定格 |
| 6 | Console | `[ShopEscExit] LoadScene Village_KenMuNi1`；`[SceneLoad] … blackFade=True` |

### ④ 程序补充

见下文。

---

## 改动清单

| 路径 | 改动 |
|------|------|
| `Assets/GameRes/Scenes/Village_KenMuNi1.unity` | `Camera` 上 `CameraComponent.smoothTime`：**0.3 → 0**（与 0912 HomeScene1/2 同款） |

**未改**：`Village_Shop` 场景相机、`ExitShopToVillage` / `LoadSceneComponentGSM` / `CameraComponent.cs`、`EnterFrom_Shop`、VCam Damping / Part3、HomeScene1/2。

---

## 为什么这样改

`smoothTime` 只参与进场 `SetFollow(forceSnap)` 手推。改为 0 后走当帧对齐，揭幕时已在门外机位。日常横移仍靠 CM FramingTransposer 的 XDamping/SoftZone，不读这个字段。

否决：公共换场等 onComplete 再揭幕（面大）；只挪默认 VCam 靠近店门（消灭不了契约缝，还打乱村街构图）。
