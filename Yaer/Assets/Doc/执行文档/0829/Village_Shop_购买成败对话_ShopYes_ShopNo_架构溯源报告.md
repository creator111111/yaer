# Village_Shop — 购买成败对话 `Village_ShopYes` / `Village_ShopNo` — 架构溯源报告

**文档版本**：v1.0（2026-08-29）  
**文档性质**：【架构侦探】只读溯源 + 接线拍板（**本阶段未改代码 / Prefab / 场景 / CSV**）  
**Unity**：2020.3.48f1  
**场景**：`Village_Shop` · 点「决定」购买  
**对白资产**：  
- 成功：`Assets/GameRes/Prefabs/Dialogue/Village_ShopYes.prefab`  
- 失败：`Assets/GameRes/Prefabs/Dialogue/Village_ShopNo.prefab`  

关联：`OnConfirmClick` · `TryTriggerShopkeeperSpecial` · `bypassGoldCheckForBagJoint` · Head/点头特殊对白 · `0829/Village_ShopHead_*`（立绘对照，本期可简化）

---

## ① 结论一句话

**现网「决定」只做扣款/入包 + `ShopDebugLogger`，无 TriggerStory。推荐方案 A：结算完成后再经 GSM `TryTriggerShopkeeperSpecial`（或薄封装 `TryTriggerPurchaseResult`）播 `Village_ShopYes` / `Village_ShopNo`。仅「真正入包成功」播 Yes；仅「金币不足」播 No；数量 0 / 堆叠超 / 出售未实现 / 存档空只 Log 不播。正式验 No 必须关货币旁路；旁路入包成功仍播 Yes。两 Prefab 根名可加载、文案齐、店句已开合层脸——无嵌 GoOut 大立绘，成败短句本期不抄 ShopHead 先立绘时序。**

---

## ② 原因（通俗）

买成了 / 钱不够，老板娘要对玩家说话，不能只在 Console 打一行。  
工程里点头对白已经会：**藏买卖界面 → 播剧情 → 结束后露界面 + 热区 + 合层脸复位**。购买成败应走同一扇门，别让商店 UI 自己乱开剧情叠两段话。

---

## ③ 用户怎么验

| # | 操作 | 通过 |
|---|------|------|
| 1 | Inspector 关掉 `ShopFormLogic.bypassGoldCheckForBagJoint`；钱够 → 决定 | 扣款入包后播 **Yes**（店：「谢谢惠顾~~」）；买卖 UI 藏起；结束回 Idle |
| 2 | 旁路关、钱不够 → 决定 | **不入包**；播 **No**（店：「哎哟，你好像没什么钱呢。」） |
| 3 | 数量全 0 → 决定 | 仅 Warning；**不播 No** |
| 4 | Yes/No 播放中再点决定 / 头 | **不叠**第二段（UI 已藏 + `HasRunningStory`） |
| 5 | Console | `[ShopSpecial] TriggerStory Village_ShopYes/No`；无 Missing Prefab |
| 6 | 旁路=true 时「钱不够」 | **测不出 No**（属预期，勿当失败对白已验） |

---

## ④ 给程序

### A. 两 Prefab 内容（磁盘真源）

加载路径：`DialoguePath.GetPath(name)` → `Assets/GameRes/Prefabs/Dialogue/{name}.prefab`（根 `m_Name` = 文件名 ✅）。

#### `Village_ShopYes`

| 项 | 值 |
|----|-----|
| 根名 | `Village_ShopYes` |
| Actor | Merchant（老板娘）+ Yaer（雅尔） |
| 图序 | `FightingPanelVisible` → `NormalDialogueUIAlpha`(1s) → 句1 → 句2 |
| 句1 | 店 · `UseShopkeeperPortrait=true` · **ShopFace=2** ·「**谢谢惠顾~~**」 |
| 句2 | 雅 · FaceType=22 ·「早知道就从家带些钱了。」 |
| 雅大立绘 GoOut | ❌ **未嵌** CanvasGroup 立牌；BB 有 `GoOutStoryYaerPainting` 变量但 **`_objectReferences` 空**（孤儿声明） |
| 可播性 | ✅ 故事名对齐即可 Trigger；店合层脸走现网 Portrait |

#### `Village_ShopNo`

| 项 | 值 |
|----|-----|
| 根名 | `Village_ShopNo` |
| Actor | Merchant + Yaer |
| 图序 | Fighting → UIAlpha → 句1 → 句2 → 句3 |
| 句1 | 店 · Portrait · **ShopFace=3** ·「**哎哟，你好像没什么钱呢。**」 |
| 句2 | 店 · Portrait · ShopFace=3 ·「多拿些材料来换吧~」 |
| 句3 | 雅 · FaceType=22 ·「不好意思。。。。」 |
| 雅大立绘 | 同 Yes：无嵌立牌；BB 未绑 |
| 可播性 | ✅ |

**对照 ShopHead**：成败线 **无** GoOut 嵌套，**不必**本期做「先立绘后框」T1；P1 若产品要大立绘再单独立项（抄 Head）。

### B. `OnConfirmClick` 成败出口（现网 + 播片拍板）

```
OnConfirmClick
  ├─ 出售 Tab          → LogSellNotImplemented     → ❌ 不播
  ├─ 数量全 0 / total≤0 → LogZeroQuantityWarning   → ❌ 不播
  ├─ 背包空            → LogArchiveUnavailable     → ❌ 不播
  ├─ 堆叠将超          → LogStackOverflow          → ❌ 不播 No（文案是「没钱」，不符）
  ├─ bypass=false
  │    ├─ goldData 空  → LogArchiveUnavailable     → ❌ 不播
  │    └─ TrySpend 失败 → LogInsufficientGold      → ✅ 【ShopNo】先 Log 再 Trigger
  ├─（旁路跳过扣款 或 扣款成功）
  ├─ AddMainItem + SaveBag + LogPurchase + 清数量  → ✅ 【ShopYes】结算后再 Trigger
  └─ ❌ 现网无任何 TriggerStory
```

