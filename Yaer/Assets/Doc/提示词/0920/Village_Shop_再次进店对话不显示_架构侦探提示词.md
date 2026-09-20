# Cursor Agent Prompt · 再次进店：对话不显示

> **角色**：【架构侦探】只读查清「第二次及以后进商店」为什么对白不显示；报告通过后再施工  
> **日期**：2026-09-20  
> **场景**：`Village_Shop` · 正常换场再次进店（非第一次）  
> **现象（用户）**：现在**再次进入商店**时，对话会不显示  
> **产品期望（钉死，与 0830 一致）**：  
> 1. **第 1 次**进店：仍播 `Village_ShopStart`（长开场，存档只播一次）  
> 2. **第 2 次及以后**：每次播 `Village_ShopRepeat`（短招呼），对白可见、说完再买卖  
> **不是**：改第一次开场台本；改点头/点胸/购买 Yes·No；改成二进宫静默（0827 旧设定已作废）  
> **报告落盘**：`Assets/Doc/执行文档/0920/Village_Shop_再次进店对话不显示_架构溯源报告.md`

把下面「侦探」整段交给 Cursor Agent。没写清「是没 Trigger、触发了但 UI 看不见、还是头像/字幕被藏没了」之前，不要施工。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 用户白话

> 再次进入商店，对话不显示了。

「不显示」可能是下面三种里的一种，侦探必须先钉死是哪一种：

| 类型 | 玩家看到 | 预扫嫌疑 |
|------|----------|----------|
| **A. 根本没播** | 黑幕一开就是买卖栏，没有对话框 | Trigger 没跑 / 加载失败 / 又被静默分支吃掉 |
| **B. 播了但看不见** | 有剧情锁着，但框/字透明或被 UI 挡住 | 黑幕没淡完、买卖 UI 盖住、对话框 alpha=0 |
| **C. 框在、内容空** | 有对话框，没有字 / 没有老板娘脸 | 0919「对话开始先藏头像」误伤无淡入的 ShopRepeat；或店合层脸没 Apply |

### 现网应有行为（0830 已施工，须复核是否还在）

```
再次进店（ShouldPlayShopStartStory == false）
  → TryDeferBlackFadeForCover → TryDeferCoverForShopRepeat
  → 黑幕内 TriggerStory("Village_ShopRepeat")
  → 壳 Ready 后淡出黑幕
  → 对白中：藏买卖 UI、热区关
  → onStoryEnd：显 UI、热区开
```

`Village_ShopRepeat`：**无**框淡入节点；老板娘脸靠店合层 / `UseShopkeeperPortrait`。  
0919 村庄小头像施工曾写：对话开始时 `HideAll`，并点名 **ShopRepeat 要验收「欢迎~」不能被藏丢**。若那次改完没验过 Repeat，很像 C 或连带 B。

### 嫌疑优先级

| # | 嫌疑 | 怎么验 |
|---|------|--------|
| **H1** | `TriggerStory(Village_ShopRepeat)` 没成功（Missing Prefab / 已有剧情在跑 / 兜底没跑） | Console 有无 `[ShopRepeat] TriggerStory` / Warning |
| **H2** | Trigger 成功，但买卖 UI 或黑幕把对话框盖住 / 对话框未打开 | Hierarchy：NormalDialogue 是否实例化、alpha、sorting |
| **H3** | 0919 `OnDialogueStarted` 强制 HideAll，ShopRepeat 无淡入，第一句老板娘脸或整段表现被弄没 | 对照 `DialogueTMPUGUI.OnDialogueStarted` 与店句 `UseShopkeeperPortrait` 时序 |
| **H4** | `ShouldPlayShopStartStory` 判断反了，或 Start used 后走回「静默 Idle」旧分支 | 读 `TryDefer` / `OnEnterScene` 现网是否仍分到 Repeat |
| **H5** | 只影响「再进」，第一次 Start 正常——说明缺口在 Repeat 专用管线，不是整店对话坏了 | 验收矩阵里对照测一次新档首进 |

### 禁止

- 本阶段不改代码、不改 Prefab、不改场景、不改存档。  
- 不要把产品改回「二进宫不说话」。  
- 不要为了修 Repeat 去动 ShopStart 分层闸门，除非报告证明同源。  
- 设计说不清的写入 `Assets/Doc/OPEN_QUESTIONS.md`。

---

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Project_context.md
@Assets/Doc/02_SYSTEM_SPEC.md
@Assets/Doc/执行文档/8月/0830/Village_Shop_非首次进店Village_ShopRepeat_架构溯源报告.md
@Assets/Doc/施工说明/0830/Village_Shop_非首次进店Village_ShopRepeat_施工说明.md
@Assets/Doc/执行文档/0919/Village_村庄对话框出现时闪默认小头像_架构溯源报告.md
@Assets/Doc/施工说明/0919/Village_村庄对话框出现时闪默认小头像_施工说明.md
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Scene/Village_Shop/Village_ShopSceneManager.cs
@Assets/Scripts/Game/GameRuntime/NodeCanvas/NodeCanvasExtend/DialogueTMPUGUI.cs
@Assets/GameRes/Prefabs/Dialogue/Village_ShopRepeat.prefab
@Assets/GameRes/Prefabs/Dialogue/Village_ShopStart.prefab
@Assets/Doc/OPEN_QUESTIONS.md

