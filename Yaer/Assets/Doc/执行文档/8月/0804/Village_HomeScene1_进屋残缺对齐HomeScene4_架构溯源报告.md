# Village_HomeScene1 进屋残缺对齐 HomeScene4 — 架构溯源报告

**文档版本**：v1.1（2026-08-04）  
**文档性质**：【架构侦探】盘点 + **【施工已完成】**  
**范围**：`House_Npc1` → `Village_HomeScene1` 进不去 / 进了不可玩；对照 **HomeScene4 可玩清单**（现网场景名已更名为 `Village_HomeScene23`）与样板 `Village_HomeScene2`  

> **施工结果（2026-08-04）**：已按 OPEN 默认拍板交付专用 `Village_HomeScene1SceneManager` + Config、Build、右门→村、双侧 EnterPos；**未改**龙宫 `HomeScene1` / `HomeScene1Manager`。下文保留侦探原文。

**Unity**：2020.3.48f1  

---

## ① 结论一句话

**村门牌写对了（`House_Npc1` → `Village_HomeScene1`），屋里是龙宫 HomeScene1 毛坯拷贝：挂错 Manager/Config、无 SceneName/无 Build、EnterPos 无村来源、右门仍飞 `ForestScene`、村回程表缺本场景。Editor 偶发能「加载进场景」但身份错 + 出门坏；正式包几乎必失败。与 HomeScene4 修复前同型，应按专用 `Village_HomeScene1*` 最小补齐，禁止改龙宫 `HomeScene1Manager` 将就。**

组合：**加载侧** Build 未登记（正式包进不去）+ **可玩侧** Manager/落点/出门/回程残缺（进了也不可玩）。

---

## ② 原因（生活类比）

门牌钉的是「村 1 号屋」，钥匙也能插上；推开门却发现屋里挂着「龙宫房本」、户口本没登记这套房、门口没贴纸、后门通向森林——人要么被派出所拦在门外（Build），要么进了屋站错位置、一出门飞去森林、地图地点变成「家」。

### 村门侧（入口字符串）

| 项 | 核实 |
|----|------|
| `House_Npc1` | Prefab 实例；`NextSceneName = Village_HomeScene1` |
| 场景文件 | **存在** `Assets/GameRes/Scenes/Village_HomeScene1.unity` |
| 结论 | **进门目标名对**；问题在目标场景未交付齐 |

### G1–G8 核实（相对提示词预扫）

| ID | 预梳理 | 核实 |
|----|--------|------|
| G1 | `SceneName` 无 `Village_HomeScene1` | **证实**；仅有龙宫 `HomeScene1`；村侧有 `Village_HomeScene2` / `Village_HomeScene23`（曾用名 4） |
| G2 | Build 无本场景 | **证实**；Build 有 `HomeScene1.unity`（龙宫）、`Village_HomeScene2`、仍登记旧 path `Village_HomeScene23.unity`（磁盘已是 23），**无** `Village_HomeScene1` |
| G3 | 挂 `HomeScene1Manager` | **证实**（script guid `9843b6b6…`）→ `nowSceneName = HomeScene1`；`PlaceName.Home`；可能播龙宫 BGM |
| G4 | config → `HomeScene1.asset` | **证实**（guid `d77b8a33…`）；`canCreatePlayer=1`（**不是**禁生成，但是错资产身份） |
| G5 | EnterPos 无 `Village_KenMuNi1` | **证实**：仅 `ForestScene`→RightBorn、`HomeScene2`（pos 空） |
| G6 | 门 Next=`ForestScene` | **证实**：属 **RightDoor**（启用）；**LeftDoor** Next 空且 `SceneChangeDoor` **Enabled=0** |
| G7 | 村回程无 `Village_HomeScene1` | **证实**：村表有 House4 / HomeScene4 / HomeScene2 / Shop… **无** HomeScene1 |
| G8 | 无专用 Manager/Config | **证实**：无 `Village_HomeScene1SceneManager`、无 `Village_HomeScene1.asset` |

### 额外高危（比 HomeScene4 更「龙宫化」）

`HomeScene1Manager.OnInit` 在 `SelectClothesSceneData.exitTimes == 0` 时会：

