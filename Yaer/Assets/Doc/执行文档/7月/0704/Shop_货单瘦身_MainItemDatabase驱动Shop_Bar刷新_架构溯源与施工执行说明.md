# Shop · 货单瘦身：MainItemDatabase 驱动 Shop_Bar 刷新 — 架构溯源与施工执行说明

**文档版本**：v1.1（2026-07-05 · 追加 Play 测试问题修复清单）  
**文档性质**：架构侦探产出（只读溯源 + 施工指引；**本阶段先写文档，代码待施工**）  
**触发**：
- `ShopCatalogConfig` 与 `MainItemDatabase` **重复维护** Icon / Name / Price，策划改两处易不一致
- 用户截图：`ShopCatalogConfig` 仍手填 8 条 buy / 3 条 sell，而 Database 已有完整字段
- 诉求：**购买侧刷 CostItem，出售侧刷 MaterialItem**，不再维护独立货单清单

**依据**：
- `Assets/Doc/00_MASTER_PROMPT.md`【架构侦探】
- `Assets/Doc/执行文档/0704/MainItem_道具固有属性表_架构溯源与施工执行说明.md`（v2 · Database 唯一源）
- `Assets/Doc/执行文档/0704/Shop_货单配置Asset与Shop_Bar数据刷新_架构溯源与施工执行说明.md`（**本任务取代其「货单双列表」数据层**）
- 关联代码：`MainItemDefProvider.cs`、`ShopFormLogic.cs`、`ShopBarRowView.cs`、`ShopCatalogConfig.cs`
- 关联资源：`MainItemDatabase.asset`、`ShopCatalogConfig.asset`、`Village_Shop.unity`

**Unity 版本**：2020.3.48f1  

---

## ① 结论（一句话）

**废弃（或降级为可选）`ShopCatalogConfig` 的 buyItems / sellItems 手填清单；`ShopFormLogic` 打开商店时向 `MainItemDefProvider` 取过滤结果——购买页 = `CostItem` 且 `buyPrice >= 0`，出售页 = `MaterialItem` 且 `sellPrice >= 0`——按 Database 条目顺序 `Instantiate Shop_Bar` 并 Bind Icon / Name / Price；策划以后只改 `MainItemDatabase.asset` 一处。**

---

## ①.1 范围冻结（2026-07-05 · 策划 / 施工对齐）

| 项 | 本阶段约定 | 说明 |
|----|------------|------|
| **唯一数据源** | **`MainItemDatabase.asset`** | Icon、displayName、buyPrice、sellPrice、itemType |
| **购买页行来源** | **`itemType == CostItem` 且 `buyPrice >= 0`** | 未定价（-1）的消耗品 **不上架** |
| **出售页行来源** | **`itemType == MaterialItem` 且 `sellPrice >= 0`** | 未定价素材 **不出售** |
| **TaskItem** | **永不进商店列表** | 任务道具不可买卖 |
| **ShopCatalogConfig** | **删除 buy/sell 列表** 或整文件废弃 | 见 §4.3 替代方案 |
| **UI 行为** | **保留** 双 Scroll + Tab、动态 Instantiate | 仅改 **读数入口** |
| **本阶段不做** | 背包持有数过滤、真扣金币、按 NPC 分店 | 出售页仍静态列表；阶段六再接背包 |
| **本阶段不做（续）** | 商店特价 / 折扣覆盖 Database 价 | 需要时再开 `ShopPriceOverride` |

---

## ② 玩家在做什么 / 会遇到什么现象

| 现象 | 原因（生活类比） |
|------|------------------|
| 改 Database 里小生命药 20 块，商店还显示旧价 | **两本价目表**（货单 + 档案）没同步 |
| 新增消耗品忘了加进 ShopCatalog | 货单是**手工台账**，Database 加了也不自动上架 |
| MpBall 在货单里 price=0，Database 里 buyPrice=-1 | 双源冲突，不知道听谁的 |
| 策划问「还要维护 ShopCatalog 吗？」 | **不需要**——档案柜里标了类型和价，货架自动挂 |

**生活类比**：`MainItemDatabase` 是**带分类标签的总档案**（消耗品 / 素材 / 任务）；商店是**自动筛选器**——进货页扫「可买的消耗品」，收货页扫「可卖的素材」。`ShopCatalogConfig` 是多余的**手写进货单**，可以收掉。

---

## ③ 工程现状快照（架构侦探 · 只读）

### 3.1 MainItemDatabase（已有 · 应成为唯一源）

