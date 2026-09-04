# Village_Chief_House — 续聊战斗待机与室内主角显隐 — 施工说明

**文档版本**：v1.0（2026-09-02）  
**文档性质**：【施工员】按侦探报告方案 A + S1 + 结束合并落地  
**Unity**：2020.3.48f1  
**侦探报告**：`执行文档/0902/Village_Chief_House_续聊战斗待机与室内主角显隐_架构溯源报告.md`  
**产品**：续聊藏室内 Home 主角 + 古莎旁战斗形态待机；结束**一次**黑幕关双待机、恢复主角；默认仍开古莎动画合层

---

## 沟通摘要

### ① 结论一句话

**已接线 GSM 显隐；须在 Unity 跑一次 Setup 预置「雅儿战斗待机」（场景+Prefab 双写）。结束塞进既有换古莎黑幕，未开第二次黑幕、未切真 Combat。**

### ② 原因（通俗）

续聊时不想看见屋里走路的雅儿，又要在古莎旁边站一个穿铠甲的样子。  
贴纸最稳，别真改玩家战斗状态机。结束换古莎本来就有一次黑——关贴纸、亮回玩家一起做完。

### ③ 用户检查清单

| # | 操作 | 通过 |
|---|------|------|
| 0 | Unity：`Tools / Scene / Setup Chief House 雅儿战斗待机预置`（或等 `Library/ChiefHouseYaerCombatStandbySetup.request` 自动跑） | Console 完成双写 |
| 1 | 续聊开场：看不见室内主角；古莎旁可见战斗待机 | |
| 2 | 续聊中：无双雅儿；操作仍锁在对话 | |
| 3 | 结束后一次黑幕：双待机关；室内主角恢复可走 | |
| 4 | （默认）古莎动画合层在村长旁可见；无动画「背景」盖房 | |
| 5 | 同档再进：静默正确态 | |
| 6 | EnterPos / 楼梯 / 针线包 Tips / 门口初次：不回归 | |

### ④ 程序补充

见下文。

---

## ① 改动清单

| # | 文件 | 动作 |
|---|------|------|
| 1 | `ChiefHouseYaerCombatStandbySetupEditor.cs`（新建） | Setup 双写「雅儿战斗待机」单 SR（铠甲基本无/1） |
| 2 | `Village_Chief_HouseSceneManager.cs` | S1 揭黑前 `ApplyContinueTalkVisuals(true)`；结束黑幕合并 `false`；`SetPlayerVisualVisible`；进房静默 |
| 3 | Prefab/场景合层 | **须 Unity Setup 落盘**（磁盘施工前可能尚无该 GO） |

**未改**：`isFightingScene`、门口初次、CSV、UI GoOut 立绘、Loading、二次 BlackPanel。

---

## ② 时序（落地后）

```
Defer 全黑 Trigger 续聊
  → Finalize：ApplyContinueTalkVisuals(true)  ← 关玩家 SR + 亮贴纸
  → CloseFormFade 揭黑
  → …对白…
  → OnBlackFullyShownForGushaSwap
       ApplyGushaVisual(true)              ← 关古莎待机 + 开动画合层
       ApplyContinueTalkVisuals(false)     ← 关贴纸 + 亮玩家
       MarkFlag → 淡出
```

---

## ③ Setup

| 项 | 值 |
|----|-----|
| 菜单 | `Tools / Scene / Setup Chief House 雅儿战斗待机预置` |
| 自动 | `Library/ChiefHouseYaerCombatStandbySetup.request` |
| 帧 | `ArtRes/.../Combat/Idle/铠甲基本无/1.png` |
| 脚位 | 古莎待机 local + (-4.5, 0, 0) |
| SortingOrder | 10 |
| 默认 | Active=false |

---

## ④ OPEN

| ID | 项 | 状态 |
|----|----|------|
| Q1 仍开动画合层 | 默认是 | ✅ |
| Q2 帧选型 | 基本无；可跟存档头饰另案 | ⏳ 美术 |
| Q3 单帧 | 本期单 SR | ✅ |
| Q5 预置落盘 | 依赖 Unity Setup | ⏳ 验收 |
