# Cursor Agent Prompt · 商店：无法正常卖出素材

> **角色**：【架构侦探】只读查清「贩卖」为什么卖不出去；报告通过后再施工  
> **日期**：2026-09-20  
> **场景 / UI**：村商店 `Village_Shop` · 商店面板「贩卖」页 +「决定」  
> **现象（用户截图 + 测试）**：已切到「贩卖」，列表里有虫喙 / 藤蔓果 / 史莱姆核等素材，点「决定」后**无法正常卖出**  
> **产品期望（钉死）**：贩卖页填数量 → 点决定 → **扣背包素材、加金币**，和购买页「扣钱入包」对称；卖成后数量清零、合计刷新  
> **不是**：改购买逻辑；改老板娘立绘/对白时序；改货单烘焙工具；改素材图标美术  
> **报告落盘**：`Assets/Doc/执行文档/0920/Village_Shop_贩卖无法卖出素材_架构溯源报告.md`

把下面「侦探」整段交给 Cursor Agent。没写清「是从未接入，还是接入后坏了；卖出要改哪几个 API」之前，不要施工。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 用户白话

> 商店测过了，素材卖不出去。截图在「贩卖」页，列表有虫喙、藤蔓果、史莱姆核，底下有总计和「决定」。

### 预扫假说（可推翻，勿当结论）

0713 / OPEN_QUESTIONS 曾明确写过：

| 来源 | 写了什么 |
|------|----------|
| `OPEN_QUESTIONS.md` 商店货币节 Q5 | 出售 Tab 点决定 **仅 Log「出售结算未接入」**，不同 PR |
| `ShopFormLogic.OnConfirmClick` | `_isBuyTabActive == false` 时调 `ShopDebugLogger.LogSellNotImplemented()` 然后 **return**，不改背包、不加钱 |
| 0713 购买入包文档 | 本阶段**不做**出售出包 |

所以最可能是：**卖出从未接上，不是突然坏了。**  
侦探仍须用现网代码再证一遍（防止后来有人半接入又坏了）。若仍是故意未接入，报告要写成「根因 = 未实现」，并给出与购买对称的最小接入方案，而不是只写「已知未做」就结束。

### 截图里顺带核对（次要，勿当主因）

贩卖页三行大致是：虫喙 5×1、藤蔓果 5×1、史莱姆核 5×10 → 若合计应是 **60**，截图「总计」显示 **68**。  
可能是旧购买合计没清、DigitStrip 显示错、或某行数量/单价读错。主因仍先钉「决定」有没有结算；合计不对单独写一行。

### 词不要混

| 词 | 意思 |
|----|------|
| **购买** | 花钱，道具进背包（现网已通） |
| **贩卖 / 出售** | 素材出背包，金币增加（本期要查的） |
| **决定** | 成交按钮；购买与出售共用，靠当前 Tab 分支 |

### 禁止

- 本阶段不改代码、不改 Prefab、不改 Database。  
- 不要为了卖出把购买扣款旁路重新打开。  
- 不要建议在 Update 里轮询背包。  
- 设计说不清的写入 `Assets/Doc/OPEN_QUESTIONS.md`。

---

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Project_context.md
@Assets/Doc/02_SYSTEM_SPEC.md
@Assets/Doc/OPEN_QUESTIONS.md
@Assets/Doc/执行文档/7月/0713/商店背包联合_购买入包可见_架构溯源与施工执行说明.md
@Assets/Doc/执行文档/7月/0713/Shop_货币金币对接_购买扣款闭环_架构溯源与施工执行说明.md
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Shop/ShopFormLogic.cs
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Shop/ShopDebugLogger.cs
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Shop/ShopBarRowView.cs
@Assets/Scripts/Game/DataTable/MainItem/MainItemDefProvider.cs
@Assets/Scripts/Game/GameMgr/Component/Archive/ArchiveDataClass/Quest/QuestManager.cs

你现在是【架构侦探】。Unity 2020.3.48f1 / C#。
禁止修改任何代码、Prefab、场景、Database、CSV。只读扫描 + 写溯源报告。

报告写入：
Assets/Doc/执行文档/0920/Village_Shop_贩卖无法卖出素材_架构溯源报告.md

---

## 背景（策划白话）

村商店「贩卖」页已经能看到素材列表（虫喙、藤蔓果、史莱姆核等），玩家填数量后点「决定」，测试发现**无法正常卖出素材**。

要查清：
1. 是从来就没接出售结算，还是接过又坏了
2. 点「决定」时现网到底走哪条分支、Console 会出现什么
3. 若要修好：扣哪份背包、加哪份金币、怎么和购买对称、会不会误伤购买

期望修好后：
- 贩卖页 qty>0 的素材整单出包
- 按卖价加金币并落盘
- 数量清零、总计刷新
- 背包不够则整单失败、不扣半单
- 购买页行为不变

---

## 必读 / 优先扫描

历史文档只作对照，以现网代码为准：

