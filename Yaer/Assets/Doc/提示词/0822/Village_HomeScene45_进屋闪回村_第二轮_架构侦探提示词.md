# Cursor Agent Prompt · Village_HomeScene45：进屋闪回村 Bug — 第二轮深挖

> **角色**：【架构侦探】只读复盘 v1 施工 + 扩查未覆盖链路；报告拍板后【施工员】修复  
> **日期**：2026-08-22  
> **现象**：从村 `House_Npc45` 进屋后，**仍有概率立刻闪回 `Village_KenMuNi1`**  
> **用户反馈（2026-08-22 晚）**：已按 v1 报告施工并测试，**闪回依旧**  
> **场景**：`Assets/GameRes/Scenes/Village_HomeScene45.unity`（主查）、`Village_KenMuNi1.unity`（次要）  
> **前序报告**：`Assets/Doc/执行文档/0822/Village_HomeScene45_进屋闪回村_架构溯源报告.md`（v1.0，根因 R1）  
> **本阶段**：只读；**必须**用磁盘 YAML + Play Console 时间线证伪 v1「已修好」假设

把下面「侦探」整段复制给 Cursor Agent（Agent Mode）执行。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 用户原话

> 经过测试闪回问题依旧存在，让架构侦探再查查。

### v1 结论回顾

v1 裁定 **R1**：进屋落在 `RightBorn` 过近 `RightDoor`（`TriggerWhenMoveIn=1`）→ 立刻 `LoadScene(KenMuNi1)`。  
推荐 **方案 A**：`EnterPos` 改绑 **`EnterFrom_Village`（-24.12, -3.65）**。

### ⚠️ 磁盘预扫：v1 方案可能「半施工」（第二轮首要核实）

**施工后现网 YAML（2026-08-22 晚）与 v1 建议对拍：**

| 检查项 | v1 建议 | **现网磁盘** | 是否生效 |
|--------|---------|--------------|----------|
| `EnterPosConfig` `Village_KenMuNi1` → pos | 绑 `EnterFrom_Village` | 绑 **`3588313296614849449`（EnterFrom_Village）** | ✅ 已绑 |
| **`EnterFrom_Village` 坐标** | **(-24.12, -3.65)** | **(-5.41, -3.65)** | ❌ **仍在右门区** |
| `RightBorn` 坐标 | 保留 Map 元数据 | **(-5.4, -3.65)** | — |
| `DefaultBornPos`（EnterPos fallback） | 应远离门 | **(-5.73, -3.65)** | ❌ **仍在门区** |
| `Map.leftBornTsf` | 宜指向进屋点 | **`{fileID: 0}` 空** | ⚠️ `LeftBorn` 已改名，`Map.Find("LeftBorn")` 失败 |
| 村 `ExitFrom_HomeScene45` | 另案（出村落点） | 已建 **(-4.3, 7.41)**，EnterPos 已绑 | 与**进屋闪回**无直接关系 |

**预扫推论**：若 Play 时实际落点仍在 **x≈-5.4** 一带，则 **R1 未被消除**——不是 v1 判错，而是 **只改了 EnterPos 引用、未把 `EnterFrom_Village` 挪到 -24.12**（或挪完又被布局改回）。侦探须用 Scene 视图 + Console **证实落点坐标**，勿假设「已绑 EnterFrom_Village = 已修好」。

```
v1 施工常见误区：
  改名 LeftBorn → EnterFrom_Village  ✅
  EnterPos 改绑新节点               ✅
  节点坐标仍与 RightBorn 重合        ❌  → 闪回依旧
```

### 第二轮须扩查的假说（v1 未穷尽）

| ID | 假说 | 为何仍可能导致闪回 |
|----|------|-------------------|
| **R1b** | **半施工：EnterFrom_Village 坐标未远离门** | 与 v1 同机制，落点仍在 RightDoor Trigger 内/边缘 |
| **R2** | `LastSceneName` 字符串未命中 EnterPos | 旧名 `Village_HomeScene3`/`Village_House4`/`HomeScene45` → fallback **`DefaultBornPos` (-5.73)** |
| **R3** | **存档路径** `archiveStart` 为 true | `BaseGameSceneManager.InitPlayer` **跳过 EnterPos**，用 `PlayerSceneData.pos`（可能是屋内门口旧坐标） |
| **R4** | 初始化时序：门 `OnInit` 早于玩家，无免疫帧 | 玩家生成瞬间 `OnTriggerEnter2D` → RightDoor |
| **R5** | **村侧连触**：`House_Npc45` 双击 / 进出 Trigger 叠两次 `LoadScene` | Console 可见**两条**换场或先进 45 再立刻 村 |
| **R6** | `TownPlayerLocomotion` / WalkArea 首帧推挤 | 落点安全但首帧被推向右门 Trigger |
| **R7** | 室内另有 `SceneChangeDoor`（`Object` 下门/NPC 链） | v1 只查了 MapLeft/RightDoor |
| **R8** | 出村回村后落在 `ExitFrom_HomeScene45` 与 `House_Npc45` Trigger 重叠 | 体感像「进屋闪回」，实为**出村落点**踩村门（若用户描述含出村后异常） |

