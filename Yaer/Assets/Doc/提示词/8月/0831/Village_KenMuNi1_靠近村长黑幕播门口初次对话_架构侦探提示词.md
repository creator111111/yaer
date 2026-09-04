# Cursor Agent Prompt · KenMuNi1 合层「村长」：靠近自动黑幕 → 门口初次对话

> **角色**：先【架构侦探】只读定点与时序，报告后再【施工员】  
> **日期**：2026-08-31  
> **场景**：`Assets/GameRes/Scenes/Village_KenMuNi1.unity`  
> **Hierarchy 锚点（用户红箭头）**：合层美术物体 **`村长`**（旁近 **`村长家门`**；同层还有 `兵`、`艾米艾莉` 等）  
> **产品设定（钉死）**：玩家 **靠近村长身边** → **自动** 黑屏渐入 →（全黑后）进入对话 → 黑屏渐出；播 **`Village_村长家门口初次对话`**  
> **不是**：点 `House_Chief` 进屋换场；不是点 E 才播（靠近即触发）  
> **依赖**：门口对话 Prefab / Import Face123 / 三立绘（见 `执行文档/0831/Village_村长家门口初次对话_三人大立绘与Face123导入_架构溯源报告.md`）——场景触发可与 Prefab 并行设计，但验收须 Prefab 可播  
> **本阶段（侦探）**：只读；禁止改场景 / 代码  
> **报告落盘**：`Assets/Doc/执行文档/0831/Village_KenMuNi1_靠近村长黑幕播门口初次对话_架构溯源报告.md`

把下面「侦探」整段复制给 Cursor Agent（Agent Mode）执行。施工 Prompt 见文末。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 产品时序（期望）

```
玩家走进「村长」附近（合层脚位一带）
  → 自动（无需点 E）
  → BlackPanel 黑幕渐入（ShowFade）
  → 全黑后 TriggerStory("Village_村长家门口初次对话")
  → 对话壳 / 三大立绘就绪
  → 黑幕渐出（HideFade）
  → 玩家看完门口对白（雅+古+村长）
  → 对白结束还控；存档记「已播过」（倾向单次）
```

**禁止**理解成：靠近直接露对话不黑幕；或黑幕只为进屋换场。

### 场景锚点（用户截图）

| 物体 | 角色假说 |
|------|----------|
| **`村长`**（红箭头） | 合层 **美术立绘**；须配置为靠近触发源（或仅作对齐参考） |
| **`村长家门`** | 美术门；**≠** 本期触发真源（进屋另案 `House_Chief`） |
| `House_Chief`（若 Objects 下） | `SceneChangeDoor` 进 `Village_Chief_House`——**与门口对白解耦** |

0831 门口报告 Q1 曾写「门前新建 Trigger」——本需求 **拍板挂点对齐合层 `村长`**（身旁），侦探须在 YAML/Hierarchy 核世界坐标与 Z。

### 合层美术 vs 交互实体（老农先例）

| 老农（0830） | 村长（本期倾向） |
|--------------|------------------|
| 合层 `农` = 仅 SR 装饰 | 合层 **`村长`** = 装饰（Z 可能 ≠0） |
| `Objects/Npc_Farmer` = Interactive + StoryTrigger | 新建 **`Objects/Npc_Chief`**（或报告定名）Z=0、进 `sceneObjs` |
| 合层保留不删 | 合层 `村长` **保留**；交互体对齐其脚位 |

**严禁**只在合层 `村长` 上硬挂 2D 物理导致排序/Z 翻车（对齐老农结论）。

### 靠近 = 自动（Trigger 模式）

| 方案 | `SimpleStoryTrigger` | 倾向 |
|------|----------------------|------|
| **T1 · Enter** | `TriggerType.Enter`：脚进范围即触发 | ✅ 简单「靠近即播」 |
| T2 · Stay | 范围内停 N 秒再播 | 防蹭边误触；可选 |
| T3 · Click | 点/E | ❌ 产品要自动 |

碰撞：挂 `InteractiveComponent` + Collider2D（范围盖住村长身旁可走区）；与玩家 `PlayerFoot` / Interactive 规则对齐现网。

### 黑幕 + Trigger 样板

