# Cursor Agent Prompt · 第一章村民家室内改播 Idle_DayLight / Walk_DayLight

> **角色**：先【架构侦探】，报告拍板后再开【施工员】  
> **日期**：2026-08-18  
> **范围**：第一章肯姆尼村（`Village_KenMuNi1`）**进入村民家室内后**，主角 Home 待机/走路改为播 **`Idle_DayLight` / `Walk_DayLight`**。  
> **已拍板（2026-08-18，开发者）**：只改村民家，**千万不要改龙宫**；`Village_HomeScene1 / 2 / 23` **全部要开**；`Village_House4`、磁盘 `Village_HomeScene3` **算村民家（要开）**；屋里眨眼 **仍用现网 `Bink`**。村外街道、战斗、龙宫 `HomeScene1/2` **明确排除**。  
> **本阶段**：只溯源、不改代码 / Animator / Override / Prefab / 场景  
> **已知约束**：C# Home 状态机只认状态名 `Idle` / `Bink` / `Walk`；底图里 DayLight 节点目前是孤岛，**禁止**只接线不改 C#（会 `IsName` 对不上卡死）

把下面整段复制给 Cursor Agent（Agent Mode）执行。本阶段**只溯源、不改代码**。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 产品需求（开发者原话）

> 在第一章的村庄内，进了村民家里面就使用 `Idle_DayLight` 和 `Walk_DayLight`。  
> 这个功能需要先实现。

验收口径（**已拍板，侦探按此写范围表，勿再征求「龙宫要不要」**）：

| 场景 | 期望动画 |
|------|----------|
| 第一章村外街道 `Village_KenMuNi1` | **保持现网**（不要改成 DayLight） |
| `Village_HomeScene1` / `2` / `23` | **全部开**：待机 `Idle_DayLight`，走路 `Walk_DayLight` |
| `Village_House4`、磁盘 `Village_HomeScene3` | **算村民家，同样开**（场景缺失/未进 Build 也要把名字写进开关名单，避免以后补屋漏开） |
| 龙宫 `HomeScene1` / `HomeScene2` | **严禁改**（含禁止改共用 Idle/Walk Clip、禁止用会误伤龙宫的 Indoor 总开关） |
| `Village_Shop` | 排除（无主角走路） |
| 从屋里走回村街道 | **立刻回到**现网村外那套（非 DayLight） |
| 眨眼 `Bink` | **屋里仍用现网 Bink**，不做 `Bink_DayLight` |

### 生活类比

衣柜里本来有「晚上睡衣」（`Idle`/`Walk`）和「白天家居服」（`Idle_DayLight`/`Walk_DayLight`）。现在衣服已经做进衣柜（Clip + Override 表），但出门程序只按「睡衣」这个抽屉名取衣服。进村民家要把程序改成：进了这几栋屋，就改抽「白天家居服」那个抽屉，出屋再换回去。**不能把白天衣服另开一个没人叫的房间**（孤岛状态），否则程序找不到人。

### 现网动画双轨（`02_SYSTEM_SPEC` §4）

- **Home**：`Assets/GameRes/RuntimeController/Entity/Player/Home/`  
  底图 `Home_Dress_Crown.controller`；换装 Override：`Home_Armor_Crown` / `Home_Armor_ArmorHead` / `Home_Armor_NoHeadWear`。  
  Bool：`Walk`（无 `Run`）。C#：`PlayerHomeCsRuntimeController` → `PlayerHomeSM`。
- **Combat**：另一套控制器，Bool `Run`。村民家 Config 已是 `isFightingScene=0`，进屋应走 Home，不走 Combat。
- 运行时换控制器：`PlayerLogic` → `PlayerRuntimeControllerComponent.GetAnimatorController(..., Config.isFightingScene)` → `UpdateRuntimeController`。

### 现网 C# 只注册三个可进状态

| C# | Animator Bool / 状态名 |
|----|------------------------|
| `HomeWalkState` | args=`Walk`，`IsName("Walk")` |
| `HomeIdleState` | args=`IdleSubState_Idle`，`IsName("Idle")` |
| `HomeBinkState` | args=`IdleSubState_Bink`，`IsName("Bink")` |

入口：`PlayerHomeSM.cs`、`HomeIdleSubSM.cs`。  
同步规则：`BaseStateMachine.Update` 仅当 `AnimatorStateInfo.IsName(当前 C# StateName)` 为真才 `Enter`/`Update`。

### 现网 Animator 资产（预扫）

