# SewingKit · 非消耗唯一一件 · 架构溯源报告

> 日期：2026-09-22  
> 角色：架构侦探（**只读**，未改代码 / 资源 / Prefab）  
> Unity：2020.3.48f1  
> 产品钉死：**针线包不是消耗品，就一个**；背包角标不应出现 **`3`**  
> 提示词：`Assets/Doc/提示词/0922/SewingKit_非消耗唯一一件_架构侦探提示词.md`  
> 对照：0901 续聊入包定案（TaskItem×1 + Tips）

---

## ① 结论一句话

**类型已对（Database=`TaskItem`，`IsCanUse=false`）；角标「3」= 持有量真叠到 3，不是 Icon 画上去的。**  
根因：`AddMainItem` / `GetItemActionTask` **无「已有则跳过」**，任意重复发放（调试加物、DialogDebug 重播、旧档多次入包等）会 `num += count`（上限 10）；`MenuFormMainItemBtn` **无条件** `num.text = item.num`，TaskItem=3 必显「3」。  
最小修：**方案 B+C**（唯一道具入包钳 1 + 读档/Refresh 修旧档）+ **方案 A 顺带**把 JSON `itemType` 从误写的 MaterialItem 改回 0；**方案 D**（唯一件/重要道具隐藏角标，或 `num≤1` 隐藏）对拍剑/地图，推荐同票；**否决**全局 `MaxStack=1`。空桶/满桶 **必须排除**在唯一钳之外。

---

## ② 为何会出现「3」

### A. 类型表（非消耗是否成立）

| 源 | SewingKit `itemType` | 枚举含义 |
|----|----------------------|----------|
| `MainItemDatabase.asset`（`itemId: 17`，`legacyNumericId: 18`） | **0** | **TaskItem**（重要道具，不能当药喝） |
| `MainItemConfig.json` | **2** | MaterialItem（与 Database **不一致**；v2 真源是 Database） |
| 入包后 `MenuFormMainItemInfo.itemType` | Refresh 用 `MainItemDefProvider` **覆盖为 Def.ItemType** | 正常读档后 = TaskItem |
| `IsCanUse("SewingKit")` | 仅 `CostItem` 为 true | **false**（非消耗已满足） |
| 买卖 | Database `buyPrice/sellPrice: -1` | 不进商店买卖表 |

**裁定**：产品「不是消耗品」在运行时**已满足**。JSON=2 是文档/旧表误导，不单独造成「当血瓶用」；仍应同步为 0，避免某条仍读 JSON 的工具链搞错。角标「3」**不是**类型错成 CostItem。

### B. 数量：为什么是 3

| 点 | 现网 | 说明 |
|----|------|------|
| `AddMainItem` | 已有键则 `num += count`，再钳 `MaxStackPerItem=10` | **无唯一门控** |
| `GetItemActionTask` | 直接 `AddMainItem(ItemName, Num)` | **无「已有则跳过」** |
| 续聊 Prefab | **仅 1 处** `GetItemActionTask(ItemName="SewingKit", Num=1)` + Tips `GetSewingKit` | 图内不是一次发 3 |
| 续聊门闩 | `ShouldPlayChiefContinue`：门口已用 ∧ 续聊未用；Trigger 时 `OnStoryTriggered` 记档 | **正常同档只应发 1 次** |
| 其它发奖 | 全工程无第二处 `AddMainItem(SewingKit)`（除通用调试：`AA_TestPanel` / `AddItemComponentGT`） | 「3」多半 = **重复入包三次**（调试/重播/旧档），不是 Prefab×3 |
| 对拍 AiLinSword | 同样走 `AddMainItem` 无唯一钳 | **重复 GetItem 同样会叠** |

生活类比：针线包是村长送的**一件**家当；账本却按药水规则「再来一件就 +1」，三次就变成角上写 3。

### C. UI：角标规则

| 点 | 现网 |
|----|------|
| `MenuFormMainItemBtn.UpdateInfo` | `num.text = $"{item.num}"` **无条件**（含 TaskItem） |
| 剑 / 地图 | 同路径：持有 1 也会显示 **「1」**（无隐藏逻辑） |
| 角标「3」 | TMP 数量，**不是** Icon 像素 |

产品「不像消耗品」：最小先保证持有=1（无「3」）；金样若要不显示「1」，再上方案 D。

---

## ③ 方案对比（含空桶排除）

