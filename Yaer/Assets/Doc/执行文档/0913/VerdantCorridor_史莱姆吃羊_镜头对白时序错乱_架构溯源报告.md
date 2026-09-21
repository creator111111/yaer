# VerdantCorridor 史莱姆吃羊 · 镜头对白时序错乱 — 架构溯源报告

**文档版本**：v1.0（2026-09-13）  
**文档性质**：【架构侦探】只读溯源；**未改**代码 / Prefab / 场景 / Git  
**Unity**：2020.3.48f1  
**现象（用户）**：苍翠走廊 `SlimeEatSheepStoryTrigger` 演出 **镜头与对白先后乱了**；以东城郊同款为正确基准  
**产品期望**：走廊 **镜头↔台词顺序** 与 `ForestEastSceneSlimeEatSheep` **同构**；走廊仍用 `SlimeEatSheepStoryAction2` / Mgr2  
**不是**：改东城郊 Prefab；改 `CameraMoveTaskAction` 源码；把走廊 `StoryPrefabName` 改成东城郊名；用 C# Wait 硬凑  
**提示词**：`Assets/Doc/提示词/0913/VerdantCorridor_史莱姆吃羊_镜头对白时序错乱_架构侦探提示词.md`  
**前案（无对白）**：`Assets/Doc/执行文档/0913/VerdantCorridor_史莱姆吃羊无对白_架构溯源报告.md`  
**前案施工**：`Assets/Doc/施工说明/0913/VerdantCorridor_史莱姆吃羊补对白_施工说明.md`  
**施工说明**：`Assets/Doc/施工说明/0913/VerdantCorridor_史莱姆吃羊_镜头对白时序对齐_施工说明.md`（2026-09-13 已按方案 A 落地）

---

## 沟通摘要

### ① 结论一句话

**台词已经有了，乱的是连线**：走廊图把「推镜 + 停 1 秒」插在第一句前面，又把「拉回 + Follow」挪到 **6 句全说完之后**；东城郊是「先说『那是……』再推镜、看完拉回、再吐槽」。时序完全由 NodeCanvas **connections** 决定，镜头代码没有随机。

### ② 原因（通俗）

东城郊像：先在玩家这边惊呼一声 → 镜头给吃羊特写停一下 → 拉回来 → 再骂、再下决心 → 黑幕刷怪。  
走廊现在像：一进门镜头先冲过去，停在羊尸那边把所有话讲完，再把镜头拽回来。  
这是同日「补对白」施工把 6 句接在「Cam→Pos2 → Wait」后面造成的（施工说明里还写成「定格那边播对话」），**和东城郊金标准相反**。不是摄像机脚本坏了，也不是 CameraPos2 摆远了。

### ③ 用户需要做什么（检查清单）

| # | 操作 | 现网预期（未修） |
|---|------|------------------|
| 1 | 新档走进走廊触发器 | 立刻推镜；**第一句出现时镜头已在羊/史莱姆处** |
| 2 | 推镜后停约 1s | 停完才开始全套 6 句（镜头仍在 Pos2） |
| 3 | 「它们居然在吃…」及后续 | 镜头 **尚未** 拉回玩家 |
| 4 | 全部台词结束后 | 才拉回 + Follow → 黑幕 → 刷怪 |
| 5 | 对照东城郊同触发器 | 第一句时镜头仍在玩家侧，再推镜 |
| 6 | 拍板后施工：只改走廊 Prefab **connections** | 对齐金标准；**保住 Action2** |

### ④ 程序补充

见下文 §1～§6。

---

## 1. 结论（程序向）

| 项 | 裁定 |
|----|------|
| **断点** | `VerdantCorridorSlimeEatSheep.prefab` 的 **connections**，不是缺台词、不是 Load 失败 |
| **金标准序** | `0→1→2→3→4→5→6→7→8→9→10→11→12→13`（东城郊 id 连续） |
| **走廊现网序** | `0→1→2→8→9→10→11→12→13→3→4→5→6→7` |
| **节点本身** | 走廊 6× Statement、两段 CameraMove、Wait=1、Follow、BlackMask、**Action2("start")** 都在；**不必删节点** |
| **参数** | 推/拉 Duration=1、Wait=1、UI 淡入 0.7，与东城郊同量级；**不解释乱序** |
| **镜头代码** | `CameraMoveTaskAction`：`DOMove` 完成才 `EndAction`；先后只由图连线决定 |
| **前案关系** | 「无对白」已结：现网 **已有 6 句**；本案是补台词时接线做错（含一次「定格播对话」的二次改线） |
| **推荐** | **方案 A**：只改 connections（必要时挪节点坐标方便看图）；**禁止**改 Action2→Action1 |

