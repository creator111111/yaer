# Cursor Agent Prompt · NewGameStory 开场分层时间间隔对齐 KenMuNi

> **角色**：先【架构侦探】，报告通过后再定【施工员】方案  
> **日期**：2026-08-07  
> **范围**：序章 `NewGameStory` **已有** BG / 大立绘 / 对话框淡入，但**时间间隔不对** → 对齐进村 `Village_KenMuNiStart` 标准节奏（顺序 + 一律 0.5s）  
> **本阶段**：只对照标准 vs 现网差距，**不施工**  
> **关联**：若点击推进仍异常，见同目录 `NewGameStory_对话卡死与开场异常_架构侦探提示词.md`；**本期主修节奏参数与前奏结构**，不把「点不动」当主目标（除非改间隔必须碰到同一节点）

把下面整段复制给 Cursor Agent（Agent Mode）执行。本阶段**只溯源、不改代码**。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 产品目标（开发者已定 · 2026-08-07）

开发者确认：开场**已经有**背景、大立绘、对话框的淡入淡出效果，问题是 **时间间隔不太对**。

要对齐的标准真源：

`Assets/Doc/技术文档/演出相关/Village_KenMuNiStart_开场分层显现_技术说明.md`

标准玩家体感：

```
拍1  对话场景 BG 可见
  → 空拍 A（HoldAfterBgVisible = 0.5s）
拍2  场景大立绘淡入（Duration = 0.5s）
  → 空拍 B（对话框节点 Delay = 0.5s）
拍3  对话框 UI 淡入 + Mask 小头像同出（Duration = 0.5s）
  → Statement 首句
```

### 现网线索（助手 2026-08-07 再扫 · 须侦探钉死）

`NewGameStory.prefab` 开场 **第一个 Action 节点**现网更像 **旧并行三件套**（`executionMode=1` Parallel）：

| 节点 | 现网参数线索 | 与标准差距 |
|------|--------------|------------|
| `NormalDialogueBlackMask` | StartAlpha≈1，Duration≈**1.0**，与其它并行 | 标准前奏**不用**对话壳 BlackMask 当拍1 |
| `MecanimSetTrigger YaerShow` | 立绘靠 Animator Clip 时长 | 标准是 **CanvasGroup Fade Duration=0.5** |
| `NormalDialogueUIAlpha` | Delay≈**0.5**，Duration≈**0.7**，EndActon 多空，PrepareMask 多空 | 标准 Duration=**0.5**、串行、EndAction 阻塞、可 PrepareMask |

因此「有淡入、但间隔不对」的高概率原因：

1. **并行**：黑幕 / 立绘动画 / 对话框 Delay+Fade 叠在一起，体感间隔乱。  
2. **Duration 不统一**：框 0.7 ≠ 标准 0.5；立绘时长跟 Clip 走。  
3. **缺串行空拍**：无 Wait Hold 0.5「只见 BG」；空拍 B 虽有 Delay0.5，但与并行叠后不等于标准空拍。

（0806 曾声称改成串行 Wait→YaerPainting Fade→UIAlpha；若现网已回退到并行，侦探须写清「文档债 / 回退」。）

### 调节奏的「旋钮」清单（施工员将来只拧这些）

| 拍 | 含义 | 配置位置 | 目标值 |
|----|------|----------|--------|
| 空拍 A | BG 亮屏后再等 | `WaitVillageStartBgReveal.HoldAfterBgVisibleSeconds` | **0.5** |
| 拍2 | 大立绘淡入 | 场景立绘 `CanvasGroupAlpha.Duration`（`YaerPainting`） | **0.5** |
| 空拍 B | 立绘落定→对话框 | 对话框 `NormalDialogueUIAlpha.Delay` | **0.5** |
| 拍3 | 对话框淡入 | 同节点 `Duration` | **0.5** |

硬约束（继承标准文档）：

- 前奏须 **串行**（立绘 Fade 完成后再跑对话框 Delay）。  
- Fade 任务 `EndActionOnAnimationEnd=true`（或等价阻塞）。  
- 空拍 Delay **不要**从对话树开跑就与黑幕/漫画淡出重叠。  
- Prepare 白名单（若做拍1）；**禁止**名字广扫 Panel。  
- **不要**只把 UIAlpha Duration 改成 0.5 却留着并行 BlackMask+YaerShow——体感仍会对不齐。

### 范围冻结

