# Village_Chief_House — 自由移动光亮 DayLight — 架构溯源报告

**文档版本**：v1.0（2026-09-02）  
**文档性质**：【架构侦探】只读定根因（**本阶段未改代码 / Animator / 场景**）  
**Unity**：2020.3.48f1  
**场景**：`Village_Chief_House`（**自由移动**阶段）  
**现象**：村长家里面主角动画不对，**不是光亮形态**  
**产品期望**：与其它 NPC 家里一致 → Home **`Idle`/`Walk`/`Bink` 槽播 `*_DayLight` 片子**（状态名不变）  
**对标**：`Village_HomeScene1/2/23/45`、`Village_House4`  
**不是**：Combat；龙宫；村街；改状态机名；续聊「战斗待机涂层」演出  
**提示词**：`提示词/0902/Village_Chief_House_自由移动光亮DayLight_架构侦探提示词.md`  
**机制上游**：`执行文档/8月/0818/第一章村民家室内_IdleWalk_DayLight_架构溯源报告.md` · `0822/村民家室内_Bink_DayLight_*`

---

## 沟通摘要

### ① 结论一句话

**H1 成立：`VillageHomeDayLightAnimApplier` 白名单没有 `Village_Chief_House`，进房虽走 Home（`isFightingScene=0`）仍静默保留暗版 Idle/Walk/Bink。施工默认 F1：白名单加该常量即可对齐村民家光亮；龙宫/村街勿动。**

### ② 原因（通俗）

村民家进门后，程序会偷偷把「暗版家居服」换成「白天光亮片子」，抽屉名还叫 Idle/Walk/Bink。  
村长家当初没写进这份「哪些屋要换白天衣服」的名单，所以一直穿着暗版。  
不是打成战斗跑，也不是缺白天动画文件。

### ③ 用户检查清单（施工后）

| # | 操作 | 通过 |
|---|------|------|
| 1 | 村长家站立/走路/眨眼：光亮形态，对齐 `HomeScene23` | |
| 2 | Console：`[VillageHomeDayLight] 已换白天` 且 `scene=Village_Chief_House` | |
| 3 | 出屋回 `Village_KenMuNi1`：非 DayLight（村街现网） | |
| 4 | 龙宫 `HomeScene1/2`：仍非 DayLight | |
| 5 | 自由移动不是 Combat `Run`；续聊还控后仍是光亮 Home | |

### ④ 程序补充

见下文 §①～§⑦。

---

## ① 结论（程序向）

| 项 | 裁定 |
|----|------|
| 现网播什么 | **暗版 Home**（`Idle`/`Walk`/`Bink` 原槽）；**不是** Combat |
| 根因 | **H1**：`VillageHomeSceneNames` **缺** `SceneName.Village_Chief_House` |
| Config | `Village_Chief_House.asset` · `isFightingScene: 0`（与 HomeScene23 同） |
| 方案 | **F1** 白名单追加常量；注释「村长家算村民家光亮」 |
| 否 | F2 手写换片；F3/E 改磁盘共用槽；F4 改 `isFightingScene`；Indoor 总开关 |
| 出屋 | 玩家重建 → 新场景不在名单 → 自动非 DayLight |

---

## ② 现网链路钉死（任务 1）

```
进 Village_Chief_House
  → Config.isFightingScene == false
  → 加载 Home_{衣服}_{头饰} 控制器
  → PlayerLogic.UpdateRuntimeController
       name.Contains("Home") → true
       → VillageHomeDayLightAnimApplier.ApplyIfVillageHome(loaded)
            sceneName = GetActiveScene().name  // "Village_Chief_House"
            VillageHomeSceneNames.Contains? → false
            → ★ 静默 return loaded（暗版）
       → ChangeRuntimeController<PlayerHomeCsRuntimeController>
  → 自由移动：Bool Walk；片子为暗版 Idle/Walk/Bink
```

对照进 `Village_HomeScene23`：Contains → true → Clone Override → remap 三槽 → 日志「已换白天」。

---

## ③ 假说证伪（任务 2）

| ID | 假说 | 结果 | 证据 |
|----|------|------|------|
| **H1** | 白名单缺 Chief | ✅ **主因** | `VillageHomeDayLightAnimApplier.cs` L33–40 仅 1/2/23/House4/45 |
| **H2** | 误挂 Combat | ❌ | `isFightingScene: 0`；走 Home 分支 |
| **H3** | 缺白天 Clip 回退 | ❌ 未进名单，不会跑到找 Clip | 加名单后若 Warning 再查 Override |
| **H4** | 场景名不一致 | ❌ | 常量/文件均为 `Village_Chief_House`；`_Door` 仅为 EnterPos 键，**勿**进白名单 |
| **H5** | 把续聊战斗涂层当自由移动 | 边界说明 | 可走阶段应 DayLight Home；涂层另案 |

---

## ④ 白名单现网（磁盘）

```csharp
// VillageHomeDayLightAnimApplier.VillageHomeSceneNames
Village_HomeScene1
Village_HomeScene2
Village_HomeScene23
Village_House4
Village_HomeScene45
// ❌ 无 Village_Chief_House
// ❌ 无 HomeScene1/2（龙宫）
// ❌ 无 Village_KenMuNi1 / Village_Shop
```

| Config | isFightingScene | DayLight？ |
|--------|-----------------|------------|
| Village_HomeScene23 | 0 | ✅ 在名单 |
| Village_Chief_House | 0 | ❌ 不在名单 ← 本期 |
| HomeScene1（龙宫） | 0 | ❌ 故意不在 |
| Forest* | 1 | Combat，无关 |

---

## ⑤ 进房日志期望（任务 3）

| 场景 | 期望 Console |
|------|----------------|
| HomeScene23 | `[VillageHomeDayLight] 已换白天 Idle/Walk/Bink。scene=Village_HomeScene23 …` |
| Chief_House（修前） | **无**该成功日志（静默跳过） |
| Chief_House（F1 后） | `…scene=Village_Chief_House …` 成功换白天 |
| 龙宫 HomeScene1 | 无换白天日志；仍暗版 |

---

## ⑥ 方案与边界（任务 4）

| 方案 | 做法 | 倾向 |
|------|------|------|
| **F1** | `VillageHomeSceneNames.Add(SceneName.Village_Chief_House)` | ✅ |
| F2～F4 | 重复逻辑 / 改磁盘 / 改战斗旗 | ❌ |

### 与续聊「战斗待机」案

| 阶段 | 形态 |
|------|------|
| 自由移动 | **Home + DayLight 片子**（本 F1） |
| 续聊中 | 藏玩家 SR + 场景战斗待机涂层（0902 另案）；**勿**把玩家改成 Combat 控制器 |
| 续聊结束还控 | 玩家可视恢复后须仍是 **Home DayLight**（依赖进房已 Apply；若中途换过控制器须重新走 Home+Applier） |

### 最小施工

1. `VillageHomeDayLightAnimApplier.cs`：白名单加 `SceneName.Village_Chief_House`  
2. 注释：村长家室内自由移动 = 村民家光亮；禁止用 Indoor/`!isFightingScene` 总开关  
3. 若出现「找不到白天 Clip」Warning → 查该套 Home Override 的 `*_DayLight` 行（同 0818/0822），**禁止**改 `RegisterState` 名、禁止方案 E  
4. 同步 OPEN

---

## ⑦ OPEN（任务 5）

产品本条已钉死：**村长家算村民家光亮白名单**。见 `OPEN_QUESTIONS.md` 0902 节。

---

## ⑧ 给施工员的一句话

**只改一行白名单：加上 `Village_Chief_House`。别动龙宫，别改状态名。**