`Home_Dress_Crown.controller` 主层已有 **`Idle_DayLight`、`Walk_DayLight` 两个状态**，`m_Transitions` 为空，无进线。  
它们的作用更像「让 Override 列表里出现这两段 Clip」，**运行时播不到**。

Override 表（三套铠甲换装）已把 DayLight 原 Clip 映射到对应换装 Clip：

| 原 Clip（裙子底图） | None | Crown | ArmorHead |
|---------------------|------|-------|-----------|
| `Idle_DayLight` | `ArmorNoneIdle_DayLight` | `ArmorCrownIdle_DayLight` | `ArmorIdle_DayLight` |
| `Walk_DayLight` | `ArmorNoneWalk_DayLight` | `ArmorCrownWalk_DayLight` | `ArmorWalk_DayLight` |

磁盘 Clip：

- `Assets/Animation/Object/Yaer/Home/Dress/Idle_DayLight.anim` / `Walk_DayLight.anim`
- `.../Home/None|Crown|Armor/` 下对应 `*Idle_DayLight` / `*Walk_DayLight`

**硬否决**：Override 的 **`Idle`/`Walk` 槽**仍指向旧 Idle/Walk Clip。若施工只改这两槽为 DayLight，**龙宫也会变白天**。开发者已拍板「千万不要改龙宫」→ 方案 E **直接否决**，侦探不必再问。

### 第一章「村民家」范围（已拍板 + 侦探须核实交付状态）

| 场景 | 产品 | 侦探还要核实 |
|------|------|----------------|
| `Village_HomeScene1` / `2` / `23` | **全部开 DayLight** | 门、Manager、能否进屋 |
| `Village_House4` | **算，要开** | 场景文件是否缺失、能否进；缺失也要把常量写入启用名单 |
| 磁盘 `Village_HomeScene3` | **算，要开** | 有无 `SceneName` 常量、能否进 Build；未接通也要把场景名写入启用名单 |
| `Village_Shop` | **排除** | 无主角 |
| 龙宫 `HomeScene1` / `HomeScene2` | **严禁开、严禁误伤** | 推荐方案不得改共用 Clip / 不得用 `IndoorType` 一把梭（龙宫也是室内） |
| 村街道 `Village_KenMuNi1` | **不开** | 当前是 Home 还是 Combat，避免误伤街道 |

村外配置资产列表里**没有**独立 `Village_KenMuNi1.asset`（可能复用森林 Config）——必须确认街道动画身份。

### 严禁的施工方向（预判，侦探可推翻但须写理由）

1. 只在 Animator 里给 `Idle_DayLight`/`Walk_DayLight` 拉过渡、不改 C# 状态名对齐。  
2. 把全项目 Home 的 `Idle`/`Walk` Clip 直接换成 DayLight（龙宫/其它 Home 场景会一起变）。  
3. 在 `TownPlayerLocomotion` 里抢写 `Walk`/`Run`（`02_SYSTEM_SPEC` §4 已禁止 Combat 抢参）。  
4. 把村屋改成 Combat 控制器来「换一套动画」。

### 侦探须比较的方案（报告里出推荐 + 否决理由）

| 方案 | 摘要 | 对 C# `IsName` | 范围可控（仅村民家） | 换装 Override | 风险 |
|------|------|----------------|----------------------|---------------|------|
| A 新 Bool `DayLight` + 平行状态 `Idle_DayLight`/`Walk_DayLight` |  indoors SetBool；C# 状态名跟着改或双注册 | 必须改注册表 | 好（按场景 SetBool） | 已有 Clip 映射，可复用 | 状态图/Bink 衔接复杂 |
| B 室内仍进 `Idle`/`Walk`，只换 Motion | 进屋把 Animator 状态上的 Clip 换成 DayLight | 名字不变，最安全 | 取决于「何时换 Clip」 | 可能绕过 Override 表 | 要确认换装仍生效 |
| C 室内专用 Override / 复制一份 Controller | 路径 Helper 按「村民家」加载另一套，Idle/Walk 槽已是 DayLight | 不改状态图 | 好 | 要为 Dress+三套头饰都备齐 | 资源份数增加 |
| D BlendTree 在 Idle 内切 DayLight | 参数 0/1 换片子，状态名仍是 Idle | 安全 | 好 | Idle_DayLight 要进树 | 改底图，所有 Override 槽要重对 |
| E 仅改 Override 的 Idle/Walk 槽 | 最省事 | 安全 | **差**（龙宫一起变） | 直接用 | **已拍板否决**（千万不要改龙宫） |

