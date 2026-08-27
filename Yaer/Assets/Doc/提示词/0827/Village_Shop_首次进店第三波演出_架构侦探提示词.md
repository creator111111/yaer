# Cursor Agent Prompt · Village_Shop 首次进店 · 第三波（雅古立绘淡入 + 结束黑幕放慢）

> **角色**：【架构侦探】只读溯源 → 输出 **最小修复清单** 给【施工员】  
> **日期**：2026-08-27（第二波 R1/R2 修复后 · 策划/验收新需求）  
> **场景**：`Village_Shop.unity` · 进店 `Door_Shop`  
> **对话 Prefab**：`Village_ShopStart.prefab`  
> **前置报告**：`0827/Village_Shop_首次进店第二波遗留_架构溯源报告.md`（v1.0）  
> **本阶段**：只读；禁止改代码 / 场景 / Prefab

把下面「侦探」整段复制给 Cursor Agent（Agent Mode）执行。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 本轮仅查两项（与 R1/R2 独立）

| # | 需求 | 验收标准 |
|---|------|----------|
| **P1** | **开头入场：雅儿 + 古莎大立绘淡入** | 换场黑幕淡出后，左侧两位主角 **可见地从 alpha 0 渐入**（非瞬间出现）；与 0629「左侧两女主」构图一致 |
| **P2** | **剧情结束：黑幕淡入淡出放慢** | `Village_ShopStart` 对白结束 → 黑幕 **更慢** 盖住 → 再慢速淡出露出 `UI_Shop`；体感明显慢于换场黑幕/当前默认 |

### 磁盘预扫（2026-08-27 · 施工后现状）

| 项 | 预扫结论 |
|----|----------|
| `Village_ShopStart` 图 Node1 | **已有** `CanvasGroupAlphaActionTask`：`GoOutStoryYaerPainting` + `GushaPainting`，`0→1`，**Duration=1.0s**，`EndActionOnAnimationEnd=true` |
| KenMuNi 对照 | 有 **`WaitVillageStartBgRevealActionTask`**（BG 亮后再空拍）+ GSM **`PrepareVillageStartDialogueUiForDeferredCover`** 把两 Painting **alpha 置 0** |
| Shop DeferCover | **已有** `TryDeferBlackFadeForCover`；**未见** 等价 Prepare / LayerRevealGate |
| 结束黑幕 | `OnShopStartStoryEnd` → `ShowShopBlackFade` → `CloseFormFade` |
| `BlackPanel.prefab` | `BlackMask.showTime=1`，`hideTime=1.35`（全局 Animator 速度） |
| `ShowBlackFormArgs` | **无** 自定义时长字段 —— 结束黑幕 **无法** 单独放慢 unless 扩展 API 或运行时改 `BlackMask` |

### P1 关键推断（待 Play 证伪）

**「图里已有淡入 Action，但肉眼看不到淡入」** 常见原因：

1. **初始 alpha≠0**：Prefab 实例 CanvasGroup 默认 1 → 淡入从 1→1 无效  
2. **黑幕时序**：`CloseFormFade`（换场）与 Node1 淡入 **并行或顺序错** —— 淡入在黑幕下播完，亮屏时已是 alpha=1  
3. **缺 KenMuNi 式 Prepare**：Shop GSM **未** 在 Trigger 前把两 Painting 置 0  
4. **缺 Wait 节点**：KenMuNi 有 `WaitVillageStartBgRevealActionTask`；Shop **无** —— 无法在「背景已亮、立绘仍 0」空拍后再淡入  
5. **BB 变量未绑**：`GoOutStoryYaerPainting` / `GushaPainting` CanvasGroup 引用为空 → Action 静默跳过（Console `[CanvasGroupAlpha] 未找到`）

**目标时序（0629 + KenMuNi 分层亮屏简化版）**：

