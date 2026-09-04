# Village_Shop — 没钱仍能购买 + 成败对白验收 — 架构溯源报告

**文档版本**：v1.0（2026-08-30）  
**文档性质**：【架构侦探】只读溯源 + 旁路/验收拍板（**本阶段未改代码 / Prefab / 场景 / CSV**）  
**Unity**：2020.3.48f1  
**场景**：`Village_Shop` · 点「决定」购买  
**用户现象**：没有钱仍会购买成功；失败路径测不到 / 不成交反馈不对  
**对白资产（产品既定）**：  
- 成功：`Assets/GameRes/Prefabs/Dialogue/Village_ShopYes.prefab`  
- 失败：`Assets/GameRes/Prefabs/Dialogue/Village_ShopNo.prefab`  
- **首次进店**：`Village_ShopStart`（**≠** 购买成功，本期勿改接线）

关联：`bypassGoldCheckForBagJoint` · `OnConfirmClick` · `TryTriggerPurchaseResult` · `TrySpendPlayerGold` · `ShopPanel` / 场景 `UI_Shop` 序列化 · `0829/...ShopYes_ShopNo_架构溯源报告.md`

---

## ① 结论一句话

**「没钱仍能买成」主因是货币旁路 `bypassGoldCheckForBagJoint` 现网为 true（脚本默认 + `ShopPanel.prefab`=1 + `Village_Shop.unity` 场景实例=1 三处一致），跳过 `TrySpendPlayerGold` 后直接入包并播 Yes——扣款 API / 钱包读档正常，不是货币系统坏了。成败对白已接线（0829 施工已落地）：钱够/旁路入包 → `Village_ShopYes`；仅旁路关且金币不足 → `Village_ShopNo`。正式默认拍板：旁路改 false（三处同步）；关旁路后才能验收失败对白。购买成功不得播 `Village_ShopStart`。**

---

## ② 原因（通俗）

开发时怕「没钱卡死入包联合验收」，商店上留了一道「先别管钱、直接塞包」的开关，而且**脚本默认、UI Prefab、商店场景里的 UI_Shop 全开着**。

玩家钱包再空，点「决定」也会：跳过扣款 → 东西进包 → 播「谢谢惠顾」那条成功对白。  
失败那条「哎哟，你好像没什么钱呢」**代码里有，但旁路开着永远走不到**——所以感觉「并不会购买失败」，不是 `Village_ShopNo` Prefab 坏了。

若有人把成功对白说成 `Village_ShopStart`：那是**首次进店**对白，和买成买败无关，别接到购买出口。

---

## ③ 用户检查清单（关旁路后怎么验）

| # | 前置 | 操作 | 通过 |
|---|------|------|------|
| 1 | 旁路 **关**（Inspector/`ShopFormLogic` / 场景 UI_Shop / Prefab 均为 false）；金币 **&lt;** Total2 合计 | 点「决定」 | **不入包**；金币不变；播 **ShopNo**（店：「哎哟，你好像没什么钱呢。」）；Console 有 `LogInsufficientGold` + `[ShopSpecial] TriggerStory Village_ShopNo` |
| 2 | 旁路 **关**；金币 **≥** 合计 | 点「决定」 | 扣款入包；播 **ShopYes**（店：「谢谢惠顾~~」）；`[ShopSpecial] … Village_ShopYes` |
| 3 | 旁路 **开**；金币再少 | 点「决定」 | **仍入包 + Yes**（旁路设计预期）；**不得**当成「失败对白已验收」 |
| 4 | 数量全 0 | 点「决定」 | 仅 Warning；**不播 No** |
| 5 | 堆叠将超 | 点「决定」 | `LogStackOverflow`；**不播 No**（文案是没钱，不符） |
| 6 | Console | — | 不足时故事名 = Prefab 根名 `Village_ShopYes` / `Village_ShopNo`；无 Missing Prefab |

**如何把金币刷到不够（现网工具已齐）**

