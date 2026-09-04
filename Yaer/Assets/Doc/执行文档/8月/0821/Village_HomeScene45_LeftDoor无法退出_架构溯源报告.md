# Village_HomeScene45 · LeftDoor 无法退出 — 架构溯源报告

**文档性质**：架构侦探产出（只读；**本阶段不改资产/代码**）  
**日期**：2026-08-21  
**场景**：`Assets/GameRes/Scenes/Village_HomeScene45.unity`  
**测试现象**：人在室内走进 `Map/MapLeft/LeftDoor` **无法离开**（无换场）  
**先例**：`0820/Village_HomeScene3改名45与进屋黑屏`（改名前快照须用现网 YAML 证伪）  
**Unity**：2020.3.48f1  

---

## ① 结论一句话

**现网两扇门都出不了屋：LeftDoor 已填回村地址且走进即触发开关打开，但门锁电路（Interactive / `componentsList`）是空的，走进不会调用 `LoadScene`；RightDoor 虽有完整 Interactive，但 `SceneChangeDoor` 组件被禁用。村门进屋与回村 EnterPos / Manager / Build 已接通；主出口走左还是走右仍未拍板。**

---

## ② 原因（生活类比）

| 现象 | 类比 |
|------|------|
| LeftDoor 出不去 | 门牌写了「回村」、感应开关也打开了，但**门锁没装电线**——人撞上去没人理 |
| RightDoor 也出不去 | 门锁电路齐了，但**换场主板电源拔了**（组件 Disable） |
| 合层 `村民家3合层` | 墙上的画，不是门 |
| 进屋能进、出门不能 | 村口门修好了；屋里两扇真门都半成品 |

**走进 LeftDoor 时有没有调用 `LoadScene`？**  
静态结论：**没有**（卡在门初始化：缺 `InteractiveComponent` → `SceneChangeDoor.OnInit` 提前 return，不挂 `onEnterInteractiveEvent`）。

---

## ③ 用户需要做什么（检查清单）

> 施工前勾选；**主出口未拍板前勿双门同时可走**。

### A. LeftDoor 逐项（开发者正在测的这扇）

| # | 项 | 现网 | 勾 |
|---|-----|------|----|
| A1 | GameObject Active | `m_IsActive: 1` | ✅ |
| A2 | 父节点 MapLeft | Active | ✅ |
| A3 | 挂 `SceneChangeDoor` | 有，且 `m_Enabled: 1` | ✅ |
| A4 | `NextSceneName` | `Village_KenMuNi1`（正确，非 Forest / 旧 3） | ✅ |
| A5 | `TriggerWhenMoveIn` | `1`（走进应换场） | ✅ |
| A6 | Trigger Collider | 有 `BoxCollider2D`、`isTrigger`，Layer 8 | ✅ |
| A7 | `componentsList` | **`[]` 空** | ❌ **主因** |
| A8 | Interactive / Listener / EntityControl | **无**（对照 RightDoor 齐件） | ❌ **主因** |
| A9 | Console 预期 | `[SceneChangeDoor] 缺少 InteractiveComponent，跳过门初始化。name=LeftDoor` | 进 Play 核对 |

### B. RightDoor（另一扇，须一并知晓）

| # | 项 | 现网 | 勾 |
|---|-----|------|----|
| B1 | GO Active | `1` | ✅ |
| B2 | Interactive / Collider | 齐（含子物体 InteractiveComponent） | ✅ |
| B3 | `NextSceneName` | `Village_KenMuNi1` | ✅ |
| B4 | `SceneChangeDoor.m_Enabled` | **`0`（禁用）** | ❌ 现网也不能出门 |

### C. 主出口待确认（单独一行）

| # | 项 | 状态 |
|---|-----|------|
| C1 | **45 号屋主出口是 LeftDoor 还是 RightDoor？** | **未拍板** → 记 OPEN Q1；施工员等你选一套再改 |

### D. 回程 / 进屋（现网已通，施工出门时勿弄坏）

