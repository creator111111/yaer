# ForestEast 倒树 · 进洞相机贴边界底边 — 施工说明

**文档版本**：v1.0（2026-09-14）  
**文档性质**：【施工员】按侦探报告方案 **A（+InvalidateCache + 抖完再贴）**  
**Unity**：2020.3.48f1  
**侦探报告**：`Assets/Doc/执行文档/0914/ForestEast_倒树进洞相机贴边界底边_架构溯源报告.md`

---

## 沟通摘要

### ① 结论一句话

**进倒树后镜头会按公式贴到 `CameraTreeInArea` 底边；边界盒没挪，出洞走原来的还原。**

### ② 原因（通俗）

进洞以前只换了「镜头能活动的盒子」和放大倍率，没有把镜头往下按。竖直方向又不跟玩家，所以镜头还停在盒子里偏上的高度，头顶空一大块。现在进洞后主动按到底。

### ③ 用户检查清单

| # | 操作 | 通过判据 |
|---|------|----------|
| 1 | 左/右入口刚进洞 | 镜头明显更低；上沿不露洞外穿帮 |
| 2 | Console `[TreeBridgeCam]` | 有 `minY / size / floorY / beforeY / afterY`（afterY≈−2.9） |
| 3 | 洞内左右走 | 仍被树洞盒夹；Y 保持贴底附近 |
| 4 | 出洞 | Size 回 7.9、边界回 CameraArea、构图恢复洞外 |
| 5 | 读档在洞内 | 加载后贴底 |
| 6 | 爬行再停 | 可短暂抖；停后仍贴底 |
| 7 | Hierarchy `CameraTreeInArea` | Transform/Polygon **未改** |

### ④ 程序补充

见下文。

---

## 改动清单

| 项 | 路径 | 说明 |
|----|------|------|
| **贴底 API** | `CameraComponent.SnapLiveOrthoYToConfinerFloor` | `floorY = bounds.min.y + OrthoSize`；Force 只改 Y；`PreviousStateIsValid=false`；`InvalidateCache` |
| **GSM 透传** | `CameraComponentGSM.SnapLiveOrthoYToConfinerFloor` | 一行透传 |
| **写入点** | `ForestEastTreeBridgeStoryMgr.ChangeCamera(true)` | 切盒 + Size=5 **之后** 调贴底；`false` 仍只 Reset Size |
| **抖完再贴** | `StopCameraAction` 末尾 | 若 `playerIsInTreeBridge` 再 Force 一次（OPEN Q2） |
| **换盒清缓存** | `SetConfiner` | 赋值后 `InvalidateCache`（进洞/出洞都受益） |

公式（Ortho + Confine Screen Edges，无 padding）：

```
targetY = CameraTreeInArea.bounds.min.y + OrthographicSize
现网 ≈ -7.9 + 5 = -2.9（禁止代码里写死 -2.9）
```

### 未改

- `CameraTreeInArea` / `CameraArea` 几何  
- ScreenY / TrackedObjectOffset / DeadZone  
- 树桥碰撞 / SFX / 玩法数值  
- `StopCameraAction` 拽 MainCamera 到 `(0,0)`（OPEN Q3 另案）  
- 每帧夹紧（OPEN Q1 否）

---

## OPEN 施工默认（已落地）

| ID | 施工默认 |
|----|----------|
| Q1 每帧夹 | **否** |
| Q2 抖完再贴 | **是** |
| Q3 MainCamera→(0,0) | **本期否** |
| Q4 padding | **否** |

---

## 替代方案（未采用）

- **B** 临时 ScreenY/Offset：DeadZone=1 时不直观，出洞易漏还原  
- **C** 洞内第二 VCam：改动大  
- **D** 改边界：用户否决  

---

## 剩余风险

- 爬行抖 MainCamera 期间肉眼可能短暂偏高，停抖后应回贴底。  
- 若日后把 DeadZoneHeight 降下来，Force 一次可能被跟拍抬回，再开 Periodic clamp。  
- 本机未 Play；请按清单验收。
