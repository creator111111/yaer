# ForestScene 士兵回头-国王出场演出系统说明

## 1. 目标与范围

本文档用于说明 `ForestScene` 首次入场剧情中“士兵回头 -> 国王出场 -> 莱伊起飞”这条演出链路的实现方式、关键参数与改动建议。

适用对象：
- 程序：需要调整时序、触发逻辑、动画事件时。
- 策划/技术美术：需要调整演出节奏（谁先动、间隔多久、何时播音效）时。

---

## 2. 关键文件

- 演出逻辑脚本：`Assets/Scripts/Game/GameRuntime/Entities/SceneEntities/ForestScene/ForestSceneLaiFlyStory.cs`
- 触发入口：`Assets/Scripts/Game/GameRuntime/Entities/SceneEntities/ForestScene/HomeDoorStoryTriggerLogic.cs`
- 对话/图节点资源：`Assets/GameRes/Prefabs/Dialogue/ForestSceneLaiFlyStory.prefab`
- 场景管理（首进条件、相机/面板联动）：`Assets/Scripts/Game/GameRuntime/GameSceneManager/Scene/Forest/ForestSceneManager.cs`

---

## 3. 触发链路（从进门到国王出场）

1. 玩家进入门口触发器（`HomeDoorStoryTriggerLogic`）。
2. 若 `ForestSceneData.homeDoorStoryComplete == false`，触发剧情：
   - `TriggerStory("ForestSceneLaiFlyStory")`
3. 剧情图（NodeCanvas）执行到对应 Action 节点时，调用 `ForestSceneLaiFlyStory` 的方法：
   - `PreparePlay()`：准备“士兵回头 + 国王/莱伊激活”
   - `BeginSoldierTurnEarly()`（可选）：若要让士兵更早开始回头，可在图中提前单独调用
   - `LaiFly()`：播放莱伊起飞动画

---

## 4. 核心时序（当前实现）

### 4.1 PreparePlay 主流程

`PreparePlay()` 内部会启动协程 `CoPreparePlayAfterSoldierLead()`：

1. 若 `soldierTurnAnimator == null`：
   - 直接激活 `king`、`lai`，关闭 `NormalLai`
   - 立即执行 `PlayKingShowAnim()`
   - 该分支用于防止空引用导致演出中断（兜底）
2. 若有士兵动画器：
   - 若之前未触发过士兵回头，则触发 `StartSoldierTurn()`
   - 等待 `soldierTurnLeadBeforeKingSeconds` 秒
   - 激活 `king`、`lai`，并关闭 `NormalLai`

### 4.2 士兵回头与国王行走的衔接

- `StartSoldierTurn()` 只会触发一次（受 `soldierTurnTriggered` 保护）。
- 士兵回头动画结束时，由动画事件回调 `OnSoldierTurnEnd()`，再调用 `PlayKingShowAnim()` 播放国王行走。

这意味着：
- **“角色激活时间”** 由协程等待参数控制；
- **“国王真正开始走”** 由士兵回头动画事件控制。

---

## 5. 关键参数说明

### `soldierTurnLeadBeforeKingSeconds`（默认 1.5）

定义：士兵开始回头后，等待多久再激活国王/莱伊对象。

设计原因：
- 将“开始转头”和“角色出现”解耦，便于调节镜头节奏。
- 即使国王最终行走由 `OnSoldierTurnEnd` 驱动，也可先激活对象用于提前入镜、准备特效或立绘切换。

调参建议：
- 感觉“太突然”：适当增大（例如 `1.8~2.2`）。
- 感觉“拖沓”：适当减小（例如 `1.0~1.3`）。
- 若要在不改 `PreparePlay` 调用时机的前提下让士兵更早 1 秒回头，可在图里提前调用 `BeginSoldierTurnEarly()`。

---

## 6. 音效与动画事件

脚本中注册了两个动画事件组件：

