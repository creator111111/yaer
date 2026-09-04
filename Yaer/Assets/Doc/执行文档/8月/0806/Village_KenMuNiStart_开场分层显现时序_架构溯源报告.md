# Village_KenMuNiStart 开场分层显现时序 — 架构溯源报告

**文档版本**：v1.0（2026-08-06）  
**文档性质**：【架构侦探】只读溯源（**禁止改代码 / Prefab / 场景 / 图集 / CSV / 台本**）  
**范围**：第一章进村 `Village_KenMuNiStart` **已接通且 0804 零漏缝已修**；本期只溯源「开场分层显现」——不要黑幕一抬 BG+框+人齐出。  
**依据**：
- `Assets/Doc/00_MASTER_PROMPT.md`【架构侦探】
- `Assets/Doc/提示词/0806/Village_KenMuNiStart_开场分层显现时序_架构侦探提示词.md`
- 上一轮：`0804/进村开场对话遮罩时序_禁止露景漏缝_架构溯源报告.md`（A′ 已施工）
- 现网：`Village_KenMuNiSceneManager` / `StoryComponentGSM` / `Village_KenMuNiStart.prefab` / `NormalDialogueNewPanel`

**Unity**：2020.3.48f1  

---

## ① 结论一句话

**现网「齐出」是双重叠加：① 前奏（立绘→对话框）全在 System 黑幕下幕后播完；② 淡出前 `SnapVillageStartDialogueOpaque` 把立绘/字幕一次性拉满 alpha——亮屏瞬间 BG+框+人全员砸脸。推荐方案 A（+Prefab 重排）：黑幕在「仅 BG 盖满村景」后立即淡出，废除对框/立绘的 Snap，亮屏下按「对话框 → 立绘」各约 1s 再进首句；BG 本身已是 Prefab 内 1920×1080 全屏 Image，可当拍1遮罩。**

---

## ② 原因（生活类比）

### 生活类比

舞台开幕：现在是**大幕（System 黑幕）一直拉着**，幕后布景、字幕条、演员全换好妆；大幕一拉开——布景、话筒、演员**同时站在台中央**。产品要的是：大幕先拉开露出**布景（BG）** → 过约一秒出**字幕条** → 再过约一秒**演员上场** → 再开始念台词。

上一轮 0804 为了「禁止先露空村」，故意把前奏塞进幕布后面，并用 Snap 保证亮屏时遮罩已满——**零漏缝对了，分层观感被顺手抹掉了**。本期是在零漏缝之上加「分层显现」，不是推翻进村换场。

### 现网为何看不见分层（钉死）

| 机制 | 代码/配置事实 | 对玩家的效果 |
|------|---------------|--------------|
| A′ 延迟淡出 | `TryDeferBlackFadeForCover` → Trigger → `onStoryTriggered` → `WaitForInvoke(1.8s)` → Snap → `CloseFormFade` | 前奏整段在黑幕后 |
| Prefab 前奏 | Node0 藏战斗立绘 → Node1 雅/古立绘 Alpha 0→1（Duration=0.7）→ Node2 对话框 UI Alpha 0→1（0.7）→ Node3 首句 | **顺序是立绘→框**，与产品「框→立绘」**反序**；且**无独立「出 BG」节点** |
| Snap | `SnapVillageStartDialogueOpaque`：`dialogueUICanvasGroup=1` + 名含 Painting/Bottom/subtitles 的 CanvasGroup=1（跳过 BlackMask） | 亮屏前把分层一次性抹平 |
| BG | Prefab 根下 `BG`：Image 1920×1080，**无 CanvasGroup、无 Fade 节点**，实例化后即 alpha=1 | 亮屏时 BG 已在；但与框、人同时露 |
| 对话壳 BlackMask | `NormalDialogueNewPanel/BlackMask` 默认 **alpha=0** | 不能指望它挡村景 |
| EndActionOnAnimationEnd | 立绘/对话框 Alpha 任务均为空 `{}`（≈false） | 淡入可能 fire-and-forget，树可提前进 Statement；靠 1.8s + Snap 兜底观感 |

