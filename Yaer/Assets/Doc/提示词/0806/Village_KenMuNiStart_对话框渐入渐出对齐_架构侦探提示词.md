# Cursor Agent Prompt · Village_KenMuNiStart 对话框出现需与 BG/立绘一致的渐入渐出

> **角色**：先【架构侦探】，报告通过后再定【施工员】方案  
> **日期**：2026-08-06  
> **范围**：`Village_KenMuNiStart` 开场分层已通；本期只补齐/对齐 **对话框（字幕条）出现时的渐入渐出**，与背景、人物立绘同一套观感。不改台本、不推翻分层顺序。  
> **本阶段**：只摸清现网框 Fade 是否真在播、时长/曲线是否与 BG·立绘一致、结尾有无渐出，**不施工**

把下面整段复制给 Cursor Agent（Agent Mode）执行。本阶段**只溯源、不改代码**。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 产品目标（开发者白话 · 已定）

开场分层时：

- **背景 BG**、**人物立绘**已有渐变显现（玩家能感到「淡出来」）。  
- **对话框出现**也要加 **渐入渐出**，**和背景、人物保持一致**（同一套时长手感 / 同一类透明度动画，不要框突然砸出来或硬切消失）。

继承硬约束（0806 已施工，不可回退）：

```
拍1 仅 BG（零漏缝）
  → 拍2 对话框出现（本期要「渐入」，观感对齐 BG/立绘）
  → 拍3 立绘渐入
  → 正常对话
  →（若产品要）对白结束时框/立绘/BG 的「渐出」也对齐 —— 侦探须问清是否含结尾
```

### 现网漂移（侦探必须对齐「文档说有 Fade」vs「玩家体感」）

| 来源 | 说法 | 可疑点 |
|------|------|--------|
| OPEN / 0806 分层施工 | Prefab：`框(Delay1+Fade1) → 立绘并行 Fade1` | 文档写框**已有** Duration=1 Fade；用户仍说要「加」→ 可能**没播出来**、**看不出**、或**与 BG/立绘手感不一致** |
| Prefab 序列化片段 | `NormalDialogueUIAlphaAnimationTaskAction`：`EndAlpha=1`，`Duration=1`，`Delay=1`，`EndActonOnAnimationEnd=true`；`StartAlpha` 字段在 JSON 里常为空 | StartAlpha 默认是否真为 0？空 BB 是否导致从当前 alpha（已是 1？）「淡」到 1 → **体感硬切** |
| 立绘 | `CanvasGroupAlphaActionTask` Duration=1，`EndActionOnAnimationEnd=true` | 框任务类不同、参数名拼写不同（`EndActon…` vs `EndAction…`）——是否一边阻塞一边 fire-and-forget |
| BG 拍1 | 多靠 System `CloseFormFade` 露出已满不透明 BG；BG 自身可能仍无 CanvasGroup Fade | 「与背景一致」= 对齐黑幕淡出时长，还是要给 BG 也做 CanvasGroup 0→1？**须拍板** |
| Prepare 白名单 | 淡出黑幕前把 `dialogueUICanvasGroup` 置 0 | 框应从 0 淡到 1；若随后被别处瞬间写 1，Fade 被抹掉 |
| 结尾 | 分层报告主写「出现」；用户写「渐入**渐出**」 | 须核实：是否包含 **对白结束时框淡出**，还是仅开场出现用「渐入渐出」口头禅指淡入 |

### 高度可疑假说（优先验证）

1. **框 Fade 参数无效**：`StartAlpha` 未显式=0，或 Prepare 后又被别逻辑拉满，DOFade(1←1) 无动画。  
2. **只淡了 `subtitlesCanvasGroup`**：视觉上框底图/装饰/整条 Bottom 仍硬切，和立绘整组 CanvasGroup 淡入不对齐。  
3. **时长/延迟与立绘不一致**：框 Delay1+Fade1，立绘无 Delay 仅 Fade1——用户要「一致」可能指 **同一 Duration、同一缓动，不要框另套手感**。  
4. **缺渐出**：开场有淡入，结束时框/BG/立绘硬关；用户要三层一起淡出。  
5. **BG 参照系不清**：BG 用黑幕淡出、立绘用 CanvasGroup；框应对齐哪一条——侦探比选后写进 OPEN。

