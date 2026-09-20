# Village_HomeScene23 对话不要大立绘 — 架构溯源报告

> 日期：2026-09-20  
> 角色：架构侦探（只读，未改代码 / Prefab / 场景 / CSV）  
> Unity：2020.3.48f1  
> 现象：屋内点椅子，第一句已是孩子「妈妈，有外人！」，屏幕左侧仍有雅儿大半身，挡住屋子并压在对话框上。

---

## ① 结论一句话

左侧那张是 **`Village_QuestOffer_NPC23` 自己嵌进去的 `GoOutStoryYaerPainting`（父节点是说话人「雅尔」，没有 Mask）**，开场节点先把它从透明淡到不透明，然后才播第一句；**改这三张民居任务对话 Prefab，不要改共用面板、不要改立绘母体。**

| | 大立绘（本期关掉） | 小头像（本期保留） |
|--|------------------|------------------|
| 玩家看到 | 左侧大半身，约 854×949，锚点偏左 | 对话框左下角小格 |
| 物体 | 对话 Prefab 里 `雅尔` 的子物体 `GoOutStoryYaerPainting` | `NormalDialogueNewPanel` 里 `Bottom/Mask` 下另一份同名实例 |
| 有没有 Mask | **没有**。挂进 `DialogueSceneContainer`，整张原图直接画 | **有**。Mask 裁成小窗 |
| 第一句说话人 | 孩子 NPC2「妈妈，有外人！」。雅尔要到第 6 句才开口 | 跟台词走，本场淡入没有预亮小头像 |

判据（截图就是这一张，不是 Mask 漏图）：

1. 节点名 `GoOutStoryYaerPainting`，父节点是对话 Prefab 里的 Actor `雅尔`（`_roleName: 1`），再往上是 `Village_QuestOffer_NPC23`。  
2. 实例改过位置：`AnchoredPosition (-559, 52)`，`SizeDelta 853.9938 × 949.0917`，缩放仍是母体的 1。面板里那份小头像是另一处实例，位置约 `(-13.8, -90)`、缩放 `0.65`，并且在 `Mask` 下面。  
3. 面板的 `DialogueSceneContainer` 自己是空的，运行时才把对话 Prefab 塞进去。大立绘跟着这段剧情来，不是面板默认摆着。  
4. 图里第一句说话人是 NPC2，不是雅尔。小头像这条线本场 `PrepareMaskAvatarOnFadeIn` 为空（不预亮）。左侧雅儿不可能是小头像提前亮出来。

---

## ② 原因

大白话：这段对话在孩子开口之前，先花 1 秒把雅儿大半身从看不见淡到看得见。所以第一句还是「妈妈，有外人！」，雅儿已经站在左边了。不是因为雅儿在说话。

母体 `Assets/Prefabs/DialougeProtrait/GoOutStoryYaerPainting.prefab` 默认 **激活、CanvasGroup.alpha = 1**。这段图又在第一句之前显式淡入，所以即使母体默认是满的，开场也会先压成 0 再拉到 1，等淡完才出字幕。

调用链（点椅子这场，截图就是它）：

```
Village_HomeScene23 / NpcChair
  Npc23QuestStoryTrigger.ResolveStoryPrefabName
    Quest_002 未接 → Village_QuestOffer_NPC23
  SimpleStoryTrigger → StoryComponentGSM.TriggerStory
  NormalDialogueFormNewLogic 把 Prefab 实例化到
    NormalDialogueNewPanel / DialogueSceneContainer
  图节点顺序（Offer）：
    18 藏战斗面板
    → 0  CanvasGroupAlpha：GoOutStoryYaerPainting  0 → 1，时长 1 秒，等播完
    → 1  对话框淡入（PrepareMaskAvatarOnFadeIn 空，不预亮小头像）
    → 2  NPC2「妈妈，有外人！」
```

黑板变量 `GoOutStoryYaerPainting` 绑的是这段 Prefab 里那份 CanvasGroup（实例 fileID `8129792323373277873`），不是面板 Mask 里那份。`CanvasGroupAlphaActionTask` 只改透明度，不碰 Mask。

台本 `Assets/Dialog/Village_NPC23椅子孩子第一天对话.csv`：第 1 句 Speaker=2，雅在第 6 句。Prefab 图与 CSV 一致，第一句 Actor 就是 NPC2。大立绘在第 1 句之前已经淡完，不是「雅儿说话所以出立绘」，也不是面板默认显示。

---

## ③ 用户需要做什么

进 `Village_HomeScene23`，点椅子上的 NPC（`NpcChair`，第一句应是「妈妈，有外人！」）：从对话框出现到这段说完，左侧都不能再有大半身；对话框、台词、左下角小头像还在。

再点屋内另一个会说话的 NPC（`Npc1`）：确认那边本来就没有这张大半身（见下，它播的不是民居台本）。

然后去村开场、村长家、商店各点一段本来有大立绘的对话，确认大半身还在。

接过任务后再点椅子：进行中不够 5 个藤蔓果会播感谢，够了会播交付。这两段也要从头到尾没有大半身。

---

## ④ 给施工员的补充

### 本场景会播的对话（全路径）

场景 `Assets/GameRes/Scenes/Village_HomeScene23.unity` 里，只有两处填了剧情 Prefab 名。门、`NpcXiaer` 没有 `StoryPrefabName`。