侦探可合并 A/B/C，但必须给 **一个推荐方案** 和 **最小改动文件列表**。

---

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Project_context.md
@Assets/Doc/02_SYSTEM_SPEC.md
@Assets/Doc/OPEN_QUESTIONS.md
@Assets/Doc/技术文档/场景相关/Village_HomeScene23_可玩民居室内_技术说明.md
@Assets/Doc/技术文档/场景相关/场景切换.md
@Assets/Doc/执行文档/0807/雅儿白天待机帧图重命名_Crown_None_Armor_架构侦探执行说明.md
@Assets/GameRes/RuntimeController/Entity/Player/Home
@Assets/Scripts/Game/GameRuntime/Entities/Player/Components/CsAnimator
@Assets/Scripts/Game/GameRuntime/Entities/Player/Components/PlayerRuntimeControllerComponent.cs
@Assets/Scripts/Game/GameMgr/Manager/Res/PathHelper/PlayerControllerResPathHelper.cs
@Assets/Scripts/Game/Static/Name/Res/SceneName.cs

你现在是【架构侦探】。Unity 2020.3.48f1 / C#。
禁止修改任何代码、Animator Controller、Override、Prefab、场景、Clip。
只读扫描 + 写溯源报告。

---

## 背景（策划白话）

1. 第一章进肯姆尼村后，**走进村民家里**，主角的待机和走路要换成白天那两段：`Idle_DayLight`、`Walk_DayLight`。屋里眨眼 **仍用现在的 `Bink`**。
2. **只改村民家，千万不要改龙宫** `HomeScene1` / `HomeScene2`（共用 Idle/Walk 资源也不许动）。
3. 要开的屋：`Village_HomeScene1`、`Village_HomeScene2`、`Village_HomeScene23` **全部**；`Village_House4`、磁盘 `Village_HomeScene3` **也算，同样开**。村外街道保持现网。
4. 动画片子和 Override 表里很多已经填过了，但进屋里还是播旧 Idle/Walk。不要当成「缺资源」，先当「没人通知动画机该切白天」。
5. 目标：摸清「进屋用哪套 Home 控制器、状态名怎么对齐、怎么按场景名单打开白天、怎么出屋还原且龙宫零误伤」；给出**最小可施工方案**。范围已拍板，侦探**不要再问龙宫/Bink/这几栋屋要不要开**。

---

## 必读 / 优先扫描线索

### A. 进屋后当前到底在播什么
- 村民家 Config `isFightingScene` → 加载的是 Home 还是 Combat
- `PlayerRuntimeControllerComponent` / `PlayerControllerResPathHelper.GetPath()` 实际路径（Dress.controller vs Armor_*.overrideController）
- 运行时 Animator 当前状态名：是否一直是 `Idle`/`Bink`/`Walk`
- `Idle_DayLight`/`Walk_DayLight` 是否从未被 Play

### B. 状态机契约（必核对，防止卡死）
- `PlayerHomeSM` / `HomeIdleSubSM` 的 `RegisterState` 参数
- `BaseStateMachine` 的 `IsName` 门闩
- Home 过渡：`Null` → `Walk` 或 `IdleSubState`；Idle 子机 `Idle`/`Bink`
- DayLight 两节点：无进线、无出线、无 Bool 参数

### C. 资源是否齐（换装三态 + 裙子）
- Dress 底图 Clip 是否就是白天源
- None / Crown / ArmorHead Override 的 DayLight 行是否都已填、有无 Missing
- `Home_Armor_NoHeadWear` 文件名 vs `RuntimeControllerName` / PathHelper 拼出来的 `Home_Armor_None` 是否对得上（预扫有命名差，必须核实，否则无头饰换装会加载失败）
- 确认无 `Bink_DayLight` 也不做；屋里眨眼继续走现网 `Bink` 状态/Clip

### D. 「村民家」范围表（产品已拍板，侦探填交付状态）
开 DayLight：`Village_HomeScene1/2/23`（全部）+ `Village_House4` + 磁盘 `Village_HomeScene3`。  
不开：龙宫 `HomeScene1/2`（严禁误伤）、村街道、Shop、Combat。  
表里写清：现网能否进屋、有无 SceneName/Build、白名单应写的字符串。

