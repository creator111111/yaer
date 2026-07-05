# ChapterEndPanel：滚动字幕与 ESC 跳过（预设计）

> 文档日期：2026-04-27  
> 状态：**待实现** — 仅记录需求与实现要点，与当前 `ChapterEndFormLogic` 行为对齐。  
> 范围：**`ChapterEndPanel(Clone)`** 中 **`root`** 下 **`imgTextTalk_1` / `imgTextTalk_2`** 的「上滚 + 淡入淡出」连播，**以及 `root` 整体/标题等渐隐渐现**；统一要求**按 ESC 一键跳过**至合适结束状态（或下一段流程）。详见**第七节**。

---

## 一、现状（以工程代码为准）

| 项 | 说明 |
|----|------|
| **逻辑脚本** | `Game.GameRuntime.UI.FormLogic.ChapterEndPanel.ChapterEndFormLogic`（`Assets/Scripts/.../ChapterEndPanel/ChapterEndFormLogic.cs`） |
| **核心方法** | **`StartShowTalkText()`** → 依次/交错调用 **`ImgTextRunMoveUpAndFadeAciton`**，对 `imgTextTalk_1`、`imgTextTalk_2` 做位移与渐隐。 |
| **动画** | 起点/终点为 **`textStartNode` / `textEndNode`** 的 `localPosition`；**`GameActionMgr.runMoveToAction`** 线性上移；**`runFadeAction` + `runSequenceAction`** 做先淡入后淡出。 |
| **多句** | 通过 **`curShowTextCount`** 与 **`setImgTextSprite`** 换 `text_{序号}` 图；与 **`chapterEndTextNumData`** 中的本章节句数对齐后进入 **`OnTextShowFinsh`**。 |
| **输入** | 进入 **`OnOpen`** 时**未**为章节结束条单独开 ESC 跳过；`MapPanel` 打开后 **`MapFormLogic.SetAllowEscapeClose(false)`** 以禁止地图用 ESC 关界面（见同脚本）。 |

**结论**：当前滚动为 **DOTween/GameActionMgr 驱动的 Tween 链**；若要做 ESC 跳过，需在**同一条逻辑线**上能够**终止/快进**这些 Tween，并**收敛**到与正常播完一致的收尾（如调用 **`OnTextShowFinsh` 前应有统一入口**）。

---

## 二、产品需求：ESC 跳过滚动字幕

1. 在**章节结束、滚动字幕播放过程中**，玩家按 **ESC** 可**立即跳过**当前及剩余的字幕段（或按策划定义：只跳过**当前段**、**连按跳多段**、或**一次 ESC 到「全部播完等效状态」** —— **默认建议：一次 ESC 即跳过余下所有字幕，直接进入与正常播完后的同一后续**，避免状态分叉过多）。  
2. 跳过后：  
   - 屏幕表现应进入**无字幕滚动、或等效于播完**的状态（例如清理/隐藏 `imgTextTalk_1/2`、或瞬移到终点）；  
   - **后续流程**应能与 **`OnTextShowFinsh`** 中已有逻辑**兼容**（点亮地图目标点、渐隐 `maskBg` 等）— **尽量复用现成** `OnTextShowFinsh()`，**不要**另写一条平行收束线，以免地图高亮/菜单权限不一致。  
3. 若章节结束界面**不打开地图**的其它模式存在，**ESC** 的语义要一致，或与策划**另表**（本文默认当前流程：**地图已开 + 本面板叠在上层**）。

---

## 三、技术要点与注意点

### 3.1 Tween 与协程的清理

- 所有通过 **`GameActionMgr`** 跑在 `imgTextTalk_*`、`imgTitle` 等上的 **Tween/Sequence** 在跳过时应 **`Kill`/`DOKill`（若底层为 DOTween）** 或按 `GameActionMgr` 能力统一停止，避免**幽灵回调**在跳过后再触发 **`onComplete`** 导致**二次进** `OnTextShowFinsh`。  
- **`GameActionMgr.runDelayTimeAction`** 为序章/交错两条字幕开的延迟，跳过分支里同样需要**可取消**（若项目支持对返回值的 `Kill`/`SetLink` 到本界面 `gameObject`）。  