---

## 2. 双边播放序对照表

**节奏口诀（钉死）**：

```
第一句（玩家镜头）→ 推到 CameraPos2 → 停 1s（无新对白）→ 拉回玩家 → Follow
→ 其余台词 → 黑幕 → start 刷怪 → 收场
```

### 2.1 东城郊（金标准）`ForestEastSceneSlimeEatSheep`

连线：`0→1→2→3→4→5→6→7→8→9→10→11→12→13`

| 播放步 | id | 类型 | 镜头在哪 | 台词 |
|--------|----|------|----------|------|
| 1 | 0 | ActionList | 找 Trigger / CameraPos1=玩家 / CameraPos2；UI 淡入 0.7s | 无 |
| 2 | 1 | Statement | **仍在玩家侧** | 雅尔 Face=1「那是…………」audio=1 |
| 3 | 2 | CameraMove | Pos1→Pos2，Duration=1 | 无 |
| 4 | 3 | Wait | 停在 Pos2 | **无新对白**，1.0s |
| 5 | 4 | CameraMove | Pos2→Pos1，Duration=1 | 无 |
| 6 | 5 | CameraFollow | `isFollowPlayer=true` | 无 |
| 7 | 6 | Statement | 已跟玩家 | 「它们居然在吃其他生物！」Face=8 audio=2 |
| 8 | 7 | Statement | 同上 | 旁白（恶寒/吸食尸体） |
| 9 | 8 | Statement | 同上 | 旁白「同时她也意识到…危险」 |
| 10 | 9 | Statement | 同上 | 「在龙城郊外居然有这么危险的东西。」Face=8 audio=3 |
| 11 | 10 | Statement | 同上 | 「想过去必须要消灭他们！」Face=8 audio=4 |
| 12 | 11 | BlackMask | — | — |
| 13 | 12 | **StoryAction** | — | `"start"` → **Mgr1** |
| 14 | 13 | ActionList | — | UI 收 + 黑幕收 |

### 2.2 走廊（错）`VerdantCorridorSlimeEatSheep`

连线：`0→1→2→8→9→10→11→12→13→3→4→5→6→7`

| 播放步 | id | 类型 | 镜头在哪 | 相对金标准 |
|--------|----|------|----------|------------|
| 1 | 0 | ActionList | 同构 Setup | OK |
| 2 | 1 | CameraMove Pos1→Pos2 Dur=1 | **立刻推走** | ❌ 金标准此时应是第一句 |
| 3 | 2 | Wait 1s | 已在看点 | ❌ 仍无第一句 |
| 4 | 8 | Statement「那是。。。。」Face=1 audio=1 | **已在 Pos2** | ❌ 应在推镜前 |
| 5–9 | 9…13 | 其余 5 句 | 仍在 Pos2 | ❌ 金标准在 Follow 之后 |
| 10 | 3 | CameraMove Pos2→Pos1 Dur=1 | 台词已说完才拉回 | ❌ 应在后续台词之前 |
| 11 | 4 | CameraFollow | 偏晚 | ❌ |
| 12 | 5 | BlackMask | | 位置相对「刷怪」仍对 |
| 13 | 6 | **Action2** `"start"` | | ✅ 事件类型正确，**须保持** |
| 14 | 7 | 收场 ActionList | | OK |

**体感对照**

| 时刻 | 东城郊（对） | 走廊（错） |
|------|--------------|------------|
| 开场 | 玩家镜头说「那是……」再推镜 | 立刻推镜，第一句晚出 |
| 看点停留 | 无对白，纯看 1s | Wait 后才开始全套对白 |
| 决心台词 | 镜头已回玩家再说 | 镜头还停在羊/史莱姆处就说完 |
| 拉回镜头 | 在后续台词 **之前** | 在后续台词 **之后** |

