# Cursor Agent Prompt · 苍翠走廊史莱姆吃羊：走进触发器无对白

> **角色**：先【架构侦探】只读核实；根因拍板后再【施工员】最小化补台词 / 接线  
> **日期**：2026-09-13  
> **现象（用户实测）**：`VerdantCorridor` → Hierarchy `Objects/SlimeEatSheepStoryTrigger`；走进去 **不显示对话**；用户体感「对话文件丢了」  
> **产品期望（钉死）**：走进触发器后播放完整对白（有对话框/立绘/台词），播完再走现有 `SlimeEatSheepStoryAction2("start")` 刷史莱姆战斗；死羊/坟墓后续链保持可用  
> **不是**：改东城郊 `ForestEastSceneSlimeEatSheep` 主流程（可只读对照）；重做整套对话系统；改村庄对白；顺手大改其它 VerdantCorridor 触发器  
> **场景**：`Assets/GameRes/Scenes/VerdantCorridor.unity`（编辑器里可能挂在 InitScene 子场景下，以 Hierarchy 名 `VerdantCorridor` 为准）  
> **报告落盘**：`Assets/Doc/执行文档/0913/VerdantCorridor_史莱姆吃羊无对白_架构溯源报告.md`

把下面「侦探」整段复制给 Cursor Agent（Agent Mode）执行。施工 Prompt 见文末（根因拍板后再用）。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 产品白话

> 苍翠走廊走到「史莱姆吃羊」触发器，以前/应该有对白，现在走过去什么话都没有。  
> 人觉得对话文件丢了。要查清是 **文件真没了、触发没开火、还是 Prefab 在但台词节点被掏空**；再最小补回，别修出新 bug。

### 术语钉死

| 词 | 本案含义 |
|----|----------|
| **对话文件** | NodeCanvas 对话 Prefab：`GameRes/Prefabs/Dialogue/{StoryPrefabName}.prefab`，不是 CSV 台本（本案 Dialog 目录无同名源） |
| **不显示对话** | 无 `StatementNodeEx` 对话框/立绘台词；**仅有镜头/黑幕/刷怪**也算「没对白」 |
| **触发器** | 场景物体 `SlimeEatSheepStoryTrigger`（`SimpleStoryTrigger` + `SlimeEatSheepStroy2`） |
| **东城郊同款** | `ForestEastScene` 的同名触发器 + `ForestEastSceneSlimeEatSheep`（有完整台词，可对照） |

### 现网接线（助手预扫 · 须核实）

```
VerdantCorridor.unity
  Objects/SlimeEatSheepStoryTrigger
    SimpleStoryTrigger
      StoryPrefabName = "VerdantCorridorSlimeEatSheep"
      SingleUseInArchive = 1
      triggerType = Enter (1)
    SlimeEatSheepStroy2  （苍翠走廊版场景故事物体）
    BoxCollider2D Trigger + CldInteractiveListener + InteractiveComponent

走进 → onEnterInteractiveEvent
  → SimpleStoryTrigger.TriggerStory()
  → StoryComponentGSM.TriggerStory("VerdantCorridorSlimeEatSheep")
  → ResMgr.LoadAsset(DialoguePath = Assets/GameRes/Prefabs/Dialogue/VerdantCorridorSlimeEatSheep.prefab)
  → NormalDialogueNewPanel.StartDialogue(go)
```

**Prefab 是否存在**：✅ 文件在盘上（不是 Missing 路径名）。  
**图内容（预扫）**：

| Prefab | 台词 StatementNodeEx | 收尾事件 |
|--------|----------------------|----------|
| `ForestEastSceneSlimeEatSheep` | ✅ 多句雅尔/旁白 | `SlimeEatSheepStoryAction` + `"start"` |
| `VerdantCorridorSlimeEatSheep` | ❌ **0 句** | `SlimeEatSheepStoryAction2` + `"start"` ✅ |

苍翠走廊现网节点链（预扫）：

```
0 Find+UI淡入 → 1 镜头到 CameraPos2 → 2 Wait(1) → 3 镜头回
→ 4 CameraFollow → 5 黑幕 → 6 Action2("start") → 7 UI/黑幕收
```

→ **主嫌疑：对话内容被掏空（或从未从东城郊拷全），不是「磁盘删文件」。**

### 东城郊台词对照（施工参考 · 产品须确认是否照搬）

`ForestEastSceneSlimeEatSheep` 现有中文台词顺序（摘要）：

