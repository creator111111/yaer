# 第一章村民家室内改播 Idle_DayLight / Walk_DayLight — 架构溯源报告

**文档版本**：v1.0（2026-08-18）  
**文档性质**：【架构侦探】只读溯源；**本文件不施工**（不改代码 / Animator / Override / Prefab / 场景 / Clip）  
**Unity**：2020.3.48f1 / C#  
**范围**：肯姆尼村 **村民家室内** 待机/走路切白天；龙宫、村街道、商店、Combat **排除**  
**已拍板（开发者 2026-08-18）**：只改村民家、千万不要改龙宫；`Village_HomeScene1/2/23` 全部开；`Village_House4` 与磁盘 `Village_HomeScene3` 算村民家要开；屋里眨眼仍用现网 `Bink`

关联：

- 提示词：`Assets/Doc/提示词/0818/第一章村民家室内_IdleWalk_DayLight_架构侦探提示词.md`
- 规范：`Assets/Doc/02_SYSTEM_SPEC.md` §4 Home/Combat 双轨
- 切场：`Assets/Doc/技术文档/场景相关/场景切换.md`
- 民居样板：`Assets/Doc/技术文档/场景相关/Village_HomeScene23_可玩民居室内_技术说明.md`
- 白天帧图另案（已施工改名，未接线）：`Assets/Doc/执行文档/0807/雅儿白天待机帧图重命名_Crown_None_Armor_架构侦探执行说明.md`

---

## ① 结论一句话

**屋里播不出白天动画，不是缺片子，是程序进村民家后仍然走进名叫 `Idle` / `Walk` 的那两格；底图上的 `Idle_DayLight` / `Walk_DayLight` 是孤岛（无进线、C# 也不认这两个名字）。白天 Clip 和三套铠甲 Override 行都已经填好了。推荐方案 B：进屋只把 `Idle`/`Walk` 格子里的片子临时换成白天那两段，状态名不动，眨眼继续 `Bink`；用村民家场景名白名单开关，龙宫零误伤。方案 E（改共用 Idle/Walk 槽）已否决。**

---

## ② 原因（生活类比）

衣柜里已经挂了「晚上睡衣」（`Idle`/`Walk`）和「白天家居服」（`Idle_DayLight`/`Walk_DayLight`）。换装三套铠甲也各自做了白天衣服（Override 表里多了两行）。

但进村民家时，出门程序只认三个抽屉名：`Idle`、`Walk`、`Bink`。白天衣服被挂在旁边一间没门的空房间里——动画机永远走不到，C# 的 `IsName` 也对不上那两个新名字。

所以：

- **不是** 缺 `Idle_DayLight.anim`；
- **不是** Override 漏填白天行；
- **是** 「没人通知动画机：进了这几栋屋，改抽白天那一格」；
- 若只给孤岛拉线、不改 C# 状态名 → `BaseStateMachine` 会因 `IsName` 对不上卡死。

---

## ③ 用户需要做什么

范围 / 龙宫 / Bink / 这几栋屋 **已拍板，不必再选**。本阶段用户只需：

1. **认方案**：推荐 **B**（进屋运行时只换 Idle/Walk 片子；Bink 不动；出屋因玩家重建自动恢复）。  
2. **按清单验收**（施工后；本阶段先不当作已修好）。  
3. House4 进不去、HomeScene3 未接通 **另案补屋**，但白名单里 **先写上场景名**，避免以后补屋漏开。

### 验收清单（施工后）

| # | 操作 | 期望 |
|---|------|------|
| 1 | InitScene → 第一章进村 `Village_KenMuNi1` | 街道保持现网（Combat 跑/站，**不是** DayLight） |
| 2 | 进 `Village_HomeScene1` | 站着白天待机、走两步白天走路 |
| 3 | 同屋站着等眨眼 | **仍是现网 `Bink`**，不是白天 Idle 硬切、也没有新的 Bink_DayLight |
| 4 | 进 `Village_HomeScene2`（或 23） | 同上：白天 Idle/Walk + 旧 Bink |
| 5 | 出门回村街道 | **立刻**回到村外现网那套（非 DayLight） |
| 6 | 再进龙宫 `HomeScene1` / `HomeScene2` | **仍是旧 Idle/Walk**，未被改成白天 |
| 7 | 换头饰（Crown / ArmorHead / 无头饰）再进一户村屋 | 白天片子跟换装走，Bink 仍是该头饰的旧眨眼 |
| 8 | Console | 无「状态切换失败: Idle/Walk/Bink」；无把龙宫当村民家的误伤日志 |

House4 / HomeScene3：**当前进不去不挡本期验收**；将来能进时自动走同一白名单即可。

---

## ④ 给程序看的补充

### 4.1 调用链：进屋后到底在播什么

