# Village_HomeScene3 → Village_HomeScene45 改名 + 专用 Manager + 进屋黑屏 — 架构溯源报告

**文档性质**：架构侦探产出（只读；**本阶段不改资产/代码**）  
**日期**：2026-08-20  
**产品目标**：
1. 场景 **`Village_HomeScene3`** → **`Village_HomeScene45`**
2. **专用村屋 Manager**（禁止继续挂龙宫 `HomeScene1Manager`）
3. 查明 **进不去 / 黑屏**（与改名同案盘点，可分批施工）

**磁盘现状**：`Assets/GameRes/Scenes/Village_HomeScene3.unity`（meta GUID `1a5fdf21746c7764899405628bc1edc7`）  
**先例**：
- `0804/Village_HomeScene4改名Village_HomeScene23`
- `0804/Village_HomeScene1_进屋残缺对齐HomeScene4`
- `0820/Village_HomeScene1_进屋黑屏与未注册`（Object None 型——**本期不是**）
- `0818/第一章村民家室内_IdleWalk_DayLight`（误挂龙宫 Manager）

**Unity**：2020.3.48f1  

---

## ① 结论一句话

**改名必须三位一体对齐为 `Village_HomeScene45`，并新建专用 `Village_HomeScene45SceneManager` + Config；现网「进不去」主因是未进 Build + 村无门指向本场景（`House_Npc45` 仍指缺失的 `Village_House4`）；「进了黑屏/不可玩」主因是误挂龙宫 `HomeScene1Manager`（可能 NRE 取 `HomeScene1Xiaer`）+ 错 Config/地点/右门飞森林——与 0804 HomeScene1 残缺案同型，不是 Object `componentsList` None。**

---

## ② 原因（生活类比）

| 动作 | 类比 |
|------|------|
| 3 → 45 改名 | 门牌从「3 号」换成「45 号」；户口本、派出所登记、回村路标必须同一张 |
| 现网屋内 | 房本仍写「龙宫」、派出所没登记这套房、村口那扇写着「45」的门却开向**不存在的 House4** |
| 黑屏 | 进错派出所流程时去关「夏尔」开关——屋里根本没有此人 → 总闸跳掉 |

只改文件名不改字符串 → 进不去；只改名仍挂龙宫 Manager → 进了也黑/身份错。

---

## ③ 用户需要做什么（检查清单）

> 施工前对照勾选；**改名项**与**可玩/黑屏项**分开。

### A. 可玩身份 / 黑屏（建议先做或与改名同批）

| # | 项 | 现网 | 目标 |
|---|-----|------|------|
| A1 | 场景脚本 | 龙宫 `HomeScene1Manager`（guid `9843b6b6…`） | **新建** `Village_HomeScene45SceneManager`（仿 `Village_HomeScene1SceneManager`） |
| A2 | Config | 龙宫 `HomeScene1.asset`（guid `d77b8a33…`） | **新建** `Village_HomeScene45.asset`（可复制 `Village_HomeScene1/2` 村屋模板） |
| A3 | `nowSceneName` | 运行时写成 `HomeScene1` | `SceneName.Village_HomeScene45` |
| A4 | `SetNowPlace` | `PlaceName.Home`（龙宫） | `PlaceName.KenMuNi` |
| A5 | Build | **无**本场景 path | 登记 `…/Village_HomeScene45.unity` |
| A6 | 室内右门 | `NextSceneName=ForestScene`（启用） | **`Village_KenMuNi1`**（对齐可玩民居） |
| A7 | 室内左门 | 禁用、Next 空、`componentsList: []` | 保持禁用或按样板；**勿只启用不补 Interactive** |
| A8 | 室内 EnterPos | 仅 `ForestScene` / `HomeScene2` | 补 **`Village_KenMuNi1` → 合适 Born** |
| A9 | 村门 | `House_Npc45` → **`Village_House4`**（场景**不存在**） | → **`Village_HomeScene45`** |
| A10 | 村 EnterPos | 有 `Village_House4`，**无** HomeScene3 | 增加/改 **`lastScene: Village_HomeScene45`** |