- `kingShowAniEventCpn.RegisterEvent("ShowWalkAudio", ShowWalkAudio)`
  - 在国王行走动画关键帧触发脚步声
  - 通过 `kingSoundSfxCpn` 动态切换 `"主角跑步走路音效/土地跑{N}.mp3"`
- `LaiFlyAniEventCpn.RegisterEvent("ShowLaiFlyAudio", ShowLaiFlyAudio)`
  - 在莱伊起飞关键帧播放飞行动效声音

实现要点：
- 音效事件应绑定在动画关键帧，而不是固定时间 `Wait`，这样在动画变速时仍能对齐动作。

---

## 7. 为什么采用“动画事件 + 协程等待”的混合方案

重要修改/设计原因：
- 仅用协程 `WaitForSeconds`：容易在动画时长调整后错位。
- 仅用动画事件：不方便做“提前激活对象/先入镜”这类演出预备。
- 混合方案把“对象激活节奏”和“关键动作落点”分开控制，兼顾稳定性与可调性。

---

## 8. 复杂逻辑替代方案（备选）

### 方案 A：全事件驱动（不使用等待）

做法：
- `PreparePlay()` 只做触发士兵回头；
- 在士兵动画里增加中间事件（例如 `OnSoldierTurnHalf`）来激活 `king/lai`；
- 结束事件继续触发 `PlayKingShowAnim()`。

优点：
- 与动画帧最对齐，改动画时不易漂移。  
缺点：
- 依赖动画资源维护，事件漏配会直接断流程。

### 方案 B：全时间驱动（不使用动画结束事件）

做法：
- 用固定等待控制士兵回头后直接播 `PlayKingShowAnim()`。

优点：
- 逻辑简单，调试方便。  
缺点：
- 对动画长度变化敏感，后期调资源容易失配。

### 方案 C：状态机化演出（推荐给后续大演出）

做法：
- 抽象为 `StoryPerformanceStateMachine`（SoldierTurn, KingEnter, LaiFly, DialogueResume）。

优点：
- 可观测、可回放、可跳步骤，利于复杂剧情复用。  
缺点：
- 初期改造成本高，不适合当前小范围调整。

---

## 9. 常见问题排查

1. **国王不走路**
   - 检查士兵动画末尾是否调用了 `OnSoldierTurnEnd()`。
   - 检查 `king` 对象是否激活、`Animator` 是否存在 `ShowKing` 状态。

2. **士兵回头被触发多次**
   - 观察是否多次调用 `PreparePlay()`/`BeginSoldierTurnEarly()`。
   - `soldierTurnTriggered` 已有保护，若仍重复，优先排查节点图重入。

3. **莱伊不播放飞行动画**
   - 检查 `LaiFly()` 是否被剧情节点调用。
   - 检查 `lai` 对象 Animator 中是否存在 `"LaiFly"`。

4. **音效不触发**
   - 检查动画事件字符串是否与注册名一致：`ShowWalkAudio` / `ShowLaiFlyAudio`。
   - 检查 `SoundToggleComponent` 引用是否丢失。

---

## 10. 建议的后续维护规范

- 改动演出节奏时，优先只改 `soldierTurnLeadBeforeKingSeconds`，避免同时改多个节点导致难以回归。
- 若修改动画长度，必须同步回归：
  - 士兵回头末帧事件是否仍触发；
  - 国王脚步声是否仍在脚落地帧。
- 若调整剧情图调用顺序，先确认 `PreparePlay` 与 `BeginSoldierTurnEarly` 不会互相重复触发。

---

*本文基于当前仓库脚本与 `ForestSceneLaiFlyStory.prefab` 中的剧情图序列化内容整理。如剧情图重编排，请同步更新第 3/4/5 节。*

修改方向
HomeDoorStoryTrigger碰到玩家开始播放士兵回头动画
需要新建一个脚本就是碰到玩家就播放一个动画