**路径**：`Assets/GameRes/Config/MainItem/MainItemDatabase.asset`  
**访问**：`MainItemDefProvider.GetDef(itemId)` / `ResolveIcon(itemId)`

| 字段 | 用途 |
|------|------|
| `itemId` | `EMainItemName` |
| `icon` | Inspector 拖 Sprite；空则图集 / ArtRes 兜底 |
| `displayName` | 列表 / 背包展示名 |
| `itemType` | `TaskItem(0)` / `CostItem(1)` / `MaterialItem(2)` |
| `buyPrice` | `>= 0` 可购买；`-1` 不可买 |
| `sellPrice` | `>= 0` 可出售；`-1` 不可卖 |

**枚举**（`PlayerBagData.cs`）：

```csharp
public enum BagItemType
{
    TaskItem,      // 0 — 不进商店
    CostItem,      // 1 — 购买页候选
    MaterialItem,  // 2 — 出售页候选
}
```

### 3.2 当前 Database 条目 · 过滤预览（2026-07-05）

| itemId | displayName | itemType | buyPrice | sellPrice | 购买页 | 出售页 |
|--------|-------------|----------|----------|-----------|--------|--------|
| AiLinSword | 艾琳之剑 | TaskItem | -1 | -1 | — | — |
| XiaerPower | 夏尔的牵挂 | TaskItem | -1 | -1 | — | — |
| **HpBall** | 生命之珠 | CostItem | **200** | -1 | ✅ | — |
| MpBall | 体力之珠 | CostItem | **-1** | -1 | ❌ | — |
| Map | 地图 | TaskItem | -1 | -1 | — | — |
| GushaNacklace | 古莎的项链 | TaskItem | -1 | -1 | — | — |
| **InsectBeak** | 虫喙 | MaterialItem | -1 | **5** | — | ✅ |
| **TenWangFruit** | 藤蔓果 | MaterialItem | -1 | **5** | — | ✅ |
| **SlimeCore** | 史莱姆核 | MaterialItem | -1 | **5** | — | ✅ |
| **SmallHpPotion** | 小生命药 | CostItem | **20** | -1 | ✅ | — |
| **SmallMpPotion** | 小体力药 | CostItem | **20** | -1 | ✅ | — |
| **LargeHpPotion** | 大生命药 | CostItem | **50** | -1 | ✅ | — |
| **LargeMpPotion** | 大体力药 | CostItem | **50** | -1 | ✅ | — |
| **BowlLiquid** | 碗装液体 | CostItem | **500** | -1 | ✅ | — |
| **Fish** | 鱼 | CostItem | **500** | -1 | ✅ | — |

**过滤后预期行数**：

| 列表 | 行数 | 说明 |
|------|------|------|
| 购买页 | **7** | 全部 CostItem 且 buyPrice≥0（**不含 MpBall**） |
| 出售页 | **3** | 全部 MaterialItem 且 sellPrice≥0 |

### 3.3 ShopCatalogConfig（待废弃 · 与 Database 冲突）

**路径**：`Assets/GameRes/Config/Shop/ShopCatalogConfig.asset`

| 问题 | 示例 |
|------|------|
| Icon 重复手拖 | 8 条 buy 每条再拖一遍 Sprite |
| Price 与 Database 不一致 | 货单 MpBall price=0，Database buyPrice=-1 |
| sell price=0 | Database 素材 sellPrice=5 |
| displayName 已空 | 说明已在用 Provider 兜底，货单字段冗余 |

**当前程序**：`ShopFormLogic.RefreshBuyList / RefreshSellList` 遍历 `catalog.buyItems` / `sellItems`。

### 3.4 相关脚本现状

| 脚本 | 现状 | 本任务改动 |
|------|------|------------|
| `ShopFormLogic` | 依赖 `[SerializeField] ShopCatalogConfig catalog` | 改读 `MainItemDefProvider` 过滤列表 |
| `ShopBarRowView.Bind(ShopItemEntry)` | entry.icon / displayName / price | 新增 `Bind(MainItemDef, isBuyRow)` 或等价 |
| `ShopMainItemDisplayCache` | 货单 displayName 优先 | **可删除**（直接读 def.DisplayName） |
| `ShopMainItemIconResolver` | 已 Obsolete，转 Provider | 无新逻辑 |
| `ShopCatalogSetupEditor` | 绑定 catalog 引用 | 去掉 catalog 字段绑定 |

---

## ④ 目标架构（施工完成后）

### 4.1 数据流

