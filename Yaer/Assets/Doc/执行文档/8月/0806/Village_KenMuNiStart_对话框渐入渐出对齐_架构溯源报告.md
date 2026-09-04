# Village_KenMuNiStart 对话框渐入渐出对齐 — 架构溯源报告

**文档版本**：v1.0（2026-08-06）  
**文档性质**：【架构侦探】只读溯源（**禁止改代码 / Prefab / 场景 / 图集 / CSV / 台本**）  
**范围**：`Village_KenMuNiStart` 开场分层已通；本期只对齐 **对话框（字幕条）出现的渐入渐出**观感，与 BG/立绘一致。不改台本、不推翻分层顺序。  
**依据**：
- `Assets/Doc/00_MASTER_PROMPT.md`【架构侦探】
- `Assets/Doc/提示词/0806/Village_KenMuNiStart_对话框渐入渐出对齐_架构侦探提示词.md`
- 分层 / Mask 白名单：`0806/Village_KenMuNiStart_开场分层显现时序_…`、`0806/…分层后小头像不显示_…`
- Prefab / 任务：`Village_KenMuNiStart.prefab`、`NormalDialogueUIAlphaAnimationTaskAction`、`CanvasGroupAlphaActionTask`、`DialogueTMPUGUI`

**Unity**：2020.3.48f1  

---

## ① 结论一句话

**Prefab 上对话框「已有」Delay1+Fade1 节点，但淡入发生在 `subtitlesGroup` 仍 `SetActive(false)` 期间——玩家看不见；首句 `OnSubtitlesRequest` 才把框 Active 打开，此时 alpha 已是 1 → 体感硬切蹦出。推荐最小修：淡入前强制激活字幕条（alpha 仍为 0），并显式 `StartAlpha=0`、Duration/缓动对齐立绘 1.0；渐出壳内已有 0.7s，是否与开场 1.0/BG 成对对齐须拍板。**

---

## ② 原因（生活类比）

### 生活类比

提词板（对话框）的灯光师**在幕后**把灯从暗拧到亮（Fade 真的跑了），但提词板还藏在柜子里（`SetActive(false)`）。轮到念第一句时才把板子从柜子拽出来——板子灯已经全亮，观众只看到**啪一下出现**，以为从没渐入。背景靠拉开大幕露出来，演员（立绘）一直在台上淡入，所以只有提词板「不对齐」。

### 文档 vs 体感

| 来源 | 说法 | 核实 |
|------|------|------|
| 0806 分层施工 | 框 `Delay1+Fade1` | **节点在**：`NormalDialogueUIAlpha` Duration=1 Delay=1 EndActon=true |
| 玩家要「加渐入」 | 框突然蹦 | **淡入播了但不可见**；不是「没挂节点」 |
| Prepare 白名单 | 淡出黑幕前 `dialogueUICanvasGroup=0` | **正确**；与 Fade 起点一致，不是抹掉 Fade 的主因 |
| 结尾「渐出」 | 用户口头含渐出 | 壳内已有框/立绘 **0.7s** 淡出；**无 BG 淡出**；开场框 Duration=1 ≠ 结尾 0.7 |

---

## ③ 用户需要做什么（拍板 + 验收）

### 拍板

1. **「渐出」范围**：仅修开场框**渐入**，还是开场+对白结束**成对渐出**也对齐到 Duration=1？  
   - 默认建议：**先修开场可见渐入**；结尾若要对齐，把 `DialogueEndSubtitlesCanvasGroupFade` / `Painting.Fade` 的 0.7 收到与开场一致（或仅本 Prefab 旁路）——动通用壳须评估影响面。  
2. **框对齐谁**：对齐立绘 CanvasGroup **Duration=1**（推荐），还是对齐黑幕 `hideTime=1`（数值碰巧同为 1，机制不同）？  
3. **BG 是否补 CanvasGroup 0→1**：才算「三层同一套」？  
   - 默认建议：**本期可不补**（拍1 继续靠黑幕露出已满 BG）；若强要求视觉同款淡入再开方案 C。

### 验收清单

1. 进村开场：框**明显淡入**（能看出透明度变化），时长手感与立绘≈1s，无硬切蹦出。  
2. 分层顺序不变（BG→框→立绘）；零漏缝；Mask 小头像仍在。  
3. 若拍板含渐出：结束时框/人不硬切；不叠双重黑幕卡死。  
4. DialogDebug 同 Prefab 观感一致。