### 现网 vs 目标时序图（必出）

```
【现网 · 玩家体感 = 齐出】
地图 → 黑幕进 Village_KenMuNi1
  → Ready 仍全黑
  → TryDeferBlackFadeForCover
  → TriggerStory("Village_KenMuNiStart")
  → Open NormalDialogueNewPanel（Middle）+ Instantiate Prefab
       · BG 已在（被 System 黑幕挡住）
       · onStoryTriggered 触发
  → 幕后前奏：立绘淡入(≈0.7) → 对话框淡入(≈0.7)（玩家看不见）
  → Wait 1.8s
  → Snap：立绘/字幕 alpha 强制=1
  → CloseFormFade（System 黑幕淡出）
       ★ 玩家此刻同时看到：BG + 对话框 + 立绘
  → 首句 Statement 可交互（可能已在幕后提前跑到）

【目标 · 分层舞台】
地图 → 黑幕进村（保留）
  → Ready 仍全黑 → TriggerStory（保留 A′ 零漏缝精神）
  → Prefab 实例化：BG=可见盖满；对话框 alpha=0；立绘 alpha=0
  →【拍1】CloseFormFade（或 BG 就绪后立刻淡出）→ 玩家只见对话场景 BG
  → 等待 ≈1s
  →【拍2】对话框显现（≈1s 淡入或间隔）
  → 等待 ≈1s
  →【拍3】人物立绘显现
  → 等待 ≈1s（或立绘落定立刻）← 待拍板
  →【正常】首句可点 / 打字机
  → onStoryEnd → 还控（homeDoor 锁镜头等不变）
```

**硬约束继承**：全程禁止裸露村景；保留进村 + 只播一次；禁止改回「地图只播对话不进村」。

---

## ③ 用户需要做什么（拍板 + 验收）

### 拍板（施工前）

1. **间隔**：严格 1.0s，还是 Serialized 可调（建议默认 **1.0**，村 SceneManager / Prefab Duration 可配）？  
2. **立绘后**：再等 1s 才给首句，还是立绘落定立刻可点？（建议：**落定立刻可点**，少一次空等）  
3. **DialogDebug** 拖同 Prefab：是否与正式进村同一套三拍？（建议：**是**——节奏挂 Prefab，进村旁路只改「黑幕相对拍1」）  
4. **BG**：是否必须再做一次 FadeIn？还是「黑幕淡出露出已就绪的全屏 BG」即算拍1？（建议：**后者**，少改节点；BG 已 1920×1080）

### 验收清单

1. 新档进村：肉眼可见 **BG →（≈1s）→ 对话框 →（≈1s）→ 立绘**，再可正常点对话。  
2. 三拍期间**看不到裸村景**（BG 盖满或等价遮罩；继承零漏缝）。  
3. 对白结束还控正常；再进村不重播。  
4. DialogDebug 同 Prefab：分层节奏一致（若拍板「一致」）。  
5. 其它场景 `LoadScene` 黑幕无回归。

---

## ④ 给程序看的补充

### 4.1 节点对照表

| 产品层 | 现网候选节点 / 组件 | 现 alpha 初值 | 现谁在改它 |
|--------|---------------------|---------------|------------|
| 背景 BG | `Village_KenMuNiStart` 根下 **`BG`**（Image 1920×1080，无 CanvasGroup） | Image a=1，始终 Active | **无人 Fade**；实例化即满；被 System 黑幕挡住至 CloseFormFade |
| 对话框 | `NormalDialogueNewPanel` 的 `dialogueUI.subtitlesCanvasGroup`（Bottom/字幕条）；Snap 还扫名含 `Bottom`/`subtitles`/`Subtitle` 的 CanvasGroup | 壳内 Bottom 等 Prefab 上多为 1；开场靠 Node2 `NormalDialogueUIAlphaAnimationTaskAction` Start→0 再淡到 1 | Prefab Node2（Duration=0.7）；**Snap 在淡出前强制=1** |
| 人物立绘 | Prefab BB：`GoOutStoryYaerPainting`、`GushaPainting`（CanvasGroup） | 母体 Prefab `m_Alpha:1`；Node1 执行时写 StartAlpha(≈0) 再 DOFade→1 | Prefab Node1 双 `CanvasGroupAlphaActionTask`（各 0.7）；**Snap 强制=1** |
| 对话内遮罩 | `NormalDialogueNewPanel/BlackMask` | **alpha=0** | Snap **刻意跳过**；默认不挡景 |
| System 黑幕 | `BlackPanel`（System 组） | 换场全黑 | `CloseFormFade`；压在 Middle 对话之上 |

