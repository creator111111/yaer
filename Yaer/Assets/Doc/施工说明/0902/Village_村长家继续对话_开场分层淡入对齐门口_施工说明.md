# Village_村长家继续对话 — 开场分层淡入对齐门口 — 施工说明

**文档版本**：v1.0（2026-09-02）  
**文档性质**：【施工员】按侦探报告 T1′ + T3 落地  
**Unity**：2020.3.48f1  
**侦探报告**：`执行文档/0902/Village_村长家继续对话_开场分层淡入对齐门口_架构溯源报告.md`  
**产品**：进屋续聊可见压黑 → 立绘淡入 → 对话框淡入（非三件套瞬现）；对齐门口初次手感

---

## 沟通摘要

### ① 结论一句话

**已把揭黑挪到「树 Instantiate + 白名单 alpha=0」之后；续聊 Prefab/Setup 关掉 PrepareMask 并显式 StartAlpha=0。未整壳覆盖、未开 Loading。**

### ② 原因（通俗）

续聊图里本来就有淡入，但换场黑幕在对话树还没摆好就揭开了，先看见空客厅再突然齐活。  
现在等壳摆好、透明度先压到 0，再揭黑，你就能看见 0→1。顺便把误开的框出预亮小头像关了。

### ③ 用户检查清单

| # | 操作 | 通过 |
|---|------|------|
| 1 | 进屋续聊：压黑 → 立绘淡入 → 对话框淡入，非瞬现 | |
| 2 | 时长接近门口（立绘约 1s → 框 Delay0.5+1s） | |
| 3 | 首句前框可空字；小头像不早于首句乱亮 | |
| 4 | 三人摆位/Scale 仍对齐门口 | |
| 5 | 针线包 GetItem / Tips / Save 仍在 | |
| 6 | 门口初次分层未坏；无蛋糕读条 | |
| 7 | 续聊结束换古莎黑幕仍正常 | |

### ④ 程序补充

见下文。

---

## ① 改动清单

| # | 文件 | 动作 |
|---|------|------|
| 1 | `Village_Chief_HouseSceneManager.cs` | **T1′** 轮询树就绪 → `TryPrepareContinueLayeredReveal` → 再 CloseFormFade |
| 2 | `Village_村长家继续对话.prefab` | **T3** PrepareMask=false；StartAlpha 显式 0 |
| 3 | `VillageChiefContinueDialogueSetupEditor.cs` | PrepareMaskAvatarOnFadeIn=false 防回潮 |

**未改**：门口 Prefab、Loading API、针线包节点、结束淡出、摆位。

---

## ② 时序（落地后）

```
Defer Trigger 续聊
  → onStoryTriggered（早于 Instantiate）
  → 轮询：DialogueSceneContainer 下有 DialogueTreeController
  → Prepare：字幕条 + 雅/古/村 CanvasGroup alpha=0（白名单）
  → S1 显隐（战斗待机）
  → CloseFormFade 揭黑 → 玩家看见 Prefab 内 0→1
```

---

## ③ OPEN

| ID | 项 | 状态 |
|----|----|------|
| Q3 T1′+T3 | 已施工 | ✅ |
| Q4 PrepareMask | false | ✅ |
| Q5 二次对话黑幕 T1 | 仅验收不够时再上 | ⏳ |