| 样板 | 做法 | 适用 |
|------|------|------|
| **店进店 Cover** | 已黑时 Trigger → 壳就绪再 HideFade | ✅ 时序最接近「黑里进对话再亮」 |
| 换场 `LoadScene` 黑幕 | 切场景专用 | ❌ 本期不换场 |
| Prefab 内 BlackMask Action | 图内再黑一层 | ⚠️ 易与系统黑幕叠；倾向 **系统 BlackPanel** 由 Trigger/GSM 管 |

倾向伪代码：

```
OnPlayerNearChief (首次):
  Open BlackPanel ShowFade
  onShowEnd:
    TriggerStory("Village_村长家门口初次对话")
    // 对话壳就绪（onStoryTriggered / Panel 打开）后 HideFade
  SingleUseInArchive = true  // 倾向；与 Q5 对齐
```

侦探须点名：黑幕 API 走 `UIComponentGM`+`BlackPanel` / `BlackFadeComponent`，**谁订阅**（专用 `ChiefDoorStoryTrigger` 子类 vs GSM 方法）。

### 与门口 Prefab 依赖

| 项 | 状态（0831 报告） |
|----|-------------------|
| CSV | ✅ `Village_村长家门口初次对话.csv` |
| 对话 Prefab | ❌ 待 Import/三立绘（可并行；触发名先钉死） |
| Story 名 | **`Village_村长家门口初次对话`**（与 Prefab 名一致） |

场景施工可先挂 Trigger；若 Prefab 未好，验收标「Trigger 通 + Prefab 通」两段。

### 本期边界

| 做 | 不做 |
|----|------|
| ✅ 配置合层旁交互：靠近 → 黑幕 → 门口对白 | ❌ 改 `House_Chief` 进屋逻辑当对白 |
| ✅ 存档单次（倾向） | ❌ 每次靠近都黑幕重播（除非产品改口） |
| ✅ Z=0 Objects 实体 + sceneObjs | ❌ 合层 `村长` 直接当唯一物理体（除非侦探证伪可挂） |
| ✅ 与三立绘 Prefab 名对齐 | ❌ 本期重做晚宴/商店黑幕 |

### 严禁

- 靠近无黑幕直接弹对白（违背设定）  
- 黑幕未全黑就 Trigger 导致露景穿帮  
- 触发绑在 `House_Chief` 门上导致「想进屋却播门口对白」或反过来  
- Update 堆业务；须 `StoryComponentGSM.TriggerStory`  

### 开放

| ID | 问题 | 助手倾向 |
|----|------|----------|
| Q1 | Enter 立刻 vs Stay 0.3～0.5s？ | **Enter**；误触再改 Stay |
| Q2 | 黑幕时长？ | 对齐村/店默认 show/hide；可调 |
| Q3 | 单次存档键 = Story 名？ | **SingleUseInArchive** 对齐 Prefab 名 |
| Q4 | 对白结束后是否自动提示进屋？ | 台本已有「快进屋」；门仍手动 |
| Q5 | Prefab 未完工时场景是否先合入？ | 可先合 Trigger；Play 验收等 Prefab |

---

## 侦探 Prompt（复制给 Agent）