```
MainItemDatabase.asset
        │
        ▼
MainItemDefProvider.EnsureLoaded()
        │
        ├── GetShopBuyCandidates()     ← 新增
        │      filter: CostItem && buyPrice >= 0
        │      order: entries 列表顺序（或 legacyNumericId）
        │
        └── GetShopSellCandidates()    ← 新增
               filter: MaterialItem && sellPrice >= 0
               order: 同上
        │
        ▼
ShopFormLogic.RefreshBuyList / RefreshSellList
        │
        ▼ Instantiate Shop_Bar.prefab
ShopBarRowView.Bind(def, isBuyRow)
        ├── Icon  ← def.Icon（Provider 已解析）
        ├── Name  ← def.DisplayName
        └── Price ← isBuyRow ? def.BuyPrice : def.SellPrice
```

### 4.2 Provider 新增 API（建议）

```csharp
// MainItemDefProvider.cs — 伪代码
public static IReadOnlyList<MainItemDef> GetShopBuyCandidates()
{
    EnsureLoaded();
    return _database.entries
        .Where(e => e.itemType == BagItemType.CostItem && e.buyPrice >= 0)
        .Select(e => GetDef(e.itemId))
        .Where(def => def != null)
        .ToList();
}

public static IReadOnlyList<MainItemDef> GetShopSellCandidates()
{
    EnsureLoaded();
    return _database.entries
        .Where(e => e.itemType == BagItemType.MaterialItem && e.sellPrice >= 0)
        .Select(e => GetDef(e.itemId))
        .Where(def => def != null)
        .ToList();
}
```

**排序约定**：默认 **Database `entries` 数组顺序**（与 `legacyNumericId` 一致）；策划可通过在 Inspector **上下拖动 Entry** 调整货架顺序。

**重要修改原因**：过滤逻辑集中在 Provider，背包 / 菜单 / 商店共用同一套「什么叫可买可卖」，避免 ShopFormLogic 内再写一遍 if。

### 4.3 ShopCatalogConfig 去留 · 替代方案

| 方案 | 做法 | 适用 |
|------|------|------|
| **A · 整文件删除（推荐）** | 删 `ShopCatalogConfig.cs` + `.asset`；`ShopFormLogic` 零引用 | 全游戏 **一家通用店**，规则就是 Cost/Material 过滤 |
| **B · 保留为「白名单」** | 只留 `List<EMainItemName> buyWhitelist` / `sellWhitelist`；空=不过滤 | 某 NPC 店只卖 subset |
| **C · 保留为「黑名单」** | `List<EMainItemName> hiddenFromShop` | 全局规则 + 个别下架 |

**本任务默认采用方案 A**；若 KenMuNi 村需「只卖药不卖鱼」，阶段六再开方案 B，**不在本任务 scope**。

### 4.4 ShopBarRowView Bind 签名（建议）

```csharp
// 新增主 Bind；旧 ShopItemEntry  overload 标记 Obsolete 后删除
public void Bind(MainItemDef def, bool isBuyRow)
{
    ItemId = def.ItemId;
    Price = isBuyRow ? def.BuyPrice : def.SellPrice;
    ApplyIcon(def.Icon);
    ApplyName(def.DisplayName);
    ApplyPrice(Price);
    if (!isBuyRow) ApplySellQuantityPlaceholder();
}
```

**替代方案说明**：也可保留 `Bind(ShopItemEntry)` 作 Editor 预览用，但会增加双路径维护成本；**推荐只留 MainItemDef 一条 Bind**。

### 4.5 策划工作流（施工后）

| 想做什么 | 只改 Database |
|----------|----------------|
| 新消耗品进购买页 | `itemType = CostItem`，`buyPrice` 填 ≥0 |
| 某消耗品下架 | `buyPrice = -1`（或改 TaskItem） |
| 新素材进出售页 | `itemType = MaterialItem`，`sellPrice` 填 ≥0 |
| 改单价 | 改 `buyPrice` / `sellPrice` |
| 改图标 / 名字 | 拖 `icon` / 改 `displayName` |
| 调整货架顺序 | Inspector 里拖动 `entries` 顺序 |

**不再需要** 打开 `ShopCatalogConfig`  duplicate 行、手拖 Icon。

---

## ⑤ 施工阶段（DB-0 ～ DB-5）

### 阶段 DB-0 · Provider 过滤 API

| 做 | 说明 |
|----|------|
| `MainItemDefProvider` 增加 `GetShopBuyCandidates` / `GetShopSellCandidates` | §4.2 |
| Editor 菜单或 `[ContextMenu]` Debug 打印两行数 | 快速对照 §3.2 表格（7 / 3） |

