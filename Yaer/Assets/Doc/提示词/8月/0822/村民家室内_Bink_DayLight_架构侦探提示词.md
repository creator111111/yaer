# Cursor Agent Prompt · 村民家室内改播 Bink_DayLight（白天眨眼）

> **角色**：先【架构侦探】，报告拍板后再开【施工员】  
> **日期**：2026-08-22  
> **范围**：第一章肯姆尼村 **村民家室内**，主角 Home 眨眼从现网 `Bink` 改为 **`Bink_DayLight`**（与已落地的 `Idle_DayLight` / `Walk_DayLight` 同场景白名单、同方案 B 思路）。  
> **已拍板（2026-08-22，开发者）**：**推翻 0818 Q3「屋里仍用现网 Bink」**；村民家室内眨眼改用白天素材。龙宫 `HomeScene1/2`、村街道、Combat **严禁误伤**。  
> **本阶段**：只溯源、不改代码 / Animator / Override / Prefab / 场景 / Clip  
> **已知约束**：C# Home 状态机只认状态名 **`Bink`**（不是 `Bink_DayLight`）；禁止只拉 Animator 过渡不改 C#（会 `IsName` 对不上卡死）

把下面整段复制给 Cursor Agent（Agent Mode）执行。本阶段**只溯源、不改代码**。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 产品需求（开发者原话）

> 眨眼的素材补好了，新建了白天的眨眼动画，需要替换一下。  
> 眨眼动画本质是 **3 帧循环**；Armor / Crown / None 各只有 3 帧。

已交付 Clip（侦探须核实帧图、时长、Override 接线是否齐全）：

| 换装 | 新 Clip 路径 |
|------|----------------|
| Dress（底图） | `Assets/Animation/Object/Yaer/Home/Dress/Bink_DayLight.anim` |
| None | `Assets/Animation/Object/Yaer/Home/None/ArmorNoneBink_DayLight.anim` |
| Crown | `Assets/Animation/Object/Yaer/Home/Crown/ArmorCrownBink_DayLight.anim` |
| ArmorHead | `Assets/Animation/Object/Yaer/Home/Armor/ArmorBink_DayLight.anim` |

新帧图目录：`Assets/ArtRes/Animation/Yaer/Home/Blink/`  
内含 `None1~3`、`Crown1~3`、`Armor` / `Armor2` / `Armor3`（共 9 张）。

### 验收口径（已拍板，侦探按此写范围表）

| 场景 | 期望动画 |
|------|----------|
| 第一章村外街道 `Village_KenMuNi1` | **保持现网**（眨眼仍是旧 `Bink`，若村外也走 Home） |
| `Village_HomeScene1` / `2` / `23` / `45` | **开**：待机/走路已是 DayLight 时，眨眼也播 **`Bink_DayLight`** |
| `Village_House4`、磁盘 `Village_HomeScene3` | **算村民家，同样开**（场景缺失/未进 Build 也要把名字写进开关名单） |
| 龙宫 `HomeScene1` / `HomeScene2` | **严禁改**（眨眼仍播旧 `Bink`；禁止改磁盘共用 `Bink` Clip） |
| `Village_Shop` | 排除 |
| 从屋里走回村街道 | **立刻回到**现网（非 DayLight 眨眼） |
| C# 状态名 | **仍是 `Bink`**；只换片子，不改 `RegisterState` 的 `StateName` |

### 生活类比

待机/走路已经换成「白天家居服」了，眨眼还穿着「晚上睡衣」。新白天眨眼衣服（Clip）已经放进衣柜，但 `VillageHomeDayLightAnimApplier` 进门时**故意没动 `Bink` 抽屉**（见现网注释）。要把 **`Bink` 抽屉名不变**，只在村民家屋里把抽屉里的片子临时换成 `Bink_DayLight`，出屋还原。**不能把抽屉改名叫 `Bink_DayLight`**，否则 C# `IsName("Bink")` 对不上会卡死。

### 与 0818 Idle/Walk DayLight 的关系

- 现网已施工：`VillageHomeDayLightAnimApplier.cs`（方案 B）在村民家白名单场景内，**运行时 Clone Override**，把 `Idle`/`Walk` 槽换成 `Idle_DayLight`/`Walk_DayLight` 生效片。  
- **第 87 行注释明确写「Bink 行故意不改」**——本次要补的缺口。  
- 白名单现网（侦探核实是否与 Idle/Walk 一致、有无遗漏）：`Village_HomeScene1/2/23`、`Village_House4`、`Village_HomeScene45`。  
- 上案报告：`Assets/Doc/执行文档/0818/第一章村民家室内_IdleWalk_DayLight_架构溯源报告.md`