```csharp
GetSceneEntityLogic<HomeScene1Xiaer>().gameObject.SetActive(false);
```

本场景 **无** `HomeScene1Xiaer`（`sceneObjs` 仅挂 Npc1 + 若干空槽）。`GetSceneEntityLogic` 返回 null 后取 `.gameObject` → **NRE，可中断 SceneManager 初始化**。  
换装已完成的存档会跳过该分支，体感变成「能进但地点/出门错」；新档/未换装更易直接崩黑。

### 玩家会不会生成？

| 检查 | 结果 |
|------|------|
| `canCreatePlayer` | **1**（绑的是龙宫 Config，但仍允许创建） |
| EnterPos 从村 | **未命中** → `DefaultBornPos` `(-24.12, -3.65)` |
| RightBorn（样板进门侧） | `(24.68, -3.65)` —— 与 HomeScene2 同高；HomeScene23 后来把进门 Born 调到约 Y=-1.3～-2.2，**验收时贴地核对** |

→ 主因不是「禁止创建玩家」，而是身份错 + 落点兜底 + 出门/回程坏 + Build。

---

## ③ 用户需要做什么

### 拍板（OPEN）

1. **出门主链用右门还是左门？**  
   - **默认建议：右门**（现已启用且有 Interactive；对齐 HomeScene23「右门回村、左门关」）  
   - 左门现 `SceneChangeDoor` 关、且 `componentsList` 空——若改开左门须先补 Interactive 三件套，否则重蹈 HomeScene4 黑屏  
2. **进门 Born**：`Village_KenMuNi1` → **RightBorn**（近右门）还是 LeftBorn？默认 **RightBorn**（与 23 一致）  
3. **Born Y**：先用现坐标验收；若飘空/入地再对齐 Ground（参考 23 的 Y≈-1.3～-2.2）  
4. **确认不改龙宫** `HomeScene1` / `HomeScene1Manager` / 龙宫存档 Data  

### 验收（施工后）

1. InitScene → 村 → 踩 `House_Npc1` → 进入 `Village_HomeScene1`  
2. Hierarchy 有 Player，可见可走；地点为肯姆尼（非「家」）  
3. Console 无「场景未找到」/ 无 Xiaer NRE / 无 Interactive 缺失抛崩  
4. 主出口回 `Village_KenMuNi1`，落在 `House_Npc1` 门外  
5. 龙宫 `HomeScene1` 从开局进仍正常（未误伤）

---

## ④ 给程序看的补充

### 4.1 与龙宫 `HomeScene1` 隔离表（严禁混修）

| 项 | 龙宫 `HomeScene1` | 村屋 `Village_HomeScene1`（目标态） |
|----|-------------------|-------------------------------------|
| 文件 | `Scenes/HomeScene1.unity` | `Scenes/Village_HomeScene1.unity` |
| `SceneName` | `HomeScene1` | **`Village_HomeScene1`（待加）** |
| Manager | `HomeScene1Manager` | **`Village_HomeScene1SceneManager`（待建）** |
| Config | `HomeScene1.asset` | **`Village_HomeScene1.asset`（待建）** |
| Build | **已有** | **待加**（勿改龙宫条目） |
| `nowSceneName` | `HomeScene1` | 必须 = 文件名 |
| `PlaceName` | `Home` | `KenMuNi` |
| 存档 Data | `HomeScene1Data` / 开场剧情 | **不要**共用；村屋 Manager 不调 Xiaer / FirstEnter |
| 村门 | — | `House_Npc1` → `Village_HomeScene1`（已配，保留） |

**禁止**：把村门改成加载龙宫 `HomeScene1`；禁止改 `HomeScene1Manager` 行为来「兼容」村屋。

### 4.2 Diff 清单（HomeScene2 / HomeScene23 现网 vs 本场景）

