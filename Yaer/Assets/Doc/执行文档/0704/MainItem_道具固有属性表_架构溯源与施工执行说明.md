# MainItem · 道具固有属性表（Asset 唯一源）— 架构溯源与施工执行说明

**文档版本**：**v2**（2026-07-05 定稿，**取代 v1「扩展 JSON」方案**）  
**文档性质**：架构侦探产出（只读溯源 + 施工指引；**本阶段先写文档，代码待施工**）  
**调查日期**：2026-07-05  
**触发**：
- 填 `ShopCatalogConfig` 时 Icon / Name / Price 不应由货单决定，应绑定在**道具本身**
- JSON 无法直观拖 **Sprite**；策划需要 Inspector 一眼看到 Icon / 名称 / 价格

**依据**：
- `Assets/Doc/00_MASTER_PROMPT.md`【架构侦探】
- `Assets/Doc/执行文档/0704/Shop_货单配置Asset与Shop_Bar数据刷新_架构溯源与施工执行说明.md`
- `Assets/Doc/执行文档/0704/MainItem_商店六道具ID补全_架构溯源与施工执行说明.md`
- ~~v1 本文档「扩展 MainItemConfig.json」~~ → **已废弃，见 §3.4**

**Unity 版本**：2020.3.48f1  

**范围说明**：**本任务只建 `MainItemDatabase.asset` + 读取层 + 背包改读**；`ShopCatalogConfig` 瘦身 → **另开任务**。

---

## ① 结论（一句话）

**新建 ScriptableObject 总表 `MainItemDatabase.asset` 作为道具唯一数据源：每条 `MainItemDefEntry` 含 `itemId`、Inspector 可拖的 `icon`、`displayName`、`buyPrice` / `sellPrice` 及详情文案；运行时经 `MainItemDefProvider.GetDef(itemId)` 统一供给背包 / 商店 / 菜单；`MainItemConfig.json` 迁移完成后降级为只读归档或删除，不再参与运行时加载。**

---

## ①.1 范围冻结（2026-07-05 · v2）

| 项 | 本阶段约定 | 说明 |
|----|------------|------|
| **唯一数据源** | **`MainItemDatabase.asset`** | 一张 SO，内含 `List<MainItemDefEntry>` |
| **固有属性** | **Icon（Sprite）、Name、BuyPrice、SellPrice** | Inspector 直接编辑 |
| **道具 ID** | **`EMainItemName itemId`** | 与枚举、存档 string 键一致 |
| **访问入口** | **`MainItemDefProvider`** | `GetDef(itemId)` → 只读视图 |
| **JSON 主表** | **迁移后停用** | 从 JSON **一次性导入** 初始 `.asset`；运行时不再 `LoadConfig` |
| **图集兜底** | **可选** | Entry 已拖 `icon` 时**优先用 Sprite**；空则回退 `MainItem_Icon` + name |
| **本阶段不做** | 改 `ShopCatalogConfig`、商店 Bind 改读 Provider | **商店表格之后另改** |
| **本阶段不做（续）** | 药水使用效果、扣金币、Excel 自动导 Asset | 可选手动维护 Asset |

---

## ② 玩家在做什么 / 会遇到什么现象

| 现象 | 原因（生活类比） |
|------|------------------|
| JSON 里看不见 Icon，只能靠名字猜文件 | 文本表没有「照片槽」，不像档案袋里能贴照片 |
| 商店货单和背包各写一遍名字价 | 缺**一本总档案**；货单成了临时笔记本 |
| Sprite 名叫「红烧鱼」、ID 叫 `Fish` | 没有统一档案时，只能在使用处手拖引用 |

**生活类比**：`MainItemDatabase.asset` 是**带照片的道具总档案柜**（Inspector 打开就能看图标和价）；`ShopCatalog` 以后只是**今日上架清单**（勾选卖哪些 ID）。JSON 是旧的纸质索引卡，迁移进档案柜后收进库房。

