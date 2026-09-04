# Cursor Agent Prompt · 村长家自由移动：主角改为光亮形态（对齐村民家 DayLight）

> **角色**：先【架构侦探】短证白名单/控制器，再【施工员】最小扩名单  
> **日期**：2026-09-02  
> **场景**：`Village_Chief_House`（自由移动阶段）  
> **现象（用户）**：村长家里面主角动画不对，**不是光亮形态**  
> **产品期望（钉死）**：自由移动时主角动画与**其它 NPC 家里**一致 → **光亮形态**（现网即 Home 的 **`Idle_DayLight` / `Walk_DayLight` / `Bink_DayLight`** 换片，状态名仍 Idle/Walk/Bink）  
> **对标真理源**：`Village_HomeScene1` / `2` / `23` / `45` 等已开 DayLight 的村民家  
> **不是**：改成 Combat 战斗跑；不是改龙宫；不是改村街道；不是推翻 Home 状态机改状态名；不是本期「续聊战斗待机涂层」那条演出（对话中藏主角另案）  
> **报告落盘**：`Assets/Doc/执行文档/0902/Village_Chief_House_自由移动光亮DayLight_架构溯源报告.md`  
> **施工落盘**：`Assets/Doc/施工说明/0902/Village_Chief_House_自由移动光亮DayLight_施工说明.md`

把「侦探」段先复制给 Agent；拍板后用文末「施工」段。

---

## 提示词助手预梳理（须再证，勿当唯一真相）

### 产品语义

| 说法 | 现网技术对应 |
|------|----------------|
| 光亮形态 | `VillageHomeDayLightAnimApplier` 把 Home 的 Idle/Walk/Bink **槽片子**换成 `*_DayLight` |
| 其它 NPC 家里 | 白名单内：`Village_HomeScene1/2/23/45`、`Village_House4` 等 |
| 自由移动 | 进屋可走、**非**续聊藏主角/战斗涂层演出时段 |
| 不对 | 仍播暗版/默认 `Idle`/`Walk`（未换白天片），或误走 Combat |

### 现网机制（助手预扫 · 高度可疑主因）

```
PlayerLogic.UpdateRuntimeController
  → 按资源名分流 Home / Combat
  → VillageHomeDayLightAnimApplier.ApplyIfVillageHome(controller)
       → 仅当 SceneManager.GetActiveScene().name ∈ 白名单
       → 运行时 Override：Idle/Walk/Bink ← Idle_DayLight/Walk_DayLight/Bink_DayLight
```

白名单（磁盘现网，**无**村长家）：

```csharp
Village_HomeScene1, Village_HomeScene2, Village_HomeScene23,
Village_House4, Village_HomeScene45
// ❌ 未见 SceneName.Village_Chief_House
```

| 项 | 村长家预扫 | 村民家 |
|----|------------|--------|
| `isFightingScene` | `0`（Home） | `0` |
| DayLight 白名单 | **未包含** → 保持暗版 Idle/Walk | ✅ 含 → 光亮 |
| 状态机 | 应仍 Home（Walk） | 同 |

**H1（主因倾向）**：`Village_Chief_House` 未进 `VillageHomeSceneNames` → Applier 静默原样返回 → 自由移动不是光亮形态。

### 其它假说（须并列证伪）

| ID | 假说 | 证伪 |
|----|------|------|
| **H1** | 白名单缺 `Village_Chief_House` | 读 Applier；进房 Console 无 `[VillageHomeDayLight] 已换白天` |
| **H2** | 进房误挂 Combat 控制器 | `isFightingScene`、控制器名含 Combat、Bool 为 Run |
| **H3** | 白名单有了但缺白天 Clip / Override 行 → Warning 回退暗版 | Console `[VillageHomeDayLight] 找不到白天 Clip` |
| **H4** | activeScene.name 与常量不一致（拼写/未加载真场景） | `GetActiveScene().name` vs `SceneName.Village_Chief_House` |
| **H5** | 用户把「续聊里战斗待机涂层」当成自由移动形态 | 区分：可走时 Home 是否 DayLight |

### 方案倾向（施工默认）

| 方案 | 做法 | 倾向 |
|------|------|------|
| **F1 · 白名单加 `Village_Chief_House`** | `VillageHomeDayLightAnimApplier.VillageHomeSceneNames` 增加常量；注释写「村长家算村民家光亮」 | ✅ **主修**（对齐 0818/0822 机制，零新架构） |
| F2 · Chief GSM 里手写换片 | 重复逻辑 | ❌ |
| F3 · 改磁盘 Home 共用 Clip | 误伤龙宫 | ❌（0818 已否 E） |
| F4 · 村长家改 `isFightingScene` | 与光亮 Home 相反 | ❌ |

若 F1 后仍暗：再查 H3 Override / Clip，**不要**先改状态名。

### 与相关案边界

| 案 | 关系 |
|----|------|
| 0818 Idle/Walk DayLight、0822 Bink_DayLight | **同一套 Applier**；本期只扩场景名 |
| 0901 村长家 2.5D / 降速 | 移动手感另案；**本期只动画片子** |
| 0902 续聊战斗待机显隐 | **对话演出**；自由移动恢复后须仍是 **Home 光亮**，勿留 Combat 控制器 |
| 龙宫 HomeScene1/2 | **禁止**进白名单、禁止改共用槽 |

### 本期边界

| 做 | 不做 |
|----|------|
| ✅ 证 H1～H5；默认 F1 加白名单 | ❌ 新写第二套换片系统 |
| ✅ 自由移动 Idle/Walk/Bink 光亮对齐村民家 | ❌ 改龙宫 / 村街 / Combat |
| ✅ 出村长家回村街恢复非 DayLight | ❌ 改状态机注册名为 Idle_DayLight |
| ✅ 与续聊藏主角案解耦说明 | ❌ 用战斗待机涂层冒充自由移动光亮 |