**验证 DB-0**：Play 前在 Editor 点 Debug → Console 输出 `Buy=7 Sell=3`。

### 阶段 DB-1 · ShopBarRowView 改 Bind

| 做 | 说明 |
|----|------|
| 新增 `Bind(MainItemDef def, bool isBuyRow)` | §4.4 |
| `ApplyIcon(Sprite)` 直接设 `def.Icon` | 不再经 ShopItemEntry |
| 删除或 Obsolete `ShopMainItemDisplayCache` | Name 直读 def |

**验证 DB-1**：单元 / 手动：对 HpBall def Bind 后 Icon/Name/Price=200。

### 阶段 DB-2 · ShopFormLogic 改刷新

| 做 | 说明 |
|----|------|
| `Awake` 内 `MainItemDefProvider.EnsureLoaded()` | 保证 Editor Play 同步可用 |
| `RefreshBuyList` 遍历 `GetShopBuyCandidates()` | 替换 `catalog.buyItems` |
| `RefreshSellList` 遍历 `GetShopSellCandidates()` | 替换 `catalog.sellItems` |
| 移除 `[SerializeField] ShopCatalogConfig catalog` 及 DefaultCatalogAssetPath | |
| `ValidateListRefreshInputs` 不再检查 catalog | |

**验证 DB-2**：Village_Shop Play → 购买 **7 行**、出售 **3 行**；HpBall 单价 **200**（非货单旧值）。

### 阶段 DB-3 · 清理 ShopCatalogConfig

| 做 | 说明 |
|----|------|
| 删除 `ShopCatalogConfig.cs`、`ShopCatalogConfig.asset` | 方案 A |
| 删 `ShopItemEntry` 类（若无其他引用） | |
| 更新 `ShopCatalogSetupEditor`：不再绑定 catalog | 改菜单文案为「Database Driven Lists」 |

**验证 DB-3**：工程内 **零引用** ShopCatalogConfig；编译通过。

### 阶段 DB-4 · 价格 / 合计 / 阶段四交易对齐

| 做 | 说明 |
|----|------|
| `GetCurrentHpBallBuyTotal` 仍读 HpBall 行 Bind 价 | 价来自 def.BuyPrice=200，与 Database 一致 |
| 阶段四 `OnConfirmClick` 注释更新 | 「价来自 MainItemDatabase」 |
| 若 MpBall 将来 `buyPrice>=0` | **自动出现在购买页**，无需改货单 |

**验证 DB-4**：改 Database HpBall buyPrice → Play → 行价 + TxtTotal 联动变化。

### 阶段 DB-5 · Database 数据校对 + 文档

| 做 | 说明 |
|----|------|
| 新 6 道具 **icon 字段**在 Database 拖入（截图路径 `ArtRes/UI/Item/Icon/`） | 与策划手册 Icon 约定一致 |
| MpBall：确认策划意图 — 保持 `-1` 不上架，或填买价上架 | |
| 素材 sellPrice 与策划对齐（当前 5） | |
| 更新 `0629/商店系统_策划拆解` 中 ShopCatalog 章节 | 指向 Database |

**验证 DB-5**：7 行购买页 Icon 全非空（或图集兜底可见）；策划仅维护 Database 验收通过。

---

## ⑥ 验收清单（DB-V1 ～ DB-V7）

| ID | 操作 | 期望 |
|----|------|------|
| DB-V1 | 打开 `MainItemDatabase`，确认 Cost/Material 分类与价 | 与 §3.2 一致 |
| DB-V2 | Village_Shop Play，默认购买 Tab | **7** 行 Shop_Bar；**无** MpBall |
| DB-V3 | 切出售 Tab | **3** 行；虫喙 / 藤蔓果 / 史莱姆核；单价 **5** |
| DB-V4 | Database 改 SmallHpPotion buyPrice 20→99，再 Play | 购买页该行显示 **99**；**未改 ShopCatalog**（已删） |
| DB-V5 | Database 某 CostItem buyPrice 改 -1 | 购买页 **少一行** |
| DB-V6 | Database 新增 MaterialItem + sellPrice=10 | 出售页 **多一行** |
| DB-V7 | 全项目搜索 `ShopCatalogConfig` | **0** 运行时引用 |

---

## ⑦ 技术要点与踩坑

### 7.1 为什么用「类型 + 价格」双条件

仅 `itemType == CostItem` 会把 **MpBall（buyPrice=-1）** 也摆上架；仅 `buyPrice>=0` 会把 **误标类型的 TaskItem** 摆上去。  
**双条件** = 「是消耗品 **且** 老板愿意卖」。