| # | 项 | 现网 |
|---|-----|------|
| D1 | 村门 `House_Npc45.NextSceneName` | `Village_HomeScene45` |
| D2 | 村 EnterPos `lastScene: Village_HomeScene45` | 有 |
| D3 | 室内 EnterPos `lastScene: Village_KenMuNi1` | 有（落点绑 `RightBorn`） |
| D4 | Build | 已含 `Village_HomeScene45.unity` |
| D5 | 专用 Manager / Config | `Village_HomeScene45SceneManager` + `.asset` |

### E. 验收标准（施工后）

| # | 操作 | 通过 |
|---|------|------|
| 1 | 村 `House_Npc45` 进 45 | 不黑屏 |
| 2 | 按拍板主出门离开 | 回 `Village_KenMuNi1`，落在该屋门外 |
| 3 | 走 LeftDoor | 若主出口是左：换场；若产品禁用左：走进无换场且报告写明「非故障」 |
| 4 | 非主出口 | 不误飞森林、不双门打架 |
| 5 | Console | 无门相关 NRE /「缺少 Interactive」残留于主出门 |

---

## ④ 给程序看的补充

### 4.1 应然链 vs 现网

```
玩家走进 LeftDoor 或 RightDoor
  → SceneChangeDoor.OnInit（须有 Interactive）
  → TriggerWhenMoveIn → onEnterInteractiveEvent → EnterDoor
  → LoadScene("Village_KenMuNi1")
  → 村 EnterPos lastScene == Village_HomeScene45
  → 落点 House_Npc45 门外
```

| 环节 | LeftDoor | RightDoor |
|------|----------|-----------|
| Active / Next / TriggerWhenMoveIn | 通 | Next 通；**组件 Disable** |
| Interactive 齐件 | **断** | 通 |
| LoadScene | **不会调用** | **不会调用** |
| 村 EnterPos / 村门 | （共用）已通 | （共用）已通 |

其它离开入口（对话 `LoadSceneTaskAction` / 地图 UI / 流程）：本场景 YAML **未见**；现网离开依赖 **仅门**。

### 4.2 左右门现网对照表

| 出口 | Active | SceneChangeDoor | NextSceneName | TriggerWhenMoveIn | Collider/Layer | Interactive / componentsList | 在 sceneObjs | 走进会换场？ |
|------|--------|-----------------|---------------|-------------------|----------------|------------------------------|--------------|--------------|
| MapLeft/LeftDoor | 1 | 有且 Enabled | `Village_KenMuNi1` | 1 | BoxCollider2D Trigger / L8 | **`componentsList: []`，无 Interactive** | 否（门走 Map.OnInit，正常） | **否** |
| MapRight/RightDoor | 1 | 有但 **Enabled=0** | `Village_KenMuNi1` | 1 | 齐 | 有（子物体 `1105019192`） | 否（同上） | **否** |

`sceneObjs` 仅登记 `Npc1`；左右门由 `Map.OnInit` → `leftDoor/rightDoor.OnInit`，**未进 sceneObjs 不是主因**。

### 4.3 因果：走进左门为何不 LoadScene

`SceneChangeDoor.OnInit`（摘录逻辑）：

1. `if (!enabled) return;` — LeftDoor 组件启用，过。  
2. `interactiveComponent = componentSystem.TryGetComponent<InteractiveComponent>();`  
3. **null → LogError「缺少 InteractiveComponent，跳过门初始化」→ return**  
4. 因此永不订阅 `onEnterInteractiveEvent` / `onClickInteractiveEvent` → **不进 `EnterDoor` → 不调 `LoadScene`**

YAML 证据：

- LeftDoor `componentsList: []`（fileID `3588313294666480546`）  
- 组件列表仅：Transform + BoxCollider2D + SceneChangeDoor + ComponentSystemMono + SceneEntity  
- **缺** InteractiveColliderListener、InteractiveEntityControl、Interactive 子物体（RightDoor 有）

