# Cursor Agent Prompt · Village_HomeScene3 → Village_HomeScene45 改名 + 专用场景管理器 + 进屋黑屏

> **角色**：【架构侦探】只溯源、不改代码  
> **日期**：2026-08-20  
> **产品目标**：  
> 1. 将场景 **`Village_HomeScene3`** 改名为 **`Village_HomeScene45`**  
> 2. **同时改场景管理器**（须专用村屋 Manager，禁止继续挂龙宫 `HomeScene1Manager`）  
> 3. 查明现网 **进不去 / 黑屏** 原因（与改名同案盘点，可分批施工）  
> **磁盘现状**：`Assets/GameRes/Scenes/Village_HomeScene3.unity`（开发者 Project 红箭头）  
> **先例**：`0804/Village_HomeScene4改名Village_HomeScene23`；`0804/Village_HomeScene1_进屋残缺`；`0820/Village_HomeScene1_进屋黑屏与未注册`  
> **本阶段**：只读 + 写溯源报告，**不施工**

把下面整段复制给 Cursor Agent（Agent Mode）执行。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 开发者问题（白话）

1. 把这个场景改成 **`Village_HomeScene45`**。  
2. 场景管理器也要改。  
3. 现在这个场景也是 **进不去 / 黑屏**——查清原因。

### 改名契约（与 0804 相同「三位一体」）

```
LoadScene("Village_HomeScene45")
  == 文件 Village_HomeScene45.unity
  == SceneName.Village_HomeScene45
  == XxxSceneManager.nowSceneName
  == 村门 NextSceneName（若有门指向本屋）
  == 村 EnterPos.lastScene（回程）
  == EditorBuildSettings path
```

`SceneAssetPath` **无别名表**：只改文件名不改字符串 → 进不去。

| 旧 | 新 |
|----|-----|
| `Village_HomeScene3` | **`Village_HomeScene45`** |

注意：新名是 **`45`**，不是 `4`/`5`/`23`；勿与 `Village_HomeScene2` / `23` / `1` 混淆。

### 预扫结论方向（可证伪）

| 项 | 预判 |
|----|------|
| 场景文件 | ✅ 存在 `Village_HomeScene3.unity` |
| 专用 Manager | ❌ **无** `Village_HomeScene3SceneManager`；场景 YAML 挂的是 **龙宫** `HomeScene1Manager`（guid `9843b6b6…`）+ `HomeScene1.asset`（guid `d77b8a33…`）——与 0818/HomeScene1 残缺案同型 |
| `SceneName.Village_HomeScene3` | ✅ 已有（DayLight 白名单用）；改名后须 **替换/新增** `Village_HomeScene45`，白名单同步 |
| Build | 0818 称「未进 Build / 无门」——侦探再对拍 `EditorBuildSettings` |
| 村门入口 | 搜 `NextSceneName` / `House_*` 是否已有指向 `Village_HomeScene3`；若无，改名后仍要规划谁进这屋（OPEN） |
| 黑屏 | 可能叠加：① 错 Manager 身份（龙宫逻辑 NRE/错 Place）；② Object 组件 None（同 HomeScene1 0820）；③ Build/换场断链。须用 Console/堆栈钉死主因 |
| 保留勿改 | 龙宫 `HomeScene1`/`HomeScene1Manager`；`Village_HomeScene23`；对话 Prefab 名含 Npc3 的不必因场景改名而改 |

生活类比：门牌要从「3 号屋」换成「45 号屋」；屋里房本还写着「龙宫」→ 派出所不认、灯也可能整栋不亮。先查清房本和跳闸点，再统一换门牌。

### 必读

- `Assets/Doc/执行文档/0804/Village_HomeScene4改名Village_HomeScene23_架构溯源报告.md`
- `Assets/Doc/执行文档/0804/Village_HomeScene1_进屋残缺对齐HomeScene4_架构溯源报告.md`
- `Assets/Doc/执行文档/0820/Village_HomeScene1_进屋黑屏与未注册_架构溯源报告.md`
- `Assets/Doc/执行文档/0818/第一章村民家室内_IdleWalk_DayLight_架构溯源报告.md`（HomeScene3 误挂龙宫 Manager）
- `SceneName.cs`、`VillageHomeDayLightAnimApplier.cs`
- `Village_HomeScene1SceneManager.cs` / `Village_HomeScene23SceneManager.cs`（专用 Manager 样板）
- 场景：`Village_HomeScene3.unity`（Manager/Config guid、Object、门）
- `EditorBuildSettings.asset`、村 `Village_KenMuNi1` 门与 EnterPos

---

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Project_context.md
@Assets/Doc/02_SYSTEM_SPEC.md
@Assets/Doc/OPEN_QUESTIONS.md
@Assets/Doc/执行文档/0804/Village_HomeScene4改名Village_HomeScene23_架构溯源报告.md
@Assets/Doc/执行文档/0804/Village_HomeScene1_进屋残缺对齐HomeScene4_架构溯源报告.md
@Assets/Doc/执行文档/0820/Village_HomeScene1_进屋黑屏与未注册_架构溯源报告.md
@Assets/Doc/执行文档/0818/第一章村民家室内_IdleWalk_DayLight_架构溯源报告.md
@Assets/Scripts/Game/Static/Name/Res/SceneName.cs
@Assets/Scripts/Game/GameRuntime/Entities/Player/Components/VillageHomeDayLightAnimApplier.cs
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Scene/Village_House/Village_HomeScene1SceneManager.cs
@Assets/GameRes/Scenes/Village_HomeScene3.unity
@Assets/ProjectSettings/EditorBuildSettings.asset

