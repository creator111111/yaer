# Village_HomeScene45 — 进屋立刻闪回村 — 架构溯源报告

**文档版本**：v1.0（2026-08-22）  
**文档性质**：【架构侦探】只读盘点；**本文件不施工**（不改场景 / 代码）  
**Unity**：2020.3.48f1 / C#  
**现象场景**：`Assets/GameRes/Scenes/Village_HomeScene45.unity`  
**对照**：`Village_HomeScene1.unity`（进屋稳）、`Village_HomeScene2.unity`（`EnterFrom_Village` 进屋）

关联：

- 提示词：`Assets/Doc/提示词/0822/Village_HomeScene45_进屋闪回村_架构侦探提示词.md`
- 先例：`0608/Village_OutSide…弹回村外`（落点绑错 Born + `TriggerWhenMoveIn` 门叠加）
- 出门链：`0822/RightDoor回村`（RightDoor 已启用 ✅）
- 出村落点：`0822/回村门口落点`（村侧 `ExitFrom`，**与本期进屋闪回无关**）

---

## ① 结论一句话（根因 **R1**）

**从村进屋后 `EnterPos` 把玩家落在 `RightBorn`（-5.4, -3.65），距已启用的 `RightDoor` 走进即走 Trigger（`TriggerWhenMoveIn=1`）仅约 **2.6～4.8 世界单位**；脚碰撞体 + 首帧位移/物理步即可踩进 Trigger → `SceneChangeDoor.EnterDoor` → 立刻 `LoadScene(Village_KenMuNi1)`，表现为「有概率闪回村」。施工：对齐 HomeScene2，将进屋 `EnterPos` 改绑 **`EnterFrom_Village`（现网同名节点 `LeftBorn`，-24.12, -3.65）**，保留 `RightBorn` 仅作 Map 元数据。**

---

## ② 为何「有概率」而非 100%

| 因素 | 说明 |
|------|------|
| **落点与 Trigger 间距偏窄** | 静态计算：`RightBorn` 中心到 RightDoor Trigger 最近边约 **2.6** 单位；HomeScene1 同口径约 **3.0** 单位，45 更贴门 |
| **布局左移右墙** | `MapRight` 45 为 **(18.36,0)**，HomeScene1/2 为 **(28.8,0)**；`RightDoor` 本地位 45 为 **(-18.16)** vs 1 号 **(-30.67)** → 45 的出门 Trigger 世界 X 约 **0.2**，更靠近室内出生区 |
| **无进屋缓冲点** | HomeScene2 用 **`EnterFrom_Village`（-24.12）** 作 EnterPos；45 虽有同坐标空物体（现名 `LeftBorn`），但 **EnterPos 仍绑 `RightBorn`** |
| **首帧输入 / 物理** | 玩家按住走向右、刚体插值、Capsule 半宽 + `OverlapPadding=0.2` 会放大踩线概率 |
| **代码无免疫** | `SceneChangeDoor` 无进屋后冷却帧；`isEnter` 只防同场景重复，**不防**「刚进房就被右门踢出」 |

生活类比：刚跨进门槛还站在门垫上，后门感应区就在脚边——稍微挪一步或脚盒压线就弹回村口。

---

## ③ 用户需要做什么

1. **认根因**：闪回是 **室内 RightDoor 误触发**，不是村侧 `ExitFrom` 落点问题（那是出村后落哪）。  
2. **认施工**：改 **`Village_HomeScene45`** 的进屋 EnterPos，**不是**改 `Village_KenMuNi1`（除非同时验出村）。  
3. **Play 前先开日志**：Console 过滤 `SceneChangeDoor`、`VillageHomeScene45Debug`、`LoadScene`。  
4. 施工后：**进村 20 次 0 闪回**；主动走向 RightDoor 仍能出村。

### Play 复现步骤（施工前/后对比）

1. 打开 `Village_HomeScene45.unity`，Scene 视图选中 `Map/MapRight/RightDoor`，勾选 **Gizmos** 显示 Collider。  
2. 再选中 `Map/RightBorn`、`Map/LeftBorn`（待改名 `EnterFrom_Village`），目视与绿框距离。  
3. Play → 村 `House_Npc45` 按 E 进屋，**重复 ≥10 次**（可轻按右方向键模拟习惯走位）。  
4. 闪回瞬间查 Console 是否出现：  
   `[SceneChangeDoor] Enter name=RightDoor ... activeScene=Village_HomeScene45 ... next=Village_KenMuNi1`  
5. 对照 `[VillageHomeScene45Debug] lastScene=Village_KenMuNi1`（应命中 EnterPos，非 fallback）。

### 验收清单（施工后）

