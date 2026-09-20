# ForestEast 倒树 · 进洞相机贴 CameraTreeInArea 底边 — 架构溯源报告

**文档版本**：v1.0（2026-09-14）  
**文档性质**：【架构侦探】只读核实；**未改**代码 / 场景边界 / Git  
**Unity**：2020.3.48f1  
**场景**：`ForestEastScene` 倒树 / 树洞（`TreeBridgeLogic` / `isInTreeBridge`）  
**现象**：刚进倒树摄像机偏上，上方空太多；洞内希望整体更低  
**产品期望**：  
1. **不改** `CameraTreeInArea` Transform / Polygon  
2. 进洞后镜头贴该边界盒**底边**（Confiner + Ortho 最低合法中心 Y）  
3. 洞内左右仍受该盒约束；**出洞恢复**洞外逻辑  
**不是**：挪/压矮边界；全局改 ScreenY 且出洞不还原；改树桥碰撞/SFX；改洞外 `CameraArea`  
**提示词**：`Assets/Doc/提示词/0914/ForestEast_倒树进洞相机贴边界底边_架构侦探提示词.md`  
**OPEN**：`OPEN_QUESTIONS.md` 本节 Q1～Q4

---

## 沟通摘要

### ① 结论一句话

推荐 **方案 A**：在 `ForestEastTreeBridgeStoryMgr.ChangeCamera(true)` 切完 Confiner + OrthoSize 之后，按公式 **`cameraY = CameraTreeInArea.bounds.min.y + orthoSize`**（现网约 **-2.9**）对主 VCam `ForceCameraPosition` **只改 Y**。ForestEast **没有 Part3 双机**；`DeadZoneHeight=1` 下一般 **Force 一次不会被跟拍抬回去**。

### ② 原因（通俗）

进洞时只换了「镜头能活动的盒子」和「放大倍率」，**没有把镜头往下按**。竖直方向又几乎不跟玩家（死区拉满），所以镜头还停在盒子合法范围里偏上的旧高度，头顶就空一大块。边界盒不能挪（一挪就漏景），只要进洞后主动贴底即可；出来走原来的 `ChangeCamera(false)` 即可。

### ③ 用户需要做什么（检查清单）

| # | 操作 | 现网（未施工） |
|---|------|----------------|
| 1 | 从左/右入口进倒树 | 镜头偏高，上沿空 |
| 2 | Hierarchy：`CameraTreeInArea` | `(258.34, -7.9, 0)`，本地高 0～15.8，**勿改** |
| 3 | 进洞后 VCam OrthoSize | **5**（代码写死） |
| 4 | 出洞 | Size 回 **7.9**，Confiner 回 `CameraArea` |
| 5 | 拍板施工 | 只改 `ChangeCamera(true)` 写入点；不改边界资产 |

### ④ 程序补充

见下文。

---

## 1. 结论（程序向）

| 项 | 裁定 |
|----|------|
| **目标 Y 公式** | `targetY = boundingShape.bounds.min.y + orthoSize`（Ortho + **Confine Screen Edges**） |
| **现网数字** | `min.y ≈ -7.9`，`orthoSize = 5` → **`targetY ≈ -2.9`**；**禁止**裸写 -2.9，要从 bounds + Size 算并注释 |
| **为何偏高** | 进洞不 Force Y；`DeadZoneHeight=1` 不跟玩家 Y；旧相机 Y 落在合法带 `[-2.9, 2.9]` 偏上段 |
| **推荐** | **A**：`ChangeCamera(true)` 末尾 Force 只改 Y；`false` 不留偏移 |
| **B** | 临时改 ScreenY/TrackedObjectOffset，出洞还原；DeadZone=1 时 ScreenY 改构图语义更绕，**不首选** |
| **C/D** | 第二台 VCam / 改边界 → 否 |
| **双机** | 场景 `virtualCameraPart3 = null` → **只 Force 主 VCam** |
| **会否被抬回** | 正常走洞：**不会**（死区满 + Confiner Damping=0）。爬行 `CameraAction` 抖的是 **MainCamera**，不改 VCam；抖完可再贴一次（OPEN Q2） |
| **读档** | `BaseGameSceneManager.CheckPlayerHasInSpcArea` → 同一 `ChangeCamera(true)`，施工后自动带贴底 |