---

## ④ 给程序看的补充

### 4.1 三层对照表（出现）

| 产品层 | 现网执行体 | Duration / Delay | 玩家开场可见渐变？ |
|--------|------------|------------------|---------------------|
| **BG** | System `BlackMask.HideFade`（`hideTime` 默认 **1**）；Prefab `BG` **无** CanvasGroup | ≈1s 黑幕淡出 | **是**（露已满不透明 BG） |
| **对话框** | Prefab Node1 `NormalDialogueUIAlphaAnimationTaskAction` → `dialogueUICanvasGroup`（=`subtitlesCanvasGroup` / Bottom） | Delay=**1**，Duration=**1**，EndAlpha=1，`StartAlpha={}`≈0，`EndActonOnAnimationEnd=true` | **否（体感）**：Fade 时 `subtitlesGroup` 多为 **Inactive** |
| **立绘** | Prefab Node2 并行 `CanvasGroupAlphaActionTask` × GoOut/Gusha | Duration=**1**，无 Delay，`EndActionOnAnimationEnd=true`，`StartAlpha={}`≈0 | **是**（场景实例一直 Active，仅 alpha 变） |

### 4.2 开场时序图（alpha / Active）

```
Prepare（进村）：dialogueUICanvasGroup=0；场景立绘 CG=0；BG Active
  → CloseFormFade（拍1，黑幕 hide≈1s）→ 只见 BG

Prefab Node1 框 UIAlpha：
  · 写入 StartAlpha≈0，Delay 1s，再 DOFade→1（1s）
  · ★ 此时 DialogueTMPUGUI 里 subtitlesGroup 仍常为 SetActive(false)
  · → 透明度在变，屏幕上仍只有 BG

Prefab Node2 立绘并行 Fade 0→1（1s）→ 玩家看见人淡入

Node3 首句 Statement
  → OnSubtitlesRequest → subtitlesGroup.SetActive(true)
  → alpha 已是 1 → ★ 框硬切出现（用户抱怨点）

正常对白…
  → OnDialogueFinished
       → OnDialoguePreEnd → 各 Painting.Fade(0, 0.7)
       → subtitles DOFade(0, 0.7) → CloseForm
       → BG 随关壳/无独立渐出
```

### 4.3 根因归类（可多选）

| 归类 | 成立？ | 说明 |
|------|--------|------|
| 参数未挂 Fade 节点 | **否** | Prefab 已有 Delay1+Fade1 |
| StartAlpha 空导致 1→1 | **低** | `{}` 对 float 默认 0；Prepare 也置 0；主因不是这个，但建议施工时**显式写 0** |
| **绑对 CG 但 UI Inactive，淡入不可见** | **是（主因）** | `Hide()` / 句间关 Active；首句才打开 |
| 与立绘时长不一致 | **部分** | 开场 Duration 同为 1；框多 Delay1（拍间隔）；结尾框/人 0.7 ≠ 开场 1 |
| 缺渐出 | **部分** | 有 0.7s 框+立绘淡出；无 BG；未与开场 1.0 成对 |
| Prepare 事后拉满抹掉 Fade | **否** | 白名单只在亮屏前置 0，不在 Fade 中途拉 1 |

**锚点**：

```139:144:Assets/Scripts/Game/GameRuntime/NodeCanvas/NodeCanvasExtend/DialogueTMPUGUI.cs
        void Hide() {
            subtitlesGroup.gameObject.SetActive(false);
            // ...
        }
```

```213:214:Assets/Scripts/Game/GameRuntime/NodeCanvas/NodeCanvasExtend/DialogueTMPUGUI.cs
            subtitlesGroup.gameObject.SetActive(true);  // 首句才打开 → 此时 Fade 已结束
```

```27:56:Assets/Scripts/Game/GameRuntime/NodeCanvas/NodeCanvasNode/ActionTask/NormalDialoguePanel/NormalDialogueUIAlphaAnimationTaskAction.cs
            canvasGroup.alpha = StartAlpha.value;
            // Delay + DOFade —— 不保证 gameObject.activeSelf
```

### 4.4 方案比选表

