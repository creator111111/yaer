# Cursor Agent Prompt · 门口进村长家：去掉读条，改普通黑屏切场

> **角色**：先【架构侦探】短扫钉调用点与续聊遮罩时序，再【施工员】最小改  
> **日期**：2026-09-02  
> **现象（用户截图）**：从村长家**门口**进 `Village_Chief_House` 时出现 **LoadingPanel**（蛋糕 Q 版 + 粉进度条）  
> **产品期望（钉死 · 推翻 0831「进屋=读条」决议）**：  
> 1. 门口 → 进屋：**不要读条加载**  
> 2. 与其它正常场景切换一样：**系统 BlackPanel 黑屏淡入淡出**即可  
> 3. **LoadingPanel（蛋糕读条）只留给「有时间跳转」的情况**，普通进屋不算时间跳转  
> **不是**：取消进屋；不是取消进屋后自动播 `Village_村长家继续对话`；不是改落点 / WalkArea  
> **上游（将被改口）**：  
> - `执行文档/0831/门口三人立绘对白结束_Loading进Village_Chief_House_架构溯源报告.md`（当时定 L2=Loading）  
> - `施工说明/0901/...自动播继续对话`（依赖 Loading 盖景）  
> **报告落盘**：`Assets/Doc/执行文档/0902/Village_门口进村长家_改黑屏切场_架构溯源报告.md`  
> **施工落盘**：`Assets/Doc/施工说明/0902/Village_门口进村长家_改黑屏切场_施工说明.md`

把「侦探」段先复制给 Agent；拍板后用文末「施工」段。

---

## 提示词助手预梳理（须再证，勿当唯一真相）

### 产品语义

| 说法 | 含义 |
|------|------|
| 不用读条 | **禁止**开 `LoadingPanel`（蛋糕图 + 进度条） |
| 正常黑屏 | `LoadScene(..., blackFade: true)` → 系统 **`BlackPanel`**（与 LeftDoor / 楼梯上楼等同路） |
| 读条留给谁 | **仅时间跳转**类演出；门口进屋、日常进门 **不算** |

### 现网链路（助手预扫 · 须磁盘证伪）

```
Village_村长家门口初次对话 结束
  → ChiefNearDoorStoryTrigger.OnStoryFinished
  → LoadSceneComponentGSM.LoadSceneWithLoadingPanel(Village_Chief_House)
       → OpenUIForm(LoadingPanel)
       → LoadScene(scene, blackFade: false)   // 故意不用黑幕
  → Chief_House OnEnterScene
       → TryTriggerChiefContinueOnce()        // 注释写「趁 LoadingPanel 仍盖住」
```

| 锚点 | 路径 | 角色 |
|------|------|------|
| 自动进屋 | `ChiefNearDoorStoryTrigger.cs` ≈ 调 `LoadSceneWithLoadingPanel` | **本期主改点** |
| 读条助手 | `LoadSceneComponentGSM.LoadSceneWithLoadingPanel` | 保留 API，供真·时间跳转 |
| 普通黑幕 | `LoadSceneComponentGSM.LoadScene(..., blackFade:true)` | **门口进屋应对齐此路** |
| 手动门 | `SceneChangeDoor.ShowLoadingUI` → 同上 Loading 助手 | 须查 `House_Chief` 是否勾了读条 |
| 续聊遮罩 | `Village_Chief_HouseSceneManager.OnEnterScene` | 从「靠 Loading 盖」改「靠 BlackPanel 盖」须验收无露景 |

### 关键假说

| ID | 假说 | 倾向 |
|----|------|------|
| **H1** | 自动进屋唯一读条源 = `ChiefNearDoorStoryTrigger` → `LoadSceneWithLoadingPanel` | ✅ 主因 |
| **H2** | `House_Chief` 门 `ShowLoadingUI=true`，手动进门也会读条 | 须 Hierarchy / Prefab 核实；产品「门口进」应对齐一并改黑幕 |
| **H3** | 改黑幕后，续聊 Trigger 时机与 BlackPanel 淡出竞态 → 露室内景一帧 | 须对照进村开场教训；施工时保证全黑内或黑幕未淡完前 Trigger |

