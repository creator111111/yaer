# Village_Shop 贩卖无法卖出素材 — 架构溯源报告

> 日期：2026-09-20  
> 角色：架构侦探（只读，未改代码 / Prefab / 场景 / Database / CSV）  
> Unity：2020.3.48f1  
> 对照：0713 购买入包 / 货币扣款两份执行说明；`OPEN_QUESTIONS` 商店货币节 Q5。以现网代码为准。

---

## ① 结论一句话

**从未接入（设计债），不是回归。** 切到贩卖后点「决定」仍直接打 Log「出售结算未接入」并 return，不扣包、不加钱。

---

## ② 原因

大白话：贩卖页只做了「能看见列表、能填数量、能算合计」。真正把素材卖出去的结算，当初就故意没写。点「决定」时程序一看现在是贩卖页，打一句日志就回来了。所以背包和金币都不会动。不是列表坏了，也不是扣款 API 坏了。

### A. 点「决定」现网分支（钉死）

`ShopFormLogic.OnConfirmClick`：

```
点 BtnConfirm / Confirm
  → OnConfirmClick()
      if (!_isBuyTabActive)                    // 切贩卖后为 false
          ShopDebugLogger.LogSellNotImplemented()
          return                               // 不改存档、不播对白
      // 以下整段只给购买：CollectBuyLines → 堆叠预检 → TrySpendPlayerGold
      // → AddMainItem → SavePlayerBag → 清零 → Yes/No 对白
```

| 问题 | 现网答案 |
|------|----------|
| 切贩卖后 `_isBuyTabActive` | **false**（`SwitchToSellTab` 第 271 行） |
| 是否仍直接 Log 后 return | **是**（357–360 行） |
| 有没有半成品卖出函数被注释 / 旁路 | **没有。** 全文件无 `TrySell` / `CollectSellLines` / 出售结算体；出售侧只有切 Tab、收集行视图、合计、清零数量 |
| Console 固定出现 | **`[ShopDebug] 出售结算未接入`**（`ShopDebugLogger.LogSellNotImplemented`） |

历史：0713 货币闭环文档写明「出售为 P1；工期紧则只 Log」。`OPEN_QUESTIONS` 商店货币节 Q5：「出售是否同 PR？**否**：出售 Tab 点决定仅 Log『出售结算未接入』」。注释仍写「出售为 P1」「本阶段未接入」。因此是**从未接入**，不是「曾接入又断了」。

### B. 列表与合计（次要）

| 检查 | 现网 |
|------|------|
| 行从哪来 | Bake：`MainItemDatabase` 里 `MaterialItem` 且 `sellPrice >= 0`（`GetShopSellCandidates` / Bake 同条件）。不是运行时临时拼行，是场景里 Bake 死行 |
| 单价 | Bake 写 `ShopBarRowView.bakedPrice = entry.sellPrice`；Awake 赋给 `Price`。合计用 `Price`，即卖价 |
| 数量 | 出售行也挂 `ShopBuyRowQuantityInput`（组件名带 Buy，买卖共用）；`QuantityForTotal` 参与 `SumRowTotals` |
| 切 Tab | `SwitchToSellTab` → `ResetAllSellQuantityInputs` + `RefreshTotal2`；并打 Log「切换到出售页」 |

合计公式：`GetCurrentSellTotal` = Σ(`QuantityForTotal` × `Price`)，只扫 `_sellRowViews`。

若截图三行肉眼以为合计 60、界面却是 68：先对每行 Bake 的 `Price` 与填写数量；**不要用「总计不对」代替「卖不出去」**。卖不出去的主因是 A 节的提前 return。

### C. 出售应对称的购买顺序（方案，本阶段不改代码）

购买现网：

```
收集 qty>0 → 预检堆叠 → TrySpendPlayerGold(total) → AddMainItem×N → SavePlayerBag
→ 清零 → RefreshTotal2 → Yes/No 对白
```

出售应对称：

```
收集 qty>0（扫 _sellRowViews，仿 CollectBuyLinesWithQuantity）
→ 预检背包每行 held ≥ qty（整单；不够则失败、一行都不扣）
→ TryRemoveMainItem×N（全部成功后再往下）
→ GetPlayerGoldData().AddGold(total) + QuestManager.SavePlayerGold()
→ QuestManager.SavePlayerBag()（或现有 SavePlayerBag 私有方法）
→ ResetAllSellQuantityInputs + RefreshTotal2
→ （对白：产品未定，见 OPEN）
```

