# Cursor Agent Prompt · 「获得了艾琳之剑」Tips 效果溯源 → 老农发道具复用

> **角色**：先【架构侦探】只读溯源，报告拍板后再【施工员】（老农发道具接线）  
> **日期**：2026-08-30  
> **用户截图**：全屏横幅「**获得了艾琳之剑**」（花边黑底金字）——序章皇宫开宝箱拿剑提示  
> **产品目标（白话）**：  
> 1. **先查清**：这效果是怎么触发、走哪条代码/资源的（不是猜）  
> 2. **再挂钩**：老农那边也要**获得道具**并弹出**同款提示效果**  
> **样板场景**：龙宫卧室宝箱 `HomeScene2Box` + 剧情 `HomeScene2Box`  
> **老农侧（已有基础）**：`Npc_Farmer` + `Village_老农打水任务`（发什么道具本期侦探须问清/写开放问题，可先定接线方案）  
> **本阶段**：只读；禁止改代码 / Prefab / 图集 / CSV  
> **报告落盘**：`Assets/Doc/执行文档/0830/获得道具Tips横幅_艾琳之剑溯源与老农复用_架构溯源报告.md`

把下面「侦探」整段复制给 Cursor Agent（Agent Mode）执行。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 用户原话 → 侦探必须直接回答

| 问题 | 预扫假说（须证伪后写入结论） |
|------|------------------------------|
| 「获得艾琳之剑」怎么触发？ | 开宝箱 → `TriggerStory("HomeScene2Box")` → 图内 `ExecuteFunction` 调 `HomeScene2Box.OnHomeScene2Box_GetSword` → **入包** + **`TipsComponentGSM.OpenTipsForm("GetAiLinSword")`** |
| 横幅字是什么？ | **不是**运行时 TMP 拼「获得了xxx」；是 **TipsPanel** 用图集里名为 `GetAiLinSword` 的 **整图 Sprite**（中/英/日各一套）做 fill 动画 |
| 老农怎么复用？ | 同样：`AddMainItem(道具)` + `OpenTipsForm(新TipKey)`；须有对应 Tip 图进图集，否则 `GetTipsSprite==null` **静默不弹** |

### 现网链路假说（剑 · 完整树）

```
玩家点宝箱 Interactive
  → HomeScene2Box.OpenBox
  → StoryComponentGSM.TriggerStory("HomeScene2Box")
  → 对话图节点…
       ExecuteFunction → OnHomeScene2Box_OpenBox（动画/存档）
       …
       ExecuteFunction → OnHomeScene2Box_GetSword
            ├─ PlayerBagData.AddMainItem(EMainItemName.AiLinSword)   // 真发道具
            └─ TipsComponentGSM.OpenTipsForm("GetAiLinSword")       // 横幅提示
                 ├─ TipsFormProxy.GetTipsSprite("GetAiLinSword")
                 │     ← tipsInfo.spriteatlas / _en / _jp
                 │     ← 源图 TipInfoAtlas*/GetAiLinSword.png
                 └─ OpenUIForm("TipsPanel") → TipsFormLogic
                       Image fillAmount 扫过 + Item 音效
```

| 层 | 路径（预扫） | 作用 |
|----|--------------|------|
| 实体 | `HomeScene2Box.cs` | 开箱 TriggerStory；GetSword 入包+Tips |
| 剧情 | `HomeScene2Box.prefab` | ExecuteFunction 调上述方法 |
| 入口 | `TipsComponentGSM.OpenTipsForm` | 开 TipsPanel / 排队 AddTipsInfo |
| UI | `TipsFormLogic` + `TipsPanel` Prefab | 横幅底 + Image 播图 |
| 资源 | `ArtRes/.../TipInfoAtlas/GetAiLinSword.png` + Atlas | **文案在图里** |
| 音效 | TipsForm Item 类型 `soundSfxCpn`（「获得物品」类） | 预扫有 `获得物品音效.mp3` |

