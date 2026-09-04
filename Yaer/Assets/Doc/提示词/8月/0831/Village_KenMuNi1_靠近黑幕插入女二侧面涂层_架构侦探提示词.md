# Cursor Agent Prompt · 靠近 NPC 黑幕切换：插入女二侧面涂层（玩家在其下）与老人对白错层

> **角色**：先【架构侦探】只读溯源与分层方案，报告后再【施工员】  
> **日期**：2026-08-31  
> **场景**：`Village_KenMuNi1`（挂点对齐近期「靠近村长 / 门口对白」线；若实为老农须在报告证伪改挂）  
> **产品设定（钉死）**：  
> 1. 玩家 **走近该 NPC** → 屏幕 **黑幕淡入淡出一下**  
> 2. 黑幕用途：**添加女二号侧面美术**（用户附件：侧身左向、蝶翼裙全身立绘）进场景涂层  
> 3. **以「玩家在女二图片涂层下面」为基准**，前后错开，再与 **老人** 对话（视觉纵深：女二盖住/错开玩家，对白面向老人）  
> **关联**：`提示词/0831/Village_KenMuNi1_靠近村长黑幕播门口初次对话_架构侦探提示词.md`（黑幕+靠近）；门口三立绘 Prefab 是 **UI 对话壳**，本期侧重 **场景侧身涂层**，二者勿混为一谈  
> **本阶段（侦探）**：只读；禁止改场景 / 代码 / 导图（可只记录附件落盘建议）  
> **报告落盘**：`Assets/Doc/执行文档/0831/Village_KenMuNi1_靠近黑幕插入女二侧面涂层_架构溯源报告.md`

把下面「侦探」整段复制给 Cursor Agent（Agent Mode）执行。施工 Prompt 见文末。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 产品白话

> 走近 NPC 时先黑一下，黑的时候把 **女二侧面图** 摆进场里；亮屏后玩家站在女二图层 **下面**（被侧面立绘挡住或前后错开），再跟 **老人** 说话——像舞台分层，不是只弹 UI 大立绘。

### 用户附件美术（女二侧面）

| 项 | 描述 |
|----|------|
| 构图 | **全身侧身**（面朝左）、透明底 |
| 特征 | 尖耳、浅褐长发、白衣 + 青蓝蝶翼半透明裙、赤足 |
| 用途假说 | **场景 Sprite** 涂层（非 Mask 小头像、非正脸 UI `GushaPainting`） |
| 落盘建议 | `Assets/ArtRes/.../女二侧面` 或报告定路径；Import 为 Sprite、PPU 对齐村立绘 |

**角色身份**：项目「女二」通常 = **古莎**；侦探核对是否已有同图 / 是否新资源；勿与铠甲正脸 `GushaPainting` 混用。

### 期望时序

```
走近触发 NPC（村长旁 / 或老人旁 —— OPEN）
  → BlackPanel ShowFade（全黑）
  → 【黑幕内】启用/生成「女二侧面」场景物体；设好 Sorting 相对玩家
  → （可选同拍）TriggerStory 门口对白 / 老人对白
  → HideFade
  → 可见：女二侧面涂层在玩家「之上」；与老人对话前后错开
  → 对白结束：侧面涂层是否保留/淡出/销毁 —— OPEN
```

黑幕 **不只是**「遮丑切对话」，核心是 **无缝插入女二侧面层**。

### 分层规则（钉死 · 施工须可测）

| 层级意图 | 规则 |
|----------|------|
| **基准** | **玩家渲染在女二侧面图之下**（女二 Sorting 更高，或女二在更「前」的 sortingLayer） |
| 与老人 | **前后错开**：老人、女二、玩家三者世界位 / sorting 不叠成一团；对话朝向老人 |
| 村 2.5D | Y=纵深；勿用 Z 做村庄位移（`02_SYSTEM_SPEC`）；排序用 **SortingLayer / sortingOrder** 或现网 `VillageSceneObjectDepthSort` / 玩家 `DepthComponent` |

对照现网：

