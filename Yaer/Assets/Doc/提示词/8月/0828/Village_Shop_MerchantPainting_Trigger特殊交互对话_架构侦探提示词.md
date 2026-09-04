# Cursor Agent Prompt · Village_Shop：MerchantPainting Trigger（Head/Chest）特殊交互对话 + 表情

> **角色**：先【架构侦探】只读溯源，报告拍板后再【施工员】  
> **日期**：2026-08-28  
> **场景**：`Village_Shop.unity` · `商店界面合层` → `MerchantPainting`  
> **用户 Hierarchy（截图 · 已加交互区）**：
> ```
> 商店界面合层
>   ├── 背景
>   └── MerchantPainting
>         ├── Body
>         ├── Face
>         └── Trigger          ← 用户新建
>               ├── Head      ← 点头交互区
>               └── Chest     ← 点胸交互区
> ```
> **台本真源**：`Assets/Doc/执行文档/6月/0601/Village_商店老板娘特殊交互_对白台本_执行说明.md`  
> **表情真源（现网）**：店句走 `Face1～5` + `BodyType` Toggle（**不是** 0601 表里的 Smile/Angry）  
> **本阶段范围**：**点 Head / Chest → 播对应特殊对话 → 对话过程中老板娘（及雅）表情照常变**；胸部线 **C6 黑屏转树屋** 只出挂钩方案，是否本期实现由侦探裁定并写清  
> **本阶段**：只读；禁止改代码 / Prefab / 场景 / CSV

把下面「侦探」整段复制给 Cursor Agent（Agent Mode）执行。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 产品需求（白话）

> 玩家在商店里 **点老板娘头 / 胸**（`Trigger/Head`、`Trigger/Chest`）→ 弹出 **特殊短对话**（0601 §1 / §1.2）→ 对话过程中：  
> - **店（老板娘）**：场景合层 `Body`/`Face` 子物体切换 +（若已接）Mask 小表情，与首次进店店句 **同一套表情系统**  
> - **雅**：对话框小表情 / 立绘走原 `DialogueFaceType` 链  
> 交互区用户已在 Hierarchy 挂好，缺的是：**怎么点到 → 怎么 TriggerStory → 台本/表情怎么对齐现网**。

### 与「首次进店」边界（钉死 · 勿混）

| 需求 | Prefab / 触发 | 本期？ |
|------|---------------|--------|
| 首次进店整段对白 | `Village_ShopStart` · GSM 自动 Trigger | ❌ 已另期；**勿重做** |
| **点头特殊对白** | 建议 `Village_ShopKeeper_HeadClick` | ✅ **本期主目标** |
| **点胸特殊对白** | 建议 `Village_ShopKeeper_ChestClick` | ✅ **本期主目标**（树屋后续可分期） |
| 买卖 UI | `UI_Shop` | 对白期是否藏 Bar：侦探对齐 ShopStart 现网做法 |

### 0601 台本 vs 0827 表情系统（必写迁移表）

0601 表给老板娘填的是策划语义脸（`Smile`/`Angry`…），**现网店句已否决**把这些塞进 `DialogueFaceType`，改用：

| 层 | 现网约定 |
|----|----------|
| CSV Speaker `店` | Actor `老板娘` · `UseShopkeeperPortrait=true` |
| Face | **`Face1`～`Face5`**（`ShopkeeperFaceType`） |
| Body | **`Normal` / `Red` / `YinXian`**（`BodyType` 列） |
| 雅句 Face | 仍用 `DialogueFaceType`（`Daze`/`Unhappy`…） |

侦探必须在报告给出 **0601 每句店 FaceType → Face1～5（+ 是否换身）建议映射**；开放问题可留给策划改名，但施工不能卡在旧 Smile 键上。

### 点击链路假说（Village_Shop 纯 UI · 须裁定）

`Village_Shop` 是 **纯 UI 场景**（0713），合层多半是 **世界空间 SR / 或挂在 Canvas 下的 Image**。用户只加了 `Trigger/Head|Chest` 节点，**未必已有可点组件**。