| 清单项 | HomeScene2 / 23 现网 | `Village_HomeScene1` 现状 | 阻塞？ |
|--------|----------------------|---------------------------|--------|
| `SceneName` 常量 | 有，=文件名 | **无** | **是** |
| 专用 Manager | `Village_HomeScene2/23SceneManager` | 挂 **龙宫** `HomeScene1Manager` | **是** |
| `nowSceneName` | =文件名 | `HomeScene1` | **是** |
| `PlaceName` | KenMuNi | Home | **是**（体验/存档） |
| 专用 Config | `Village_HomeScene2.asset` 等；`canCreatePlayer=1` | 共用龙宫 `HomeScene1.asset` | **是**（身份） |
| Build | 已登记 | **未登记** | **是**（包体） |
| 进门 EnterPos | `lastScene=Village_KenMuNi1` → 门口 Born | 仅 Forest / HomeScene2 | **是** |
| 出门回村 | 主门 → `Village_KenMuNi1` | 右门 → **`ForestScene`** | **是** |
| 辅门 | 左关（23） | 左关、Next 空 | 暂可；勿盲目启用 |
| 村回程 EnterPos | 有对应 lastScene | **无** `Village_HomeScene1` | **是** |
| Born 贴地 | 2 为 -3.65；23 已调 | Default/Right 均为 **-3.65** | 验收项 |
| Npc | 可玩侧另案 | Npc1 已有 `HomeScene1Npc1` 对话组件 | 本期非阻塞 |
| Xiaer 逻辑 | 无 | Manager 可能 NRE 取龙宫 Xiaer | **条件阻塞** |

> 注：对照文档仍称「HomeScene4」；磁盘/常量现网主身份为 **`Village_HomeScene23`**。样板复制请用 `Village_HomeScene2SceneManager` 或 `Village_HomeScene23SceneManager` 最小室内集。**勿顺带改 HomeScene23 改名残留**（Build 仍写 HomeScene4 等另案）。

### 4.3 最小改动建议（只建议，本阶段不施工）

复制 HomeScene2/23 模式：

1. **新增** `SceneName.Village_HomeScene1 = "Village_HomeScene1"`  
2. **新建** `Village_HomeScene1SceneManager`（抄 2/23：`nowSceneName`、KenMuNi、`IndoorType`；**不要** Xiaer / FirstEnter / 龙宫 BGM）  
3. **新建** Config `Village_HomeScene1.asset`：`canCreatePlayer/canMove=1`，`isFightingScene=0`；场景改绑此 Config + 新 Manager  
4. **Build** 加入 `Village_HomeScene1.unity`  
5. 场景 `EnterPosConfig`：`lastScene=Village_KenMuNi1` → **RightBorn**（拍板可改 Left）  
6. **RightDoor**：`NextSceneName=Village_KenMuNi1`；保持 Interactive；LeftDoor 保持禁用  
7. 村 `EnterPosConfig`：补 `lastScene=Village_HomeScene1` + `House_Npc1` 门外 Transform（可复用同屋门外已有 Born，对齐 NPC2/4 做法）  
8. 验收贴地；必要时调 Born Y  

**不必**：改 `LoadScene` / `InitPlayer` 总管线；改龙宫场景；改 `House_Npc1` 目标名为龙宫。

### 4.4 推荐出门左右门

| 门 | 现状 | 建议 |
|----|------|------|
| **RightDoor** | Active；Door 启用；有 Interactive；Next=`ForestScene` | **主出口**：改 Next=`Village_KenMuNi1` |
| **LeftDoor** | Active 物体；Door **禁用**；Next 空；`componentsList` 空 | **保持禁用**；勿当主链 |

### 4.5 开放问题（已追加 OPEN）

| ID | 问题 | 默认建议 |
|----|------|----------|
| Q1 | 出门主链左/右？ | **右门** |
| Q2 | 从村进门 Born？ | **RightBorn** |
| Q3 | Born Y 是否本轮必调？ | 先验收；飘空再调 |
| Q4 | 是否允许改龙宫 Manager？ | **否** |

---

## 施工员下一轮最小化清单（拍板后）

1. SceneName + Manager.cs + Config.asset  
2. 场景换挂 Manager/Config；EnterPos；RightDoor Next  
3. Build + 村回程 EnterPos  
4. Play：村 ↔ 屋闭环；抽检龙宫 HomeScene1 未伤  
