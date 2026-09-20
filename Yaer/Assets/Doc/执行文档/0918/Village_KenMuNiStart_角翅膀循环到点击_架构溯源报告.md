# Village_KenMuNiStart · 角/翅膀循环到点击 — 逻辑溯源报告

**文档版本**：v1.0（2026-09-18）  
**文档性质**：【架构侦探】只读；**未改**代码 / Prefab / Clip / Controller / CSV  
**Unity**：2020.3.48f1  
**对照提示词**：`Assets/Doc/提示词/0918/Village_KenMuNiStart_角翅膀循环到点击_架构侦探提示词.md`  
**运行时真源**：`Assets/GameRes/Prefabs/Dialogue/Village_KenMuNiStart.prefab`（场景 `Village_KenMuNi1` 播的是这个 Prefab，不是 Generated `.asset`）

---

## ① 结论一句话

卷角和翅膀是在**字幕出现之前**就播完并被藏掉的，所以看起来闪一下就没了；点继续的那一下，图上**没有人负责再藏**。

---

## ② 原因（大白话 + 调用链）

玩家点到这两句时，对话图先把小动画播完、藏起来，**然后**才把字幕摆出来等你点。动画一共约 0.6 秒、不循环，播完就被关掉。字幕挂着的时候，动画已经不在了。

0804 定过「不循环、播完就藏」。那条作废。等你点继续才进下一句，这条还在。

### 成品图实际怎么走（以 Prefab 为准，和导入器默认一致）

两条都是直线，中间没有分支，也没有「藏动画」节点：

```
「雅尔角会动吗，我的还能动呢。」（节点 11，古莎）
  → 播放 UI Animator：Anim_Gusha / Play（节点 12）
       waitUntilFinish = true
       hideWhenFinished = true
  → 「古莎卷了卷角」（节点 13，古莎，Face=Happy）
  → 下一句「好神奇！再来一次！」（节点 14）

「不是角，是我头上的小翅膀。」（节点 20，雅尔）
  → 播放 UI Animator：Anim_Yaer / Play（节点 21）
       两个布尔同样是 true
  → 「雅尔呼扇呼扇头上的一对小翅膀。」（节点 22，雅尔，Face=Laugh）
  → 下一句「真可爱~」（节点 23）
```

两个容器在 Prefab 里默认是关着的（`m_IsActive: 0`）。其它句子没有播放节点，所以不会自己冒出来。

### 为什么是「闪一次」

`PlayUiAnimatorActionTask` 在「要等播完」时，每帧看两件事，中了就 `SetActive(false)` 再放行：

1. 当前状态叫 `Play`，并且 `normalizedTime >= 1`（一圈播完）
2. 或者从开播起超过大约 5 秒

两个 Clip 都是 `m_LoopTime: 0`，采样 10 帧/秒，长度 0.6 秒。所以先撞上第 1 条，大约 0.6 秒就藏。5 秒超时不是现在这次闪没的原因，但循环之后如果还用这套等待，慢读会被它掐掉。

Controller 不用背锅：`Anim_Gusha_Horn` / `Anim_Yaer_Wing` 都只有一个状态 `Play`，没有出口过渡，默认就是这个状态。动画消失是脚本把物体关掉，不是状态机切到空状态。

### 点继续打在谁身上，谁负责藏

点对话框继续，走的是：

`DialogueTMPUGUI` 等点击 → `info.Continue()` → `StatementNodeEx.OnStatementFinish` → `DialogueTree.Continue()` → 进入下一节点。

这条路上**没有** `SetActive(false)`。`PlayUiAnimatorActionTask.OnStop` 也只清自己的标记，不藏物体。  
所以：现网的藏，只发生在「Clip 播完或 5 秒到了」。点继续时没有钩子。

整段对话结束时，`StoryComponentGSM` 会把整棵剧情物体关掉，容器会跟着没。这只能兜住「对话已经结束」，兜不住「下一句立绘还在、动画还挡着」。

跳过对话（界面上的跳过）不是直接拆掉图，而是每帧当成已经点了继续，仍会逐句走进 `Continue`。以后只要把隐藏挂在这个 `Continue` 上，跳过也会藏。

### 只改 Clip 勾循环（方案 3）为什么不行

循环之后，`normalizedTime` 过了第一圈仍然 `>= 1`，而且会一直大于 1。Task 还是会在第一圈结束时藏掉，字幕照样后出。  
如果为了循环把这个判断删掉、又继续「等播完」，循环永远等不完，字幕出不来；大约 5 秒还会被超时藏掉。所以**不能只改 Clip**。

---

## ③ 用户需要做什么（验收清单）

施工完成后再 Play。从 `Village_KenMuNi1` 进开场对白 `Village_KenMuNiStart`。

| 操作 | 期望 |
|------|------|
| 播到「古莎卷了卷角」 | `Anim_Gusha` 看得见，并且**一直重复**；这句话还在；不点就一直转 |
| 这一句点继续 | 卷角马上停并消失；下一句出来；立绘不被挡住 |
| 播到「雅尔呼扇呼扇头上的一对小翅膀。」 | `Anim_Yaer` 同样：不点就循环，点了就藏 |
| 其它句子 | 两个小动画都不出现 |
| 这一句停超过 5 秒再点 | 循环不能被掐掉 |
| 开着跳过，或整段对话结束 | 两个容器不能留在立绘上 |

---

## ④ 给施工员的补充

### 推荐：方案 2（只推这一种）