```
门 SceneChangeDoor.NextSceneName
  → LoadSceneComponentGSM.LoadScene
  → 旧场景 PlayerHandlerComponentGSM.OnShutdown → HideEntity（玩家销毁）
  → 新场景 BaseGameSceneManager.OnInit → InitPlayer → CreatePlayer
  → PlayerLogic 初始化
       GetAnimatorController(..., Config.isFightingScene)
         PlayerControllerResPathHelper.GetPath()
           isFightingScene==false → Home_{衣服}_{头饰}.controller / .overrideController
           isFightingScene==true  → Combat_...
       UpdateRuntimeController
         资源名含 "Home" → PlayerHomeCsRuntimeController → PlayerHomeSM
         否则 → Combat
```

**切场会重建玩家**，不是 DontDestroy 带着旧 Animator 过门。因此：

- 进屋开关只要挂在 **本次** `UpdateRuntimeController` 即可；
- 出屋不必「关 Bool / 还原片子」——村街道会重新 Load Combat，龙宫会重新 Load 未改过的 Home 资产；
- **禁止**改磁盘上的 Override 资产本体（会污染之后加载的龙宫）。

村民家 Config 全部 `isFightingScene=0`，进屋走 **Home**，不走 Combat。与 `02_SYSTEM_SPEC` §4 一致。

### 4.2 状态名契约（硬门闩）

| C# | Register 参数 args / StateName | Animator Bool | Animator 状态 |
|----|--------------------------------|---------------|---------------|
| `HomeWalkState` | `"Walk"` / `"Walk"` | `Walk` | `Walk` |
| `HomeIdleState` | `"IdleSubState_Idle"` / `"Idle"` | `IdleSubState_Idle` | `Idle`（在子机 `IdleSubState` 内） |
| `HomeBinkState` | `"IdleSubState_Bink"` / `"Bink"` | `IdleSubState_Bink` | `Bink` |

入口：

- `PlayerHomeSM`：`Walk` + 子机 `IdleSubState`；`Enter` 默认进 Idle 子机。
- `HomeIdleSubSM.Enter`：**先** `ChangeState<HomeBinkState>()`（进屋先眨眼再待机，现网如此）。
- `BaseState.SetAnimatorEnter`：`SetBool(argsName, true)`。
- `BaseStateMachine.Update`：**仅当** `AnimatorStateInfo.IsName(当前 C# StateName)` 为真才 `Enter`/`Update`；对不上 1 秒后 Warning「状态切换失败」。

底图 `Home_Dress_Crown.controller`：

- 参数只有：`Walk`、`IdleSubState`、`IdleSubState_Idle`、`IdleSubState_Bink`。**没有** `DayLight`。
- 主层默认态 `Null` → `Walk` 或进 `IdleSubState`。
- `Idle_DayLight`、`Walk_DayLight` 在 **Base Layer**，`m_Transitions: []`，无进线、无出线、无参数。作用就是让 Override 列表出现这两段 Clip。
- **无** `Bink_DayLight`。屋里眨眼继续现网 `Bink` 状态 + Clip。

**硬否决**：只给孤岛拉过渡、不改 C# `StateName`。施工若走平行态（方案 A），必须同时改注册表，且要把白天待机放进 `IdleSubState` 才能跟 Bink 衔接——现网白天节点不在子机里，直接拉线会拆眨眼。

### 4.3 资源是否齐（换装三态 + 裙子）

底图 Motion（裙子 Dress）：

| 状态 | Clip |
|------|------|
| `Idle` | `Assets/Animation/Object/Yaer/Home/Dress/Idle.anim` |
| `Walk` | `.../Dress/Walk.anim` |
| `Bink` | `.../Dress/Bink.anim` |
| `Idle_DayLight`（孤岛） | `.../Dress/Idle_DayLight.anim` |
| `Walk_DayLight`（孤岛） | `.../Dress/Walk_DayLight.anim` |

三套铠甲 Override（均基于 `Home_Dress_Crown` guid `04b2f348…`），白天行 **已填、无 Missing**：

| 原 Clip（裙子） | None（`Home_Armor_NoHeadWear`） | Crown | ArmorHead |
|-----------------|----------------------------------|-------|-----------|
| `Idle` | `ArmorNoneIdle` | `ArmorCrownIdle` | `ArmorIdle` |
| `Walk` | `ArmorNoneWalk` | `ArmorCrownWalk` | `ArmorWalk` |
| `Bink` | `ArmorNoneBink` | `ArmorCrownBink` | `ArmorBink` |
| `Idle_DayLight` | `ArmorNoneIdle_DayLight` | `ArmorCrownIdle_DayLight` | `ArmorIdle_DayLight` |
| `Walk_DayLight` | `ArmorNoneWalk_DayLight` | `ArmorCrownWalk_DayLight` | `ArmorWalk_DayLight` |