### Play 时间线取证（第二轮必填，无日志不得下结论）

请按时间顺序记录 **单次闪回** 的 Console（过滤词见下表）：

| 顺序 | 期望日志 | 说明 |
|------|----------|------|
| 1 | `[SceneChangeDoor] Enter ... House_Npc45` 或村门路径 | 进村触发 |
| 2 | `LoadScene` / 黑幕 | 加载 45 |
| 3 | `[VillageHomeScene45Debug] lastScene=???` | **必须为 `Village_KenMuNi1`** |
| 4 | 玩家实际 `transform.position`（Scene 或临时 Debug） | **是否为 (-24.12,*) 还是 (-5.4,*)** |
| 5 | `[SceneChangeDoor] Enter name=RightDoor activeScene=Village_HomeScene45` | **若出现 → R1/R1b** |
| 6 | 第二条 `LoadScene` → KenMuNi1 | 闪回确认 |

**过滤关键字**：`SceneChangeDoor`、`VillageHomeScene45Debug`、`LoadScene`、`进入场景`、`archiveStart`、`PlayerSceneData`。

### 与 HomeScene2 硬对拍（坐标级）

| 节点 | HomeScene2 | HomeScene45 现网 |
|------|------------|------------------|
| 进屋 EnterPos 节点 | `EnterFrom_Village` **(-24.12, -3.65)** | `EnterFrom_Village` **(-5.41, -3.65)** ❌ |
| 与 RightDoor Trigger 水平距 | **>20** | **~2～5** ❌ |
| RightDoor `TriggerWhenMoveIn` | 右门常 Disable | **1** |

### 生活类比

v1 让你换到「安全门垫」牌子，但门垫还摆在后门感应器上——牌子对了，位置没挪，所以还是会弹出去。

### 严禁

- 不读 YAML 坐标就写「EnterPos 已改绑，R1 已排除」  
- 把村侧 `ExitFrom_HomeScene45` 当进屋闪回主因（除非时间线证明）  
- 未区分 **R2 fallback** 与 **R1b 坐标** 就混为一谈  
- 建议禁用 RightDoor 作为最终方案（除非方案对比后拍板）

---

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/02_SYSTEM_SPEC.md
@Assets/Doc/技术文档/场景相关/场景切换.md
@Assets/Doc/执行文档/0822/Village_HomeScene45_进屋闪回村_架构溯源报告.md
@Assets/Doc/执行文档/0822/Village_HomeScene45_回村门口落点_架构溯源报告.md
@Assets/Doc/执行文档/6月/0608/Village_OutSide_Village_KenMuNi1_换场落点错误弹回村外_架构溯源与修复执行说明.md
@Assets/Scripts/Game/GameRuntime/Entities/SceneEntities/CommonEntity/SceneChangeDoor.cs
@Assets/Scripts/Game/GameRuntime/Entities/Component/Map/Map.cs
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Base/BaseGameSceneManager.cs
@Assets/Scripts/Game/GameMgr/Component/ChangeScene/ChangeSceneComponentGM.cs
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Scene/Village_House/Village_HomeScene45SceneManager.cs
@Assets/GameRes/Scenes/Village_HomeScene1.unity
@Assets/GameRes/Scenes/Village_HomeScene2.unity
@Assets/GameRes/Scenes/Village_HomeScene45.unity
@Assets/GameRes/Scenes/Village_KenMuNi1.unity
@Assets/GameRes/Prefabs/Entity/Player/Player.prefab
@Assets/Prefabs/Stairs.prefab

你现在是【架构侦探】（第二轮）。Unity 2020.3.48f1。
禁止改场景/代码。只读 + 写 **v2 溯源报告**；须包含 **v1 施工核验表** 与 **Play 时间线模板**。

---

## 背景

v1 报告建议改绑 `EnterFrom_Village` 修进屋闪回。用户测试后**仍闪回**。须先证伪「v1 是否真施工到位」，再扩查 R2～R8。

---

## 侦探任务清单

### A. v1 施工核验（最高优先级）

| 项 | v1 要求 | 现网 YAML | Play 实测 | 结论 |
|----|---------|-----------|-----------|------|
| EnterPos → EnterFrom_Village | ✅ | | | |
| EnterFrom_Village 坐标 | -24.12 | | | |
| DefaultBornPos 坐标 | 应远离门 | | | |
| Map.leftBornTsf | 非空或不影响 | | | |
| RightDoor Enabled/Trigger | 保持 | | | |

