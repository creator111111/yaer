# Village · 对话框出现时闪默认小头像 · 架构溯源报告

> 日期：2026-09-19  
> 角色：架构侦探（只读，未改代码 / Prefab / 场景 / CSV）  
> 复现入口：`Assets/GameRes/Prefabs/Dialogue/Village_KenMuNiStart.prefab`  
> 对照作废：0902「进村开场必须框淡入时预亮小头像」。本次以用户原话为准：框刚出现不要小头像；小头像跟第一句话一起出，并且直接是这一句的表情。

---

## ① 结论一句话

**主因是框淡入任务在出字之前就把 Mask 小头像点亮了；现网只有 4 张村庄对话图把这个开关打开。** 要同时做两件最小的事：这 4 张图把 `PrepareMaskAvatarOnFadeIn` 改成 false，并在共用淡入里先把上一场留下的小头像藏掉。不要改母体面板，不要改 Painting 默认脸，不要重导对话图。

---

## ② 原因

### 大白话

对话框淡入时，有的图会提前喊一声「把某某的某张脸亮起来」。框还是空的，左边已经有脸。等第一句话真正开始，系统再按这句话换一次脸。

进村开场就是这样：淡入时亮的是雅儿「大笑」，第一句「好漂亮的村子。」也是雅儿「大笑」。所以开场不是两张不同的表情对切，而是**脸比字先出来**。用户把这张提前出现的脸叫成默认脸。

真正「先一张脸、再换成另一个人」的，是另外 3 张仍开着预亮的图：商店点头、商店宝箱、出村长家送树屋。它们淡入时亮雅儿「得意」，第一句却是老板娘或古莎。

其余村庄对话图这个开关已经是关的，或根本没有这个字段。它们自己不会提前点脸。但对话面板会留下来复用：上一场谁在说话，那张脸的开关还开着。下一场框一淡入，旧脸会跟着框一起出现，第一句再换掉。所以只改开场一张图，村庄其它对话仍可能闪。

母体面板里各立绘默认是关的，旧小头像图也是关的。不是面板上焊死了一张默认脸。

### 从框出现到第一句换脸（现网调用链）

```
StoryComponentGSM.TriggerStory
  → 打开或复用 NormalDialogueNewPanel（已开着就不再新建，Awake 不会再跑）
  → DialogueTree 开始
       DialogueTMPUGUI.OnDialogueStarted     // 现网不藏 Mask
  → 框淡入节点 NormalDialogueUIAlphaAnimationTaskAction.OnExecute
       1. 字幕条若是关的，SetActive(true)
          // 子物体保持上次的亮/灭。上一场立绘若还亮着，会立刻回来
       2. ClearSubtitleTextsForEmptyFrame    // 清名字和正文 → 有框无字
       3. 若 PrepareMaskAvatarOnFadeIn == true 且角色不是 None
            PrepareMaskAvatarForFadeIn
              → DialogueMaskAvatarPresenter.Apply(MaskAvatarRole, MaskAvatarFace)
                 HideAll → 打开该角色 Painting → UpdateFace
          // ★ 默认/预亮脸的写入点。此时还没有第一句
       4. 字幕条 CanvasGroup 从 StartAlpha 淡到 EndAlpha（一般 0→1）
          // 小头像在 Mask 下面，跟着框一起淡出来
  → 淡入结束（EndActonOnAnimationEnd=true）才进下一节点
  → 第一句 Statement
       DialogueTMPUGUI.OnSubtitlesRequest
         → OnGetNewStatement(本句角色, 本句 FaceType)
            → Presenter.Apply
         店句走 ApplyShopkeeperPortrait，村长门口句走 ApplyChiefPortrait
         // ★ 第一句的脸在这里才写上。若和预亮不是同一张，玩家就看到再刷一次
  → 后面每句仍走同一条 OnGetNewStatement → Apply，不动
```

字段名（代码里就这三个，没有别名）：

