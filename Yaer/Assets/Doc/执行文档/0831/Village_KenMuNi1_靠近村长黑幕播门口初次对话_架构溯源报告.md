# Village_KenMuNi1 — 靠近合层「村长」黑幕播门口初次对话 — 架构溯源报告

**文档版本**：v1.0（2026-08-31）  
**文档性质**：【架构侦探】只读定点 + 时序拍板（**本阶段未改场景 / 代码**）  
**Unity**：2020.3.48f1  
**场景**：`Assets/GameRes/Scenes/Village_KenMuNi1.unity`  
**产品**：靠近合层 **`村长`** 身旁 → **自动**黑幕渐入 → 全黑后 `TriggerStory("Village_村长家门口初次对话")` → 黑幕渐出  
**不是**：`House_Chief` 进屋换场；不是点 E 才播  
**依赖**：门口对话 Prefab / Face123 Import / 三立绘（可并行；Story 名钉死）  
**提示词**：`提示词/0831/Village_KenMuNi1_靠近村长黑幕播门口初次对话_架构侦探提示词.md`  
**关联**：`执行文档/0831/Village_村长家门口初次对话_三人大立绘与Face123导入_架构溯源报告.md` · 老农 `Npc_Farmer` · 店 `ShowShopBlackFade`

---

## 沟通摘要

### ① 结论一句话

**合层 `村长` 仅 Transform+SR 且 Z≈2.8，不宜挂物理；应对齐老农新建 `Objects/Npc_Chief`（Z=0、Enter、sceneObjs），用系统 BlackPanel ShowFade→全黑后 TriggerStory→壳就绪 HideFade；与 `House_Chief` 进屋门解耦；存档单次。**

### ② 原因（通俗）

合层里的「村长」只是贴画，不能当门铃。  
要在她脚边放一个看不见的感应区：人一走进去先黑屏，黑透了再播门口三人对话，然后再亮回来。  
旁边那扇能进屋的门是另一回事，别绑在一起。

### ③ 用户检查清单（施工后）

| # | 操作 | 通过 |
|---|------|------|
| 1 | 走近村长身旁（**不点 E**） | 先黑幕，再出门口对白 |
| 2 | 黑幕全黑前 | 不露三大立绘穿帮 |
| 3 | 对白名 | `Village_村长家门口初次对话` |
| 4 | 同档再走近 | **不再**触发（单次） |
| 5 | 点 `House_Chief` | **只进屋**，不误播/不挡门 |
| 6 | 合层 `村长` | 美术仍在；Console 无 Missing |

---

## ① 结论（程序向）

同沟通摘要「结论一句话」。核心拍板：

| 项 | 裁定 |
|----|------|
| 交互真源 | **`Objects/Npc_Chief`**（非合层 `村长`） |
| 触发 | **`TriggerType.Enter`** + `SingleUseInArchive=true` |
| 黑幕 | 系统 **BlackPanel** Show→Trigger→Hide（仿店，非换场 Defer） |
| 进屋门 | **`House_Chief` 不动**；与对白解耦 |

---

## ② 合层锚点（磁盘已核实）

| 物体 | 组件 | local（父下） | 世界约（父 `(-172.42,-7.8,0)`） |
|------|------|---------------|--------------------------------|
| **`村长`** `\u6751\u957F` | Transform + **SpriteRenderer**；Layer **0**；无 Collider / Interactive | `(14.74, 6.59, **2.82**)` | **≈(-157.7, -1.2, 2.82)** |
| **`村长家门`** | Transform + SR | `(13.44, 10.96, 3.08)` | ≈(-159.0, 3.2, 3.1) |
| **`House_Chief`** | SceneChangeDoor PrefabInstance（Objects） | — | **`(-158.3, 1.9, 0)`** → `Village_Chief_House` |
| **`Npc_Chief`** | ❌ **不存在** | — | — |
| **`Npc_Farmer`**（样板） | Layer21；`BaseSceneEntity`+Interactive+Collider+`SimpleStoryTrigger`(Click)；Z=0；已入 `sceneObjs` | `(-82.6, 2.655, 0)` | 合层 `农` 旁 |

