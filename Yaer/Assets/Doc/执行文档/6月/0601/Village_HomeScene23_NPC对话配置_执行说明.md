# Village_HomeScene23 — 可对话 NPC 配置执行说明

**文档性质**：架构侦探产出（只读分析 + Unity 侧操作指引，**本阶段不改代码**）  
**依据**：`Assets/Doc/00_MASTER_PROMPT.md`【架构侦探】；`Assets/Doc/02_SYSTEM_SPEC.md`；`Assets/Doc/执行文档/0512/场景切换与对话触发跳转_架构溯源报告.md`  
**目标场景**：`Assets/GameRes/Scenes/Village_HomeScene23.unity`  
**Unity 版本**：2020.3.48f1  

---

## 1. 结论（一句话）

**只把 `NpcXiaer` 拖进场景不够——可对话 NPC 需要「交互碰撞 + 触发脚本 + 对话预制体」三件套；你当前场景里的雅尔已经能点，但挂的是龙宫专用脚本 `HomeScene1Xiaer`，且对话资源路径与程序约定不一致，所以一点击就报「加载资源失败: …HomeScene1XiaerFinally.prefab」。按本文用 `SimpleStoryTrigger` + 放在正确目录的新对话 prefab 即可。**

---

## 2. 你遇到的现象与原因

| 现象 | 原因（大白话） |
|------|----------------|
| 放了 `NpcXiaer` 却没有对话 | `Resources/Object/NpcXiaer.prefab` 主要是**立绘/动画**，没有「点一下播哪段剧情」的逻辑 |
| Console 报 `加载资源失败: Assets/GameRes/Prefabs/Dialogue/HomeScene1XiaerFinally.prefab` | 脚本去加载的路径是 **`Dialogue/文件名.prefab`（根目录）**，而工程里龙宫对话实际在 **`Dialogue/HomeScene1/` 子文件夹**，路径对不上 |
| 以为要在「场景编辑器」里额外登记 NPC | 不需要单独 NPC 编辑器；要在 **Hierarchy 的 `Entity` 节点下** 摆好带组件的物体，并从 **InitScene 正常进游戏** 测（见 §6） |

### 2.1 当前 `Village_HomeScene23` 里已经有什么（静态阅读）

| 物体 | 位置 | 脚本 / 配置 | 说明 |
|------|------|-------------|------|
| `Entity/Npc1` | `Entity` 下 | `SimpleStoryTrigger`，`StoryPrefabName = HomeScene1Npc1` | 通用「点击触发对话」模板，**可照着复制** |
| `Entity/NpcXiaer` | `Entity` 下 | **`HomeScene1Xiaer`**（龙宫专用） | 会根据存档决定播 `HomeScene1GoOutXiaer` 或 `HomeScene1XiaerFinally` → 触发你看到的报错 |
| `SceneManager` | 根节点 | `Village_House4SceneManager` | 场景能跑，但 `nowSceneName` 写的是 `Village_House4`（与场景文件名 `Village_HomeScene23` 不一致），**存档/换场核对时要注意**，与对话报错无直接关系 |

**重要**：`NpcXiaer` 在场景里**并不是**「只拖了个 prefab」——已有 `EntityControl`、`InteractiveComponent`、`Body` 碰撞等；问题在于 **剧情脚本与资源路径**，不是完全没配置。

---

## 3. 可对话 NPC 最小原理（生活类比）

可以把 NPC 想成「自动售货机」：

1. **外壳**：Sprite / Animator（`NpcXiaer` 长什么样）  
2. **感应区**：`InteractiveComponent` + `Body` 上的触发碰撞（玩家靠近、能点）  
3. **按钮逻辑**：`SimpleStoryTrigger` 或专用脚本（决定播哪段剧情）  
4. **货道里的商品**：`GameRes/Prefabs/Dialogue/某名字.prefab`（NodeCanvas 对话图）

缺 2～4 任意一项，都只会看到人、不会出对话框。

### 3.1 程序里的加载规则（给策划核对路径用）

