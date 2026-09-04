# Shop · 货单配置 Asset + Shop_Bar 数据刷新 — 架构溯源与施工执行说明

**文档性质**：架构侦探产出（只读溯源 + 施工指引；**本阶段先写文档，代码待施工**）  
**调查日期**：2026-07-05  
**依据**：
- `Assets/Doc/00_MASTER_PROMPT.md`【架构侦探】
- `Assets/Doc/执行文档/0704/Shop_Bar列表滚动_架构溯源与施工执行说明.md`（Scroll 壳 SC-0～SC-4）
- `Assets/Doc/执行文档/0704/Shop_购买出售双列表Tab切换_架构溯源与施工执行说明.md`（双 Scroll + Tab；**§①.1「Editor 手摆行数」已被本任务取代**）
- `Assets/Doc/执行文档/0629/商店系统_策划拆解_执行说明.md` §3.5 / §3.6 / 阶段五～六（`ShopConfig.json` 预告）
- 关联 Prefab：`Assets/GameRes/Prefabs/UI/Shop/Shop_Bar.prefab`
- 关联场景：`Assets/GameRes/Scenes/Village_Shop.unity`

**Unity 版本**：2020.3.48f1  

---

## ① 结论（一句话）

**新增商店货单 ScriptableObject（`ShopCatalogConfig`）：每条货目绑定道具 ID，并配置 Icon / Name / Price；Asset 内分 `buyItems` / `sellItems` 两个列表决定 BUY / SELL 各生成多少条 `Shop_Bar`；运行时由列表控制器按配置 `Instantiate` 行预制体并刷新 UI，Tab 仍切换 `Bar_ListScroll_Buy` / `_Sell` 显隐。**

---

## ①.1 范围冻结（2026-07-05 · 策划 / 施工对齐）

| 项 | 本阶段约定 | 说明 |
|----|------------|------|
| **数据来源** | **`ShopCatalogConfig`（.asset）** | Inspector 可编辑；**不**在场景里手摆 `Shop_Bar` 数量 |
| **单条货目字段** | **Icon、Name、Price** + 道具 ID | 展示信息跟着道具走；Price 为商店价（买价 / 卖价） |
| **货单划分** | **`buyItems` / `sellItems` 两个列表** | 列表长度 = 对应 Scroll `Content` 下 `Shop_Bar` 行数 |
| **UI 刷新** | **运行时 Instantiate + Bind** | 读 Asset → 清 Content → 按条生成 `Shop_Bar` → 写 Icon/Name/Price |
| **Scroll 结构** | 继承 0704 + 双列表 Tab 文档 | `Bar_ListScroll_Buy` / `_Sell`；Tab `SetActive` 互斥 |
| **本阶段不做** | Tab 高亮、真扣金币 / 背包、出售页背包过滤 | 出售列表**先按 Asset 静态货单**生成；背包联动留阶段六 |
| **本阶段不做（续）** | Excel 导表、`ShopConfig.json` 运行时 Load | 见 §4.4 替代方案；首版用 SO 落地 |

---

## ② 玩家在做什么 / 会遇到什么现象

| 现象 | 原因（生活类比） |
|------|------------------|
| 改商店卖什么、卖多少钱，不应改场景 Hierarchy | 货单应像「价目表 Excel」，改 **Asset** 即可，不用动 UI 树 |
| 购买页 2 种药水、出售页 3 种素材，行数不同 | 价目表分 **进货页 / 收货页** 两栏，栏里各有几条就显示几条 |
| 每行要显示图标、名字、单价 | `Shop_Bar` 是「标价牌」；数据从货单 **刷** 上去，不是写死在 Prefab 里 |
| 以后加新道具进商店，只加 Asset 一行 | 程序按列表 **自动生成** 行，不用 Duplicate 预制体 |

**生活类比**：`ShopCatalogConfig` 是老板手里的 **价目本**；`Shop_Bar` 是货架上的 **空白价签**；开店时按价目本条目数挂价签、填图标名字价格。BUY / SELL 是两本不同的价目页，页数（行数）由各自列表条目数决定。

