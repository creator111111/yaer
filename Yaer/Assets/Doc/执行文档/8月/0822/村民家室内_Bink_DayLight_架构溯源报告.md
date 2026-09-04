# 第一章村民家室内改播 Bink_DayLight（白天眨眼）— 架构溯源报告

**文档版本**：v1.0（2026-08-22）  
**文档性质**：【架构侦探】只读溯源；**本文件不施工**（不改代码 / Animator / Override / Prefab / 场景 / Clip）  
**Unity**：2020.3.48f1 / C#  
**范围**：肯姆尼村 **村民家室内** 眨眼切白天；龙宫、村街道、商店、Combat **排除**  
**产品决议（2026-08-22，开发者）**：**推翻 0818 Q3「屋里仍用现网 Bink」**；村民家白名单内眨眼改 `Bink_DayLight`；C# 状态名 **仍是 `Bink`**。

关联：

- 提示词：`Assets/Doc/提示词/0822/村民家室内_Bink_DayLight_架构侦探提示词.md`
- 上案（Idle/Walk）：`Assets/Doc/执行文档/0818/第一章村民家室内_IdleWalk_DayLight_架构溯源报告.md`
- 现网 Applier：`Assets/Scripts/Game/GameRuntime/Entities/Player/Components/VillageHomeDayLightAnimApplier.cs`
- 规范：`Assets/Doc/02_SYSTEM_SPEC.md` §4 Home/Combat 双轨

---

## ① 结论一句话

**屋里眨眼仍是旧片，根因是「三道门都没开」：`VillageHomeDayLightAnimApplier` 第 87 行故意不改 `Bink` 槽；底图 Controller 与三套 Override **没有** `Bink_DayLight` 孤岛行（类比 `Idle_DayLight`）；磁盘上四套 `*_Bink_DayLight.anim` 全是旧片复制件，**未引用** `ArtRes/.../Blink/` 新帧图。推荐延续方案 B′：补孤岛 + Override 行 + 重接 Clip 帧图，再扩 Applier 对称 remap `Bink`→`Bink_DayLight`；C# 仍 `IsName("Bink")`。方案 E 已否决。**

---

## ② 原因（生活类比）

待机/走路进屋时，程序已经帮你把抽屉里的片子换成了「白天家居服」（`Idle_DayLight` / `Walk_DayLight`），但 **眨眼那个抽屉（`Bink`）被施工员故意留空**——注释写「Bink 行故意不改」。

更糟的是：美术把新白天眨眼衣服（`ArtRes/.../Blink/` 九张图）放进了仓库，也建了名叫 `Bink_DayLight` 的 `.anim` 文件，但这些文件 **里面仍塞着晚上那套旧图**；Controller 的 Override 表里也 **没有** `Bink_DayLight` 这一行，运行时 Applier 就算想换也 **找不到白天片子从哪取**。

所以不是 C# 不认眨眼，而是 **片源、Override 表、Applier 三处都没接上**。

---

## ③ 用户需要做什么

范围 / 龙宫 / 白名单 **已拍板**，本阶段用户只需：

1. **认方案**：推荐 **B′**（扩展现有 `VillageHomeDayLightAnimApplier`，对称 Idle/Walk 做法 remap `Bink` 槽；**禁止**改 C# 状态名、**禁止**方案 E 改磁盘共用 `Bink` Clip）。  
2. **知悉 Clip 缺口**：四套 `*_Bink_DayLight.anim` 需美术/动画师 **重接** `ArtRes/.../Blink/` 帧图后再验收；裙子 Dress **无** 白天眨眼帧目录，需产品拍板（见 OPEN 技术项 T1）。  
3. **按清单验收**（施工后；本阶段先不当作已修好）。

### 验收清单（施工后）

