# Cursor Agent Prompt · NewGameStory 开场分层显现对齐 Village_KenMuNiStart 标准

> **角色**：先【架构侦探】，报告通过后再定【施工员】方案  
> **日期**：2026-08-06  
> **范围**：序章 `NewGameStory` 开场显现节奏，**统一**到 `Village_KenMuNiStart` 已落地标准（顺序 + 时间间隔）。不改台本文案、不重做漫画流程本身。  
> **本阶段**：只对照标准 vs NewGame 现网差距，**不施工**

把下面整段复制给 Cursor Agent（Agent Mode）执行。本阶段**只溯源、不改代码**。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 产品目标（开发者已定）

`NewGameStory` 开场要与 `Village_KenMuNiStart` **同一套标准**：

- **顺序相同**  
- **时间间隔相同**（现网试调 Hold / Duration / Delay = **0.5s**）

标准真源（已落地技术说明，勿用早期「框先于立绘」侦探稿）：

`Assets/Doc/技术文档/演出相关/Village_KenMuNiStart_开场分层显现_技术说明.md`

### 标准玩家体感（必须对齐）

```
拍1  对话场景 BG 可见
  → 空拍 A（HoldAfterBgVisible = 0.5s）
拍2  场景大立绘并行淡入（Duration = 0.5s）
  → 空拍 B（对话框节点 Delay = 0.5s）
拍3  对话框 UI 淡入 + Mask 小头像同出（Duration = 0.5s）
  → Statement 首句（写演员名 + 打字机）
```

硬约束（从标准文档继承，NewGame 须等价满足）：

- 分层期间**禁止**先露「未遮罩的场景/角色齐出」再补 UI（体感零漏缝）。  
- Prepare **白名单**（字幕条 + 该 Prefab 场景立绘）；**禁止**按名字广扫整棵 `NormalDialogueNewPanel`。  
- Fade 任务 `EndActionOnAnimationEnd=true`（或等价阻塞）。  
- 空拍 Delay **不要**从对话树开跑就与黑幕/漫画淡出重叠（否则亮屏后几乎无「只见 BG」）。  
- 闸门：亮屏完成再开始 Hold（KenMuNi 用 `VillageStartLayerRevealGate`；NewGame 可复用或抽公共/专用等价物——侦探比选）。

### NewGame 现网漂移（侦探必须钉死）

| 层 | KenMuNiStart 标准 | NewGame 现网线索（须核实） |
|----|-------------------|----------------------------|
| 入口 | 地图黑幕进村 → `TryDeferBlackFadeForCover` → 全黑 Trigger | `NewGameSceneManager.OnEnterScene` → 开漫画 → `onFinishEvent` → `TriggerStory("NewGameStory")` + BGM；**无**村旁路 |
| Prefab | Wait闸门 → 立绘 Fade → Delay → 对话框 Fade(+PrepareMask) → Statement | `NewGameStory.prefab` 开场 ActionList：**BlackMask** + Mecanim `YaerShow` + `NormalDialogueUIAlpha`(Duration≈0.7, Delay≈0.5)，**未见** WaitBgReveal / 立绘 CanvasGroup 前奏对齐 |
| 顺序 | BG → 空拍 → **大立绘** → 空拍 → **对话框** | 现网更像 BlackMask/触发器/对话框并行或旧序——**须画现网时序图** |
| 间隔 | 一律 **0.5s** | 现网 0.7 / 0.5 等，与标准不一致 |
| Prepare/闸门 | 白名单 + Gate Signal | NewGame **很可能没有** |

**关键差异（开放问题）**：NewGame 前有**漫画遮罩**，未必走 System 换场黑幕延迟淡出。侦探须回答：漫画关完瞬间场景是否已裸露？分层旁路挂在「漫画结束前预 Trigger」还是「漫画结束后立刻 Prepare + 等价闸门」？