```text
你是【架构侦探】。Unity 2020.3.48f1 / C#。禁止修改代码、场景、Prefab。默认中文。

@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Project_context.md
@Assets/Doc/02_SYSTEM_SPEC.md

## 产品目标
Village_KenMuNi1：玩家靠近合层美术物体「村长」身边 → 自动黑屏渐入 → 全黑后播
Village_村长家门口初次对话 → 黑屏渐出。
须配置场景里的「村长」（用户 Hierarchy 红箭头，旁近「村长家门」）。
不是 House_Chief 进屋门；不是点击才播。

## 必读
@Assets/GameRes/Scenes/Village_KenMuNi1.unity
@Assets/Scripts/Game/GameRuntime/Entities/SceneEntities/CommonEntity/SimpleStoryTrigger.cs
@Assets/Scripts/Game/GameRuntime/Entities/SceneEntities/Village_KenMuNi/FarmerQuestStoryTrigger.cs
@Assets/Doc/施工说明/0830/Village_KenMuNi1_老农基础对话交互_施工说明.md
@Assets/Doc/执行文档/0830/Village_KenMuNi1_老农基础对话交互_架构溯源报告.md
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Black/BlackFormLogic.cs
@Assets/Scripts/Game/GameRuntime/UI/Component/BlackFade/BlackFadeComponent.cs
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Scene/Village_Shop/Village_ShopSceneManager.cs
（进店黑幕内 Trigger 再淡出的 Cover 时序）
@Assets/Doc/执行文档/0831/Village_村长家门口初次对话_三人大立绘与Face123导入_架构溯源报告.md
@Assets/Doc/OPEN_QUESTIONS.md（门口初次对话 Q1）
@Assets/Dialog/Village_村长家门口初次对话.csv

检索：村长、村长家门、House_Chief、Npc_Farmer、ShowFade、HideFade、TriggerStory、SingleUseInArchive。

## 侦探任务
1. 定位合层「村长」「村长家门」世界坐标 / 组件（是否仅 SR）；Objects 下有无已有 Chief 交互体。
2. 裁定：交互挂合层 vs 新建 Npc_Chief（对齐 Npc_Farmer）。
3. 设计靠近自动触发（Enter/Stay）+ 系统黑幕 ShowFade→TriggerStory→HideFade 时序；对照店 Cover，画序列图。
4. 与 House_Chief 进屋门解耦说明；存档单次方案。
5. 最小改动清单（场景 + 可选 ChiefDoorNearStoryTrigger 脚本）+ 验收 + 更新 OPEN Q1。
6. 注明对门口对话 Prefab 的依赖（名必须一致）。

## 报告落盘
Assets/Doc/执行文档/0831/Village_KenMuNi1_靠近村长黑幕播门口初次对话_架构溯源报告.md

结构：①结论 ②合层锚点 ③交互实体方案 ④黑幕时序 ⑤与进屋门解耦 ⑥最小清单 ⑦验收 ⑧OPEN ⑨程序补充

沟通：①结论一句话 ②原因 ③用户检查清单 ④程序补充
```

---

## 施工 Prompt（报告拍板后另开 Agent 再复制）

```text
你是【施工员】。Unity 2020.3.48f1 / C#。最小化修改。默认中文。

@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0831/Village_KenMuNi1_靠近村长黑幕播门口初次对话_架构溯源报告.md

## 目标
1. 配置 Village_KenMuNi1：靠近合层「村长」旁 → 自动黑幕渐入 → TriggerStory("Village_村长家门口初次对话") → 黑幕渐出。
2. 合层「村长」保留美术；交互按报告（倾向 Objects/Npc_Chief，Z=0，sceneObjs）。
3. 与 House_Chief 进屋换场解耦；存档单次（若报告要求）。
4. 禁止无黑幕直弹对白；禁止 Update 堆业务。

## 依赖
若门口对话 Prefab 尚未落盘：先保证 Trigger 名与报告一致；Play 全链路验收须 Prefab 可播
（可与「三人大立绘 + Face123 Import」施工并行，但合并验收）。

## 落盘
Assets/Doc/施工说明/0831/Village_KenMuNi1_靠近村长黑幕播门口初次对话_施工说明.md
同步 OPEN_QUESTIONS 门口 Q1。

## 验收
- [ ] 走近村长身旁（不点 E）→ 先黑幕再出门口对白
- [ ] 黑幕全黑前不露三大立绘穿帮
- [ ] 对白名为 Village_村长家门口初次对话
- [ ] 播过后同档不再触发（若单次）
- [ ] 点 House_Chief 仍只进屋，不误播/不挡门
- [ ] 合层「村长」美术仍在；Console 无 Missing

## 沟通风格
①结论 ②原因 ③用户检查清单 ④程序补充
```

---

## 给开发者

1. 先跑「侦探 Prompt」→ 定点合层 `村长` 与黑幕时序。  
2. 门口对话 Prefab（三人 + Face123）若未好，可与本触发 **并行**，但最终要一起验收。  
3. Hierarchy 锚点：**`村长`**（旁 **`村长家门`**）；交互真源倾向 **Objects 新实体**，不要只改合层节点。
