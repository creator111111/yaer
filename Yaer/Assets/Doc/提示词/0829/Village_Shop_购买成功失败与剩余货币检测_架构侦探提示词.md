# Cursor Agent Prompt · Village_Shop：购买成功/失败检测 + 剩余货币可读性

> **角色**：先【架构侦探】只读溯源，报告拍板后再【施工员】  
> **日期**：2026-08-29  
> **场景 / UI**：`Village_Shop` · `UI_Shop` / `ShopFormLogic`  
> **产品目标（白话）**：  
> 1. 查清项目里 **有没有货币系统**、钱存在哪、谁能读/扣  
> 2. 下一步要做 **购买成功 / 购买失败检测**（钱够成交、不够拒绝等）  
> 3. 明确 **能不能检测玩家剩余货币**，以及成功/失败后玩家/程序怎么感知  
> **本阶段**：只读；禁止改代码 / Prefab / 场景 / 存档  
> **报告落盘**：`Assets/Doc/执行文档/0829/Village_Shop_购买成功失败与剩余货币检测_架构溯源报告.md`

把下面「侦探」整段复制给 Cursor Agent（Agent Mode）执行。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 用户原问 → 侦探必须直接回答

| 问题 | 预扫假说（须证伪后写入结论） |
|------|------------------------------|
| 项目有货币系统吗？ | ✅ **有**：存档 `PlayerGoldData.gold`（键 `PlayerGold`）；任务发奖 `AddGold`；商店消费 `TrySpendGold` / `QuestManager.TrySpendPlayerGold` |
| 能检测剩余货币吗？ | ✅ **能**：`GetPlayerGoldData().gold` 或 `CanAfford(need)` |
| 购买成功/失败检测？ | ⚠️ **逻辑侧多半已有**（扣款失败 return、成功入包）；**玩家可见反馈**（Tips/UI）与 **旁路开关** 才是缺口假说 |

### 现网货币真源（勿另造第二钱包）

| 层 | 路径 / API | 作用 |
|----|------------|------|
| 存档 | `PlayerGoldData` | `gold`；`CanAfford`；`TrySpendGold`；`AddGold` |
| 门面 | `QuestManager` | `GetPlayerGoldData` / `TrySpendPlayerGold` / `SavePlayerGold` / 任务 `Grant…Gold` |
| 商店 | `ShopFormLogic.OnConfirmClick` | 合计 →（可选）扣款 → 入包 |
| 调试 | `ShopDebugLogger` | `LogInsufficientGold(need, have)` / 购买成功 Log |

**禁止**：商店 UI 自建 `int localGold`；在 `Update` 轮询扣款。

### 购买闭环与「旁路」假说（关键）

`ShopFormLogic` 预扫存在：

```csharp
[SerializeField] bool bypassGoldCheckForBagJoint = true; // 开发默认 true
```

| `bypass` | 行为假说 |
|----------|----------|
| **true（现网开发默认）** | **跳过** `TrySpendPlayerGold`，直接入包 → **验不出**真失败「钱不够」 |
| **false** | 走扣款；不足 → `LogInsufficientGold` + return（不入包） |

侦探必须：磁盘核实默认值；说明「要做成功/失败检测」时 **必须关旁路**（或提供正式检测模式），否则验收永远「假成功」。

### 成功 / 失败分支清单（须扩全）

点「决定」可能失败/成功的原因（侦探按代码补全表）：

| 结果 | 条件（假说） | 现网感知 | 玩家可见？ |
|------|--------------|----------|------------|
| 失败 | 数量全 0 | Console Warning | ? |
| 失败 | 堆叠将超 MaxStack | Console? | ? |
| 失败 | 金币不足（旁路关） | `LogInsufficientGold(need, have)` | ? 仅 Log |
| 失败 | 存档不可用 | Archive Log | ? |
| 失败 | 出售 Tab 未实现 | SellNotImplemented | ? |
| **成功** | 扣款（或旁路）+ 入包 + Save | `LogPurchaseIntoBag` | ? |

**本期产品「检测」含义要拍板**：

| 含义 | 说明 |
|------|------|
| **D1 · 程序可判定** | `bool` / 分支 / Console / Debug — 现网可能已够 |
| **D2 · 玩家可感知** | TipsPanel / 商店内文案 / 剩余金币数字刷新 — 0713 开放问题曾「常驻显示持有金币 · 本阶段不做」 |
| **D3 · 预检** | 点决定前 Total2 变红 / 决定按钮灰；用 `CanAfford(total)` | |

