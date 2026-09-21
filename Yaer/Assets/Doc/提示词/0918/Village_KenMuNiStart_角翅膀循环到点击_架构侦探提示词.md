# Cursor Agent Prompt · Village_KenMuNiStart：角/翅膀动画点继续前一直循环

> **角色**：先【架构侦探】只读核实「播一次就藏」的断点；拍板后【施工员】最小改  
> **日期**：2026-09-18  
> **场景 / Prefab**：`Assets/GameRes/Prefabs/Dialogue/Village_KenMuNiStart.prefab`  
> **目标物体**：根下 **`Anim_Gusha`**（古莎卷角）、**`Anim_Yaer`**（雅尔小翅膀）  
> **现象（用户）**：对话里这两个小动画会自动播，**播一次就消失**  
> **产品期望（钉死）**：对应那句对白还在、**玩家没点继续之前，动画一直循环**；玩家点继续后动画停并藏起来，不挡立绘，对话照常往下走  
> **不是**：改台本文字；改立绘表情/渐入；把整段开场对话重导或重写；让其它句子也播这两个动画；改村庄走路/战斗  
> **历史对照（必读，勿当唯一真相）**：  
> - 0804 拍板 Q2 是 **约 8～12 fps、不循环、播完隐藏**（见执行说明）  
> - 本次是用户**改口**：只推翻「不循环 + 播完立刻藏」，**不推翻**「等玩家点继续再往下」  
> **报告落盘**：`Assets/Doc/执行文档/0918/Village_KenMuNiStart_角翅膀循环到点击_架构溯源报告.md`

把下面「侦探」整段交给 Cursor Agent。施工见文末，侦探没写清点击后谁负责隐藏之前，不要施工。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 产品白话

> 进村开场对白里，古莎卷角、雅尔扇翅膀，现在闪一下就没了。  
> 要改成：这句话还挂在屏幕上、人没点的时候，小动画一直重复；点了下一句，动画才收掉。

### 名词拆开（避免再混）

| 名称 | 是什么 | 谁控制 |
|------|--------|--------|
| **`Anim_Gusha`** | 古莎卷角 UI 容器（Animator） | Clip `Anim_Gusha_Horn`，状态名 `Play` |
| **`Anim_Yaer`** | 雅尔翅膀 UI 容器（Animator） | Clip `Anim_Yaer_Wing`，状态名 `Play` |
| **`PlayUiAnimatorActionTask`** | 对话图里「播放 UI Animator」节点 | 播前显示；`waitUntilFinish` 为真则等播完；`hideWhenFinished` 为真则藏容器 |
| **Anim 行** | CSV `Type=Anim`，`Extra=Anim_Gusha` / `Anim_Yaer` | 导入器生成 **Play → Statement**（先播动画，再出字幕等点击） |

### 现网链路（预扫）

```
CSV Anim 行（ID9 卷角 / ID17 翅膀）
  → DialogueCsvGraphBuilder.CreatePlayUiAnimatorNode
       waitUntilFinish = true
       hideWhenFinished = true
  → 连到同一行的 Statement（字幕，等玩家点继续）

PlayUiAnimatorActionTask.OnUpdate
  → 状态 normalizedTime >= 1，或超过约 5 秒
  → FinishAndMaybeHide()（SetActive false）
  → EndAction，才轮到字幕

Clip 现状（须复核文件是否仍如此）
  Anim_Gusha_Horn.anim / Anim_Yaer_Wing.anim
  → m_LoopTime: 0（不循环）
```

预判（可证伪）：动画在 **字幕出现之前** 就播完并被藏掉，所以玩家看到的是「闪一次就没了」。只把 Clip 勾成 Loop **不够**——Task 仍用 `normalizedTime >= 1` 当结束，循环动画第一圈结束照样被藏；若 Task 一直等循环结束，字幕会永远不出来。

### 「播一次就消失」嫌疑优先级