父链：`村长` → 合层节点（Transform `445886…`，local `(-172.42,-7.8,0)`）→ `Design(0,0)` → `Map(0,0)`。

**裁定**：合层 `村长` = **仅装饰**（Z≠0）。**禁止**只在合层节点上硬挂 2D 物理（对齐 0830 老农结论）。

---

## ③ 交互实体方案（拍板）

对齐老农 **方案 A**：合层保留美术，Objects 新建交互体。

| 项 | 拍板 |
|----|------|
| 新建 | **`Objects/Npc_Chief`**（名可微调，报告用此） |
| 世界位 | 对齐合层 `村长` 脚位 **≈(-157.7, -1.2)**，**强制 Z=0**；Scene 微调贴脚 |
| 组件 | `BaseSceneEntity` + `InteractiveComponent` + Collider2D(**Trigger**) + **`ChiefNearDoorStoryTrigger`**（`: SimpleStoryTrigger`） |
| Layer | **21**（对齐 `Npc_Farmer`） |
| `requirePlayerOverlap` | **1**（近距） |
| 碰撞范围 | 盖住村长身旁可走区（可参考 Farmer ≈2.2×2.8，**勿盖死 `House_Chief` 门热区**） |
| `StoryPrefabName` | **`Village_村长家门口初次对话`** |
| `triggerType` | **`Enter`**（T1） |
| `SingleUseInArchive` | **true** |
| `sceneObjs` | **须登记** |
| 合层 `村长` | **保留**，不删 |

| 触发模式 | 裁定 |
|----------|------|
| **T1 Enter** | ✅ 靠近即播 |
| T2 Stay 0.3～0.5s | 误触再改 |
| T3 Click/E | ❌ 产品要自动 |

**证据**：`SimpleStoryTrigger.TriggerStory()` 为 **`protected virtual`** → 子类可覆写插入黑幕，无需改基类全部 Click 行为。

---

## ④ 黑幕时序（系统 BlackPanel · 非换场 Defer）

| 样板 | 做法 | 本期 |
|------|------|------|
| 店 `ShowShopBlackFade` | Open BlackPanel FadeShow → `onShowEnd` 再业务 | ✅ **仿此主动开黑** |
| 村/店 `TryDeferBlackFadeForCover` | **换场已在黑里**再 Trigger | ❌ 本期不换场，**勿塞进 Defer** |
| Prefab 内 BlackMask | 图内再黑 | ❌ 易叠黑 |

```
玩家 Enter Npc_Chief（SingleUse 未用）
  → OpenUIForm(BlackPanel, FadeShow)   // UIComponentGM + ShowBlackFormArgs
  → onShowEnd（全黑）:
       TriggerStory("Village_村长家门口初次对话")
       订阅 StoryComponentGSM.onStoryTriggered
  → onStoryTriggered（对话壳就绪）:
       极短 hold（可选 0～0.15s）
       → BlackFormLogic HideFade / CloseFormHideFade
  → 玩家看完门口对白
  → onStoryEnd → SingleUse 记档（基类已有）
  → 还控
```

序列图：

```
[Explore] --Enter--> [ShowFade] --全黑--> [TriggerStory]
                                              |
                                    onStoryTriggered
                                              |
                                         [HideFade] --> [Dialogue] --> [End/单次]
```

| 项 | 拍板 |
|----|------|
| 谁编排 | **`ChiefNearDoorStoryTrigger`** 覆写 `TriggerStory()`（或 Enter 路径）；**不**改 GSM 进村 Defer |
| 未全黑就 Trigger | ❌ |
| 无黑幕直弹对白 | ❌ |
| 超时兜底 | 壳未起来仍 HideFade，防永久卡黑（可短超时） |

---

## ⑤ 与 `House_Chief` 进屋门解耦

| | 门口对白 | 进屋 |
|--|----------|------|
| 实体 | **`Npc_Chief`** | **`House_Chief`** |
| 位置 | 合层村长脚边 ≈(-157.7,-1.2, **0**) | (-158.3, 1.9, 0) 略偏门 |
| 行为 | Enter → 黑幕 → 对白 | Click/E → `Village_Chief_House` |
| 存档 | Story **SingleUse** | 换场无关 |