| 方案 | 做法摘要 | 与 BG/立绘一致？ | 改动面 | 风险 | 推荐？ |
|------|----------|------------------|--------|------|--------|
| **A** | 淡入前 `subtitlesGroup.SetActive(true)`（alpha 保持 0）+ Prefab 显式 StartAlpha=0；Duration 保持 1 对齐立绘 | 开场框≈立绘 | 小：改 `NormalDialogueUIAlpha…OnExecute` **或** Prefab 前加「显示字幕条」任务 | 改通用任务影响所有用该 Action 的对话——通常合理（FadeIn 本就该看得见） | **推荐** |
| B | 框改绑与立绘相同的 `CanvasGroupAlphaActionTask`（BB 拖 Bottom） | 任务类统一 | Prefab + 须保证 Active | 仍要解决 Inactive；收益有限 | 次选 |
| C | BG 加 CanvasGroup，三层统一 0→1 Duration=1 | 三层同机制 | Prefab+Prepare 白名单扩 BG CG | 与黑幕叠淡；零漏缝要重验 | 仅当拍板「BG 也要同款淡」 |
| D | 结尾三层渐出对齐 Duration=1；BG 另议 | 成对 | 壳 `DialogueTMPUGUI` / Painting.Fade 默认 0.7 | 动通用结束节奏 | 开场修完后按拍板加 |

**方案 A 落地要点（建议）**

1. `NormalDialogueUIAlphaAnimationTaskAction.OnExecute`：若目标为渐入（EndAlpha > StartAlpha），在 Delay/Fade 前 `canvasGroup.gameObject.SetActive(true)`（可注释说明：避免 Inactive 期间淡入不可见）。  
   - 替代：仅 `Village_KenMuNiStart` 前插专用 Show 任务——改动面更窄，其它对话不受益。  
2. Prefab Node1：Inspector 写死 `StartAlpha=0`（避免空 BB 误解）。  
3. 不改分层 Delay=1（拍2 间隔）；Fade Duration=1 对齐立绘。  
4. **不动** Prepare 白名单；**不**恢复 Snap。

### 4.5 「渐出」现状（若拍板要成对）

| 层 | 现网结束行为 | Duration |
|----|--------------|----------|
| 对话框 | `DialogueEndSubtitlesCanvasGroupFade` DOFade(0) | **0.7** |
| 立绘 | `OnDialoguePreEnd` → `StoryFormPainting.Fade(0)` | **0.7** |
| BG | 无独立淡出；随 `CloseForm` | — |

Prefab 尾节点仅 `FightingPanelVisible(true)`，**无**框/立绘/BG 渐出 Action。

### 4.6 施工员最小改动清单（只建议）

| 优先级 | 文件 | 做什么 |
|--------|------|--------|
| 1 | `NormalDialogueUIAlphaAnimationTaskAction.cs` **或** 本 Prefab 前插 Show | 淡入前 Active=true |
| 2 | `Village_KenMuNiStart.prefab` Node1 | 显式 StartAlpha=0；确认 Duration=1 / EndActon=true |
| 3 | （可选·拍板后）结尾 0.7→1.0 / BG 渐出 | 评估通用壳影响；优先进村专用则另议 |
| 不改 | 台本；分层顺序；Prepare 白名单；名字广扫 | |

**禁止**：Update 轮询 alpha；取消分层；破坏 Mask 白名单；为对齐重写全游戏开关场。

### 4.7 开放问题（已追加 OPEN）

| ID | 问题 | 默认建议 |
|----|------|----------|
| Q1 | 「渐出」是否包含对白结束成对对齐？ | **本期先修开场可见渐入**；结尾另拍 |
| Q2 | 框对齐立绘 Duration=1，还是对齐黑幕？ | **对齐立绘 Duration=1** |
| Q3 | BG 是否补 CanvasGroup Fade 才算三层一致？ | **本期否**；拍1 保持黑幕露 BG |

---

## 施工员下一轮最小化清单（建议 · 待拍板后开）

1. 让框 Fade 在 **Active 可见**状态下从 0→1（通用任务或 Prefab 专用 Show）。  
2. Prefab 显式 StartAlpha=0；Duration 与立绘同为 1。  
3. 按 §③ 验收：框明显淡入、分层/零漏缝/小头像不回归。  
4. 若拍板要结尾成对：再改 0.7 与 BG，勿与开场主修绑死。  
