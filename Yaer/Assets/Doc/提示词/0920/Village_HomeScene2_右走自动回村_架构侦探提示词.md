# Cursor Agent Prompt · Village_HomeScene2：走到右边自动回村（对齐村长家）

> **角色**：【架构侦探】只读核对现网门字段，再给最小改法；报告通过后再施工  
> **日期**：2026-09-20  
> **场景**：`Assets/GameRes/Scenes/Village_HomeScene2.unity`  
> **产品期望（钉死）**：在这家屋里，**走到右边就自动回村庄**，方式和村长家最右边一样（走进触发、黑幕、不要读条、不要按 E）  
> **对照金样**：`Village_Chief_House` 的 `RightDoor`（0920 已施工：Active、`TriggerWhenMoveIn=1`、`ShowLoadingUI=0`、`NextSceneName=Village_KenMuNi1`、`EnterPosKey` 对准村门前）  
> **不是**：改村长家门；改 HomeScene1/23/45；改屋里 NPC 对话；把左门也改成走进触发，除非报告证明必须关左门防双出口  
> **报告落盘**：`Assets/Doc/执行文档/0920/Village_HomeScene2_右走自动回村_架构溯源报告.md`

把下面「侦探」整段交给 Cursor Agent。没写清「改哪扇门的哪几个字段、回村落在哪、会不会一进屋就踩门闪回去」之前，不要施工。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 产品白话

> 村民家 2 现在回村方式和村长家不一样。  
> 要改成：人走到屋子右边，自动回村里，和村长家最右边那扇门一样。

### 现网预扫（HomeScene2 YAML）

| 物体 | Active | SceneChangeDoor | NextSceneName | TriggerWhenMoveIn | ShowLoadingUI | EnterPosKey |
|------|--------|-----------------|---------------|-------------------|---------------|-------------|
| `LeftDoor` | **关**（`m_IsActive: 0`） | 开着 | `Village_KenMuNi1` | **0**（要交互，不是走进） | 0 | 空 |
| `RightDoor` | **关** | **组件 Enabled=0** | **空** | **0** | 0 | 空 |

村里进这家：`Village_KenMuNi1` 上 `House_NPC2`，`NextSceneName=Village_HomeScene2`。  
回村落点表已有一行：`lastScene: Village_HomeScene2` → `ExitFrom_HomeScene2`。

村长家金样（不要照抄坐标，只照抄行为）：

| 字段 | 村长家 RightDoor |
|------|------------------|
| 物体开 | 是 |
| Next | `Village_KenMuNi1` |
| TriggerWhenMoveIn | **1**（走进就走） |
| ShowLoadingUI | **0**（黑幕，不读条） |
| EnterPosKey | `Village_Chief_House_Door`（落到村门前，不和别的门抢） |

### 必须防的坑

HomeScene45 曾经：右门 Trigger 横跨出生点，人一生成就闪回村。  
HomeScene2 的 `RightDoor` 碰撞盒预扫是 **宽 2、高 20**。启用走进触发前，必须确认**室内出生点不在这个盒子里**。

### 禁止

- 本阶段不改场景、不改代码。  
- 不要改村长家。  
- 设计说不清（左门还留不留）写入 `OPEN_QUESTIONS.md`。默认建议：左门保持关掉或保持按 E，**主出口只做右走自动**。

---

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Project_context.md
@Assets/Doc/02_SYSTEM_SPEC.md
@Assets/Doc/技术文档/场景相关/场景切换.md
@Assets/Doc/施工说明/0920/Village_Chief_House_最右边没法回村_施工说明.md
@Assets/Doc/执行文档/0920/Village_Chief_House_最右边没法回村_架构溯源报告.md
@Assets/GameRes/Scenes/Village_HomeScene2.unity
@Assets/GameRes/Scenes/Village_KenMuNi1.unity
@Assets/Doc/OPEN_QUESTIONS.md

