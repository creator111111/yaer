# Cursor Agent Prompt · ForestEast 倒树：进洞相机贴 CameraTreeInArea 底边（不挪边界）

> **角色**：先【架构侦探】只读核实公式与写入点；拍板后【施工员】最小改  
> **日期**：2026-09-14  
> **场景**：`ForestEastScene` 倒树 / 树洞（`TreeBridge` / `isInTreeBridge`）  
> **现象（用户实测 + 截图）**：刚进倒树时摄像机 **偏上**，画面上方空太多；洞内整段都希望再低一点  
> **产品期望（钉死）**：  
> 1. **不改** `CameraTreeInArea` 的 Transform / Polygon（挪边界会穿帮漏景）  
> 2. 进洞后摄像机 **贴住该边界盒底边**（在 Confiner + OrthoSize 允许的最低合法 Y）  
> 3. 洞内左右移动仍被现有 `CameraTreeInArea` 约束；**出洞恢复**洞外相机逻辑，不污染洞外构图  
> **不是**：下移/压矮 `CameraTreeInArea`；改全局共享 VCam 的 `ScreenY` / `TrackedObjectOffset` 且出洞不还原；改树桥玩法/碰撞/SFX；改洞外 `CameraArea`  
> **对照**：  
> - 进洞：`ForestEastTreeBridgeStoryMgr.ChangeCamera(true)` → 切 Confiner=`CameraTreeInArea` + `ChangeVirtualCameraShowSize(5)`  
> - 出洞：`ChangeCamera(false)` → 切回 `oldCameraBoundingArea`（`CameraArea`）+ `ResetVirtualCameraShowSize()`（7.9）  
> - 读档已在洞内：`BaseGameSceneManager` 也会 `ChangeCamera(true)`  
> **报告落盘**：`Assets/Doc/执行文档/0914/ForestEast_倒树进洞相机贴边界底边_架构溯源报告.md`

把下面「侦探」整段交给 Cursor Agent。施工见文末。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 产品白话

> 倒树里面镜头太高了，上面空一大块。  
> 边界盒别动（一动就穿帮），只要进洞后镜头往下贴着盒底就行；出来还是外面的镜头。

### 现网关键数（须再核实）

| 项 | 现网值 |
|----|--------|
| `CameraTreeInArea` Transform | `(258.34, **-7.9**, 0)` |
| Polygon 本地 Y | `0` → `15.8` → 世界约 **-7.9 ~ +7.9** |
| 进洞 Ortho Size | **5**（代码写死） |
| 出洞 Ortho Size | **7.9** |
| Confiner | `m_ConfineScreenEdges: 1`（夹的是画面边，不是裸相机点） |
| Framing | `ScreenY=0.5`，`TrackedObjectOffset=(0,0,0)`，**`DeadZoneHeight=1`**（竖直几乎不跟玩家） |

### 「贴底」目标 Y（公式，侦探须用现网 Confiner 模式验算）

在 **Confine Screen Edges + Orthographic** 下，画面下沿贴盒底时：

```
cameraCenterY ≈ bounds.min.y + orthoSize
             ≈ -7.9 + 5
             ≈ -2.9
```

→ 进洞后应把虚拟相机 **Y 压到约 -2.9**（X 仍跟玩家 / 现有逻辑；Z 不变）。Size 若变，公式跟着变，**禁止写死魔法数不注释**。

### 为何只挪边界不行 / 为何只手拖相机不行

| 做法 | 问题 |
|------|------|
| 下移 `CameraTreeInArea` | 用户否决：穿帮漏景 |
| 压矮 Polygon 顶 | 同属改边界，非本案 |
| Scene 里手拖 Cinemachine Y | 进洞 `ChangeCamera` / 跟拍一跑就覆盖 |
| 全局改 `ScreenY` | 洞外构图一起坏（除非进/出对称还原，且与 DeadZone=1 交互须核实） |

### 方案优先级（侦探排）

| # | 方向 | 利弊 |
|---|------|------|
| **A（推荐）** | 在 `ChangeCamera(true)` 成功切 Confiner+Size 后，**按公式计算底边 Y**，对 Live VCam（及项目双机策略下需同步的 Part3）`ForceCameraPosition` / 等价吸附，**只改 Y** | 写入点唯一；出洞走现有 false 分支即可；不碰边界资产 |
| **B** | 进洞临时改 Framing（`TrackedObjectOffset.y` 或 `ScreenY`），出洞还原 | 能「看起来更低」；须与 DeadZone=1 核实是否生效；双机都要还原 |
| **C** | 树洞专用第二台 VCam + Priority | 干净但改动面大，非最小 |
| **D（否决）** | 改 `CameraTreeInArea` Transform/Polygon | 用户明确禁止 |