```
全黑 DeferCover Trigger
  → 合层/商店背景就绪
  → CloseFormFade（换场黑幕淡出，用户看见店背景）
  → [可选短 hold]
  → 雅/古 CanvasGroup 0→1 淡入（1s 或策划指定）
  → 对话框 alpha 渐入
  → 首句对白
```

### P2 关键推断

| 段 | 现网 | 需求 |
|----|------|------|
| 结束 **淡入**（ShowFade） | ~1s（`showTime`） | **更慢**（建议侦探给出 1.5～2.5s 参考区间 + 是否仅 Shop 生效） |
| 结束 **淡出**（CloseFormFade→HideFade） | ~1.35s（`hideTime`） | **更慢** |
| 换场黑幕 | 同上全局 Prefab | **勿误改**全局 —— 须 **Shop 结束专用** 参数 |

**修复方向候选**：

| 方案 | 做法 | 风险 |
|------|------|------|
| **A** | `ShowBlackFormArgs` 增 `showTime`/`hideTime` 可选字段 → `BlackFormLogic` 临时改 `BlackMask` | 最小 API 扩展，可复用 |
| **B** | `Village_ShopSceneManager` 常量 + OpenUI 后 `GetComponent<BlackMask>()` 设速 | 不动 Args，略 hack |
| **C** | 单独 Shop 用 BlackPanel 变体 Prefab | 维护两份 Prefab |
| **D** | 改全局 `BlackPanel` show/hide | **否决** —— 影响所有换场 |

### 严禁

- 把 P1 当成「图里没 Action 要新建」—— **先查时序与 alpha 初值**  
- 把 P2 改成改全局 BlackPanel 默认值  
- 动 CSV / 表情三轨 / F3 Mask（已修）  
- 侦探阶段改代码

---

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/02_SYSTEM_SPEC.md
@Assets/Doc/执行文档/0827/Village_Shop_首次进店第二波遗留_架构溯源报告.md
@Assets/Doc/执行文档/0827/Village_Shop_首次进店验收失败_架构溯源报告.md
@Assets/Doc/执行文档/6月/0629/商店系统_策划拆解_执行说明.md
@Assets/GameRes/Prefabs/Dialogue/Village_ShopStart.prefab
@Assets/GameRes/Prefabs/Dialogue/Village_KenMuNiStart.prefab
@Assets/GameRes/Prefabs/UI/BlackPanel.prefab
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Scene/Village_Shop/Village_ShopSceneManager.cs
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Scene/Village_KenMuNi/Village_KenMuNiSceneManager.cs
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Scene/Village_KenMuNi/VillageStartLayerRevealGate.cs
@Assets/Scripts/Game/GameRuntime/NodeCanvas/NodeCanvasNode/ActionTask/Village_KenMuNi/WaitVillageStartBgRevealActionTask.cs
@Assets/Scripts/Game/GameRuntime/NodeCanvas/NodeCanvasNode/ActionTask/Common/CanvasGroupAlphaActionTask.cs
@Assets/Scripts/Game/GameRuntime/NodeCanvas/NodeCanvasNode/ActionTask/NormalDialoguePanel/NormalDialogueUIAlphaAnimationTaskAction.cs
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Black/BlackFormLogic.cs
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Black/ShowBlackFormArgs.cs
@Assets/Scripts/Game/GameRuntime/UI/Control/BlackMask.cs
@Assets/Scripts/Game/GameRuntime/UI/Component/BlackFade/BlackFadeComponent.cs

你现在是【架构侦探】。Unity 2020.3.48f1 / C#。
禁止修改任何代码、场景、Prefab。只读 + 写「第三波演出时序报告 + 最小修复清单」。

---

## 背景

0827 商店首次进店：R1 DeferCover、R2 表情等已在施工/验收中。
策划/验收 **第三波** 仅提 **演出 polish**：

1. **P1**：开头雅儿（`GoOutStoryYaerPainting`）+ 古莎（`GushaPainting`）**大立绘淡入**  
2. **P2**：整段 `Village_ShopStart` **结束后** 黑幕 **淡入+淡出放慢**