### 现网动画双轨（`02_SYSTEM_SPEC` §4）

- **Home**：`Assets/GameRes/RuntimeController/Entity/Player/Home/`  
  底图 `Home_Dress_Crown.controller`；换装 Override：`Home_Armor_Crown` / `Home_Armor_ArmorHead` / `Home_Armor_NoHeadWear`。  
  Bool：`Walk`（无 `Run`）。C#：`PlayerHomeCsRuntimeController` → `PlayerHomeSM`。
- 运行时换控制器：`PlayerLogic.UpdateRuntimeController` → 若 `loaded.name.Contains("Home")` 则走 `VillageHomeDayLightAnimApplier.ApplyIfVillageHome`。

### 现网 C# 眨眼链路（侦探须画清）

| 时机 | 代码入口 |
|------|----------|
| 进 Idle 子状态机 | `HomeIdleSubSM.Enter` → `ChangeState<HomeBinkState>()` |
| 走路停下 | `HomeWalkState.Update` → `ChangeState<HomeBinkState>()` |
| 播完回待机 | `HomeBinkState.Update` → `IsFinished` → `ChangeState<HomeIdleState>()` |

C# 注册：`RegisterState<HomeBinkState>("IdleSubState_Bink", "Bink")`  
Animator：`Home_Dress_Crown.controller` → `IdleSubState` 子机 → **`Bink`** 状态，Motion 底图为 `Bink.anim`。

### 现网 vs 新片（预扫疑点，侦探必须核实）

| 疑点 | 预扫现象 | 侦探要结论 |
|------|----------|------------|
| Clip 时长 | 旧 `Bink` 约 **3s**，0s / 2s 各眨一次；新 `ArmorBink_DayLight` 约 **1.5s**、帧更密 | 改片后 `IsFinished` 节奏是否变快？要不要对齐旧时长？ |
| LoopTime | 旧 `Bink` `m_LoopTime: 0`；部分 `*_Bink_DayLight` 为 `1` | Loop 是否导致 `IsFinished` 永不触发、卡在 Bink？ |
| 帧图是否已换 | `ArmorNoneBink_DayLight` 与 `ArmorNoneBink` **GUID 相同**（疑为复制未换图）；`ArmorBink_DayLight` GUID 不同 | 逐套核对是否引用 `ArtRes/.../Blink/` 新图 |
| Dress 裙子 | `Bink_DayLight.anim` 仍引用旧 Dress 眨眼 GUID（`Dress/Idle/Bink/01~03`） | 裙子有无白天眨眼素材？无则 Dress 走哪套？ |
| Override 表 | `Home_Dress_Crown.controller` 有 `Idle_DayLight`/`Walk_DayLight` 孤岛态；**未见 `Bink_DayLight` 行** | 底图 + 三套 Override 是否都要加 `Bink`→`Bink_DayLight` 映射？ |
| Applier 扩展 | 仿 `RemapOriginalSlot(ClipIdle, idleDay)` 加 `Bink`→`Bink_DayLight` 是否足够？ | 是否复用 `FindEffectiveClip(pairs, "Bink_DayLight")`？ |

### 严禁的施工方向（预判，侦探可推翻但须写理由）

1. 新建 Animator 状态 `Bink_DayLight` 并改 C# `StateName`（除非同步改注册表，否则卡死）。  
2. 直接改磁盘 Override 的 `Bink` 槽为 DayLight（方案 E，龙宫一起变）。  
3. 用 `TerrainType.IndoorType` / `!isFightingScene` 当总开关（龙宫也是室内 Home）。  
4. 只改 `.anim` 不换 Applier（屋里可能仍走旧 Override 链）。  
5. 把 `Bink_DayLight` 拉成 Animator 孤岛态、只接线不改 C#（0818 Idle_DayLight 已证此路不通）。

### 侦探须比较的方案

| 方案 | 摘要 | 对 `IsName("Bink")` | 范围可控 | 换装 Override | 风险 |
|------|------|----------------------|----------|---------------|------|
| **B'（推荐候选）** | 扩展现有 `VillageHomeDayLightAnimApplier`：Clone 后把 `Bink` 槽换成 `Bink_DayLight` 生效片 | 状态名不变，最安全 | 同 Idle/Walk 白名单 | 需 Override 有 `Bink_DayLight` 行 | Clip 时长/Loop 影响 `IsFinished` |
| C | 复制一套「全白天」Home Controller，PathHelper 按场景加载 | 不改状态图 | 好 | 四套都要备齐 | 资源份数多 |
| A | 新 Bool + 平行态 `Bink_DayLight` | 必须改 C# 注册 | 好 | 可复用 | 与 Idle 子机衔接复杂 |
| E | 直接改磁盘 `Bink` Clip | 安全 | **差**（龙宫误伤） | 简单 | **否决** |