你现在是【架构侦探】。Unity 2020.3.48f1 / C#。
禁止修改任何代码、Prefab、场景、存档、CSV。只读扫描 + 写溯源报告。

报告写入：
Assets/Doc/执行文档/0920/Village_Shop_再次进店对话不显示_架构溯源报告.md

---

## 背景（策划白话）

现在再次进入商店时，对话会不显示。

产品要求（0830 已定，不要改回旧静默）：
- 第一次进店：Village_ShopStart
- 第二次及以后：每次 Village_ShopRepeat（欢迎~ 等短招呼），说完再买卖

用户说「对话不显示」。你必须先分清是：
A 根本没 Trigger，
B Trigger 了但框/字看不见，
C 有框但没字或没有老板娘脸。

近期 0919 为了消灭「框出时闪默认小头像」，在对话开始时会 Hide 小头像，并点名 ShopRepeat（无淡入）要防把「欢迎~」的老板娘弄丢。这次优先核对那次改动有没有误伤再进店。

---

## 必读 / 优先扫描

### A. 再进店现网调用链（以代码为准，0830 文档可能过期）

读 `Village_ShopSceneManager`：

- `ShouldPlayShopStartStory`
- `TryDeferBlackFadeForCover` → `TryDeferCoverForShopRepeat`
- `TryTriggerShopRepeatGreetingIfNeeded`（OnEnterScene 兜底）
- 对白中藏/显 `UI_Shop`、热区
- `onStoryEnd` 是否把 Repeat 和特殊对白走同一套显 UI

写出：第二次进店时，哪一步应该 Trigger `Village_ShopRepeat`；现网会不会在某一步 return / 超时强制淡出后丢掉对白。

### B. 分清 A/B/C（报告①必须写死一种主类型）

| 证据 | 指向 |
|------|------|
| Console 无 `[ShopRepeat] TriggerStory`，或有 Warning 未启动 | A |
| 有 Trigger 日志，但 Game 视图无对话框 / alpha=0 / 被 Bar 挡住 | B |
| 有对话框，无「欢迎~」字幕，或无老板娘脸 | C |

对照测一次**新档第一次**进店：若 Start 正常、仅再进坏，缺口在 Repeat 管线。

### C. 0919 与店合层脸

读：

- `DialogueTMPUGUI.OnDialogueStarted`（HideAll 时机）
- ShopRepeat 句是否 `UseShopkeeperPortrait`
- 店合层脸 Apply 是在 Statement 时还是依赖淡入 Prepare

回答：HideAll 会不会把老板娘脸藏掉且第一句 Apply 没回来，看起来像「没对话」；还是字幕本身也没出来。

### D. Prefab 与加载

- `Village_ShopRepeat.prefab` 是否仍在 `GameRes/Prefabs/Dialogue/`，根名是否仍是 `Village_ShopRepeat`
- 有没有被改坏、缺 Actor、缺 Statement，导致 Trigger 成功但立刻空结束

### E. 推荐一种最小改法

只推一种，并满足：

1. 再次进店能看见 ShopRepeat 对白（字 + 该有的老板娘表现）
2. 第一次 ShopStart 不变
3. 点头 / 点胸 / Yes·No 不变
4. 0919「框出时不要闪默认小头像」的村庄目标不要整段撤回；若冲突，写最小隔离（只修店句 / Repeat），并必要时记入 OPEN

另外两种各用一句话否决（例如「二进宫改回静默」「把 HideAll 整段删掉」）。

---

## 报告结构（固定）

① 结论一句话（A/B/C + 根因组件）  
② 原因（大白话 + 从进店到对白应出现的调用链，标断点）  
③ 用户需要做什么（同档二进店看 Console 关键词；对照新档首进）  
④ 给施工员的补充：改哪些文件、不要改哪些、推荐方案、否决方案

发现设计说不清的，追加到 `Assets/Doc/OPEN_QUESTIONS.md`，不要改核心设计。
```

---

## 施工员提示词（侦探报告落盘并确认方案后再贴）

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0920/Village_Shop_再次进店对话不显示_架构溯源报告.md
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Scene/Village_Shop/Village_ShopSceneManager.cs
@Assets/Scripts/Game/GameRuntime/NodeCanvas/NodeCanvasExtend/DialogueTMPUGUI.cs
@Assets/GameRes/Prefabs/Dialogue/Village_ShopRepeat.prefab

你现在是【施工员】。Unity 2020.3.48f1 / C#。
只按溯源报告推荐方案做最小改。
不要改回「二进宫静默」。不要改 ShopStart 台本。不要动点头/点胸/购买 Yes·No，除非报告证明同源必须一起修。

目标：
- 同档第二次及以后进店，能看见 Village_ShopRepeat 对白
- 第一次进店 ShopStart 仍正常
- 0919 村庄「框出时空头像」不要被整段撤掉；按报告做隔离修补

限制：
- 禁止在 Update 里堆显隐
- 注释说明断点为什么在这一处
- 施工说明写入：`Assets/Doc/施工说明/0920/Village_Shop_再次进店对话不显示_施工说明.md`
- 没有新架构就不要新造技术文档

完成后用大白话给验收清单：新档首进看开场；同档再进看「欢迎~」等短招呼是否显示；对白中买卖栏应隐藏，结束后再出现。
```