- **统一**：顺序 + 0.5s 间隔（与技术说明一致）  
- **可改建议**：`NewGameStory.prefab` 前奏节点结构与参数；必要时 NewGame 旁路与 Gate（若缺「只见 BG」则方案里写清）  
- **不改**：台本文案、漫画分页、进村已落地 Prefab（除非抽公共）、其它无关对话

---

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Project_context.md
@Assets/Doc/02_SYSTEM_SPEC.md
@Assets/Doc/OPEN_QUESTIONS.md
@Assets/Doc/技术文档/演出相关/Village_KenMuNiStart_开场分层显现_技术说明.md
@Assets/Doc/技术文档/演出相关/NewGameCartoonPanel漫画开场策划案.md
@Assets/Doc/执行文档/0806/NewGameStory_开场分层对齐KenMuNi标准_架构溯源报告.md
@Assets/Doc/提示词/0806/NewGameStory_开场分层对齐KenMuNi标准_架构侦探提示词.md
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Scene/NewGame/NewGameSceneManager.cs
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Scene/Village_KenMuNi/Village_KenMuNiSceneManager.cs
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Scene/Village_KenMuNi/VillageStartLayerRevealGate.cs
@Assets/Scripts/Game/GameRuntime/NodeCanvas/NodeCanvasNode/ActionTask/Village_KenMuNi/WaitVillageStartBgRevealActionTask.cs
@Assets/Scripts/Game/GameRuntime/NodeCanvas/NodeCanvasNode/ActionTask/NormalDialoguePanel/NormalDialogueUIAlphaAnimationTaskAction.cs
@Assets/GameRes/Prefabs/Dialogue/NewGameStory.prefab
@Assets/GameRes/Prefabs/Dialogue/Village_KenMuNiStart.prefab

你现在是【架构侦探】。Unity 2020.3.48f1 / C#。
禁止修改任何代码、Prefab、场景、图集、CSV、台本。只读扫描 + 写溯源报告。

---

## 背景（策划白话）

1. `NewGameStory` 开场已经能看到背景、大立绘、对话框的淡入淡出，但**时间间隔不对**。
2. 要对齐进村 `Village_KenMuNiStart`：**同一顺序 + 一律 0.5s**（见技术说明第三节）。
3. 开发者需要的是「改哪些 Prefab 字段 / 是否要改前奏结构」的最小清单，不是重做整段剧情。
4. 本期只溯源差距与改法；不施工。

---

## 必读 / 优先扫描线索

### A. 标准节奏勾选（对照技术说明 §三～§四）

逐条标 NewGame：**已有且值对 / 有但值错 / 缺 / 机制不同**

- [ ] 空拍 A：Wait + Hold **0.5**
- [ ] 大立绘：CanvasGroup Fade **0.5**（阻塞）
- [ ] 空拍 B：对话框 Delay **0.5**（立绘完成后再计）
- [ ] 对话框：Duration **0.5**（阻塞；按需 PrepareMask）
- [ ] 前奏串行（非 BlackMask+YaerShow+UIAlpha 并行）
- [ ] （可选拍1）全黑 Prepare + HideFade + Gate Signal；无则说明「只见 BG」能否单靠 Prefab 达到

### B. Prefab 对照表（必出）

对比 `Village_KenMuNiStart` vs `NewGameStory` **开场前奏**：

| 标准职责 | KenMuNi 节点与参数 | NewGame 现节点与参数 | 差距 |
|----------|-------------------|----------------------|------|
| 串/并行 | | executionMode=? | |
| 空拍 A Hold 0.5 | | | |
| 大立绘 Fade 0.5 | GoOut/Gusha… | YaerShow 或 YaerPainting？ | |
| 对话框 Delay+Dur 0.5 | | Delay=? Duration=? | |
| BlackMask | 前奏不用 | 若有：Duration=? | |
| Statement 衔接 | 前奏阻塞后 | | |

特别核实：`YaerShow` Mecanim 与标准 CanvasGroup Fade 是替代还是冲突；若保留 YaerShow，间隔如何等价 0.5。

### C. 现网时序图（必画）

```
漫画结束 → TriggerStory("NewGameStory") + BGM
  → 壳 Open
  → 前奏（标并行或串行、各 Duration/Delay）
  → 首句
```

标出：BG 可见瞬间、立绘开始/结束、对话框开始/结束、与标准 0.5 空拍的偏差（秒级估计即可）。

### D. 方案比选（以「拧旋钮对齐 0.5」为第一目标）

