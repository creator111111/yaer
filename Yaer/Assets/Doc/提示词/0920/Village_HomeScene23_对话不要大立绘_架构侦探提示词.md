# Cursor Agent Prompt · Village_HomeScene23：室内对话不要大立绘

> **角色**：【架构侦探】只读查清大立绘是谁亮的，并列出要改的对话文件；报告通过后再施工  
> **日期**：2026-09-20  
> **场景**：`Assets/GameRes/Scenes/Village_HomeScene23.unity`  
> **现象（用户截图）**：室内点开对话，左侧出一张雅儿**大立绘**（白发、铠甲、角），挡住屋子。截图第一句是「妈妈，有外人！」，说话人不是雅儿，大立绘已经在  
> **产品期望（钉死）**：这个场景里的对话**都不要大立绘**。对话框、台词、对话框里的小头像保持现网  
> **不是**：关掉整个对话框；改台词；改村开场 / 村长家 / 商店那些本来就要大立绘的对话；把 Mask 小头像体系拆掉  
> **报告落盘**：`Assets/Doc/执行文档/0920/Village_HomeScene23_对话不要大立绘_架构溯源报告.md`

把下面「侦探」整段交给 Cursor Agent。没写清「改哪几份对话、会不会误伤别的场景」之前，不要施工。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 两个东西不要混

| 词 | 玩家看到的 | 本期 |
|----|------------|------|
| **大立绘** | 屏幕左侧大半个雅儿，压在对话框和屋子上面 | **不要** |
| **小头像** | 对话框左下角小方框里的脸 | **留着** |

截图里两样叠在一起。先分清亮的是「对话 Prefab 里单独放的大立绘」，还是「小头像那张图没被 Mask 裁住，看起来像大立绘」。两种改法不一样。

### 这场已知会播的对话（须在场景里再数一遍）

| 线索 | 路径 | 备注 |
|------|------|------|
| 场景上写死的剧情名 | `Village_HomeScene23.unity` 里 `StoryPrefabName: Village_QuestOffer_NPC23` | 第一句对得上截图「妈妈，有外人！」 |
| 另一条 | 同场景 `StoryPrefabName: HomeScene1Npc1` | 要确认是不是龙宫对话，播的时候会不会也出大立绘 |
| 台本 | `Assets/Dialog/Village_NPC23椅子孩子第一天对话.csv` | 第 1 句说话人是 `2`，不是雅。雅到第 6 句才说 |
| 换对话的脚本 | `Npc23QuestStoryTrigger.cs` | 接任务后可能换成另一份 Prefab，那些也算「这个场景里的对话」 |

场景里若还有别的 Trigger / 别的 Prefab 名，全部列入清单，不要只写椅子这场。

### 嫌疑

| # | 嫌疑 | 怎么验 |
|---|------|--------|
| **A** | `Village_QuestOffer_NPC23` 开场有立绘淡入（`CanvasGroupAlpha` / 导入器「立绘 CanvasGroup 淡入」），第一句之前就把雅儿大立绘拉出来 | 读 Prefab 图：淡入节点指向哪一个物体；该物体是全屏立绘还是 Mask |
| **B** | 对话 Prefab 里嵌了 `Yaer` / `GoOutStoryYaerPainting`，默认就是显示的 | 看层级默认 Active，不要只看代码 |
| **C** | 共用对话框 `NormalDialogueNewPanel` 的 Mask 没裁住，立绘原图撑满左侧 | 若成立，不能关母体，否则村开场小头像一起没；要查这场为什么没裁住 |
| **D** | 说话人一切到雅，通用逻辑自动出大立绘，所以后几句也会有 | 用台本对：第一句说话人不是雅，若第一句已经有，就不是「雅说话才出」 |

### 禁止

- 本阶段不改代码、不改 Prefab、不改场景、不改 CSV。  
- 不要为了藏大立绘把对话框或小头像一起藏掉。  
- 不要改 `Village_KenMuNiStart`、村长家门口、商店开场的大立绘。  
- 设计说不清的写入 `Assets/Doc/OPEN_QUESTIONS.md`。

---

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Project_context.md
@Assets/Doc/02_SYSTEM_SPEC.md
@Assets/Doc/技术文档/场景相关/Village_HomeScene23_可玩民居室内_技术说明.md
@Assets/Doc/技术文档/演出相关/NormalDialogueNewPanel_遮罩立绘对话头像_Prefab技术说明.md
@Assets/Doc/执行文档/6月/0601/Village_HomeScene23_屋内NPC对白台本_执行说明.md
@Assets/GameRes/Scenes/Village_HomeScene23.unity
@Assets/GameRes/Prefabs/Dialogue/Village_QuestOffer_NPC23.prefab
@Assets/Dialog/Village_NPC23椅子孩子第一天对话.csv
@Assets/Scripts/Game/GameRuntime/Entities/SceneEntities/Village_House/Npc23QuestStoryTrigger.cs
@Assets/GameRes/Prefabs/UI/NormalDialogueNewPanel.prefab