| # | 嫌疑 | 怎么验 |
|---|------|--------|
| **A（高优先）** | Play 节点 `waitUntilFinish + hideWhenFinished`，Clip 不循环，播完即藏，藏完才出字幕 | 成品 Prefab 图里 ID9/ID17 的 Play 节点这两个布尔是否仍为 true；Play 的下一节点是不是等点击的 Statement |
| **B** | 只改 Loop 仍会被 `normalizedTime >= 1` 或 5 秒超时藏掉 | 读 `PlayUiAnimatorActionTask.OnUpdate`，写清循环时 normalizedTime 会不会一过 1 就进隐藏 |
| **C** | 成品图已被手改，和导入器默认不一致 | 以 Prefab 序列化图为准，不要只信 Builder |
| **D** | Controller 状态本身不循环，或播完切到空状态 | 看 `Anim_Gusha_Horn.controller` / `Anim_Yaer_Wing.controller` 的 `Play` 状态 |
| **E** | 玩家点的是对话框继续，隐藏必须挂在「离开这句」而不是「Clip 结束」 | 找出 Statement 点继续后，图上有没有现成的「藏 Anim」节点；没有就写明最小新增点 |

### 复现 / 验收矩阵

| 操作 | 期望 |
|------|------|
| 播到「古莎卷了卷角」 | `Anim_Gusha` 可见并**循环**；字幕在；不点就一直循环 |
| 这一句点继续 | 卷角**马上停并隐藏**；进入下一句；立绘不被挡住 |
| 播到「雅尔呼扇呼扇头上的一对小翅膀。」 | `Anim_Yaer` 同样：不点就循环，点了就藏 |
| 其它句子 | 两个 Anim **都不出现** |
| 整段对话结束 / 中途跳过（若现网有跳过） | 容器不残留在立绘上 |
| 读得很慢（超过 5 秒才点） | **不能**因为旧的 5 秒超时把循环掐掉 |

### 侦探须回答

1. 成品图里，卷角句、翅膀句是不是「Play 播完并隐藏 → 才显示字幕等点击」？是 → 主因 A。  
2. 玩家「点击」具体打在哪个节点（Statement 继续）？离开该句时，谁负责 `SetActive(false)`？现网有没有这个钩子？  
3. 全项目还有哪些地方用 `PlayUiAnimatorActionTask` / `Type=Anim`？改 Task 默认行为会不会误伤别的对话？  
4. 推荐的最小改法（只推一种，并写明为何不选另外两种）：  
   - **方案 1**：Clip 循环 + Play 不等待、不立刻藏；字幕期间循环；离开 Statement 时再藏。  
   - **方案 2**：Task 增加「循环直到本句结束」参数，旧的「播一次就藏」调用点保持原样。  
   - **方案 3**：只改 Clip `Loop Time`，不改 Task。——预判此方案**不能**当修复，侦探若否决须写一句原因。  
5. 重导 CSV 会不会把成品图上的前奏/立绘引用冲掉？若会，施工必须手改成品图或改导入器，**禁止**整图覆盖 Prefab。

### 禁止

- 本阶段不改代码、不改 Prefab、不改 Clip、不改 CSV。  
- 不要把 Q2 旧结论「播完隐藏」再写回推荐方案。用户已改口。  
- 设计仍不清楚的，记入 `Assets/Doc/OPEN_QUESTIONS.md`，不要在报告里假装已拍板。

---

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Project_context.md
@Assets/Doc/02_SYSTEM_SPEC.md
@Assets/GameRes/Prefabs/Dialogue/Village_KenMuNiStart.prefab
@Assets/GameRes/Animation/Dialogue/Anim_Gusha_Horn.anim
@Assets/GameRes/Animation/Dialogue/Anim_Yaer_Wing.anim
@Assets/Scripts/Game/GameRuntime/NodeCanvas/NodeCanvasNode/ActionTask/Common/PlayUiAnimatorActionTask.cs
@Assets/Editor/Tool/Dialogue/DialogueCsvGraphBuilder.cs
@Assets/Doc/执行文档/8月/0804/Village_KenMuNiStart_角翅膀帧动画制作与对话触发_架构溯源与执行说明.md
@Assets/Doc/OPEN_QUESTIONS.md

你现在是【架构侦探】。Unity 2020.3.48f1 / C#。
禁止修改任何代码、Prefab、场景、AnimationClip、AnimatorController、CSV、台本。只读扫描 + 写溯源报告。

报告写入：
Assets/Doc/执行文档/0918/Village_KenMuNiStart_角翅膀循环到点击_架构溯源报告.md

---

## 背景（策划白话）