1. Play → `Tools / Debug / Player Gold Tool…`  
2. 看当前余额与商店 Total2；点 **「减少 Spend」** 把余额减到 **&lt; 合计**（不足整笔失败、不钳成 0）  
3. **务必先关旁路**，再点「决定」验 ShopNo  

临时手工：进店后选中场景 `UI_Shop` → Inspector 取消勾选 `bypassGoldCheckForBagJoint`（仅当次 Play 有效；正式应改磁盘默认）。

---

## ④ 给程序

### A. 根因树（假说证伪）

| # | 假说 | 结果 | 证据 |
|---|------|------|------|
| 1 | `bypassGoldCheckForBagJoint == true` | ✅ **主因** | 脚本默认 `true`；`ShopPanel.prefab` 与 `Village_Shop.unity` 均为 `1` |
| 2 | `TrySpendPlayerGold` 永不失败 / 读错钱包 | ❌ 否 | `PlayerGoldData.TrySpendGold`：`gold < amount` → false、不改写；`QuestManager` 失败不 Save |
| 3 | UI「没钱」≠ 存档真金币 | ❌ 非本期主因 | `MenuFormLogic.RefreshMoneyFromArchive` 读 `GetPlayerGoldData().gold`；与扣款同源 |
| 4 | 成败对白反了 / 总走 success | ❌ 对白逻辑对；**被旁路挡死失败支** | `TryNotifyPurchaseDialogue(false/true)` → `TryTriggerPurchaseResult` → Yes/No 常量正确 |

**一句话钉死**：旁路开 → 永远假成功（入包+Yes）；关旁路后扣款失败支可走、可播 No。

### B. `OnConfirmClick` 现网分支（含对白 · 对照 0829）

```
OnConfirmClick
  ├─ 出售 Tab          → LogSellNotImplemented     → ❌ 不播
  ├─ 数量全 0 / total≤0 → LogZeroQuantityWarning   → ❌ 不播
  ├─ 背包空            → LogArchiveUnavailable     → ❌ 不播
  ├─ 堆叠将超          → LogStackOverflow          → ❌ 不播 No
  ├─ bypass=false
  │    ├─ goldData 空  → LogArchiveUnavailable     → ❌ 不播
  │    └─ TrySpend 失败 → LogInsufficientGold
  │                       → TryNotifyPurchaseDialogue(false)
  │                       → TryTriggerPurchaseResult(false)
  │                       → Village_ShopNo           ✅ 【已接线】
  ├─（旁路=true 跳过扣款  或  扣款成功）
  ├─ AddMainItem + SaveBag + LogPurchase + 清数量
  └─ TryNotifyPurchaseDialogue(true)
       → TryTriggerPurchaseResult(true)
       → Village_ShopYes                              ✅ 【已接线】
```

| 对照 0829 报告 | 状态 |
|----------------|------|
| GSM 常量 Yes/No + `TryTriggerPurchaseResult` | ✅ **已落地**（0829「仍缺接线」过时） |
| `OnConfirmClick` 成功/不足两处挂 Trigger | ✅ **已落地** |
| 货币真检可验收 | ❌ **仍被旁路挡死**（正式默认未关） |
| 经 GSM Special，UI 不直开 TriggerStory | ✅ 保持 |

### C. Prefab 名对拍（防 Start 误用）

| Prefab | 根 `m_Name` | 用途 | 购买链路引用 |
|--------|-------------|------|--------------|
| `Village_ShopStart` | ✅ 同名 | **首次进店** | ❌ **不**当购买成功；`ShopStartStoryName` 专用 |
| `Village_ShopYes` | ✅ 同名 | 购买成功短反应 | ✅ `PurchaseSuccessStoryName` |
| `Village_ShopNo` | ✅ 同名 | 金币不足短反应 | ✅ `PurchaseFailInsufficientGoldStoryName` |

**钉死**：购买成功出口 = **ShopYes**；用户若写 ShopStart，属名称混用，施工勿改成 Start。

### D. 旁路真值（谁说了算）

