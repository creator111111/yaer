# ForestEast · 树洞行走镜头上漂 · 架构溯源报告

> 日期：2026-09-22  
> 角色：架构侦探（**只读**，未改代码 / 场景 / Prefab）  
> Unity：2020.3.48f1  
> 场景：`ForestEastScene` 倒树 / 树洞（`TreeBridgeLogic` / `isInTreeBridge`）  
> 产品钉死：洞内**走着走着镜头往上移**；人贴下沿、上方空一大块；**不改** `CameraTreeInArea` 几何  
> 提示词：`Assets/Doc/提示词/0922/ForestEast_树洞行走镜头上漂_架构侦探提示词.md`  
> 对照：0914 进洞贴底；0922 树桥 Align / `StopCameraAction` 去掉 rig→(0,0)

---

## ① 结论一句话

**本案不是「0914 贴底失效」，而是「贴底只在进洞/停爬瞬间」+「爬行晃动抖错了对象」。**  
`CameraAction` 变量名叫 MainCamera，实际 DOMove 的是 **`CameraComponent.gameObject`（Camera 根 rig）**——连带 VCam、Brain、**CameraTreeInArea** 一起在世界上移 `y±0.3`；0922 为治出洞闪滑去掉了 Stop 时的 **rig→(0,0)**，Kill Tween 后 **父节点 Y 可残留**；随后 `SnapLiveOrthoYToConfinerFloor` **只 Force VCam Y**，按**已偏移的** Confiner bounds 算 floor，**拉不回 rig 锚点**。多段 `ClimbMoveState` 再进再抖 → **越走越高**。  
纯 A/D 且不进爬行态时 Framing（`DeadZoneHeight=1`）**不会**把 Y 抬高。推荐 **方案 B（主）**：改晃动目标，禁止抖 Camera rig；**方案 A（辅）**：Stop 时仅 **rig.local/world Y 复位** 再 Snap（勿恢复全轴 (0,0)）。**否决**挪边界 / 洞内永久改 DeadZoneHeight。

---

## ② 复现矩阵（代码推演；Play 验收前）

| 操作 | 进洞揭幕瞬间偏高？ | 随后左右走会上漂？ | 爬行时更明显？ |
|------|--------------------|--------------------|----------------|
| 左口进洞 | **推演：否/低**（ChangeCamera Snap + Align 再 Snap） | **是**（进入 ClimbMove 后） | **是** |
| 右口进洞 | 同左 | **是** | **是** |
| 洞内只 A/D、不爬（不进 ClimbMoveState） | — | **否/极低** | — |
| 自动爬 / CameraAction 段 | — | **是** | **是** |
| 停爬后继续走 | — | **是**（再进 ClimbMove → `hasPlayCameraAction` 重置 → 再抖） | **是** |
| 读档落在洞内 | **低**（`CheckPlayerHasInSpcArea` → ChangeCamera+Align） | 同「随后走」 | 同左 |

**与 0914 区分**

| 现象 | 主因 | 本件 |
|------|------|------|
| 刚进洞偏高 | 只换盒/Size、不 Force Y | 0914 已修；本案默认揭幕贴底仍在 |
| **走着上漂** | rig DOMove 残留 + Snap 不复位父节点 | **本案主因** |

---

## ③ 调用链 / Y 谁在改

### 进洞（揭幕前）

```
ForestEastTreeEnterTrigger (isEnterTree=true)
  → 黑幕 onShowEnd
      → ChangeCamera(true)
           → Confiner=CameraTreeInArea；OrthoSize=5
           → SnapLiveOrthoYToConfinerFloor          ★ 0914
      → AlignCameraAfterTreeBridgeChange            ★ 0922 策略 T
           → SetFollow(player)（smoothTime=0 → 当帧）
           → SnapLiveOrthoYToConfinerFloor          ★ 再贴底
      → ClimbUp → ClimbMove…
      → CloseFormFade
```

### 洞内爬行晃动（上漂源）

```
ClimbMoveState.Update（洞内、≥0.2s、每段 Enter 一次）
  → CameraAction()
       → DOMove(CameraComponent.gameObject, y±0.3) 循环  ★ 整 rig（误称 mainCamera）
ClimbMoveState.Exit / 出洞前
  → StopCameraAction()
       → Kill Tween
       → （0922）不再 →(0,0)
       → 若仍在洞内 → SnapLiveOrthoYToConfinerFloor   ★ 只 Force VCam
```

### Snap 公式（仍生效）

`floorY = confiner.bounds.min.y + orthoSize`  
现网盒底约 **-7.9**、洞内 Size **5** → floorY ≈ **-2.9**；合法带约 **[-2.9, 2.9]**（带宽 ~5.8）。  
**DeadZoneHeight=1** → Framing **不跟**玩家 Y；纯走不会靠 CM 把镜头顶到上沿。

### 关键代码（现网）

```148:165:Assets/Scripts/Game/GameMgr/Manager/Story/ForestEastTreeBridgeStoryMgr.cs
    public void CameraAction()
    {
        // ...
        var mainCamera = cameraMgr.CameraComponent.gameObject;  // 实为 Camera 根，非 Main Camera 子物体
        var basePos = mainCamera.transform.position;
        // DOMove y±0.3 无限循环 —— 连带 Confiner 子物体世界坐标上移
```

```168:186:Assets/Scripts/Game/GameMgr/Manager/Story/ForestEastTreeBridgeStoryMgr.cs
    public void StopCameraAction()
    {
        // 0922：禁止拽 MainCamera→(0,0)……
        cameraTween.Kill(true);
        // …仅 Kill；再 Snap VCam 贴底 —— 不复位 rig 父节点 Y
```

