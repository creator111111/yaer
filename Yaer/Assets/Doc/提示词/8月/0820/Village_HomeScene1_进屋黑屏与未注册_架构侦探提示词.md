# Cursor Agent Prompt · Village_HomeScene1 进屋黑屏 + 场景对象未注册

> **角色**：【架构侦探】只溯源、不改代码  
> **日期**：2026-08-20  
> **场景**：`Assets/GameRes/Scenes/Village_HomeScene1.unity`  
> **测试现象（开发者已测）**：进该场景 **Game 黑屏**；Console 显示场景对象**未到场景管理器注册并初始化**；另有 **`NullReferenceException` @ `ComponentSystem.InitComponents`**。  
> **本阶段**：只读扫描 + 写溯源报告，**不施工**  
> **范围**：查清黑屏根因链（谁先崩 → 为何未注册 → 为何黑）；给出最小修复清单。不扩远程点击产品设计（可引用 0820 报告）。

把下面整段复制给 Cursor Agent（Agent Mode）执行。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 开发者问题（白话）

1. 场景管理器好像没注册好。  
2. 进去是**黑屏**。  
3. Console 一堆「未注册」+ 一个 `InitComponents` 空引用——**根因是哪个？先后关系？怎么修？**

### 现场 Console（截图，时间线）

| 时间 | 日志 | 预扫含义 |
|------|------|----------|
| 16:58:37 | `[DialogueCsvGraphBuilder] Speaker「1」…导入已中止` | **旧错误**，CSV 导入工具；**一般不是**本次进场景黑屏主因（侦探确认时间线） |
| 17:34:15 | `NullReferenceException` → `ComponentSystem.InitComponents()` | **很可能先崩**：`componentsList` 里有 **null** 条目，或 component 未挂全 |
| 17:34:15 | `场景对象未到场景管理器注册并初始化=>Npc1` | `BaseSceneEntityLogic.Start`：`isInit==false` |
| 同上 | `=>面包` `=>土豆` `=>木箱` `=>木桶` `=>米袋` | Object 下物品同样未完成 `OnInit` |
| （截图未列全时） | 饼干？门？Map？ | 侦探补全所有同文案 Error |

### 预扫结论方向（可证伪）——对齐 HomeScene23 黑屏先例

| 层 | 预判 |
|----|------|
| 「未注册」文案出处 | `BaseSceneEntityLogic.Start`：若 GSM 未成功 `SceneEntity.OnInit` → `isInit` 仍 false → 打 Error（**症状**，未必是第一刀） |
| 黑屏常见链 | 某实体 `OnInit` → `InitComponentSystem` → `InitComponents` **NRE/抛异常** → SceneManager / 相机 / 黑幕 / 玩家链路中断 → **全黑** |
| NRE 直接点 | `ComponentSystem.InitComponents` 对 list 里 **null** 调 `component.Init(this)` |
| 配置侧诱因 | 新建饼干/面包等时：`ComponentSystemMono.componentsList` 有空槽、或缺 `InteractiveComponent` 却被 GetComponent 硬取（HomeScene23 曾是门缺 Interactive 抛崩） |
| 绑定侧 | `objRoot` 未指到 `Object`、无 `SceneEntity`、Manager 错挂——可导致未注册；但 **NRE 优先查** |
| 非主因 | Speaker「1」导入报错（除非同帧进场景也触发导入，证据不足） |

生活类比：总电闸跳了（Init NRE）→ 整栋楼黑；各房间「没在物业登记」（未注册）是停电后巡楼才发现的连带告警。

### 同类已修先例（必须对拍）

`Assets/Doc/技术文档/场景相关/Village_HomeScene23_可玩民居室内_技术说明.md` §6「黑屏事故与修复」：

- 缺 Interactive → 初始化抛异常 → 黑屏  
- 「未注册」是连带  

本期 HomeScene1 是 **Object 物品刚配** 后黑屏，优先查新挂物体的 ComponentSystem 列表与 Interactive。

### 已有相关报告

- `Assets/Doc/执行文档/0820/Village_HomeScene1_Object全量配置与GSM绑定_架构溯源报告.md`（配置应然；与现网事故对拍是否已施工半套）
- `Assets/Doc/执行文档/0820/物品交互对话_远程点击触发_架构溯源报告.md`

### 必读代码

- `BaseSceneEntityLogic.cs`（Start 未注册 Error；OnInit → InitComponentSystem）
- `SceneEntity.cs` / `SceneEntityComponentGSM.cs`
- `ComponentSystem.cs` → `InitComponents`
- `ComponentSystemMono`（序列化 componentsList 如何填）
- `Village_HomeScene1SceneManager.cs`
- 场景 YAML/Inspector：Object 下 Npc1、面包、土豆、木箱、木桶、米袋、饼干的组件列表

---

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Project_context.md
@Assets/Doc/02_SYSTEM_SPEC.md
@Assets/Doc/OPEN_QUESTIONS.md
@Assets/Doc/技术文档/场景相关/Village_HomeScene23_可玩民居室内_技术说明.md
@Assets/Doc/执行文档/0820/Village_HomeScene1_Object全量配置与GSM绑定_架构溯源报告.md
@Assets/Scripts/Game/GameRuntime/Entities/Base/BaseSceneObj/BaseSceneEntityLogic.cs
@Assets/Scripts/Game/GameRuntime/Entities/Base/SceneEntity.cs
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Component/SceneEntityComponentGSM.cs
@Assets/Scripts/GameFramework/CoreExtend/Component/ComponentSystem.cs
@Assets/GameRes/Scenes/Village_HomeScene1.unity
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Scene/Village_House/Village_HomeScene1SceneManager.cs

