# Village_Shop 再次进店对话不显示 — 架构溯源报告

> 日期：2026-09-20  
> 角色：架构侦探（只读，未改代码 / Prefab / 场景 / 存档 / CSV）  
> Unity：2020.3.48f1  
> 对照：0830 ShopRepeat 接线；0919 村庄框出时藏默认小头像。以现网代码为准。

---

## ① 结论一句话

**主类型 B（Trigger 了，框/字看不见）。** 根因是 `Village_ShopRepeat` **没有**对话框淡入节点，而上一场对白结束会把 `subtitlesCanvasGroup.alpha` 淡到 0；再进店第一句只 `SetActive(true)` 不拉回 alpha，整条字幕条透明。不是 0919 把老板娘脸藏丢（那是 C，且第一句会 `ApplyShopkeeperPortrait` 拉回）。

---

## ② 原因

大白话：再进店程序有说话，老板娘短招呼也在播，但对话框透明度还停在上一场结束时的「全透明」，所以你觉得「没对话」。第一次进店的长开场自己带「框从透明淡到不透明」，所以正常；短招呼图当初为了省事没加这一步。

### A. 再进店现网调用链（0830 已接线，文档「静默」过期）

```
换场进 Village_Shop
  TryDeferBlackFadeForCover
    ShouldPlayShopStartStory() == false（存档已用过 Village_ShopStart）
    → TryDeferCoverForShopRepeat
         Hide UI_Shop + 热区 OFF + 锁相机对焦合层
         TriggerStory("Village_ShopRepeat")     ← 主 Trigger
         onStoryTriggered → 0.15s → FinalizeShopRepeatCoverAndCloseBlack
              （无 Prepare / 无分层闸门；只 CloseFormFade 黑幕）
         onStoryEnd → OnShopkeeperSpecialStoryEnd（与点头/Yes·No 同套：显 UI + 热区）
  OnEnterScene
    TryTriggerShopRepeatGreetingIfNeeded
      若 Defer 已 HasRunningStory → return（不双开）
      否则兜底再 Trigger 一次
```

| 步骤 | 现网会不会丢掉对白 |
|------|-------------------|
| `ShouldPlayShopStartStory` | 只闸 Start；非首次故意走 Repeat，**不会**静默（0830 已废 0827） |
| `TriggerStory` 失败 | Warning + 回退默认淡出并 Show UI；Console 有 `[ShopRepeat] TriggerStory 未启动` |
| Cover 超时 3s | 强制 `CloseFormFade`，**不**取消已 Trigger 的树 |
| 结束 | Special 语义显 UI，**不**走 Start 慢黑幕 |

第二次进店「应该」出现的 Console：`[ShopRepeat] 黑幕阶段 TriggerStory Village_ShopRepeat`（或兜底那句）。有这句就不是 A。

### B. 为何是 B 不是 A/C

| 证据 | 含义 |
|------|------|
| Prefab 在 `Assets/GameRes/Prefabs/Dialogue/Village_ShopRepeat.prefab`，根名 `Village_ShopRepeat`，4 句 Statement，Actor=老板娘 | 可加载，不是空图立刻结束 |
| 图**没有** `NormalDialogueUIAlphaAnimationTaskAction`（村庄多数图有；`ShopHead`/`ShopYes`/`ShopNo` 都有，**唯独 Repeat 没有**） | 不会把字幕条 alpha 拉回 1 |
| `DialogueTMPUGUI.DialogueEndSubtitlesCanvasGroupFade`：`DOFade(0, 0.7)` 后 `SetActive(false)` | 任一对话结束（含第一次 ShopStart）留下 alpha=0 |
| 再进店首句 `OnSubtitlesRequest`：`subtitlesGroup.SetActive(true)` **不写 alpha=1** | 条重新打开但仍透明 → **看不见框和字** |
| 0919 `OnDialogueStarted` → `HideAllMaskAvatars` | 只藏 Mask 小头像；店句第一句仍 `UseShopkeeperPortrait` → `ApplyShopkeeperPortrait` + 合层 `ShopkeeperFaceRegistry.Apply` | 脸会回来；**不是**「整段对话没了」的主因 |