| 黑板字段 | 类型 | 不写 `_value` 时 |
|----------|------|------------------|
| `PrepareMaskAvatarOnFadeIn` | bool | false，不预亮 |
| `MaskAvatarRole` | `DialogueRoleName` | 0 = None，即使勾了预亮也会直接 return |
| `MaskAvatarFace` | `DialogueFaceType` | 0 = None |

枚举对照（本报告用到的）：`Yaer=1`，`Smug=3`，`Smile=4`，`Laugh=6`，`Happy=10`，`Normal=12`。

### 开场钉死（Village_KenMuNiStart）

框淡入节点就是 `NormalDialogueUIAlphaAnimationTaskAction`（在前奏 Action 链里，大立绘 `CanvasGroupAlpha` 之后）。

| 项 | 现网值 |
|----|--------|
| `PrepareMaskAvatarOnFadeIn` | **true** |
| `MaskAvatarRole` | **Yaer(1)** |
| `MaskAvatarFace` | **Laugh(6)** |
| 第一句 | 演员「雅尔」，`FaceType=Laugh(6)`，台词「好漂亮的村子。」 |
| 预亮脸和第一句是不是同一张 | **是，都是雅儿大笑** |

所以开场磁盘上**对不上**「先 Smile 再换成另一张」。玩家看到的是：框淡入过程中大笑已经在，字还没出；第一句再 `Apply` 一次同一张大笑。产品仍算错，因为框出现时不该有小头像。

`GoOutStoryYaerPainting` 的 `defaultFace` 指向 `Armor_NoHeadWear_Smile`。`Apply` 里是先 `SetActive`（Awake 会打开这张 Smile）再立刻 `UpdateFace`。同一帧盖掉，不当主因。`SetDefaultPainting` 现在只同步头饰，不再强制 Smile。

### 三种可能，实际是哪一种

| # | 假设 | 结论 |
|---|------|------|
| 1 | 每张村庄图各自勾了预亮，都要改字段 | **只对 4 张成立**（见下表「要改字段」）。不是 37 张都勾了 |
| 2 | 淡入任务或 Painting 默认就会亮一张脸 | **半成立，这是要改的共用点**。淡入任务只有开关为 true 才 `Apply`。但淡入会把字幕条重新打开，**不会先藏掉上一场留下的脸**。`Presenter.Awake` 的 `HideAll` 只在面板第一次创建时跑；下一场对话若面板还在，Awake 不再跑 |
| 3 | 母体面板默认亮着一张脸 | **不成立**。`NormalDialogueNewPanel` 里 GoOut / Dress / Amy / Aliy / Gusha / Chief / Merchant 的 `m_IsActive` 都是 0。旧头像物体 `Yaer`（`actorPortrait`）也是关的。`useMaskAvatar=1`，白名单以外不会亮旧图 |

主因是 **1 的那 4 张 + 2 的残留脸**。只改 4 张字段，残留脸还在；只改代码不去掉那 4 张的 true，淡入仍会马上再亮一张脸。

### 关开场预亮，会不会把分层显现弄错位

只写风险，本阶段不改。

大立绘、背景、等待，走的是另一批节点（`WaitVillageStartBgReveal`、`CanvasGroupAlphaActionTask`），不读 `PrepareMaskAvatarOnFadeIn`。关掉预亮**不会**改它们的先后和时长。

会变的只有小头像：现在是「框淡入的同时大笑已经在」；改完是「框先淡入、没有小头像，淡入结束后第一句才出大笑+字」。框本身仍按原 Duration 淡入。这是用户要的，不是分层错位。

---

## ③ 用户需要做什么

验收时不要只看一张图。

1. 新档进 `Village_KenMuNi1`，看开场 `Village_KenMuNiStart`。框开始出现到第一句字出来之前，左边必须是空的。第一句「好漂亮的村子。」出现的同一下，才出雅儿大笑。不要先闪别的脸。
2. 再抽两场下面「要改字段」里的图：`Village_ShopHead`（第一句应直接是老板娘，不要先闪雅儿得意）、`Village_村长家门口初次对话`（这张开关已经是关的，用来确认没被带坏：第一句古莎「奶奶。」，框空的时候不要有雅儿）。
3. 后面句子换脸应和现在一样。大立绘淡入不要丢。

