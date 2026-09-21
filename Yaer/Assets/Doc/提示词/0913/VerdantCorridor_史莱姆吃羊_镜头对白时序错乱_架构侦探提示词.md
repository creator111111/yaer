# Cursor Agent Prompt · 苍翠走廊史莱姆吃羊：镜头与对白时序乱（对齐东城郊）

> **角色**：先【架构侦探】只读对照核实；根因拍板后再【施工员】只改走廊对话图连线/节点序  
> **日期**：2026-09-13  
> **现象（用户实测）**：`VerdantCorridor` 的 `SlimeEatSheepStoryTrigger` 演出 **完全不对**——**摄像机移动与对话出现时机乱了**；用户指定以 `ForestEastScene` 同名触发器为 **正确基准** 严肃对比  
> **产品期望（钉死）**：走廊这段演出的 **镜头↔台词先后顺序** 与东城郊 `ForestEastSceneSlimeEatSheep` **同构**（第一句 → 推镜看羊/史莱姆 → 停留 → 镜头回玩家 → 再跟后续台词 → 黑幕刷怪）；走廊仍用 `SlimeEatSheepStoryAction2` / Mgr2  
> **不是**：改东城郊 Prefab/场景当实验场不还原；重做对话 UI；改 `CameraMoveTaskAction` 全局实现；把走廊 `StoryPrefabName` 改成东城郊名（会打错 Mgr1）  
> **基准场景物体**：`ForestEastScene/Objects/SlimeEatSheepStoryTrigger` → `StoryPrefabName=ForestEastSceneSlimeEatSheep`  
> **问题场景物体**：`VerdantCorridor/Objects/SlimeEatSheepStoryTrigger` → `StoryPrefabName=VerdantCorridorSlimeEatSheep`  
> **报告落盘**：`Assets/Doc/执行文档/0913/VerdantCorridor_史莱姆吃羊_镜头对白时序错乱_架构溯源报告.md`  
> **与前案关系**：同日「无对白」提示词（`VerdantCorridor_史莱姆吃羊无对白_架构侦探提示词.md`）针对 **台词缺失**；现网走廊 Prefab **已有 Statement**，本案主诉升级为 **connections 时序错乱**。侦探须以本案为准，勿只报「有台词就算好」。

把下面「侦探」整段复制给 Cursor Agent（Agent Mode）。施工 Prompt 见文末。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 产品白话

> 东城郊那段：先说一句「那是……」，镜头再推过去看史莱姆吃羊，停一下，镜头回来，再继续骂/决心台词，最后黑幕刷怪。  
> 苍翠走廊现在镜头和对白对不上号，顺序乱了。请严格按东城郊的先后对齐，别自创节奏。

### 权威时序（ForestEast 金标准 · 来自 Prefab connections）

`ForestEastSceneSlimeEatSheep.prefab` 连线：`0→1→2→3→4→5→6→7→8→9→10→11→12→13`

| 步 | 节点 | 类型 | 内容 / 参数 |
|----|------|------|-------------|
| 0 | id0 | ActionList | Find `SlimeEatSheepStoryTrigger`；`CameraPos1`=玩家；子物体 `CameraPos2`；UI 淡入 0.7s |
| 1 | id1 | **Statement** | 雅尔 Face=1「那是…………」audio1 — **推镜前** |
| 2 | id2 | **CameraMove** | Pos1→Pos2，Duration=1 |
| 3 | id3 | Wait | 1.0s（停在吃羊镜头） |
| 4 | id4 | **CameraMove** | Pos2→Pos1，Duration=1 |
| 5 | id5 | CameraFollow | `isFollowPlayer=true` |
| 6 | id6 | Statement | 雅尔 Face=8「它们居然在吃其他生物！」audio2 |
| 7 | id7 | Statement | 旁白（恶寒/吸食尸体） |
| 8 | id8 | Statement | 旁白「同时她也意识到这个生物十分危险」 |
| 9 | id9 | Statement | 雅尔 Face=8「在龙城郊外居然有这么危险的东西。」audio3 |
| 10 | id10 | Statement | 雅尔 Face=8「想过去必须要消灭他们！」audio4 |
| 11 | id11 | BlackMask | 黑幕 |
| 12 | id12 | StoryAction | `SlimeEatSheepStoryAction` + `"start"`（东城郊 Mgr1） |
| 13 | id13 | ActionList | UI 收 + 黑幕收 |

**节奏口诀（钉死）**：

```
第一句（玩家镜头）→ 推到 CameraPos2 → 停 1s → 拉回玩家 → Follow
→ 其余台词 → 黑幕 → start 刷怪 → 收场
```