进村开场对话 Prefab `Village_KenMuNiStart` 里有两个小动画：
- `Anim_Gusha`：古莎卷角（对白「古莎卷了卷角」）
- `Anim_Yaer`：雅尔头上小翅膀（对白「雅尔呼扇呼扇头上的一对小翅膀。」）

现在：对话进行到这两句时动画会自动播，**播一次就消失**。
要改成：**玩家不点继续，动画就一直循环**；点了继续，动画停掉并隐藏，不挡立绘，后面的对白照旧。

0804 曾经定过「不循环、播完隐藏」。那条作废。等点击再进下一句，这条仍有效。

---

## 必读 / 优先扫描线索

### A. 成品图真实拓扑（以 Prefab 为准）
- 图里 `Anim_Gusha` / `Anim_Yaer` 的「播放 UI Animator」节点：`waitUntilFinish`、`hideWhenFinished`、`stateName` 实际值
- 每个 Play 节点的下一跳是不是带字幕、等点击的 Statement
- 点继续之后有没有隐藏 Anim 的节点

### B. 为何「播一次就没」
- `PlayUiAnimatorActionTask`：`normalizedTime >= 1` 与约 5 秒超时，是否都会 `SetActive(false)`
- 两个 Clip 的 `m_LoopTime`，以及对应 Controller 的 `Play` 状态是否循环
- 明确写出：只改 Loop、不改 Task，玩家还会不会看到「一圈就消失」或「字幕永远不出来」

### C. 改动爆炸半径
- `CreatePlayUiAnimatorNode` 写死 `waitUntilFinish=true`、`hideWhenFinished=true`
- 全仓库还有哪些 `Type=Anim` 或直接使用该 Task 的对话
- 若只有这两句，推荐改 Task 加开关，不要让所有 Anim 行突然变成死循环

### D. 推荐一种最小改法
必须同时满足：
1. 字幕已经显示、玩家还没点时，对应 Anim 可见且循环
2. 点继续的同一时刻（或离开该 Statement 时）隐藏，不能拖到很后面
3. 不点也不会被 5 秒超时掐掉
4. 其它句子、其它对话的「播一次就藏」行为不变（若现网还有这种用法）
5. 不重导、不覆盖整张 `Village_KenMuNiStart` 图，除非报告证明手改节点重导后会丢，并给出不丢前奏的改法

---

## 报告结构（固定）

① 结论一句话  
② 原因（大白话 + 调用链）  
③ 用户需要做什么（验收清单）  
④ 给施工员的补充：改哪些文件、不要改哪些、推荐方案和否决方案各一句原因

发现设计说不清的，追加到 `Assets/Doc/OPEN_QUESTIONS.md`，不要改核心设计。
```

---

## 施工员提示词（侦探报告落盘并确认方案后再贴）

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0918/Village_KenMuNiStart_角翅膀循环到点击_架构溯源报告.md
@Assets/GameRes/Prefabs/Dialogue/Village_KenMuNiStart.prefab
@Assets/Scripts/Game/GameRuntime/NodeCanvas/NodeCanvasNode/ActionTask/Common/PlayUiAnimatorActionTask.cs

你现在是【施工员】。Unity 2020.3.48f1 / C#。
只按溯源报告里的推荐方案做最小改。不要重写对话树，不要改台本原文，不要改立绘渐入。

目标：
- 「古莎卷了卷角」显示期间，`Anim_Gusha` 一直循环，直到玩家点继续
- 「雅尔呼扇呼扇头上的一对小翅膀。」显示期间，`Anim_Yaer` 一直循环，直到玩家点继续
- 点继续后对应容器隐藏，不挡后面的立绘
- 其它句子不播出这两个动画
- 读句子超过 5 秒也不能把循环掐掉

限制：
- 禁止在 Update 里堆新的业务逻辑；若必须轮询，沿用现有 Task 的 OnUpdate，并加注释说明为什么不能改成等 Clip 结束
- 复杂分支写清替代方案（为什么不采用「只勾 Clip Loop」）
- 若报告说改导入器，只改 Anim 这两句需要的开关，别让其它 Anim 行行为漂移
- 施工说明写入：`Assets/Doc/施工说明/0918/Village_KenMuNiStart_角翅膀循环到点击_施工说明.md`
- 要改技术说明才写；有则放入 `Assets/Doc/技术文档/`，没有不要新造空文档

完成后用大白话给出验收清单，供验收员对照 Play。
```