---

## 2. 调用链

### 2.1 进洞（运行时）

```
ForestEastTreeEnterTrigger（isEnterTree=true）
  触碰玩家 → 禁操作 / 暂停
  Idle|Run|Squat 后
    playerIsInTreeBridge = true
    蹲下 → 自动走 1s
    BlackPanel FadeShow.onShowEnd
      传送玩家 X 到 enterNodeLeft/Right
      ForestEastTreeBridgeStoryMgr.ChangeCamera(true, cameraMgr)
        ★ 现网：
          ChangeCameraBoundingArea(CameraTreeInArea.Polygon)
          ChangeVirtualCameraShowSize(5)
          // 无 Force Y → 旧 Y 仅被 Confiner 夹进 [-2.9, 2.9]，常偏上
        ★ 施工后应追加：
          targetY = collider.bounds.min.y + GetVirtualCameraShowSize()
          vcam.ForceCameraPosition((x, targetY, z), rot)
          PreviousStateIsValid = false
          confiner.InvalidateCache()（建议）
      ClimbSM → ClimbUpState → 爬行时 CameraAction（抖 MainCamera）
      OnEnterOrOutTreeBridge(true)
```

### 2.2 出洞

```
Trigger isEnterTree=false
  StopCameraAction()
  …黑幕…
  ChangeCamera(false)
    Confiner → oldCameraBoundingArea（CameraArea）
    ResetVirtualCameraShowSize() → 7.9
  无需贴底残留；跟拍继续走洞外构图
```

### 2.3 读档已在洞内

```
InitPlayer 完成
  SetFollow(player)          // smoothTime=0.3 可能先朝玩家 Y≈-6.61 手推
  CheckPlayerHasInSpcArea
    isInTreeBridge → ChangeCamera(true)
```

跟拍理想 Y 会被 Confiner 抬到 ≥ -2.9；再 Force 贴底可与进洞手感一致，避免手推中途停在中间高度。

---

## 3. 证据表

### 3.1 CameraTreeInArea

| 项 | 值 |
|----|-----|
| Transform | `(258.34, -7.9, 0)` |
| Polygon 本地 | `(0,0)-(81,0)-(81,15.8)-(0,15.8)` |
| 世界 AABB Y | **-7.9 ~ +7.9** |
| Trigger | true（仅作 Confiner Shape） |

`TreeBridgeLogic.newCameraBoundingArea` → 此物体；`oldCameraBoundingArea` → `CameraArea`（洞外，本地 Y ±7.9，世界原点）。

### 3.2 Size / Confiner / Framing（ForestEast 主 VCam）

| 项 | 值 |
|----|-----|
| 进洞 OrthoSize | **5**（`ChangeCamera` 写死） |
| 出洞 OrthoSize | **7.9**（`ResetVirtualCameraShowSize`） |
| `m_ConfineScreenEdges` | **1** |
| `m_Damping` | **0**（无缓入，贴底可立即到公式值） |
| `m_ScreenY` | 0.5 |
| `m_TrackedObjectOffset` | (0,0,0) |
| `m_DeadZoneHeight` | **1** |
| `m_SoftZoneHeight` | **2**（提示词写 1，场景实测为 **2**） |
| `m_YDamping` | 0 |
| `virtualCameraPart3` | **null**（无双机） |

合法相机中心 Y（进洞 Size=5）：

```
min = bounds.min.y + size = -7.9 + 5 = -2.9
max = bounds.max.y - size =  7.9 - 5 =  2.9
贴底 → -2.9
```

### 3.3 公式是否成立？

**是。** Orthographic + Confine Screen Edges：画面下沿 = `cameraY - orthoSize`，贴盒底 ⇒ `cameraY = min.y + orthoSize`。Damping=0、无项目内 padding。Size 若改，公式跟 `GetVirtualCameraShowSize()` / 刚写入的 Size，**不要**魔法数 -2.9。

### 3.4 DeadZone=1 与 Force 一次

竖直死区拉满 → 玩家上下移动**不会**因 Framing 改相机 Y。Force 到 -2.9 后，左右仍由 Framing X + Confiner 管。  
**一般不需要每帧夹紧**（OPEN Q1）。若验收发现某扩展又改 SoftZone/死区，再加 Periodic clamp。

