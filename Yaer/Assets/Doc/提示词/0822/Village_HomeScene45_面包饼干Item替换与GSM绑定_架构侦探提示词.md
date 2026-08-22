# Cursor Agent Prompt · Village_HomeScene45：面包/饼干替换 Item 预制体 + GSM 绑定

> **角色**：先【架构侦探】盘点缺口，报告拍板后【施工员】替换预制体并更新场景管理器登记  
> **日期**：2026-08-22  
> **目标场景**：`Assets/GameRes/Scenes/Village_HomeScene45.unity`  
> **开发者要求**：把 `Object` 下的 **面包 / 饼干**（现为空壳 Sprite）替换成工程 Item 预制体，并更新 **场景管理器（GSM）** 绑定。  
> **预制体路径**：  
> - `Assets/GameRes/Prefabs/Item/面包.prefab`（guid `5eae284707f16754cb8c7faa72639c28`）  
> - `Assets/GameRes/Prefabs/Item/饼干.prefab`（guid `a0b2f1cab9d948c48b282d96c49da283`）  
> **本阶段侦探**：只读、不改场景 / Prefab / 代码

把下面「侦探」整段复制给 Cursor Agent（Agent Mode）执行。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 产品需求（开发者原话）

> 把这个场景里面的面包饼干替换成这个预制体；场景管理器也要修改。

### 现场 Hierarchy（开发者截图）

```
Village_HomeScene45
  Object/
    NPC45          ← 已配置（本期不动）
    面包           ← 红箭头：空壳，要换成 Item 预制体
    饼干
  Map/Design/村民家3合层/
    … 面包、饼干 … ← 美术装饰层，**不是**交互体（勿当替换目标）
```

### 现网磁盘预扫（`Village_HomeScene45.unity`）

| 物体 | 位置（Object 下 local） | 组件 | 结论 |
|------|-------------------------|------|------|
| **面包** | `(-22, 0.635, 7.5)` | 仅 Transform + SpriteRenderer（Layer **0**） | **空壳**，无 SceneEntity / 无对话 |
| **饼干** | 预扫约 `z≈8.46`（Layer **0**） | 同上 | **空壳** |
| **NPC45** | 已齐三件套 | SceneEntity + Interactive + Story=`Village_Npc45` | ✅ 本期不动 |

**Entity → SceneEntityComponentGSM**（场景管理器绑定）：

| 项 | 现网 |
|----|------|
| `objRoot` | 指向 `Object` ✅ |
| `sceneObjs` | **仅 NPC45** 一条（`751582444`） |
| **缺** | 面包 / 饼干 的 `SceneEntity` **未登记** |

`Village_HomeScene45SceneManager.cs`：**无需改 C#**（无 per-item 逻辑）；「改场景管理器」= 更新 **GSM 的 `sceneObjs`** + 确保替换后实体在 `objRoot` 子树内（运行时也会 `GetComponentsInChildren<SceneEntity>` 重扫，但 YAML 漏登记要记）。

### Item 预制体已有什么（施工可复用，勿手搓）

两套 Prefab **已是完整互动物品**（仿 HomeScene1 施工结果）：

| Prefab | StoryPrefabName | 交互方式 | 关键配置 |
|--------|-----------------|----------|----------|
| `面包.prefab` | `Village_Npc1_mianbao` | **远程点击** | `requirePlayerOverlap: 0`；Layer **21** |
| `饼干.prefab` | `Village_Npc1_bingan` | **远程点击** | 同上 |

结构：`SceneEntity` + `SimpleStoryTrigger` + `ComponentSystemMono` + `BaseEntityControll` + `Components/Interactive` + `Clds/Body`（BoxCollider2D + RaycastListener）。

对话 Prefab 磁盘已存在：`Assets/GameRes/Prefabs/Dialogue/Village_Npc1_mianbao.prefab`、`Village_Npc1_bingan.prefab`。

### 生活类比

屋里桌上摆着 **两张印刷画**（Map 合层里的面包饼干）+ **两个空相框**（Object 下只有 Sprite）。要把空相框换成 **带门铃和剧本的真物品预制体**，并在住户登记表（GSM）里写上名字；**不要**把墙上的装饰画当成交互对象，否则会点错或叠两层图。

### 推荐施工步骤（侦探可微调）

1. **记录**现网 `Object/面包`、`Object/饼干` 的 **世界坐标 / localPosition**（替换后摆回同位置）。  
2. **删除** `Object` 下两个空壳 GameObject。  
3. **拖入**（或 PrefabInstance）`Item/面包.prefab`、`Item/饼干.prefab` 到 **`Object` 下**（不是 Map/Design）。  
4. 设回位置；确认 Layer 21、碰撞盖住可点区域。  
5. **Entity.sceneObjs** 追加两个新实例的 `SceneEntity`（与 NPC45 并列）。  
6. **美术层去重（侦探须裁定）**：`村民家3合层` 内同名面包/饼干是否 **Disable SpriteRenderer** 或删子节点，避免与交互体 **叠两张图**。  
7. **勿改** Item 预制体源文件（除非发现 StoryPrefabName 错）；场景侧用实例即可。  
8. **componentsList 禁止 None**（0820 黑屏教训：空槽会 NRE）。

### 与 NPC45 的差异（验收别混）

| 项 | NPC45 | 面包 / 饼干 |
|----|-------|-------------|
| 交互 | 走近出 E | **远处鼠标点** |
| `requirePlayerOverlap` | true | **false**（预制体已设） |
| 根 Z | **0**（近距必须） | 可保留较高 Z（排序用，远程不依赖 overlap） |
| StoryPrefabName | `Village_Npc45` | `Village_Npc1_mianbao` / `_bingan` |