---

## ④ 给施工员的补充

### 推荐方案（一句）

淡入开始时先藏光 Mask 小头像；村庄里仍为 true 的 4 张图把 `PrepareMaskAvatarOnFadeIn` 改成 false；第一句仍走现有 `OnGetNewStatement` / 店句 / 村长句。编辑器前奏默认值也改成 false，防止 Setup 重跑把预亮写回去。

具体落点：

1. `DialogueMaskAvatarPresenter` 增加一个对外的「全部藏起」方法（内部就是现有 `HideAllPaintings`，并清掉 `shopkeeperMaskActive` / `chiefMaskActive`，否则下一句 `Invoke(None)` 会以为店/村长还亮着）。
2. `NormalDialogueUIAlphaAnimationTaskAction`：判定为淡入之后、可选预亮之前，先调用上面的藏起。预亮分支保留，给以后真的要「框和头像一起出」的图用。注释写明：村庄产品是空框，不能在这里无条件 `Apply`。
3. `DialogueTMPUGUI.OnDialogueStarted` 同样先藏一次。否则没有淡入节点的图（如 `Village_ShopRepeat`）仍会带出上一场的脸。第一句 `Apply` 仍在 `OnSubtitlesRequest`，时序不变。
4. 只改下面 4 张图的那一个 bool，不要动节点、台词、`MaskAvatarRole/Face` 占位。
5. `DialoguePreludeOptions.PrepareMaskAvatarOnFadeIn` 默认 **true → false**。门口 / 续聊 Setup 已经显式写 false，不用再改。不要改 `DialoguePreludeBuilder` 里「未勾预亮时 Role=Yaer、Face=Smug」的占位，那个在开关为 false 时不会 `Apply`。

### 要改的文件

| 全路径 | 改什么 | 不要改什么 |
|--------|--------|------------|
| `Assets/Scripts/Game/GameRuntime/UI/FormLogic/Story/DialogueMaskAvatarPresenter.cs` | 加对外藏起方法 | 不要改 `Apply` / 换装 / 店句 / 村长映射 |
| `Assets/Scripts/Game/GameRuntime/NodeCanvas/NodeCanvasNode/ActionTask/NormalDialoguePanel/NormalDialogueUIAlphaAnimationTaskAction.cs` | 淡入时先藏 Mask，再按开关决定是否预亮 | 不要删预亮字段；不要改清字、不要改 Duration |
| `Assets/Scripts/Game/GameRuntime/NodeCanvas/NodeCanvasExtend/DialogueTMPUGUI.cs` | 对话开始时藏一次 Mask | 不要改 `OnSubtitlesRequest` 的换脸顺序 |
| `Assets/GameRes/Prefabs/Dialogue/Village_KenMuNiStart.prefab` | `PrepareMaskAvatarOnFadeIn` true→false | 不要改 Laugh 占位，不要改分层节点，不要重导 |
| `Assets/GameRes/Prefabs/Dialogue/Village_ShopHead.prefab` | 同上 true→false | 不要改第一句店旗 |
| `Assets/GameRes/Prefabs/Dialogue/Village_ShopChest.prefab` | 同上 true→false | 同上 |
| `Assets/GameRes/Prefabs/Dialogue/Village_出村长家送树屋.prefab` | 同上 true→false | 不要改古莎第一句 |
| `Assets/Editor/Tool/Dialogue/DialoguePreludeOptions.cs` | 默认值改为 false，注释改成「村庄空框；要预亮必须显式 true」 | 不要改 Door/Continue Setup（它们已经是 false） |

### 否决方案

