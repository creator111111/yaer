# NewGameStory — 主角大立绘不出现 — 架构溯源报告

**文档版本**：v1.0（2026-09-11）  
**文档性质**：【架构侦探】只读溯源；**未改**代码 / Prefab / 动画 / 场景 / Git  
**Unity**：2020.3.48f1  
**现象**：新游戏漫画后进入 `NewGameStory`，**字有、主角大立绘全程不出现**；可推进  
**期望**：可见 `YaerPainting`（连衣裙+皇冠），表情可切；点到树尾；进村开场无回归  
**范围钉死**：序章 `NewGameStory`（**不是** `Village_KenMuNiStart` / `GoOutStoryYaerPainting` / Mask 小头像）  
**提示词**：`Assets/Doc/提示词/0911/NewGameStory_主角大立绘不出现_架构侦探提示词.md`  
**并行**：与 0911 StartScene Build Settings 案、村长家/巨树案解耦  

---

## 沟通摘要

### ① 结论一句话

**根因 = H1：旁路 Prepare 已把 `YaerPainting` alpha 置 0 并关掉故事 Animator，但 Prefab 前奏仍是 0807-B「并行三件套」（BlackMask + `YaerShow` Trigger + UIAlpha），没有 `CanvasGroupAlpha` 淡回 → 立绘永远透明。文档标 0807-D「已施工」与磁盘 Prefab 漂移（脚本侧 D / Prefab 侧 B）。**

### ② 原因（通俗）

开场脚本先把雅儿大立绘「藏成看不见」，并关掉负责播入场动画的开关。  
对话树却还在用旧办法：发 `YaerShow` 触发器指望动画把人播出来。  
开关已经关了，触发器打空；树里又没有「把透明度淡回 1」的节点 → **字幕条能淡出来，人出不来**。

### ③ 用户需要做什么（1 分钟自查）

| # | 操作 | 预期 |
|---|------|------|
| 1 | 完整新游戏：漫画播完进龙宫对白 | 字有、**大立绘没有**（本案现象） |
| 2 | Console 过滤 `[NewGameStory][Prepare]` | 应见 `hide YaerPainting`、`disable Animator` |
| 3 | 对白首句时 Hierarchy 选场景 `YaerPainting` | `CanvasGroup.alpha ≈ 0`；故事根 `Animator.enabled = false` |
| 4 | Console 过滤 `[CanvasGroupAlpha]` | **不应**出现（现 Prefab 无该节点） |
| 5 | （对照）DialogDebug 只拖 `NewGameStory` | 可能**反而有立绘**（无 Prepare）——**不能**当正式验收通过 |

### ④ 程序补充

见下文 §1～§8。

---

## 1. 结论（程序向）

| 项 | 裁定 |
|----|------|
| **主因** | **H1 成立**：Prepare 藏立绘 + 关 Animator，与 Prefab 旧 `YaerShow` 前奏 **不成对** |
| **成对关系** | 旁路期望淡入走 **`CanvasGroupAlpha` → `TryRestoreStoryAnimatorAfterPortraitFade`（落到 YaerShow 末帧）**；Prefab **零**该节点 |
| **文档 vs 磁盘** | **H7 成立**：OPEN 写 0807-D 已成对；磁盘 Prefab = **0807-B 并行回退态**；`NewGameSceneManager` 注释/实现仍为 D |
| **推荐修复** | **方案 A**：Prefab 改串行对齐村线 / OPEN-D；**禁止** C 强 alpha=1、D 改框架；紧急 B 仅 unblock 且须 OPEN |
| **排除** | 素材丢、Mask、村线 GoOut、漫画分页——非本案主因 |

---

## 2. 证据链

### 2.1 触发与旁路（脚本侧已是 D）

路径：`Assets/Scripts/Game/GameRuntime/GameSceneManager/Scene/NewGame/NewGameSceneManager.cs`

```
漫画全黑 onFinish
  → OnCartoonFinishedFullyBlack
  → System BlackPanel RawShow
  → 关漫画 Form
  → VillageStartLayerRevealGate.ResetForDeferredCover
  → TriggerStory("NewGameStory")
  → onStoryTriggered → hold 0.15s
  → PrepareNewGameLayeredReveal   ← 本案关键
  → HideFade（拍1）→ SignalBgFullyVisible
```

**`PrepareNewGameLayeredReveal` 对立绘 / Animator 实际动作：**