### 方案倾向（施工默认）

| 方案 | 做法 | 倾向 |
|------|------|------|
| **F1 · 自动进屋改 `LoadScene(..., blackFade:true)`** | `ChiefNearDoorStoryTrigger` 不再调 `LoadSceneWithLoadingPanel`；改调普通 `LoadScene(Village_Chief_House)`（默认黑幕） | ✅ 主修 |
| **F2 · 手动 `House_Chief` 门 `ShowLoadingUI=false`** | 与 LeftDoor / 楼梯案一致 | ✅ 若 H2 成立必做 |
| F3 · 删掉 `LoadSceneWithLoadingPanel` API | 破坏真·时间跳转调用方 | ❌ 只停用门口路径 |
| F4 · 假读条时长改短 | 仍是蛋糕 UI | ❌ 产品不要这套皮 |

**续聊遮罩（改黑幕后）**：

| 项 | 倾向 |
|----|------|
| 仍在 `OnEnterScene` 调 `TryTriggerChiefContinueOnce` | ✅ 保留门闩逻辑 |
| 注释/实现 | 改为依赖 **BlackPanel 仍盖住**（`blackFade:true` 换场期间），勿再写「靠 LoadingPanel」 |
| 若仍露景 | 用 `LoadScene` 的 `stayAction`（全黑回调）里挂旗 / 或对齐全黑后再 Trigger；侦探比选，**禁止**为遮露景又开回 Loading |

### 本期边界

| 做 | 不做 |
|----|------|
| ✅ 门口自动进屋：Loading → BlackPanel | ❌ 取消自动进屋 / 取消续聊 |
| ✅ 核对并关掉门口相关门的 ShowLoadingUI（若勾了） | ❌ 全局禁止 LoadingPanel（时间跳转仍要） |
| ✅ 验收续聊不露景、落点仍对 | ❌ 改 EnterPos / WalkArea / 对话台本 |
| ✅ 更新 OPEN：推翻「进屋=读条」旧决议 | ❌ 改楼梯上楼、出村长家送树屋等已定黑幕案 |

### 严禁

- 用缩短假读条、换 Loading 图「看起来像黑屏」糊弄  
- 改完自动进屋却留下 `House_Chief` 手动门仍读条（若产品「门口进」含手动门）  
- 为修露景重新 `LoadSceneWithLoadingPanel`  
- 误改地图选关、章末时间跳转等正当 Loading 调用方  

### 对照文档

- `Assets/Doc/02_SYSTEM_SPEC.md` §5.2（换场：BlackPanel vs LoadingPanel）  
- `Assets/Doc/技术文档/场景相关/场景切换.md`（若在）  
- `Assets/Doc/执行文档/0831/门口三人立绘对白结束_Loading进Village_Chief_House_架构溯源报告.md`（旧决议）  
- `Assets/Doc/执行文档/0901/Village_门口对白结束_Loading进村长家_自动播继续对话_架构溯源报告.md`  
- `Assets/Doc/执行文档/0901/Village_Chief_House_楼梯上楼换场巨树2楼_架构溯源报告.md`（`ShowLoadingUI=false` 样板）  

---

## 侦探 Prompt（复制给 Agent · 先跑）

