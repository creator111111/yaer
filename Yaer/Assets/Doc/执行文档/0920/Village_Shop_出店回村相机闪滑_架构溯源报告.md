# Village_Shop 出店回村相机闪滑 — 架构溯源报告

> 日期：2026-09-20  
> 角色：架构侦探（只读，未改代码 / 场景 / Prefab）  
> Unity：2020.3.48f1  
> 对照：0912 HomeScene1/2 Stairs 定格；0829 ESC 离店。以现网代码与 YAML 为准。

---

## ① 结论一句话

**与 Stairs 同源**：回村 `InitPlayer → SetFollow(forceSnap)` 在 `smoothTime=0.3` 下手推，黑幕 hold≈0.3s 就揭幕，对齐未完就露景。最小改法仍是 **方案 A**：只把 `Village_KenMuNi1` 的 `CameraComponent.smoothTime` 改为 **0**（日常跟拍仍靠 CM Damping，不靠这个字段）。

---

## ② 原因

大白话：从商店回村时，镜头先停在村里默认机位（偏右），人却刷在店门外（偏左很多）。黑幕还没淡完，镜头就开始慢慢滑过去追人，所以你会看到闪一下又大滑。龙宫下楼是同一条缝：手推没收完就揭幕。村里滑得更狠，因为默认机位和门外落点差了约 **62** 个世界单位（龙宫下楼大约 20）。

### 出店调用链

```
ESC / 离开按钮
  → Village_ShopSceneManager.ExitShopToVillage
       stayAction = HideShopUiRoot（只藏店 UI，不碰回村相机）
       LoadSceneComponentGSM.LoadScene(Village_KenMuNi1, stayAction)
         blackFade 默认 true
  → 黑幕全黑 → 卸店 → 加载 Village_KenMuNi1
  → BaseGameSceneManager.InitPlayer
       SetPlayerPos（LastScene=Village_Shop → EnterFrom_Shop）
       CameraComponentGSM.SetFollow(player)   // forceSnap 默认 true
            smoothTime=0.3 → 手推 VCam + 清 Follow（_isSmoothingSnap）
  → Ready 后 hold mapTransitionBlackHoldSeconds(0.3s) → CloseFormFade
       （不等 SetFollow onComplete；出店无 TryDeferBlackFadeForCover 接管）
  → 揭幕时手推仍在跑 → 闪 + 可见大滑
```

### 与 Stairs 对照表

| 检查项 | Stairs（0912 已知） | 出店回村（现网） |
|--------|---------------------|------------------|
| 换场入口 | Stairs `SceneChangeDoor` | `ExitShopToVillage` / 离开按钮（同源 LoadScene） |
| 目标场景 | HomeScene1 / 2 | `Village_KenMuNi1` |
| blackFade / hold | true / ≈0.3s | **true / 0.3s**（`LoadScene` 默认；`mapTransitionBlackHoldSeconds`） |
| InitPlayer → SetFollow forceSnap | 是 | **是**（`BaseGameSceneManager` 337 行，无 onComplete） |
| 目标 `CameraComponent.smoothTime` | 已改为 0 | **磁盘仍为 `0.3`**（`Village_KenMuNi1` Camera 节点已序列化） |
| 默认 VCam 世界坐标 | HS1 约 (-19.5, 0) 等 | **`VCam_Street`**：父 `Camera`(32.56, 0) + 本地 (0,0) → **(32.56, 0)**；Priority 10。`VCam_Part3` Priority **0** 待机，进场不亮 |
| 落点 | Stairs EnterPos | **`EnterFrom_Shop` (-29.04, -6.5)**；EnterPos 表 `lastScene: Village_Shop` 已配对 |
| 默认机位 ↔ 落点 | 下楼 Δx≈20 | **Δx ≈ -61.6**，**Δy ≈ -6.5**（比 Stairs 更大，滑更明显） |

**裁定：同源**（同一 SetFollow 手推 + 早揭幕）。不是「另写一套出店相机 bug」；落点正确，不是落错点再追。

### B. 出店路径「额外」改相机（加重 / 排除）