### 7.2 Icon：Database 拖 Sprite vs 图集

与 MainItem v2 一致：

1. **Entry.icon 已拖** → 商店直接用（中文文件名 Sprite 也可以，如「红烧鱼」）
2. **未拖** → `MainItem_Icon` 图集按 **enum 名**（`Fish`）取
3. Editor 兜底 → `ArtRes/UI/Item/Icon/{enum}.png`

**策划注意**：新 6 道具若 PNG 名是中文、enum 是 `Fish`，应在 **Database 拖 icon**，不要只靠图集。

### 7.3 与旧 ShopCatalog 行数差异

| 差异 | 原因 |
|------|------|
| 购买 8→7 | 货单含 MpBall；Database buyPrice=-1 过滤掉 |
| 出售 price 0→5 | 以 Database sellPrice 为准 |

施工后 **以 Database 为准**；若策划仍要 MpBall 可买，改 Database `buyPrice` 即可。

### 7.4 Async Load

`ShopFormLogic.Awake` 调用 `EnsureLoaded()`；若 Database 尚未异步完成，首帧可能 0 行 → 可在 `Start` 再 Refresh 一次，或 Village_Shop 场景预加载 Database（与背包 Init 一致）。

### 7.5 出售页仍不过滤背包

本任务 **只改列表来源**；出售行 Number 列仍占位 `1`。  
**阶段六**：`GetShopSellCandidates()` 结果再 ∩ 背包 count>0。

### 7.6 多商店 / NPC 差异

本任务 **全局一套过滤规则**。  
若以后要「武器店只卖剑、村医只卖药」，在 Provider 加 `shopId` 参数 + 方案 B 白名单，**另开文档**，不在此 scope。

---

## ⑧ 策划速查：还要维护 ShopCatalog 吗？

| 问题 | 答案 |
|------|------|
| 还要维护 ShopCatalogConfig 吗？ | **不要**（施工 DB-3 后删除） |
| 新道具怎么进商店？ | 只改 **MainItemDatabase** |
| 购买页显示什么？ | 所有 **CostItem 且 buyPrice≥0** |
| 出售页显示什么？ | 所有 **MaterialItem 且 sellPrice≥0** |
| 任务道具会进商店吗？ | **不会**（TaskItem） |
| 图标在哪配？ | Database **icon** 槽；或图集 + enum 名 |
| 顺序怎么调？ | Database **entries 列表顺序** |

---

## ⑨ 文档关系

| 文档 | 关系 |
|------|------|
| `MainItem_道具固有属性表_架构溯源与施工执行说明.md` v2 | **上游数据源**；本任务消费其 Output |
| `Shop_货单配置Asset与Shop_Bar数据刷新_架构溯源与施工执行说明.md` | **数据层被本任务取代**；UI Instantiate 部分仍有效 |
| `Shop_购买出售双列表Tab切换_架构溯源与施工执行说明.md` | Tab / 双 Scroll **不变** |
| `0629/商店系统_策划拆解_执行说明.md` | 施工后更新 §3.5 数据来源 |

---

## ⑪ Play 测试问题修复清单（2026-07-05 · 施工员优先）

> **来源**：Village_Shop 实机 Play 反馈四条问题。以下已对照场景 `Village_Shop.unity`、Prefab `Shop_Bar.prefab`、`ShopFormLogic` / `MainItemDefProvider` 源码做根因分析，并给出 **Fix 编号** 供施工排期。

### ⑪.0 问题总览

| # | 现象 | 根因类别 | Fix 编号 |
|---|------|----------|----------|
| 1 | Viewport 上不该有 Image | 滚动壳施工遗留 Unity 默认 Scroll View 结构 | **Fix-S1** |
| 2 | 滚轮很难滑、几乎滚不动 | ScrollSensitivity 过低 + 滚动条被禁用 | **Fix-S2** |
| 3 | Shop_Bar 的 Icon 没刷上道具图 | Icon 解析链 + Database 未绑 Sprite + 绑定时未兜底 | **Fix-I1～I4** |
| 4 | 出售列表怎么做 / 看不到 | 场景未建 Sell Scroll、引用未绑 | **Fix-L1～L3** |

---

### ⑪.1 问题一：Viewport 不需要 Image

#### 现象

策划 / 测试认为 `Bar_ListScroll` / `Viewport` 节点上的 **Image 组件多余**，只需要裁剪（Mask）即可。

#### 根因分析

