# Cursor Agent Prompt · 门口对白结束 → Loading 进村长家 → 自动播「继续对话」

> **角色**：先【架构侦探】只读定方案，报告后再【施工员】  
> **日期**：2026-09-01  
> **产品设定（钉死）**：  
> 1. 玩家在村外播完 **`Village_村长家门口初次对话`**  
> 2. **直接传送**进入 **`Village_Chief_House`**（现网已有 Loading 进屋，见 0831 施工）  
> 3. 进屋后 **自动继续**触发对话 **`Village_村长家继续对话`**（台本/树已有；须可 Play）  
> **不是**：进屋后还要点 NPC / 按 E 才播；不是另开一条「晚宴」线替代续聊  
> **本阶段（侦探）**：只读；禁止改代码 / 场景 / Prefab  
> **报告落盘**：`Assets/Doc/执行文档/0901/Village_门口对白结束_Loading进村长家_自动播继续对话_架构溯源报告.md`

把下面「侦探」整段复制给 Cursor Agent（Agent Mode）执行。施工 Prompt 见文末。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 产品时序（期望全链）

```
Village_KenMuNi1 · 靠近 Npc_Chief
  →（既有）BlackPanel → TriggerStory("Village_村长家门口初次对话")
  → 门口三人戏播完
  →（既有 0831）LoadSceneWithLoadingPanel("Village_Chief_House")
  → 室内 EnterPos：lastScene=Village_KenMuNi1 落点
  →【本需求】自动 TriggerStory("Village_村长家继续对话")
  → 玩家看完续聊还控（无需点门内 NPC）
```

**禁止**理解成：只进屋不播续聊；或续聊仍挂在门外/门上点 E。

### 与现网资产对照（预扫 · 须磁盘证伪）

| 项 | 现状（助手预扫） | 侦探须核实 |
|----|------------------|------------|
| 门口对白 Prefab | ✅ `Prefabs/Dialogue/Village_村长家门口初次对话.prefab` | Play 可完整结束 |
| 门口结束 → Loading 进屋 | ✅ `ChiefNearDoorStoryTrigger.OnStoryFinished` → `LoadSceneWithLoadingPanel` | 施工说明 0831 |
| `Village_Chief_House` GSM | ✅ 有；`OnEnterScene` **仅 Debug**，**无** TriggerStory | 续聊挂点缺口 |
| 续聊 CSV | ✅ `Assets/Dialog/Village_村长家继续对话.csv`（村/雅/古；村 Face1～3） | Speaker/Face 能否 Import |
| 续聊 Generated 树 | ✅ `DialogueTrees/Generated/Village_村长家继续对话.asset` | 是否与 CSV 同步 |
| 续聊对话 Prefab | ❌ **未见** `Prefabs/Dialogue/Village_村长家继续对话.prefab` | **高概率依赖**：须 Setup/Import 成品壳 |
| 晚宴台本 | 另有 `Village_村长家晚宴对白台本` | **≠** 本期续聊；勿混用 |

### 「传送」语义（钉死）

| 说法 | 现网对应 | 本期 |
|------|----------|------|
| 传送进村长家 | `LoadSceneWithLoadingPanel(Village_Chief_House)` | ✅ **复用既有**，勿再写第二套换场 |
| 黑幕切场 | `blackFade:true` | ❌ 对白→进屋主表现已定为 Loading |
| 点 `House_Chief` | 手动二次进入 | 保留；**是否也播续聊**见 OPEN Q2 |

### 进屋后自动播续聊 · 方案倾向

| 方案 | 做法 | 倾向 |
|------|------|------|
| **C1 · Chief_House GSM `OnEnterScene` + 门闩** | 进屋后若「应从门口戏续播」且本档未播过 → `TriggerStory("Village_村长家继续对话")` | ✅ 对齐 `TryTriggerVillageStartStoryOnce` / 进村开场 |
| C2 · Loading `callBack` 里直接 Trigger | 切场回调立刻播 | ⚠️ 场景/GSM/EnterPos 可能未 Ready；易竞态 |
| C3 · 室内场景放 `SimpleStoryTrigger` Enter | 落点进 Collider 再播 | ⚠️ 落点漂移会漏播；且「自动」语义弱 |
| C4 · 门口图末 Action 链式 Trigger 室内名 | 未进屋就 Trigger 室内 Prefab | ❌ 场景错位 |

**门闩倾向**（侦探拍板其一并写清存档键）：

| 门闩 | 含义 | 倾向 |
|------|------|------|
| **F1 · 门口戏已播过 + 续聊未播** | `StoryTriggerCountData`：门口名已用 ∧ 续聊名未用 → 进屋播一次 | ✅ 简单；手动再进门也会播一次（若续聊未播） |
| F2 · 显式 Pending 旗 | 门口 `onStoryEnd` 置 `pendingChiefContinue=true`；进屋消费 | 更严「仅自动进屋这条链」；多一字段 |
| F3 · 每次进 Chief_House 都播 | 无单次 | ❌ 产品续聊应单次 |