请只查 P1/P2，输出可施工最小 diff；**勿回归** R1/R2/F3 已修项。

---

## 侦探任务清单

### A. Play 复现（新档首次进店 · 必做）

记录 **肉眼 + Console**：

| 时刻 | 观察项 |
|------|--------|
| 换场黑幕淡出 | 店背景何时可见 |
| 雅/古大立绘 | **是否瞬间出现** vs **1s 渐入** |
| 对话框 | `NormalDialogueUIAlpha` 与立绘谁先谁后 |
| 对白结束 | 黑幕淡入/淡出 **各约几秒**（秒表或录屏） |
| UI_Shop 出现 | 是否在黑幕 **完全淡出后** |

Console：`[CanvasGroupAlpha]` · `[ShopStart]` · `[VillageStart]` · `BlackPanel`

### B. P1 · 雅/古开头淡入

#### B1. 磁盘核对

1. `Village_ShopStart` 图 **节点顺序**（FightingPanel → 立绘淡入 → 对话框 alpha → 首句）画出链路图。  
2. 两 Painting 实例 **CanvasGroup 初始 alpha**（Prefab 序列化 + 运行时 Trigger 后、CloseFormFade 前）。  
3. BB 变量 `GoOutStoryYaerPainting` / `GushaPainting` 是否 **已拖引用**。  
4. 与 `Village_KenMuNiStart` diff：Wait 节点、Duration（KenMuNi 0.5s vs Shop 1.0s）、Prepare 白名单。

#### B2. 时序核对（DeferCover 后）

1. `TryDeferBlackFadeForCover` → `onStoryTriggered` → `ShopStartStoryReadyHoldSeconds(0.15s)` → `CloseFormFade` **发生在图 Node 哪一步之前/之后/并行**？  
2. Node1 淡入若 `EndActionOnAnimationEnd=true`，图 **是否阻塞** 到淡入结束才进对话框？  
3. Shop 是否 **缺少** KenMuNi 的：  
   - `PrepareVillageStartDialogueUiForDeferredCover`（Painting alpha=0）  
   - `VillageStartLayerRevealGate` + `WaitVillageStartBgRevealActionTask`  
4. **根因裁定**：看不见淡入是 **初值 / 时序 / BB 空 / Action 未跑** 哪一种（可多选）？

#### B3. 修复方案（须给推荐 + 备选）

| 方案 | 内容 | 改动类型 |
|------|------|----------|
| 最小 | GSM Trigger 前 **Painting alpha=0**（抄 KenMuNi Prepare 子集） | 代码 |
| 时序 | DeferCover **CloseFormFade 延后** 到「背景亮且 Painting 仍为 0」 | 代码 + 可选 Gate |
| 图 | 增 **Wait** 节点（Shop 专用或复用 KenMuNi Task） | NodeCanvas |
| 图 | 调 Duration / Delay（如 1.0→1.5s、Delay 0.3s） | Prefab 图 |
| 初值 | Prefab CanvasGroup **序列化 alpha=0** | Prefab |

**验收通过标准**：换场黑幕淡出后，**连续可见** 雅/古 **至少 0.8s 的 alpha 上升**（非瞬现）。

### C. P2 · 结束黑幕放慢

#### C1. 现网链路

1. `OnShopStartStoryEnd` → `ShowShopBlackFade` → `onShowEnd` 内 `ShowShopUiRoot` + `CloseFormFade` —— 画出 **ShowFade / HideFade** 各段耗时来源。  
2. `BlackMask.showTime` / `hideTime` 与 Animator `Show`/`Hide` clip 长度关系（`animator.speed = 1/showTime`）。  
3. `ShowBlackFormArgs` 是否 **无法** 传自定义时长 —— 确认 API 缺口。

#### C2. 范围