| 方案 | 做法 | 适用假说 | 风险 |
|------|------|----------|------|
| **A · UGUI 热区（推荐倾向）** | Head/Chest 挂透明 `Image`（Raycast）+ `IPointerClickHandler` 或 Button | 合层已在 GraphicRaycaster 下 | EventSystem 被 UI_Shop 挡住需排序 |
| **B · 2D Collider + PhysicsRaycaster / OnMouse** | BoxCollider2D isTrigger + 相机 Raycast | SR 世界空间 | 与 UI 射线抢点；Layer 矩阵 |
| **C · 全屏不可见 Button 分区** | 不绑合层，另做热区 Overlay | 摆位易偏 | 和立绘缩放不同步 |
| **D · 临时用 Keyboard Debug** | ❌ 仅验收辅助，不能当产品 | — | — |

### 对白驱动假说（复用现网 · 勿另起炉灶）

```
点击 Head / Chest
  →（互斥 + HasRunningStory 守卫）
  → StoryComponentGSM.TriggerStory("Village_ShopKeeper_HeadClick" | "…_ChestClick")
  → 店句：ShopkeeperFaceRegistry.Apply(body, face) + MerchantMask（若已接）
  → 雅句：DialogueMaskAvatarPresenter / Painting 原链
  → 对白结束 → 恢复买卖 UI（若曾藏）
```

**胸部线 C6**：黑屏 + 切树屋 + C7/C8 —— 与 ShopStart 黑幕组件可复用；侦探裁定 **本期做完店内段（C1～C5）即可**，还是 **必须含转场**。

### 须对拍的现成资产

| 资产 | 用途 |
|------|------|
| `Village_ShopStart.prefab` + Merchant Actor | 复制 Actor 壳 / 不嵌 Painting 的样板 |
| `Village_商店首次对话.csv` | 店行 Face1～5 + BodyType 填法样板 |
| `ShopkeeperFaceController` / Registry | 合层 Toggle 真源 |
| `DialogueTMPUGUI` · `UseShopkeeperPortrait` | 店句表情桥 |
| 0601 台本 §1 / §1.2 | 文案与分支语义 |
| `UI_Shop` / Shop Bar | 对白期是否挡点击、是否藏 |

### 严禁（本阶段）

- 改代码 / Prefab / 场景 / CSV  
- 把点头/点胸并进 `Village_ShopStart` 同一棵图  
- 店句 Face 走 `DialogueFaceType`（Smile/Angry）或扩枚举塞 Face1  
- 用 `Update` 轮询鼠标坐标切脸  
- 头/胸同一帧双触发、对白中仍可连点开第二段  
- 本期重做首次进店存档旗标 / DeferCover（除非点交互必须复用其「藏 UI」API，只写挂钩）

---

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/02_SYSTEM_SPEC.md
@Assets/Doc/执行文档/6月/0601/Village_商店老板娘特殊交互_对白台本_执行说明.md
@Assets/Doc/执行文档/0827/Village_Shop_老板娘BodyFace子物体切换与CSV列设计_架构溯源报告.md
@Assets/Doc/执行文档/0827/Village_ShopStart_新建Merchant对话NPC_架构溯源报告.md
@Assets/Doc/执行文档/0827/MerchantPainting_UI版_商人对话框小表情_架构溯源报告.md
@Assets/Doc/执行文档/0827/Village_Shop_首次进店Village_ShopStart联调_架构溯源报告.md
@Assets/Doc/执行文档/7月/0713/Village_KenMuNi1_Door_Shop换场Village_Shop纯UI_架构溯源与施工执行说明.md
@Assets/Doc/执行文档/6月/0629/商店系统_策划拆解_执行说明.md
@Assets/GameRes/Scenes/Village_Shop.unity
@Assets/ArtRes/Scene/Village/商店界面合层.prefab
@Assets/Prefabs/DialougeProtrait/MerchantPainting.prefab
@Assets/GameRes/Prefabs/Dialogue/Village_ShopStart.prefab
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Scene/Village_Shop/Village_ShopSceneManager.cs
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Shop/ShopkeeperFaceController.cs
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Shop/ShopkeeperFaceRegistry.cs
@Assets/Scripts/Game/GameRuntime/NodeCanvas/NodeCanvasExtend/DialogueTMPUGUI.cs
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Story/DialogueMaskAvatarPresenter.cs
@Assets/Editor/Tool/Dialogue/ShopkeeperCsvDefaults.cs
@Assets/Editor/Tool/Dialogue/DialogueCsvParser.cs