```
玩家点击 NPC
  → SimpleStoryTrigger / HomeScene1Xiaer 等
  → StoryComponentGSM.TriggerStory("某名字")
  → 加载 Assets/GameRes/Prefabs/Dialogue/某名字.prefab
  → 打开对话 UI，播放 NodeCanvas 图
```

**路径必须满足**：`Assets/GameRes/Prefabs/Dialogue/{StoryPrefabName}.prefab`（**不要**多一层 `HomeScene1/` 子目录，除非程序改 `DialoguePath`）。

现有龙宫资源在 `Dialogue/HomeScene1/` 下，与上述规则不一致，所以在本场景直接复用 `HomeScene1XiaerFinally` 会失败。

---

## 4. 推荐做法：在本场景新建「村内雅尔」对话（最简单）

### 4.1 准备对话 prefab（先做内容，再挂 NPC）

| 步骤 | 操作 |
|------|------|
| 1 | 用 **`Tools → Dialogue → Import CSV`** 生成 `.asset`，或复制 `Assets/GameRes/Prefabs/Dialogue/Village_KenMuNiStart.prefab` 改名 |
| 2 | 保存为 **`Assets/GameRes/Prefabs/Dialogue/Village_HomeScene23_Xiaer.prefab`**（名字自定，但记住 **不要** 放进 `HomeScene1` 子文件夹） |
| 3 | 在 prefab 上 **Bind Graph**，Actor 绑「雅尔」「古莎」等（参考 `0525/CSV转NodeCanvas对话树_架构溯源与执行说明.md`） |
| 4 | （可选）先在 **`DialogDebug.unity`** 里拖 prefab 试播，确认字幕/立绘正常（见 `0525/DialogDebug对话测试场景_施工执行说明.md`） |

> **替代方案**：把 `HomeScene1/HomeScene1XiaerFinally.prefab` **复制一份**到 `Dialogue/` 根目录并改名。仅适合临时验证，不推荐长期用龙宫剧情名。

### 4.2 在场景里配置 NPC（复制 `Npc1`，不要只拖 Resources 里的 NpcXiaer）

1. 打开 **`Village_HomeScene23.unity`**。  
2. 在 Hierarchy 找到 **`SceneManager → … → Entity`**（所有可交互实体都应在此节点下）。  
3. **复制** `Entity` 下的 **`Npc1`**（已含 `SimpleStoryTrigger` + 交互结构）。  
4. 重命名为例如 **`NpcXiaer_Village`**，摆到想要的位置。  
5. 在 **`SimpleStoryTrigger`** 上改：  
   - **Story Prefab Name** = `Village_HomeScene23_Xiaer`（与 §4.1 prefab **文件名一致**，不含 `.prefab`）  
   - **Trigger Type** = `Click`（点击触发，与 `Npc1` 相同）  
6. 换立绘：改根物体上的 **SpriteRenderer / Animator**（可从 `NpcXiaer` 上抄外观，或拖 `Resources/Object/NpcXiaer` 当参考再合并到复制体上）。  
7. **删除或禁用** 多余的 **`HomeScene1Xiaer`** 组件（若你从旧 `NpcXiaer` 复制而来）。

### 4.3 交互组件自检（点不到人时查）

选中 NPC 根物体，确认：

| 检查项 | 期望 |
|--------|------|
| `EntityControl`（或 `BaseEntityControll`） | `entityType = NPC`，`canTouchWithPlayer = true` |
| `InteractiveComponent` | `interactiveCollider` 指向 **Body** 的 `BoxCollider2D`；`raycastListeners` 含 Body 上的监听脚本 |
| 子物体 **Body** | 有 **Trigger** 碰撞盒，尺寸能罩住角色 |
| 物体在 **`Entity` 下** | 与 `objRoot` 一致，否则场景实体系统可能不初始化 |

**不要**只从 Project 拖 `Resources/Object/NpcXiaer.prefab` 到场景根——那样通常**没有** `SimpleStoryTrigger` / `SceneEntity` 链路。

### 4.4 若坚持改现有 `Entity/NpcXiaer`

