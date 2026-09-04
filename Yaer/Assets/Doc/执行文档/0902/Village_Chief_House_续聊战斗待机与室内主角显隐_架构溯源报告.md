# Village_Chief_House — 续聊战斗待机与室内主角显隐 — 架构溯源报告

**文档版本**：v1.0（2026-09-02）  
**文档性质**：【架构侦探】只读定方案（**本阶段未改代码 / 场景 / Prefab / 美术**）  
**Unity**：2020.3.48f1  
**场景 / 对白**：`Village_Chief_House` · `Village_村长家继续对话`  
**产品**：开场藏室内 Home 主角 + 在 **`古莎待机` 旁**显示战斗形态待机；结束 **一次** BlackPanel 内关战斗待机与古莎待机，恢复室内主角；（默认）仍开 **`古莎动画合层`**  
**提示词**：`提示词/0902/Village_Chief_House_续聊战斗待机与室内主角显隐_架构侦探提示词.md`  
**上游**：`执行文档/0901/Village_Chief_House_续聊结束黑幕换古莎动画待机_架构溯源报告.md` · 正面古莎未出现排查（拆包合层教训） · 0902 进屋黑幕 F1′  

---

## 沟通摘要

### ① 结论一句话

**载体用 A（合层预置「雅儿战斗待机」涂层，勿切真玩家 Combat）；开场 S1 在续聊全黑未揭前藏玩家 SpriteRenderer + 亮待机；结束扩展现有 `OnBlackFullyShownForGushaSwap` 同一次黑幕关双待机并恢复主角、默认仍开动画合层。缺现成战斗待机 Prefab——帧图有、合层壳无，须美术/Setup 预置；场景拆包合层与 Prefab 资产双写。**

### ② 原因（通俗）

续聊时不想看见屋里走路的雅儿，又要在古莎旁边站一个「穿铠甲待机」的样子。  
别真把玩家改成战斗状态机（室内规则会乱），像村外侧面古莎那样摆一张场景贴纸最稳。  
结束换古莎本来就有一次黑幕——把关贴纸、关古莎待机、亮回玩家都塞进这同一次，别再闪第二下黑。

### ③ 用户检查清单（施工后）

| # | 操作 | 通过 |
|---|------|------|
| 1 | 续聊开场：看不见室内主角；古莎待机旁可见战斗形态待机 | |
| 2 | 续聊中：无双雅儿；操作仍锁在对话 | |
| 3 | 结束后一次黑幕：战斗待机与古莎待机皆关；室内主角恢复可走 | |
| 4 | （默认）古莎动画合层在村长旁可见；无双古莎；无动画「背景」盖房 | |
| 5 | 同档再进：静默正确态，不重复换装黑幕 | |
| 6 | EnterPos / 楼梯不回归；针线包 Tips 仍在 | |
| 7 | 门口初次对话未被误伤 | |

### ④ 程序补充

见下文 §①～§⑩。

---

## ① 结论（程序向）

| 项 | 裁定 |
|----|------|
| 战斗待机载体 | **A · 场景预置涂层**（命名建议 **`雅儿战斗待机`** / `YaerCombatStandby`） |
| B 真玩家切 Combat | ❌ 次选；踩 Home/`Walk` vs Combat/`Run`；勿改 `isFightingScene` |
| C 克隆实体 | ❌ 除非 A 无任何可贴图 |
| 玩家显隐 | **关/开子级 `SpriteRenderer.enabled`**（根 GO 保持 Active）；禁止 `HideEntity` / 整根 `SetActive(false)` |
| 开场挂点 | **S1**：续聊全黑内（`FinalizeChiefContinueCoverAndCloseBlack` 揭黑前 / 兜底 Trigger 成功且仍可盖黑时） |
| 结束挂点 | **扩展** `OnBlackFullyShownForGushaSwap`；**禁止**第二次 BlackPanel |
| 古莎动画合层 | **默认仍开**（与关待机不矛盾）；若产品只要关待机+恢复主角 → OPEN 改口 |
| 美术 | 有 Combat Idle **帧目录**；**无**现成「雅儿战斗待机」合层 Prefab → **OPEN 依赖**；勿用 UI `GoOutStoryYaerPainting` |
| 合层真源 | 场景 `Design/村长家合层` **拆包**；Setup 必须 **场景 + Prefab 资产**双写（0901 H2） |