给 `PlayUiAnimatorActionTask` 加一个**默认关闭**的开关（建议名 `loopUntilContinue`，中文注释写清「字幕期间循环，点继续再藏」）。

为真时必须同时做到：

1. 播前把容器打开，从头播状态 `Play`。
2. **马上** `EndAction(true)`，让后面的字幕节点跑起来。不要进现在的等待，也不要武装那 5 秒。
3. 在 `EndAction` **之前**订一次 `DialogueTree.OnSubtitlesRequest`。下一次字幕请求就是紧挨着的那句（节点 13 / 22）。此时 `info.Continue` 已经是 `OnStatementFinish`。把它包一层：**先** `SetActive(false)`，**再**调用原来的 `Continue`，然后退订。  
   原因：播放节点和字幕节点是排队的，播放任务不能一边活着一边等点击，否则字幕出不来。隐藏必须挂在「这句的继续回调」上，不能挂在 Clip 结束上。
4. 再订一次 `DialogueTree.OnDialogueFinished`：如果这句还没点、整段就被停掉，也藏一次，避免跳过/中断残留。整棵物体被关掉只是额外保险，不能代替这一下。
5. 开关为假时，现有「播一次、播完藏、5 秒封顶」一行都不要动。

同时把两个 Clip 的 `m_LoopTime` 改成 `1`：

- `Assets/GameRes/Animation/Dialogue/Anim_Gusha_Horn.anim`
- `Assets/GameRes/Animation/Dialogue/Anim_Yaer_Wing.anim`

Controller **不用改**。不勾 Loop 的话，新开关也只会停在最后一帧，看起来不像在扇。

然后只改成品图里**两个**播放节点，把新开关设为真：

- 节点 `$id=12`（`Anim_Gusha`）
- 节点 `$id=21`（`Anim_Yaer`）

新开关为真时，忽略这两个节点上现有的 `waitUntilFinish` / `hideWhenFinished`（它们现在都是 true）。不要靠把 `waitUntilFinish` 改成 false 来实现：那个分支会立刻调用 `FinishAndMaybeHide()`，`hideWhenFinished` 仍为真时，动画会在同一帧被藏掉，字幕期间什么都看不到。

`OnUpdate` 里如果还要留轮询，只留给旧的「播一次就藏」。新开关不要再轮询 `normalizedTime`。注释里写明：循环动画第一圈结束 `normalizedTime` 就 ≥ 1，不能拿它当「等玩家点击」。

### 为什么不选另外两种

- **方案 1（Play 不等待、不立刻藏，离开字幕再插一个藏的节点）**：图上没有藏的节点。`waitUntilFinish=false` 仍会立刻藏，必须再改 Task，等于还是要动 Task。再往 64 个节点的成品图里插节点、改连线，比加一个开关更容易把前奏弄断。
- **方案 3（只勾 Clip Loop）**：见上面。第一圈结束照样被藏；硬等循环结束则字幕不出，5 秒还会掐掉。

### 改哪些、不要改哪些

| 要改 | 不要改 |
|------|--------|
| `PlayUiAnimatorActionTask.cs`（加开关 + 包住下一次 `Continue`） | CSV 台词、`DialogueCsvGraphBuilder`（本期不重导，导入器仍写死 wait+hide） |
| 两个 Clip 的 `m_LoopTime` | 两个 Controller |
| 成品 Prefab 图上节点 12、21 的新开关 | 前奏（藏战斗面板、等背景、立绘淡入 0.5 秒）、立绘表情、其它句子 |
| | 村庄走路 / 战斗 |

### 爆炸半径

全仓库 `Type=Anim` 只有开场台本两行：`Assets/Dialog/Village_村内雅古开场对白台本.csv` 的 ID 9、ID 17。  
`PlayUiAnimatorActionTask` 的运行时节点只在这个 Prefab 里（Generated 资源不是进村时播的那张图）。  
村长家门口 / 继续 / 送树屋等 Prefab 里虽有 `Anim_Gusha`、`Anim_Yaer` 黑板名，那是从壳子抄来的，**没有**播放节点。  
新开关默认 false，就不会把以后别的「播一次就藏」带成死循环。本期不要改导入器默认值。

### 重导会不会冲掉前奏

会，如果用生成图覆盖成品 Prefab。  
`Tools/Dialogue/Import CSV` 本身只写出 `Assets/GameRes/DialogueTrees/Generated/` 下的 `.asset`（重名还会另起文件名），**不会**写回 `Village_KenMuNiStart.prefab`。  
但成品图和 Generated 已经不是同一张：成品有 `WaitVillageStartBgReveal`、立绘淡入 0.5 秒且等淡入结束；Generated 是另一套淡入时长，没有这个等待节点。  
**禁止**用 Generated 覆盖整张 Prefab，也**不要**再跑 `Tools/Dialogue/Setup KenMuNiStart Horn Wing Anim`（它会把整个 Prefab 存一遍，这场修动画不需要它）。  
以后若有人必须重导这两句：先改导入器，仅当 `Extra` 为 `Anim_Gusha` / `Anim_Yaer` 时把新开关写成真，其它 Anim 行仍保持 wait+hide；并且只合并这两处节点，不要整图替换。

### 施工说明落点

`Assets/Doc/施工说明/0918/Village_KenMuNiStart_角翅膀循环到点击_施工说明.md`  
不必新写技术文档。复杂处在注释里说明「为什么不能等 Clip 的 normalizedTime」。