| 方案 | 做法 | 何时选 | 裁决 |
|------|------|--------|------|
| **A** | Database 已是 TaskItem；JSON `SewingKit.itemType` **2→0** | 双源不一致 | **要做**（卫生，非角标主因） |
| **B** | `AddMainItem`（或 `GetItem`）：唯一件已有则不再加 / 钳到 1 | 叠到 3 的主因 | **必做** |
| **C** | `ParseInternal` / `Refresh` / `ClampAllItemStacks` 旁路：SewingKit（及同表唯一件）`num>1` → 1 | 修旧档已有 3 | **必做** |
| **D** | 唯一件隐藏角标；或 TaskItem 且 `num≤1` 隐藏（空桶 `num>1` 仍显） | 展示层对拍剑 | **推荐同票** |
| **E** | 全局 `MaxStackPerItem=1` | — | **否决**（误伤药/球） |

### 唯一件白名单（禁止「全体 TaskItem 钳 1」）

| 纳入唯一钳 1 | 排除（数量玩法） |
|--------------|------------------|
| **SewingKit**（本期钉死） | **EmptyWaterBucket**（1～4） |
| 建议同表：`AiLinSword` / `Map` / `GushaNacklace` / `XiaerPower`（同类「一件家当」） | **FullWaterBucket** |
| — | 全部 CostItem / MaterialItem（药、球、素材） |

**勿**写「所有 TaskItem 上限 1」——空桶也是 TaskItem，会误伤老农打水。

### 必答

1. **最小集合？** **B+C**（数据层唯一）；**A** 顺带；**D** 推荐同做（去掉「1」也像消耗品的观感）。  
2. **空桶？** 白名单排除 Empty/FullWaterBucket。  
3. **会不会误伤药水堆叠？** 不碰 `MaxStackPerItem`；只钳白名单。

推荐落地形状（施工参考，本阶段未改）：

```text
PlayerBagData.AddMainItem:
  if (IsUniqueMainItem(name) && HasMainItem(name))
      → return 或 SetMainItemCount(name, 1)；不累加

Clamp / Refresh:
  foreach unique → num = Min(num, 1)

MenuFormMainItemBtn（方案 D）:
  if (IsUniqueMainItem || (TaskItem && num <= 1)) → 隐藏 num TMP
  else → 显示数量（空桶 2～4、药水等）
```

---

## ④ 要改文件（路径级；本阶段未改）

| 全路径 | 预期改动 |
|--------|----------|
| `…/PlayerBagData.cs` | 唯一件判定；`AddMainItem` 不叠；读档/Clamp 修旧档 |
| `…/GetItemActionTask.cs` | 可选：已有唯一件则跳过（或只依赖 Bag API） |
| `…/MenuFormMainItemBtn.cs` | 方案 D：隐藏唯一件角标 |
| `…/MainItemConfig.json` | SewingKit `itemType` **2→0** |
| `MainItemDatabase.asset` | **不必改类型**（已是 0） |
| 续聊 Prefab / Tips 图 | **不改**（保持 GetItem×1 + GetSewingKit） |
| 商店 / 药水 MaxStack | **不改** |

施工说明建议：`Assets/Doc/施工说明/0922/SewingKit_非消耗唯一一件_施工说明.md`

---

## ⑤ 验收表

| # | 步骤 | 期望 |
|---|------|------|
| 1 | 新档走续聊首次获得 | 背包针线包 **1**；不能当药喝 / 不进消耗快捷逻辑 |
| 2 | 角标 | 无「3」；按 D：无数字或仅符合剑/地图金样 |
| 3 | 同档再触发 GetItem / 调试再加 | 仍为 **1**，不叠 |
| 4 | 旧档已有 3 | 读档或进包 Refresh 后变为 **1** |
| 5 | 空桶数量 1～4 | **不被**唯一钳误伤 |
| 6 | 血药 / 球 / 商店药水 | 仍可堆叠消耗 |
| 7 | Tips「获得了针线包」 | 仍正常 |

---

## ⑥ 风险与回滚

| 风险 | 说明 | 回滚 |
|------|------|------|
| 白名单漏空桶 | TaskItem 一刀切会毁打水 | 只用显式白名单；测空桶×4 |
| 唯一件第二次 GetItem 无反馈 | Tips 可能仍弹但包内仍 1 | 可接受；或 GetItem 已有则跳过 Tips（另议，本期可不做） |
| 只做 D 不修数据 | 角标没了但账本仍 3 | **禁止**；必须 B+C |
| 全局 MaxStack=1 | 药/球全坏 | **禁止** |
| JSON 不同步 | 工具链误当素材 | A 一并改 |

---

## 附录 · 强制排除复核

| 否决项 | 本票 |
|--------|------|
| 只改 Icon 去「3」 | 「3」是 TMP |
| 全局 MaxStack=1 | 误伤药水 |
| 删掉 GetItem 节点 | 丢首次获得 |
| 做成可消耗「用掉」 | 产品明确非消耗 |
| 改 Tips 文案/图 | 不在范围 |
