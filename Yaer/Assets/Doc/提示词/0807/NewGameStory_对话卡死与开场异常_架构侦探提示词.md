# Cursor Agent Prompt · NewGameStory 对话卡死（点击不推进）+ 开场异常

> **角色**：先【架构侦探】，报告通过后再定【施工员】方案  
> **日期**：2026-08-07  
> **优先级**：**先把序章对话本身修正常**（能点开、能点下去、开场体感可玩），分层对齐 KenMuNi 的「完美节奏」可排二期  
> **本阶段**：只溯源、不施工

把下面整段复制给 Cursor Agent（Agent Mode）执行。本阶段**只溯源、不改代码**。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 玩家现象（开发者口述 · 2026-08-07）

1. **点击对话不会继续**（打字机打完或显示完后，点屏幕/对话框无法进下一句）。  
2. **开场不对**（分层/黑幕/立绘/对话框时序或可见性异常；具体以现网 Play 为准）。  
3. 开发者判断：`NewGameStory` **本身就不正常了** → 本期目标是 **先恢复可玩对话链路**，不是继续打磨分层美学。

### 高嫌疑：0806「半施工」断裂（必须钉死）

| 层 | OPEN / 溯源报告声称（2026-08-06「已施工」） | 磁盘现网线索（助手扫到，须侦探核实） |
|----|---------------------------------------------|--------------------------------------|
| Prefab | 串行 Wait0.5 → YaerPainting Fade0.5 → UIAlpha Delay0.5+Dur0.5(+PrepareMask) | `NewGameStory.prefab` 前奏 ActionList **已是** `WaitVillageStartBgReveal` → `CanvasGroupAlpha(YaerPainting)` → `NormalDialogueUIAlpha`(PrepareMask=true) |
| SceneManager | 漫画全黑后 `Gate.Reset` → Trigger → Prepare 白名单 → System `HideFade` 拍1 → `Signal` | `NewGameSceneManager.cs` **仍是** `onFinishEvent` 里直接 `TriggerStory("NewGameStory")` + BGM，**未见** Gate / Prepare / HideFade / Signal |
| 文档 | OPEN Q1–Q3 标 ✅ 已施工 | 与 SceneManager **不一致** → 可能「Prefab 改了、旁路没改 / 改后又回退 / 文档误标」 |

**推论（待证）**：树在等村闸门/分层前奏，旁路未 Signal 或黑幕未按设计收；或前奏/遮罩把输入、可见性弄坏 → 体感「开场怪 + 点不动」。  
注意：`VillageStartLayerRevealGate` **默认 `IsBgFullyVisible=true`**，且 Wait 有 **8s 超时强制 Signal**——若「永远点不动」不能只甩锅永久卡 Wait，须继续查 **首句后 Continue / IsAlphaHide / 射线 / 黑幕残留**。

### 对照：正常样例

- 进村 `Village_KenMuNiStart`：旁路在 `Village_KenMuNiSceneManager.TryDeferBlackFadeForCover`（Reset → Trigger → Prepare → HideFade → Signal），Prefab Wait 与旁路成对。  
- 点击推进真源：`DialogueTMPUGUI`（`OnPointerClick` → `anyKeyDown`；`IsAlphaHide` 为 true 时点击无效；`info.Continue()` 交还对话树）。

### 本期范围冻结

- **主修**：NewGame 路径下 `NewGameStory` **可触发 → 开场可理解 → 点击可逐句推进 → 能正常走到树结束**。  
- **可建议**：补全或回退 0806 半施工、Prefab 前奏、漫画 Finish 与黑幕衔接、输入锁。  
- **不主修**：台本文案、漫画分页内容、进村已落地逻辑（除非证明共用 Gate/UIAlpha 污染了 NewGame）、完美 0.5s 美学对齐（二期）。

---

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Project_context.md
@Assets/Doc/02_SYSTEM_SPEC.md
@Assets/Doc/OPEN_QUESTIONS.md
@Assets/Doc/执行文档/0806/NewGameStory_开场分层对齐KenMuNi标准_架构溯源报告.md
@Assets/Doc/提示词/0806/NewGameStory_开场分层对齐KenMuNi标准_架构侦探提示词.md
@Assets/Doc/技术文档/演出相关/Village_KenMuNiStart_开场分层显现_技术说明.md
@Assets/Doc/技术文档/演出相关/NewGameCartoonPanel漫画开场策划案.md
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Scene/NewGame/NewGameSceneManager.cs
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Scene/Village_KenMuNi/Village_KenMuNiSceneManager.cs
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Scene/Village_KenMuNi/VillageStartLayerRevealGate.cs
@Assets/Scripts/Game/GameRuntime/NodeCanvas/NodeCanvasNode/ActionTask/Village_KenMuNi/WaitVillageStartBgRevealActionTask.cs
@Assets/Scripts/Game/GameRuntime/NodeCanvas/NodeCanvasNode/ActionTask/NormalDialoguePanel/NormalDialogueUIAlphaAnimationTaskAction.cs
@Assets/Scripts/Game/GameRuntime/NodeCanvas/NodeCanvasExtend/DialogueTMPUGUI.cs
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Story/NormalDialogueFormNewLogic.cs
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Cartoon/NewGameCartoonFormLogic.cs
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Cartoon/NewGameCartoonFormProxy.cs
@Assets/GameRes/Prefabs/Dialogue/NewGameStory.prefab
@Assets/GameRes/Prefabs/Dialogue/Village_KenMuNiStart.prefab

