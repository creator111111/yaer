# VerdantCorridor 史莱姆吃羊无对白 — 架构溯源报告

**文档版本**：v1.0（2026-09-13）  
**文档性质**：【架构侦探】只读溯源；**未改**代码 / Prefab / 场景 / Git  
**Unity**：2020.3.48f1  
**现象（用户）**：`VerdantCorridor` → `Objects/SlimeEatSheepStoryTrigger`，走进去 **不显示对话**；体感「对话文件丢了」  
**产品期望**：走进后播完整对白（对话框/立绘/台词），再走 `SlimeEatSheepStoryAction2("start")` 刷怪；死羊/坟墓链保持可用  
**不是**：改东城郊 `ForestEastSceneSlimeEatSheep` 当主流程；重做对话系统；改村庄对白；把走廊 `StoryPrefabName` 指到东城郊 Prefab  
**场景**：`Assets/GameRes/Scenes/VerdantCorridor.unity`  
**提示词**：`Assets/Doc/提示词/0913/VerdantCorridor_史莱姆吃羊无对白_架构侦探提示词.md`  
**施工说明**：`Assets/Doc/施工说明/0913/VerdantCorridor_史莱姆吃羊补对白_施工说明.md`（2026-09-13 已按方案 A 落地）

---

## 沟通摘要

### ① 结论一句话

**文件没丢**：`VerdantCorridorSlimeEatSheep.prefab` 在盘上且能被加载；**无对白是因为图里 0 个 `StatementNodeEx`**（从入库起就是空台词壳），触发链仍会跑镜头/黑幕/`Action2("start")` 刷怪——用户以为「文件丢了」，其实是 **Prefab 在但台词被掏空（或从未从东城郊拷全）**。

### ② 原因（通俗）

走廊这条线有自己的对话 Prefab 和 Mgr2，接线是通的。但对话图只做了「找镜头 → 推镜 → 黑幕 → 刷史莱姆」，**中间没有一句台词节点**。东城郊同款图有 6 句雅尔/旁白。所以走进去可能有镜头晃一下，却永远看不到对话框。

### ③ 用户需要做什么（检查清单）

| # | 操作 | 现网预期（未修） |
|---|------|------------------|
| 1 | Project 打开 `GameRes/Prefabs/Dialogue/VerdantCorridorSlimeEatSheep.prefab` | NodeCanvas：**无** Statement 节点 |
| 2 | 对照打开 `ForestEastSceneSlimeEatSheep.prefab` | **6** 句 `StatementNodeEx` |
| 3 | 新档进走廊，走进 `SlimeEatSheepStoryTrigger` | 无对话框；可能有镜头/黑幕/随后刷怪 |
| 4 | 同存档再进一次，看组件 `SimpleStoryTrigger.enabled` | 若已播过应为 `false`（SingleUse） |
| 5 | 抽测同场景 `VerdantCorridorFirstEnter` | 应有台词（系统未坏） |
| 6 | 拍板：走廊是否照搬东城郊 6 句原文 | 记 OPEN_QUESTIONS Q1 |

### ④ 程序补充

见下文 §1～§6。

---

## 1. 结论（程序向）

| 项 | 裁定 |
|----|------|
| **「文件丢了」形态** | **否 Missing**：路径 `Assets/GameRes/Prefabs/Dialogue/VerdantCorridorSlimeEatSheep.prefab` 存在（guid `8efddb2c…`） |
| **真正断点** | Prefab 对话图 **0× StatementNodeEx**；触发/加载/Action2 链仍通 |
| **Git** | 首次入库（`96c5156a`）起 Statement 计数即为 **0** → **从未拷全台词**，非后期误删 |
| **场景接线** | `StoryPrefabName=VerdantCorridorSlimeEatSheep`；`triggerType=1`（Enter）；`SingleUseInArchive=1`；挂 `SlimeEatSheepStroy2`（Mgr2） |
| **收尾事件** | 走廊图已是 `SlimeEatSheepStoryAction2` + `"start"`（正确）；东城郊为 `SlimeEatSheepStoryAction`（Mgr1） |
| **CSV 台本** | `Assets/Dialog` **无** 同名源 → 补台词只能改 Dialogue Prefab（或另导入） |
| **推荐修复** | **方案 A**：在走廊 Prefab 镜头与黑幕/`start` 之间插入 Statement 节点；事件 **保持 Action2**；无新台本时默认对齐东城郊 6 句 |

