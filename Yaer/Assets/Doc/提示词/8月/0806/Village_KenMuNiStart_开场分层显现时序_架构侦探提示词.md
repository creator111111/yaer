# Cursor Agent Prompt · Village_KenMuNiStart 开场分层显现：先 BG → 对话框 → 立绘

> **角色**：先【架构侦探】，报告通过后再定【施工员】方案  
> **日期**：2026-08-06  
> **范围**：第一章自动播 `Village_KenMuNiStart` **已接通且遮罩漏缝已修**；本期只改**开场显现节奏**——不要「黑幕一抬全员齐出」。  
> **本阶段**：只摸清现网前奏 / Snap / 黑幕淡出与 Prefab 节点显隐关系，**不施工**

把下面整段复制给 Cursor Agent（Agent Mode）执行。本阶段**只溯源、不改代码**。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 产品目标（开发者白话 · 已定）

第一章进村自动开场对话时，加载/显现要按**固定三拍**，拍与拍之间各隔约 **1 秒**，再进入正常打字/点击对话流程：

```
① 先出背景 BG
     ↓ 约 1 秒
② 再出对话框（字幕条 / Bottom 等对话 UI）
     ↓ 约 1 秒
③ 再出人物立绘（雅儿 / 古莎等场景大立绘）
     ↓ 约 1 秒（或立绘落定后立刻）
④ 进入正常对话流程（首句可点、打字机等）
```

**体感目标**：像舞台分层拉开，而不是黑幕淡出后 BG+框+人同时砸脸。

### 现网漂移（侦探必须对齐「上一轮施工」vs「本期产品」）

| 来源 | 说法 / 现状 | 与本期冲突点 |
|------|-------------|--------------|
| `0804/进村开场对话遮罩时序_…` | 方案 A′：仍全黑时 Trigger → 前奏幕后播完 → 再 `CloseFormFade` | **遮罩零漏缝仍要保留**；本期是在此之上加「分层显现」 |
| `Village_KenMuNiSceneManager` | `VillageStartPreludeCoverSeconds = 1.8f`；`onStoryTriggered` 后等前奏再淡出黑幕 | 黑幕下「幕后播完」再亮屏 → 玩家**看不见**分层节奏 |
| `SnapVillageStartDialogueOpaque` | 淡出黑幕前把立绘/字幕 CanvasGroup **瞬间提到 alpha=1** | 直接**抹掉** Prefab 前奏的淡入层次；亮屏瞬间全员齐出 |
| Prefab 前奏（上一轮报告） | 藏战斗面板 → 立绘 Alpha≈0.7s → 对话框 Alpha≈0.7s | **顺序可能是「立绘→对话框」**，与产品「先框后立绘」相反；且未必含独立「先出 BG」一步 |
| 产品新定稿 | **BG → 对话框 → 立绘**，各隔 ≈1s，再进正常对话 | 须重排节点、时机与黑幕关系 |

### 期望玩家时序（逻辑；实现方案由侦探比选）

```
地图点肯姆尼 → 黑幕进 Village_KenMuNi1（现有旁路保留）
  → 仍全黑或仅允许 BG 先可见（禁止裸露村景）
  →【拍1】对话场景背景 BG 显现
  → 等待 ≈1s
  →【拍2】对话框显现
  → 等待 ≈1s
  →【拍3】人物立绘显现
  → 等待 ≈1s（或立绘落定）
  →【正常流程】首句可交互 / 打字机推进
  → 对白结束 → 还控（镜头锁等与 homeDoor 现设计对齐）
```

**硬约束（继承 0804，不可回退）**：

- 全程**禁止先露村庄场景再补对话遮罩**（零漏缝仍有效）。  
- 保留进 `Village_KenMuNi1` + `TriggerStory("Village_KenMuNiStart")` + 同档只播一次。  
- **禁止**改回「点地图只播对话不进村」。

---

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Project_context.md
@Assets/Doc/02_SYSTEM_SPEC.md
@Assets/Doc/执行文档/0804/第一章进村插入Village_KenMuNiStart_架构溯源报告.md
@Assets/Doc/执行文档/0804/进村开场对话遮罩时序_禁止露景漏缝_架构溯源报告.md
@Assets/Doc/提示词/0804/进村开场对话遮罩时序_禁止露景漏缝_架构侦探提示词.md
@Assets/Doc/OPEN_QUESTIONS.md
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Scene/Village_KenMuNi/Village_KenMuNiSceneManager.cs
@Assets/GameRes/Prefabs/Dialogue/Village_KenMuNiStart.prefab