| 场景物体 | 触发器 | 运行时实际 Prefab | 会不会出大立绘 | 要不要改 |
|----------|--------|-------------------|----------------|----------|
| `NpcChair` | `Npc23QuestStoryTrigger`（场景字段只是默认名） | 见下三行 | 会 | **要改这三张** |
| ↳ Quest_002 未接 | 代码写死 | `Assets/GameRes/Prefabs/Dialogue/Village_QuestOffer_NPC23.prefab` | 会。`雅尔/GoOutStoryYaerPainting`，开场 0→1、1 秒。第一句 NPC2「妈妈，有外人！」 | 要 |
| ↳ 进行中且藤蔓果不足，或已交付，或其它脏状态 | 代码写死 | `Assets/GameRes/Prefabs/Dialogue/Village_QuestThanks_NPC23.prefab` | 会。同一物体，开场 0→1、0.7 秒。第一句 NPC3「感谢你」 | 要 |
| ↳ 进行中且够 5 个 | 代码写死 | `Assets/GameRes/Prefabs/Dialogue/Village_QuestTurnIn_NPC23.prefab` | 会。同一物体，开场 0→1、0.7 秒。第一句 NPC3「有了这些今晚就不用愁了」 | 要 |
| `Npc1` | `SimpleStoryTrigger` | `Assets/GameRes/Prefabs/Dialogue/HomeScene1Npc1.prefab` | **不会。** 只有 Actor `npc1`，没有立绘子物体，图里也没有 CanvasGroup 淡入 | **不要改它的立绘** |

`HomeScene1Npc1` 另说一行：这是龙宫 `HomeScene1.unity` 的对话（第一句 NPC1「被父亲训斥了。。。。摸摸。」），不该拿来当民居台词。本场 `Npc1` 挂错了名字。**不要为了这场去改它的立绘规则**；换不换民居台本是另案（见 OPEN）。0601 建议名 `Village_HomeScene23_Npc1` 等 Prefab 磁盘上不存在。

三张任务图里的大立绘是同一份嵌套：母体 guid `4c0e9909764ce6e4eb37971a6fe20fd3`，实例名改成 `GoOutStoryYaerPainting`，位置都是 `(-559, 52)`。Thanks / TurnIn 与 Offer 同结构。

### 文件表

| 全路径 | 改什么 | 不要改什么 |
|--------|--------|------------|
| `Assets/GameRes/Prefabs/Dialogue/Village_QuestOffer_NPC23.prefab` | 见推荐方案 | 台词、选项、接任务节点、对话框淡入、小头像字段 |
| `Assets/GameRes/Prefabs/Dialogue/Village_QuestThanks_NPC23.prefab` | 同上 | 同上 |
| `Assets/GameRes/Prefabs/Dialogue/Village_QuestTurnIn_NPC23.prefab` | 同上 | 同上 |
| `Assets/Prefabs/DialougeProtrait/GoOutStoryYaerPainting.prefab` | 不改 | 母体默认激活、alpha=1。改了村开场 / 村长家 / 商店的大立绘一起没 |
| `Assets/GameRes/Prefabs/UI/NormalDialogueNewPanel.prefab` | 不改 | `Mask` 下小头像、`DialogueSceneContainer` |
| `Assets/GameRes/Prefabs/Dialogue/HomeScene1Npc1.prefab` | 不改 | 龙宫台词与结构 |
| `Assets/Scripts/.../Npc23QuestStoryTrigger.cs` | 不改 | 只负责换 Prefab 名，不负责立绘 |
| `Assets/Dialog/Village_NPC23椅子孩子第一天对话.csv` | 不改、不重导 | 重导会把开场淡入立绘再做回来 |
| `Assets/GameRes/Scenes/Village_HomeScene23.unity` | 不改 | 换 `Npc1` 的剧情名不在本票 |

### 推荐方案（只此一种）

只动手改上面三张任务 Prefab，两步都要做，不要重导 CSV：

1. 删掉（或断开）开场那个 `CanvasGroupAlphaActionTask`（目标 `GoOutStoryYaerPainting`，EndAlpha=1）。上一节点直接接到「对话框淡入」。Offer 是节点 18→0→1，Thanks/TurnIn 是 0→1→2，接法相同：跳过立绘淡入，保留对话框淡入。  
2. 把 Actor `雅尔` 下面那份 `GoOutStoryYaerPainting` 实例设为 **不激活**（只改这三张上的实例，不改母体）。

为什么两步都要：只删淡入节点，母体默认 alpha=1、默认激活，打开就仍是满的大半身。只关掉物体、留下淡入节点，节点仍会去改这块 CanvasGroup；不激活可以挡住绘制，但节点还在，以后有人再激活会立刻淡出来。两步一起，这段从打开到结束都没有大立绘。小头像在面板 `Mask` 里，不走这个节点。

手改节点不会丢台词：语句、选项、`QuestAccept` 都在后面的节点上，只拆开场那一条淡入边。

### 否决方案

- 只藏第一句：第 6 句雅尔开口时大立绘还在，而且 Thanks/TurnIn 整段都会亮。  
- 关掉整个 `NormalDialogueNewPanel` 的立绘根或立绘母体：村开场、村长家、商店的大立绘一起没；小头像若被一起关掉，本场还要留的小格也没了。

### 会误伤的其它场景

按推荐方案：**无。** 这三张 Prefab 只被 `Npc23QuestStoryTrigger` 按任务状态加载，不进村开场 / 村长家 / 商店。

不要顺手改 `HomeScene1Npc1`、立绘母体、`NormalDialogueNewPanel`。
