# Cursor Agent Prompt · 老农打水：当日不可再接 + ResetQuest 预留 + 禁空跑「完成结算」

> **角色**：先【架构侦探】只读溯源与方案拍板，报告后再【施工员】最小化落地  
> **日期**：2026-08-31  
> **场景 / 任务**：`Village_KenMuNi1` · `Npc_Farmer` · `Quest_003`「老农的浇地水」  
> **产品改口（钉死 · 覆盖 0830「交完同档可再接」）**：  
> 1. **接取之后，当日不可再次接取**（同一「游戏日」只帮一次；未做日期跳转前 = 同存档内交完就锁）  
> 2. **尚无日期跳转**：先在任务系统留 **`ResetQuest`（或等价）公开接口**，供日后跳日/新一天时清「今日已做」锁，再允许 Offer  
> 3. **禁止空跑结算**：接取/交付后老人**不得**再反复播 `Village_老农打水任务_完成结算.prefab`——那种「对白还在、不给金币、不弹特效/Tips」是 bug，必须消掉  
> **本阶段（侦探）**：只读；禁止改代码 / Prefab / Quest 表  
> **报告落盘**：`Assets/Doc/执行文档/0831/Village_老农打水_当日不可再接与禁空跑结算_架构溯源报告.md`

把下面「侦探」整段复制给 Cursor Agent（Agent Mode）执行。施工 Prompt 见文末，**须等报告拍板后再开**。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 与旧决议冲突（须在报告写清「改口」）

| 旧（0830/0831 交完可再接） | 新（本需求） |
|---------------------------|--------------|
| `TurnedIn` → Offer（帮/不帮） | **当日不可再 Offer/Accept** |
| `repeatable=true` + Accept 允许 TurnedIn 重接 | 重接仅经 **`ResetQuest`**（日后跳日调用）；同存档默认锁死 |
| 交完再谈可再发空桶 | 交完再谈 → **短循环对白**，不发桶、不发金 |

参考已施工：`施工说明/0830/Village_老农打水_交完可再接任务_施工说明.md` —— 本需求是**产品改口**，不是忘了旧案。

### 用户点名的坏表现（须复现路径）

| 现象 | 产品判定 |
|------|----------|
| 再点老农仍进 **`Village_老农打水任务_完成结算`** | ❌ 不允许（除非真正可交且尚未 TurnIn） |
| 结算对白播完但 **无金币、无 Tips/特效** | ❌ 空跑；`QuestTurnInAction` 失败仍播全图是典型病因假说 |
| 期望 | 已接未交够 → **`_进行中`**；已交/当日已锁 → **短句循环**（对齐 Npc23 `Thanks`，或新建「今日谢过」Prefab） |

### 现网锚点（预扫）

| 层 | 路径 / 行为 |
|----|-------------|
| Trigger | `FarmerQuestStoryTrigger`：`null/TurnedIn→Offer`；`InProgress+CanTurnIn→完成结算`；否则 `_进行中` |
| 交付图 | `Assets/GameRes/Prefabs/Dialogue/Village_老农打水任务_完成结算.prefab`（句末 `QuestTurnInAction`） |
| 发奖 | TurnIn **成功**才 `GrantQuestRewards`；失败只 Log → **对白仍会播完**（空跑） |
| 样板 | `Npc23QuestStoryTrigger`：`TurnedIn→Thanks`（**不**回 Offer）——更接近新产品口径 |
| Config | `Quest_003.repeatable=true`（交完可再接时改的） |

### 状态机目标倾向（侦探拍板，可改）

| 状态 | 应播 Prefab | 接取 / 发奖 |
|------|-------------|-------------|
| 未接（`state==null`）或 **Reset 后** | Offer（帮/不帮） | 帮 → Accept + 空桶×4 + Tips |
| `InProgress` 且满桶不足 | `_进行中` | 不 Accept、不 TurnIn |
| `InProgress` 且可交 | `_完成结算` **仅此一次有效交付** | TurnIn 成功才发金 |
| `TurnedIn`（当日已交、未 Reset） | **短循环**（复用 `_进行中` / 新建 `_已完成` / 对齐 Thanks） | **禁止**再进 `_完成结算`、禁止再 Accept |
| 日后跳日 | 调 `ResetQuest("Quest_003")` | 清锁 → 可再 Offer |