### 时序 / 遮罩（进村开场教训）

进村曾踩过：`OnEnterScene` 在黑幕淡出后 Trigger → 露景漏缝。  
本期进屋主表现是 **LoadingPanel**（非 BlackPanel），侦探须画清：

```
Loading 关 / 场景 Ready / EnterPos 落点
  → 何时 TriggerStory(继续对话)
  → 对话壳异步打开期间，室内景是否可接受短暂可见
  → 是否需要「壳 Ready 前压一层」或可接受 Loading 刚关立刻出对白
```

对照样板：

| 样板 | 挂点 | 备注 |
|------|------|------|
| `Village_KenMuNiStart` | 村 GSM `TryTrigger…Once` | 后修过遮罩时序 |
| `Village_ShopStart` | 进店 Cover / 首次旗 | 店内首次对白 |
| 门口戏本身 | `ChiefNearDoorStoryTrigger` 主动 ShowFade | 屋外专用，**勿**把续聊塞回该组件跨场景 Trigger |

倾向：**续聊挂室内 GSM**；门口 Trigger 只负责「结束 → Loading 进屋」，不跨场景直接 `TriggerStory(继续对话)`。

### Prefab 依赖（验收阻断项）

`TriggerStory("Village_村长家继续对话")` → `DialoguePath` →  
`Assets/GameRes/Prefabs/Dialogue/Village_村长家继续对话.prefab`

若 Prefab 未落盘 → 再现「加载资源失败」（对齐 0831 门口戏）。侦探须在报告中单列：

| 子任务 | 内容 |
|--------|------|
| P0 触发链 | GSM/门闩/单次 |
| **P0 Prefab** | Setup/Import 成品：CSV → Generated → Prefab；立绘是否仍要雅+古+村三人（CSV 有三角色） |
| 样板 | 可 Copy 门口初次对话壳再 Import 续聊 CSV（须核实菜单/Editor 是否已有；无则最小复用门口 Setup） |

### 本期边界

| 做 | 不做 |
|----|------|
| ✅ 门口戏结束 →（既有）Loading 进 `Village_Chief_House` → **自动**播 `Village_村长家继续对话` | ❌ 改进屋目标场景名 |
| ✅ 续聊 Prefab 可加载可 Play（缺则补 Setup） | ❌ 把续聊改成晚宴台本 |
| ✅ 同档续聊单次（倾向） | ❌ 每次点 `House_Chief` 重播（除非产品改口） |
| ✅ 与门口 Loading 进屋施工共存 | ❌ 拆掉 `LoadSceneWithLoadingPanel` 改回纯黑幕主表现 |
| ✅ 落点正确后再还控/对白 | ❌ Update 堆业务；须 `StoryComponentGSM.TriggerStory` |

### 严禁

- 进屋后无显式 `TriggerStory("Village_村长家继续对话")`（假自动）  
- 在村场景跨场景直接 Trigger 室内对白（未进屋）  
- 续聊 Prefab 缺失时只改 Trigger 名绕过  
- 手动点门与自动进屋行为未在报告中写清（漏播 / 误播）  
- 混用 `Village_村长家晚宴对白台本`  

### 开放（侦探须答）

| ID | 问题 | 助手倾向 |
|----|------|----------|
| Q1 | 续聊挂点 C1/C2/C3？ | **C1** GSM `OnEnterScene` |
| Q2 | 仅自动进屋播，还是「门口已播且续聊未播」时手动进门也播？ | **F1**（未播则进门也补播一次）；若产品只要自动链则改 F2 |
| Q3 | Loading 刚关到对白壳 Ready 是否要二次遮罩？ | 先可接受短露景；若穿帮再对齐进村 A′ |
| Q4 | 续聊 Prefab 是否三人立绘？ | CSV 有村/雅/古 → **倾向三人**，对齐门口壳 |
| Q5 | Prefab Setup：新建菜单 vs 复用门口 Setup 改名？ | 最小复用门口流水线 |

---

## 侦探 Prompt（复制给 Agent）