### 2.3 走廊节点清单（现网已齐，只接线错）

| id | 类型 | 要点 |
|----|------|------|
| 0 | ActionList | Find Trigger + Pos1/Pos2 + UI 淡入 0.7 |
| 1 | CameraMove | Start=Pos1 End=Pos2 Duration=1 |
| 2 | Wait | waitTime=1 |
| 3 | CameraMove | Start=Pos2 End=Pos1 Duration=1 |
| 4 | CameraFollow | isFollowPlayer=true |
| 5 | BlackMask | Duration=1 |
| 6 | **SlimeEatSheepStoryAction2** | `"start"` |
| 7 | ActionList | UI 收 + 黑幕收 |
| 8 | Statement | 「那是。。。。」雅尔 Face=1 audio=1 |
| 9 | Statement | 「它们居然在吃其他生物！」Face=8 audio=2 |
| 10 | Statement | 旁白恶寒（无 audio） |
| 11 | Statement | 旁白危险（无 audio） |
| 12 | Statement | 「在龙城郊外…」Face=8 audio=3 |
| 13 | Statement | 「想过去必须要消灭他们！」Face=8 audio=4 |

---

## 3. 证据表

| # | 证据 | 结论 | 状态 |
|---|------|------|------|
| E1 | 走廊 connections 原文：`0→1→2→8→…→13→3→4→5→6→7` | 推镜块在台词前，拉回块在台词后 | ✅ |
| E2 | 东城郊 connections：`0→1→…→13` 连续 | 第一句 id1 在 CameraMove id2 之前 | ✅ |
| E3 | 走廊 6× `StatementNodeEx` 文案/Face/audio 槽与东城郊同套 | **有台词**；本案不是「无对白」 | ✅ |
| E4 | `_boundGraphObjectReferences` audio guid 四条与东城郊相同 | 语音槽位已对齐；非时序主因 | ✅ |
| E5 | `CameraMoveTaskAction.Move`：`DOMove` 完才 `EndAction` | 时序 = 图连线，不是 C# 乱序 | ✅ |
| E6 | 推/拉 Duration=1、Wait=1，两边同 | **排除**参数差为乱序主因 | ✅ |
| E7 | `CameraPos2`：东城郊 local x≈**22**；走廊 ≈**20.22** | 只影响拍到什么，不改先后 | ✅ 排除时序主因 |
| E8 | 走廊 id6 仍是 `SlimeEatSheepStoryAction2` | 对齐时序时 **禁止**改成 Action1 | ✅ 约束 |
| E9 | 前案施工说明节点链：`Cam→Pos2 → Wait → 8~13 台词 → Cam回` | **现网错序就是这次接线** | ✅ |
| E10 | 施工说明「时机修正」：故意改成定格 Pos2 再说话 | 与产品现钉的东城郊金标准 **冲突** | ✅ 根因史 |
| E11 | Play 秒表（第几秒出句 / 推镜） | 侦探未实机；静态连线已足够定性 | ⚠️ 待验收 Play |

### 嫌疑复核

| 嫌疑 | 裁定 |
|------|------|
| **A connections 顺序错** | **主因 · 已证实** |
| **B 节点在但未接入主链** | 现网 14 节点均在主链上；是 **顺序错** 不是孤儿节点 |
| **C CameraPos2 摆位** | **排除为时序主因** |
| **D SingleUse / 触发没开火** | 用户是「播了但乱」；排除为主因 |
| **E Action2 vs Action1** | 走廊 Action2 **正确**；对齐时勿改 |

---

## 4. 与「无对白」前案关系

| 案 | 主诉 | 现网 |
|----|------|------|
| 0913 上午「无对白」 | Prefab **0× Statement** | 已施工补 6 句 |
| **本案** | 有对白但 **镜头↔台词先后乱** | connections = `0→1→2→8…13→3…7` |