PathHelper 实际路径：

- Dress：`Home_Dress_Crown.controller`
- 铠甲：`Home_Armor_{Crown|ArmorHead|NoHeadWear}.overrideController`

**预扫「NoHeadWear vs None」核实**：磁盘文件是 `Home_Armor_NoHeadWear`；`ClothesName.HeadWear.NoHeadWear = "NoHeadWear"`；PathHelper 拼出来 **对得上**，无头饰换装 **不会**因文件名加载失败。  
`RuntimeControllerName.HomeControllers` 里的 `"Home_Armor_None"` 是死字符串（全工程无引用），**不是**运行时加载路径。本期不必改它。

### 4.4 「村民家」范围表（产品已拍板，本表只填交付状态）

白名单判断必须用 **Unity 场景文件名** `SceneManager.GetActiveScene().name`，禁止用 `TerrainType.IndoorType`、禁止 `isFightingScene==false`（龙宫也是室内 + Home）。

| 场景名（白名单应写的字符串） | 产品 | 现网能否进屋 | SceneName 常量 | Config / Manager | Build | 备注 |
|------------------------------|------|--------------|----------------|------------------|-------|------|
| `Village_HomeScene1` | **开** | **能**（`House_Npc1`） | 有 | 专用 Manager + `isFightingScene=0` | 已登记 | 右门回村 |
| `Village_HomeScene2` | **开** | **能**（`House_NPC2`） | 有 | 同上 | 已登记 | 样板民居 |
| `Village_HomeScene23` | **开** | **能**（`House_Npc4`） | 有 | 同上 | 已登记 | 曾用名 HomeScene4 |
| `Village_House4` | **开** | **不能**（`.unity` **磁盘缺失**；村内至少 5 扇门仍写此名） | **有** `SceneName.Village_House4` | Manager/Config 在，Config `isFightingScene=0` | Build **仍登记**断链 | **名单仍写 `Village_House4`** |
| `Village_HomeScene3` | **开** | **不能**（无门、未进 Build） | **无常量** | 误挂 **龙宫** `HomeScene1Manager` + `HomeScene1.asset` | **未登记** | **名单仍写 `Village_HomeScene3`**；施工建议顺手加常量，白名单用场景文件名不要用 `nowSceneName`（否则会被当成龙宫） |
| `Village_KenMuNi1` | **不开** | — | 有 | 复用 **`ForestScene.asset`，`isFightingScene=1`** | 已登记 | **街道身份 = Combat（Run）**，不是 Home Idle/Walk |
| `Village_Shop` | **排除** | 能进店 | 有 | `canCreatePlayer=0` | 已登记 | 无主角走路 |
| `HomeScene1` / `HomeScene2` | **严禁开** | 能进龙宫 | 有 | `isFightingScene=0`，亦 `IndoorType` | 已登记 | 共用同一套 Home Idle/Walk 资产 |
| Combat 场景 | **不开** | — | — | `isFightingScene=1` | — | 禁止改 `Run`、禁止抢写 |

村街道补充：没有独立 `Village_KenMuNi1.asset`，SceneManager 绑的是森林 Config。出屋「恢复现网」= 重新创建玩家并加载 **Combat**，不是把 Home 从 DayLight 拨回晚上睡衣。因此方案 B **不会**把白天动画带出村。

村 `Map/LeftDoor` 仍写 `NextSceneName=HomeScene1` 且物体 **未激活**——那是去龙宫的残门，**不要**当成村民家。

### 4.5 方案对比

| 方案 | 摘要 | `IsName` | 仅村民家 | 换装 | 龙宫 | 结论 |
|------|------|----------|----------|------|------|------|
| **B（推荐）** | 进屋 Clone 一份 Override，把 `Idle`/`Walk` 原 Clip 映射到 **已经 Override 好的** DayLight Clip；Bink 行不动 | 名字仍是 Idle/Walk/Bink，**最安全** | 白名单 + 仅本次运行时实例 | 复用现成 DayLight 行 | 不改资产、不改图 | **推荐** |
| A | 新 Bool `DayLight` + 真正播平行态 | 必须改注册或动态改 StateName | 可按场景 SetBool | 可复用 | 图改了但默认关可保龙宫 | **否决作主方案**：白天节点在主层不在 Idle 子机，Bink 衔接要重接；改底图所有 Home 场景共图 |
| C | 复制 Dress+三套 Override，Idle/Walk 槽直接填白天；PathHelper 按白名单换路径 | 不改状态图 | 好 | 要备齐 4 份 | 旧资产不动 | **B 的退路**：资源份数增加，换装新增还要双份维护 |
| D | Idle/Walk 改 BlendTree，参数 0/1 切片子 | 名字仍 Idle | 可 | Idle_DayLight 进树后 Override 仍有效 | 改共用底图，默认 0 理论上龙宫不变 | **否决作主方案**：动共用 Controller，回归面比 B 大 |
| **E** | 只改 Override 的 Idle/Walk 槽为白天 | 安全 | **差** | 直接用 | **龙宫一起变** | **已拍板否决** |

