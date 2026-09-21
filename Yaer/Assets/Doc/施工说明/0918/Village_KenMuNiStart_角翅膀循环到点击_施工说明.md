# Village_KenMuNiStart · 角/翅膀循环到点击 — 施工说明

**文档版本**：v1.0（2026-09-18）  
**文档性质**：【施工员】方案 **2（loopUntilContinue，默认关）**  
**Unity**：2020.3.48f1  
**侦探报告**：`Assets/Doc/执行文档/0918/Village_KenMuNiStart_角翅膀循环到点击_架构溯源报告.md`

---

## 沟通摘要

### ① 结论一句话

**卷角和翅膀会一直转到你点继续，点了才收掉；别的句子还是不播。**

### ② 原因（通俗）

以前是字幕出来之前就把小动画播完关掉，所以只闪一下。现在播放任务不再等动画结束，字幕马上出来；点继续（或整段被停）时才把容器关掉。两段动画本身也改成循环，否则只会停在最后一帧。

### ③ 用户检查清单

从 `Village_KenMuNi1` 进开场对白 `Village_KenMuNiStart`。

| # | 操作 | 通过判据 |
|---|------|----------|
| 1 | 播到「古莎卷了卷角」 | `Anim_Gusha` 看得见，并且一直重复；不点就一直转 |
| 2 | 这一句点继续 | 卷角马上停并消失；下一句出来；立绘不被挡住 |
| 3 | 播到「雅尔呼扇呼扇头上的一对小翅膀。」 | `Anim_Yaer` 同样：不点就循环，点了就藏 |
| 4 | 其它句子 | 两个小动画都不出现 |
| 5 | 这一句停超过 5 秒再点 | 循环不能被掐掉 |
| 6 | 开着跳过，或整段对话结束 | 两个容器不能留在立绘上 |

### ④ 程序补充

见下文。

---

## 改动清单

| 项 | 路径 | 说明 |
|----|------|------|
| 新开关 | `PlayUiAnimatorActionTask.cs` 的 `loopUntilContinue` | 默认关。为真：播前打开、从头播 `Play`，立刻 `EndAction`；把隐藏包进下一句 `Continue`；整段结束也藏一次 |
| Clip | `Anim_Gusha_Horn.anim`、`Anim_Yaer_Wing.anim` | `m_LoopTime` 0 → 1 |
| 成品图 | `Village_KenMuNiStart.prefab` 节点 `$id=12`、`$id=21` | 只这两处 `loopUntilContinue=true`。原有 `waitUntilFinish` / `hideWhenFinished` 仍为 true，但开关为真时忽略它们 |

## 没改什么

- CSV、`DialogueCsvGraphBuilder`（本期不重导）
- 两个 Controller
- 前奏、立绘表情、其它句子
- 村庄走路 / 战斗

## 为什么不走另外两条

- **图上再插一个藏动画节点**：`waitUntilFinish=false` 仍会立刻藏，还是要改任务；64 个节点的成品图改连线更容易把前奏弄断。
- **只勾 Clip 循环**：第一圈结束 `normalizedTime` 仍 ≥ 1，任务照样藏；硬等循环结束则字幕不出，约 5 秒还会被旧超时掐掉。

## 重导注意

不要用 Generated 覆盖 `Village_KenMuNiStart.prefab`，也不要跑 `Tools/Dialogue/Setup KenMuNiStart Horn Wing Anim`。导入器仍把 Anim 行写成「等播完再藏」。以后若必须重导这两句，先改导入器，并且只合并节点 12、21。