你现在是【架构侦探】。Unity 2020.3.48f1 / C#。
禁止修改任何代码、Prefab、场景、图集、CSV、台本。只读扫描 + 写溯源报告。

---

## 背景（策划白话）

1. 新游戏进序章后，`NewGameStory` 对话 **点了不往下走**，开场也不对。
2. 昨天（0806）做过「开场分层对齐 KenMuNi」施工；文档写已落地，但现网可能 **Prefab 与 SceneManager 旁路没成对**。
3. 本期 **先恢复对话可玩**（点击推进 + 开场不离谱），分层美学对齐可以二期再抠。
4. 只溯源根因与最小修复方案；不施工。

---

## 必读 / 优先扫描线索

### A. 双问题拆开验（禁止混成一句糊弄）

| ID | 现象 | 侦探要回答 |
|----|------|------------|
| P0-点击 | 点对话框/屏幕不进下一句 | 卡在树的哪一节点？是没进 Statement，还是 Statement 在等输入但 `anyKeyDown`/`Continue` 失效？ |
| P1-开场 | 开场不对 | 现网时序图：漫画关 → 黑幕 → 壳 → Wait/闸门 → 立绘 → 框 → 首句；与「可玩基线」差在哪 |

可玩基线（本期验收底线，不是 KenMuNi 完美版）：

```
漫画结束 → 对话壳起来 → 能看见对话内容（至少框+字可读）
  → 打字机/显示完成后点击 → 下一句
  → 可连续推进到剧情结束（或至少连续多句无卡死）
```

### B. 0806 半施工核对（必出对照表）

逐项标：**报告声称 / 磁盘实有 / 是否成对**

- [ ] `NewGameSceneManager`：Gate.Reset / Prepare 白名单 / HideFade / Signal  
- [ ] `NewGameStory.prefab`：Wait / YaerPainting Fade / UIAlpha / 与 Statement 连线 / ActionList 串并行  
- [ ] `VillageStartLayerRevealGate`：NewGame 路径是否 Reset 却永不 Signal；默认 true 是否被村流程污染残留  
- [ ] Wait 8s 超时：若超时后能进首句，用户「点不动」是否发生在 **超时之后**（则根因在输入/Continue，不在 Wait）  
- [ ] OPEN「已施工」是否误标（建议侦探在报告里写清文档债）

### C. 点击推进链路（P0 主链）

从首句 `StatementNodeEx` 起画：

```
StatementNodeEx
  → DialogueTMPUGUI.OnSubtitlesRequest
  → TextAnimation / 等语音
  → WaitForInputToMoveNext（anyKeyDown）
  → info.Continue()
  → 下一节点
```

必查：

| 嫌疑点 | 文件/字段 | 问什么 |
|--------|-----------|--------|
| 点击被吞 | `IsAlphaHide`；谁设 true/false（`NormalDialogueFormNewLogic`） | 渐入后是否一直 true？ |
| 射线 | 全屏 BlackMask / 漫画残留 / System 黑幕 / CanvasGroup.blocksRaycasts | 点是否打到 `DialogueTMPUGUI`？ |
| LateUpdate 清旗 | `DialogueTMPUGUI.LateUpdate` 每帧 `anyKeyDown=false` | 与 `WaitUntil(..., PreLateUpdate)` 时序是否在 NewGame 路径异常？进村是否同样代码却正常？ |
| Continue 抛错 | `info.Continue` try/catch | Console 是否有警告？树是否已停？ |
| 仍停在前奏 | Wait / Fade `EndActionOnAnimationEnd` / BB `YaerPainting` 空引用 | 根本没到可点状态，用户误以为「点对话无效」 |
| 音频句 | `skipOnInput` + 语音等待 | 有语音的句是否更易复现？ |

对照：同一套 `DialogueTMPUGUI` 下 `Village_KenMuNiStart` 能否正常点——**若村正常、NewGame 不行 → 根因在 NewGame 入口/Prefab/遮罩，不在通用点击类。**