你现在是【架构侦探】。Unity 2020.3.48f1 / C#。
禁止修改任何代码、Prefab、场景、CSV。只读扫描 + 写溯源报告。

报告写入：
Assets/Doc/执行文档/0920/Village_HomeScene23_对话不要大立绘_架构溯源报告.md

---

## 背景（策划白话）

Village_HomeScene23 是肯姆尼村的民居室内。
这里的对话不需要大立绘。

用户截图：对话已经打开，第一句是「妈妈，有外人！」（说话的是屋里的孩子，不是雅儿），屏幕左侧仍然有雅儿大半身立绘，挡着屋子，也压在对话框上。

要的效果：
- 这个场景里点出来的对话，都不要这张大立绘
- 对话框和台词还在
- 对话框里的小头像还在（没说要去掉）
- 别的场景本来要大立绘的，不要跟着改没

---

## 必读 / 优先扫描

### A. 先分清亮的是哪一张图

必须写成一张对照，不要把大立绘和小头像当成同一个开关：

| | 大立绘 | 小头像 |
|--|--------|--------|
| 玩家看到 | 左侧大半身 | 对话框左下角小格 |
| 本期 | 关掉 | 保留 |

写出截图里那张雅儿是：
1. 对话 Prefab 里单独的立绘物体（CanvasGroup / Painting），还是
2. NormalDialogueNewPanel 里 Mask 没裁住、原图漏出来

判据写在报告里（节点名、父节点、有没有 Mask、第一句说话人是谁）。

### B. 这个场景会播哪些对话

从 `Village_HomeScene23.unity` 把所有会 `TriggerStory` / 填写了剧情 Prefab 名的物体列全。
已知线索（须复核，不要漏接任务后换上的那份）：

- `StoryPrefabName: Village_QuestOffer_NPC23`
- `StoryPrefabName: HomeScene1Npc1`
- `Npc23QuestStoryTrigger` 按任务状态还会加载哪些 Prefab

每份对话回答：会不会出大立绘、出的是哪一个物体、要不要改。
全路径列表。不要只写「室内对话」四个字。

### C. 为什么第一句不是雅儿，雅儿已经在

台本 `Village_NPC23椅子孩子第一天对话.csv` 第 1 句 Speaker 是 `2`，雅在第 6 句。
若大立绘在第 1 句就在，找出是开场淡入、Prefab 默认显示，还是面板默认显示。
不要把原因写成「雅儿说话所以出立绘」，除非证据表明第 1 句时雅儿节点还没开、截图是后几句。

### D. 推荐一种最小改法

必须同时满足：

1. 本场景每段对话从打开到结束都没有大立绘
2. 小头像、字幕、点继续都还在
3. 不重导、不覆盖整张对话图，除非报告证明手改节点重导会丢，并写出不丢的改法
4. 村开场、村长家、商店的大立绘不变

只推一种。另外两种各用一句话否决（例如「只藏第一句」「关掉整个 NormalDialogueNewPanel 的立绘根」）。

若 HomeScene1Npc1 其实是龙宫对话、不该在本场景播，单独写一行，不要把它的立绘规则改掉来凑这场。

---

## 报告结构（固定）

① 结论一句话（大立绘是哪个物体 + 改 Prefab 还是改共用代码）  
② 原因（大白话 + 从点 NPC 到立绘出现的调用链）  
③ 用户需要做什么（进 HomeScene23，点截图这场，再点本场景另一个 NPC）  
④ 给施工员的补充：文件表（全路径、改什么、不要改什么）、推荐方案、否决方案、会误伤的其它场景（没有就写无）

发现设计说不清的，追加到 `Assets/Doc/OPEN_QUESTIONS.md`，不要改核心设计。
```

---

## 施工员提示词（侦探报告落盘并确认方案后再贴）

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0920/Village_HomeScene23_对话不要大立绘_架构溯源报告.md
@Assets/GameRes/Scenes/Village_HomeScene23.unity
@Assets/GameRes/Prefabs/Dialogue/Village_QuestOffer_NPC23.prefab

你现在是【施工员】。Unity 2020.3.48f1 / C#。
只按溯源报告列出的文件、推荐方案做最小改。
不要改台词，不要重导 CSV，不要动村开场、村长家、商店的大立绘。

目标：
- Village_HomeScene23 里播出的对话都不再出现大立绘
- 对话框、台词、小头像仍在
- 报告写明不要改的 Prefab，保持原样

限制：
- 禁止在 Update 里堆显隐逻辑
- 不要把 NormalDialogueNewPanel 的立绘根整个关掉，除非报告证明这场漏出的就是它，并且改法不会影响其它场景
- 施工说明写入：`Assets/Doc/施工说明/0920/Village_HomeScene23_对话不要大立绘_施工说明.md`
- 没有新架构就不要新造技术文档

完成后用大白话给验收清单：进这个屋子，点「妈妈，有外人！」那场，再点另一个 NPC，确认左侧没有大立绘，对话框还在。
```