你现在是【架构侦探】。Unity 2020.3.48f1 / C#。
禁止修改任何代码、Prefab、场景、CSV、台本。只读扫描 + 写「Trigger 特殊交互对话」溯源报告。

---

## 背景（策划白话）

1. 用户已在 `MerchantPainting` 下加了 **`Trigger/Head`、`Trigger/Chest`**。  
2. 玩家 **点击** 对应区域 → 播 **特殊对话**（0601：点头货币争执 / 点胸特殊服务）。  
3. 对话过程中老板娘要 **变表情**（与现网店句同一套 Body×Face），雅也要变表情。  
4. 本阶段 **不出施工**，只摸清：点击怎么接、对话 Prefab/CSV 怎么建、表情怎么复用、与买卖 UI / 首次进店如何互斥。

---

## 侦探任务清单

### A. 钉死 Trigger 现状（场景 YAML）

| 项 | 填 |
|----|-----|
| `Trigger` / `Head` / `Chest` 上现有组件？ | Collider / Image / Button / 无？ |
| Layer、Rect、是否 RaycastTarget | |
| 父链：世界空间 SR 还是挂在 Canvas 下？ | |
| `EventSystem` / GraphicRaycaster / PhysicsRaycaster 谁管点？ | |
| `UI_Shop`（买卖条）是否盖住热区？对白中谁挡谁？ | |

### B. 点击 → TriggerStory 架构选型（必填推荐）

在预梳理 **A/B/C** 中拍板，并回答：

1. 点击脚本挂哪？`Head`/`Chest` 各一，还是 `Trigger` 父级分流？  
2. Story 名 / Prefab 名是否沿用 0601 建议（`Village_ShopKeeper_HeadClick` / `…_ChestClick`）？  
3. 谁调 `TriggerStory`：`Village_ShopSceneManager` 封装 vs 热区组件直接调 StoryGSM？  
4. **互斥**：`HasRunningStory`、头胸互斥、对白中禁用热区、结束后再开 —— 现网有无现成 API？  
5. 首次进店对白播放中：热区应 **强制关** 还是本来就被藏 UI 挡住？

### C. 对白资产方案（两分支独立）

| 分支 | Prefab | CSV | Actor 需求 | 表情列 |
|------|--------|-----|------------|--------|
| 点头 §1 | | | 雅尔 + 老板娘（Merchant 壳？） | 店=`Face1～5`+Body；雅=`DialogueFaceType` |
| 点胸 §1.2 店内段 | | | 同上 | 同上 |
| 点胸 C6+ 树屋 | | | 可能仅雅 | 分期？ |

要求：

- **勿**与 `Village_ShopStart` 混图。  
- 给出「复制 ShopStart 最小壳」还是「新建空图再 Import CSV」的推荐步骤。  
- **H3 动作句 / C6 转场** 用 ActionNode，勿当 Say。

### D. 表情迁移表（必出）

把 0601 §1 / §1.2 **每一句店** 的旧 FaceType 映射到现网：

| 台本序号 | Speaker | 旧 FaceType | 建议 ShopFace | 建议 BodyType | 雅句 Face（若有） |
|----------|---------|-------------|---------------|---------------|-------------------|
| H1 | 店 | Smile | Face? | Normal? | — |
| … | | | | | |

并说明：**Mask 小表情是否必须本期同步**（若 MerchantMask 已接，应与合层双轨；若未接，写缺口）。

### E. 与买卖 UI / 首次进店共存

画清状态机（白话即可）：