侦探可合并 B'/C，但必须给 **一个推荐方案** 和 **最小改动文件列表**。

---

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Project_context.md
@Assets/Doc/02_SYSTEM_SPEC.md
@Assets/Doc/OPEN_QUESTIONS.md
@Assets/Doc/执行文档/0818/第一章村民家室内_IdleWalk_DayLight_架构溯源报告.md
@Assets/Scripts/Game/GameRuntime/Entities/Player/Components/VillageHomeDayLightAnimApplier.cs
@Assets/Scripts/Game/GameRuntime/Entities/Player/Components/CsAnimator/Home/Idle/HomeBinkState.cs
@Assets/Scripts/Game/GameRuntime/Entities/Player/Components/CsAnimator/Home/Idle/HomeIdleSubSM.cs
@Assets/Scripts/Game/GameRuntime/Entities/Player/Components/CsAnimator/Home/HomeWalkState.cs
@Assets/Scripts/Game/GameRuntime/Entities/Player/PlayerLogic.cs
@Assets/GameRes/RuntimeController/Entity/Player/Home
@Assets/Animation/Object/Yaer/Home/Dress/Bink.anim
@Assets/Animation/Object/Yaer/Home/Dress/Bink_DayLight.anim
@Assets/Animation/Object/Yaer/Home/None/ArmorNoneBink.anim
@Assets/Animation/Object/Yaer/Home/None/ArmorNoneBink_DayLight.anim
@Assets/Animation/Object/Yaer/Home/Crown/ArmorCrownBink.anim
@Assets/Animation/Object/Yaer/Home/Crown/ArmorCrownBink_DayLight.anim
@Assets/Animation/Object/Yaer/Home/Armor/ArmorBink.anim
@Assets/Animation/Object/Yaer/Home/Armor/ArmorBink_DayLight.anim
@Assets/ArtRes/Animation/Yaer/Home/Blink
@Assets/GameRes/Prefabs/Entity/Player/Player.prefab

你现在是【架构侦探】。Unity 2020.3.48f1 / C#。
禁止修改任何代码、Animator Controller、Override、Prefab、场景、Clip。
只读扫描 + 写溯源报告。

---

## 背景（策划白话）

1. 村民家室内 Idle/Walk 已（或正在）走 DayLight；**眨眼也要换成白天版**。
2. 新 Clip 已建好（见上表）；素材在 `ArtRes/.../Blink/`，每套 **3 帧**（开→闭→开）。
3. **只改村民家白名单场景**；龙宫、村街道、Shop、Combat **零误伤**。
4. **状态名必须仍是 `Bink`**；只换 Motion/Override 片子，不换 C# 状态名。
5. 目标：摸清「新 Clip 是否接对帧图、Override 表缺什么、Applier 怎么扩、Clip 时长/Loop 对 `HomeBinkState.IsFinished` 的影响」，给出**最小可施工方案**。

---

## 必读 / 优先扫描线索

### A. 新 Clip 资产验收（逐文件）
- 四套 `*_Bink_DayLight.anim` 是否引用 `ArtRes/.../Blink/` 新 GUID
- `m_SampleRate`、`m_StopTime`、`m_LoopTime` 与旧 `Bink` 对比表
- 是否同时驱动 `Animation` 与 `Animation/ShadowAnimator` 两条曲线
- Dress 底图 `Bink_DayLight` 是否仍用旧帧（若是，列缺口）

### B. Override / Controller 接线
- `Home_Dress_Crown.controller`：是否有 `Bink_DayLight` 备用 Clip 槽（类比 `Idle_DayLight`）
- `Home_Armor_Crown` / `Home_Armor_ArmorHead` / `Home_Armor_NoHeadWear`：是否已有 `Bink`→`*Bink_DayLight` 映射行；缺则列
- 运行时 `VillageHomeDayLightAnimApplier.ApplyIfVillageHome` 当前只 remap Idle/Walk；扩 Bink 的伪代码是否一行对称即可

### C. 状态机与节奏
- `HomeBinkState` 依赖 `IsFinished`：新 Clip 更短/Loop 时，进屋先眨、走停再眨，节奏是否合理
- Idle↔Bink 子机过渡（`IdleSubState_Bink` / `IdleSubState_Idle`）换片后是否仍通
- 开 DayLight Idle 后，Bink 帧风格是否与 Idle_DayLight 一致（美术验收点）