### 易混点（侦探必须写清）

| 易混 | 真相假说 |
|------|----------|
| `GetItemActionTask` | **只** `AddMainItem`，**不**弹 Tips（项链线 VerdantCorridor 可对拍） |
| `AddTipsInfoActionTask` | 对话图内调 `OpenTipsForm(TipKey, Info)`——可复用，但默认 **Info** 类型；剑用的是 **Item**（默认） |
| 成就 Tips / SystemTipsPanel | **另一套**；不是这张花边横幅 |
| 动态改字 | 现网 **不支持**按道具名拼字；新道具要 **新 Key + 新图**（或产品接受暂用旧图——开放） |

### 老农复用方案倾向（侦探拍板）

| 方案 | 做法 | 倾向 |
|------|------|------|
| **A · 对话图 Action** | `Village_老农打水任务` 末（或接任务后）插：`AddMainItem` + `OpenTipsForm`（可用 ExecuteFunction / 扩展 Action / 现有 AddTipsInfo+GetItem 组合） | ✅ 对白驱动，少新建实体脚本 |
| **B · C# 仿 HomeScene2Box** | Npc_Farmer 或 Quest 回调里双调 | ✅ 任务系统接取时更干净 |
| **C · 只 GetItemActionTask** | 有道具无横幅 | ❌ 不满足截图效果 |
| **D · 新写一套横幅 UI** | — | ❌ 禁止 |

**资源门槛（P0）**：老农发的道具须定：

1. `EMainItemName` / MainItemDatabase 是否已有  
2. TipKey 命名（如 `GetXxx`）  
3. 中/英/日 `GetXxx.png` 打进 `tipsInfo*.spriteatlas`  

未进图集 → `OpenTipsForm` 直接 return，**看起来像「效果坏了」**。

### 本期边界

| 做 | 不做 |
|----|------|
| ✅ 钉死剑提示全链路（代码+Prefab+图集+音效） | ❌ 重做 TipsPanel 视觉 |
| ✅ 列出老农复用最小步骤与挂点 | ❌ 完整打水任务玩法（交水桶等）可挂钩 |
| ✅ 区分入包 vs 横幅 vs GetItemActionTask | ❌ 动态 TMP「获得了{0}」大改（除非报告另开） |
| ✅ 验收：剑样板复现 + 老农方案表 | ❌ 改成就/未开放 Tips |

### 严禁（本阶段）

- 改代码 / Prefab / 图集 / CSV  
- 结论写「随便 Debug.Log」代替 Tips  
- 忽略图集 Key 与文件名对齐规则  
- 把成就弹窗当获得道具横幅  

### 须对拍资产

| 资产 | 用途 |
|------|------|
| 用户截图 | 目标视觉 |
| `HomeScene2Box.cs` | GetSword 真源 |
| `HomeScene2Box.prefab` | ExecuteFunction 节点 |
| `TipsComponentGSM.cs` / `TipsFormLogic.cs` / `TipsFormProxy.cs` | Tips 管线 |
| `TipsPanel` Prefab + TipInfoAtlas*`/GetAiLinSword.png` | 横幅图 |
| `GetItemActionTask` / `AddTipsInfoActionTask` | 对话内替代路径 |
| `Village_老农打水任务.prefab` / Npc_Farmer | 复用挂点 |
| MainItem / 老农要发的道具 ID | 开放或产品指定 |