---

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Project_context.md
@Assets/Doc/02_SYSTEM_SPEC.md
@Assets/Doc/执行文档/0806/Village_KenMuNiStart_开场分层显现时序_架构溯源报告.md
@Assets/Doc/执行文档/0806/Village_KenMuNiStart_分层后小头像不显示_架构溯源报告.md
@Assets/Doc/OPEN_QUESTIONS.md
@Assets/GameRes/Prefabs/Dialogue/Village_KenMuNiStart.prefab
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Scene/Village_KenMuNi/Village_KenMuNiSceneManager.cs
@Assets/Scripts/Game/GameRuntime/NodeCanvas/NodeCanvasNode/ActionTask/NormalDialoguePanel/NormalDialogueUIAlphaAnimationTaskAction.cs
@Assets/Scripts/Game/GameRuntime/NodeCanvas/NodeCanvasNode/ActionTask/Common/CanvasGroupAlphaActionTask.cs

你现在是【架构侦探】。Unity 2020.3.48f1 / C#。
禁止修改任何代码、Prefab、场景、图集、CSV、台本。只读扫描 + 写溯源报告。

---

## 背景（策划白话）

1. `Village_KenMuNiStart` 分层节奏已经有了（先 BG、再框、再人）。
2. 背景和人物看起来是**渐变出来**的；对话框出现也要有**渐入渐出**，和它们**保持一致**，不要框突然蹦出来（或结束时突然没）。
3. 文档里可能已经挂了「对话框 UI 透明度动画」——侦探须核实：现网到底有没有、玩家为什么仍不满意。
4. 本期只溯源对齐方案；保住分层顺序、零漏缝、Mask 小头像白名单成果。

---

## 必读 / 优先扫描线索

### A. 钉死三层「出现」各自怎么淡

| 产品层 | 现网执行体（须核实） | Duration/Delay | 玩家可见？ |
|--------|----------------------|----------------|------------|
| BG | CloseFormFade？BG CanvasGroup？Image.color？ | | |
| 对话框 | `NormalDialogueUIAlphaAnimationTaskAction` → 哪个 CanvasGroup | | |
| 立绘 | `CanvasGroupAlphaActionTask` × 雅/古 | | |

画出开场时序：黑幕淡出 → 框节点 → 立绘节点 → 首句；标出每段 alpha 曲线。

### B. 对话框任务是否真在「渐入」
- Prefab Node 上 `StartAlpha` / `EndAlpha` / `Duration` / `Delay` / `EndActonOnAnimationEnd` 运行时值  
- `GetDialogueUICanvasGroup()` 绑的是整条 Bottom 还是仅字幕  
- Prepare 置 0 之后、Fade 开始前，有无代码/动画把 alpha 瞬间改回 1  
- DialogDebug 与进村旁路体感是否一致

### C. 「与背景、人物一致」的度量
- 建议对齐项（侦探勾选推荐）：同一 `Duration`（现立绘 1.0）、同一缓动（Linear/OutQuad 等）、是否都要 Delay、是否都要阻塞再进下一拍  
- BG 若仍无 CanvasGroup Fade：框应对齐「立绘 CanvasGroup」还是「黑幕 CloseFormFade 时长」？

### D. 「渐出」范围
- 搜对白结束 / `onStoryEnd` / Prefab 尾部是否有框·立绘·BG 的 1→0  
- 若无：补渐出挂在 Prefab 尾还是壳 Close？会否与通用关面板黑幕叠两层