### D. 范围与白名单
- 与 Idle/Walk DayLight 共用 `VillageHomeSceneNames` 是否足够
- 龙宫 `HomeScene1/2` 确认仍播旧 `Bink`
- 出屋回村 / 换装再进屋：Bink 是否正确跟铠甲 Override

### E. 历史决议更新
- `OPEN_QUESTIONS.md` 0818 Q3「屋里 Bink 用现网」→ **本次产品改口为「村民家用 Bink_DayLight」**；报告里写清 supersede，勿再按旧 Q3 施工

---

## 侦探任务清单

1. **结论一句话**：屋里仍播旧眨眼的根因（Applier 故意跳过 Bink / Override 无 Bink_DayLight 行 / Clip 未接新帧 / 其它）。
2. **新 Clip 验收表**（Dress / None / Crown / Armor）：帧图来源、时长、Loop、Shadow 曲线、与旧 Bink 差异。
3. **推荐方案**（优先扩展现有 Applier 方案 B）+ 否决项理由；必须保证：
   - C# `IsName("Bink")` 不变；
   - 仅村民家白名单启用；
   - 出屋还原；
   - 裙子 + 三套头饰 Override 正确。
4. **Clip 时长/Loop 风险**：`IsFinished`、进屋先眨、走停再眨；若 3 帧太短，是否需把 Timeline 垫到与旧 Bink 同长（侦探给建议，本阶段不改）。
5. **最小改动文件列表**（只建议，本阶段不改）：预计含 `VillageHomeDayLightAnimApplier.cs`、四套 Override（或底图 controller）、可能需修的 `*_Bink_DayLight.anim`。
6. **验收清单**（给用户）：
   - InitScene → 第一章进村 → 进 `Village_HomeScene1`（或 2/23/45）→ 站定看眨眼是否为**白天帧** → 走两步停 → 再眨一次 → 换 Crown/ArmorHead/None 各测一次
   - 出屋回村 → 眨眼不应变 DayLight
   - 进龙宫 → 仍为旧 `Bink`，未被误伤
7. **开放问题**追加 `OPEN_QUESTIONS.md` 新节「村民家室内 Bink_DayLight · 2026-08-22」（仅技术项，如 Dress 缺白天眨眼帧怎么办）。
8. **禁止**：改资产；给孤岛状态拉线当完工；把全局 `Bink` Clip 换成 DayLight；改 Combat `Run`；任何会让龙宫播 `Bink_DayLight` 的改法。

---

## 输出要求

写入：`Assets/Doc/执行文档/0822/村民家室内_Bink_DayLight_架构溯源报告.md`

报告结构固定（中文、大白话优先）：

① 结论一句话  
② 原因（生活类比：待机走路已是白天服，眨眼仍抽晚上那一格）  
③ 用户需要做什么（范围已拍板：只验收 + 认方案）  
④ 给程序看的补充：调用链、状态名契约、新 Clip 验收表、方案对比（E 已否决）、白名单、Clip 时长风险、`IsFinished` 影响、最小文件列表、仅技术开放问题

完成后用 MASTER 固定四段式口头汇报；详细以报告文件为准。
```

---

## 施工员续跑（侦探报告拍板后再开）

> 本阶段先不要贴。等侦探报告给出推荐方案后再开。

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/02_SYSTEM_SPEC.md
@Assets/Doc/执行文档/0822/村民家室内_Bink_DayLight_架构溯源报告.md
@Assets/Doc/OPEN_QUESTIONS.md
@Assets/Scripts/Game/GameRuntime/Entities/Player/Components/VillageHomeDayLightAnimApplier.cs

你现在是【施工员】。按上述溯源报告做**最小化修改**，使村民家室内播 `Bink_DayLight`，出屋恢复现网 `Bink`。

必须遵守：
- C# 与 Animator 状态名 `IsName("Bink")` 对齐，禁止只拉孤岛线；
- 范围仅限村民家白名单（与 Idle/Walk DayLight 一致）；**千万不要改龙宫** `HomeScene1/2`，也不改村街道/Combat；
- 优先扩展现有 `VillageHomeDayLightAnimApplier` 方案 B，禁止改磁盘共用 `Bink` Clip（方案 E）；
- 换装 Override（Dress / Crown / ArmorHead / None）进屋仍正确；
- 不在 Update 堆业务；不抢写 Combat 的 `Run`；
- 注意 `HomeBinkState.IsFinished` 与新 Clip 时长/Loop 的衔接。

每次提交说明：改了哪些文件、白天眨眼如何打开/关闭、如何验收进门与出门、三套头饰各测一次。
```