### 严禁

- 只换 Sprite、不挂 Item 预制体（仍无法对话）  
- 把交互预制体挂在 `Map/Design/村民家3合层` 下（不在 `objRoot`）  
- 改 `Village_HomeScene45SceneManager.cs` 将就（除非报告发现缺模块）  
- `componentsList` 留 `fileID: 0` 空槽  
- 误删 NPC45 或破坏 0821 左右门链路

---

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/02_SYSTEM_SPEC.md
@Assets/Doc/OPEN_QUESTIONS.md
@Assets/Doc/执行文档/0820/Village_HomeScene1_Object全量配置与GSM绑定_架构溯源报告.md
@Assets/Doc/执行文档/0820/物品交互对话_远程点击触发_架构溯源报告.md
@Assets/Doc/执行文档/0820/Village_HomeScene1_进屋黑屏与未注册_架构溯源报告.md
@Assets/Doc/执行文档/0822/Village_HomeScene45_NPC45配置与GSM绑定_架构溯源报告.md
@Assets/Doc/执行文档/0821/Village_HomeScene45_LeftDoor无法退出_架构溯源报告.md
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Component/SceneEntityComponentGSM.cs
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Scene/Village_House/Village_HomeScene45SceneManager.cs
@Assets/GameRes/Scenes/Village_HomeScene45.unity
@Assets/GameRes/Prefabs/Item/面包.prefab
@Assets/GameRes/Prefabs/Item/饼干.prefab
@Assets/GameRes/Prefabs/Dialogue/Village_Npc1_mianbao.prefab
@Assets/GameRes/Prefabs/Dialogue/Village_Npc1_bingan.prefab

你现在是【架构侦探】。Unity 2020.3.48f1。
禁止修改场景、Prefab、代码。只读扫描 + 写溯源报告。

---

## 背景

45 号村民家 `Object` 下面包、饼干目前只有一张图，不能点不能播对白。工程里已有配好的 Item 预制体（远程点击 + 对话名已写好）。要换成预制体，并让场景管理器能注册到这两个实体。

---

## 侦探任务清单

### A. Object vs Map 双层面包/饼干

- 列出 `Object` 与 `Map/Design/村民家3合层` 下各有哪些「面包」「饼干」
- 裁定：替换后美术层如何处理（隐藏 / 保留 / 删）

### B. 现网空壳盘点

| 物体 | Transform | 组件齐全？ | SceneEntity？ | 应在 sceneObjs？ |
|------|-----------|------------|---------------|------------------|

### C. Item 预制体对拍

- `StoryPrefabName`、Layer、`requirePlayerOverlap`、`componentsList` 有无 None
- 对话 Prefab 是否存在、能否加载

### D. GSM / 场景管理器

- `objRoot`、`sceneObjs` 现网几条；替换后应有几条（NPC45 + 面包 + 饼干 = 3？）
- `Village_HomeScene45SceneManager.cs` 是否需改（预期：否）

### E. 推荐方案 + 最小改动列表

- Unity Editor 操作步骤（删空壳 → 实例化 Prefab → 调位置 → 登记 sceneObjs）
- 是否需改 Prefab 源（预期：否）

### F. 验收清单

| # | 操作 | 通过 |
|---|------|------|
| 1 | 进屋不黑屏 | Console 无 NRE / 无「未注册=>面包」 |
| 2 | **远处**点面包 | 播 `Village_Npc1_mianbao`，无需走近 |
| 3 | **远处**点饼干 | 播 `Village_Npc1_bingan` |
| 4 | 走近 NPC45 按 E | 仍正常（不回归） |
| 5 | 左右门出屋 | 仍正常（0821） |
| 6 | 视觉上 | 不叠两层面包/饼干（若叠，记 OPEN） |

### G. 开放问题

写入 `OPEN_QUESTIONS.md`「Village_HomeScene45 面包饼干 Item 替换 · 2026-08-22」。

---

## 输出要求

写入：`Assets/Doc/执行文档/0822/Village_HomeScene45_面包饼干Item替换与GSM绑定_架构溯源报告.md`

报告结构：① 结论一句话 ② 原因 ③ 用户检查清单 ④ 施工步骤 + sceneObjs 条目 + 美术层去重方案

口头汇报用 MASTER 四段式。
```

---

## 施工员续跑（侦探报告拍板后再贴）

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0822/Village_HomeScene45_面包饼干Item替换与GSM绑定_架构溯源报告.md
@Assets/GameRes/Scenes/Village_HomeScene45.unity
@Assets/GameRes/Prefabs/Item/面包.prefab
@Assets/GameRes/Prefabs/Item/饼干.prefab

你现在是【施工员】。按溯源报告做**最小化修改**：

1. 删除 `Object` 下空壳「面包」「饼干」；
2. 实例化 `Item/面包.prefab`、`Item/饼干.prefab` 到 `Object` 下，位置对齐原空壳；
3. `Entity` → `SceneEntityComponentGSM.sceneObjs` 登记 NPC45 + 面包 + 饼干 的 SceneEntity；
4. 按报告处理 `Map/Design` 内装饰层重复_sprite（隐藏或删）；
5. **不改** `Village_HomeScene45SceneManager.cs`、不改 Item 预制体源（除非报告明确要求）；
6. 确认 `componentsList` 无 None。

验收：远处点面包/饼干播对白；进屋不黑屏；NPC45 与出门不回归。

每次提交说明：场景改了什么、sceneObjs 几条、美术层如何处理。
```
