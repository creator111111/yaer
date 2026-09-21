# Cursor Agent Prompt · Village_HomeScene1：物品悬停光标变眼睛

> **角色**：【架构侦探】只读核对这 6 个物品现在缺什么，才能在鼠标放上去时变成眼睛；报告通过后再施工  
> **日期**：2026-09-20  
> **场景**：`Assets/GameRes/Scenes/Village_HomeScene1.unity`  
> **物体（用户红线圈出，都在 `Object` 下）**：饼干、面包、木桶、米袋、木箱、土豆  
> **产品期望（钉死）**：鼠标移到这些可交互物体上，光标变成**眼睛**；移开恢复。不要新画第五种光标  
> **不是**：改 `Npc1`（人，不在红线里）；改点击后的对白；改成抓取手或对话气泡  
> **报告落盘**：`Assets/Doc/执行文档/0920/Village_HomeScene1_物品悬停光标变眼睛_架构溯源报告.md`

把下面「侦探」整段交给 Cursor Agent。没写清「每个物体挂在哪、TargetState 填什么、会不会和已有点击盒打架」之前，不要施工。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 词不要混

| 词 | 是什么 | 本期 |
|----|--------|------|
| **眼睛** | 现网光标状态 `View`。贴图是睁眼 / 眨一下，样例是村外 `StoneBrand` 观察碑 | **要用这个** |
| **手** | `Catch`。商店头、史莱姆、宝箱用 | 不要 |
| **气泡** | `Chat`。可对话 NPC 用，例如民居椅子 | 不要安到这 6 个物品上 |
| **Npc1** | 同场景的人，红线没圈 | 不要改成眼睛 |

游戏已经有光标中枢 `CursorComponentGM`。场景物体悬停走 `CursorChangeTrigger`，进范围登记、离开注销。禁止在业务脚本里直接 `Cursor.SetCursor`，否则对话框开关后光标会卡住。

### 这 6 个物品以前的结论（0820，可能已过期）

`Village_HomeScene1_Object全量配置与GSM绑定` 当时说它们是远程点击物品（`requirePlayerOverlap=false`），要挂 SceneEntity + 点击盒 + 对白。  
那份报告**没写光标**。侦探以场景现网组件为准，不要按 0820 把交互三件套再做一遍，除非现网仍然点不了。

### 嫌疑

| # | 嫌疑 | 怎么验 |
|---|------|--------|
| **A** | 6 个物体都没有 `CursorChangeTrigger`，所以鼠标放上去不变 | 在场景 YAML / Prefab 上逐个看有没有这个组件、`TargetState` 是不是 View |
| **B** | 触发器挂了，但没有能被鼠标碰到的 Collider2D，光标脚本进不去 | 对照 StoneBrand：Trigger 挂在哪个碰撞体上 |
| **C** | 挂在了子物体上，父物体另一层碰撞把鼠标吃掉 | 写清射线先打到谁 |
| **D** | 有的是 Prefab 实例，改场景实例会被 Prefab 覆盖，或改 Prefab 会连到别的场景 | 列出是场景独有还是共用 Prefab |

### 禁止

- 本阶段不改场景、不改 Prefab、不改代码。  
- 不要新做光标图，不要直接 `SetCursor`。  
- 不要把 Npc1 改成眼睛。  
- 设计说不清的写入 `Assets/Doc/OPEN_QUESTIONS.md`。

---

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Project_context.md
@Assets/Doc/02_SYSTEM_SPEC.md
@Assets/Doc/执行文档/8月/0829/Village_Shop_Head悬停光标变化_架构溯源报告.md
@Assets/Doc/执行文档/8月/0820/Village_HomeScene1_Object全量配置与GSM绑定_架构溯源报告.md
@Assets/Scripts/Game/GameMgr/Component/Cursor/CursorChangeTrigger.cs
@Assets/Scripts/Game/GameMgr/Component/Cursor/CursorComponentGM.cs
@Assets/GameRes/Scenes/Village_HomeScene1.unity
@Assets/Doc/OPEN_QUESTIONS.md

你现在是【架构侦探】。Unity 2020.3.48f1 / C#。
禁止修改任何代码、场景、Prefab。只读扫描 + 写溯源报告。