```text
你是【架构侦探】。Unity 2020.3.48f1 / C#。禁止修改代码、场景、Prefab。默认中文。

@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Project_context.md
@Assets/Doc/02_SYSTEM_SPEC.md

## 产品目标
玩家播完 Village_村长家门口初次对话 后，经现网 Loading 传送进入 Village_Chief_House，
进屋后立刻自动触发 Village_村长家继续对话（无需点 E / 点 NPC）。
续聊资源：@Assets/GameRes/DialogueTrees/Generated/Village_村长家继续对话.asset
（及对应 CSV）。须裁定 Prefab 是否已可加载。

## 必读（链路上游 · 已落地）
@Assets/Doc/执行文档/0831/门口三人立绘对白结束_Loading进Village_Chief_House_架构溯源报告.md
@Assets/Doc/施工说明/0831/门口三人立绘对白结束_Loading进Village_Chief_House_施工说明.md
@Assets/Scripts/Game/GameRuntime/Entities/SceneEntities/Village_KenMuNi/ChiefNearDoorStoryTrigger.cs
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Component/LoadSceneComponentGSM.cs

## 必读（室内挂点 / 进场景播对白样板）
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Scene/Village_House/Village_Chief_HouseSceneManager.cs
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Scene/Village_KenMuNi/Village_KenMuNiSceneManager.cs
（TryTriggerVillageStartStoryOnce / StoryTriggerCountData）
@Assets/Doc/执行文档/0804/第一章进村插入Village_KenMuNiStart_架构溯源报告.md
@Assets/Doc/执行文档/0804/进村开场对话遮罩时序_禁止露景漏缝_架构溯源报告.md
@Assets/Doc/技术文档/场景相关/场景切换.md
@Assets/Doc/技术文档/场景相关/加载地图的加载条功能.md

## 必读（续聊资源）
@Assets/Dialog/Village_村长家继续对话.csv
@Assets/GameRes/DialogueTrees/Generated/Village_村长家继续对话.asset
@Assets/Doc/执行文档/0831/Village_村长家门口初次对话_加载资源失败修复_施工执行说明.md
（Prefab 缺失 = 加载失败 教训）
检索：Village_村长家继续对话、Village_村长家晚宴、OnEnterScene、TriggerStory、
StoryTriggerCountData、CheckStoryUsed、DialoguePath、House_Chief。

## 侦探任务
1. 核实全链现状：门口结束 → Loading 进屋是否已通；Chief_House OnEnterScene 缺口。
2. 核实续聊：CSV / Generated / Prefab 三者；缺 Prefab 则列为 P0 依赖与 Setup 最小路径。
3. 裁定挂点 C1/C2/C3 与门闩 F1/F2；画序列图（Loading 关 → Trigger → 壳 Ready → 还控）。
4. 写清与 House_Chief 手动进屋关系（是否补播续聊）。
5. 最小改动清单（GSM + 可选 Setup Editor + 存档键）+ 验收 + 更新 OPEN。
6. 禁止把续聊混成晚宴台本；禁止拆掉既有 Loading 进屋。

## 报告落盘
Assets/Doc/执行文档/0901/Village_门口对白结束_Loading进村长家_自动播继续对话_架构溯源报告.md

结构：①结论 ②现网缺口 ③续聊资源三态 ④挂点与门闩 ⑤时序/遮罩 ⑥与手动门关系
⑦最小清单 ⑧验收 ⑨OPEN ⑩程序补充

沟通：①结论一句话 ②原因 ③用户检查清单 ④程序补充
```

---

## 施工 Prompt（报告拍板后另开 Agent 再复制）

```text
你是【施工员】。Unity 2020.3.48f1 / C#。最小化修改。默认中文。

@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0901/Village_门口对白结束_Loading进村长家_自动播继续对话_架构溯源报告.md

## 目标
1. 门口初次对话结束 →（保持既有）Loading 进 Village_Chief_House。
2. 进屋后自动 TriggerStory("Village_村长家继续对话")；同档单次（按报告门闩）。
3. 若报告要求：补齐 Prefabs/Dialogue/Village_村长家继续对话.prefab（Setup/Import），
   保证 DialoguePath 可加载；立绘按报告（倾向三人）。
4. 禁止 Update 堆业务；禁止改用晚宴台本；禁止拆 LoadSceneWithLoadingPanel。

## 落盘
Assets/Doc/施工说明/0901/Village_门口对白结束_Loading进村长家_自动播继续对话_施工说明.md
同步 OPEN_QUESTIONS。

## 验收
- [ ] 屋外门口三人戏可完整播完
- [ ] 结束后自动 Loading → Village_Chief_House，落点正确
- [ ] 进屋后无需操作即自动开始 Village_村长家继续对话
- [ ] 续聊可完整播完；Console 无该 Prefab「加载资源失败」
- [ ] 同档再进村长家：不重复播续聊（若报告要求单次）
- [ ] House_Chief 手动进：行为符合报告（补播一次或不播）
- [ ] House_Tree / 晚宴台本未被误改

## 沟通风格
①结论 ②原因 ③用户检查清单 ④程序补充
```

---

## 给开发者

1. **先跑「侦探 Prompt」** → 定挂点（倾向室内 GSM）+ 续聊 Prefab 是否缺。  
2. 报告拍板后再跑「施工 Prompt」。  
3. 上游依赖：0831「门口结束 Loading 进屋」应已合入；本期只补 **进屋后自动续聊**。  
4. 续聊名钉死：**`Village_村长家继续对话`**（与 Generated / 未来 Prefab 同名）。