---

## ③ 工程现状快照（架构侦探 · 只读）

### 3.1 `Shop_Bar.prefab` 行结构（当前）

```
Shop_Bar（482×88，Image + Button 行选中）
├── Shadow
├── Icon          ← Image，道具图标
├── Name          ← Text，道具名称
├── Price         ← Text，单价
└── Number        ← Text（第四列；0629 文档称 TxtStock，Prefab 实际名 Number）
```

| Prefab 节点 | 0629 / 旧脚本约定 | 说明 |
|-------------|-------------------|------|
| `Icon` | `Image` / `ImgIcon` | **以 Prefab 为准**：Bind 时 Find `"Icon"` |
| `Name` | `TxtName` | Bind 时 Find `"Name"` |
| `Price` | `TxtPrice` | Bind 时 Find `"Price"` |
| `Number` | `TxtStock` | `ShopBuyRowQuantityInput` 仍 Find `"TxtStock"` → **需对齐**，见 §6.2 |

### 3.2 道具主数据（已有 · 不含价格）

**路径**：`Assets/GameRes/Config/MainItemConfig/MainItemConfig.json`  
**运行时**：`PlayerBagData` → `LoadConfig<MainItemDataTableRow>`

| 字段 | 示例 HpBall | 商店是否直接用 |
|------|-------------|----------------|
| `name` | `HpBall` | ✅ 与 `EMainItemName` 一致，作道具 ID |
| `cnName` | `生命之珠` | ✅ 可作 Name **默认值** |
| `itemType` | `1`（消耗品） | 出售过滤阶段六再用 |
| **price** | ❌ 无 | 商店价 **只在** `ShopCatalogConfig` |

**图标资源**：
- 列表小图：`Assets/ArtRes/UI/Item/Icon/{ItemName}.png`
- 图集：`Assets/GameRes/Atlas/MainItem_Icon.spriteatlas`（Sprite 名 = 枚举名）

### 3.3 程序侧（商店 · 当前）

| 项 | 状态 | 问题 |
|----|------|------|
| `ShopFormLogic.HpBallUnitPrice = 200` | 写死常量 | 应迁入 `ShopCatalogConfig.buyItems[].price` |
| `rowHpBall` / `rowMpBall` 序列化 + Find | 场景预置两行 | 与「动态货单」冲突；接 Asset 后 **废弃预置行** |
| `ShopBuyRowQuantityInput` | Find `TxtStock` | Prefab 为 `Number`，需统一命名或 Bind 兼容 |
| 货单配置 | ❌ 无 | 0629 预告 `ShopConfig.json`；**本任务改用 SO** |
| 列表动态生成 | ❌ 无 | 可参考 `AchievementFormLogic` Instantiate 行 Prefab |

### 3.4 与前置文档关系

| 前置文档 | 本任务处理 |
|----------|------------|
| 0704 滚动壳 | **保留** Viewport 559、Spacing 16、滚轮、双 Scroll |
| 0704 Tab 切换 | **保留** Buy/Sell Scroll `SetActive`；行数 **不再手摆** |
| 0629 `ShopConfig.json` | **首版不实现**；SO 字段对齐后便于日后导出 JSON，见 §4.4 |

---

## ④ 目标架构（施工完成后）

### 4.1 数据层：`ShopCatalogConfig.asset`

**建议路径**：`Assets/GameRes/Config/Shop/ShopCatalogConfig.asset`  
**脚本**：`ShopCatalogConfig.cs`（`ScriptableObject`）

