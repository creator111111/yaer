# Cursor Agent Prompt · ForestEast 树洞：走着走着镜头往上移

> **角色**：【架构侦探】只读；对照 0914 进洞贴底、0922 树桥 Align / 停晃改动，查清「洞内行走镜头上漂」  
> **日期**：2026-09-22  
> **场景**：`ForestEastScene` 倒树 / 树洞（`TreeBridgeLogic` / `isInTreeBridge`）  
> **现象（用户 + 截图）**：树洞里**走着走着摄像机往上移**；人贴画面下沿，上方空一大块（天/洞顶空区）  
> **不是**：挪/压矮 `CameraTreeInArea`；改洞外 `CameraArea`；改换场 `smoothTime`；关爬行晃动音效；改 Part3（ForestEast 无 Part3）  

把下面「侦探」整段交给 Agent。没分清「进洞就偏高」vs「走着才上漂」、没对拍 `CameraAction` / Snap 时机之前，不要施工。

---

## 提示词助手预梳理（须证伪）

### 产品钉死

| 项 | 要求 |
|----|------|
| 洞内观感 | 镜头应保持贴底/合理高度；**左右走不应越走越高** |
| 边界 | **不改** `CameraTreeInArea` 几何 |
| 出洞 | 恢复洞外 Size/Confiner，不残留洞内 Y |

### 现网已知（对照，勿当终裁）

| 案 | 结论 |
|----|------|
| **0914 进洞贴底** | 偏高因 `DeadZoneHeight=1` 不跟 Y + 进洞只换盒/Size；修法：`ChangeCamera(true)` 后 `SnapLiveOrthoYToConfinerFloor`（`min.y + orthoSize`） |
| **0922 树桥闪滑** | 揭幕前 `AlignCameraAfterTreeBridgeChange`；`StopCameraAction` **去掉** MainCamera→(0,0) |
| **爬行晃动** | `CameraAction` 对 **MainCamera** DOMove `y±0.3` 循环；停时 Kill Tween，洞内再 Snap 一次 |

假说（侦探须证伪）：

1. **贴底只在进洞/停爬瞬间**，洞内左右走无持续约束 → 一旦 Y 被抬高（晃动抢位 / Confiner 合法带上沿 / Framing）就**回不来**  
2. **`CameraAction` 抖 MainCamera** 与 CM Brain 抢位，0922 去掉回 (0,0) 后，表现为**越走越往上**  
3. 用户截图若含战斗立绘/HUD，确认是否同一洞内路径，勿误判成战斗相机

### 强制排除

| 勿当主因 | 理由 |
|----------|------|
| 换场 `smoothTime` | 洞内日常跟拍不走进场手推 |
| 挪边界盒 | 0914/产品否决 |
| 全局 ScreenY 永久改掉且出洞不还原 | 出洞易穿帮 |

---

## 侦探（复制给 Agent）