你现在是【架构侦探】。Unity 2020.3.48f1 / C#。
禁止修改场景、代码、Build。只读扫描 + 写溯源报告。

---

## 背景

1. 开发者要把 `Village_HomeScene3` 改名为 `Village_HomeScene45`，并改场景管理器。
2. 现网该场景进不去 / 黑屏，须一并查明。
3. 对齐 0804 改名三位一体 + HomeScene1 专用 Manager 样板；禁止改龙宫将就。
4. 本期只出影响面、黑屏根因、施工顺序建议。

---

## 必查

### A. 改名影响面（rg 全库）

搜并列表（运行时必改 vs 文档可后改）：

| 类别 | 旧字符串/资产 | 新应对 |
|------|---------------|--------|
| 场景文件 + meta GUID | Village_HomeScene3.unity | 改名保留 GUID |
| SceneName 常量 | Village_HomeScene3 | Village_HomeScene45 |
| DayLight 白名单 | 同上 | 同步 |
| 新建 Manager + .meta | （现无） | Village_HomeScene45SceneManager |
| 新建/改 Config.asset | 勿继续引用龙宫 HomeScene1.asset | Village_HomeScene45.asset |
| Build Settings | path | 新 path；旧 path 删或改 |
| 村门 NextSceneName | 谁指向 HomeScene3？ | → 45 |
| 村 EnterPos lastScene | 回程表 | 补/改 45 |
| 室内门回村 | RightDoor/LeftDoor | NextSceneName=Village_KenMuNi1？ |
| PlaceName / SetNowPlace | | |
| 文档 / OPEN / 注释 | | 可后改，列清单 |

误伤黑名单：勿改 `HomeScene1Npc*` 对话、`Village_HomeScene23`、龙宫 `HomeScene1`。

### B. 场景管理器现状 vs 目标

| 项 | 现网（对拍 YAML） | 目标 |
|----|-------------------|------|
| 脚本 | HomeScene1Manager？ | **新建** Village_HomeScene45SceneManager |
| nowSceneName | HomeScene1？ | Village_HomeScene45 |
| config | HomeScene1.asset？ | 村屋专用 Config（可复制 HomeScene2/1 村屋模板） |
| canCreatePlayer / BGM / Place | | 对齐可玩民居 |

明确：**不能**只改场景文件名却继续挂龙宫 Manager。

### C. 黑屏 / 进不去（与改名拆清，但同报告）

分两层填表：

| 层 | 可能原因 | 证据 |
|----|----------|------|
| 进不去 | Build 未登记；村无门；NextSceneName 错 | |
| 进了黑屏 | 错 Manager 龙宫逻辑；InitComponents NRE；未注册；黑幕未关；相机 | Console/堆栈/对照 0820 |

若开发者未贴本次 Console：侦探写「静态最可能主因」+「进 Play 须核对的日志清单」。

对照样板可玩屋：`Village_HomeScene2` / `Village_HomeScene1`（已修专用 Manager 后）。

### D. 推荐施工顺序（只建议）

1. 先修可玩身份（专用 Manager + Config + Build + 门/EnterPos）——否则改名也黑  
2. 再三位一体改名为 45（或同批改，但清单要完整）  
3. Object/NPC 交互另案（除非黑屏就是 Object None）

### E. 验收（施工后）

| # | 操作 | 通过 |
|---|------|------|
| 1 | rg 运行时无 `Village_HomeScene3` 场景名残留（除历史文档） | |
| 2 | Build 含 `Village_HomeScene45.unity` | |
| 3 | 从村进该屋 | **不黑屏**；nowSceneName/地点正确 |
| 4 | 出门回村 | EnterPos 正确 |
| 5 | DayLight 白名单含 45 | |
| 6 | 龙宫 HomeScene1、HomeScene23 不回归 | |

---

## 侦探任务

1. **结论一句话**：改名必做三位一体 + 专用 Manager；黑屏主因是什么。  
2. **改名影响面表**（必改文件列表）。  
3. **Manager/Config 现网 vs 目标**。  
4. **黑屏/进不去因果**（对照 HomeScene1/23）。  
5. **施工顺序 + 验收**。  
6. OPEN：村哪扇门进 45 号屋；旧档 LastSceneName=HomeScene3 是否兼容；文档是否同轮改。  
7. **禁止**：改资产；用龙宫 Manager 顶村屋；只改文件名不改字符串；动 HomeScene23/龙宫。

---

## 输出

写入：`Assets/Doc/执行文档/0820/Village_HomeScene3改名45与进屋黑屏_架构溯源报告.md`

① 结论一句话  
② 原因（生活类比：换门牌 + 换房本；灯不亮先查跳闸）  
③ 用户检查清单（改名项 / 黑屏项分开勾）  
④ 程序：三位一体表、rg 命中、Manager guid 证据、黑屏对照、OPEN  

MASTER 四段式口头汇报。
```

---

## 施工员续跑（报告拍板后再贴）

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0820/Village_HomeScene3改名45与进屋黑屏_架构溯源报告.md
@Assets/Doc/OPEN_QUESTIONS.md

你现在是【施工员】。按报告将 Village_HomeScene3 改名为 Village_HomeScene45，并换成专用村屋场景管理器；同时消除进屋黑屏/进不去。

必须：场景文件名 = SceneName = nowSceneName = Build path = 门/EnterPos 字符串一致；禁止继续挂龙宫 HomeScene1Manager；保留场景/脚本 meta GUID 若报告要求；不改龙宫与 HomeScene23。

提交说明：改了哪些引用、新 Manager/Config 名、进屋是否不黑、回村是否正常。
```