- `OPEN_QUESTIONS.md` 商店货币节 Q5（出售是否同 PR）
- 0713 购买入包 / 货币扣款两份执行说明
- `ShopFormLogic.OnConfirmClick` / `SwitchToSellTab` / `GetCurrentSellTotal`
- `ShopDebugLogger.LogSellNotImplemented`
- `MainItemDefProvider.GetShopSellCandidates` / `TryGetSellPrice`
- `QuestManager` 金币增减 API（买用 `TrySpendPlayerGold`；卖要用哪一个加钱）
- `PlayerBagData` 扣道具 API（有没有 `TryRemoveMainItem` / 同类）

### A. 先钉「决定」在贩卖页做了什么

写出从点「决定」到函数返回的完整分支：

1. `_isBuyTabActive` 在切贩卖后是不是 false
2. 是否仍直接 `LogSellNotImplemented` 后 return
3. 有没有半成品的卖出函数被注释、被旁路、被别的按钮占用
4. Console 固定会出现哪句 Log（方便用户自证）

结论必须二选一写死：
- **从未接入**（设计债），或
- **曾接入但现网坏了**（回归；写清断在哪一环）

### B. 列表与合计是否正常（次要）

对贩卖列表：

| 检查 | 要回答 |
|------|--------|
| 行从哪来 | Database MaterialItem + sellPrice≥0？还是 Bake 死行？ |
| 单价 | 每行 `ShopBarRowView.Price` 是不是 SellPrice |
| 数量 | 输入是否写入 `QuantityForTotal` |
| 总计 68 | 截图若三行合计应为 60，68 从哪来？切 Tab 时有没有 Reset + RefreshTotal2 |

合计错可以记为次要问题；不要用「总计不对」代替「卖不出去」的主因。

### C. 卖出闭环应对齐购买的哪几步

对照购买现网顺序，画出出售应对称的顺序（只画方案，本阶段不改）：

```
购买现网（摘要）：
  收集 qty>0 → 预检堆叠 → 扣金币 → AddMainItem → SaveBag → 清零 → 成败对白

出售应对称：
  收集 qty>0 → 预检背包够不够 → 扣素材 → 加金币 → 双落盘？ → 清零 → （对白？）
```

必须回答：

1. 扣素材用哪个 API；不够时整单失败还是卖到没有
2. 加金币用哪个 API；要不要立刻 `SaveSpcData` / SaveGold
3. 背包 Save 与金币 Save 谁先谁后；只存一边会不会读档对不上（对照老农打水「任务存了包没存」的教训）
4. 成败对白：现网 Yes/No 是不是只给购买用；出售要不要播、播哪段。若产品没说，写入 OPEN_QUESTIONS，不要擅自接 ShopYes

### D. 推荐一种最小改法

只推一种，并满足：

1. 贩卖能真实卖出素材并加钱
2. 购买链路一行都不改语义（旁路开关默认保持现网）
3. 不重写商店 UI，不重 Bake 全表
4. 背包不足 / 数量全 0 有明确失败路径

另外两种各用一句话否决（例如「先只加钱不扣包」「点决定直接走购买代码」）。

---

## 报告结构（固定）

① 结论一句话（未接入 / 回归；主因一句）  
② 原因（大白话 + OnConfirmClick 分支调用链）  
③ 用户需要做什么（切贩卖、填数量、点决定、看 Console 是否出现「出售结算未接入」；再对一下背包数量和金币）  
④ 给施工员的补充：改哪些文件、不要改哪些、推荐方案、否决方案、OPEN 里要不要改口 Q5

发现设计说不清的（尤其出售成败对白），追加到 `Assets/Doc/OPEN_QUESTIONS.md`，不要改核心设计。
```

---

## 施工员提示词（侦探报告落盘并确认方案后再贴）

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0920/Village_Shop_贩卖无法卖出素材_架构溯源报告.md
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Shop/ShopFormLogic.cs
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Shop/ShopDebugLogger.cs

你现在是【施工员】。Unity 2020.3.48f1 / C#。
只按溯源报告推荐方案接入「贩卖」真实结算。不要改购买语义，不要重 Bake 列表，不要改老板娘对白图（除非报告明确要求）。

目标：
- 贩卖页 qty>0 点「决定」：扣对应素材、按卖价加金币、落盘
- 背包不够或数量全 0：整单失败，不出现半扣
- 成功后数量清零、总计刷新
- 购买页原有扣款入包、Yes/No 对白保持原样

限制：
- 禁止在 Update 里轮询
- 注释说明为什么出售不能再只打「出售结算未接入」就 return
- 复杂分支写清替代方案（为什么不复用购买入包函数硬改）
- 施工说明写入：`Assets/Doc/施工说明/0920/Village_Shop_贩卖无法卖出素材_施工说明.md`
- 若 OPEN Q5 要从「未接入」改成「已接入」，施工后提醒更新 OPEN，或按报告自行补一行

完成后用大白话给验收清单：贩卖页卖 1 个藤蔓果，看背包少 1、金币按卖价增加；再测背包为 0 时点决定应失败。
```
