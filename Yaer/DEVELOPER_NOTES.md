# 技术债与特殊逻辑说明

本文档记录与**战斗立绘在剧情衔接处闪烁**相关的设计背景与约束，供后续开发、评审与排障时查阅。

---

## 问题描述

在部分流程中（例如：「国王演出」对话结束后，**紧接着**进入战斗教学、主角自言自语等**下一段**对话），对话系统会在前一段树正常播完时触发 `StoryComponentGSM.OnStoryEnd()`。

`OnStoryEnd` 中原本会立即根据设置调用 `FightingFormLogic.UpdateBattleImageVisiable(true)`，在几乎同一时序内「恢复」战斗大立绘的显示。而下一段教学对话的加载/开启又会按既有约定**再次关闭**战斗立绘（对话开始前 `SetFightingBattleIllustrationVisible(false)`）。

两套逻辑在同一时间窗内「先开后关」，导致战斗立绘在屏幕上**抢跑一帧**或短暂露脸，玩家感知为**立绘闪烁**。根因是 **OnStoryEnd 的「立绘恢复」与「紧接的下一道对话」的时序未错开**，而非单纯某一 UI 开关写错。

---

## 解决方案

在 **`FightingFormLogic`** 中引入**时序缓冲**（特指从 **`OnStoryEnd` 触发的、应当显示战斗立绘** 这一路径）：

- 对「剧情结束、按设置要恢复立绘」的调用，增加 `fromStoryEndRestore`（或同等语义）分支：**不立即** `SetActive` / 应用立绘状态，而是启动协程，在可配置延迟（约 **0.3～0.5 秒**，序列化字段 `_storyEndBattleImageDelay`，默认约 0.4s）后再执行真正的显示与刷新（`ApplyBattleImageShowNow` 等）。

- 若在此延迟窗口内，**下一段对话已开始**并再次将战斗立绘置为不显示，则通过 `UpdateBattleImageVisiable(false)` / `CancelPendingStoryEndBattleImageShow()` **取消**未完成的协程，避免「延迟结束仍把立绘打开」的二次问题。

- 非「故事结束补显」路径（如设置项切换、`Show()`、界面初始化等）仍**立即**按条件显示/隐藏，不受该延迟影响，以免改变全局行为。

- 在 **`StoryComponentGSM`**, `OnStoryEnd` 中恢复立绘时须传入「故事结束补显」语义（例如 `SetFightingBattleIllustrationVisible(configData.showBattleImage, isStoryEndRestore: true)`），与对话**开始**时的 `false` 调用成对，形成明确契约。

> **维护警告**：若无充分回归测试，**不要随意删除**上述延迟、不要把 `OnStoryEnd` 恢复立绘**改回**仅单参数调用、也不要将延迟改为 0，否则易复现**剧情衔接处战斗立绘闪烁**类回归。相关实现处已配有中文注释，修改前请同步阅读 `FightingFormLogic` 与 `StoryComponentGSM` 中对应说明。

---

## 受影响模块：Dialogue 与战斗立绘的耦合点

| 侧 | 角色 | 说明 |
|----|------|------|
| **Dialogue / 剧情** | `StoryComponentGSM` | 在对话预制加载前统一关闭战斗立绘；在 `OnStoryEnd` 中按设置恢复，并通过第二参数触发**延迟**恢复路径。与 `NormalDialogueFormNewLogic` 在对话树结束链路上联动。 |
| **战斗立绘** | `FightingFormLogic` | 立绘显隐、血量/破衣等刷新；**新增**的延迟协程、取消与 `OnClose` / `Hide` 时的清理。 |
| **设置** | `SettingsConfigData.showBattleImage`、`SettingManager` | 是否允许显示大立绘；`OnStoryEnd` 恢复时读档与对话开始时的 `false` 成对。 |
| **场景/存档** | `ForestSceneData.homeDoorStoryComplete` 等 | 与 `UpdateBattleImageVisiable` 内 `targetVisible` 的与运算，门剧情未满足时本就不应显战斗立绘。 |

**耦合本质**：对话系统不直接画战斗立绘，但通过 **`OnStoryEnd` / 新对话 `OnStoryPrefabLoad`** 在同一流程里**间接驱动** `FightingFormLogic` 的显隐。缺少时间缓冲时，「结束一段」与「开始下一段」在帧序上可重叠，表现为立绘与对话 UI 的**显示冲突**；缓冲与可取消的协程即对此耦合点的显式工程化解。

---

## 相关文件（便于定位）

- `Assets/Scripts/Game/GameRuntime/UI/FormLogic/Fighting/FightingFormLogic.cs` — 延迟、`ApplyBattleImageShowNow`、`CancelPendingStoryEndBattleImageShow`、协程实现与注释。  
- `Assets/Scripts/Game/GameRuntime/GameSceneManager/Component/StoryComponentGSM.cs` — `OnStoryEnd`、`SetFightingBattleIllustrationVisible`、与对话开始处对立绘的关闭。

---

*文档随实现演进可增补案例（如其他「多段对话紧密衔接」关卡）；若将来改为统一由「所有对话都结束」再恢复立绘，可在此记录迁移决策与回滚条件。*
