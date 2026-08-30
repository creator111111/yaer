# Cursor Agent Prompt · Village_Shop：没钱仍能买成 + 成败对白验收（Yes/No）

> **角色**：先【架构侦探】只读溯源，报告拍板后再【施工员】  
> **日期**：2026-08-30  
> **场景**：`Village_Shop` · 点「决定」购买  
> **用户现象（钉死）**：**没有钱仍会购买成功**；购买失败路径测不到 / 不成交反馈不对  
> **对白资产（产品既定，须与用户 @ 对拍）**：  
> - **成功**：`Assets/GameRes/Prefabs/Dialogue/Village_ShopYes.prefab`（用户若写了 `Village_ShopStart`，侦探须澄清：Start=首次进店，**不是**购买成功）  
> - **失败**：`Assets/GameRes/Prefabs/Dialogue/Village_ShopNo.prefab`  
> **关联**：`bypassGoldCheckForBagJoint` · `OnConfirmClick` · `TryTriggerPurchaseResult` · `TrySpendPlayerGold` · ShopPanel Prefab 序列化值 · 0829 成败对白报告  
> **本阶段**：只读；禁止改代码 / Prefab / 场景 / CSV / 存档  
> **报告落盘**：`Assets/Doc/执行文档/0830/Village_Shop_没钱仍能购买与成败对白验收_架构溯源报告.md`

把下面「侦探」整段复制给 Cursor Agent（Agent Mode）执行。

---

## 提示词助手预梳理（侦探须核实，勿当唯一真相）

### 用户原话 → 侦探必须直接回答

| 用户说 | 预扫假说（须证伪） |
|--------|-------------------|
| 「没有钱还是会购买成功」 | `ShopFormLogic.bypassGoldCheckForBagJoint == true`（脚本默认 **且** `ShopPanel.prefab` 序列化为 **1**）→ **跳过** `TrySpendPlayerGold` → 直接入包 → **永远假成功** |
| 「并不会购买失败」 | 旁路开时走不到金币不足分支 → **播不出** `Village_ShopNo`；不是 Prefab 坏了 |
| @ `Village_ShopStart` + `Village_ShopNo` | Start=首次进店对白；购买成功既定名是 **`Village_ShopYes`**。侦探结论里写清：用户是否混用；成败线 **不要**误改成播 Start |

### 现网已施工假说（0829 报告后可能已接线）

```
OnConfirmClick
  → 数量 0 / 堆叠 / 出售 / 存档空 → return（不播 No）
  → if (!bypassGoldCheckForBagJoint)
        TrySpend 失败 → LogInsufficientGold → TryNotifyPurchaseDialogue(false) → ShopNo
  → else  // 【旁路开 = 用户测到的「没钱也能买」】
        跳过扣款
  → AddMainItem + Save → TryNotifyPurchaseDialogue(true) → ShopYes
```

| 项 | 假说 |
|----|------|
| 成败对白 | 已有 `TryTriggerPurchaseResult` → Yes/No |
| 货币真检 | **被旁路挡住**；正式失败验收必须 **关旁路** |
| Prefab 值 | `Assets/GameRes/Prefabs/UI/ShopPanel.prefab` 内 `bypassGoldCheckForBagJoint: 1` 优先于改脚本默认 |

### 与 ShopStart 的边界（防误改）

| Prefab | 用途 | 本期 |
|--------|------|------|
| `Village_ShopStart` | 首次进店 | ❌ **不**当购买成功对白；勿改接线到 Start |
| `Village_ShopYes` | 购买成功短反应 | ✅ 成功出口 |
| `Village_ShopNo` | 金币不足短反应 | ✅ 失败出口（仅钱不够） |

### 侦探必拍板（施工前）

| 问 | 助手倾向 |
|----|----------|
| 正式游玩旁路默认？ | **关**（`false`）：没钱不能买、能播 No |
| 开发联合验收旁路？ | 可保留字段，但 **默认 false**；或 Editor-only / 开发菜单开关，避免场景 Prefab 一直 true |
| 关旁路后钱不够 | 不入包 + 播 **ShopNo** |
| 钱够 | 扣款入包 + 播 **ShopYes** |
| 旁路仍 true 时「钱不够」点决定 | 仍入包 + 播 Yes（属旁路设计）；**不得**当「失败对白已验收」 |

### 本期边界