你现在是【架构侦探】。Unity 2020.3.48f1 / C#。
禁止修改场景、Prefab、代码。只读扫描 + 写溯源报告。

---

## 背景

1. 进 `Village_HomeScene1` 黑屏。
2. Console：`ComponentSystem.InitComponents` NRE + 多个「场景对象未到场景管理器注册并初始化=>Xxx」。
3. 刚在配 Object（Npc1 + 物品）。怀疑场景管理器没注册好。
4. 本期钉死：第一崩溃点、未注册与黑屏因果关系、最小修复清单。

---

## 必查

### A. 时间线与因果

1. 完整抄录 17:34:15 前后所有 Error（含堆栈，尤其 NRE 上一帧是谁调用 InitComponents）。  
2. 判断：**先 NRE 还是先未注册**？  
3. Speaker「1」是否与本次进场景无关？

### B. 「未注册」机制（症状）

- `isInit` 谁置 true？谁应调用 `SceneEntity.OnInit`？  
- `objRoot` / `sceneObjs` 现网指向？Object 下物体是否都有 `SceneEntity`？  
- 若 OnInit 中途抛异常：哪些物体 isInit 仍 false → Start 刷 Error？

### C. NRE 根因（主嫌疑）

对每个报「未注册」的物体 + 门/Map 相关实体：

| 检查 | 说明 |
|------|------|
| `ComponentSystemMono.componentsList` | 有无 **None/null** 槽？ |
| InteractiveComponent | 引用断、未加入 list、OnInit GetComponent 硬取？ |
| RaycastListener / Body | 半配导致 Init 空引用？ |
| 对照可玩样板 | `Village_HomeScene2` 某 NPC 或 HomeScene23 NpcChair |

在场景序列化或逻辑上钉死：**哪一个物体的 Init 先炸**。

### D. 黑屏机制

对拍 HomeScene23 §6：

- SceneManager Awake/OnInit 是否被异常打断？  
- 黑幕 Panel / Camera / 玩家创建是否未跑完？  
- 是否「逻辑黑」（相机无跟随）还是「UI 黑幕未关」？

### E. 最小修复建议（不施工）

按优先级列：

1. 清掉 componentsList 空引用 / 补齐 Interactive  
2. 保证 objRoot=Object 且均有 SceneEntity  
3. 必要时加固 InitComponents 跳过 null（可选，治标）  
4. 远程点击开关未落地 ≠ 黑屏主因（除非施工半套改坏了 Listener）

### F. 验收

| # | 操作 | 通过 |
|---|------|------|
| 1 | InitScene → 进 Village_HomeScene1 | **不黑屏**；无 InitComponents NRE |
| 2 | Console | **无**「未注册=>Npc1/面包/…」 |
| 3 | 能看见室内与玩家 | 相机/光照正常 |
| 4 | （次要）Npc1/物品交互 | 不黑后再验 |

---

## 侦探任务

1. **结论一句话**：黑屏因某某初始化崩溃；未注册是连带还是绑定漏。  
2. **因果链**（谁先崩 → 黑屏 → 未注册日志）。  
3. **现网绑定对拍**（Manager / objRoot / 各物体组件表）。  
4. **NRE 责任物体** + componentsList 证据。  
5. **最小修复清单**（用户可照做）。  
6. OPEN：是否要给 InitComponents 加 null 防护。  
7. **禁止**：改资产；把锅只甩给「没拖进 sceneObjs」却忽略 NRE；把 CSV Speaker 当黑屏主因若无证据。

---

## 输出

写入：`Assets/Doc/执行文档/0820/Village_HomeScene1_进屋黑屏与未注册_架构溯源报告.md`

① 结论一句话  
② 原因（生活类比：总闸跳了 vs 房间没挂号）  
③ 用户检查清单（先修哪个物体/哪个列表）  
④ 程序：堆栈解读、isInit 机制、与 HomeScene23 黑屏对照、修复顺序、OPEN  

MASTER 四段式口头汇报。
```

---

## 施工员续跑（报告拍板后再贴）

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0820/Village_HomeScene1_进屋黑屏与未注册_架构溯源报告.md
@Assets/Doc/技术文档/场景相关/Village_HomeScene23_可玩民居室内_技术说明.md
@Assets/Doc/OPEN_QUESTIONS.md
@Assets/GameRes/Scenes/Village_HomeScene1.unity

你现在是【施工员】。按报告消除 Village_HomeScene1 进屋黑屏与「场景对象未注册」错误。

必须：先修初始化崩溃点（NRE / 缺组件），再保证 Object 下实体被 GSM 正常 OnInit；对齐 HomeScene23 黑屏修复经验；不改龙宫 HomeScene1；不把远程点击需求捆进本次除非报告写明同一半套施工导致。

提交说明：谁先崩、怎么修的、进场景是否不黑且无未注册 Error。
```
