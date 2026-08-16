# NewGameStory 对话卡死与开场异常 — 架构溯源报告

**文档版本**：v1.1（2026-08-07）  
**文档性质**：【架构侦探】溯源 + 【施工员】方案 B 已落地  
**拍板**：开发者选定 **B — 回退 Prefab 保可玩**（分层对齐 KenMuNi 二期再接）  
**Unity**：2020.3.48f1  

---

## ① 结论一句话

**卡死点**：0806「半施工」——`NewGameStory.prefab` 已改成村式 `Wait`+`CanvasGroup` 分层前奏，但 `NewGameSceneManager` 仍是漫画结束后直接 `TriggerStory`（无 Gate/Prepare/HideFade）；再叠加 `Start.anim` 仍每帧写 `YaerPainting.m_Alpha=0`，去掉 `YaerShow` 后立绘/开场体感崩。  
**已按 B 恢复**：Prefab 前奏回退为并行 `BlackMask(1→0)` + `YaerShow` + `UIAlpha(Delay0.5/Dur0.7)`，与现网「简单 Trigger」成对。

---

## ② 原因（生活类比）

昨天给序章换了「进村那种分步开灯」的提词板，但灯光员（SceneManager）还没接到新 cue 表——戏开了一半，幕布/立绘对不上。  
更糟的是：默认动画一直把大立绘透明度按成 0；以前靠 `YaerShow` 拉回来，分层改造把这根绳子剪了，立绘就「永远不出来」。  
本期先把提词板换回旧的简单开场，保证能点下去；完美分层以后再配齐旁路。

---

## ③ 用户需要做什么（验收）

1. **新游戏**：漫画结束 → 对话壳起来 → 能看见框/字（及 YaerShow 立绘）→ **点击可连续推进多句** → 能走到树尾（进 HomeScene1）。  
2. 开场不永久黑屏、不永久无字、不永久挡输入。  
3. DialogDebug 拖 `NewGameStory` 可点完。  
4. 进村 `Village_KenMuNiStart` 无回归（本期未改村旁路/村 Prefab）。  
5. （可选·二期）再做 A：补 NewGame Gate/Prepare/HideFade + Prefab 标准 0.5 分层。

---

## ④ 给程序看的补充

### 4.1 现网时序（修复前 → 修复后）

```
【修复前 · 半施工断裂】
漫画 CloseFormShowFade（Form 自带黑幕满）
  → onFinish：TriggerStory + BGM（无 Gate/Prepare/HideFade）
  → 关漫画 Form（黑幕随 Form 走）
  → Prefab 串行：Wait(Gate 默认 true → 只 Hold0.5)
       → CanvasGroupAlpha(YaerPainting)（与 Start.anim 抢 alpha）
       → UIAlpha(+PrepareMask) → Statement
  → 体感：开场怪 / 立绘不对 / 易被说成「点不动」

【修复后 · 方案 B】
漫画 Finish 同上（SceneManager 未改）
  → Prefab 并行：BlackMask 1→0 + YaerShow + UIAlpha(0.7, 无 PrepareMask)
  → Statement → 点击 Continue 推进
```

### 4.2 0806 半施工对照表

| 项 | 报告/OPEN 声称 | 磁盘修复前 | 成对？ |
|----|----------------|------------|--------|
| `NewGameSceneManager` Gate/Prepare/HideFade/Signal | ✅ 已施工 | **仅** `TriggerStory` + BGM | ❌ |
| `NewGameStory.prefab` Wait→Fade→UIAlpha | ✅ 已施工 | 串行 Wait+CanvasGroup+UIAlpha(+PrepareMask) | Prefab 单边 |
| Gate 默认 `IsBgFullyVisible=true` | DialogDebug 不卡 | 未 Reset 时 Wait 只 Hold | Wait 本身非永久卡 |
| OPEN「已施工」 | Q1–Q3 ✅ | 与 SceneManager 不一致 | **文档误标 / 旁路未落盘或已回退** |

### 4.3 点击链路表

| 嫌疑 | 结论 |
|------|------|
| 永久卡在 Wait | Gate 默认 true → **否**（最多 Hold；8s 超时也有） |
| `IsAlphaHide` 一直 true | 仅关字幕按钮路径会设；前奏 UIAlpha **不**设此旗 |
| System 黑幕挡点 | 漫画黑幕在 Form 上，关 Form 带走；ShowEnd 也会关 raycast |
| 未到 Statement / 开场崩 | **主因**：分层前奏与旁路不成对 + 无 YaerShow 对抗 `Start.anim` alpha=0 |
| 通用 `DialogueTMPUGUI` 坏 | 进村同套点击类正常 → **不在通用点击类** |

### 4.4 方案比选（已拍板 B）

| 方案 | 内容 | 先可玩？ | 本期 |
|------|------|----------|------|
| A | 补全 NewGame 旁路与 Prefab 成对 | 能，但改动大 | 二期 |
| **B** | Prefab 回退简单前奏，对齐现 Trigger | **最快** | **已做** |
| C | 只修点击/遮罩 | 若根因在前奏则不够 | 否 |

### 4.5 施工改动清单

| 文件 | 改动 |
|------|------|
| `Assets/GameRes/Prefabs/Dialogue/NewGameStory.prefab` | 前奏 ActionList：`executionMode=1` 并行；`NormalDialogueBlackMaskTaskAction` + `MecanimSetTrigger(YaerShow)` + `NormalDialogueUIAlpha`(Dur0.7/Delay0.5，PrepareMask 关)；去掉 `WaitVillageStartBgReveal` / `CanvasGroupAlpha` |
| `NewGameSceneManager.cs` | **不改**（保持简单 Trigger，与 B 成对） |
| 村相关 | **不改** |

> BB 变量 `YaerPainting` 可暂留，二期分层再绑；不影响现网并行前奏。

### 4.6 开放问题（已记 OPEN）

| ID | 问题 | 决议 |
|----|------|------|
| Q1 | 0806「已施工」是否误标？ | **是**：Prefab 改了，SceneManager 旁路现网不存在 |
| Q2 | 本期 A 还是 B？ | **B** 回退 Prefab 保可玩 |
| Q3 | Gate 共用是否 NewGame 专用 Reset/Signal？ | 二期做 A 时再定；本期不碰 Gate |

### 4.7 替代方案说明

- **若验收仍点不动**：再查 `DialogueTMPUGUI` / 全屏 Raycast / Console `Continue` 警告（方案 C），不动村。  
- **若要完美分层**：走方案 A，须 SceneManager + Prefab **成对**改，并处理 `Start.anim` 与 CanvasGroup 抢 alpha（或继续保留 YaerShow 落定后触发）。