### D. 开场异常（P1）与漫画衔接

```
NewGame OnEnterScene → Open NewGameCartoonPanel
  → 播完/跳过 → CloseFormShowFade（先黑）
  → onFinish → TriggerStory("NewGameStory") + BGM
  → 关漫画 Form
  → 对话壳 + Prefab 前奏 → Statement
```

钉死瞬间：

1. 漫画关闭瞬间：System/Form 黑幕是否还在？会不会挡点击？  
2. 无 Gate 旁路时，带 `WaitVillageStartBgReveal` 的 Prefab 实际行为（默认 Ready 只跑 Hold？）  
3. UIAlpha `StartAlpha` 序列化是否为空 → 框透明度怪异 / 看不见字却在等点击  
4. PrepareMask / 清字是否导致「有框无字」或「空名闪一下」被说成开场不对  

### E. DialogDebug 对照（强烈建议写进验收）

- DialogDebug 直接拖 `NewGameStory`：能否点下去？开场如何？  
- 若 Debug **正常**、正式 NewGame **卡** → 根因在漫画 Finish / 黑幕 / SceneManager，不在 Prefab 台词树。  
- 若 Debug **也卡** → 根因在 Prefab 前奏或对话壳共用逻辑。

### F. 范围与禁止

- **优先方案方向**（侦探比选，推荐恢复可玩的最小刀）：  
  - A：补全 NewGame 旁路，与 Prefab Wait 成对（对齐村）  
  - B：Prefab 暂时去掉 Wait/分层依赖，回到「简单 Trigger 就能播完」的可玩态，分层二期再接  
  - C：只修点击/遮罩/IsAlphaHide，不动分层  
- **禁止**：改台本；重做漫画；为对齐重写全游戏对话系统；在 Update 堆补丁；把村逻辑拆坏却不写回归项。

---

## 侦探任务清单

1. **结论一句话**：P0/P1 各自根因（可同一断裂点）；推荐恢复可玩的方案（A/B/C）。

2. **现网时序图**（漫画 → 首句 → 第一次点击 → Continue），标卡死点。

3. **0806 半施工对照表**（声称 vs 磁盘 vs 是否成对）。

4. **点击链路表**：IsAlphaHide / 射线 / anyKeyDown / Continue / 是否未到 Statement。

5. **方案比选**（至少 A/B/C）— **以「先可玩」为第一排序**，完美分层为第二。

6. **施工员最小改动清单**（只建议，文件级）。

7. **验收清单**  
   - 新游戏：漫画结束 → 对话可读 → **点击可连续推进**（多句）。  
   - 开场不永久黑屏、不永久无字、不永久挡输入。  
   - DialogDebug 拖 `NewGameStory` 可点完。  
   - 进村 `Village_KenMuNiStart` 无回归。  
   - （可选）分层节奏二期再验。

8. **开放问题**追加 OPEN（「NewGameStory 对话卡死与开场异常 · 2026-08-07」）：  
   - 0806 文档「已施工」是否回滚/误标？  
   - 本期选 A 补旁路还是 B 回退 Prefab 保可玩？  
   - Gate 共用是否要在 NewGame 专用 Reset/Signal，避免与村串味？

9. **禁止**：改代码；改 Prefab；擅自改 OPEN 决议区以外的设计；把「点不动」直接写成「玩家不会点」而不查链路。

---

## 输出要求

写入：`Assets/Doc/执行文档/0807/NewGameStory_对话卡死与开场异常_架构溯源报告.md`

报告结构固定（中文、大白话优先）：

① 结论一句话（卡死点 + 推荐恢复方案）  
② 原因（生活类比：戏开了一半幕布/提词板对不上）  
③ 用户需要做什么（拍板 A/B/C + 验收）  
④ 给程序看的补充：时序图、半施工对照、点击链、方案表、最小改动、开放问题

完成后用 MASTER 固定四段式口头汇报；详细以报告文件为准。
```

---

## 施工员续跑（侦探报告拍板后再开）

> 本阶段先不要贴。等确认：① 卡死点在前奏还是 Continue ② 选补旁路还是回退 Prefab 保可玩 后再写。

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0807/NewGameStory_对话卡死与开场异常_架构溯源报告.md
@Assets/Doc/OPEN_QUESTIONS.md

你现在是【施工员】。按上述溯源报告做最小化修改，优先恢复 NewGameStory「可点、可推进、开场可玩」。
分层完美对齐 KenMuNi 非本期必须。禁止在 Update 堆补丁。每次说明：改了哪些文件、卡死点如何解除、如何验证点击推进与进村无回归。
```