```
你是【架构侦探】。只读分析，不改代码、不改场景、不提 MR。

任务：查清 ForestEast 树洞内「走着走着镜头往上移」的根因，给出最小修复推荐。必须与 0914「刚进洞偏高」区分：本案强调**行走过程中上漂**。

### 必读

@Assets/Doc/执行文档/0914/ForestEast_倒树进洞相机贴边界底边_架构溯源报告.md
@Assets/Doc/施工说明/0914/ForestEast_倒树进洞相机贴边界底边_施工说明.md
@Assets/Doc/执行文档/0922/换场与读档_相机闪滑_全量盘点_架构溯源报告.md
@Assets/Doc/施工说明/0922/换场与读档_相机闪滑_施工说明.md
@Assets/Scripts/Game/GameMgr/Manager/Story/ForestEastTreeBridgeStoryMgr.cs
@Assets/Scripts/Game/GameRuntime/Entities/SceneEntities/CommonEntity/ForestEastTreeEnterTrigger.cs
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Component/CameraGSM/CameraComponent.cs
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Component/CameraGSM/CameraComponentGSM.cs

场景：`ForestEastScene` — `CameraTreeInArea` / `CameraArea` / TreeBridge 相关；VCam Framing DeadZoneHeight / SoftZone / Confiner。

### A. 复现矩阵（必须填）

| 操作 | 进洞揭幕瞬间偏高？ | 随后左右走会上漂？ | 爬行时更明显？ |
|------|--------------------|--------------------|----------------|
| 左口进洞 | ？ | ？ | ？ |
| 右口进洞 | ？ | ？ | ？ |
| 洞内只 A/D、不爬 | — | ？ | — |
| 自动爬 / CameraAction 段 | — | ？ | ？ |
| 停爬后继续走 | — | ？ | — |

记录：主 VCam world Y、公式 `floorY = bounds.min.y + orthoSize`、MainCamera.local/world Y（若被 DOMove）。

### B. 对拍 0914 / 0922 契约

1. `ChangeCamera(true)` / `AlignCameraAfterTreeBridgeChange` / 读档 `CheckPlayerHasInSpcArea` 是否仍 Snap？  
2. `DeadZoneHeight` 洞内是否仍为 1（Force 后跟拍会不会抬 Y）？  
3. `CameraAction` 是否改 MainCamera；Brain 是否每帧覆盖；停晃后 0922 去掉 →(0,0) 后是否残留偏移？  
4. Confiner 合法 Y 带 `[floorY, ceilY]` 是否过宽，走路时被顶到上沿？  
5. 是否有第二处改 VCam Y（剧情、Impulse、战斗立绘相机）？

### C. 方案对比

| 方案 | 做法 | 何时选 |
|------|------|--------|
| **A** | 洞内定期/每 Fixed 或「无爬行晃动时」保持 Snap 贴底（或钳 VCam.y ≤ floorY+ε） | 上漂因合法带内漂移且 DeadZone 不跟回 |
| **B** | `CameraAction` 改抖 VCam 偏移或短时 Noise；停晃后强制 Snap；禁止抖 MainCamera | 复现与爬行晃动强相关 |
| **C** | 洞内临时减小 DeadZoneHeight 跟人 Y，出洞还原 | 需严格出洞还原；易误伤 |
| **D** | 挪/压矮 CameraTreeInArea | **否决** |

必答：

1. 0914 贴底是否仍生效？若生效，为何走着还会高？  
2. 0922 去掉 MainCamera→0 是否加重本案？  
3. 最小改动文件路径；如何验收「走全程不往上飘」且「出洞正常」？

### D. 验收清单

| # | 步骤 | 期望 |
|---|------|------|
| 1 | 左/右口进洞揭幕 | 贴底，上方不多余空 |
| 2 | 洞内左右走全程（不爬） | **镜头 Y 不上漂** |
| 3 | 爬行晃动段 + 停爬再走 | 可轻微抖；停后贴底；再走不上漂 |
| 4 | 出洞 | Size/Confiner/观感恢复洞外 |
| 5 | 读档落在洞内 | 同贴底，走也不上漂 |
| 6 | `CameraTreeInArea` 几何 | **未改** |

### E. 报告结构

① 结论一句话（上漂主因 + 与 0914 关系）  
② 复现矩阵  
③ 调用链 / Y 谁在改  
④ 方案对比与推荐  
⑤ 要改文件路径级  
⑥ 验收表  
⑦ 风险与回滚（0922 闪滑是否回潮）  

写入：`Assets/Doc/执行文档/0922/ForestEast_树洞行走镜头上漂_架构溯源报告.md`
```

---

## 施工员（侦探闭环后再复制；未闭环勿用）

```
@Assets/Doc/执行文档/0922/ForestEast_树洞行走镜头上漂_架构溯源报告.md
@Assets/Doc/施工说明/0914/ForestEast_倒树进洞相机贴边界底边_施工说明.md
@Assets/Doc/施工说明/0922/换场与读档_相机闪滑_施工说明.md

你是【施工员】。按报告最小修树洞「走着镜头往上移」。

约束：
- 禁止改 CameraTreeInArea / CameraArea 多边形与 Transform
- 禁止为治上漂把全局 ScreenY 改死且出洞不还原
- 保留爬行晃动产品感（可改实现，勿无声无故删）
- 勿回退 0922 换场定格到拖垮闪滑修复；若动 StopCameraAction，写清与闪滑的取舍
- 写入：`Assets/Doc/施工说明/0922/ForestEast_树洞行走镜头上漂_施工说明.md`
```

---

## 使用顺序

1. 复制「侦探」→ 先填复现矩阵（只走 vs 爬行）  
2. 确认推荐方案（A 贴底保持 / B 改晃动目标）  
3. 复制「施工员」→ 按验收表打洞内全程