| 步骤 | 行为 | 日志关键词 |
|------|------|------------|
| 字幕条 | `dialogueUICanvasGroup.alpha = 0` | （无专用 log） |
| BG | 兜底 `BG.SetActive(true)` | — |
| **YaerPainting** | `DialogueSceneContainer` 下找名 → `CanvasGroup.alpha = 0` | `[NewGameStory][Prepare] hide YaerPainting` |
| **故事 Animator** | 子树找带参数 `YaerShow` 或 `KingMove` 的 Animator → **`enabled = false`** | `[NewGameStory][Prepare] disable Animator on …` |

注释写明期望：淡入由 **CanvasGroupAlpha** 完成，淡完再开 Animator、落到 **YaerShow 末帧** 供 **KingMove**。

恢复入口：`CanvasGroupAlphaActionTask.TryRestoreStoryAnimatorAfterPortraitFade`  
（`Assets/Scripts/.../Common/CanvasGroupAlphaActionTask.cs` ≈89–127）

### 2.2 Prefab 前奏真源（磁盘 = 0807-B）

路径：`Assets/GameRes/Prefabs/Dialogue/NewGameStory.prefab`  
图序列化（`_boundGraphSerialization`）首节点 `$id:"0"`：

| 项 | 现网值 |
|----|--------|
| 结构 | **单 ActionList**，`executionMode: 1` = **Parallel** |
| ① | `NormalDialogueBlackMaskTaskAction` StartAlpha=1，Duration=**1.0** |
| ② | `MecanimSetTrigger` parameter=**`YaerShow`** |
| ③ | `NormalDialogueUIAlphaAnimationTaskAction` EndAlpha=1，Delay=**0.5**，Duration=**0.7** |
| Wait | **无** `WaitVillageStartBgRevealActionTask` |
| 立绘淡入 | **无** `CanvasGroupAlphaActionTask`（全文 count=0） |
| BB | `_serializedBlackboard` **仅** `Volume`；**无** `YaerPainting` CanvasGroup 变量 |
| 后续 | 另有 ActionNode：`MecanimSetTrigger("KingMove")`（仍依赖能从 YaerShow 转出） |

**磁盘 Hierarchy 上的 `YaerPainting`：** `m_IsActive: 1`，CanvasGroup **`m_Alpha: 1`**（Prefab 默认可见）——运行时被 Prepare **盖成 0**，故「Prefab 里看着有」≠ Play 可见。

### 2.3 对照村线成对样板

路径：`Assets/GameRes/Prefabs/Dialogue/Village_KenMuNiStart.prefab`

| 顺序 | KenMuNi（成对） | NewGameStory（现网） |
|------|-----------------|----------------------|
| 闸门空拍 | `WaitVillageStartBgReveal` Hold=0.5 | **无** |
| 立绘 | `CanvasGroupAlpha` ×2（GoOut雅儿 / 古莎）0→1 Dur0.5，`EndActionOnAnimationEnd=true` | **无**；改 `YaerShow` Trigger |
| 对话框 | UIAlpha Delay0.5+Dur0.5，阻塞 + PrepareMask | 与 BlackMask/`YaerShow` **并行**，Dur0.7 |
| 旁路 Prepare | 村 SceneManager 藏立绘 alpha=0 | NewGame **同样藏** + **额外关 Animator** |

→ 村线「藏 → Prefab 淡回」成对；序章「藏 → Prefab 仍靠已禁用的 Animator」断裂。

### 2.4 为何 `YaerShow` 救不了（机制）

1. Prepare 已 `Animator.enabled = false` → `MecanimSetTrigger("YaerShow")` **无效**。  
2. 即便 Animator 开着：`YaerShow.anim` 对 `Fg/YaerPainting` 的 `m_Alpha` 为 **0→1（≈0.83s）**，旧路径本可显人；**现被关动画堵死**。  
3. Prefab **没有**任何路径调用 `CanvasGroupAlpha` → 不会走 `TryRestoreStoryAnimator…` → Animator 一直关、alpha 一直 0。  
4. 对话框走 `NormalDialogueUIAlpha`（字幕条），与大立绘 CanvasGroup **解耦** → 符合「字有、人没有」。

### 2.5 动画抢写（H3）

| 资产 | 现网 | 对本案 |
|------|------|--------|
| `Start.anim` | **无** `Fg/YaerPainting` 的 `m_Alpha` 曲线（仅 King 等） | **不**再是「Start 每帧写 0」主因 |
| 默认态 `New State` | `m_WriteDefaultValues: 0`（注释：防分层空拍失效） | 已按 D 处理 |
| `YaerShow` / `KingMove` 态 | Write Defaults 仍为 **1** | 正常播 Clip 时可接受 |
| 风险（修 A 后） | 若只 `enabled=true` 却不落到 YaerShow 末帧，仍可能被默认态/WD 干扰 | A 必须 `EndActionOnAnimationEnd=true` 走现成 Restore |