| # | 操作 | 期望 |
|---|------|------|
| 1 | InitScene → 第一章进村 `Village_KenMuNi1` | 街道保持现网（Combat，眨眼若走 Home 则仍是旧 `Bink`） |
| 2 | 进 `Village_HomeScene1`（或 2 / 23 / 45） | 站定等眨眼 → **白天帧**（与 `Idle_DayLight` 风格一致） |
| 3 | 同屋走两步停 | 再走停后再眨一次，仍是白天帧 |
| 4 | 换 Crown / ArmorHead / None 各进一户 | 三套铠甲眨眼均走各自 `*Bink_DayLight`，非旧夜间帧 |
| 5 | 出屋回村街道 | **立刻**回到现网旧眨眼（非 DayLight） |
| 6 | 进龙宫 `HomeScene1` / `HomeScene2` | 眨眼 **仍是旧 `Bink`**，Idle/Walk 也不变白天 |
| 7 | Console | 无「状态切换失败: Bink」；无 `Bink是循环动画IsFinished无法判断` 刷屏（若 Clip 已改 `LoopTime=0`） |

`Village_House4` 场景文件磁盘缺失、仍保留白名单占位；`Village_HomeScene3` 已改名为 `Village_HomeScene45`（见 §4.4）。

---

## ④ 给程序看的补充

### 4.1 调用链：眨眼何时触发

```
PlayerLogic.UpdateRuntimeController
  → controllerAsset.name.Contains("Home")
  → VillageHomeDayLightAnimApplier.ApplyIfVillageHome(loaded)   // 现网只换 Idle/Walk
  → PlayerHomeCsRuntimeController → PlayerHomeSM
       Enter 默认进 IdleSubState 子机
       HomeIdleSubSM.Enter → ChangeState<HomeBinkState>()        // 进屋先眨
       HomeWalkState.Update 静止 → ChangeState<HomeBinkState>()  // 走停再眨
       HomeBinkState.Update → IsFinished → HomeIdleState
```

**状态名契约（硬门闩，不可改）**：

| C# | Register | Animator Bool | Animator 状态 | Motion 槽名 |
|----|----------|---------------|---------------|-------------|
| `HomeBinkState` | `"IdleSubState_Bink"` / **`"Bink"`** | `IdleSubState_Bink` | `Bink`（在子机 `IdleSubState` 内） | 底图 Key = **`Bink`** |

`BaseState.IsFinished`：`IsName(stateName) && normalizedTime >= 1`；若 `StateInfo.loop==true` 会 `Debug.LogWarning` 但仍用 `normalizedTime>=1` 判断（现网 None/Crown/Armor 旧 `Bink` 部分 `LoopTime=1`，龙宫/村外仍在用，本期若新 Clip 建议统一 `LoopTime=0` 对齐 Dress `Bink`）。

### 4.2 现网 Applier 为何跳过 Bink

```87:91:Assets/Scripts/Game/GameRuntime/Entities/Player/Components/VillageHomeDayLightAnimApplier.cs
            // Bink 行故意不改：屋里眨眼继续现网 Bink。
            runtime.ApplyOverrides(pairs);
            Debug.Log(LogTag + " 已换白天 Idle/Walk。scene=" + sceneName + " asset=" + loaded.name
                + " idle=" + idleDay.name + " walk=" + walkDay.name);
```

白名单（与 Idle/Walk 共用，侦探核实）：

```30:37:Assets/Scripts/Game/GameRuntime/Entities/Player/Components/VillageHomeDayLightAnimApplier.cs
        private static readonly HashSet<string> VillageHomeSceneNames = new HashSet<string>
        {
            SceneName.Village_HomeScene1,
            SceneName.Village_HomeScene2,
            SceneName.Village_HomeScene23,
            SceneName.Village_House4,
            SceneName.Village_HomeScene45,
        };
```

出屋还原：切场重建玩家，村街道加载 Combat，龙宫加载未改磁盘资产——与 0818 报告一致，**不必**运行时「关开关」。

### 4.3 新 Clip 验收表（侦探逐文件核实）