- **仅** Shop 首次对白结束黑幕放慢，还是 **所有** `ShowShopBlackFade`？  
- **换场** `LoadScene` 黑幕 **必须不变** —— 侦探须明确隔离方案。

#### C3. 修复方案（须推荐具体秒数区间）

| 方案 | 说明 |
|------|------|
| A · 扩展 `ShowBlackFormArgs` | 可选 `float? showDuration` / `hideDuration` |
| B · Shop 常量 + 运行时设 `BlackMask` | `ShopEndBlackFadeInSeconds` / `OutSeconds` |
| C · 结束黑幕 **ShowFade 回调内延迟** 再显 UI | 仅拉长「全黑停留」，不改 Animator |

给出 **建议值**（如淡入 2.0s、淡出 2.0s）及 **是否需全黑 hold**（如 0.3～0.5s）。

**验收通过标准**：结束黑幕 **明显慢于** 进村换场；UI_Shop 仍在黑幕淡出 **之后** 完整出现。

### D. 目标时序图（修完后 · 须输出 mermaid）

```
进店(全黑) → DeferCover Trigger → 背景就绪 → CloseFormFade
  → [P1] 雅/古 0→1 淡入 → 对话框渐入 → 对白
  → 对白结束 → [P2] 慢速 ShowFade → (可选 hold) → 显 UI_Shop → 慢速 HideFade
```

### E. 最小修复清单（P0/P1 · 给施工员）

| 优先级 | 项 | 类型 | 文件 | 动作 |
|--------|-----|------|------|------|
| P0 | P1 | | | |
| P0/P1 | P2 | | | |

### F. 回归清单（修完后）

| # | 操作 | 通过标准 |
|---|------|----------|
| 1 | 新档首次进店 | R1 仍不闪店；**P1** 雅/古可见淡入 |
| 2 | 播完 Village_ShopStart | **P2** 结束黑幕慢于换场；UI 正常 |
| 3 | 二进宫 | 无对白；换场黑幕 **速度未变** |
| 4 | ID5/ID34 店句 | R2 表情 **未回归** |

### G. 开放问题

- P1 淡入时长：沿用图内 **1.0s** 还是对齐 KenMuNi **0.5s** 或策划 **1.5s**？  
- P1 是否需 **仅雅/古** 淡入，老板娘合层 **始终可见**（0629 右侧已在场景中）？  
- P2 是否要 **全黑停留** 再淡出，还是仅拉长 fade 曲线？

---

## 输出要求

写入：`Assets/Doc/执行文档/0827/Village_Shop_首次进店第三波演出_架构溯源报告.md`

结构（MASTER 四段式）：

① **结论一句话**（P1 根因 + P2 推荐方案与秒数）  
② **原因**（时序图 + KenMuNi 对照 + BlackMask 机制）  
③ **验收复测清单**  
④ **给程序**：P0/P1 施工表 + 目标时序 mermaid

口头汇报同样四段式。
```

---

## 施工员续跑（侦探报告拍板后再贴）

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0827/Village_Shop_首次进店第三波演出_架构溯源报告.md
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Scene/Village_Shop/Village_ShopSceneManager.cs
@Assets/GameRes/Prefabs/Dialogue/Village_ShopStart.prefab
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Black/ShowBlackFormArgs.cs
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Black/BlackFormLogic.cs

你现在是【施工员】。严格按第三波报告 P0→P1 修复，**只动 P1 立绘淡入 + P2 结束黑幕**，不回归 R1/R2/F3。

必须遵守：
- P1：优先 KenMuNi Prepare（alpha=0）+ 时序/Gate；图内 Action 已存在则 **勿重复造轮子**；
- P2：结束黑幕 **Shop 专用** 放慢；**禁止**改全局 BlackPanel 默认 show/hide 影响换场；
- 代码含详细注释；修一条验一条。

提交说明：P1/P2 各修复点、建议秒数实测、回归表结果。
```