### B. 改名三位一体（字符串必须一致）

| # | 项 | 旧 | 新 |
|---|-----|----|-----|
| B1 | 场景文件 | `Village_HomeScene3.unity` | `Village_HomeScene45.unity`（**保留** meta GUID） |
| B2 | `SceneName` 常量 | `Village_HomeScene3` | **替换为** `Village_HomeScene45`（或弃旧加新，白名单勿双挂旧文件名） |
| B3 | DayLight 白名单 | `SceneName.Village_HomeScene3` | → `Village_HomeScene45` |
| B4 | Manager / Config 文件名 | （无专用） | `Village_HomeScene45SceneManager` / `.asset` |
| B5 | 门 / EnterPos / Build | 见上表 | 全部写 **45**，勿残留 3 |

### C. 不要动

龙宫 `HomeScene1` / `HomeScene1Manager`；`Village_HomeScene23`；对话 Prefab（如 `HomeScene1Npc1`——屋里现网 Story 名可另案，**不必因场景改名而改**）。

### D. 验收（施工后）

| # | 通过标准 |
|---|----------|
| 1 | rg 运行时无 `Village_HomeScene3` 场景名残留（除历史文档） |
| 2 | Build 含 `Village_HomeScene45.unity` |
| 3 | 村 `House_Npc45` → 进屋 **不黑屏**；地点肯姆尼；`nowSceneName=Village_HomeScene45` |
| 4 | 出门回村 EnterPos 正确 |
| 5 | DayLight 白名单含 45 |
| 6 | 龙宫 / HomeScene23 不回归 |

---

## ④ 给程序看的补充

### 4.1 三位一体契约（目标）

```
LoadScene("Village_HomeScene45")
  == 文件 Village_HomeScene45.unity
  == SceneName.Village_HomeScene45
  == Village_HomeScene45SceneManager.nowSceneName
  == House_Npc45.NextSceneName
  == 村 EnterPos.lastScene（回程）
  == EditorBuildSettings path
```

`SceneAssetPath` **无别名**：只改文件名不改字符串 → 进不去。

### 4.2 改名影响面（rg）

| 类别 | 现网命中 | 施工动作 | 优先级 |
|------|----------|----------|--------|
| 场景文件 + meta | `Village_HomeScene3.unity` / GUID `1a5fdf21…` | 改名保留 GUID | 运行时必改 |
| `SceneName.cs` | `Village_HomeScene3` 常量 + 注释 | → `Village_HomeScene45` | 必改 |
| DayLight 白名单 | `VillageHomeDayLightAnimApplier` | 同步 45 | 必改 |
| 专用 Manager | **无**（仅有 1/2/23） | **新建** 45 | 必改 |
| Config | 场景挂龙宫 `HomeScene1.asset` | **新建** 村屋 45.asset 并改引用 | 必改 |
| Build | 无 3；有 1/2/23/龙宫1 | 加 45 path | 必改 |
| 村门 | `House_Npc45`→`Village_House4`；**无人**指 HomeScene3 | Next→45 | 必改 |
| 村 EnterPos | `Village_House4` 等；无 3 | 补/改 45 | 必改 |
| 室内门 | Right→`ForestScene` | →`Village_KenMuNi1` | 必改 |
| 文档 / OPEN / 提示词 | 多处写 HomeScene3 | 可后改，列清单 | 文档可后 |
| `Village_House4` 常量/白名单 | 仍存在；场景文件 **0** | 门改指 45 后，House4 可留白名单占位或另案删 | OPEN |

**误伤黑名单**：勿改 `HomeScene1Npc*`、`Village_HomeScene23`、龙宫 `HomeScene1`、门物体名 `House_Npc45`（只改其 Next 字符串）。

### 4.3 Manager / Config 现网 vs 目标