**严禁**：StoryPrefab 写到 `House_Chief`；`Npc_Chief` 碰撞盖住整扇门导致「想进屋却先黑幕」。  
台本末句「快进屋」→ 播完后玩家 **手动** 点门（Q4）。

覆盖门口三立绘报告原 Q1「门前新建 Trigger」→ 本报告钉死为 **合层 `村长` 旁 `Npc_Chief`**（非门、非进屋门）。

---

## ⑥ 最小施工清单

| # | 动作 | 优先级 |
|---|------|--------|
| 1 | 新建 `ChiefNearDoorStoryTrigger.cs`：Enter + BlackPanel Show→Trigger→Hide | **P0** |
| 2 | 场景 `Objects/Npc_Chief`：位/Collider/Interactive/Trigger/单次/`sceneObjs` | **P0** |
| 3 | 合层 `村长` 不动美术 | — |
| 4 | 回归：`House_Chief` 仍只进屋 | **P0** |
| 5 | 门口 Prefab 就绪后联调验收 | **P0**（依赖） |
| 6 | 同步 OPEN（门口 Q1 + 本节） | P0 |

**不改**：GSM 进村 DeferCover；晚宴；商店黑幕大改；Update 堆业务。

**依赖**：

| 项 | 状态 |
|----|------|
| CSV | ✅ `Village_村长家门口初次对话.csv` |
| Dialogue Prefab | ❌ 待 Import/三立绘 |
| Story 名 | **必须** `Village_村长家门口初次对话` |

场景 Trigger **可先合入**；全链路 Play 须 Prefab 可播。

---

## ⑦ 验收清单

同沟通摘要 §③；另：

- [ ] Prefab 未好时：Enter 仍能开黑幕并尝试 Trigger（Console 可有 Missing，**不崩**）
- [ ] Prefab 好后：三立绘在黑幕后显现，无穿帮
- [ ] 同档第二次走近：不黑幕、不重播

---

## ⑧ OPEN

| ID | 问题 | 侦探倾向 | 状态 |
|----|------|----------|------|
| Q1 | Enter 立刻 vs Stay？ | **Enter** | ✅ 本报告拍板 |
| Q2 | 黑幕时长？ | 默认 BlackPanel show/hide；可调 | ⏳ |
| Q3 | 单次键？ | **SingleUseInArchive** + Prefab 名 | ✅ |
| Q4 | 结束后自动提示进屋？ | 否；台本已有「快进屋」，门手动 | ✅ |
| Q5 | Prefab 未完工先合场景？ | **可**；联调等 Prefab | ✅ |
| Q6 | 碰撞是否避开门热区？ | **是**；人像旁为主 | ⏳ 施工微调 |

同步：门口三立绘报告 **Q1** → 触发点 = **合层村长旁 `Npc_Chief` + 黑幕 Enter**（见 `OPEN_QUESTIONS.md`）。

---

## ⑨ 程序补充（速查）

| API / 样板 | 用途 |
|------------|------|
| `SimpleStoryTrigger` Enter / `SingleUseInArchive` / `protected virtual TriggerStory` | 基类事件与存档；子类插黑幕 |
| `StoryComponentGSM.TriggerStory` / `onStoryTriggered` / `onStoryEnd` | 播与壳就绪 / 结束记档 |
| `UIComponentGM.OpenUIForm(BlackPanel)` + `ShowBlackFormArgs` | 同店 `ShowShopBlackFade` |
| `BlackFormLogic` / `BlackFadeComponent.HideFade` | 淡出 |
| `Npc_Farmer`（KenMuNi1 Objects） | 三件套 + `sceneObjs` 样板（Click→本期改 **Enter**） |
| 合层 `村长` 世界 ≈ | **(-157.7, -1.2)**；交互体 **Z=0** |
| `House_Chief` | **(-158.3, 1.9, 0)**；仅进屋，勿绑 Story |