```text
你是【架构侦探】。Unity 2020.3.48f1 / C#。
禁止修改代码/场景/Prefab。只读扫描 + 写溯源报告。
默认中文。沟通：①结论 ②原因 ③用户检查清单 ④程序补充。

@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Project_context.md
@Assets/Doc/02_SYSTEM_SPEC.md
@Assets/Doc/提示词/0902/Village_门口进村长家_改黑屏切场_架构侦探提示词.md
@Assets/Doc/OPEN_QUESTIONS.md

## 产品（钉死 · 改口）
从村长家门口进入 Village_Chief_House：不要 LoadingPanel 蛋糕读条；
改和其他场景一样的系统 BlackPanel 黑屏。
LoadingPanel 仅留给时间跳转。自动进屋后的续聊逻辑须保留。

## 必读
@Assets/Scripts/Game/GameRuntime/Entities/SceneEntities/Village_KenMuNi/ChiefNearDoorStoryTrigger.cs
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Component/LoadSceneComponentGSM.cs
@Assets/Scripts/Game/GameRuntime/Entities/SceneEntities/CommonEntity/SceneChangeDoor.cs
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Scene/Village_House/Village_Chief_HouseSceneManager.cs
（OnEnterScene / TryTriggerChiefContinueOnce）
检索：LoadSceneWithLoadingPanel、ShowLoadingUI、House_Chief、Village_Chief_House、blackFade。
场景/门：Village_KenMuNi1 的 House_Chief（若有）ShowLoadingUI 当前值。

## 任务
1. 列出所有「进 Village_Chief_House」且会开 LoadingPanel 的调用点（自动对白结束 + 手动门等）。
2. 画出改黑幕后时序：对白结束 → BlackPanel → LoadScene → OnEnterScene 续聊；标露景风险点。
3. 推荐 F1/F2；写清 stayAction 是否要动；最小改动清单。
4. 标明哪些 LoadSceneWithLoadingPanel 调用必须保留（时间跳转）。
5. 更新 OPEN：0831「进屋=Loading」改为「进屋=黑幕」；时间跳转例外写清。

## 报告
Assets/Doc/执行文档/0902/Village_门口进村长家_改黑屏切场_架构溯源报告.md
```

---

## 施工 Prompt（根因拍板后复制给 Agent）

```text
你是【施工员】。Unity 2020.3.48f1 / C#。最小化修改。默认中文。
沟通：①结论 ②原因 ③用户检查清单 ④程序补充。

@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0902/Village_门口进村长家_改黑屏切场_架构溯源报告.md
@Assets/Doc/提示词/0902/Village_门口进村长家_改黑屏切场_架构侦探提示词.md

## 目标
村长家门口 → Village_Chief_House：不再出现 LoadingPanel 读条；
使用与其它日常换场相同的 BlackPanel 黑屏。
进屋后自动播 Village_村长家继续对话 的门闩逻辑保留且不露景。

## 默认施工方向（若报告未改口）
1. **F1**：`ChiefNearDoorStoryTrigger` 将对白结束后的
   `LoadSceneWithLoadingPanel(Village_Chief_House)`
   改为 `LoadScene(Village_Chief_House)`（默认 blackFade:true）。
2. **F2**：若 `House_Chief`（或等价进门）`ShowLoadingUI=true`，改为 false。
3. 更新 `Village_Chief_HouseSceneManager` 注释：遮罩依赖 BlackPanel，不再写 LoadingPanel。
4. 若验收露景：按报告用 stayAction / 全黑时机微调 Trigger，禁止开回读条。
5. 代码详细注释写原因（产品：读条仅时间跳转）；同步 OPEN_QUESTIONS.md。
6. 保留 `LoadSceneWithLoadingPanel` API，勿删。

## 约束
- 禁止用 LoadingPanel 任何变体冒充黑屏
- 禁止取消自动进屋或续聊
- 禁止改 EnterPos / WalkArea / 对话 Prefab 台本
- 禁止误改地图/章末等正当时间跳转 Loading 调用方
- 回归：LeftDoor 出屋、楼梯上楼黑幕、出村长家送树屋黑幕

## 落盘
Assets/Doc/施工说明/0902/Village_门口进村长家_改黑屏切场_施工说明.md

## 验收
- [ ] 门口三人戏结束 → 进屋：只见黑屏，不见蛋糕读条/粉进度条
- [ ] 进屋落点正确；自动续聊仍播且无明显露景漏缝
- [ ] 手动进村长家门（若保留）同样黑屏不读条
- [ ] 其它场景日常门换场黑屏正常
- [ ] 仍使用 LoadingPanel 的时间跳转流程未被误改（抽一条回归）
```

---

## 给开发者（一句话）

0831 曾故意用 **`LoadSceneWithLoadingPanel`** 进屋；产品现改为与日常门一样的 **`LoadScene` 黑幕**。主改 `ChiefNearDoorStoryTrigger`，并核对手动门 `ShowLoadingUI`；续聊照播，只是盖景从读条换成黑屏。