> 规范依据：`02_SYSTEM_SPEC` — 镜头用 `CameraComponent`/`CameraMove` 的完成回调衔接，**禁止**用乱序连线硬凑。

### 走廊现网时序（助手预扫 · 已解析 connections · 须再核实）

`VerdantCorridorSlimeEatSheep.prefab` 连线：`0→1→2→8→9→10→11→12→13→3→4→5→6→7`

| 步 | 节点 | 类型 | 相对金标准的问题 |
|----|------|------|------------------|
| 0 | id0 | ActionList | 同构 OK |
| 1 | id1 | **CameraMove Pos1→Pos2** | ❌ **抢在第一句之前**（金标准此时应是 Statement「那是……」） |
| 2 | id2 | Wait 1s | ❌ 仍无第一句，已在看点停住 |
| 3 | id8 | Statement「那是。。。。」 | ❌ 出现在 **推镜+Wait 之后**（金标准在推镜前） |
| 4–8 | id9…13 | 其余 5 句台词 | ❌ 整段落在 **镜头尚未拉回** 之时（金标准在 Follow 之后） |
| 9 | id3 | CameraMove Pos2→Pos1 | ❌ 被挪到 **全部台词之后** |
| 10 | id4 | CameraFollow | 同上偏晚 |
| 11 | id5 | BlackMask | |
| 12 | id6 | **Action2** `"start"` | ✅ 事件类型正确（须保持，勿改 Action1） |
| 13 | id7 | 收场 ActionList | OK |

**体感对照**：

| 时刻 | 东城郊（对） | 走廊（错） |
|------|--------------|------------|
| 开场 | 玩家镜头说「那是……」再推镜 | 立刻推镜，第一句晚出 |
| 看点停留 | 无对白，纯看 1s | Wait 后才开始全套对白 |
| 决心台词 | 镜头已回玩家再说 | 镜头还停在羊/史莱姆处就说完 |
| 拉回镜头 | 在后续台词 **之前** | 在后续台词 **之后** |

> 生活类比：东城郊是「先惊呼一声 → 镜头给特写 → 看完再回来吐槽」；走廊变成「先推镜头 → 停着把所有话讲完 → 再把镜头拽回来」。

### 场景侧（非主因 · 须核对）

| 项 | ForestEast | VerdantCorridor |
|----|------------|-----------------|
| 触发器名 | `SlimeEatSheepStoryTrigger` | 同名 |
| StoryPrefabName | `ForestEastSceneSlimeEatSheep` | `VerdantCorridorSlimeEatSheep` |
| 故事脚本 | `SlimeEatSheepStroy` + Mgr1 | `SlimeEatSheepStroy2` + Mgr2 |
| 子物体 CameraPos2 | local x≈**22** | local x≈**20.22**（场景摆位可不同，**不解释时序乱**） |
| triggerType | Enter + SingleUse | 同 |

镜头节点语义：`CameraMoveTaskAction` 用 DOMove 等到 `Duration` 完成再 `EndAction`——时序完全由 **图连线** 决定，不是代码随机。

### 嫌疑优先级

| # | 嫌疑 | 裁定倾向 |
|---|------|----------|
| **A. Prefab connections 顺序错（主）** | 走廊把「推镜块」整段插到第一句前，又把「拉回+Follow」整段挪到全台词后 | **主因** |
| **B. 节点在但未接入主链** | 旧案曾无台词；现有台词但接线错 | 与 A 叠加史 |
| **C. CameraPos2 摆位** | 只影响拍到什么，不改先后 | 排除为时序主因 |
| **D. 存档 SingleUse / 触发没开火** | 会「整段不播」；用户说的是播了但乱 | 次要排查 |
| **E. Action2 vs Action1** | 走廊用 Action2 正确；对齐时序时 **禁止**改成 Action1 | 约束 |

### 侦探须回答

1. 用 connections 画出两边 **完整播放序表**（逐步：镜头在哪、哪句台词）。是否与上表一致？  
2. 乱序断点是 **仅 connections**，还是还有节点参数（Duration/Wait/Face/audio）差异？  
3. 走廊台词文案/audio 引用是否已与东城郊对齐？（预扫文案已齐；audio 槽位须核 `_boundGraphObjectReferences`）  
4. 修复是否 **只重接连线** 即可达到金标准？还是要删/挪节点？  
5. **方案 ≥2**：  
   - **A（推荐）**：改 `VerdantCorridorSlimeEatSheep` 的 connections（及必要时节点 id 排布），使序 = 金标准，**仅**把 id12 位的事件保持为 `SlimeEatSheepStoryAction2`  
   - **B**：以东城郊 Prefab 为模板复制图，再替换 StoryAction→Action2、核对 Blackboard/场景名  
   - **C**：改代码插 Delay 硬凑（**禁止**）  
   列利弊 + 验收标准。