**严禁（核实后维持）**：

1. 只拉孤岛线不改 C#；  
2. 全局把 Idle/Walk Clip 换成 DayLight（=E）；  
3. 在 `TownPlayerLocomotion` 里抢写 `Walk`/`Run`；  
4. 把村屋改成 Combat 来「换一套动画」；  
5. 用 `IndoorType` 或 `!isFightingScene` 当白天总开关。

B 与 C 可合并：先 B；若 Clone Override 在 Dress `.controller` 上取 Clip 失败，再落 C。

### 4.6 推荐方案 B — 施工要点（最小改动）

**做法**：在 `PlayerLogic.UpdateRuntimeController` 拿到已加载的 Home 控制器之后、`ChangeRuntimeController` 之前：

1. 若当前 **Unity 场景名** 不在村民家白名单 → 原样使用，**什么都不改**。  
2. 若在白名单：`var runtime = new AnimatorOverrideController(loaded);`（**必须 new**，禁止改 `loaded` 资产）。  
3. `GetOverrides` 找到原 Clip 名 `Idle` / `Idle_DayLight` / `Walk` / `Walk_DayLight`（裙子底图就是这些名字）。  
4. 把 `Idle` 槽写成当前生效的 `Idle_DayLight` 片子（铠甲 Override 里已经是 `Armor*Idle_DayLight`）；`Walk` 同理。  
5. **不要动 `Bink` 行。**  
6. 用这份 runtime 再交给 Home 状态机。

**坑（必须写进施工）**：`UpdateRuntimeController` 用 `controllerAsset.name.Contains("Home")` 决定 Home/Combat 状态机。Clone 出来的 Override **默认名字不含 Home**，若先 Clone 再判断，会误走 Combat → 卡死。正确顺序：**先按原资源名选 Home SM，再 Clone 换片子**；或 Clone 后把 `runtime.name = loaded.name`。

白名单常量（即使进不去也要写）：

```csharp
Village_HomeScene1
Village_HomeScene2
Village_HomeScene23
Village_House4
Village_HomeScene3   // 建议同时在 SceneName.cs 补 const，避免魔法字符串
```

Bink：Idle↔Bink 仍走现网子机过渡（`IdleSubState_Idle` / `IdleSubState_Bink`）。只换 Idle 的 Motion，眨眼状态名和 Clip 都不变，衔接保持通。

换装：不要手写 Armor 路径去 Load 白天 Clip；从 **当前 Override 表里已经映射好的 Idle_DayLight 行** 取片子，裙子/三头饰自动正确。

### 4.7 最小改动文件列表（只建议，本阶段不改）

| 文件 | 动作 |
|------|------|
| `Assets/Scripts/Game/Static/Name/Res/SceneName.cs` | 新增 `Village_HomeScene3` 常量（供白名单；不表示本期接通该屋） |
| **新建** `…/Player/Components/` 下小 Helper（如 `VillageHomeDayLightAnimApplier.cs`） | 白名单 + Clone Override + 只换 Idle/Walk；注释写清为何不用 IndoorType、为何必须 new |
| `PlayerLogic.cs` 的 `UpdateRuntimeController` | 调 Helper；**先判 Home 再换片子** |

**不改**：`Home_Dress_Crown.controller`、三套 `.overrideController`、任何 Clip、龙宫场景/Manager、村街道 Config、Combat、`TownPlayerLocomotion`、Bink。

House4 / HomeScene3 **不在本期修进屋**；只预留名字。

### 4.8 仅技术开放问题

产品 Q1 龙宫否 / Q2 House4+HomeScene3 要开 / Q3 屋里 Bink 用现网 → **已决议，见 OPEN 新节结论行**。

施工前只需确认下面技术项（建议默认已写在 OPEN）：

| ID | 问题 | 建议默认 |
|----|------|----------|
| T1 | `Village_HomeScene3` 无常量，白名单写字面量还是先加 `SceneName`？ | **先加常量再引用** |
| T2 | House4 场景缺失时名单写谁？ | **`Village_House4`**（与门上 Next、现有常量一致） |
| T3 | B 的 Clone Override 若验收失败？ | **改走方案 C**（复制 4 份控制器），仍禁止 E |

---

## 修订记录

| 日期 | 说明 |
|------|------|
| 2026-08-18 | v1.0 侦探溯源：根因=孤岛+无场景开关；推荐 B；E 否决；范围表填交付状态 |