| 做 | 不做 |
|----|------|
| ✅ 钉死「没钱仍成功」根因（旁路 / 扣款 API / 读错钱包 / UI 显示假金币） | ❌ 重做货币系统 |
| ✅ 核实现网 Yes/No 是否已接线、关旁路后能否走到 No | ❌ 把成功对白改成 ShopStart |
| ✅ 拍板旁路默认值 + Prefab/脚本谁说了算 | ❌ 出售成交对白 |
| ✅ 最小施工清单 + 验收步骤（含如何把金币刷到不够） | ❌ 堆叠失败硬播「没钱」No |

### 严禁（本阶段）

- 改代码 / Prefab / 场景 / CSV / 存档  
- 未核实旁路就断言「扣款坏了 / 货币系统没有」  
- 把 `Village_ShopStart` 接到购买成功出口  
- 旁路仍开时宣称「购买失败 / ShopNo 已验收」  

### 须对拍资产

| 资产 | 用途 |
|------|------|
| `ShopFormLogic.cs` · `OnConfirmClick` / `bypassGoldCheckForBagJoint` | 成败 + 旁路真源 |
| `ShopPanel.prefab` | 序列化旁路实际值 |
| `Village_ShopSceneManager` · `TryTriggerPurchaseResult` | Yes/No 故事名 |
| `Village_ShopYes` / `Village_ShopNo` | 对白 Prefab |
| `Village_ShopStart` | 仅对照：勿与成败混淆 |
| `PlayerGoldData` / `QuestManager.TrySpendPlayerGold` | 扣款是否真执行 |
| `0829/...ShopYes_ShopNo_架构溯源报告.md` | 标过时点：接线是否已落地 |

---

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/02_SYSTEM_SPEC.md
@Assets/Doc/执行文档/0829/Village_Shop_购买成败对话_ShopYes_ShopNo_架构溯源报告.md
@Assets/Doc/提示词/0829/Village_Shop_购买成败对话_ShopYes_ShopNo_架构侦探提示词.md
@Assets/Doc/提示词/0829/Village_Shop_购买成功失败与剩余货币检测_架构侦探提示词.md
@Assets/GameRes/Prefabs/Dialogue/Village_ShopYes.prefab
@Assets/GameRes/Prefabs/Dialogue/Village_ShopNo.prefab
@Assets/GameRes/Prefabs/Dialogue/Village_ShopStart.prefab
@Assets/GameRes/Prefabs/UI/ShopPanel.prefab
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Shop/ShopFormLogic.cs
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Scene/Village_Shop/Village_ShopSceneManager.cs
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Shop/ShopDebugLogger.cs
@Assets/Scripts/Game/GameMgr/Component/Archive/ArchiveDataClass/Quest/PlayerGoldData.cs
@Assets/Scripts/Game/GameMgr/Component/Archive/ArchiveDataClass/Quest/QuestManager.cs

你现在是【架构侦探】。Unity 2020.3.48f1 / C#。
禁止修改任何代码、Prefab、场景、CSV、存档。只读扫描 + 写溯源报告。

---

## 背景（策划白话）

1. 玩家测试：**钱不够点「决定」仍然买成功**（物品进包 / 像成交了），感觉「不会购买失败」。  
2. 购买成功要播 **`Village_ShopYes`**，钱不够失败要播 **`Village_ShopNo`**（用户若提到 ShopStart，请澄清 Start≠购买成功）。  
3. 本阶段只查清：为什么没钱还能成；旁路/扣款/对白哪一段断；正式玩要怎么改默认与怎么验收。

---

## 侦探任务清单

### A. 复现「没钱仍成功」根因树
按优先级证伪并画出唯一主因：

| # | 假说 | 如何证伪 |
|---|------|----------|
| 1 | `bypassGoldCheckForBagJoint` 为 true（脚本默认 / Prefab 序列化） | 读字段默认值 + `ShopPanel.prefab` YAML |
| 2 | `TrySpendPlayerGold` 永不失败 / 读错钱包 | 扫实现与 `CanAfford` |
| 3 | UI 显示的「没钱」不是存档真金币 | MenuPanel Money vs `PlayerGoldData.gold` |
| 4 | 成败对白反了或总走 success | `TryNotifyPurchaseDialogue` / `TryTriggerPurchaseResult` 调用点 |

结论必须一句话钉死主因（可并列次因）。