### 4.2 Prefab 前奏树（已核实）

```
Node0  FightingPanelVisible（藏战斗立绘意图）
  → Node1  ActionList：YaerPainting Alpha 0→1 (0.7s) + GushaPainting Alpha 0→1 (0.7s)
  → Node2  NormalDialogueUIAlpha 0→1 (0.7s)   ← 对话框
  → Node3  StatementNodeEx 首句「好漂亮的村子。」
  → …
```

- **无 BG Fade 节点**。  
- **顺序 = 立绘 → 对话框**，与产品 **对话框 → 立绘** 相反。  
- 两立绘 `EndActionOnAnimationEnd` 为空；对话框 `EndActonOnAnimationEnd`（拼写如此）为空 → 可能不阻塞后续 Statement。

### 4.3 方案比选表

| 方案 | 做法摘要 | 玩家能否看见三拍 | 零漏缝 | 改动面 | 风险 | 推荐？ |
|------|----------|------------------|--------|--------|------|--------|
| **A** | 黑幕在「仅 BG 盖满」后淡出；框/立绘保持 0；亮屏下 Prefab 播框→立绘（各≈1s）；**废除/改写 Snap**（勿再拉满框与立绘） | **能** | **能**（靠全屏 BG） | 村 SceneManager 旁路 + Prefab 前奏重排/Duration | 须保证淡出瞬间立绘/框确为 0；黑幕淡出时长≠拍间隔时要另加 Wait | **推荐** |
| B | 全程用对话内 BlackMask 代替 System 黑幕分层 | 半透/分层时可能 | 须抬 BlackMask 并处理层级 | 动通用壳逻辑 | 易挡字、易回归其它对话 | 不推荐 |
| C | **仅**改 Prefab 顺序+1s，并废除 Snap，但仍等满前奏再 CloseFormFade | **不能**（仍在幕后） | 能 | 小 | 改完玩家仍齐出 | 单独不够 |
| D | SceneManager 三拍协程驱动显隐，Prefab 前奏 Duration=0 | 能（仅进村） | 能 | 旁路变胖 | DialogDebug 不一致；易与树抢 alpha | 次选 |
| **A+C** | A 的黑幕点 + C 的 Prefab 重排（实质落地形态） | **能** | **能** | 专用旁路+本 Prefab | 同 A | **落地推荐名** |

### 4.4 与 0804 Snap / PreludeCoverSeconds 的关系

| 项 | 0804 角色 | 本期建议 |
|----|-----------|----------|
| `SnapVillageStartDialogueOpaque` | 淡出前保证遮罩满，防漏边 | **必须删或改成反操作**：淡出前 **只保证 BG 满、框与立绘=0**（可改名 `PrepareVillageStartLayeredReveal`）。继续 Snap 满框/立绘 = 分层必败 |
| `VillageStartPreludeCoverSeconds = 1.8f` | 等幕后前奏播完 | **不应再等于「三拍总时长」**。改为：壳+Prefab 实例化 + BG 就绪后的短等待（或 `onStoryTriggered` 后下一帧/极短 hold）即 `CloseFormFade`；三拍时长交给 Prefab/亮屏后逻辑 |
| 超时 `1.8+3s` | 防永久卡黑 | 保留超时兜底，但超时回调**仍须**按「BG 盖满再淡出」准备显隐，避免裸村；可把超时改短（如壳失败 2～3s） |
| A′ 零漏缝精神 | 仍全黑时 Trigger | **保留**；只改「何时 CloseFormFade」与「淡出时哪些层可见」 |