### E. 范围冻结
- **保留**：分层顺序（BG→框→立绘）、零漏缝、Prepare 白名单、只播一次、台本、Mask 小头像修复  
- **可改建议**：Prefab 框 Alpha 节点参数；必要时给 BG 加 CanvasGroup 与立绘同款任务；结尾淡出节点  
- **禁止**：改回齐出 Snap；名字广扫 CanvasGroup；重写通用对话壳关面板逻辑（除非证实必须且进村专用旁路）

---

## 侦探任务清单

1. **结论一句话**：框为何体感没有（或不齐）渐入渐出；推荐最小对齐改法。

2. **三层对照表 + 时序图**（出现；若含结尾则再画一张渐出）。

3. **根因归类**（可多选）  
   - 参数未生效 / 绑错 CanvasGroup / 与 BG·立绘时长不一致 / 缺渐出 / 其它

4. **方案比选表**（至少 3 档，推荐 1 个）

   | 方案 | 做法摘要 | 与 BG/立绘一致？ | 改动面 | 风险 | 推荐？ |
   |------|----------|------------------|--------|------|--------|
   | A | 只修 Prefab：框 StartAlpha=0、Duration/缓动对齐立绘；显式 EndAction 阻塞 | | | | |
   | B | 框改走与立绘相同的 `CanvasGroupAlphaActionTask`（绑同一层级 CanvasGroup） | | | | |
   | C | BG 也加 CanvasGroup Fade，三层统一 Duration；框按同模板 | | | | |
   | D | 另补结尾三层渐出（1→0），开场与收尾成对 | | | | |

5. **施工员最小改动清单**（只建议）  
   - 优先只动 `Village_KenMuNiStart.prefab` 前奏/尾奏节点参数；非必要不动 `NormalDialogueUIAlphaAnimationTaskAction` 通用类。  
   - 若动通用任务，说明对其它对话 Prefab 的影响面。

6. **验收清单**  
   - 进村开场：框**明显淡入**，时长手感与立绘（及拍板后的 BG）一致，无硬切蹦出。  
   - 分层顺序不变；零漏缝；Mask 小头像仍在。  
   - 若拍板含渐出：对白结束框/人/BG 淡出不硬切，且不叠双重黑幕卡死。  
   - DialogDebug 同 Prefab 观感一致。

7. **开放问题**追加 OPEN（「Village_KenMuNiStart 对话框渐入渐出对齐 · 2026-08-06」）：  
   - 「渐出」是否包含对白结束？  
   - 框对齐立绘 Duration=1，还是对齐黑幕淡出时长？  
   - BG 是否补 CanvasGroup Fade 才算「三层一致」？

8. **禁止**：改台本；取消分层；Update 轮询补 alpha；破坏 Prepare 白名单；为对齐重写全游戏对话开关场。

---

## 输出要求

写入：`Assets/Doc/执行文档/0806/Village_KenMuNiStart_对话框渐入渐出对齐_架构溯源报告.md`

报告结构固定（中文、大白话优先）：

① 结论一句话（根因 + 推荐方案）  
② 原因（生活类比：幕布、演员、提词板要同一套灯光淡入）  
③ 用户需要做什么（拍板渐出范围 / BG 是否补 Fade + 验收）  
④ 给程序看的补充：三层对照表、时序图、方案表、最小改动、开放问题

完成后用 MASTER 固定四段式口头汇报；详细以报告文件为准。
```

---

## 施工员续跑（侦探报告拍板后再开）

> 本阶段先不要贴。等确认「只修开场淡入」还是「开场+结尾成对渐出」，以及 BG 是否补 CanvasGroup Fade 后再写。

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0806/Village_KenMuNiStart_对话框渐入渐出对齐_架构溯源报告.md
@Assets/Doc/OPEN_QUESTIONS.md

你现在是【施工员】。按上述溯源报告做**最小化修改**，使 Village_KenMuNiStart 对话框出现的渐入渐出与背景、人物立绘观感一致。
保留分层顺序、零漏缝与 Mask 白名单；优先改本 Prefab 节点参数。
禁止在 Update 堆补丁。每次提交说明：改了哪些文件、三层如何对齐、如何验证。
```