### 严禁

- 用 `TerrainType.Indoor` / `!isFightingScene` 总开关（龙宫误伤）  
- 改磁盘共用 Idle/Walk 为白天（方案 E）  
- C# `IsName("Idle_DayLight")` 改状态名  
- 为光亮把村长家改成战斗场景  

### 对照文档 / 代码

- `Assets/Scripts/Game/GameRuntime/Entities/Player/Components/VillageHomeDayLightAnimApplier.cs`  
- `PlayerLogic.UpdateRuntimeController`（先 Home/Combat 分流再 Apply）  
- `02_SYSTEM_SPEC.md` §4  
- `执行文档/8月/0818/第一章村民家室内_IdleWalk_DayLight_架构溯源报告.md`  
- `执行文档/8月/0822/村民家室内_Bink_DayLight_架构溯源报告.md`  
- Config：`Village_Chief_House.asset` vs `Village_HomeScene23.asset`  
- 对照 Play：`Village_HomeScene23` vs `Village_Chief_House`  

---

## 侦探 Prompt（复制给 Agent · 先跑）

```text
你是【架构侦探】。Unity 2020.3.48f1 / C#。
禁止修改代码/Animator/场景。只读扫描 + 写溯源报告。
默认中文。沟通：①结论 ②原因 ③用户检查清单 ④程序补充。

@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Project_context.md
@Assets/Doc/02_SYSTEM_SPEC.md
@Assets/Doc/提示词/0902/Village_Chief_House_自由移动光亮DayLight_架构侦探提示词.md
@Assets/Doc/OPEN_QUESTIONS.md
@Assets/Doc/执行文档/8月/0818/第一章村民家室内_IdleWalk_DayLight_架构溯源报告.md

## 产品
村长家自由移动时主角须为光亮形态，与其它 NPC 家里一致（DayLight）。
不是 Combat，不是龙宫。

## 必读
@Assets/Scripts/Game/GameRuntime/Entities/Player/Components/VillageHomeDayLightAnimApplier.cs
@Assets/Scripts/Game/GameRuntime/Entities/Player/PlayerLogic.cs
@Assets/GameRes/Config/SceneManagerConfig/Village_Chief_House.asset
@Assets/GameRes/Config/SceneManagerConfig/Village_HomeScene23.asset
@Assets/Scripts/Game/Static/Name/Res/SceneName.cs
检索：VillageHomeSceneNames、ApplyIfVillageHome、Idle_DayLight、Village_Chief_House、isFightingScene。

## 任务
1. 钉死村长家现网播的是暗版 Home / Combat / 其它。
2. 按 H1～H5 证伪；确认白名单是否缺 Chief。
3. 对比进 HomeScene23 与 Chief_House 的 UpdateRuntimeController 日志期望。
4. 推荐 F1；写清与续聊「战斗待机」案的边界（自由移动恢复后须 DayLight Home）。
5. 更新 OPEN：村长家算不算村民家光亮白名单 → 产品本条已要求算。

## 报告
Assets/Doc/执行文档/0902/Village_Chief_House_自由移动光亮DayLight_架构溯源报告.md
```

---

## 施工 Prompt（根因拍板后复制给 Agent）

```text
你是【施工员】。Unity 2020.3.48f1 / C#。最小化修改。默认中文。
沟通：①结论 ②原因 ③用户检查清单 ④程序补充。

@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0902/Village_Chief_House_自由移动光亮DayLight_架构溯源报告.md
@Assets/Doc/提示词/0902/Village_Chief_House_自由移动光亮DayLight_架构侦探提示词.md

## 目标
Village_Chief_House 自由移动：主角 Idle/Walk/Bink 为光亮形态，观感对齐其它村民家。
出屋回村街恢复非 DayLight。龙宫零误伤。

## 默认施工方向（若报告未改口）
1. **F1**：`VillageHomeDayLightAnimApplier` 白名单加入 `SceneName.Village_Chief_House`。
2. 注释写明：村长家室内自由移动 = 村民家光亮策略；勿用 Indoor 总开关。
3. 若 Console 报缺白天 Clip：再修 Override/行，禁止改状态名、禁止方案 E。
4. 同步 OPEN_QUESTIONS.md。

## 约束
- 禁止把龙宫 / KenMuNi1 街道加入白名单
- 禁止改 C# 状态名为 Idle_DayLight
- 禁止改 isFightingScene 当主修
- 禁止用续聊战斗涂层代替自由移动光亮
- 回归：HomeScene23 光亮仍在；龙宫仍暗；村街仍 Combat；出村长家恢复

## 落盘
Assets/Doc/施工说明/0902/Village_Chief_House_自由移动光亮DayLight_施工说明.md

## 验收
- [ ] 村长家站立/走路/眨眼：光亮形态，对齐 HomeScene23 观感
- [ ] Console 有 `[VillageHomeDayLight] 已换白天` 且 scene=Village_Chief_House
- [ ] 出屋回 KenMuNi1：非 DayLight（村街现网）
- [ ] 龙宫 Home 仍非 DayLight
- [ ] 自由移动不是 Combat Run；续聊演出另案不回归破坏还控后的光亮 Home
```

---

## 给开发者（一句话）

其它村民家靠 **`VillageHomeDayLightAnimApplier` 白名单**换光亮片子；村长家多半**没进名单**，所以仍是暗版 Home。施工默认：**白名单加上 `Village_Chief_House`** 即可对齐。