```csharp
// 单条货目：跟着道具走，但允许 Inspector 覆盖展示与价格
[Serializable]
public class ShopItemEntry
{
    public EMainItemName itemId;     // 道具 ID，对齐 MainItemConfig.name / 枚举
    public Sprite icon;              // 列表图标；空则运行时从 MainItem_Icon 图集取
    public string displayName;         // 空则回退 MainItemConfig.cnName
    public int price;                // 买价或卖价（由所在列表 buy/sell 决定语义）
}

[CreateAssetMenu(fileName = "ShopCatalogConfig", menuName = "Config/Shop/ShopCatalogConfig")]
public class ShopCatalogConfig : ScriptableObject
{
    public List<ShopItemEntry> buyItems;
    public List<ShopItemEntry> sellItems;
}
```

**首版示例数据（写入默认 .asset）**：

| 列表 | itemId | displayName（可空） | price | icon |
|------|--------|---------------------|-------|------|
| buyItems[0] | HpBall | （空→生命之珠） | 200 | HpBall.png |
| buyItems[1] | MpBall | （空→体力之珠） | 待定 | MpBall.png |
| sellItems[0] | InsectBeak | （空→虫喙） | 待定 | InsectBeak.png |
| sellItems[1] | TenWangFruit | | 待定 | TenWangFruit.png |
| sellItems[2] | SlimeCore | | 待定 | SlimeCore.png |

> **重要**：`buyItems.Count` = 购买 Scroll 行数；`sellItems.Count` = 出售 Scroll 行数；**改 Asset 列表即改 UI 行数**，无需改场景。

### 4.2 表现层：`Shop_Bar` + 行绑定脚本

**新增组件**：`ShopBarRowView.cs`（挂在 `Shop_Bar` Prefab 根节点）

| 职责 | 说明 |
|------|------|
| `Bind(ShopItemEntry entry, ShopBarRowContext ctx)` | 写 `Icon.sprite`、`Name.text`、`Price.text` |
| 缓存 `EMainItemName ItemId` | 供合计、交易、选中态 |
| 数量列 | 购买行挂 `ShopBuyRowQuantityInput`；出售行 Number 显示持有数（阶段六） |

**Bind 节点约定（与 Prefab 一致）**：

```
Icon   → Image.sprite
Name   → Text / TMP（优先 Text，与 Prefab 一致）
Price  → Text，显示 entry.price.ToString()
Number → 购买：TMP_InputField；出售：只读持有数（本阶段可仍显示 "1" 占位）
```

### 4.3 控制层：列表刷新 + Tab

```
ShopFormLogic（或拆出的 ShopBarListController）
├── [SerializeField] ShopCatalogConfig catalog
├── [SerializeField] Transform buyContent      // Bar_ListScroll_Buy/Viewport/Content
├── [SerializeField] Transform sellContent
├── [SerializeField] GameObject shopBarPrefab  // Shop_Bar.prefab
│
├── RefreshBuyList()   // Destroy 旧子节点 → foreach buyItems → Instantiate → Bind
├── RefreshSellList() // 同上，sellItems
├── SwitchToBuyTab()  // Buy Scroll On, Sell Off（继承 Tab 文档）
└── SwitchToSellTab()
```

**刷新时机**：

| 时机 | 调用 |
|------|------|
| 商店 `Awake` / `Start` | `RefreshBuyList()` + `RefreshSellList()`；默认 `SwitchToBuyTab()` |
| 修改 Asset 后（Editor） | 可选：`[ContextMenu("Refresh Preview")]` 或 Play 重进 |
| 阶段六 · 打开出售 Tab | `RefreshSellList()` 可叠加背包过滤（本阶段不做） |

**对象池（可选 · 非首版）**：条目数少（≤10），直接 Destroy/Instantiate 即可；条目变多再引入池。

### 4.4 目标 Hierarchy（运行时）

```
Bar
├── Bar_BG
├── Bar_ListScroll_Buy
│   └── Viewport / Content          ← 运行时 0 子节点起；RefreshBuyList 生成 N 行
└── Bar_ListScroll_Sell             ← 默认 Inactive
    └── Viewport / Content          ← RefreshSellList 生成 M 行
```