---

## ② 「战斗形态待机」身份（任务 1）

| ID | 方案 | 磁盘/风险 | 裁定 |
|----|------|-----------|------|
| **A** | 合层预置 SR/合层涂层，默认关 | 对齐 `古莎待机` / `GushaSidePortrait` 精神；脚位可摆 | ✅ **默认** |
| B | `PlayerLogic` 换 Combat 控制器 + 挪位 Idle | `UpdateRuntimeController` 双轨；室内 `isFightingScene` 应为 false；Town 禁写 `Run` | ⚠️ 仅 A 不可行时 |
| C | Instantiate 战斗 Avatar | 生命周期/DepthZone 缓存 | ⚠️ 末选 |

### 可用资源 vs 缺口

| 资源 | 路径 / 状态 | 用途 |
|------|-------------|------|
| 古莎待机 | 合层内 SR；`localPos≈(33.4, 3.27, 2.67)`；场景+Prefab 均有 | 脚位参照；对话中保持显示 |
| 古莎动画合层 | `ArtRes/Animation/古莎动画合层.prefab`；场景已预置（0901 Setup） | 结束换人目标 |
| Combat Idle 帧 | `ArtRes/Animation/Yaer/Combat/Idle/`（铠甲基本/疲劳/战损 × 冠/护头/无） | **可贴图源**（建议「基本」态单帧或短循环） |
| Home 白天 Idle | `ArtRes/Animation/Yaer/Home/Idle/*Idle_DayLight`（原「战斗服待机拷贝」改名） | ❌ 观感偏白天 Home，非「战斗形态」首选 |
| **雅儿战斗待机 Prefab** | **磁盘无**（`ArtRes/Animation` 仅古莎相关合层 Prefab） | ⚠ **须新建**或美术交付 |
| UI GoOut 立绘 | `GoOutStoryYaerPainting` | ❌ 禁止冒充场景待机 |

**施工默认（有帧无壳时）**：Setup 菜单用「铠甲基本{冠|护头|无}」第 1 帧（或产品指定态）生成单 SR `雅儿战斗待机`，脚位在古莎待机左侧/旁（Δ 可调），SortingOrder≈10～11（&lt; 村长 12，对齐待机量级），默认 `Active=false`。头饰跟存档可选 P1。

---

## ③ 全时序（任务 2）— 现网 + 本期写入点

```
进屋 LoadScene 黑幕
  → TryDeferBlackFadeForCover（应播续聊）
       TriggerStory(继续对话)
       onStoryTriggered → …
       ★【开场 S1 · 揭黑前】
            SetPlayerVisualVisible(false)      ← 藏室内 Home 形象
            yaerCombatStandby.SetActive(true)  ← 亮战斗待机（古莎旁）
            （古莎待机保持 Active）
       → CloseFormFade 揭黑 → 续聊前奏/对白
  → …续聊…
  → onStoryEnd → OnChiefContinueStoryEnd
       → OpenSystemBlackFade（既有 · 唯一结束黑幕）
       → ★【结束 · 全黑内合并】OnBlackFullyShownForGushaSwap
            ApplyGushaVisual(showAnim:true)    ← 关古莎待机 + 开动画合层（0901）
            yaerCombatStandby.SetActive(false) ← 关战斗待机（与古莎待机一起）
            SetPlayerVisualVisible(true)       ← 恢复室内主角
            MarkGushaAnimStandbyFlag()
       → CloseFormFade → 还控
```