| 位置 | 当前值 | 运行时影响 |
|------|--------|------------|
| `ShopFormLogic.cs` 字段默认 | `true` | 新建组件时的默认 |
| `Assets/GameRes/Prefabs/UI/ShopPanel.prefab` | `1` | GF/Prefab 打开时 |
| `Assets/GameRes/Scenes/Village_Shop.unity`（场景 UI_Shop） | `1` | **正规进店主路径**（场景常驻 UI） |

Unity 序列化：**场景 / Prefab 实例值优先于脚本字段初始值**。只改脚本默认、不改场景 → 进店仍旁路开。

### E. 旁路策略拍板（给施工）

| 项 | 拍板 |
|----|------|
| 运行时正式默认 | **`bypassGoldCheckForBagJoint = false`** |
| 同步改盘 | **脚本默认 + `ShopPanel.prefab` + `Village_Shop.unity` 三处一致为 false/0** |
| 开发联调 | **保留** SerializeField，可手开；注释写清「开=验不出钱不够失败 / ShopNo」 |
| 仅 Editor 开关 / 开发菜单 | **可选 P1**（见开放问题 Q1）；本期 P0 先关默认即可 |
| 旁路仍 true 时「钱不够」点决定 | 仍入包 + Yes（设计如此）；**不得**宣称失败对白已验 |

### F. 扣款 API（确认健康）

| API | 行为 |
|-----|------|
| `PlayerGoldData.CanAfford(amount)` | `amount>0 && gold>=amount` |
| `PlayerGoldData.TrySpendGold(amount)` | 不足 → false、**不改** gold |
| `QuestManager.TrySpendPlayerGold(amount)` | TrySpend 失败不 Save；成功才 `SavePlayerGold` |

关旁路后失败分支会真实走到，无需重做货币系统。

### G. 最小施工清单（本阶段不执行）

| # | 动作 | 优先级 |
|---|------|--------|
| 1 | `ShopFormLogic`：`bypassGoldCheckForBagJoint` 默认改为 **false**；注释改为「正式默认关；联调可手开」 | **P0** |
| 2 | `ShopPanel.prefab`：`bypassGoldCheckForBagJoint: 0` | **P0** |
| 3 | `Village_Shop.unity` 场景 UI_Shop：同上改为 **0**（漏改则正规进店仍假成功） | **P0** |
| 4 | 关旁路后验收：钱不够 → 不入包 + ShopNo；钱够 → 扣款入包 + ShopYes | **P0** |
| 5 | 验收说明写入提交说明：刷金减少工具 + 旁路开时勿宣称失败已验 | **P0** |
| 6 | 成败对白接线 | ✅ **已齐，勿重做** |
| 7 | 成功对白改 ShopStart / 出售成交对白 / 堆叠播 No | ❌ 不做 |

**预期 diff（仅默认值）**

- `ShopFormLogic.cs`（默认 + 注释）  
- `ShopPanel.prefab`  
- `Village_Shop.unity`  
- **一般不改** `Village_ShopSceneManager` / Yes·No Prefab / `PlayerGoldData`

### H. 验收清单（施工后）

同 §③；额外确认：

| # | 检查 |
|---|------|
| A | 磁盘三处旁路均为 false 后，**不**依赖当次 Inspector 手关也能测出 No |
| B | 旁路故意再勾上 → 仍假成功（证明开关仍可用） |
| C | Console 无把 ShopStart 当成购买成败故事名 |

### I. 开放问题

| ID | 问题 | 倾向 | 状态 |
|----|------|------|------|
| Q1 | 旁路是否改为仅开发菜单 / Editor-only，而不是挂在正式 Prefab/场景？ | P1 可选；本期先默认 false | ⏳ 待产品 |
| Q2 | Menu 显示与存档不一致时，玩家以为「没钱」的错觉如何防？ | 现网同源读档；若再报错先对 Console `archive gold` vs Menu `display` | ⏳ 观察 |
| Q3 | 0829「决定只 Log、无 Trigger」是否仍成立？ | **否，已过时**；本报告标接线已落地 | ✅ 本报告 |

（已追加 `OPEN_QUESTIONS.md`。）