| # | 操作 | 通过 |
|---|------|------|
| 1 | 村进屋 **20 次** | **0 次**立刻回村 |
| 2 | 进屋站位 | 在 **`EnterFrom_Village` / LeftBorn（-24.12）** 一侧，**不在** RightDoor Trigger 绿框内 |
| 3 | 主动走向 RightDoor | 仍能正常回 `Village_KenMuNi1` |
| 4 | Console | 进屋后、未走向右门前 **无** 意外 `RightDoor` EnterDoor |
| 5 | 与 HomeScene1/2 体感 | 先进屋站稳，再出门 |

### Console 过滤关键字

| 关键字 | 用途 |
|--------|------|
| `SceneChangeDoor` | 是否右门 `EnterDoor` 导致闪回 |
| `VillageHomeScene45Debug` | `lastScene` 是否 `Village_KenMuNi1` |
| `LoadScene` | 换场次序与目标场景 |
| `缺少 InteractiveComponent` | 门链断（本期不应出现） |

---

## ④ 给程序看的补充

### 4.1 代码链（证实 R1 机制）

```
SetPlayerPos(RightBorn)   // 现网 EnterPos
  → PlayerFoot 进入 / 下一物理步进入 RightDoor BoxCollider2D (IsTrigger)
  → CldInteractiveListener.OnTriggerEnter2D
  → InteractiveComponent.onEnterInteractiveEvent
  → SceneChangeDoor.EnterDoor (TriggerWhenMoveIn=1)
  → LoadScene(Village_KenMuNi1)
```

`SceneChangeDoor.cs` L53-64：仅 `TriggerWhenMoveIn` 时订阅 `onEnterInteractiveEvent`；**无**进屋免疫。  
`InteractiveComponent.cs`：`OverlapPadding = 0.2f` 扩大交互判定。

### 4.2 落点与 Trigger 几何（Gizmos 验收）

**Map 根 (0,0,0)**，下列为世界坐标（≈ local）。

| 节点 | HomeScene1（稳） | HomeScene45（现网） | 与 RightDoor Trigger 关系（45） |
|------|------------------|---------------------|--------------------------------|
| **进屋 EnterPos** | `RightBorn` **(-6.91, -3.65)** | `RightBorn` **(-5.4, -3.65)** ❌ | 距 Trigger 最近边约 **2.6**（偏近） |
| **备用进屋点（已存在未绑）** | `LeftBorn` (-24.12, -3.65) | **`LeftBorn` (-24.12, -3.65)** | 距 Trigger **>23** ✅ 安全 |
| `DefaultBornPos`（fallback） | (-24.12, -3.65) | **(-5.61, -3.65)** ❌ | 与 RightBorn 几乎重合，更危险 |
| `MapRight` | (28.8, 0) | **(18.36, 0)** | 45 右墙左移 ~10.4 |
| **RightDoor GO** | 28.8+(-30.67)≈**-1.87** | 18.36+(-18.16)≈**0.20** | 45 更靠室内中心 |
| **RightDoor Trigger AABB（X）** | 约 **[-3.87, -1.87]** | 约 **[-2.80, -0.65]** | Offset -1.93, Size 2.15 |

**PlayerFoot**（`Player.prefab`）：`CapsuleCollider2D` Size ≈ (1, 2)，脚底约 +0.28 X 偏移；半宽约 **0.5**。  
**水平间距（进屋落点中心 → Trigger 最近边）**：45 约 **2.6**；1 号屋约 **3.0**；改绑 **-24.12** 后约 **23.5**。

**Scene 视图 Gizmos**：选中 `RightDoor` 看绿色 BoxCollider2D；选中 `RightBorn` / `LeftBorn` 看落点；两者 X 差约 **18.7** 单位，一眼可判「进屋点是否离门太近」。

### 4.3 EnterPos 与 fallback（C）

| 检查项 | 现网 |
|--------|------|
| `EnterPosConfig` `Village_KenMuNi1` → pos | **`RightBorn`** `3588313296241062192` ❌ |
| `LastSceneName` 进村时（预期） | **`Village_KenMuNi1`**（`House_Npc45` 换场写入） |
| 命中失败 fallback | **`DefaultBornPos` (-5.61)** — 与 RightBorn 同区，**更糟**（H1 次要风险） |
| `nowSceneName` | **`Village_HomeScene45`** ✅（与村侧 EnterPos `lastScene` 字符串一致） |

### 4.4 门配置对拍（D）

| 门 | Enabled | NextSceneName | TriggerWhenMoveIn | Collider（Size / Offset） |
|----|---------|---------------|-------------------|---------------------------|
| **45 LeftDoor** | **0** ✅ | 空 | **0** | 2.52×20 / (-1.24, 0) |
| **45 RightDoor** | **1** ✅ | `Village_KenMuNi1` | **1** | 2.15×20 / (-1.93, 0) |
| **1 RightDoor** | 1 | `Village_KenMuNi1` | 1 | 2×20 / (-1, 0) |

左门已关，**闪回只可能来自 RightDoor**（或极罕见的其它 LoadScene，YAML 未见）。