报告写入：
Assets/Doc/执行文档/0920/Village_HomeScene1_物品悬停光标变眼睛_架构溯源报告.md

---

## 背景（策划白话）

Village_HomeScene1 的 Object 下面，用户圈了 6 个可交互物体：
饼干、面包、木桶、米袋、木箱、土豆。

鼠标放到它们上面时，光标应该变成眼睛。移开就恢复。
Npc1 不在这次范围内。

眼睛就是现有的 View 光标（观察碑 StoneBrand 那种），不要新画一张，不要改成手或对话气泡。

---

## 必读 / 优先扫描

光标中枢以 0829 报告 + 现网代码为准：

- 状态：Normal / Catch / View / Chat
- 场景悬停组件：`CursorChangeTrigger`，字段 `TargetState`
- 真正改系统光标的只有 `CursorComponentGM`
- 样例：`Village_KenMuNi1` 的 `StoneBrand`（View）

0820 的 HomeScene1 物品配置只说明它们当时要能远程点击，不能当成「现在已经有眼睛光标」。

### A. 六个物体现网各挂了什么

做成表，每行一个物体，写场景里的完整层级名：

1. 有没有 `CursorChangeTrigger`，TargetState 现在是什么
2. 鼠标碰得到的 Collider2D 在哪个子物体上
3. 是场景里手摆的，还是 Prefab 实例。若是 Prefab，写 Prefab 路径，并说明改 Prefab 还会影响哪些场景
4. 点击对白（StoryTrigger）还在不在。本期默认不要拆掉点击

缺一个算一个，不要写「Object 下的物品」六个字交差。

### B. 眼睛光标的正确挂法

对照 StoneBrand（或现网另一个已经是 View 的物体）写清：

- 组件挂在碰撞体同一物体，还是挂在根上再引用碰撞体
- 鼠标进入、离开各调谁
- 对话打开时，这颗眼睛会不会盖住对话框自己的光标（优先级）。若会，写明要不要把物品光标优先级放低，不要猜

### C. 推荐一种最小改法

只推一种，并满足：

1. 这 6 个物体，鼠标放上变 View（眼睛），移开恢复
2. 不新增光标贴图，不直接 SetCursor
3. Npc1、其它场景的 Chat/Catch 不变
4. 远程点击这些物品播对白的功能还在

若 6 个里有的已经是 View，表里写「不用改」。
另外两种各用一句话否决（例如「改成 Chat」「在 Update 里射线检测再 SetCursor」）。

---

## 报告结构（固定）

① 结论一句话（缺的是 Trigger 还是碰撞体；改场景还是改 Prefab）  
② 原因（大白话 + 从鼠标移入到光标切换的现成调用链）  
③ 用户需要做什么（Play 后逐个把鼠标放到这 6 个物体上，再移到 Npc1 上看没有被改成眼睛）  
④ 给施工员的补充：每个物体改什么、不要改什么、推荐方案、否决方案

发现设计说不清的，追加到 `Assets/Doc/OPEN_QUESTIONS.md`，不要改核心设计。
```

---

## 施工员提示词（侦探报告落盘并确认方案后再贴）

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0920/Village_HomeScene1_物品悬停光标变眼睛_架构溯源报告.md
@Assets/GameRes/Scenes/Village_HomeScene1.unity
@Assets/Scripts/Game/GameMgr/Component/Cursor/CursorChangeTrigger.cs

你现在是【施工员】。Unity 2020.3.48f1 / C#。
只按溯源报告给这 6 个物体补悬停光标：饼干、面包、木桶、米袋、木箱、土豆。
光标状态用 View（眼睛）。不要新画光标，不要直接 Cursor.SetCursor。

不要改 Npc1。不要拆掉这些物体原来的点击对白。
报告写明不用改的物体保持原样。若报告说改 Prefab 会连到别的场景，就只改 HomeScene1 的实例。

限制：
- 禁止在 Update 里自己射线检测换光标
- 沿用 CursorChangeTrigger + CursorComponentGM
- 施工说明写入：`Assets/Doc/施工说明/0920/Village_HomeScene1_物品悬停光标变眼睛_施工说明.md`
- 没有新架构就不要新造技术文档

完成后用大白话给验收清单：鼠标逐个放到这 6 个物体上应是眼睛，移开恢复；放到 Npc1 上不要变成眼睛。
```