**帧图来源说明**：`ArtRes/Animation/Yaer/Home/Blink/` 共 9 张（None1~3、Crown1~3、Armor / Armor2 / Armor3），**全工程 `.anim` 内零引用**（Grep 无匹配）。

| 换装 | Clip 路径 | 引用帧图（实测 GUID → 路径） | 是否接 Blink 新图 | SampleRate | StopTime | LoopTime | Shadow 曲线 | 与旧 Bink 差异 |
|------|-----------|------------------------------|-------------------|------------|----------|----------|-------------|----------------|
| **Dress** | `Dress/Bink_DayLight.anim` | 与 `Bink.anim` **完全相同**：`e007b339`/`356d24cb`/`2f5e07b0` → `ArtRes/.../Dress/Idle/Bink/01~03` | **否** | 12 | 3.083s | **0** | Animation + ShadowAnimator | **逐字节同旧片**（仅文件名不同） |
| **None** | `None/ArmorNoneBink_DayLight.anim` | 与 `ArmorNoneBink` 相同：`e9df3793`/`62c7a4b8`/`ecb2a7dd`（旧 None 夜间眨眼） | **否** | 12 | 3.083s | **1** | 双曲线齐全 | 同旧片；应接 `None1~3`（`53aaa2bb`/`02a658bc`/`4952101f`） |
| **Crown** | `Crown/ArmorCrownBink_DayLight.anim` | 与 `ArmorCrownBink` 相同：`7de841d2`/`06268241`/`0a56f9b8` | **否** | 12 | 3.083s | **1** | 双曲线齐全 | 同旧片；应接 `Crown1~3`（`3abd6adf`/`c6446f52`/`927db22a`） |
| **ArmorHead** | `Armor/ArmorBink_DayLight.anim` | 与 `ArmorBink` 相同：`af4fedd8`/`31d84ac6`/`5192b531` | **否** | 24 | **1.542s** | **1** | 双曲线齐全 | 同旧片（非「更短新片」）；应接 `Armor`/`Armor2`/`Armor3`（`5ef0452f`/`5466bdb0`/`5fa817ec`） |

**旧 Bink 节奏（供对齐参考）**：

- Dress / None / Crown：约 **3.08s**，0s 与 2s 各眨一次（中间长待机 hold），`Dress/Bink` 的 `LoopTime=0`。
- ArmorHead：约 **1.54s**，0s 与 1s 各眨一次，帧更密（24fps）。

**Blink 新图目录（已交付，未接线）**：

| 文件 | GUID |
|------|------|
| `Blink/None1.png` | `53aaa2bbc73cc0248b19ca64fa386ef5` |
| `Blink/None2.png` | `02a658bc48c5bb3449d988d3708a7465` |
| `Blink/None3.png` | `4952101f344eb734dbdeaaf8bbab1a43` |
| `Blink/Crown1.png` | `3abd6adf9c2643d4cb8f495ac048871d` |
| `Blink/Crown2.png` | `c6446f526c04e3947aa7449838d6bc34` |
| `Blink/Crown3.png` | `927db22a72a778f4b894b02a6c492d08` |
| `Blink/Armor.png` | `5ef0452fe64664c44af3ebe2310f3401` |
| `Blink/Armor2.png` | `5466bdb0584c4a04ea956996cd627ebb` |
| `Blink/Armor3.png` | `5fa817ec80ed1f14da7274b0a9135a6f` |

**裙子缺口**：`ArtRes/.../Blink/` **无** Dress 裙子帧；`Bink_DayLight.anim` 仍指向旧 `Dress/Idle/Bink/` 三帧——产品需决定 Dress 是否沿用旧裙眨眼 / 暂不换 / 另补素材。

### 4.4 Override / Controller 接线（侦探核实）

**底图 `Home_Dress_Crown.controller`**：