**开放（写入 OPEN_QUESTIONS，勿擅自拍）**：

| ID | 问题 | 助手倾向 |
|----|------|----------|
| Q1 | 当日锁粒度：仅 `TurnedIn` 锁，还是「一旦 Accept 就算今日已占用」？ | 倾向 **交完（TurnedIn）后锁**；进行中仍催促；若产品要「接了就占坑」再另说 |
| Q2 | 短循环用现成 `_进行中` 还是新建 `_今日已完成`？ | 倾向 **新建短句** 或暂复用 `_进行中`（文案可能「快去打水」不贴切） |
| Q3 | `repeatable` 字段：保持 true（语义=可经 Reset 重复）还是改 false？ | 倾向 **true + 仅 Reset 放行**，避免与埃吉尔语义搅混时在报告写清 |
| Q4 | Reset 是否清背包残留空/满桶？ | 倾向 **本期只清任务状态/进度**；桶另议 |

### `ResetQuest` 接口预留（施工须落地 · 侦探定签名）

倾向（可改）：

```csharp
// QuestManager 公开 API；暂无日期系统调用方，可 Editor/Debug 先调
void ResetQuest(string questId);
// 行为倾向：清 questStates / questProgress 中该 id（回到「未接」）；
// 不自动发奖、不自动改背包；日志 [Quest] Reset ...
```

替代方案：`ClearQuestForNewDay(questId)` / 记 `lastCompletedDay`——无日历前用「有状态=已占用、Reset=新一天」。报告写明推荐与否决理由。

### 本期边界

| 做（侦探写清 + 后续施工） | 不做 |
|---------------------------|------|
| ✅ 当日不可再接的状态机 + Trigger 切图 | ❌ 实现真实日期 / 昼夜跳转 UI |
| ✅ `ResetQuest`（或等价）可调用接口 | ❌ 改井换桶、Tips 美术、金币 500 |
| ✅ 消灭空跑 `_完成结算` | ❌ 重做整段老农台本长对白 |
| ✅ 改口记入 OPEN_QUESTIONS / 报告 | ❌ 改 Quest_001/002 每日规则（除非共用 Reset 无伤） |

### 严禁

- 交完后仍 `TurnedIn→Offer` 同档无限再接（旧改口作废）  
- 已 TurnedIn / 不可交时仍 Resolve 到 `_完成结算`  
- 对话图里 TurnIn 失败仍当成功播「报酬」却不发奖（空跑）——Trigger 层就不该进这张图  
- 假造日期系统大工程  

---

## 侦探 Prompt（复制给 Agent）