```
Idle(可买卖、可点头胸)
  → 点 Head/Chest → Talking(禁热区、?藏 Bar)
  → End → Idle
首次进店 Talking 期间：热区 = OFF
```

对白期是否复用 ShopStart 的「藏 `UI_Shop`」逻辑？只设计挂钩，不重做首次进店。

### F. 胸部线分期裁定（必写）

| 范围 | 是否本期施工建议 |
|------|------------------|
| C1～C5 店内对白 + 表情 | |
| C6 黑屏转树屋 | |
| C7～C8 树屋对白 | |

若分期：店内段结束行为是什么（回 Idle？占位黑屏？）写入开放问题。

### G. 最小施工清单（给施工员，本阶段不执行）

| # | 模块 | 动作 | 优先级 |
|---|------|------|--------|
| 1 | Head/Chest 可点组件 + 点击脚本 | | P0 |
| 2 | 两份对话 Prefab + CSV Import | | P0 |
| 3 | 店句 Face/Body 列按迁移表填写 | | P0 |
| 4 | Merchant Actor 壳（若需要） | | P0 |
| 5 | 对白期禁热区 / 可选藏 UI | | P1 |
| 6 | 胸部转场 / 树屋 | | P2 或下期 |
| 7 | 技术说明短文 | | P2 |

### H. 验收清单

| # | 操作 | 通过 |
|---|------|------|
| 1 | 进店（非首次或首次结束后）点 Head | 播点头对白，店脸随句变，雅脸随句变 |
| 2 | 点 Chest | 播点胸对白（按分期范围），表情正确 |
| 3 | 对白中再点头/胸 | **不**开第二段、无叠对话 |
| 4 | 买卖按钮：对白中行为符合报告（禁用或藏） | |
| 5 | 首次进店自动对白进行中点头胸 | 无干扰 |
| 6 | Console | 无 NRE / Missing Actor / Face 校验失败 |

### I. 开放问题（写入报告；设计不清则追加 OPEN_QUESTIONS）

- 0601 店脸 Smile/Angry → Face1～5 的正式对照是否由策划签字？  
- 点头/点胸是否可重复触发，还是各只播一次（存档旗标）？  
- 胸部线树屋场景名 / 落点是否已有资产？  
- `Trigger` 是否也要同步进 `商店界面合层.prefab` / `MerchantPainting.prefab`（场景实例 vs Prefab 源）？

---

## 输出要求

写入：`Assets/Doc/执行文档/0828/Village_Shop_MerchantPainting_Trigger特殊交互对话_架构溯源报告.md`

结构（MASTER 四段式 + 表）：

① 结论一句话（点击方案 A/B/C + 对白/表情如何复用现网 + 胸部是否分期）  
② 原因（通俗：热区现状、为何不能走旧 Smile 键、与 ShopStart 如何隔离）  
③ 用户检查清单（场景里要确认的组件、两段对白怎么验收）  
④ 给程序：迁移表 + 状态机 + 最小文件清单 + 开放问题

口头汇报同样用 MASTER 四段式。
```

---

## 施工员续跑（侦探报告拍板后再贴）

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0828/Village_Shop_MerchantPainting_Trigger特殊交互对话_架构溯源报告.md
@Assets/Doc/执行文档/6月/0601/Village_商店老板娘特殊交互_对白台本_执行说明.md
@Assets/GameRes/Scenes/Village_Shop.unity

你现在是【施工员】。只按报告实现「点 Head/Chest → 特殊对话 + 表情」最小闭环。

必须遵守：
- 两分支独立 Prefab/CSV，禁止并进 Village_ShopStart；
- 店句表情只用 Face1～5 + BodyType，复用 ShopkeeperFaceRegistry / 现网桥，禁止扩 DialogueFaceType；
- 对白中禁止叠触发；优先报告推荐的点击方案；
- 胸部转场/树屋仅当报告列入本期才做，否则停在分期边界并注释挂钩；
- 禁止 Update 堆业务；代码含详细注释；重要取舍写清原因。

提交说明：改了哪些文件、头/胸如何验收、表情如何跟句、未做项（若有）。
```
