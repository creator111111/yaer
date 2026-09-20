# NewGameStory — 主角大立绘恢复 — 施工说明

**文档版本**：v1.0（2026-09-11）  
**文档性质**：【施工员】按侦探报告方案 **A** 最小落地  
**Unity**：2020.3.48f1  
**侦探报告**：`Assets/Doc/执行文档/0911/NewGameStory_主角大立绘不出现_架构溯源报告.md`  
**根因**：H1 + H7 — Prepare 藏立绘/关 Animator，Prefab 仍是 0807-B 并行 `YaerShow`，不成对

---

## 沟通摘要

### ① 结论一句话

**已把 `NewGameStory.prefab` 前奏改成串行 Wait→`CanvasGroupAlpha(YaerPainting)`→UIAlpha，与现网 Prepare 成对；未砍旁路、未改村线。**

### ② 原因（通俗）

开场脚本先把大立绘透明度打成 0，并关掉入场动画开关。  
旧对话树还在发 `YaerShow` 指望动画把人播出来——开关已关，触发器打空，树里又没有「透明度淡回 1」→ 字有、人没有。  
现在改为：等 BG 闸 → 用 CanvasGroup 淡回立绘（淡完自动开 Animator 并落到 YaerShow 末帧）→ 再淡对话框。

### ③ 用户检查清单

| # | 操作 | 通过判据 |
|---|------|----------|
| 1 | **完整新游戏**：漫画播完进龙宫对白 | 可见 `YaerPainting` 淡入（连衣裙+皇冠） |
| 2 | Console 过滤 `[NewGameStory][Prepare]` | 仍有 `hide YaerPainting` / `disable Animator` |
| 3 | 随后过滤 `[CanvasGroupAlpha]` | 应有 `Animator … YaerShow end`（或 re-enable） |
| 4 | Hierarchy 首句时选场景 `YaerPainting` | `CanvasGroup.alpha ≈ 1`；故事根 Animator **已恢复** |
| 5 | 点多句至 King 出场 | `KingMove` 不崩 |
| 6 | DialogDebug 只拖同 Prefab | 不永久卡（闸默认 Ready）；**不能**单凭此项当正式通过 |
| 7 | 村线 `Village_KenMuNiStart` | 无回归 |

### ④ 程序补充

见下文。

---

## 改动清单

| 项 | 路径 | 说明 |
|----|------|------|
| **F_A Prefab 前奏** | `Assets/GameRes/Prefabs/Dialogue/NewGameStory.prefab` | 首 ActionList：`executionMode=0`（串行） |

### 前奏节点（现网）

| 顺序 | Task | 关键参数 |
|------|------|----------|
| 1 | `WaitVillageStartBgRevealActionTask` | Hold **0.5** |
| 2 | `CanvasGroupAlphaActionTask` | BB/`_name`=`YaerPainting`；0→1；Dur **0.5**；**`EndActionOnAnimationEnd=true`**（必走 Restore→YaerShow 末帧） |
| 3 | `NormalDialogueUIAlphaAnimationTaskAction` | Delay **0.5** + Dur **0.5**；`EndActonOnAnimationEnd=true`；~~原 PrepareMask=true（对齐 KenMuNi）~~ → **0911 小头像案已改为 false（空框）**，见同目录 `NewGameStory_开局小头像空框取消预亮_施工说明.md` |

### Blackboard

| 变量 | 类型 | 说明 |
|------|------|------|
| `Volume` | float | 保留 |
| **`YaerPainting`** | `CanvasGroup` | 新增；id `c8f0a1b2-3d4e-5f67-8901-23456789abcd`；运行时按物体名解析场景 DialogueScene 下大立绘（**勿**绑 Mask 子树同名） |

### 已去掉（前奏主路径）

- `NormalDialogueBlackMaskTaskAction`
- `MecanimSetTrigger("YaerShow")`

### 保留（禁止动）

- `NewGameSceneManager.PrepareNewGameLayeredReveal`（藏立绘 + 关 Animator）
- 后续 `MecanimSetTrigger("KingMove")` 及全部 Statement
- 村线 `Village_KenMuNiStart.prefab` / GameFramework / PNG

---

## 成对时序（完整新游戏）

```
漫画全黑 → TriggerStory(NewGameStory)
  → Prepare：YaerPainting.alpha=0；故事 Animator.enabled=false
  → HideFade（拍1）→ SignalBgFullyVisible
  → Prefab 串行：
       Wait(0.5)
    → CanvasGroupAlpha 0→1（EndOnEnd）→ TryRestoreStoryAnimator → Play(YaerShow, normalized=1)
    → UIAlpha Delay0.5+Dur0.5（+PrepareMask）
    → Statement…
  → 后续 KingMove（依赖 YaerShow 末帧）
```

---

## 未改代码文件

- `NewGameSceneManager.cs`
- `CanvasGroupAlphaActionTask.cs`（已有 Restore，直接复用）
- 任何村线 Prefab / 动画 Clip / GameFramework

---

## 剩余风险

| 风险 | 说明 |
|------|------|
| BB 场景未手拖引用 | 依赖 `_name=YaerPainting` 按名查找；须是 DialogueScene 大立绘，勿 Mask 下同名 |
| CSV 重导冲掉前奏 | 再导入可能漂回并行三件套；验收前核对首 ActionList |
| DialogDebug 误判 | 无 Prepare 时旧路径也可能「看起来好」；正式以完整新游戏为准 |
| MaskAvatarFace=6（旧） | 曾对齐 KenMuNi；已另案 **关预亮**，不再预亮 Laugh |

---

## 验收对照（报告 §5 / §8）

- [ ] 漫画后大立绘淡入可见  
- [ ] Prepare hide/disable 后出现 `[CanvasGroupAlpha] … YaerShow`  
- [ ] KingMove / 点至树尾正常  
- [ ] 村线无回归  
- [ ] 未删 Prepare、未强制 alpha=1 当终局  