### 复现 / 验收矩阵

| 操作 | 期望 |
|------|------|
| 刚进倒树（左/右入口各 1） | 镜头明显更低；上沿不穿出树洞美术/不露不该露的洞外 |
| 洞内左右走 | 仍被 `CameraTreeInArea` 夹住；Y 保持贴底（或不被抬回偏高） |
| 出洞 | Size/边界/构图恢复洞外；无残留偏移 |
| 读档已在树洞 | 加载后同样贴底（走 GSM 的 `ChangeCamera(true)`） |
| 虫事件 `CameraAction` 抖 Y | 不永久把镜头抬离贴底（或抖完回贴底——侦探裁定） |

### 侦探须回答

1. Confine Screen Edges 下最低合法 `cameraCenterY` 是否确为 `min.y + size`？有无 padding / damping 导致略高？  
2. 双机（主 VCam / Part3）进洞是否都要 Force？现网 ForestEast 是否用 Part3？  
3. `DeadZoneHeight=1` 下，仅 Force 一次后，后续帧会不会被 Framing/跟拍抬回去？若会，A 是否需每帧夹紧或临时关 Follow Y？  
4. `CameraAction`（+0.3 循环）与贴底是否冲突？  
5. 推荐 **A / A+微调**；写出施工写入点与出洞还原清单。

### 必读

1. `Assets/Project_context.md`、`Assets/Doc/02_SYSTEM_SPEC.md`（相机用 onComplete，忌硬等）  
2. `ForestEastTreeBridgeStoryMgr.ChangeCamera`  
3. `ForestEastTreeEnterTrigger`（黑幕后调 ChangeCamera）  
4. `BaseGameSceneManager` 读档 `isInTreeBridge` 分支  
5. `CameraComponent`：`ChangeCameraBoundingArea` / `ChangeVirtualCameraShowSize` / `ForceCameraPosition` / `SetFollow`  
6. 场景：`CameraTreeInArea`、`CameraArea`、Cinemachine Confiner  
7. 本提示词 + 用户「偏上」截图  

### 禁止（侦探阶段）

- 禁止改代码 / 场景边界资产 / Git  
- 禁止首推改 `CameraTreeInArea`  
- 禁止只改共享 Framing 且无出洞还原方案  

---

## 【架构侦探】任务（整段复制执行）

你是【架构侦探】。Unity **2020.3.48f1** / C#。只读。

### 输出

`Assets/Doc/执行文档/0914/ForestEast_倒树进洞相机贴边界底边_架构溯源报告.md`

结构：

1. **结论一句话**（推荐方案 + 目标 Y 公式）  
2. **调用链**（进洞/出洞/读档 → ChangeCamera → Confiner/Size → 当前为何偏高）  
3. **证据表**（边界、Size、ConfineScreenEdges、DeadZone、双机）  
4. **方案 ≥2** + 推荐 + 与 `CameraAction` 关系  
5. 不清处记 `Assets/Doc/OPEN_QUESTIONS.md`  

回答风格：①结论 ②原因白话 ③检查清单 ④程序补充。

---

## 【施工员】Prompt（侦探报告拍板后再复制）

> **前置**：报告确认 **不改 CameraTreeInArea**；进洞贴底用公式 `min.y + orthoSize`（或报告修订公式）；明确双机与是否防回弹。  
> **目标**：进倒树后镜头贴 `CameraTreeInArea` 底边；洞内不穿帮；出洞/读档行为正确。  
> **优先写入点**：`ForestEastTreeBridgeStoryMgr.ChangeCamera`（`isEnterTree==true` 分支，在切 Confiner + Size **之后**）；`false` 分支保证无残留。读档路径复用同一 API，勿另开旁路。  
> **质量**：详细注释（公式、为何不改边界、替代方案）；忌无注释魔法数；最小改，不重写相机系统。  
> **禁止**：改 `CameraTreeInArea` / `CameraArea` 几何；全局永久改 ScreenY；改树桥玩法数值。  
> **文档**：`Assets/Doc/施工说明/0914/ForestEast_倒树进洞相机贴边界底边_施工说明.md`  
> **验收**：按上文矩阵；进/出各 ≥2 次；读档在洞内 1 次；截图对比「贴底前后」上沿空域。

---

## 【验收员】Prompt（可选）

> Debug 前缀建议 `[TreeBridgeCam]`：进洞打 `bounds.min.y`、`orthoSize`、计算目标 Y、实际 vcam.position.y；出洞打 Size 与 BoundingShape 名。  
> 输出：通过项 + 剩余风险（CameraAction 抖、双机、帧后被抬回）。