---

## ③ 工程现状快照（架构侦探 · 只读）

### 3.1 当前运行时：JSON + 图集约定

| 组件 | 路径 | 现状 |
|------|------|------|
| JSON | `GameRes/Config/MainItemConfig/MainItemConfig.json` | 15 道具；无 price、无 icon 字段 |
| Row | `MainItemDataTableRow.cs` | id, name, cnName, detail*, itemType |
| 加载 | `PlayerBagData.Init()` → `LoadConfig` | 异步 JSON |
| Icon | `iconAtlas.GetSprite(item.name)` | **name 当 Sprite 名**，JSON 不参与 |

### 3.2 当前商店：货单重复存展示与价

| 属性 | 来源 | 问题 |
|------|------|------|
| Icon | `ShopCatalogConfig.entry.icon` 手拖 | 与道具档案脱节 |
| Name | `displayName` 或兜底字典 | 双轨 |
| Price | `entry.price` | 非道具固有 |

### 3.3 v1 方案为何废弃

| v1（扩展 JSON + iconKey） | v2（Asset 唯一源） |
|---------------------------|-------------------|
| Icon 靠字符串 key 找 PNG | **Inspector 直接拖 Sprite** |
| 策划改价改 JSON 文本 | **Inspector 改 int 字段** |
| 与 ShopCatalog 编辑体验不一致 | **与 ShopCatalog 同为 SO List**，学习成本低 |

### 3.4 文档关系

| 文档 / 资源 | v2 处理 |
|-------------|---------|
| 本文档 v1 | **废弃**；施工以 v2 为准 |
| `MainItem_商店六道具ID补全` | enum + JSON 行已完成；**迁移进 Database** |
| `Shop_货单配置Asset` | 后续任务：货单只留 ID 列表 |
| `MainItemConfigExcelTool` | **暂不改造**；需要时再做了 Excel→Asset 导出 |

---

## ④ 目标架构（施工完成后）

### 4.1 资源与脚本结构

**Asset 路径（建议）**：`Assets/GameRes/Config/MainItem/MainItemDatabase.asset`

**脚本（建议路径）**：`Assets/Scripts/Game/DataTable/MainItem/`

```
MainItemDefEntry（[Serializable] 单条道具 · 不是独立 .asset）
├── itemId: EMainItemName          ← 必填，唯一
├── icon: Sprite                   ← 必填（或允许空 + 图集兜底）
├── displayName: string            ← 中文展示名（原 cnName）
├── itemType: BagItemType          ← 任务 / 消耗 / 素材
├── buyPrice: int                  ← -1 = 不可买 / 未定价
├── sellPrice: int                 ← -1 = 不可卖 / 未定价
├── detail: string                 ← [TextArea] 中文详情
├── detailEn: string
└── detailJp: string

MainItemDatabase : ScriptableObject
├── [CreateAssetMenu] Config/MainItem/MainItemDatabase
└── entries: List<MainItemDefEntry>

MainItemDef（运行时只读视图 · 可由 Entry 构造，不必再建 SO）
├── ItemId, DisplayName, Icon, BuyPrice, SellPrice, ItemType, Detail*

MainItemDefProvider（静态或单例 Mgr）
├── Database 引用（Resources / 序列化常量路径 / GameManager 注入）
├── GetDef(EMainItemName) / GetDef(string name)
├── TryGetBuyPrice / TryGetSellPrice
└── EnsureLoaded() — Editor 与 Play 均可同步读 Asset
```

**为何用「一张总表 + List」而不是「每个道具一个 .asset」**：

| 方案 | 优点 | 缺点 |
|------|------|------|
| **一张 MainItemDatabase（推荐）** | 一处打开看全表；与 ShopCatalog 一致 | 单文件变大（15～50 条可接受） |
| 每道具一个 ItemDef.asset | 模块化 | 15+ 文件难总览；易漏 ID |

### 4.2 Icon 解析优先级