---

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Project_context.md
@Assets/Doc/02_SYSTEM_SPEC.md
@Assets/Doc/技术文档/演出相关/Village_KenMuNiStart_开场分层显现_技术说明.md
@Assets/Doc/技术文档/演出相关/NewGameCartoonPanel漫画开场策划案.md
@Assets/Doc/执行文档/0806/Village_KenMuNiStart_开场分层显现时序_架构溯源报告.md
@Assets/Doc/OPEN_QUESTIONS.md
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Scene/NewGame/NewGameSceneManager.cs
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Scene/Village_KenMuNi/Village_KenMuNiSceneManager.cs
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Scene/Village_KenMuNi/VillageStartLayerRevealGate.cs
@Assets/GameRes/Prefabs/Dialogue/NewGameStory.prefab
@Assets/GameRes/Prefabs/Dialogue/Village_KenMuNiStart.prefab

你现在是【架构侦探】。Unity 2020.3.48f1 / C#。
禁止修改任何代码、Prefab、场景、图集、CSV、台本。只读扫描 + 写溯源报告。

---

## 背景（策划白话）

1. 进村开场 `Village_KenMuNiStart` 的分层显现已经定标准并落地（见技术说明）。
2. 序章 `NewGameStory` 开场也要**统一同一标准**：顺序和时间间隔都一样（BG → 空拍 0.5 → 大立绘 0.5 → 空拍 0.5 → 对话框 0.5 → 首句）。
3. NewGame 前面有漫画，和进村黑幕路径不一样——侦探要找**等价挂点**，不要生搬硬套把漫画流程拆坏。
4. 本期只溯源差距与最小复用方案；不施工。

---

## 必读 / 优先扫描线索

### A. 标准清单勾选（对照技术说明 §二～§七）
逐条标 NewGame：**已有 / 缺 / 需等价物**

- [ ] 全黑或等价遮罩下 Trigger / Prepare  
- [ ] Prepare 白名单（字幕条 + NewGameStory 场景立绘名）  
- [ ] 亮屏完成闸门 → Wait + Hold 0.5  
- [ ] 场景大立绘并行 Fade 0.5（EndAction 阻塞）  
- [ ] 对话框 Delay 0.5 + Fade 0.5（清字、按需 PrepareMask）  
- [ ] 禁止名字广扫 Panel  
- [ ] DialogDebug 拖同 Prefab 不永久卡闸

### B. NewGame 现网时序（必画）
```
新游戏 → Load NewGameScene
  → OnEnterScene → Open NewGameCartoonPanel
  → 漫画播放/跳过
  → onFinishEvent → TriggerStory("NewGameStory") + BGM
  → ? 壳 Open / Prefab 前奏 / 场景是否已可见
  → 首句
```
钉死：漫画关闭瞬间、对话壳出现瞬间、大立绘出现瞬间、对话框出现瞬间。

### C. Prefab 对照表
对比 `Village_KenMuNiStart` vs `NewGameStory` 前奏节点：

| 标准职责 | KenMuNi 节点 | NewGame 现节点 | 差距 |
|----------|--------------|----------------|------|
| 藏战斗/其它 | | | |
| Wait BG + Hold 0.5 | | | |
| 大立绘 Fade 0.5 | | BB 立绘名？ | |
| 对话框 Delay+Fade 0.5 | | BlackMask / UIAlpha / YaerShow？ | |
| Statement | | | |

特别核实：NewGame 的 `NormalDialogueBlackMaskTaskAction`、`MecanimSetTrigger(YaerShow)` 与标准「CanvasGroup 立绘 Fade」是替代还是冲突。

### D. 旁路挂点比选（NewGame 专用）
候选（比较优劣，推荐一个）：

| 方案 | 挂点摘要 | 能否对齐 0.5 节奏 | 风险 |
|------|----------|------------------|------|
| A | 漫画结束前：仍遮罩时 Trigger + Prepare，关漫画≈拍1，再 Gate Signal | | |
| B | 漫画结束后：立刻全屏占位/对话 BG 盖景 + Prepare，再分层 | | |
| C | 仅改 Prefab 前奏参数/节点，SceneManager 不旁路 | | 易无「只见 BG」或露景 |
| D | 抽公共 `LayerRevealGate` + Prepare 工具，村与 NewGame 共用 | | 改动面略大 |

优先：**专用旁路或薄公共抽取**，勿破坏其它换场默认黑幕契约；勿把村逻辑硬编码进 NewGame 却无法 DialogDebug。