| 问题 | 答案 |
|------|------|
| 扣素材 API | `PlayerBagData.TryRemoveMainItem(itemId, count)`。不够返回 false、不改数量。须**先整单预检**再逐行扣，禁止「卖到没有」 |
| 加金币 API | 无 `TryAddPlayerGold` 门面。对齐任务发奖：`GetPlayerGoldData().AddGold(total)` 后立刻 `SavePlayerGold()`。不要只改内存 |
| 落盘顺序 | 建议：**先扣包（内存）→ 加金币并 SaveGold → SaveBag**。只存一边读档会对不上（有果没钱 / 有钱还有果）。购买是先扣钱落盘再入包再存包；出售对称为先出包再加钱，两边都要 Save |
| 成败对白 | `TryNotifyPurchaseDialogue` / GSM `TryTriggerPurchaseResult` 现网只服务购买 Yes/No。OPEN 另节 Q6 已写「出售播 No？**否**」。出售成功要不要播 Yes、失败播什么：**产品没说**，写入 OPEN，**不要擅自接 ShopYes** |

---

## ③ 用户需要做什么

1. 正规从 InitScene 进游戏，进村商店。  
2. 点「贩卖 / SELL」。Console 应有 `[ShopDebug] 切换到出售页`。  
3. 在素材行填数量（qty>0），看 Total2 是否跟着变。  
4. 点「决定」。  
5. **自证主因**：Console 必须出现 **`[ShopDebug] 出售结算未接入`**。  
6. 再对背包素材数量和金币：应与点决定前相同（现网不会改）。  
7. 施工后再测：qty>0 整单出包、金币按卖价增加且读档仍在、数量清零、Total2 归零；背包不够则整单失败、不扣半单；切回购买，原购买行为不变。

---

## ④ 给施工员的补充

### 改哪些 / 不要改哪些

| 全路径 | 做什么 |
|--------|--------|
| `Assets/Scripts/Game/GameRuntime/UI/FormLogic/Shop/ShopFormLogic.cs` | **主改。** `_isBuyTabActive == false` 时走出售结算，删掉「仅 Log 就 return」的占位 |
| `Assets/Scripts/Game/GameRuntime/UI/FormLogic/Shop/ShopDebugLogger.cs` | 可加出售成功 / 背包不足 Log；保留或改写 `LogSellNotImplemented`（接入后不应再被正常路径调用） |
| `QuestManager` / `PlayerGoldData` / `PlayerBagData` | **尽量不改。** 复用 `TryRemoveMainItem`、`AddGold`、`SavePlayerGold`、`SavePlayerBag` |
| 购买分支（`OnConfirmClick` 里 `_isBuyTabActive == true` 整段） | **一行语义都不要改**；含 `bypassGoldCheckForBagJoint` 默认值 |
| Bake / `MainItemDatabase` / Prefab / 场景 | **不重 Bake、不重写 UI** |
| `Village_ShopYes` / `Village_ShopNo` | **本期不要擅自接到出售**，除非产品先答 OPEN |

### 推荐方案（只此一种）

在 `OnConfirmClick` 的出售分支实现对称闭环：

1. 仿 `CollectBuyLinesWithQuantity` 扫 `_sellRowViews`，只收 qty>0。  
2. 全 0 → 打现有零数量 Warning，return（与购买一致）。  
3. 预检每行 `GetMainItemCount >= qty`；任一行不够 → Warning、整单失败、不改存档。  
4. 再逐行 `TryRemoveMainItem`（预检过仍失败则打 Error 并中止；理想情况不应发生）。  
5. `AddGold(total)` + `SavePlayerGold()` + `SavePlayerBag()`。  
6. `ResetAllSellQuantityInputs` + `RefreshTotal2`。  
7. 对白：默认**不播**；等 OPEN 拍板后再接。

购买代码路径保持独立，不要把出售行塞进 `CollectBuyLinesWithQuantity`。

### 否决方案

- 先只加钱不扣包：读档后白嫖金币，素材还在。  
- 点决定直接走购买代码：会按买价扣金币并往包里加素材，和贩卖相反。

### OPEN 里要不要改口 Q5

要。0713「出售是否同 PR？否」是当时工期边界，现网仍是占位 Log。本票起应改为：**出售结算另开施工，接入真实出包加币**；Q5 从「待确认 / 否」改为「本期要做」。

出售成败对白产品未定，追加独立 OPEN 节，不要改购买 Yes/No 规则。

### 会误伤的其它场景

按推荐方案（只扩出售分支）：**无。** 购买旁路与 Yes/No 不动。