```
1. entry.icon != null  → 直接使用（Inspector 拖入的 Sprite）
2. 否则 iconAtlas.GetSprite(itemId.ToString())
3. 否则 Editor：AssetDatabase.Load Icon/{name}.png
4. 否则 null + Warning 日志
```

**说明**：拖 Sprite **不依赖** PNG 文件名与 `itemId` 一致；「红烧鱼.png」可拖给 `Fish`，档案仍在 Database 一条里。

### 4.3 Price 语义

| 字段 | 值 | 含义 |
|------|-----|------|
| `buyPrice` | `>= 0` | 固有买价（商店购买参考价） |
| `buyPrice` | `-1` | 不可购买 / 未定价 |
| `sellPrice` | `>= 0` | 固有卖价 |
| `sellPrice` | `-1` | 不可出售 / 未定价 |

**Fish / BowlLiquid**：`itemType = 消耗品`，买价见 §4.5（与策划已定稿一致）。

### 4.4 与枚举、存档的关系

- **`EMainItemName`**：代码与存档字典键 **`itemId.ToString()`**，**保留不改**
- Database 中 **每个 enum 值最多一条 Entry**；Provider 建 `Dictionary<EMainItemName, MainItemDef>`
- **追加新道具**：先 enum 末尾 + Database 加一行（与 ID 补全文档一致）

### 4.5 首版填表（迁移自 JSON + ShopCatalog 买价）

施工创建 `.asset` 时写入 **15 条**（id 顺序可与 JSON 一致，Runtime 不依赖 numeric id）：

| itemId | displayName | buyPrice | sellPrice | itemType | icon |
|--------|-------------|----------|-----------|----------|------|
| HpBall | 生命之珠 | 200 | -1 | CostItem | 拖 HpBall 或暂空 |
| MpBall | 体力之珠 | -1 | -1 | CostItem | 暂空；**buy 原 Shop=0 改 -1 待策划** |
| SmallHpPotion | 小生命药 | 待填 | -1 | CostItem | 策划后续提供 |
| SmallMpPotion | 小体力药 | 20 | -1 | CostItem | 同上 |
| LargeHpPotion | 大生命药 | 50 | -1 | CostItem | 同上 |
| LargeMpPotion | 大体力药 | 50 | -1 | CostItem | 同上 |
| BowlLiquid | 碗装液体 | 500 | -1 | CostItem | 同上 |
| Fish | 鱼 | 500 | -1 | CostItem | 同上 |
| InsectBeak | 虫喙 | -1 | -1 | MaterialItem | 图集已有 |
| … | （其余 7 条从 JSON 抄 displayName + detail + itemType） | -1 | -1 | 按 JSON | 图集 / 暂空 |

> detail 长文案从 `MainItemConfig.json` **复制**到 Entry 的 TextArea；**不要求**策划在 Inspector 重打。

### 4.6 职责边界（本任务 vs 后续）

```
MainItemDatabase.asset          ← 本任务：Icon, Name, Price, detail, itemType
        ↑ MainItemDefProvider.GetDef(itemId)
PlayerBagData / 菜单 / 商店      ← 本任务：背包改读 Provider
ShopCatalogConfig               ← 后续：只列 buy/sell 的 itemId 列表
```

---

## ⑤ 分阶段施工

### 阶段 IA-0 · 定义 SO 与 Entry 脚本

**新建**：

- `MainItemDefEntry.cs`
- `MainItemDatabase.cs`（含 `[CreateAssetMenu(fileName = "MainItemDatabase", menuName = "Config/MainItem/MainItemDatabase")]`）

| 做 | 不做 |
|----|------|
| Entry 字段按 §4.1 | 不改 PlayerBagData |
| `buyPrice`/`sellPrice` 默认 -1 | |

**验证 IA-0**：编译通过；Project 窗口可 Create → MainItemDatabase。

### 阶段 IA-1 · Editor 从 JSON 生成初始 Asset（推荐）