---

## 2. 调用链（走进 → 图 → 刷怪）

```
玩家进入 BoxCollider2D（Trigger）
  → InteractiveComponent.onEnterInteractiveEvent
  → SimpleStoryTrigger.OnEnterTriggerStory
       （玩家未死）
  → TriggerStory / TryStartBoundStory
       SingleUse？CheckStoryUsed("VerdantCorridorSlimeEatSheep") → 已用则 return
       StoryComponentGSM.TriggerStory("VerdantCorridorSlimeEatSheep")
         → HasRunningStory=true
         → ResMgr.LoadAsset(DialoguePath =
              "Assets/GameRes/Prefabs/Dialogue/VerdantCorridorSlimeEatSheep.prefab")
         → OnStoryPrefabLoad(go)
              Open/Get NormalDialogueNewPanel
              → StartDialogue(go)

对话图现网节点（全部 ActionNode，无 Statement）：
  0  FindChild(CameraPos2) + UI 淡入
  1  CameraMove → CameraPos2
  2  Wait(约 1s)（预扫/结构）
  3  CameraMove 回
  4  CameraFollowPlayer
  5  BlackMask 黑幕
  6  ★ SlimeEatSheepStoryAction2("start")
        → SlimeEatSheepStoryMgr2.ParseStoryAcitonArgs("start")
        → OnSceneStoryTrigger(true) → Stroy2.BattleStoryStartOrEnd(true)
        → TriggerSlime() 激活史莱姆
  7  UI/黑幕收尾

★ 断点：全程无 StatementNodeEx → NormalDialogue 无台词/立绘句可播
  （「不显示对话」成立；镜头/刷怪仍可能发生）

剧情结束 → StoryComponentGSM.OnStoryEnd
  → StoryTriggerCountData.OnStoryTriggered(名字)  // SingleUse 记档
```

**场景字段（已核实）**

| 字段 | 值 |
|------|-----|
| 物体 | `SlimeEatSheepStoryTrigger`（Layer 21，`m_IsActive:1`） |
| `StoryPrefabName` | `VerdantCorridorSlimeEatSheep` |
| `SingleUseInArchive` | `1` |
| `triggerType` | `1` = Enter |
| 故事物体 | `SlimeEatSheepStroy2`（guid `ebe5da4c…`） |
| 史莱姆列表 | `slimeLogics` 2 个引用 |

---

## 3. 证据表

| # | 证据 | 结论 | 状态 |
|---|------|------|------|
| E1 | 磁盘存在 `VerdantCorridorSlimeEatSheep.prefab` + `.meta` | 非路径 Missing | ✅ |
| E2 | Prefab 内 `StatementNodeEx` 计数 = **0**；`_text` = 0 | **无台词节点** | ✅ |
| E3 | 同 Prefab `SlimeEatSheepStoryAction2` = 1，`eventArgs=start` | 刷怪事件已接 Mgr2 | ✅ |
| E4 | `ForestEastSceneSlimeEatSheep`：`StatementNodeEx` = **6**；Action1（非 2） | 对照有完整对白 | ✅ |
| E5 | Git：首次跟踪该 Prefab 时 Statement 已为 0 | 从未有台词，非后期删光 | ✅ |
| E6 | `Assets/Dialog` 无 EatSheep / VerdantCorridorSlime CSV | 无独立台本源可导入 | ✅ |
| E7 | 场景 `SimpleStoryTrigger` 字段与提示词一致 | 触发配置正确 | ✅ |
| E8 | `VerdantCorridorFirstEnter.prefab` 含 StatementNodeEx | 同场景对话系统可用 | ✅ |
| E9 | `InitSomeEventState` switch **无** `VerdantCorridorSlimeEatSheep`→Mgr2 | 读档战斗态同步缺口（次要 F） | ✅ |
| E10 | Mgr1 与 Mgr2 共用存档键 `SlimeEatSheepStory_hasCreateGrave` / `_hasTriggerSlime` | 串台风险（次要 E）；**不解释无台词** | ✅ |
| E11 | `DialoguePath.GetPath` 拼 `…/Dialogue/{name}.prefab` | 加载路径正确 | ✅ |
| E12 | Play 复现矩阵（有无镜头/刷怪） | 侦探未实机；由 Prefab 静态钉死「无台词」 | ⚠️ 待 Play |