**N = catalog.buyItems.Count，M = catalog.sellItems.Count**（Asset 驱动，非 Editor 手摆）。

### 4.5 替代方案说明

| 方案 | 做法 | 优点 | 缺点 | 本任务 |
|------|------|------|------|--------|
| **A · ScriptableObject 货单（推荐）** | `ShopCatalogConfig.asset`，Inspector 编辑 | 策划 / 程序改价目直观；与 Unity 工作流贴合；**行数随列表变** | 大量条目时不如 Excel；与现有 JSON 表双轨 | ✅ **选用** |
| B · `ShopConfig.json` + DataTable | 0629 原方案，`LoadConfig` 异步读表 | 与 `MainItemConfig` 一致；可 Excel 导出 | 首版施工量大；改价需改 JSON / 导表 | 阶段五后可 **从 SO 导出** |
| C · 仅引用 `EMainItemName`，价格写代码 | 枚举 + 字典 | 最快 | 违反「数据跟着道具走、可配置」；难维护 | ❌ 不采用 |
| D · 每条货目一个独立 `.asset` | `ShopItemEntrySO` × N | 极度模块化 | 文件爆炸；列表关系不直观 | ❌ 过度设计 |

**SO → JSON 迁移路径（预留）**：`ShopItemEntry` 字段与 0629 `ShopConfig` 列对齐（`itemId`、`buyPrice`/`sellPrice`、`side`），日后加 Editor 菜单 `Export ShopCatalog to JSON`。

---

## ⑤ 分阶段施工

### 阶段 SD-0 · 定义数据结构与 Asset

| 做 | 不做 |
|----|------|
| 新建 `ShopItemEntry` + `ShopCatalogConfig.cs` | 不改 `ShopFormLogic` 业务 |
| `CreateAssetMenu` → 创建 `ShopCatalogConfig.asset` | 不实现交易 |
| 填入首版 buyItems（HpBall、MpBall）与 sellItems（素材 3 种） | MpBall / 素材 price 可先填 0 或占位，策划后补 |

**验证 SD-0**：Project 窗口可见 `.asset`；Inspector 可增删 buy/sell 列表条目；改列表长度保存成功。

### 阶段 SD-1 · 行绑定 `ShopBarRowView`

| 做 | 不做 |
|----|------|
| `ShopBarRowView.Bind(ShopItemEntry)` 写 Icon/Name/Price | 不接背包 |
| `displayName` 空时读 `MainItemConfig` cnName（同步或 Awake 缓存表） | |
| `icon` 空时从 `MainItem_Icon` 图集 `GetSprite(itemId.ToString())` | |
| 挂到 `Shop_Bar.prefab` 根节点 | |

**验证 SD-1**：Play 模式下手动 Instantiate 一行 + Bind 一条 entry → Game 视图图标名价正确。

### 阶段 SD-2 · 列表控制器 `RefreshBuyList` / `RefreshSellList`

| 做 | 不做 |
|----|------|
| 清空 `buyContent` / `sellContent` 旧子节点 | Tab 高亮 |
| 按 `catalog.buyItems` / `sellItems` **Instantiate** `shopBarPrefab` | 出售背包过滤 |
| 每行 `GetComponent<ShopBarRowView>().Bind(entry)` | |
| 购买行：`EnsureComponent<ShopBuyRowQuantityInput>()` | |

**验证 SD-2**：Asset buy=2、sell=3 → Play 后 Buy Content **2** 行、Sell Content **3** 行；改 Asset 为 buy=3 → 重进 Play **3** 行。

### 阶段 SD-3 · 接入 `ShopFormLogic` + 双 Scroll Tab

| 做 | 不做 |
|----|------|
| 序列化 `catalog`、`buyContent`、`sellContent`、`shopBarPrefab` | 删除旧 `HpBallUnitPrice` 常量前确保 TxtTotal 改读 Bind 价 |
| `Awake`：`RefreshBuyList()`、`RefreshSellList()` | |
| `SwitchToBuyTab` / `SwitchToSellTab`：Scroll `SetActive`（Tab 文档） | |
| 绑 `btnSell`；移除对 `rowHpBall`/`rowMpBall` 预置依赖 | |
| `UsesScrollListLayout` 识别 `Bar_ListScroll_Buy` | |