**新建（可选）**：`MainItemDatabaseEditor.cs` 或菜单 `Tools/MainItem/Import Database From JSON`

| 步骤 | 说明 |
|------|------|
| 读 `MainItemConfig.json` | 填 displayName、detail*、itemType |
| 读 `ShopCatalogConfig.asset`（仅 Editor） | 抄 buyPrice 到对应 itemId（MpBall 等） |
| Icon | 尝试 `AssetDatabase.LoadAssetAtPath` → `ArtRes/UI/Item/Icon/{name}.png` |
| 写出 `MainItemDatabase.asset` | 保存到 §4.1 路径 |

**替代**：纯手动 Create Asset + 复制 15 行（不跑脚本）。

**验证 IA-1**：选中 Database → Inspector 见 15 条；HpBall buyPrice=200。

### 阶段 IA-2 · `MainItemDefProvider`

| 做 | 说明 |
|----|------|
| `GetDef(EMainItemName)` | O(1) 字典 |
| Icon 优先级 §4.2 | |
| 常量路径 `MainItemDatabaseAssetPath` | 与 ShopCatalog 引用方式类似 |

**Database 加载方式（二选一，施工前定一种）**：

| 方式 | 说明 |
|------|------|
| **A · 固定路径 LoadAssetAtPath（推荐）** | Editor / 有 ResMgr 时同步加载；Village_Shop 测试场景可用 |
| B · Resources.Load | 需把 asset 放 Resources，改路径约定 |

**验证 IA-2**：Play 前手动测 `GetDef(HpBall).DisplayName == 生命之珠`。

### 阶段 IA-3 · `PlayerBagData` 改读 Provider

| 改 | 不改 |
|----|------|
| `RefreshMainItemRuntimeData`：`icon` ← `GetDef(name).Icon` | ShopCatalogConfig |
| `detail` / `itemType` ← GetDef | |
| `GetItemRow` → 改为 `GetDef` 或内部转 Row 兼容层 | |
| **移除或 `#if false`** `LoadConfig<MainItemDataTableRow>(MainItemConfig.json)` | |

**兼容层（可选）**：保留 `MainItemDataTableRow GetItemRow` 内部 `GetDef` 转 Row，减少全项目替换。

**验证 IA-3**：进游戏 → 背包 / 菜单道具 Icon、详情与改前一致或更好。

### 阶段 IA-4 · 收敛商店兜底类（不强制改 Bind）

| 项 | 操作 |
|----|------|
| `ShopMainItemDisplayCache` | 改为调用 `MainItemDefProvider` |
| `ShopMainItemIconResolver` | 标记 Obsolete；逻辑并入 Provider |

**验证 IA-4**：Village_Shop Play **仍走 ShopCatalog**（行为不变）；Provider 单测可用。

### 阶段 IA-5 · JSON 降级与文档

| 做 | 说明 |
|----|------|
| `MainItemConfig.json` 文件头或 Doc 注明 **已归档** | 勿再改 JSON 期望生效 |
| `MainItemDataTableRow` | 保留类以免破坏 GF DataTable 引用；或仅 Editor Import 用 |
| 另文预告 | `Shop_货单瘦身_仅ID列表_施工说明.md` |

**验证 IA-5**：运行时无 Load MainItemConfig 日志；改 Database Inspector → Play 背包名价变。

---

## ⑥ 技术要点与踩坑

### 6.1 双源禁止期

迁移完成前 **不要** 同时改 JSON 和 Asset。以 **Database 为准** 后，JSON 仅作 Import 源。

### 6.2 Icon 拖入 vs 图集

- **已拖 Sprite**：背包 / 商店直接用，**不必** PNG 名 = enum
- **未拖**：仍走 `MainItem_Icon`；新 6 道具 Icon 策划到位后在 Database 拖入即可

### 6.3 Async Load 与 Village_Shop