### 嫌疑复核（A–F）

| 嫌疑 | 裁定 |
|------|------|
| **A Prefab 无台词** | **主因 · 已证实** |
| **B SingleUse 已用** | **可叠加「连镜头都没有」**；不能解释 Prefab 内 0 Statement；验收用新档 |
| **C 触发未开火** | **排除为主因**（配置 Enter + Trigger 碰撞齐全；FirstEnter 等同场景正常） |
| **D 加载失败** | **排除**（文件在；无理由专挂此名；若 null 会有 Error） |
| **E Mgr 存档键串台** | **另案**；影响坟/刷怪态，非无对话框 |
| **F InitSomeEventState 缺走廊名** | **另案/可选小补**；非无台词主因 |

---

## 4. 与东城郊对照表

| 项 | ForestEast（东城郊） | VerdantCorridor（苍翠走廊） |
|----|----------------------|-----------------------------|
| Dialogue Prefab | `ForestEastSceneSlimeEatSheep` | `VerdantCorridorSlimeEatSheep` |
| StatementNodeEx | **6** | **0** |
| 刷怪 Action | `SlimeEatSheepStoryAction` → **Mgr1** | `SlimeEatSheepStoryAction2` → **Mgr2** |
| 场景故事物体 | `SlimeEatSheepStroy`（对照用） | `SlimeEatSheepStroy2` |
| 触发器名 | 同名 `SlimeEatSheepStoryTrigger` | 同左 |
| SingleUse | （东城郊场景另核） | **1** |
| CSV | 无 | 无 |

### 东城郊 6 句（施工默认文案 · 待产品确认是否照搬）

| # | 角色 | 中文 | Face / audio（对照 Prefab） |
|---|------|------|------------------------------|
| 1 | 雅尔 | 那是………… | FaceType=1；audio=1 |
| 2 | 雅尔 | 它们居然在吃其它生物！ | audio=2 |
| 3 | 旁白 | 雅尔背后涌上一股恶寒，眼前的事物让她顿感生理不适，超出她常识的生物正在吸取一个动物尸体，莫名的诡异让她有些作呕。 | （旁白） |
| 4 | 旁白 | 同时她也意识到这个生物十分危险 | （旁白） |
| 5 | 雅尔 | 在龙城郊外居然有这么危险的东西。 | FaceType=8；audio=3 |
| 6 | 雅尔 | 想过去必须要消灭他们！ | FaceType=8；audio=4 |

> 插入位置（推荐）：**镜头推/拉与 CameraFollow 之后、黑幕与 `Action2("start")` 之前**（对齐东城郊「先说话 → 再黑幕刷怪」节奏）。若走廊文案要改「龙城郊外」等地名，产品另给台本（OPEN Q1）。

### 走廊现网节点链（无台词）

```
0 Find+UI淡入 → 1 镜头到 CameraPos2 → 2 Wait → 3 镜头回
→ 4 CameraFollow → 5 黑幕 → 6 Action2("start") → 7 UI/黑幕收
```

---

## 5. 修复方案对比

### 方案 A（推荐 · 最小）

只改 `Assets/GameRes/Prefabs/Dialogue/VerdantCorridorSlimeEatSheep.prefab`：