| 阶段 | 室内 Home 可视 | 雅儿战斗待机 | 古莎待机 | 古莎动画合层 |
|------|----------------|--------------|----------|--------------|
| 续聊进行中 | ❌ | ✅ | ✅ | ❌ |
| 结束全黑后 | ✅ | ❌ | ❌ | ✅（默认） |
| 读档续聊已用 | ✅ | ❌ | ❌ | ✅（静默 Apply） |

**兜底**：`OnEnterScene`→`TryTriggerChiefContinueOnce` 若未走 Defer，仍须在 Trigger 成功后、尽量仍有遮罩时做 S1；无遮罩则接受一帧风险并打日志（优先保证 Defer 主路径）。

---

## ④ 玩家显隐安全做法（任务 3）

| 做法 | 可用性 | 说明 |
|------|--------|------|
| **子级 `SpriteRenderer.enabled` 批量开关** | ✅ 推荐 | DepthZone 已约定缓存玩家全部 SR；根逻辑/碰撞/落点保留 |
| 整根 `gameObject.SetActive(false)` | ❌ | 易断输入订阅、物理、换场脚 |
| `EntityComponentGM.HideEntity` | ❌ | 卸实体，非「藏皮」 |
| 改 `isFightingScene` / 换 Combat 控制器 | ❌ 非 A 主修 | 规范 §4 双轨风险 |
| 挪玩家到古莎旁 | ❌ 不需要 | 待机涂层自带脚位；避免楼梯 ClosestPoint |

建议在 `Village_Chief_HouseSceneManager`（或小工具方法）实现：

```text
SetPlayerVisualVisible(bool visible)
  → PlayerLogic 上 GetComponentsInChildren<SpriteRenderer>(true)
  → r.enabled = visible
```

续聊中对话本已 `BlockOtherInteraction`；恢复可视后依赖既有还控，**勿**额外永久锁操作。

---

## ⑤ 与 0901 结束链合并（任务 4）

| 现网 | 本期增量 |
|------|----------|
| `OnChiefContinueStoryEnd` → BlackPanel → `ApplyGushaVisual(true)` → 记旗 → 淡出 | 同回调内：**关战斗待机** + **恢复玩家可视** |
| `ApplyGushaVisualFromArchive` 进房静默 | 同步：**战斗待机关** + **玩家可视**（续聊已用/旗已立） |

**是否仍开古莎动画合层？**

| 选项 | 倾向 |
|------|------|
| **仍开**（关待机 ≠ 不要正面古莎） | ✅ **默认**（用户本条未否决 0901） |
| 只关待机+恢复主角，不开动画合层 | 须产品改口 → OPEN Q |

**禁止**：结束再 Open 第二次 BlackPanel 专门换主角。

---

## ⑥ 读档 / 合层拆包（任务 5）

### 静默态

| 条件 | 古莎待机 | 动画合层 | 战斗待机 | 玩家 Home 可视 |
|------|----------|----------|----------|----------------|
| 续聊未用 | ✅ | ❌ | ❌ | ✅ |
| 续聊进行中（仅运行时） | ✅ | ❌ | ✅ | ❌ |
| 续聊已用 / 换人旗 | ❌ | ✅ | ❌ | ✅ |

进房只走 `ApplyGushaVisualFromArchive` + `EnsureYaerCombatStandby(false)` + `SetPlayerVisualVisible(true)`，**不再**开黑幕。

### 合层

| 位置 | 状态（磁盘） |
|------|----------------|
| `Prefab/村长家合层`（guid `5cad…`） | 有 `古莎待机`；嵌 `古莎动画合层`（4271a266） |
| 场景 `Design/村长家合层` | **拆包 GO**；有 `古莎待机`；已有动画合层名引用（0901 Setup 后） |
| 教训 | 新预置 **必须**写场景合层；可扩 `ChiefHouseGushaAnimStandbySetupEditor` 或新建 `…YaerCombatStandbySetup` |

---

