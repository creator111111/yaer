# Cursor Agent Prompt · Village_KenMuNi1：`House4 (3)` 改名 `House_Shop` 影响面

> **角色**：【架构侦探】只读溯源——能否直接改 Hierarchy 名、会不会断功能  
> **日期**：2026-08-27  
> **村场景**：`Assets/GameRes/Scenes/Village_KenMuNi1.unity`  
> **目标物体**：`Objects` 下 **`House4 (3)`** → 拟改名 **`House_Shop`**  
> **用户 Hierarchy（截图）**：选中 `House4 (3)`；同级有 `Door_Shop`、`House_NPC2`、`House_Chief`、若干 `House4 (*)` / `House_Npc*`；另有 `EnterFrom_Shop`  
> **本阶段**：只读；**禁止改名、禁止改场景/代码**

把下面「侦探」整段复制给 Cursor Agent（Agent Mode）执行。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 产品需求（白话）

> 村里 `Objects` 下有个叫 **`House4 (3)`** 的物体，想改成 **`House_Shop`**，跟旁边的 `Door_Shop` 命名风格对齐。先查：**能不能直接改？改了会不会影响进屋 / 进店 / 落点 / GSM 登记？**

### 命名通则（钉死）

```
Hierarchy 物体名（House_xxx / Door_xxx）
  ≠ NextSceneName（目标场景字符串）
  ≠ SceneName 常量
  ≠ EnterPosConfig.lastScene

换场靠的是：
  SceneChangeDoor.NextSceneName  +  sceneObjs 引用(fileID)  +  双侧 EnterPos
不是靠「物体叫什么」。
```

**推论（待证伪）**：多数情况下 **只改 `m_Name` 不影响换场**；但若有 `GameObject.Find("House4 (3)")` / `transform.Find` / 动画路径 / 文档外硬编码名字符串，则会断。

### 磁盘预扫：目标物体 vs 真·商店门（2026-08-27）

| 检查项 | **`House4 (3)`**（拟改名） | **`Door_Shop`**（已有商店门） |
|--------|---------------------------|-------------------------------|
| 父节点 | `Objects`（Stairs 实例，PrefabInstance `&2112093712`） | `Objects`（Stairs 实例，PrefabInstance `&1457861539`） |
| `m_Name` | `House4 (3)` | `Door_Shop` |
| `NextSceneName`（YAML） | **`Village_House4`** | **`Village_Shop`** |
| 产品语义（预扫） | 更像 **House4 系户门/房子实例**，不是进店主路径 | 0713 已拍板：**进店真源** → 纯 UI `Village_Shop` |
| 回村落点 | 与 `Village_House4` / `Village_HomeScene23` 链相关？ | `EnterFrom_Shop` + `EnterPos` `lastScene: Village_Shop` |
| 代码硬编码名 | Scripts 中 **未搜到** `"House4 (3)"` | 注释/文档大量提 `Door_Shop`；Find 名待侦探复核 |

**预扫推论（侦探必须裁定，勿直接当结论）**：

1. **改名本身**很可能只是 Hierarchy 可读性改动，**不自动改** `NextSceneName`。  
2. 改完后物体叫 `House_Shop`，但若仍指向 `Village_House4`，会和旁边真正的 `Door_Shop→Village_Shop` **语义撞车**——报告须明确提醒策划/开发：**名字像商店，链路却是 House4**。  
3. 若产品意图是「这个物体就是商店建筑/门」，侦探须写清：改名 ≠ 接商店；接商店要另改 `NextSceneName` / sceneObjs / EnterPos（**本需求默认不做换场改造**，除非用户确认）。

### 勿混物体（Hierarchy 同级）

| 名字 | 勿当成 |
|------|--------|
| `Door_Shop` | 已是进 `Village_Shop` 的门；**不是**本次改名目标 |
| `House4 (4)/(5)/(6)` | 其它 House4 实例；可能同指 `Village_House4` |
| `House_Npc*` / `House_NPC2` / `House_Chief` | 其它户门；命名样板可参考，勿误改 |
| `EnterFrom_Shop` | 离店回村落点空物体；**改 `House4 (3)` 名不应动它** |