| 项 | 现网（YAML） | 目标 |
|----|--------------|------|
| 脚本 | `HomeScene1Manager` guid `9843b6b62f10aa745916f25edc3fc914` | `Village_HomeScene45SceneManager` |
| config | `d77b8a3360e9a4742b2949e878b95464` = `SceneManagerConfig/HomeScene1.asset` | `Village_HomeScene45.asset` |
| nowSceneName | 代码写死 `SceneName.HomeScene1` | `SceneName.Village_HomeScene45` |
| Place | `PlaceName.Home` | `PlaceName.KenMuNi` |
| 样板 | — | 复制 `Village_HomeScene1SceneManager` 行为（室内脚步、Debug 日志） |

**不能**只改场景文件名却继续挂龙宫 Manager。

### 4.4 进不去 vs 黑屏（分层）

| 层 | 原因 | 证据 |
|----|------|------|
| **进不去** | Build 未登记 | `EditorBuildSettings` 无 `Village_HomeScene3`（有 1/2/23） |
| **进不去** | 村无门指本场景 | 全库 GameRes 无 `NextSceneName: Village_HomeScene3`；`House_Npc45`→`Village_House4`，而 **`Village_House4.unity` 不存在** |
| **进了黑屏** | 龙宫 Manager 取夏尔 | `HomeScene1Manager.OnInit`：`exitTimes==0` 时 `GetSceneEntityLogic<HomeScene1Xiaer>().gameObject`；本场景 **无** Xiaer → **NRE 可断 Init**（同 0804 HomeScene1 残缺） |
| **进了不可玩** | 身份错 | nowSceneName/Place/BGM 龙宫化；右门 `ForestScene`；EnterPos 无村来源 |
| **非主因** | Object None | 本场景 `componentsList` **无** `{fileID:0}` 空槽（≠ HomeScene1 0820） |
| **次要** | 仅 Npc1 + `HomeScene1Npc1` Story | 对话资源另案；不解释整屋黑屏 |

**Play 须核对的日志（开发者未贴 Console 时）**：

1. 是否出现 `未找到该场景实体逻辑HomeScene1Xiaer` + 随后 NRE  
2. 是否 `InitComponents` NRE（预期本期无）  
3. `nowSceneName` / 地点是否变成 Home  
4. LoadScene 失败 / 场景未找到（未进 Build）

### 4.5 推荐施工顺序

1. **新建** `Village_HomeScene45SceneManager` + `Village_HomeScene45.asset`；场景改挂二者；清龙宫引用  
2. 室内：RightDoor→`Village_KenMuNi1`；EnterPos 补村；对齐可玩民居  
3. **改名** 场景文件 3→45（保留 GUID）+ `SceneName` + DayLight + Build path  
4. 村：`House_Npc45.NextSceneName`→`Village_HomeScene45`；EnterPos 补 45（可同时处理旧 `Village_House4` 回程项）  
5. Object/NPC/对话润色 **另案**（除非验收仍黑且日志指向组件）

### 4.6 OPEN

| ID | 问题 | 建议默认 | 状态 |
|----|------|----------|------|
| Q1 | 村哪扇门进 45 号屋？ | **`House_Npc45`**（名已对齐；改 Next 即可） | 待确认 |
| Q2 | 旧档 `LastSceneName=Village_HomeScene3` / `Village_House4`？ | **不双写兼容可接受**（对齐 0804） | 待确认 |
| Q3 | 文档/OPEN 是否同轮把 3 改成 45？ | 运行时先改；文档可后 | 待确认 |
| Q4 | 白名单是否保留 `Village_House4`？ | 门改走后可暂留占位；缺场景另案 | 待确认 |

---

## ⑤ 验收回写（施工后填）

| # | 结果 |
|---|------|
| 运行时无 HomeScene3 残留 | |
| Build 含 45 | |
| House_Npc45 进屋不黑 | |
| 回村 EnterPos | |
| DayLight 含 45 | |
| 龙宫/23 不回归 | |