| 项 | 现网 | 判定 |
|----|------|------|
| 店内 `CancelFollow` / `SetLock` | `LockShopCameraPipeline` 只动**商店场景**相机 | 换场后 KenMuNi1 是新 GSM/新 Camera，**不残留** |
| `stayAction` | 只 `HideShopUiRoot` | **不**改回村机位 |
| 回村 `OnEnterScene` CancelFollow | 仅 `homeDoorStoryComplete == false`（门口开场未完） | 正常出店回村时开场已 used → **不进**；不是本案主因 |
| Part3 Zone | 区心约 (-133, 21)、盒约 80×58 → 世界约 x∈[-173,-93] | `EnterFrom_Shop` x=-29 **不在区内**；进场当帧不会切 Part3 Priority |

以上可作加重因素排除或降级；**主因仍是 KenMuNi1 的 smoothTime=0.3 + hold 0.3 揭幕**。

---

## ③ 用户需要做什么

1. 村里进店 → ESC 或离开回村：揭幕时是否闪、是否大滑（预期现网有，且偏左大滑）。  
2. 再进再出 ≥2 次：同一现象。  
3. Hierarchy：`Village_KenMuNi1` → `Camera` → `CameraComponent` → 现网 **`smoothTime = 0.3`**（施工后应为 **0**）。  
4. 回村后只左右走：日常跟拍应仍顺（验收项 3）。  
5. 对照龙宫 Stairs：仍应定格（验收项 4）。  
6. Console：`[ShopEscExit] LoadScene Village_KenMuNi1`；`[SceneLoad] scene=Village_KenMuNi1 blackFade=True`。

---

## ④ 给施工员的补充

### 改哪些 / 不要改哪些

| 全路径 | 做什么 |
|--------|--------|
| `Assets/GameRes/Scenes/Village_KenMuNi1.unity` | **只改** `Camera` 上 `CameraComponent.smoothTime`：**0.3 → 0**（与 0912 HS1/HS2 同款） |
| `Village_Shop.unity` 的 `smoothTime` | **不改**（店内无玩家跟拍；预判不用） |
| `ExitShopToVillage` / `LoadSceneComponentGSM` / `CameraComponent.cs` | **不改**契约；不必等 onComplete 再揭幕 |
| `EnterFrom_Shop` / EnterPos 表 | **不改**（落点已对） |
| `VCam_Street` / Part3 / Framing Damping | **不改**（日常跟拍靠这些） |
| HomeScene1/2 | **不改**（Stairs 已定格） |

### 推荐方案（只此一种）— 方案 A

仅 `Village_KenMuNi1` 的 `CameraComponent.smoothTime = 0`。进场 `SetFollow` 走当帧 `ApplyFollowWithCinemachineStateAligned`，揭幕时已在门外机位。

| 必答 | 答案 |
|------|------|
| 会不会误伤村里日常横移 / Part3 纵深？ | **不会按主因误伤。** `smoothTime` 只参与 `forceSnap` 手推；日常跟拍是 CM FramingTransposer 的 XDamping/SoftZone。Part3 只改 Priority，不写 Body、不读 smoothTime |
| 要不要动 Village_Shop 场景相机？ | **不用** |
| 为何不把 Forest「禁止 smoothTime=0」套到本案？ | Forest 门口林恩要**保留可见运镜**（0912 纠偏否决瞬切终态）。本案与 Stairs 一样是**换场黑幕下定格**，产品要揭幕已到位，不是演出推镜 |

### 否决方案

- **方案 B**（公共换场等 onComplete 再揭幕）：A 已够；改契约面大、所有 blackFade 换场回归成本高。  
- **方案 C**（只挪默认 VCam 靠近 EnterFrom_Shop）：缩小滑幅但 **消灭不了**「hold 0.3 vs 手推未完」的契约缝；还会打乱村街默认构图。

### 验收矩阵

| # | 操作 | 期望 |
|---|------|------|
| 1 | 村里进店 → ESC 或离开回村 | 揭幕无闪、无可见大滑 |
| 2 | 再进店再出 ≥2 次 | 同上 |
| 3 | 回村后只左右走 | 日常跟拍仍顺 |
| 4 | （对照）龙宫 Stairs 上下 | 仍定格 |
| 5 | Console | `[ShopEscExit] LoadScene` / `[SceneLoad] … Village_KenMuNi1` |

### OPEN

无新核心设计冲突。施工后若验收发现「从其它入口进村也瞬切」属预期副作用（与 Stairs 允许 Forest→HS1 瞬切同口径），不必另开产品票除非策划要「仅出店瞬切、其它进村仍手推」——那种才要特判，本票不推荐。