### 必读 / 扫描

**必读**

1. `Assets/Project_context.md`  
2. `Assets/Doc/02_SYSTEM_SPEC.md`（摄像机 onComplete、禁止 Wait 硬匹配——本案 Wait 是图内设计节点，对齐东城郊即可）  
3. 本提示词「权威时序」表  
4. `ForestEastSceneSlimeEatSheep.prefab`（金标准）  
5. `VerdantCorridorSlimeEatSheep.prefab`（问题图）  
6. `CameraMoveTaskAction.cs`  
7. 场景两侧 `SlimeEatSheepStoryTrigger` 的 `CameraPos2` 子物体  

**对照 Play（建议）**

- 东城郊走一次：记录「第几秒出第一句 / 何时推镜 / 何时回镜 / 何时出第二句」  
- 走廊走一次：同一检查表填差异  

### 禁止

- 本阶段禁止改代码 / Prefab / 场景 / Git（仅侦探）  
- 禁止改东城郊金标准图「迁就」走廊  
- 禁止走廊改用 `ForestEastSceneSlimeEatSheep` 文件名或 `SlimeEatSheepStoryAction`（Mgr1）  
- 禁止在 `Update` 或 C# 里堆镜头延迟补丁  

---

## 【架构侦探】任务（整段复制执行）

你是【架构侦探】。Unity **2020.3.48f1** / C#。只读；**禁止**改代码、Prefab、场景、Git。

### 输出

`Assets/Doc/执行文档/0913/VerdantCorridor_史莱姆吃羊_镜头对白时序错乱_架构溯源报告.md`  
（无 `0913` 目录则创建。）

固定结构：

1. **结论一句话**（时序为何乱；是否仅 connections）  
2. **双边播放序对照表**（逐步：镜头位置 vs 台词）  
3. **证据表**（connections 原文摘要、节点类型、已证实/排除）  
4. **与「无对白」前案关系**（现网是否已有台词）  
5. **修复方案 ≥2** + 推荐 + 回归（Action2、刷怪、SingleUse、CameraPos2）+ 验收  
6. 不清处记 `Assets/Doc/OPEN_QUESTIONS.md`

### 限制

- 不改代码；不提交  
- 默认中文；大白话 + 路径/字段  

---

## 【施工员】Prompt（侦探报告拍板后再复制）

> **前置**：溯源报告确认走廊图时序错，金标准为东城郊序。  
> **目标**：`VerdantCorridorSlimeEatSheep` 播放序与东城郊同构：  
> `Setup →「那是……」→ 推镜 Pos2 → Wait1 → 拉回 Pos1 → Follow → 其余台词 → 黑幕 → Action2("start") → 收场`  
> **优先**：只改该对话 Prefab 的 **connections**（必要时微调节点坐标便于编辑）；保持 `SlimeEatSheepStoryAction2`；保持 audio/Face/文案除非报告要求。  
> **禁止**：改东城郊 Prefab；改 Action2→Action1；改 `StoryPrefabName`；改 `CameraMoveTaskAction` 源码；用 C# Wait 硬匹配；在 Update 堆逻辑。  
> **文档**：`Assets/Doc/施工说明/0913/VerdantCorridor_史莱姆吃羊_镜头对白时序对齐_施工说明.md`  
> **验收**：  
> 1）走廊 Play：第一句出现时镜头仍在玩家侧，**尚未**推到 CameraPos2；  
> 2）推镜 → 停约 1s **无新对白**；  
> 3）拉回并 Follow 后，才出现「它们居然在吃…」及后续句；  
> 4）黑幕后史莱姆按 Mgr2 刷出；  
> 5）东城郊同触发器抽测未回归；  
> 6）同存档二次进入仍受 SingleUse 约束。  

---

## 【验收员】Prompt（施工后可选）

> Debug 标签 `[SlimeEatSheepCamOrder]`（短生命周期）：在 `CameraMoveTaskAction.OnExecute` 起止、各 Statement 展示时打时间戳与 Start/EndPos 名（若可从对话层钩；否则人工秒表验收即可）。  
> 输出：逐步通过/失败 + 剩余风险（CameraPos2 世界坐标是否拍到羊/史莱姆）。