| 嫌疑 | 判定 |
|------|------|
| 1 GO/父未激活 | 无关 |
| 2 Next 空/错名 | 无关（已是 `Village_KenMuNi1`） |
| 3 TriggerWhenMoveIn=false | 无关（已是 true） |
| 4 无 Collider | 无关（有 Trigger） |
| 5 **componentsList 空 / 缺 Interactive** | **主因** |
| 6 未进 sceneObjs | 无关（Map 初始化） |
| 7 CheckNextSceneUnlock | 无关（事件未绑，走不到） |
| 8 产品故意禁用左门 | **非现网形态**（现网左门 Active+Enabled+填了 Next，像半成品启用） |
| 9 Build / 场景名 | 无关（进屋已通；断点在调用前） |
| 10 EnterPos 假阴性 | 无关（根本没换场） |

**次因（整屋出不去）**：RightDoor `SceneChangeDoor.m_Enabled: 0` → 即使 Interactive 齐也不初始化换场。

### 4.4 与可玩民居样板差异

| 屋 | 主出口习惯 | 左门 | 右门 | 45 更像？ |
|----|------------|------|------|-----------|
| `Village_HomeScene23` | **右门** | SceneChangeDoor **Disable**、Next 空 | 启用 → `Village_KenMuNi1`、Trigger=1、Interactive 齐 | 右门「主板拔电」像故意关；左门却在填地址——**半套 23 反装** |
| `Village_HomeScene1` | **右门** | Disable + Next 空 + list 空 | 启用 → 村、Trigger=1 | 同上，1/23 惯例是右 |
| `Village_HomeScene2` | **HouseDoor**（偏左） | GO 禁用 | GO 禁用 | 45 无 HouseDoor，不像 2 |
| `Village_HomeScene45` | **未拍板** | 想启用但缺 Interactive | Next 已改村但组件禁用 | **半成品**，勿当已决议 |

### 4.5 回程双侧 EnterPos

| 场景 | 应有 lastScene | 应有 pos | 现网 |
|------|----------------|----------|------|
| `Village_KenMuNi1` | `Village_HomeScene45` | `House_Npc45` 门外 | ✅ `lastScene: Village_HomeScene45`（pos `5601461775652594521`） |
| `Village_HomeScene45` | `Village_KenMuNi1` | 门口 Born | ✅ 有；pos = **`RightBorn`**（`3588313296241062192`） |

残留：

| 残留 | 位置 | 说明 |
|------|------|------|
| `ForestScene` EnterPos | 室内 SceneManager | 次要；进屋来源已是村，可施工时清或留 |
| `Village_House4` 字符串 | 村场景其它门/旧引用 | 与本案出门无关；0820 OPEN Q4 |

室内 EnterPos **无** `HomeScene2`；0820 旧表该项已替换为村。

### 4.6 村门对拍

| 项 | 现网 |
|----|------|
| `House_Npc45.NextSceneName` | **`Village_HomeScene45`**（PrefabInstance 覆盖） |
| 旧 `Village_House4` | 村内仍有其它处字符串残留；**本门已改指 45** |

**进屋能进、出门不能出** → 重点在室内门（已钉死）；村侧三位一体对出门目标已齐。

### 4.7 0820 → 现网 diff

| 项 | 0820（改名前快照） | 现网 0821 | 与「出不去」关系 |
|----|-------------------|-----------|------------------|
| 场景文件 | `Village_HomeScene3` | `Village_HomeScene45.unity` | 已修 |
| Manager | 龙宫 `HomeScene1Manager` | **`Village_HomeScene45SceneManager`**，`nowSceneName=Village_HomeScene45`，Place KenMuNi | 已修（黑屏主因） |
| Config | 龙宫 `HomeScene1.asset` | **`Village_HomeScene45.asset`**，`isFightingScene:0` | 已修 |
| Build | 无 | **有** path | 已修 |
| 村门 House_Npc45 | → `Village_House4` | → **`Village_HomeScene45`** | 已修 |
| 村 EnterPos | 无 45 / 有 House4 | **有 `Village_HomeScene45`** | 已修 |
| 室内 EnterPos | Forest / HomeScene2 | **KenMuNi1 + 残留 Forest** | 回程已通 |
| LeftDoor | 禁用、Next 空、list `[]` | **Active、Next=村、Trigger=1，list 仍 `[]`** | **出门主因仍在** |
| RightDoor | 启用 → `ForestScene` | Next 已改村，但 **SceneChangeDoor Disable** | **出门仍断**；至少不再飞森林 |