| 方案 | 为什么不用 |
|------|------------|
| 只改开场一个 Prefab | 商店点头、宝箱、送树屋仍会先闪雅儿得意；面板复用时其它村庄图仍会带出上一张脸 |
| 只改母体 `NormalDialogueNewPanel` | 现网立绘默认已经全关，改面板消不掉淡入时的 `Apply` |
| 把预亮脸改成和第一句一样（冒充不闪） | 开场已经是同一张大笑，问题是「比字先出来」。商店那 3 张第一句不是雅儿，改脸对不上。0911 已否决这种做法 |
| 删掉预亮代码，让所有对话都不能预亮 | 做得到隔离，不必删。没有村庄以外的图正在靠这个开关。删了以后想恢复「框和头像同拍」要再加回来 |

### 村庄对话清单（37 张，全路径）

「会不会先默认再换」按**现网、且面板是新打开的**来写。若上一场脸还留着，没勾预亮的图也会闪，那部分靠共用藏起解决，不必逐张改字段。

| 全路径 | 框淡入 / 预亮 | 预亮角色表情 | 第一句 | 现网会不会先另一张脸 | 要不要改 |
|--------|----------------|--------------|--------|----------------------|----------|
| `Assets/GameRes/Prefabs/Dialogue/Village_KenMuNiStart.prefab` | 有淡入，**预亮 true** | 雅儿 Laugh(6) | 雅尔 Laugh「好漂亮的村子。」 | 脸相同，但脸比字先出 | **改字段 + 靠共用藏起** |
| `Assets/GameRes/Prefabs/Dialogue/Village_ShopHead.prefab` | 有淡入，**预亮 true** | 雅儿 Smug(3) | 老板娘（店句）「说起来，看你的穿着…」 | **会**：先雅儿得意，再老板娘 | **改字段** |
| `Assets/GameRes/Prefabs/Dialogue/Village_ShopChest.prefab` | 有淡入，**预亮 true** | 雅儿 Smug(3) | 老板娘（店句）「嗯嗯嗯？？？～」 | **会**：先雅儿得意，再老板娘 | **改字段** |
| `Assets/GameRes/Prefabs/Dialogue/Village_出村长家送树屋.prefab` | 有淡入，**预亮 true** | 雅儿 Smug(3) | 古莎 Surprised「雅尔，雅尔真觉得…」 | **会**：先雅儿得意，再古莎惊讶 | **改字段** |
| `Assets/GameRes/Prefabs/Dialogue/Village_村长家门口初次对话.prefab` | 有淡入，预亮 **false**（Role 占位仍是雅儿 Smug，不会 Apply） | 不亮 | 古莎 Happy「奶奶。」 | 新面板不会。残留脸靠共用藏起 | **不用改字段**，回归 |
| `Assets/GameRes/Prefabs/Dialogue/Village_村长家继续对话.prefab` | 有淡入，预亮 **false** | 不亮 | 村长（Chief 句） | 同上 | **不用改字段**，回归 |
| `Assets/GameRes/Prefabs/Dialogue/Village_ShopStart.prefab` | 有淡入，预亮 false，Role=None | 不亮 | 雅尔 Surprised「这里是？」 | 自身不预亮 | 不用改字段 |
| `Assets/GameRes/Prefabs/Dialogue/Village_ShopNo.prefab` | 有淡入，预亮 false | 不亮 | 老板娘店句「哎呀，你好像没什么钱呢。」 | 自身不预亮 | 不用改字段 |
| `Assets/GameRes/Prefabs/Dialogue/Village_ShopYes.prefab` | 有淡入，预亮 false | 不亮 | 老板娘店句「谢谢惠顾~~」 | 自身不预亮 | 不用改字段 |
| `Assets/GameRes/Prefabs/Dialogue/Village_ShopRepeat.prefab` | **无淡入** | 无 | 老板娘店句「欢迎~」 | 无空框预亮；面板残留仍可能闪一帧 | 不用改字段，靠对话开始时藏起 |
| `Assets/GameRes/Prefabs/Dialogue/Village_Leader‌GuShaAmyAliy.prefab` | 有淡入，**没有预亮字段**（代码当 null，不 Apply） | 不亮 | 古莎 Smile「啊，雅尔你来啦~」 | 自身不预亮 | 不用改字段 |
| `Assets/GameRes/Prefabs/Dialogue/Village_Aegir_QuestTurnIn.prefab` | 有淡入，没有预亮字段 | 不亮 | 雅尔 Smug「今天的数量达成了。」 | 自身不预亮 | 不用改字段 |
| `Assets/GameRes/Prefabs/Dialogue/Village_Aegir_QuestOffer.prefab` | 有淡入，预亮 false | 不亮 | 雅尔 Surprised「啊，是你呀。」 | 自身不预亮 | 不用改字段 |
| `Assets/GameRes/Prefabs/Dialogue/Village_Aegir_bingan.prefab` | 有淡入，预亮 false | 不亮 | 雅尔 Surprised | 自身不预亮 | 不用改字段 |
| `Assets/GameRes/Prefabs/Dialogue/Village_Aegir_lansedehua.prefab` | 有淡入，预亮 false | 不亮 | 雅尔 Happy | 自身不预亮 | 不用改字段 |
| `Assets/GameRes/Prefabs/Dialogue/Village_Aegir_mianbao.prefab` | 有淡入，预亮 false | 不亮 | 雅尔 Surprised | 自身不预亮 | 不用改字段 |
| `Assets/GameRes/Prefabs/Dialogue/Village_Aegir_yifu.prefab` | 有淡入，预亮 false | 不亮 | 雅尔 ChiBie | 自身不预亮 | 不用改字段 |
| `Assets/GameRes/Prefabs/Dialogue/Village_Aegir_zaotai.prefab` | 有淡入，预亮 false | 不亮 | 雅尔 Surprised | 自身不预亮 | 不用改字段 |
| `Assets/GameRes/Prefabs/Dialogue/Village_Npc1.prefab` | 有淡入，预亮 false | 不亮 | NPC1 Normal | 自身不预亮 | 不用改字段 |
| `Assets/GameRes/Prefabs/Dialogue/Village_Npc1_bingan.prefab` | 有淡入，预亮 false | 不亮 | 雅尔 Happy | 自身不预亮 | 不用改字段 |
| `Assets/GameRes/Prefabs/Dialogue/Village_Npc1_huangmi.prefab` | 有淡入，预亮 false | 不亮 | 雅尔 Smile | 自身不预亮 | 不用改字段 |
| `Assets/GameRes/Prefabs/Dialogue/Village_Npc1_mianbao.prefab` | 有淡入，预亮 false | 不亮 | 雅尔 Happy | 自身不预亮 | 不用改字段 |
| `Assets/GameRes/Prefabs/Dialogue/Village_Npc1_muxiang.prefab` | 有淡入，预亮 false | 不亮 | 雅尔 Surprised | 自身不预亮 | 不用改字段 |
| `Assets/GameRes/Prefabs/Dialogue/Village_Npc1_muzhiyuantong.prefab` | 有淡入，预亮 false | 不亮 | 雅尔 Surprised | 自身不预亮 | 不用改字段 |
| `Assets/GameRes/Prefabs/Dialogue/Village_Npc1_tudou.prefab` | 有淡入，预亮 false | 不亮 | 雅尔 Smile | 自身不预亮 | 不用改字段 |
| `Assets/GameRes/Prefabs/Dialogue/Village_Npc45.prefab` | 有淡入，预亮 false | 不亮 | NPC4 Normal | 自身不预亮 | 不用改字段 |
| `Assets/GameRes/Prefabs/Dialogue/Village_NpcChairChild.prefab` | 有淡入，预亮 false | 不亮 | NPC2，FaceType None | 自身不预亮 | 不用改字段 |
| `Assets/GameRes/Prefabs/Dialogue/Village_QuestOffer_NPC23.prefab` | 有淡入，预亮 false | 不亮 | NPC2 Normal「妈妈，有外人！」 | 自身不预亮 | 不用改字段 |
| `Assets/GameRes/Prefabs/Dialogue/Village_QuestThanks_NPC23.prefab` | 有淡入，预亮 false | 不亮 | NPC3 Normal | 自身不预亮 | 不用改字段 |
| `Assets/GameRes/Prefabs/Dialogue/Village_QuestTurnIn_NPC23.prefab` | 有淡入，预亮 false | 不亮 | NPC3 Normal | 自身不预亮 | 不用改字段 |
| `Assets/GameRes/Prefabs/Dialogue/Village_TreeHouseLock.prefab` | 有淡入，预亮 false | 不亮 | 雅尔 Normal「锁上了打不开」 | 自身不预亮 | 不用改字段 |
| `Assets/GameRes/Prefabs/Dialogue/Village_Well_NeedQuest.prefab` | 有淡入，预亮 false | 不亮 | 雅尔 Normal | 自身不预亮 | 不用改字段 |
| `Assets/GameRes/Prefabs/Dialogue/Village_Well_NoEmptyBucket.prefab` | 有淡入，预亮 false | 不亮 | 雅尔 Normal | 自身不预亮 | 不用改字段 |
| `Assets/GameRes/Prefabs/Dialogue/Village_老农打水任务.prefab` | 有淡入，预亮 false | 不亮 | 雅尔 Surprised「好大的一片田。」 | 自身不预亮 | 不用改字段 |
| `Assets/GameRes/Prefabs/Dialogue/Village_老农打水任务_今日已完成.prefab` | 有淡入，预亮 false | 不亮 | 老人 Normal | 自身不预亮 | 不用改字段 |
| `Assets/GameRes/Prefabs/Dialogue/Village_老农打水任务_进行中.prefab` | 有淡入，预亮 false | 不亮 | 老人 Normal | 自身不预亮 | 不用改字段 |
| `Assets/GameRes/Prefabs/Dialogue/Village_老农打水任务_完成结算.prefab` | 有淡入，预亮 false | 不亮 | 老人 Normal | 自身不预亮 | 不用改字段 |

