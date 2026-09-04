# Village_KenMuNiStart 开场分层显现 — 技术说明

> 文档日期：2026-08-06  
> 状态：**已落地**（节奏现网试调 Hold/Duration/Delay = **0.5s**）  
> 范围：第一章进村对话 `Village_KenMuNiStart` 的「BG → 大立绘 → 对话框(+Mask 小头像)」分层显现；含零漏缝旁路、闸门、Prepare 白名单。

**关联执行文档（侦探溯源，勿当现网实现）**：
- `Assets/Doc/执行文档/0806/Village_KenMuNiStart_开场分层显现时序_架构溯源报告.md`
- `Assets/Doc/执行文档/0806/Village_KenMuNiStart_分层后小头像不显示_架构溯源报告.md`
- `Assets/Doc/执行文档/0806/Village_KenMuNiStart_对话框渐入渐出对齐_架构溯源报告.md`
- 前置：`Assets/Doc/执行文档/0804/进村开场对话遮罩时序_禁止露景漏缝_架构溯源报告.md`

**相关技术文档**：
- `Assets/Doc/技术文档/演出相关/NormalDialogueNewPanel_遮罩立绘对话头像_Prefab技术说明.md`
- `Assets/Doc/技术文档/演出相关/DialogDebug对话测试场景_技术说明.md`

---

## 一、结论

进村仍**全黑 Trigger**（继承 0804 零漏缝），黑幕在「仅 BG 盖满村景」后淡出；亮屏后按：

**BG → 空拍 → 大立绘 → 空拍 → 对话框 + Mask 小头像 → 首句**

节奏闸门与黑幕时长解耦；`Prepare` 只用白名单，**禁止**按名字广扫整棵 `NormalDialogueNewPanel`。

（早期侦探稿曾写「框先于立绘」；产品定稿顺序以本文为准。）

---

## 二、玩家体感时序（现网）

```
地图 → 黑幕进 Village_KenMuNi1
  → Ready 仍全黑
  → TryDeferBlackFadeForCover
  → TriggerStory("Village_KenMuNiStart")
       · VillageStartLayerRevealGate.Reset（锁闸）
       · Open 壳 + Instantiate Prefab（BG 已在，被黑幕挡住）
  → PrepareVillageStartLayeredReveal（白名单：字幕条=0，场景大立绘=0，BG Active）
  → CloseFormFade（拍1）
  → 黑幕淡完 → Gate.Signal（开闸）
  → Prefab：WaitVillageStartBgReveal（等闸门 + Hold）
  → 只见 BG 空拍 Hold
  → 场景大立绘并行 Fade
  → 空拍（对话框 Delay）
  → 对话框 UI Fade（Active 字幕条、清字、预亮 Mask 雅儿）
  → Statement 首句（写演员名 + 打字机）
  → …对白…
  → 结束：框/立绘约 0.7s 淡出（通用壳，未与开场成对对齐）
```

**硬约束**：禁止裸村景；同档只播一次；不改回「地图只播对话不进村」。

---

## 三、节奏参数（现网试调）

| 拍 | 含义 | 配置位置 | 现网值 |
|----|------|----------|--------|
| 拍1 | 黑幕淡出露 BG | `BlackMask.hideTime`（System） | ≈1.0（未改） |
| 空拍 A | BG 亮屏后再等 | Prefab `HoldAfterBgVisibleSeconds` | **0.5** |
| 拍2 | 大立绘淡入 | Prefab 立绘 `CanvasGroupAlpha.Duration` | **0.5** |
| 空拍 B | 立绘落定→对话框 | Prefab 对话框节点 `Delay` | **0.5** |
| 拍3 | 对话框淡入 | 同节点 `Duration` | **0.5** |

调节奏：只改 Prefab 上述字段。**不要**把空拍 Delay 从「对话树开跑」就开始计（会与黑幕淡出重叠，亮屏后几乎无「只见 BG」）。

DialogDebug 拖同 Prefab：闸门默认 Ready，仍跑 Hold 空拍，不会永久卡住。

---

## 四、Prefab 前奏节点

路径：`Assets/GameRes/Prefabs/Dialogue/Village_KenMuNiStart.prefab`

| 顺序 | 职责 | 类型要点 |
|------|------|----------|
| 0 | 藏战斗面板 | `FightingPanelVisible` |
| 1 | 等 BG 亮屏 + 空拍 | `WaitVillageStartBgRevealActionTask` |
| 2 | 雅儿/古莎场景大立绘并行淡入 | `ActionList` Parallel + `CanvasGroupAlpha`（`EndActionOnAnimationEnd=true`） |
| 3 | 对话框淡入 + 预亮 Mask | `NormalDialogueUIAlphaAnimationTaskAction`（`PrepareMaskAvatarOnFadeIn`，Yaer / Laugh） |
| 4+ | 首句 Statement… | `StatementNodeEx` |