### 4.5 「进入正常对话」闸门

- 首句 = Prefab `StatementNodeEx`（Node3）；打字机/点击在对话 UI 系统内。  
- `onStoryTriggered` = 壳 Open + `StartDialogue` 回调，**≠** 首句可点。  
- 现网因 Alpha 任务可能不阻塞，Statement 可能过早；分层施工时应：  
  - Prefab：框/立绘 Fade 的 **`EndActionOnAnimationEnd = true`**（或插入 Wait 节点），保证三拍完成前不进首句；  
  - 或临时关点击（`interactable=false` / 挡射线）至拍3结束——优先用树阻塞，少动通用壳。

### 4.6 施工员最小改动清单（只建议，不施工）

| 优先级 | 文件 | 做什么 |
|--------|------|--------|
| 1 | `Village_KenMuNiSceneManager.cs` | 缩短/取消「等满 1.8s 前奏」；**废除满不透明 Snap** → 淡出前：BG 满、Painting/Bottom/字幕=0；超时仍防卡黑 |
| 2 | `Village_KenMuNiStart.prefab` 图 | 重排前奏：**对话框 Alpha →（间隔/Duration≈1）→ 立绘 Alpha**；Duration 默认 1.0；Fade 须等结束再进 Statement；**不要动台本文案节点内容** |
| 3 | （可选）BG | 若拍板要「BG 也淡入」：给 `BG` 加 CanvasGroup + 首节点 Fade；否则靠黑幕淡出即拍1 |
| 不改 | 通用 `LoadScene` 默认契约；其它村对话 Prefab；台本文案；进村换场主链 | |

**禁止**：Update 轮询补洞；取消进村换场；重写整棵对话系统；破坏零漏缝（淡出瞬间框立绘必须已藏、BG 必须盖景）。

### 4.7 相关锚点

| 文件 | 要点 |
|------|------|
| `Village_KenMuNiSceneManager` | `TryDeferBlackFadeForCover` / `FinalizeVillageStartCoverAndCloseBlack` / `SnapVillageStartDialogueOpaque` / `VillageStartPreludeCoverSeconds` |
| `LoadSceneComponentGSM` | Ready 后问 `TryDeferBlackFadeForCover`，未接管走默认 hold |
| `StoryComponentGSM.OnStoryPrefabLoad` | Open 壳 → `onStoryTriggered` → `StartDialogue` |
| `NormalDialogueFormNewLogic` | `dialogueUICanvasGroup`；`BlackMask`；实例化 Dialogue 树 |
| `CanvasGroupAlphaActionTask` / `NormalDialogueUIAlphaAnimationTaskAction` | Prefab 淡入执行体 |
| `Village_KenMuNiStart.prefab` | `BG` + BB 立绘 + 前奏 Node0–2 |

### 4.8 开放问题（已追加 OPEN）

| ID | 问题 | 默认建议 |
|----|------|----------|
| Q1 | 间隔严格 1.0s 还是可调？ | 默认 **1.0**，Serialized / Prefab Duration 可调 |
| Q2 | 立绘后再等 1s 才首句，还是落定立刻可点？ | **落定立刻可点** |
| Q3 | BG 是否必须全屏盖村景？ | **是**；现网 `BG` 1920×1080 已满足，验收盯层级与淡出瞬间 |
| Q4 | DialogDebug 是否与进村同一套三拍？ | **是**（节奏在 Prefab；旁路只管黑幕点） |

---

## 施工员下一轮最小化清单（建议 · 待拍板后开）

1. 村旁路：BG 就绪且框/立绘为 0 → 再 `CloseFormFade`；去掉「满不透明 Snap」。  
2. Prefab：前奏改为 **框 → 立绘**，间隔/时长≈1s，Fade 阻塞后再进首句。  
3. 按 §③ 验收分层可见 + 零裸村 + 无换场回归。  