**验证 SD-3**：默认购买页；点 SELL 见出售货单行数 = sellItems.Count；点 BUY 切回；Console 无 NullReference。

### 阶段 SD-4 · 命名对齐与旧逻辑迁移

| 项 | 操作 |
|----|------|
| `Number` vs `TxtStock` | **二选一**：Prefab 重命名 `Number`→`TxtStock`，或 `ShopBuyRowQuantityInput` 增加 Find `"Number"` 兜底 |
| `ShopFormLogic.ApplyHpBallUnitPriceLabel` | 删除或改为 Refresh 后由 `ShopBarRowView` 写 Price |
| `GetBuyQuantity(EMainItemName)` | 改为遍历 Buy Content 行，`ShopBarRowView.ItemId` 匹配 |
| `TxtTotal` | 仍可按选中行 / HpBall 行数量 × Bind 价（阶段三逻辑，最小改动） |

**验证 SD-4**：改数量 → TxtTotal 仍正确；`[ShopDebug]` 决定按钮 Log 仍可用。

### 阶段 SD-5 · 场景清理（Editor）

| 做 | 不做 |
|----|------|
| 删除 `Bar_ListScroll_* / Content` 下 **手摆** 的测试 `Shop_Bar` | 不删 Scroll 壳 |
| `ShopFormLogic` Inspector 拖 `ShopCatalogConfig`、两个 Content | |
| 可选：Editor 菜单 `Tools/Shop/Validate Shop Catalog` 检查 icon/price 空项 | |

---

## ⑥ 技术要点与踩坑

### 6.1 Name / Icon 与 MainItemConfig 的分工

| 字段 | 权威来源 | ShopCatalog 角色 |
|------|----------|------------------|
| 道具 ID | `EMainItemName` + MainItemConfig.name | `itemId` **必填** |
| 中文名 | MainItemConfig.cnName | `displayName` **可选覆盖**（活动价签、简称） |
| 图标 | ArtRes Icon 或图集 | `icon` **可选覆盖**（特殊商店皮肤） |
| 价格 | **仅商店** | `price` **必填**；买 / 卖列表各自语义 |

### 6.2 Prefab 节点名与旧脚本

| 风险 | 处理 |
|------|------|
| `ShopBuyRowQuantityInput` Find `TxtStock`，Prefab 为 `Number` | SD-4 统一命名；**推荐 Prefab 改回 `TxtStock`** 与 0629 一致 |
| `ShopFormLogic` Find `TxtPrice` | 改为 `ShopBarRowView` 内部 Find `Price` |

### 6.3 Refresh 与 Scroll 滚动位置

| 做法 | 说明 |
|------|------|
| 仅 **Awake 刷新一次** | Tab 切换不 Re-Refresh → 滚动位置自然保留 |
| 若阶段六「每次开出售页 Refresh」 | Refresh 前可存 `ScrollRect.verticalNormalizedPosition` 再还原，或仅 Destroy 变化行 |

### 6.4 Content 为空时的 Layout

Refresh 前 Content **无子节点** 是正常的；Instantiate 后 `ContentSizeFitter` + `VerticalLayoutGroup` 自动撑高。超过 6 行仍靠 0704 滚轮。

### 6.5 与 Achievement 动态列表的异同

| 项 | Achievement | Shop |
|----|-------------|------|
| 数据来源 | JSON 配置 + Mgr | **ShopCatalogConfig.asset** |
| Prefab | `AchievementItem` | `Shop_Bar` |
| 触发 | 配置 Load 回调 | 商店 Awake / Tab（出售后续） |

---

## ⑦ 验收清单（本任务 · 数据驱动货单）

