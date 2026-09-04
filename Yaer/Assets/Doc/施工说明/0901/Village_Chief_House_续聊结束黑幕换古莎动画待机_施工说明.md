# Village_Chief_House — 续聊结束黑幕换古莎动画待机 — 施工说明

**文档版本**：v1.0（2026-09-01）  
**文档性质**：【施工员】按侦探报告最小落地  
**Unity**：2020.3.48f1  
**侦探报告**：`执行文档/0901/Village_Chief_House_续聊结束黑幕换古莎动画待机_架构溯源报告.md`  
**产品**：续聊结束 → BlackPanel → 关「古莎待机」→ 开「古莎动画合层」（关其子「背景」）

---

## 沟通摘要

### ① 结论一句话

**GSM 在续聊 Trigger 成功后订 `onStoryEnd`，全黑内换人并记档旗；合层源预置动画实例（默认关、关背景）；读档已换则静默 Active、不再黑幕。**

### ② 原因（通俗）

续聊播完要把静态古莎换成动画合层，必须在黑屏里切，否则会闪两个古莎。  
动画 Prefab 自带「背景」图层，室内已有房间底，必须关掉。

### ③ 用户检查清单

| # | 操作 | 通过 |
|---|------|------|
| 1 | 续聊完整结束后 | 自动黑幕再亮 |
| 2 | 亮后 | 待机关；动画合层在村长旁可见 |
| 3 | 无双古莎；房间底不被「背景」盖住 | |
| 4 | 同档再进房 | 不重复黑幕；直接动画合层 |
| 5 | 续聊 / 针线包 Tips 回归 | |
| 6 | 晚宴结束 | **不**触发本逻辑 |
| 7 | Console | 无 BlackPanel / 空引用 Error |

菜单（预置）：`Tools / Scene / Setup Chief House 古莎动画合层预置`

### ④ 程序补充

见下文。

---

## ① 改动清单

| # | 文件 | 动作 |
|---|------|------|
| 1 | `Prefab/村长家合层.prefab`（场景真源 guid `5cad…`） | 预置 `古莎动画合层`：默认关、脚位≈待机、关「背景」、Sorting+8 |
| 2 | `Village_Chief_HouseSceneManager.cs` | Apply 静默视觉；Trigger 成功订 end；BlackPanel 换人 + 旗 |
| 3 | `ChiefHouseGushaAnimStandbySetupEditor.cs` | 一键预置 + `Library/ChiefHouseGushaAnimSetup.request` |

**未改**：村外侧面、UI 立绘、续聊台本、Loading 进屋、晚宴；Animator 真帧另案。

---

## ② 时序

```
OnEnterScene
  → ApplyGushaVisualFromArchive()   // 旗或续聊已用 → 静默动画
  → TryTriggerChiefContinueOnce()   // 成功 → += onStoryEnd
  → …续聊播完…
  → onStoryEnd → FadeShow → 全黑换人 + 记 Village_ChiefHouse_GushaAnimStandby
  → CloseFormFade
```

---

## ③ 存档旗

| 键 | 含义 |
|----|------|
| `Village_村长家继续对话` | 续聊播完（既有，早于换人） |
| `Village_ChiefHouse_GushaAnimStandby` | 黑幕换人成功 |

Q7：续聊已用但旗未立 → 进房静默 Apply 动画。

---

## ④ 剩余风险

| 风险 | 处置 |
|------|------|
| 合层未跑 Setup | Play 找不到动画合层 → 先跑菜单 |
| 脚位肉眼微调 | 改合层内实例 localPosition |
| 真 Animator 帧动画 | 另案（现网无 Clip） |