### 3.5 CameraAction

| 项 | 行为 |
|----|------|
| 触发 | `ClimbMoveState` 洞内爬行约 0.2s |
| 对象 | **`Camera.main` Transform**，不是 VCam |
| 动作 | Y±0.3 循环 DOTween |
| 停止 | 出爬行态 / 出洞 Trigger：`StopCameraAction`；末尾还有一次把 MainCamera 移向 **`Vector2.zero`**（怪，Brain 下帧会盖回） |

→ **不永久抬高 VCam**。贴底以 VCam 为准。建议抖停后若仍在洞内再 Force 一次（OPEN Q2）。`(0,0)` 另案（OPEN Q3）。

---

## 4. 方案对比与推荐

| # | 方向 | 评价 |
|---|------|------|
| **A** | `ChangeCamera(true)` 后公式 Force Y | **推荐**：写入点唯一；不碰边界；出洞走 false；读档复用 |
| **A+** | A + `InvalidateCache` +（可选）`StopCameraAction` 末再贴底 | 施工建议顺手 Invalidate；抖完回贴见 OPEN Q2 |
| **B** | 临时 ScreenY / TrackedObjectOffset | DeadZone=1 时不直观；双端还原易漏；不首选 |
| **C** | 洞内第二 VCam | 改动大 |
| **D** | 改 `CameraTreeInArea` | **用户否决** |

**替代方案**：若产品接受「跟玩家脚」而不是「贴盒底」，可临时把 DeadZoneHeight 降下来——会改变爬行时上下跟拍，**非本案钉死目标**。

### 4.1 施工写入点草案（给施工员）

文件：`ForestEastTreeBridgeStoryMgr.ChangeCamera`

```
ChangeCameraBoundingArea(collider)
if (isEnterTree) {
  ChangeVirtualCameraShowSize(5)
  // 贴底：不改 CameraTreeInArea 几何
  var vcam = cameraMgr.CameraComponent.VirtualCamera
  float size = cameraMgr.GetVirtualCameraShowSize()
  float floorY = collider.bounds.min.y + size
  var p = vcam.transform.position
  vcam.ForceCameraPosition(new Vector3(p.x, floorY, p.z), vcam.transform.rotation)
  vcam.PreviousStateIsValid = false
  // 建议：confiner.InvalidateCache()
} else {
  ResetVirtualCameraShowSize()
}
```

可选：`CameraComponent` 抽 `SnapLiveOrthoYToConfinerFloor(Collider2D)` 并注释公式，StoryMgr 一行调用（便于读档/抖完复用）。

出洞还原清单：

- [ ] Confiner 已回 `CameraArea`
- [ ] Size 已回 7.9
- [ ] 未留下进洞专用 ScreenY/Offset
- [ ] 未改 `CameraTreeInArea` 资产

---

## 5. 验收矩阵

| 操作 | 期望 |
|------|------|
| 左/右入口刚进洞 | 镜头明显更低；上沿不露洞外穿帮 |
| 洞内左右走 | 仍被 `CameraTreeInArea` 夹；Y 保持贴底附近 |
| 出洞 | Size/边界/构图恢复洞外 |
| 读档在洞内 | 加载后贴底 |
| 爬行 CameraAction | 可短暂抖；不永久抬离贴底 |

Debug 前缀建议：`[TreeBridgeCam]` — 打 `bounds.min.y`、`orthoSize`、计算 `floorY`、实际 `vcam.position.y`。

---

## 6. 侦探须答汇总

1. **公式**：确为 `min.y + size`；Damping=0，无 padding。  
2. **双机**：ForestEast Part3 为空，只 Force 主 VCam。  
3. **回弹**：DeadZoneHeight=1 下 Force 一次即可；一般不必每帧夹。  
4. **CameraAction**：抖 MainCamera，不改 VCam；建议停抖后再贴一次。  
5. **推荐 A（+InvalidateCache）**；写入点 `ChangeCamera(true)`；出洞走现有 false。

---

*拍板后把提示词【施工员】段交给 Agent。禁止改 `CameraTreeInArea` / `CameraArea` 几何。*
