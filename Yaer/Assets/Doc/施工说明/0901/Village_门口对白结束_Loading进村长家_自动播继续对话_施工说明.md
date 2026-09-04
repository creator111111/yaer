# 门口对白结束 → Loading 进村长家 → 自动播继续对话 — 施工说明

**文档版本**：v1.0（2026-09-01）  
**文档性质**：【施工员】按侦探报告最小落地  
**Unity**：2020.3.48f1  
**侦探报告**：`执行文档/0901/Village_门口对白结束_Loading进村长家_自动播继续对话_架构溯源报告.md`  
**产品**：屋外门口戏播完 →（既有）Loading 进 `Village_Chief_House` → **进屋自动** `TriggerStory("Village_村长家继续对话")`

---

## 沟通摘要

### ① 结论一句话

**已在 `Village_Chief_HouseSceneManager.OnEnterScene` 挂 C1+F1 自动续聊；并新增 Setup 菜单落盘续聊 Prefab（须在 Unity 执行一次，否则仍会「加载资源失败」）。**

### ② 原因（通俗）

进屋后原来没人喊「接着聊」。现在进村长家时会查：门口戏播过了、续聊还没播过，就自动开续聊。  
续聊的壳 Prefab 以前没有，所以加了和门口戏一样的一键 Setup。

### ③ 用户检查清单

| # | 操作 | 通过 |
|---|------|------|
| 1 | Unity 菜单：`Tools / Dialogue / Setup Village 村长家继续对话 Prefab`（或等 `Library/ChiefContinueSetup.request` 自动跑） | Console：`[ChiefContinueSetup] Prefab 已写入…` |
| 2 | Project 可见 `Assets/GameRes/Prefabs/Dialogue/Village_村长家继续对话.prefab` | |
| 3 | Play：屋外门口三人戏完整播完 → 自动 Loading 进屋 | |
| 4 | **进屋后无需操作**即开始续聊；Console 有 `[ChiefContinue] OnEnterScene TriggerStory …` | |
| 5 | 续聊可播完；**无**该 Prefab「加载资源失败」 | |
| 6 | 同档再进村长家：**不**重复播 | |
| 7 | `House_Chief`：门口已播且续聊未播 → 补播一次；已播过 → 静默 | |
| 8 | `House_Tree` / 晚宴台本未被误改 | |

### ④ 程序补充

见下文。

---

## ① 改动清单

| # | 文件 | 动作 |
|---|------|------|
| 1 | `Village_Chief_HouseSceneManager.cs` | `OnEnterScene` → `TryTriggerChiefContinueOnce`；门闩 F1 |
| 2 | `VillageChiefContinueDialogueSetupEditor.cs`（新建） | Setup 菜单 + `Library/ChiefContinueSetup.request` 自动跑 |
| 3 | `Prefabs/Dialogue/Village_村长家继续对话.prefab` | **须 Unity Setup 落盘**（磁盘施工前缺失） |

**未改**：`ChiefNearDoorStoryTrigger`（仍只 Loading 进屋）；晚宴台本；`House_Tree`；换场主路径。

---

## ② 门闩与挂点（已落地）

```
OnEnterScene
  → ShouldPlayChiefContinue()
       CheckStoryUsed("Village_村长家门口初次对话")
    ∧ !CheckStoryUsed("Village_村长家继续对话")
    ∧ counts != null
  → TriggerStory("Village_村长家继续对话")
```

- **挂点 C1**：室内 GSM `OnEnterScene`（趁 Loading 未关时开壳）  
- **门闩 F1**：同上；手动 `House_Chief` 进房同样补播一次（OPEN Q2 待产品确认）  
- **禁止**：门口组件跨场景 Trigger 续聊；C2 Loading callBack（竞态）

---

## ③ Prefab Setup

| 项 | 值 |
|----|-----|
| 菜单 | `Tools / Dialogue / Setup Village 村长家继续对话 Prefab` |
| 自动 | 工程根 `Library/ChiefContinueSetup.request` → 编译后 delayCall 消费 |
| 优先壳 | `Village_村长家门口初次对话.prefab` |
| 回退壳 | `Village_KenMuNiStart.prefab` |
| CSV | `Assets/Dialog/Village_村长家继续对话.csv` |
| 产出 | `Assets/GameRes/Prefabs/Dialogue/Village_村长家继续对话.prefab` + Generated 树重写 |

---

## ④ 验收对照（侦探 §⑧）

- [ ] 门口三人戏可完整播完  
- [ ] Loading → `Village_Chief_House`，落点 `EnterFrom_Village`  
- [ ] 进屋自动续聊（无需点 E）  
- [ ] 无续聊 Prefab 加载失败  
- [ ] 同档不重复  
- [ ] `House_Chief` 补播 / 已播静默  
- [ ] 晚宴 / `House_Tree` 未误改  
- [ ] Console `[ChiefContinue] OnEnterScene TriggerStory …`

---

## ⑤ 剩余风险

| 风险 | 说明 | 处置 |
|------|------|------|
| Prefab 未 Setup | Play 仍「加载资源失败」 | **先跑菜单 / 等 request** |
| Loading 关后短露景 | 侦探 Q3：本期可接受 | 穿帮再 P1 二次遮罩 |
| 产品只要「自动链」不补播 | 现 F1 会在手动门补播 | 改 F2 Pending（OPEN Q2） |
