# ForestEast · 倒树外壳「外」透明改为切镜头后 — 架构溯源报告

**文档版本**：v1.0（2026-09-14）  
**文档性质**：【架构侦探】只读核实；**未改**代码 / 场景 / Git  
**Unity**：2020.3.48f1  
**场景**：`ForestEastScene` 倒树 / `TreeBridgeLogic.OuterSprite`（子物体「外」）  
**现象 / 期望**：靠近就淡「外」→ 改为 **切完洞内镜头之后** 再淡；出洞切回后还原  
**产品期望（钉死）**：靠近不透；`ChangeCamera` 写完后再 Fade(0)/(1)；读档洞内一致透明；不叠两次  
**不是**：改爬行 / Pass Fall / 遮罩 / 嘎吱 / 相机 Size·Confiner；绑到 `CameraAction` 抖镜  
**提示词**：`Assets/Doc/提示词/0914/ForestEast_倒树外壳透明改切镜头后_架构侦探提示词.md`  
**OPEN**：本节 Q1～Q4

---

## 沟通摘要

### ① 结论一句话

现网靠 **倒树 Interactive Enter/Exit → `OuterSpriteFade(0/1)`** 一走近就透；切镜是另一条 **`ChangeCamera`**。推荐 **方案 A**：去掉 Interactive 淡出订阅；把 `OuterSpriteFade` 公开并加 null 防护；在 **`ForestEastTreeBridgeStoryMgr.ChangeCamera` 末尾** 按 `isEnterTree` 调 Fade(0)/Fade(1)。进/出交互与读档洞内都走 `ChangeCamera`，写入点唯一。

### ② 原因（通俗）

现在「走到树旁边」和「镜头切进洞里」是两回事，透明却绑在「走到旁边」。改成绑在「镜头切完」上，走近就不透了，进洞黑幕里切完镜再淡，出来再淡回来。

### ③ 用户检查清单（验收矩阵）

| 操作 | 期望 |
|------|------|
| 洞外走近（不进洞） | 「外」**不**透 |
| 洞外走开 | 仍不透明 |
| 左/右进洞（黑幕→切镜） | **切镜完成后**开始淡到透明 |
| 洞内爬 | 保持透明，不闪回 |
| 左/右出洞 | 切回洞外后淡回不透明 |
| 读档已在树洞 | 「外」透明 |
| Pass Fall 后 | 不因本案额外坏（树销毁则无外壳） |

### ④ 程序补充

见下文。施工说明待拍板：`施工说明/0914/ForestEast_倒树外壳透明改切镜头后_施工说明.md`。

---

## 1. 「切镜头」定义（本案）

| 算 | 不算 |
|----|------|
| `ChangeCamera(true/false)`：换 Confiner / OrthoSize / 贴底 | 靠近 Interactive |
| | 洞内 `CameraAction` 抖 |
| | Pass Fall 倒下动画 |

用户口头与提示词一致；**不**绑 `CameraAction`。

---

## 2. 现网两条链

### 2.1 靠近淡出（要拆）

```
倒树根 BoxCollider2D Trigger（Offset≈(-29.83,4.1) Size≈(83.9×8.21)）
  → InteractiveComponent Enter/Exit
  → TreeBridgeLogic.OnInit 订阅：
       onEnter → OuterSpriteFade(0)
       onExit  → OuterSpriteFade(1)
  → OuterSprite.DOFade(…, OuterSpriteFadeTime=1)
```

`OuterSprite` → 场景「外」SR `7198735743813921975`；`OuterSpriteFade` 现为 **private**，**无** null 防护。

### 2.2 进/出洞切镜（要挂淡出）

```
ForestEastTreeEnterTrigger.ChangePlayerPos
  黑幕 FadeShow.onShowEnd：
    挪人 → ChangeCamera(isEnterTree) → Climb / OnEnterOrOutTreeBridge
    → CloseFormFade（关黑幕）
```

`ChangeCamera` 现网只做：换 BoundingArea + Size/贴底或 Reset；**不**调外壳。

读档：`BaseGameSceneManager.CheckPlayerHasInSpcArea` → `isInTreeBridge` → `ChangeCamera(true)`（**无**黑幕）。

全库 `ChangeCamera(` 调用点仅上述 **两处**（EnterTrigger + 读档）。

---

## 3. 侦探必答