**H1 成立时**：即使将来 Fade 修好，仍须保留 Restore→YaerShow 末帧（供 KingMove），并避免半施工再开 Animator 停在 Start。

### 2.6 文档 vs 磁盘（H7）

| 来源 | 声称 | 磁盘核实 |
|------|------|----------|
| `OPEN_QUESTIONS.md` 0807-D | Prefab 串行 Wait→Fade→UIAlpha；已去 BlackMask/YaerShow | Prefab **仍并行三件套** |
| `NewGameSceneManager` 类注释 | 方案 D 成对 | **旁路代码在**；Prefab 不配套 |
| 0807 间隔报告 / 卡死报告 | 曾 B 回退保可玩，再 D 成对 | **脚本 D + Prefab B** = 半施工/漂移同构 |

已在 `OPEN_QUESTIONS.md` 标注漂移，并新增 0911 节（终局 A / 紧急 B 待确认）。

---

## 3. 假说表（H1～H7）

| ID | 假说 | 裁定 | 证据 |
|----|------|------|------|
| **H1** | Prepare 藏立绘+关 Animator，Prefab 无 CanvasGroup 淡回 | **✅ 主因** | Prepare 代码 + Prefab 首 ActionList 无 Fade / 有 YaerShow |
| **H2** | 已有 CanvasGroupAlpha 但 BB 绑错 | **排除（当前）** | Prefab 无该 Task；BB 无立绘变量。**施工 A 时须防再犯** |
| **H3** | Start.anim / WD 每帧写 alpha=0 | **非主因**；修后须防 | Start 已无 Yaer alpha；默认态 WD=OFF；YaerShow Clip 仍写 alpha |
| **H4** | Prepare 找错物体 / 未找到 Container | **待 Play 日志确认**；架构上路径正确 | 有对白 ⇒ 壳大体就绪；以 `hide YaerPainting` 为准 |
| **H5** | Active=false / Faces 键错整人消失 | **非主因** | Prefab Active=1、alpha 默认 1；体感更符「透明」 |
| **H6** | DialogDebug 正常、完整链路才坏 | **高度同构（待复验）** | DialogDebug 不跑 NewGame Prepare → Animator 仍开，YaerShow 可能显人 |
| **H7** | 文档「D 已施工」与 Prefab 漂移 | **✅ 成立** | OPEN vs 磁盘对照 |

---

## 4. 排除项（一句话）

| 项 | 是否本案主因 |
|----|--------------|
| 立绘 PNG / 图集丢失 | **否**（节点在、默认 alpha=1） |
| Mask 小头像 / Presenter | **否**（大立绘场景物体） |
| 村线 `GoOutStoryYaerPainting` | **否**（序章用 `YaerPainting`） |
| 漫画分页本身 | **否**（漫画后对白已开） |
| 点击推进逻辑坏 | **否**（用户可推进） |

---

## 5. 建议施工步骤（成对 · 只建议不施工）

### 方案 A（首选 · 对齐 D / 村线）

1. 改 `NewGameStory.prefab` 前奏为**串行**（勿并行叠三件套）：  
   - `WaitVillageStartBgReveal`（Hold **0.5**，复用村 Gate；DialogDebug 闸默认 Ready 不永卡）  
   - `CanvasGroupAlpha`：`YaerPainting` **0→1**，Duration **0.5**，**`EndActionOnAnimationEnd=true`**（必走 Restore→YaerShow 末帧）  
   - `NormalDialogueUIAlpha`：Delay **0.5** + Dur **0.5**；按需 PrepareMask（对齐 KenMuNi）  
2. **去掉**前奏主路径：`NormalDialogueBlackMask` + `MecanimSetTrigger(YaerShow)`。  
3. BB 增加并绑定场景 **`YaerPainting` CanvasGroup**（DialogueScene 下大立绘；**勿**绑 Mask 下同名）。  
4. **保留**现网 `PrepareNewGameLayeredReveal`（藏立绘 + 关 Animator）；**禁止**只删 Prepare 当终局。  
5. **保留**后续 `KingMove` Trigger；靠 Fade 后落到 YaerShow 末帧衔接。  
6. **禁止**改村线 Prefab、换 PNG、改 GameFramework、只强制 `alpha=1`。  
7. 施工说明：`Assets/Doc/施工说明/0911/NewGameStory_主角大立绘恢复_施工说明.md`。