**若 EnterFrom_Village 仍在 x≈-5.4**：直接裁定 **R1b 半施工** 为主因，并说明为何用户感觉「改了没用」。

### B. Play 时间线（单次闪回完整链）

按 §预梳理表格填空；附建议：闪回时在 `SetPlayerPos` 后打一次 `Debug.Log` 坐标（侦探只建议，本阶段不改代码则写「施工员可加临时日志」）。

### C. LastSceneName / EnterPos 命中（R2）

- 进村瞬间 `ChangeSceneComponentGM.LastSceneName` 实际字符串  
- 与 `EnterPosConfig.lastScene` 是否 **完全一致**（大小写、旧名）  
- 未命中时 fallback 落点坐标  
- `House_Npc45` 换场写入的 lastScene 来源（`SceneChangeDoor` / `Stairs` 预制体）

### D. 存档路径（R3）

- `ProcedureComponentGM.archiveStart` 进屋时是否为 true  
- 若为 true，`PlayerSceneData.pos` 是否在门区  
- 新游戏 vs 读档 vs 村内往返是否复现率不同（解释「有概率」）

### E. 室内全域换场源扫描（R7）

除 `Map/MapRight/RightDoor`、`MapLeft/LeftDoor` 外，搜索 `Village_HomeScene45.unity` 内所有：
- `SceneChangeDoor` / `NextSceneName` 含 `KenMuNi` 或 `Village_`  
- `Object` 下是否有第二道门

### F. 村侧连触（R5 / R8）

- `House_Npc45`：`TriggerWhenMoveIn`、Collider 与 `ExitFrom_HomeScene45` 距离  
- 闪回后是否立刻又进 45（ping-pong）  
- 用户描述的「闪回」是 **仅回村** 还是 **村↔屋抖动**

### G. 首帧位移（R6）

- `TownPlayerLocomotion` 进屋首帧是否改 `Rigidbody2D.position`  
- WalkArea / 碰撞推挤是否把玩家从 -24 推向右门（若坐标已修仍闪回才查）

### H. 方案对比（含 v1 未采纳项）

| 方案 | 说明 |
|------|------|
| **A'** | **`EnterFrom_Village` 坐标改回 (-24.12,-3.65)** + `DefaultBornPos` 同步左移 + `Map.leftBornTsf` 手动绑 EnterFrom |
| B | 外移/缩小 RightDoor Trigger（场景） |
| C | `SceneChangeDoor` 进屋免疫 N 帧（C#，注明回归面） |
| D | RightDoor 改按 E 出村 |
| E | 修 `LastSceneName` 旧字符串（若 R2） |

### I. 验收清单（v2 施工后）

| # | 操作 | 通过 |
|---|------|------|
| 1 | 新游戏 + 读档各进村 20 次 | 0 闪回 |
| 2 | 进屋落点 X | **≤ -20**（与 HomeScene2 同量级），**非** -5.4 |
| 3 | Console | 无意外 `RightDoor` EnterDoor |
| 4 | `lastScene` 日志 | 恒为 `Village_KenMuNi1` |
| 5 | 走向 RightDoor | 仍能出村 |

### J. 开放问题

更新 `OPEN_QUESTIONS.md`「Village_HomeScene45 进屋闪回村 · v2 · 2026-08-22」。

---

## 输出要求

写入：`Assets/Doc/执行文档/0822/Village_HomeScene45_进屋闪回村_架构溯源报告_v2.md`

报告结构：
1. **结论一句话**（R1b / R2 / … 主因）  
2. **v1 为何没修好**（施工核验表）  
3. **Play 时间线证据**（或「待用户补日志」清单）  
4. **最小施工步骤**（优先场景坐标，其次字符串/存档）  
5. 用户验收清单

MASTER 四段式口头汇报。
```

---

## 施工员续跑（v2 报告拍板后再贴）

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0822/Village_HomeScene45_进屋闪回村_架构溯源报告_v2.md
@Assets/GameRes/Scenes/Village_HomeScene45.unity

你现在是【施工员】。按 v2 报告修复进屋闪回（用户已测 v1 无效）。

必须遵守：
- 若报告裁定 R1b：将 `EnterFrom_Village` 挪至 **(-24.12, -3.65, 0)**（对齐 HomeScene2），并确认 EnterPos 仍绑该节点；
- 同步把 **`DefaultBornPos`** 移到安全区（勿与 RightDoor Trigger 重叠），避免 R2 fallback 踩雷；
- `Map` 组件 **`leftBornTsf`** 手动拖入 `EnterFrom_Village`（`Find LeftBorn` 已失效）；
- 保留 RightDoor 出门能力；Play 后打印一次落点坐标验收；
- 若报告要求修 LastSceneName / 存档，按报告最小改动。

提交说明：改前改后坐标表、20 次进村结果、Console 截图描述。
```
