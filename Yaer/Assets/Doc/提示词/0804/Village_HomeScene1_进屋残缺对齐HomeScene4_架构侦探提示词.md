# Cursor Agent Prompt · House_Npc1 → Village_HomeScene1 无法进屋（疑似场景残缺，对齐 HomeScene4 修复）

> **角色**：先【架构侦探】，报告通过后再定【施工员】方案  
> **日期**：2026-08-04  
> **范围**：从 `Village_KenMuNi1` / **`House_Npc1`** 无法正常进入 **`Village_HomeScene1`**；怀疑与此前 **`Village_HomeScene23` 半成品屋**同类问题。本期按 HomeScene4 已通清单做**同等排查**，并给出最小修复建议。  
> **本阶段**：只溯源、不改代码 / 场景  
> **强对照**：`Village_HomeScene23` 可玩民居修复（0804 进屋报告 + 技术说明）；样板另参 `Village_HomeScene2`

把下面整段复制给 Cursor Agent（Agent Mode）执行。本阶段**只溯源、不改代码**。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 现象

- 村里点 **`House_Npc1`** 门（绿碰撞盒在门口）→ 目标 **`Village_HomeScene1`**  
- **无法进入 / 进了也不可玩**（开发者怀疑场景残缺，同 HomeScene4 当时：无主角、不能动、换场身份错）

### 与龙宫 `HomeScene1` 严禁混淆

| 名字 | 路径 | 用途 |
|------|------|------|
| **`HomeScene1`** | `Assets/GameRes/Scenes/HomeScene1.unity` | 龙宫开局室内；`SceneName.HomeScene1`；`HomeScene1Manager`；**已在 Build** |
| **`Village_HomeScene1`** | `Assets/GameRes/Scenes/Village_HomeScene1.unity` | 肯姆尼村 **Npc1 民居室内**；村门已写此名 |

改修时**禁止**把村屋改成加载龙宫 `HomeScene1`，也禁止误改龙宫 Manager/存档逻辑。

### 静态已扫高危缺口（对照 HomeScene4 修复前）

| ID | 线索 | HomeScene4 修复前同类？ |
|----|------|-------------------------|
| G1 | `SceneName.cs` **无** `Village_HomeScene1`（仅有龙宫 `HomeScene1`） | 有（曾无/错常量） |
| G2 | **Build Settings 无** `Village_HomeScene1.unity`（有的是 `HomeScene1.unity`） | 有 |
| G3 | 场景挂的是 **`HomeScene1Manager`**（guid `9843b6b6…`），`nowSceneName` 必为龙宫名 | HomeScene4 曾挂错/漂移 Manager |
| G4 | `config` 指向 **`HomeScene1.asset`**（龙宫配置） | 曾绑错 Config |
| G5 | `EnterPosConfig` 仅有 `ForestScene` / `HomeScene2`，**无 `Village_KenMuNi1`** → 从村进屋落点走 DefaultBorn | 曾空/错来源 |
| G6 | 门 `NextSceneName: ForestScene`（残留） | HomeScene4 曾指错出口 |
| G7 | 村 `EnterPosConfig` **无** `lastScene: Village_HomeScene1` 回程（现有 HomeScene2/4/Shop…） | 曾缺回村落点 |
| G8 | 无 `Village_HomeScene1SceneManager` / 无专用 Config 资产 | HomeScene4 后已新建专用 |

村门侧：`House_Npc1` → `NextSceneName = Village_HomeScene1`（KenMuNi1 内已配置）——**入口字符串可能对，目标场景未交付齐**。

### 修复对齐清单（来自 HomeScene4 技术说明 · 侦探按此 diff）

可玩民居室内最低闭环：

1. `SceneName.Village_HomeScene1` = 文件名  
2. 专用 `Village_HomeScene1SceneManager`，`nowSceneName` 对齐；`IndoorType`；`PlaceName.KenMuNi`  
3. 专用 Config：`canCreatePlayer=1`，`isFightingScene=0`  
4. Build 登记本场景  
5. `EnterPos`：`lastScene=Village_KenMuNi1` → 门口 Born（注意 Y 贴地，防 HomeScene4 飘空）  
6. 出门主链（左或右，与美术门一致）→ `Village_KenMuNi1`  
7. 村回程：`lastScene=Village_HomeScene1` + `House_Npc1` 门外 Transform  

对照文档：

- `Assets/Doc/技术文档/场景相关/Village_HomeScene23_可玩民居室内_技术说明.md`
- `Assets/Doc/执行文档/0804/Village_HomeScene23_进屋主角不出现_架构溯源报告.md`
- `Assets/Doc/执行文档/6月/0606/Village_HomeScene2_House_Npc2进村换场_施工执行说明.md`
- `Assets/Doc/技术文档/场景相关/场景切换.md`

