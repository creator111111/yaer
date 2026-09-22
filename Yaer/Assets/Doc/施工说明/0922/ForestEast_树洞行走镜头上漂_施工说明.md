# ForestEast · 树洞行走镜头上漂 — 施工说明

**文档版本**：v1.0（2026-09-22）  
**文档性质**：【施工员】方案 B（主）+ A（辅）  
**Unity**：2020.3.48f1  
**侦探报告**：`Assets/Doc/执行文档/0922/ForestEast_树洞行走镜头上漂_架构溯源报告.md`

---

## 沟通摘要

### ① 结论一句话

**洞内走/爬时镜头不应再越走越高；爬行晃动仍在，出洞无 0922 闪滑回潮。**

### ② 原因（通俗）

以前晃动抖的是整棵相机根（边界盒一起抬），停了还不把根 Y 收回来，贴底公式跟着抬高的盒子算 → 越走越高。现改抖构图偏移，停时只清根 local Y 再贴底。

### ③ 用户检查清单

| # | 步骤 | 期望 |
|---|------|------|
| 1 | 左/右口进洞揭幕 | 贴底；上方不多余空 |
| 2 | 洞内左右走（尽量不爬） | 镜头 Y 不上漂 |
| 3 | 爬行晃动 + 停爬再走 | 可轻抖；停后贴底；再走不上漂 |
| 4 | 出洞 | 恢复洞外；**无大 X 闪滑** |
| 5 | 读档落洞内 | 贴底；走也不上漂 |
| 6 | CameraTreeInArea | **未改几何** |

### ④ 程序补充

见下文。

---

## 改动清单

| 路径 | 改动 |
|------|------|
| `ForestEastTreeBridgeStoryMgr.cs` | `CameraAction` 改抖 `FramingTransposer.TrackedObjectOffset.y`（±0.3 循环）；`StopCameraAction` 还原 Offset + `ResetCameraRigLocalY` + Snap |
| `CameraComponent.cs` | 新增 `ResetCameraRigLocalY`（只清 local Y） |

**未改**：ClimbMoveState 触发时机；Align / ChangeCamera；CameraTreeInArea 几何；禁止恢复 Stop 全轴 (0,0)。

---

## 为什么这样改

抖根节点会抬 Confiner；只 Snap VCam 治不了父节点残留。改构图偏移保留晃动手感且不动盒；Stop 只复位 Y 避免 0922 闪滑回潮。