- 已有孤岛态：`Idle_DayLight`、`Walk_DayLight`（无过渡线，仅供 Override 列表出现 Key）。
- **`Bink`** 在 `IdleSubState` 子机内，Motion = `Bink.anim`（guid `c9abe863`）。
- **未见 `Bink_DayLight` 孤岛态**（Grep 全 `Home/` 目录零匹配）。

**三套 Override**（均 5 行：Bink / Idle / Walk / Idle_DayLight / Walk_DayLight）：

| 原 Clip Key（裙子） | NoHeadWear | Crown | ArmorHead |
|---------------------|------------|-------|-----------|
| `Bink` | `ArmorNoneBink` | `ArmorCrownBink` | `ArmorBink` |
| `Idle_DayLight` | `ArmorNoneIdle_DayLight` | `ArmorCrownIdle_DayLight` | `ArmorIdle_DayLight` |
| `Walk_DayLight` | `ArmorNoneWalk_DayLight` | `ArmorCrownWalk_DayLight` | `ArmorWalk_DayLight` |
| **`Bink_DayLight`** | **缺失** | **缺失** | **缺失** |

施工须 **仿 Idle_DayLight**：底图加孤岛 `Bink_DayLight` 态 + 三套 Override 各加一行 `Bink_DayLight` → `*Bink_DayLight.anim`，Applier 才能 `FindEffectiveClip(pairs, "Bink_DayLight")`。

### 4.5 推荐方案 B′ + 否决项

| 方案 | 摘要 | `IsName("Bink")` | 仅村民家 | 换装 | 龙宫 | 结论 |
|------|------|------------------|----------|------|------|------|
| **B′（推荐）** | 补 `Bink_DayLight` 孤岛 + Override 行；修 Clip 接 Blink 图；扩 Applier `RemapOriginalSlot(Bink, binkDay)` | **不变，最安全** | 同现网白名单 | 从 Override 表取生效片 | 不改磁盘共用槽 | **推荐** |
| C | 复制四套「全白天」Controller，PathHelper 按场景换路径 | 不变 | 好 | 四套都要备齐 | 旧资产不动 | B′ 失败退路 |
| A | 新 Bool + 平行态 `Bink_DayLight` 改 C# 注册 | 必须改 | 好 | 可复用 | 图默认可保 | **否决**：与 Idle 子机衔接成本高 |
| **E** | 直接改磁盘 `Bink` / Override 的 `Bink` 槽为 DayLight | 安全 | **差** | 简单 | **龙宫一起变** | **已否决** |

**严禁**：

1. 新建 Animator 状态名 `Bink_DayLight` 并改 C# `RegisterState` 的 `"Bink"`（`IsName` 卡死）。  
2. 只拉孤岛过渡线、不改 Applier remap（0818 已证 Idle 孤岛路不通）。  
3. 用 `TerrainType.IndoorType` / `!isFightingScene` 当总开关（龙宫误伤）。  
4. 只改 `.anim` 不重接 Override / Applier（屋里仍可能走铠甲旧槽）。

**Applier 扩展伪代码（对称 Idle/Walk）**：

```csharp
private const string ClipBink = "Bink";
private const string ClipBinkDayLight = "Bink_DayLight";

AnimationClip binkDay = FindEffectiveClip(pairs, ClipBinkDayLight);
// 可与 idleDay/walkDay 同样：缺 binkDay 时整单 fallback 或仅跳过 Bink（施工拍板）
bool binkOk = RemapOriginalSlot(pairs, ClipBink, binkDay);
```

### 4.6 Clip 时长 / Loop 与 `IsFinished` 风险