侦探推荐最小闭环（倾向：**关旁路 + D1 验收清单 + D2 用现成 Tips 最小提示**），写入开放问题等用户选。

### 「剩余货币」读法（给施工用）

```
剩余 = QuestManager.getInstance().GetPlayerGoldData()?.gold ?? （不可用）
能否买 = goldData.CanAfford(buyTotal)   // 或 TrySpend 前比较
扣款后剩余 = 再次读 gold（成功分支内）
```

侦探须写清：存档从哪来（GSM Archive vs GM fallback）、纯 UI 店从 InitScene 进是否可读。

### 关联旧文档（必对拍，标过时点）

| 文档 | 用途 |
|------|------|
| `0713/Shop_货币金币对接_购买扣款闭环_…` | 货币架构真源；当时「无 TrySpend」——**现网可能已施工，勿照抄旧缺口** |
| `0713/商店背包联合_购买入包可见_…` | 旁路 `bypassGoldCheckForBagJoint` 来源 |
| `0629/商店系统_策划拆解` §5.1 货币 | 产品：展示持有金币等 |
| `OPEN_QUESTIONS` 货币相关 Q | 常驻显示金币、失败 Tips 等 |

### 本期边界

| 做 | 不做 |
|----|------|
| ✅ 回答：有无货币、如何读剩余、成功/失败现网断在哪 | ❌ 新做第二套货币 |
| ✅ 列出检测方案 D1/D2/D3 + 最小施工清单 | ❌ 限购/多货币/钻石 |
| ✅ 旁路与正式检测关系 | ❌ 强制做完出售加币（可挂钩） |
| ✅ 剩余金币 UI 是否已有节点 | ❌ 重做 Total2 / Bake 列表 |

### 严禁（本阶段）

- 改代码 / Prefab / 场景 / 存档数据  
- 结论写「没有货币系统」却不扫 `PlayerGoldData`  
- 把旁路开启时的「必成功入包」当成正式购买成功检测已闭环  
- `Update` 里轮询金币  

---

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/02_SYSTEM_SPEC.md
@Assets/Doc/执行文档/7月/0713/Shop_货币金币对接_购买扣款闭环_架构溯源与施工执行说明.md
@Assets/Doc/执行文档/7月/0713/商店背包联合_购买入包可见_架构溯源与施工执行说明.md
@Assets/Doc/执行文档/6月/0629/商店系统_策划拆解_执行说明.md
@Assets/Scripts/Game/GameMgr/Component/Archive/ArchiveDataClass/Quest/PlayerGoldData.cs
@Assets/Scripts/Game/GameMgr/Component/Archive/ArchiveDataClass/Quest/QuestManager.cs
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Shop/ShopFormLogic.cs
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Shop/ShopDebugLogger.cs
@Assets/GameRes/Scenes/Village_Shop.unity

你现在是【架构侦探】。Unity 2020.3.48f1 / C#。
禁止修改任何代码、Prefab、场景、CSV、存档。只读扫描 + 写「购买成功/失败与剩余货币检测」溯源报告。

---

## 背景（策划白话）

1. 想确认：游戏里 **有没有货币**，钱够不够能不能查。  
2. 下一步要做商店 **买成了 / 买失败** 的检测（尤其钱不够）。  
3. 本阶段只摸清：现成 API、现网点「决定」走哪、旁路有没有挡住真检测、玩家能不能看到结果、最小还要补什么。

---

## 侦探任务清单

### A. 钉死货币系统（直接答用户）

| 问 | 答（必填） |
|----|------------|
| 有没有货币系统？ | 有 / 无 + 一句话 |
| 唯一真源字段？ | 类名.字段 / 存档键 |
| 如何读剩余货币？ | API 调用链 |
| 如何判断够不够？ | `CanAfford` / 比较 / `TrySpend` |
| 谁在加钱？谁在扣钱？ | 表 |

### B. 钉死购买成功/失败现网链路

画出 `OnConfirmClick` 分支树（白话即可）：

```
点决定
  → 出售？…
  → 数量 0？…
  → 背包空？…
  → 堆叠超？…
  → bypass？跳过扣款 : TrySpend → 失败？…
  → 入包 + Save → 成功
```

每一失败/成功出口写清：**返回值或仅 Log？有无 Tips？是否改存档？**

### C. 旁路与「能检测吗」

| 项 | 填 |
|----|-----|
| `bypassGoldCheckForBagJoint` 默认 | |
| 旁路开时：金币不足会失败吗？ | |
| 正式做成功/失败检测的前置 | 关旁路？另开检测模式？ |
| Console 是否已能打印 need/have？ | |