---

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/02_SYSTEM_SPEC.md
@Assets/Scripts/Game/GameRuntime/Entities/SceneEntities/HomeScene2/HomeScene2Box.cs
@Assets/GameRes/Prefabs/Dialogue/HomeScene2Box.prefab
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Component/TipsComponentGSM.cs
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Tips/TipsFormLogic.cs
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Tips/TipsFormProxy.cs
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Tips/ETipsType.cs
@Assets/Scripts/Game/GameRuntime/NodeCanvas/NodeCanvasNode/ActionTask/Common/GetItemActionTask.cs
@Assets/Scripts/Game/GameRuntime/NodeCanvas/NodeCanvasNode/ActionTask/UIPanel/AddTipsInfoActionTask.cs
@Assets/GameRes/Prefabs/UI/TipsPanel.prefab
@Assets/ArtRes/UI/Form/TipsPanel/Char/TipInfoAtlas/GetAiLinSword.png
@Assets/GameRes/Prefabs/Dialogue/Village_老农打水任务.prefab
@Assets/Doc/施工说明/0830/Village_KenMuNi1_老农基础对话交互_施工说明.md

你现在是【架构侦探】。Unity 2020.3.48f1 / C#。
禁止修改任何代码、Prefab、图集、场景、CSV。只读扫描 + 写溯源报告。

---

## 背景（策划白话）

1. 截图里「获得了艾琳之剑」是序章开箱提示，要查清**怎么触发的**。  
2. 老农也要给玩家道具，并弹出**同一类效果**。  
3. 本阶段只摸清链路与复用清单；不施工发奖。

---

## 侦探任务清单

### A. 复现剑提示全链路
从点箱到横幅：方法名、故事名、Tips Key、图集路径、音效、ETipsType。画顺序图。

### B. 钉死「字从哪来」
确认是 Sprite 整图还是动态文本；Key 与 png/atlas 命名规则；无图时行为（静默？Error？）。

### C. 对照其它获得道具
`GetHpBall` / `GetXiaerPower` / 项链 `GetItemActionTask`：谁弹 Tips、谁只入包。

### D. 老农复用方案拍板
推荐挂点（对话图 vs Quest 回调）；入包 API；Tips Key；是否要新美术图。  
列出最小施工清单（本阶段不执行）。

### E. 开放问题
- 老农发的道具 ID / 中文名是什么？  
- Tip 图谁出（美术 / 暂用占位）？  
- 发奖时机：对白某一句后 / 接任务时 / 交任务时？  

### F. 验收清单（施工后用；本期写进报告）

| # | 操作 | 通过 |
|---|------|------|
| 1 | 序章再开箱（或读档未开箱） | 再现「获得了艾琳之剑」横幅 |
| 2 | 老农线触发发奖 | 背包有道具 + **同款 TipsPanel 横幅** |
| 3 | 无对应 Tip 图时 | Console 有「未找到Tips图片」或按现网静默——报告写清 |
| 4 | 连发多条 Tips | 队列不叠坏（对拍 TipsFormLogic 队列） |

---

## 输出要求

写入：`Assets/Doc/执行文档/0830/获得道具Tips横幅_艾琳之剑溯源与老农复用_架构溯源报告.md`

MASTER 四段式：  
① 结论（触发一句话 + 老农怎么复用）  
② 原因（通俗：入包和横幅是两步；字在图里）  
③ 用户检查清单（剑怎么验、老农缺什么资源）  
④ 给程序：链路表 + Key/图集规则 + 方案 A/B + 最小 diff + 开放问题
```

---

## 施工员续跑（侦探报告 + 产品定下道具/Tip 图后贴）

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0830/获得道具Tips横幅_艾琳之剑溯源与老农复用_架构溯源报告.md
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Component/TipsComponentGSM.cs
@Assets/GameRes/Prefabs/Dialogue/Village_老农打水任务.prefab

你现在是【施工员】。按报告让老农发道具时弹出与「获得了艾琳之剑」同款 Tips 横幅。

必须遵守：
- 复用 TipsComponentGSM.OpenTipsForm + TipsPanel；禁止新横幅系统；
- 入包与提示都做；不要只调 GetItemActionTask 却忘了 Tips；
- TipKey 必须在 tipsInfo 中/英/日图集有对应 Sprite；
- 默认 ETipsType.Item（与剑一致），除非报告另定；
- 代码含详细注释；重要取舍写清原因。

提交说明：发什么道具、TipKey、挂在对话哪一节点、如何验收。
```