### B. 钉死 OnConfirmClick 现网分支（含对白）
画完整树：哪些出口入包、哪些播 Yes、哪些播 No、哪些只 Log。  
对照 0829 报告：标「已接线 / 仍缺 / 被旁路挡死」。

### C. Prefab 名对拍（防 Start 误用）
表：`ShopStart` / `ShopYes` / `ShopNo` 各自用途、谁被购买链路引用。  
明确：**购买成功不得播 Start**。

### D. 旁路策略拍板（给施工）
推荐：

| 项 | 拍板 |
|----|------|
| 运行时默认 | `bypassGoldCheckForBagJoint = false` |
| Prefab | `ShopPanel` 同步改为 0，避免场景覆盖脚本 |
| 开发联调 | 保留字段可手开；注释写清「开=验不出钱不够失败」 |

若有更好方案（仅 Editor 开关）可写入，但默认必须让正式测试「没钱=失败」。

### E. 最小施工清单（本阶段不执行）

| # | 动作 | 优先级 |
|---|------|--------|
| 1 | 关旁路默认（脚本 + ShopPanel Prefab 一致） | **P0** |
| 2 | 确认关旁路后：钱不够 → 不入包 + ShopNo | **P0** |
| 3 | 钱够 → 扣款入包 + ShopYes | **P0** |
| 4 | 验收说明：如何把金币刷到低于合计 | **P0** |
| 5 | 文档：旁路开时勿宣称失败已验 | P1 |

### F. 验收清单（写入报告）

| # | 前置 | 操作 | 通过 |
|---|------|------|------|
| 1 | 旁路 **关**；金币 **&lt;** 合计 | 决定 | **不入包**；播 **ShopNo**；金币不变 |
| 2 | 旁路 **关**；金币 **≥** 合计 | 决定 | 扣款入包；播 **ShopYes** |
| 3 | 旁路 **开**；金币再少 | 决定 | 仍入包（旁路预期）；**不**当失败验收 |
| 4 | 数量 0 | 决定 | 不播 No |
| 5 | Console | — | 不足时有 `LogInsufficientGold`；故事名=Yes/No Prefab 名 |

### G. 开放问题
- 旁路是否改为仅开发菜单，而不是 SerializeField 挂在正式 Prefab？  
- Menu 显示金币与存档不一致时，玩家以为「没钱」的错觉如何防？  

---

## 输出要求

写入：`Assets/Doc/执行文档/0830/Village_Shop_没钱仍能购买与成败对白验收_架构溯源报告.md`

MASTER 四段式：  
① 结论（主因一句话 + 旁路默认拍板 + Yes/No 是否已接线）  
② 原因（通俗：为什么测起来「没钱也能买」）  
③ 用户检查清单（关旁路后怎么验失败/成功）  
④ 给程序：分支表 + Prefab/字段真值 + 最小 diff 清单 + 开放问题
```

---

## 施工员续跑（侦探报告拍板后贴）

```
@Assets/Doc/00_MASTER_PROMPT.md
@Assets/Doc/执行文档/0830/Village_Shop_没钱仍能购买与成败对白验收_架构溯源报告.md
@Assets/GameRes/Prefabs/UI/ShopPanel.prefab
@Assets/Scripts/Game/GameRuntime/UI/FormLogic/Shop/ShopFormLogic.cs
@Assets/Scripts/Game/GameRuntime/GameSceneManager/Scene/Village_Shop/Village_ShopSceneManager.cs
@Assets/GameRes/Prefabs/Dialogue/Village_ShopYes.prefab
@Assets/GameRes/Prefabs/Dialogue/Village_ShopNo.prefab

你现在是【施工员】。按报告修复「没钱仍能购买成功」，并保证成败对白可验收。

必须遵守：
- 正式默认关闭 bypassGoldCheckForBagJoint（脚本默认与 ShopPanel Prefab 一致为 false）；
- 钱不够：不入包、不扣款，播 Village_ShopNo；钱够：扣款入包后播 Village_ShopYes；
- 禁止把购买成功接到 Village_ShopStart；
- 先结算再对白；经 GSM TryTriggerPurchaseResult，禁止 UI 直开 TriggerStory；
- 代码含详细注释；重要取舍写清原因（尤其旁路为何默认关）。

提交说明：改了哪些默认值、如何验收没钱失败、未做项。
```