| 系统 | 路径 |
|------|------|
| 场景物遮挡 | `VillageSceneObjectDepthSort`（青石围栏金样） |
| 玩家排序 | `TownPlayerLocomotion` / Depth 相关 |
| UI 大立绘 | `DialogueSceneContainer` 内 Canvas——**另一套**；侧面涂层优先 **世界 SR** |

### 与「靠近村长黑幕播门口对白」关系

| | 靠近村长提示词 | 本需求 |
|--|----------------|--------|
| 黑幕 | 有 | **有**，且目的写清 = 插女二侧面 |
| 对白 | `Village_村长家门口初次对话` | 与 **老人** 对话——须裁定是否同一条 / 另绑老农 |
| 女二 | 仅 UI 三立绘里的古莎正脸 | **新增场景侧面涂层** |
| 玩家分层 | 未强调 | **玩家在女二下** 为验收硬条件 |

倾向：同一靠近触发里 **串**「插侧面 → 播对白」；侦探画一张合并时序，避免两次黑幕。

### 「该 NPC / 老人」身份（开放必答）

| 假说 | 含义 | 助手倾向 |
|------|------|----------|
| **A** | 靠近 **村长**，与村长（用户口中老人）对白；女二侧面站身旁 | 接续 0831 村长线 |
| B | 靠近 **老农 `Npc_Farmer`**，与老人打水对白；插女二侧面 | 若产品口「老人」=老农 |
| C | 靠近村长，但对白仍是门口三人戏；侧面只是场景氛围层 | 可能 |

报告 **必须拍板 A/B/C**，并写 StoryPrefabName。

### 侧面物体方案倾向

| 方案 | 做法 | 倾向 |
|------|------|------|
| **S1** | 场景预置 `GushaSidePortrait`（默认关）；黑幕内 `SetActive(true)` + 校准 sorting | ✅ 简单 |
| S2 | 运行时 Instantiate Prefab | 灵活；注意销毁 |
| S3 | 只用对话 UI 侧图 | ❌ 难满足「玩家在涂层下」世界遮挡 |

结束策略 OPEN：对白完隐藏 / 常驻到离区 / 再黑幕摘掉。

### 本期边界

| 做 | 不做 |
|----|------|
| ✅ 靠近 → 黑幕 → 插入女二侧面 → 玩家在其下 | ❌ 把侧面图塞进 Mask 小头像充数 |
| ✅ 与老人对白错层可玩 | ❌ 改村庄 Y→Z |
| ✅ 排序可验收（截图/Gizmo） | ❌ 重做女二正脸表情系统 |
| ✅ 与村长黑幕触发合并或解耦写清 | ❌ 无黑幕瞬切露馅 |

### 严禁

- 亮屏后才「弹」出女二（应用黑幕遮插入）  
- 女二 sorting **低于**玩家导致「玩家压在女二上」（违背基准）  
- 侧面 SR 与 UI `GushaPainting` 抢同一套逻辑硬绑  
- 合层装饰直接当唯一物理触发却 Z≠0 翻车（对齐 Npc_Farmer 先例）  

---

## 侦探 Prompt（复制给 Agent）