| 项 | 现状（`Village_Shop.unity` 只读） |
|----|----------------------------------|
| `Viewport` 组件 | **RectMask2D** + **Image**（`raycastTarget = 1`） |
| 来源 | `DefaultControls.CreateScrollView` 默认给 Viewport 挂 Image + Mask；`ShopBarListScrollSetupEditor.ConfigureViewport` 只 **补** 了 `RectMask2D`，**未删 Image** |
| 影响 | 多一层无意义 Graphic；部分 Unity 版本下 Viewport Image 还会参与射线检测，与滚动体验纠缠 |

**结论**：已使用 **RectMask2D** 时，Viewport **不需要 Image**。裁剪只靠 RectMask2D 即可。

#### 施工 Fix-S1

| 步骤 | 操作 |
|------|------|
| S1-1 | `Village_Shop` → `UI_Shop/Bar/Bar_ListScroll_Buy/Viewport`（及 `_Sell` 若已 Duplicate）→ **Remove Component → Image** |
| S1-2 | 保留 **RectMask2D**；确认 Viewport 仍被 ScrollRect 的 `Viewport` 字段引用 |
| S1-3 | 改 `ShopBarListScrollSetupEditor.ConfigureViewport`：若检测到 `RectMask2D`，**自动 DestroyImmediate Viewport 上的 Image**（避免下次跑菜单又加回来） |
| S1-4 | **滚动命中区域**：Viewport 无 Image 后，须保证 **ScrollRect 所在节点**（`Bar_ListScroll_*` 根）仍有 **Image 且 raycastTarget = true**（透明即可），否则鼠标拖拽滚动会失效 — 见 Fix-S2 |

**验收 Fix-V-S1**：Viewport Inspector 仅见 RectTransform + RectMask2D；列表仍被正确裁剪。

---

### ⑪.2 问题二：滚轮太难滑

#### 现象

购买列表超过 6 行时，鼠标滚轮要滚很多下才动一点，**很难滑动**。

#### 根因分析

| 项 | 现状 | 说明 |
|----|------|------|
| `ScrollRect.scrollSensitivity` | **1** | Unity 默认；Content 总高 ≈ 7×(88+16) > 700px，每格滚轮只移 1px，体感「转不动」 |
| 垂直滚动条 | **已禁用** | `ShopBarListScrollSetupEditor.HideVerticalScrollbarInGame` 把 `Scrollbar Vertical` 设为 Inactive 且 `verticalScrollbar = null` |
| 拖拽滚动 | 依赖 Graphic 射线 | 仅滚轮时无滚动条可拖，Sensitivity 又低 → 体验差 |
| Viewport Image | raycastTarget = 1 | 非主因，但 Fix-S1 后需把 raycast 移到 Scroll 根节点 |

**结论**：当前 SC-4「仅滚轮、隐藏滚动条」在 **Sensitivity=1** 下不可用，必须 **提高灵敏度** 和/或 **恢复滚动条**。

#### 施工 Fix-S2

| 步骤 | 操作 | 推荐值 |
|------|------|--------|
| S2-1 | Buy / Sell 两个 `ScrollRect` → **Scroll Sensitivity** | **30～40**（先 30 Play 微调） |
| S2-2 | **二选一**（推荐 A） | |
| | **A · 恢复垂直滚动条** | 显示 `Scrollbar Vertical`，`ScrollRect.verticalScrollbar` 重新绑定；策划可拖条浏览 |
| | **B · 仍隐藏滚动条** | 保持 SC-4，但 Sensitivity ≥ 30，并确保 Scroll 根 Image `raycastTarget=true` 支持 **按住拖拽** |
| S2-3 | 改 `ShopBarListScrollSetupEditor` | 新建/更新 Scroll 时写入 `scrollSensitivity = 30f`；菜单可选「保留滚动条 / 仅滚轮」 |
| S2-4 | `ShopCatalogSetupEditor` Duplicate Sell Scroll 时 | **同步复制** 上述 ScrollRect 参数，避免 Sell 页仍用旧灵敏度 |

**验收 Fix-V-S2**：购买页 7 行，滚轮 **2～3 格** 可滚过一行；或滚动条拖到底可见最后一行。

---

### ⑪.3 问题三：Icon 没有刷进 Shop_Bar

#### 现象

运行时 `Shop_Bar` 的 **Icon** 列为空（或仍是 Prefab 默认占位图），Name / Price 可能有值。

#### 根因分析（多层叠加，需全部排查）

**① 数据层：MainItemDatabase 里 icon 为空**