### 侦探须填的「改名安全七件套」

| # | 检查项 | 通过标准 |
|---|--------|----------|
| 1 | 磁盘 YAML 存在 `m_Name: House4 (3)` | 已保存场景 |
| 2 | `NextSceneName` 现值 | 记录；确认改名**不会**改此字段 |
| 3 | `SceneEntity` / `sceneObjs` | 靠 **fileID 引用**还是靠名字？ |
| 4 | 全仓字符串 `"House4 (3)"` | 代码 / Prefab / Timeline / Animator / Doc |
| 5 | `GameObject.Find` / `transform.Find` / 按名过滤 | 无命中则可改 |
| 6 | Prefab 覆盖层 | Stairs 实例 `propertyPath: m_Name` 只改 override |
| 7 | 与 `Door_Shop` / `House_Shop` 新名冲突 | 场景内是否已有同名 GO |

### 须比较的方案

| 方案 | 说明 | 预判 |
|------|------|------|
| **A（推荐若仅可读性）** | Hierarchy 把 `House4 (3)` → `House_Shop`；**不动** NextSceneName / EnterPos / Door_Shop | 低风险，但名实可能不符 |
| B | 改名为更中性名（如 `House4_ShopArea` / `House_NearShop`）避免与 `Door_Shop` 混淆 | 若产品只要「好看名字」且物体仍进 House4 |
| C | 改名 + 把 NextSceneName 改成 `Village_Shop` | **超出本需求**；等于抢 Door_Shop 职责，须另开施工单 |
| D | 不改名 | 功能零风险 |

### 严禁（本阶段）

- 直接在编辑器/YAML 里改名或改 `NextSceneName`  
- 把 `Door_Shop` 改掉或删掉  
- 把 `House4 (3)` 的 `NextSceneName` 悄悄改成 `Village_Shop`（与本需求「只改物体名」不符）  
- 因改名去动 `EnterFrom_Shop` / `Village_Shop` GSM  
- 把其它 `House4 (*)` 一并改名

---

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/02_SYSTEM_SPEC.md
@Assets/Doc/技术文档/场景相关/场景切换.md
@Assets/Doc/执行文档/7月/0713/Village_KenMuNi1_Door_Shop换场Village_Shop纯UI_架构溯源与施工执行说明.md
@Assets/Doc/提示词/0822/Village_KenMuNi1_House_NPC2与村长门无法进屋_架构侦探提示词.md
@Assets/Scripts/Game/Static/Name/Res/SceneName.cs
@Assets/Scripts/Game/GameRuntime/Entities/SceneEntities/CommonEntity/SceneChangeDoor.cs
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Scene/Village_Shop/Village_ShopSceneManager.cs
@Assets/Prefabs/Stairs.prefab
@Assets/GameRes/Scenes/Village_KenMuNi1.unity
@Assets/GameRes/Scenes/Village_Shop.unity
@Assets/GameRes/Scenes/Village_House4.unity
@ProjectSettings/EditorBuildSettings.asset

你现在是【架构侦探】。Unity 2020.3.48f1。
禁止改场景/代码/Prefab/Config。只读 + 写「改名影响面」溯源报告。

---

## 背景

用户想把 `Village_KenMuNi1` → `Objects` → **`House4 (3)`** 改名为 **`House_Shop`**。
旁边已有 **`Door_Shop`**（进 `Village_Shop`）与 **`EnterFrom_Shop`**。
需裁定：只改 Hierarchy 名是否安全；会不会影响换场/交互/GSM；名实是否冲突。

---

## 侦探任务清单

### A. 锁定目标物体（先确认场景已保存）

| 项 | 填 |
|----|-----|
| Hierarchy 路径 | `Objects/House4 (3)`？ |
| Prefab | Stairs？guid？ |
| PrefabInstance fileID | 预扫 `&2112093712`？ |
| Active / 组件 | SceneChangeDoor？ Interactive？ Collider？ |
| `NextSceneName` | 预扫为 `Village_House4`——核实 |
| 是否在 `sceneObjs` | fileID 引用？ |