1. 在黑幕/`start` 前插入 **6× StatementNodeEx**（文案默认抄东城郊，含 en/jp/Face/audio）。  
2. **保留** `SlimeEatSheepStoryAction2("start")`，禁止改成 Action1。  
3. 重接 DTConnection：镜头段 → **台词段** → 黑幕 → Action2 → 收尾。

| 利 | 弊 |
|----|-----|
| 不动代码/场景/Mgr；改动面单 Prefab | 手工接线易错，需 NodeCanvas 打开验收 |
| 刷怪/坟墓仍走 Mgr2 | 若产品要改文案需二次改 Prefab |

### 方案 B（编辑器拷贝子图）

从东城郊 Prefab 拷贝台词子图到走廊，再把末尾 Action **改回 Action2**（若拷贝带过来 Action1）。

| 利 | 弊 |
|----|-----|
| 三语文案/表情一次对齐 | 易误把 Action1 带进走廊 → 打到 Mgr1 |
| | 仍须人工检查连接与 actorParameters |

### 方案 C（禁止）

把场景 `StoryPrefabName` 改成 `ForestEastSceneSlimeEatSheep`。

| 为何禁止 |
|----------|
| 会执行 **Mgr1** Action，走廊场景物体是 **Stroy2/Mgr2** → 刷怪/坟墓错绑 |
| 存档 SingleUse 记成东城郊剧情名，走廊计数乱 |

### 可选代码小补（非本期台词必需）

在 `SimpleStoryTrigger.InitSomeEventState` 增加：

```csharp
case "VerdantCorridorSlimeEatSheep":
    SlimeEatSheepStoryMgr2.getInstance().InitBattleData(SingleUseInArchive, enabled);
    break;
```

仅改善「已用过读档」时 Mgr2 初始化对称性；**不恢复对白**。默认记 OPEN Q3，台词案之后另议。

### 推荐与禁止

| 项 | 内容 |
|----|------|
| **推荐** | **方案 A**（必要时用 B 加速拷贝，但必须核对 Action2） |
| **禁止** | 方案 C；改东城郊 Prefab 不还原；改 Action2→Action1；未立项改 Mgr 共用存档键；重写对话 UI |

### 回归风险与验收

| 风险 | 验收 |
|------|------|
| 无对白仍刷怪 | 新档走进：**≥1 句对话框可见**，再黑幕刷怪 |
| Action 打错 Mgr | 刷的是走廊 `slimeLogics`；坟/死羊交互仍走 Stroy2 |
| SingleUse | 同存档再进不再播主对白；`enabled=false` |
| 其它走廊对白 | 抽测 `VerdantCorridorFirstEnter` 等仍正常 |
| 东城郊未误伤 | 东城郊吃羊线仍 6 句 + Action1 |
| 存档串台 E | 先东城郊再走廊：坟/史莱姆态是否错（**另案**，本期可记风险） |

**验收标准（施工后）**

1. 新档走进触发器：有对话框台词（建议可见全部 6 句或产品确认句数）。  
2. 对白结束后史莱姆出现可打。  
3. 打完 → 死羊/坟墓链不毁。  
4. 同存档再进：不重复主对白。  
5. 抽测其它走廊对白仍正常。

---

## 6. OPEN_QUESTIONS

已记入 `Assets/Doc/OPEN_QUESTIONS.md`：

| ID | 问题 | 施工默认 | 状态 |
|----|------|----------|------|
| Q1 | 走廊对白是否与东城郊 **完全同文**（含「龙城郊外」）？ | **是**（无甲方新台本则照搬 6 句+三语） | 待确认 |
| Q2 | 台词插入点：镜头完再说话，还是说话中推镜？ | **镜头段后、黑幕前**（对齐东城郊节奏） | 待确认 |
| Q3 | 是否本期补 `InitSomeEventState` → Mgr2？ | **本期否**；先只补 Prefab 台词 | 待确认 |
| Q4 | Mgr1/Mgr2 共用存档键是否拆开？ | **另案**；本期不改 | 待立项 |

---

## 7. 给施工员的一句话

**只改走廊 Dialogue Prefab：补 Statement，保住 Action2；绝不要把 StoryPrefabName 改成东城郊。**