| # | 操作 | 期望 |
|---|------|------|
| SD-V1 | 看 `ShopCatalogConfig.asset` | 含 `buyItems`、`sellItems` 两列表；条目可增删 |
| SD-V2 | Asset buy=2、sell=3，Play | Buy **2** 行、Sell **3** 行；Icon/Name/Price 与 Asset 一致 |
| SD-V3 | Asset buy 改为 4 条，重进 Play | Buy **4** 行；**无需**改场景 Hierarchy |
| SD-V4 | 点 SELL / BUY | 双 Scroll 互斥显隐；行数仍由 Asset 决定 |
| SD-V5 | buy ≥ 7 条 | 购买列表滚轮可用；无侧边 Scrollbar |
| SD-V6 | 改 Asset 某条 price | Play 后对应行 Price 文本更新 |
| SD-V7 | Console | 无 Missing 组件；`icon` 空时图集兜底成功 |

> **接数据后追加**：0629 阶段四决定 Log、阶段六出售背包过滤与持有数。

---

## ⑧ 待确认问题

| ID | 问题 | 影响 | 状态 |
|----|------|------|------|
| ~~Q1~~ | 行数谁决定 | — | ✅ **`ShopCatalogConfig` 列表长度**（推翻 Tab 文档「Editor 手摆」） |
| Q2 | MpBall 买价、素材卖价具体数值 | Asset 填表 | 📝 策划补；可先占位 0 |
| Q3 | `Number` 改 `TxtStock` 还是脚本兼容 | SD-4 | 📝 **推荐改 Prefab** 对齐 0629 |
| Q4 | 控制器放 `ShopFormLogic` 内还是拆 `ShopBarListController` | 文件结构 | 📝 条目少可 **先内聚**；超过 ~200 行再拆 |
| Q5 | 日后是否导出 `ShopConfig.json` | 0629 对齐 | 📝 预留字段即可；**非本任务** |

> 仍无结论的写入 `Assets/Doc/OPEN_QUESTIONS.md`。

---

## ⑨ 给程序看的入口索引

| 主题 | 路径 |
|------|------|
| 行预制体 | `Assets/GameRes/Prefabs/UI/Shop/Shop_Bar.prefab` |
| 道具主表 | `Assets/GameRes/Config/MainItemConfig/MainItemConfig.json` |
| 道具枚举 | `Assets/Scripts/Game/Static/Enum/Goods/EMainItemName.cs` |
| 主表 Row 类 | `Assets/Scripts/Game/DataTable/MainItem/MainItemDataTableRow.cs` |
| 商店逻辑（待改） | `Assets/Scripts/Game/GameRuntime/UI/FormLogic/Shop/ShopFormLogic.cs` |
| 数量输入 | `ShopBuyRowQuantityInput.cs` |
| 动态列表参考 | `AchievementFormLogic.cs` |
| SO 先例 | `PlayerStaminaConfig.cs` |
| 滚动壳 + Tab | `0704/Shop_Bar列表滚动_*`、`0704/Shop_购买出售双列表Tab切换_*` |
| 策划货单预告 | `0629/商店系统_策划拆解_执行说明.md` §3.6 / 阶段五 |

**本任务新增（施工时创建）**：

| 文件 | 说明 |
|------|------|
| `ShopCatalogConfig.cs` | SO + `ShopItemEntry` |
| `ShopBarRowView.cs` | 单行 Bind |
| `Assets/GameRes/Config/Shop/ShopCatalogConfig.asset` | 默认货单 |
| （可选）`ShopBarListController.cs` | Refresh 逻辑 |

---

## ⑩ 文档变更记录

| 日期 | 说明 |
|------|------|
| 2026-07-05 | 初稿：推翻「Editor 手摆行数」；定稿 `ShopCatalogConfig` + buy/sell 列表驱动 `Shop_Bar` 数量与 Icon/Name/Price 刷新；分阶段 SD-0～SD-5 与验收 SD-V1～V7 |