### E. 切场时机
- `InitPlayer` / `UpdateRuntimeController` 只在创建玩家时调一次，还是每次 LoadScene 都会换控制器
- 村→屋、屋→村 是否会重建玩家；若不重建，只 SetBool 够不够，要不要在 `OnEnterScene` 补一次
- `TerrainType.IndoorType` **不能**当 DayLight 总开关（龙宫也是室内脚步）。必须用**村民家场景名白名单**（1/2/23 + House4 + HomeScene3），严禁 `isFightingScene==false` 一把梭

---

## 侦探任务清单

1. **结论一句话**：屋里播不出 DayLight 的根因（孤岛状态 / 只 Override 了备用槽 / 没有场景开关 / 其它）。
2. **范围表**（产品已拍板，侦探只填「现网能否进屋 / 缺什么」）：
   - **开**：`Village_HomeScene1`、`2`、`23`、`Village_House4`、磁盘 `Village_HomeScene3`
   - **不开**：龙宫 `HomeScene1/2`、村街道 `Village_KenMuNi1`、`Village_Shop`、Combat
3. **推荐方案**（A/B/C/D 或组合；**E 已否决**）+ 否决项理由；必须保证：
   - C# `IsName` 对得上（Idle/Walk/Bink 名字策略：白天只换 Idle/Walk 的片子或平行态，**Bink 状态名与 Clip 都不改**）；
   - **仅白名单村民家**启用，龙宫零误伤；
   - 出屋还原；
   - 裙子 + 三套头饰换装仍正确。
4. **Bink**：已拍板维持现网眨眼；报告里只需确认屋里 Idle↔Bink 衔接在开 DayLight 后仍通。
5. **最小改动文件列表**（只建议，本阶段不改）。House4 / HomeScene3 即使当前进不去，开关名单也要预留场景名。
6. **验收清单**（给用户）：InitScene → 第一章进村 → 进 HomeScene1 与 2（或 23）→ 站着白天待机、走两步白天走路、眨眼仍是旧 Bink → 出门回村动画恢复 → **再进龙宫确认仍是旧 Idle/Walk、未被改成白天** → 换头饰再进一户村屋。
7. **开放问题**追加 `OPEN_QUESTIONS.md` 新节「村民家室内 DayLight 动画 · 2026-08-18」。  
   **Q1～Q3 已决议，只记结论不要再问**：Q1 龙宫否；Q2 House4+HomeScene3 算要开；Q3 屋里 Bink 用现网。  
   侦探只追加**技术**开放项（例如 HomeScene3 无 SceneName 常量怎么进白名单、House4 场景缺失时名单仍写谁）。
8. **禁止**：改资产；给孤岛状态拉线当完工；把 Idle/Walk 全局换成 DayLight；改 Combat `Run`；任何会让龙宫播 DayLight 的改法。

---

## 输出要求

写入：`Assets/Doc/执行文档/0818/第一章村民家室内_IdleWalk_DayLight_架构溯源报告.md`

报告结构固定（中文、大白话优先）：

① 结论一句话  
② 原因（生活类比：衣柜有白天衣服，程序进村屋仍抽晚上那一格）  
③ 用户需要做什么（范围已拍板：只验收 + 认方案）  
④ 给程序看的补充：调用链、状态名契约、方案对比（E 已否决）、白名单、最小文件列表、仅技术开放问题

完成后用 MASTER 固定四段式口头汇报；详细以报告文件为准。
```

---

## 施工员续跑（侦探报告拍板后再开）

> 本阶段先不要贴。等侦探报告给出推荐方案（范围/Bink/龙宫已拍板，不必再等产品）。

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/02_SYSTEM_SPEC.md
@Assets/Doc/执行文档/0818/第一章村民家室内_IdleWalk_DayLight_架构溯源报告.md
@Assets/Doc/OPEN_QUESTIONS.md

你现在是【施工员】。按上述溯源报告做**最小化修改**，使第一章肯姆尼村民家室内播 Idle_DayLight / Walk_DayLight，出屋恢复现网动画。

必须遵守：
- C# 与 Animator 状态名 `IsName` 对齐，禁止只拉孤岛线；
- 范围仅限村民家白名单：`Village_HomeScene1/2/23`、`Village_House4`、`Village_HomeScene3`；**千万不要改龙宫** `HomeScene1/2`，也不改村街道/Combat；
- 屋里眨眼继续现网 `Bink`，只换 Idle/Walk 为 DayLight；
- 换装 Override（Dress / Crown / ArmorHead / None）进屋仍正确；
- 不在 Update 堆业务；不抢写 Combat 的 Run。

每次提交说明：改了哪些文件、白天如何打开/关闭、如何验收进门与出门。
```