| 风险 | 现网 | 施工建议 |
|------|------|----------|
| 仅 3 帧、时长压到 ~0.25s | 旧片眨眼段 0.25s，但整体 Clip 3s/1.5s 才结束 | **保持与旧 Bink 同 StopTime**（3.08s / 1.54s），3 帧后 hold 开眼帧至片尾，避免进屋/走停后「眨完立刻回 Idle」节奏变快 |
| `LoopTime=1` | None/Crown/Armor 旧 `Bink` 为 1；`IsFinished` 仍可用但 Console 可能 Warning | 新 `*_Bink_DayLight` 建议 **`LoopTime=0`**，对齐 Dress `Bink` |
| Loop 导致永不结束 | 若误开 Loop 且逻辑依赖片尾 | 统一 `LoopTime=0` + 验收 Console 无 Bink Warning |

**不影响** Idle↔Bink 子机过渡：仍走 `IdleSubState_Bink` / `IdleSubState_Idle` Bool，只换 Motion 片子。

### 4.7 范围与白名单

| 场景名 | 产品 | Applier 现网 | 备注 |
|--------|------|--------------|------|
| `Village_HomeScene1/2/23` | **开** | **已写入** | |
| `Village_HomeScene45` | **开** | **已写入** | 原 `Village_HomeScene3` 已改名 |
| `Village_House4` | **开** | **已写入** | `.unity` 可能缺失，占位 |
| `Village_HomeScene3` | 产品表仍写 | **未写入** | 已改名为 45；旧档/旧门若仍写 `3` 需另案或白名单补字面量 |
| `Village_KenMuNi1` | **不开** | 未写入 | Combat |
| `HomeScene1/2`（龙宫） | **严禁** | 未写入 | |
| `Village_Shop` | 排除 | 未写入 | 无主角 |

### 4.8 历史决议更新（supersede 0818 Q3）

| 文档 | 旧决议 | 新决议（2026-08-22） |
|------|--------|----------------------|
| OPEN §村民家室内 DayLight Q3 | 屋里眨眼仍用现网 `Bink` | **作废** → 村民家白名单内改 `Bink_DayLight` |
| 0818 溯源报告 §① / 验收 #3 | 眨眼仍是旧 `Bink` | 施工目标改为白天眨眼；本报告为准 |

### 4.9 最小改动文件列表（只建议，本阶段不改）

| 文件 | 动作 |
|------|------|
| `VillageHomeDayLightAnimApplier.cs` | 增 `ClipBink` / `ClipBinkDayLight`；`FindEffectiveClip` + `RemapOriginalSlot`；日志带上 bink 片名；更新类注释 |
| `Home_Dress_Crown.controller` | 增孤岛态 `Bink_DayLight`（Motion = `Bink_DayLight.anim`），**无过渡线** |
| `Home_Armor_NoHeadWear.overrideController` | 增行 `Bink_DayLight` → `ArmorNoneBink_DayLight` |
| `Home_Armor_Crown.overrideController` | 增行 `Bink_DayLight` → `ArmorCrownBink_DayLight` |
| `Home_Armor_ArmorHead.overrideController` | 增行 `Bink_DayLight` → `ArmorBink_DayLight` |
| `Dress/Bink_DayLight.anim` | 重接帧图（待产品定 Dress 素材） |
| `None/ArmorNoneBink_DayLight.anim` | 重接 `Blink/None1~3`；建议 `LoopTime=0`、StopTime 对齐旧 3.08s |
| `Crown/ArmorCrownBink_DayLight.anim` | 重接 `Blink/Crown1~3`；同上 |
| `Armor/ArmorBink_DayLight.anim` | 重接 `Blink/Armor*`；StopTime 对齐旧 1.54s |

**不改**：`HomeBinkState.cs` 状态名、`PlayerHomeSM`、龙宫场景、村街道 Combat、`Bink.anim` 等磁盘共用夜间片。

### 4.10 仅技术开放问题

见 `OPEN_QUESTIONS.md` §「村民家室内 Bink_DayLight · 2026-08-22」。

---

## 修订记录

| 日期 | 说明 |
|------|------|
| 2026-08-22 | v1.0 侦探溯源：根因=Applier 跳过 + 无 Bink_DayLight 行 + Clip 未接 Blink 图；推荐 B′；supersede 0818 Q3 |