### 4.5 样板 HomeScene2 — `EnterFrom_Village`（E）

| 项 | HomeScene2 | HomeScene45 现网 |
|----|------------|------------------|
| 进屋 EnterPos | **`EnterFrom_Village`** `3588313296614849449` | **`RightBorn`** ❌ |
| 节点坐标 | **(-24.12, -3.65)** | 同坐标节点存在，名为 **`LeftBorn`**，**未绑 EnterPos** |
| `Map.leftBornTsf` | → 同 Transform | → **已指向 `LeftBorn`** ✅ |
| RightDoor | GO **Inactive** / 换场 Disable | **Active + TriggerWhenMoveIn=1** |
| 方案 A | 已实施 | **可直接复制**：EnterPos 改绑 `LeftBorn`（建议改名为 `EnterFrom_Village`） |

### 4.6 假说排除表

| ID | 假说 | 裁定 |
|----|------|------|
| **H1** | LastScene 未命中 → DefaultBorn | **次要**；fallback (-5.61) 仍在门区；主因仍是绑 RightBorn |
| **H2** | 村门双击 LoadScene | **待 Play 证伪**；闪回若仅一条 RightDoor 日志则排除 |
| **H3** | 左门误启用 | **排除**（LeftDoor Disable） |
| **H4** | NextSceneName 错 | **排除**（RightDoor=`Village_KenMuNi1`） |
| **H5** | 黑幕时序 | **待 Play**；若有 EnterDoor 日志则仍为 R1 |
| **H6** | 村 ExitFrom 叠加 | **排除为本期主因**（现象是进村闪回，非出村再进） |

### 4.7 根因裁定

| 优先级 | 编号 | 结论 |
|--------|------|------|
| **主因** | **R1** | 进屋 `RightBorn` 过近 RightDoor Trigger + `TriggerWhenMoveIn=1` + 无免疫 |
| 次要 | R2 | EnterPos 未命中时 fallback `DefaultBornPos` 同区 |
| 低 | R3 | 初始化时序（需 Console 与 Gizmos 复核） |
| — | R4 | 其它换场源：室内 YAML **未见** |

### 4.8 方案对比

| 方案 | 裁定 |
|------|------|
| **A：新建/复用 `EnterFrom_Village` + 改 EnterPos** | ✅ **推荐**（45 已有 -24.12 的 `LeftBorn`，改绑即可） |
| B：挪 `MapRight` 回 28.8 | ❌ 牵动美术，面大 |
| C：缩小 RightDoor Collider | ❌ 可能走不出屋 |
| D：C# 进屋免疫帧 | ❌ 回归面大；场景方案优先 |
| E：RightDoor 改按 E | ❌ 与 1/23 走进即走不一致 |

### 4.9 推荐施工步骤（Unity Editor）

1. 打开 `Village_HomeScene45.unity`。  
2. （建议）将 `Map/LeftBorn` **重命名**为 **`EnterFrom_Village`**（与 HomeScene2 一致）；坐标保持 **(-24.12, -3.65, 0)**。  
3. 选中 **SceneManager** → `EnterPosConfig`：将 `lastScene: Village_KenMuNi1` 的 **pos** 从 **`RightBorn`** 改为 **`EnterFrom_Village`（原 LeftBorn）**。  
4. **保留** `Map` 组件 `rightBornTsf` → `RightBorn`（Map 元数据，勿删节点）。  
5. **不改** RightDoor（保持 Enable、`TriggerWhenMoveIn=1`、`Next=Village_KenMuNi1`）。  
6. **不改** `Village_HomeScene45SceneManager.cs`、村侧 `ExitFrom_HomeScene45`（另案）。  
7. Play：§3 验收 20 次 + Console 过滤。

### 4.10 最小改动文件列表

| 文件 | 动作 |
|------|------|
| `Assets/GameRes/Scenes/Village_HomeScene45.unity` | EnterPos 改绑；可选 `LeftBorn` → `EnterFrom_Village` 改名 |
| `SceneChangeDoor.cs` | **本期不改** |
| `Village_KenMuNi1.unity` | **本期不改**（进屋闪回） |

### 4.11 严禁

- 只调村侧 `ExitFrom_HomeScene45` 当修进屋闪回  
- 禁用 RightDoor 出门来「修」进屋  
- 未看 `[SceneChangeDoor] Enter name=RightDoor` 就断定非门问题  
- 把 `LeftBorn` 当进屋点却 **不改 EnterPos**（节点在，绑错了）

### 4.12 开放问题

见 `OPEN_QUESTIONS.md` §「Village_HomeScene45 进屋闪回村 · 2026-08-22」。

---

## 修订记录

| 日期 | 说明 |
|------|------|
| 2026-08-22 | v1.0 侦探：R1 RightBorn 过近 RightDoor；方案 A 改绑 -24.12 EnterFrom_Village |