### Q1 Interactive 是否仅用于外壳淡出？

**是（对 TreeBridgeLogic 而言）**：`OnInit` 里对该 Interactive **只**订了 Fade 两行。去掉订阅后，Trigger/组件仍在，但 **无其它 TreeBridge 逻辑依赖 Enter/Exit**。不改触发范围、不删组件。

### Q2 ChangeCamera 是否覆盖进/出/读档？旁路？

| 路径 | 是否走 ChangeCamera |
|------|---------------------|
| 交互进洞 | ✅ `isEnterTree=true` |
| 交互出洞 | ✅ `false` |
| 读档已在洞内 | ✅ `CheckPlayerHasInSpcArea` → `true` |
| 旁路传送不经 EnterTrigger | 现网玩法不提供；若以后有，须另调 ChangeCamera 或 Fade |

### Q3 null 防护？

需要。现网 `OuterSprite.DOKill()` 无判空；对照 Pass Fall Attached 教训，公开 API 应 **`OuterSprite == null` 则 Warn 并 return**。

### Q4 Fall 存档态？

`CheckFall()==true` 时 **跳过** OnInit 订阅并 **Destroy** 整棵倒树（含「外」）。改完后：已倒下档无外壳可淡，属正确；未倒下走 ChangeCamera 淡出即可。Fall 动画过程不依赖 Interactive 淡出。

### Q5 黑幕与淡出时序

`ChangeCamera` 在 **黑幕仍盖着** 时调用（`CloseFormFade` 之前）。方案 A：淡出在黑幕后开始，玩家常在揭幕时已接近透明——**可接受**。若产品要「看见洞内再透」→ **A+B**（把 Fade 挪到 `CloseFormFade` 回调；读档无黑幕须在 ChangeCamera 仍调一次）。

---

## 4. 方案裁定

| # | 方向 | 裁定 |
|---|------|------|
| **A** | 去 Interactive Fade；ChangeCamera 末尾 Fade | **推荐默认** |
| A+B | A + 揭幕后淡（读档仍 ChangeCamera） | 产品嫌「黑幕里就淡」再用 |
| C | 靠近 + ChangeCamera 双淡 | **否决**（违背靠近不透） |
| D | 只改 EnterTrigger | **慎用**（读档易漏） |

**推荐：A**  
**写入点**：`ForestEastTreeBridgeStoryMgr.ChangeCamera` 末尾；`TreeBridgeLogic` 去订阅 + public Fade。

---

## 5. 给施工员的最小改清单（6 条）

1. `TreeBridgeLogic.OnInit`：**删除/注释** Interactive `onEnter`/`onExit` 对 `OuterSpriteFade` 的订阅。  
2. `OuterSpriteFade` 改为 **public**（或同名公开包装）；`OuterSprite == null` 安全跳过 + Warn。  
3. `ForestEastTreeBridgeStoryMgr.ChangeCamera`：切 Confiner/Size/贴底 **全部做完后**，`isEnterTree` → `storyLogic.OuterSpriteFade(0)`，否则 `Fade(1)`（`storyLogic` 已有 null 早退）。  
4. 注释写清：靠近不透、切镜后再透、出洞/读档同源。  
5. **禁止**：改 Interactive 范围、改 `OuterSpriteFadeTime`、改相机公式、改 Pass Fall、绑 `CameraAction`。  
6. 验收按文首矩阵勾选；施工说明落盘。

**文件列表（2）**：

- `Assets/Scripts/Game/GameRuntime/Entities/SceneEntities/ForestEastScene/TreeBridgeLogic.cs`  
- `Assets/Scripts/Game/GameMgr/Manager/Story/ForestEastTreeBridgeStoryMgr.cs`

---

## 6. OPEN

| ID | 问题 | 默认 | 状态 |
|----|------|------|------|
| Q1 | A 还是 A+B？ | **A**（黑幕内开始淡可接受） | 待用户/施工确认 |
| Q2 | 旁路进洞不走 ChangeCamera？ | 现网无；以后须同源调用 | 降级 |
| Q3 | 「外」场景 Active=0？ | 另核（可能 Fall/误关）；本案只改 Fade 时机 | 待 Play |
| Q4 | Fall 后外壳？ | Destroy 整树，无外壳，正确 | ✅ |

---

## 7. 侦探声明

未改代码 / 场景 / Git。证据已够拍板方案 A。
