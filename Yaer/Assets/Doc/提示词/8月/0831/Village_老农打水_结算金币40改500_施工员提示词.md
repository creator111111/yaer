# Cursor Agent Prompt · 老农打水结算金币：40 → 500

> **角色**：【施工员】最小化改配置；必要时先只读核对发奖路径（勿写长侦探报告）  
> **日期**：2026-08-31  
> **任务 / 场景**：`Quest_003`「老农的浇地水」· `Village_KenMuNi1` · 交满桶×4 结算  
> **产品拍板（钉死）**：结算应得 **500 金币**；现网 **40 金币不对**  
> **背景**：0830 施工说明写明 Gold **40（金额待策划）**；策划现已定为 **500**  
> **本阶段**：只改发奖数额与必要同步源；禁止顺手改桶逻辑 / 接任务 / 井交互 / Tips  
> **说明落盘**：`Assets/Doc/施工说明/0831/Village_老农打水_结算金币40改500_施工说明.md`

把下面「施工」整段复制给 Cursor Agent（Agent Mode）执行。

---

## 提示词助手预梳理（施工须核实）

### 已知锚点（助手预扫 · 勿当唯一真相）

| 项 | 预扫 |
|----|------|
| 任务行 | `Assets/GameRes/Config/QuestConfig/QuestConfig.json` → `questId: "Quest_003"` |
| 现网奖励 | `rewards[0].type = "Gold"`, `amount = "40"` ← **要改成 `"500"`** |
| 发奖链路 | `QuestTurnInAction` → `QuestManager` 发奖读配置表 `rewards`（首版仅 Gold） |
| 历史文档 | `施工说明/0830/Village_老农打水_空满桶道具与井交互及接任务_施工说明.md` 已写「暂定 40」 |

### 可能还要同步的地方（施工自查，有则改、无则记「无」）

| 候选 | 动作 |
|------|------|
| `QuestConfig.json` | ✅ 必改 `amount` 40→500 |
| Editor / ScriptableObject / 其它 Quest 表副本 | 若工程另有同内容资产，与 JSON **对齐**；只改一处会漏 |
| 对话 Prefab / `_完成结算` 里硬编码加金 | 若存在与表重复的 `AddGold(40)`，**删硬编码或改为 500**（优先以 QuestConfig 为准，避免双发） |
| UI / Tips 文案若写死「40」 | 有则改；无「获得金币」Tip 则不动 Tips |

### 本期边界

| 做 | 不做 |
|----|------|
| ✅ `Quest_003` 结算 Gold **500** | ❌ 改目标件数、道具、可重复标记 |
| ✅ 核对 TurnIn 只发配置数额（不双发） | ❌ 重做结算对白 / 立绘 / 井逻辑 |
| ✅ 写短施工说明 + 验收步骤 | ❌ 改其它任务（Quest_001/002）金额 |

### 严禁

- 改完 JSON 却运行时仍读旧缓存/未重新加载配置仍显示 40  
- 对话图再手动 `AddGold(500)` 导致与 TurnIn **发两次**  
- 把可重复任务改成不可重复、或动满桶×4 目标  

---

## 施工 Prompt（复制给 Agent）

```text
你是【施工员】。Unity 2020.3.48f1 / C#。最小化修改。默认中文。

@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Project_context.md
@Assets/Doc/02_SYSTEM_SPEC.md

## 产品目标
老农打水任务（Quest_003「老农的浇地水」）交满桶×4 结算时，应发 **500 金币**。
现网是 **40 金币**，错误。0830 施工曾写「暂定 40、金额待策划」——现已拍板 **500**。

## 必读上下文
@Assets/GameRes/Config/QuestConfig/QuestConfig.json
@Assets/Doc/施工说明/0830/Village_老农打水_空满桶道具与井交互及接任务_施工说明.md
@Assets/Scripts/Game/GameMgr/Component/Archive/ArchiveDataClass/Quest/QuestManager.cs
（发奖：TurnIn 后按 configRow.rewards 发 Gold）

相关对白（只核对是否硬编码加金，无则勿改）：
@Assets/Dialog/Village_老农打水任务_完成结算.csv
（及对应 Prefab / Generated，路径按仓库惯例）

## 施工步骤
1. 定位 Quest_003 的 Gold amount=40，改为 **500**。
2. 全文检索 `Quest_003` / `老农的浇地水` / 与打水结算相关的 `AddGold` / 硬编码 `40`（限本任务语境），确认：
   - 发奖唯一来源是 QuestConfig（或改后唯一一致为 500）；
   - 无「配置 500 + 对话再加 40/500」双发。
3. 若存在 QuestConfig 的其它同步副本（asset / 烘焙物），与 JSON 对齐。
4. 落盘短说明：
   `Assets/Doc/施工说明/0831/Village_老农打水_结算金币40改500_施工说明.md`
   写清：改了哪些文件、发奖路径一句话、验收步骤、剩余风险。

## 验收清单（施工说明里照抄）
- [ ] 接 Quest_003 → 打满桶×4 → 老农完成结算对白结束
- [ ] 金币 **+500**（不是 +40）
- [ ] 交完可再接的流程下，**再交一次仍 +500**（若 repeatable=true 仍生效）
- [ ] Console 无异常；无双发（一次结算不应 +1000）

## 禁止
- 改桶道具、井交互、Tips 图、接任务选项
- 改 Quest_001 / Quest_002 奖励
- 大范围重构 QuestManager
- 发现设计不清时写入 Assets/Doc/OPEN_QUESTIONS.md，勿擅自改核心设计

## 沟通风格
① 结论一句话 ② 原因 ③ 用户检查清单 ④（可选）程序补充
```

---

## 给开发者

1. 新开 Agent 对话，切 **Agent Mode**。  
2. 复制上文「施工 Prompt」整段发送。  
3. 验收：交一次任务看金币是否 **+500**；再接再交确认仍是 500、不是双发。