```text
你是【架构侦探】。Unity 2020.3.48f1 / C#。禁止修改代码、场景、资源。默认中文。

@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Project_context.md
@Assets/Doc/02_SYSTEM_SPEC.md

## 产品目标
1. 玩家走近指定 NPC → 黑幕淡入淡出。
2. 黑幕期间添加「女二号侧面」美术到场景涂层（用户附件：侧身蝶翼裙全身图）。
3. 分层基准：玩家渲染在女二侧面图之下；与老人对话时前后错开。
4. 与既有「靠近村长黑幕→门口初次对话」方案对齐或明确合并/拆分。

## 必读
@Assets/Doc/提示词/0831/Village_KenMuNi1_靠近村长黑幕播门口初次对话_架构侦探提示词.md
@Assets/Doc/执行文档/0831/Village_KenMuNi1_靠近村长黑幕播门口初次对话_架构溯源报告.md
（若尚无该报告则以提示词+场景为准）
@Assets/Doc/执行文档/0831/Village_村长家门口初次对话_三人大立绘与Face123导入_架构溯源报告.md
@Assets/Scripts/Game/GameRuntime/Entities/Component/Physics/VillageSceneObjectDepthSort.cs
@Assets/Scripts/Game/GameRuntime/Entities/Player/Components/TownPlayerLocomotion.cs
@Assets/Doc/执行文档/0830/Village_KenMuNi1_精灵池中对齐青石围栏遮挡_架构溯源报告.md
@Assets/Scripts/Game/GameRuntime/Entities/SceneEntities/CommonEntity/SimpleStoryTrigger.cs
@Assets/Scripts/Game/GameRuntime/UI/Component/BlackFade/BlackFadeComponent.cs
@Assets/GameRes/Scenes/Village_KenMuNi1.unity
@Assets/Prefabs/DialougeProtrait/GushaPainting.prefab

检索：村长、老人、Npc_Farmer、Npc_Chief、SortingOrder、DepthSort、Gusha、侧面、ShowFade。

## 侦探任务
1. 拍板触发 NPC + 对白 Prefab（A 村长门口 / B 老农 / C 其它）——写进结论，勿含糊。
2. 女二侧面：场景 SR Prefab 结构、世界坐标相对玩家/老人、SortingLayer/Order 数值建议（保证玩家在其下）。
3. 黑幕内插入时序（与 TriggerStory 同拍还是先插层再对白）；结束后侧面去留。
4. 与 UI 三立绘/Mask 古莎的职责边界表。
5. 最小改动清单 + 验收（必须含「玩家被女二侧面挡住/错开」肉眼项）+ OPEN。

## 报告落盘
Assets/Doc/执行文档/0831/Village_KenMuNi1_靠近黑幕插入女二侧面涂层_架构溯源报告.md

结构：①结论 ②触发与对白身份 ③黑幕+插层时序 ④Sorting 方案 ⑤与 UI 立绘边界 ⑥最小清单 ⑦验收 ⑧OPEN ⑨程序补充

沟通：①结论一句话 ②原因 ③用户检查清单 ④程序补充
```

---

## 施工 Prompt（报告拍板后另开 Agent 再复制）

```text
你是【施工员】。Unity 2020.3.48f1 / C#。最小化修改。默认中文。

@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0831/Village_KenMuNi1_靠近黑幕插入女二侧面涂层_架构溯源报告.md

## 目标
1. 走近报告指定 NPC → 黑幕淡入淡出。
2. 黑幕内启用女二侧面场景涂层（导入用户侧身图）；sorting 保证玩家在其下。
3. 与老人（报告指定）对白前后错开可玩；对白名按报告。
4. 详细注释；说明落盘：
   Assets/Doc/施工说明/0831/Village_KenMuNi1_靠近黑幕插入女二侧面涂层_施工说明.md

## 约束
- 村庄纵深不用 Z 位移；排序用现网 Sorting/DepthSort 体系。
- 勿用 Mask/UI 正脸古莎冒充场景侧面层。
- 与「靠近村长黑幕」「门口三立绘」按报告合并，避免双重黑幕。
- 导图路径与 Import 设置写入施工说明。

## 验收
- [ ] 靠近触发 → 先黑再亮，亮后场上有女二侧面
- [ ] 玩家站位上：女二图在玩家「上面」（玩家在涂层下）
- [ ] 与老人对话时三人/两人不糊成一层，前后可读
- [ ] 黑幕前不裸切露馅；存档单次若报告要求则生效
- [ ] UI 对话大立绘/Mask 回归不坏

## 沟通风格
①结论 ②原因 ③用户检查清单 ④程序补充
```

---

## 给开发者

1. 把女二侧面 PNG 放进工程（或施工时按报告路径导入）。  
2. 先跑「侦探 Prompt」——务必拍板：靠近的是 **村长还是老农**、对白用哪条 Prefab。  
3. 再跑「施工 Prompt」。  
4. 验收时看 Scene：玩家应在女二侧面 **下方图层**，不是压在女二上。