```379:394:Assets/Scripts/Game/GameRuntime/GameSceneManager/Component/CameraGSM/CameraComponent.cs
        public void SnapLiveOrthoYToConfinerFloor(Collider2D confinerShape)
        {
            float floorY = confinerShape.bounds.min.y + orthoSize;
            // 只 Force 主 VCam Y；不碰 Camera 根 Transform
            virtualCamera.ForceCameraPosition(new Vector3(p.x, floorY, p.z), rot);
```

### 场景结构（为何抖 rig 致命）

`Camera` 根子节点含：`Cinemachine`（VCam）、`Main Camera`（Brain）、`CameraArea`、`CameraTreeInArea`。  
DOMove 根节点 = **整棵跟拍树 + 边界盒一起漂**。文档写「抖 MainCamera」与代码不符——以代码为准。

### 0914 / 0922 契约对拍

| 检查项 | 现网 | 裁定 |
|--------|------|------|
| `ChangeCamera(true)` 仍 Snap？ | ✅ | 揭幕贴底仍在 |
| Align / 读档仍 Snap？ | ✅ | 同上 |
| 洞内行走持续贴底？ | ❌ | **无** |
| DeadZoneHeight 洞内 | 仍为 **1** | 不跟 Y；非「走路 Framing 抬高」主因 |
| CameraAction 目标 | **Camera rig** | 上漂主因 |
| 0922 去掉 →(0,0) | ✅ | **加重** Y 残留；勿整段回退（会回潮 X 闪滑） |

---

## ④ 方案对比与推荐

| 方案 | 做法 | 何时选 | 裁决 |
|------|------|--------|------|
| **A** | 无晃动时周期性 Snap；或 Stop 时 **仅复位 rig Y→进洞锚点/本地 0** 再 Snap | 作安全网 / 清残留 | **辅** |
| **B** | `CameraAction` 改抖 **Main Camera 本地偏移** / CM Noise / Impulse；**禁止** DOMove Camera rig；停晃再 Snap | 与爬行强相关 | **主修** |
| **C** | 洞内临时减小 DeadZoneHeight，出洞还原 | 不治 rig 漂移；出洞易漏 | **否决为主** |
| **D** | 挪/压矮 CameraTreeInArea | 产品/0914 否决 | **否决** |

### 必答

1. **0914 贴底是否仍生效？为何走着还会高？**  
   仍生效于进洞/Align/停爬瞬间。走着高 = rig 被 DOMove 抬走 + Snap 不复位父节点；不是 Force 被 Framing 顶回去。

2. **0922 去掉 →(0,0) 是否加重？**  
   **是**（Y 残留无清理）。正确意图是避免出洞 **X 闪滑**；施工应 **只补 Y 复位或改抖目标**，禁止恢复全轴 (0,0)。

3. **最小文件 / 验收**  
   主改 `ForestEastTreeBridgeStoryMgr.cs`（`CameraAction` / `StopCameraAction`）；可选 `CameraComponent` 抽 `ResetCameraRigY`。验收：洞内全程（含多段爬）镜头 Y 不单调上升；出洞无 0922 闪滑回潮；盒几何未改。

---

## ⑤ 要改文件（路径级；本阶段未改）

| 全路径 | 预期改动 |
|--------|----------|
| `…/ForestEastTreeBridgeStoryMgr.cs` | B：改晃动目标；A：Stop 仅清 rig Y 再 Snap |
| `…/CameraComponent.cs` | 可选：rig Y 复位 helper |
| `ClimbMoveState.cs` | **默认不动**（触发时机可保留） |
| `ForestEastTreeEnterTrigger` / Align 时序 | **不动**（保留 0922 策略 T） |
| `ForestEastScene` · `CameraTreeInArea` / `CameraArea` | **禁止改几何** |

施工说明：`Assets/Doc/施工说明/0922/ForestEast_树洞行走镜头上漂_施工说明.md`

---

## ⑥ 验收表

| # | 步骤 | 期望 |
|---|------|------|
| 1 | 左/右口进洞揭幕 | 贴底；上方不多余空；Console `afterY≈floorY` |
| 2 | 洞内左右走全程（尽量不爬） | **镜头 Y 不上漂** |
| 3 | 爬行晃动 + 停爬再走 | 可轻微抖；停后贴底；再走不上漂 |
| 4 | 出洞 | Size/Confiner/观感恢复洞外；**无 X 大闪滑**（0922） |
| 5 | 读档落在洞内 | 贴底；走也不上漂 |
| 6 | `CameraTreeInArea` 几何 | **未改** |

建议同时打日志：`Camera` rig world Y、`Main Camera` world Y、VCam world Y、`floorY`。

---

## ⑦ 风险与回滚（0922 闪滑）

| 风险 | 说明 | 处置 |
|------|------|------|
| 恢复 Stop 全轴 →(0,0) | **回潮** 出洞闪滑 | **禁止**；最多只复位 **Y** |
| 改 Align / ChangeCamera 顺序 | 破坏策略 T | 禁止无关改动 |
| 洞内每帧 Force Y | 与 Confiner/X 抢帧 | 限「无 Tween」或 Stop/进洞 |
| 删爬行晃动 | 产品要保留手感 | 只改实现，勿无声删 |
| 误改盒子 | 漏景 | 几何冻结 |

**回滚**：还原 `CameraAction`/`StopCameraAction`；保留 0914 Snap 与 0922 Align。