**结论**：0820 改名/进屋项大体已施工；**出门半成品**（左门补了地址没补 Interactive；右门关了换场组件）是本期「走 LeftDoor 出不去」的锅。

### 4.8 调用链（程序速查）

```
MapControlComponentGSM → Map.OnInit
  → LeftDoor.OnInit / RightDoor.OnInit
       → SceneChangeDoor：要 Interactive
       → EnterDoor → LoadSceneComponentGSM.LoadScene(NextSceneName)
            → ChangeSceneComponentGM（LastSceneName = Village_HomeScene45）
            → 目标 GSM.SetPlayerPos 匹配 EnterPosConfig.lastScene
```

Play 须看日志（开发者未贴本次 Console）：

1. `[SceneChangeDoor] 缺少 InteractiveComponent，跳过门初始化。name=LeftDoor`  
2. 有无 `[SceneChangeDoor] Enter name=LeftDoor ...`（预期无）  
3. 有无 LoadScene / 未注册 / Unlock（预期走不到）

### 4.9 OPEN（只记录，不施工）

| ID | 问题 | 建议默认（待确认） | 状态 |
|----|------|-------------------|------|
| Q1 | 45 号屋主出口是 LeftDoor 还是 RightDoor？ | 对拍 1/23 现网偏**右门**；开发者正在测**左门**，若要左出门须把左门补齐 Interactive 并处理右门（保持 Disable 或去交互，防双出口） | **待拍板** |
| Q2 | 走出后唯一目标是否 `Village_KenMuNi1`？ | **是**；现网 Next 已写村；禁止再指 ForestScene | 建议确认即可 |
| Q3 | 走进即走还是按 E？ | 现网左右均 `TriggerWhenMoveIn:1`，对齐 1/23 右门 | 建议确认即可 |

写入 `OPEN_QUESTIONS.md` 同节。

### 4.10 施工员最小改动方向（拍板后；本期不改）

**若选 LeftDoor 主出口：**

1. 按 RightDoor / HomeScene23 可玩门样板，给 LeftDoor 补 Interactive 子物体 + Listener + EntityControl，写入 `componentsList`。  
2. 保持 RightDoor `SceneChangeDoor` 禁用（或清空 Next），避免双门。  
3. 可选：进屋 EnterPos 落点改绑 `LeftBorn`（现绑 RightBorn）。

**若选 RightDoor 主出口：**

1. 启用 RightDoor 的 `SceneChangeDoor`（`m_Enabled: 1`）。  
2. LeftDoor：Disable SceneChangeDoor 或清空 Next，并勿只开 GO 不补 Interactive。  
3. EnterPos 保持 `RightBorn` 即可。

**未拍板：停。** 只列上两套，等开发者选一套。

---

## ⑤ 验收回写（施工后填）

| # | 结果 |
|---|------|
| 主出口决议 | **LeftDoor**（2026-08-21 施工：补 Interactive；RightDoor SceneChangeDoor 保持 Disable） |
| 指定门可回村 | 待 Play：走进 LeftDoor → `Village_KenMuNi1` |
| 非主出口不打架 / 不飞森林 | RightDoor 换场仍 Disable；Next 已是村名（不开就不飞） |
| Console 无缺 Interactive（主出门） | 待 Play：不应再出现 `缺少 InteractiveComponent...LeftDoor` |
| 龙宫 / HomeScene23 不回归 | 未改其它场景 |