### 3.2 与 `InputComponentGM`、ESC 体系的关系

- **`BaseUIFormLogic`** 支持 **`SetAllowEscapeClose(allow)`** 时通过 **`InputComponentGM.onEscPressed`** 调 **`CloseFormOnEsc`**。章节结束在播字幕时，更合理的语义是**「ESC = 跳过」**而非**「关闭整个 ChapterEndPanel」**，故实现时**不宜**与「ESC 关界面」**混用同一路由**，建议：  
  - **专为本界面**在 **`Update`/`OnUpdate`** 或**专用输入通道**中检测 `KeyCode.Escape` / 项目内统一**跳过键**；或  
  - **复用** `InputComponentGM` 但**单独回调**到 `ChapterEndFormLogic.SkipRollingSubtitles()`，**不**调 `CloseFormOnEsc`（若策划要求 ESC=跳过且**不能**关界面）。  
- 需与 **地图** 的 ESC 互斥已存在：`mapLogic.SetAllowEscapeClose(false)`。跳过实现后仍应保证**不会误关地图**或**关错层级**。

### 3.3 状态机建议（避免与递归打架）

- 当前 **`ImgTextRunMoveUpAndFadeAciton`** 通过 **递归** 与 **`curShowTextCount++`** 推进。若不做重构，**跳过**时至少应：  
  - 将 **`curShowTextCount` 或等价计数** 置为「**已满足结束条件**」**再** 调 **`OnTextShowFinsh()`**（或把 **`OnTextShowFinsh` 抽成在「已停止所有 Tween 之后**仅调用一次`」`的 guard）；  
  - 并 **`hasEnd`** 等标志与正常结束路径**一致**，防止重复进 **`OnTextShowFinsh`**。  
- **替代方案**：将「单句轮播」改为**显式 for/while 状态机** + 可**一次性 break** 到 `Finished`，便于维护；改动面较大，**可选二版**。

### 3.4 可配置项（与策划对表）

| 项 | 建议 |
|----|------|
| **按键** | 默认 ESC；若主机/手柄需映射，走项目输入表。 |
| **按一次** | 跳过**全部**剩余句（推荐 M1）；或**只跳当前句**（需定义「下一句谁播」的衔接）。 |
| **音效/演出** | 是否保留最后一句 0.3s 渐隐、是否播放「唰」一声；初版可**硬切**到结束态。 |
| **调试** | 日志带上 `curChapterId`、`curShowTextCount`，便于对 `chapterEndTextNumData` 排错。 |

---

## 四、验收清单（预填）

- [ ] 播字幕中按 ESC，**无** Tween/延迟在跳过**之后**仍触发旧 `onComplete` 再进两次结束逻辑。  
- [ ] 跳过后，**`OnTextShowFinsh` 能且仅能**在「等价于自然播完」时执行一次。  
- [ ] 地图 **`SelectPlaceLight`** 等行为与**不按 ESC 播完**一致。  
- [ ] 已打开 `MapPanel` 时，ESC **不会**误关世界地图或打开菜单（依策划：若 ESC 只用于跳过，**暂时屏蔽**关地图/关 ChapterEnd 的误触直至策划改需求）。  
- [ ] 章节句数为 **0** 或资源缺失时的边界表现（不崩、不卡输入）。

---

## 五、与现有脚本的索引

| 脚本 / 方法 | 说明 |
|-------------|------|
| `ChapterEndFormLogic` | 章节结束主逻辑 |
| `StartShowTalkText` / `ImgTextRunMoveUpAndFadeAciton` / `OnTextShowFinsh` | 滚动与收尾，跳过逻辑宜在此**附近**接枝 |
| `GameActionMgr` | 位移/淡入淡出/Sequence/延迟，**清理**是跳过实现的关键 |
| `InputComponentGM`、**`SetAllowEscapeClose`** | 注意与「ESC=关界面」的**语义区隔** |
| `NodeCanvas` **`ChapterEndAction`** | 打开 `ChapterEndPanel` 的入口（若从剧情树进），非字幕本身 |