### B. 全仓「旧名」引用扫描（必做）

至少搜索：

- `"House4 (3)"`（含空格与括号）
- `House4 (3)` 无引号变体
- 路径片段 `Objects/House4`

输出表：

| 位置（文件:行） | 类型（代码Find/YAML名/文档/其它） | 改名后是否必改 | 风险 |
|-----------------|-----------------------------------|----------------|------|

### C. 换场链路是否依赖物体名

对照样板 `Door_Shop` / `House_Npc1`：

| 环节 | 依赖物体名？ | 依赖什么 |
|------|--------------|----------|
| SceneChangeDoor 进门 | | NextSceneName / 组件 |
| SceneEntityComponentGSM | | sceneObjs 引用？ |
| EnterPos 回程 | | lastScene 字符串 + Transform 引用？ |
| Interactive 按 E | | 组件链 |

**结论句**：改 `m_Name` 后，进屋/进店链路 **断 / 不断**。

### D. 与 `Door_Shop` 名实冲突裁定（必填）

| 问题 | 裁定 |
|------|------|
| `House4 (3)` 当前进哪个场景？ | |
| `Door_Shop` 当前进哪个场景？ | |
| 改成 `House_Shop` 后，测试/策划是否会误以为它进商店？ | |
| 场景内是否已有名为 `House_Shop` 的物体？ | |
| 推荐方案 | **A / B / C / D**（见预梳理）及一句话理由 |

### E. 若拍板「可以改」——最小施工清单（给施工员，本阶段不执行）

| # | 动作 | 是否必须 |
|---|------|----------|
| 1 | 改 PrefabInstance `m_Name` override → `House_Shop` | |
| 2 | 改任何 Find/字符串引用 | 有则必须 |
| 3 | 改 NextSceneName | **默认否** |
| 4 | 改 Door_Shop / EnterFrom_Shop | **默认否** |
| 5 | 保存场景 + 冒烟验收 | |

### F. 验收清单（改名后冒烟，供施工/验收员）

| # | 操作 | 通过 |
|---|------|------|
| 1 | Hierarchy 见 `House_Shop`，无重复名 | |
| 2 | 靠近该物体：交互/换场行为与改名前一致 | |
| 3 | 靠近 `Door_Shop`：仍进 `Village_Shop` | |
| 4 | 离店回村：仍落 `EnterFrom_Shop` | |
| 5 | Console 无缺组件 / LoadScene 失败 / NullRef（Find 名） | |

### G. 开放问题

写入报告末「开放问题」；必要时记 `OPEN_QUESTIONS.md`：

- 产品是否其实想让该物体改指 `Village_Shop`（与 Door_Shop 合并/替换）？  
- 若仍指 `Village_House4`，是否改用不易误解的名字？

---

## 输出要求

写入：`Assets/Doc/执行文档/0827/Village_KenMuNi1_House4(3)改名House_Shop_架构溯源报告.md`

结构（对齐 MASTER）：

① 结论一句话（能不能直接改 + 最大风险）  
② 原因（通俗：名字≠换场目标；与 Door_Shop 关系）  
③ 用户检查清单  
④ 给程序：引用表 + 推荐方案 A/B/C/D + 施工步骤（最小）

口头汇报同样用 MASTER 四段式。
```

---

## 施工员续跑（侦探报告拍板后再贴）

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0827/Village_KenMuNi1_House4(3)改名House_Shop_架构溯源报告.md
@Assets/GameRes/Scenes/Village_KenMuNi1.unity

你现在是【施工员】。仅按报告「可改」项执行 Hierarchy 改名。

必须遵守：
- 默认只改 `House4 (3)` → `House_Shop`（PrefabInstance m_Name）；
- 禁止改 NextSceneName、Door_Shop、EnterFrom_Shop、Village_Shop GSM，除非报告明确要求；
- 若报告发现 Find/字符串引用，一并最小修改；
- 改完保存场景；按报告验收表冒烟。

提交说明：改了哪些文件、冒烟结果、是否名实不符遗留。
```