你现在是【架构侦探】。Unity 2020.3.48f1 / C#。
禁止修改任何代码、场景、Prefab。只读扫描 + 写溯源报告。

报告写入：
Assets/Doc/执行文档/0920/Village_HomeScene2_右走自动回村_架构溯源报告.md

---

## 背景（策划白话）

Village_HomeScene2 的进出方式要改。
回到村庄：改成走到右边就自动回去，和村长家一样。

村长家：人走到最右边，走进触发，黑幕回村，不读条，落到这家村门前。

这家现在：左门物体是关的，目标虽是村里但要按交互；右门物体关着，换场组件也关着，目标是空的。

---

## 必读 / 优先扫描

### A. 把门和落点钉死

1. `LeftDoor` / `RightDoor` 世界坐标、Collider 范围、是否盖住室内出生点（EnterPos / DefaultBorn）
2. 村里 `House_NPC2` 怎么进这家（走进还是按 E）
3. `ExitFrom_HomeScene2` 世界坐标是不是就在 `House_NPC2` 门外
4. 若右门回村不填 EnterPosKey，会不会和别的 lastScene 抢落点；要不要学村长家加一把专用键（例如仍用 `Village_HomeScene2`，或新键）。写明推荐，避免落到别人家门口

### B. 对齐村长家的字段表

给出施工表（现网值 → 改后值），至少包括：

- RightDoor：Active、SceneChangeDoor 启用、NextSceneName、TriggerWhenMoveIn、ShowLoadingUI、EnterPosKey
- LeftDoor：留关 / 留按 E / 必须禁用防双出口。默认倾向：**右门自动回村即可**；左门不要再开成第二扇走进门，除非现网其实靠左门才能出去且产品要保留

### C. 防一进屋就闪回

用数字证明室内落点是否在 RightDoor 触发盒内。若在，写出先挪门或缩盒再开 Trigger，禁止只开 TriggerWhenMoveIn。

### D. 推荐一种最小改法

只改 HomeScene2 的门（和必要时村里 EnterPos 的键，不改坐标除非落点不在这家门口）。
不要改村长家，不要改其它民居。

另外两种各用一句话否决（例如「改左门当自动出口」「右门开 Loading 读条」）。

---

## 报告结构（固定）

① 结论一句话（开哪扇门、走进回哪）  
② 原因（大白话 + 现网为什么走右边出不去）  
③ 用户需要做什么（进 HomeScene2，走到最右，应黑幕回 House_NPC2 门外；出生点不要立刻被弹出去）  
④ 给施工员的补充：字段表、不要改什么、若触发盒罩住出生点先怎么挪

发现设计说不清的，追加到 `Assets/Doc/OPEN_QUESTIONS.md`，不要改核心设计。
```

---

## 施工员提示词（侦探报告落盘并确认方案后再贴）

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0920/Village_HomeScene2_右走自动回村_架构溯源报告.md
@Assets/Doc/施工说明/0920/Village_Chief_House_最右边没法回村_施工说明.md
@Assets/GameRes/Scenes/Village_HomeScene2.unity
@Assets/GameRes/Scenes/Village_KenMuNi1.unity

你现在是【施工员】。Unity 2020.3.48f1 / C#。
只按报告改 Village_HomeScene2 的回村方式，对齐村长家：走到右边自动黑幕回村，不读条。
不要改村长家，不要改其它民居，不要改 NPC 对话。

若报告说触发盒罩住出生点，先按报告挪门或缩盒，再开 TriggerWhenMoveIn。
回村落点必须在 House_NPC2 门外（ExitFrom_HomeScene2 或报告指定的 Transform）。

施工说明写入：
Assets/Doc/施工说明/0920/Village_HomeScene2_右走自动回村_施工说明.md

完成后用大白话给验收清单：进这家走到最右应回村门口；刚进屋不要立刻被送出去；左门按报告留着或保持关闭。
```