```text
你是【架构侦探】。Unity 2020.3.48f1 / C#。禁止修改任何代码、Prefab、配置、CSV。默认中文。

@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Project_context.md
@Assets/Doc/02_SYSTEM_SPEC.md

## 产品目标（改口）
老农打水 Quest_003：
1. 任务接取并完成交付后，**今日不能再次接取**（尚无日期跳转：同存档交完即锁）。
2. 先留 **ResetQuest（或等价）公开接口**，供日后跳日重置后再 Offer。
3. 修复坏体验：老人反复播 @Assets/GameRes/Prefabs/Dialogue/Village_老农打水任务_完成结算.prefab，
   却不给金币、不弹特效/Tips（空跑结算）——必须定位根因并给出切图/门禁方案。

本需求 **覆盖** 0830「交完同档可再接」决议。

## 必读
@Assets/Scripts/Game/GameRuntime/Entities/SceneEntities/Village_KenMuNi/FarmerQuestStoryTrigger.cs
@Assets/Scripts/Game/GameRuntime/Entities/SceneEntities/Village_House/Npc23QuestStoryTrigger.cs
@Assets/Scripts/Game/GameMgr/Component/Archive/ArchiveDataClass/Quest/QuestManager.cs
@Assets/Scripts/Game/GameRuntime/NodeCanvas/NodeCanvasNode/ActionTask/Common/QuestTurnInAction.cs
@Assets/GameRes/Config/QuestConfig/QuestConfig.json
@Assets/GameRes/Prefabs/Dialogue/Village_老农打水任务_完成结算.prefab
@Assets/GameRes/Prefabs/Dialogue/Village_老农打水任务_进行中.prefab
@Assets/GameRes/Prefabs/Dialogue/Village_老农打水任务.prefab
@Assets/Doc/施工说明/0830/Village_老农打水_交完可再接任务_施工说明.md
@Assets/Doc/施工说明/0830/Village_老农打水_空满桶道具与井交互及接任务_施工说明.md
@Assets/Doc/OPEN_QUESTIONS.md（Quest_003 / 交完可再接相关段）

## 侦探任务
1. 画出接取→井→可交→TurnIn→再点老农 的 Prefab 切换与状态变化；标出「空跑完成结算」的复现条件。
2. 对比 Npc23：TurnedIn 为何不回 Offer；老农交完可再接改了哪几处。
3. 给出新产品状态机（表）：未接 / InProgress不足 / InProgress可交 / TurnedIn当日锁 / Reset 后。
4. 设计 ResetQuest API：签名、清什么、不清什么、谁在无日期系统时调用（Debug/日后跳日）。
5. 最小改动清单（Trigger / AcceptQuest 门禁 / repeatable 语义 / 是否新建短对白 Prefab）。
6. 开放问题写入报告 + 建议更新 OPEN_QUESTIONS（改口条目）；未拍板勿当已定。

## 报告落盘
Assets/Doc/执行文档/0831/Village_老农打水_当日不可再接与禁空跑结算_架构溯源报告.md

结构建议：①结论 ②空跑复现链路 ③与交完可再接冲突点 ④目标状态机 ⑤ResetQuest 方案 ⑥最小施工清单 ⑦验收 ⑧OPEN ⑨给程序

沟通：①结论一句话 ②原因 ③用户检查清单 ④（可选）程序补充
```

---

## 施工 Prompt（报告拍板后另开 Agent 再复制）

```text
你是【施工员】。Unity 2020.3.48f1 / C#。最小化修改。默认中文。

@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0831/Village_老农打水_当日不可再接与禁空跑结算_架构溯源报告.md

## 目标
按侦探报告落地：
1. Quest_003 交完（TurnedIn）后当日不可再接；再谈老农不进 Offer、不进空跑 `_完成结算`。
2. QuestManager 增加 ResetQuest（或报告定名）公开接口；无日期系统暂不挂流程，可供 Debug 调用验证。
3. 仅当 InProgress 且 CanTurnInCollectQuest 为真时才播 `_完成结算`；TurnIn 失败路径不得成为日常循环。

## 约束
- 保持现有架构；组件解耦；禁止 Update 堆业务。
- 不实现真实日期跳转 UI/系统。
- 不改金币 500、井逻辑、Tips 图（除非报告点名门禁必需）。
- 覆盖旧「交完可再接」行为；同步改注释与 OPEN_QUESTIONS 改口状态。
- 技术/施工说明放入：
  Assets/Doc/施工说明/0831/Village_老农打水_当日不可再接与禁空跑结算_施工说明.md

## 验收
- [ ] 未接 → Offer → 帮 → Accept + 空桶 Tips
- [ ] 进行中满不足 → 只播 `_进行中`，绝不是 `_完成结算`
- [ ] 满×4 → `_完成结算` → +500 金（一次）
- [ ] 交完再点老农 → 短循环；**不再** `_完成结算`；无金、无空桶 Tips
- [ ] 交完后再选帮（若仍露出 Offer）应失败/不可见——以报告为准
- [ ] 调 ResetQuest("Quest_003") 后可再次 Offer→帮→整条重来
- [ ] 无空跑：不会出现「结算对白播完却无金无特效」的循环

## 沟通风格
①结论 ②原因 ③用户检查清单 ④程序补充
```

---

## 给开发者

1. **先**新开 Agent，复制「侦探 Prompt」→ 出报告。  
2. 确认报告里 Q1～Q4（尤其短循环用哪张对白、Reset 清不清桶）。  
3. **再**开 Agent，复制「施工 Prompt」，`@` 上报告执行。  
4. 无日期系统时：用 Debug/临时菜单调一次 `ResetQuest` 验收「新一天」。