### 方案 B（紧急 unblock · 非终局）

- 临时去掉 Prepare 对 `YaerPainting` 藏 alpha / 关 Animator，恢复纯 YaerShow 可玩。  
- **必须**写 OPEN（已预留 Q2）；标注与分层标准冲突，随后仍须回到 A。

### 验收（完整新游戏链路）

1. 漫画后可见 `YaerPainting` 淡入 → 可点多句。  
2. King 出场 / `KingMove` 不崩。  
3. Console：Prepare hide/disable → 随后应有 `[CanvasGroupAlpha] … YaerShow`。  
4. DialogDebug 拖同 Prefab 不永久卡。  
5. `Village_KenMuNiStart` 无回归。

---

## 6. 剩余风险

| 风险 | 说明 |
|------|------|
| King 立绘 | Prefab 内 King CanvasGroup 默认可为 0；靠 KingMove Clip；衔接依赖 YaerShow 末帧 |
| 半施工 | 只加 Fade 不设 `EndActionOnAnimationEnd` → Animator 不恢复 / KingMove 断 |
| BB 绑错 | 绑到 Mask 下同名 → 小头像/大立绘错乱（村线踩过坑） |
| DialogDebug 误判 | 无 Prepare 时旧 YaerShow 也可能「看起来好」 |
| 再漂移 | CSV 重导 / 误提交旧 Prefab 可能再次冲掉串行前奏 |

---

## 7. OPEN_QUESTIONS

已更新：

- 0807-D：标注 **Prefab 漂移**；旁路仍算已施工。  
- 新节 **0911**：Q1 终局 A；Q2 紧急 B 是否允许（待确认）；Q3 DialogDebug 不算正式通过（已决议）。

---

## 8. 可复制给施工员（5～10 条）

> 根因已拍板：**H1 + H7（脚本 D / Prefab B 不成对）**。成对修 Prefab，勿单边砍旁路当终局。

1. **目标**：完整新游戏漫画后，`NewGameStory` 可见主角大立绘 `YaerPainting` 并淡入；字幕仍可推进；KingMove 不崩。  
2. **优先方案 A**：改 `Assets/GameRes/Prefabs/Dialogue/NewGameStory.prefab` 前奏为串行 Wait(0.5) → `CanvasGroupAlpha(YaerPainting 0→1, Dur0.5, EndActionOnAnimationEnd=true)` → UIAlpha(Delay0.5+Dur0.5)。  
3. **去掉**前奏主路径 `NormalDialogueBlackMask` + `MecanimSetTrigger(YaerShow)`；**保留**后续 `KingMove`。  
4. Blackboard 绑定场景 DialogueScene 下 **`YaerPainting` CanvasGroup**；禁止绑 Mask 子树同名。  
5. **禁止**删除/掏空 `PrepareNewGameLayeredReveal` 当终局；**禁止**强制 `alpha=1` / 强开 Animator 当终局；**禁止**改村线 Prefab / GameFramework / 换 PNG。  
6. 若产品要紧急可玩：方案 B 仅回退 NewGame Prepare 藏立绘/关 Animator，并在 OPEN Q2 标注非终局。  
7. 施工说明落盘：`Assets/Doc/施工说明/0911/NewGameStory_主角大立绘恢复_施工说明.md`。  
8. 验收必须走 **完整新游戏**；DialogDebug 仅作辅证。  
9. 验收日志：`[NewGameStory][Prepare] hide/disable` 后应出现 `[CanvasGroupAlpha] … YaerShow`；Hierarchy 首句时 alpha≈1、Animator 已恢复。  
10. 回归：`Village_KenMuNiStart` 分层；点多句至树尾；King 出场。

---

## 附录：关键锚点

| 主题 | 路径 |
|------|------|
| 旁路 Prepare / 拍1 | `.../Scene/NewGame/NewGameSceneManager.cs` |
| 淡入 + 恢复 Animator | `.../ActionTask/Common/CanvasGroupAlphaActionTask.cs` |
| 对话 Prefab | `Assets/GameRes/Prefabs/Dialogue/NewGameStory.prefab` |
| 村线样板 | `Assets/GameRes/Prefabs/Dialogue/Village_KenMuNiStart.prefab` |
| Controller / Clips | `Assets/Animation/UI/NewGameScene/` |
| 立绘脚本 | `.../Painting/YaerPainting.cs`（非 GoOut） |
| 技术标准 | `Assets/Doc/技术文档/演出相关/Village_KenMuNiStart_开场分层显现_技术说明.md` |
| 历史 | `执行文档/8月/0807/NewGameStory_*` |