| 出口 | 播片 |
|------|------|
| 入包成功（含 **bypass 未扣款仍入包**） | **`Village_ShopYes`** |
| 金币不足（仅 bypass=false） | **`Village_ShopNo`** |
| 数量 0 / 堆叠 / 出售 / 存档空 | **只 Log** |

时序钉死：**先结算（或失败判定）→ 再 Hide+Trigger**；禁止先播对白再扣款。

### C. 接线方案（拍板 A）

| 方案 | 裁定 |
|------|------|
| **A · 经 GSM `TryTriggerShopkeeperSpecial`** | **✅** Hide/Show/热区/`HasRunningStory`/结束 `ResetDefault` 现成 |
| A' · 薄封装 `TryTriggerPurchaseResult(bool success)` | ✅ 推荐：内部映射常量 Yes/No 再调 Special（注释写清「购买成败 = 特殊对白同管线」） |
| B · ShopFormLogic 直接 `TriggerStory` | ❌ 易漏藏 UI / 叠对白 |
| C · 仅 Tips | ❌ 产品已指定 Prefab |

**调用方式**（与离店同款，已核实）：

```csharp
var shopGsm = GameManager.GetGameSceneManager() as Village_ShopSceneManager;
shopGsm?.TryTriggerShopkeeperSpecial(...); // 或 TryTriggerPurchaseResult
```

`ShopFormLogic` **已** `using` / 强转 GSM（见 `OnExitClick`），无需新接口层。

**建议常量**（GSM 旁现有 Head/Chest 名）：

- `PurchaseSuccessStoryName = "Village_ShopYes"`  
- `PurchaseFailInsufficientGoldStoryName = "Village_ShopNo"`  

### D. 状态机与互斥

| 场景 | 行为 |
|------|------|
| Yes/No 进行中 | `HideShopUiRoot` → 决定钮不可点；热区 OFF；`HasRunningStory` 挡 Head/Chest / 再一次 Special |
| ESC 离店 | 现网对白中已忽略 ESC；**保持** |
| 首次进店窗口 | `TryTriggerShopkeeperSpecial` 若 `ShouldPlayShopStartStory` 仍 true 会拒绝——Idle 可买时首对白已过，正常 |
| 对白结束 | 复用 `OnShopkeeperSpecialStoryEnd` → `ResetDefault` + Show UI + 热区 ON |
| 额外禁用决定钮 | **不必**（UI 已藏）；勿另写一套按钮锁 |

### E. Prefab 最小补齐

| 项 | 优先级 | 说明 |
|----|--------|------|
| 根名 / 文案 / Portrait 店句 | 已齐 | P0 只接线 |
| GoOut 大立绘 + BB 绑定 + 先立绘后框 | **本期不做** | Prefab 无立牌；成败短反应可只出框+合层脸 |
| 删孤儿 BB 变量 | P2 清理 | 不影响 Trigger |

### F. 旁路与验收

| `bypassGoldCheckForBagJoint` | 成功路径 | 失败「钱不够」 |
|------------------------------|----------|----------------|
| **true（现网默认）** | 跳过扣款仍入包 → **仍播 Yes** | **走不到** TrySpend 失败 → **验不出 No** |
| **false** | 扣款+入包 → Yes | TrySpend 失败 → **No** |

正式成败对白验收：**关旁路**；钱不够可用刷金工具把余额调低。

### G. 最小施工清单（本阶段不执行）

| # | 动作 | 优先级 |
|---|------|--------|
| 1 | GSM：常量 Yes/No；可选 `TryTriggerPurchaseResult(bool)` | **P0** |
| 2 | `OnConfirmClick`：入包成功后调 GSM → Yes | **P0** |
| 3 | 金币不足分支：Log 后调 GSM → No（return 前） | **P0** |
| 4 | 其它失败出口不调 No | **P0** |
| 5 | 验收说明：关旁路；Console 故事名 | **P0** |
| 6 | Prefab 大立绘时序 | P1（产品要再做） |
| 7 | 出售成交对白 | ❌ 本期不做 |

**预期 diff**

- `Village_ShopSceneManager.cs`（常量 + 可选薄封装）  
- `ShopFormLogic.OnConfirmClick`（两处挂 Trigger）  
- **一般不改** Prefab / Tips / 货币 API  

### H. 验收清单

| # | 操作 | 通过 |
|---|------|------|
| 1 | 旁路关、钱够 → 决定 | 入包扣款后 Yes；UI 藏；结束 Idle |
| 2 | 旁路关、钱不够 → 决定 | 不入包；No |
| 3 | 数量 0 | 不播 No |
| 4 | 对白中点头/决定 | 不叠 |
| 5 | Console | 故事名=Prefab 名；无 Missing |

### I. 开放问题

| ID | 问题 | 决议 | 状态 |
|----|------|------|------|
| Q1 | 接线方案？ | **A / A' 经 GSM Special** | ✅ |
| Q2 | 哪些失败播 No？ | **仅金币不足** | ✅ |
| Q3 | bypass 成功是否播 Yes？ | **是** | ✅ |
| Q4 | 堆叠失败播什么？ | **不播 No**（文案不符）；另文案 P2 | ✅ |
| Q5 | Yes/No 要否雅大立绘完整分层？ | **本期否**（Prefab 无嵌） | ✅ |
| Q6 | 出售未实现播 No？ | **否** | ✅ |

（已追加 `OPEN_QUESTIONS.md`。）