| 方案 | 摘要 | 能否对齐 0.5 | 风险 | 推荐？ |
|------|------|--------------|------|--------|
| A | Prefab 改串行：Wait→YaerPainting Fade0.5→UIAlpha Delay0.5+Dur0.5；去并行 BlackMask/YaerShow 主路径；旁路按需补 Gate | | | |
| B | 仅改现有 UIAlpha Duration 0.7→0.5，其余不动 | | 并行仍乱 | |
| C | 保留 YaerShow，只调 Clip/Delay 凑体感 | | 难精确 0.5、难维护 | |
| D | Prefab + NewGameSceneManager 旁路成对（对齐村拍1） | | 改动略大 | |

推荐须回答：开发者「只调间隔」时，**最小是否必须改结构（去并行）**，还是改参数就够。

### E. 施工员参数表（报告里直接可抄）

给出建议终值表，例如：

| 字段 | 建议值 |
|------|--------|
| HoldAfterBgVisibleSeconds | 0.5 |
| YaerPainting CanvasGroup Duration | 0.5 |
| UIAlpha Delay | 0.5 |
| UIAlpha Duration | 0.5 |
| EndActionOnAnimationEnd | true |
| ActionList executionMode | Serial |
| BlackMask / YaerShow | 从前奏主路径移除或后置（侦探定） |

### F. 范围与禁止

- **禁止**：改台本；拆漫画；为对齐重写全游戏开场系统；把间隔改成与标准不同的值（除非用户另拍板）；名字广扫 Panel。  
- **回归**：进村 `Village_KenMuNiStart`；DialogDebug 拖 `NewGameStory`；点击推进不因改前奏永久卡死。

---

## 侦探任务清单

1. **结论一句话**：间隔不对的根因（并行？参数？缺 Wait？）；推荐方案（A/B/C/D）。

2. **标准 vs 现网时序图**（两列对照）。

3. **Prefab 对照表 + 参数差距表**。

4. **方案比选**（至少 3 档，推荐 1 个）— 明确「只改数字够不够」。

5. **施工员最小改动清单**（文件级 + 上表终值）。

6. **验收清单**  
   - 新游戏：漫画后 → **BG → 0.5 空拍 → 大立绘 0.5 → 0.5 空拍 → 框 0.5 → 首句**（肉眼对齐进村）。  
   - 分层期间无离谱并行叠化。  
   - DialogDebug 拖同 Prefab 节奏大体一致、不卡死。  
   - 进村开场无回归；点击仍可推进。

7. **开放问题**追加 OPEN（「NewGameStory 开场间隔对齐 KenMuNi · 2026-08-07」）：  
   - 是否丢掉前奏 BlackMask / YaerShow？  
   - NewGame 是否必须补 Gate 旁路才有「只见 BG」？  
   - 0806「已施工」与现网并行是否回退/误标？

8. **禁止**：改代码；擅自把标准改成非 0.5；把「间隔不对」写成「玩家错觉」而不出参数表。

---

## 输出要求

写入：`Assets/Doc/执行文档/0807/NewGameStory_开场间隔对齐KenMuNi_架构溯源报告.md`

报告结构固定（中文、大白话优先）：

① 结论一句话（根因 + 推荐方案）  
② 原因（生活类比：灯光 cue 有了，节拍器没对准）  
③ 用户需要做什么（拍板是否去并行 / 是否补旁路 + 验收）  
④ 给程序看的补充：时序对照、Prefab 参数表、方案表、终值表、最小改动、开放问题

完成后用 MASTER 固定四段式口头汇报；详细以报告文件为准。
```

---

## 施工员续跑（侦探报告拍板后再开）

> 本阶段先不要贴。等确认：① 只改参数还是必须改串行结构 ② BlackMask/YaerShow 去留 ③ 是否补 NewGame Gate 旁路 后再写。

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/技术文档/演出相关/Village_KenMuNiStart_开场分层显现_技术说明.md
@Assets/Doc/执行文档/0807/NewGameStory_开场间隔对齐KenMuNi_架构溯源报告.md
@Assets/Doc/OPEN_QUESTIONS.md

你现在是【施工员】。按上述溯源报告与 KenMuNi 技术说明，做最小化修改，使 NewGameStory 开场分层顺序与间隔（一律 0.5s）与 Village_KenMuNiStart 一致。
优先改 Prefab 前奏参数/串行结构；旁路仅在报告认定「只见 BG」必需时补。禁止在 Update 堆补丁。每次说明：改了哪些字段到何值、如何验证与进村开场一致。
```