### E. Mask / 服装（相关但非主修）
NewGame 大立绘多为室内 Dress；若对齐时预亮 Mask，须注意 `yaerUseGoOutOnly` 等服装线（见 0806 Dress 提示词）——**本期主目标是分层节奏**；服装另案可交叉引用，勿喧宾夺主，但验收勿引入黑窗回归。

### F. 范围冻结
- **统一**：顺序 + 0.5s 间隔（与技术说明一致）  
- **可改建议**：`NewGameStory.prefab` 前奏；`NewGameSceneManager` 旁路；闸门复用/改名通用  
- **不改**：漫画分页内容、台本文案、进村已落地逻辑（除非抽公共）、其它无关对话 Prefab

---

## 侦探任务清单

1. **结论一句话**：NewGame 缺标准的哪几环；推荐挂点方案（A/B/C/D）。

2. **现网 vs 标准时序图**（必出，两列对照）。

3. **差距清单表**（组件/Prefab/参数逐项）。

4. **方案比选表**（至少 3 档，推荐 1 个）— 重点回答漫画路径如何等价「拍1 只见 BG」。

5. **施工员最小改动清单**（只建议）  
   - 是否复用 `WaitVillageStartBgRevealActionTask` / Gate（改名通用？）还是 NewGame 专用拷贝。  
   - Prefab 节点重排与 0.5 参数表。  
   - Prepare 白名单立绘名（NewGameStory BB 实际节点）。

6. **验收清单**  
   - 新游戏：漫画结束 → **BG → 0.5 空拍 → 大立绘 0.5 → 0.5 空拍 → 框+小头像 0.5 → 首句**（肉眼与进村开场一致）。  
   - 分层期间无裸景齐出；无对话框空名闪一下。  
   - BGM 仍在漫画结束后按现设计播放。  
   - DialogDebug 拖 `NewGameStory` 分层大体一致、不卡死。  
   - 进村 `Village_KenMuNiStart` 无回归；其它换场黑幕无回归。

7. **开放问题**追加 OPEN（「NewGameStory 开场分层对齐 KenMuNi 标准 · 2026-08-06」）：  
   - 漫画结束与拍1 的衔接：关漫画=露 BG，还是另需黑幕/占位？  
   - Gate / Wait 任务是否抽成通用名（去 Village 前缀）？  
   - NewGame 是否仍保留 BlackMask / YaerShow，还是改纯 CanvasGroup Fade？

8. **禁止**：改台本；拆毁漫画流程；在 Update 轮询补洞；名字广扫 Panel；为对齐重写全游戏对话开场系统；把间隔改回与标准不同的值（除非用户另拍板）。

---

## 输出要求

写入：`Assets/Doc/执行文档/0806/NewGameStory_开场分层对齐KenMuNi标准_架构溯源报告.md`

报告结构固定（中文、大白话优先）：

① 结论一句话（缺环 + 推荐方案）  
② 原因（生活类比：同一套舞台灯光 cue，换剧场入口）  
③ 用户需要做什么（拍板漫画衔接 + 验收）  
④ 给程序看的补充：时序对照、差距表、方案表、最小改动、开放问题

完成后用 MASTER 固定四段式口头汇报；详细以报告文件为准。
```

---

## 施工员续跑（侦探报告拍板后再开）

> 本阶段先不要贴。等确认「漫画结束如何等价拍1」与 Gate 是否抽通用后再写。

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/技术文档/演出相关/Village_KenMuNiStart_开场分层显现_技术说明.md
@Assets/Doc/执行文档/0806/NewGameStory_开场分层对齐KenMuNi标准_架构溯源报告.md
@Assets/Doc/OPEN_QUESTIONS.md

你现在是【施工员】。按上述溯源报告与 KenMuNi 技术说明，做**最小化修改**，使 NewGameStory 开场分层顺序与间隔（0.5s）与 Village_KenMuNiStart 一致。
保留漫画→对话衔接与 BGM；Prepare 白名单；禁止名字广扫。
禁止在 Update 堆补丁。每次提交说明：改了哪些文件、如何等价拍1、如何验证与进村开场一致。
```