---

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Project_context.md
@Assets/Doc/02_SYSTEM_SPEC.md
@Assets/Doc/技术文档/场景相关/场景切换.md
@Assets/Doc/技术文档/场景相关/Village_HomeScene23_可玩民居室内_技术说明.md
@Assets/Doc/执行文档/0804/Village_HomeScene23_进屋主角不出现_架构溯源报告.md
@Assets/Doc/执行文档/6月/0606/Village_HomeScene2_House_Npc2进村换场_施工执行说明.md
@Assets/Doc/OPEN_QUESTIONS.md

你现在是【架构侦探】。Unity 2020.3.48f1 / C#。
禁止修改任何代码、Prefab、场景、Build。只读扫描 + 写溯源报告。

---

## 背景（策划白话）

1. 从村里 **`House_Npc1`** 进不了 / 进了不可玩 **`Village_HomeScene1`**。  
2. 怀疑和之前的 **`Village_HomeScene23` 场景残缺**一样。  
3. 截图：门口有交互碰撞（绿框），门本身可能已配目标名。  
4. 目标：按 HomeScene4 已通清单，diff 出 HomeScene1 村屋缺什么；推荐最小修复（新建专用 Manager vs 其它），并与龙宫 `HomeScene1` 划清边界。

---

## 必读 / 优先扫描线索

### A. 换场是否「进得去」
- `House_Npc1`：`SceneChangeDoor`、Active、sceneObjs、`NextSceneName` 是否确为 `Village_HomeScene1`
- `ChangeSceneComponentGM` / Build：加载 `Village_HomeScene1` 失败还是加载成功但黑屏/空场景
- Console：`[SceneChangeDoor]` / `[SceneLoad]` / 场景未找到

### B. 进得去之后是否「可玩」（HomeScene4 同款）
- 挂载的 SceneManager 实际类型（预扫为 `HomeScene1Manager`）与 `nowSceneName`
- `config.canCreatePlayer` / InitPlayer / 相机 Follow
- `EnterPosConfig` 是否含 `Village_KenMuNi1`；Born Y 与 GroundColliders
- 出门 NextSceneName；村回程 EnterPos 是否缺本场景名

### C. 与龙宫 HomeScene1 隔离
- 列出两者文件、Manager、Config、Build、存档 Data 差异  
- 明确：村屋**必须**独立身份，禁止共用 `nowSceneName=HomeScene1`

### D. 与 HomeScene4 / HomeScene2 diff 表（必出）

| 清单项 | HomeScene2/4 现网 | Village_HomeScene1 现状 | 是否阻塞 |
|--------|-------------------|------------------------|----------|
| SceneName 常量 | | | |
| 专用 Manager | | | |
| 专用 Config | | | |
| Build | | | |
| 进门 EnterPos | | | |
| 出门回村 | | | |
| 村回程 EnterPos | | | |
| Born 贴地 | | | |

---

## 侦探任务清单

1. **结论一句话**：进不去是 Build/加载失败，还是进去了但 Manager/落点/生成玩家残缺（可组合）。  
2. **完整缺口表**（相对 HomeScene4 可玩清单）。  
3. **施工员最小改动建议**（只建议）：优先复制 `Village_HomeScene23SceneManager` / Config 模式新建 `Village_HomeScene1*`；禁止改龙宫 `HomeScene1Manager` 行为来「将就」村屋。  
4. **出门用左门还是右门**：按本场景门美术与现 Active 门拍板建议（写入开放问题）。  
5. **验收清单**：InitScene → 村 → House_Npc1 → 进屋有主角可走 → 出门回落 House_Npc1 门外。  
6. **开放问题**追加 OPEN（「Village_HomeScene1 进屋 · 2026-08-04」）。  
7. **禁止**：改资产；把村屋 NextSceneName 改成龙宫 `HomeScene1`；顺带改 HomeScene23 改名任务。

---

## 输出要求

写入：`Assets/Doc/执行文档/0804/Village_HomeScene1_进屋残缺对齐HomeScene4_架构溯源报告.md`

报告结构固定（中文、大白话优先）：

① 结论一句话  
② 原因（生活类比：门牌写对了，屋里还是毛坯/挂错房本）  
③ 用户需要做什么（拍板出门左右门 + 验收）  
④ 给程序看的补充：与龙宫隔离表、diff 清单、最小改动、开放问题

完成后用 MASTER 固定四段式口头汇报；详细以报告文件为准。
```

---

## 施工员续跑（侦探报告拍板后再开）

> 本阶段先不要贴。等确认专用 Manager/Config 与出门左右门后再写。

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0804/Village_HomeScene1_进屋残缺对齐HomeScene4_架构溯源报告.md
@Assets/Doc/技术文档/场景相关/Village_HomeScene23_可玩民居室内_技术说明.md
@Assets/Doc/OPEN_QUESTIONS.md

你现在是【施工员】。按上述溯源报告做**最小化修改**，使 House_Npc1 → Village_HomeScene1 可进屋游玩并回村。
对齐 HomeScene4 可玩民居清单；禁止改龙宫 HomeScene1 / HomeScene1Manager 来顶替村屋身份。
注意 Born 贴地。每次提交说明改动与验收步骤。
```