### 不要算进本次村庄清单

这些图没有 `PrepareMaskAvatarOnFadeIn=true`。只改上面 4 张字段不会碰到它们。共用「淡入 / 对话开始时藏起」会让它们也变成空框再出第一句，和本次产品一致，**不是**拆掉「故意框和头像一起淡入」（现网没有这种勾选）。

| 为什么提到 | 文件 | 处理 |
|------------|------|------|
| 新游戏开场，0911 已关预亮 | `Assets/GameRes/Prefabs/Dialogue/NewGameStory.prefab`（false，占位仍是雅儿 Laugh，第一句是 Unhappy） | **不要改这张图** |
| 肯姆尼场景里挂了森林检视 | `Village_KenMuNi1` 上的 `ForestSceneStoneBrand` | 不进村庄改表 |
| 村外场景挂的是西路图 | `Village_OutSide` 上的 `WestRappRoadViewVillage*`、`WestRappRoadVillageTalkWithAmyAliy`、`WestRappRoadVillageTalkWithNPC2` | 不进村庄改表 |
| 民居 23 挂了龙宫 NPC 图 | `HomeScene1Npc1` | 不进村庄改表 |
| 夜村 / 旧 `Village.unity` 挂的是森林图 | `ForestScene*` | 不进村庄改表 |
| 龙宫、森林、东郊、翠绿走廊全部对话图 | 扫描结果：淡入节点要么没有预亮字段，要么不会 Apply | **不要改这些 Prefab** |

### 村庄以外会被误伤的文件

**无。** 全库 `PrepareMaskAvatarOnFadeIn=true` 只有上面 4 张村庄图。`NewGameStory` 已经是 false。

---

## 回归时要看、但不用改字段的场次

开场、`Village_ShopHead`、`Village_村长家门口初次对话`。若还要第三场，看 `Village_ShopRepeat`（没有淡入，用来确认「对话开始就藏起」有没有把欢迎句的老板娘头像弄丢）。