1. 雅尔：「那是…………」  
2. 雅尔：（吃其它动物相关句，见图内 `_text`）  
3. 旁白：（恶心/吸食尸体描写）  
4. 旁白：「同时她也意识到这个生物十分危险」  
5. 雅尔：「在龙城郊外居然有这么危险的东西。」  
6. 雅尔：「想过去必须要消灭他们！」  
→ 黑幕 → `start` 刷怪  

> 苍翠走廊文案是否与东城郊完全相同、还是另有台本：侦探记入 OPEN_QUESTIONS；**施工默认**在无甲方新台本时，**结构对齐东城郊台词节点**，事件节点 **必须保持 Action2**（Mgr2），禁止改成 Action（Mgr1）。

### 嫌疑优先级（可推翻）

| # | 嫌疑 | 若成立的体感 |
|---|------|--------------|
| **A. Prefab 图无台词（主嫌疑）** | `VerdantCorridorSlimeEatSheep` 无 `StatementNodeEx` | 走过去无对话框；可能仍有镜头/黑幕/刷怪 |
| **B. 存档一次性已用过** | `SingleUseInArchive` + `StoryTriggerCountData` 已记 `VerdantCorridorSlimeEatSheep` | 组件 `enabled=false`，连镜头都没有 |
| **C. 触发/交互未开火** | 碰撞层、Interactive 未注册、玩家死亡跳过 | 完全无反应 |
| **D. 加载失败** | AB/路径/`OnStoryPrefabLoad` null | Console Error；无演出 |
| **E. Mgr1/Mgr2 存档键串台（次要）** | `SlimeEatSheepStoryMgr` 与 `Mgr2` 共用 `SlimeEatSheepStory_hasCreateGrave` / `_hasTriggerSlime` | 场景态错（坟/史莱姆已刷），**不解释「无台词节点」** |
| **F. InitSomeEventState 缺走廊名** | switch 只有 `ForestEastSceneSlimeEatSheep`，无 `VerdantCorridorSlimeEatSheep`→Mgr2 | 读档后战斗态不同步；**非「无台词」主因** |

### 复现矩阵（侦探须填）

| 步骤 | 操作 | 期望（现网未修） | 结果 |
|------|------|------------------|------|
| 1 | 新档或清 `VerdantCorridorSlimeEatSheep` 触发计数后进走廊 | 可再次触发 | |
| 2 | 走进 `SlimeEatSheepStoryTrigger` | 有/无对话框？有/无镜头？ | |
| 3 | Hierarchy 选中触发器看 `SimpleStoryTrigger.enabled` | 已用过应为 false | |
| 4 | 打开两份 Dialogue Prefab 对比 Statement 数量 | 东城郊>0；走廊=0 | |
| 5 | Console 有无 Load null / Trigger 拒绝 | | |

**最短路径**：Project 打开 `VerdantCorridorSlimeEatSheep.prefab` → NodeCanvas 看有无台词节点；再 Play 走一次对照体感。

### 侦探须回答

1. 「文件丢了」是哪一种：**路径 Missing** / **Prefab 在但无台词** / **触发被存档关掉**？  
2. 触发链是否仍能 `TriggerStory` 成功？失败点在哪？  
3. 走廊图节点清单 vs 东城郊：差在哪些 Statement？事件是否已是 Action2？  
4. 存档键串台（E）是否影响本案验收？是否单独立项？  
5. **修复方案（≥2）** + 推荐 + 回归风险：  
   - **A**：在走廊 Prefab **补回 StatementNodeEx**（对照东城郊顺序，插在镜头与黑幕/`start` 之间）  
   - **B**：若产品确认走廊=东城郊同文，用编辑器工具从东城郊拷贝台词子图再改 Action→Action2  
   - **C**：仅改 `StoryPrefabName` 指东城郊 Prefab（**不推荐**：会打到 Mgr1，走廊 Mgr2/场景物体错绑）  
   列利弊；禁止方案须写明。

### 必读 / 扫描

**必读**

1. `Assets/Project_context.md`  
2. `Assets/Doc/02_SYSTEM_SPEC.md`（剧情须 NodeCanvas / `StoryComponentGSM.TriggerStory`）  
3. 本提示词预梳理  
4. `SimpleStoryTrigger.cs`（Enter / SingleUse / `InitSomeEventState`）  
5. `StoryComponentGSM.cs`（`TriggerStory` / `OnStoryPrefabLoad`）  
6. `SlimeEatSheepStroy2.cs` / `SlimeEatSheepStoryMgr2.cs` / `SlimeEatSheepStoryAction2.cs`  
7. Prefab：`VerdantCorridorSlimeEatSheep` vs `ForestEastSceneSlimeEatSheep`  
8. 场景：`VerdantCorridor.unity` 中 `SlimeEatSheepStoryTrigger` 序列化字段  