### D. 剩余货币 UI / 反馈缺口

| 项 | 有无 | 路径 |
|----|------|------|
| 商店常驻显示持有金币 | | |
| 购买成功 Tips / 动画 | | |
| 购买失败 Tips（钱不够） | | |
| 菜单/其它处显示金币 | | |

对照 0629「需展示当前持有金币」、0713 OPEN Q：本期建议做哪些。

### E. 方案拍板（检测落地）

| 方案 | 内容 | 倾向 |
|------|------|------|
| **S1 · 最小程序检测** | 关旁路；保留/补齐 Debug Log；验收用 Console + 读档 gold | |
| **S2 · + 玩家 Tips** | 失败/成功弹现成 SystemTips（或等价） | |
| **S3 · + 常驻剩余金币** | UI 数字绑 `gold`，购买后 Refresh | |
| **S4 · + 预检** | `CanAfford` 灰按钮 / Total2 变色 | |

推荐组合（如 S1+S2）；写清不改货币中枢。

### F. 最小施工清单（本阶段不执行）

| # | 动作 | 优先级 |
|---|------|--------|
| 1 | 正式检测前：`bypassGoldCheckForBagJoint = false`（或文档化验收开关） | P0 |
| 2 | 确认失败分支可读剩余 `have`（已有则免改） | P0 |
| 3 | 玩家可见成功/失败（按拍板 S2） | P0/P1 |
| 4 | 剩余货币展示 Refresh（按拍板 S3） | P1 |
| 5 | 预检（S4） | P2 |
| 6 | 短技术说明 / 更新旧 0713「无扣款」过时句 | P2 |

**排除**：新货币类型；出售完整经济（除非顺带一句挂钩）；改价表结构。

### G. 验收清单

| # | 操作 | 通过 |
|---|------|------|
| 1 | 读 `PlayerGoldData.gold`（或 API） | 能得到剩余货币数字 |
| 2 | 钱够买 → 决定 | 成功：金币减少、入包、有检测信号（Log/Tips） |
| 3 | 钱不够 → 决定（旁路关） | 失败：金币不变、不入包、有失败信号（含 need/have） |
| 4 | 旁路开 | 文档标明「非正式货币验收」 |
| 5 | 数量 0 / 堆叠超 | 各失败分支可区分 |

### H. 开放问题

- 玩家失败提示用 Tips 还是店内文案？  
- 是否必须常驻显示剩余金币？  
- 旁路默认改 false 还是保留开发开关？  
- 成功是否也要 Tips，还是只刷新背包/金币数？  

---

## 输出要求

写入：`Assets/Doc/执行文档/0829/Village_Shop_购买成功失败与剩余货币检测_架构溯源报告.md`

结构（MASTER 四段式 + 表）：

① 结论一句话（有货币吗 + 剩余怎么读 + 成功/失败现网到哪 + 还缺什么）  
② 原因（通俗：钱包在存档；旁路会让「失败测不到」；Log≠玩家感知）  
③ 用户检查清单（怎么自己看剩余金币、怎么测够/不够）  
④ 给程序：API 表 + OnConfirm 分支表 + 方案 S1～S4 + 最小 diff + 开放问题

口头汇报同样用 MASTER 四段式；**前三句必须直接回答「有没有货币 / 能不能读剩余 / 成功失败检测现网状态」**。
```

---

## 施工员续跑（侦探报告 + 用户选定 S 方案后再贴）

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0829/Village_Shop_购买成功失败与剩余货币检测_架构溯源报告.md
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Shop/ShopFormLogic.cs
@Assets/Scripts/Game/GameMgr/Component/Archive/ArchiveDataClass/Quest/PlayerGoldData.cs
@Assets/Scripts/Game/GameMgr/Component/Archive/ArchiveDataClass/Quest/QuestManager.cs

你现在是【施工员】。用户已选定检测方案：【填写 S1/S2/S3…】。
只按报告实现购买成功/失败可检测，并正确使用剩余货币 API。

必须遵守：
- 货币真源仅 PlayerGoldData / QuestManager，禁止第二钱包；
- 正式货币验收须关闭 bypass（或按报告开关）；
- 失败不扣款不入包；成功才 Save；
- 禁止 Update 轮询；代码含详细注释；重要取舍写清原因。

提交说明：改了哪些文件、如何验收够/不够、剩余货币如何读、未做项。
```