前案 OPEN Q2「台词插在镜头后、黑幕前」被落实成 **整段镜头（含推镜+Wait）之后再说话**，后又改成「定格 Pos2 播完再拉回」。  
产品现钉死：**第一句在推镜前；其余句在拉回+Follow 之后**。本案 **覆盖** 前案 Q2 的施工默认，不要再按「定格那边说话」修回去。

---

## 5. 修复方案对比

目标播放序（走廊 **id 保持不变**，只改连线）：

```
0 Setup
→ 8 「那是……」
→ 1 Cam Pos1→Pos2
→ 2 Wait 1s
→ 3 Cam Pos2→Pos1
→ 4 Follow
→ 9→10→11→12→13 其余台词
→ 5 黑幕
→ 6 Action2("start")
→ 7 收场
```

即 connections 改为：

`0→8→1→2→3→4→9→10→11→12→13→5→6→7`

### 方案 A（推荐 · 最小）

只改 `VerdantCorridorSlimeEatSheep.prefab` 的 **connections**（NodeCanvas 重接即可）；可选挪 `_position` 让画布从左到右好读。不改文案、audio、Action2、场景。

| 利 | 弊 |
|----|-----|
| 改动面单文件、单字段组；节点可复用 | 手工接线仍可能接错，须按上表验收 |
| 刷怪仍走 Mgr2 | 画布上节点 id 不连续（8 插在 0 后），编辑时别按 id 数字顺序点 |

### 方案 B

以东城郊 Prefab 为模板复制整图，再把 id12 位事件改成 Action2，核 Blackboard / 演员引用。

| 利 | 弊 |
|----|-----|
| 连线天然同构 | 易误带 `SlimeEatSheepStoryAction`（Mgr1）；比 A 面大 |

### 方案 C（禁止）

在 `CameraMoveTaskAction` 或 C# 里插 Delay / 改 Update 硬凑。违反 `02_SYSTEM_SPEC`（镜头用完成回调衔接），且会污染全局镜头任务。

### 推荐与禁止

| 项 | 内容 |
|----|------|
| **推荐** | **方案 A** |
| **禁止** | 方案 C；改东城郊金标准图；Action2→Action1；改 `StoryPrefabName`；改 `CameraMoveTaskAction` 源码 |

### 回归与验收

| 风险 | 验收 |
|------|------|
| 又接成「Pos2 定格说话」 | 第一句出现时镜头 **尚未** 到 CameraPos2 |
| Wait 段出第二句 | 推镜后约 1s **无新对白** |
| 拉回太晚 | Follow 之后才出「它们居然在吃…」 |
| 打错 Mgr | 黑幕后走廊史莱姆出现（Action2 / Mgr2） |
| 东城郊误伤 | 抽测东城郊序未变 |
| SingleUse | 同存档二次进入不重播 |
| CameraPos2 | 推过去应能看到羊/吃羊演出；摆位偏差另案，不在本期改场景 |

**验收标准（施工后）**

1. 走廊 Play：第一句时镜头仍在玩家侧，**尚未**推到 CameraPos2。  
2. 推镜 → 停约 1s **无新对白**。  
3. 拉回并 Follow 后，才出现「它们居然在吃…」及后续句。  
4. 黑幕后史莱姆按 Mgr2 刷出。  
5. 东城郊同触发器抽测未回归。  
6. 同存档二次进入仍受 SingleUse 约束。

---

## 6. OPEN_QUESTIONS

已记入 `Assets/Doc/OPEN_QUESTIONS.md`：

| ID | 问题 | 施工默认 | 状态 |
|----|------|----------|------|
| Q1 | 是否以东城郊 connections 为唯一金标准（覆盖前案「定格 Pos2 说话」）？ | **是** | 待确认 |
| Q2 | 是否只改 connections、不改 CameraPos2 场景坐标？ | **是**（摆位另案） | 待确认 |
| Q3 | 第一句标点「那是。。。。」是否改成省略号「…………」？ | **本期否**（与东城郊 Prefab 实际 `_text` 同为四个「。」） | 待确认 |

---

## 7. 给施工员的一句话

**只重接走廊对话图：`0→8→1→2→3→4→9…13→5→6→7`；保住 Action2；不要再按「镜头先推过去再说话」修。**