你现在是【架构侦探】。Unity 2020.3.48f1 / C#。
禁止修改任何代码、Prefab、场景、图集、CSV、台本。只读扫描 + 写溯源报告。

---

## 背景（策划白话）

1. 第一章开始会自动播 `Village_KenMuNiStart`，能播、遮罩漏缝也修过了，但**开场加载观感不对**。
2. 想要的顺序是：**先背景 BG →（隔约 1 秒）→ 对话框 →（隔约 1 秒）→ 人物立绘 →（隔约 1 秒）→ 再进正常对话**。
3. 怀疑现网要么在黑幕后把前奏播完再亮屏（玩家看不到分层），要么 `Snap…Opaque` 把大家一起拉满透明度，亮屏瞬间齐出。
4. 本期只溯源「分层显现」怎么挂、挂在 Prefab 前奏还是 SceneManager 旁路；**不施工**。

---

## 必读 / 优先扫描线索

### A. 钉死「现在亮屏时玩家实际看到什么」
- `TryDeferBlackFadeForCover` → `onStoryTriggered` → `WaitForInvoke(1.8s)` → `SnapVillageStartDialogueOpaque` → `CloseFormFade` 整条链
- 黑幕淡出瞬间：BG / 对话框 / 立绘各自 alpha、Active 是否已全为可见
- `SnapVillageStartDialogueOpaque` 扫了哪些节点名（Painting / Bottom / subtitles…），是否把「分层」一次性抹平

### B. Prefab 前奏树（NodeCanvas / ActionTask）
- `Village_KenMuNiStart.prefab` 开场序列：谁在 FadeIn、顺序、Duration
- 对话场景里的 **BG** 节点叫什么、挂在哪（Dialogue 实例下 Image？CanvasGroup？）
- 对话框 = `NormalDialogueNewPanel` 的 Bottom/字幕，还是 Prefab 内自带框？
- 立绘 = Prefab 内雅/古 `*Painting` CanvasGroup，还是面板内嵌？
- 现前奏是否为「立绘先、框后」——与产品「框先、立绘后」是否反序

### C. 黑幕 vs 可见分层（核心矛盾）
- System 级 `BlackPanel` 压在 Middle 对话之上：黑幕下播前奏 → **玩家看不见分层**
- 若要让玩家看见「BG→框→立绘」：黑幕必须在**拍1之前或拍1同时**开始淡出；但 **拍1 仅 BG 时仍须盖住村景**（BG 全屏？对话内 BlackMask？）
- 对照：对话壳 `BlackMask` 默认 alpha、是否可临时当遮罩

### D. 「进入正常对话」的闸门
- 首句 Statement / 打字机 / 点击推进从哪一帧开始可交互
- 能否在三拍完成前锁点击、禁打字机，三拍后再放行
- `onStoryTriggered` 是否过早（壳开了 ≠ 首句可点）

### E. 间隔 1 秒的挂点候选
1. 只改 Prefab 前奏节点顺序 + Duration=1（DialogDebug 同步受益）  
2. SceneManager 旁路分三拍 WaitForInvoke（仅进村观感；Debug 可另设）  
3. 新薄 ActionTask（如分步 Show BG / Box / Painting）  
4. 关掉 Snap，改「黑幕在拍1就绪后淡出，后续拍在亮屏下播」

### F. 范围冻结
- **保留**：进村换场、A′ 零漏缝精神、`StoryTriggerCountData` 只播一次、台本文案  
- **可改建议**：前奏顺序/时长、Snap 是否废除/延后、黑幕关闭时机相对三拍的位置  
- **不改**：其它村对话、通用 LoadScene 默认契约（若动须进村专用旁路）

---

## 侦探任务清单

1. **结论一句话**：现网为何体感「齐出/看不到分层」；推荐挂在 Prefab 前奏还是 SceneManager 旁路。