对照：新档**第一次**进店走 `Village_ShopStart`，图内有 UI 淡入 + Prepare 分层，故正常。仅再进坏 → 缺口在 Repeat 管线（缺淡入），不在 Start。

### C. 0919 与店合层脸

| 问题 | 答案 |
|------|------|
| HideAll 时机 | `OnDialogueStarted`（为无淡入图补藏上一场脸） |
| ShopRepeat 句 | 四句 `UseShopkeeperPortrait=true`；不靠淡入 Prepare 出脸 |
| 店合层脸 | Statement 时 `ShopkeeperFaceRegistry.Apply`；Mask 同帧 `ApplyShopkeeperPortrait` |
| HideAll 会不会弄成「没对话」 | **不会。** 最多短暂无 Mask 脸，第一句 Apply 会亮。字幕看不见是 alpha，不是 HideAll |

不要把 0919 整段撤回；与本次冲突时只给 Repeat 补「框可见」路径。

### D. Prefab 与加载

| 检查 | 结果 |
|------|------|
| 路径 / 根名 | ✅ `Village_ShopRepeat` |
| Actor / Statement | ✅ Merchant + 欢迎~ / 来看看吧~ / … 四句 |
| 淡入节点 | ❌ **无**（相对 ShopHead 的缺口） |
| 被 0919 改坏预亮 | Repeat 本无 `PrepareMaskAvatarOnFadeIn` 字段；0919 改的是另外 4 张图 |

---

## ③ 用户需要做什么

1. 同档：先正常进店听完 ShopStart（或确认存档已播过 Start），离店，**再进店一次**。  
2. Console 搜 `[ShopRepeat]`：  
   - 有 `TriggerStory Village_ShopRepeat` → 不是 A；再看 Game 视图是否全无框（B）或有框无字/无脸（C）。  
   - 只有 `TriggerStory 未启动` Warning → 才按 A 查加载。  
3. 新档**第一次**进店：ShopStart 应仍正常（对照）。  
4. 施工后：再进店应看见「欢迎~」字幕条 + 老板娘合层/Mask；点头 / 点胸 / Yes·No 仍正常；其它村庄图框出时仍不要先闪默认小头像。

---

## ④ 给施工员的补充

### 改哪些 / 不要改哪些

| 全路径 | 做什么 |
|--------|--------|
| `Assets/GameRes/Prefabs/Dialogue/Village_ShopRepeat.prefab` | **主改。** 在第一句前插入 `NormalDialogueUIAlphaAnimationTaskAction`：StartAlpha→EndAlpha=1（可短 Duration，对齐 `Village_ShopHead`）；**`PrepareMaskAvatarOnFadeIn = false`**（勿预亮雅儿） |
| `DialogueTMPUGUI.cs` / `DialogueMaskAvatarPresenter.cs` | **不要**为修本 bug 删掉 `OnDialogueStarted` / `HideAllMaskAvatars` |
| `Village_ShopSceneManager.cs` | **不要**改回二进宫静默；Defer / Special 结束语义保持 |
| `Village_ShopStart` / Head / Chest / Yes / No | **不改** |
| 0919 已改的 4 张村庄 Prefab | **不改回**预亮 true |

### 推荐方案（只此一种）

给 `Village_ShopRepeat` 补与 `Village_ShopHead` 同构的**对话框淡入**节点（空框淡入，`PrepareMaskAvatarOnFadeIn=false`），再进第一句「欢迎~」。这样面板复用后 alpha 从 0 拉回 1；0919 的 HideAll 仍在，第一句店旗 Apply 出老板娘，不闪默认雅儿。

（可选加固，非本票必须：若还有其它无淡入村庄图，再考虑在 `OnSubtitlesRequest` 出字时兜底 `alpha=1`——另开，避免误伤 Start 的 Prepare 时序时要单独测。）

### 否决方案

- 二进宫改回静默：0830 产品已废，与「每次短招呼」相反。  
- 把 `HideAll` / `OnDialogueStarted` 整段删掉：会让 0919 村庄「框出闪默认小头像」回流；也修不好 alpha=0 的 B。

### OPEN

出售对白等无关本票。若产品确认「所有无淡入图」都要代码层兜底 alpha，再记一笔；本票最小只修 Repeat Prefab 即可，**不必改核心设计**。