## ⑦ 最小施工清单 + Setup（任务 6）

1. **美术/Setup · A 预置**
   - 新建或生成 `雅儿战斗待机`（单 SR 起步即可）于合层，旁 `古莎待机`，默认关  
   - **双写**：`Prefab/村长家合层` + 场景 `Design/村长家合层`  
   - 菜单：扩现有 Gusha Setup 或新菜单 `Setup Chief House 雅儿战斗待机预置`
2. **`Village_Chief_HouseSceneManager`**
   - 字段/按名解析 `yaerCombatStandby`  
   - S1：Defer 揭黑前 `ApplyContinueTalkVisuals(inTalk:true)`  
   - 结束全黑：`ApplyContinueTalkVisuals(inTalk:false)` 并入 `OnBlackFullyShownForGushaSwap`（先/后于 `ApplyGushaVisual` 均可，建议同 try 块）  
   - 进房归档：`inTalk:false` 静默  
   - `SetPlayerVisualVisible`；详细注释 A vs B；**勿**二次 BlackPanel
3. **OPEN**：动画合层默认仍开；战斗待机 Prefab/帧选型待确认  
4. **不做**：门口初次同套；Loading；改 CSV/三人 UI 立绘；`isFightingScene=true`

### 与分层淡入案关系

0902「开场分层对齐门口」若同批施工：S1 藏玩家/亮待机须在**揭黑前**完成，与 T1′「alpha 备好再揭」兼容——都在全黑阶段写完再 `CloseFormFade`。

---

## ⑧ 风险表

| 风险 | 缓解 |
|------|------|
| 双雅儿 | 亮待机前必须先关玩家 SR |
| 结束留战斗壳 | 结束回调强制 `standby=false`；归档静默再保险 |
| 拆包漏预置 | Setup 双写 + Console 找不到则 Warning |
| DepthZone 缓存 | 只改 `enabled`，不 Destroy SR；必要时续聊后再刷新缓存（若有） |
| 落点/楼梯 | 不挪玩家 Transform |
| B 误用 | 报告默认否决；代码注释写清 |

---

## ⑨ OPEN 待确认

| ID | 问题 | 施工默认 | 状态 |
|----|------|----------|------|
| Q1 | 结束是否仍开古莎动画合层？ | **是** | ✅ 默认 / 产品可改口 |
| Q2 | 战斗待机用哪套帧（基本/疲劳/战损 × 头饰）？ | **铠甲基本 + 跟存档头饰**（无则基本无） | ⏳ 美术确认 |
| Q3 | 单帧 SR 还是真 Animator 循环？ | **单帧 SR 先落地**；循环另案 | ✅ 施工默认 |
| Q4 | 门口初次是否同套显隐？ | **否** | ✅ |
| Q5 | 合层壳谁做？ | Setup 生成 or 美术 Prefab | ⏳ |

---

## ⑩ 代码锚点速查

| 主题 | 路径 |
|------|------|
| 续聊 defer / 揭黑 | `Village_Chief_HouseSceneManager.TryDeferBlackFadeForCover` / `FinalizeChiefContinueCoverAndCloseBlack` |
| 结束换人黑幕 | `OnChiefContinueStoryEnd` / `OnBlackFullyShownForGushaSwap` / `ApplyGushaVisual` |
| 双轨规范 | `02_SYSTEM_SPEC` §4；`PlayerLogic.UpdateRuntimeController` |
| 侧面样板 | `ChiefNearDoorStoryTrigger`（全黑启用涂层） |
| Setup 样板 | `ChiefHouseGushaAnimStandbySetupEditor`（场景+资产双写） |
| Combat Idle 帧 | `Assets/ArtRes/Animation/Yaer/Combat/Idle/` |

---

## ⑪ 给施工员的一句话

**贴纸站古莎旁、藏玩家皮；结束塞进现有换古莎那一次黑幕。先补「雅儿战斗待机」预置（场景拆包别漏），再接线。**