原 `LoadConfig` 为异步；Database 可用 **同步 LoadAsset**（Editor 路径）或启动时预加载一次。Provider 需处理 `GetDef` 时 Database 尚未加载 → Log + 返回 null-safe。

### 6.4 唯一性校验

建议 Editor `OnValidate`：重复 `itemId`、displayName 空、`buyPrice` 与 `sellPrice` 同时为 -1 等 Warning。

### 6.5 与 Monster/Quest 等 JSON 表共存

仅 **MainItem** 改 SO；其他 Config 仍 JSON DataTable，**不强行统一**，避免 scope 膨胀。

### 6.6 本任务仍不改 ShopCatalog

Database 已有价时，Shop 仍读货单 `price` → **短期可能双价**。接受至「货单瘦身」任务；或施工 IA-4 后仅加 Debug 对比日志。

---

## ⑦ 验收清单（v2 · Asset 唯一源）

| # | 操作 | 期望 |
|---|------|------|
| IA-V1 | Project 存在 `MainItemDatabase.asset` | ≥15 条 Entry；Inspector 可拖 Icon |
| IA-V2 | `GetDef(SmallHpPotion)` | displayName=小生命药；buyPrice 与 Asset 一致 |
| IA-V3 | 背包中有 HpBall | Icon 来自 Entry 或图集兜底 |
| IA-V4 | 改 Database 某条 displayName → Play | 背包 / 菜单展示更新 |
| IA-V5 | 运行时 **不** 加载 MainItemConfig.json | 无 LoadConfig MainItem 日志 |
| IA-V6 | ShopCatalog **未改** | 商店仍可 Play |
| IA-V7 | Console | 无重复 itemId；无 NullReference |

> **后续任务**：Shop 货单只列 ID → Shop_Bar 全读 `GetDef`；去掉货单 icon/name/price 字段。

---

## ⑧ 待确认问题

| ID | 问题 | 状态 |
|----|------|------|
| ~~Q1~~ | JSON 还是 Asset 唯一源 | ✅ **Asset（v2）** |
| Q2 | MpBall buyPrice：Shop 曾填 0 | 📝 建议 Database **-1** 直至策划定价 |
| Q3 | 素材 sellPrice | 📝 先 -1 |
| Q4 | Database 加载：固定路径 vs Resources | 📝 推荐 **固定路径**（IA-2） |
| Q5 | JSON 文件是否删除 | 📝 建议 **保留归档 + Import 用**，运行时不用 |
| Q6 | 6 张新 Icon | 📝 策划提供后在 Database 拖入，不阻塞 IA-0～3 |

---

## ⑨ 给程序看的入口索引

| 主题 | 路径 |
|------|------|
| **新建 · Database Asset** | `GameRes/Config/MainItem/MainItemDatabase.asset` |
| **新建 · 脚本** | `Scripts/Game/DataTable/MainItem/MainItemDatabase.cs` 等 |
| 枚举 | `EMainItemName.cs` |
| 背包（**IA-3 改**） | `PlayerBagData.cs` |
| JSON（**迁移源 · 运行时停用**） | `MainItemConfig/MainItemConfig.json` |
| 商店货单（**本任务不改**） | `ShopCatalogConfig.cs` |
| 旧兜底（**IA-4 收敛**） | `ShopMainItemDisplayCache.cs`、`ShopMainItemIconResolver.cs` |
| SO 先例 | `PlayerStaminaConfig.cs` |
| 0629 | `0629/商店系统_策划拆解_执行说明.md` |

---

## ⑩ 文档变更记录

| 日期 | 版本 | 说明 |
|------|------|------|
| 2026-07-05 | v1 | 扩展 MainItemConfig.json + iconKey + Provider（**已废弃**） |
| 2026-07-05 | **v2** | 定稿 **MainItemDatabase.asset 唯一源**；Icon 可 Inspector 拖 Sprite；JSON 迁移后停用；分阶段 IA-0～IA-5 |