**扫描**

- 同场景其它触发器（`FirstEnterStoryTrigger` 等）是否正常有台词（对照「系统坏了」）  
- Git 历史（只读）：走廊 Prefab 是否曾有 Statement 后被删  
- `Assets/Dialog` 有无对应 CSV（预扫无）  

### 禁止

- **禁止修改代码 / Prefab / 场景 / Git**（本阶段仅侦探）  
- 禁止把走廊 `StoryPrefabName` 直接改成东城郊名应付（打错 Mgr）  
- 禁止为修本案重写对话 UI / PureMVC  
- 禁止未评估就改 Mgr1/Mgr2 共用存档键（可记 OPEN_QUESTIONS，默认另案）  

---

## 【架构侦探】任务（整段复制执行）

你是【架构侦探】。Unity **2020.3.48f1** / C#。只读；**禁止**改代码、Prefab、场景、Git。

### 输出

写到：`Assets/Doc/执行文档/0913/VerdantCorridor_史莱姆吃羊无对白_架构溯源报告.md`  
（若无 `0913` 目录则创建。）

固定结构：

1. **结论一句话**（文件是否丢；无对白的真正断点）  
2. **调用链**（走进 → TriggerStory → Prefab 图 → Action2；标断点）  
3. **证据表**（Prefab 节点清单、场景字段、存档 SingleUse；已证实/可疑/排除）  
4. **与东城郊对照表**  
5. **修复方案对比**（≥2）+ 推荐 + 回归（刷怪、坟墓对白、SingleUse、Action2）+ 验收标准  
6. 设计不清记入 `Assets/Doc/OPEN_QUESTIONS.md`（尤其：走廊是否沿用东城郊原文）

### 限制

- 不改代码；不提交 Git  
- 默认中文；大白话 + 路径/字段名  

---

## 【施工员】Prompt（侦探报告拍板后再复制）

> **前置**：`0913` 溯源报告已确认：走廊 Prefab 无台词（或报告钉死的等价根因）与推荐方案。  
> **目标**：走进 `VerdantCorridor/SlimeEatSheepStoryTrigger` 能看到完整对白，结束后仍走 `SlimeEatSheepStoryAction2("start")` 刷史莱姆；坟墓后续对白仍可用。  
> **优先**：只改 `Assets/GameRes/Prefabs/Dialogue/VerdantCorridorSlimeEatSheep.prefab`（补 `StatementNodeEx` + 接线）；必要时极小改 `InitSomeEventState` 补走廊名→Mgr2（仅当报告要求）。  
> **禁止**：把 `StoryPrefabName` 改成 `ForestEastSceneSlimeEatSheep`；把 Action2 改成 Action；改东城郊 Prefab 当实验场不还原；在 Update 堆业务；未立项就改 Mgr 共用存档键。  
> **文案**：若 OPEN_QUESTIONS 未另给台本，按报告默认（通常对齐东城郊句序）；中/英/日与 FaceType/audio 尽量与对照 Prefab 一致。  
> **文档**：`Assets/Doc/施工说明/0913/VerdantCorridor_史莱姆吃羊补对白_施工说明.md`  
> **验收**：  
> 1）新档走进触发器：**有对话框台词**（≥1 句可见）；  
> 2）对白结束后史莱姆按设计出现/可打；  
> 3）打完 → 死羊交互/坟墓链不毁；  
> 4）同存档再进：**不再重复**主对白（SingleUse）；  
> 5）抽测 `VerdantCorridorFirstEnter` 等其它走廊对白仍正常。  

---

## 【验收员】Prompt（施工后可选）

> 短生命周期 Debug（如 `[SlimeEatSheepVCDebug]`）：`TriggerStory` 时打 `StoryPrefabName`、`CheckStoryUsed`、Load 是否 null；对话图开始时打 Statement 节点数（若可从 Graph 取）。  
> Play：新档走一次录「有无对话框」；再读档走一次录「是否正确不再触发」。  
> 输出：通过/失败项 + 剩余风险（Mgr 存档键串台、镜头 CameraPos 子物体缺失）。