---

## 六、版本记录

| 版本 | 日期 | 说明 |
|------|------|------|
| 0.1 | 2026-04-27 | 首版：预设计 ESC 跳过滚动字幕、与 `ChapterEndFormLogic` 现状对齐、技术注意与验收项。 |
| 0.2 | 2026-05-08 | 增补 **`root` 渐隐渐现** 纳入 ESC 跳过范围（与滚动字幕同一语义）。 |

---

## 七、`root` 渐隐渐现与 ESC 跳过（新增需求）

### 7.1 层级与问题描述

`ChapterEndPanel(Clone)` 在 Hierarchy 中与章末演出相关的结构大致为：

- **`ChapterEndPanel(Clone)`**
  - **`maskBg`**：全屏遮罩/背景层。
  - **`root`**（本节重点）：章末标题与字幕区域的**父节点**，其下常见子节点包括：
    - **`imgTitle`**：章节标题图。
    - **`imgTextTalk_1` / `imgTextTalk_2`**：滚动字幕用图（与第二节、第三节已述逻辑一致）。
    - **`textStartNode` / `textEndNode`**：字幕位移参考节点。
  - **`Components`**：脚本等挂载节点。
  - **`imgBigTitle`**（或预制体中实际命名）：大标题等补充元素。

**现状问题**：除 **`imgTextTalk_*` 上滚 + 淡入淡出** 外，**挂在 `root` 或其子节点上的「整块渐隐渐现」动效**（例如标题/容器整体的 Alpha 或 CanvasGroup 动画，具体以 `ChapterEndFormLogic` 与 `GameActionMgr` 实际绑定为准）**当前同样无法用 ESC 跳过**。玩家在序章结束等流程中若希望快进，会被迫看完**滚动字幕 + root 侧渐隐渐现**两段不可中断的演出。

### 7.2 产品需求（策划口径）

1. **与第二节「ESC 跳过滚动字幕」同一按键、同一语义**：在 **`root` 相关渐隐渐现播放过程中**，按 **ESC** 应能**立即结束**该段渐隐渐现（或快进至与正常播完等效的可见/隐藏状态），**不得**仅跳过字幕而仍卡在 root 动效中途。  
2. **收束一致**：跳过后 `root` 下各 UI 的透明度/显隐应与**自然播完**一致，避免半透、叠图错误或阻挡后续地图点击。  
3. **与滚动字幕的先后关系**：若流程上为「先 root 渐现 → 再滚字幕」或交错，ESC **默认仍建议一次按键跳过「当前剩余的全部章末演出」**（含 root 动效 + 未播完字幕），仅保留**一条**进入 `OnTextShowFinsh` 或等价收尾的路径，避免分支爆炸；若策划坚持「只跳过字幕、root 必须播完」需**另开评审**并在本文档单独成条。  
4. **技术实现提示**（供程序对齐第三节）：对绑定在 **`root`、`imgTitle`** 或其它子节点上的 **`GameActionMgr` / DOTween** 序列，在跳过入口中**与 `imgTextTalk_*` 一并 Kill/收敛**，并注意 **`onComplete` 只触发一次**。

### 7.3 验收补充（在第四节基础上增加）

- [ ] **`root` 渐隐渐现进行中**按 ESC，动效**立即终止或等价播完**，无跳过后再触发旧回调导致闪屏/二次收尾。  
- [ ] 仅滚字幕结束、**`root` 动效仍播一半**的中间态不得作为稳定终态；终态与「全程不按 ESC」一致。  
- [ ] `maskBg`、地图 `MapPanel` 叠层与 ESC 语义仍满足第四节已有条目。