1. **Remove Component** → `HomeScene1Xiaer`。  
2. **Add Component** → `SimpleStoryTrigger`。  
3. **Story Prefab Name** 填你在 §4.1 新建的名字（例如 `Village_HomeScene23_Xiaer`）。  
4. 保存场景。

---

## 5. 验收步骤（Play Mode）

1. 从 **`InitScene`** 启动游戏（不要单独 Open `Village_HomeScene23` 再 Play，否则 `GameManager` / 对话 UI 可能不全）。  
2. 换场或调试进入 **`Village_HomeScene23`**。  
3. 走到 NPC 旁，应出现 **交互键提示**（若项目已开）。  
4. **点击** NPC → 弹出对话 UI，字幕正常。  
5. Console **无** `加载资源失败: Assets/GameRes/Prefabs/Dialogue/...`。  

**失败时对照**：

| Console / 现象 | 处理 |
|----------------|------|
| 加载资源失败 | 检查 prefab 是否在 `GameRes/Prefabs/Dialogue/` **根目录**，名字与 `StoryPrefabName` 一致 |
| 能靠近但没有按键/点不了 | 查 §4.3 `InteractiveComponent`、Body 碰撞、`canTouchWithPlayer` |
| 点了完全没反应 | 是否仍挂着错误的脚本；是否从 InitScene 进入；是否 `StoryComponentGSM` 已有剧情在播 |
| 图播了但没字/没立绘 | 回 DialogDebug 测 prefab；查 Graph 是否 Bind、Actor 是否绑定 |

---

## 6. 常见误区（简短）

| 误区 | 正确理解 |
|------|----------|
| 「拖 NpcXiaer prefab = 可对话 NPC」 | Prefab 多为美术；对话靠 **场景实体 + SimpleStoryTrigger + Dialogue prefab** |
| 「HomeScene1Xiaer 在哪都能用」 | 仅龙宫逻辑 + 存档 `HomeScene1Data`；村内应 **新建剧情名 + SimpleStoryTrigger** |
| 「对话 prefab 放子文件夹也行」 | 当前代码只认 **`Dialogue/{名}.prefab`** 根路径 |
| 「单独 Play 村庄场景就能测全功能」 | 对话依赖全局 UI / `GameManager`，请 **InitScene 进**；纯测图可用 **DialogDebug** |
| 「还要去 Resource Editor 登记 NPC」 | 对话 prefab 若走 AB，打包前需在 Resource Editor 登记**资源路径**；日常 Editor 下只要路径对、InitScene 进即可 |

---

## 7. 与现有文档的关系

| 主题 | 文档 |
|------|------|
| 对话触发总链 | `0512/场景切换与对话触发跳转_架构溯源报告.md` §3 |
| CSV → 对话图 | `0525/CSV转NodeCanvas对话树_架构溯源与执行说明.md` |
| 无 GF 快速试播 | `0525/DialogDebug对话测试场景_施工执行说明.md` |
| 室内场景管理器（同套 Entity 结构来源） | `0530/Village_House4场景管理器_施工执行说明.md` |

---

## 8. 给程序的补充（可选后续，非本文必须）

| 项 | 说明 |
|----|------|
| `DialoguePath` 与 `HomeScene1/` 子目录 | 可统一把龙宫 prefab 移到 `Dialogue/` 根，或扩展 `GetPath` 支持子路径，避免策划踩坑 |
| `Village_HomeScene23` 与 `Village_House4SceneManager` | 场景名与 `nowSceneName` 不一致，换场/存档需单独立项 |
| `SceneName.Village_HomeScene23` | `SceneName.cs` 中若未登记，换场工具链需补常量 |

---

## 9. 文档修订

| 日期 | 说明 |
|------|------|
| 2026-06-01 | 初版：针对 Village_HomeScene23 配置可对话 NPC；说明 HomeScene1Xiaer 与 Dialogue 路径不匹配导致的加载失败 |

**文档路径**：`Assets/Doc/执行文档/0601/Village_HomeScene23_NPC对话配置_执行说明.md`