Blackboard 场景立绘名：`GoOutStoryYaerPainting` / `GushaPainting`（**不是** `Bottom/Mask/...` 下同名节点）。

---

## 五、代码与资源锚点

| 文件 | 职责 |
|------|------|
| `Village_KenMuNiSceneManager.cs` | `TryDeferBlackFadeForCover`；`PrepareVillageStartLayeredReveal` 白名单；淡出完成 `Signal` |
| `VillageStartLayerRevealGate.cs` | `ResetForDeferredCover` / `SignalBgFullyVisible` / `IsBgFullyVisible` |
| `WaitVillageStartBgRevealActionTask.cs` | 等闸门 + Hold（超时 8s 兜底开闸） |
| `Village_KenMuNiStart.prefab` | 前奏树与 Hold/Duration/Delay |
| `NormalDialogueUIAlphaAnimationTaskAction.cs` | 渐入前 Active；清字；可选预亮 Mask |
| `DialogueTMPUGUI.cs` | 首句写入 `actorName` |
| `DialogueMaskAvatarPresenter.cs` | Activate 时 alpha&lt;1 则拉回 1 |
| `CanvasGroupAlphaActionTask.cs` | `Delay`；阻塞至动画结束 |
| `DialoguePreludeBuilder.cs` | 工具默认：藏战斗 → 立绘 → 对话框 |

**已废除（勿恢复）**：
- `SnapVillageStartDialogueOpaque`
- `GetComponentsInChildren` + `name.Contains(Painting|GoOut|…)` 广扫 Panel

---

## 六、关键设计说明

### 6.1 闸门为何存在

若立绘/空拍从树开跑就 `Delay`，会叠在 System 黑幕淡出上 → 亮屏后几乎没有「只见 BG」。  
现网：`HideFade` 完成 → `LoadSceneComponentGSM.onEndLoadingSceneEvent` → `Signal` → Wait 才开始 Hold。

### 6.2 Prepare 为何白名单

Mask 小头像与场景大立绘**同名**。广扫会把 Mask Painting `alpha=0`，Presenter 只 `SetActive` → 黑窗。  
只动：`dialogueUICanvasGroup` + `DialogueSceneContainer` 下场景立绘。

### 6.3 对话框渐入清字 + 预亮 Mask

- 字幕条须先 Active，否则 Fade 在幕后跑完 → 首句硬切。  
- Prefab 默认名「雅尔」会在空框露馅 → 渐入清空；首句再写名与正文。  
- Mask 在 `Bottom`（`subtitlesGroup`）下，预 `Apply` 后随父 CanvasGroup 同次淡入。

### 6.4 未采用的替代方案

| 方案 | 原因 |
|------|------|
| 立绘 Delay 从树开跑硬加到 2s | 与 `hideTime` 耦合 |
| Prepare 排除子树但仍广扫 | 其它开场易再漏；已拍板白名单 |
| 渐入时填好第一句 | 跨节点偷看 Statement，耦合大 |
| 结尾 0.7 与开场成对 | 动通用壳，另案 |

---

## 七、其它开场复用

1. 全黑 Trigger → Prepare **白名单**（字幕条 + 该 Prefab 场景立绘）→ `CloseFormFade`。  
2. Trigger 前 `Reset` 闸门；淡出完成 `Signal`。  
3. Prefab：`Wait…BgReveal` → 立绘 Fade → Delay 空拍 → 对话框 Fade（按需 PrepareMask）→ Statement。  
4. 禁止名字模糊匹配扫整棵对话壳。  
5. Fade 任务 `EndActionOnAnimationEnd=true`。

---

## 八、验收

| # | 步骤 | 期望 |
|---|------|------|
| 1 | **新档**进村 | BG → 空拍 → 大立绘 → 空拍 → 框+小头像 → 首句 |
| 2 | 分层期间 | 无裸村；无齐出 |
| 3 | 对话框刚出现 | 无残留空名；首句再出字 |
| 4 | Mask | 与对话框同出；换人切换 |
| 5 | 对白结束 | 还控正常；再进村不重播 |
| 6 | DialogDebug | 分层大体一致 |
| 7 | 其它换场 | 默认黑幕契约无回归 |

---

## 九、开放项

- 开场 0.5s 为试调值，定稿可再改 Prefab。  
- 结尾淡出 0.7 与开场成对对齐：可选另案，勿与本文绑死。