2. **现网 vs 目标时序图**（必出）  
   标出：TriggerStory、壳 Ready、BG/框/立绘各自可见时刻、Snap、CloseFormFade、首句可交互。

3. **节点对照表**（侦探在 Prefab/壳上钉死名字）

   | 产品层 | 现网候选节点 / 组件 | 现 alpha 初值 | 现谁在改它 |
   |--------|---------------------|---------------|------------|
   | 背景 BG | | | |
   | 对话框 | | | |
   | 人物立绘 | | | |

4. **方案比选表**（至少 3 档，推荐 1 个）

   | 方案 | 做法摘要 | 玩家能否看见三拍 | 零漏缝 | 改动面 | 风险 | 推荐？ |
   |------|----------|------------------|--------|--------|------|--------|
   | A | 黑幕在「仅 BG 盖满」后淡出，再亮屏播框→立绘（各 1s） | | | | | |
   | B | 全程亮屏前用对话内全屏遮罩代替 System 黑幕，遮罩下不可见则改半透/分层 | | | | | |
   | C | 仅改 Prefab 前奏顺序+1s，并废除/延后 Snap | | | | | |
   | D | SceneManager 三拍协程驱动显隐，Prefab 前奏 Duration=0 | | | | | |

5. **与 0804 Snap / PreludeCoverSeconds 的关系**  
   - Snap 是否必须删或改成「只 snap BG」？  
   - `1.8f` 是否应改为「三拍总时长 ≈3s + 余量」？  
   - 超时兜底如何避免卡黑又不打断分层。

6. **施工员最小改动清单**（只建议）  
   - 动哪些脚本 / Prefab 节点 / 是否动通用 LoadScene。  
   - 优先：**Village_KenMuNiStart / Village_KenMuNiSceneManager 专用**，勿破坏其它对话开场。

7. **验收清单**  
   - 新档进村：肉眼可见 **BG →（≈1s）→ 对话框 →（≈1s）→ 立绘**，再可正常点对话。  
   - 三拍期间**看不到裸村景**（继承零漏缝）。  
   - 对白结束后还控正常；再进村不重播。  
   - DialogDebug 拖同 Prefab：分层节奏是否一致（或注明「仅进村旁路有分层」）。  
   - 其它场景 `LoadScene` 黑幕无回归。

8. **开放问题**追加 `OPEN_QUESTIONS.md`（新开「Village_KenMuNiStart 开场分层显现 · 2026-08-06」）：  
   - 间隔严格 1.0s 还是可调（建议默认 1.0，Serialized）？  
   - 立绘出完后再等 1s 才给首句，还是立绘落定立刻可点？  
   - BG 是否必须全屏盖住村景（尺寸/层级）？  
   - DialogDebug 是否与正式进村同一套三拍？

9. **禁止**：改台本文案；取消进村换场；在 Update 轮询补洞；为分层重写整棵对话系统；破坏 0804 零漏缝。

---

## 输出要求

写入：`Assets/Doc/执行文档/0806/Village_KenMuNiStart_开场分层显现时序_架构溯源报告.md`

报告结构固定（中文、大白话优先）：

① 结论一句话（为何齐出 + 推荐方案）  
② 原因（生活类比：窗帘、布景、演员分步上场）  
③ 用户需要做什么（拍板间隔/可点时机 + 验收）  
④ 给程序看的补充：时序图、节点表、方案表、最小改动、开放问题

完成后用 MASTER 固定四段式口头汇报；详细以报告文件为准。
```

---

## 施工员续跑（侦探报告拍板后再开）

> 本阶段先不要贴。等确认「黑幕相对三拍的关闭点」与「间隔是否严格 1s / 立绘后是否再等 1s」后再写。

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0806/Village_KenMuNiStart_开场分层显现时序_架构溯源报告.md
@Assets/Doc/OPEN_QUESTIONS.md

你现在是【施工员】。按上述溯源报告做**最小化修改**，使 Village_KenMuNiStart 开场按「BG → 约1s → 对话框 → 约1s → 立绘 → 再进正常对话」显现，且不露裸村景。
保留进村换场与只播一次；优先 Prefab/村 SceneManager 专用旁路。
禁止在 Update 堆补丁。每次提交说明：改了哪些文件、三拍如何保证、如何验证。
```