| 道具 | Database `icon` | 说明 |
|------|-----------------|------|
| HpBall / MpBall 等老 9 项 | ✅ 已拖 Sprite | 应能显示 |
| 小/大生命药、小/大体力药、碗装液体、鱼（6 项） | ❌ `icon: {fileID: 0}` | **未绑** |
| 美术资源 | `ArtRes/UI/Item/Icon/` 下 PNG 名为 **中文**（如「红烧鱼」「精灵秘药30%」） | 与 enum 名 `Fish`、`SmallHpPotion` **不一致** |

**② 解析链：Bind 用的是缓存 `def.Icon`，不是实时 Resolve**

```
MainItemDefProvider.RebuildCache()
  → MainItemDef.Icon = ResolveIconInternal(...)   // 建缓存时算一次
ShopBarRowView.Bind(def)
  → ApplyIcon(def.Icon)                           // 不再调 ResolveIcon
```

若建缓存时图集 **尚未异步加载**（`GameManager` 不可用，Village_Shop 测试场景常见），且 Entry 未拖 icon、Editor 兜底 `Icon/{enum}.png` 也不存在 → **`def.Icon == null` 写死进 MainItemDef**。

**③ 图集兜底名 ≠ 中文文件名**

`ResolveIconInternal` 图集分支：`GetSprite(itemId.ToString())` → 要找 **`Fish`**，不是「红烧鱼」。中文 PNG **必须**在 Database **icon 槽手拖**，或把 PNG 以 enum 名入库并打进 `MainItem_Icon` 图集。

**④ 旧货单 Icon 已删，无迁移**

以前 `ShopCatalogConfig` 里手拖的 Icon **不会**自动写回 Database；改 Database 驱动后，若 Database 未绑 icon，商店比旧版 **更空**。

**⑤ UI 层（低概率，顺带验）**

`ShopBarRowView` 通过 `transform.Find("Icon")?.GetComponent<Image>()` 绑定；Prefab 结构正确（`Shop_Bar/Icon` 有 Image）。若 `sprite == null`，代码会 `iconImage.enabled = false` → **看起来「没图标」**。

#### 施工 Fix-I1～I4

| Fix | 负责 | 操作 |
|-----|------|------|
| **Fix-I1 · 策划数据** | 策划 | 打开 `MainItemDatabase.asset`，给 6 个新消耗品 **icon 槽拖入** `ArtRes/UI/Item/Icon/` 下对应 Sprite（中文文件名即可，不强制改名） |
| **Fix-I2 · Bind 实时解析** | 程序 | `ShopBarRowView.Bind` 改为：`ApplyIcon(MainItemDefProvider.ResolveIcon(def.ItemId))`，**不要只用 def.Icon** |
| **Fix-I3 · 图集加载后重刷** | 程序 | `MainItemDefProvider` 图集异步 Load 完成、`RebuildCache()` 之后，通知 `ShopFormLogic` 再调 `RefreshBuyList()` / `RefreshSellList()`（可用静态 event 或 Start 延迟一帧 Refresh） |
| **Fix-I4 · 诊断 Log** | 程序 | Bind 时若 `ResolveIcon` 仍 null，`Debug.LogWarning` 输出 itemId + 提示「Database 拖 icon 或补 enum 名 PNG/图集」 |

**替代方案说明**：

| 方案 | 做法 | 取舍 |
|------|------|------|
| **A（推荐）** | Database Inspector 拖 icon | 最直观，支持中文 Sprite 名 |
| **B** | 统一 PNG 文件名为 enum（`Fish.png`）+ 打进 `MainItem_Icon` | 程序友好，美术要改名 |
| **C** | Database 增加 `iconKey` 字符串字段 | 改动大，本阶段不做 |

**验收 Fix-V-I**：

- [ ] HpBall 行 Icon 显示生命之珠图  
- [ ] 小生命药等 6 项在 Fix-I1 后显示对应中文 Icon  
- [ ] Console 无 `[ShopBarRowView] Icon null` 类 Warning（或仅剩未配数据的项）

---

### ⑪.4 问题四：出售列表怎么做

#### 现象

测试不知道 **出售页列表从哪来**；点击 SELL Tab 可能 **空白 / 仍显示购买列表 / Console 警告**。

#### 根因分析

| 项 | 现状 | 后果 |
|----|------|------|
| 场景 Hierarchy | 仅有 **`Bar_ListScroll`**（旧名），**无 `Bar_ListScroll_Sell`** | Sell 无容器 |
| `ShopFormLogic` 序列化 | `sellContent: null`、`barListScrollSell: null`、`btnSell: null`（场景快照） | `RefreshSellList()` → `ValidateListRefreshInputs` **直接跳过** |
| 程序逻辑（已实现） | `GetShopSellCandidates()` = MaterialItem 且 sellPrice≥0 | 数据侧应有 **3 行**：虫喙、藤蔓果、史莱姆核 |
| 一键施工菜单 | **`Tools → Shop → Setup Database Driven Lists`** | 会 Duplicate Buy→Sell、清空 Content、绑引用 — **场景尚未跑过** |

**结论**：出售列表 **不是再写一份货单**，而是 **(1) 场景双 Scroll 壳 + (2) Database 素材类过滤**；当前卡在 **(1) 场景未施工**。

#### 施工 Fix-L1～L3

**Fix-L1 · 场景结构（必做）**

1. 打开 `Assets/GameRes/Scenes/Village_Shop.unity`  
2. 菜单 **`Tools → Shop → Setup Database Driven Lists`**  
3. 确认 Hierarchy：

```
UI_Shop/Bar/
├── Bar_BG
├── Bar_ListScroll_Buy/          ← 原 Bar_ListScroll 重命名
│   └── Viewport/Content         ← 运行时 RefreshBuyList 生成行
└── Bar_ListScroll_Sell/         ← Duplicate 自 Buy，默认 SetActive(false)
    └── Viewport/Content         ← 运行时 RefreshSellList 生成行
```

4. 确认 `UI_Shop` 上 `ShopFormLogic`：`buyContent` / `sellContent` / `barListScrollBuy` / `barListScrollSell` / `btnSell`（绑 `SELL` 按钮）**非空**

**Fix-L2 · 数据（策划 / 已具备则跳过）**

在 `MainItemDatabase` 中，出售候选须同时满足：

| 字段 | 值 |
|------|-----|
| `itemType` | **MaterialItem** |
| `sellPrice` | **≥ 0**（当前虫喙 / 藤蔓果 / 史莱姆核 = 5） |

**不需要**单独维护出售货单 Asset。

**Fix-L3 · 程序 Tab 切换（已实现，验收即可）**

- `SwitchToSellTab()`：`Bar_ListScroll_Buy` SetActive(false)，`Bar_ListScroll_Sell` SetActive(true)  
- `RefreshSellList()` 在 `Awake` / `Start` 已调用  

若 Sell 仍空：查 Console 是否有 `[ShopFormLogic] RefreshSellList 跳过：Content 未就绪`。

#### 出售列表「制作」流程（给策划 / 测试）

```
① MainItemDatabase：素材 itemType=MaterialItem，sellPrice 填好，icon 拖好
        ↓
② 场景跑 Setup Database Driven Lists（仅首次或 Scroll 被删时）
        ↓
③ Play → 点 SELL Tab
        ↓
④ 应见 3 行 Shop_Bar（虫喙 / 藤蔓果 / 史莱姆核），单价 = sellPrice
```

**验收 Fix-V-L**：

- [ ] Play 点 **SELL**，购买 Scroll 隐藏、出售 Scroll 显示  
- [ ] 出售 Content 下 **3 行** `Shop_Bar_InsectBeak` 等  
- [ ] 每行 Price = 5；Icon 在 Fix-I 完成后可见  

---

### ⑪.5 施工排期建议（Fix 依赖）

```
Fix-L1 场景双 Scroll（出售列表前提）
    ↓
Fix-S1 Viewport 去 Image
    ↓
Fix-S2 滚动灵敏度 / 滚动条
    ↓
Fix-I1 Database 拖 icon（策划）
Fix-I2～I4 程序 Bind + 重刷（可与 I1 并行）
    ↓
Fix-V-S1 / V-S2 / V-I / V-L 全量 Play 验收
```

| 优先级 | Fix | 预估 |
|--------|-----|------|
| P0 | L1 出售 Scroll 场景施工 | 0.5h |
| P0 | I1 + I2 Icon 显示 | 1h |
| P1 | S2 滚动体验 | 0.5h |
| P2 | S1 Viewport 去 Image + 改 Editor 脚本 | 0.5h |
| P2 | I3 图集异步后重刷 | 0.5h |

---

## ⑩ 修订记录

| 日期 | 说明 |
|------|------|
| 2026-07-05 | 首版：废弃 ShopCatalog 双列表；Database 按 CostItem/MaterialItem + 价格过滤驱动 Shop_Bar；DB-0～DB-5 与 DB-V1～V7 验收 |
| 2026-07-05 | **v1.1**：追加 §⑪ Play 测试四条问题（Viewport Image / 滚轮灵敏度 / Icon 未刷 / 出售列表施工）及 Fix-S/I/L 验收项 |
